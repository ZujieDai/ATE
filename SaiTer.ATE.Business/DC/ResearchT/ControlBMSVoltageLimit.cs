using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 控制导引电压限值测试
    /// </summary>
    public class ControlBMSVoltageLimit : BusinessBase
    {
        string ItemStatus;
        string ItemFlow;
        int trlTimeOut_S = 30;
        double DemandVoltage = 745;
        double DemandCurrent = 20;
        double NormalCC1 = 3.9;
        double Normal2CC1 = 3.9;
        double MinCC1 = 2.8;
        double MaxCC1 = 5;

        public ControlBMSVoltageLimit(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // BMS需求电压设置(V)=745|BMS需求电流设置(A)=20|CC1正常电压设置(V)=3.9|CC1正常电压设置2(V)=4.1|CC1异常小电压(V)=2.8|CC1异常大电压(V)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 6)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                NormalCC1 = double.Parse(strParams[2].Split('=')[1]);
                Normal2CC1 = double.Parse(strParams[3].Split('=')[1]);
                MinCC1 = double.Parse(strParams[4].Split('=')[1]);
                MaxCC1 = double.Parse(strParams[5].Split('=')[1]);
            }
            // 限制值的范围
            if (NormalCC1 < 3.65)
            {
                NormalCC1 = 3.65;
            }
            if (NormalCC1 > 4.37)
            {
                NormalCC1 = 4.37;
            }
            if (Normal2CC1 < 3.65)
            {
                Normal2CC1 = 3.65;
            }
            if (Normal2CC1 > 4.37)
            {
                Normal2CC1 = 4.37;
            }

            if (MinCC1 < 2)
            {
                MinCC1 = 2;
            }
            if (MinCC1 > 3.2)
            {
                MinCC1 = 3.2;
            }

            if (MaxCC1 < 4.8)
            {
                MaxCC1 = 4.8;
            }
            if (MaxCC1 > 5.7)
            {
                MaxCC1 = 5.7;
            }

        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[4] = true;//5通道
            channelopen[6] = true;//7通道
            canchannelopen[4] = true;//CAN5通道
            canchannelopen[5] = true;//CAN6通道
            canchannelopen[6] = true;//CAN7通道
            canchannelopen[7] = true;//CAN8通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");

            // 模拟插拔枪
            SetCPReresh();
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
                ControlEquipMent.BMS.BMSSetResistance(lstIDs, 1000);
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
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    double Resistance1 = (NormalCC1) / (12 - 2 * NormalCC1) * 1000;//正常CC1
                    double Resistance1_2 = (Normal2CC1) / (12 - 2 * Normal2CC1) * 1000;//正常CC2
                    double Resistance2 = (MinCC1) / (12 - 2 * MinCC1) * 1000;//最小CC1
                    double Resistance3 = (MaxCC1) / (12 - 2 * MaxCC1) * 1000;//最大CC1
                    Resistance1 = RetainDecimals<double>(Resistance1, 1);
                    Resistance1_2 = RetainDecimals<double>(Resistance1_2, 1);
                    Resistance2 = RetainDecimals<double>(Resistance2, 1);
                    Resistance3 = RetainDecimals<double>(Resistance3, 1);
                    double Resistance4 = 1030;
                    double Resistance5 = 970;

                    #region 第一个点
                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("开启导引中，并设置正常CC1阻值1：" + Resistance1);
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);
                    SendNoticeToUIAndTxtFile("启动充电中");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    InsertCharger(testWorkParam.lstIDs);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance1, "CC1电压(V)", "3.65", "4.37");

                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("设置正常CC1阻值2：" + Resistance1_2);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1_2);
                    Thread.Sleep(3 * 1000);
                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance1_2, "CC1电压(V)", "3.65", "4.37");
                    Thread.Sleep(2 * 1000);
                    //Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", chargerState == "充电中", $"CC1正常电压{NormalCC1}充电中调整到正常电压{Normal2CC1}");

                    SendNoticeToUIAndTxtFile("设置正常CC1阻值1：" + Resistance1);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1);
                    Thread.Sleep(5 * 1000);
                    //dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, $"CC1正常电压{Normal2CC1}充电中调整到正常电压{NormalCC1}", "是");
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", chargerState == "充电中", $"CC1正常电压{Normal2CC1}充电中调整到正常电压{NormalCC1}");
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第三个点
                    //SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置异常CC1小电压，阻值：" + Resistance2);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance2);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    CountDownTimeInfo("CC1异常小电压，请检查充电桩是否连接!\r\n（勾上为不可连接）", 999, 2);
                    ItemStatus = $"异常CC1小电压(阻值{Resistance2})";
                    ItemFlow = "充电枪连接";
                    ProcessData();
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第四个点CC1Min
                    //SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置异常CC1大电压，阻值：" + Resistance3);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance3);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    CountDownTimeInfo("CC1异常大电压，请检查充电桩是否连接!\r\n（勾上为不可连接）", 999, 2);
                    ItemStatus = $"异常CC1大电压(阻值{Resistance3})";
                    ItemFlow = "充电枪连接";
                    ProcessData();

                    //SendNoticeToUIAndTxtFile("启动充电中");
                    //Charger_Start_DC();
                    ////SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    //dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //string chargingState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    //if (chargingState.Contains("等待握手报文") || chargingState.Contains("等待低压辅助电源")
                    //    || chargingState.Contains("空闲状态") || chargingState.Contains("充电结束")
                    //    || chargingState.Contains("等待辩识报文SPN2560=0x00"))
                    //{
                    //    ProcessDataIsNor(chargingState, dImgs, "CC1异常小电压充电状态", chargingState);
                    //}
                    //else
                    //{
                    //    ProcessDataIsNor(chargingState, dImgs, "CC1异常小电压充电状态", "0");    // 不通过
                    //}
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第五个点CC1Min-充电中
                    //SetCPReresh();
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值1000", "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("设置触发中...");
                    OscillographInstrument_SetTrigger(6, 2, 0, "FALL", false, 50);
                    System.Threading.Thread.Sleep(8 * 1000);
                    SendNoticeToUIAndTxtFile("设置到CC1异常小电压，阻值" + Resistance2);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance2);

                    SendNoticeToUIAndTxtFile("等待触发...");
                    int timeout = 35;
                    bool istrigger = false;
                    while (timeout-- > 0)
                    {
                        istrigger = ControlEquipMent.Oscillograph.Oscillograph_ReadTrigger().FirstOrDefault().Value == 0;
                        if (istrigger)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    System.Threading.Thread.Sleep(1000 * 10);
                    var dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    string CSTFaultSon = OscillographInstrumentReadValue(15, true, 6);
                    ProcessDataIsNor(CSTFaultSon, dImgs, "CST连接器异常", "1", $"CC1异常小电压(阻值{Resistance2})");
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第六个点CC1Min-充电中
                    //SetCPReresh();
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值1000", "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("设置触发中...");
                    OscillographInstrument_SetTrigger(6, 2, 0, "FALL", false, 50);
                    System.Threading.Thread.Sleep(8 * 1000);
                    SendNoticeToUIAndTxtFile("设置到CC1异常大电压，阻值" + Resistance3);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance3);

                    SendNoticeToUIAndTxtFile("等待触发...");
                    timeout = 35;
                    istrigger = false;
                    while (timeout-- > 0)
                    {
                        istrigger = ControlEquipMent.Oscillograph.Oscillograph_ReadTrigger().FirstOrDefault().Value == 0;
                        if (istrigger)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    System.Threading.Thread.Sleep(1000 * 10);
                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    CSTFaultSon = OscillographInstrumentReadValue(15, true, 6);
                    ProcessDataIsNor(CSTFaultSon, dImgs, "CST连接器异常", "1", $"CC1异常大电压(阻值{Resistance3})");
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第七个点
                    //SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置正常CC1电压，阻值：" + Resistance4);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance4);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance4, "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, "是否充电中", "是"); chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "正常CC1阻值" + Resistance4, chargerState, chargerState == "充电中", "充电状态");
                    #endregion

                    PullCharger(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第八个点
                    //SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置正常CC1电压，阻值：" + Resistance5);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance5);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance5, "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, "是否充电中", "是");
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "正常CC1阻值" + Resistance4, chargerState, chargerState == "充电中", "充电状态");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = ItemStatus + "|" + ItemFlow + "|-|-|否|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = ItemStatus + "|" + ItemFlow + "|-|-|是|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    SaveTrialData(LstTrialData[k]);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }
    }
}
