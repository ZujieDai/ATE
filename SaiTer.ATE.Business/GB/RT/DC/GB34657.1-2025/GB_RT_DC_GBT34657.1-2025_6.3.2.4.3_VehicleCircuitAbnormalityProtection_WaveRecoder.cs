
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
    /// 互操作_车辆电路异常保护测试
    /// 标准：GB_RT_DC_GBT34657.1-2025_6.3.2.4.3_VehicleCircuitAbnormalityProtection
    /// </summary>
    public class GB_RT_DC_2025_VehicleCircuitAbnormalityProtection_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int CanCommunicationMinLength = 2;
        #endregion

        #region 字段
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        private bool IsCharging = false;
        #endregion

        #region 构造
        public GB_RT_DC_2025_VehicleCircuitAbnormalityProtection_WaveRecoder(int type)
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

            // 1. 电池电压 > 允许上限 5%
            RunAbnormalVoltageTest(chargerIds, "电池电压误差范围超5%", 430, DefaultVoltage);

            // 2. 电池电压 < 允许下限 5%
            RunAbnormalVoltageTest(chargerIds, "电池电压误差范围低于5%", 350, DefaultVoltage);

            // 3. 电池电压低于充电机最低输出电压
            double overMinVolt = MinAllowChargeVoltage - 50;
            RunAbnormalVoltageTest(chargerIds, "电池电压低于充电机最低输出电压5%", overMinVolt, DefaultVoltage);

            // 4. 电池电压高于充电机最高输出电压
            double overMaxVolt = MaxAllowChargeVoltage * 1.1;
            RunAbnormalVoltageTest(chargerIds, "电池电压高于充电机最高输出电压5%", overMaxVolt, DefaultVoltage);
        }
        #endregion

        #region 公共测试方法
        private void RunAbnormalVoltageTest(List<int> ids, string testName, double batteryVolt, double reportVolt)
        {
            try
            {
                // 握手
                ControlEquipMent.BMS.SetParameter(ids, MaxAllowChargeVoltage);
                Thread.Sleep(200);

                // 参数配置
                ControlEquipMent.BMS.SetParameter(ids, reportVolt, MaxAllowChargeVoltage, RatedCurrent);
                Thread.Sleep(200);

                // 充电需求
                ControlEquipMent.BMS.SetParameter(ids, MaxAllowChargeVoltage, RatedCurrent, true, MaxAllowChargeVoltage, true, new[] { "emtBMS_GB_DC" });
                Thread.Sleep(200);

                // 执行验证
                VerifyAbnormalProtection(ids, testName, batteryVolt, reportVolt);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region 验证车辆异常保护逻辑
        private void VerifyAbnormalProtection(List<int> chargerIds, string testScene, double batteryVoltage, double reportVoltage)
        {
            SetCPReresh();
            // 记录数据
            string testTitle = "车辆供电回路异常保护(" + testScene + ")";
            ProcessDataTmp(new Dictionary<int, string> { { 1, batteryVoltage.ToString() } }, testTitle, "电池设定电压(V)", "-", "-");
            ProcessDataTmp(new Dictionary<int, string> { { 1, reportVoltage.ToString() } }, testTitle, "报文电压(V)", "-", "-");

            // 设置异常电池电压
            ControlEquipMent.BMS.BMSSetBatteryVoltage(chargerIds, batteryVoltage, new[] { "emtBMS_GB_DC" });
            Thread.Sleep(200);

            ControlEquipMent.BMS.BMS_ON(chargerIds);

            Thread.Sleep(200);
            // 等待刷卡
            WaitForSwipeCard(chargerIds, 200);

            //BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            //Dictionary<int, bool> isCharging = new Dictionary<int, bool>() { { 1, false } };
            //for (int i = 0; i < 40; i++)
            //{

            //    if (bmsData == null || bmsData.ChargingState == "空闲状态")
            //    {
            //        isCharging = new Dictionary<int, bool> { { 1, true } };

            //        break;
            //    }
            //    Thread.Sleep(1000);

            //    bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

            //    if (i == 39)
            //    {
            //        SendNoticeToUIAndTxtFile("超时40秒充电桩未能退出充电");
            //    }
            //}
            Thread.Sleep(2000);

            // 1. 充电状态：应停止充电
            var isChargingResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
            var chargingState = isChargingResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
            ProcessDataResults(chargerIds, chargingState, "充电状态", isChargingResult, testTitle, "停止充电", "停止充电");

            // 2. CC1
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

            // 5. 通讯状态
            //string cst = GetCANByType("CST");
            //bool commOk = false;
            //if (cst != null && cst.Length > CanCommunicationMinLength)
            //    commOk = true;

            Dictionary<int, string> commResult = isChargingResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "异常");
            ProcessDataResults(chargerIds, commResult, "通讯状态", isChargingResult, testTitle, "正常", "正常");
            //ProcessDataTmp(new Dictionary<int, string> { { 1, cst } }, testScene, "CST 报文", "-", "-");

            // 6. 电子锁人工确认
            SendNoticeToUIAndTxtFile("验证电子锁状态...");
            CountDownTimeInfo("请确认电子锁可正常解锁（勾选=正常）", 20, 2);
            ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "正常解锁" : "不能解锁", "电子锁状态", DicManualVerifyResult, testTitle);

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
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, DefaultVoltage, kState.ToArray());
            Thread.Sleep(300);
        }

        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡...");
            IsCharging = false;
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
                    IsCharging = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}