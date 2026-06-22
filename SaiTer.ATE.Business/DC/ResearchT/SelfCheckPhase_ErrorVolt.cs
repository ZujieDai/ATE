using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 自检阶段测试流程--异常电压
    /// 包括 自检阶段测试-开始前>=10V不正常电池电压时握手电压超上限/超下限/正常值
    ///      自检阶段测试-开始前<10V不正常电池电压时握手电压超上限/超下限
    /// 5种情况
    /// </summary>
    public class SelfCheckPhase_ErrorVolt : BusinessBase
    {
        public SelfCheckPhase_ErrorVolt(int trialType) { TrialType = trialType; }
        private double wsdy握手电压 = 750;
        private double dcdy电池电压 = 390;//自检阶段测试<10V握手电压低于充电机下限默认150V, >=10V时, 电池电压默认100V

        private double dzfzdl电子负载电流 = 5000;
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
        public override void InitializeParams()
        {
            Init();

            //自检阶段测试-开始前>=10V不正常电池电压时握手电压超限  三种情况
            //握手电压超上限值(V)=1050|电池电压Ut-error(V)=100
            //握手电压正常值(V)=500|电池电压Ut-error(V)=100
            //握手电压超下限值(V)=150|电池电压Ut-error(V)=100




            //自检阶段测试 < 10V握手电压低于充电机下限 / 车辆最高允许充电总电压不匹配试验  两种情况
            //握手电压超上限值(V)=1050       
            //握手电压超下限值(V)=150

            wsdy握手电压 = LstChargerInfo[0].NominalVoltage;

            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                wsdy握手电压 = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                dcdy电池电压 = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
            if (strParams.Length >= 3)
            {
                dzfzdl电子负载电流 = Convert.ToDouble(strParams[2].Split('=')[1]);
            }
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(8);
                //过压控制开关
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(100);
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
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
                #region  ------  此部分代码保留,作用可忽略---------------


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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                #endregion

                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                InitOscillograph();

                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                //Thread.Sleep(100);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, dcdy电池电压, LstChargerInfo[0].NominalVoltage, 250);
                //Thread.Sleep(100);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, wsdy握手电压, 250, false, wsdy握手电压);
                //Thread.Sleep(100);
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //Thread.Sleep(100);
                //ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                //Thread.Sleep(100);

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(100);
                if (TrialType != (int)EmTrialType.自检阶段测试正常充电 && TrialType != (int)EmTrialType.车辆最高允许充电总电压不匹配试验)
                {
                    //过压控制开关
                    var kstate = GetKStatus16_Charging_DC();
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(100);
                }
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, dcdy电池电压);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, wsdy握手电压);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, wsdy握手电压, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);

                //设置测试条件
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    //外侧电压具体是多少???
                    d1.Add(testWorkParam.lstIDs[i], dcdy电池电压.ToString("F2"));
                }
                SetConditionValue("K1K2外侧电压(V)", d1);


                try
                {
                    SendNoticeToUIAndTxtFile("设置录波仪触发模式");

                    //这里添加设置录波仪触发模式,,  参数需要修改
                    //ControlEquipMent.Oscillograph.Oscillograph_Trigger("RISE", 6, false, 1, "10", "Single", 0);
                    //OscillographInstrument_SetTrigger(60, 8, 0, "RISE", false, 70);
                    ControlEquipMent.Oscillograph.Oscillograph_Trigger("RISE", 2, false, 1, "6", "Single", 20);
                    Thread.Sleep(2500);
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

                    SendNoticeToUIAndTxtFile("等待触发中...");
                    timeout = 15;
                    bool istrigger = false;
                    while (timeout-- > 0)
                    {
                        istrigger = ControlEquipMent.Oscillograph.Oscillograph_ReadTrigger().FirstOrDefault().Value == 0;
                        if (istrigger)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    if (!istrigger)
                    {
                        Dictionary<int, string> DTrigger = new Dictionary<int, string>();
                        foreach (int id in testWorkParam.lstIDs)
                            DTrigger.Add(id, "超时未触发");
                        ProcessDataTmp(DTrigger, "正常充电", "录波仪是否触发", " - ", " - ");
                    }
                    dicImagePath = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    Thread.Sleep(1000 * 20);
                    SendNoticeToUIAndTxtFile("判断充电状态");
                    ProcessData();

                    CountDownTimeInfo("请人工确认是否有告警！\r\n 勾选上代表有告警", 100, 2);
                    string statusName = TrialItem.ItemName.Split('-').Length > 1 ? TrialItem.ItemName.Split('-')[1] : TrialItem.ItemName;
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "有告警", statusName, true, "是否有告警");
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "未告警", statusName, false, "是否有告警");
                    }

                    if (TrialType == (int)EmTrialType.自检阶段测试正常充电)
                    {
                        Dictionary<int, string> dic = new Dictionary<int, string>();


                        SendNoticeToUIAndTxtFile("读取BHM报文时间点...");
                        ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(15, true, 9, 200, false);
                        GetTriggerTime_Single2(15, true, 9, 200, 0, false, true, 0.05);
                        double BHMTime = 0;
                        dic.Add(testWorkParam.lstIDs[0], (BHMTime * 1000).ToString());
                        ProcessDataTmp(dic, "正常充电", "BHM报文时间(ms)", "0", "-");


                        SendNoticeToUIAndTxtFile("读取输出稳定绝缘电压的值...");

                        //BHM报文电压(V)
                        double BHM_Voltage = System.Math.Abs(GetData_Value(15, true, 9, 0.99));
                        dic.Clear();
                        dic.Add(testWorkParam.lstIDs[0], BHM_Voltage.ToString());
                        ProcessDataTmp(dic, "正常充电", "BHM报文电压(V)", "-", "-");


                        //输出稳定绝缘电压的值(V)
                        dic.Clear();
                        dic = ControlEquipMent.Oscillograph.Oscillograph_ReadMeasure("MAXimum", 8, false, 0);
                        double Max = Convert.ToDouble(dic[1]);
                        double Y1 = LstChargerInfo[0].NominalVoltage >= BHM_Voltage ? BHM_Voltage : LstChargerInfo[0].NominalVoltage;
                        ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(8, Y1, Max);
                        ProcessDataTmp(dic, "正常充电", "输出稳定绝缘电压的值(V)", "-", (1.05 * Y1).ToString());





                        SendNoticeToUIAndTxtFile("读取K1和K2进行绝缘检测时间点...");
                        double K1K2Time = GetTriggerTime_Single2(7, false, 0, 6, 0, false, false, 0.05);
                        dic.Clear();
                        dic.Add(testWorkParam.lstIDs[0], (K1K2Time * 1000).ToString());
                        ProcessDataTmp(dic, "正常充电", "再闭合K1和K2进行绝缘检测时间点(ms)", (BHMTime * 1000).ToString(), "-");


                        //SendNoticeToUIAndTxtFile("读取泄放时间...");
                        ////ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(7, false, 0, -3, false);
                        ////GetTriggerTime_Single2(7, false, 0, -3, 0, false, false, 0.05);
                        //double BleedTime1 = GetTriggerTime_Single2(8, false, 0, Max * 0.9, 1, false, false, 0.05);//泄放时间
                        //Thread.Sleep(100);
                        //double BleedTime2 = GetTriggerTime_Single2(8, false, 0, 60, 1, false, false, 0.05);//泄放时间
                        //double BleedTime = RetainDecimals<double>(BleedTime2 - BleedTime1);

                        //BleedTime= Math.Abs(BleedTime);

                        //dic.Clear();
                        //dic.Add(testWorkParam.lstIDs[0], BleedTime.ToString());
                        //ProcessDataTmp(dic, "正常充电", "泄放时间(s)", "0", "1");



                        //SendNoticeToUIAndTxtFile("读取泄放后断开K1K2时间点...");
                        //double K1K2Time2 = GetTriggerTime_Single2(7, false, 0, 6, 1, false, false, 0.05);//泄放后断开K1K2时间点
                        //dic.Clear();
                        //dic.Add(testWorkParam.lstIDs[0], K1K2Time2.ToString());
                        //ProcessDataTmp(dic, "正常充电", "泄放后断开K1K2时间点(s)", BleedTime2.ToString(), "-");



                        SendNoticeToUIAndTxtFile("读取CRM报文时间点...");
                        double CRMTime1 = GetTriggerTime_Single2(15, true, 10, 1, 0, false, false, 0.05);//CRM报文时间点
                        dic.Clear();
                        dic.Add(testWorkParam.lstIDs[0], (CRMTime1 * 1000).ToString());
                        ProcessDataTmp(dic, "正常充电", "CRM报文时间点(ms)", (K1K2Time * 1000).ToString(), "-");


                        SendNoticeToUIAndTxtFile("读取BRM多包标识报文...");
                        double BRMTime1 = GetTriggerTime_Single2(15, true, 11, 1, 0, false, false, 0.05);//BRM多包标识报文
                        dic.Clear();
                        dic.Add(testWorkParam.lstIDs[0], (BRMTime1 * 1000).ToString());
                        ProcessDataTmp(dic, "正常充电", "BRM多包标识报文(ms)", (CRMTime1 * 1000).ToString(), "-");



                        //SendNoticeToUIAndTxtFile("读取辅源电压中...");
                        //dic.Clear();
                        //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        //{
                        //    double APSVoltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].APSVoltage;
                        //    dic.Add(testWorkParam.lstIDs[i], APSVoltage.ToString("F2"));
                        //}
                        //ProcessDataTmp(dic, "正常充电", "辅源电压(V)", "11.4", "12.6");
                        //Thread.Sleep(100);



                        ////硬件没有直接采集辅源电流,需要软件根据U/R计算出来,这里写计算后的结果
                        ////......计算辅源电流
                        //double current = 0;
                        //dic.Clear();
                        //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        //{
                        //    dic.Add(testWorkParam.lstIDs[0], current.ToString("F2"));
                        //}
                        //ProcessDataTmp(dic, "正常充电", "辅源电流(A)", "-", "10");




                        CountDownTimeInfo("确认充电中充电枪插头可靠被锁止！\r\n注：勾选上为可靠锁止", 20, 2);
                        dic.Clear();
                        foreach (var item in DicManualVerifyResult)
                        {
                            dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                        }
                        ProcessDataTmp(dic, "正常充电", "应可靠锁止", "-", "-");

                    }

                }

                catch (Exception ex) { Log.Log.LogException(ex); }
            }
        }
        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
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


                    if (dicImagePath != null)
                    {
                        sbtmp.Append(dicImagePath[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }


                    string result = "";

                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);

                    if (TrialType == (int)EmTrialType.自检阶段测试正常充电)
                    {
                        if (state > 4 && state <= 9)//>=3  or  >4 ????
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            //result = "允许充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            //result = "不允许充电";
                        }
                        //界面展示的数据项格式
                        //状态|数据名称|测量值|上限|下限|结果      
                        //自检阶段测试 - 开始前 >= 10V不正常电池电压时握手电压超上限
                        string statusName = TrialItem.ItemName.Split('-').Length > 1 ? TrialItem.ItemName.Split('-')[1] : TrialItem.ItemName;
                        LstTrialData[i].ExtentData = statusName
                            + "|" + "当前充电阶段"
                            + "|" + "-"
                            + "|" + "-"
                            + "|" + AllEquipStateData.DicBMS_DC_StateData[item].ChargingState
                            + "|" + sbtmp.ToString();
                    }
                    else
                    {
                        if (state > 4 && state <= 9)//>=3  or  >4 ????
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            //result = "允许充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            //result = "不允许充电";
                        }
                        //界面展示的数据项格式
                        //状态|数据名称|测量值|上限|下限|结果      
                        //自检阶段测试 - 开始前 >= 10V不正常电池电压时握手电压超上限
                        string statusName = TrialItem.ItemName.Split('-').Length > 1 ? TrialItem.ItemName.Split('-')[1] : TrialItem.ItemName;
                        LstTrialData[i].ExtentData = statusName
                            + "|" + "当前充电阶段"
                            + "|" + "-"
                            + "|" + "-"
                            + "|" + AllEquipStateData.DicBMS_DC_StateData[item].ChargingState
                            + "|" + sbtmp.ToString();
                    }
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);

                    SaveTrialData(LstTrialData[k]);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }
        /// <summary>
        ///录波仪初始化
        /// </summary>
        private void InitOscillograph()
        {
            try
            {
                if (TrialType == (int)EmTrialType.自检阶段测试开始前大于10V不正常电池电压时握手电压超上限 ||
                TrialType == (int)EmTrialType.自检阶段测试开始前大于10V不正常电池电压时握手电压超下限 ||
                TrialType == (int)EmTrialType.自检阶段测试开始前大于10V不正常电池电压时握手电压正常)
                {
                    InitOscillograph_1();
                }

                else if (TrialType == (int)EmTrialType.自检阶段测试小于10V握手电压低于充电机下限 ||
                   TrialType == (int)EmTrialType.车辆最高允许充电总电压不匹配试验)
                {
                    InitOscillograph_2();
                }

                else if (TrialType == (int)EmTrialType.自检阶段测试小于10V握手电压高于充电机上限)
                {
                    InitOscillograph_3();
                }

                else if (TrialType == (int)EmTrialType.自检阶段测试正常充电)
                {
                    InitOscillograph_4();
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void InitOscillograph_1()
        {
            ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
            SetChannelOpenInit();
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[4] = true;//5通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_TimeBase("1");
        }

        private void InitOscillograph_2()
        {
            ControlEquipMent.Oscillograph.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);

            SetChannelOpenInit();
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[4] = true;//5通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[8] = true;//CAN9通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_TimeBase("1");

        }
        private void InitOscillograph_3()
        {
            ControlEquipMent.Oscillograph.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
            SetChannelOpenInit();
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[4] = true;//5通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[8] = true;//CAN9通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_TimeBase("1");
        }

        private void InitOscillograph_4()
        {
            ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");
            ControlEquipMent.Oscillograph.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(true);
            SetChannelOpenInit();
            channelopen[1] = true;//2通道
            channelopen[2] = true;//3通道
            channelopen[3] = true;//4通道
            channelopen[4] = true;//5通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道
            canchannelopen[8] = true;//CAN9通道
            canchannelopen[9] = true;//CAN10通道
            canchannelopen[10] = true;//CAN11通道
            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_TimeBase("2");
            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(8);
            ControlEquipMent.Oscillograph.Oscillograph_AddMeasure("MAXimum", 8, false, 0);

        }
        bool[] channelopen = new bool[8];
        bool[] canchannelopen = new bool[20];
        public void SetChannelOpenInit()
        {
            for (int i = 0; i < channelopen.Length; i++)
            {
                channelopen[i] = false;
            }
            for (int i = 0; i < canchannelopen.Length; i++)
            {
                canchannelopen[i] = false;
            }
        }
    }
}
