using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流_电压纹波
    /// </summary>
    public class RippleVoltage_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double Allowance = 0;//带载余量
        int Count = 0;

        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储



        double BMSDemandVolt_Min = 0;
        double BMSDemandVolt_Max = 0;
        double 判定准则 = 0;
        double InputVoltDiff = 0;
        public RippleVoltage_GB_DC(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMS需求电压最小值(V)=400|BMS需求电压最大值(V)=745|负载电流设置(A)=40|判定准则(±%)=0.5|输入电压偏移修正(%)=0
            BMSDemandVolt_Min = Convert.ToDouble(strParams[0].Split('=')[1]);
            BMSDemandVolt_Max = Convert.ToDouble(strParams[1].Split('=')[1]);
            //BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            判定准则 = Convert.ToDouble(strParams[2].Split('=')[1]);
            if(strParams.Length >= 4)
            {
                InputVoltDiff = Convert.ToDouble(strParams[3].Split('=')[1]) / 100;
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

                List<double> BMSVolts = new List<double>() { BMSDemandVolt_Min, (BMSDemandVolt_Min + BMSDemandVolt_Max) / 2, BMSDemandVolt_Max };
                double MidCurrent = Convert.ToDouble((RatedCurrent * 0.5).ToString("f2"));
                List<double> Currents = new List<double>() { MidCurrent, 0, Convert.ToDouble(((MaxOutputPower * 1000) / MinAllowChargeVoltage).ToString("f2")) };
                // 最小电压段
                //for (int c = 0; c < Current.Length; c++)
                //{
                //    TrialMethod(BMSVolts[0], Current[2], 0.85 + InputVoltDiff, Current[c]);
                //    TrialMethod(BMSVolts[0], Current[2], 1.0, Current[c]);
                //    TrialMethod(BMSVolts[0], Current[2], 1.15 - InputVoltDiff, Current[c]);
                //}

                //// 中间电压段
                //Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / (BMSDemandVolt_Min + BMSDemandVolt_Max) / 2).ToString("f2"));
                //for (int c = 0; c < Current.Length; c++)
                //{
                //    TrialMethod(BMSVolts[1], Current[2], 0.85 + InputVoltDiff, Current[c]);
                //    TrialMethod(BMSVolts[1], Current[2], 1.0, Current[c]);
                //    TrialMethod(BMSVolts[1], Current[2], 1.15 - InputVoltDiff, Current[c]);
                //}

                //// 最高电压段
                //Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / BMSDemandVolt_Max).ToString("f2"));
                //for (int c = 0; c < Current.Length; c++)
                //{
                //    TrialMethod(BMSVolts[2], Current[2], 0.85 + InputVoltDiff, Current[c]);
                //    TrialMethod(BMSVolts[2], Current[2], 1.0, Current[c]);
                //    TrialMethod(BMSVolts[2], Current[2], 1.15 - InputVoltDiff, Current[c]);
                //}

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
                        Currents[2] = Convert.ToDouble(((MaxOutputPower * 1000) / BMSDemandVolt).ToString("f2"));
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
                //TrialMethod(BMSDemandVolt_Min, 0.85 + InputVoltDiff, LoadCurrent);

                //TrialMethod(BMSDemandVolt_Max, 0.85 + InputVoltDiff, LoadCurrent);

                //TrialMethod(BMSDemandVolt_Min, 1.0, LoadCurrent);

                //TrialMethod(BMSDemandVolt_Max, 1.0, LoadCurrent);

                //TrialMethod(BMSDemandVolt_Min, 1.15 - InputVoltDiff, LoadCurrent);

                //TrialMethod(BMSDemandVolt_Max, 1.15 - InputVoltDiff, LoadCurrent);



                #region ====暂不用以下流程====
                /*
                int waitTime = 50;

                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", "500", "Output_DC_V", "1", "V", false, "1", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", "400", "Output_DC_I", "1", "A", false, "50", "-2");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", "400", "Input_AC_I", "1", "A", false, "5", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", "500", "Input_AC_V", "1", "V", false, "10", "0");
                Thread.Sleep(waitTime);
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 1);//
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);//
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                List<double> BMSVoltage = new List<double>();
                List<double> BMSCurrent = new List<double>();
                BMSCurrent.Add(MaxAllowChargeCurrent);
                double DCurrent = 0;
                DCurrent = Convert.ToDouble(((MaxOutputPower * 1000) / MidAllowChargeVoltage).ToString("f2"));
                DCurrent = DCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DCurrent;
                BMSCurrent.Add(DCurrent);
                DCurrent = Convert.ToDouble(((MaxOutputPower * 1000) / MaxAllowChargeVoltage).ToString("f2"));
                DCurrent = DCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DCurrent;
                BMSCurrent.Add(DCurrent);



                BMSVoltage.Add(MinAllowChargeVoltage);
                BMSVoltage.Add(MidAllowChargeVoltage);
                BMSVoltage.Add(MaxAllowChargeVoltage);



                double[] Current = new double[3];
                Current[0] = 0;
                double NominalCurrent = (MaxOutputPower * 1000) / MaxAllowChargeVoltage;
                double MidCurrent = NominalCurrent / 2;

                Current[1] = MidCurrent;



                Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / MinAllowChargeVoltage).ToString("f2"));
                Current = CompareMaximum(Current, MaxAllowChargeCurrent);
                List<double> ACVoltage = new List<double>();

                ACVoltage.Add(0.85 * 220);
                ACVoltage.Add(1.0 * 220);
                ACVoltage.Add(1.15 * 220);

                #region 最小值电压

                SetOscilloscopeVolt(BMSVoltage[0]);
                for (int j = 0; j < ACVoltage.Count; j++)
                {
                    for (int k = 0; k < Current.Length; k++)
                    {
                        VoltageAccuracy(BMSVoltage[0], BMSCurrent[0], Current[k], ACVoltage[j], Count, Allowance, k);
                        Count++;
                    }
                }
                Count = Count - 1;

                #endregion
                */
                #endregion

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
            //BMSVolt = BMSVolt > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSVolt;
            //Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            BMSVolt = BMSVolt > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSVolt;
            BMSCurrent = BMSCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : BMSCurrent;
            Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            double LoadCurrent = Current == MaxAllowChargeCurrent ? Current - 10 : Current;
            if (LoadCurrent * BMSVolt >= MaxOutputPower * 1000)
                LoadCurrent -= 10;
            //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BMSVolt);
            //Thread.Sleep(2000);

            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * Rate);
            //Thread.Sleep(2000);
            SetACSource(testWorkParam.lstIDs, 220 * Rate);
            int waitTime = 50;
            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_V", "1", "V", false, "300", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1", "A", false, "50", "-2");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "Input_AC_V", "1", "V", false, "10", "0");
            Thread.Sleep(waitTime);

            //设置时基ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0");
            //添加测量值
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "AVERage", 1);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发，自动

            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


            SendNoticeToUIAndTxtFile(string.Format("设置BMS电压 {0}V,电流{1}A，恒压模式", BMSVolt, BMSCurrent));
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
            WaitDCVoltage(testWorkParam.lstIDs, BMSVolt);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double data = 0, minValue, maxValue;
            if (LoadCurrent > 0)
            {
                double LoadVolt = BMSVolt == MinAllowChargeVoltage ? BMSVolt - 10 : BMSVolt - 20;
                SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", LoadVolt, LoadCurrent));
                if (BMSVolt == MaxAllowChargeVoltage)
                    LoadCurrent = LoadCurrent - 10;
                SetLoadPara(testWorkParam.lstIDs, LoadVolt, LoadCurrent, BMSVolt, LoadCurrent);
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

            ProcessDataResult(testWorkParam.lstIDs, Current.ToString(), "需求电流(A)", true, sState);
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
            ProcessDataTmp(dic, sState, "充电电流(A)", "-", "-");

            Dictionary<int, string> dicVolt = new Dictionary<int, string>();
            minValue = BMSVolt * (1 - (判定准则 + 1) / 100);
            maxValue = BMSVolt * (1 + (判定准则 + 0.2) / 100);
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
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(3000);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dicVolt, sState, "输出电压(V)", "-", "-", dImgs);

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
            //ProcessDataTmp(dic, "输入额定电压" + Rate * 100 + "%", "功率分析仪交流电压A(V)", 
            //    ((220 * Rate) * (1 - (判定准则 + 1) / 100)).ToString("F2"), ((220 * Rate) * (1 + (判定准则 + 0.2) / 100)).ToString("F2"));
            ProcessDataTmp(dic, sState, "功率分析仪交流电压A(V)","-", "-");//交流源目前不做判断

            dic.Clear();
            foreach (var item in testWorkParam.lstIDs)
            {
                data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel2RMSVolt;
                int count = 300;
                while (count-- > 0)
                {
                    if (data < 220 * Rate * 0.9 || data > 220 * Rate * 1.1)
                    {
                        data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel2RMSVolt;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(item, data.ToString("F2"));
            }
            //ProcessDataTmp(dic, "输入额定电压" + Rate * 100 + "%", "功率分析仪交流电压B(V)",
            //    ((220 * Rate) * (1 - (判定准则 + 1) / 100)).ToString("F2"), ((220 * Rate) * (1 + (判定准则 + 0.2) / 100)).ToString("F2"));
            ProcessDataTmp(dic, sState, "功率分析仪交流电压B(V)", "-", "-");//交流源目前不做判断

            dic.Clear();
            foreach (var item in testWorkParam.lstIDs)
            {
                data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel3RMSVolt;
                int count = 300;
                while (count-- > 0)
                {
                    if (data < 220 * Rate * 0.9 || data > 220 * Rate * 1.1)
                    {
                        data = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel3RMSVolt;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(item, data.ToString("F2"));
            }
            //ProcessDataTmp(dic, "输入额定电压" + Rate * 100 + "%", "功率分析仪交流电压C(V)",
            //    ((220 * Rate) * (1 - (判定准则 + 1) / 100)).ToString("F2"), ((220 * Rate) * (1 + (判定准则 + 0.2) / 100)).ToString("F2"));
            ProcessDataTmp(dic, sState, "功率分析仪交流电压C(V)", "-", "-");//交流源目前不做判断



            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道，切换交流耦合");
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", Channel1, "Output_DC_V", "1", "V", false, "30", "-2");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1", "A", false, "50", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "Input_AC_V", "1", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 1);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 2);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);//
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            //设置时基ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0");
            Thread.Sleep(5000);
            //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            //Thread.Sleep(3000);

            SendNoticeToUIAndTxtFile("读示波器交流电压峰峰值");
            Dictionary<int, string> dic2 = new Dictionary<int, string>();
            dic2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1, true);//交流电压峰峰值
            ProcessDataTmp(dic2, sState, "输出电压交流分量峰峰值(V)", "-", "-");

            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            Dictionary<int, string> dicResult = new Dictionary<int, string>();
            foreach (var item in dic2)
            {
                double Xripple = System.Math.Abs(Convert.ToDouble(item.Value) * 100 * 0.5 / Convert.ToDouble(dicVolt[item.Key]));//电压纹波因数
                dicResult.Add(item.Key, Xripple.ToString("F2"));
            }
            //if (BMSVolt != BMSDemandVolt_Min)
            {
                ProcessDataTmp(dicResult, sState, "电压纹波因数(%)", "0", 判定准则.ToString(), dImgs);

            }
            SetLoadDCOFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        public override void ProcessData()
        {

        }
        /// <summary>
        /// 比较最大值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxvalue"></param>
        /// <returns></returns>
        public double[] CompareMaximum(double[] value, double maxvalue)
        {
            try
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = value[i] >= maxvalue ? maxvalue : value[i];
                }

                return value;
            }
            catch
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = 0;
                }
                return value;
            }
        }


        /// <summary>
        /// 比较最大值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxvalue"></param>
        /// <returns></returns>
        public double CompareMaximum(double value, double maxvalue)
        {
            try
            {
                value = value >= maxvalue ? maxvalue : value;
                return value;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 示波器设置电压纹波
        /// </summary>
        /// <param name="Voltage">电压值</param>
        public void SetOscilloscopeVolt(double Voltage)
        {
            if (Voltage == 200)
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "", "", "", "", "", "", false, "1", "");//通道1设置2
                System.Threading.Thread.Sleep(100);
            }
            if (Voltage == 600)
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "", "", "", "", "", "", false, "2.5", "");//通道1设置2
                System.Threading.Thread.Sleep(100);
            }
            if (Voltage == 1000)
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "", "", "", "", "", "", false, "5", "");//通道1设置2
                System.Threading.Thread.Sleep(100);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

        }
    }
}
