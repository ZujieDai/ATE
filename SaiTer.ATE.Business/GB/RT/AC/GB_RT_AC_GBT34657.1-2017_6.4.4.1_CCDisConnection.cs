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
    /// 国标研测交流：CC断线测试
    /// </summary>
    public class GB_RT_AC_CCDisConnection : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        /// <summary>
        /// CC断开后的延时时间
        /// </summary>
        int sleepTime = 2000;
        private string maxValue = "100";
        public GB_RT_AC_CCDisConnection(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();

            string[] ItemParams = TrialItem.ItemParams.Split('|')[0].Split('=');
            if (ItemParams.Length == 2)
            {
                sleepTime = Convert.ToInt32(ItemParams[1]);
            }
            string[] TrialParams = TrialItem.ResultParams.Split('|')[0].Split('=');

            if (TrialParams.Length == 2)
            {
                maxValue = TrialParams[1];
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
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage > 10)
                    {
                        List<bool> ks = GetKStatus16_Charging();
                        ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, ks);
                    }
                }

                //设置测试条件
                SetConditionValues();


                #region 充电前CC断线
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                Thread.Sleep(sleepTime);
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                Thread.Sleep(sleepTime);
                //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 3, "10.5", "Single");//上升边沿触发，自动
                Thread.Sleep(sleepTime);
                //设置时基400ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1");//低值
                Thread.Sleep(sleepTime);
                SendNoticeToUIAndTxtFile("启动示波器");
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);
                Thread.Sleep(2000);

                //模拟CC断线（CC断线之后PE还是通路所以CP电压不会变，需要把PE一起断开）
                List<bool> Ks = GetKStatus16_Charging();
                Ks[2] = false;
                Ks[5] = false;//PE
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                Thread.Sleep(sleepTime);
                SendNoticeToUIAndTxtFile("正在分析示波器数据");
                //读取分析数据
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(1000);
                //CCTime(testWorkParam.lstIDs, 3, -10.8, 2, 1);//CC动作时间
                CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1, 2);//CC动作时间
                CPPWMDownTime(testWorkParam.lstIDs, 3, -10.8, 2, 1);//CP纹波消失时间

                //读取卡点时间
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                Data_Tmp = GetOSCTime(OscTime_Tmp);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, "CC断线（充电前）", "S1开关切换时间(ms)", "0", maxValue, dImgs);

                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "CC断线（充电前）", "CP电压(V)", "11.2", "12.8");
                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency.ToString());
                }
                ProcessDataTmp(Data_Tmp, "CC断线（充电前）", "CP频率(Hz)", "-", "-");
                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
                }
                ProcessDataTmp(Data_Tmp, "CC断线（充电前）", "CP占空比(%)", "-", "-");
                foreach (int item in testWorkParam.lstIDs)
                {
                    string state = AllEquipStateData.DicBMS_AC_StateData[item].SystemState;
                    ProcessDataResult(testWorkParam.lstIDs, state, "充电状态", state != "充电中", "CC断线（充电前）");
                }
                CountDownTimeInfo("充电接口CC线断开后后，充电设备的锁止装置必须解锁供电插头。\r\n(注:勾选上为已解锁)", 20, 2);
                if (DicManualVerifyResult.First().Value)
                    ProcessDataResult(testWorkParam.lstIDs, "已解锁", "锁止装置解锁", true, $"CC断线（充电前）");
                else
                    ProcessDataResult(testWorkParam.lstIDs, "未解锁", "锁止装置解锁", false, $"CC断线（充电前）");
                #endregion

                Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);

                #region 充电中CC断线
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                ////检测刷卡
                //WaitSwipingCard(testWorkParam.lstIDs, 0);


                if (testWorkParam.lstIDs.Count > 0)
                {
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "10", "1.2");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1.5");//低值
                    Thread.Sleep(100);
                    SendNoticeToUIAndTxtFile("添加示波器测量项");
                    //添加测量值
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    //Thread.Sleep(100);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    //Thread.Sleep(100);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    //Thread.Sleep(100);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    //Thread.Sleep(100);

                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "10.8", "Single");//上升边沿触发，自动
                    Thread.Sleep(100);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    SendNoticeToUIAndTxtFile("开始充电");

                    //闭合开关S2，启动充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //检测是否充电
                    WaitSwipingCard(testWorkParam.lstIDs, 0);
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
                    //输出电压时可能出现杂波导致提前触发
                    if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                        Thread.Sleep(300);
                        ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                        Thread.Sleep(300);
                    }

                    Thread.Sleep(3500);
                    SendNoticeToUIAndTxtFile("模拟导引CP断线");

                    //模拟CC断线（CC断线之后PE还是通路所以CP电压不会变，需要把PE一起断开）
                    Ks = GetKStatus16_Charging();
                    Ks[2] = false;
                    Ks[5] = false;//PE
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Thread.Sleep(sleepTime);
                    SendNoticeToUIAndTxtFile("正在分析示波器数据");
                    //读取分析数据
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    CPPWMDownTime(testWorkParam.lstIDs, 3, -10.8, 1, 1);//CP纹波消失时间
                    ACDownTime(testWorkParam.lstIDs, 1, 55, 2);//AC回路动作时间

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "CC断线（充电中）", "K1K2断开时间(ms)", "0", maxValue, dImgs);

                    //断线后的电压
                    //Data_Tmp = new Dictionary<int, string>();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    //}
                    //ProcessDataTmp(Data_Tmp, "CC断线（充电中）", "断线后输出电压(V)", "0", "20");

                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "CC断线（充电中）", "CP电压(V)", "11.2", "12.8");
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "CC断线（充电中）", "CP频率(Hz)", "-", "-");
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "CC断线（充电中）", "CP占空比(%)", "-", "-");
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        string state = AllEquipStateData.DicBMS_AC_StateData[item].SystemState;
                        ProcessDataResult(testWorkParam.lstIDs, state, "充电状态", state != "充电中", "CC断线（充电中）");
                    }
                    CountDownTimeInfo("充电接口CC线断开后后，充电设备的锁止装置必须解锁供电插头。\r\n(注:勾选上为已解锁)", 20, 2);
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "已解锁", "锁止装置解锁", true, $"CC断线（充电中）");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "未解锁", "锁止装置解锁", false, $"CC断线（充电中）");
                    #endregion
                }
            }
        }

        public override void ProcessData()
        {

        }
    }
}
