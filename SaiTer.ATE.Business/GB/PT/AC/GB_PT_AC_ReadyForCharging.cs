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
    /// 国标交流充电准备就绪
    /// </summary>
    public class GB_PT_AC_ReadyForCharging : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double CPDutyMin, CPDutyMax, CPVoltMin, CPVoltMax;

        double CPPWMUpMax = 10;
        double CPPWMDownMax = 13;
        double CPNegativeMin = -12.8, CPNegativeMax = -11.2; //CP负电压的上下限

        public GB_PT_AC_ReadyForCharging(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            try
            {
                Init();
                //占空比下限(%)=2.00|占空比上限(%)=96.00|CP电压下限(V)=11.2|CP电压上限(V)=12.8|CP负电压下限(V)=-12.8|CP负电压上限(V)=-11.2
                string[] strParams = TrialItem.ResultParams.Split('|');
                CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
                CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
                CPVoltMin = Convert.ToDouble(strParams[2].Split('=')[1]);
                CPVoltMax = Convert.ToDouble(strParams[3].Split('=')[1]);
                //CPPWMUpMax = Convert.ToDouble(strParams[2].Split('=')[1]);
                //CPPWMDownMax = Convert.ToDouble(strParams[3].Split('=')[1]);
                if (strParams.Length >= 6)
                {
                    CPNegativeMin = Convert.ToDouble(strParams[4].Split('=')[1]);
                    CPNegativeMax = Convert.ToDouble(strParams[5].Split('=')[1]);
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
                    int sleepTime = 50;

                    #region 具备S2开关的车辆
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
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    Thread.Sleep(sleepTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发，自动

                    //闭合CP,断开S2
                    var Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);


                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(sleepTime);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);
                    Data_Tmp.Clear();
                    //读取B2状态数据判断
                    // Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值                   
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP正电压(V)", "8.2", "9.8");
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MIN", 3);//低值
                    //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP负电压(V)", CPNegativeMin.ToString(), CPNegativeMax.ToString());
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP频率(Hz)", "970", "1030");
                    Data_Tmp.Clear();
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    ProcessDataTmp(Data_Tmp, "状态2’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    //上升沿下降沿触发闭合，关闭滤波功能
                    List<bool> lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);

                    //有的示波器测量值只能添加4个
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Equals("HYQCP"))
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                        Thread.Sleep(sleepTime);
                    }
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    Dictionary<int, string> dd = new Dictionary<int, string>();

                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(sleepTime);
                    //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 1, "100", "Single");//上升边沿触发，自动
                    Thread.Sleep(sleepTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "-1.5");//低值
                    Thread.Sleep(sleepTime);

                    SendNoticeToUIAndTxtFile("启动示波器，闭合导引S2开关，开始充电");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(2000);

                    //闭合开关S2，启动充电
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Data_Tmp.Clear();
                    int time = 100;
                    while (time-- > 0)
                    {
                        double CPVolt = 0;
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            CPVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                            if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                                Data_Tmp[testWorkParam.lstIDs[i]] = CPVolt.ToString();
                            else
                                Data_Tmp.Add(testWorkParam.lstIDs[i], CPVolt.ToString());
                        }

                        if (CPVolt >= 5.2 && CPVolt <= 6.8)
                            break;
                        else
                            Thread.Sleep(100);
                    }
                    ProcessDataTmp(Data_Tmp, "状态3", "CP电压(V)", "5.2", "6.8");


                    Thread.Sleep(3000);

                    //设置测试条件
                    d1.Clear();
                    d2.Clear();
                    d3.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                        d3.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[key].PhaseA_Current.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SetConditionValue("带载电流(A)", d3);
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.5", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");

                    //设置时基500ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1", "0");//低值
                    Thread.Sleep(sleepTime);
                    SendNoticeToUIAndTxtFile("添加示波器测量值");
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

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "0", "Auto");//边沿触发，自动

                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("启动示波器");

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    //读取C状态CP电压值
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP正电压(V)", "5.2", "6.8");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP负电压(V)", CPNegativeMin.ToString(), CPNegativeMax.ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP频率(Hz)", "970", "1030");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                       //ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)1", "10", "90");
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    //上升沿下降沿触发闭合，关闭滤波功能
                    lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);


                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);

                    #endregion

                    #region 不具备S2开关的车辆

                    //断开CP,S2
                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);


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
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(sleepTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                    Thread.Sleep(sleepTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发，自动

                    Thread.Sleep(1000);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);



                    //闭合CP,断开S2
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);


                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(sleepTime);

                    //闭合开关S2，启动充电
                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Data_Tmp.Clear();
                    time = 100;
                    while (time-- > 0)
                    {
                        double CPVolt = 0;
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            CPVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                            if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                                Data_Tmp[testWorkParam.lstIDs[i]] = CPVolt.ToString();
                            else
                                Data_Tmp.Add(testWorkParam.lstIDs[i], CPVolt.ToString());
                        }

                        if (CPVolt >= 5.2 && CPVolt <= 6.8)
                            break;
                        else
                            Thread.Sleep(100);
                    }
                    ProcessDataTmp(Data_Tmp, "状态3", "CP电压(V)", "5.2", "6.8");


                    //闭合开关S2，启动充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //检测是否充电
                    WaitSwipingCard(testWorkParam.lstIDs, 0);

                    Thread.Sleep(3000);

                    //设置测试条件
                    d1.Clear();
                    d2.Clear();
                    d3.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                        d3.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[key].PhaseA_Current.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SetConditionValue("带载电流(A)", d3);
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.5", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");

                    //设置时基500ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1", "0");//低值
                    Thread.Sleep(sleepTime);
                    SendNoticeToUIAndTxtFile("添加示波器测量值");
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

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "0", "Auto");//边沿触发，自动

                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("启动示波器");

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    //读取C状态CP电压值
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP正电压(V)", "5.2", "6.8");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP负电压(V)", CPNegativeMin.ToString(), CPNegativeMax.ToString());

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP频率(Hz)", "970", "1030");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                                                                                                                       //ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)1", "10", "90");
                    ProcessDataTmp(Data_Tmp, "状态3’", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                    //上升沿下降沿触发闭合，关闭滤波功能
                    lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);


                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);

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
