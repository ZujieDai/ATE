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
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:急停功能试验、启动急停装置试验
    /// </summary>

    public class EmergencyStopResearch : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 20;

        /// <summary>
        /// 电子负载电流(mA)
        /// </summary>
        Double ElectronicLoadCurrent = 2000;
        public EmergencyStopResearch(int type)
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
            channelopen[5] = true;//6通道
            channelopen[6] = true;//7通道

            SetChannel(channelopen, canchannelopen);

            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.1");

            ControlEquipMent.Oscillograph?.Oscillograph_MEASureOpen(true);

            //ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(testWorkParam.lstIDs, 1, 1, true, "DC", "500", "5000", "100", "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200", "BLUE", true, false, false, false);//通道1
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");
            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(4);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("MIN", 4, false, 0);

            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();
            DemandVoltage = DemandVoltage > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : DemandVoltage;
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(4);
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




                    ProcessDataTmp(dic, TrialItem.ItemName + "充电设置", "电压需求(V)", "-", "-");

                    ProcessDataTmp(dicC, TrialItem.ItemName + "充电设置", "电流需求(A)", "-", "-");




                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    SendNoticeToUIAndTxtFile("带载8秒中...");
                    Thread.Sleep(8000);

                    SendNoticeToUIAndTxtFile("设置触发...");
                    double EStop =Convert.ToDouble(OscillographInstrumentReadValue(6, false, 0));
                    double TriggerCurrent = 6;
                    //CJB出现了-0.03的情况
                    if (EStop < -0.1)
                    {
                        TriggerCurrent = -6;
                        OscillographInstrument_SetTrigger(TriggerCurrent, 6, 0, "BISLope", false, 50);
                    }
                    else
                    {
                        TriggerCurrent = 6;
                        OscillographInstrument_SetTrigger(TriggerCurrent, 6, 0, "BISLope", false, 50);
                    }
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("JCZT"))
                    {
                        if (EStop < 0)
                        {
                            TriggerCurrent = -9;
                        }
                        else
                        {
                            TriggerCurrent = 9;
                        }
                    }

                    // 回馈负载会导致电压异常和时间异常
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    CountDownTimeInfo("请按下急停按钮后点击是或者确认!", 9999, 0);

                    ReadTriggerTypeOscillograph(35);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("判断结果中!");


                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(7, false, 0, 0, true);
                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    double time = 0;
                    if (K1K2s2 > 8.5)
                    {
                        //time = GetTriggerTime_Single(7, false, 0, 6, 0, false, false, 0.05);//0到100ms
                        time = GetTriggerTime_Single(7, false, 0, 8.5, 0, false, false, 0.05);//0到100ms
                    }
                    else
                    {
                        //time = GetTriggerTime_Single(7, false, 0, 6, 1, false, false, 0.05);//0到100ms
                        time = GetTriggerTime_Single(7, false, 0, 8.5, 1, false, false, 0.05);//0到100ms
                    }


                    //double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));

                    //double time = 0;
                    //if (K1K2s2 > 2)
                    //{
                    //    time = GetTriggerTime_Single(6, false, 0, System.Math.Abs(TriggerCurrent), 0, false, false, 0.05);
                    //}
                    //else
                    //{
                    //    time = GetTriggerTime_Single(6, false, 0, System.Math.Abs(TriggerCurrent), 1, false, false, 0.05);

                    //}
                    double Min = OscillographInstrumentReadMeasure("MINimum", 4, false, 0);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    Dictionary<int, string> dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }

                    Dictionary<int, string> dVoltage = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dVoltage.Add(item, Math.Abs(Min).ToString("F2"));
                    }

                    ProcessDataTmp(dTime, TrialItem.ItemName, "断开K1K2时间(ms)", "0", "100", dImgs);
                    ProcessDataTmp(dVoltage, TrialItem.ItemName, "急停后的充电电压(V)", "0", "60");

                    CountDownTimeInfo("请恢复急停按钮！",9999,0);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }

    }
}
