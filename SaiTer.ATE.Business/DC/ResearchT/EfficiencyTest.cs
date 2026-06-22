using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  效率试验
    /// </summary>
    public class EfficiencyTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double RealPower = 0;
        double Rate = 0;
        double efficiency = 0;
        double powerfactor = 0;

        public EfficiencyTest(int type)
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
                    SendNoticeToUIAndTxtFile("负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
                    double[] Voltage = new double[7];
                    double[] Current = new double[7];

                    //double RatedMidVoltage = (MaxAllowChargeVoltage + RatedMinVoltage) / 2;


                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = MaxAllowChargeVoltage / 2;
                    Voltage[2] = MaxAllowChargeVoltage;
                    Voltage[3] = MaxAllowChargeVoltage;
                    Voltage[4] = MaxAllowChargeVoltage;
                    Voltage[5] = RatedMidVoltage > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : RatedMidVoltage;
                    Voltage[6] = RatedMinVoltage > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : RatedMinVoltage;

                    Current[0] = RatedCurrent;
                    Current[1] = RatedCurrent;
                    Current[2] = RatedCurrent * 0.2;
                    Current[3] = RatedCurrent * 0.5;
                    Current[4] = RatedCurrent;
                    Current[5] = (MaxOutputPower * 1000) / RatedMidVoltage;
                    Current[6] = MaxAllowChargeCurrent;

                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);


                    bool[] LOAD = new bool[7];
                    for (int i = 0; i < 7; i++)
                    {
                        double VoltageMax = Current[i] / 0.24;

                        if (VoltageMax <= Voltage[i])
                        {
                            LOAD[i] = true;
                        }
                    }



                    //Current[0] = GetRelCurrent(Voltage[0], Current[0]);
                    //Current[1] = GetRelCurrent(Voltage[1], Current[1]);
                    //Current[2] = GetRelCurrent(Voltage[2], Current[2]);
                    //Current[3] = GetRelCurrent(Voltage[3], Current[3]);
                    //Current[4] = GetRelCurrent(Voltage[4], Current[4]);
                    //Current[5] = GetRelCurrent(Voltage[5], Current[5]);

                    SendNoticeToUIAndTxtFile("启动充电");
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
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    for (int i = 0; i < 7; i++)
                    {

                        if (IsRLoad(Voltage[i], Current[i]))
                        {
                            SetLoadDCOFF(testWorkParam.lstIDs);
                            double CheckVoltage = 0;

                            SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}", Voltage[i]));
                            if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState != "充电中")
                            {
                                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                                Thread.Sleep(300);
                                if (!CheckSwipingCard(testWorkParam.lstIDs))
                                {
                                    return;
                                }
                            }
                            Thread.Sleep(1000);
                            if (i == 4 || i == 6)
                            {
                                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], MaxAllowChargeCurrent, true, Voltage[i]);
                                Thread.Sleep(300);
                                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], MaxAllowChargeCurrent, true, Voltage[i]);
                            }
                            else
                            {
                                if (Voltage[i] == MaxAllowChargeVoltage)
                                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current[i] + 20, true, Voltage[i]);
                                else
                                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current[i], true, Voltage[i]);
                                Thread.Sleep(300);
                                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current[i] + 20, true, Voltage[i]);
                            }

                            CheckVoltage = Voltage[i];
                            int timeout = 40;
                            while (timeout > 0)
                            {
                                double value = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage;
                                timeout--;
                                if (value >= CheckVoltage * 0.8 && value <= CheckVoltage * 1.2)
                                {
                                    break;
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            Stopwatch st = new Stopwatch();

                            if (i == 4 || i == 6)
                            {
                                double f_loadVolt = Voltage[i], f_loadCurrent = Current[i], r_loadVolt = Voltage[i], r_loadCurrent = Current[i];
                                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                                {
                                    f_loadVolt = Voltage[i] - 20;
                                    if(Voltage[i] == MaxAllowChargeVoltage)
                                        f_loadCurrent = Current[i] - 5;
                                    else
                                        f_loadCurrent = Current[i] + 10;
                                    SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0},充电电流为{1}", f_loadVolt, f_loadCurrent));
                                }
                                else /*if (ControlEquipMent.ResistanceLoad != null)*/
                                {
                                    r_loadVolt = Voltage[i] - 5;
                                    r_loadCurrent = Current[i] - 5;
                                    SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0},充电电流为{1}", r_loadVolt, r_loadCurrent));
                                }
                                SetLoadPara(testWorkParam.lstIDs, f_loadVolt, f_loadCurrent, r_loadVolt, r_loadCurrent);
                                Thread.Sleep(800);
                                //SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 20, Current[i] + 10, Voltage[i] - 10, Current[i]);
                                //Thread.Sleep(800);
                                SetLoadDCON(testWorkParam.lstIDs);
                                st.Start();
                                while (st.ElapsedMilliseconds / 1000 < 15)
                                {
                                    double current = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                                    if (current >= (Current[i] - 5) * 0.9 && current <= (Current[i] - 5) * 1.1)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0},充电电流为{1}", Voltage[i] - 20, Current[i]));
                                SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 20, Current[i], Voltage[i] - 20, Current[i]);

                                Thread.Sleep(300);
                                SetLoadDCON(testWorkParam.lstIDs);
                                st.Restart();
                                while (st.ElapsedMilliseconds / 1000 < 25)
                                {
                                    double current = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                                    if (current >= (Current[i]) * 0.9 && current <= (Current[i]) * 1.1)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                            }


                            st.Restart();

                            while (st.ElapsedMilliseconds / 1000 < 35)
                            {

                                double Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;

                                if (i == 4 || i == 6)
                                {
                                    double value = Current[i] - 5;
                                    if (Current_4 > value * 0.9)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (Current_4 > Current[i] * 0.9)
                                    {
                                        break;
                                    }
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
                            if (Rate <= 0.5)
                            {
                                string chargeMode = Voltage[i] == MaxAllowChargeVoltage ? "恒压" : "恒流";
                                string itemFlow = "20%≤P0/PN≤50%" + chargeMode;
                                dic.Add(LstChargerInfo[0].ChargerId, totalPower.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,输入功率", "-", "-");
                                dic.Clear();
                                dic.Add(LstChargerInfo[0].ChargerId, RealPower.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,输出功率", "-", "-");
                                dic.Clear();
                                dic.Add(LstChargerInfo[0].ChargerId, efficiency.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,效率值", "88", "100");
                            }
                            else
                            {
                                string chargeMode = Voltage[i] == MaxAllowChargeVoltage ? "恒压" : "恒流";
                                string itemFlow = "50%＜P0/PN≤100%" + chargeMode;
                                dic.Add(LstChargerInfo[0].ChargerId, totalPower.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,输入功率", "-", "-");
                                dic.Clear();
                                dic.Add(LstChargerInfo[0].ChargerId, RealPower.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,输出功率", "-", "-");
                                dic.Clear();
                                dic.Add(LstChargerInfo[0].ChargerId, efficiency.ToString("F3"));
                                ProcessDataTmp(dic, itemFlow, "电压" + Voltage[i] + "V,电流" + Current[i] + "A,效率值", "93", "100");
                            }
                            SetLoadDCOFF(testWorkParam.lstIDs);
                            Thread.Sleep(300);
                            //SetLoadDCOFF(testWorkParam.lstIDs);

                            Thread.Sleep(1000);
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
