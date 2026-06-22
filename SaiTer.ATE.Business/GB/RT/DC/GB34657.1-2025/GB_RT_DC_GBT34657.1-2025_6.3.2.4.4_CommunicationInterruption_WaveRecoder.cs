using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 通信中断测试（34657.1-2025A类 6.3.2.4.4）
    /// </summary>
    public class GB_RT_DC_2025_CommunicationInterruption_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 500;
        private const int HighVoltage = 750;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        #endregion

        #region 字段
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_CommunicationInterruption_WaveRecoder(int type)
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
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(200);
            SetConditionValues();
            RunAbnormalVoltageTest(testWorkParam.lstIDs, 390, 500);
        }
        #endregion

        #region 公共测试方法
        private void RunAbnormalVoltageTest(List<int> ids, double batteryVolt, double reportVolt)
        {
            try
            {
                SetCPReresh();

                // 握手
                ControlEquipMent.BMS.SetParameter(ids, HighVoltage);
                Thread.Sleep(200);

                // 参数配置
                ControlEquipMent.BMS.SetParameter(ids, 460, 500, 3);
                Thread.Sleep(200);

                // 充电需求
                ControlEquipMent.BMS.SetParameter(ids, 460, 3, true, batteryVolt);
                Thread.Sleep(200);

                // 执行验证
                VerifyAbnormalProtection();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region 验证车辆异常保护逻辑
        private void VerifyAbnormalProtection()
        {

            int iClcs = 0;
            while (iClcs == 0)
            {
                CountDownTimeInfo("请输入重连次数", 999, 3);
                try
                {
                    int.TryParse(InputData, out iClcs);

                }
                catch (Exception ex)
                {
                }
            }

            Thread.Sleep(200);

            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            
            Thread.Sleep(200);
            // 等待刷卡
            WaitForSwipeCard(testWorkParam.lstIDs, 200);
            //进入能量传输阶段
            //WaitEnergyTransmission(15);
            //
            bool Reconnection = ReconnectionTest(iClcs);
            // 关闭BMS
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);

            if (Reconnection)
            {
                ExternalVoltageReconnection(10);
                ExternalVoltageReconnection(100);
            }


            CountDownTimeInfo("请确认充电中充电枪插头不可被解锁。\r\n(注:勾选上为不可被解锁)", 15, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否不可解锁");
            Thread.Sleep(1000);


        }

        private void ExternalVoltageReconnection(int v)
        {


            Thread.Sleep(200);

            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

            Thread.Sleep(200);
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);

            // 等待刷卡
            WaitForSwipeCard(testWorkParam.lstIDs, 200);
            //模拟通讯中断S+，S-断开
            double R4 = 0;
            double volt = 0;
            var ks = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out R4, out volt)[0];
            ks[21] = false;
            ks[20] = false;
            ks[31] = false;
            if (v >= 60)
                ks[26] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, ks.ToArray());
            Thread.Sleep(3000);
            // 模拟重连：S+、S-短接恢复
            SendNoticeToUIAndTxtFile($"模拟重连：S+、S-恢复");

            ks[21] = true;
            ks[20] = true;
            ks[31] = true;
            if (v >= 60)
                ks[26] = true;

            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, ks.ToArray());
            Thread.Sleep(2000);
            // 等待进入充电中
            var Charging = WaitCharging(15);

            var testTitle = v > 60 ? $"外侧电压大于60V" : "外侧电压小于60V";

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
            var img = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            var rest = false;
            var vaule = "";
            if (v < 60)
            {
                if (Charging)
                {
                    rest = true;
                    vaule = "恢复充电";
                }
                else
                {
                    rest = false;
                    vaule = "不允许充电";
                }
            }
            else
            {
                if (Charging)
                {
                    rest = false;
                    vaule = "恢复充电";
                }
                else
                {
                    rest = true;
                    vaule = "不允许充电";
                }
            }
           
            ProcessDataResults(testWorkParam.lstIDs, vaule, v >60 ? "不允许充电": "恢复充电", new Dictionary<int, bool> { { 1, rest } }, testTitle);


            // 1. 充电状态：应停止充电
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            var waveData = new WaveData();
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData, "充电电压");
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref waveData, "充电电流");
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref waveData, "辅源电压");
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref waveData, "K1K2");

            // 2. CC1
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, testTitle, "CC1电压", "-", "-");
            // 2. CC1
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC2Voltage.ToString() } }, testTitle, "CC2电压", "-", "-");
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.ChargingVoltage.ToString() } }, testTitle, "充电电压", "-", "-");
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.APSVoltage.ToString() } }, testTitle, "辅源电压", "-", "-");


            // 4. S3/S4 应闭合
            var S3S4rest =false; var C1C2rest = false;
           var S3S4vaule = ""; var C1C2vaule = "";
            if (v < 60)
            {
                if (bmsData.APSVoltage < VoltageThreshold5)
                {
                    rest = false;
                    vaule = "断开";
                }
                else
                {
                    rest = true;
                    vaule = "闭合";
                }

                if (bmsData.ChargingVoltage < VoltageThreshold60)
                {
                    C1C2rest = false;
                    C1C2vaule = "断开";
                }
                else
                {
                    C1C2rest = true;
                    C1C2vaule = "闭合";
                }
            }
            else
            {
                if (bmsData.APSVoltage < VoltageThreshold5)
                {
                    rest = true;
                    vaule = "断开";
                }
                else
                {
                    rest = false;
                    vaule = "闭合";
                }
                if (bmsData.ChargingVoltage < VoltageThreshold60)
                {
                    C1C2rest = false;
                    C1C2vaule = "断开";
                }
                else
                {
                    C1C2rest = true;
                    C1C2vaule = "闭合";
                }
            }
            // 3. C1/C2
            var c1c2 = new Dictionary<int, bool> { { 1, C1C2rest } };
            ProcessDataResults(testWorkParam.lstIDs, C1C2vaule, v > 60 ? "断开" : "闭合", c1c2, testTitle + " C1C2状态");

            var s3s4 = new Dictionary<int, bool> { { 1, S3S4rest } };
            ProcessDataResults(testWorkParam.lstIDs, S3S4vaule, v > 60 ? "断开" : "闭合", s3s4, testTitle + " S3S4状态");

            // 5. 通讯状态
            var commState = new Dictionary<int, bool> { { 1, v > 60? Charging==false: Charging == true } };

            ProcessDataResults(testWorkParam.lstIDs, commState[1] ? "异常" : "正常", "通讯正常", commState, testTitle + " 通讯状态");
            ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, "重新连接响应测试", "时序图", "-", "-", img);

        }

        private bool ReconnectionTest(int iClcs)
        {

            for (int i = 0; i < iClcs; i++)
            {
                var testTitle = $"第{i+1}次重连";
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                SendNoticeToUIAndTxtFile($"第{i + 1}次重连...");

                Thread.Sleep(3000);
                SendNoticeToUIAndTxtFile($"断开S+-,关闭CAN报文发送");

                //模拟通讯中断S+，S-断开
                var ks = GetKStatus16_Charging_DC();
                ks[21] = false;
                ks[20] = false;
                ks[31] = false;

                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, ks.ToArray());
                Thread.Sleep(3000);

                // 1. 充电状态：应停止充电
                BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
                var waveData1 = new WaveData();
                var waveData2 = new WaveData();
                var waveData3 = new WaveData();
                var waveData8 = new WaveData();

                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData1, "充电电压");
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref waveData2, "充电电流");
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref waveData3, "辅源电压");
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref waveData8, "K1K2");

                Dictionary<int, bool> isCharging = new Dictionary<int, bool>() { { 1, bmsData.ChargingState != "充电中" } };
                ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "停止充电" : "允许充电",
                        "停止充电",
                        isCharging,
                        testTitle + " 充电状态");
                // 2. CC1
                ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, testTitle, "CC1电压", "-", "-");

                // 3. C1/C2 应断开
                var c1c2 = new Dictionary<int, bool> { { 1, bmsData.ChargingState != "充电中" } };
                ProcessDataResults(testWorkParam.lstIDs, c1c2[1] ? "断开" : "闭合", "断开", c1c2, testTitle + " C1C2状态");

                // 4. S3/S4 应闭合
                var s3s4 = new Dictionary<int, bool> { { 1, bmsData.ChargingState != "充电中" } };
                ProcessDataResults(testWorkParam.lstIDs, s3s4[1] ? "闭合" : "断开", "闭合", s3s4, testTitle + " S3S4状态");

                // 5. 通讯状态
                var commState = new Dictionary<int, bool> { { 1, bmsData.ChargingState != "充电中" } };

                ProcessDataResults(testWorkParam.lstIDs, commState[1] ? "异常" : "正常", "异常", commState, testTitle + " 通讯状态");

                Thread.Sleep(3000);
                // 模拟重连：S+、S-短接恢复
                SendNoticeToUIAndTxtFile($"模拟重连：S+、S-恢复");

                ks[21] = true;
                ks[20] = true;
                ks[31] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, ks.ToArray());

                SendNoticeToUIAndTxtFile($"等待30秒充电桩重连");
                Dictionary<int, string> img;
                if (WaitCharging(30))
                {
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    img = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);

                    ProcessDataResults(testWorkParam.lstIDs, "重连成功", "重连成功", new Dictionary<int, bool> { { 1,true} }, testTitle + " 是否重连");
                    ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, testTitle, "时序图", "-", "-",img);
                    SendNoticeToUIAndTxtFile($"第{i + 1}次重连成功!");
                    continue;
                }
                else
                {
                    img = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    ProcessDataResults(testWorkParam.lstIDs, "重连失败", "重连成功", new Dictionary<int, bool> { { 1, false } }, testTitle + " 是否重连");
                    ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, testTitle, "时序图", "-", "-", img);
                    SendNoticeToUIAndTxtFile($"第{i + 1}次重连失败!,未能进入充电中");
                    return false;
                }

            }
            return true;
        }

        private bool WaitCharging(int v)
        {
            for (int i = 0; i < v; i++) 
            {
                if (AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault().ChargingState == "充电中")
                {
                    return true;
                }
                Thread.Sleep(1000);
            }
             return false;
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
