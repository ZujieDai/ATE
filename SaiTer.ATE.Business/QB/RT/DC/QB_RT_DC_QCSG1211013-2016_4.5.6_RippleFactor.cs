using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 企标研测直流：纹波系数试验
    /// </summary>
    public class QB_RT_DC_RippleFactor : BusinessBase
    {
        int trlTimeOut_S = 30;
        string tapewidth;

        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储


        public QB_RT_DC_RippleFactor(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[3] = true;//4通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");

            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(false);

            ControlEquipMent.Oscillograph?.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(4);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("PTOPeak", 4, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("RMS", 4, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Min", 4, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Max", 4, false, 0);
        }

        public override void InitializeParams()
        {
            Init();
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');

            tapewidth = ControlEquipMent.ResistanceLoad != null ? "FULL" : "5000";
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
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 4, 4, true, "DC", tapewidth, "5000", Channel1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4
                SetACSource(lstIDs, 220);
                Thread.Sleep(500);
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
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
                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    d1.Add(testWorkParam.lstIDs[i], RatedCurrent.ToString());
                    d2.Add(testWorkParam.lstIDs[i], MaxOutputPower.ToString());
                }
                SetConditionValue("额定电流(A)", d1);
                SetConditionValue("最大功率(kW)", d2);

                SendNoticeToUIAndTxtFile("开启负载并机");
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);

                List<double> BMSVolts = new List<double>() { MinAllowChargeVoltage, (MinAllowChargeVoltage + MaxAllowChargeVoltage) / 2, MaxAllowChargeVoltage };
                double MidCurrent = Convert.ToDouble((RatedCurrent * 0.5).ToString("f2"));
                List<double> Currents = new List<double>() { MidCurrent, 0, Convert.ToDouble(((MaxOutputPower * 1000) / MinAllowChargeVoltage).ToString("f2")) };
                List<double> ACSourceRate = new List<double>() { 1, 0.85, 1.15 };
                for (int v = 0; v < BMSVolts.Count; v++)
                {
                    string UText = "Umin";
                    if (v == 1)
                    {
                        UText = "Umen";
                        // 中间电压段
                        Currents[2] = Convert.ToDouble(((MaxOutputPower * 1000) / BMSVolts[1]).ToString("f2"));
                    }
                    else if (v == 2)
                    {
                        UText = "Umax";
                        // 最高电压段
                        Currents[2] = Convert.ToDouble(((MaxOutputPower * 1000) / MaxAllowChargeVoltage).ToString("f2"));
                    }
                    Currents = CompareMaximum(Currents.ToArray(), MaxAllowChargeCurrent).ToList();
                    ProcessDataResult(testWorkParam.lstIDs, "-", BMSVolts[v].ToString(), true, $"设定充电机输出电压--{UText}");
                    for (int a = 0; a < ACSourceRate.Count; a++)
                    {
                        for (int c = 0; c < Currents.Count; c++)
                        {
                            //交流源是100%并且输出电流为中间电流的时候记录参考值
                            bool isMid = a == 0 && c == 0;
                            //double current = c == 2 ? Currents[c] - 10 : Currents[c];
                            TrialMethod(BMSVolts[v], Currents.Max(), ACSourceRate[a], Currents[c]);
                        }
                    }
                }

                SendNoticeToUIAndTxtFile("取消负载并机");
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
            }

        }

        private void TrialMethod(double BMSVolt, double BMSCurrent, double Rate, double Current)
        {
            string sState = "输入额定电压" + Rate * 100 + "%";
            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstIDs[0]].ChargingState;
            if (!BMSInfo.Contains("充电中"))
            {
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
            }
            BMSCurrent = BMSCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : BMSCurrent;
            Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            double LoadCurrent = Current == MaxAllowChargeCurrent ? Current - 10 : Current;
            //带载余量
            if (LoadCurrent * BMSVolt >= MaxOutputPower * 1000)
                LoadCurrent -= 5;

            SetACSource(testWorkParam.lstIDs, 220 * Rate);

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(testWorkParam.lstIDs, 4, 4, true, "DC", tapewidth, "5000", Channel1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4
            //启动示波器
            ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);
            Thread.Sleep(3000);


            SendNoticeToUIAndTxtFile(string.Format("设置BMS电压 {0}V,电流{1}A，恒压模式", BMSVolt, BMSCurrent));
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
            WaitDCVoltage(testWorkParam.lstIDs, BMSVolt);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double data = 0, minValue, maxValue;
            if (LoadCurrent > 0)
            {
                if (ControlEquipMent.ResistanceLoad != null)
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt, LoadCurrent));
                else
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt - 20, LoadCurrent));
                if (BMSVolt == MaxAllowChargeVoltage)
                    LoadCurrent = LoadCurrent - 10;
                SetLoadPara(testWorkParam.lstIDs, BMSVolt - 20, LoadCurrent, BMSVolt, LoadCurrent);
                Thread.Sleep(200);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, LoadCurrent);
                Thread.Sleep(3 * 1000);
            }
            else
            {
                //foreach (var item in testWorkParam.lstIDs)
                //{
                //    dic.Add(item, "0");
                //}
                //ProcessDataTmp(dic, sState, "充电电流(A)", "-", "-");
            }

            ProcessDataResult(testWorkParam.lstIDs, Current.ToString(), "输出电流点(A)", true, sState);
            minValue = LoadCurrent - 5;
            maxValue = LoadCurrent + 5;
            foreach (var item in testWorkParam.lstIDs)
            {
                data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                int count = 30;
                while (count-- > 0)
                {
                    if (data < minValue || data > maxValue)
                    {
                        data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(item, data.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "输出电流实测值(A)", "-", "-");

            Dictionary<int, string> dicVolt = new Dictionary<int, string>();
            minValue = BMSVolt * 0.9;
            maxValue = BMSVolt * 1.02;
            //dic = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MEAN", 1);//电压平均值
            foreach (var item in testWorkParam.lstIDs)
            {
                data = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage;
                int count = 30;
                while (count-- > 0)
                {
                    if (data < minValue || data > maxValue)
                    {
                        data = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                dicVolt.Add(item, data.ToString("F2"));
            }
            ControlEquipMent.Oscillograph.Oscillograph_IsRun(false);
            Thread.Sleep(2000);
            dImgs = ControlEquipMent.Oscillograph.OscillographSaveScreen();
            ProcessDataTmp(dicVolt, sState, "输出电压平均值(V)", "-", "-", dImgs);

            dic.Clear();
            foreach (var item in testWorkParam.lstIDs)
            {
                data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel1RMSVolt;
                int count = 300;
                while (count-- > 0)
                {
                    if (data < 220 * Rate * 0.9 || data > 220 * Rate * 1.1)
                    {
                        data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel1RMSVolt;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(item, data.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "功率分析仪交流电压A(V)", "-", "-");//交流源目前不做判断

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(testWorkParam.lstIDs, 4, 4, true, "AC", tapewidth, "5000", Channel1, "DC-out-V", "V", false, "3", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4
            ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);
            Thread.Sleep(10 * 1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(2000);
            dImgs = ControlEquipMent.Oscillograph.OscillographSaveScreen();

            #region 纹波峰值系数
            SendNoticeToUIAndTxtFile("读录波仪交流电压有效值");
            dic.Clear();
            foreach (int i in testWorkParam.lstIDs)
            {
                double RMS = OscillographInstrumentReadMeasure("RMS", 4, false, 0);
                int count = 10;
                while (count-- > 0)
                {
                    if (RMS == 0 || RMS > maxValue)
                    {
                        RMS = OscillographInstrumentReadMeasure("RMS", 4, false, 0); //交流分量峰峰值
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(i, RMS.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "输出电压交流分量有效值(V)", "-", "-");

            Dictionary<int, string> dicResult = new Dictionary<int, string>();
            foreach (var item in dic)
            {
                double Xripple = System.Math.Abs(Convert.ToDouble(item.Value) * 100 * 0.5 / Convert.ToDouble(dicVolt[item.Key]));//电压纹波因数
                dicResult.Add(item.Key, Xripple.ToString("F2"));
            }
            //if (BMSVolt != BMSDemandVolt_Min)
            {
                ProcessDataTmp(dicResult, sState, "纹波有效值系数(%)", "0", "0.5");

            }
            #endregion

            #region 纹波峰值系数
            SendNoticeToUIAndTxtFile("读录波仪交流电压峰峰值");
            dic.Clear();
            foreach (int i in testWorkParam.lstIDs)
            {
                double PKPK = OscillographInstrumentReadMeasure("PTOPeak", 4, false, 0);
                int count = 10;
                while (count-- > 0)
                {
                    if (PKPK == 0 || PKPK > maxValue)
                    {
                        PKPK = OscillographInstrumentReadMeasure("PTOPeak", 4, false, 0); //交流分量峰峰值
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(i, PKPK.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "输出电压交流分量峰峰值(V)", "-", "-");

            dicResult = new Dictionary<int, string>();
            foreach (var item in dic)
            {
                double Xripple = System.Math.Abs(Convert.ToDouble(item.Value) * 100 * 0.5 / Convert.ToDouble(dicVolt[item.Key]));//电压纹波因数
                dicResult.Add(item.Key, Xripple.ToString("F2"));
            }
            //if (BMSVolt != BMSDemandVolt_Min)
            {
                ProcessDataTmp(dicResult, sState, "纹波峰值系数(%)", "0", "1", dImgs);

            }
            #endregion

            SetLoadDCOFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        public override void ProcessData()
        {

        }
    }
}
