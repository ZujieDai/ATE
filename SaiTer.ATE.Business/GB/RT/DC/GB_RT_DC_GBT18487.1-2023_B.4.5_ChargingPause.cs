using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流研测：充电暂停（能量传输）
    /// </summary>
    public class GB_RT_DC_ChargingPause : BusinessBase
    {

        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 60;
        /// <summary>
        /// 暂停时间10min内等待时间(min)
        /// </summary>
        int PauseWaitTime1 = 5;
        /// <summary>
        /// 暂停时间超过10min等待时间(min)
        /// </summary>
        int PauseWaitTime2 = 12;

        bool[] channelopen = new bool[8];
        bool[] canchannelopen = new bool[20];
        /// <summary>
        ///录波仪初始化
        /// </summary>
        private void InitOscillograph()
        {
            try
            {
                ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");

                channelopen[0] = true;//1通道
                channelopen[1] = true;//2通道
                channelopen[3] = true;//4通道
                channelopen[6] = true;//7通道
                channelopen[7] = true;//8通道
                canchannelopen[3] = true;//CAN 4通道
                canchannelopen[4] = true;//CAN 5通道
                canchannelopen[13] = true;//CAN 14通道
                canchannelopen[14] = true;//CAN 15通道

                SetChannel(channelopen, canchannelopen);

                ControlEquipMent.Oscillograph.Oscillograph_TimeBase("1");
                ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public GB_RT_DC_ChargingPause(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {
            InitOscillograph();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');

            //BMS需求电压设置(V)=745|BMS需求电流设置(A)=50|暂停时间10min内(min)=5|暂停时间超过10min(min)=12
            if (strParams.Length >= 6)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                PauseWaitTime1 = Convert.ToInt32(double.Parse(strParams[2].Split('=')[1]));
                PauseWaitTime2 = Convert.ToInt32(double.Parse(strParams[3].Split('=')[1]));
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
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(100);
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

                    DemandCurrent = DemandCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;

                    ControlEquipMent.BMS.BMS_OFF(lstIDs);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage + 20, DemandCurrent, true, DemandVoltage);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage + 20, LstChargerInfo[0].ChargerType, 0);
                    Thread.Sleep(3000);

                    //SendNoticeToUIAndTxtFile("设置导引参数中...");
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent, true, DemandVoltage);
                    //WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 20);
                    //Thread.Sleep(2000);

                    //模拟电池电压使暂停状态不会停止充电
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage, DemandCurrent + 10, DemandVoltage, DemandCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, true);
                    WaitDCCurrent(testWorkParam.lstIDs, DemandCurrent);
                    Thread.Sleep(1000 * 5);

                    Dictionary<int, string> dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停充电前", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicCurr, "暂停充电前", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("录波仪设置触发中");
                    OscillographInstrument_SetTrigger(DemandVoltage * 0.5, 8, 0, "FALL", false, 30, "Single");
                    Thread.Sleep(5000);

                    #region 暂停充电时
                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, DemandVoltage);
                    //模拟电池电压
                    var kstate = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out double r, out double b).First().Value;
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, r, b, kstate.ToArray());
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);
                    Thread.Sleep(100);
                    //回馈载的电压会影响测试结果
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}
                    //SetLoadDCOFF(testWorkParam.lstIDs);

                    CountDownTimeInfo("请确认电子锁保持可靠被锁止。\r\n(注:勾选上为可靠锁止)", 20, 2);
                    var dic = new Dictionary<int, string>();
                    foreach (var item in DicManualVerifyResult)
                    {
                        dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                    }
                    ProcessDataTmp(dic, "暂停状态发起时", "保持可靠锁止", "-", "-");

                    CountDownTimeInfo("请确认充电机设备是否进入暂停状态", 999, 0);
                    Thread.Sleep(1000 * 5);

                    ReadTriggerType(testWorkParam.lstIDs, 10);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    var dImages = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-", dImages);
                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < 0 || DCVoltage > 60)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停状态数据", "充电电压(V)", "-", "60");
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < 0 || DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicCurr, "暂停状态数据", "输出电流(A)", "-", "5");
                    #endregion

                    #region DC+DC-(C5C6)断开发送CST报文并不允许恢复充电
                    SendNoticeToUIAndTxtFile("录波仪设置触发中");
                    ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);
                    OscillographInstrument_SetTrigger(1, 15, 14, "RISE", true, 0, "Single");
                    Thread.Sleep(5000);

                    //模拟DC+-断开
                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态断开C5C6");
                    kstate = GetKStatus16_Charging_DC();
                    kstate[26] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(5000);

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    kstate[18] = true;//DC+
                    kstate[19] = true;//DC-
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent, true, DemandVoltage - 20);
                    Thread.Sleep(3000);

                    dImages = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    double CH15_14 = GetData_Value(15, true, 14, 0.999);
                    if (CH15_14 == 1)
                        ProcessDataResult(testWorkParam.lstIDs, dImages, "是", "是否发送CST中止报文", true, "断开C5C6后恢复充电");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, dImages, "否", "是否发送CST中止报文", false, "断开C5C6后恢复充电");

                    dic = new Dictionary<int, string>();
                    double current = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingCurrent;
                    if (current > 5)
                        ProcessDataResult(testWorkParam.lstIDs, "是", "是否恢复充电", false, "断开C5C6后恢复充电");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "否", "是否恢复充电", true, "断开C5C6后恢复充电");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs, false);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPReresh();

                    #region 恢复充电（不可超过10min）
                    SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage + 20, DemandCurrent))
                    {
                        return;
                    }
                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage, DemandCurrent + 10, DemandVoltage, DemandCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, true);
                    WaitDCCurrent(testWorkParam.lstIDs, DemandCurrent);
                    Thread.Sleep(1000 * 5);

                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停充电前", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicCurr, "暂停充电前", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, DemandVoltage);
                    //模拟电池电压
                    kstate = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out double r1, out double b1).First().Value; ;
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, r1, b1, kstate.ToArray());
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);
                    Thread.Sleep(100);
                    //回馈载的电压会影响测试结果
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}
                    //SetLoadDCOFF(testWorkParam.lstIDs);

                    CountDownTimeInfo("请确认充电机设备是否进入暂停状态", 999, 2);
                    Thread.Sleep(1000 * 5);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-");
                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < 0 || DCVoltage > 60)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停状态数据", "充电电压(V)", "-", "60");
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < 0 || DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicCurr, "暂停状态数据", "输出电流(A)", "-", "5");

                    OscillographInstrument_SetTrigger(DemandVoltage * 0.5, 8, 0, "RISE", false, 80, "Single");
                    ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

                    while (PauseWaitTime1 > 0)
                    {
                        SendNoticeToUIAndTxtFile($"充电暂停等待时间剩余{PauseWaitTime1}分钟");
                        Thread.Sleep(60 * 1000);
                        PauseWaitTime1--;
                    }

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent, true, DemandVoltage - 20);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 20);
                    Thread.Sleep(5000);
                    SetLoadDCON(testWorkParam.lstIDs, true);
                    WaitDCCurrent(testWorkParam.lstIDs, DemandCurrent);

                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    dImages = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    ProcessDataTmp(dicVolt, "暂停状态进入充电", "充电电压(V)", (DemandVoltage * 0.9).ToString(), (DemandVoltage * 1.1).ToString(), dImages);
                    ProcessDataTmp(dicCurr, "暂停状态进入充电", "充电电流(A)", (DemandCurrent * 0.9).ToString(), (DemandCurrent * 1.1).ToString());
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs, false);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPReresh();

                    #region 暂停时间超过10min
                    SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage + 20, DemandCurrent))
                    {
                        return;
                    }
                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage, DemandCurrent + 10, DemandVoltage, DemandCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs, true);
                    WaitDCCurrent(testWorkParam.lstIDs, DemandCurrent);
                    Thread.Sleep(1000 * 5);

                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停充电前", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicCurr, "暂停充电前", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备模拟进入暂停充电状态");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, DemandVoltage);
                    //模拟电池电压
                    kstate = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out double r2, out double b2).First().Value; ;
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, r2, b2, kstate.ToArray());
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage, false);
                    Thread.Sleep(100);
                    //回馈载的电压会影响测试结果
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}
                    //SetLoadDCOFF(testWorkParam.lstIDs);

                    CountDownTimeInfo("请确认充电机设备是否进入暂停状态", 999, 2);
                    Thread.Sleep(1000 * 5);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                        dic.Add(item, state);
                    }
                    ProcessDataTmp(dic, "暂停状态数据", "充电状态", "-", "-");
                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < 0 || DCVoltage > 60)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "暂停状态数据", "充电电压(V)", "-", "60");
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < 0 || DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicCurr, "暂停状态数据", "输出电流(A)", "-", "5");

                    OscillographInstrument_SetTrigger(DemandVoltage * 0.5, 8, 0, "RISE", false, 80, "Single");
                    ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

                    while (PauseWaitTime2 > 0)
                    {
                        SendNoticeToUIAndTxtFile($"充电暂停等待时间剩余{PauseWaitTime2}分钟");
                        Thread.Sleep(60 * 1000);
                        PauseWaitTime2--;
                    }

                    SendNoticeToUIAndTxtFile("设备模拟从暂停充电状态恢复到充电中状态");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent, true, DemandVoltage - 20);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage - 20);
                    Thread.Sleep(5000);
                    SetLoadDCON(testWorkParam.lstIDs, true);
                    WaitDCCurrent(testWorkParam.lstIDs, DemandCurrent);

                    dicVolt = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < 0 || DCVoltage > 60)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicVolt.Add(item, DCVoltage.ToString("F2"));
                    }
                    dicCurr = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < 0 || DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicCurr.Add(item, DCCurrent.ToString("F2"));
                    }
                    dImages = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    ProcessDataTmp(dicVolt, "暂停状态进入充电", "充电电压(V)", "-", "60");
                    ProcessDataTmp(dicCurr, "暂停状态进入充电", "充电电流(A)", "-", "5");
                    #endregion
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
