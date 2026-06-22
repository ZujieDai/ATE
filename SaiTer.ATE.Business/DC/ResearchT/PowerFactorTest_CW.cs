using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  功率因数试验(恒功率)
    /// </summary>
    public class PowerFactorTest_CW : BusinessBase
    {
        int trlTimeOut_S = 30;
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double RealPower = 0;
        double Rate = 0;
        double powerfactor = 0;

        public PowerFactorTest_CW(int type)
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
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            //BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            //ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            //RatedMinVoltage = MaxOutputPower * 1000 / MaxAllowChargeCurrent;
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
                    double[] Voltage = new double[7];
                    double[] Current = new double[7];
                    var IsCWRate = ConfigurationManager.AppSettings["IsCWRate"];
                    if (IsCWRate != null && Convert.ToBoolean(IsCWRate))
                    {
                        Voltage = new double[12];
                        Current = new double[12];
                    }

                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = MaxAllowChargeVoltage / 2;
                    Voltage[2] = MaxAllowChargeVoltage;
                    Voltage[3] = MaxAllowChargeVoltage;
                    Voltage[4] = MaxAllowChargeVoltage;     //U'max Pmax/U'max
                    Voltage[5] = (MaxOutputPower * 1000 / MaxAllowChargeCurrent + MaxAllowChargeVoltage) / 2; //U'men Pmax/U'men
                    Voltage[6] = MaxOutputPower * 1000 / MaxAllowChargeCurrent; //U'min Pmax/U'min
                    if (Voltage.Length == 12)
                    {
                        Voltage[7] = (CWHightVoltH + CWHightVoltL) / 2;
                        Voltage[8] = CWHightVoltL;
                        Voltage[9] = CWLowerVoltH;
                        Voltage[10] = (CWLowerVoltH + CWLowerVoltL) / 2;
                        Voltage[11] = CWLowerVoltL;
                    }

                    Current[0] = RatedCurrent;
                    Current[1] = RatedCurrent;
                    Current[2] = RatedCurrent * 0.2;
                    Current[3] = RatedCurrent * 0.5;
                    Current[4] = MaxOutputPower * 1000 / Voltage[4];
                    Current[5] = MaxOutputPower * 1000 / Voltage[5];
                    Current[6] = MaxOutputPower * 1000 / Voltage[6];
                    if (Current.Length == 12)
                    {
                        Current[7] = MaxOutputPower * 1000 / Voltage[7];
                        Current[8] = MaxOutputPower * 1000 / CWHightVoltL;
                        Current[9] = MaxOutputPower * 1000 / CWLowerVoltH;
                        Current[10] = MaxOutputPower * 1000 / Voltage[10];
                        Current[11] = MaxOutputPower * 1000 / CWLowerVoltL;
                    }
                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);


                    SendNoticeToUIAndTxtFile("启动充电");
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}

                    for (int i = 0; i < Voltage.Length; i++)
                    {
                        TrialMethod(Voltage[i], Current[i], true);
                    }
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        for (int i = 0; i < Voltage.Length; i++)
                        {
                            TrialMethod(Voltage[i], Current[i], true);
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
                    if ((voltage + 20) >= MaxAllowChargeVoltage)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, current, false, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", MaxAllowChargeVoltage, current));
                        CheckVoltage = MaxAllowChargeVoltage;
                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage + 20, current, false, voltage);
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage + 20, current));
                        CheckVoltage = voltage + 20;
                    }
                }
                WaitDCVoltage(testWorkParam.lstIDs, CheckVoltage);
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
                    if (current == MaxAllowChargeVoltage)
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
                        SetLoadPara(testWorkParam.lstIDs, voltage, current + 20, voltage - 5, current);
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
                string chargerType = "";
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && Customer.Contains("ZD"))
                    chargerType = type ? "（恒压）" : "（恒流）";
                string itemFlow = Rate <= 0.5 ? "20%≤P0/PN≤50%" + chargerType : "50%＜P0/PN≤100%" + chargerType;
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
                    //string itemFlow = "20%≤P0/PN≤50%" + chargerType;
                    dic.Add(LstChargerInfo[0].ChargerId, powerfactor.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "0.95", "1");
                }
                else
                {
                    //string itemFlow = "50%＜P0/PN≤100%" + chargerType;
                    dic.Add(LstChargerInfo[0].ChargerId, powerfactor.ToString("F3"));
                    ProcessDataTmp(dic, itemFlow, "电压" + voltage + "V,电流" + current + "A,功率因数", "0.98", "1");
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
