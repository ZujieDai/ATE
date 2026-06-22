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
    /// 国标产测直流：稳流精度
    /// </summary>
    public class GB_PT_DC_SteadyCurrentPrecision : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        //double 负载最小电流设置 = 0;
        //double 负载最大电流设置 = 0;
        double 判定准则 = 0;
        double InputVoltDiff = 0;
        double Iz = 500;    //中电压实测输出电流
        public GB_PT_DC_SteadyCurrentPrecision(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            //BMS需求电压最小值(V)=400|BMS需求电压最大值(V)=745|负载电流设置(A)=40|判定准则(±%)=0.5|输入电压偏移修正(%)=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            //负载最大电流设置 = Convert.ToDouble(strParams[0].Split('=')[1]);//40
            //负载最小电流设置 = Convert.ToDouble(strParams[1].Split('=')[1]);//70
            判定准则 = Convert.ToDouble(strParams[0].Split('=')[1]);//(±1%)
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 4)
            {
                InputVoltDiff = Convert.ToDouble(strParams[3].Split('=')[1]) / 100;
            }
        }

        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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

                List<double> BMSVolts = new List<double>() { MidAllowChargeVoltage, MinAllowChargeVoltage, MaxAllowChargeVoltage };
                List<double> BMSCurrents = new List<double>() { 0.5 * RatedCurrent, 0.2 * RatedCurrent, RatedCurrent };
                List<double> ACSourceRate = new List<double>() { 1, 0.85, 1.15 };
                //小功率交流源不能做大功率测试的输入
                if(Customer != null && Customer.Equals("ZLX"))
                {
                    ACSourceRate = new List<double>() { 1 };
                }
                for (int i = 0; i < BMSCurrents.Count; i++)
                {
                    int rateInC = 50;
                    if (i == 1) rateInC = 20;
                    else if (i == 2) rateInC = 100;
                    //ProcessDataResult(testWorkParam.lstIDs, "-", BMSCurrents[i].ToString(), true, $"设定充电机输出电流--{rateInC}%In");
                    d1 = new Dictionary<int, string>();
                    var dicResult = new Dictionary<int, EmTrialResult>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, "-");
                        dicResult.Add(item, EmTrialResult.NA);
                    }
                    ProcessDataResults(testWorkParam.lstIDs, d1, BMSCurrents[i].ToString(), dicResult, $"设定充电机输出电流--{rateInC}%In");
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
            }

        }

        private void TrialMethod(double BMSDemandVolt, double rate, double current, bool isMid)
        {
            string sState = "额定输入电压" + rate * 100 + "%";
            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstIDs[0]].ChargingState;
            if (!BMSInfo.Contains("充电中"))
            {
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    //ProcessDataResult(testWorkParam.lstIDs, "-", "无法启动充电", false, $"设置BMS电压{LstChargerInfo[0].NominalVoltage}V,电流250A");
                    return;
                }
            }

            double bmsVolt = BMSDemandVolt + 20 >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSDemandVolt + 20;
            //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, 220 * rate);
            if (Customer != null && !Customer.Equals("ZLX"))
            {
                SetACSource(testWorkParam.lstIDs, 220 * rate);
                Thread.Sleep(2 * 1000);
            }
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, bmsVolt, current, false, bmsVolt);
            SendNoticeToUIAndTxtFile(string.Format("设置BMS需求电压{0}V,需求电流{1}A,恒流模式。启动负载，并等待带载电流稳定", bmsVolt, current));
            WaitDCVoltage(testWorkParam.lstIDs, bmsVolt);
            Thread.Sleep(3 * 1000);

            if (BMSDemandVolt + 20 >= MaxAllowChargeVoltage)
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 10, current + 20, BMSDemandVolt - 10, current + 5);
            else
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt, current + 20, BMSDemandVolt, current + 5);
            Thread.Sleep(500);
            SetLoadDCON(testWorkParam.lstIDs);
            //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
            WaitDCCurrent(testWorkParam.lstIDs, current);   //恒流模式电流不会拉高
            Thread.Sleep(3 * 1000);

            //ProcessDataResult(testWorkParam.lstIDs, bmsVolt.ToString(), "需求电压(V)", true, sState);
            d1 = new Dictionary<int, string>();
            var dicResult = new Dictionary<int, EmTrialResult>();
            foreach (int item in testWorkParam.lstIDs)
            {
                d1.Add(item, bmsVolt.ToString());
                dicResult.Add(item, EmTrialResult.NA);
            }
            ProcessDataResults(testWorkParam.lstIDs, d1, "需求电压(V)", dicResult, sState);
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
            Dictionary<int, string> dicSteadyCurrent = new Dictionary<int, string>();  //稳压精度
            Dictionary<int, string> dicCurrent = new Dictionary<int, string>();    //输出电流
            foreach (var item in testWorkParam.lstIDs)
            {
                var Im = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                int count = 60;
                while (count-- > 0)
                {
                    if (Im < minCurrent || Im > maxCurrent)
                    {
                        Im = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }

                if (isMid) Iz = Im;
                dicCurrent.Add(item, Im.ToString("F2"));
                double steadyCurrentRate = (Im - Iz) / Iz * 100;     //稳流精度
                dicSteadyCurrent.Add(item, steadyCurrentRate.ToString("F2"));
            }
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
