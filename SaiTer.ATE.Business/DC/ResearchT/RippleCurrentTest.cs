using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研测（录波仪）：电流纹波试验
    /// </summary>
    public class RippleCurrentTest : BusinessBase
    {
        int trlTimeOut_S = 30;

        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储



        public RippleCurrentTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_StorageDepth("1000000");
            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");

            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "1", "");

            ControlEquipMent.Oscillograph?.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("PTOPeak", 1, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Min", 1, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Max", 1, false, 0);

        }

        public override void InitializeParams()
        {
            Init();
            dImgs = new Dictionary<int, string>();//图片存储
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "50", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200Hz", "BLUE", true, false, false, false);//通道1
                ControlEquipMent.Oscillograph?.Oscillograph_StorageDepth("500000");

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
                    SendNoticeToUIAndTxtFile("开启并机继电器");
                    CombineControlResistance();

                    SendNoticeToUIAndTxtFile("开启负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
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
                        d3.Add(testWorkParam.lstIDs[i], MaxOutputPower.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SetConditionValue("最大功率(kW)", d3);

                    ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "10Hz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(MaxAllowChargeVoltage.ToString(), RatedCurrent.ToString(), $"电流纹波频率≤10Hz", 1.5);

                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "5kHz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(MaxAllowChargeVoltage.ToString(), RatedCurrent.ToString(), $"电流纹波频率≤5kHz", 6);

                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "150kHz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(MaxAllowChargeVoltage.ToString(), RatedCurrent.ToString(), $"电流纹波频率≤150kHz", 9);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    double tVoltageSet = MaxOutputPower * 1000 / MaxAllowChargeCurrent;
                    tVoltageSet = tVoltageSet > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : tVoltageSet;
                    CheckSwipingCard(testWorkParam.lstIDs);
                    ////////////////////////////////
                    ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "10Hz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(tVoltageSet.ToString(), MaxAllowChargeCurrent.ToString("F2"), $"电流纹波频率≤10Hz", 1.5);

                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "5kHz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(tVoltageSet.ToString(), MaxAllowChargeCurrent.ToString("F2"), $"电流纹波频率≤5kHz", 6);

                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "AC", "300", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "150kHz", "BLUE", true, false, false, false);//通道1
                    //启动录波仪
                    CurrentRipple(tVoltageSet.ToString(), MaxAllowChargeCurrent.ToString("F2"), $"电流纹波频率≤150kHz", 9);



                    SendNoticeToUIAndTxtFile("关闭并机继电器");
                    SingleControlResistance();

                    SendNoticeToUIAndTxtFile("取消负载并机");

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }




        public void CurrentRipple(string tVoltageSet, string tCurrentSet, string state, double maxValue)//电流纹波
        {
            try
            {
                if (IsRLoad(Convert.ToDouble(tVoltageSet), Convert.ToDouble(tCurrentSet)))
                {
                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach(int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, tVoltageSet);
                        d2.Add(item, tCurrentSet);
                    }
                    ProcessDataTmp(d1, state, "输出直流电压点(V)", "-", "-");
                    ProcessDataTmp(d2, state, "输出电流点(A)", "-", "-");

                    Double voltageSet = System.Math.Abs(Convert.ToDouble(tVoltageSet));
                    Double currentSet = System.Math.Abs(Convert.ToDouble(tCurrentSet));//注意：取绝对值做比较


                    ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

                    if (voltageSet >= MaxAllowChargeVoltage)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltageSet, currentSet, false, voltageSet);
                        WaitDCVoltage(lstIDs, voltageSet);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        if (ControlEquipMent.ResistanceLoad != null)
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltageSet + 5, currentSet, false, voltageSet);
                        else
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltageSet + 20, currentSet, false, voltageSet);
                        WaitDCVoltage(lstIDs, voltageSet + 20);
                        Thread.Sleep(2000);
                    }

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载稳定");
                    if (voltageSet >= MaxAllowChargeVoltage)
                    {
                        SetLoadPara(testWorkParam.lstIDs, voltageSet - 10, currentSet + 20, voltageSet - 5, currentSet);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);

                    }
                    else
                    {
                        SetLoadPara(testWorkParam.lstIDs, voltageSet, currentSet + 20, voltageSet, currentSet);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);
                    }
                    WaitDCCurrent(testWorkParam.lstIDs, currentSet);
                    Thread.Sleep(15 * 1000);

                    int timeout = 35;
                    double Current = 0;
                    while (timeout-- > 0)
                    {
                        Current = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                        if (Current > currentSet * 0.9)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    double CurrentSet = currentSet;
                    Current = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                    ControlEquipMent.Oscillograph.Oscillograph_IsRun(false);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(testWorkParam.lstIDs[0], Current.ToString());
                    // ProcessDataTmp(dic, "预置条件", "带载状态", (CurrentSet * 0.8).ToString("F2"), (CurrentSet * 1.1).ToString("F2"));
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("读录波仪交流电压峰峰值");
                    dic.Clear();
                    foreach (int i in testWorkParam.lstIDs)
                    {
                        double PKPK = OscillographInstrumentReadMeasure("PTOPeak", 1, false, 0);
                        int count = 10;
                        while (count-- > 0)
                        {
                            if (PKPK == 0 || PKPK > maxValue)
                            {
                                PKPK = OscillographInstrumentReadMeasure("PTOPeak", 1, false, 0); //交流分量峰峰值
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                        }
                        dic.Add(i, PKPK.ToString("F2"));
                    }

                    dImgs = ControlEquipMent.Oscillograph.OscillographSaveScreen();

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (current < currentSet * 0.9 || current > currentSet * 1.1)
                            {
                                current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                                Thread.Sleep(100);
                                continue;
                            }
                            break;
                        }
                        d1.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(d1, state, "直流输出电流(A)", (currentSet * 0.9).ToString("F2"), (currentSet * 1.1).ToString("F2"));
                    ProcessDataTmp(dic, state, "输出电流纹波峰峰值(A)", "0", maxValue.ToString(), dImgs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
