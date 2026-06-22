using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 开关S断开测试（录波板）  //GB_RT_DC_GBT34657.1-2017_6.3.4.2_S2DisconnectTest_WaveRecoder
    /// </summary>
    public class GB_RT_DC_2025_SwitchS_Disconnected_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int HighVoltage = 750;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int CanCommunicationMinLength = 2;
        #endregion

        #region 字段
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_SwitchS_Disconnected_WaveRecoder(int type)
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



            RunAbnormalVoltageTest(chargerIds, 390, 500);
        }
        #endregion

        #region 公共测试方法
        private void RunAbnormalVoltageTest(List<int> ids, double batteryVolt, double reportVolt)
        {
            try
            {
                // 握手
                ControlEquipMent.BMS.SetParameter(ids, HighVoltage);
                Thread.Sleep(200);

                // 参数配置
                ControlEquipMent.BMS.SetParameter(ids, DefaultVoltage, MaxAllowChargeVoltage, 3);
                Thread.Sleep(200);

                // 充电需求
                ControlEquipMent.BMS.SetParameter(ids, DefaultVoltage, 3, true, batteryVolt);
                Thread.Sleep(200);

                // 执行验证
                VerifyAbnormalProtection(ids, batteryVolt, reportVolt);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region 验证车辆异常保护逻辑
        private void VerifyAbnormalProtection(List<int> chargerIds, double batteryVoltage, double reportVoltage)
        {
            SetCPReresh();
            // 记录数据
            string testTitle = "开关S断开";
            // 设置异常电池电压
            ControlEquipMent.BMS.BMSSetBatteryVoltage(chargerIds, batteryVoltage, new[] { "emtBMS_GB_DC" });
            Thread.Sleep(200);

            ControlEquipMent.BMS.BMS_ON(chargerIds);

            Thread.Sleep(200);
            // 等待刷卡
            WaitForSwipeCard(chargerIds, 200);

            Thread.Sleep(6000);

            var ks = GetKStatus16_Charging_DC();
            ks[22] = false;
            SendNoticeToUIAndTxtFile($"发送S断线");
            ControlEquipMent.BMS.BMSSetKState_DC(chargerIds, 1000, BatteryVoltage, ks.ToArray());
            Thread.Sleep(5000);
            // 1. 充电状态：应停止充电
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            Dictionary<int, bool> isCharging = new Dictionary<int, bool>() { { 1, bmsData.ChargingState != "充电中" } };
            ProcessDataResults(chargerIds, isCharging[1] ? "停止充电" : "允许充电",
                    "停止充电",
                    isCharging,
                    testTitle + " 充电状态");
            // 2. CC1
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, testTitle, "CC1电压", "-", "-");

            // 3. C1/C2 应断开
            var c1c2 = new Dictionary<int, bool> { { 1, bmsData.ChargingVoltage < VoltageThreshold60 } };
            ProcessDataResults(chargerIds, c1c2[1] ? "断开" : "闭合", "断开", c1c2, testTitle + " C1C2状态");

            // 4. S3/S4 应断开
            var s3s4 = new Dictionary<int, bool> { { 1, bmsData.APSVoltage < VoltageThreshold5 } };
            ProcessDataResults(chargerIds, s3s4[1] ? "断开" : "闭合", "断开", s3s4, testTitle + " S3S4状态");

            // 5. 通讯状态
            string cst = GetCANByType("CST");
            bool commOk = false;
            if (cst != null && cst.Length > CanCommunicationMinLength)
                commOk = true;

            var commState = new Dictionary<int, bool> { { 1, commOk } };
            ProcessDataResults(chargerIds, commState[1] ? "正常" : "异常", "正常", commState, testTitle + " 通讯状态");
            ProcessDataTmp(new Dictionary<int, string> { { 1, cst } }, cst, "CST 报文", "-", "-");


            // 关闭BMS
            ControlEquipMent.BMS.BMS_OFF(chargerIds);

            CountDownTimeInfo("请确认充电中充电枪插头可被解锁。\r\n(注:勾选上为可被解锁)", 15, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否可解锁");
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
                var bmsList = AllEquipStateData.DicBMS_DC_StateData
                    .Where(x => ids.Contains(x.Value.ChargerID))
                    .Select(x => x.Value)
                    .ToList().First();


                int state = ChangeBMSChargeStatus(bmsList.ChargingState);
                if (state >= 9)
                {
                    break;
                }



                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}
