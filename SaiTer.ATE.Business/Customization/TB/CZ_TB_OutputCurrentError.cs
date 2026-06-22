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
    /// 输出电流误差
    /// </summary>
    public class CZ_TB_OutputCurrentError : BusinessBase
    {
        int trlTimeOut_S = 30;
        double OutputVoltage1 = 0;
        double OutputVoltage2 = 0;

        double ResiLoadCurrent1 = 0;
        double ResiLoadCurrent2 = 0;
        public CZ_TB_OutputCurrentError(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //输出电压1(V)=500|输出电压2(V)=750|输出电流1(A)=50|输出电流2(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputVoltage1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            OutputVoltage2 = Convert.ToDouble(strParams[1].Split('=')[1]);
            ResiLoadCurrent1 = Convert.ToDouble(strParams[2].Split('=')[1]);
            ResiLoadCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
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
                SetCPRersh_EUDC();
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

                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

                int BMSInfo = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[0]].SystemState);
                if (BMSInfo < 20 || BMSInfo > 23)
                {
                    if (!CheckSwipingCard(testWorkParam.lstIDs, OutputVoltage1 + 10, ResiLoadCurrent1))
                    {
                        return;
                    }
                }
                else
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage1 + 10, ResiLoadCurrent1, true, OutputVoltage1);

                //Thread.Sleep(500);
                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, OutputVoltage1 + 10);
                Thread.Sleep(3000);

                SendNoticeToUIAndTxtFile(string.Format("设置带载电压" + OutputVoltage1 + "V,带载电流" + "{0}A，等待负载稳定", ResiLoadCurrent1.ToString("F1")));
                SetLoadPara(testWorkParam.lstIDs, OutputVoltage1, ResiLoadCurrent1 + 5, OutputVoltage1, ResiLoadCurrent1 + 5);
                Thread.Sleep(500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, ResiLoadCurrent1);
                Thread.Sleep(3000);

                Dictionary<int, string> dic = new Dictionary<int, string>();
                Dictionary<int, string> dicC = new Dictionary<int, string>();
                Thread.Sleep(5000);//等待功率分析仪采集数据
                double minData;
                double maxData;
                if (ResiLoadCurrent1 < 5)
                {
                    minData = ResiLoadCurrent1 - 0.15;
                    maxData = ResiLoadCurrent1 + 0.15;
                }
                else
                {
                    minData = ResiLoadCurrent1 - 0.3;
                    maxData = ResiLoadCurrent1 + 0.3;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    int count = 10;
                    while (count-- > 0)
                    {
                        if (data < minData || data > maxData)
                        {
                            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }
                    }

                    dicC.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    dic.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt.ToString("F2"));
                }
                ProcessDataTmp(dic, "输出电流设定误差", "需求电压" + OutputVoltage1 + "V", "-", "-");
                ProcessDataTmp(dicC, "输出电流设定误差", "需求电流" + ResiLoadCurrent1 + "A", minData.ToString("F2"), maxData.ToString("F2"));


                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage2 + 10, ResiLoadCurrent2, true, OutputVoltage2);
                Thread.Sleep(500);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage2, 200, true, OutputVoltage2);
                //Thread.Sleep(500);
                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, OutputVoltage2 + 10);
                Thread.Sleep(3000);
                SendNoticeToUIAndTxtFile(string.Format("设置带载电压" + OutputVoltage2 + "V,带载电流" + "{0}A，等待负载稳定", (ResiLoadCurrent2 - 5).ToString("F1")));


                SetLoadPara(testWorkParam.lstIDs, OutputVoltage2, ResiLoadCurrent2 + 5, OutputVoltage2, ResiLoadCurrent2 + 5);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, ResiLoadCurrent2);
                Thread.Sleep(3000);

                dic.Clear();
                dicC.Clear();
                if (ResiLoadCurrent2 < 5)
                {
                    minData = ResiLoadCurrent2 - 0.15;
                    maxData = ResiLoadCurrent2 + 0.15;
                }
                else
                {
                    minData = ResiLoadCurrent2 - 0.3;
                    maxData = ResiLoadCurrent2 + 0.3;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                    int count = 30;
                    while (count-- > 0)
                    {
                        if (data < minData || data > maxData)
                        {
                            data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent;
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }
                    }

                    dicC.Add(testWorkParam.lstIDs[i], data.ToString("F2"));
                    dic.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt.ToString("F2"));
                }
                ProcessDataTmp(dic, "输出电流设定误差", "需求电压" + OutputVoltage2 + "V", "-", "-");
                ProcessDataTmp(dicC, "输出电流设定误差", "需求电流" + ResiLoadCurrent2 + "A", minData.ToString("F2"), maxData.ToString("F2"));
                SetLoadDCOFF(testWorkParam.lstIDs);
                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
            }

        }
        public override void ProcessData()
        {

        }

    }
}
