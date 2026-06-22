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
    /// 欧标研测直流：直流输出电流调节
    /// </summary>
    public class CCS2_RT_DC_OutputCurrentRegulation : BusinessBase
    {
        int trlTimeOut_S = 30;
        double BMSDemandVolt = 0;
        double OutputCurrent = 50;

        public CCS2_RT_DC_OutputCurrentRegulation(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            //BMS需求电流起始值(A)=50|BMS需求电压(V)=600
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                OutputCurrent = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                BMSDemandVolt = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
            else
            {
                BMSDemandVolt = (MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2;
            }
        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt + 20, OutputCurrent))
                {
                    return;
                }
                Thread.Sleep(2000);

                SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + OutputCurrent + "A，等待负载稳定");
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt, OutputCurrent + 10, BMSDemandVolt, OutputCurrent + 10);
                Thread.Sleep(500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, OutputCurrent);
                Thread.Sleep(1000 * 3);

                Dictionary<int, string> dic = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double dCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    int count = 10;
                    while (count-- > 0)
                    {
                        if (dCurrent < OutputCurrent * 0.97 || dCurrent > OutputCurrent * 1.03)
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
                ProcessDataTmp(dic, "调节电流前", "直流输出电流(A)", "-", "-");

                List<double> lstCurrent = new List<double>() { OutputCurrent + 4, OutputCurrent + 10, OutputCurrent + 59, OutputCurrent + 8 };
                double[] Currents = RetainDecimals<double>(lstCurrent.ToArray());
                Currents = CompareMaximum(Currents, MaxAllowChargeCurrent);
                if (Currents.Max() * (BMSDemandVolt + 20) > MaxOutputPower * 1000)
                    BMSDemandVolt = Math.Round(MaxOutputPower * 1000 * 0.95 / Currents.Max());
                string[] sStates = new string[4] { "调节电流4A（小于5A）", "调节电流6A（5至50A）", "调节电流49A（5至50A）", "调节电流51A（大于50A）" };

                for (int c = 0; c < Currents.Length; c++)
                {
                    //调节电流 < 5A,误差允许±150mA
                    //调节电流 5-50A，误差允许±1.5A
                    //调节电流 > 50A,误差允许±3% * 桩最大输出电流
                    double error = 0.15;
                    if (c > 0)
                    {
                        double diff = Currents[c] - Currents[c - 1];
                        if (diff < 5)
                            error = 0.15;
                        else if(diff >= 5 && diff <= 50)
                            error = 1.5;
                        else if(diff > 50)
                            error = MaxAllowChargeCurrent * 0.03;
                    }
                    string sState = sStates[c];
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt + 20, Currents[c], false, BMSDemandVolt);
                    ProcessDataResult(testWorkParam.lstIDs, (BMSDemandVolt + 20).ToString(), "BMS需求电压(V)", true, sState);
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (Currents[c]) + "A，等待负载稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt, Currents[c] + 10, BMSDemandVolt, Currents[c]);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, Currents[c]);
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

                    dic = new Dictionary<int, string>();
                    double minCurrent = Currents[c] - error;
                    double maxCurrent = Currents[c] + error;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double dCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                        int count = 100;
                        while (count-- > 0)
                        {
                            if (dCurrent < minCurrent || dCurrent > maxCurrent)
                            {
                                dCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                                Thread.Sleep(200);
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
