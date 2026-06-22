using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 目前测是S2开关断开闭合
    /// </summary>
    public class S2ControlTesting : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
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
        public S2ControlTesting(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BreakTime = double.Parse(strParams[0].Split('=')[1]);
            //DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
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
               // SetCPReresh();
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

            
             


                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 50;
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
                    Thread.Sleep(waitTime);
                   
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(waitTime);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Auto");//上升边沿触发，自动
                    Thread.Sleep(waitTime);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


                    

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

                    Thread.Sleep(7000);

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
                      
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);
                    SendNoticeToUIAndTxtFile("断开S2开关");
                    //模拟S2断开
                    //List<bool> Ks = GetKStatus16_Charging();
                    //Ks[0] = false;
                    //ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                   ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("分析示波器数据");

                    //读取分析数据
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1);//CP动作时间
                    ACDownTime(testWorkParam.lstIDs, 1, 100, 2);//AC回路动作时间

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) ).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "S2断开", "K1K2断开时间(ms)", "0", "3000", dImgs);



                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "-10", 3, "6.8", "Single");//上升边沿触发，自动
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "-10", 3, "6.8", "Auto");//上升边沿触发，自动
                    Thread.Sleep(waitTime);

                    Thread.Sleep(3000);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    SendNoticeToUIAndTxtFile("模拟S2闭合");

                    //模拟S2闭合
                    //List<bool> Ks = GetKStatus16_Charging();
                    //Ks[0] = false;
                    //ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    Thread.Sleep(3000);//TRW项目只延时3秒
                    //Thread.Sleep(3000);//研发样机再加3秒
                    SendNoticeToUIAndTxtFile("分析示波器数据");

                    //读取分析数据
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    CPPWMDownTime(testWorkParam.lstIDs, 3, 8, 1);//CP动作时间
                    ACUpTime(testWorkParam.lstIDs, 1, 30, 2);//AC回路动作时间

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
