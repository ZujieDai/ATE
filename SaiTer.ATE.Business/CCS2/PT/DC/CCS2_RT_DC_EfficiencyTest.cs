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
    public class CCS2_RT_DC_EfficiencyTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double RealPower = 0;
        double Rate = 0;
        double efficiency = 0;
        double powerfactor = 0;

        public CCS2_RT_DC_EfficiencyTest(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            //BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            //ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            //RatedMinVoltage = MaxOutputPower * 1000 / MaxAllowChargeCurrent;
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
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
                    double[] Current = new double[3];

                    Current[0] = MaxOutputPower * 1000.0 / MaxAllowChargeVoltage;
                    Current[1] = 0.5f * MaxOutputPower * 1000.0 / MaxAllowChargeVoltage;
                    Current[2] = 0.2f * MaxOutputPower * 1000.0 / MaxAllowChargeVoltage;
                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);


                    string efficiencyLimit = "94";
                    for (int i = 0; i < Current.Length; i++)
                    {
                        if (MaxAllowChargeVoltage * Current[i] < 0.5f * MaxOutputPower * 1000f)
                            efficiencyLimit = "92";
                        else
                            efficiencyLimit = "94";
                        TrialMethod(MaxAllowChargeVoltage, Current[i], efficiencyLimit);
                    }

                    SendNoticeToUIAndTxtFile("负载取消并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                }
            }

            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        private void TrialMethod(double voltage, double current, string efficiencyLimit)
        {
            string itemFlow = "效率测试";

            if (ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState) < 20)
            {
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
            }

            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, current, false, voltage);
            WaitDCVoltage_EU_DC(testWorkParam.lstIDs, MaxAllowChargeVoltage);
            Thread.Sleep(3000);

            SetLoadPara(testWorkParam.lstIDs, voltage - 20, current, voltage, current);
            Thread.Sleep(500);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrent_EU_DC(testWorkParam.lstIDs, current);
            Thread.Sleep(3000);

            //float totalPower = common.STPA_ST8804_GetCommonDataByID(viPADevID, 1, 3) / 1000.0f;//ΣA组三相总功率
            //float RealPower = common.STPA_Get_ChannelDataByID(viPADevID, 4, 3) / 1000.0f;// 通道4 功率
            //float efficiency = Convert.ToSingle(Math.Round((RealPower / totalPower) * 100, 3));
            Dictionary<int, string> listTotal = new Dictionary<int, string>();
            Dictionary<int, string> listReal = new Dictionary<int, string>();
            Dictionary<int, string> listEfficiency = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                double TotalPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel1Power / 1000f
                    + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel2Power / 1000f
                    + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel3Power / 1000f;
                double RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power / 1000f;
                double efficiency = Convert.ToSingle(Math.Round((RealPower / TotalPower) * 100, 3));
                int count = 100;
                while (count-- > 0)
                {
                    if (efficiency > Convert.ToSingle(efficiencyLimit) && efficiency < 100)
                        break;
                    //双枪同测有输出没输入
                    //if ((RealPower * 1000f > voltage * current * 0.5f && TotalPower <= 1) || (efficiency >= Convert.ToSingle(efficiencyLimit) / 2 && efficiency <= 100))
                    //{
                    //    Thread.Sleep(500);
                    //    TotalPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel1Power / 1000f
                    //        + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel2Power / 1000f
                    //        + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel3Power / 1000f;
                    //    RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power / 1000f;
                    //    efficiency = Convert.ToSingle(Math.Round((RealPower / TotalPower) * 100, 3));
                    //    if ((RealPower * 1000f > voltage * current * 0.5f && TotalPower <= 1) || (efficiency >= Convert.ToSingle(efficiencyLimit) / 2 && efficiency <= 100))
                    //        break;
                    //}
                    Thread.Sleep(100);
                    TotalPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel1Power / 1000f
                        + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel2Power / 1000f
                        + AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel3Power / 1000f;
                    RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power / 1000f;
                    efficiency = Convert.ToSingle(Math.Round((RealPower / TotalPower) * 100, 3));
                }
                listTotal.Add(item, TotalPower.ToString("F3"));
                listReal.Add(item, RealPower.ToString("F3"));
                listEfficiency.Add(item, efficiency.ToString("F3"));
            }
            //如果是双枪同测输入只有一个，输入对应的是双枪输出的总和
            //if (testWorkParam.lstIDs.Count == 2 && listEfficiency.Select(e => e >= 0 && e < 5).ToList().Count > 0)
            //{
            //    float TotalPower = listTotal.Max();
            //    float RealPower = listReal.Sum();
            //    float efficiency = Convert.ToSingle(Math.Round((RealPower / TotalPower) * 100, 3));
            //    listTotal = new List<float>() { TotalPower };
            //    listReal = new List<float>() { RealPower };
            //    listEfficiency = new List<float>() { efficiency };
            //}

            ProcessDataTmp(listTotal, itemFlow, "需求电压" + voltage + "V,需求电流" + current + "A,输入功率(kW)", "-", "-");
            ProcessDataTmp(listReal, itemFlow, "需求电压" + voltage + "V,需求电流" + current + "A,输出功率(kW)", "-", "-");
            ProcessDataTmp(listEfficiency, itemFlow, "需求电压" + voltage + "V,需求电流" + current + "A,效率(η)", efficiencyLimit, "100");

            SetLoadDCOFF(testWorkParam.lstIDs);
            Thread.Sleep(3000);

        }

        public override void ProcessData()
        {

        }

    }
}
