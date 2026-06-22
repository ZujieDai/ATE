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
    /// 国标直流  最大恒功率输出试验
    /// </summary>
    public class MaxConstantPower_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public MaxConstantPower_GB_DC(int type)
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
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            ModuleMinimumVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
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

                double[] Current = new double[3];
                double[] Voltage = new double[3];
                RatedMidVoltage = (MaxAllowChargeVoltage + ModuleMinimumVoltage) / 2;
                Current[0] = (MaxOutputPower * 1000) / MaxAllowChargeVoltage;

                Current[1] = (MaxOutputPower * 1000) / RatedMidVoltage;
                Current[2] = MaxAllowChargeCurrent;
                Current = RetainDecimals<double>(Current);
                Voltage[0] = MaxAllowChargeVoltage;
                Voltage[1] = RatedMidVoltage;
                Voltage[2] = ModuleMinimumVoltage;

                Voltage = RetainDecimals<double>(Voltage);
                Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                SetConditionValues();
                int Count = 0;
                for (int i = 0; i < 3; i++)
                {
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    if (i >= 1)
                    {
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压{0}V, 恒流模式运行", Voltage[i] + 20));
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i] + 20, 200, false, Voltage[i] + 20);
                        Thread.Sleep(3000);
                        SendNoticeToUIAndTxtFile(string.Format("设置回馈负载需求电压{0}V,需求电流{1}A，启动负载，等待带载稳定", Voltage[i], Current[i] + 20));
                        SetLoadPara(testWorkParam.lstIDs, Voltage[i], Current[i] + 20, Voltage[i], Current[i]);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(1000 * 15);
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压{0}V, 恒流模式运行", Voltage[i]));
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], 200, false, Voltage[i]);
                        SendNoticeToUIAndTxtFile(string.Format("设置回馈负载需求电压{0}V,需求电流{1}A，启动负载，等待带载稳定", Voltage[i] - 10, Current[i] + 20));
                        SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 10, Current[i] + 20, Voltage[i] - 10, Current[i]);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(1000 * 15);
                    }


                    int timeout = 35;
                    double Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    while (timeout-- > 0)
                    {
                        Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                        if (Current_4 > Current[i] * 0.9)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    Current_4 = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;

                    double OutputPower = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4Power;
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    if (Count == 2)
                    {
                        //dic.Add(LstChargerInfo[0].ChargerId, OutputPower.ToString("F2"));
                        //ProcessDataTmp(dic, "导引电压" + Voltage[i] + "V,带载电流" + (Current[i] + 20).ToString() + "A", "输出功率值(Kw)", (MaxOutputPower * 0.99).ToString("F2"), (MaxOutputPower * 1.2).ToString("F2"));
                        dic.Clear();
                        dic.Add(LstChargerInfo[0].ChargerId, Current_4.ToString("F2"));
                        ProcessDataTmp(dic, "导引电压" + Voltage[i] + "V,带载电流" + (Current[i] + 20).ToString() + "A", "输出电流值(A)", (Current[i] * 0.9).ToString("F2"), (Current[i] * 1.1).ToString("F2"));
                    }
                    else
                    {
                        dic.Clear();
                        dic.Add(LstChargerInfo[0].ChargerId, OutputPower.ToString("F2"));
                        ProcessDataTmp(dic, "导引电压" + Voltage[i] + "V,带载电流" + (Current[i] + 20).ToString() + "A", "输出功率值(Kw)", (MaxOutputPower * 0.99).ToString("F2"), (MaxOutputPower * 1.2).ToString("F2"));
                    }

                    Count++;

                }
                SetLoadDCOFF(testWorkParam.lstIDs);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
