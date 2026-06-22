using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标研测交流：充电准备就绪
    /// </summary>
    public class GB_RT_AC_ReadyForCharging : BusinessBase
    {
        bool isPlugCharger = false;     //是否即插即充
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double CPDutyMin, CPDutyMax;

        double CPNegativeMin = -12.8, CPNegativeMax = -11.2; //CP负电压的上下限

        public GB_RT_AC_ReadyForCharging(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            try
            {
                Init();
                //占空比下限(%)=3.00|占空比上限(%)=97.00|CP负电压下限(V)=-12.8|CP负电压上限(V)=-11.2|是否为即插即充(0是刷卡1是即插即充)=1.00
                string[] strParams = TrialItem.ResultParams.Split('|');
                CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
                CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
                if (strParams.Length >= 4)
                {
                    CPNegativeMin = Convert.ToDouble(strParams[2].Split('=')[1]);
                    CPNegativeMax = Convert.ToDouble(strParams[3].Split('=')[1]);
                }
                //是否为即插即充(0是刷卡1是即插即充)=0
                if (strParams.Length >= 5)
                {
                    isPlugCharger = Convert.ToDouble(strParams[4].Split('=')[1]) == 1;
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
                // ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                SetCPReresh();


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
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                        //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();

                try
                {
                    int sleepTime = 50;
                    string StatusName = "具备S2开关的车辆";

                    #region 具备S2开关的车辆
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.0");
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
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    Thread.Sleep(sleepTime);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发，自动
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "6.8", 3, "-10", "Single");//上升边沿触发，自动
                    Thread.Sleep(sleepTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");//低值
                    Thread.Sleep(1000);

                    //闭合CP,断开S2
                    var Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    string sState = $"{StatusName}(状态2’)";
                    CollectionCPPwm("8.2", "9.8", CPNegativeMin.ToString(), CPNegativeMax.ToString(), CPDutyMin.ToString(), CPDutyMax.ToString(), "10", "13", sState);
                    ////设置时基400ms
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "0");//低值
                    //Thread.Sleep(1000);
                    //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);

                    //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ////添加测量值
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    //Thread.Sleep(sleepTime);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//通道3采样，否则截图会有残影
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");//低值
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(2500);

                    //闭合开关S2，启动充电
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(2000);

                    sState = $"{StatusName}(状态3’)";
                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(4000);
                    Data_Tmp.Clear();
                    int time = 100;
                    while (time-- > 0)
                    {
                        double volt = 0;
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                                Data_Tmp[testWorkParam.lstIDs[i]] = volt.ToString();
                            else
                                Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                        }

                        if (volt > 50)
                            break;
                        else
                            Thread.Sleep(100);
                    }
                    ProcessDataTmp(Data_Tmp, sState, "充电电压(V)", "50", "-");
                    CollectionCPPwm("5.2", "6.8", CPNegativeMin.ToString(), CPNegativeMax.ToString(), CPDutyMin.ToString(), CPDutyMax.ToString(), "7", "13", sState);

                    CountDownTimeInfo("确认充电中充电枪插头可靠被锁止。\r\n(注:勾选上为可靠锁止)", 20, 2);
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "可靠锁止", "应可靠锁止", true, $"{StatusName}(状态3’)");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "未锁止", "应可靠锁止", false, $"{StatusName}(状态3’)");
                    #endregion

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    SetCPReresh();
                    StatusName = "不配置S2开关的车辆";
                    #region 不配置S2开关的车辆
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.0");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "10.0", "Auto");//通道3采样，否则截图会有残影
                    Thread.Sleep(sleepTime);
                    ////设置时基400ms
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");//低值
                    //Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    Thread.Sleep(sleepTime);

                    //不具备S2开关
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);

                    //即插即充会触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");//上升边沿触发，自动
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0.5");//低值
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.1", "0.0005");//低值
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.6");//低值
                    Thread.Sleep(sleepTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    //闭合CP
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(2500);

                    if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
                    {
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                        Thread.Sleep(1000);
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    //}
                    if (isPlugCharger)
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                    else
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
                    ProcessDataTmp(Data_Tmp, $"{StatusName}(状态3)", "CP正电压(V)", "5.2", "6.8", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.0");
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    //Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "-3", "Single");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "10", "0.05");//低值
                    Thread.Sleep(sleepTime);

                    //非即插即充
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);
                    //检测是否刷卡
                    WaitSwipingCard(testWorkParam.lstIDs, 0);
                    Thread.Sleep(1500);

                    sState = $"{StatusName}(状态3’)";
                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(4000);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp.Clear();
                    time = 100;
                    while (time-- > 0)
                    {
                        double volt = 0;
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                                Data_Tmp[testWorkParam.lstIDs[i]] = volt.ToString();
                            else
                                Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                        }

                        if (volt > 50)
                            break;
                        else
                            Thread.Sleep(100);
                    }

                    ProcessDataTmp(Data_Tmp, sState, "充电电压(V)", "50", "-");
                    CollectionCPPwm("5.2", "6.8", CPNegativeMin.ToString(), CPNegativeMax.ToString(), CPDutyMin.ToString(), CPDutyMax.ToString(), "7", "13", sState);

                    CountDownTimeInfo("确认充电中充电枪插头可靠被锁止。\r\n(注:勾选上为可靠锁止)", 20, 2);
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "可靠锁止", "应可靠锁止", true, $"{StatusName}(状态3’)");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "未锁止", "应可靠锁止", false, $"{StatusName}(状态3’)");
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
