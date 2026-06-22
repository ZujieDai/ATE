using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 企标研测直流：谐波电流试验
    /// </summary>
    public class QB_RT_DC_HarmonicsCurrent : BusinessBase
    {
        public QB_RT_DC_HarmonicsCurrent(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();

        }
        public override void InitEquiMent()
        {
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


                //关闭功率分析仪谐波检测
                ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 1, false);
                Thread.Sleep(100);
                ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 2, false);
                Thread.Sleep(100);
                ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 3, false);
                Thread.Sleep(100);

                SetCPReresh();
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

                try
                {

                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();
                    Thread.Sleep(7000);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    double[] Voltage = new double[4];
                    double[] Current = new double[4];

                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = MaxAllowChargeVoltage / 2;
                    Voltage[2] = MaxAllowChargeVoltage / 2;
                    Voltage[3] = MaxAllowChargeVoltage;
                    Voltage = RetainDecimals<double>(Voltage);

                    Current[0] = MaxOutputPower * 1000 * 0.25 / Voltage[0];
                    Current[1] = MaxOutputPower * 1000 * 0.45 / Voltage[1];
                    Current[2] = MaxOutputPower * 1000 * 0.55 / Voltage[2];
                    Current[3] = MaxOutputPower * 1000 * 0.95 / Voltage[3];
                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                    List<string> sState = new List<string>()
                    {
                        $"额定功率{Voltage[0] * Current[0] / 1000.0 / MaxOutputPower * 100:F0}%",
                        $"额定功率{Voltage[1] * Current[1] / 1000.0 / MaxOutputPower * 100:F0}%",
                        $"额定功率{Voltage[2] * Current[2] / 1000.0 / MaxOutputPower * 100:F0}%",
                        $"额定功率{Voltage[3] * Current[3] / 1000.0 / MaxOutputPower * 100:F0}%",
                    };

                    SendNoticeToUIAndTxtFile("启动充电");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //启动功率分析仪谐波检测
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 1, true);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 2, true);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 3, true);
                    Thread.Sleep(100);



                    for (int i = 0; i < Voltage.Length; i++)
                    {
                        TrialMethod(Voltage[i], Current[i], sState[i]);
                    }

                    SendNoticeToUIAndTxtFile("负载取消并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);

                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voltage">需求电压</param>
        /// <param name="current">需求电流</param>
        /// <param name="type">恒压=true，恒流=false</param>
        private void TrialMethod(double voltage, double current, string sState)
        {
            SendNoticeToUIAndTxtFile("测试点【" + sState + "】，电压【" + voltage.ToString() + "】，电流【" + current.ToString() + "】------------");
            //if (IsRLoad(voltage, current))
            {
                SetLoadDCOFF(testWorkParam.lstIDs);
                double CheckVoltage = 0, CheckCurrent = 0;

                if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState != "充电中")
                {
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                }
                Thread.Sleep(1000);

                if ((current + 20) >= MaxAllowChargeCurrent)
                {
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, MaxAllowChargeCurrent, true, voltage);
                    SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, MaxAllowChargeCurrent));
                }
                else
                {
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltage, current + 20, true, voltage);
                    SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", voltage, current + 20));
                }
                WaitDCVoltage(testWorkParam.lstIDs, voltage);
                Thread.Sleep(3000);

                if (current == MaxAllowChargeCurrent)
                {
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current - 5));
                    else if (ControlEquipMent.ResistanceLoad != null)
                        SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current - 5));
                    SetLoadPara(testWorkParam.lstIDs, voltage - 20, current - 5, voltage - 5, current - 5);
                    CheckCurrent = current - 5;
                }
                else
                {
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                        SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 20, current));
                    else if (ControlEquipMent.ResistanceLoad != null)
                        SendNoticeToUIAndTxtFile(string.Format("设置负载需求电压为{0}，充电电流为{1}", voltage - 5, current));
                    SetLoadPara(testWorkParam.lstIDs, voltage - 20, current, voltage - 5, current);
                    CheckCurrent = current;
                }
                Thread.Sleep(300);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, CheckCurrent);
                Thread.Sleep(2000);


                Stopwatch st = new Stopwatch();
                st.Restart();
                while (st.ElapsedMilliseconds / 1000 < 35)
                {
                    double Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    if (Current_4 > CheckCurrent * 0.9)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                Thread.Sleep(3000);//等待功率分析仪计算数据

                SendNoticeToUIAndTxtFile("读取谐波电流含有率中...");
                double CurrentHarmonic = ControlEquipMent.PowerAnalyzer.ReadCurrentHarmonic_Total(testWorkParam.lstIDs,1);
                for (int j = 0; j < 10; j++)
                {
                    if (CurrentHarmonic < 0 || CurrentHarmonic > 12)
                    {
                        Thread.Sleep(500);
                        CurrentHarmonic = ControlEquipMent.PowerAnalyzer.ReadCurrentHarmonic_Total(testWorkParam.lstIDs, 1);
                    }
                    else
                    {
                        break;
                    }
                }

                Dictionary<int, string> dic = new Dictionary<int, string>();
                if (voltage * current / 1000.0 / MaxOutputPower <= 0.5)
                {
                    dic.Add(LstChargerInfo[0].ChargerId, CurrentHarmonic.ToString("F3"));
                    ProcessDataTmp(dic, sState, "谐波电流含有率(%)", "0", "12");
                }
                else
                {
                    dic.Add(LstChargerInfo[0].ChargerId, CurrentHarmonic.ToString("F3"));
                    ProcessDataTmp(dic, sState, "谐波电流含有率(%)", "0", "5");
                }
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
                //SetLoadDCOFF(testWorkParam.lstIDs);

                Thread.Sleep(1000);
            }
        }

        public override void ProcessData()
        {

        }
    }
}
