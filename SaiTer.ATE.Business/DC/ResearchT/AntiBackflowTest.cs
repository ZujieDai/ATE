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
    /// 研测（录波仪）：防逆流功能试验
    /// </summary>
    public class AntiBackflowTest : BusinessBase
    {
        int trlTimeOut_S = 30;

        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储



        public AntiBackflowTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");

            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            //ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "5", "-4");
            ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "300", "5000", Channel2, "DC-out-I", "A", false, "5", "-4", 1, 1, 2, "10Hz", "BLUE", true, false, false, false);//通道1

            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("");
            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Min", 1, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Max", 1, false, 0);

            ControlEquipMent.Oscillograph?.Oscillograph_Trigger("RISE", 4, false, 0, "650", "Auto", 50);
        }

        public override void InitializeParams()
        {
            Init();
            dImgs = new Dictionary<int, string>();//图片存储
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "50", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200Hz", "BLUE", true, false, false, false);//通道1

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
                    SendNoticeToUIAndTxtFile("开启并机继电器");
                    CombineControlResistance();

                    SendNoticeToUIAndTxtFile("开启负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                        d3.Add(testWorkParam.lstIDs[i], MaxOutputPower.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SetConditionValue("最大功率(kW)", d3);

                    //double DemandVoltage = 500;
                    //double DemandCurrent = 20;
                    //Double LoadVoltage = 480;
                    //Double LoadCurrent = 10;
                    //Double LoadVoltage2 = 700;
                    //Double LoadCurrent2 = 10;
                    double DemandVoltage = MaxAllowChargeVoltage >= 500 ? 500 : MaxAllowChargeVoltage;
                    double DemandCurrent = 20;
                    Double LoadVoltage = DemandVoltage - 20;
                    Double LoadCurrent = 10;
                    Double LoadVoltage2 = DemandVoltage + 200;
                    Double LoadCurrent2 = 10;

                    if (DemandVoltage < 390 && !(Customer != null && Customer.Contains("DH")))
                        BatteryVoltage = DemandVoltage - 10;
                    else
                        BatteryVoltage = 390;


                    SendNoticeToUIAndTxtFile("启动充电中");

                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }

                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    //WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);

                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                    SendNoticeToUIAndTxtFile("设置负载参数中...");
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        SetLoadPara(testWorkParam.lstIDs, LoadVoltage, LoadCurrent, DemandVoltage, LoadCurrent2);
                        SendNoticeToUIAndTxtFile("开启负载...");
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, LoadCurrent, 40);
                        Thread.Sleep(5000);
                    }

                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        SetLoadPara(testWorkParam.lstIDs, LoadVoltage2, LoadCurrent2, LoadVoltage2, 0, true, false);
                        WaitDCVoltage(testWorkParam.lstIDs, LoadVoltage2);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("发送过压中...");
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, MaxAllowChargeVoltage + 300);
                        //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, LoadVoltage2);
                        Thread.Sleep(2000);
                        List<bool> Ks = GetKStatus16_Charging_DC();
                        Ks[26] = true;
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, Ks.ToArray());
                    }
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    WaitDCCurrent(testWorkParam.lstIDs, LoadCurrent2);
                    //}


                    ReadTriggerType(testWorkParam.lstIDs, 15);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);

                    Dictionary<int, string> InDCImin = ControlEquipMent.Oscillograph?.Oscillograph_ReadMeasure("MIN", 1, false, 0);
                    Dictionary<int, string> dImgs1 = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    Dictionary<int, string> DCImin = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = Convert.ToDouble(InDCImin[item]);
                        DCImin.Add(item, DCCurrent.ToString("F2"));
                    }

                    ProcessDataTmp(DCImin, "防逆流功能", "直流回路反向电流(A)", "-0.5", "-", dImgs1);

                    CountDownTimeInfo("请确认桩是否报警\r\n注：勾选上为有报警提示", 20, 2);
                    ProcessData();

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    if (ControlEquipMent.FeedbackLoad == null && ControlEquipMent.LoopFeedbackLoad == null && ControlEquipMent.StarLoopFeedbackLoad == null)
                    {
                        var Ks = GetKStatus16_Charging_DC();
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVoltage);
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, Ks.ToArray());
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
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
                        LstTrialData[k].ExtentData = "防逆流功能" + "|是否报警|-|-|是|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = "防逆流功能" + "|是否报警|-|-|否|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
