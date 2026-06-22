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
    /// 保护接地导体连续性丢失测试（录波版）
    /// 标准：GB/T 34657.1-2025  6.3.4.9
    /// 功能：模拟充电前/充电中车辆接口断开，验证保护、状态、时序、报文
    /// </summary>
    public class GB_RT_DC_2025_ProtectiveGroundingLoss_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int HighVoltage = 750;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int CanCommunicationMinLength = 2;
        #endregion

        #region 私有变量
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造函数
        public GB_RT_DC_2025_ProtectiveGroundingLoss_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 基类重写
        public override void InitEquiMent()
        {
            // 设备初始化（预留）
        }

        public override void InitializeParams()
        {
            Init();
            _tempTestData.Clear();
            _bmsDemandVoltage = LstChargerInfo?.FirstOrDefault()?.NominalVoltage ?? 0;
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
                SendNoticeToUIAndTxtFile($"{TrialItem.ItemName} 结束 ------------------>");
                SendMessageEndThisTrial();
            }
        }

        public override void ProcessData()
        {
            // 数据处理（预留）
        }
        #endregion

        #region 测试主流程
        /// <summary>
        /// 测试总入口：循环执行所有待测试充电桩
        /// </summary>
        private void StartTestFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile($"开始 {TrialItem.ItemName} ------------------>");
                _StopWatch.Restart();

                while (true)
                {
                    List<int> pendingIds = GetPendingChargerIds();
                    if (pendingIds.Count == 0)
                        break;

                    if (IsTestTimeout())
                    {
                        HandleTimeout(pendingIds);
                        break;
                    }

                    ExecuteTestItems(pendingIds);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 执行充电前 + 充电中断开测试
        /// </summary>
        private void ExecuteTestItems(List<int> chargerIds)
        {
            try
            {
                BMSCanOpenRev(true);

                Thread.Sleep(200);
                SetConditionValues();

                // 1. 握手阶段
                ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage);
                Thread.Sleep(200);

                // 2. 参数配置
                ControlEquipMent.BMS.SetParameter(chargerIds, DefaultVoltage, 750, 3);
                Thread.Sleep(200);

                // 3. 充电需求
                ControlEquipMent.BMS.SetParameter(chargerIds, 500, 3, true, DefaultVoltage);
                Thread.Sleep(200);

                // 4. 执行两个场景测试
                RunAbnormalTest(chargerIds, "充电前");
                RunAbnormalTest(chargerIds, "充电中");
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                BMSCanOpenRev(false);

            }
        }
        #endregion

        #region 核心测试：车辆接口断开 + 录波
        /// <summary>
        /// 执行车辆接口断开异常测试（含录波）
        /// </summary>
        private void RunAbnormalTest(List<int> chargerIds, string testScene)
        {

          
            SetCPReresh();
            Thread.Sleep(200);

            string testTitle = $"保护接地导体连续性丢失({testScene})";
            SendNoticeToUIAndTxtFile("录波板启动录波...");

            // 启动录波 + BMS
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
            // 充电前：提前断开
            ControlEquipMent.BMS.BMS_ON(chargerIds);

            if (testScene == "充电前")
            {
                SendNoticeToUIAndTxtFile("断开PE");

                //var kState = ControlEquipMent.BMS.BMS_GetKState(chargerIds).First().Value;
                var kState = GetKStatus16_Charging_DC();
                kState[17] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(chargerIds, 1000, 390, kState.ToArray());
                Thread.Sleep(200);
                // 6. 人工确认：充电枪可解锁
                CountDownTimeInfo("点击充电按钮是否能充电。\r\n(注:勾选上为不可充电)", 60, 2);
                ProcessDataConnect("充电前PE断开 不可充电", "是否不可充电");
            }
            else
            {
                Thread.Sleep(200);
                WaitForSwipeCard(chargerIds, 100, testScene);
                Thread.Sleep(5000);
                // 设置K状态：模拟开关PE断开
                var kState = GetKStatus16_Charging_DC();
                kState[17] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(chargerIds, 1000, 390, kState.ToArray());
                WaitDCVoltage(lstIDs, 0, 20);
                Thread.Sleep(2000);

            }

            // 停止录波
            SendNoticeToUIAndTxtFile("停止录波...");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 读取录波数据
            WaveData cc1WaveData = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref cc1WaveData, "CC1电压");

            WaveData outputVoltageWave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref outputVoltageWave, "充电电压");


            VerifyProtectionLogic(chargerIds, testScene);
            // 关闭BMS并验证保护
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            // 保存截图
            var screenImg = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, "时序图", "时序图", "-", "-", screenImg);
        }
        #endregion

        #region 保护逻辑验证
        /// <summary>
        /// 验证车辆接口断开后的保护逻辑
        /// </summary>
        private void VerifyProtectionLogic(List<int> chargerIds, string testScene)
        {
            string testTitle = $"保护接地导体连续性丢失({testScene})";
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS数据获取失败，测试中止");
                return;
            }

            // 1. 充电状态：必须停止充电
            Dictionary<int, bool> chargeStateResult = new Dictionary<int, bool>
            { { 1, bmsData.ChargingState != "充电中" } };
            ProcessDataResults(chargerIds,
                chargeStateResult[1] ? "停止充电" : "允许充电",
                "停止充电",
                chargeStateResult,
                testTitle + " 充电状态");

            // 2. CC1电压
            ProcessDataTmp(
                new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } },
                testTitle, "CC1电压", "-", "-");

            // 3. C1/C2 应断开
            Dictionary<int, bool> c1c2Result = new Dictionary<int, bool>
            { { 1, bmsData.ChargingVoltage < VoltageThreshold60 } };
            ProcessDataResults(chargerIds,
                c1c2Result[1] ? "断开" : "闭合",
                "断开",
                c1c2Result,
                testTitle + " C1C2状态");

            // 4. S3/S4 应断开
            Dictionary<int, bool> s3s4Result = new Dictionary<int, bool>
            { { 1, bmsData.APSVoltage < VoltageThreshold5 } };
            ProcessDataResults(chargerIds,
                s3s4Result[1] ? "断开" : "闭合",
                "断开",
                s3s4Result,
                testTitle + " S3S4状态");

            // 5. 通讯状态 & CST报文
            string cstMsg = GetCANByType("CST");
            bool commNormal = cstMsg != null && cstMsg.Length > CanCommunicationMinLength;
            Dictionary<int, bool> commResult = new Dictionary<int, bool> { { 1, commNormal } };

            ProcessDataResults(chargerIds,
                commResult[1] ? "正常" : "异常",
                "正常",
                commResult,
                testTitle + " 通讯状态");

            ProcessDataTmp(
                new Dictionary<int, string> { { 1, cstMsg } },
                "暂停异常测试", "CST 报文", "-", "-");

            // 6. 人工确认：充电枪可解锁
            CountDownTimeInfo("请确认充电中充电枪插头可被解锁。\r\n(注:勾选上为可被解锁)", 15, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否可解锁");
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 获取待测试充电桩ID
        /// </summary>
        private List<int> GetPendingChargerIds()
        {
            testWorkParam.lstIDs.Clear();
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                if (!testWorkParam.lstIDs.Contains(item.ChargerId))
                    testWorkParam.lstIDs.Add(item.ChargerId);
            }
            return testWorkParam.lstIDs;
        }

        /// <summary>
        /// 判断是否测试超时
        /// </summary>
        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;
        }

        /// <summary>
        /// 超时处理：标记失败
        /// </summary>
        private void HandleTimeout(List<int> ids)
        {
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                item.TrialResult = EmTrialResult.Fail;
                item.TrialValue = _StopWatch.Elapsed.Seconds.ToString();
                var charger = LstChargerInfo.FirstOrDefault(x => x.ChargerId == item.ChargerId);
                if (charger != null)
                    item.PKID = charger.PKID;
                item.ExtentData = "-|-|-|-|null";
                SendTrialDataToUI(item);
            }
        }

        /// <summary>
        /// 复位设备状态
        /// </summary>
        private void ResetEquipmentState()
        {
            var kState = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, HighVoltage, DefaultVoltage, kState.ToArray());
            Thread.Sleep(300);
        }

        /// <summary>
        /// 等待刷卡启动充电会结束充电
        /// </summary>
        private void WaitForSwipeCard(List<int> ids, int timeoutSec, string v)
        {

            SendNoticeToUIAndTxtFile("等待刷卡...");
            MessgaeInfo(true, "请刷卡充电!", true);

            int timeout = timeoutSec;
            while (timeout-- > 0)
            {
                var bmsState = AllEquipStateData.DicBMS_DC_StateData
                    .Where(x => ids.Contains(x.Value.ChargerID))
                    .Select(x => x.Value)
                    .FirstOrDefault();
                //等待CRO_AA报文

                if (bmsState != null && ChangeBMSChargeStatus(bmsState.ChargingState) >= 9)
                    break;

                Thread.Sleep(1000);
            }
           
            MessgaeInfo(false, "");
        }
        #endregion
    }
}