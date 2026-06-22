using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:测量值更新时间
    /// </summary>
    public class MeasurementUpdate : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 60;

        /// <summary>
        /// 改变电流
        /// </summary>
        Double ChangeCurrent = 20;



        public MeasurementUpdate(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {


            SendNoticeToUIAndTxtFile("设备初始化中...");
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");
            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


            SendNoticeToUIAndTxtFile("初始化录波仪配置中...");
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            canchannelopen[0] = true;//CAN1通道
            canchannelopen[1] = true;//CAN2通道
            SetChannel(channelopen, canchannelopen);
            OscillographInstrumentUpdateTime_TimeBase(DemandCurrent - ChangeCurrent);
            //ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
            OscillographInstrumentUpdateTime_Gear(DemandCurrent);
            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);


            SetCPReresh();
        }

        public void OscillographInstrumentUpdateTime_Gear(double demandCurrent)
        {


            try
            {
                if (demandCurrent <= 80)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "10", "");
                }
                if (demandCurrent > 80 && demandCurrent <= 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "20", "");
                }
                if (demandCurrent > 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "40", "");
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        /// <summary>
        /// 设置录波仪电流时基
        /// </summary>
        /// <param name="Current"></param>
        public void OscillographInstrumentUpdateTime_TimeBase(double Current)
        {


            try
            {

                if (Current <= 30)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
                }
                if (Current > 30)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }


        }

        public override void InitializeParams()
        {
            Init();

            DemandCurrent = DemandCurrent >= 30 ? DemandCurrent : 30;
            ChangeCurrent = ChangeCurrent > DemandCurrent ? DemandCurrent - 1 : ChangeCurrent;
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
                    SetConditionValues();

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


                    Dictionary<int, string> dicChange = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dicChange.Add(item, ChangeCurrent.ToString("F2"));
                    }



                    ProcessDataTmp(dic, TrialItem.ItemName + "充电设置", "电压需求(V)", "-", "-");

                    ProcessDataTmp(dicC, TrialItem.ItemName + "充电设置", "电流需求(A)", "-", "-");

                    ProcessDataTmp(dicChange, TrialItem.ItemName + "充电设置", "改变电流(A)", "-", "-");


                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, MaxAllowChargeVoltage, false))
                    {
                        return;
                    }
                    Thread.Sleep(5 * 1000);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    //SendNoticeToUIAndTxtFile("启动充电中");
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    Thread.Sleep(3000);

                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 30;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 30;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "正常状态", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "正常状态", "充电电流(A)", "-", "-");

                    string CSSValue = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 2))).ToString();
                    Dictionary<int, string> DCSSValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        DCSSValue.Add(item, CSSValue);
                    }

                    ProcessDataTmp(DCSSValue, "正常状态", "CCS桩报文电流值(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");

                    double TriggerCurrent = -(DemandCurrent + ChangeCurrent) / 2;
                    OscillographInstrument_SetTrigger(TriggerCurrent, 15, 1, "RISE", true, 20);
                    System.Threading.Thread.Sleep(2000);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, ChangeCurrent, false, 390);


                    ReadTriggerTypeOscillograph(30);


                    double Current1 = 0;
                    //ChangeCurrent = ChangeCurrent + ErrorValue;
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                    {
                        Current1 = ChangeCurrent * 1.01;
                    }
                    else
                    {

                        Current1 = ChangeCurrent * 0.95;
                    }
                    double Current2 = -(ChangeCurrent * 1.015 + 1);
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //double time = GetTriggerTime_X1X2(1, false, 0, 15, true, 2, Current1, Current2, 1, 0, false, 0.05, false, false);
                    //ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, Current1, true);     //该方法卡点是设置屏幕比例的坐标点，不是数值
                    GetTriggerTime_Single(1, false, 0, Current1, 1, false, true, 0.05);
                    double time = GetTriggerTime_Single(15, true, 2, Current2, 0, false, false, 0.05);

                    OscillographInstrument_CursorPosition_SetChannel(1, false, 0);
                    OscillographInstrument_CursorPosition_Y_Value(1, DemandCurrent, ChangeCurrent);
                    

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    string CSSValue2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 2))).ToString();
                    Dictionary<int, string> DCSSValue2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        DCSSValue2.Add(item, CSSValue2);
                    }
                    ProcessDataTmp(DCSSValue2, "改变电流后", "CCS桩报文电流值(A)", "-", "-");

                    Dictionary<int, string> dicChangedCurrent = new Dictionary<int, string>();
                    double ChangedCurrent = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(1, false, 0)));
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dicChangedCurrent.Add(item, CSSValue2);
                    }
                    ProcessDataTmp(dicChangedCurrent, "改变电流后", "充电电流值(A)", "-", "-");

                    double Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    //double ICM = CSSValue2;
                    //double SettingError3 = System.Math.Abs(ICM - Current_4);
                    //double SetttingCheckValue = 0.015 * Current_4 + 1;

                    double SettingError = System.Math.Abs(((Current_4 - ChangeCurrent) / ChangeCurrent) * 100);
                    SettingError = RetainDecimals(SettingError);
                    double SettingError2 = System.Math.Abs(((Current_4 - ChangeCurrent)));
                    SettingError2 = RetainDecimals(SettingError2);
                    double Current_Message = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(1, false, 0)));
                    double MeasureError = System.Math.Abs(Current_Message - Current_4);
                    MeasureError = RetainDecimals(MeasureError);
                    double MeasureCheck = 0.015 * Current_4 + 1;

                    Dictionary<int, string> DTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        DTime.Add(item, (time * 1000).ToString());
                    }
                    ProcessDataTmp(DTime, "改变电流后", "∆X时间差(ms)", "0", "1000");


                    Dictionary<int, string> DCurrentError = new Dictionary<int, string>();
                    if (ChangeCurrent <= 30)
                    {
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            DCurrentError.Add(item, SettingError2.ToString("F2"));
                        }
                        ProcessDataTmp(DCurrentError, "改变电流后", "设定误差(A)", "0", "0.3");
                    }
                    else
                    {
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            DCurrentError.Add(item, SettingError.ToString("F2"));
                        }
                        ProcessDataTmp(DCurrentError, "改变电流后", "设定误差(%)", "0", "1");
                    }

                    Dictionary<int, string> DMeasureError = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        DMeasureError.Add(item, MeasureError.ToString("F2"));
                    }

                    ProcessDataTmp(DMeasureError, "改变电流后", "测量误差(%)", "0", MeasureCheck.ToString("F2"), dImgs);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


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
