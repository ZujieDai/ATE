using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
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
    /// 稳压精度试验(源自南网企标，目前河南产测使用)
    /// </summary>
    public class GB_PT_DC_SteadyVoltAccuracy_XJ_QB : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double 判定准则 = 0;
        double Uz = 500;    //中载实测输出电压

        public GB_PT_DC_SteadyVoltAccuracy_XJ_QB(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //判定准则(±%)=0.5
            string[] strParams = TrialItem.ResultParams.Split('|');
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);//(±1%)
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
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


                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

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
                            TrialMethod(BMSVolts[v], MaxAllowChargeCurrent, ACSourceRate[a], Currents[c], isMid);
                        }
                    }
                }
            }

        }

        private void TrialMethod(double BMSVolt, double BMSCurrent, double Rate, double Current, bool isMid)
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
            BMSVolt = BMSVolt > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSVolt;
            BMSCurrent = BMSCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : BMSCurrent;
            //Current = Current > MaxAllowChargeCurrent - 5 ? MaxAllowChargeCurrent - 5 : Current;
            Current = Current > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Current;
            double LoadCurrent = Current == MaxAllowChargeCurrent ? Current - 10 : Current;
            if (!Customer.Equals("XJ"))
            {
                if ((LoadCurrent + 10) * BMSVolt >= MaxOutputPower * 1000)
                    LoadCurrent = (MaxOutputPower * 1000 / BMSVolt) - 10;//超出了最大值，把负载做处理
            }
            //LoadCurrent -= 10;

            SetACSource(testWorkParam.lstIDs, 220 * Rate);


            SendNoticeToUIAndTxtFile(string.Format("设置BMS电压 {0}V,电流{1}A，恒压模式", BMSVolt, BMSCurrent));
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
            if (!WaitDCVoltage(testWorkParam.lstIDs, BMSVolt, 30))
            {
                //可能因为导引的指令下发失败没有充起来
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVolt, BMSCurrent, true, BMSVolt);
                WaitDCVoltage(testWorkParam.lstIDs, BMSVolt, 30);
            }
            Thread.Sleep(3 * 1000);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double data = 0, minValue, maxValue;
            if (LoadCurrent > 0)
            {
                if (ControlEquipMent.ResistanceLoad != null)
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt, LoadCurrent));
                else
                    SendNoticeToUIAndTxtFile(string.Format("设置负载电压 {0}V,电流{1}A，恒压模式", BMSVolt - 20, LoadCurrent));
                SetLoadPara(testWorkParam.lstIDs, BMSVolt - 20, LoadCurrent, BMSVolt, LoadCurrent);
                Thread.Sleep(200);
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
            Thread.Sleep(5000);//这里加延时，客户的桩电压采集偶尔会误差较大

            //ProcessDataResult(testWorkParam.lstIDs, Current.ToString(), "输出电流设定值(A)", true, sState);
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
            ProcessDataTmp(dic, sState, "输出电流(A)", "-", "-");

            Dictionary<int, string> dicSteadyVolt = new Dictionary<int, string>();  //稳压精度
            Dictionary<int, string> dicVolt = new Dictionary<int, string>();    //输出电压
            minValue = BMSVolt * (1 - (判定准则 + 1) / 100);
            maxValue = BMSVolt * (1 + (判定准则 + 0.2) / 100);
            foreach (var item in testWorkParam.lstIDs)
            {
                var Um = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSVolt;
                int count = 60;
                while (count-- > 0)
                {
                    if (Um < minValue || Um > maxValue)
                    {
                        Um = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSVolt;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                if (isMid) Uz = Um;
                dicVolt.Add(item, Um.ToString("F2"));
                double steadyVoltRate = (Um - Uz) / Uz * 100;     //稳压精度
                dicSteadyVolt.Add(item, steadyVoltRate.ToString("F2"));
            }
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
