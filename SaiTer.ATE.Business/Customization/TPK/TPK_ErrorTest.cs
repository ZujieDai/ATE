using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_ErrorTest : BusinessBase  //误差测试 单枪测试
    {
        public TPK_ErrorTest(int trialType) { TrialType = trialType; }

        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError, EnergyError;


        public override void InitializeParams()
        {
            Init();
            //充电电压(V)=750|充电电流(A)=20|老化时间(分)=3|监测数据间隔时间(秒)=20|电压误差(%)=5|电流误差(%)=5|电量误差(%)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            AgingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            AgingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            AgingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            IntervalTime = Convert.ToDouble(strParams[3].Split('=')[1]);
            VoltError = Convert.ToDouble(strParams[4].Split('=')[1]);
            CurrError = Convert.ToDouble(strParams[5].Split('=')[1]);
            EnergyError = Convert.ToDouble(strParams[6].Split('=')[1]);
        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                byte[] x = new byte[0];
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // ControlEquipMent.FeedbackLoad.FeedbackLoad_NoParallel(lstIDs);
                // SetCPReresh();
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭负载");
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }
        /// <summary>
        /// 测试流程
        /// </summary>
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();

                //  ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);

                AgingVolt = AgingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt;
                if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr, MaxAllowChargeVoltage, false))
                {
                    return;
                }
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, 200, true, AgingVolt);
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile(string.Format("设置BMS电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                if (AgingVolt == MaxAllowChargeVoltage)
                    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 10, AgingCurr, AgingVolt - 5, AgingCurr);  // 原AgingCurr + 20
                else
                    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 20, AgingCurr, AgingVolt - 20, AgingCurr); // 原AgingCurr + 20
                Thread.Sleep(1000);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrentWithTime(testWorkParam.lstIDs, AgingCurr, 40);
                Thread.Sleep(1000 * 5);

                Dictionary<int, string> dicAgingEnergy = new Dictionary<int, string>();   //如何获取 电表数据
                ControlEquipMent.BMS.BMSClearEnergy(testWorkParam.lstIDs);
                Thread.Sleep(500);
                double BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);


                Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    if (voltage == 0)
                    {
                        Thread.Sleep(500);
                        voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    }
                    dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                    double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                    if (current == 0)
                    {
                        Thread.Sleep(500);
                        current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                    }
                    dicAgingCurr.Add(itmp, current.ToString("F2"));

                  
                    if (BMSEnergy == 0)
                    {
                        Thread.Sleep(500);
                        BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);
                    }
                    dicAgingEnergy.Add(itmp, BMSEnergy.ToString("F2"));



                }
                ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", AgingVolt), "1秒时桩实际电压(V)", ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", AgingCurr), "1秒时桩实际电流(V)", ((1.0 - CurrError / 100) * AgingCurr).ToString(), ((1.0 + CurrError / 100) * AgingCurr).ToString());
                ProcessDataTmp(dicAgingEnergy, string.Format("电量{0}KwH", BMSEnergy.ToString("F2")), string.Format("{0}秒时桩实际电量(KwH)", AgingTime), ((1.0 - EnergyError / 100) * BMSEnergy).ToString("F2"), ((1.0 + EnergyError / 100) * BMSEnergy).ToString("F2"));





                Stopwatch sw = new Stopwatch();
                sw.Start();
                int count = 0;
                while (sw.ElapsedMilliseconds / 1000 <= AgingTime * 60)
                {
                    dicAgingVolt.Clear();
                    dicAgingCurr.Clear();
                    dicAgingEnergy.Clear();

                    Thread.Sleep(Convert.ToInt32(IntervalTime * 1000));
                    count++;
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));

                        BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);
                        if (BMSEnergy == 0)
                        {
                            Thread.Sleep(500);
                            BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);
                        }
                        dicAgingEnergy.Add(itmp, BMSEnergy.ToString("F2"));
                    }
                    int time = count * Convert.ToInt32(IntervalTime) + 1;
                    ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", AgingVolt), string.Format("{0}秒时桩实际电压(V)", time), ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                    ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", AgingCurr), string.Format("{0}秒时桩实际电流(A)", time), ((1.0 - CurrError / 100) * AgingCurr).ToString(), ((1.0 + CurrError / 100) * AgingCurr).ToString());
                    ProcessDataTmp(dicAgingEnergy, string.Format("电量{0}KwH", BMSEnergy.ToString("F2")), string.Format("{0}秒时桩实际电量(KwH)", time), ((1.0 - EnergyError / 100) * BMSEnergy).ToString("F2"), ((1.0 + EnergyError / 100) * BMSEnergy).ToString("F2"));
                }

            }
        }
        public override void ProcessData()
        {

        }
    }
}
