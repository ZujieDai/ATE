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
    public class CurrentLimitCharacteristic_CJB : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 600;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 50;
        public CurrentLimitCharacteristic_CJB(int type)
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
            DemandCurrent = MaxAllowChargeCurrent;
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

                    List<double> Voltages = new List<double>()
                    { DemandVoltage, DemandVoltage, DemandVoltage - 130, DemandVoltage  - 260, DemandVoltage - 305, DemandVoltage - 214, DemandVoltage - 118, DemandVoltage, DemandVoltage};
                    List<double> Currents = new List<double>()
                    //{ DemandCurrent - 20, DemandCurrent - 10, DemandCurrent + 5, DemandCurrent + 15, DemandCurrent + 20, DemandCurrent + 10, DemandCurrent + 18, DemandCurrent - 5, DemandCurrent - 15 };
                    { DemandCurrent * 0.2, DemandCurrent * 0.4, DemandCurrent * 0.5, DemandCurrent * 0.5, DemandCurrent * 0.5, DemandCurrent  * 0.5, DemandCurrent  * 0.5, DemandCurrent * 0.4, DemandCurrent * 0.2 };

                    for (int i = 0; i < Voltages.Count; i++)
                    {
                        string sState = Voltages[i] == DemandVoltage ? "限压" : "限流";
                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        if (i == 0 || Voltages[i - 1] != Voltages[i])
                        {
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltages[i], DemandCurrent * 0.5, false, 390);
                            WaitDCVoltage(testWorkParam.lstIDs, Voltages[i]);
                            Thread.Sleep(3000);
                        }

                        double resistLoadCurrent = Currents[i] > DemandCurrent * 0.5 ? DemandCurrent * 0.5 : Currents[i];
                        SetLoadPara(testWorkParam.lstIDs, Voltages[i] - 20, Currents[i], Voltages[i] - 5, resistLoadCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(100);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, Currents[i], 35);
                        Thread.Sleep(3000);

                        double judgeCurrent = 0, judgeVolt = 0;
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 20;
                            judgeCurrent = Voltages[i] == DemandVoltage ? Currents[i] : DemandCurrent * 0.5;
                        }
                        else
                        {
                            judgeVolt = Voltages[i] == DemandVoltage ? DemandVoltage : Voltages[i] - 5;
                            judgeCurrent = resistLoadCurrent;
                        }
                        var dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            int timeout = 50;
                            while (timeout-- > 0)
                            {
                                if (DCVoltage >= judgeVolt * 0.95 && DCVoltage <= judgeVolt * 1.05)
                                    break;
                                Thread.Sleep(300);
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            }
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        var dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            int timeout = 50;
                            while (timeout-- > 0)
                            {
                                if (DCCurrent >= judgeCurrent * 0.95 && DCCurrent <= judgeCurrent * 1.05)
                                    break;
                                Thread.Sleep(300);
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            }
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
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
}
