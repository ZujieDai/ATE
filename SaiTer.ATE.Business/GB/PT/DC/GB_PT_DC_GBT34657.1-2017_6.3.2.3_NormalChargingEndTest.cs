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
    /// 正常充电结束测试
    /// </summary>
    public class GB_PT_DC_NormalChargingEndTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 500;
        double DemandCurrent = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间

        public GB_PT_DC_NormalChargingEndTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // BMS需求电压设置(V)=500|负载电流设置(A)=30
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            //bool[] channelopen = new bool[8];
            //bool[] canchannelopen = new bool[20];
            //channelopen[0] = true;//1通道
            //channelopen[1] = true;//2通道
            //channelopen[3] = true;//4通道
            //channelopen[6] = true;//7通道
            //channelopen[7] = true;//8通道
            //canchannelopen[4] = true;//CAN5通道
            //canchannelopen[6] = true;//CAN7通道
            //canchannelopen[7] = true;//CAN8通道
            //canchannelopen[13] = true;//CAN14通道
            //canchannelopen[14] = true;//CAN15通道
            //SetChannel(channelopen, canchannelopen);
            //ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");    //设置太大会导致卡点失败

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
                var Ks = GetKStatus16_Charging_DC();
                Ks[22] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());

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
                    //设置测试条件
                    SetConditionValues();


                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");
                    #region 第一个点
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);


                    SendNoticeToUIAndTxtFile("设置触发...");


                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage_DC", "1", "V", false, "300", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_Current_DC", "1", "A", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_Current_AC", "1", "V", false, "10", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_Voltage_AC", "1", "A", false, "10", "0");
                    Thread.Sleep(waitTime);

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");


                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//1通道平均值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//2通道平均值
                    Thread.Sleep(waitTime);



                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 2, "10", "Single");
                    //Thread.Sleep(waitTime);

                    ////启动示波器
                    //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    Thread.Sleep(10 * 1000);
                    SendNoticeToUIAndTxtFile("发送停止充电命令中...");
                    //CC1断线
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    var Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());

                    SendNoticeToUIAndTxtFile("等待触发中...");
                    int timeout = 15;

                    //读取分析数据
                    Thread.Sleep(5000);
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - 5, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);//AC回路动作时间

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    //读取卡点时间
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 2, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "被动终止充电", "电流停止时间差(ms)", "0", "-", dImgs);

                    Dictionary<int, string> dTzsl = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dTzsl.Add(itmp.Key, (DemandCurrent / (Convert.ToDouble(itmp.Value) / 1000)).ToString());
                    }
                    ProcessDataTmp(dTzsl, "被动终止充电", "电流停止时间差(A/s)", "0", "-");


                    CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    ProcessDataConnect("被动终止充电");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SetCPReresh();

                    #region 主动停充
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage, DemandCurrent - 10);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");


                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage_DC", "1", "V", false, "300", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_Current_DC", "1", "A", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_Current_AC", "1", "V", false, "10", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_Voltage_AC", "1", "A", false, "10", "0");
                    Thread.Sleep(waitTime);

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");


                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//1通道平均值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//2通道平均值
                    Thread.Sleep(waitTime);



                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 2, "10", "Single");
                    //Thread.Sleep(waitTime);

                    ////启动示波器
                    //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);
                    Thread.Sleep(10 * 1000);

                    CountDownTimeInfo("请手动控制桩停止充电再点击确认或者是。", 99999, 1);

                    //读取分析数据
                    Thread.Sleep(5000);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - 5, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);//AC回路动作时间

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 2, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "主动终止充电", "电流停止时间差(ms)", "0", "-", dImgs);

                    dTzsl = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dTzsl.Add(itmp.Key, (DemandCurrent / (Convert.ToDouble(itmp.Value) / 1000)).ToString());
                    }
                    ProcessDataTmp(dTzsl, "主动终止充电", "电流停止时间差(A/s)", "0", "-");

                    CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    ProcessDataConnect("主动停止充电");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SetCPReresh();


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}
