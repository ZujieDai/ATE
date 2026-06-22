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
    /// <summary>
    /// 直流电压调节精度测试
    /// </summary>
    public class CCS2_PT_DC_VoltageRegulationDC : BusinessBase
    {
        public CCS2_PT_DC_VoltageRegulationDC(int type)
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

                    //ControlEquipMent.BMS.BMS_ON(lstIDs);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, LstChargerInfo[0].NominalVoltage);

                    Double DemandVoltage1 = MinAllowChargeVoltage;

                    Double DemandVoltage3 = MaxAllowChargeVoltage;

                    Double DemandVoltage2 = (DemandVoltage1 + DemandVoltage3) / 2;



                    Double CheckVoltage1 = DemandVoltage1 * (1 - ErrorRate);

                    Double CheckVoltage2 = DemandVoltage1 * (1 + ErrorRate);


                    Double CheckVoltage3 = DemandVoltage2 * (1 - ErrorRate);

                    Double CheckVoltage4 = DemandVoltage2 * (1 + ErrorRate);



                    Double CheckVoltage5 = DemandVoltage3 * (1 - ErrorRate);

                    Double CheckVoltage6 = DemandVoltage3 * (1 + ErrorRate);




                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//


                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage3;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);






                    #region 最高电压-100%


                    if (IsRLoad(DemandVoltage3, DemandCurrent))
                    {


                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent - 5, DemandVoltage3, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);

                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);

                        Dictionary<int, string> dicV = new Dictionary<int, string>();

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最大电压", "充电电压(V)", CheckVoltage5.ToString(), CheckVoltage6.ToString());
                    }
                    #endregion




                    #region 最高电压-50%
                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage3 / 2;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage3, DemandCurrent))
                    {


                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, 5, DemandVoltage3, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent - 5, DemandVoltage3, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);

                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最大电压-50%", "充电电压(V)", CheckVoltage5.ToString(), CheckVoltage6.ToString());
                    }
                    #endregion


                    #region 最高电压-20%
                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage3 / 5;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage3, DemandCurrent))
                    {





                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, 5, DemandVoltage3, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent - 5, DemandVoltage3, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最大电压-20%", "充电电压(V)", CheckVoltage5.ToString(), CheckVoltage6.ToString());
                    }
                    #endregion





                    #region 中间电压-100%

                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage2;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage2, DemandCurrent))
                    {


                        if (ControlEquipMent.FeedbackLoad == null)
                        {
                            Double MaxCurrent1 = DemandVoltage2 * 0.12;
                            DemandCurrent = CompareThisMaximum(DemandCurrent, MaxCurrent1);
                        }
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent - 5, DemandVoltage2, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "中间电压-100%", "充电电压(V)", CheckVoltage3.ToString(), CheckVoltage4.ToString());
                    }
                    #endregion




                    #region 中间电压-50%


                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage2 / 2;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage2, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent - 5, DemandVoltage2, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "中间电压-50%", "充电电压(V)", CheckVoltage3.ToString(), CheckVoltage4.ToString());
                    }
                    #endregion


                    #region 中间电压-20%

                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage2 / 5;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage3, DemandCurrent))
                    {

                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent + 5, true, 390);


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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent - 5, DemandVoltage2, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "中间电压-20%", "充电电压(V)", CheckVoltage3.ToString(), CheckVoltage4.ToString());
                    }
                    #endregion



                    #region 最小电压-100%


                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage1;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage1, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent + 5, true, 390);



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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent - 5, DemandVoltage1, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最小电压-100%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());
                    }
                    #endregion



                    #region 最小电压-50%


                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage1 / 2;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage1, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent - 5, DemandVoltage1, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最小电压-50%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());
                    }
                    #endregion


                    #region 最小电压-20%


                    DemandCurrent = MaxOutputPower * 1000 / DemandVoltage1 / 5;
                    DemandCurrent = CompareThisMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (IsRLoad(DemandVoltage1, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent + 5, true, 390);

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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent - 5, DemandVoltage1, DemandCurrent - 5);
                        SetLoadDCON(testWorkParam.lstIDs);
                        //CountDownTimeInfo("等待带载稳定", 20, 0);
                        WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                        Dictionary<int, string> dicV = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                            dicV.Add(item, DCVoltage.ToString("F2"));
                        }



                        ProcessDataTmp(dicV, "最小电压-20%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());
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

                ErrorRate = Convert.ToDouble(strParams[0].Split('=')[1]) / 100;
            }

        }

        public override void ProcessData()
        {

        }

        public double CompareThisMaximum(double value, double maxvalue)
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
    }
}
