using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电准备就绪测试(录波板)   //GB_RT_DC_GBT34657.1-2017_6.3.2.3_ChargingReadyTestTotal_WaveRecoder
    /// </summary>
    public class GB_RT_DC_2025_ChargingReadyTestTotal_WaveRecoder : BusinessBase
    {
        private int ChargingReadyType;
        private string StatusName = "";
        int trlTimeOut_S = 30;
        double DemandVoltage = 500;
        double DemandCurrent = 20;
        double BatteryVolt = 300;
        Dictionary<int, string> CROValue2;

        public GB_RT_DC_2025_ChargingReadyTestTotal_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //a)分别模拟正常的车辆端电池电压（接触器外端电压与通信报文电池电压误差范围≤±5%且在充电机正常输出电压范围内）、
            //非正常车辆端电池电压（接触器外端电压与通信报文电池电压误差范围>±5%和/或不在充电机正常输出电压范围内），检查该阶段 K1 和 K2 状态、2)充电状态；
            //b)检查该阶段通信状态；
            //c)检查该阶段车辆接口锁止状态。
            //BMS需求电压设置(V)=750|BMS需求电流设置(A)=20|电池电压(V)=300
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 3)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                BatteryVolt = double.Parse(strParams[2].Split('=')[1]);
            }

        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");


            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s


        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartItemFlow();

            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                ControlEquipMent.BMS.SetParameter(lstIDs, MaxAllowChargeVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, BatteryVolt, MaxAllowChargeVoltage, MaxAllowChargeCurrent);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetResistance(lstIDs, 1000);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, BatteryVolt);
                Thread.Sleep(100);
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
                    testWorkParam.lstIDs.Clear();
                    for (int i = 0; i < LstTrialData.Count; i++)
                    {
                        if (LstTrialData[i].IsCheck)
                        {
                            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                            {
                                if (!testWorkParam.lstIDs.Contains(LstTrialData[i].ChargerId))
                                {
                                    testWorkParam.lstIDs.Add(LstTrialData[i].ChargerId);
                                }
                            }
                        }
                    }
                    //是否全部有结论
                    if (testWorkParam.lstIDs.Count <= 0) break;
                    //是否超时
                    if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
                    {
                        for (int i = 0; i < LstTrialData.Count; i++)
                        {
                            if (LstTrialData[i].IsCheck)
                            {
                                if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                                {
                                    LstTrialData[i].TrialResult = EmTrialResult.Fail;
                                    LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                                    int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                                    LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                                    //界面展示的数据项格式
                                    //
                                    LstTrialData[i].ExtentData = "-|-|-|-|null";
                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }

                    double[] ErrorBatteryVoltages = new double[2]
                    {

                        Math.Round(BatteryVolt * (1 - 3.5 / 100.0)),    //290
                        Math.Round(BatteryVolt * (1 + 3.5 / 100.0)),    //310
                        //Math.Round(BatteryVolt * (1 - 30 / 100.0)),   //200
                        //Math.Round(BatteryVolt * (1 + 30 / 100.0))    //400
                    };
                    for (int i = 0; i < ErrorBatteryVoltages.Length; i++)
                    {
                        ChargingReadyType = i;
                        double ErrorBatteryVoltage = ErrorBatteryVoltages[i];
                        switch (i)
                        {
                            case 0:
                                StatusName = "接触器外端电压与通信报文电池电压误差围≥-5％";
                                break;
                            case 1:
                                StatusName = "接触器外端电压与通信报文电池电压误差围≤+5％";
                                break;
                                //case 2:
                                //    StatusName = "接触器外端电压与通信报文电池电压误差围>5％";
                                //    break;
                                //case 3:
                                //    StatusName = "接触器外端电压与通信报文电池电压误差围<-5％";
                                //    break;
                        }
                        TrialMethod(ErrorBatteryVoltage);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void TrialMethod(double ErrorBatteryVoltage)
        {
            // 模拟插拔枪
            SetCPReresh();

            SendNoticeToUIAndTxtFile("设置导引参数中...");
            //StorageBatteryVoltage = ErrorBatteryVoltage;
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
            System.Threading.Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, ErrorBatteryVoltage, MaxAllowChargeVoltage, MaxAllowChargeCurrent);
            //CountDownTimeInfo("已设置报文电池电压【" + ErrorBatteryVoltage.ToString() + "】", 100, 2);
            System.Threading.Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);

            System.Threading.Thread.Sleep(200);
            ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVolt);
            //CountDownTimeInfo("已设置实际输出电池电压【" + BatteryVolt.ToString() + "】", 100, 2);
            System.Threading.Thread.Sleep(200);
            List<bool> Ks = GetKStatus16_Charging_DC();
            Ks[27] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVolt, Ks.ToArray());
            System.Threading.Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            System.Threading.Thread.Sleep(200);
            Ks[27] = true;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVolt, Ks.ToArray());

            d1 = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                d1.Add(testWorkParam.lstIDs[i], ErrorBatteryVoltage.ToString());
            }
            ProcessDataTmp(d1, StatusName, "BCP电池电压(V)", "-", "-");
            d1 = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                d1.Add(testWorkParam.lstIDs[i], BatteryVolt.ToString());
            }
            ProcessDataTmp(d1, StatusName, "蓄电池电压(V)", "-", "-");

            SendNoticeToUIAndTxtFile("录波板启动录波...");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

            SendNoticeToUIAndTxtFile("启动充电中");
            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = 200;
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;

                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 9);
                if (ALLCanCharge)
                {
                    MessgaeInfo(false, "请刷卡充电!");
                    break;
                }

                Thread.Sleep(1000);
            }
            MessgaeInfo(false, "请刷卡充电!");


            CountDownTimeInfo("请确认电子锁是否可靠锁止。\r\n注：勾选上为可靠锁止", 20, 2);
            ProcessDataConnect(StatusName, "是否可靠锁止");

            WaitDCVoltage(lstIDs, DemandVoltage, 15);

            SendNoticeToUIAndTxtFile("判断结果中...");


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);

            ProcessData();

            SendNoticeToUIAndTxtFile("关闭导引中!");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



        }

        public override void ProcessData()
        {
            VerifyProtectionLogic(lstIDs);

            //读取录波板数据
            WaveData CH_BCPBatteryVoltage = new WaveData();
            WaveData CH_OutputVoltage = new WaveData();
            WaveData CH_OutputCurrent = new WaveData();
            WaveData CH_CROReadyState = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 23, 1, ref CH_BCPBatteryVoltage, "BCP_BatteryVoltage");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 30, 1, ref CH_CROReadyState, "CRO_ReadyState");

            double CROValueEnd = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CROReadyState, CH_CROReadyState.LinePoints_Y.Count - 2);


            var dicImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
            foreach (var item in testWorkParam.lstIDs)
            {
                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LstTrialData[k].ItemName = iIndex.ToString();

                string passValue = "0";
                if (ChargingReadyType == 0 || ChargingReadyType == 1)//范围内
                {
                    passValue = "170";
                    if (CROValueEnd != 170)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应能充电(CRO-AA)|{passValue}|{passValue}|" + CROValueEnd + "|" + dicImgs[item];
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应能充电(CRO-AA)|{passValue}|{passValue}|" + CROValueEnd + "|" + dicImgs[item];
                    }
                }
                else
                {
                    if (CROValueEnd == 170)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[i].ExtentData = $"{StatusName}|超范围应不能充电(CRO-00)|{passValue}|{passValue}|" + CROValueEnd + "|" + dicImgs[item];
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[i].ExtentData = $"{StatusName}|超范围应不能充电(CRO-00)|{passValue}|{passValue}|" + CROValueEnd + "|" + dicImgs[item];
                    }
                }
                //界面展示的数据项格式
                //状态|数据名称|测量值|上限|下限|结果
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
                iIndex++;
            }
        }

        private void VerifyProtectionLogic(List<int> chargerIds)
        {

            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS数据获取失败，测试中止");
                return;
            }
            if (ChargingReadyType == 0 || ChargingReadyType == 1)
            {
                // 1. 充电状态：必须停止充电
                Dictionary<int, string> chargeStateValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingState);
                Dictionary<int, EmTrialResult> chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingState == "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                ProcessDataResults(chargerIds, chargeStateValue, "充电状态", chargeStateResult, StatusName, "允许充电", "允许充电");

                // 3. C1/C2 
                Dictionary<int, string> c1c2Result = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "闭合" : "断开");
                ProcessDataResults(chargerIds, c1c2Result, "C1C2状态", chargeStateResult, StatusName, "闭合", "闭合");


                Dictionary<int, string> commResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "异常");
                ProcessDataResults(chargerIds, commResult, "通讯状态", chargeStateResult, StatusName, "正常", "正常");
            }
            else
            {
                // 1. 充电状态：必须停止充电
                Dictionary<int, string> chargeStateValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingState);
                Dictionary<int, EmTrialResult> chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingState != "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                ProcessDataResults(chargerIds, chargeStateValue, "充电状态", chargeStateResult, StatusName, "停止充电", "停止充电");

                // 3. C1/C2 
                Dictionary<int, string> c1c2Result = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "闭合" : "断开");
                ProcessDataResults(chargerIds, c1c2Result, "C1C2状态", chargeStateResult, StatusName, "断开", "断开");


                Dictionary<int, string> commResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "异常");
                ProcessDataResults(chargerIds, commResult, "通讯状态", chargeStateResult, StatusName, "正常", "正常");
            }
        }
    }
}
