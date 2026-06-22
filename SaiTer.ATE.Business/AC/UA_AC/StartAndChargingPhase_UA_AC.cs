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
    /// 美标交流启动和充电阶段测试
    /// </summary>
    public class StartAndChargingPhase_UA_AC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double CPDutyMin = 9.5;
        double CPDutyMax = 96.5;
        double CPPWMUpMax = 10;
        double CPPWMDownMax = 13;

        public StartAndChargingPhase_UA_AC(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            try
            {
                Init();
                //占空比下限(%)=9.50|占空比上限(%)=96.50|CP上升时间上限(us)=10|CP下降时间上限(us)=13
                string[] strParams = TrialItem.ResultParams.Split('|');
                CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
                CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
                if (strParams.Length > 2)
                {
                    CPPWMUpMax = Convert.ToDouble(strParams[2].Split('=')[1]);
                    CPPWMDownMax = Convert.ToDouble(strParams[3].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
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
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                try
                {
                    //断开CP,S2
                    List<bool> Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    int waitTime = 50;

                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "2", "-3");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.2", "0");//时基
                    Thread.Sleep(waitTime);

                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    Thread.Sleep(waitTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发，自动
                    Thread.Sleep(waitTime);

                    Thread.Sleep(1000);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    Thread.Sleep(1000);

                    //读取A状态CP电压值
                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    //}
                    //ProcessDataTmp(Data_Tmp, "状态1", "CP电压(V)", CPVoltMin.ToString(), CPVoltMax.ToString());
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                    ProcessDataTmp(Data_Tmp, "A状态", "CP电压(V)", "11.4", "12.6", dImgs);
                    //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                    //ProcessDataTmp(Data_Tmp, "A状态", "CP电压(V)", "11.4", "12.6");

                    if (TrialType == (int)EmTrialType.CP信号测试_TB)
                    {
                        //增加的待机功耗（TB用）
                        Dictionary<int, double> Power_Tmp = new Dictionary<int, double>();
                        Power_Tmp = ControlEquipMent.ElectricMeter.EM_GetTotalPower(testWorkParam.lstIDs);
                        Data_Tmp.Clear();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            Data_Tmp.Add(testWorkParam.lstIDs[i], Power_Tmp[testWorkParam.lstIDs[i]].ToString());
                        }
                        ProcessDataTmp(Data_Tmp, "A状态", "待机功耗(W)", "0", "5");
                    }

                    //闭合CP,断开S2
                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);

                    //读取B1状态CP电压值
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "2", "-3");
                    //Thread.Sleep(500);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                    if (Customer != null && Customer.ToUpper().Equals("NT"))
                        ProcessDataTmp(Data_Tmp, "B1状态", "CP电压(V)", "8.4", "9.6", dImgs);
                    else
                        ProcessDataTmp(Data_Tmp, "B1状态", "CP电压(V)", "8.36", "9.59", dImgs);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);

                    //读取B2状态数据判断
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    if (Customer != null && Customer.ToUpper().Equals("NT"))
                        ProcessDataTmp(Data_Tmp, "B2状态", "CP正电压(V)", "8.4", "9.6", dImgs);
                    else
                        ProcessDataTmp(Data_Tmp, "B2状态", "CP正电压(V)", "8.36", "9.59", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "2", "3");
                    Thread.Sleep(50);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(250);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "B2状态", "CP负电压(V)", "-12.6", "-11.4", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(50);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(250);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "B2状态", "CP频率(Hz)", "980", "1020", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    if (Customer != null && Customer.Equals("NT"))
                        ProcessDataTmp(Data_Tmp, "B2状态", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString(), dImgs);
                    else
                        ProcessDataTmp(Data_Tmp, "B2状态", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    //上升沿下降沿触发闭合，关闭滤波功能
                    List<bool> lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(500);

                    //有的示波器测量值只能添加4个
                    if (Customer != null && Customer.Equals("HYQCP"))
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                        Thread.Sleep(waitTime);
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    //ProcessDataTmp(dd, "B2状态", "CP上升时间(us)", "-", "2");
                    ProcessDataTmp(dd, "B2状态", "CP上升时间(us)", "0", "10", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    if (Customer != null && Customer.Equals("NT"))
                        ProcessDataTmp(dd, "B2状态", "CP下降时间(us)", "0", "13", dImgs);
                    else
                        ProcessDataTmp(dd, "B2状态", "CP下降时间(us)", "0", "13");

                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//低值
                    Thread.Sleep(waitTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 1, "100", "Single");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    SendNoticeToUIAndTxtFile("启动示波器，闭合导引S2开关，开始充电");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    //闭合开关S2，启动充电


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    //Thread.Sleep(100);
                    SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    //卡点
                    //SetCursor_Zero(testWorkParam.lstIDs, 3, 0, 1);//设置触发点为0
                    CPPWMDownTime(testWorkParam.lstIDs, 3, 8.2, 1);//判断CP变化时刻
                    ACUpTime(testWorkParam.lstIDs, 1, 50, 2);//判断交流电压变化时刻


                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "C2状态", "K1K2闭合时间(ms)", "0", "3000", dImgs);


                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                        d3.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Current.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SetConditionValue("带载电流(A)", d3);
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.5", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.2", "0");//低值
                    Thread.Sleep(waitTime);
                    SendNoticeToUIAndTxtFile("添加示波器测量值");
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//上升沿
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//下降沿
                    Thread.Sleep(waitTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "0", "Auto");//边沿触发，自动
                    Thread.Sleep(waitTime);

                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("启动示波器");

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    //读取C状态CP电压值
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "2", "-3");
                    Thread.Sleep(250);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    if (Customer != null && Customer.ToUpper().Equals("NT"))
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP正电压(V)", "5.4", "6.6", dImgs);
                    else
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP正电压(V)", "5.47", "6.53", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "2", "3");
                    Thread.Sleep(50);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(250);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "C2状态", "CP负电压(V)", "-12.6", "-11.4", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(50);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(250);
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "C2状态", "CP频率(Hz)", "980", "1020", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                       //ProcessDataTmp(Data_Tmp, "C状态", "CP占空比(%)1", "10", "90");
                    if (Customer != null && Customer.Equals("NT"))
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString(), dImgs);
                    else
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    string customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    ///TRW客户要求每隔5秒再测一次,共测3次
                    if (customer != null && customer.Equals("TRW"))
                    {
                        Thread.Sleep(5000);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP频率(Hz)2", "980", "1020");
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                           //ProcessDataTmp(Data_Tmp, "C状态", "CP占空比(%)2", "10", "90");
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP占空比(%)2", CPDutyMin.ToString(), CPDutyMax.ToString());

                        Thread.Sleep(5000);

                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP频率(Hz)3", "980", "1020");

                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                           //ProcessDataTmp(Data_Tmp, "C状态", "CP占空比(%)3", "10", "90");
                        ProcessDataTmp(Data_Tmp, "C2状态", "CP占空比(%)3", CPDutyMin.ToString(), CPDutyMax.ToString());
                    }
                    //上升沿下降沿触发闭合，关闭滤波功能
                    lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);

                    if (Customer != null && Customer.Equals("HYQCP"))
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                        Thread.Sleep(waitTime);
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "C2状态", "CP上升时间(us)", "0", "7", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    if (Customer != null && Customer.Equals("NT"))
                        ProcessDataTmp(dd, "C2状态", "CP下降时间(us)", "0", "13", dImgs);
                    else
                        ProcessDataTmp(dd, "C2状态", "CP下降时间(us)", "0", "13");

                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                }

                catch (Exception ex)
                {
                    SendException(ex);
                }
            }
        }

        public override void ProcessData()
        {

        }

    }
}


