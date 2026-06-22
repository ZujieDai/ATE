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
    /// 国标直流      功率控制
    /// </summary>
    public class PowerControl_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        string ItemFlow = "";
        double 判定准则 = 0;//±%


        public PowerControl_GB_DC(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SetCPReresh();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            if (strParams.Length > 1)
            {
                //电源模块恒功率最小电压(V)
                ModuleMinimumVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                    ItemFlow = "预先确认";
                    CountDownTimeInfo("请确认桩是否具备功率控制功能   \r\n勾选代表具备", 20, 2);
                    ProcessData();
                    if (!DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        return;
                    }


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    double OutputPower_New = 0;
                    #region
                    for (int k = 0; k < 3; k++)
                    {
                        string sState = "";
                        if (k == 0)
                        {
                            sState = "最大输出功率25%";
                            CountDownTimeInfo("请设置充电桩最大输出功率 25%", 30, 0);
                            OutputPower_New = MaxOutputPower * 0.25;
                        }
                        if (k == 1)
                        {
                            sState = "最大输出功率50%";
                            CountDownTimeInfo("请设置充电桩最大输出功率 50%", 30, 0);
                            OutputPower_New = MaxOutputPower * 0.50;
                        }
                        if (k == 2)
                        {
                            sState = "最大输出功率75%";
                            CountDownTimeInfo("请设置充电桩最大输出功率 75%", 30, 0);
                            OutputPower_New = MaxOutputPower * 0.75;
                        }
                        //ModuleMinimumVoltage = OutputPower_New * 1000 / MaxAllowChargeCurrent;
                        RatedMidVoltage = (MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2;

                        double[] Current = new double[3];
                        double[] Voltage = new double[3];
                        Voltage[0] = MaxAllowChargeVoltage;
                        //Voltage[2] = (OutputPower_New * 1000) / MaxAllowChargeCurrent;
                        Voltage[2] = ModuleMinimumVoltage;
                        Voltage[1] = (MaxAllowChargeVoltage + Voltage[2]) / 2;
                        Voltage = RetainDecimals<double>(Voltage);
                        Current[0] = (OutputPower_New * 1000) / MaxAllowChargeVoltage;
                        Current[1] = (OutputPower_New * 1000) / Voltage[1];
                        Current[2] = (OutputPower_New * 1000) / ModuleMinimumVoltage;
                        Current = RetainDecimals<double>(Current);
                        Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                        int Count = 0;
                        int rowIndex = 1;
                        for (int i = 0; i < 3; i++)
                        {

                            if (IsRLoad(Voltage[i], Current[i]))
                            {
                                int index = Count + 1;
                                SetLoadDCOFF(testWorkParam.lstIDs);
                                Thread.Sleep(3000);
                                if (!CheckSwipingCard(testWorkParam.lstIDs))
                                {
                                    return;
                                }
                                double CheckVoltage = 0;
                                if (i >= 1)
                                {
                                    SendNoticeToUIAndTxtFile("正在测试第" + index + "个点" + "电压为" + (Voltage[i] + 20).ToString() + "电流为" + Current[i].ToString());
                                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], 200, true, Voltage[i]);
                                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i] + 20, Current[i], true, Voltage[i]);
                                    CheckVoltage = Voltage[i] + 20;
                                    Thread.Sleep(3000);
                                    SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压{0}V,需求电流{1}A，启动负载，等待带载稳定", Voltage[i], Current[i] + 20));
                                    SetLoadPara(testWorkParam.lstIDs, Voltage[i], Current[i] + 20, Voltage[i], Current[i]);
                                    Thread.Sleep(300);
                                    SetLoadDCON(testWorkParam.lstIDs);
                                    Thread.Sleep(1000 * 15);
                                }
                                else
                                {
                                    SendNoticeToUIAndTxtFile("正在测试第" + index + "个点" + "电压为" + (Voltage[i]).ToString() + "电流为" + Current[i].ToString());
                                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], 200, true, Voltage[i]);
                                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current[i], true, Voltage[i]);
                                    CheckVoltage = Voltage[i];
                                    Thread.Sleep(3000);
                                    SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压{0}V,需求电流{1}A，启动负载，等待带载稳定", Voltage[i] - 10, Current[i] + 20));
                                    SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 10, Current[i] + 20, Voltage[i], Current[i]);
                                    Thread.Sleep(300);
                                    SetLoadDCON(testWorkParam.lstIDs);
                                    Thread.Sleep(1000 * 15);
                                }
                                int timeout = 40;
                                while (true)
                                {
                                    double value = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage;
                                    timeout--;
                                    if (value >= CheckVoltage * 0.8 && value <= CheckVoltage * 1.2)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(3000);

                                timeout = 35;
                                double Current_4 = 0;
                                while (timeout-- > 0)
                                {
                                    Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                                    if (Current_4 > Current[i] * 0.9)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }

                                Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;

                                SendNoticeToUIAndTxtFile("延时倒计时30s。");
                                Thread.Sleep(30 * 1000);

                                d1 = new Dictionary<int, string>();
                                d2 = new Dictionary<int, string>();
                                foreach(int item in testWorkParam.lstIDs)
                                {
                                    d1.Add(item, Voltage[i].ToString());
                                    d2.Add(item, Current[i].ToString());
                                }
                                ProcessDataTmp(d1, sState, "BMS需求电压(V)", "-", "-");
                                ProcessDataTmp(d2, sState, "BMS需求电流(A)", "-", "-");

                                double OutputPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4Power;
                                timeout = 30;
                                while (timeout-- > 0)
                                {
                                    if (OutputPower < OutputPower_New * (1 - 判定准则) || OutputPower > OutputPower_New * (1 + 判定准则))
                                    {
                                        OutputPower = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4Power;
                                        Thread.Sleep(100);
                                    }
                                    break;
                                }

                                double ErrorRate = (1 - (OutputPower / OutputPower_New)) * 100;
                                ErrorRate = RetainDecimals<double>(ErrorRate);
                                Dictionary<int, string> dic = new Dictionary<int, string>();
                                foreach (var item in testWorkParam.lstIDs)
                                {
                                    //dic.Add(LstChargerInfo[0].ChargerId, OutputPower.ToString("F2"));
                                    //ProcessDataTmp(dic, "导引电压" + Voltage[i] + "V,带载电流" + (Current[i] + 20).ToString() + "A", "输出功率值(Kw)", (MaxOutputPower * 0.99).ToString("F2"), (MaxOutputPower * 1.2).ToString("F2"));

                                    dic.Add(item, ErrorRate.ToString("F2"));
                                }
                                Dictionary<int, string> dicPowerNew = new Dictionary<int, string>();
                                foreach (var item in testWorkParam.lstIDs)
                                {
                                    dicPowerNew.Add(item, OutputPower_New.ToString("F2"));
                                }
                                ProcessDataTmp(dicPowerNew, sState, "充电设置输出功率(kW)", "-", "-");
                                Dictionary<int, string> dicPower = new Dictionary<int, string>();
                                foreach (var item in testWorkParam.lstIDs)
                                {
                                    double Power = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                                    dicPower.Add(item, Power.ToString("F2"));
                                }
                                ProcessDataTmp(dicPower, sState, "功率分析仪功率(kW)", "-", "-");
                                rowIndex++;
                                ProcessDataTmp(dic, sState, "输出功率误差(%)", (-判定准则).ToString("F2"), (+判定准则).ToString("F2"));
                            }
                            Count++;
                        }
                    }



                    #endregion
                }
            }
            catch (Exception ex) { SendException(ex); }

        }
        public override void ProcessData()
        {
            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].ItemName = ItemFlow;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;

                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = ItemFlow + "|是否具备充电控制功能|-|-|是";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = ItemFlow + "|是否具备充电控制功能|-|-|否";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式             
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }

        }
    }
}
