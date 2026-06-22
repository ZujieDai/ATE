using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标研测直流：充电回路故障
    /// </summary>
    public class GB_RT_DC_ChargingLoopFault : BusinessBase
    {
        int trlTimeOut_S = 30;
        string ItemFlow = "";
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 20;
        /// <summary>
        /// 短路装置
        /// </summary>
        int ShortCircuitChargeID = 2;
        List<int> MylstIDs = new List<int>();


        public GB_RT_DC_ChargingLoopFault(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {

            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[1] = true;//2通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道

            canchannelopen[3] = true;//CAN4通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");

            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();

            MylstIDs = new List<int>();
            string[] strItems = TrialItem.ItemParams.Split('|');
            if (strItems.Length >= 1)
            {
                double value = double.Parse(strItems[0].Split('=')[1]);
                ShortCircuitChargeID = Convert.ToInt32(value);
                MylstIDs.Add(ShortCircuitChargeID);
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
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(1000);
                List<bool> Ks = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
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
                    SetConditionValues();

                    #region 绝缘故障
                    ItemFlow = "绝缘故障";
                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(1, 15, 4, "RISE", true, 50);
                    Thread.Sleep(2000);

                    //CountDownTimeInfo("请人工模拟直流供电回路出现绝缘故障", 999, 0);
                    var lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[4] = true;//DC+非对称漏电33K
                    lstKState[8] = false;
                    lstKState[12] = true;//DC-非对称漏电33K
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstKState.ToArray());
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, false, MaxAllowChargeVoltage);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);

                    SendNoticeToUIAndTxtFile("等待刷卡");
                    int timeout = 30;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) > 4 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9); // >=3 or >4??
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    ReadTriggerTypeOscillograph(30);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //double Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));
                    var Data15_4 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 4)[testWorkParam.lstIDs.First()];
                    //读取波形中是否有大于1的数据
                    double Value15_4 = Data15_4.FirstOrDefault(d => d >= 1);
                    Dictionary<int, string> dValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dValue.Add(item, Value15_4.ToString("F2"));
                    }
                    ProcessDatamessage(Value15_4.ToString(), dImgs);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, ItemFlow, "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, ItemFlow, "输出电流(A)", "-", "-");

                    CountDownTimeInfo("请恢复直流供电回路故障", 999, 0);
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPReresh();

                    #region 车辆测电压异常
                    ItemFlow = "车辆测电压异常";
                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(1, 15, 4, "RISE", true, 50);
                    Thread.Sleep(2000);

                    //CountDownTimeInfo("请人工模拟车辆侧充电回路电压异常", 999, 0);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 100);
                    Thread.Sleep(100);
                    //过压控制开关
                    var kstate = GetKStatus16_Charging_DC();
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, false, MaxAllowChargeVoltage);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);

                    SendNoticeToUIAndTxtFile("等待刷卡");
                    timeout = 30;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) > 4 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9); // >=3 or >4??
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    ReadTriggerTypeOscillograph(30);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));
                    Data15_4 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 4)[testWorkParam.lstIDs.First()];
                    //读取波形中是否有大于1的数据
                    Value15_4 = Data15_4.FirstOrDefault(d => d >= 1);
                    dValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dValue.Add(item, Value15_4.ToString("F2"));
                    }
                    ProcessDatamessage(Value15_4.ToString(), dImgs);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, ItemFlow, "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, ItemFlow, "输出电流(A)", "-", "-");

                    CountDownTimeInfo("请恢复直流供电回路故障", 999, 0);
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPReresh();

                    #region 短路故障
                    ItemFlow = "短路故障";
                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(1, 15, 4, "RISE", true, 50);
                    Thread.Sleep(2000);

                    CountDownTimeInfo("请人工确认是否具备短路保护功能,\r\n （勾选上代表具有）", 100, 2);
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "是", "是否具备短路保护功能", true, "短路");
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "否", "是否具备短路保护功能", false, "短路");
                        continue;
                    }

                    CountDownTimeInfo("请确认充电枪插头插入到短路装置!!!", 99999, 0);

                    SendNoticeToUIAndTxtFile("开启导引中");

                    if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(ShortCircuitChargeID))
                    {
                        Dictionary<int, string> datas = new Dictionary<int, string>();
                        datas.Add(ShortCircuitChargeID, "不存在该枪号");
                        ProcessDataTmpThis(MylstIDs, datas, TrialItem.ItemName, "结果", "-", "-");
                        return;
                    }

                    ControlEquipMent.BMS.BMS_OFF(MylstIDs);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);

                    CountDownTimeInfo("请手动设置短路测试仪模拟短路故障!!!", 999, 0);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(MylstIDs);

                    Thread.Sleep(2000);
                    MessgaeInfo(true, "请刷卡充电!", true);
                    timeout = 200;
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => MylstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    WaitDCVoltage(MylstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(5000);

                    ReadTriggerTypeOscillograph(30);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));
                    Data15_4 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 4)[testWorkParam.lstIDs.First()];
                    //读取波形中是否有大于1的数据
                    Value15_4 = Data15_4.FirstOrDefault(d => d >= 1);
                    dValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dValue.Add(item, Value15_4.ToString("F2"));
                    }
                    ProcessDatamessage(Value15_4.ToString(), dImgs);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in MylstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, ItemFlow, "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, ItemFlow, "输出电流(A)", "-", "-");

                    CountDownTimeInfo("请恢复直流供电回路故障", 999, 0);

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(MylstIDs);

                    CountDownTimeInfo("请恢复充电机正常！！", 999, 0);
                    CountDownTimeInfo("请确认充电枪插头插回到之前导引装置上!!!", 99999, 0);

                    #endregion
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public void ProcessDatamessage(string value, Dictionary<int, string> dImages)
        {
            foreach (var item in testWorkParam.lstIDs)
            {
                StringBuilder sbtmp = new StringBuilder();
                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);

                if (value != "1")
                {

                    LstTrialData[k].TrialResult = EmTrialResult.Fail;

                    LstTrialData[i].ExtentData = ItemFlow + "|CST电压异常报文|-|-|" + value + "|" +sbtmp.ToString();
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    LstTrialData[i].ExtentData = ItemFlow + "|CST电压异常报文|-|-|" + value + "|" + sbtmp.ToString();
                }
                //界面展示的数据项格式
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
                iIndex++;
            }
        }

        public override void ProcessData()
        {

        }
    }
}
