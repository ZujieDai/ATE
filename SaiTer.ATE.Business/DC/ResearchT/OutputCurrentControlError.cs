using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:输出电流控制误差测试
    /// </summary>

    public class OutputCurrentControlError : BusinessBase
    {

        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 50;
        /// <summary>
        /// 需求电流2(A)
        /// </summary>
        Double DemandCurrent2 = 15;
        /// <summary>
        /// 判定准则1(%)
        /// </summary>
        Double ErrorPercentage = 1;




        /// <summary>
        /// 判定准则3(A)
        /// </summary>
        Double ErrorValue = 1.5;

        public OutputCurrentControlError(int type)
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


            if (strParams.Length >= 6)
            {

                ErrorPercentage = double.Parse(strParams[3].Split('=')[1]);

                ErrorValue = double.Parse(strParams[5].Split('=')[1]);
            }
            else if (strParams.Length == 2)
            {
                ErrorPercentage = double.Parse(strParams[0].Split('=')[1]);

                ErrorValue = double.Parse(strParams[1].Split('=')[1]);
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

                    // >30和<30是不同的判断逻辑
                    DemandVoltage = MidAllowChargeVoltage;
                    DemandCurrent = RatedCurrent * 0.2;
                    DemandCurrent = DemandCurrent < 30 ? 40 : DemandCurrent;
                    DemandCurrent = DemandCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;
                    DemandCurrent2 = DemandCurrent2 > 30 ? 30 : DemandCurrent2;


                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile($"设置BMS需求电压{DemandVoltage}V，需求电流{DemandCurrent}A，请稍候...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage + 20, LstChargerInfo[0].ChargerType, 0);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);
                    Thread.Sleep(5 * 1000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SendNoticeToUIAndTxtFile($"设置负载需求电压{DemandVoltage - 10}V，需求电流{DemandCurrent + 20}A，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 20, DemandVoltage - 10, DemandCurrent + 5);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 20);

                    Dictionary<int, string> ReldicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        int count = 10;
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;

                        while (count-- > 0)
                        {

                            if (DCCurrent < DemandCurrent * 0.95 || DCCurrent > DemandCurrent * 1.05)
                            {
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                            DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }
                        ReldicC.Add(item, DCCurrent.ToString("F2"));
                    }

                    Dictionary<int, string> Reldic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        Reldic.Add(item, DCVoltage.ToString("F2"));
                    }

                    string info = $"需求电压{DemandVoltage}V，电流{DemandCurrent}A";
                    ProcessDataTmp(Reldic, info, "直流输出电压(V)", "-", "-");
                    ProcessDataTmp(ReldicC, info, "直流输出电流(A)", "-", "-");


                    Dictionary<int, string> dErrorRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double Current_4 = Convert.ToDouble(ReldicC[item]);
                        //double ErrorRate = System.Math.Abs((Current_4 - DemandCurrent) / DemandCurrent);
                        //ErrorRate = ErrorRate * 100;
                        double ErrorRate = Current_4 - DemandCurrent;
                        dErrorRate.Add(item, ErrorRate.ToString("F2"));
                    }
                    double errorRange = DemandCurrent * ErrorPercentage / 100;
                    //ProcessDataTmp(dErrorRate, info, "电流误差(%)", "0", ErrorPercentage.ToString());
                    ProcessDataTmp(dErrorRate, info, "电流误差(A)", (-errorRange).ToString("F2"), errorRange.ToString("F2"));




                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    Thread.Sleep(4000);

                    SendNoticeToUIAndTxtFile($"设置需求电压{DemandVoltage}V，需求电流{DemandCurrent2}A，请稍候...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent2, false, DemandVoltage);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);
                    Thread.Sleep(5 * 1000);
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SendNoticeToUIAndTxtFile($"设置负载需求电压{DemandVoltage - 10}V，需求电流{DemandCurrent2 + 20}A，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent2 + 20, DemandVoltage - 10, DemandCurrent2 + 5);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent2, 20);

                    Dictionary<int, string> ReldicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        int count = 10;
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;

                        while (count-- > 0)
                        {

                            if (DCCurrent < DemandCurrent2 * 0.95 || DCCurrent > DemandCurrent2 * 1.05)
                            {
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                            DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }

                        ReldicC2.Add(item, DCCurrent.ToString("F2"));
                    }

                    Dictionary<int, string> Reldic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        Reldic2.Add(item, DCVoltage.ToString("F2"));
                    }


                    info = $"需求电压{DemandVoltage}V，电流{DemandCurrent2}A";
                    ProcessDataTmp(Reldic2, info, "充电电压(V)", "-", "-");
                    ProcessDataTmp(ReldicC2, info, "充电电流(A)", "-", "-");

                    Dictionary<int, string> dErrorRate2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double Current_4 = Convert.ToDouble(ReldicC2[item]);
                        double ErrorRate = Current_4 - DemandCurrent2;
                        dErrorRate2.Add(item, ErrorRate.ToString("F2"));
                    }
                    ProcessDataTmp(dErrorRate2, info, "电流误差(A)", (-ErrorValue).ToString(), ErrorValue.ToString());

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
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
