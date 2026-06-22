using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 保护接地连续性测试（PE断针测试）
    /// </summary>
    public class PEDisConnection : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string disConnectionTime = "3000";//断线上限时间
        private int MaxVolt = 20;
        public PEDisConnection(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            disConnectionTime = strParams[0].Split('=')[1];
            if (strParams.Length > 1)
            {
                MaxVolt = (int)Convert.ToDouble(strParams[1].Split('=')[1]);
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
                List<bool> Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
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

                //升源，启动BMS
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                        //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }

                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                ////检测刷卡
                //WaitSwipingCard(testWorkParam.lstIDs, 0);


                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);

                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "");
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

                    string ItemFlow = "PE断线";
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (!string.IsNullOrEmpty(Customer) && Customer.Equals("NT"))
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "10.8", "Single");//上升边沿触发，自动
                        Thread.Sleep(waitTime);
                        //设置时基400ms
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1.5");//低值
                        Thread.Sleep(waitTime);
                        //启动示波器
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                        Thread.Sleep(waitTime);

                        List<bool> ks = GetKStatus16_Charging();
                        ks[0] = false;
                        SendNoticeToUIAndTxtFile("充电前准备就绪");
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);
                        Thread.Sleep(1000);

                        ks = GetKStatus16_Charging();
                        ks[2] = false;  //CC也要断开回路，不然CP电压到不了12V
                        ks[3] = false;
                        ks[5] = false;
                        SendNoticeToUIAndTxtFile("发送断线");
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);
                        Thread.Sleep(2000);
                        ItemFlow = "PE断线(充电前)";

                        //断线后的电压
                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
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
                        ProcessDataTmp(Data_Tmp, ItemFlow, "断线后输出电压(V)", "0", MaxVolt.ToString(), dImgs);

                        string info = $"请检查充电桩是否符合PE断线(充电前)的标准。\r\n注：勾选上为PASS，否则为FAIL";
                        CountDownTimeInfo(info, 20, 2);
                        if (DicManualVerifyResult.First().Value)
                            ProcessDataResult(testWorkParam.lstIDs, "是", "是否符合标准", true, ItemFlow);
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "否", "是否符合标准", false, ItemFlow);

                        ks = GetKStatus16_Charging();
                        ks[0] = true;
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);

                        ItemFlow = "PE断线(充电中)";
                    }

                    //闭合开关S2，启动充电

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "10.8", "Single");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(waitTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1.5");//低值
                    Thread.Sleep(waitTime);


                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
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

                    Thread.Sleep(5000);

                    //设置测试条件
                    SetConditionValues();

                    //模拟PE断线（其实为PE断针）
                    List<bool> Ks = GetKStatus16_Charging();
                    Ks[2] = false;  //CC也要断开回路，不然CP电压到不了12V
                    Ks[5] = false;
                    SendNoticeToUIAndTxtFile("发送断线");
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Thread.Sleep(3000);
                    if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
                    {
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    }
                    //读取分析数据
                    Thread.Sleep(1000);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1);//CP动作时间
                    ACDownTime(testWorkParam.lstIDs, 1, 55, 2);//AC回路动作时间

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, ItemFlow, "K1K2断开时间(ms)", "0", disConnectionTime, dImgs);

                    //断线后的电压
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
                    ProcessDataTmp(Data_Tmp, ItemFlow, "断线后输出电压(V)", "0", MaxVolt.ToString());

                    if (!string.IsNullOrEmpty(Customer) && Customer.Equals("NT"))
                    {
                        SetCPReresh();
                        if (!CheckSwipingCard(testWorkParam.lstIDs))
                        {
                            return;
                        }
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "10.8", "Single");//上升边沿触发，自动
                        Thread.Sleep(waitTime);
                        //启动示波器
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "");
                        Thread.Sleep(waitTime);

                        //等待10秒模拟充满电
                        SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                        Thread.Sleep(2000);
                        SendNoticeToUIAndTxtFile("充电中，等待BMS充满电");
                        Thread.Sleep(1000 * 20);
                        List<bool> ks = GetKStatus16_Charging();
                        ks[0] = false;
                        SendNoticeToUIAndTxtFile("充电结束，BMS充满电");
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);
                        Thread.Sleep(1000);

                        ks = GetKStatus16_Charging();
                        ks[0] = false;
                        ks[2] = false;  //CC也要断开回路，不然CP电压到不了12V
                        ks[5] = false;
                        SendNoticeToUIAndTxtFile("发送断线");
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);
                        Thread.Sleep(2000);
                        ItemFlow = "PE断线(充满电)";

                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        //断线后的电压
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
                        ProcessDataTmp(Data_Tmp, ItemFlow, "断线后输出电压(V)", "0", MaxVolt.ToString(), dImgs);

                        string info = $"请检查充电桩是否符合PE断线(充满电)的标准。\r\n注：勾选上为PASS，否则为FAIL";
                        CountDownTimeInfo(info, 20, 2);
                        if (DicManualVerifyResult.First().Value)
                            ProcessDataResult(testWorkParam.lstIDs, "是", "是否符合标准", true, ItemFlow);
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "否", "是否符合标准", false, ItemFlow);
                    }
                }
            }
        }



        public override void ProcessData()
        {

        }


    }
}
