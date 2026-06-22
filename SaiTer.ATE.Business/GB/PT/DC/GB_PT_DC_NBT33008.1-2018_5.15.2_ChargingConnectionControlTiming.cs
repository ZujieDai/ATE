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
    /// 国标产测直流：充电连接控制时序试验
    /// </summary>
    public class GB_PT_DC_ChargingConnectionControlTiming : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt1 = 0;
        double ResiLoadCurrent1 = 0;
        double BMSDemandVolt2 = 0;
        double ResiLoadCurrent2 = 0;
        public GB_PT_DC_ChargingConnectionControlTiming(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            //恒流模式电压(V)=420|恒流模式电流(A)=18|恒压模式电压(V)=450|恒压模式电流(A)=15
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            ResiLoadCurrent1 = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt2 = Convert.ToDouble(strParams[2].Split('=')[1]);
            ResiLoadCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
        }
        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();



                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    SendNoticeToUIAndTxtFile("设置充电机恒流模式");
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt1, ResiLoadCurrent1, false, BMSDemandVolt1);
                        if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt1, ResiLoadCurrent1))
                        {
                            return;
                        }
                    }
                    else
                    {
                        //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt1 - 20, ResiLoadCurrent1, false, BMSDemandVolt1);
                        if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt1 - 20, ResiLoadCurrent1))
                        {
                            return;
                        }
                    }
                    Thread.Sleep(500);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");

                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt1 - 20, ResiLoadCurrent1 + 10, BMSDemandVolt1 - 20, ResiLoadCurrent1 - 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, ResiLoadCurrent1, 20);
                    Thread.Sleep(2000);
                    double cur = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                    if (cur < 1)
                    {
                        SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");
                        SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt1 - 20, ResiLoadCurrent1 + 10, BMSDemandVolt1 - 20, ResiLoadCurrent1 - 5);
                        Thread.Sleep(500);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, ResiLoadCurrent1, 20);
                        Thread.Sleep(2000);
                    }


                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double data = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, data.ToString("F2"));
                    }
                    //ProcessDataTmp(dic, "恒流模式", "充电电压", (BMSDemandVolt1 - 25).ToString("F2"), (BMSDemandVolt1 - 15).ToString("F2"));
                    ProcessDataTmp(dic, "恒流模式", "充电电压", "-", "-");
                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double data = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        double data = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dic.Add(item, data.ToString("F2"));
                    }
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        double error = ResiLoadCurrent1 < 30 ? 0.3 : ResiLoadCurrent1 * 1 / 100;
                        ProcessDataTmp(dic, "恒流模式", "充电电流", (ResiLoadCurrent1 - error).ToString(), (ResiLoadCurrent1 + error).ToString());
                    }
                    else
                    {
                        double error = ResiLoadCurrent1 - 5 < 30 ? 0.3 : (ResiLoadCurrent1 - 5) * 1 / 100;
                        ProcessDataTmp(dic, "恒流模式", "充电电流", (ResiLoadCurrent1 - 5 - error).ToString(), (ResiLoadCurrent1 - 5 + error).ToString());
                    }

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double data = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(item, data.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "恒流模式", "辅源电压", "11.2", "12.8");





                    SendNoticeToUIAndTxtFile("关闭负载");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //Thread.Sleep(2000);
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    SendNoticeToUIAndTxtFile("设置充电机为恒压模式");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt2, ResiLoadCurrent2, true, BMSDemandVolt2);
                    Thread.Sleep(5000);
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt2 - 20, ResiLoadCurrent2 - 5, BMSDemandVolt2, ResiLoadCurrent2 - 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, ResiLoadCurrent1 - 5, 20);
                    Thread.Sleep(1000);
                    cur = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                    if (cur < 1)
                    {
                        SendNoticeToUIAndTxtFile("负载未启动成功， 重新启动负载");
                        SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt2 - 20, ResiLoadCurrent2 - 5, BMSDemandVolt2, ResiLoadCurrent2 - 5);
                        Thread.Sleep(500);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, ResiLoadCurrent1 - 5, 20);
                        Thread.Sleep(1000);//等待回馈负载电流稳定
                    }
                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double data = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        double data = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic.Add(item, data.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "恒压模式", "充电电压", (BMSDemandVolt2 - 5).ToString("F2"), (BMSDemandVolt2 + 5).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double data = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, data.ToString("F2"));
                    }
                    //ProcessDataTmp(dic, "恒压模式", "充电电流", (ResiLoadCurrent2 - 7).ToString(), (ResiLoadCurrent2).ToString());
                    ProcessDataTmp(dic, "恒压模式", "充电电流", "-", "-");

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double data = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(item, data.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "恒压模式", "辅源电压", "11.2", "12.8");

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 100, true, 390);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
