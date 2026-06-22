using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class GB_PT_DC_InsulationDetection : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public GB_PT_DC_InsulationDetection(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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
                var kstate = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(300);
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //添加测试条件
                    SetConditionValues();
                    //d1 = new Dictionary<int, string>();
                    //foreach (int item in testWorkParam.lstIDs)
                    //{
                    //    d1.Add(item, LstChargerInfo[0].NominalVoltage.ToString("F2"));
                    //}
                    //SetConditionValue("BMS需求电压(V)", d1);

                    #region 在绝缘检测前，模拟 K1 和 K2 外侧电压绝对值＞10 V，检查充电机应停止绝缘检测过程，并发出告警提示
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 100);
                    Thread.Sleep(1000);
                    //过压控制开关
                    var kstate = GetKStatus16_Charging_DC();
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(3000);

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        //外侧电压具体是模拟过压后的导引充电电压
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    //SetConditionValue("K1K2外侧电压(V)", d1);
                    ProcessDataTmp(d1, "绝缘前模拟外侧电压>10V", "绝缘检测前外侧电压(V)", "-", "-");


                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        SendNoticeToUIAndTxtFile($"等待刷卡(枪{testWorkParam.lstIDs[i]})");
                        int timeout = 100;
                        MessgaeInfo(true, $"请刷卡充电!(枪{testWorkParam.lstIDs[i]})", true);
                        while (timeout-- > 0)
                        {
                            if (ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState) > 4)
                            {
                                break;
                            }

                            System.Threading.Thread.Sleep(1000);
                        }
                        MessgaeInfo(false, "");
                    }

                    d1 = new Dictionary<int, string>();
                    var dicResult = new Dictionary<int, EmTrialResult>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                        dicResult.Add(item, ChangeBMSChargeStatus(d1[item]) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    }
                    ProcessDataResults(testWorkParam.lstIDs, d1, "充电状态", dicResult, "绝缘前模拟外侧电压>10V");

                    CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
                    d1 = new Dictionary<int, string>();
                    dicResult = new Dictionary<int, EmTrialResult>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, DicManualVerifyResult[item] ? "有告警" : "未告警");
                        dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                    }
                    ProcessDataResults(testWorkParam.lstIDs, d1, "应发出告警提示", dicResult, "绝缘前模拟外侧电压>10V");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    //过压控制开关
                    kstate = GetKStatus16_Charging_DC();
                    kstate[26] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(100);
                    #endregion

                    bool isCharger2015 = false;
                    string[] ChargerNum = ConfigurationManager.AppSettings["ChargerNum_2015"] != null ?
                        ConfigurationManager.AppSettings["ChargerNum_2015"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
                    if (ChargerNum.Contains(testWorkParam.lstIDs.FirstOrDefault().ToString()))
                    {
                        isCharger2015 = true;
                    }

                    //根据额定电压选择档位（100Ω/V <= 绝缘电阻/充电电压 <= 500Ω/V）
                    int KStatus1 = 0, KStatus2 = 0;
                    string resistanceVal = "";
                    if (isCharger2015)
                    {
                        if (600000 / BMSDemandVolt <= 500 * 0.95 && 600000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 7;
                            KStatus2 = 15;
                            resistanceVal = "600KΩ";
                        }
                        else if (300000 / BMSDemandVolt <= 500 * 0.95 && 300000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 6;
                            KStatus2 = 14;
                            resistanceVal = "300KΩ";
                        }
                        else if (200000 / BMSDemandVolt <= 500 * 0.95 && 200000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 5;
                            KStatus2 = 13;
                            resistanceVal = "200KΩ";
                        }
                        else if (100000 / BMSDemandVolt <= 500 * 0.95 && 100000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 4;
                            KStatus2 = 12;
                            resistanceVal = "100KΩ";
                        }
                        else if (75000 / BMSDemandVolt <= 500 * 0.95 && 75000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 3;
                            KStatus2 = 11;
                            resistanceVal = "75KΩ";
                        }
                        else if (20000 / BMSDemandVolt <= 500 * 0.95 && 20000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 2;
                            KStatus2 = 10;
                            resistanceVal = "20KΩ";
                        }
                        else if (15300 / BMSDemandVolt <= 500 * 0.95 && 15300 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 1;
                            KStatus2 = 9;
                            resistanceVal = "15.3KΩ";
                        }
                        else
                        {
                            d1 = new Dictionary<int, string>();
                            dicResult = new Dictionary<int, EmTrialResult>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, "未找到合适档位");
                                dicResult.Add(item, EmTrialResult.NA);
                            }
                            ProcessDataResults(testWorkParam.lstIDs, d1, "应发出告警提示", dicResult, "绝缘前模拟外侧电压>10V");
                            return;
                        }
                    }
                    else
                    {
                        if (300000 / BMSDemandVolt <= 500 * 0.95 && 300000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 7;
                            KStatus2 = 15;
                            resistanceVal = "300KΩ";
                        }
                        else if (100000 / BMSDemandVolt <= 500 * 0.95 && 100000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 6;
                            KStatus2 = 14;
                            resistanceVal = "100KΩ";
                        }
                        else if (75000 / BMSDemandVolt <= 500 * 0.95 && 75000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 5;
                            KStatus2 = 13;
                            resistanceVal = "75KΩ";
                        }
                        else if (33000 / BMSDemandVolt <= 500 * 0.95 && 33000 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 4;
                            KStatus2 = 12;
                            resistanceVal = "33KΩ";
                        }
                        else if (29700 / BMSDemandVolt <= 500 * 0.95 && 29700 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 3;
                            KStatus2 = 11;
                            resistanceVal = "29.7KΩ";
                        }
                        else if (24800 / BMSDemandVolt <= 500 * 0.95 && 24800 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 2;
                            KStatus2 = 10;
                            resistanceVal = "24.8KΩ";
                        }
                        else if (22900 / BMSDemandVolt <= 500 * 0.95 && 22900 / BMSDemandVolt >= 100 * 1.05)
                        {
                            KStatus1 = 1;
                            KStatus2 = 9;
                            resistanceVal = "22.9KΩ";
                        }
                        else
                        {
                            d1 = new Dictionary<int, string>();
                            dicResult = new Dictionary<int, EmTrialResult>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, "未找到合适档位");
                                dicResult.Add(item, EmTrialResult.NA);
                            }
                            ProcessDataResults(testWorkParam.lstIDs, d1, "应发出告警提示", dicResult, "绝缘前模拟外侧电压>10V");
                            return;
                        }
                    }

                    SendNoticeToUIAndTxtFile($"设置DC+非对称漏电{resistanceVal}");
                    List<bool> lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[KStatus1] = true;//DC+非对称漏电300K
                                               //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod($"DC+非对称漏电{resistanceVal}", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile("设置DC-非对称漏电300KΩ");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[KStatus2] = true;//DC-非对称漏电300K
                                               //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod($"DC-非对称漏电{resistanceVal}", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile($"设置DC-、DC+对称漏电{resistanceVal}");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[KStatus1] = true;//DC+非对称漏电300K
                    lstKState[8] = false;
                    lstKState[KStatus2] = true;//DC-非对称漏电300K
                                               //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod($"DC+、DC-对称漏电{resistanceVal}", "应能充电", "应能告警", lstKState);

                    //根据额定电压选择档位（绝缘电阻/充电电压 <= 100Ω/V）
                    if (isCharger2015)
                    {
                        if (600000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 7;
                            KStatus2 = 15;
                            resistanceVal = "600KΩ";
                        }
                        else if (300000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 6;
                            KStatus2 = 14;
                            resistanceVal = "300KΩ";
                        }
                        else if (200000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 5;
                            KStatus2 = 13;
                            resistanceVal = "200KΩ";
                        }
                        else if (100000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 4;
                            KStatus2 = 12;
                            resistanceVal = "100KΩ";
                        }
                        else if (75000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 3;
                            KStatus2 = 11;
                            resistanceVal = "75KΩ";
                        }
                        else if (20000 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 2;
                            KStatus2 = 10;
                            resistanceVal = "20KΩ";
                        }
                        else if (15300 / BMSDemandVolt <= 100 * 1.05)
                        {
                            KStatus1 = 1;
                            KStatus2 = 9;
                            resistanceVal = "15.3KΩ";
                        }
                        else
                        {
                            d1 = new Dictionary<int, string>();
                            dicResult = new Dictionary<int, EmTrialResult>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, "未找到合适档位");
                                dicResult.Add(item, EmTrialResult.NA);
                            }
                            ProcessDataResults(testWorkParam.lstIDs, d1, "应发出告警提示", dicResult, "绝缘前模拟外侧电压>10V");
                            return;
                        }
                    }
                    else
                    {
                        if (300000 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 7;
                            KStatus2 = 15;
                            resistanceVal = "300KΩ";
                        }
                        else if (100000 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 6;
                            KStatus2 = 14;
                            resistanceVal = "100KΩ";
                        }
                        else if (75000 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 5;
                            KStatus2 = 13;
                            resistanceVal = "75KΩ";
                        }
                        else if (33000 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 4;
                            KStatus2 = 12;
                            resistanceVal = "33KΩ";
                        }
                        else if (29700 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 3;
                            KStatus2 = 11;
                            resistanceVal = "29.7KΩ";
                        }
                        else if (24800 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 2;
                            KStatus2 = 10;
                            resistanceVal = "24.8KΩ";
                        }
                        else if (22900 / BMSDemandVolt <= 100 * 0.95)
                        {
                            KStatus1 = 1;
                            KStatus2 = 9;
                            resistanceVal = "22.9KΩ";
                        }
                        else
                        {
                            d1 = new Dictionary<int, string>();
                            dicResult = new Dictionary<int, EmTrialResult>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, "未找到合适档位");
                                dicResult.Add(item, EmTrialResult.NA);
                            }
                            ProcessDataResults(testWorkParam.lstIDs, d1, "应发出告警提示", dicResult, "绝缘前模拟外侧电压>10V");
                            return;
                        }
                    }

                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[KStatus1] = true;//DC+非对称漏电33K
                    TrialMethod($"DC+非对称漏电{resistanceVal}", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[KStatus2] = true;//DC-非对称漏电33K
                    TrialMethod($"DC-非对称漏电{resistanceVal}", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[KStatus1] = true;//DC+非对称漏电33K
                    lstKState[8] = false;
                    lstKState[KStatus2] = true;//DC-非对称漏电33K
                    TrialMethod($"DC+、DC-对称漏电{resistanceVal}", "应不能充电", "应能告警", lstKState, false);
                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        private void TrialMethod(string sState, string ItemName1, string ItemName2, List<bool> lstKState, bool CanCharge = true)
        {
            SetCPReresh();  // 模拟插拔枪
            //if (!CheckSwipingCard(testWorkParam.lstIDs))
            //{
            //    return;
            //}
            SendNoticeToUIAndTxtFile("开始设置充电参数");
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstKState.ToArray());
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                double insulationVolt = 0;
                int timeout = 300;
                MessgaeInfo(true, $"请刷卡充电!(枪{testWorkParam.lstIDs[i]})", true);
                while (timeout-- > 0)
                {
                    int chargingState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState);
                    bool ALLCanCharge = chargingState >= 2 && chargingState < 9;
                    if (ALLCanCharge)
                    {
                        insulationVolt = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                MessgaeInfo(false, "");

                // 等待进入充电状态
                MessgaeInfo(true, $"枪{testWorkParam.lstIDs[i]}等待充电中...", true);
                timeout = 600;
                while (timeout-- > 0)
                {
                    int bmsState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState);
                    if (bmsState <= 5)
                    {
                        double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                        insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                    }
                    bool ALLCanCharge = bmsState == 9 || bmsState == 13 || bmsState == 0;  //进入充电中或充电结束阶段
                    if (ALLCanCharge)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }
                MessgaeInfo(false, "");
                //等待泄放
                Thread.Sleep(2000);

                d1 = new Dictionary<int, string>();
                d1.Add(testWorkParam.lstIDs[i], LstChargerInfo[0].NominalVoltage.ToString());
                ProcessDataTmp(d1, sState, "车辆通信握手报文的最高允许充电电压(V)", "-", "-");

                d1 = new Dictionary<int, string>();
                d1.Add(testWorkParam.lstIDs[i], insulationVolt.ToString());
                //绝缘检测电压应符合 GB/T 18487.1—2015 中 B.3.3 的规定
                ProcessDataTmp(d1, sState, "绝缘电压(V)", (LstChargerInfo[0].NominalVoltage * 0.99).ToString("F2"), (LstChargerInfo[0].NominalVoltage * 1.01).ToString("F2"));
            }

            d1 = new Dictionary<int, string>();
            d2 = new Dictionary<int, string>();
            var dicResult = new Dictionary<int, EmTrialResult>();
            var dicVolt = new Dictionary<int, string>();
            if (CanCharge)
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    int timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage < LstChargerInfo[0].NominalVoltage * 0.9 || voltage > LstChargerInfo[0].NominalVoltage * 1.1)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                        else
                            break;
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "-", "-");
                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                    d1.Add(item, state);
                    d2.Add(item, state == "充电中" ? "能充电" : "不能充电");
                    dicResult.Add(item, state == "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d2, ItemName1, dicResult, sState);
            }
            else
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    int timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage > 20)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "0", "20");
                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                    d1.Add(item, state);
                    d2.Add(item, state == "充电中" ? "能充电" : "不能充电");
                    dicResult.Add(item, state != "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d2, ItemName1, dicResult, sState);
            }


            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
            d1.Clear();
            d1 = new Dictionary<int, string>();
            dicResult = new Dictionary<int, EmTrialResult>();
            foreach (int item in testWorkParam.lstIDs)
            {
                d1.Add(item, DicManualVerifyResult[item] ? "有告警" : "未告警");
                dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
            }
            ProcessDataResults(testWorkParam.lstIDs, d1, ItemName2, dicResult, sState);

            List<bool> lstK = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstK.ToArray());
            Thread.Sleep(300);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        //private void ProcessDataResult(string CheckState, string ItemName, string strResult, EmTrialResult trialResult)
        //{

        //    LstTrialData[0].BarCode = LstChargerInfo[0].BarCode;
        //    LstTrialData[0].TrialName = TrialItem.ItemName;
        //    LstTrialData[0].SchemeName = TrialItem.SchemeName;
        //    LstTrialData[0].SchemeID = TrialItem.SchemeID;
        //    LstTrialData[0].ItemName = iIndex.ToString();
        //    LstTrialData[0].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[0].ChargerId].ChargingVoltage;

        //    LstTrialData[0].TrialResult = trialResult;


        //    LstTrialData[0].PKID = LstChargerInfo[0].PKID;
        //    //界面展示的数据项格式
        //    //状态|测试结果     
        //    LstTrialData[0].ExtentData = CheckState + "|" + ItemName + "|-|-|" + strResult + "|" + "报表(勿删)";
        //    LstTrialData[0].Data2 = LstTrialData[0].ExtentData;
        //    LstTrialData[0].Data3 = TrialItem.TrialOrder.ToString();
        //    SendTrialDataToUI(LstTrialData[0]);
        //    SaveTrialData(LstTrialData[0]);
        //    iIndex++;

        //}
        public override void ProcessData()
        {

        }
    }
}
