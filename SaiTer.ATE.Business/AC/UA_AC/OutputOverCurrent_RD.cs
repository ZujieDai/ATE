using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出过流测试（研测）
    /// </summary>
    public class OutputOverCurrent_RD : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double OutputCurrent = 40;
        double DemandCurrent = 30;
        string sState = "";
        private int MaxVolt = 20;
        public OutputOverCurrent_RD(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputCurrent = Convert.ToDouble(strParams[0].Split('=')[1]);
            DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            if (strParams.Length > 2)
            {
                MaxVolt = (int)Convert.ToDouble(strParams[2].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                SetCPReresh();
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + DemandCurrent + "|" + OutputCurrent + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                //    {
                //        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                //        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //    }
                //}
                //设置测试条件
                SetConditionValues();

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                    SendNoticeToUIAndTxtFile("检测充电桩充电状态。");
                    int waitTime = 50;
                    //需要计算输出电流的设定值，让示波器的输出电流保持在三格的范围
                    string OutputCurrentScale = "60";
                    if (OutputCurrent * 1.4 * 2 < 30)
                        OutputCurrentScale = "10";
                    else if (OutputCurrent * 1.4 * 2 < 60)
                        OutputCurrentScale = "20";
                    else if (OutputCurrent * 1.4 * 2 < 120)
                        OutputCurrentScale = "40";

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.0");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "500", "0.3");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_Current", "50", "A", false, OutputCurrentScale, "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "-1.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(waitTime);

                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 1);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 1);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 1);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 2);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 2);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 2);//低值
                    Thread.Sleep(waitTime);

                    //设置触发，AC特性有峰值需要*1.4
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 2, ((OutputCurrent + DemandCurrent + 2.0) / 2.0 * 1.4).ToString("F2"), "Single");//上升边沿触发，自动
                    //double triggerCurrent = (OutputCurrent + DemandCurrent) / 2.0 * 1.4 - 1.0;
                    double triggerCurrent = OutputCurrent * 1.414 * 0.9;
                    if(DemandCurrent > triggerCurrent - 3)
                        triggerCurrent = OutputCurrent * 1.414 * 0.95;
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 2, triggerCurrent.ToString("F2"), "Single");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    //设置时基500ms
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null
                        && Customer != null && (!Customer.ToString().ToUpper().Contains("WR") && !Customer.ToString().ToUpper().Contains("HYQC")))   //WR的泰克示波器只有十格
                    {
                        //泰克示波器是横向16格，这里用1s的时基
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "3");//低值
                    }
                    else
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "2000", "4");//低值
                    }
                    Thread.Sleep(waitTime);

                    Thread.Sleep(1000);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    Thread.Sleep(1000);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                        {
                            SendNoticeToUIAndTxtFile("未检测到" + testWorkParam.lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                            testWorkParam.lstIDs.Remove(testWorkParam.lstIDs[i]);
                        }
                    }

                    //无充电桩在测试，直接退出 
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }

                    Thread.Sleep(2000);

                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage,DemandCurrent); 

                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                    SendNoticeToUIAndTxtFile("已发送负载正常电流值：" + DemandCurrent + "A，等待负载稳定。");
                    Thread.Sleep(8000);

                    //监测是否需要停止充电，根据GB/T 18487-2023 A.3.10.4PWM信号停止后3s内停止充电，然后3s内需要断开S2（广州YT测试刚好不通过，会受到导引没停止充电的影响）
                    if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC)
                    {
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                int time = 600;
                                while (time-- > 0)
                                {
                                    var CPFrequency = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency;
                                    if (CPFrequency < 10)
                                    {
                                        time -= 3;
                                        Thread.Sleep(300);
                                        CPFrequency = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency;
                                        if (CPFrequency < 10)
                                        {
                                            if (Customer != null && Customer.ToString().ToUpper().Contains("HYQC"))
                                                SendNoticeToUIAndTxtFile("检测到PWM消失，根据A.3.10.4PWM信号停止后3s内停止充电，然后3s内需要断开S2");
                                            ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                                            Thread.Sleep(3000);
                                            ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                                            return;
                                        }
                                    }
                                    Thread.Sleep(100);
                                }
                            });
                        }
                    }
                    //SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    //SendNoticeToUIAndTxtFile("已发送负载过流值：" + OutputCurrent + "A，等待负载稳定。");
                    if (TrialType == (int)EmTrialType.不动作电流测试)
                    {
                        SendNoticeToUIAndTxtFile("已发送负载不动作电流值：" + OutputCurrent + "A，等待负载稳定。");
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("已发送负载过流值：" + OutputCurrent + "A，等待负载稳定。");
                    }
                    Thread.Sleep(20000);

                    if (TrialType == (int)EmTrialType.不动作电流测试)
                    {
                        sState = "不动作电流测试";
                    }
                    else
                    {
                        sState = "输出过流测试";
                    }
                    if (Customer != null && Customer.ToString().ToUpper().Contains("HYQC"))
                    {
                        var Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);
                        var dic = new Dictionary<int, string>();
                        foreach (int item in Data_Tmp.Keys)
                        {
                            dic.Add(item, (Convert.ToDouble(Data_Tmp[item]) / Math.Sqrt(2)).ToString("F2"));
                        }
                        ProcessDataTmp(dic, sState, "最大电流(A)", "-", "-");
                    }

                    //采集数据
                    SendNoticeToUIAndTxtFile("正在分析示波器数据");
                    //泰克示波器需要等待5秒以上才可以绘制出波形图
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                        Thread.Sleep(15000);
                    else if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope_MDO34")) != null)
                        Thread.Sleep(5000);
                    else
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                        Thread.Sleep(1000);
                    }
                    //读取分析数据（卡点顺序不能更改，HY的示波器会在读数据的时候报错，原因未知）
                    ACUpTime(testWorkParam.lstIDs, 2, triggerCurrent * 1.05, 1, 1250000.0, 1);//AC回路动作时间
                    ACDownTime(testWorkParam.lstIDs, 1, 60, 2, 1250000.0, 2);//42.4*根号2
                    //ACUpTime(testWorkParam.lstIDs, 2, triggerCurrent, 1);//AC回路动作时间
                    //ACDownTime(testWorkParam.lstIDs, 1, 60, 2);//42.4*根号2
                    //CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1);//CP动作时间

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, sState, "输出电源断开时间(ms)", "5000", "10000", dImgs);

                    //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    //ProcessDataTmp(Data_Tmp, sState, "CP电压(V)", "11.4", "12.6");

                    //Data_Tmp = new Dictionary<int, string>();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
                    //}
                    //ProcessDataTmp(Data_Tmp, sState, "CP占空比(%)", "10", "90");

                    //CountDownTimeInfo("请确认供电接口电子锁是否正确解锁（勾选上代表是）", 20, 2);
                    //ProcessData();

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int timeout = 20;
                        double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        while (timeout-- > 0)
                        {
                            if (volt > 0 && volt < MaxVolt)
                                break;
                            Thread.Sleep(300);
                            volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString("F2"));
                    }
                    //ProcessDataTmp(Data_Tmp, "输出过流测试", "保护后桩输出电压(V)", "0", "30");

                    if (TrialType == (int)EmTrialType.不动作电流测试)
                    {
                        ProcessDataTmp(Data_Tmp, "不动作电流测试", "桩输出电压(V)", "80", "260");
                    }
                    else
                    {
                        ProcessDataTmp(Data_Tmp, "输出过流测试", "保护后桩输出电压(V)", "0", MaxVolt.ToString());
                    }

                    SendNoticeToUIAndTxtFile("关闭负载。重启交流源恢复桩故障");

                    ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                }
            }
        }


        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                    string strResult = "否";
                    if (DicManualVerifyResult.Values.FirstOrDefault())
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "是";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = sState + "|是否解锁|-|-|" + strResult;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }
            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
