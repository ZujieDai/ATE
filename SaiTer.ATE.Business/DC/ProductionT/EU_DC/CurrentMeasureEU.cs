using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电流测量误差欧标
    /// </summary>
    public class CurrentMeasureEU : BusinessBase
    {
        public CurrentMeasureEU(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        double DemandCurrent2 = 60;
        double ErrorRate = 2; //误差(%)

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

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, true, 390);
                    //Thread.Sleep(10 * 1000);
                 
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, LstChargerInfo[0].NominalVoltage);

                    Double DemandVoltage1 = MinAllowChargeVoltage;

                    Double DemandVoltage3 = MaxAllowChargeVoltage;

                    Double DemandVoltage2 = (DemandVoltage1 + DemandVoltage3) / 2;



                    Double DemandCurrent1 = MaxOutputPower * 1000 / DemandVoltage3;

                 
                    DemandCurrent1= CompareMaximum(DemandCurrent1, MaxAllowChargeCurrent);

                    


                    Double CheckCurrent1 = DemandCurrent1 * (1 - ErrorRate);

                    Double CheckCurrent2 = DemandCurrent1 * (1 + ErrorRate);

                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);

                    Double CheckVoltage1 = DemandVoltage1 * (1 - ErrorRate);

                    Double CheckVoltage2 = DemandVoltage1 * (1 + ErrorRate);


                    Double CheckVoltage3 = DemandVoltage2 * (1 - ErrorRate);

                    Double CheckVoltage4 = DemandVoltage2 * (1 + ErrorRate);



                    Double CheckVoltage5 = DemandVoltage3 * (1 - ErrorRate);

                    Double CheckVoltage6 = DemandVoltage3 * (1 + ErrorRate);




                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//




                    #region 最高电压-100%





                    if (IsRLoad(DemandVoltage3, DemandCurrent1))
                    {
                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent1 + 5, DemandVoltage3, DemandCurrent1);
                        SetLoadDCON(testWorkParam.lstIDs);


                        CountDownTimeInfo("等待带载稳定", 20, 0);

                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最大电压-100%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion


                    Double DemandCurrent2 = MaxOutputPower * 1000 / DemandVoltage3 / 2;

                        DemandCurrent2 = CompareMaximum(DemandCurrent2, MaxAllowChargeCurrent);

                        CheckCurrent1 = DemandCurrent2 * (1 - ErrorRate);
                        CheckCurrent2 = DemandCurrent2 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);
                    #region 最高电压-50%


                    if (IsRLoad(DemandVoltage3, DemandCurrent2))
                    {


                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, 5, DemandVoltage3, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent2, true, 390);

                        int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage3 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage3 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent2, DemandVoltage3 - 5, DemandCurrent2);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最大电压-50%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion


                    Double DemandCurrent3 = MaxOutputPower * 1000 / DemandVoltage3 / 10;

                    DemandCurrent3 =CompareMaximum(DemandCurrent3, MaxAllowChargeCurrent);


                    CheckCurrent1 = DemandCurrent3 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent3 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);


                    #region 最高电压-10%


                    if (IsRLoad(DemandVoltage3, DemandCurrent3))
                    {


                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, 5, DemandVoltage3, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent3, true, 390);

                        int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage3 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage3 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent3 + 5, DemandVoltage3 - 5, DemandCurrent3);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最大电压-10%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }

                    #endregion


                    Double DemandCurrent4 = MaxOutputPower * 1000 / DemandVoltage2;


                    DemandCurrent4 =CompareMaximum(DemandCurrent4, MaxAllowChargeCurrent);


                    CheckCurrent1 = DemandCurrent4 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent4 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);


                    #region 中间电压-100%


                    if (IsRLoad(DemandVoltage2, DemandCurrent4))
                    {


                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent4, true, 390);

                        int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage2 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage2 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent4 + 5, DemandVoltage2, DemandCurrent4);
                        //ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(lstIDs, DemandVoltage2 - 20, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        //ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(lstIDs);

                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "中间电压-100%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion


                    Double DemandCurrent5 = MaxOutputPower * 1000 / DemandVoltage2 / 2;

                    DemandCurrent5 = CompareMaximum(DemandCurrent5, MaxAllowChargeCurrent);

   

                    CheckCurrent1 = DemandCurrent5 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent5 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);
                    #region 中间电压-50%



                    if (IsRLoad(DemandVoltage2, DemandCurrent5))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent5, true, 390);

                       int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage2 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage2 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent5, DemandVoltage2 - 5, DemandCurrent5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "中间电压-50%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion

                    Double DemandCurrent6 = MaxOutputPower * 1000 / DemandVoltage2 / 10;

                    DemandCurrent6 =CompareMaximum(DemandCurrent6, MaxAllowChargeCurrent);


                    CheckCurrent1 = DemandCurrent6 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent6 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);


                    #region 中间电压-20%


                    if (IsRLoad(DemandVoltage2, DemandCurrent6))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent6, true, 390);

                        int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage2 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage2 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent6 + 5, DemandVoltage2 - 5, DemandCurrent6);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "中间电压-10%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion


                    Double DemandCurrent7 = MaxOutputPower * 1000 / DemandVoltage1;
             

                    DemandCurrent7 =CompareMaximum(DemandCurrent7, MaxAllowChargeCurrent);


                    CheckCurrent1 = DemandCurrent7 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent7 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);
                    #region 最小电压-100%

                    if (IsRLoad(DemandVoltage1, DemandCurrent7))
                    {

                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent7, true, 390);

                       int  timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage1 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage1 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent7 + 5, DemandVoltage1 - 5, DemandCurrent7);

                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最小电压-100%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion

                    Double DemandCurrent8 = MaxOutputPower * 1000 / DemandVoltage1/2;


                    DemandCurrent8 = CompareMaximum(DemandCurrent8, MaxAllowChargeCurrent);



                    CheckCurrent1 = DemandCurrent8 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent8 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);

                    #region 最小电压-50%

                    if (IsRLoad(DemandVoltage1, DemandCurrent8))
                    {

                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        //SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent + 5, DemandVoltage1, DemandCurrent + 5);

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent8, true, 390);

                        int timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage1 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage1 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent8 + 5, DemandVoltage1 - 5, DemandCurrent8);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最小电压-50%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion
                    Double DemandCurrent9 = MaxOutputPower * 1000 / DemandVoltage1 / 2;


                    DemandCurrent9 = CompareMaximum(DemandCurrent9, MaxAllowChargeCurrent);



                    CheckCurrent1 = DemandCurrent9 * (1 - ErrorRate);
                    CheckCurrent2 = DemandCurrent9 * (1 + ErrorRate);
                    CheckCurrent1 = RetainDecimals<double>(CheckCurrent1);
                    CheckCurrent2 = RetainDecimals<double>(CheckCurrent2);
                    #region 最小电压-20%

                    if (IsRLoad(DemandVoltage1, DemandCurrent9))
                    {

                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent9, true, 390);

                       int  timeout2 = 100;
                        while (timeout2-- > 0)
                        {
                            bool exist = AllEquipStateData.DicBMS_EU_DC_StateData.Any(c => c.Value.ChargingVoltage >= DemandVoltage1 * 0.9 && c.Value.ChargingVoltage <= DemandVoltage1 * 1.02);
                            timeout2--;
                            if (exist)
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (timeout2 < 0)
                            {
                                break;
                            }
                            SendNoticeToUIAndTxtFile("等待电压稳定倒计时:" + timeout2 + "s");
                        }

                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent9 + 5, DemandVoltage1 - 5, DemandCurrent9);
                        SetLoadDCON(testWorkParam.lstIDs);
                        CountDownTimeInfo("等待带载稳定", 20, 0);
                        Dictionary<int, string> dicC = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }



                        ProcessDataTmp(dicC, "最小电压-10%负载", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                    }
                    #endregion
                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);




                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }


        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 1)
            {

                ErrorRate = Convert.ToDouble(strParams[0].Split('=')[1])/100;
            }

        }

        public override void ProcessData()
        {

        }

        public double CompareMaximum(double value, double maxvalue)
        {
            try
            {
                value = value >= maxvalue ? maxvalue : value;
                return value;
            }
            catch
            {
                return 0;
            }
        }
        public static T RetainDecimals<T>(T value)
        {
            try
            {
           
                    decimal value2 = Convert.ToDecimal(value);
                    string value3 = value2.ToString("f3");
                    value = (T)Convert.ChangeType(value3, typeof(T));
                


                return value;
            }
            catch
            {

            }
            return default(T);
        }
    }
}
