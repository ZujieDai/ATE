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
    public class OutputVoltageSetErrorTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double Error = 0.5;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public OutputVoltageSetErrorTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //±误差(%)=0.5
            string[] strParams = TrialItem.ResultParams.Split('|');
            Error = Convert.ToDouble(strParams[0].Split('=')[1]);
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

                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();
                d1 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    d1.Add(testWorkParam.lstIDs[i], RatedCurrent.ToString());
                }
                SetConditionValue("额定电流(A)", d1);

                double[] BMSVolts = new double[3] { MinAllowChargeVoltage, (MinAllowChargeVoltage + MaxAllowChargeVoltage) / 2, MaxAllowChargeVoltage };
                string[] sStates = new string[3] { $"输出电压点Umin={BMSVolts[0]}V", $"输出电压点Umen={BMSVolts[1]}V", $"输出电压点Umax={BMSVolts[2]}V" };

                ProcessDataResult(testWorkParam.lstIDs, "-", $"{RatedCurrent / 2}", true, $"设定输出电流点(A) —— 50%In");
                for (int v = 0; v < BMSVolts.Length; v++)
                {
                    double current = RatedCurrent / 2;

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolts[v], current + 10, true, BMSVolts[v]);
                    ProcessDataResult(testWorkParam.lstIDs, BMSVolts[v].ToString(), "BMS需求电压(V)", true, sStates[v]);
                    Thread.Sleep(500);

                    SendNoticeToUIAndTxtFile(string.Format("设置带载电压" + BMSVolts[v] + "V,带载电流" + "{0}A，等待负载稳定", current.ToString("F1")));
                    SetLoadPara(testWorkParam.lstIDs, BMSVolts[v] - 20, current, BMSVolts[v], current);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, current);
                    Thread.Sleep(3000);

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        var data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                        int count = 30;
                        while (count-- > 0)
                        {
                            if (data < current * 0.9 || data > current * 1.1)
                            {
                                data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                        }
                        d1.Add(item, data.ToString("F2"));
                    }
                    ProcessDataTmp(d1, sStates[v], "直流输出电流(A)", "-", "-");

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    Thread.Sleep(5000);//等待功率分析仪采集数据
                    double minData = BMSVolts[v] * (1 - Error / 100);
                    double maxData = BMSVolts[v] * (1 + Error / 100);
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                        int count = 100;
                        while (count-- > 0)
                        {
                            if (data < minData || data > maxData)
                            {
                                data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                            {
                                break;
                            }
                        }

                        dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    }
                    ProcessDataTmp(dic, sStates[v], "直流输出电压(V)", (BMSVolts[v] * (1 - Error / 100)).ToString("F2"), (BMSVolts[v] * (1 + Error / 100)).ToString("F2"));
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                }


                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage2, 200, true, OutputVoltage2);
                //Thread.Sleep(500);
                //WaitDCVoltage(testWorkParam.lstIDs, OutputVoltage2);
                //Thread.Sleep(2000);

                //SendNoticeToUIAndTxtFile(string.Format("设置带载电压" + OutputVoltage2 + "V,带载电流" + "{0}A，等待负载稳定", current.ToString("F1")));
                //SetLoadPara(testWorkParam.lstIDs, OutputVoltage2 - 20, current, OutputVoltage2, current);
                //Thread.Sleep(500);
                //SetLoadDCON(testWorkParam.lstIDs);
                //WaitDCCurrent(testWorkParam.lstIDs, current);
                //Thread.Sleep(3000);

                //d1 = new Dictionary<int, string>();
                //foreach (var item in testWorkParam.lstIDs)
                //{
                //    var data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                //    int count = 30;
                //    while (count-- > 0)
                //    {
                //        if (data < current * 0.9 || data > current * 1.1)
                //        {
                //            data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                //            Thread.Sleep(1000);
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }
                //    d1.Add(item, data.ToString("F2"));
                //}
                //ProcessDataTmp(d1, "输出电压设定误差", "充电电流(A)", "-", "-");


                //dic.Clear();
                //minData = OutputVoltage2 * (1 - Error / 100);
                //maxData = OutputVoltage2 * (1 + Error / 100);
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                //    int count = 100;
                //    while (count-- > 0)
                //    {
                //        if (data < minData || data > maxData)
                //        {
                //            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                //            Thread.Sleep(100);
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }

                //    dic.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                //}
                //ProcessDataTmp(dic, "输出电压设定误差", "输出" + OutputVoltage2 + "V", (OutputVoltage2 * (1 - Error / 100)).ToString("F2"), (OutputVoltage2 * (1 + Error / 100)).ToString("F2"));
                //SetLoadDCOFF(testWorkParam.lstIDs);
            }

        }
        public override void ProcessData()
        {

        }

    }
}
