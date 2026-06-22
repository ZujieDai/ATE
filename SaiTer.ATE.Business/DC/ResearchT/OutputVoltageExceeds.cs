using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using SaiTer.ATE.InterFace;
using System.Configuration;


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:输出电压超过车辆允许值测试、蓄电池二重保护功能试验
    /// </summary>

    public class OutputVoltageExceeds : BusinessBase
    {
        string sState = "";
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 550;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 20;
        /// <summary>
        /// 过压增量
        /// </summary>
        double OverVoltDiff = 100;


        public OutputVoltageExceeds(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {

            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet(  "Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道

            canchannelopen[3] = true;//CAN4通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");

            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();

            sState = TrialType == (int)EmTrialType.输出电压超过车辆允许值测试 ? "输出电压超过车辆允许值测试" : "蓄电池二重保护功能试验";
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length >= 2)
            {
                OverVoltDiff = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            DemandVoltage = DemandVoltage + 20 > MaxAllowChargeVoltage ? (MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2 : DemandVoltage;

            if (DemandVoltage < 390 && !(Customer != null && Customer.Contains("DH")))
                BatteryVoltage = DemandVoltage - 10;
            else
                BatteryVoltage = 390;

        }
        public override void ExecuteMethod()
        {
            try
            {
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, true);

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
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);

                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, BatteryVoltage);
                Thread.Sleep(1000);
                List<bool> Ks = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVoltage, Ks.ToArray());
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
                    SetConditionValues();


                    double Overvoltage = DemandVoltage + OverVoltDiff;

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dic.Add(item, DemandVoltage.ToString());
                    }
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dicC.Add(item, DemandCurrent.ToString());
                    }
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, Overvoltage.ToString());
                    }
                    d2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d2.Add(item, (DemandVoltage).ToString());
                    }
                    ProcessDataTmp(dic, "充电设置", "电压需求(V)", "-", "-");
                    ProcessDataTmp(dicC, "充电设置", "电流需求(A)", "-", "-");
                    ProcessDataTmp(d1, "充电设置", "过压电压(V)", "-", "-"); 
                    ProcessDataTmp(d2, "充电设置", "车辆最高允许充电总电压(V)", "-", "-");



                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, DemandVoltage))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    Thread.Sleep(5000);


                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach(int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, "模拟过压前充电", "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, "模拟过压前充电", "输出电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(DemandVoltage + 5, 4, 0, "RISE", false, 50);
                    Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("发送过压命令中...");
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        SetLoadPara(testWorkParam.lstIDs, Overvoltage, DemandCurrent + 10, Overvoltage, DemandCurrent + 10);
                    }
                    else
                    {
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, Overvoltage);
                        Thread.Sleep(1000);
                        List<bool> Ks = GetKStatus16_Charging_DC();
                        Ks[26] = true;
                        ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, Overvoltage, Ks.ToArray());
                    }

                    ReadTriggerTypeOscillograph(30);

                    SendNoticeToUIAndTxtFile("设置卡点位置中...");
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 0.5, true);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    double Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));


                    Dictionary<int, string> dValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dValue.Add(item, Value15_4.ToString("F2"));
                    }


                    //ProcessDataTmp(dValue, TrialItem.ItemName , " CST电压异常报文", "-", "-");
                    ProcessDatamessage(Value15_4.ToString());

                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    //double position1 = 0;
                    //if (K1K2s2 > 2)
                    //{
                    //    position1 = GetTriggerTime_Single_VoltageExceeds(7, false, 0, 6, 0, false, false, 0.05);
                    //}
                    //else
                    //{
                    //    position1 = GetTriggerTime_Single_VoltageExceeds(7, false, 0, 6, 1, false, false, 0.05);
                    //}

                    //double position2 = 0;
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    position2 = GetTriggerTime_Single_VoltageExceeds(4, false, 0, Overvoltage, 0, false, false, 0.05);
                    //}
                    //else
                    //{
                    //    position2 = GetTriggerTime_Single_VoltageExceeds(4, false, 0, DemandVoltage - 10, 1, false, false, -0.01);
                    //}
                    //position2 = GetTriggerTime_Single_VoltageExceeds(4, false, 0, Overvoltage - 10, 0, false, true, 0.05);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(4, false, 0, 0, true);  //电压上升触发位置

                    if (ControlEquipMent.FeedbackLoad == null && ControlEquipMent.LoopFeedbackLoad == null && ControlEquipMent.StarLoopFeedbackLoad == null)
                    {
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                        Thread.Sleep(1000);
                        List<bool> Ks = GetKStatus16_Charging_DC();
                        ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, Overvoltage, Ks.ToArray());
                    }

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, "模拟过压后充电", "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, "模拟过压后充电", "输出电流(A)", "-", "-");


                    //double time = System.Math.Abs(GetTriggerTime_Position(1, false, 0, position1, position2));
                    double time = 0;
                    if (K1K2s2 > 2)
                    {
                        time = GetTriggerTime_Single(7, false, 0, 6, 1, false, false, 0.05, true);
                    }
                    else
                    {
                        time = GetTriggerTime_Single(7, false, 0, 6, 0, false, false, 0.05, true);
                    }

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_XY(4, false, 0);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(4, Overvoltage, DemandVoltage - 5);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    Dictionary<int, string> dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }
                    ProcessDataTmp(dTime, sState, " K1K2下降时间(ms)", "0", "1000", dImgs);

                    if (TrialType == (int)EmTrialType.输出电压超过车辆允许值测试)
                    {
                        //double position3 = GetTriggerTime_Single_VoltageExceeds(2, false, 0, 6, 1, false, false, 0.05);
                        //double time2 = System.Math.Abs(GetTriggerTime_Position(1, false, 0, position2, position3));
                        double time2 = GetTriggerTime_Single(2, false, 0, 6, 0, false, false, 0.05, true);
                        ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_XY(4, false, 0);
                        ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(4, Overvoltage, DemandVoltage - 5);

                        Dictionary<int, string> dImgs2 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                        Dictionary<int, string> dTime2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {

                            dTime2.Add(item, (time2 * 1000).ToString("F2"));
                        }
                        ProcessDataTmp(dTime2, sState, " K3K4下降时间(ms)", "0", "1000", dImgs2);
                    }

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    ProcessDataConnect(sState, "是否报警");
                    CountDownTimeInfo("请确认桩电子锁是否能正常解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    ProcessDataConnect(sState);

                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.ToString().ToUpper().Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", null, TrialItem.ItemName);
                    }

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

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


                    if (value!="1")
                    {

                        LstTrialData[k].TrialResult = EmTrialResult.Fail;

                        LstTrialData[i].ExtentData = TrialItem.ItemName + "|CST电压异常报文|-|-|" + value + "|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                        LstTrialData[i].ExtentData = TrialItem.ItemName + "|CST电压异常报文|-|-|" + value + "|报表(勿删)";
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

        }
        public  void ProcessDataWarn()
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


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|是" + "|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|否" + "|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

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
        public void ProcessDataConnect()
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


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|正常解锁|-|-|是" + "|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|正常解锁|-|-|否" + "|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

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
    }
}
