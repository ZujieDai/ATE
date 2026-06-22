using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试：限流特性试验
    /// </summary>
    public class QB_RT_DC_CurrentLimitCharacteristic : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 700;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 50;
        public QB_RT_DC_CurrentLimitCharacteristic(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            OscilloscoCurrentLimiting();
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
            SetCPReresh();
        }

        public override void InitializeParams()
        {
            Init();

            //BMS需求电压(V)=600|BMS需求电流(A)=80
            string[] strParams = TrialItem.ResultParams.Split('|');
            DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
            DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
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
                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(3000);

                    //根据开普的报告：限压2个点，限流5个点，限压2个点，总共7个点
                    List<double> Voltages = new List<double>()//电压只能用于回馈负载
                    { DemandVoltage , DemandVoltage , DemandVoltage-30 , DemandVoltage-60, DemandVoltage-90, DemandVoltage-120, DemandVoltage-150, DemandVoltage, DemandVoltage };
                    List<double> Currents = new List<double>()
                    { DemandCurrent - 10, DemandCurrent - 5, DemandCurrent +5 , DemandCurrent + 10, DemandCurrent + 15, DemandCurrent + 20, DemandCurrent + 25, DemandCurrent - 10, DemandCurrent - 5};


                    for (int i = 0; i < Voltages.Count; i++)
                    {
                        string sState = Currents[i] < DemandCurrent ? "限压" : "限流";
                        //SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        if (i == 0 || Voltages[i - 1] != Voltages[i])
                        {
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                            //WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);
                            Thread.Sleep(3000);
                        }

                        double resistLoadCurrent = Currents[i] > DemandCurrent ? DemandCurrent : Currents[i];
                        SetLoadPara(testWorkParam.lstIDs, Voltages[i] - 20, Currents[i], DemandVoltage, Currents[i]);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(100);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, resistLoadCurrent, 35);
                        Thread.Sleep(3000);

                        double judgeCurrent = 0, judgeVolt = 0;
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 20;
                            judgeCurrent = Voltages[i] == DemandVoltage ? Currents[i] : DemandCurrent;
                        }
                        else
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 5;
                            judgeCurrent = resistLoadCurrent;
                        }
                        var dic = new Dictionary<int, string>();
                        var dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            int timeout = 50;
                            while (timeout-- > 0)
                            {
                                if (sState == "限压")
                                {
                                    if (DCVoltage >= judgeVolt * 0.95 && DCVoltage <= judgeVolt * 1.05)
                                        break;
                                }
                                else
                                {
                                    if (DCCurrent >= judgeCurrent * 0.95 && DCCurrent <= judgeCurrent * 1.05)
                                        break;
                                }
                                Thread.Sleep(300);
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            }
                            dic.Add(item, DCVoltage.ToString("F2"));
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
                        //foreach (var item in testWorkParam.lstIDs)
                        //{
                        //    double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        //    int timeout = 50;
                        //    while (timeout-- > 0)
                        //    {
                        //        if (DCCurrent >= judgeCurrent * 0.95 && DCCurrent <= judgeCurrent * 1.05)
                        //            break;
                        //        Thread.Sleep(300);
                        //        DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        //        if (sState != "限流")
                        //        {
                        //            break;
                        //        }
                        //    }
                        //    dicC.Add(item, DCCurrent.ToString("F2"));
                        //}
                        if (sState == "限压")
                        {
                            ProcessDataTmp(dicC, sState, "充电电流(A)", "-", "-");
                            ProcessDataTmp(dic, sState, "充电电压(V)", (judgeVolt * 0.95).ToString(), (judgeVolt * 1.05).ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC, sState, "充电电流(A)", (judgeCurrent * 0.95).ToString(), (judgeCurrent * 1.05).ToString());
                            ProcessDataTmp(dic, sState, "充电电压(V)", "-", "-");
                        }
                    }
                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }



        public void OscilloscoCurrentLimiting()
        {
            try
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "150", "-2");//通道1设置
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "20", "0");//通道2设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道3设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20M", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "40", 1, "650", "Auto");
                System.Threading.Thread.Sleep(100);
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0");//设置滚动，时基和触发延时
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");//设置测量项为XY
                System.Threading.Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }


        /// <summary>
        /// 示波器限压特性2
        /// </summary>
        public void OscilloscoCurrentLimiting2()
        {
            try
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "40", 1, "650", "Single");
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 示波器限压特性3
        /// </summary>
        public void OscilloscoCurrentLimiting3()
        {
            try
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, "650", "Single");
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public override void ProcessData()
        {

        }

    }

    /// <summary>
    /// 企标研测直流：限流特性试验
    /// </summary>
    public class QB_RT_DC_CurrentLimitCharacteristic_Old : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 700;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 50;
        public QB_RT_DC_CurrentLimitCharacteristic_Old(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.BMS.BMS_OFF(lstIDs);
        }

        public override void InitializeParams()
        {
            Init();

            //BMS需求电压(V)=600|BMS需求电流(A)=80
            string[] strParams = TrialItem.ResultParams.Split('|');
            DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
            DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            DemandVoltage = DemandVoltage > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : DemandVoltage;
            DemandCurrent = DemandCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;
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
                SetCPReresh();
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
                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(3000);

                    List<double> Voltages = new List<double>()
                    { DemandVoltage, DemandVoltage, DemandVoltage - 80, DemandVoltage - 270, DemandVoltage - 140, DemandVoltage, DemandVoltage};
                    List<double> Currents = new List<double>()
                    { DemandCurrent - 20, DemandCurrent - 10, DemandCurrent + 10, DemandCurrent + 10, DemandCurrent + 10, DemandCurrent - 5, DemandCurrent - 15 };

                    for (int i = 0; i < Voltages.Count; i++)
                    {
                        string sState = Voltages[i] == DemandVoltage ? "限压" : "限流";
                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        if (i == 0 || Voltages[i - 1] != Voltages[i])
                        {
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltages[i], DemandCurrent, false, 390);
                            WaitDCVoltage(testWorkParam.lstIDs, Voltages[i]);
                            Thread.Sleep(3000);
                        }

                        double resistLoadCurrent = Currents[i] > DemandCurrent ? DemandCurrent : Currents[i];
                        SetLoadPara(testWorkParam.lstIDs, Voltages[i] - 20, Currents[i], Voltages[i] - 5, resistLoadCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(100);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(3000);

                        var dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        var dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
                        double judgeCurrent = 0, judgeVolt = 0;
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 20;
                            judgeCurrent = Voltages[i] == DemandVoltage ? Currents[i] : DemandCurrent;
                        }
                        else
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 5;
                            //judgeCurrent = resistLoadCurrent;
                            judgeCurrent = resistLoadCurrent > DemandCurrent ? DemandCurrent : resistLoadCurrent;
                        }
                        ProcessDataTmp(dicC, sState, "充电电流(A)", (judgeCurrent * 0.95).ToString(), (judgeCurrent * 1.05).ToString());
                        ProcessDataTmp(dic, sState, "充电电压(V)", (judgeVolt * 0.95).ToString(), (judgeVolt * 1.05).ToString());
                    }
                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }

    }
}
