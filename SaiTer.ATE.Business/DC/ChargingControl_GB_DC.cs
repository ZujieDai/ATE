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
    /// 国标直流:充电控制功能试验、通信功能试验
    /// </summary>
    public class ChargingControl_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public ChargingControl_GB_DC(int type)
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
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent / 2 >= 30 ? LstChargerInfo[0].NominalCurrent / 2 : 30;
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
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

                    //BMSDemandVolt = BMSDemandVolt > LstChargerInfo[0].NominalVoltage * 0.5 ? LstChargerInfo[0].NominalVoltage * 0.5 : BMSDemandVolt;
                    if (BMSDemandVolt > 500)
                        BMSDemandVolt = 600;
                    else
                        BMSDemandVolt = BMSDemandVolt / 2;
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, MaxAllowChargeVoltage, false))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    double canVolt = IsResistanceLoad() ? BMSDemandVolt - 5 : BMSDemandVolt - 20;
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
                    //WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(1000 * 3);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 10, BMSDemandVolt - 5, ResiLoadCurrent);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int time = 10;
                        while (time-- > 0)
                        {
                            if (current >= ResiLoadCurrent * 0.9 && current <= ResiLoadCurrent * 1.1)
                                break;
                            Thread.Sleep(300);
                            current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电流(A)", (ResiLoadCurrent * 0.9).ToString("F2"), (ResiLoadCurrent * 1.1).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int time = 10;
                        while(time-- > 0)
                        {
                            if (voltage >= canVolt * 0.9 && voltage <= canVolt * 1.1)
                                break;
                            Thread.Sleep(300);
                            voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        }
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电压(V)", (canVolt * 0.9).ToString("F2"), (canVolt * 1.1).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    WaitDCLoadOFF(testWorkParam.lstIDs, 5);
                    Thread.Sleep(3 * 1000);

                    //控制变化
                    //BMSDemandVolt = BMSDemandVolt * 0.5;
                    //有点桩改变需求电压小于当前充电机电压会直接过压保护，例如江门AK
                    //BMSDemandVolt = BMSDemandVolt + 250 > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSDemandVolt + 250;
                    if (BMSDemandVolt > 500)
                        BMSDemandVolt = 750;
                    else
                        BMSDemandVolt = BMSDemandVolt * 2;
                    ResiLoadCurrent = MaxOutputPower * 1000.0 / BMSDemandVolt > 45 ? 45 : Math.Round(MaxOutputPower * 1000.0 * 0.95 / BMSDemandVolt);

                    canVolt = IsResistanceLoad() ? BMSDemandVolt - 5 : BMSDemandVolt - 20;
                    canVolt = BMSDemandVolt;
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, false, BMSDemandVolt);
                    WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(1000 * 3);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 10, BMSDemandVolt - 5, ResiLoadCurrent);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int time = 10;
                        while (time-- > 0)
                        {
                            if (current >= ResiLoadCurrent * 0.9 && current <= ResiLoadCurrent * 1.1)
                                break;
                            Thread.Sleep(300);
                            current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        }
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电流(A)", (ResiLoadCurrent * 0.9).ToString("F2"), (ResiLoadCurrent * 1.1).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int time = 10;
                        while (time-- > 0)
                        {
                            if (voltage >= canVolt && voltage <= canVolt)
                                break;
                            Thread.Sleep(300);
                            voltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        }
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电压(V)", (canVolt * 0.9).ToString("F2"), (canVolt * 1.1).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);

                    if (TrialType == (int)EmTrialType.充电控制)
                    {
                        CountDownTimeInfo("请人工检查充电机应按照制造商声明的方式手动设定充电参数，并实施充电启停操作。\r\n注：勾选上为可以正常解锁", 20, 2);
                        //ProcessDataConnect("应能手动设定充电参数，并实施充电启停操作", "是否具备");
                        if (DicManualVerifyResult.First().Value)
                            ProcessDataResult(testWorkParam.lstIDs, "是", "应能手动设定充电参数，并实施充电启停操作", true, "是否具备");
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "否", "应能手动设定充电参数，并实施充电启停操作", false, "是否具备");
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
