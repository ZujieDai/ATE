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
    public class CZ_TB_PrecisionSteadyCurrent : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double 判定准则 = 0;
        double InputVoltDiff = 0;
        double Iz = 500;    //中电压实测输出电流
        List<double> BMSVolts;
        List<double> BMSCurrents;

        public CZ_TB_PrecisionSteadyCurrent(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //误差(±%)=1|输出电压Umen(V)=350|输出电压Umax(V)=500|输出电流1(A)=10|输出电流2(A)=7
            string[] strParams = TrialItem.ResultParams.Split('|');
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);//(±1%)
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 2)
            {
                BMSVolts = new List<double>() 
                { 
                    Convert.ToDouble(strParams[1].Split('=')[1]),
                    Convert.ToDouble(strParams[2].Split('=')[1])
                };
                BMSCurrents = new List<double>()
                {
                    Convert.ToDouble(strParams[3].Split('=')[1]),
                    Convert.ToDouble(strParams[4].Split('=')[1]),
                };
            }
            else
            {
                BMSVolts = new List<double>() { MidAllowChargeVoltage, MinAllowChargeVoltage, MaxAllowChargeVoltage };
                double MidCurrent = Convert.ToDouble((RatedCurrent * 0.5).ToString("f2"));
                BMSCurrents = new List<double>() { 0.5 * RatedCurrent, 0.2 * RatedCurrent, RatedCurrent };
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

                //SetCPReresh();
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

                for (int i = 0; i < BMSCurrents.Count; i++)
                {
                    int rateInC = 50;
                    if (i == 1) rateInC = 20;
                    else if (i == 2) rateInC = 100;
                    ProcessDataResult(testWorkParam.lstIDs, "-", BMSCurrents[i].ToString() + "A", true, $"设定充电机输出电流");
                    for (int a = 0; a < ACSourceRate.Count; a++)
                    {
                        for (int v = 0; v < BMSVolts.Count; v++)
                        {
                            //交流源是100%并且输出电压为中间电压的时候记录参考值
                            bool isMid = a == 0 && v == 0;
                            TrialMethod(BMSVolts[v], ACSourceRate[a], BMSCurrents[i], isMid);
                        }
                    }
                }

                //Dictionary<int, string> dic = new Dictionary<int, string>();
                //SendNoticeToUIAndTxtFile("输入电压85%额定值(V)");
                //TrialMethod(dic, 0.85 + InputVoltDiff, 负载最小电流设置, "电压85%额定值");
                //TrialMethod(dic, 0.85 + InputVoltDiff, 负载最大电流设置, "电压85%额定值");


                //SendNoticeToUIAndTxtFile("输入电压100%额定值(V)");
                //TrialMethod(dic, 1, 负载最小电流设置, "电压100%额定值");
                //TrialMethod(dic, 1, 负载最大电流设置, "电压100%额定值");


                //SendNoticeToUIAndTxtFile("输入电压115%额定值(V)");
                //TrialMethod(dic, 1.15 - InputVoltDiff, 负载最小电流设置, "电压115%额定值");
                //TrialMethod(dic, 1.15 - InputVoltDiff, 负载最大电流设置, "电压115%额定值");

                SetLoadDCOFF(testWorkParam.lstIDs);
                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }

        }

        private void TrialMethod(double BMSDemandVolt, double rate, double current, bool isMid)
        {
            string sState = "额定输入电压" + rate * 100 + "%";
            int BMSInfo = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState);
            if (BMSInfo < 20 || BMSInfo > 23)
            {
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    ProcessDataResult(testWorkParam.lstIDs, "-", "无法启动充电", false, $"设置BMS电压{LstChargerInfo[0].NominalVoltage}V,电流250A");
                    return;
                }
            }

            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * rate);
            SetACSource(testWorkParam.lstIDs, 220 * rate);
            Thread.Sleep(2 * 1000);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt + 5, current, false, BMSDemandVolt);
            SendNoticeToUIAndTxtFile(string.Format("设置BMS需求电压{0}V,需求电流{1}A,恒流模式。启动负载，并等待带载电流稳定", BMSDemandVolt, current));
            if (!WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSDemandVolt, 30))
            {
                //可能因为导引的指令下发失败没有充起来
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt + 5, current, false, BMSDemandVolt);
                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSDemandVolt, 30);
            }
            Thread.Sleep(3 * 1000);

            SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt, current + 10, BMSDemandVolt, current + 10);
            Thread.Sleep(1500);
            SetLoadDCON(testWorkParam.lstIDs);
            //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
            WaitDCCurrent_EU_DC(testWorkParam.lstIDs, current);   //恒流模式电流不会拉高
            Thread.Sleep(3 * 1000);

            ProcessDataResult(testWorkParam.lstIDs, BMSDemandVolt.ToString(), "需求电压(V)", true, sState);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                var data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                int count = 30;
                while (count-- > 0)
                {
                    if (data < BMSDemandVolt * 0.99 || data > BMSDemandVolt * 1.01)
                    {
                        data = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                dic.Add(item, data.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "输出电压(V)", "-", "-");

            double minCurrent = current * (1 - 判定准则 / 100);
            double maxCurrent = current * (1 + 判定准则 / 100);
            Dictionary<int, string> dicSteadyCurrent = new Dictionary<int, string>();  //稳流精度
            Dictionary<int, string> dicCurrent = new Dictionary<int, string>();    //输出电流
            foreach (var item in testWorkParam.lstIDs)
            {
                var Im = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                int count = 20;
                while (count-- > 0)
                {
                    if (isMid)
                    {
                        if (Math.Abs((Im - current) / current * 100) <= 判定准则)
                            break;
                    }
                    else
                    {
                        if (Math.Abs((Im - Iz) / Iz * 100) <= 判定准则)
                            break;
                    }
                    Im = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    Thread.Sleep(1000);
                }

                if (isMid) Iz = Im;
                dicCurrent.Add(item, Im.ToString("F2"));
                double steadyCurrentRate = (Im - Iz) / Iz * 100;     //稳流精度
                dicSteadyCurrent.Add(item, steadyCurrentRate.ToString("F2"));
            }
            //ProcessDataTmp(dic, strState, $"充电电压{bmsVolt}(A),测输出电流", (current * (1 - 判定准则 / 100)).ToString("F2"), (current * (1 + 判定准则 / 100)).ToString("F2"));
            //ProcessDataTmp(dic, strState, $"充电电压{BMSDemandVolt}(V),测输出电流", minCurrent.ToString("F2"), maxCurrent.ToString("F2"));
            ProcessDataTmp(dicCurrent, sState, "输出电流(A)", "-", "-");
            ProcessDataTmp(dicSteadyCurrent, sState, "稳流精度(%)", (-判定准则).ToString("F2"), 判定准则.ToString("F2"));
            SetLoadDCOFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        public override void ProcessData()
        {

        }
    }
}
