using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 开关S断开测试
    /// </summary>
    public class S2DisconnectTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 750;
        double ResistanceLoadCurrent = 120;

        public S2DisconnectTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // BMS需求电压设置(V)=750|电阻负载电流设置(A)=120|电子负载电流(mA)=2000
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                ResistanceLoadCurrent = double.Parse(strParams[1].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            SetLoadDCOFF(lstIDs);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);

            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            canchannelopen[4] = true;//CAN5通道
            canchannelopen[5] = true;//CAN6通道
            canchannelopen[6] = true;//CAN7通道
            canchannelopen[7] = true;//CAN8通道
            SetChannel(channelopen, canchannelopen);
            //ControlEquipMent.Oscillograph.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "500", "5000", "200", "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200", "BLUE", true, false, false, false);//通道1
            //滤波会导致电流下降延长
            if (ControlEquipMent.ResistanceLoad == null)
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetFiltering(lstIDs, 1, "FULL", 0, 0, 0, "200");//通道1
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");

            // 模拟插拔枪
            SetCPReresh();
        }

        public override void ExecuteMethod()
        {
            try
            {
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, true);

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
                if (ControlEquipMent.ResistanceLoad == null)
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetFiltering(lstIDs, 1, "5000", 1, 1, 2, "200");//通道1

                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);

                List<bool> Ks = GetKStatus16_Charging_DC();
                Ks[22] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                Thread.Sleep(500);
                //CountDownTimeInfo("请设置电子锁恢复", 999, 0);
                SendNoticeToUIAndTxtFile("关闭负载中!");
                SetLoadDCOFF(lstIDs);
                //ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(lstIDs);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
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

                    //CountDownTimeInfo("请手动设置电子锁失效后点击确认", 999, 0);

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, ResistanceLoadCurrent + 10))
                    {
                        return;
                    }
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, ResistanceLoadCurrent + 10, true, 390);
                    SendNoticeToUIAndTxtFile("启动负载...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, ResistanceLoadCurrent, DemandVoltage, ResistanceLoadCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, ResistanceLoadCurrent, 35);
                    Thread.Sleep(5 * 1000);
                    //设置电子负载
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x00);// 关闭短路
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x20, 0x01);// 设置负载的控制模式（20H ）操作模式（0 为面板操作模式，1 为远程操作模式）
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x00);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x2A, Convert.ToUInt32(ElectronicLoadCurrent) * 10);// 设置或读取负载的定电流值（2AH/2BH ）倍数 定电流值*10
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x21, 0x01);// 控制负载输入状态（21H ） 负载输入状态（0 为输出OFF，1 为输出ON）
                    //Thread.Sleep(200);
                    //SendNoticeToUIAndTxtFile("带载中...");
                    //Thread.Sleep(5000);

                    //设置测试条件
                    SetConditionValues();

                    // 记录带载后数据
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt.ToString("f2"));
                    }
                    ProcessDataTmp(d1, "充电过程中功分仪充电数据", $"电压(V)", "-", "-");
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent.ToString("f2"));
                    }
                    ProcessDataTmp(d1, "充电过程中功分仪充电数据", $"电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置触发...");
                    OscillographInstrument_SetTrigger(5, 3, 0, "RISE", false, 20);
                    System.Threading.Thread.Sleep(3000);

                    //CountDownTimeInfo("请按下电子锁后点击确认", 999, 0);
                    SendNoticeToUIAndTxtFile("模拟开关S断开");
                    if (this.Customer.Equals("CJB"))
                        CountDownTimeInfo("请断开桩的开关S后点击确认", 999, 0);
                    else
                    {
                        List<bool> Ks = GetKStatus16_Charging_DC();
                        Ks[22] = false;
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                    }

                    ReadTriggerTypeOscillograph(20);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("判断结果中!");

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(15, true, 5, -3, true);

                    double CH15_5time = GetTriggerTime_Single(15, true, 5, 1, 0, false, false, 0);//0到100ms
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    Dictionary<int, string> dCH15 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCH15.Add(item, (CH15_5time * 1000).ToString("F2"));
                    }

                    ProcessDataTmp(dCH15, "开关S断开", "中止充电报文发送延时(ms)", "0", "100", dImgs);
                    //TSField.LstItemData[Count].LstRecordDataWord[5] = CH15_5time.ToString();

                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(7, false, 0, -3, true);
                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    double K1K2time = 0;
                    if (K1K2s2 > 2)
                    {
                        K1K2time = GetTriggerTime_Single(7, false, 0, 6, 0, false, false, 0.05);//0到100ms
                    }
                    else
                    {
                        K1K2time = GetTriggerTime_Single(7, false, 0, 6, 1, false, false, 0.05);//0到100ms
                    }
                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    Dictionary<int, string> dK1K2time = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dK1K2time.Add(item, (K1K2time * 1000).ToString("F2"));
                    }

                    ProcessDataTmp(dK1K2time, "开关S断开", "断开K1K2延时(ms)", "0", "100", dImgs2);

                    //TSField.LstItemData[Count].LstRecordDataWord[6] = K1K2time.ToString();


                    //double Ta, Tb, Tc = 0;
                    //GetThreeTime(out Ta, out Tb, out Tc);
                    //ProcessDataResult(testWorkParam.lstIDs, Tb.ToString("F2"), "CSD统计报文时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    //Thread.Sleep(1000);
                    //ProcessDataResult(testWorkParam.lstIDs, Tc.ToString("F2"), "BSD统计报文时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    //Thread.Sleep(1000);
                    //if (Ta > Tb && Ta > Tc)
                    //{
                    //    //ProcessDataIsNor("是", dImgs2, "K3K4辅源断开在CSD和BSD之后是否合格", "是");
                    //    ProcessDataResult(testWorkParam.lstIDs, Ta.ToString("F2"), "K3K辅源断开时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    //}
                    //else
                    //{
                    //    //ProcessDataIsNor("否", dImgs2, "K3K4辅源断开在CSD和BSD之后是否合格", "是");
                    //    ProcessDataResult(testWorkParam.lstIDs, Ta.ToString("F2"), "K3K辅源断开时间", false, "K3K4辅源断开在CSD和BSD之后是否合格");
                    //}


                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);

                    double CurrentTime = GetTriggerTime_Single(1, false, 0, 5, 0, false, false, 0.05, true);//0到100ms

                    Dictionary<int, string> dImgs3 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    Dictionary<int, string> dCurrentTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCurrentTime.Add(item, (CurrentTime * 1000).ToString("F2"));
                    }

                    ProcessDataTmp(dCurrentTime, "开关S断开", "电流降至5A以下延时(ms)", "0", "50", dImgs3);


                    double Value15_5 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 5)));
                    double Value15_6 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 6)));


                    ProcessDataIsNor(Value15_5.ToString(), dImgs3, "CST故障中止报文", "1", "开关S断开");


                    ProcessDataIsNor(Value15_6.ToString(), dImgs3, "CST连接器异常报文", "1", "开关S断开");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);

                    ProcessDataConnect("开关S断开", "是否报警");

                    CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 999, 2);

                    ProcessDataConnect("开关S断开", "是否正常解锁");

                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", true, "开关S断开");
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
