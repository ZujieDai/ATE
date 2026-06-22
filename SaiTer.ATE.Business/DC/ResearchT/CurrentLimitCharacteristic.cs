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
    /// 研发测试：限流特性试验（弃用）
    /// </summary>
    public class CurrentLimitCharacteristic : BusinessBase
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
        double 负载需求电压 = 750;
        double 负载需求电流 = 15;
        double 判定电压 = 500;
        double 判定电流 = 10;
        public CurrentLimitCharacteristic(int type)
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

            //BMS需求电压=750|BMS需求电流=80|负载需求电压=750|负载需求电流=15|判定电压=500|判定电流=10 （手动负载
            string[] strParams = TrialItem.ResultParams.Split('|');
            DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
            DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            if (strParams.Length >= 6)
            {
                负载需求电压 = double.Parse(strParams[2].Split('=')[1]);
                负载需求电流 = double.Parse(strParams[3].Split('=')[1]);
                判定电压 = double.Parse(strParams[4].Split('=')[1]);
                判定电流 = double.Parse(strParams[5].Split('=')[1]);
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
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("启动充电中");

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);


                    //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    
                    //if (Customer != null && Customer.Contains("JCZT"))
                    //    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 100, DemandCurrent + 5, 负载需求电压, 负载需求电流);
                    //else
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent - 30, DemandVoltage, DemandCurrent - 20);

                    SetLoadDCON(testWorkParam.lstIDs);


                    //if (Customer != null && Customer.Contains("JCZT"))
                    //    WaitDCCurrentWithTime(testWorkParam.lstIDs, 负载需求电流, 35);
                    //else
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent - 30, 35);

                    Thread.Sleep(2000);


                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }


                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    //if (Customer != null && Customer.Contains("JCZT"))
                    //{
                    //    ProcessDataTmp(dic, "正常状态", "充电电压(V)", (判定电压 * 0.95).ToString(), (判定电压 + 5).ToString(), dImgs);
                    //    ProcessDataTmp(dicC, "正常状态", "充电电流(A)", (判定电流 * 0.95).ToString(), (判定电流 * 1.05).ToString(), dImgs);

                    //    OscilloscoCurrentLimiting2();
                    //}
                    //else
                    {
                        //ProcessDataTmp(dic, "正常状态", "充电电压(V)", "-", "-", dImgs);
                        ProcessDataTmp(dic, "限压", "充电电压(V)", (DemandVoltage * 0.95).ToString(), (DemandVoltage * 1.05).ToString());
                        //ProcessDataTmp(dicC, "正常状态", "充电电流(A)", "-", "-", dImgs);
                        ProcessDataTmp(dicC, "限压", "充电电流(A)", ((DemandCurrent - 30) * 0.95).ToString(), ((DemandCurrent - 30) * 1.05).ToString());


                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent - 10, DemandVoltage - 10, DemandCurrent - 10);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic, "限压", "充电电压(V)", ((DemandVoltage) * 0.95).ToString(), ((DemandVoltage) * 1.05).ToString());
                        ProcessDataTmp(dicC, "限压", "充电电流(A)", ((DemandCurrent - 10) * 0.95).ToString(), ((DemandCurrent - 10) * 1.05).ToString());


                        OscilloscoCurrentLimiting2();
                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 80, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 80);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 100, DemandCurrent + 5, DemandVoltage - 100, DemandCurrent + 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        Dictionary<int, string> dic2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic2.Add(item, DCVoltage.ToString("F2"));
                        }
                        Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }
                        Dictionary<int, string> dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic2, "限流", "充电电压(V)", ((DemandVoltage - 100) * 0.95).ToString(), ((DemandVoltage - 100) * 1.05).ToString());
                        ProcessDataTmp(dicC2, "限流", "充电电流(A)", (DemandCurrent * 0.95).ToString(), (DemandCurrent * 1.05).ToString());


                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 130, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 130);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 150, DemandCurrent + 15, DemandVoltage - 150, DemandCurrent + 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic2.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic2, "限流", "充电电压(V)", ((DemandVoltage - 150) * 0.95).ToString(), ((DemandVoltage - 150) * 1.05).ToString());
                        ProcessDataTmp(dicC2, "限流", "充电电流(A)", (DemandCurrent * 0.95).ToString(), (DemandCurrent * 1.05).ToString());


                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 270, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 270);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 280, DemandCurrent + 20, DemandVoltage - 280, DemandCurrent + 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic2.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic2, "限流", "充电电压(V)", ((DemandVoltage - 280) * 0.95).ToString(), ((DemandVoltage - 280) * 1.05).ToString());
                        ProcessDataTmp(dicC2, "限流", "充电电流(A)", (DemandCurrent * 0.95).ToString(), (DemandCurrent * 1.05).ToString());


                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 130, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 190);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 190, DemandCurrent + 10, DemandVoltage - 190, DemandCurrent + 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic2.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic2, "限流", "充电电压(V)", ((DemandVoltage - 190) * 0.95).ToString(), ((DemandVoltage - 190) * 1.05).ToString());
                        ProcessDataTmp(dicC2, "限流", "充电电流(A)", (DemandCurrent * 0.95).ToString(), (DemandCurrent * 1.05).ToString());


                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 60, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 60);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 80, DemandCurrent + 10, DemandVoltage - 80, DemandCurrent + 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic2.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic2, "限流", "充电电压(V)", ((DemandVoltage - 100) * 0.95).ToString(), ((DemandVoltage - 100) * 1.05).ToString());
                        ProcessDataTmp(dicC2, "限流", "充电电流(A)", (DemandCurrent * 0.95).ToString(), (DemandCurrent * 1.05).ToString());


                        OscilloscoCurrentLimiting3();
                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);
                        Thread.Sleep(2000);
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent - 20, DemandVoltage, DemandCurrent - 10);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent - 20, 35);
                        Thread.Sleep(2000);

                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(8000);

                        Dictionary<int, string> dic3 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic3.Add(item, DCVoltage.ToString("F2"));
                        }
                        Dictionary<int, string> dicC3 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC3.Add(item, DCCurrent.ToString("F2"));
                        }
                        Dictionary<int, string> dImgs3 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic3, "限压", "充电电压(V)", (DemandVoltage * 0.95).ToString(), (DemandVoltage * 1.05).ToString());
                        ProcessDataTmp(dicC3, "限压", "充电电流(A)", ((DemandCurrent - 20) * 0.95).ToString(), ((DemandCurrent - 20) * 1.05).ToString());


                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent - 5, DemandVoltage - 20, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent - 5, 35);
                        Thread.Sleep(2000);
                        ReadTriggerType(testWorkParam.lstIDs, 10);
                        Thread.Sleep(2000);
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dic, "限压", "充电电压(V)", ((DemandVoltage) * 0.95).ToString(), ((DemandVoltage) * 1.05).ToString());
                        ProcessDataTmp(dicC, "限压", "充电电流(A)", ((DemandCurrent - 5) * 0.95).ToString(), ((DemandCurrent - 5) * 1.05).ToString());

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
