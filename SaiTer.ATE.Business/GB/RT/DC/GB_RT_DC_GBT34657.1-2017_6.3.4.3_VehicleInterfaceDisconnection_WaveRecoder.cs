using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    /// 研发测试:车辆接口断开测试(录波板)   
    /// </summary>

    public class GB_RT_DC_VehicleInterfaceDisconnection_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 750;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 60;

        /// <summary>
        /// 电子负载电流(mA)
        /// </summary>
        //Double ElectronicLoadCurrent = 2000;
        public GB_RT_DC_VehicleInterfaceDisconnection_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {

            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //if (strParams.Length >= 1)
            //{
            //    ElectronicLoadCurrent = double.Parse(strParams[0].Split('=')[1]);
            //}
            if (strParams.Length >= 1)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
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
                List<bool> Ks = GetKStatus16_Charging_DC();
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


                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dic.Add(item, DemandVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dicC.Add(item, DemandCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电设置", "电压需求(V)", "-", "-");

                    ProcessDataTmp(dicC, "充电设置", "电流需求(A)", "-", "-");




                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    //SendNoticeToUIAndTxtFile("启动充电中");

                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);


                    //SendNoticeToUIAndTxtFile("设置电子负载参数中...");
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x00);// 关闭短路
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x20, 0x01); //设置负载的控制模式（20H ）操作模式（0 为面板操作模式，1 为远程操作模式）
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x00);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
                    //Thread.Sleep(200);
                    //UInt32 para = Convert.ToUInt32(ElectronicLoadCurrent * 10);
                    //ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x2A, para);// // 设置或读取负载的定电流值（2AH/2BH ）
                    //Thread.Sleep(200);
                    //ControlEquipMent.ElectronicLoad?.ElectronicLoad_ON(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);

                    Thread.Sleep(2 * 1000);

                    SetLoadDCON(testWorkParam.lstIDs);

                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    SendNoticeToUIAndTxtFile("带载20秒中...");
                    Thread.Sleep(20 * 1000);

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    Thread.Sleep(2 * 1000);

                    SendNoticeToUIAndTxtFile("发送CC1断线中...");
                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());

                    Thread.Sleep(7000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("判断结果中!");


                    //读取录波板数据
                    double Time_CC1 = 0;
                    double Time_CST = 0;
                    double Time_K1K2 = 0;
                    WaveData CH_CC1 = new WaveData();
                    WaveData CH_CST = new WaveData();
                    WaveData CH_K1K2 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 42, 3, ref CH_CST, "CST_FaultState");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CC1, true, 5, ref Time_CC1);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CST, true, 0.5, ref Time_CST);
                    double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                    }

                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_CST);//设置光标2
                    double CH15_5time = Math.Abs(Time_CC1 - Time_CST);
                    Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    Dictionary<int, string> dCH15 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dCH15.Add(item, (CH15_5time).ToString("F2"));
                    }

                    ProcessDataTmp(dCH15, TrialItem.ItemName, "中止充电报文发送延时(ms)", "0", "100", dImgs);
                    //TSField.LstItemData[Count].LstRecordDataWord[5] = CH15_5time.ToString();


                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                    double Time_Stop = Math.Abs(Time_CC1 - Time_K1K2);
                    Dictionary<int, string> dImgs2 = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                    Dictionary<int, string> dK1K2time = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dK1K2time.Add(item, (Time_Stop).ToString("F2"));
                    }

                    ProcessDataTmp(dK1K2time, TrialItem.ItemName, "断开K1K2延时(ms)", "0", "100", dImgs2);

                    //TSField.LstItemData[Count].LstRecordDataWord[6] = K1K2time.ToString();

                    double Time_BSD = 0, Time_CSD = 0, Time_K3K4 = 0;
                    WaveData CH_BSD = new WaveData();
                    WaveData CH_CSD = new WaveData();
                    WaveData CH_K3K4 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 45, 0, ref CH_BSD, "BSD_SOC");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 52, 0, ref CH_CSD, "CSD_ChargerID");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref CH_K3K4, "K3K4");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BSD, true, 1, ref Time_BSD);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CSD, true, 0.5, ref Time_CSD);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K3K4, false, 2, ref Time_K3K4);

                    ProcessDataResult(testWorkParam.lstIDs, Time_CSD.ToString("F2"), "CSD统计报文时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    Thread.Sleep(1000);
                    ProcessDataResult(testWorkParam.lstIDs, Time_BSD.ToString("F2"), "BSD统计报文时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    Thread.Sleep(1000);
                    if (Time_K3K4 > Time_BSD && Time_K3K4 > Time_CSD)
                    {
                        //ProcessDataIsNor("是", dImgs2, "K3K4辅源断开在CSD和BSD之后是否合格", "是");
                        ProcessDataResult(testWorkParam.lstIDs, Time_K3K4.ToString("F2"), "K3K辅源断开时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                    }
                    else
                    {
                        double K3K4Value = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K3K4, CH_K3K4.LinePoints_X.Count - 1);
                        if (Time_K3K4 == 100 && K3K4Value < 1)
                        {
                            ProcessDataResult(testWorkParam.lstIDs, ">100", "K3K辅源断开时间", true, "K3K4辅源断开在CSD和BSD之后是否合格");
                        }
                        else
                        {
                            //ProcessDataIsNor("否", dImgs2, "K3K4辅源断开在CSD和BSD之后是否合格", "是");
                            ProcessDataResult(testWorkParam.lstIDs, Time_K3K4.ToString("F2"), "K3K辅源断开时间", false, "K3K4辅源断开在CSD和BSD之后是否合格");
                        }
                    }

                    double Time_OutputCurrent = 0;
                    double Time_CST_ConnectorFault = 0;
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_CST_ConnectorFault = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST_ConnectorFault, "CST_ConnectorFault");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputCurrent, false, 5, ref Time_OutputCurrent);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CST_ConnectorFault, true, 0.5, ref Time_CST_ConnectorFault);

                    double CurrentTime = Math.Abs(Time_CC1 - Time_OutputCurrent); ;//0到100ms

                    Dictionary<int, string> dImgs3 = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                    Dictionary<int, string> dCurrentTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCurrentTime.Add(item, (CurrentTime).ToString("F2"));
                    }

                    ProcessDataTmp(dCurrentTime, TrialItem.ItemName, "电流降至5A以下延时(ms)", "0", "500", dImgs3);


                    double Value15_5 = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST, CH_CST.LinePoints_X.Count - 1);
                    double Value15_6 = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST_ConnectorFault, CH_CST_ConnectorFault.LinePoints_X.Count - 1);


                    ProcessDataIsNor(Value15_5.ToString(), dImgs3, "CST故障中止报文", "1");


                    ProcessDataIsNor(Value15_6.ToString(), dImgs3, "CST连接器异常报文", "1");

                    //CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    }

                    ProcessDataWarn("是否报警");

                    //CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 999, 2);
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 999, 2);
                    }

                    ProcessDataConnect(TrialItem.ItemName, "电子锁在直流60V以下后正常解锁");


                    //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", true, TrialItem.ItemName);
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
