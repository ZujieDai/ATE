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
    /// 研发测试:输出电流响应时间
    /// </summary>
    public class OutputCResponseTime_CJB : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        //Double DemandVoltage = 500;
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
        public OutputCResponseTime_CJB(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //回馈负载电流设置(A)=100|小于等于20A下降电流(A)=20|大于20A下降电流(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 3)
            {
                DemandCurrent = double.Parse(strParams[0].Split('=')[1]);
                MinusCurrent1 = double.Parse(strParams[1].Split('=')[1]);
                MinusCurrent2 = double.Parse(strParams[2].Split('=')[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            //滤波会导致电流下降延长
            if (ControlEquipMent.ResistanceLoad == null)
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetFiltering(lstIDs, 1, "FULL", 0, 0, 0, "200");//通道1

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            SetLoadDCOFF(lstIDs);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            canchannelopen[0] = true;//CAN1通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
            OscillographInstrumentStopRate_Gear(DemandCurrent);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);

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
                if (ControlEquipMent.ResistanceLoad == null)
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetFiltering(lstIDs, 1, "5000", 1, 1, 2, "200");//通道1

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
                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    dic.Add(item, DemandVoltage.ToString("F2"));
                    //}

                    //Dictionary<int, string> dicC = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    dicC.Add(item, DemandCurrent.ToString("F2"));
                    //}

                    //Dictionary<int, string> ddC1 = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    ddC1.Add(item, MinusCurrent1.ToString("F2"));
                    //}
                    //Dictionary<int, string> ddC2 = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    ddC2.Add(item, MinusCurrent1.ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, TrialItem.ItemName + "充电设置", "电压需求(V)", "-", "-");
                    //ProcessDataTmp(dicC, TrialItem.ItemName + "充电设置", "电流需求(A)", "-", "-");
                    //ProcessDataTmp(ddC1, TrialItem.ItemName + "负载设置", "下降电流差值1(A)", "-", "-");
                    //ProcessDataTmp(ddC2, TrialItem.ItemName + "负载设置", "下降电流差值2(A)", "-", "-");

                    TrialMethon(MinAllowChargeVoltage, "Umin");
                    TrialMethon((MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2, "Umen");
                    TrialMethon(MaxAllowChargeVoltage, "Umax");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }

        private void TrialMethon(double DemandVoltage, string UText)
        {
            ProcessDataResult(testWorkParam.lstIDs, "-", DemandVoltage.ToString(), true, $"设定充电机输出电压--{UText}");
            //负载电压小于200时会出现电流无法上去的问题
            double BMSDemandVolt = DemandVoltage - 20 <= MinAllowChargeVoltage + 20 ? MinAllowChargeVoltage + 10 : DemandVoltage;
            if (AllEquipStateData.DicPowerAnalyzer_StateData.First().Value.Channel4RMSVolt < 50)
            {
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
                SendNoticeToUIAndTxtFile("开启导引中");
                if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent, MaxAllowChargeVoltage, false))
                {
                    return;
                }
            }
            else
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent, false, 390);
                SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].ChargerType, 0);
                Thread.Sleep(5 * 1000);
                if (AllEquipStateData.DicPowerAnalyzer_StateData.First().Value.Channel4RMSVolt < 50)
                {
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent, MaxAllowChargeVoltage, false))
                    {
                        return;
                    }
                }
            }
            //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            SendNoticeToUIAndTxtFile("启动充电中");

            SendNoticeToUIAndTxtFile("开启负载中...");
            SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, DemandCurrent + 10, DemandVoltage, DemandCurrent);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
            Thread.Sleep(3 * 1000);

            #region 第一个点
            OscillographInstrumentStopRate_TimeBase(MinusCurrent1);
            double Current_Y1 = (DemandCurrent - MinusCurrent1) * 0.99;
            ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(1, DemandCurrent, Current_Y1);
            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
            var dic2 = new Dictionary<int, string>();
            var dicC2 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                dic2.Add(item, DCVoltage.ToString("F2"));
                double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                dicC2.Add(item, DCCurrent.ToString("F2"));
            }
            ProcessDataTmp(dic2, "调整前(小于等于20A下降电流)", "充电电压(V)", "-", "-");
            ProcessDataTmp(dicC2, "调整前(小于等于20A下降电流)", "充电电流(A)", "-", "-");
            double Level = -(DemandCurrent - MinusCurrent1 / 2);
            SendNoticeToUIAndTxtFile("设置录波仪触发中...");
            ControlEquipMent.Oscillograph?.Oscillograph_Trigger("RISE", 15, true, 1, Level.ToString(), "Single", 20);
            Thread.Sleep(3 * 1000); //加延迟防止变化太快
            SendNoticeToUIAndTxtFile("下发指令改变电流点1...");
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent - MinusCurrent1, false, 390);
            ReadTriggerTypeOscillograph(30);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);

            double DownCurrent = (DemandCurrent - MinusCurrent1) * 0.99;
            SystemEvent.MessageInfo(true, "判断结果中...");

            double time = GetTriggerTime_SingleWidthCount(1, false, 0, DownCurrent, 1, false, false, 0.1, 200);
            Dictionary<int, string> dTime = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                dTime.Add(item, (time * 1000).ToString("F2"));
            }

            dic2 = new Dictionary<int, string>();
            dicC2 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                dic2.Add(item, DCVoltage.ToString("F2"));
                double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                dicC2.Add(item, DCCurrent.ToString("F2"));
            }
            ProcessDataTmp(dic2, "调整后(小于等于20A下降电流)", "充电电压(V)", "-", "-");
            ProcessDataTmp(dicC2, "调整后(小于等于20A下降电流)", "充电电流(A)", ((DemandCurrent - MinusCurrent1) * 0.9).ToString(), ((DemandCurrent - MinusCurrent1) * 1.1).ToString());
            Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
            double checktime1 = MinusCurrent1 / 20.0;
            ProcessDataTmp(dTime, "调整后(小于等于20A下降电流)", "时间差(ms)", "0", (checktime1 * 1000).ToString(), dImgs);
            SystemEvent.MessageInfo(false, "判断结果中...");
            #endregion

            SendNoticeToUIAndTxtFile("关闭负载中!");
            SetLoadDCOFF(testWorkParam.lstIDs);

            SendNoticeToUIAndTxtFile("关闭导引中!");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            SetCPReresh();

            #region 第二个点


            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            SendNoticeToUIAndTxtFile("开启导引中");

            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent, false, 390);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            OscillographInstrumentStopRate_TimeBase(MinusCurrent2);
            double Current_Y2 = (DemandCurrent - MinusCurrent2) * 0.99;
            ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(1, DemandCurrent, Current_Y2);
            ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
            SendNoticeToUIAndTxtFile("启动充电中");
            SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].ChargerType, 0);
            Thread.Sleep(5 * 1000);

            SendNoticeToUIAndTxtFile("开启负载中...");
            SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
            Thread.Sleep(3 * 1000);

            dic2 = new Dictionary<int, string>();
            dicC2 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                dic2.Add(item, DCVoltage.ToString("F2"));
                double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                dicC2.Add(item, DCCurrent.ToString("F2"));
            }
            ProcessDataTmp(dic2, "调整前(大于20A下降电流)", "充电电压(V)", "-", "-");
            ProcessDataTmp(dicC2, "调整前(大于20A下降电流)", "充电电流(A)", "-", "-");

            SendNoticeToUIAndTxtFile("设置录波仪触发中...");
            Level = -(DemandCurrent - MinusCurrent2 / 2);
            ControlEquipMent.Oscillograph?.Oscillograph_Trigger("RISE", 15, true, 1, Level.ToString(), "Single", 20);
            Thread.Sleep(3 * 1000); //加延迟防止变化太快

            SendNoticeToUIAndTxtFile("下发指令改变电流点2...");
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, DemandCurrent - MinusCurrent2, false, 390);

            ReadTriggerTypeOscillograph(30);

            ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, -3, true);
            DownCurrent = (DemandCurrent - MinusCurrent2) * 0.99;
            SystemEvent.MessageInfo(true, "判断结果中...");
            time = GetTriggerTime_SingleWidthCount(1, false, 0, DownCurrent, 1, false, false, 0.05, 50);

            double checktime = MinusCurrent2 / 20;

            dic2 = new Dictionary<int, string>();
            dicC2 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                dic2.Add(item, DCVoltage.ToString("F2"));
                double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                dicC2.Add(item, DCCurrent.ToString("F2"));
            }
            ProcessDataTmp(dic2, "调整后(大于20A下降电流)", "充电电压(V)", "-", "-");
            ProcessDataTmp(dicC2, "调整后(大于20A下降电流)", "充电电流(A)", ((DemandCurrent - MinusCurrent2) * 0.9).ToString(), ((DemandCurrent - MinusCurrent2) * 1.1).ToString());
            Dictionary<int, string> dImgs2 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
            Dictionary<int, string> dTime2 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {

                dTime2.Add(item, (time * 1000).ToString("F2"));
            }
            ProcessDataTmp(dTime2, "调整后(大于20A下降电流)", "时间差(ms)", "0", (checktime * 1000).ToString(), dImgs2);

            SystemEvent.MessageInfo(false, "判断结果中...");
            #endregion

            SendNoticeToUIAndTxtFile("关闭负载中!");
            SetLoadDCOFF(testWorkParam.lstIDs);
        }


        public override void ProcessData()
        {

        }

    }
}
