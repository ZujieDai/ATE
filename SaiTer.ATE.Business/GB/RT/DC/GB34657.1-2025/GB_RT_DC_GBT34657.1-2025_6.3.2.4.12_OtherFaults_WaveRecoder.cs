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
    /// 其他充电故障测试（录波版）
    /// 标准：GB/T 34657.1-2025  6.3.2.4.12
    /// 测试项：
    /// 1. 充电中充电机急停故障
    /// 2. 充电中交流电源停电
    /// 验证：停止充电、C1/C2断开、S3/S4断开、电压泄放、通信中止、电子锁解锁
    /// </summary>
    public class GB_RT_DC_2025_OtherFaults_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int HighVoltage = 750;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int CanCommunicationMinLength = 2;
        private const int MaxReleaseTimeMs = 100; // 电压泄放最大允许时间
        #endregion

        #region 私有变量
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_OtherFaults_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 基类重写
        public override void InitEquiMent() { }

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

        public override void ProcessData() { }
        #endregion

        #region 测试主流程
        private void StartTestFlow()
        {
            SendNoticeToUIAndTxtFile($"开始 {TrialItem.ItemName} ------------------>");
            _StopWatch.Restart();

            while (true)
            {
                List<int> pendingIds = GetPendingChargerIds();
                if (pendingIds.Count == 0) break;
                if (IsTestTimeout()) { HandleTimeout(pendingIds); break; }

                ExecuteFaultTest(pendingIds);
            }
        }

        /// <summary>
        /// 执行所有故障测试：急停 + 交流停电
        /// </summary>
        private void ExecuteFaultTest(List<int> chargerIds)
        {
            try
            {
                Thread.Sleep(200);
                SetConditionValues();

                // 1. 初始化BMS参数
                InitBMSParameters(chargerIds);

                // 2. 测试1：充电机急停故障
                RunChargerEmergencyStopTest(chargerIds);

                // 3. 测试2：交流电源停电故障
                RunACPowerOffTest(chargerIds);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 初始化BMS充电参数
        /// </summary>
        private void InitBMSParameters(List<int> chargerIds)
        {
            ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage); Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(chargerIds, DefaultVoltage, 500, 3); Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(chargerIds, 500, 3, true, DefaultVoltage); Thread.Sleep(200);
        }
        #endregion

        #region 故障测试子流程
        /// <summary>
        /// 测试1：充电机急停
        /// </summary>
        private void RunChargerEmergencyStopTest(List<int> chargerIds)
        {
            StartWaveRecordAndCharge(chargerIds);
            CountDownTimeInfo("请按下充电桩急停按钮，完成后点击确认", 60, 0);
            Thread.Sleep(2500);
            StopWaveAndAnalyze(chargerIds, "充电机急停故障");

            // 急停复位
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            Thread.Sleep(5000);
            CountDownTimeInfo("请恢复急停按钮至正常状态，点击继续", 60, 0);
        }

        /// <summary>
        /// 测试2：交流电源停电
        /// </summary>
        private void RunACPowerOffTest(List<int> chargerIds)
        {
            StartWaveRecordAndCharge(chargerIds);
            CountDownTimeInfo("请断开交流输入电源，完成后点击确认", 60, 0);
            Thread.Sleep(2500);
            StopWaveAndAnalyze(chargerIds, "交流电源停电故障");

            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            Thread.Sleep(5000);
        }
        #endregion

        #region 录波 + 分析（通用封装）
        /// <summary>
        /// 启动录波 + 启动充电
        /// </summary>
        private void StartWaveRecordAndCharge(List<int> chargerIds)
        {
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_ON(chargerIds);
            Thread.Sleep(200);
            WaitForSwipeCard(chargerIds, 200);
            WaitDCVoltage(lstIDs, 500);
            Thread.Sleep(3000);
        }

        /// <summary>
        /// 停止录波 + 解析波形 + 判定泄放时间
        /// </summary>
        private void StopWaveAndAnalyze(List<int> chargerIds, string testType)
        {
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

            WaveData c1c2Wave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(chargerIds, 6, ref c1c2Wave);

            double tStart = 0, tEnd = 0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(c1c2Wave, true, DefaultVoltage - 5, ref tStart, 0, true);
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(c1c2Wave, false, 0, ref tEnd, tStart);

            SetWaveRecorderCursor(tStart, tEnd);

            // 获取光标时间差
            var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存截图
            var screenshot = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            // 判定泄放时间 ≤100ms
            ProcessDataTmp(timeDiffData, $"{testType} - C1C2断开", "泄放时间(ms)", "0", MaxReleaseTimeMs.ToString(), screenshot);

            // 验证保护逻辑
            VerifyProtectionLogic(chargerIds, testType);
        }
        #endregion

        #region 保护逻辑验证（GB/T 34657.1-2025）
        private void VerifyProtectionLogic(List<int> chargerIds, string testScene)
        {
            string testTitle = $"({testScene})";
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS数据获取失败，测试中止");
                return;
            }

            // 1. 充电状态：必须停止
            var chargeResult = new Dictionary<int, bool> { { 1, bmsData.ChargingState != "充电中" } };
            ProcessDataResults(chargerIds, chargeResult[1] ? "停止充电" : "继续充电", "停止充电", chargeResult, testTitle + " 充电状态");

            // 2. CC1 电压
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, testTitle, "CC1电压", "-", "-");

            // 3. C1/C2 断开（电压＜60V）
            var c1c2Result = new Dictionary<int, bool> { { 1, bmsData.ChargingVoltage < VoltageThreshold60 } };
            ProcessDataResults(chargerIds, c1c2Result[1] ? "断开" : "闭合", "断开", c1c2Result, testTitle + " C1C2状态");

            // 4. S3/S4 断开（电压＜5V）
            var s3s4Result = new Dictionary<int, bool> { { 1, bmsData.APSVoltage < VoltageThreshold5 } };
            ProcessDataResults(chargerIds, s3s4Result[1] ? "断开" : "闭合", "断开", s3s4Result, testTitle + " S3S4状态");

            // 5. 通信状态 & CST报文
            string cstMsg = GetCANByType("CST");
            bool commNormal = !string.IsNullOrEmpty(cstMsg) && cstMsg.Length > CanCommunicationMinLength;
            var commResult = new Dictionary<int, bool> { { 1, commNormal } };
            ProcessDataResults(chargerIds, commNormal ? "正常结束" : "异常", "正常结束", commResult, testTitle + " 通讯状态");
            ProcessDataTmp(new Dictionary<int, string> { { 1, cstMsg } }, testTitle, "CST 中止报文", "-", "-");

            // 6. 人工确认：车辆插头可解锁
            CountDownTimeInfo("请确认充电枪可被解锁（勾选=可解锁）", 15, 2);
            ProcessDataConnect("故障测试", "车辆插头是否可解锁");
        }
        #endregion

        #region 工具方法
        private void SetWaveRecorderCursor(double timeStart, double timeEnd)
        {
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, timeStart);
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, timeEnd);
        }

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

        private bool IsTestTimeout() => _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;

        private void HandleTimeout(List<int> ids)
        {
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                item.TrialResult = EmTrialResult.Fail;
                item.TrialValue = _StopWatch.Elapsed.Seconds.ToString();
                var charger = LstChargerInfo.FirstOrDefault(x => x.ChargerId == item.ChargerId);
                if (charger != null) item.PKID = charger.PKID;
                item.ExtentData = "-|-|-|-|null";
                SendTrialDataToUI(item);
            }
        }

        private void ResetEquipmentState()
        {
            var kState = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, HighVoltage, DefaultVoltage, kState.ToArray());
            Thread.Sleep(300);
        }

        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡...");
            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = timeoutSec;

            while (timeout-- > 0)
            {
                var bms = AllEquipStateData.DicBMS_DC_StateData
                    .FirstOrDefault(x => ids.Contains(x.Value.ChargerID)).Value;

                if (bms != null && ChangeBMSChargeStatus(bms.ChargingState) >= 9)
                    break;

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}