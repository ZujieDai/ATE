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
    internal class CPDisConnection_RT_AC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        /// <summary>
        /// CP断开后的延时时间
        /// </summary>
        private int sleepTime = 2000;

        private string maxValue = "100";
        public CPDisConnection_RT_AC(int trialType) { TrialType = trialType; }

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
                SetCPReresh();
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
                    ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                }

                //设置测试条件
                SetConditionValues();

                #region 充电前CP断线
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                Thread.Sleep(sleepTime);
                SendNoticeToUIAndTxtFile("添加示波器测量项");
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
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 3, "10.5", "Auto");//上升边沿触发，自动
                Thread.Sleep(sleepTime);
                SendNoticeToUIAndTxtFile("启动示波器");
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);

                ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                Thread.Sleep(sleepTime);
                //设置时基400ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1");//低值
                Thread.Sleep(2000);

                //模拟CP断线
                List<bool> Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                Thread.Sleep(sleepTime);
                SendNoticeToUIAndTxtFile("正在分析示波器数据");
                //读取分析数据
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(1000);
                CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1, 2);//CP动作时间
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
                ProcessDataTmp(dd, "CP断线（充电前）", "S1开关切换时间(ms)", "0", maxValue, dImgs);
                #endregion

                Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                #region 充电中CP断开
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
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
                    //Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    //Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.0");
                    //Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    //Thread.Sleep(waitTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "-1");//低值
                    Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    //Thread.Sleep(waitTime);


                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "50", 3, "10.8", "Auto");////超时触发，泰克示波器在设备层做了触发模式转换
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "50", 1, "100", "Auto");
                    Thread.Sleep(waitTime);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    SendNoticeToUIAndTxtFile("开始充电");

                    //美标没有C1状态，先刷卡再闭合
                    if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC)
                    {
                        WaitSwipingCard(testWorkParam.lstIDs, 2);
                    }
                    //闭合开关S2，启动充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //检测是否充电
                    WaitSwipingCard(testWorkParam.lstIDs, 0);

                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    //    {
                    //        SendNoticeToUIAndTxtFile("未检测到" + testWorkParam.lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    //        testWorkParam.lstIDs.Remove(testWorkParam.lstIDs[i]);
                    //    }
                    //}

                    ////无充电桩在测试，直接退出 
                    //if (testWorkParam.lstIDs.Count <= 0)
                    //{
                    //    return;
                    //}

                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");  //设置单次触发
                    Thread.Sleep(3500);

                    SendNoticeToUIAndTxtFile("模拟导引CP断线");

                    //模拟CP断线
                    Ks = GetKStatus16_Charging();
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("正在分析示波器数据");
                    //读取分析数据
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1);//CP动作时间
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
                    ProcessDataTmp(dd, "CP断线（充电中）", "S1开关切换时间(ms)", "0", maxValue, dImgs);

                    //断线后的电压
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "CP断线（充电中）", "断线后输出电压(V)", "0", "20");
                    #endregion
                }
            }
        }



        public override void ProcessData()
        {

        }


    }
}
