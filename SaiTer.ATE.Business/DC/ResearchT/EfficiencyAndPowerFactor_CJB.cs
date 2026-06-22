using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class EfficiencyAndPowerFactor_CJB : BusinessBase
    {
        int trlTimeOut_S = 30;
        double RealPower = 0;
        double Rate = 0;
        double efficiency = 0;
        double powerfactor = 0;

        public EfficiencyAndPowerFactor_CJB(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
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

                    SendNoticeToUIAndTxtFile("负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
                    double[] Voltage = new double[3];
                    double[] Current = new double[3];

                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = (MinAllowChargeVoltage + MaxAllowChargeVoltage) / 2;
                    Voltage[2] = MaxAllowChargeVoltage;

                    Current[0] = MaxAllowChargeCurrent * 0.2;
                    Current[1] = MaxAllowChargeCurrent * 0.5;
                    Current[2] = MaxAllowChargeCurrent;
                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);


                    for (int i = 0; i < Voltage.Length; i++)
                    {
                        for (int j = 0; j < Current.Length; j++)
                        {
                            TrialMethod(Voltage[i], Current[j], true);
                        }
                    }

                    SendNoticeToUIAndTxtFile("负载取消并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                }
            }

            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voltage">需求电压</param>
        /// <param name="current">需求电流</param>
        /// <param name="type">恒压=true，恒流=false</param>
        private void TrialMethod(double voltage, double current, bool type)
        {
            if (IsRLoad(voltage, current))
            {
                SetLoadDCOFF(testWorkParam.lstIDs);
                double CheckVoltage = 0, CheckCurrent = 0;

                if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState != "充电中")
                {
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    if ((current + 20) >= MaxAllowChargeCurrent)
                    {
                        if (!CheckSwipingCard(testWorkParam.lstIDs, voltage, MaxAllowChargeCurrent))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!CheckSwipingCard(testWorkParam.lstIDs, voltage, current + 20))
                        {
                            return;
                        }
                    }
                }
                Thread.Sleep(1000);

                //恒压
                if (type)
                {
                    if ((current + 20) >= MaxAllowChargeCurrent)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, MaxAllowChargeCurrent, true, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, MaxAllowChargeCurrent));
                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, current + 20, true, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, current + 20));
                    }
                    CheckVoltage = voltage;
                }
                else
                {
                    if ((current + 10) >= MaxAllowChargeCurrent)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, MaxAllowChargeCurrent, false, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, MaxAllowChargeCurrent));
                        CheckVoltage = MaxAllowChargeVoltage;
                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, current, false, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, current));
                        CheckVoltage = voltage + 20;
                    }
                }
                WaitDCVoltage(testWorkParam.lstIDs, CheckVoltage, 120);
                Thread.Sleep(3000);

                if (type)
                {
                    if (current == MaxAllowChargeCurrent)
                    {
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current - 5));
                        else if (ControlEquipMent.ResistanceLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current - 5));
                        SetLoadPara(testWorkParam.lstIDs, voltage - 20, current - 5, voltage - 5, current - 5);
                        CheckCurrent = current - 5;
                    }
                    else
                    {
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current));
                        else if (ControlEquipMent.ResistanceLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current));
                        SetLoadPara(testWorkParam.lstIDs, voltage - 20, current, voltage - 5, current);
                        CheckCurrent = current;
                    }
                }
                else
                {
                    if ((current + 10) >= MaxAllowChargeCurrent)
                    {
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current));
                        else if (ControlEquipMent.ResistanceLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current));
                        SetLoadPara(testWorkParam.lstIDs, voltage - 20, current, voltage - 5, current);
                        CheckCurrent = current;
                    }
                    else
                    {
                        if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current + 10));
                        else if (ControlEquipMent.ResistanceLoad != null)
                            SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current));
                        SetLoadPara(testWorkParam.lstIDs, voltage - 20, current + 10, voltage - 5, current);
                        CheckCurrent = current;
                    }
                }
                Thread.Sleep(300);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, CheckCurrent);
                Thread.Sleep(2000);


                Stopwatch st = new Stopwatch();
                st.Restart();
                while (st.ElapsedMilliseconds / 1000 < 35)
                {
                    double Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    if (Current_4 > CheckCurrent * 0.9)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                Thread.Sleep(3000);//等待功率分析仪计算数据

                double totalPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].TotalPower;
                RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4Power;
                Rate = RealPower / MaxOutputPower;
                efficiency = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Efficiency;//效率
                for (int j = 0; j < 5; j++)
                {
                    if (efficiency < 88 || efficiency >= 100)
                    {
                        Thread.Sleep(1000);
                        efficiency = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Efficiency;
                        RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4Power;
                        totalPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].TotalPower;
                    }
                    else
                    {
                        break;
                    }
                }
                powerfactor = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel1PowerFactor;//功率因数
                for (int j = 0; j < 5; j++)
                {
                    if (powerfactor < 0.95 || powerfactor > 1)
                    {
                        Thread.Sleep(1000);
                        powerfactor = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].TotalPowerFactor;
                    }
                    else
                    {
                        break;
                    }
                }
                Dictionary<int, string> dic = new Dictionary<int, string>();
                string itemFlow = Rate <= 0.5 ? "20%≤P0/PN≤50%" : "50%＜P0/PN≤100%";
                if (Customer != null && Customer.Contains("CJB"))
                {
                    //输入
                    Dictionary<int, string> dicV = new Dictionary<int, string>();
                    double voltageData = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel1RMSVolt;
                    dicV.Add(LstChargerInfo[0].ChargerId, voltageData.ToString("F3"));
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    double currentData = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel1RMSCurrent;
                    dicC.Add(LstChargerInfo[0].ChargerId, currentData.ToString("F3"));
                    ProcessDataTmp(dicV, itemFlow, "电压" + voltage + "V,电流" + current + "A,输入电压", "-", "-");
                    ProcessDataTmp(dicC, itemFlow, "电压" + voltage + "V,电流" + current + "A,输入电流", "-", "-");
                    //输出
                    dicV = new Dictionary<int, string>();
                    voltageData = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSVolt;
                    dicV.Add(LstChargerInfo[0].ChargerId, voltageData.ToString("F3"));
                    dicC = new Dictionary<int, string>();
                    currentData = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    dicC.Add(LstChargerInfo[0].ChargerId, currentData.ToString("F3"));
                    ProcessDataTmp(dicV, itemFlow, "电压" + voltage + "V,电流" + current + "A,输出电压", "-", "-");
                    ProcessDataTmp(dicC, itemFlow, "电压" + voltage + "V,电流" + current + "A,输出电流", "-", "-");
                }
                if (Rate <= 0.5)
                {
                    dic.Add(LstChargerInfo[0].ChargerId, totalPower.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,输入功率", "-", "-");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, RealPower.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,输出功率", "-", "-");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, efficiency.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,效率值", "88", "100");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, powerfactor.ToString("F3"));
                    if (RealPower >= MaxOutputPower * 0.2)
                        ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "0.95", "1");
                    else
                        ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "-", "-");
                }
                else
                {
                    dic.Add(LstChargerInfo[0].ChargerId, totalPower.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,输入功率", "-", "-");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, RealPower.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,输出功率", "-", "-");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, efficiency.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,效率值", "93", "100");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, powerfactor.ToString("F3"));
                    if (RealPower >= MaxOutputPower * 0.2)
                        ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "0.98", "1");
                    else
                        ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "-", "-");
                }
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
                //SetLoadDCOFF(testWorkParam.lstIDs);

                Thread.Sleep(1000);
            }
        }

        private double GetRelCurrent(double VOLTAGE, double CURRENT)
        {
            if (ControlEquipMent.FeedbackLoad != null)
            {
                return CURRENT;
            }
            else if (ControlEquipMent.ResistanceLoad != null)
            {
                Double NewCurrent = CURRENT / 2;

                Double CheckCCurrent = VOLTAGE * 0.12;

                NewCurrent = NewCurrent > CheckCCurrent ? CheckCCurrent : NewCurrent;

                return NewCurrent * 2;
            }
            return CURRENT;
        }
        public override void ProcessData()
        {

        }
    }
}
