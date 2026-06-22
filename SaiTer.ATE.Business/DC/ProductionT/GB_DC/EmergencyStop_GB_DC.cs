using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 紧急停机保护测试（急停按钮/开门保护）测示波器停机时间
    /// </summary>
    public class EmergencyStop_GB_DC : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double maxTime = 1000;
        public EmergencyStop_GB_DC(int type) { TrialType = type; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            maxTime = double.Parse(strParams[0].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_OFF(lstIDs);
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
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
                                //
                                LstTrialData[i].ExtentData = "-|-|-|-|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("启动充电");

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }


                SendNoticeToUIAndTxtFile("启动负载,并等待带载稳定");
                if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(500);
                   SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(1000 * 15);
                }
                else if (ControlEquipMent.ResistanceLoad != null)
                {
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 20);
                }
                int sleepTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);                                               //3通道  关闭  耦合  带宽  探头比   标签      阻抗 电压  反向通道  纵坐标档位  纵坐标位置
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "2.5");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                Thread.Sleep(sleepTime);

                double trigger_voltage = 0;
                if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                {
                    trigger_voltage = 42.4;
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "50", "0");//时基   延时
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "50", 1, "0", "Single");//下降边沿触发，单次
                    Thread.Sleep(300);
                }
                else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0");//时基   延时
                    Thread.Sleep(sleepTime);
                    trigger_voltage = 60;
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "7.5", 1, trigger_voltage.ToString(), "Single");//下降边沿触发，单次
                    Thread.Sleep(300);
                    Dictionary<int, bool> dicTemp = new Dictionary<int, bool>();
                    dicTemp = ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs);
                    int count = 5;
                    while (count-- > 0)
                    {
                        if (dicTemp[1])
                        {

                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "7.5", 1, trigger_voltage.ToString(), "Single");//下降边沿触发，单次
                            Thread.Sleep(500);
                        }
                        else
                        { break; }

                    }
                }

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


                string info1 = "", info2 = "", info3 = "", sState = "";
                if ((EmTrialType)TrialType == EmTrialType.紧急停机保护测试 || (EmTrialType)TrialType == EmTrialType.启动急停装置试验)
                {
                    info1 = "请按下急停按钮";
                    info2 = "请按下充电桩急停按钮。然后点击【确认】,或倒计时结束后自动判断";
                    info3 = "请恢复急停按钮";
                    sState = "紧急停机保护测试";
                }
                else if ((EmTrialType)TrialType == EmTrialType.开门保护测试)
                {
                    info1 = "请模拟开门";
                    info2 = "请模拟开门操作,然后点击【确认】,或倒计时结束后自动判断";
                    info3 = "请关门";
                    sState = "开门保护测试";
                }
                SendNoticeToUIAndTxtFile(info1);


                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(3000);
                SendNoticeToUIAndTxtFile("计算示波器数据");
                //分析波形数据
                ACDownTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 20, 1);
                ACDownTime(testWorkParam.lstIDs, 1, trigger_voltage, 2);

                //读取卡点时间
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                Data_Tmp = GetOSCTime(OscTime_Tmp);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, sState, $"电压降至{trigger_voltage}V以下时间(ms)", "0", maxTime.ToString(), dImgs);



                Dictionary<int, string> dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    string voltage = "0";
                    if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                    {
                        voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString();
                    }
                    else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                    {
                        voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString();
                    }
                    if (dicData.ContainsKey(testWorkParam.lstIDs[i]))
                    {
                        dicData[testWorkParam.lstIDs[i]] = voltage;
                    }
                    else
                    {
                        dicData.Add(testWorkParam.lstIDs[i], voltage);
                    }
                }
                ProcessDataTmp(dicData, sState, "保护后电压值", "0", "20");
                CountDownTimeInfo(info3, 60, 0);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
