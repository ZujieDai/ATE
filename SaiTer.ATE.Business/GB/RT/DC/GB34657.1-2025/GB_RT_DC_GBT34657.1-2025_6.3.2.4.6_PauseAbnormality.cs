using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 互操作_暂停异常测试(可选)
    /// 标准：GB_RT_DC_GBT34657.1-2025_6.3.2.4.5_ELockAbnormality
    /// </summary>
    public class GB_RT_DC_2025_PauseAbnormality : BusinessBase
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
        public GB_RT_DC_2025_PauseAbnormality(int type)
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
            RunAbnormalVoltageTest(chargerIds, DefaultVoltage, DefaultVoltage);
        }
        #endregion

        #region 公共测试方法
        private void RunAbnormalVoltageTest(List<int> ids, double batteryVolt, double reportVolt)
        {
            try
            {
                SetCPReresh();
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
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
            SendNoticeToUIAndTxtFile("启动录波板");
            //启动录波板
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
            Thread.Sleep(1000);

            // 记录数据
            string testTitle = "暂停异常测试";

            SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, true, MaxAllowChargeVoltage, false);
            Thread.Sleep(5000);

            SendNoticeToUIAndTxtFile("设备模拟断开车端直流供电回路的连接");
            //断开电池电压输出
            var Ks = GetKStatus16_Charging_DC();
            Ks[26] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, Ks.ToArray());

            Thread.Sleep(5000);

            SendNoticeToUIAndTxtFile("停止录波板");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 1. 充电状态：应停止充电
            var isChargingResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
            Dictionary<int, string> chargingState = isChargingResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
            ProcessDataResults(chargerIds, chargingState, "充电状态", isChargingResult, testTitle, "停止充电", "停止充电");

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

            // 5. 通讯状态
            string cst = GetCANByType("CST");
            bool commOk = false;
            if (cst != null && cst.Length > CanCommunicationMinLength)
                commOk = true;

            var commState = new Dictionary<int, bool> { { 1, commOk } };
            ProcessDataResults(chargerIds, commState[1] ? "正常" : "异常", "正常", commState, testTitle + " 通讯状态");
            ProcessDataTmp(new Dictionary<int, string> { { 1, cst } }, "暂停异常测试", "CST 报文", "-", "-");

            
            // 关闭BMS
            ControlEquipMent.BMS.BMS_OFF(chargerIds);

            CountDownTimeInfo("请确认充电中充电枪插头可被解锁。\r\n(注:勾选上为可被解锁)", 15, 2);
            ProcessDataConnect("是否可解锁", "是否可解锁");
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
                if (state >=9)
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