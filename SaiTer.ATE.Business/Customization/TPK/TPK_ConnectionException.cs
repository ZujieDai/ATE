using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_ConnectionException : BusinessBase  //连接异常试验
    {
        public TPK_ConnectionException(int trialType) { TrialType = trialType; }

        int CheckTime = 10;//人工检测时间（秒）
        string TipContent = "";
        //int GunID = 1;
       // List<int> MylstIDs = new List<int>();


        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError;
        
        double[] cc1Vot = new double[8];
        System.Timers.Timer timer;
        private void InitTimer()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 100;
            timer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer.Enabled = true;
            //绑定Elapsed事件
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);
        }

        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    if (AllEquipStateData.DicBMS_DC_StateData[i + 1].CC1Voltage > 5.3)
                    {
                        cc1Vot[i] = 6;
                    }
                }
            }
            catch (Exception ex)
            {
               Log.Log.LogException("定时事件失败:" + ex.Message);
            }
        }

        public override void InitializeParams()
        {
            Init();

            //充电电压(V)=750|充电电流(A)=100|充电时间(分)=5|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5|电量误差(%)=5|枪号=1
            //充电电压(V)=750|充电电流(A)=10|老化时间(分)=5|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5|枪号=1
            string[] strParams = TrialItem.ResultParams.Split('|');
            AgingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            AgingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            AgingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            IntervalTime = Convert.ToDouble(strParams[3].Split('=')[1]);
            VoltError = Convert.ToDouble(strParams[4].Split('=')[1]);
            CurrError = Convert.ToDouble(strParams[5].Split('=')[1]);

            for (int i = 0; i < 8; i++)
            {
                cc1Vot[i] = 0;
            }

            InitTimer();

            //th = new Thread(GetCC1Vot);
            //th.IsBackground = true;
            //Control.CheckForIllegalCrossThreadCalls = false;
            //th.Start();


            //   Control.CheckForIllegalCrossThreadCalls = false;

            //if (strParams.Length > 6)
            //{
            //    // GunID = (int)Convert.ToDouble(strParams[6].Split('=')[1]);
            //}
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
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭负载");
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
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
                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }

                    //MylstIDs.Clear();
                    //MylstIDs.Add(GunID);

                    //设置测试条件
                    SetConditionValues();


                    //设置测试条件
                    // SetConditionValues();
                    //SetLoadDCOFF(MylstIDs);
                    //ControlEquipMent.BMS.BMS_OFF(MylstIDs);
                   // SetCPReresh();

                    CountDownTimeInfo("请按一下枪锁，保持1-3秒", 40, 2);

                    double cc1Value = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].CC1Voltage;
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        if (cc1Vot[item - 1] == 6)
                        {
                            cc1Value = 6;
                        }
                        else
                        {
                            cc1Value = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;

                        }

                        d1.Add(item, cc1Value.ToString("F2"));
                    }
                    ProcessDataTmp(d1, $"按枪锁后", "CC1电压(V)", "5.5", "6.5");
                    timer.Enabled = false;  //用完停止定时器

                   
                    //起机充电     先实现单个
                    AgingVolt = AgingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt;
                    SendNoticeToUIAndTxtFile("请刷卡充电");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr + 10, MaxAllowChargeVoltage, false))   //刷卡  带参数的刷卡函数  
                    {
                        return;
                    }
                    SendNoticeToUIAndTxtFile("充电中");
                    //Thread.Sleep(500);
                    //SendNoticeToUIAndTxtFile(string.Format("设置带载电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                    //if (AgingVolt == MaxAllowChargeVoltage)
                    //    SetLoadPara(MylstIDs, AgingVolt - 10, AgingCurr, AgingVolt - 5, AgingCurr);  // 原AgingCurr + 20
                    //else
                    //    SetLoadPara(MylstIDs, AgingVolt - 20, AgingCurr, AgingVolt - 20, AgingCurr); // 原AgingCurr + 20

                    //SendNoticeToUIAndTxtFile("开启负载");
                    //SetLoadDCON(MylstIDs);    //开启负载
                    //SendNoticeToUIAndTxtFile("等待电流稳定");
                    //WaitDCCurrentWithTime(MylstIDs, AgingCurr, 40);   //等待电流稳定
                    //Thread.Sleep(1000 * 6);
                    //Thread.Sleep(1000);
                    Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                    Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                    d1.Clear();
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;  //获取充电电压 
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;   //获取充电电流
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));


                        cc1Value = AllEquipStateData.DicBMS_DC_StateData[itmp].CC1Voltage;
                        d1.Add(itmp, cc1Value.ToString("F2"));

                    }


                    ProcessDataTmp(dicAgingVolt, string.Format("正常充电电压{0}V", AgingVolt), "桩实际电压(V)", ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                    ProcessDataTmp(d1, "正常充电中", "CC1电压(V)", "3.5", "4.5");
                    //  ProcessDataTmp(dicAgingCurr, string.Format("正常充电电流{0}A", AgingCurr), "桩实际电流(V)", "0", "1");


                    SendNoticeToUIAndTxtFile("控制CC1断线  ");
                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;       //断开CC1
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                    Thread.Sleep(3000);

                    dicAgingVolt.Clear();
                    dicAgingCurr.Clear();
                    d1.Clear();

                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;  //获取充电电压 
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;   //获取充电电流
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));

                        cc1Value = AllEquipStateData.DicBMS_DC_StateData[itmp].CC1Voltage;
                        d1.Add(itmp, cc1Value.ToString("F2"));


                    }
                    ProcessDataTmp(dicAgingVolt, "CC1断线后电压", "桩实际电压(V)", "0", "60");
                    //ProcessDataTmp(dicAgingCurr, string.Format("CC1断线后电流{0}A", AgingCurr), "桩实际电流(V)", "0", "1");
                    ProcessDataTmp(d1, "CC1断线后", "CC1电压(V)", "5.5", "6.5");

                    Ks[22] = true;       //恢复 CC1
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                    Thread.Sleep(1000);


                    //------------------------------------PE断线----------------------------------------------------------------

                    AgingVolt = AgingVolt + 20 >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt + 20;  //再次起桩  
                    if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr, MaxAllowChargeVoltage, false))   //刷卡  带参数的刷卡函数  
                    {
                        return;
                    }
                    //Thread.Sleep(500);    //不带载
                    //SendNoticeToUIAndTxtFile(string.Format("设置带载电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                    //if (AgingVolt == MaxAllowChargeVoltage)
                    //    SetLoadPara(MylstIDs, AgingVolt - 10, AgingCurr, AgingVolt - 5, AgingCurr);  // 原AgingCurr + 20
                    //else
                    //    SetLoadPara(MylstIDs, AgingVolt - 20, AgingCurr, AgingVolt - 20, AgingCurr); // 原AgingCurr + 20

                    //SendNoticeToUIAndTxtFile("开启负载");
                    //SetLoadDCON(MylstIDs);    //开启负载
                    //SendNoticeToUIAndTxtFile("等待电流稳定");
                    //WaitDCCurrentWithTime(MylstIDs, AgingCurr, 40);   //等待电流稳定
                    //Thread.Sleep(1000 * 6);

                    //Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                    //Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();

                    dicAgingVolt.Clear();
                    dicAgingCurr.Clear();
                    d1.Clear();
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;  //获取充电电压 
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;   //获取充电电流
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));

                        cc1Value = AllEquipStateData.DicBMS_DC_StateData[itmp].CC1Voltage;
                        d1.Add(itmp, cc1Value.ToString("F2"));

                    }


                    ProcessDataTmp(dicAgingVolt, string.Format("正常充电电压{0}V", AgingVolt), "桩实际电压(V)", ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                   // ProcessDataTmp(dicAgingCurr, string.Format("正常充电电流{0}A", AgingCurr), "桩实际电流(V)", "0", "1");
                    ProcessDataTmp(d1, "正常充电中", "CC1电压(V)", "3.5", "4.5");

                    SendNoticeToUIAndTxtFile("控制PE断线  ");
                    Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;   //Ks[17] = false;       //PE断线
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                    Thread.Sleep(3000);
                    dicAgingVolt.Clear();
                    dicAgingCurr.Clear();
                    d1.Clear();
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;  //获取充电电压 
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;   //获取充电电流
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));

                        cc1Value = AllEquipStateData.DicBMS_DC_StateData[itmp].CC1Voltage;
                        d1.Add(itmp, cc1Value.ToString("F2"));


                    }
                    ProcessDataTmp(dicAgingVolt, "PE断线后电压", "桩实际电压(V)", "0", "60");
                    ProcessDataTmp(d1, "PE断线后", "CC1电压(V)", "5.5", "6.5");


                    Thread.Sleep(500);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);  //关闭BMS

                    //  ProcessDataTmp(dicAgingCurr, string.Format("PE断线后电流{0}A", AgingCurr), "桩实际电流(V)", "0", "1");



                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    //int count = 0;
                    //while (sw.ElapsedMilliseconds / 1000 <= AgingTime * 60)
                    //{
                    //    dicAgingVolt.Clear();
                    //    dicAgingCurr.Clear();
                    //    Thread.Sleep(Convert.ToInt32(IntervalTime * 1000));
                    //    count++;
                    //    foreach (var itmp in MylstIDs)
                    //    {
                    //        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    //        if (voltage == 0)
                    //        {
                    //            Thread.Sleep(500);
                    //            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    //        }
                    //        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                    //        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                    //        if (current == 0)
                    //        {
                    //            Thread.Sleep(500);
                    //            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                    //        }
                    //        dicAgingCurr.Add(itmp, current.ToString("F2"));
                    //    }
                    //    int time = count * Convert.ToInt32(IntervalTime) + 1;
                    //    ProcessDataTmp(dicAgingVolt, string.Format("PE断线后电压{0}V", AgingVolt), string.Format("{0}秒时桩实际电压(V)", time), "0", "20");
                    //    ProcessDataTmp(dicAgingCurr, string.Format("PE断线后电流{0}A", AgingCurr), string.Format("{0}秒时桩实际电流(A)", time), "0", "1");
                    //}






                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}