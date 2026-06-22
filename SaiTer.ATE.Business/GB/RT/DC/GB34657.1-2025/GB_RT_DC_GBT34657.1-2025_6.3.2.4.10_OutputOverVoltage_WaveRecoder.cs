using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出过压保护测试（录波版）
    /// 标准：GB/T 34657.1-2025 6.3.2.4.10
    /// 功能：模拟输出过压，验证保护、状态、时序、报文
    /// </summary>
    public class GB_RT_DC_2025_OutputOverVoltage_WaveRecoder : BusinessBase
    {
        #region 常量配置
        /// <summary>测试超时时间</summary>
        private const int TestTimeoutSeconds = 30;
        /// <summary>默认输出电压</summary>
        private const int DefaultVoltage = 390;
        /// <summary>高压设定值</summary>
        private const int HighVoltage = 750;
        /// <summary>C1/C2电压阈值</summary>
        private const int VoltageThreshold60 = 60;
        /// <summary>S3/S4电压阈值</summary>
        private const int VoltageThreshold5 = 5;
        /// <summary>CAN报文最小长度</summary>
        private const int CanCommunicationMinLength = 2;
        /// <summary>最大允许断开时间(ms)</summary>
        private const int MaxDisconnectTimeMs = 1000;
        /// <summary>过压触发电压(V)</summary>
        private const int OverVoltageTrigger = 520;
        /// <summary>录波通道常量</summary>
        private const int WaveChannel_OutputVoltage = 1;
        private const int WaveChannel_AuxVoltage = 3;
        private const int WaveChannel_CC1 = 4;
        #endregion

        #region 私有变量
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造函数
        public GB_RT_DC_2025_OutputOverVoltage_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 基类重写
        public override void InitEquiMent()
        {
            // 设备初始化
            SendNoticeToUIAndTxtFile("设备初始化完成");
        }

        public override void InitializeParams()
        {
            Init();
            _tempTestData.Clear();
            _bmsDemandVoltage = LstChargerInfo?.FirstOrDefault()?.NominalVoltage ?? 0;
            SendNoticeToUIAndTxtFile($"测试参数初始化完成，BMS额定电压：{_bmsDemandVoltage}V");
        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartTestFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                ResetEquipmentState();
                SaveTrialResult();
                SendNoticeToUIAndTxtFile($"{TrialItem.ItemName} 测试结束");
                SendMessageEndThisTrial();
            }
        }

        public override void ProcessData()
        {
            // 数据处理扩展
        }
        #endregion

        #region 测试主流程
        /// <summary>
        /// 测试总入口：循环执行所有待测试充电桩
        /// </summary>
        private void StartTestFlow()
        {
            SendNoticeToUIAndTxtFile($"开始执行 {TrialItem.ItemName}");
            _StopWatch.Restart();

            while (true)
            {
                List<int> pendingIds = GetPendingChargerIds();
                if (pendingIds.Count == 0)
                {
                    SendNoticeToUIAndTxtFile("所有充电桩测试完成");
                    break;
                }

                if (IsTestTimeout())
                {
                    HandleTimeout(pendingIds);
                    SendNoticeToUIAndTxtFile($"测试超时，已标记失败");
                    break;
                }

                ExecuteTestItems(pendingIds);
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 执行充电前配置+过压测试
        /// </summary>
        private void ExecuteTestItems(List<int> chargerIds)
        {
            try
            {
                SetConditionValues();
                SendNoticeToUIAndTxtFile("开始BMS握手与参数配置");

                // 1. 握手阶段
                ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage);
                Thread.Sleep(200);

                // 2. 参数配置
                ControlEquipMent.BMS.SetParameter(chargerIds, DefaultVoltage, 500, 3);
                Thread.Sleep(200);

                // 3. 充电需求配置
                ControlEquipMent.BMS.SetParameter(chargerIds, 500, 3, true, DefaultVoltage, true, new[] { "emtBMS_GB_DC" });
                Thread.Sleep(200);

                // 4. 执行过压测试
                RunAbnormalTest(chargerIds);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region 核心测试：过压保护+录波
        /// <summary>
        /// 执行输出过压异常测试（含录波）
        /// </summary>
        private void RunAbnormalTest(List<int> chargerIds)
        {
            if (chargerIds == null || chargerIds.Count == 0)
            {
                SendNoticeToUIAndTxtFile("无有效充电桩ID，跳过测试");
                return;
            }

            try
            {
                SetCPReresh();
                Thread.Sleep(200);

                // 启动录波
                SendNoticeToUIAndTxtFile("录波板启动");
                if (ControlEquipMent.WaveRecoderCtrl != null)
                {
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                }

                // 启动BMS并等待刷卡
                ControlEquipMent.BMS.BMS_ON(chargerIds);
                Thread.Sleep(200);
                WaitForSwipeCard(chargerIds, 100);
                Thread.Sleep(3000);

                // 模拟过压：电池电压升至520V，保持10s触发保护
                SendNoticeToUIAndTxtFile($"模拟BMS过压：{OverVoltageTrigger}V");
                ControlEquipMent.BMS.BMSSetBatteryVoltage(chargerIds, OverVoltageTrigger);
                Thread.Sleep(10000);

                // 停止录波
                SendNoticeToUIAndTxtFile("录波板停止");
                if (ControlEquipMent.WaveRecoderCtrl != null)
                {
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                }

                // 读取充电电压波形
                WaveData outputVoltageWave = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, WaveChannel_OutputVoltage, ref outputVoltageWave, "充电电压");

                // 关闭BMS
                ControlEquipMent.BMS.BMS_OFF(chargerIds);

                // 验证保护逻辑
                VerifyProtectionLogic(chargerIds);

                // 保存时序截图
                var screenImg = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, "时序图", "时序图", "-", "-", screenImg);
            }
            catch (Exception ex)
            {
                SendException(ex);
                
            }
        }
        #endregion

        #region 保护逻辑验证
        /// <summary>
        /// 验证过压后的保护逻辑
        /// </summary>
        private void VerifyProtectionLogic(List<int> chargerIds)
        {
            const string testTitle = "输出过压保护测试";
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS状态数据为空，测试中止");
                return;
            }

            // 1. 充电状态验证：必须停止
            Dictionary<int, bool> chargeStateResult = new Dictionary<int, bool>
            { { 1, bmsData.ChargingState != "充电中" } };
            ProcessDataResults(chargerIds,
                chargeStateResult[1] ? "停止充电" : "允许充电",
                "停止充电",
                chargeStateResult,
                $"{testTitle} - 充电状态");

            // 2. CC1电压记录
            ProcessDataTmp(
                new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString("F2") } },
                testTitle, "CC1电压", "-", "-");

            // 3. C1/C2 断开验证 + 时序分析
            Dictionary<int, bool> c1c2Result = new Dictionary<int, bool>
            { { 1, bmsData.ChargingVoltage < VoltageThreshold60 } };
            ProcessDataResults(chargerIds,
                c1c2Result[1] ? "断开" : "闭合",
                "断开",
                c1c2Result,
                $"{testTitle} - C1C2状态");

            AnalyzeWaveformDisconnectTime(WaveChannel_CC1, DefaultVoltage - 5, $"{testTitle} - C1C2断开时间(ms)");

            // 4. S3/S4 断开验证 + 时序分析
            Dictionary<int, bool> s3s4Result = new Dictionary<int, bool>
            { { 1, bmsData.APSVoltage < VoltageThreshold5 } };
            ProcessDataResults(chargerIds,
                s3s4Result[1] ? "断开" : "闭合",
                "断开",
                s3s4Result,
                $"{testTitle} - S3S4状态");

            AnalyzeWaveformDisconnectTime(WaveChannel_AuxVoltage, 10, $"{testTitle} - S3S4断开时间(ms)");

            // 5. CAN通讯与CST报文验证
            string cstMsg = GetCANByType("CST");
            bool commNormal = !string.IsNullOrEmpty(cstMsg) && cstMsg.Length > CanCommunicationMinLength;
            Dictionary<int, bool> commResult = new Dictionary<int, bool> { { 1, commNormal } };

            ProcessDataResults(chargerIds,
                commResult[1] ? "正常" : "异常",
                "正常",
                commResult,
                $"{testTitle} - 通讯状态");

            ProcessDataTmp(new Dictionary<int, string> { { 1, cstMsg ?? "无报文" } }, "报文数据", "CST 报文", "-", "-");

            // 6. 人工确认：充电枪可解锁
            CountDownTimeInfo("请确认：过压保护后充电枪可正常解锁\r\n(勾选为合格)", 15, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否可解锁");
        }

        /// <summary>
        /// 统一封装：波形断开时间分析（消除重复代码）
        /// </summary>
        private void AnalyzeWaveformDisconnectTime(int channel, double triggerVoltage, string resultName)
        {
            try
            {
                if (ControlEquipMent.WaveRecoderCtrl == null)
                {
                    SendNoticeToUIAndTxtFile("录波控制器为空，跳过波形分析");
                    return;
                }

                WaveData waveData = new WaveData();
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, channel, ref waveData, "波形数据");

                if (waveData == null)
                {
                    SendNoticeToUIAndTxtFile($"通道{channel}波形数据为空");
                    return;
                }

                // 获取下降沿时间
                double timeStart = 0, timeEnd = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveData, true, triggerVoltage, ref timeStart, isReverseOrder: true);
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveData, false, 0, ref timeEnd, timeStart, isReverseOrder: true);

                // 设置光标
                SetWaveRecorderCursor(timeStart, timeEnd);

                // 读取时间差
                var cursorTimes = ControlEquipMent.WaveRecoderCtrl.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
                var timeDiffData = cursorTimes.ToDictionary(k => k.Key, v => v.Value.ToString("F0"));

                // 保存截图与结果
                var timeImg = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(timeDiffData, "过压保护测试", resultName, "0", MaxDisconnectTimeMs.ToString(), timeImg);
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"波形分析异常：{ex.Message}");
                SendException(ex);
            }
        }

        /// <summary>
        /// 设置录波光标
        /// </summary>
        private void SetWaveRecorderCursor(double timeStart, double timeEnd)
        {
            if (ControlEquipMent.WaveRecoderCtrl == null) return;

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, timeStart);
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, timeEnd);
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 获取待测试充电桩ID
        /// </summary>
        private List<int> GetPendingChargerIds()
        {
            testWorkParam.lstIDs.Clear();
            var waitItems = LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait).ToList();
            foreach (var item in waitItems)
            {
                if (!testWorkParam.lstIDs.Contains(item.ChargerId))
                    testWorkParam.lstIDs.Add(item.ChargerId);
            }
            return testWorkParam.lstIDs;
        }

        /// <summary>
        /// 测试超时判断
        /// </summary>
        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;
        }

        /// <summary>
        /// 超时处理
        /// </summary>
        private void HandleTimeout(List<int> ids)
        {
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                item.TrialResult = EmTrialResult.Fail;
                item.TrialValue = _StopWatch.Elapsed.TotalSeconds.ToString("F1");
                var charger = LstChargerInfo.FirstOrDefault(x => x.ChargerId == item.ChargerId);
                if (charger != null) item.PKID = charger.PKID;
                item.ExtentData = "-|-|-|-|超时";
                SendTrialDataToUI(item);
            }
        }

        /// <summary>
        /// 设备状态复位
        /// </summary>
        private void ResetEquipmentState()
        {
            try
            {
                var kState = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, HighVoltage, DefaultVoltage, kState.ToArray());
                Thread.Sleep(300);
                SendNoticeToUIAndTxtFile("设备状态已复位");
            }
            catch
            {
                // 忽略复位异常
            }
        }

        /// <summary>
        /// 等待刷卡启动充电
        /// </summary>
        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待用户刷卡启动充电...");
            MessgaeInfo(true, "请刷卡充电!", true);

            int timeLeft = timeoutSec;
            while (timeLeft-- > 0)
            {
                var bmsState = AllEquipStateData.DicBMS_DC_StateData
                    .Where(x => ids.Contains(x.Value.ChargerID))
                    .Select(x => x.Value)
                    .FirstOrDefault();

                if (bmsState != null && ChangeBMSChargeStatus(bmsState.ChargingState) >= 9)
                {
                    SendNoticeToUIAndTxtFile("检测到充电已启动");
                    break;
                }

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}