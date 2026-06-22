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
    /// 国标研测直流：电子锁异常
    /// </summary>
    public class GB_RT_DC_LockerAbnormal : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 20;


        public GB_RT_DC_LockerAbnormal(int type)
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
            canchannelopen[8] = true;//CAN9通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("X");

            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();
        }
        public override void ExecuteMethod()
        {
            try
            {
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, true);

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
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);

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


                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, DemandVoltage))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent + 10, 35);
                    Thread.Sleep(5000);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, "模拟电子锁异常前", "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, "模拟电子锁异常前", "输出电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("设置录波仪触发中...");
                    OscillographInstrument_SetTrigger(DemandVoltage * 0.5, 4, 0, "FALL", false, 50);
                    Thread.Sleep(2000);

                    CountDownTimeInfo("请人工模拟电子未预期上锁或未可靠锁止", 999, 0);

                    ReadTriggerTypeOscillograph(30);

                    SendNoticeToUIAndTxtFile("设置卡点位置中...");
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 0.5, true);

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //double Value15_4 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(15, true, 4)));
                    var Data15_4 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 4)[testWorkParam.lstIDs.First()];
                    //读取波形中是否有大于1的数据
                    double Value15_4 = Data15_4.FirstOrDefault(d => d >= 1);

                    Dictionary<int, string> dValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dValue.Add(item, Value15_4.ToString("F2"));
                    }
                    ProcessDatamessage(Value15_4.ToString());

                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_XY(4, false, 0);

                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(1, false, 0, 0, true);  //电压上升触发位置

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                    }
                    ProcessDataTmp(d1, "模拟电子锁异常后", "输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, "模拟电子锁异常后", "输出电流(A)", "-", "-");


                    //double time = System.Math.Abs(GetTriggerTime_Position(1, false, 0, position1, position2));
                    double time = 0;
                    if (K1K2s2 > 2)
                    {
                        time = GetTriggerTime_Single(7, false, 0, 6, 1, false, false, 0.05, true);
                    }
                    else
                    {
                        time = GetTriggerTime_Single(7, false, 0, 6, 0, false, false, 0.05, true);
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    Dictionary<int, string> dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime.Add(item, (time * 1000).ToString("F2"));
                    }
                    ProcessDataTmp(dTime, "模拟电子锁异常后", " C1C2断开时间(ms)", "0", "1000", dImgs);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    double CSDTime1 = GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05) * 100;
                    dic.Add(testWorkParam.lstIDs.FirstOrDefault(), CSDTime1.ToString());
                    ProcessDataTmp(dic, "模拟电子锁异常后", "CSD统计报文时间点", "-", "-");

                    //double time2 = GetTriggerTime_Single(2, false, 0, 6, 0, false, false, 0.05, true);
                    double K3K4Time1 = GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05) * 100;
                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    Dictionary<int, string> dTime2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //dTime2.Add(item, (time2 * 1000).ToString("F2"));
                        dTime2.Add(item, K3K4Time1.ToString("F2"));
                    }
                    ProcessDataTmp(dTime2, "模拟电子锁异常后", " S3S4在统计报文后断开", CSDTime1.ToString(), "-", dImgs2);

                    CountDownTimeInfo("请恢复电子锁异常", 999, 0);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }

        public void ProcessDatamessage(string value)
        {
            foreach (var item in testWorkParam.lstIDs)
            {
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


                if (value != "1")
                {

                    LstTrialData[k].TrialResult = EmTrialResult.Fail;

                    LstTrialData[i].ExtentData = "模拟电子锁异常前" + "|CST电压异常报文|-|-|" + value + "|报表(勿删)";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    LstTrialData[i].ExtentData = "模拟电子锁异常前" + "|CST电压异常报文|-|-|" + value + "|报表(勿删)";
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
