using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 开关S断开测试（录波板）  //GB_RT_DC_GBT34657.1-2017_6.3.4.2_S2DisconnectTest_WaveRecoder
    /// </summary>
    public class GB_RT_DC_S2DisconnectTest_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 750;
        double ResistanceLoadCurrent = 120;
        double ElectronicLoadCurrent = 2000;

        public GB_RT_DC_S2DisconnectTest_WaveRecoder(int type)
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

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

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


                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    System.Threading.Thread.Sleep(3000);

                    //CountDownTimeInfo("请按下电子锁后点击确认", 999, 0);
                    SendNoticeToUIAndTxtFile("模拟开关S断开");
                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                    Thread.Sleep(7000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(testWorkParam.lstIDs);

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

                    ProcessDataTmp(dCH15, "开关S断开", "中止充电报文发送延时(ms)", "0", "100", dImgs);
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

                    ProcessDataTmp(dK1K2time, "开关S断开", "断开K1K2延时(ms)", "0", "100", dImgs2);



                    double Time_OutputCurrent = 0;
                    double Time_CST_ConnectorFault = 0;
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_CST_ConnectorFault = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 42, 3, ref CH_CST, "CST_FaultState");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST_ConnectorFault, "CST_ConnectorFault");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputCurrent, false, 5, ref Time_OutputCurrent);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CST_ConnectorFault, true, 0.5, ref Time_CST_ConnectorFault);
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputCurrent);//设置光标2
                    double CurrentTime = Math.Abs(Time_CC1 - Time_OutputCurrent); ;//0到100ms

                    Dictionary<int, string> dImgs3 = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                    Dictionary<int, string> dCurrentTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCurrentTime.Add(item, (CurrentTime).ToString("F2"));
                    }

                    ProcessDataTmp(dCurrentTime, "开关S断开", "电流降至5A以下延时(ms)", "0", "50", dImgs3);


                    double Value15_5 = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST, CH_CST.LinePoints_Y.Count - 1);
                    double Value15_6 = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST_ConnectorFault, CH_CST_ConnectorFault.LinePoints_Y.Count - 1);


                    ProcessDataIsNor(Value15_5.ToString(), dImgs3, "CST故障中止报文", "1", "开关S断开");


                    ProcessDataIsNor(Value15_6.ToString(), dImgs3, "CST连接器异常报文", "1", "开关S断开");

                    //CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请检查充电桩是否有故障报警!", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);
                    }

                    ProcessDataConnect("开关S断开", "是否报警");

                    //CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 999, 2);
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请确认电子锁在直流60V以下后正常解锁!\r\n注：勾选上为可以正常解锁", 999, 2);
                    }

                    ProcessDataConnect("开关S断开", "是否正常解锁");

                    //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
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
