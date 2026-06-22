using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:启动输出过冲
    /// </summary>

    public class StartOutputOvershoot : BusinessBase
    {

        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 60;
        /// <summary>
        /// 需求电流2(A)
        /// </summary>
        Double DemandCurrent2 = 20;
        /// <summary>
        /// 判定准则1(%)
        /// </summary>
        Double ErrorPercentage = 5;

        /// <summary>
        /// 判定准则2(%)
        /// </summary>
        Double ErrorPercentage2 = 5;



        /// <summary>
        /// 判定准则3(A)
        /// </summary>
        Double ErrorValue = 1.5;

        public StartOutputOvershoot(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
           

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');

            //BMS需求电压设置(V)=745|大于等于30A电流设置(A)=50|小于30A电流设置(A)=20|判定准则(%)=5|判定准则2(%)=5|判定准则3(A)=1.5
            if (strParams.Length >= 6)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                DemandCurrent2 = double.Parse(strParams[2].Split('=')[1]);
                ErrorPercentage = double.Parse(strParams[3].Split('=')[1]);
                ErrorPercentage2 = double.Parse(strParams[4].Split('=')[1]);
                ErrorValue = double.Parse(strParams[5].Split('=')[1]);
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
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
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



                    DemandCurrent = DemandCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;
                    DemandCurrent2 = DemandCurrent2 > 30 ? 30 : DemandCurrent2;


                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                    #region 预充电后进入充电阶段
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent))
                    {
                        return;
                    }
                    SetConditionValues();

                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, 390);
                    ////ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);


                    #region 启动输出过冲第一步 恒压

                    SendNoticeToUIAndTxtFile("正在测试第一个点...");

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);


                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 100, 5, true, 390);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 100);
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 100, DemandCurrent / 2, DemandVoltage - 100, 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    if (ControlEquipMent.LoopFeedbackLoad != null)
                        Thread.Sleep(1000 * 8);
                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage - 50, 5);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 50, DemandCurrent / 2);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}
                    System.Threading.Thread.Sleep(5000);

                    SendNoticeToUIAndTxtFile("设置示波器参数中...");
                    OscilloscopeStartupOutputOvershoot1(DemandVoltage);
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    if (ControlEquipMent.LoopFeedbackLoad != null)
                    {
                        var dicTemp1 = new Dictionary<int, int>();
                        int count1 = 10;
                        while (count1-- > 0)
                        {
                            dicTemp1 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                            if (dicTemp1[testWorkParam.lstIDs[0]] == 0)
                            {
                                //ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, (DemandVoltage - 10).ToString(), "Single");
                                //Thread.Sleep(100);
                                if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent < 10)
                                    ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, (DemandVoltage - 10).ToString(), "Single");
                                else
                                    break;
                                break;
                            }
                            else
                                Thread.Sleep(1000);

                        }
                    }
                    ReadTriggerType(testWorkParam.lstIDs, 10);

                    System.Threading.Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    System.Threading.Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("读取示波器电压最大值...");
                    Dictionary<int, string> VoDCImaxs = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);     //采集的是输入交流电流，最大值
                    System.Threading.Thread.Sleep(3000);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> dErrorRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double VoDCImax = Convert.ToDouble(VoDCImaxs[item]);
                        double ErrorRate = System.Math.Abs((VoDCImax - DemandVoltage) / DemandVoltage);
                        ErrorRate = ErrorRate * 100;
                        dErrorRate.Add(item, ErrorRate.ToString("F2"));
                    }

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

                    ProcessDataTmp(dic, "预充进入充电阶段", "稳压状态充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC, "预充进入充电阶段", "稳压状态充电电流(A)", "-", "-");
                    ProcessDataTmp(VoDCImaxs, "预充进入充电阶段", "稳压状态输入电压最大值(V)", "-", "-");
                    ProcessDataTmp(dErrorRate, "预充进入充电阶段", "稳压状态过冲测试结果(%)", "0", ErrorPercentage.ToString(), dImgs);
                    #endregion

                    #region 启动输出过冲第二步 恒流
                    //OscilloscopeStartupOutputOvershoot2(DemandCurrent);
                    SendNoticeToUIAndTxtFile("正在测试第二个点...");

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);

                    System.Threading.Thread.Sleep(5000);
                    OscilloscopeStartupOutputOvershoot2(DemandCurrent);
                    System.Threading.Thread.Sleep(3000);

                    if (ControlEquipMent.ResistanceLoad != null)
                        SendNoticeToUIAndTxtFile($"设置负载电压{DemandVoltage - 20}V，电流{DemandCurrent}A...");
                    else
                        SendNoticeToUIAndTxtFile($"设置负载电压{DemandVoltage - 20}V，电流{DemandCurrent + 20}A...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 20, DemandVoltage - 5, DemandCurrent, false);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage - 5, DemandCurrent);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 50);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    Dictionary<int, int> dicTemp = new Dictionary<int, int>();
                    int count = 10;
                    while (count-- > 0)
                    {
                        SendNoticeToUIAndTxtFile("读取是否触发...");
                        dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                        if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                        {
                            //启动负载时有继电器动作干扰，添加延时
                            //if (ControlEquipMent.LoopFeedbackLoad != null)
                            //    Thread.Sleep(5000);
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent < 5)
                                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent * 0.667).ToString(), "Single");
                            else
                                break;
                        }
                        else
                            Thread.Sleep(1000);

                    }
                    //System.Threading.Thread.Sleep(5000);

                    ReadTriggerType(testWorkParam.lstIDs, 30);

                    System.Threading.Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("读取示波器电流最大值...");
                    Dictionary<int, string> InDCImaxs = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);     //采集的是输入交流电流，最大值
                    System.Threading.Thread.Sleep(3000);

                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> dErrorRate2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double InDCImax = Convert.ToDouble(InDCImaxs[item]);
                        double ErrorRate2 = System.Math.Abs((InDCImax - DemandCurrent) / DemandCurrent);
                        ErrorRate2 = ErrorRate2 * 100;
                        dErrorRate2.Add(item, ErrorRate2.ToString("F2"));
                    }

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

                    ProcessDataTmp(dic2, "预充进入充电阶段", "稳流大于等于30A充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "预充进入充电阶段", "稳流大于等于30A充电电流(A)", "-", "-");
                    ProcessDataTmp(InDCImaxs, "预充进入充电阶段", "稳流大于等于30A输入电流最大值(A)", "-", "-");
                    ProcessDataTmp(dErrorRate2, "预充进入充电阶段", "稳流大于等于30A过冲测试结果(%)", "0", ErrorPercentage2.ToString(), dImgs2);
                    #endregion

                    #region 启动输出过冲第三步 恒流-新的测试流程
                    SendNoticeToUIAndTxtFile("正在测试第三个点...");
                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("实战示波器触发自动...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");



                    System.Threading.Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent2, false, 390);

                    System.Threading.Thread.Sleep(6000);



                    OscilloscopeStartupOutputOvershoot3(DemandCurrent2);
                    System.Threading.Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 20, DemandVoltage, DemandCurrent2, false);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage, DemandCurrent2);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 20);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    dicTemp = new Dictionary<int, int>();
                    count = 10;
                    while (count-- > 0)
                    {
                        dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                        if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                        {
                            //启动负载时有继电器动作干扰，添加延时
                            //if (ControlEquipMent.LoopFeedbackLoad != null)
                            //    Thread.Sleep(5000);
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent < 5)
                                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent2 * 0.667).ToString(), "Single");
                            else
                                break;
                        }
                        else
                            Thread.Sleep(1000);

                    }
                    //System.Threading.Thread.Sleep(5000);

                    ReadTriggerType(testWorkParam.lstIDs, 30);

                    System.Threading.Thread.Sleep(9000);


                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("读取示波器电流最大值...");
                    Dictionary<int, string> InDCImax2s = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);     //采集的是输入交流电流，最大值
                    System.Threading.Thread.Sleep(3000);

                    Dictionary<int, string> dImgs3 = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> dErrorRate3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double InDCImax2 = Convert.ToDouble(InDCImax2s[item]);
                        double ErrorRate3 = InDCImax2 - DemandCurrent2;
                        dErrorRate3.Add(item, ErrorRate3.ToString("F2"));
                    }

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


                    ProcessDataTmp(dic3, "预充进入充电阶段", "稳流小于30A充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC3, "预充进入充电阶段", "稳流小于30A充电电流(A)", "-", "-");
                    ProcessDataTmp(InDCImax2s, "预充进入充电阶段", "稳流小于30A输入电流最大值(A)", "-", "-");
                    ProcessDataTmp(dErrorRate3, "预充进入充电阶段", "稳流小于30A过冲测试结果(A)", "0", ErrorValue.ToString(), dImgs3);

                    #endregion

                    #endregion


                    #region 暂停充电后进入充电阶段
                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs, false);
                    ControlEquipMent.BMS.BMS_OFF(lstIDs);
                    Thread.Sleep(200);
                    SetCPReresh();

                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 600);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, 250, true, DemandVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 100, 5, true, DemandVoltage);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 100);
                    Thread.Sleep(2000);

                    #region 启动输出过冲第一步 恒压
                    SendNoticeToUIAndTxtFile("正在测试第一个点...");
                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);
                    Thread.Sleep(3000);

                    //模拟电池电压使暂停状态不会停止充电
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 100, DemandCurrent / 2, DemandVoltage - 100, 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    if (ControlEquipMent.LoopFeedbackLoad != null)
                        Thread.Sleep(1000 * 8);

                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);

                    SendNoticeToUIAndTxtFile("设置示波器参数中...");
                    OscilloscopeStartupOutputOvershoot1(DemandVoltage);
                    Thread.Sleep(10 * 1000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-");
                    dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicC, "暂停状态数据", "输出电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage - 50, 5);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 50, DemandCurrent / 2);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}
                    //System.Threading.Thread.Sleep(5000);

                    //SendNoticeToUIAndTxtFile("设置导引参数中...");
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);

                    ReadTriggerType(testWorkParam.lstIDs, 30);

                    System.Threading.Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    System.Threading.Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("读取示波器电压最大值...");
                    VoDCImaxs = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);
                    System.Threading.Thread.Sleep(3000);

                    dImgs = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    dErrorRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double VoDCImax = Convert.ToDouble(VoDCImaxs[item]);
                        double ErrorRate = System.Math.Abs((VoDCImax - DemandVoltage) / DemandVoltage);
                        ErrorRate = ErrorRate * 100;
                        dErrorRate.Add(item, ErrorRate.ToString("F2"));
                    }

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

                    ProcessDataTmp(dic, "暂停状态进入充电", "稳压状态充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC, "暂停状态进入充电", "充电电流(A)", "-", "-");
                    ProcessDataTmp(VoDCImaxs, "暂停状态进入充电", "稳压状态输入电压最大值(V)", "-", "-");
                    ProcessDataTmp(dErrorRate, "暂停状态进入充电", "稳压状态过冲测试结果(%)", "0", ErrorPercentage.ToString(), dImgs);
                    #endregion

                    #region 启动输出过冲第二步 恒流
                    //OscilloscopeStartupOutputOvershoot2(DemandCurrent);
                    SendNoticeToUIAndTxtFile("正在测试第二个点...");

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);
                    Thread.Sleep(10 * 1000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-");
                    dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicC, "暂停状态数据", "输出电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);

                    System.Threading.Thread.Sleep(5000);
                    OscilloscopeStartupOutputOvershoot2(DemandCurrent);
                    System.Threading.Thread.Sleep(3000);


                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 50, DemandVoltage - 5, DemandCurrent, false);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage - 5, DemandCurrent);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 50);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    dicTemp = new Dictionary<int, int>();
                    count = 10;
                    while (count-- > 0)
                    {
                        dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                        if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                        {
                            //启动负载时有继电器动作干扰，添加延时
                            //if (ControlEquipMent.LoopFeedbackLoad != null)
                            //    Thread.Sleep(5000);
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent < 5)
                                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent * 0.667).ToString(), "Single");
                            else
                                break;
                        }
                        else
                            Thread.Sleep(1000);

                    }
                    //System.Threading.Thread.Sleep(5000);

                    ReadTriggerType(testWorkParam.lstIDs, 30);

                    System.Threading.Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("读取示波器电流最大值...");
                    InDCImaxs = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);     //采集的是输入交流电流，最大值
                    System.Threading.Thread.Sleep(3000);

                    dImgs2 = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    dErrorRate2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double InDCImax = Convert.ToDouble(InDCImaxs[item]);
                        double ErrorRate2 = System.Math.Abs((InDCImax - DemandCurrent) / DemandCurrent);
                        ErrorRate2 = ErrorRate2 * 100;
                        dErrorRate2.Add(item, ErrorRate2.ToString("F2"));
                    }

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

                    ProcessDataTmp(dic2, "暂停状态进入充电", "稳流大于等于30A充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "暂停状态进入充电", "稳流大于等于30A充电电流(A)", "-", "-");
                    ProcessDataTmp(InDCImaxs, "暂停状态进入充电", "稳流大于等于30A输入电流最大值(A)", "-", "-");
                    ProcessDataTmp(dErrorRate2, "暂停状态进入充电", "稳流大于等于30A过冲测试结果(%)", "0", ErrorPercentage2.ToString(), dImgs2);
                    #endregion

                    #region 启动输出过冲第三步 恒流-新的测试流程
                    SendNoticeToUIAndTxtFile("正在测试第三个点...");
                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("实战示波器触发自动...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");

                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);
                    Thread.Sleep(10 * 1000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-");
                    dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicC, "暂停状态数据", "输出电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);



                    System.Threading.Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent2, false, DemandVoltage);

                    System.Threading.Thread.Sleep(6000);



                    OscilloscopeStartupOutputOvershoot3(DemandCurrent2);
                    System.Threading.Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 20, DemandVoltage, DemandCurrent2, false);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, false);
                    //if (ControlEquipMent.ResistanceLoad != null)
                    //{

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, DemandVoltage, DemandCurrent2);
                    //    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                    //}
                    //else
                    //{
                    //    if (ControlEquipMent.FeedbackLoad != null)
                    //    {
                    //        ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 20);
                    //        ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                    //    }
                    //}

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    dicTemp = new Dictionary<int, int>();
                    count = 10;
                    while (count-- > 0)
                    {
                        dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                        if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                        {
                            //启动负载时有继电器动作干扰，添加延时
                            //if (ControlEquipMent.LoopFeedbackLoad != null)
                            //    Thread.Sleep(5000);
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent < 5)
                                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent2 * 0.667).ToString(), "Single");
                            else
                                break;
                        }
                        else
                            Thread.Sleep(1000);

                    }
                    //System.Threading.Thread.Sleep(5000);

                    ReadTriggerType(testWorkParam.lstIDs, 30);

                    System.Threading.Thread.Sleep(9000);

                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    System.Threading.Thread.Sleep(2000);



                    SendNoticeToUIAndTxtFile("读取示波器电流最大值...");
                    InDCImax2s = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);     //采集的是输入交流电流，最大值
                    System.Threading.Thread.Sleep(3000);

                    dImgs3 = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    dErrorRate3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double InDCImax2 = Convert.ToDouble(InDCImax2s[item]);
                        double ErrorRate3 = InDCImax2 - DemandCurrent2;
                        dErrorRate3.Add(item, ErrorRate3.ToString("F2"));
                    }

                    dic3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic3.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicC3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC3.Add(item, DCCurrent.ToString("F2"));
                    }


                    ProcessDataTmp(dic3, "暂停状态进入充电", "稳流小于30A充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC3, "暂停状态进入充电", "稳流小于30A充电电流(A)", "-", "-");
                    ProcessDataTmp(InDCImax2s, "暂停状态进入充电", "稳流小于30A输入电流最大值(A)", "-", "-");
                    ProcessDataTmp(dErrorRate3, "暂停状态进入充电", "稳流小于30A过冲测试结果(A)", "0", ErrorValue.ToString(), dImgs3);





                    #endregion

                    #endregion


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);




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




            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 示波器启动输出过冲
        /// </summary>
        public void OscilloscopeStartupOutputOvershoot1(double Voltage)
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_V", "1M", "V", false, "150", "-2");//通道1设置
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1M", "A", false, "10", "-3");//通道2设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道3设置
                System.Threading.Thread.Sleep(100);

                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置

                System.Threading.Thread.Sleep(1000);

                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, (Voltage - 10).ToString(), "Single");
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0.1");//设置滚动，时基和触发延时
                System.Threading.Thread.Sleep(100);



                //ControlEquipMent.Oscilloscope?.Oscilloscope_CursorPosition_X_Time(testWorkParam.lstIDs, 1, 0, true);
                //System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");//设置测量项为XY
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//初始化测量
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 1);
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 2);
                System.Threading.Thread.Sleep(100);



                System.Threading.Thread.Sleep(100);


            }
            catch(Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 示波器启动输出过冲2
        /// </summary>
        public void OscilloscopeStartupOutputOvershoot2(double DemandCurrent)
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0.4");//设置滚动，时基和触发延时
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent * 0.667).ToString(), "Single");
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 示波器启动输出过冲3
        /// </summary>
        public void OscilloscopeStartupOutputOvershoot3(double DemandCurrent)
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent * 0.667).ToString(), "Single");
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

    }
}
