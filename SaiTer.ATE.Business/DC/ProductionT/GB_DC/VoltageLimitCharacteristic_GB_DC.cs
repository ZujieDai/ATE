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

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 限压特性试验
    /// </summary>
    public class VoltageLimitCharacteristic_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;
        double OutputVoltage1 = 0;
        double Error = 5;
        double OutputVoltage2 = 0;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        string ItemFlow = "";
        double CheckCurrent = 0;
        double SetCurrent = 0;
        public VoltageLimitCharacteristic_GB_DC(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            ////输出电压1(V)=500|输出电压2(V)=750|±误差(%)=0.5
            string[] strParams = TrialItem.ResultParams.Split('|');
            //OutputVoltage1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            //Error = Convert.ToDouble(strParams[2].Split('=')[1]);
            //OutputVoltage2 = Convert.ToDouble(strParams[1].Split('=')[1]);
            //±误差(%)=5
            if (strParams.Length > 1 && strParams[0].IndexOf('=') > -1)
            {
                Error = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(lstIDs);
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
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);

                    List<double> demandVolts = new List<double>()
                    {
                        BMSDemandVolt * 0.5,
                        BMSDemandVolt * 0.6,
                        BMSDemandVolt  * 0.7,
                        BMSDemandVolt  * 0.85,
                        BMSDemandVolt  * 0.75,
                        BMSDemandVolt  * 0.65,
                        BMSDemandVolt  * 0.5,
                    };
                    demandVolts = CompareMaximum(demandVolts.ToArray(), MaxAllowChargeVoltage).ToList();
                    demandVolts = CompareMinimum(demandVolts.ToArray(), MinAllowChargeVoltage).ToList();
                    demandVolts = RetainDecimals(demandVolts, 0);
                    List<double> setCurrents = new List<double>()
                    {
                        MaxOutputPower * 1000 * 0.8 / demandVolts[0],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[1],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[2],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[3],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[4],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[5],
                        MaxOutputPower * 1000 * 0.8 / demandVolts[6],
                    };
                    setCurrents = CompareMaximum(setCurrents.ToArray(), MaxAllowChargeCurrent).ToList();
                    setCurrents = CompareMinimum(setCurrents.ToArray(), 20).ToList();
                    setCurrents = RetainDecimals(setCurrents, 0);
                    for (int i = 0; i < demandVolts.Count; i++)
                    {
                        string sState = $"测量点{i + 1}";
                        SetLoadDCOFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);

                        double? OutputVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        if (OutputVoltage < MinAllowChargeVoltage)
                        {
                            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                            CheckSwipingCard(testWorkParam.lstIDs, demandVolts[i], setCurrents[i], MaxAllowChargeVoltage, false);
                        }
                        else
                        {
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, demandVolts[i], setCurrents[i], false, 390);
                        }
                        WaitDCVoltage(testWorkParam.lstIDs, demandVolts[i]);
                        Thread.Sleep(3000);

                        double judgeCurrent = 0, judgeVolt = 0;
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        {
                            judgeVolt = demandVolts[i] - 20;
                            judgeCurrent = setCurrents[i] + 10;
                        }
                        else
                        {
                            judgeVolt = demandVolts[i] - 5;
                            judgeCurrent = setCurrents[i];
                        }
                        SendNoticeToUIAndTxtFile("设置带载电压" + judgeVolt + "V,带载电流" + judgeCurrent + "A，恒流模式，等待负载稳定");
                        SetLoadPara(testWorkParam.lstIDs, judgeVolt, judgeCurrent, judgeVolt, judgeCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);
                        judgeCurrent = setCurrents[i];
                        Thread.Sleep(100);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, judgeCurrent, 35);
                        Thread.Sleep(3000);

                        int time = 50;
                        var dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            while (time-- > 0)
                            {
                                if (DCVoltage < judgeVolt * (1 - Error / 100) || DCVoltage > judgeVolt * (1 + Error / 100))
                                {
                                    DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                    Thread.Sleep(100);
                                }
                                else
                                    break;
                            }
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        time = 50;
                        var dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            while (time-- > 0)
                            {
                                if (DCCurrent < judgeCurrent * (1 - Error / 100) || DCCurrent > judgeCurrent * (1 + Error / 100))
                                {
                                    DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                    Thread.Sleep(100);
                                }
                                else
                                    break;
                            }
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }
                        ProcessDataTmp(dicC, sState, "充电电流(A)", (judgeCurrent * (1 - Error / 100)).ToString(), (judgeCurrent * (1 + Error / 100)).ToString());
                        ProcessDataTmp(dic, sState, "充电电压(V)", (judgeVolt * (1 - Error / 100)).ToString(), (judgeVolt * (1 + Error / 100)).ToString());
                    }
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 旧代码（弃用原因：没考虑最大功率）
                    //SetCurrent = 55;
                    //if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, SetCurrent, BMSDemandVolt, false))
                    //{
                    //    return;
                    //}
                    ////ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, SetCurrent, false, BMSDemandVolt);
                    //Thread.Sleep(500);

                    //SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (SetCurrent + 10) + "A，恒流模式，等待负载稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent-5);

                    //Thread.Sleep(500);
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Stopwatch st = new Stopwatch();
                    //st.Start();
                    //while (st.ElapsedMilliseconds / 1000 <= 30)
                    //{
                    //    CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                    //    if (CheckCurrent >= SetCurrent * 0.9 && CheckCurrent <= SetCurrent * 1.1)
                    //    {
                    //        break;
                    //    }

                    //    Thread.Sleep(1000);
                    //}
                    //st.Stop();
                    //st.Reset();

                    //if (CheckCurrent < 1)
                    //{
                    //    SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");

                    //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //    Thread.Sleep(500);
                    //    SetLoadDCON(testWorkParam.lstIDs);
                    //    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    //}
                    //Thread.Sleep(3000);

                    //double minData = SetCurrent * 0.8;
                    //double maxData = SetCurrent * 1.1;

                    //ItemFlow = "测量点1";
                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, ItemFlow, "输出电流(A)", (SetCurrent * 0.8).ToString("F2"), (SetCurrent * 1.1).ToString("F2"));





                    //SetCurrent = 30;
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, SetCurrent, false, BMSDemandVolt);
                    //Thread.Sleep(3000);
                    //SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (SetCurrent + 10) + "A，恒流模式，等待负载稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //st.Start();
                    //while (st.ElapsedMilliseconds / 1000 <= 30)
                    //{
                    //    CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                    //    if (CheckCurrent >= SetCurrent * 0.9 && CheckCurrent <= SetCurrent * 1.1)
                    //    {
                    //        break;
                    //    }

                    //    Thread.Sleep(1000);
                    //}
                    //Thread.Sleep(3000);
                    //st.Stop();
                    //st.Reset();
                    //if (CheckCurrent < 1)
                    //{
                    //    SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");
                    //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //    Thread.Sleep(500);
                    //   SetLoadDCON(testWorkParam.lstIDs);
                    //    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    //}
                    //ItemFlow = "测量点2";
                    //dic.Clear();
                    //minData = SetCurrent * 0.8;
                    //maxData = SetCurrent * 1.1;
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, ItemFlow, "输出电流(A)", (SetCurrent * 0.8).ToString("F2"), (SetCurrent * 1.1).ToString("F2"));

                    //minData = BMSDemandVolt * 0.95;
                    //maxData = BMSDemandVolt * 1.05;
                    //dic.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }

                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}

                    //ItemFlow = "测量点3";
                    //ProcessDataTmp(dic, ItemFlow, "输出电压(V)", (BMSDemandVolt * 0.95).ToString("F2"), (BMSDemandVolt * 1.05).ToString("F2"));







                    //SetCurrent = 30;
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, SetCurrent, false, BMSDemandVolt);
                    //Thread.Sleep(3000);
                    //SendNoticeToUIAndTxtFile("设置带载电压" + (BMSDemandVolt) + "V,带载电流" + (SetCurrent + 10) + "A，恒流模式，等待负载稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //st.Start();
                    //while (st.ElapsedMilliseconds / 1000 <= 30)
                    //{
                    //    CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                    //    if (CheckCurrent >= SetCurrent * 0.9 && CheckCurrent <= SetCurrent * 1.1)
                    //    {
                    //        break;
                    //    }

                    //    Thread.Sleep(1000);
                    //}
                    //Thread.Sleep(3000);
                    //if (CheckCurrent < 1)
                    //{
                    //    SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");
                    //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //    Thread.Sleep(500);
                    //   SetLoadDCON(testWorkParam.lstIDs);
                    //    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    //}
                    //minData = BMSDemandVolt * 0.95;
                    //maxData = BMSDemandVolt * 1.05;
                    //dic.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }

                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}

                    //ItemFlow = "测量点4";
                    //ProcessDataTmp(dic, ItemFlow, "输出电压(V)", (BMSDemandVolt * 0.95).ToString("F2"), (BMSDemandVolt * 1.05).ToString("F2"));
                    //dic.Clear();
                    //minData = SetCurrent * 0.95;
                    //maxData = SetCurrent * 1.05;
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}

                    //ItemFlow = "测量点5";
                    //ProcessDataTmp(dic, ItemFlow, "输出电流(A)", (SetCurrent * 0.95).ToString("F2"), (SetCurrent * 1.05).ToString("F2"));







                    //SetCurrent = 50;
                    ////if(ControlEquipMent.FeedbackLoad!=null)
                    ////{
                    //    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt - 100, SetCurrent, false, BMSDemandVolt - 100);
                    ////}

                    //Thread.Sleep(5000);
                    //SendNoticeToUIAndTxtFile("设置带载电压" + (BMSDemandVolt - 120) + "V,带载电流" + (SetCurrent + 10) + "A，恒流模式，等待负载稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 120, SetCurrent + 10, BMSDemandVolt-100 , SetCurrent);
                    //st.Start();
                    //while (st.ElapsedMilliseconds / 1000 <= 30)
                    //{
                    //    CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                    //    if (CheckCurrent >= SetCurrent * 0.9 && CheckCurrent <= SetCurrent * 1.1)
                    //    {
                    //        break;
                    //    }

                    //    Thread.Sleep(1000);
                    //}
                    //st.Stop();
                    //st.Reset();
                    //if (CheckCurrent < 1)
                    //{
                    //    SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");
                    //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 120, SetCurrent + 10, BMSDemandVolt, SetCurrent);
                    //    Thread.Sleep(500);
                    //   SetLoadDCON(testWorkParam.lstIDs);
                    //    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    //}
                    //Thread.Sleep(3000);
                    //minData = (BMSDemandVolt - 100) * 0.95;
                    //maxData = (BMSDemandVolt - 100) * 1.05;
                    //dic.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }

                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}
                    //ItemFlow = "测量点6";
                    //ProcessDataTmp(dic, ItemFlow, "输出电压", ((BMSDemandVolt - 100) * 0.95).ToString("F2"), ((BMSDemandVolt - 100) * 1.05).ToString("F2"));


                    //minData = SetCurrent * 0.95;
                    //maxData = SetCurrent * 1.05;
                    //dic.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                    //    int count = 10;
                    //    while (count-- > 0)
                    //    {
                    //        if (data < minData || data > maxData)
                    //        {
                    //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    //            Thread.Sleep(1000);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }

                    //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    //}

                    //ItemFlow = "测量点7";
                    //ProcessDataTmp(dic, ItemFlow, "输出电流", (SetCurrent * 0.95).ToString("F2"), (SetCurrent * 1.05).ToString("F2"));

                    //SetLoadDCOFF(testWorkParam.lstIDs);

                    ////ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, 200, false, BMSDemandVolt);
                    #endregion
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }
        public override void ProcessData()
        {

        }

    }
}
