using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Policy;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  稳压精度
    /// </summary>
    public class PrecisionSteadyVoltage_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double 判定准则 = 0;
        double InputVoltDiff = 0;
        double Uz = 500;    //中载实测输出电压

        public PrecisionSteadyVoltage_GB_DC(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //判定准则(±%)=0.5|输入电压偏移修正(%)=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);//(±1%)
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 2)
            {
                InputVoltDiff = Convert.ToDouble(strParams[1].Split('=')[1]) / 100;
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
                //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220);
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


                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}

                //设置测试条件
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

                List<double> BMSVolts = new List<double>() { MinAllowChargeVoltage, MidAllowChargeVoltage, MaxAllowChargeVoltage };
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
                        for(int c = 0; c < Currents.Count; c++)
                        {
                            //交流源是100%并且输出电流为中间电流的时候记录参考值
                            bool isMid = a == 0 && c == 0;
                            TrialMethod(BMSVolts[v], Currents[c] + 20, ACSourceRate[a], Currents[c], isMid);
                        }
                    }
                }

                //Dictionary<int, string> dic = new Dictionary<int, string>();
                //SendNoticeToUIAndTxtFile("输入电压85%额定值(V)");
                //TrialMethod(dic, 0.85, 负载最小电流设置, "输入电压85%额定值");
                //TrialMethod(dic, 0.85, 负载最大电流设置, "输入电压85%额定值");


                //SendNoticeToUIAndTxtFile("输入电压100%额定值(V)");
                //TrialMethod(dic, 1, 负载最小电流设置, "输入电压100%额定值");
                //TrialMethod(dic, 1, 负载最大电流设置, "输入电压100%额定值");


                //SendNoticeToUIAndTxtFile("输入电压115%额定值(V)");
                //TrialMethod(dic, 1.15, 负载最小电流设置, "输入电压115%额定值");
                //TrialMethod(dic, 1.15, 负载最大电流设置, "输入电压115%额定值");

            }

        }

        private void TrialMethod(double BMSVolt, double BMSCurrent, double Rate, double Current, bool isMid)
        {
            string sState = "输入额定电压" + Rate * 100 + "%";
            //BMSVolt = BMSVolt > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSVolt;
            //BMSCurrent = BMSCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : BMSCurrent;
            ////Current = Current > MaxAllowChargeCurrent - 5 ? MaxAllowChargeCurrent - 5 : Current;
            //Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            //double LoadCurrent = Current == MaxAllowChargeCurrent && ControlEquipMent.ResistanceLoad == null ? Current -10 : Current;
            //if ((LoadCurrent + 20) * BMSVolt >= MaxOutputPower * 1000)
            //    LoadCurrent -= 20;
            //if (LoadCurrent == BMSCurrent)
            //    LoadCurrent = BMSCurrent - 20;
            BMSCurrent = BMSCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : BMSCurrent;
            Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            double LoadCurrent = Current == MaxAllowChargeCurrent && ControlEquipMent.ResistanceLoad == null ? Current - 10 : Current;
            //带载余量
            if (LoadCurrent * BMSVolt >= MaxOutputPower * 1000)
                LoadCurrent -= 5;
            //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BMSVolt);
            //Thread.Sleep(2000);

            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * Rate);
            //Thread.Sleep(2000);
            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstIDs[0]].ChargingState;
            if (!BMSInfo.Contains("充电中"))
            {
                if (!CheckSwipingCard(testWorkParam.lstIDs, BMSVolt, BMSCurrent))
                {
                    return;
                }
            }
            SetACSource(testWorkParam.lstIDs, 220 * Rate);
            //int waitTime = 50;
            //初始化示波器
            //SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_V", "1", "V", false, "300", "0");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1", "A", false, "50", "-2");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "Input_AC_V", "1", "V", false, "10", "0");
            //Thread.Sleep(waitTime);

            ////设置时基ms
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0");
            ////添加测量值
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 1);//
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 1);//
            //Thread.Sleep(waitTime);

            ////启动示波器
            //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


            SendNoticeToUIAndTxtFile(string.Format("设置BMS电压 {0}V,电流{1}A，恒压模式", BMSVolt, BMSCurrent));
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
            if (!WaitDCVoltage(testWorkParam.lstIDs, BMSVolt, 30))
            {
                //可能因为导引的指令下发失败没有充起来
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
                WaitDCVoltage(testWorkParam.lstIDs, BMSVolt, 30);
            }
            Thread.Sleep(5 * 1000);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double data = 0, minValue, maxValue;
            if (LoadCurrent > 0)
            {
                if (ControlEquipMent.ResistanceLoad != null)
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt, LoadCurrent));
                else
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt - 20, LoadCurrent));
                SetLoadPara(testWorkParam.lstIDs, BMSVolt - 20, LoadCurrent, BMSVolt, LoadCurrent);
                Thread.Sleep(1500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, LoadCurrent);
                Thread.Sleep(3 * 1000);

                int OutTime = 20;
                Stopwatch st = new Stopwatch();
                st.Start();
                while (st.ElapsedMilliseconds / 1000 < OutTime)
                {
                    data = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent;
                    if (data > Current - 5)
                    {
                        break;
                    }
                }
            }
            else
            {
                //foreach (var item in testWorkParam.lstIDs)
                //{
                //    dic.Add(item, "0");
                //}
                //ProcessDataTmp(dic, sState, "充电电流(A)", "-", "-");
            }

            //ProcessDataResult(testWorkParam.lstIDs, Current.ToString(), "输出电流设定值(A)", true, sState);
            minValue = LoadCurrent - 10;
            maxValue = LoadCurrent + 10;
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
            ProcessDataTmp(dic, sState, "输出电流(A)", "-", "-");

            //var dicMAX = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);//顶峰值

            Dictionary<int, string> dicSteadyVolt = new Dictionary<int, string>();  //稳压精度
            Dictionary<int, string> dicVolt = new Dictionary<int, string>();    //输出电压
            minValue = BMSVolt * (1 - (判定准则 + 1) / 100);
            maxValue = BMSVolt * (1 + (判定准则 + 0.2) / 100);
            //dic = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MEAN", 1);//电压平均值
            foreach (var item in testWorkParam.lstIDs)
            {
                var Um = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSVolt;
                int count = 15;
                while (count-- > 0)
                {
                    if (isMid)
                    {
                        if (Math.Abs((Um - BMSVolt) / BMSVolt * 100) <= 判定准则)
                            break;
                    }
                    else
                    {
                        if (Math.Abs((Um - Uz) / Uz * 100) <= 判定准则)
                            break;
                    }
                    Um = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSVolt;
                    Thread.Sleep(1000);
                }
                if (isMid) Uz = Um;
                dicVolt.Add(item, Um.ToString("F2"));
                double steadyVoltRate = (Um - Uz) / Uz * 100;     //稳压精度
                dicSteadyVolt.Add(item, steadyVoltRate.ToString("F2"));
            }
            //var dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            //ProcessDataTmp(dicLimit, "输入额定电压" + Rate * 100 + "%", "输出电压极限值(V)", minValue.ToString("F2"), maxValue.ToString("F2"));

            //ProcessDataTmp(dicVolt, "输入额定电压" + Rate * 100 + "%", "输出电压测量值(V)", minValue.ToString("F2"), maxValue.ToString("F2"));
            ProcessDataTmp(dicVolt, sState, "输出电压(V)", "-", "-");

            ProcessDataTmp(dicSteadyVolt, sState, "稳压精度(%)", (-判定准则).ToString("F2"), 判定准则.ToString("F2"));

            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * rate);
            //SetACSource(testWorkParam.lstIDs, 220 * rate);
            //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, current, true, BMSDemandVolt);
            //SendNoticeToUIAndTxtFile(string.Format("设置BMS需求电压{0}V,需求电流{1}A,恒压模式。", BMSDemandVolt, current));
            //SendNoticeToUIAndTxtFile(string.Format("设置回馈负载电压{0}V,电流{1}A,启动负载，并等待带载电流稳定", BMSDemandVolt - 20, current - 10));

            //if (current > 0)
            //{
            //    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, current - 10, BMSDemandVolt - 20, current - 5);

            //    Thread.Sleep(500);
            //    SetLoadDCON(testWorkParam.lstIDs);
            //    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
            //}
            //double minVoltage = (BMSDemandVolt) * (1 - 判定准则 / 100);
            //double maxVoltage = (BMSDemandVolt) * (1 + 判定准则 / 100);
            //Dictionary<int, string> dic = new Dictionary<int, string>();
            //var dicVolt = new Dictionary<int, string>();
            //var dicCurrnet = new Dictionary<int, string>();
            //var dicSteadyVoltage = new Dictionary<int, string>();
            //foreach (var item in testWorkParam.lstIDs)
            //{
            //    double data = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
            //    int count = 30;
            //    while (count-- > 0)
            //    {
            //        if (data < minVoltage || data > maxVoltage)
            //        {
            //            data = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
            //            Thread.Sleep(1000);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }

            //    dic.Add(item, data.ToString("F2"));
            //    dicVolt.Add(item, BMSDemandVolt.ToString("F2"));
            //    dicCurrnet.Add(item, current.ToString("F2"));
            //}

            ////ProcessDataTmp(dic, strState, $"充电电流{current}(A)，测输出电压", ((BMSDemandVolt) * (1 - 判定准则 / 100)).ToString("F2"), ((BMSDemandVolt) * (1 + 判定准则 / 100)).ToString("F2"));

            //ProcessDataTmp(dicVolt, strState, "设置输出电压(V)", "-", "-");
            //ProcessDataTmp(dicCurrnet, strState, "设置输出电流(A)", "-", "-");
            //ProcessDataTmp(dic, strState, "实测输出电压(V)", "-", "-");
            if (Current > 0)
            {
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
            }
        }

        public override void ProcessData()
        {

        }
    }
}
