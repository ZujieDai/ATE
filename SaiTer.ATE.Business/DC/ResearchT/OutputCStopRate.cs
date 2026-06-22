using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
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
    /// 研发测试;输出电流停止速率
    /// </summary>
    public class OutputCStopRate : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 60;

        /// <summary>
        /// 录波仪器时间轴第一点(s)
        /// </summary>
        Double TimeBase1 = 0.2;
        /// <summary>
        /// 录波仪器时间轴第二点(s)
        /// </summary>
        Double TimeBase2 = 0.2;
        public OutputCStopRate(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {


            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


            SendNoticeToUIAndTxtFile("初始化录波仪配置中...");
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            canchannelopen[2] = true;//CAN1通道
            canchannelopen[4] = true;//CAN5通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_SetCanChild_Channel(15, 5, 13, true, "5000", "CST-stop", 0, "101AF456", "Auto", 2, 2, 0, 0, "1", "0", "", "9", "-1", "PURPle", true, false, false, false);

            OscillographInstrumentStopRate_Gear(DemandCurrent);

            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase(TimeBase1.ToString());
            ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(1, DemandCurrent * 0.95, 5);

            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);


            SetCPReresh();



        }
        public void OscillographInstrumentStopRate_Gear(double demandCurrent)
        {


            try
            {
                if (demandCurrent <= 80)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "10", "");
                }
                if (demandCurrent > 80 && demandCurrent <= 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "20", "");
                }
                if (demandCurrent > 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "40", "");
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }


        public override void InitializeParams()
        {
            Init();

            //BMS需求电压(V)=500|需求电流(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            }
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
                ControlEquipMent.Oscillograph.Oscillograph_SetCanChild_Channel(15, 5, 13, true, "5000", "CST-fault", 0, "101AF456", "Auto", 4, 2, 0, 0, "1", "0", "", "9", "-1", "PURPle", true, false, false, false);
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

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dic.Add(item, DemandVoltage.ToString("F2"));
                    }

                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dicC.Add(item, DemandCurrent.ToString("F2"));
                    }


                    ProcessDataTmp(dic, "充电设置", "电压需求(V)", "-", "-");
                    ProcessDataTmp(dicC, "充电设置", "电流需求(A)", "-", "-");


                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ////ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    Thread.Sleep(5 * 1000);


                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    Thread.Sleep(3000);

                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while(timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "主动停充前", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "主动停充前", "充电电流(A)", "-", "-");




                    #region 第一个点
                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    //OscillographInstrument_SetTrigger(5, 1, 0, "FALL", false, 80);
                    OscillographInstrument_SetTrigger(100, 15, 3, "RISE", true, 20);

                    CountDownTimeInfo("请手动控制桩停止充电再点击确认或者是。", 99999, 1);

                    ReadTriggerTypeOscillograph(30);

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);

                    double DownCurrent = 5;
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    double time = GetTriggerTime_Single(1, false, 0, DownCurrent, 1, false, false, -0.05);
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    //不先关掉负载电压会一直在
                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage > 30)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "主动停充后", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "主动停充后", "充电电流(A)", "-", "-");

                    Dictionary<int, string> dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }
                    double checktime = DemandCurrent / 100;
                    ProcessDataTmp(dTime, "主动停充后", "停充时间差(ms)", "0", (checktime * 1000).ToString());

                    Dictionary<int, string> dRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dRate.Add(item, ((DemandCurrent - DownCurrent) / time).ToString("F2"));
                    }
                    ProcessDataTmp(dRate, "主动停充后", "输出电流停止速率(A/s)", "100", "-", dImgs);
                    #endregion


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SetCPReresh();

                    #region 第二个点

                    SendNoticeToUIAndTxtFile("初始化录波仪配置中!");
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);


                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);

                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase(TimeBase2.ToString());

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(1, DemandCurrent, 5);


                    ControlEquipMent.Oscillograph?.Oscillograph_SetCanChild_Channel_Renovate(15, 3, "101956F4");


                    SendNoticeToUIAndTxtFile("开启导引中");

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    Thread.Sleep(5 * 1000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);

                    dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "被动停充前", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "被动停充前", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(100, 15, 3, "RISE", true, 20);

                    System.Threading.Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    ReadTriggerTypeOscillograph(30);


                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);
                    DownCurrent = 5;
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    time = GetTriggerTime_Single(1, false, 0, DownCurrent, 1, false, false, 0);
                    checktime = DemandCurrent / 100;

                    dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage > 30)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "被动停充后", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "被动停充后", "充电电流(A)", "-", "-");

                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    Dictionary<int, string> dTime2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dTime2.Add(item, (time * 1000).ToString("F2"));
                    }

                    ProcessDataTmp(dTime2, "被动停充后", "停充时间差(ms)", "0", (checktime * 1000).ToString());

                    Dictionary<int, string> dRate2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dRate2.Add(item, ((DemandCurrent - DownCurrent) / time).ToString("F2"));
                    }
                    ProcessDataTmp(dRate2, "被动停充后", "输出电流停止速率(A/s)", "100", "-", dImgs2);
                    #endregion


                    //SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFF(testWorkParam.lstIDs);


                    //SendNoticeToUIAndTxtFile("关闭导引中!");

                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }






        public override void ProcessData()
        {

        }

    }
}
