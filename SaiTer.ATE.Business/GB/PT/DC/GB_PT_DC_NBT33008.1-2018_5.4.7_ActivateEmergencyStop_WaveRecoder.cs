using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标产测直流：启动急停装置试验(新录波板)
    /// </summary>
    public class GB_PT_DC_ActivateEmergencyStop_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 20;

        /// <summary>
        /// 切换急停信号开关的索引号
        /// </summary>
        int SignIndex = 6;

        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间


        public GB_PT_DC_ActivateEmergencyStop_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            //急停信号开关=7
            string[] strParams = TrialItem.ItemParams.Split('|');
            if (strParams.Length > 0 && strParams[0].Split('=').Length > 1)
            {
                SignIndex = Convert.ToInt32(strParams[0].Split('=')[1]) - 1;
            }

            DemandVoltage = LstChargerInfo[0].NominalVoltage;
            DemandCurrent = LstChargerInfo[0].NominalCurrent;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");
            //切换急停信号开关
            List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
            list[SignIndex] = true;
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstIDs, list);

            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            int waitTime = 50;
            ////初始化示波器
            //SendNoticeToUIAndTxtFile("初始化示波器通道");
            //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//清除所有测量项
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, false, "DC", "20", Channel2, "AC-out-A", "50", "V", false, "50", "2.5");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "DC", "20", Channel3, "CP", "50", "V", false, "10", "0");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, true, "DC", "20", "1", "Emergency", "50", "V", false, "10", "-2.5");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20", Channel1, "AC-out-V", "50", "V", false, "250", "0");
            //Thread.Sleep(waitTime);
            ////添加测量值
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 4);//均方根
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "TOP", 3);//高值
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "BASE", 3);//低值
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(lstIDs, "250k");
            //Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(lstIDs, "X");
            //Thread.Sleep(waitTime);
            ////设置时基400ms
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "200", "0");

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

            Thread.Sleep(waitTime);
        }

        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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
                //切换急停信号开关
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
                list[SignIndex] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstIDs, list);
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
        {
            try
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
                    //设置测试条件
                    SetConditionValues();


                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);


                    //SendNoticeToUIAndTxtFile("启动充电中");
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    SendNoticeToUIAndTxtFile("带载8秒中...");
                    Thread.Sleep(8000);

                    //SendNoticeToUIAndTxtFile("设置触发...");
                    //var EStop_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 4);//低值
                    //double TriggerCurrent = 6;
                    //if (Convert.ToDouble(EStop_Tmp.Values.FirstOrDefault()) < 0)
                    //{
                    //    TriggerCurrent = -6;
                    //    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "0", 4, TriggerCurrent.ToString(), "Single");//下降边沿触发，
                    //}
                    //else
                    //{
                    //    TriggerCurrent = 6;
                    //    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "0", 4, TriggerCurrent.ToString(), "Single");//上升边沿触发，
                    //}

                    // 回馈负载会导致电压异常和时间异常
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}

                    SendNoticeToUIAndTxtFile("启动录波板...");
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);

                    CountDownTimeInfo("请按下急停按钮后点击是或者确认!", 9999, 0);

                    //停止录波板
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("判断结果中!");
                    //WaveData CH_EStop = new WaveData();
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 7, ref CH_EStop);
                    //double EStop_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_EStop, 5);
                    ////ACDownTime(testWorkParam.lstIDs, 1, 60, 2);
                    //double Time_EStop = 0;//急停信号时间
                    //if (EStop_Tmp > 2)
                    //{
                    //    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, false, 7, ref Time_EStop);
                    //}
                    //else
                    //{
                    //    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, true, 7, ref Time_EStop);
                    //}

                    //WaveData CH_Current = new WaveData();
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_Current);
                    //double Time_CurrentStop = 0;//电流停止时间
                    //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_Current, false, 5, ref Time_CurrentStop);
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_EStop);//设置光标1
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_CurrentStop);//设置光标2
                    ////ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                    //double Time_Stop = Math.Abs(Time_EStop - Time_CurrentStop);


                    double Time_EStop = 0;
                    double Time_K1K2 = 0;
                    WaveData CH_EStop = new WaveData();
                    WaveData CH_K1K2 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 7, ref CH_EStop, "EStop");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                    //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, true, 5, ref Time_EStop);
                    double EStop_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_EStop, 5);
                    if (EStop_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, false, 6, ref Time_EStop);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, true, 6, ref Time_EStop);
                    }
                    double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                    }
                    //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_EStop);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                    double Time_Stop = Math.Abs(Time_EStop - Time_K1K2);


                    //读取卡点时间
                    Dictionary<int, double> dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in dTime)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                    Dictionary<int, string> dVoltage = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dVoltage.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dd, "启动急停装置", "断开K1K2时间(ms)", "0", "100", dImgs);
                    ProcessDataTmp(dVoltage, "启动急停装置", "急停后的充电电压(V)", "0", "60");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    CountDownTimeInfo("请恢复急停按钮！", 9999, 0);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }




        public override void ProcessData()
        {

        }

    }
}
