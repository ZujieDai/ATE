using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 稳压精度
    /// </summary>
    public class CZ_TB_PrecisionSteadyVoltage : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double 判定准则 = 0;
        double InputVoltDiff = 0;
        double Uz = 500;    //中载实测输出电压
        List<double> BMSVolts;
        List<double> Currents;

        public CZ_TB_PrecisionSteadyVoltage(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //误差(±%)=0.5|需求电压(V)=300|半载0%In(A)=10|空载0%In(A)=0|满载100%In(A)=20
            string[] strParams = TrialItem.ResultParams.Split('|');
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);//(±1%)
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 2)
            {
                BMSVolts = new List<double>() { Convert.ToDouble(strParams[1].Split('=')[1]) };
                Currents = new List<double>()
                {
                    Convert.ToDouble(strParams[2].Split('=')[1]),
                    Convert.ToDouble(strParams[3].Split('=')[1]),
                    Convert.ToDouble(strParams[4].Split('=')[1]),
                };
            }
            else
            {
                BMSVolts = new List<double>() { MaxAllowChargeVoltage };
                double MidCurrent = Convert.ToDouble((RatedCurrent * 0.5).ToString("f2"));
                Currents = new List<double>() { MidCurrent, 0, Convert.ToDouble(((MaxOutputPower * 1000) / MinAllowChargeVoltage).ToString("f2")) };
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

                SetCPReresh();
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

                List<double> ACSourceRate = new List<double>() { 1 };
                for (int v = 0; v < BMSVolts.Count; v++)
                {
                    string UText = "Umin";
                    if (v == 1)
                    {
                        UText = "Umen";
                    }
                    else if (v == 2)
                    {
                        UText = "Umax";
                    }
                    Currents = CompareMaximum(Currents.ToArray(), MaxAllowChargeCurrent).ToList();
                    ProcessDataResult(testWorkParam.lstIDs, "-", BMSVolts[v].ToString() + "V", true, $"设定充电机输出电压");
                    for (int a = 0; a < ACSourceRate.Count; a++)
                    {
                        for (int c = 0; c < Currents.Count; c++)
                        {
                            //交流源是100%并且输出电流为中间电流的时候记录参考值
                            bool isMid = a == 0 && c == 0;
                            TrialMethod(BMSVolts[v], Currents[c], ACSourceRate[a], Currents[c], isMid);
                        }
                    }
                }

                SetLoadDCOFF(testWorkParam.lstIDs);
                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }

        }

        private void TrialMethod(double BMSVolt, double BMSCurrent, double Rate, double Current, bool isMid)
        {
            string sState = "输入额定电压" + Rate * 100 + "%";
            //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BMSVolt);
            //Thread.Sleep(2000);

            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * Rate);
            //Thread.Sleep(2000);
            int BMSInfo = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState);
            if (BMSInfo < 20 || BMSInfo > 23 )
            {
                if (!CheckSwipingCard(testWorkParam.lstIDs, BMSVolt, BMSCurrent + 5))
                {
                    return;
                }
            }


            SendNoticeToUIAndTxtFile(string.Format("设置BMS电压 {0}V,电流{1}A，恒压模式", BMSVolt, BMSCurrent + 5));
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent + 5, true, BMSVolt);
            if (!WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSVolt, 30))
            {
                //可能因为导引的指令下发失败没有充起来
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent + 5, true, BMSVolt);
                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSVolt, 30);
            }
            Thread.Sleep(5 * 1000);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double data = 0, minValue, maxValue;
            double LoadCurrent = Current;
            if (LoadCurrent > 0)
            {
                if (ControlEquipMent.ResistanceLoad != null)
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt, LoadCurrent));
                else
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt - 20, LoadCurrent));
                SetLoadPara(testWorkParam.lstIDs, BMSVolt - 20, LoadCurrent, BMSVolt, LoadCurrent);
                Thread.Sleep(1500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, LoadCurrent);
                Thread.Sleep(3 * 1000);

                int OutTime = 20;
                Stopwatch st = new Stopwatch();
                st.Start();
                while (st.ElapsedMilliseconds / 1000 < OutTime)
                {
                    data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
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
                data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                int count = 30;
                while (count-- > 0)
                {
                    if (data < minValue || data > maxValue)
                    {
                        data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
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
