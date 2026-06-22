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
    /// 直流电流调节精度测试
    /// </summary>
    public class CCS2_PT_DC_CurrentRegulationDC : BusinessBase
    {
        public CCS2_PT_DC_CurrentRegulationDC(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        double DemandCurrent2 = 60;
        double ErrorCurrent = 2.5;//误差(A)
        double ErrorRate = 5; //误差(%)

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
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, true, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, DemandCurrent, true, 390);

                    //Thread.Sleep(10 * 1000);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);

                    Double DemandVoltage1 = MinAllowChargeVoltage;

                    Double DemandVoltage3 = MaxAllowChargeVoltage;

                    Double DemandVoltage2 = (DemandVoltage1 + DemandVoltage3) / 2;

                    Double CheckCurrent1 = DemandCurrent * (1 - ErrorRate);

                    Double CheckCurrent2 = DemandCurrent * (1 + ErrorRate);


                    Double CheckCurrent3 = DemandCurrent2 * (1 - ErrorRate);

                    Double CheckCurrent4 = DemandCurrent2 * (1 + ErrorRate);


                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//


                    #region DemanCurrent最高电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage3);
                    if (IsRLoad(DemandVoltage3, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent + 10, DemandVoltage3 - 5, DemandCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);


                        Dictionary<int, string> dicC = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent > 50)
                        {

                            ProcessDataTmp(dicC, "最大电压", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC, "最大电压", "充电电流(A)", (DemandCurrent - ErrorCurrent).ToString(), (DemandCurrent + ErrorCurrent).ToString());
                        }
                    }
                    #endregion


                    #region DemanCurrent中间电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage2);
                    if (IsRLoad(DemandVoltage2, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent, true, 390);


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

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent + 10, DemandVoltage2 - 5, DemandCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);




                        Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC2.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent > 50)
                        {

                            ProcessDataTmp(dicC2, "中间电压", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC2, "中间电压", "充电电流(A)", (DemandCurrent - ErrorCurrent).ToString(), (DemandCurrent + ErrorCurrent).ToString());
                        }
                    }




                    #endregion




                    #region DemanCurrent最小电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage1);
                    if (IsRLoad(DemandVoltage1, DemandCurrent))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");


                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent, true, 390);



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

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent + 10, DemandVoltage1 - 5, DemandCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);



                        Dictionary<int, string> dicC3 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC3.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent > 50)
                        {

                            ProcessDataTmp(dicC3, "最小电压", "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC3, "最小电压", "充电电流(A)", (DemandCurrent - ErrorCurrent).ToString(), (DemandCurrent + ErrorCurrent).ToString());
                        }
                    }
                    #endregion


                    #region DemanCurrent2最高电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage3, DemandCurrent2, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage3);
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

                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage3 - 20, DemandCurrent2 + 10, DemandVoltage3 - 5, DemandCurrent2);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);



                        Dictionary<int, string> dicC4 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC4.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent2 > 50)
                        {

                            ProcessDataTmp(dicC4, "最高电压", "充电电流(A)", CheckCurrent3.ToString(), CheckCurrent4.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC4, "最高电压", "充电电流(A)", (DemandCurrent2 - ErrorCurrent).ToString(), (DemandCurrent2 + ErrorCurrent).ToString());
                        }
                    }
                    #endregion


                    #region DemanCurrent2中间电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent2, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage2);
                    if (IsRLoad(DemandVoltage2, DemandCurrent2))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, 5, DemandVoltage2, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent2, true, 390);


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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 20, DemandCurrent2 + 10, DemandVoltage2 - 5, DemandCurrent2);

                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);



                        Dictionary<int, string> dicC5 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC5.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent2 > 50)
                        {

                            ProcessDataTmp(dicC5, "中间电压", "充电电流(A)", CheckCurrent3.ToString(), CheckCurrent4.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC5, "中间电压", "充电电流(A)", (DemandCurrent2 - ErrorCurrent).ToString(), (DemandCurrent2 + ErrorCurrent).ToString());
                        }
                    }
                    #endregion

                    #region DemanCurrent2最小电压
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent2, true, 390);
                    WaitDCVoltage_EU_DC(testWorkParam.lstIDs, DemandVoltage1);
                    if (IsRLoad(DemandVoltage1, DemandCurrent2))
                    {
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);



                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");

                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent2, true, 390);


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
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 20, DemandCurrent2 + 10, DemandVoltage1 - 5, DemandCurrent2);
                        SetLoadDCON(testWorkParam.lstIDs);

                        CountDownTimeInfo("等待带载稳定", 20, 0);



                        Dictionary<int, string> dicC6 = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            dicC6.Add(item, DCCurrent.ToString("F2"));
                        }

                        if (DemandCurrent2 > 50)
                        {

                            ProcessDataTmp(dicC6, "最小电压", "充电电流(A)", CheckCurrent3.ToString(), CheckCurrent4.ToString());
                        }
                        else
                        {
                            ProcessDataTmp(dicC6, "最小电压", "充电电流(A)", (DemandCurrent2 - ErrorCurrent).ToString(), (DemandCurrent2 + ErrorCurrent).ToString());
                        }
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
            if (strParams.Length >= 4)
            {
                DemandCurrent = Convert.ToDouble(strParams[0].Split('=')[1]);
                DemandCurrent2 = Convert.ToDouble(strParams[1].Split('=')[1]);
                ErrorCurrent = Convert.ToDouble(strParams[2].Split('=')[1]);
                ErrorRate = Convert.ToDouble(strParams[3].Split('=')[1]) / 100;
                DemandCurrent = CompareMaximum(DemandCurrent, MaxAllowChargeCurrent);
                DemandCurrent2 = CompareMaximum(DemandCurrent2, MaxAllowChargeCurrent);
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

    }
}
