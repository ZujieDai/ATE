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
    /// 输出电流设定误差（XJ专用）
    /// </summary>
    public class GB_RT_DC_OutputCurrentSetError_XJ : BusinessBase
    {
        int trlTimeOut_S = 30;
        double Error1 = 0.3;
        double Error2 = 1;
        double BMSDemandVolt = 0;
        List<double> Test_Voltages = new List<double>();
        List<double> Test_Currents = new List<double>();
        public GB_RT_DC_OutputCurrentSetError_XJ(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            ////±误差(A) = 0.3|±误差(%) = 1
            //string[] strParams = TrialItem.ResultParams.Split('|');
            //Error1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            //Error2 = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = (MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2;

            Dictionary<string, string> strItemParams = LoadTrialItemParams(TrialItem.ResultParams);
            Error1 = ConvertStringToDouble(GetDicValue(strItemParams, "电流＜30A误差限(A)", "0.3"));
            Error2 = ConvertStringToDouble(GetDicValue(strItemParams, "电流≥30A误差限(%)", "1"));
            BMSDemandVolt = ConvertStringToDouble(GetDicValue(strItemParams, "输出电压(V)", BMSDemandVolt.ToString()));
            string stmpU = null;
            string stmpI = null;
            double dtmpU = 0;
            double dtmpI = 0;
            for (int i = 0; i < 15; i++)
            {
                //stmpU = GetDicValue(strItemParams, "输出电压" + (i + 1).ToString() + "(V)");
                stmpI = GetDicValue(strItemParams, "输出电流" + (i + 1).ToString() + "(A)");
                if (stmpI != null)
                {
                    if ( Convert.ToDouble(stmpI) == 0)
                    {
                        continue;
                    }
                    //dtmpU = ConvertStringToDouble(stmpU);
                    Test_Voltages.Add(BMSDemandVolt);

                    dtmpI = ConvertStringToDouble(stmpI);
                    dtmpI = dtmpI > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : dtmpI;
                    Test_Currents.Add(dtmpI);
                }
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
                SetConditionValues();
                d1 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    d1.Add(testWorkParam.lstIDs[i], RatedCurrent.ToString());
                }
                SetConditionValue("额定电流(A)", d1);

                bool isFeedbackLoad = ControlEquipMent.FeedbackLoad != null;
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                //List<double> lstCurrent = new List<double>() { 0.2 * RatedCurrent, 0.5 * RatedCurrent, MaxOutputPower * 1000 / BMSDemandVolt };
                //if (lstCurrent.Min() >= 30)
                //{
                //    lstCurrent.Add(20);
                //}
                double[] Currents = RetainDecimals<double>(Test_Currents.ToArray());
                Currents = CompareMaximum(Currents, MaxAllowChargeCurrent);
                //string[] sStates = new string[4] { $"输出电流点20%In={Currents[0]}A", $"输出电流点50%In={Currents[1]}A", $"Umen对应的最大输出电流值点={Currents[2]}A", $"小于30A电流点=20A" };
                List<string> sStates = new List<string>();
                foreach(var cur in Currents)
                {
                    sStates.Add($"输出电流点={cur}A");
                }

                ProcessDataResult(testWorkParam.lstIDs, "-", $"{BMSDemandVolt}", true, $"设定输出电压点(V) —— " + BMSDemandVolt.ToString());
                for (int c = 0; c < Currents.Length; c++)
                {
                    double error = Currents[c] < 30 ? Error1 : Currents[c] * Error2 / 100;
                    string sState = sStates[c];
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt + 20, Currents[c], false, BMSDemandVolt);
                    ProcessDataResult(testWorkParam.lstIDs, (BMSDemandVolt + 20).ToString(), "BMS需求电压(V)", true, sState);

                    //d1 = new Dictionary<int, string>();
                    //d2 = new Dictionary<int, string>();
                    //foreach (int item in testWorkParam.lstIDs)
                    //{
                    //    d1.Add(item, BMSDemandVolt.ToString());
                    //    d2.Add(item, Currents[c].ToString());
                    //}
                    //ProcessDataTmp(d1, "设定的输出电流值小于30A", "BMS需求电压(V)", "-", "-");
                    //ProcessDataTmp(d2, "设定的输出电流值小于30A", "BMS需求电流(A)", "-", "-");

                    Thread.Sleep(500);


                    SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (Currents[c]) + "A，等待负载稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt, Currents[c] + 20, BMSDemandVolt, Currents[c] + 10);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, Currents[c]);
                    Thread.Sleep(1000 * 3);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        int timeout = 50;
                        double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        while (timeout-- > 0)
                        {
                            if (voltage < BMSDemandVolt * 0.9 || voltage > BMSDemandVolt * 1.1)
                            {
                                voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        d1.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, sState, "直流输出电压(V)", "-", "-");

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    double minCurrent = Currents[c] - error;
                    double maxCurrent = Currents[c] + error;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double dCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                        int count = 10;
                        while (count-- > 0)
                        {
                            if (dCurrent < minCurrent || dCurrent > maxCurrent)
                            {
                                dCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                        }
                        dic.Add(testWorkParam.lstIDs[i], dCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, sState, "直流输出电流(A)", (Currents[c] - error).ToString("F2"), (Currents[c] + error).ToString("F2"));

                }
            }

        }
        public override void ProcessData()
        {

        }
    }
}
