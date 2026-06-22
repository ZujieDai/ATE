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
    /// 国标研测交流：断开开关S2测试
    /// </summary>
    public class GB_RT_AC_S2ControlTesting : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        int SleepTime = 100;
        /// <summary>
        /// K1 K2断开时间
        /// </summary>
        double BreakTime = 0;//
        /// <summary>
        /// 负载需求电流
        /// </summary>
        double DemandCurrent = 0;

        double DemandVoltage = 220;
        /// <summary>
        /// 示波器波形回读数据
        /// </summary>
        Dictionary<int, double[]> dicOscilloscopeCursorData = new Dictionary<int, double[]>();
        public GB_RT_AC_S2ControlTesting(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            try
            {
                string[] strParams = TrialItem.ResultParams.Split('|');
                //BreakTime = double.Parse(strParams[0].Split('=')[1]);
                //DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                string[] strItemParams = TrialItem.ItemParams.Split('|');
                if (strItemParams.Length >= 1 && strItemParams[0].Split('=').Length > 1)
                {
                    SleepTime = Convert.ToInt32(double.Parse(strItemParams[0].Split('=')[1]));
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //设置测试条件
                SetConditionValues();

                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 50;
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);

                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
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
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 1);//均方根
                    Thread.Sleep(waitTime);

                    //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Auto");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "3");//低值
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    Thread.Sleep(5000);

                    SendNoticeToUIAndTxtFile("断开S2开关");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);

                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    int timeout = 10;
                    //    double duty = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle;
                    //    while (timeout > 0)
                    //    {
                    //        if (duty >= 2 && duty <= 96)
                    //        {
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            Thread.Sleep(300);
                    //            duty = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle;
                    //        }
                    //    }
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], duty.ToString());
                    //}
                    //ProcessDataTmp(Data_Tmp, "S2断开", "CP占空比(%)", "2", "96");

                    SendNoticeToUIAndTxtFile("闭合S2开关");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    Thread.Sleep(1500);

                    SendNoticeToUIAndTxtFile("分析示波器数据");
                    //泰克示波器需要等待5秒以上才可以绘制出波形图
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null
                        || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope_MDO34")) != null)
                        Thread.Sleep(4000);
                    else
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(300);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1, 1250000.0);//CP动作时间
                    ACDownTime_FirstHalf(testWorkParam.lstIDs, 1, 100, 2, 1250000.0, 5);//AC回路动作时间

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(Data_Tmp, "S2断开", "CP占空比(%)", "2", "96");
                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "S2断开", "K1K2断开时间(ms)", "0", "100", dImgs);


                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8.2, 1, 1, 1250000.0, 4);
                    ACUpTime_LatterHalf(testWorkParam.lstIDs, 1, 50, 2, 1250000.0, 5);//判断交流电压变化时刻

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "S2闭合", "K1K2闭合时间(ms)", "0", "3000", dImgs);

                    Data_Tmp.Clear();

                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                        int waiteTime = 25;
                        while (waiteTime > 0)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                            if (volt < 50)
                            {
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                        }
                        volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                        Data_Tmp.Add(itmp, volt.ToString());
                    }
                    //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 1);
                    ProcessDataTmp(Data_Tmp, "S2闭合", "闭合后交流电压(V)", "50", "-");
                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    int timeout = 10;
                    //    double freq = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency;
                    //    while (timeout > 0)
                    //    {
                    //        if (freq >= 970 && freq <= 1030)
                    //        {
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            Thread.Sleep(300);
                    //            freq = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency;
                    //        }
                    //    }
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], freq.ToString());
                    //}
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    ProcessDataTmp(Data_Tmp, "S2闭合", "CP频率(Hz)", "970", "1030");
                    //Data_Tmp.Clear();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    int timeout = 10;
                    //    double duty = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle;
                    //    while (timeout > 0)
                    //    {
                    //        if (duty >= 2 && duty <= 96)
                    //        {
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            Thread.Sleep(300);
                    //            duty = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle;
                    //        }
                    //    }
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], duty.ToString());
                    //}
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(Data_Tmp, "S2闭合", "CP占空比(%)", "2", "96");
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
