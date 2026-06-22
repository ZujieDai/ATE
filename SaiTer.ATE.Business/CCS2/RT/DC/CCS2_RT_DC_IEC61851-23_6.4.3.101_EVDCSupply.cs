using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    internal class CCS2_RT_DC_EVDCSupply : BusinessBase
    {
        /// <summary>
        /// 欧标研测直流：电动汽车直流电源
        /// </summary>
        /// <param name="type"></param>
        public CCS2_RT_DC_EVDCSupply(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private int trlTimeOut_S = 5;
        private double ErrorVoltageRate = 0.05, ErrorCurrentRate = 0.05;
        double BMSVoltage = 400, BMSCurrent = 50, BMSVoltage2 = 500, BMSCurrent2 = 100, BMSVoltageOver = 1000, BMSCurrentOver = 240;
        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSVoltage = LstChargerInfo[0].NominalVoltage;
            BMSCurrent = LstChargerInfo[0].NominalCurrent;

            //BMS需求电压1(V)=400|BMS需求电流1(A)=50|BMS需求电压2(V)=500|BMS需求电流2(A)=100|电压误差(%)=5|电流误差(%)=5|BMS需求电压超过最大功率(V)=1000|BMS需求电流超过最大功率(A)=240
            if (strParams.Length >= 8)
            {
                BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                BMSVoltageOver = Convert.ToDouble(strParams[6].Split('=')[1]);
                BMSCurrentOver = Convert.ToDouble(strParams[7].Split('=')[1]);
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

        private void StartItemFlow()
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
                    SetConditionValues();

                    #region 充电设置1
                    double DemandCurrent = ControlEquipMent.ResistanceLoad != null ? BMSCurrent : BMSCurrent + 10;
                    if(DemandCurrent > MaxAllowChargeCurrent)
                        DemandCurrent = MaxAllowChargeCurrent;

                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSVoltage, DemandCurrent))
                    {
                        return;
                    }
                    //Thread.Sleep(10 * 1000);
                    Thread.Sleep(2000);//等导引电压稳定

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    if (BMSCurrent >= MaxAllowChargeCurrent)
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltage - 20, BMSCurrent - 5, BMSVoltage - 5, BMSCurrent);
                    else
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltage - 20, BMSCurrent, BMSVoltage - 5, BMSCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, BMSCurrent);
                    Thread.Sleep(3000);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("判断结果中");
                    Dictionary<int, string> dicV = new Dictionary<int, string>();
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int time = 60;
                        while (time-- > 0)
                        {
                            if (DCVoltage >= BMSVoltage * (1 - ErrorVoltageRate) && DCVoltage <= BMSVoltage * (1 + ErrorVoltageRate))
                                break;
                            Thread.Sleep(200);
                            DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        }
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int time = 60;
                        while (time-- > 0)
                        {
                            if (DCCurrent >= BMSCurrent * (1 - ErrorCurrentRate) && DCCurrent <= BMSCurrent * (1 + ErrorCurrentRate))
                                break;
                            Thread.Sleep(200);
                            DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicV, $"直流充电，需求电压{BMSVoltage}V", "充电电压(V)", (BMSVoltage * (1 - ErrorVoltageRate)).ToString(), (BMSVoltage * (1 + ErrorVoltageRate)).ToString());
                    ProcessDataTmp(dicC, $"直流充电，需求电流{BMSCurrent}A", "充电电流(A)", (BMSCurrent * (1 - ErrorCurrentRate)).ToString(), (BMSCurrent * (1 + ErrorCurrentRate)).ToString());

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    #endregion

                    #region 充电设置2
                    //if (!CheckSwipingCard(testWorkParam.lstIDs, BMSVoltage2, BMSCurrent2 + 10))
                    //{
                    //    return;
                    //}
                    DemandCurrent = ControlEquipMent.ResistanceLoad != null ? BMSCurrent2 : BMSCurrent2 + 10;
                    if (DemandCurrent > MaxAllowChargeCurrent)
                        DemandCurrent = MaxAllowChargeCurrent;
                    ControlEquipMent.BMS.SetParameter(lstIDs, BMSVoltage2, DemandCurrent, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSVoltage2);
                    //Thread.Sleep(10 * 1000);
                    Thread.Sleep(2000);//等导引电压稳定

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    if (BMSCurrent2 >= MaxAllowChargeCurrent)
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltage2 - 20, BMSCurrent2 - 5, BMSVoltage2 - 5, BMSCurrent2);
                    else
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltage2 - 20, BMSCurrent2, BMSVoltage2 - 5, BMSCurrent2);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, BMSCurrent2);
                    Thread.Sleep(3000);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("判断结果中");
                    dicV = new Dictionary<int, string>();
                    dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int time = 60;
                        while (time-- > 0)
                        {
                            if (DCVoltage >= BMSVoltage2 * (1 - ErrorVoltageRate) && DCVoltage <= BMSVoltage2 * (1 + ErrorVoltageRate))
                                break;
                            Thread.Sleep(200);
                            DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        }
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int time = 60;
                        while (time-- > 0)
                        {
                            if (DCCurrent >= BMSCurrent2 * (1 - ErrorCurrentRate) && DCCurrent <= BMSCurrent2 * (1 + ErrorCurrentRate))
                                break;
                            Thread.Sleep(200);
                            DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicV, $"直流充电，需求电压{BMSVoltage2}V", "充电电压(V)", (BMSVoltage2 * (1 - ErrorVoltageRate)).ToString(), (BMSVoltage2 * (1 + ErrorVoltageRate)).ToString());
                    ProcessDataTmp(dicC, $"直流充电，需求电流{BMSCurrent2}A", "充电电流(A)", (BMSCurrent2 * (1 - ErrorCurrentRate)).ToString(), (BMSCurrent2 * (1 + ErrorCurrentRate)).ToString());

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    #endregion

                    #region 超过最大功率
                    DemandCurrent = ControlEquipMent.ResistanceLoad != null ? BMSCurrentOver : BMSCurrentOver + 10;
                    if (DemandCurrent > MaxAllowChargeCurrent)
                        DemandCurrent = MaxAllowChargeCurrent;
                    SendNoticeToUIAndTxtFile($"设置导引需求，电压{BMSVoltageOver}V，电流{DemandCurrent}A");
                    ControlEquipMent.BMS.SetParameter(lstIDs, BMSVoltageOver, DemandCurrent, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSVoltageOver);
                    //Thread.Sleep(10 * 1000);
                    Thread.Sleep(2000);//等导引电压稳定

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    if (BMSCurrentOver >= MaxAllowChargeCurrent)
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltageOver - 20, BMSCurrentOver - 5, BMSVoltageOver - 5, BMSCurrentOver);
                    else
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltageOver - 20, BMSCurrentOver, BMSVoltageOver - 5, BMSCurrentOver);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, BMSCurrentOver);
                    Thread.Sleep(3 * 1000);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("判断结果中");
                    var dicP = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double Power = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                        int time = 60;
                        while(time-- > 0)
                        {
                            if (Power >= MaxOutputPower * (1 - (ErrorVoltageRate + ErrorCurrentRate) / 2) && Power <= MaxOutputPower * (1 + (ErrorVoltageRate + ErrorCurrentRate) / 2))
                                break;
                            Thread.Sleep(200);
                            Power = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                        }
                        dicP.Add(item, Power.ToString("F2"));
                    }
                    ProcessDataTmp(dicP, $"直流充电，需求超过最大允许功率", "输出功率(kW)", 
                        (MaxOutputPower * (1 - (ErrorVoltageRate + ErrorCurrentRate) / 2)).ToString(), (MaxOutputPower * (1 + (ErrorVoltageRate + ErrorCurrentRate) / 2)).ToString());

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    #endregion


                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭BMS中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
