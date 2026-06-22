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
    internal class GB_RT_DC_ActivateEmergencyStop : BusinessBase
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

        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间


        public GB_RT_DC_ActivateEmergencyStop(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ItemParams.Split('|');

            DemandVoltage = LstChargerInfo[0].NominalVoltage;
            DemandCurrent = LstChargerInfo[0].NominalCurrent;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            int waitTime = 50;
            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器通道");
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//清除所有测量项
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, false, "DC", "20", Channel2, "Input_AC_V", "50", "V", false, "50", "2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, true, "DC", "20", "1", "K1K2_Sign", "50", "V", false, "10", "-0.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "DC", "20", "1", "Emergency", "50", "V", false, "10", "-2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_V", "50", "V", false, "250", "0");
            Thread.Sleep(waitTime);
            //添加测量值
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "TOP", 1);//均方根
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "TOP", 2);//高值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "BASE", 2);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 2);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(lstIDs, "250k");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(lstIDs, "X");
            Thread.Sleep(waitTime);
            //设置时基400ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "200", "0");
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

                    SendNoticeToUIAndTxtFile("设置触发...");
                    var EStop_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 2);//低值
                    double TriggerCurrent = 6;
                    if (Convert.ToDouble(EStop_Tmp.Values.FirstOrDefault()) < 0)
                    {
                        TriggerCurrent = -6;
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "0", 2, TriggerCurrent.ToString(), "Single");//下降边沿触发，
                    }
                    else
                    {
                        TriggerCurrent = 6;
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "0", 2, TriggerCurrent.ToString(), "Single");//上升边沿触发，
                    }

                    //负载电压会导致测试异常
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    // 回馈负载会导致电压异常和时间异常
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    //}
                    CountDownTimeInfo("请按下急停按钮后点击是或者确认!", 9999, 0);

                    //等待触发
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("判断结果中!");
                    ACDownTime(testWorkParam.lstIDs, 3, 6, 2);
                    if (Convert.ToDouble(EStop_Tmp.Values.FirstOrDefault()) < 0)
                    {
                        ACUpTime(testWorkParam.lstIDs, 2, TriggerCurrent * 0.9, 1);
                    }
                    else
                    {
                        ACDownTime(testWorkParam.lstIDs, 2, TriggerCurrent * 0.9, 1);
                    }

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Dictionary<int, string> dTime = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in dTime)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    Dictionary<int, string> dVoltage = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dVoltage.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dTime, "启动急停装置", "断开K1K2时间(ms)", "0", "100", dImgs);
                    ProcessDataTmp(dVoltage, "启动急停装置", "急停后的充电电压(V)", "0", "60", dImgs);

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
