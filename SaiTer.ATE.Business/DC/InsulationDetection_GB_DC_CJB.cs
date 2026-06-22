using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流    绝缘检测功能试验
    /// </summary>
    public class InsulationDetection_GB_DC_CJB : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public InsulationDetection_GB_DC_CJB(int type)
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
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
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


                    SendNoticeToUIAndTxtFile("等待刷卡");
                    int timeout = 100;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        if (ChangeBMSChargeStatus(bmsData.First().Value.ChargingState) > 4)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    }
                    if (ChangeBMSChargeStatus(d1.First().Value) != 9)
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "充电状态", d1.First().Value, EmTrialResult.Pass);
                    else
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "充电状态", d1.First().Value, EmTrialResult.Fail);

                    CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "应发出告警提示", "有告警", EmTrialResult.Pass);
                    }
                    else
                    {
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "应发出告警提示", "未告警", EmTrialResult.Fail);
                    }

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


                    string[] ChargerNum = ConfigurationManager.AppSettings["ChargerNum_2015"] != null ?
                        ConfigurationManager.AppSettings["ChargerNum_2015"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
                    if (ChargerNum.Contains(testWorkParam.lstIDs.FirstOrDefault().ToString()))
                    {

                    }

                    SendNoticeToUIAndTxtFile("设置DC+非对称漏电300KΩ");
                    List<bool> lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[6] = true;//DC+非对称漏电300K
                                        //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC+非对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile("设置DC-非对称漏电300KΩ");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[14] = true;//DC-非对称漏电300K
                                         //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC-非对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile("设置DC-、DC+对称漏电300K");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[6] = true;//DC+非对称漏电300K
                    lstKState[8] = false;
                    lstKState[14] = true;//DC-非对称漏电300K
                                         //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC+、DC-对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[2] = true;//DC+非对称漏电20K
                    TrialMethod("DC+非对称漏电20KΩ", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[10] = true;//DC-非对称漏电20K
                    TrialMethod("DC-非对称漏电20KΩ", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[2] = true;//DC+非对称漏电20K
                    lstKState[8] = false;
                    lstKState[10] = true;//DC-非对称漏电20K
                    TrialMethod("DC+、DC-对称漏电20KΩ", "应不能充电", "应能告警", lstKState, false);


                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "output_DC_V", "1", "V", false, "200", "-2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "A", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "-0.6");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Time(testWorkParam.lstIDs, 1, 0, false);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "XY");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_Y(testWorkParam.lstIDs, 1, 0.987, 0.54);
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "40", 1, "60", "Auto");//下降边沿触发，单次文档上未60V
                    Thread.Sleep(waitTime);


                    SetCPReresh();  // 模拟插拔枪
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    timeout = 300;
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                        if (ALLCanCharge && AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage > 380)   //避免提前触发
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    // 等待进入充电状态
                    //Thread.Sleep(5500);     //避免提前触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    timeout = 60;
                    MessgaeInfo(true, "等待充电中...", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "等待充电中...");

                    Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");


                    ACDownTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 20, 1);//
                                                                //string Customer = ConfigurationManager.AppSettings["Customer"];
                                                                //if (Customer != null && Customer.Equals("DH"))
                                                                //{
                                                                //    Dictionary<int, double> records = new Dictionary<int, double>();
                                                                //    records.Add(testWorkParam.lstIDs[0], 0.42);
                                                                //    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, 1, records, false);
                                                                //}
                                                                //else
                    DCDownTime(testWorkParam.lstIDs, 1, 65, 2);//

                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);//读取卡点时间
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "绝缘电压", "泄放时间(ms)", "0", "1000", dImgs);
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
            ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(lstIDs);

            double insulationVolt = 0;
            int timeout = 300;
            MessgaeInfo(true, "请刷卡充电!", true);
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 2 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                if (ALLCanCharge)
                {
                    insulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                    break;
                }

                System.Threading.Thread.Sleep(1000);
            }
            MessgaeInfo(false, "请刷卡充电!");

            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstKState.ToArray());
            // 等待进入充电状态
            MessgaeInfo(true, "等待充电中...", true);
            foreach (int item in testWorkParam.lstIDs)
            {
                timeout = 600;
                while (timeout-- > 0)
                {
                    int bmsState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    if (bmsState <= 5)
                    {
                        double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                    }
                    bool ALLCanCharge = bmsState == 9 || bmsState == 13 || bmsState == 0;  //进入充电中或充电结束阶段
                    if (ALLCanCharge)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
            MessgaeInfo(false, "等待充电中...");

            d1 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                d1.Add(item, LstChargerInfo[0].NominalVoltage.ToString());
            }
            ProcessDataTmp(d1, sState, "车辆通信握手报文的最高允许充电电压(V)", "-", "-");

            d1 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                d1.Add(item, insulationVolt.ToString());
            }
            //绝缘检测电压应符合 GB/T 18487.1—2015 中 B.3.3 的规定
            ProcessDataTmp(d1, sState, "绝缘电压(V)", (LstChargerInfo[0].NominalVoltage * 0.99).ToString("F2"), (LstChargerInfo[0].NominalVoltage * 1.01).ToString("F2"));

            d1 = new Dictionary<int, string>();
            d2 = new Dictionary<int, string>();
            var dicVolt = new Dictionary<int, string>();
            string state = AllEquipStateData.DicBMS_DC_StateData[1].ChargingState;
            if (CanCharge)
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    timeout = 50;
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
                if (state == "充电中")
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Fail);
                }
            }
            else
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    timeout = 50;
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
                if (state != "充电中")
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Fail);
                }
            }


            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
            d1.Clear();
            d2.Clear();
            if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
            {
                ProcessDataResult(sState, ItemName2, "有告警", EmTrialResult.Pass);
            }
            else
            {
                ProcessDataResult(sState, ItemName2, "未告警", EmTrialResult.Fail);
            }
            List<bool> lstK = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstK.ToArray());
            Thread.Sleep(300);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        private void ProcessDataResult(string CheckState, string ItemName, string strResult, EmTrialResult trialResult)
        {

            LstTrialData[0].BarCode = LstChargerInfo[0].BarCode;
            LstTrialData[0].TrialName = TrialItem.ItemName;
            LstTrialData[0].SchemeName = TrialItem.SchemeName;
            LstTrialData[0].SchemeID = TrialItem.SchemeID;
            LstTrialData[0].ItemName = iIndex.ToString();
            LstTrialData[0].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[0].ChargerId].ChargingVoltage;

            LstTrialData[0].TrialResult = trialResult;


            LstTrialData[0].PKID = LstChargerInfo[0].PKID;
            //界面展示的数据项格式
            //状态|测试结果     
            LstTrialData[0].ExtentData = CheckState + "|" + ItemName + "|-|-|" + strResult + "|" + "报表(勿删)";
            LstTrialData[0].Data2 = LstTrialData[0].ExtentData;
            LstTrialData[0].Data3 = TrialItem.TrialOrder.ToString();
            SendTrialDataToUI(LstTrialData[0]);
            SaveTrialData(LstTrialData[0]);
            iIndex++;

        }
        public override void ProcessData()
        {

        }
    }
}
