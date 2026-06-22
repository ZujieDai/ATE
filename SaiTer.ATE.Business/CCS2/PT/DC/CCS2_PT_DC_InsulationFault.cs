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
    /// 绝缘检测
    /// </summary>

    public class CCS2_PT_DC_InsulationFault : BusinessBase
    {
        public CCS2_PT_DC_InsulationFault(int type)
        {
            TrialType = type;
        }
        private int TestTime = 80;
        private double BMSVoltage = 500;//电压
        private double BMSCurrent = 20;//电流
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 10;
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
                //恢复最高允许充电电压
                var dicSet1 = ControlEquipMent.BMS.BMSGetParameter_EU_DC(testWorkParam.lstIDs, 0x97);
                foreach (int CID in testWorkParam.lstIDs)
                {
                    var data = dicSet1[CID];
                    if (data.Count >= 15)
                    {
                        List<int> para = new List<int>();
                        for (int i = 0; i < 12; i++)
                            para.Add(Convert.ToInt32(data[i]));
                        ControlEquipMent.BMS.BMSSetPara1_EU_DC(testWorkParam.lstIDs, para.ToList(), data[12], 250, 1000);
                    }
                }
                SetCPRersh_EUDCALL();
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

                    #region DC+DC-对地300kΩ
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    var dicSet1 = ControlEquipMent.BMS.BMSGetParameter_EU_DC(testWorkParam.lstIDs, 0x97);
                    foreach (int CID in testWorkParam.lstIDs)
                    {
                        var data = dicSet1[CID];
                        if (data.Count >= 15)
                        {
                            List<int> para = new List<int>();
                            for (int i = 0; i < 12; i++)
                                para.Add(Convert.ToInt32(data[i]));
                            ControlEquipMent.BMS.BMSSetPara1_EU_DC(testWorkParam.lstIDs, para.ToList(), data[12], 250, 500);
                        }
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 500, 250, true, LstChargerInfo[0].NominalVoltage);
                    //Thread.Sleep(10*1000);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("正在发送DC+DC-对地300kΩ指令");


                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BMSMeasureVoltage, Ks.ToArray(), 7, 7, "0");




                    //检测能否刷卡
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);



                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "DC+DC-对地300kΩ", "充电电压(V)", MinAllowChargeVoltage.ToString(), (MaxAllowChargeVoltage + 5).ToString());



                    SendNoticeToUIAndTxtFile("关闭导引中...");


                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("恢复互操设置中...");


                    Thread.Sleep(25000);
                    SetCPRersh_EUDCALL();
                    #endregion


                    #region DC+DC-对地30kΩ
                    SendNoticeToUIAndTxtFile("设备正在开启导引中，请稍候...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    Thread.Sleep(2000);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("正在发送DC+DC-对地30kΩ指令");


                    Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BMSMeasureVoltage, Ks.ToArray(), 3, 3, "0");




                    //检测能否刷卡
                    WaitSwipingCard(testWorkParam.lstIDs, 3);

                    //CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);
                    int time = TestTime;
                    while (time-- > 0)
                    {
                        SendNoticeToUIAndTxtFile("判断充电机是否能充电倒计时:" + time);
                        Thread.Sleep(1000);
                    }

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "DC+DC-对地30kΩ", "充电电压(V)", "0", "20");



                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("恢复互操设置中...");


                    Thread.Sleep(25000);
                    SetCPRersh_EUDCALL();
                    #endregion



                    #region DC-对地300kΩ
                    SendNoticeToUIAndTxtFile("设备正在开启导引中，请稍候...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    Thread.Sleep(2000);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("正在发送DC-对地300kΩ指令");


                    Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BMSMeasureVoltage, Ks.ToArray(), 0, 7, "0");




                    //检测能否刷卡
                    //WaitSwipingCard(testWorkParam.lstIDs, 0);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);



                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "DC-对地300kΩ", "充电电压(V)", MinAllowChargeVoltage.ToString(), (MaxAllowChargeVoltage + 5).ToString());



                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("恢复互操设置中...");



                    SetCPRersh_EUDC();
                    #endregion


                    #region DC-对地30kΩ
                    SendNoticeToUIAndTxtFile("设备正在开启导引中，请稍候...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    Thread.Sleep(2000);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("正在发DC-对地30kΩ指令");


                    Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BMSMeasureVoltage, Ks.ToArray(), 0, 3, "0");




                    //检测能否刷卡
                    WaitSwipingCard(testWorkParam.lstIDs, 3);
                    time = TestTime;
                    while (time-- > 0)
                    {
                        SendNoticeToUIAndTxtFile("判断充电机是否能充电倒计时:" + time);
                        Thread.Sleep(1000);
                    }

                    //CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);


                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "DC-对地30kΩ", "充电电压(V)", "0", "20");



                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("恢复互操设置中...");



                    //SetCPRersh_EUDCALL(); //放到ExecuteMethod()中保证恢复
                    #endregion

                    //SendNoticeToUIAndTxtFile("关闭BMS中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }

                //设置电压电流启动BMS

                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltage, BMSCurrent, BMSMeasureVoltage);
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                //检测插枪
                //WaitSwipingCard(testWorkParam.lstIDs, 1);


                ////检测刷卡
                //WaitSwipingCard(testWorkParam.lstIDs, 0);





                //ChargeState = AllEquipStateData.DicBMS_DC_StateData.Any(kvp => kvp.Value.ChargingState != "充电中");


                //Ks = GetKStatus16_Charging_DC();
                //Ks[22] = false;


                //ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());



                //SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                //ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, BMSVoltage - 20, BMSCurrent);
                //Thread.Sleep(500);
                //ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                //SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                //int timeout = 60;
                //while (timeout-- > 0)
                //{
                //    bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= BMSCurrent * 0.85);
                //    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                //    System.Threading.Thread.Sleep(1000);
                //}






                //double BMSDemandVoltageMin = BMSVoltage - 10;
                //double BMSDemandVoltageMax = BMSVoltage + 10;
                //int timeout = 1000;

                //while (timeout-- > 0)
                //{
                //    SendNoticeToUIAndTxtFile("等到充电机到达充电中...");
                //    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //    {

                //        int count = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => kvp.Value.ChargingState.Contains("充电中"));


                //        if (!ChargeState)
                //        {
                //            break;
                //        }
                //    }

                //    System.Threading.Thread.Sleep(100);
                //}

                //SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                //ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, BMSDemandVolt - 20, 20);
                //Thread.Sleep(500);
                //ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                //SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定

            }
            catch (Exception ex) { SendException(ex); }

        }

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
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
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));

            }
        }

        public override void ProcessData()
        {
            throw new NotImplementedException();
        }
    }
}
