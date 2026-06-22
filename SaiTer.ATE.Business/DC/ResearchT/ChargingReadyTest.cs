using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using SaiTer.ATE.DataModel;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电准备就绪测试
    /// </summary>
    public class ChargingReadyTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        double BatteryVolt = 300;
        double ErrorBatteryVoltage = 300;
        Dictionary<int, string> CROValue2;

        public ChargingReadyTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // BMS需求电压设置(V)=750|BMS需求电流设置(A)=20|电池电压(V)=300|BCP当前电池电压(V)=315
            // BMS需求电压设置(V)=750|BMS需求电流设置(A)=20|电池电压(V)=300|BCP当前电池电压(V)=285
            // BMS需求电压设置(V)=750|BMS需求电流设置(A)=20|电池电压(V)=300|BCP当前电池电压(V)=380
            // BMS需求电压设置(V)=750|BMS需求电流设置(A)=20|电池电压(V)=300|BCP当前电池电压(V)=200
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 3)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                BatteryVolt = double.Parse(strParams[2].Split('=')[1]);
            }
            if (strParams.Length >= 4)
            {
                ErrorBatteryVoltage = double.Parse(strParams[3].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("Y");
            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[11] = true;//CAN12通道
            canchannelopen[12] = true;//CAN13通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            // 模拟插拔枪
            SetCPReresh();
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
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    SendNoticeToUIAndTxtFile("设置导引参数中...");
                    StorageBatteryVoltage = ErrorBatteryVoltage;
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
                    System.Threading.Thread.Sleep(500);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, StorageBatteryVoltage, MaxAllowChargeVoltage, MaxAllowChargeCurrent);
                    System.Threading.Thread.Sleep(500);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, ChargeVoltageMeasure);
                    System.Threading.Thread.Sleep(500);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    System.Threading.Thread.Sleep(500);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVolt);
                    System.Threading.Thread.Sleep(500);
                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[27] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVolt, Ks.ToArray());
                    System.Threading.Thread.Sleep(500);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    System.Threading.Thread.Sleep(500);
                    Ks[27] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVolt, Ks.ToArray());

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++) 
                    {
                        d1.Add(testWorkParam.lstIDs[i], ErrorBatteryVoltage.ToString());
                    }
                    ProcessDataTmp(d1, TrialItem.ItemName, "BCP电池电压(V)", "-", "-");
                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], BatteryVolt.ToString());
                    }
                    ProcessDataTmp(d1, TrialItem.ItemName, "蓄电池电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    ControlEquipMent.Oscillograph?.Oscillograph_Trigger("RISE", 7, false, 0, "6", "Auto", 50);

                    SendNoticeToUIAndTxtFile("启动充电中");
                    MessgaeInfo(true, "请刷卡充电!", true);
                    int timeout = 6000;
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) <= 9);
                        if (ALLCanCharge)
                        {
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }

                        System.Threading.Thread.Sleep(50);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    // 避免通道7提前触发停止
                    ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");

                    //设置测试条件
                    SetConditionValues();

                    System.Threading.Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    System.Threading.Thread.Sleep(1000 * 35);

                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        CROValue2 = ControlEquipMent.Oscillograph.GetChannelValue(15, true, 13);
                        //CROValue2 = GetData_Value(15, true, 13, 0.99);
                        if (CROValue2[testWorkParam.lstIDs[0]] == "170")
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    ProcessData();
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
            var dicImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
            foreach (var item in testWorkParam.lstIDs)
            {
                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LstTrialData[k].ItemName = iIndex.ToString();

                string passValue = "0";
                if (TrialType == (int)EmTrialType.充电准备就绪测试研发_GEPlus5 || TrialType == (int)EmTrialType.充电准备就绪测试研发_GEMinus5)
                {
                    passValue = "170";
                    if (CROValue2[item] != "170")
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                }
                else
                {
                    if (CROValue2[item] == "170")
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                }
                //界面展示的数据项格式
                //状态|数据名称|测量值|上限|下限|结果
                LstTrialData[i].ExtentData = "充电机准备就绪信号" + $"|预充后CRO-AA|{passValue}|{passValue}|" + CROValue2[item] + "|" + dicImgs[item];
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
