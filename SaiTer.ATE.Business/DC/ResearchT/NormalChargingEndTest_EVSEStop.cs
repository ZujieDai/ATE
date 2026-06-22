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
    /// 研测测试项：正常充电结束测试-主动停止充电（EVSE Stop）
    /// </summary>
    public class NormalChargingEndTest_EVSEStop : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 500;
        double DemandCurrent = 30;

        public NormalChargingEndTest_EVSEStop(int type)
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

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[2] = true;//CAN3通道
            canchannelopen[4] = true;//CAN5通道
            canchannelopen[6] = true;//CAN7通道
            canchannelopen[7] = true;//CAN8通道
            canchannelopen[13] = true;//CAN14通道
            canchannelopen[14] = true;//CAN15通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");    //设置太大会导致卡点失败

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

                    #region 主动停充
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage, DemandCurrent - 10);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    var s2Value = ControlEquipMent.Oscillograph.GetChannelValue(7, false, 0).FirstOrDefault().Value;
                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(s2Value));
                    //if (K1K2s2 < 2)
                    //{
                    //    OscillographInstrument_SetTrigger(6, 7, 0, "RISE", false, 50);
                    //}
                    //else
                    //{
                    //    OscillographInstrument_SetTrigger(6, 7, 0, "FALL", false, 50);
                    //}
                    OscillographInstrument_SetTrigger(DemandVoltage * 0.95, 8, 0, "FALL", false, 50);
                    Thread.Sleep(10 * 1000);

                    CountDownTimeInfo("请手动控制桩停止充电再点击确认或者是。", 99999, 1);

                    ReadTriggerTypeOscillograph(30);

                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 3, false);

                    double DownVolt = 5;
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //if (K1K2s2 < 2)
                    //    GetTriggerTime_Single(7, false, 0, 6, 0, false, false, -0.05);
                    //else
                    //    GetTriggerTime_Single(7, false, 0, 6, 1, false, false, -0.05);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 0, true);
                    double time = GetTriggerTime_Single(8, false, 0, DownVolt, 0, false, false, -0.05, true);

                    var dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    var dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }

                    double checktime = DemandCurrent / 100;

                    ProcessDataTmp(dTime, "主动停止充电", "停充时间差(ms)", "0", "1000", dImgs);

                    if (Customer != null && Customer.ToString().Contains("ZD"))
                    {
                        Dictionary<int, string> dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                            int timeout = 50;
                            while (timeout-- > 0)
                            {
                                if (DCCurrent > 10)
                                {
                                    DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                    Thread.Sleep(100);
                                }
                                else
                                    break;
                            }
                            dic.Add(item, DCCurrent.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "主动停止充电", "充电电流(A)", "-", "-");
                    }

                    CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", 999, 2);
                    ProcessDataConnect("主动停止充电");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    //SetCPReresh();

                    #region 第二个点--输出电流停止速率的测试项，跟这个步骤一样
                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    //OscillographInstrument_SetTrigger(5, 1, 0, "FALL", false, 80);
                    //OscillographInstrument_SetTrigger(100, 15, 3, "RISE", true, 20);  //不需要设置触发，直接卡点

                    //CountDownTimeInfo("请手动控制桩停止充电再点击确认或者是。", 99999, 1);

                    //ReadTriggerTypeOscillograph(30);

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);
                    GetTriggerTime_Single2(15, true, 3, 1, 0, false, true, 0.05);

                    double DownCurrent = 5;
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    time = GetTriggerTime_Single(1, false, 0, DownCurrent, 1, false, false, -0.05);
                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    //不先关掉负载电压会一直在
                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    var dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage > 30)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    var dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent > 5)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "停止充电后", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "停止充电后", "充电电流(A)", "-", "-");

                    dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }
                    checktime = DemandCurrent / 100;
                    ProcessDataTmp(dTime, "停止充电后", "停充时间差(ms)", "0", (checktime * 1000).ToString(), dImgs);

                    Dictionary<int, string> dRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dRate.Add(item, ((DemandCurrent - DownCurrent) / time).ToString("F2"));
                    }
                    ProcessDataTmp(dRate, "停止充电后", "输出电流停止速率(A/s)", "100", "-", dImgs);
                    #endregion

                    //SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}
