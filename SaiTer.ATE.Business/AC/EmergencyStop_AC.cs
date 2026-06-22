using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
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
    /// 紧急停机保护测试（急停按钮/开门保护）     测示波器停机时间   程控板控制示波器4通道切换急停信号或者输入电压信号
    /// </summary>
    public class EmergencyStop_AC : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double maxTime = 1000;
        public EmergencyStop_AC(int type) { TrialType = type; }


        public override void InitEquiMent()
        {
            //程控板K7继电器闭合的时候是切换到桩急停信号采样
            var list = ControlEquipMent.ControlBoard.ControlBoardReadState();
            Thread.Sleep(500);
            list[6] = true;
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
        }

        public override void InitializeParams()
        {
            Init();
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
                var list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                list[6] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                Thread.Sleep(500);

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

                string info1 = "", info2 = "", info3 = "", sState = "";
                if ((EmTrialType)TrialType == EmTrialType.紧急停机保护测试)
                {
                    info1 = "请按下急停按钮";
                    info2 = "请按下充电桩急停按钮,然后点击确认或倒计时结束后自动判断";
                    info3 = "请恢复急停按钮";
                    sState = "紧急停机保护测试";
                }
                else if ((EmTrialType)TrialType == EmTrialType.开门保护测试)
                {
                    info1 = "请模拟开门";
                    info2 = "请模拟开门操作,然后点击确认或倒计时结束后自动判断";
                    info3 = "请关门";
                    sState = "开门保护测试";
                }
                Dictionary<int, string> dicData = new Dictionary<int, string>();

                SendNoticeToUIAndTxtFile("启动充电");

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                SendNoticeToUIAndTxtFile("启动负载,并等待带载稳定");

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 10);
                Thread.Sleep(3000);
                //设置测试条件
                SetConditionValues();


                if ((EmTrialType)TrialType == EmTrialType.紧急停机保护测试)
                {
                    CountDownTimeInfo("请选择桩是否可以外引急停信号（勾选上为可以）", 20, 2);
                    if (!DicManualVerifyResult[testWorkParam.lstIDs[0]])//不能外引信号，测电压是否下降即可
                    {
                        SendNoticeToUIAndTxtFile(info1);
                        CountDownTimeInfo(info2, 60, 0);
                        Thread.Sleep(5000);

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
                        ProcessDataTmp(dicData, sState, "电压值", "0", "20");
                        CountDownTimeInfo(info3, 60, 0);
                        return;
                    }
                    else
                    {
                        CountDownTimeInfo("请确认桩急停信号电平变化方式是否为从高到低（勾选上代表是）", 20, 2);
                        bool isDown = DicManualVerifyResult[testWorkParam.lstIDs[0]];     //信号电平变化方式是否为从高到底
                        int waitTime = 50;
                        //初始化示波器
                        SendNoticeToUIAndTxtFile("初始化示波器通道");
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "AC-out-A", "50", "V", false, "50", "2.5");
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP", "50", "V", false, "10", "0");
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, true, "DC", "0.25", Channel4, "Emergency", "50", "V", false, "2.5", "-2.5");
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "AC-out-V", "50", "V", false, "250", "2.5");
                        Thread.Sleep(waitTime);
                        //添加测量值
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 4);//均方根
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                        Thread.Sleep(waitTime);
                        ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                        Thread.Sleep(waitTime);
                        //设置时基400ms
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");
                        Thread.Sleep(waitTime);
                        double value = 0;
                        if (DicManualVerifyResult[testWorkParam.lstIDs[0]])//电平从高到低， 下降沿触发
                        {
                            //ZD的急停信号，最高才2.8V，特征和交流信号一样
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "0", 4, "0.8", "Single");//下降边沿触发，
                            //value = 8;
                            //有的急停信号最高只有3V多一点
                            value = 2.5;
                        }
                        else
                        {
                            //ZD的急停信号，最高才2.8V，特征和交流信号一样
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "0", 4, "2.8", "Single");//上升边沿触发，
                            value = 3;
                        }

                        SendNoticeToUIAndTxtFile(info1);


                        CountDownTimeInfo(info2, 60, 0);
                        Thread.Sleep(3000);
                        SendNoticeToUIAndTxtFile("计算示波器数据");
                        if (isDown)
                        {
                            //分析波形数据
                            ACDownTime_ZD(testWorkParam.lstIDs, 4, value, 1);
                        }
                        else
                        {
                            Dictionary<int, double> tmp = new Dictionary<int, double>();
                            tmp.Add(1, 0.5);
                            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 4, tmp, true);
                        }
                        ACDownTime(testWorkParam.lstIDs, 1, 42.4, 2);

                        //读取卡点时间
                        ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                        Data_Tmp = GetOSCTime(OscTime_Tmp);
                        Dictionary<int, string> dd = new Dictionary<int, string>();
                        foreach (var itmp in Data_Tmp)
                        {
                            dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                        }
                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dd, sState, "电压降至42.4V以下时间(ms)", "0", maxTime.ToString(), dImgs);




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

                else if ((EmTrialType)TrialType == EmTrialType.开门保护测试)
                {
                    SendNoticeToUIAndTxtFile(info1);
                    CountDownTimeInfo(info2, 60, 0);
                    Thread.Sleep(5000);

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
                    ProcessDataTmp(dicData, sState, "电压值", "0", "20");
                    CountDownTimeInfo(info3, 60, 0);
                    return;
                }

            }

        }
        public override void ProcessData()
        {

        }
    }
}
