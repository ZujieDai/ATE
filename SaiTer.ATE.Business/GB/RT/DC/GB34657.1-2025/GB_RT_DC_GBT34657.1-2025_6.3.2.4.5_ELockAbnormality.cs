using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 互操作_电子锁异常测试
    /// 标准：GB_RT_DC_GBT34657.1-2025_6.3.2.4.5_ELockAbnormality
    /// </summary>
    public class GB_RT_DC_2025_ELockAbnormality : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int HighVoltage = 750;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int CanCommunicationMinLength = 2;
        private const int ChargeReadyState = 9;
        #endregion

        #region 字段
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_ELockAbnormality(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 基类重写
        public override void InitEquiMent()
        {
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
        }
        #endregion

        #region 测试主流程
        private void StartTestFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile($"开始 {TrialItem.ItemName} ------------------>");
                _StopWatch.Restart();

                while (true)
                {
                    var pendingIds = GetPendingChargerIds();
                    if (pendingIds.Count == 0)
                        break;

                    if (IsTestTimeout())
                    {
                        HandleTimeout(pendingIds);
                        break;
                    }

                    ExecuteAllTestItems(pendingIds);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        private void ExecuteAllTestItems(List<int> chargerIds)
        {
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            Thread.Sleep(200);
            SetConditionValues();

            // 1. 电子锁未预期上锁
            RunELockTest(chargerIds, "电子锁未预期上锁");

            // 2. 电子锁未可靠锁止
            RunELockTest(chargerIds, "电子锁未可靠锁止");
        }
        #endregion

        #region 电子锁测试公共方法
        private void RunELockTest(List<int> ids, string testName)
        {
            SetCPReresh();

            try
            {
                // 握手
                ControlEquipMent.BMS.SetParameter(ids, MaxAllowChargeVoltage);
                Thread.Sleep(200);

                // 参数配置
                ControlEquipMent.BMS.SetParameter(ids, DefaultVoltage, MaxAllowChargeVoltage, RatedCurrent);
                Thread.Sleep(200);

                // 充电需求
                ControlEquipMent.BMS.SetParameter(ids, MaxAllowChargeVoltage, RatedCurrent, true, MaxAllowChargeVoltage);
                Thread.Sleep(200);

                // 验证保护逻辑
                VerifyELockAbnormalProtection(ids, testName);

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                ProcessDataConnect("应发出告警提示", "是否有告警提示");
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region 电子锁异常验证逻辑
        private void VerifyELockAbnormalProtection(List<int> chargerIds, string testScene)
        {
            ControlEquipMent.BMS.BMS_ON(chargerIds);
            string testTitle = $"电子锁异常测试({testScene})";

            BMS_DC_StateData bmsData = null;
            Dictionary<int, EmTrialResult> isChargingResult = new Dictionary<int, EmTrialResult>();

            if (testScene == "电子锁未预期上锁")
            {
                CountDownTimeInfo("请模拟电子锁未预期上锁(如采用断开电子锁反馈信号等方式)，完成后点击确定", 100, 2);
                // 等待刷卡
                MessgaeInfo(true, "请刷卡充电! ");
                bool enterCharging = false;

                for (int i = 0; i < 40; i++)
                {
                    bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

                    if (bmsData != null && bmsData.ChargingState == "充电中")
                    {
                        enterCharging = true;
                        MessgaeInfo(false, "");
                        break;
                    }

                    Thread.Sleep(1000);
                }
                MessgaeInfo(false, "");

                isChargingResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => !enterCharging ? EmTrialResult.Pass : EmTrialResult.Fail);

                if (!enterCharging)
                {
                    SendNoticeToUIAndTxtFile("超时40秒：充电桩未进入充电中状态");
                }
            }
            else if (testScene == "电子锁未可靠锁止")
            {
                // 等待刷卡
                WaitForSwipeCard(chargerIds, 200);
                Thread.Sleep(5000);
                CountDownTimeInfo("请模拟电子锁未预期上锁(如采用断开电子锁反馈信号等方式)，完成后点击确定", 100, 2);
                Thread.Sleep(5000);

                bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

                isChargingResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
            }

            // 空值保护
            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS数据为空，测试终止");
                ControlEquipMent.BMS.BMS_OFF(chargerIds);
                return;
            }

            // 1. 充电状态验证：应禁止充电
            Dictionary<int, string> chargingState = isChargingResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "不允许充电" : "允许充电");
            ProcessDataResults(chargerIds, chargingState, "充电状态", isChargingResult, testTitle, "不允许充电", "不允许充电");

            // 2. CC1电压记录
            var CC1VoltageValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
            ProcessDataTmp(CC1VoltageValue, testTitle, "CC1电压", "-", "-");

            // 3. C1/C2 应断开
            var chargeC1C2Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < VoltageThreshold60 ? EmTrialResult.Pass : EmTrialResult.Fail);
            var C1C2State = chargeC1C2Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
            ProcessDataResults(chargerIds, C1C2State, "C1C2状态", chargeC1C2Result, testTitle, "断开", "断开");

            // 4. S3/S4 应断开
            Dictionary<int, EmTrialResult> chargeS3S4Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < VoltageThreshold5 ? EmTrialResult.Pass : EmTrialResult.Fail);
            Dictionary<int, string> S3S4State = chargeS3S4Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
            ProcessDataResults(testWorkParam.lstIDs, S3S4State, "S3S4状态", chargeS3S4Result, testTitle, "断开", "断开");

            // 5. 通讯状态验证
            string cst = GetCANByType("CHM");
            bool commOk = !(cst != null && cst.Length > CanCommunicationMinLength);
            Dictionary<int, EmTrialResult> commResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => commOk ? EmTrialResult.Pass : EmTrialResult.Fail);
            Dictionary<int, string> commState = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => commOk ? "正常" : "异常");
            ProcessDataResults(testWorkParam.lstIDs, commState, "通讯状态", commResult, testTitle, "正常", "正常");

            // 关闭BMS
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
        }
        #endregion

        #region 工具方法
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

        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;
        }

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
                    .Where(x => ids.Contains(x.Value.ChargerID))
                    .Select(x => x.Value)
                    .FirstOrDefault();

                if (bms != null)
                {
                    int state = ChangeBMSChargeStatus(bms.ChargingState);
                    if (state >= ChargeReadyState)
                    {
                        break;
                    }
                }

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}