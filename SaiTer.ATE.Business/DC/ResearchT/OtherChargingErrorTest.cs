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
using System.Timers;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研测互操：其他充电故障测试
    /// </summary>
    public class OtherChargingErrorTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 500;
        double DemandCurrent = 30;

        public OtherChargingErrorTest(int type)
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
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[4] = true;//CAN5通道
            canchannelopen[6] = true;//CAN7通道
            canchannelopen[7] = true;//CAN8通道
            canchannelopen[13] = true;//CAN14通道
            canchannelopen[14] = true;//CAN15通道
            SetChannel(channelopen, canchannelopen);
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
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);

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

                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");
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

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    var s2Value = ControlEquipMent.Oscillograph.GetChannelValue(7, false, 0).FirstOrDefault().Value;
                    var K1K2s2 = System.Math.Abs(Convert.ToDouble(s2Value));
                    if (K1K2s2 < 2)
                    {
                        OscillographInstrument_SetTrigger(6, 7, 0, "RISE", false, 50);
                    }
                    else
                    {
                        OscillographInstrument_SetTrigger(6, 7, 0, "FALL", false, 50);
                    }
                    Thread.Sleep(10 * 1000);

                    CountDownTimeInfo("请手动模拟其他故障停止充电再点击确认或者是。", 99999, 1);

                    ReadTriggerTypeOscillograph(30);

                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 3, false);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //if (K1K2s2 < 2)
                    //    GetTriggerTime_Single(7, false, 0, 6, 0, false, false, -0.05);
                    //else
                    //    GetTriggerTime_Single(7, false, 0, 6, 1, false, false, -0.05);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 0, true);
                    double DownVolt = DemandVoltage * 0.9;
                    var time = GetTriggerTime_Single(15, true, 5, 1, 0, false, false, 0.05);

                    var dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    var dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }

                    double checktime = DemandCurrent / 100;

                    ProcessDataTmp(dTime, "其他故障停止充电", "K1K2断开时间(ms)", "0", "100", dImgs);

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        d1.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "其他故障停止充电", "辅源电压", "0", "2");

                    CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    ProcessDataConnect("其他故障停止充电");
                    #endregion

                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.ToString().ToUpper().Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", null, "其他故障停止充电");
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
