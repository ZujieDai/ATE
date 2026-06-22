using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电流控制误差
    /// </summary>
    public class CCS2_PT_DC_OutputCurrentControlError : BusinessBase
    {
        int trlTimeOut_S = 30;
        double OutputCurrent1 = 0;
        double Error1 = 0.3;
        double OutputCurrent2 = 0;
        double Error2 = 1;
        double BMSDemandVolt = 0;
        public CCS2_PT_DC_OutputCurrentControlError(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //输出电流1(A) = 20 |±误差(A) = 0.3 | 输出电流2(A) = 40 |±误差(%) = 1
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputCurrent1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            Error1 = Convert.ToDouble(strParams[1].Split('=')[1]);
            OutputCurrent2 = Convert.ToDouble(strParams[2].Split('=')[1]);
            Error2 = Convert.ToDouble(strParams[3].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
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

                bool isFeedbackLoad = ControlEquipMent.FeedbackLoad != null;
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                SetConditionValues();


                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, OutputCurrent1, false, BMSDemandVolt);

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, BMSDemandVolt.ToString());
                    d2.Add(item, OutputCurrent1.ToString());
                }
                ProcessDataTmp(d1, "设定的输出电流值小于30A", "BMS需求电压(V)", "-", "-");
                ProcessDataTmp(d2, "设定的输出电流值小于30A", "BMS需求电流(A)", "-", "-");

                Thread.Sleep(500);


                SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (OutputCurrent1) + "A，等待负载稳定");
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, OutputCurrent1 + 10, BMSDemandVolt - 5, OutputCurrent1);
                Thread.Sleep(500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, OutputCurrent1);
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
                ProcessDataTmp(d1, "设定的输出电流值小于30A", "直流输出电压(V)", "-", "-");

                Dictionary<int, string> dic = new Dictionary<int, string>();
                double minCurrent = OutputCurrent1 - Error1;
                double maxCurrent = OutputCurrent1 + Error1;
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
                ProcessDataTmp(dic, "设定的输出电流值小于30A", "输出" + OutputCurrent1 + "A", (OutputCurrent1 - Error1).ToString("F2"), (OutputCurrent1 + Error1).ToString("F2"));


                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, OutputCurrent2, false, BMSDemandVolt);
                Thread.Sleep(1000);

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, BMSDemandVolt.ToString());
                    d2.Add(item, OutputCurrent2.ToString());
                }
                ProcessDataTmp(d1, "设定的输出电流值大于30A", "BMS需求电压(V)", "-", "-");
                ProcessDataTmp(d2, "设定的输出电流值大于30A", "BMS需求电流(A)", "-", "-");


                SendNoticeToUIAndTxtFile("设置带载电压" + BMSDemandVolt + "V,带载电流" + (OutputCurrent2) + "A，等待负载稳定");
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, OutputCurrent2 + 10, BMSDemandVolt - 5, OutputCurrent2);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, OutputCurrent2);
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
                ProcessDataTmp(d1, "设定的输出电流值大于30A", "直流输出电压(V)", "-", "-");

                minCurrent = OutputCurrent2 * (1 - Error2 / 100);
                maxCurrent = OutputCurrent2 * (1 + Error2 / 100);
                dic.Clear();
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
                ProcessDataTmp(dic, "设定的输出电流值大于30A", "输出" + OutputCurrent2 + "A", (OutputCurrent2 * (1 - Error2 / 100)).ToString("F2"), (OutputCurrent2 * (1 + Error2 / 100)).ToString("F2"));
                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
            }

        }
        public override void ProcessData()
        {

        }

    }
}
