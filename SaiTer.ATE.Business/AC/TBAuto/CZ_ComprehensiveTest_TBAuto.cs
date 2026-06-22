using Newtonsoft.Json;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 综合性测试（保护枪头电阻、启动和充电阶段测试和带载
    /// </summary>
    public class CZ_ComprehensiveTest_TBAuto : BusinessBase
    {
        private static bool isFirstTest = true;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double CPDutyMin, CPDutyMax, CPVoltError1, CPVoltError2, CPVoltError3, CPVoltErrorNegative, OutputVoltError;
        double CPPWMUpMax2 = 10;
        double CPPWMDownMax2 = 13;
        double CPPWMUpMax3 = 7;
        double CPPWMDownMax3 = 13;
        int Chage2Sleep = 0, StandbyPowerSleep = 0;
        double StandbyPowerMax = 5;
        double DemandVolt = 220;
        double LoadCurrent = 16;
        int ChargeTime = 10;


        public CZ_ComprehensiveTest_TBAuto(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {
            Init();

            //数据库参数格式
            //占空比下限(%)=2.00|占空比上限(%)=96.00|负向电平误差(V)=0.6|状态1正向电平误差(V)=0.6|状态2正向电平误差(V)=0.6|状态3正向电平误差(V)=0.53|状态2'上升时间上限(μs)=10|状态2'下降时间上限(μs)=13|
            //状态3'上升时间上限(μs)=7|状态3'下降时间上限(μs)=13|状态2采样等待时间(ms)=5000|待机功耗采样等待时间(ms)=1000|待机功耗上限(W)=5|输出电压判断误差(V)=10|BMS需求电流(A)=16|充电时间(s)=10

            string[] strParams = TrialItem.ResultParams.Split('|');
            CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
            CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            CPVoltErrorNegative = Convert.ToDouble(strParams[2].Split('=')[1]);
            CPVoltError1 = Convert.ToDouble(strParams[3].Split('=')[1]);
            CPVoltError2 = Convert.ToDouble(strParams[4].Split('=')[1]);
            CPVoltError3 = Convert.ToDouble(strParams[5].Split('=')[1]);
            CPPWMUpMax2 = Convert.ToDouble(strParams[6].Split('=')[1]);
            CPPWMDownMax2 = Convert.ToDouble(strParams[7].Split('=')[1]);
            CPPWMUpMax3 = Convert.ToDouble(strParams[8].Split('=')[1]);
            CPPWMDownMax3 = Convert.ToDouble(strParams[9].Split('=')[1]);
            Chage2Sleep = (int)Convert.ToDouble(strParams[10].Split('=')[1]);
            StandbyPowerSleep = (int)Convert.ToDouble(strParams[11].Split('=')[1]);
            StandbyPowerMax = Convert.ToDouble(strParams[12].Split('=')[1]);
            LoadCurrent = Convert.ToDouble(strParams[13].Split('=')[1]);
            OutputVoltError = Convert.ToDouble(strParams[14].Split('=')[1]);
            ChargeTime = (int)Convert.ToDouble(strParams[15].Split('=')[1]);
            DemandVolt = LstChargerInfo[0].NominalVoltage;
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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //插枪检测
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }

                #region 测CP;
                int sleepTime = 50;

                //断开CP,S2
                List<bool> Ks = GetKStatus16_Charging();
                Ks[0] = false;
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                //可能发一遍指令没有作用
                Thread.Sleep(1000);
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Stopwatch sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 3500)
                {
                    double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs.First()].CPVoltage;
                    if (cpVolt >= 11.2 && cpVolt <= 12.8)
                        break;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(200);
                }

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

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");
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
                    isFirstTest = false;
                }

                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(1000);

                #region -----------状态1------------------

                int timeout = 10;
                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                    timeout = 10;
                    while (timeout-- > 0)
                    {
                        if (cpVolt >= 12 - CPVoltError1 && cpVolt <= 12 + CPVoltError1)
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
                if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtDIORelay")) != null
                    && ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtElectricMeter_ZH4041")) != null)
                {
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(0, false);
                    Thread.Sleep(300);
                    //ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                    //Thread.Sleep(300);
                    //ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                    //Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(1, true);
                    Thread.Sleep(500);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
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

                if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtDIORelay")) != null)
                {
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(1, false);
                    Thread.Sleep(300);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
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
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "状态2", "CP电压(V)", (9 - CPVoltError2).ToString(), (9 + CPVoltError2).ToString());

                #endregion

                //上一步NG可能会导致刷卡一直等待，充不起来
                if (LstTrialData.Find(c => c.TrialResult == EmTrialResult.Fail) != null)
                {
                    return;
                }

                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);




                #region -----------状态2'------------------


                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "状态2", "输出电压(V)", "0", "20");

                Data_Tmp.Clear();

                timeout = 10;
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                    timeout = 10;
                    while (timeout-- > 0)
                    {
                        if (cpVolt >= 8.2 && cpVolt <= 9.8)
                            break;
                        Thread.Sleep(200);
                        cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                    }
                    Data_Tmp.Add(testWorkParam.lstIDs[i], cpVolt.ToString());
                }
                ProcessDataTmp(Data_Tmp, "状态2’", "CP正电压(V)", (9 - CPVoltError2).ToString(), (9 + CPVoltError2).ToString());

                Data_Tmp.Clear();
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > (-12 + CPVoltErrorNegative) || Convert.ToDouble(Data_Tmp.First().Value) < (-12 - CPVoltErrorNegative))
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
                    }
                    else
                        break;
                }
                //if (Convert.ToDouble(Data_Tmp.First().Value) < -10.8 && Convert.ToDouble(Data_Tmp.First().Value) > -11.2)
                //{
                //    foreach (var item in Data_Tmp.Keys)
                //    {
                //        Data_Tmp[item] = (Convert.ToDouble(Data_Tmp[item]) - 0.3).ToString();
                //    }
                //}
                ProcessDataTmp(Data_Tmp, "状态2’", "CP负电压(V)", (-12 - CPVoltErrorNegative).ToString(), (-12 + CPVoltErrorNegative).ToString());
                Data_Tmp.Clear();
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > 1030 || Convert.ToDouble(Data_Tmp.First().Value) < 970)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "状态2’", "CP频率(Hz)", "970", "1030");

                Data_Tmp.Clear();
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比    
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > CPDutyMax || Convert.ToDouble(Data_Tmp.First().Value) < CPDutyMin)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "状态2’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());


                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMUpMax2)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);
                    }
                    else
                        break;
                }
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000 * 1000).ToString());
                }
                ProcessDataTmp(dd, "状态2’", "CP上升时间(us)", "0", CPPWMUpMax2.ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMDownMax2)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);
                    }
                    else
                        break;
                }
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
                while (timeOut > 0)
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


                SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > 6.8 || Convert.ToDouble(Data_Tmp.First().Value) < 5.2)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "状态3’", "CP正电压(V)", (6 - CPVoltError3).ToString(), (6 + CPVoltError3).ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > (-12 + CPVoltErrorNegative) || Convert.ToDouble(Data_Tmp.First().Value) < (-12 - CPVoltErrorNegative))
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
                    }
                    else
                        break;
                }
                //if (Convert.ToDouble(Data_Tmp.First().Value) < -10.8 && Convert.ToDouble(Data_Tmp.First().Value) > -11.2)
                //{
                //    foreach (var item in Data_Tmp.Keys)
                //    {
                //        Data_Tmp[item] = (Convert.ToDouble(Data_Tmp[item]) - 0.3).ToString();
                //    }
                //}
                ProcessDataTmp(Data_Tmp, "状态3’", "CP负电压(V)", (-12 - CPVoltErrorNegative).ToString(), (-12 + CPVoltErrorNegative).ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > 1030 || Convert.ToDouble(Data_Tmp.First().Value) < 970)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "状态3’", "CP频率(Hz)", "970", "1030");

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > CPDutyMax || Convert.ToDouble(Data_Tmp.First().Value) < CPDutyMin)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMUpMax3)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);
                    }
                    else
                        break;
                }
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "状态3’", "CP上升时间(us)", "0", CPPWMUpMax3.ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMDownMax3)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);
                    }
                    else
                        break;
                }
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "状态3’", "CP下降时间(us)", "0", CPPWMDownMax3.ToString());


                Data_Tmp.Clear();
                double v = DemandVolt;
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    timeout = 60;
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
                #endregion

                #region 测带载
                if (isFirstTest)
                {
                    //是否为三相充电桩
                    bool isSanX = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 20
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseB_Voltage > 20
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseC_Voltage > 20;
                    //此时充电已经停止，可以设置单三相切换（TB的交流源都是要用DIO继电器控制单三相输出）
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1500);
                    if (isSanX)
                    {
                        ControlEquipMent.ControlBoard.SetRelaySwitch(2, true);
                        Thread.Sleep(300);
                        ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                        Thread.Sleep(300);
                    }
                    else
                    {
                        ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                        Thread.Sleep(300);
                        ControlEquipMent.ControlBoard.SetRelaySwitch(3, true);
                        Thread.Sleep(300);
                    }
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    //等待电压稳定
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        timeout = 60;
                        while (timeout-- > 0)
                        {
                            if (volt >= v * 0.95 && volt <= v * 1.05)
                                break;
                            Thread.Sleep(200);
                            volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        }
                    }
                }

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, DemandVolt, LoadCurrent);
                Thread.Sleep(ChargeTime * 1000);    //带载10s

                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    timeout = 10;
                    double current = 0;
                    int count = 0;
                    while (timeout-- > 0)
                    {
                        current = AllEquipStateData.DicBMS_AC_StateData[LstChargerInfo[0].ChargerId].PhaseA_Current;

                        if (current > LoadCurrent * 0.8 && current < LoadCurrent * 1.2)
                        {
                            count++;
                            if (count >= 3)
                            {
                                break;
                            }
                        }
                        Thread.Sleep(300);
                    }

                    Data_Tmp.Add(testWorkParam.lstIDs[i], current.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, "输出带载", "输出电流(A)", (LoadCurrent * 0.8).ToString(), (LoadCurrent * 1.2).ToString());
                #endregion
            }
        }

        public override void ProcessData()
        {
        }
    }
}
