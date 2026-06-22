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
    /// 研发测试:预充电功能试验
    /// </summary>
    public class PreCharging : BusinessBase
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
        /// 下降的电流差值需要小于等于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent1 = 20;
        /// <summary>
        /// 下降的电流差值需要大于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent2 = 40;

        double BatteryVoltage = 390;
        public PreCharging(int type)
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
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");

            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "10", "");

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(4, "100", "-5");

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(8, "100", "-5");

            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("Y");

            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
            SetCPReresh();
        }
        public void OscillographInstrumentStopRate_Gear(double demandCurrent)
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
        public void OscillographInstrumentStopRate_TimeBase(double Current)
        {
            try
            {
                if (Current <= 20)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
                }
                if (Current >= 40 && Current <= 80)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
                }
                if (Current > 80 && Current <= 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("1");
                }
                if (Current > 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("2");
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
            //BMS需求电压(V)=500|电池电压(V)=390
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length >= 2)
            {
                DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            if(strParams.Length >= 2)
            {
                BatteryVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, BatteryVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "充电设置", "报文电池电压(V)", "-", "-");

                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    if (K1K2s2 < 2)
                    {
                        OscillographInstrument_SetTrigger(6, 7, 0, "RISE", false, 80, "Auto");
                    }
                    else
                    {
                        OscillographInstrument_SetTrigger(6, 7, 0, "FALL", false, 80, "Auto");
                    }
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);


                    SendNoticeToUIAndTxtFile("开启导引中");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVoltage);   //互操蓄电池电压
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BatteryVoltage, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                    Thread.Sleep(500);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(300);


                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 60 * 5;
                    while (timeout-- > 0)
                    {
                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state >= 5 /*&& state <= 7*/)
                        {
                            SendNoticeToUIAndTxtFile("下发录波仪触发。");
                            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                            break;
                        }
                        if (timeout % 5 == 0)
                        {
                            int residuetime = timeout / 5;
                            SendNoticeToUIAndTxtFile("刷卡剩余倒计时:" + residuetime);
                        }
                        System.Threading.Thread.Sleep(200);
                    }

                    MessgaeInfo(false, "请刷卡充电!");

                    timeout = 20 * 5;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state==9)
                        {
                            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                            break;
                        }
                        if (timeout % 5 == 0)
                        {
                            int residuetime = timeout / 5;
                            SendNoticeToUIAndTxtFile("等到到达充电中:" + residuetime);
                        }
                        System.Threading.Thread.Sleep(200);

                    }

                    //设置测试条件
                    SetConditionValues();

                    ReadTriggerTypeOscillograph(30);
                    Thread.Sleep(6000);
                    double DownCurrent = (DemandCurrent) * 0.99;
                    SendNoticeToUIAndTxtFile("判断结果中...");

                    double Y1 = 0;
                    //GETInsertValue(4, false, 0, out Y1, 40, 5);
                    GETInsertValue(4, false, 0, out Y1, 100, 80);

                    double Y2 = 0;
                    //GETInsertValue(8, false, 0, out Y2, 40, 21);
                    GETInsertValue(8, false, 0, out Y2, 100, 80);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(4, Y1, Y2);
                    OscillographInstrument_CursorPosition_SetChannel(4, false, 0);
                    double Dvalue = 0;
                    Dvalue = Y1 - Y2;
                    Dvalue = RetainDecimals<double>(Dvalue);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    Dictionary<int, string> dY1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dY1.Add(item, Y1.ToString("F2"));
                    }

                    Dictionary<int, string> dY2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dY2.Add(item, Y2.ToString("F2"));
                    }

                    Dictionary<int, string> dY1Y2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dY1Y2.Add(item, Dvalue.ToString("F2"));
                    }


                    ProcessDataTmp(dY1, "闭合K5和K6后", "电池电压光标值(Y1)", "-", "-");
                    ProcessDataTmp(dY2, "闭合K5和K6后", "K1K2前端电压光标值(Y2)", " - ", "-");
                    ProcessDataTmp(dY1Y2, "闭合K5和K6后", "Y1和Y2差值", " 1 ", "10", dImgs);
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
