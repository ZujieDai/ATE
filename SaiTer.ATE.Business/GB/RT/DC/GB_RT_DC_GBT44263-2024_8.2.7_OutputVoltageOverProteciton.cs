using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出过压保护
    /// </summary>
    public class GB_RT_DC_OutputVoltageOverProteciton : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double OutputVoltage = 700;
        double BmsDemandVoltage = 220;
        /// <summary>
        /// 需求电流
        /// </summary>
        double DemandCurrent = 20;



        public GB_RT_DC_OutputVoltageOverProteciton(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //需求电压(V)=500|需求电流(A)=20|过压参考值(V)=800
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputVoltage = Convert.ToDouble(strParams[2].Split('=')[1]);
            BmsDemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
        }
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道

            canchannelopen[3] = true;//CAN4通道
            //canchannelopen[4] = true;//CAN5通道
            //canchannelopen[5] = true;//CAN6通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");

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
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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
                                //测试时间|输入电压|输出电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //开始检测流程
                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SetConditionValues();

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
                Thread.Sleep(500);
                if (BmsDemandVoltage < 390 && !(Customer != null && Customer.Contains("DH")))
                {
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BmsDemandVoltage - 10);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BmsDemandVoltage, MaxAllowChargeVoltage, MaxAllowChargeCurrent);
                }
                else
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BmsDemandVoltage, 250, true, BmsDemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BmsDemandVoltage, LstChargerInfo[0].ChargerType, 0);
                //检查刷卡上电
                double Voltage = 0;
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    Voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingVoltage;
                }
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[0] + "号枪处于充电状态，该枪停止检测");
                    return;
                }
                //if (!CheckSwipingCard(testWorkParam.lstIDs, OutputVoltage - 50))
                //{
                //    return;
                //}

                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage - 50, 120, true, OutputVoltage - 50);
                //Thread.Sleep(3000);

                //ControlEquipMent.FeedbackLoad?.SetFeedbackLoadParams(testWorkParam.lstIDs, OutputVoltage - 50 - 20, 20);
                SendNoticeToUIAndTxtFile("开启负载中...");
                SetLoadPara(testWorkParam.lstIDs, BmsDemandVoltage - 10, DemandCurrent + 10, BmsDemandVoltage - 5, DemandCurrent);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                Thread.Sleep(5000);

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                    d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                }
                ProcessDataTmp(d1, "过压保护前正常充电", "充电电压(V)", "-", "-");
                ProcessDataTmp(d2, "过压保护前正常充电", "充电电流(A)", "-", "-");

                SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                OscillographInstrument_SetTrigger(BmsDemandVoltage + 10, 4, 0, "RISE", false, 20);
                Thread.Sleep(2000);

                //ControlEquipMent.FeedbackLoad?.FeedbackLoad_ON(testWorkParam.lstIDs);
                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("发送输出电压异常值：" + OutputVoltage + "V，等待输出稳定。");
                    SetLoadPara(testWorkParam.lstIDs, OutputVoltage, DemandCurrent + 10, OutputVoltage, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 10);
                }
                else
                {
                    SendNoticeToUIAndTxtFile("模拟输出过压");
                    List<bool> Ks = GetKStatus16_Charging_DC();

                    Ks[26] = true;
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, OutputVoltage);
                    Thread.Sleep(3000);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, OutputVoltage, Ks.ToArray());
                }
                //ControlEquipMent.FeedbackLoad?.SetFeedbackLoadParams(testWorkParam.lstIDs, OutputVoltage, 20);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double outputVolt = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                    int timeout = 100;
                    while (timeout-- > 0)
                    {
                        double outputVoltNew = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        if (outputVoltNew > outputVolt)
                            outputVolt = outputVoltNew;
                    }
                    d1.Add(item, outputVolt.ToString("F2"));
                }
                ProcessDataTmp(d1, "模拟过压时", "直流输出电压(V)", "-", "-");

                ReadTriggerTypeOscillograph(30);

                // 关闭回馈负载
                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                {
                    SendNoticeToUIAndTxtFile("恢复正常电压：" + (BmsDemandVoltage - 20) + "V，等待直流输出稳定。");
                    SetLoadPara(testWorkParam.lstIDs, BmsDemandVoltage - 20, 20, OutputVoltage - 50, 20);

                    Thread.Sleep(500);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                }
                // 断开过压开关
                List<bool> Ks1 = GetKStatus16_Charging_DC();
                Ks1[26] = false;
                if (BmsDemandVoltage < 390 && !(Customer != null && Customer.Contains("DH")))
                {
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BmsDemandVoltage - 10);
                    Thread.Sleep(3000);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BmsDemandVoltage - 10, Ks1.ToArray());
                }
                else
                {
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(3000);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks1.ToArray());
                }

                //WaitDCVoltage(testWorkParam.lstIDs, BmsDemandVoltage);
                Thread.Sleep(1000 * 5);

                SendNoticeToUIAndTxtFile("设置卡点位置中...");
                ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(4, false, 0, -3, true);  //电压上升触发位置
                double time = GetTriggerTime_Single(15, true, 4, 1, 1, false, false, 0.05, true);

                SendNoticeToUIAndTxtFile("判断结果中...");
                double Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));
                Dictionary<int, string> dValue = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {

                    dValue.Add(item, Value15_4.ToString("F2"));
                }
                //ProcessDataTmp(dValue, TrialItem.ItemName , " CST电压异常报文", "-", "-");
                ProcessDatamessage(Value15_4.ToString());

                Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                Dictionary<int, string> dTime = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    dTime.Add(item, (time * 1000).ToString("F2"));
                }
                ProcessDataTmp(dTime, "输出过压保护", " 过压和故障检测时间(ms)", "400", "1400", dImgs);

                GetTriggerTime_Single(15, true, 4, 1, 1, false, true, 0.05, true);
                time = GetTriggerTime_Single(1, false, 0, 5, 0, false, false, 0.05, true);
                dTime = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    dTime.Add(item, (time * 1000).ToString("F2"));
                }
                dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                if (DemandCurrent <= 200)
                    ProcessDataTmp(dTime, "输出过压保护", " 电流下降时间(ms)", "0", "2000", dImgs);
                else
                    ProcessDataTmp(dTime, "输出过压保护", " 电流下降时间(ms)", "0", "3000", dImgs);
            }
        }


        public void ProcessDatamessage(string value)
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (value != "1")
                    {

                        LstTrialData[k].TrialResult = EmTrialResult.Fail;

                        LstTrialData[i].ExtentData = "输出过压保护|CST电压异常报文|-|-|" + value + "|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                        LstTrialData[i].ExtentData = "输出过压保护|CST电压异常报文|-|-|" + value + "|报表(勿删)";
                    }


                    //界面展示的数据项格式   

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
                    string strResult = "未停机";
                    if (volate < 50)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "已停机";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //测试时间|BMS需求电压|输出电压|是否停机|测试结果     
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|" + strResult;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
