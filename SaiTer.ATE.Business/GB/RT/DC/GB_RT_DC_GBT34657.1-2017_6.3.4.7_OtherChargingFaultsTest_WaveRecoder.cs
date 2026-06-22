using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
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
    /// 研测互操：其他充电故障测试(录波板)   //GB_RT_DC_GBT34657.1-2017_6.3.4.7_OtherChargingFaultsTest_WaveRecoder
    /// </summary>
    public class GB_RT_DC_OtherChargingFaultsTest_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 500;
        double DemandCurrent = 30;

        public GB_RT_DC_OtherChargingFaultsTest_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

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

                    #region 第一个点
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    SendNoticeToUIAndTxtFile("开启导引中");
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
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);


                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    CountDownTimeInfo("请手动模拟其他故障停止充电再点击确认或者是。", 99999, 1);

                    Thread.Sleep(5000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //读取录波板数据
                    double Time_OutputCurrent = 0;
                    double Time_K1K2 = 0;
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_K1K2 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputCurrent, false, 5, ref Time_OutputCurrent);
                    double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                    }

                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_OutputCurrent);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                    double Time_Stop = Math.Abs(Time_K1K2 - Time_OutputCurrent);

                    var dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    var dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dTime.Add(item, (Time_Stop).ToString("F2"));
                    }

                    double checktime = DemandCurrent / 100;

                    ProcessDataTmp(dTime, "其他故障停止充电", "K1K2断开时间(ms)", "0", "100", dImgs);

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        d1.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "其他故障停止充电", "辅源电压", "0", "0");

                    //CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    }
                    ProcessDataConnect("其他故障停止充电");
                    #endregion

                    //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", true, "其他故障停止充电");
                    }

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //SetLoadDCOFF(testWorkParam.lstIDs);
                    //SendNoticeToUIAndTxtFile("关闭导引中!");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}
