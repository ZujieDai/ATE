using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标交流启动和充电阶段测试
    /// </summary>
    public class StartAndChargingPhase_GB_TB : BusinessBase
    {
        private static bool isFirstTest = true;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double CPDutyMin, CPDutyMax, CPFreqError, CPVoltError1, CPVoltError2, CPVoltError3;
        double OutputVoltError = 10;
        double CPPWMUpMax2 = 10;
        double CPPWMDownMax2 = 13;
        double CPPWMUpMax3 = 7;
        double CPPWMDownMax3 = 13;
        int Chage2Sleep = 0, StandbyPowerSleep = 0;
        double StandbyPowerMax = 5;

        public StartAndChargingPhase_GB_TB(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            try
            {
                Init();

                //占空比下限(%)=2.00|占空比上限(%)=96.00|频率误差(Hz)=30|状态1正向电平误差(V)=0.6|状态2正向电平误差(V)=0.6|状态3正向电平误差(V)=0.53|状态2'上升时间上限(μs)=10|状态2'下降时间上限(μs)=13|
                //状态3'上升时间上限(μs)=7|状态3'下降时间上限(μs)=13|输出电压误差(V)=10|桩类型（1为刷卡桩，否则为0）=0|状态2采样等待时间(ms)=5000|待机功耗采样等待时间(ms)=1000|待机功耗上限(W)=5
                string[] strParams = TrialItem.ResultParams.Split('|');
                CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
                CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
                CPFreqError = Convert.ToDouble(strParams[2].Split('=')[1]);
                CPVoltError1 = Convert.ToDouble(strParams[3].Split('=')[1]);
                CPVoltError2 = Convert.ToDouble(strParams[4].Split('=')[1]);
                CPVoltError3 = Convert.ToDouble(strParams[5].Split('=')[1]);
                CPPWMUpMax2 = Convert.ToDouble(strParams[6].Split('=')[1]);
                CPPWMDownMax2 = Convert.ToDouble(strParams[7].Split('=')[1]);
                CPPWMUpMax3 = Convert.ToDouble(strParams[8].Split('=')[1]);
                CPPWMDownMax3 = Convert.ToDouble(strParams[9].Split('=')[1]);
                OutputVoltError = Convert.ToDouble(strParams[10].Split('=')[1]);
                IsCardCharg = double.Parse(strParams[11].Split('=')[1]) == 1;
                if (strParams.Length > 12)
                {
                    Chage2Sleep = (int)Convert.ToDouble(strParams[12].Split('=')[1]);
                    StandbyPowerSleep = (int)Convert.ToDouble(strParams[13].Split('=')[1]);
                    StandbyPowerMax = Convert.ToDouble(strParams[14].Split('=')[1]);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
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
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                {
                    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                    int i = 500;
                    while (i-- > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                        {
                            //双重判断
                            Thread.Sleep(100);
                            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                                break;
                        }
                        Thread.Sleep(100);
                    }
                }
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
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
                                //测试时间|电压|电流|CP电压|CP频率|CP占空比
                                LstTrialData[i].ExtentData = DateTime.Now.ToString()
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + "";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
               

                try
                {
                    int sleepTime = 50;

                    //断开CP,S2
                    List<bool> Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    //可能发一遍指令没有作用
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    if (isFirstTest)
                    {
                        //初始化示波器
                        SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                        ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                        Thread.Sleep(sleepTime);

                        ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                        //添加测量值
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//上升沿
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//下降沿
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "50", 3, "0", "Auto");
                        Thread.Sleep(sleepTime);
                        isFirstTest = false;
                    }

                    //启动示波器
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(sleepTime);

                    #region -----------状态1------------------

                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        int timeout = 10;
                        while (timeout-- > 0)
                        {
                            if (cpVolt >= 11.2 && cpVolt <= 12.8)
                                break;
                            Thread.Sleep(200);
                            cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], cpVolt.ToString());
                    }                   
                    ProcessDataTmp(Data_Tmp, "状态1", "CP电压(V)", (12 - CPVoltError1).ToString(), (12 + CPVoltError1).ToString());


                    //增加的待机功耗
                    //Dictionary<int, double> Power_Tmp = new Dictionary<int, double>();
                    //Power_Tmp = ControlEquipMent.ElectricMeter.EM_GetTotalPower(testWorkParam.lstIDs);
                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], Power_Tmp[testWorkParam.lstIDs[i]].ToString());
                    //}
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtElectricMeter_ZH4041")) != null)
                    {
                        if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtDIORelay")) != null)
                        {
                            ControlEquipMent.ControlBoard.SetRelaySwitch(0, false);
                            Thread.Sleep(300);
                            //ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                            //Thread.Sleep(300);
                            //ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                            //Thread.Sleep(300);
                            ControlEquipMent.ControlBoard.SetRelaySwitch(1, true);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            var lstKS = ControlEquipMent.ControlBoard.ControlBoardReadState(testWorkParam.lstIDs);
                            lstKS[9] = true;
                            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(testWorkParam.lstIDs, lstKS);
                            Thread.Sleep(300);
                        }
                    }

                    SendNoticeToUIAndTxtFile($"待机功耗采样等待{StandbyPowerSleep}ms");
                    Thread.Sleep(StandbyPowerSleep);

                    Data_Tmp.Clear();
                    //由于设备到充电桩有功耗，需要减掉一个差值
                    double powerDiff = 0;
                    Dictionary<int, double> Power_Tmp = new Dictionary<int, double>();
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtElectricMeter_ZH4041")) != null)
                    {
                        Power_Tmp = ControlEquipMent.ElectricMeter.EM_GetTotalPower_ZH(testWorkParam.lstIDs);
                        powerDiff = 1.2;
                    }
                    else
                        Power_Tmp = ControlEquipMent.ElectricMeter.EM_GetTotalPower(testWorkParam.lstIDs);
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double power = Power_Tmp[testWorkParam.lstIDs[i]] - powerDiff > 0 ? Power_Tmp[testWorkParam.lstIDs[i]] - powerDiff : 0;
                        Data_Tmp.Add(testWorkParam.lstIDs[i], power.ToString("F2"));
                    }                   
                    ProcessDataTmp(Data_Tmp, "状态1", "待机功耗(W)", "0", StandbyPowerMax.ToString());

                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtElectricMeter_ZH4041")) != null)
                    {
                        if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtDIORelay")) != null)
                        {
                            ControlEquipMent.ControlBoard.SetRelaySwitch(1, false);
                            Thread.Sleep(300);
                        }
                        else
                        {
                            var lstKS = ControlEquipMent.ControlBoard.ControlBoardReadState(testWorkParam.lstIDs);
                            lstKS[9] = false;
                            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(testWorkParam.lstIDs, lstKS);
                            Thread.Sleep(300);
                        }
                    }

                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态1", "输出电压(V)", "0", "20");

                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比              
                    ProcessDataTmp(Data_Tmp, "状态1", "CP占空比(%)", "-", "-");
                    #endregion


                    #region -----------状态2------------------
                    //闭合CP,断开S2
                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    //可能发一遍指令没有作用
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    SendNoticeToUIAndTxtFile($"状态2采样等待{Chage2Sleep}ms");
                    Thread.Sleep(Chage2Sleep);

                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        int timeout = 10;
                        while(timeout-- > 0)
                        {
                            if (cpVolt >= 8.2 && cpVolt <= 9.8)
                                break;
                            Thread.Sleep(200);
                            cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], cpVolt.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态2", "CP电压(V)", (9 - CPVoltError2).ToString(), (9 + CPVoltError2).ToString());

                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态2", "输出电压(V)", "0", "20");
                    #endregion

                    //上一步NG可能会导致刷卡一直等待，充不起来
                    if (LstTrialData.Find(c => c.TrialResult == EmTrialResult.Fail) != null)
                    {
                        return;
                    }

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);

                    #region -----------状态2’------------------
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        int timeout = 10;
                        while (timeout-- > 0)
                        {
                            if (cpVolt >= 9 - CPVoltError2 && cpVolt <= 9 + CPVoltError2)
                                break;
                            Thread.Sleep(200);
                            cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], cpVolt.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP正电压(V)", (9 -CPVoltError2).ToString(), (9 + CPVoltError2).ToString());
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP负电压(V)", "-12.8", "-11.2");
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP频率(Hz)", (1000 - CPFreqError).ToString(), (1000 + CPFreqError).ToString());
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比                   
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());
                                      

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000 * 1000).ToString());
                    }                   
                    ProcessDataTmp(dd, "状态2’", "CP上升时间(us)", "0", CPPWMUpMax2.ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000 * 1000).ToString());
                    }
                    ProcessDataTmp(dd, "状态2’", "CP下降时间(us)", "0", CPPWMDownMax2.ToString());

                    #endregion



                    #region -----------状态3------------------

                    SendNoticeToUIAndTxtFile("闭合导引S2开关，开始充电");

                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    var kstatus = GetKStatus16_Charging();
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, kstatus);
                    //可能发一遍指令没有作用
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, kstatus);
                    SendNoticeToUIAndTxtFile("等待充电状态稳定");
                    Thread.Sleep(1000);
                    int timeOut = 10;
                    while(timeOut > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle > 2 && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle < 96
                            && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > LstChargerInfo[0].NominalVoltage * 0.95
                            && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage < LstChargerInfo[0].NominalVoltage * 1.15)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        timeOut--;
                    }
                 
                    //初始化示波器
                    //SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");                 
                 
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.5", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    //采集电压不准确
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_SetGear(testWorkParam.lstIDs, 3, "2");
                    //Thread.Sleep(500);

                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");           
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP正电压(V)", (6 - CPVoltError3).ToString(), (6 + CPVoltError3).ToString());

                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_SetGear(testWorkParam.lstIDs, 3, "5");
                    //Thread.Sleep(500);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP负电压(V)", "-12.8", "-11.2");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP频率(Hz)", (1000 - CPFreqError).ToString(), (1000 + CPFreqError).ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                      
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    timeOut = 20;
                    while (timeOut > 0)
                    {
                        if(Convert.ToDouble(Data_Tmp.Values.First()) * 1000000 > CPPWMDownMax3)
                            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);
                        else
                            break;
                        Thread.Sleep(200);
                    }
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "状态3’", "CP上升时间(us)", "0", CPPWMUpMax3.ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "状态3’", "CP下降时间(us)", "0", CPPWMDownMax3.ToString());


                    Data_Tmp.Clear();
                    double v = LstChargerInfo[0].NominalVoltage;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        int timeout = 30;
                        while (timeout-- > 0)
                        {
                            if (volt >= v * 0.95 && volt <= v * 1.05)
                                break;
                            Thread.Sleep(200);
                            volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态3’", "输出电压(V)", (v - OutputVoltError).ToString(), (v + OutputVoltError).ToString());
                    #endregion
                }

                catch (Exception ex)
                {
                    Log.Log.LogException(ex, "业务异常日志");
                }
            }
        }

        public override void ProcessData()
        {

        }

    }
}


