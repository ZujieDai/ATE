using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电准备就绪测试
    /// </summary>
    public class GB_PT_DC_ChargingReady : BusinessBase
    {
        private int ChargingReadyType;
        private string StatusName = "";
        int trlTimeOut_S = 30;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        double BatteryVolt = 300;
        Dictionary<int, string> CROValue2;
        Dictionary<int, string> ChargingVoltage;
        Dictionary<int, string> ChargingState;

        public GB_PT_DC_ChargingReady(int type)
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

            //ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("Y");
            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            //bool[] channelopen = new bool[8];
            //bool[] canchannelopen = new bool[20];
            //channelopen[3] = true;//4通道
            //channelopen[6] = true;//7通道
            //channelopen[7] = true;//8通道
            //canchannelopen[11] = true;//CAN12通道
            //canchannelopen[12] = true;//CAN13通道
            //SetChannel(channelopen, canchannelopen);
            //ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
            //ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

            // 模拟插拔枪
            SetCPReresh();
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

                    double[] ErrorBatteryVoltages = new double[4]
                    {
                        Math.Round(BatteryVolt * (1 - 2.5 / 100.0)),    //285
                        Math.Round(BatteryVolt * (1 + 2.5 / 100.0)),    //315
                        Math.Round(BatteryVolt * (1 - 30 / 100.0)),   //200
                        Math.Round(BatteryVolt * (1 + 30 / 100.0))    //400
                    };
                    for (int i = 0; i < ErrorBatteryVoltages.Length; i++)
                    {
                        ChargingReadyType = i;
                        double ErrorBatteryVoltage = ErrorBatteryVoltages[i];
                        switch (i)
                        {
                            case 0:
                                StatusName = "接触器外端电压与通信报文电池电压误差围≤5％";
                                break;
                            case 1:
                                StatusName = "接触器外端电压与通信报文电池电压误差围≥-5％";
                                break;
                            case 2:
                                StatusName = "接触器外端电压与通信报文电池电压误差围>5％";
                                break;
                            case 3:
                                StatusName = "接触器外端电压与通信报文电池电压误差围<-5％";
                                break;
                        }
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(500);
                        SetCPReresh();
                        TrialMethod(ErrorBatteryVoltage);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void TrialMethod(double ErrorBatteryVoltage)
        {

            SetLoadDCOFF(testWorkParam.lstIDs);
            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
            //ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
            Thread.Sleep(1000);

            SendNoticeToUIAndTxtFile("设置导引参数中...");
            //StorageBatteryVoltage = ErrorBatteryVoltage;
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
            System.Threading.Thread.Sleep(500);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, ErrorBatteryVoltage, MaxAllowChargeVoltage, MaxAllowChargeCurrent);
            System.Threading.Thread.Sleep(500);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
            System.Threading.Thread.Sleep(500);
            ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
            System.Threading.Thread.Sleep(500);
            ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVolt);
            System.Threading.Thread.Sleep(500);
            List<bool> Ks = GetKStatus16_Charging_DC();
            Ks[27] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVolt, Ks.ToArray());
            System.Threading.Thread.Sleep(500);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            System.Threading.Thread.Sleep(500);
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

            //SendNoticeToUIAndTxtFile("设置录波仪触发中...");
            //ControlEquipMent.Oscillograph?.Oscillograph_Trigger("RISE", 7, false, 0, "6", "Auto", 50);

            SendNoticeToUIAndTxtFile("启动充电中");
            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = 6000;
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) <= 9);
                if (ALLCanCharge)
                {
                    MessgaeInfo(false, "请刷卡充电!");
                    break;
                }

                System.Threading.Thread.Sleep(50);
            }
            MessgaeInfo(false, "请刷卡充电!");
            ////SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
            //// 避免通道7提前触发停止
            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");

            CountDownTimeInfo("请确认电子锁是否可靠锁止。\r\n注：勾选上为可靠锁止", 20, 2);
            ProcessDataConnect(StatusName, "是否可靠锁止");

            //设置测试条件
            SetConditionValues();

            System.Threading.Thread.Sleep(1000);
            SendNoticeToUIAndTxtFile("判断结果中...");
            System.Threading.Thread.Sleep(1000 * 35);

            timeout = 30;

            while (timeout-- > 0)
            {
                ChargingVoltage = new Dictionary<int, string>();
                ChargingState = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    ChargingVoltage.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                    ChargingState.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState.ToString());
                }

                if (ChargingState[testWorkParam.lstIDs[0]].Contains("充电中")
                    && Convert.ToDouble(ChargingVoltage[testWorkParam.lstIDs[0]]) > DemandVoltage * 0.9)//到了充电中就退出
                {
                    Thread.Sleep(5000);//等待5s稳定
                    break;
                }
                Thread.Sleep(100);
            }


            SendNoticeToUIAndTxtFile("关闭导引中!");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
            Thread.Sleep(1000);

            ProcessData();

        }

        public override void ProcessData()
        {


            WaveData CH_OutputVoltage = new WaveData();
            WaveData CH_CROState = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 30, 0, ref CH_CROState, "CROState");

            var dicImgs = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);//截图保存
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

                string passValueMin = "0";
                string passValueMax = "20";
                if (ChargingReadyType == 0 || ChargingReadyType == 1)//范围内
                {
                    passValueMin = (DemandVoltage * 0.95).ToString();
                    passValueMax = (DemandVoltage * 1.05).ToString(); ;
                    //passValue = "170";
                    if (Convert.ToDouble( ChargingVoltage[item]) <Convert.ToDouble( passValueMin)
                        ||Convert.ToDouble(ChargingVoltage[item]) > Convert.ToDouble(passValueMax))
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        //LstTrialData[i].ExtentData = $"{StatusName}|范围内应能充电(CRO-AA)|{passValue}|{passValue}|" + CROValue2[item] + "|" + dicImgs[item];
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应能充电(CRO-AA)|{passValueMin}|{passValueMax}|" + ChargingVoltage[item] + "|" + dicImgs[item];
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应能充电(CRO-AA)|{passValueMin}|{passValueMax}|" + ChargingVoltage[item] + "|" + dicImgs[item];
                    }
                }
                else
                {
                    passValueMin = "0";
                    passValueMax = "20";
                    if (Convert.ToDouble(ChargingVoltage[item]) < Convert.ToDouble(passValueMin)
                        || Convert.ToDouble(ChargingVoltage[item]) > Convert.ToDouble(passValueMax))
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应不能充电(CRO-00)|{passValueMin}|{passValueMax}|" + ChargingVoltage[item] + "|" + dicImgs[item];
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[i].ExtentData = $"{StatusName}|范围内应不能充电(CRO-00)|{passValueMin}|{passValueMax}|" + ChargingVoltage[item] + "|" + dicImgs[item];
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
    }
}
