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
    /// 欧标研测直流：电动汽车启动紧急停止
    /// </summary>
    public class CCS2_RT_DC_EVInitiatedEmergencyShutdown : BusinessBase
    {
        public CCS2_RT_DC_EVInitiatedEmergencyShutdown(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
        int trlTimeOut_S = 0;


        public override void InitEquiMent()
        {
            OscilloscopeCrashStop();
        }

        /// <summary>
        /// 急停输出电流停止速率
        /// </summary>
        public void OscilloscopeCrashStop()
        {
            int time = 50;
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, false, "DC", "20M", Channel1, "Output_V", "1M", "V", false, "300", "-2.5");//通道1设置2
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "DC", "20M", Channel2, "Output_I", "1M", "A", false, "50", "-2.5");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, true, "DC", "0.5", Channel3, "CP_Voltage", "1M", "V", false, "10", "2.5");
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, false, "DC", "20M", Channel4, "AC-in-V", "1M", "V", false, "250", "0");
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "500", "0");//设置滚动，时基和触发延时
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(time);
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                var dicSet1 = ControlEquipMent.BMS.BMSGetParameter_EU_DC(lstIDs, 0x97);
                foreach (int CID in testWorkParam.lstIDs)
                {
                    var data = dicSet1[CID];
                    if (data.Count >= 15)
                    {
                        List<int> para = new List<int>();
                        for (int i = 0; i < 12; i++)
                            para.Add(Convert.ToInt32(data[i]));
                        para[9] = 0;    //闭合S2
                        ControlEquipMent.BMS.BMSSetPara1_EU_DC(lstIDs, para.ToList(), data[12], MaxAllowChargeCurrent, MaxAllowChargeVoltage);
                    }
                }
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        private void StartItemFlow()
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

                    SetConditionValues();
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);//

                    SetLoadPara(testWorkParam.lstIDs, MaxAllowChargeVoltage - 20, RatedCurrent + 10, MaxAllowChargeVoltage - 10, RatedCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, RatedCurrent);
                    Thread.Sleep(3000);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (RatedCurrent / 2).ToString(), "Single");
                    Thread.Sleep(2000);//

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double CPVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage;
                        int timeout1 = 50;
                        while(timeout1-- > 0)
                        {
                            if (CPVoltage >= 5.47 && CPVoltage <= 6.53)
                                break;
                            Thread.Sleep(300);
                            CPVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage;
                        }
                        dic.Add(item, CPVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "启动急停前", "CP电压(V)", "5.47", "6.53");

                    SendNoticeToUIAndTxtFile("正在发送断开S2开关命令中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    var dicSet1 = ControlEquipMent.BMS.BMSGetParameter_EU_DC(testWorkParam.lstIDs, 0x97);
                    foreach (int CID in testWorkParam.lstIDs)
                    {
                        var data = dicSet1[CID];
                        if (data.Count >= 15)
                        {
                            List<int> para = new List<int>();
                            for (int i = 0; i < 12; i++)
                                para.Add(Convert.ToInt32(data[i]));
                            //断开S2
                            para[0] = 0;
                            para[1] = 0;
                            para[2] = 0;
                            para[3] = 0;
                            ControlEquipMent.BMS.BMSSetPara1_EU_DC(testWorkParam.lstIDs, para.ToList(), data[12], MaxAllowChargeCurrent, MaxAllowChargeVoltage);
                        }
                    }
                    SendNoticeToUIAndTxtFile("等待触发中...");
                    int timeout = 30;
                    while(timeout--  > 0)
                    {
                        if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
                            break;
                        Thread.Sleep(1000);
                    }
                    //CountDownTimeInfo("判断触发延时", 10, 0);
                    SendNoticeToUIAndTxtFile("判断结果中...");
                    //CountDownTimeInfo("判断充电延时", 5, 0);
                    Thread.Sleep(5000);

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        int timeout1 = 50;
                        while (timeout1-- > 0)
                        {
                            if (DCVoltage >= 0 && DCVoltage <= 20)
                                break;
                            Thread.Sleep(300);
                            DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        }
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "启动急停", "充电电压(V)", "0", "20");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double CPVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage;
                        int timeout1 = 50;
                        while (timeout1-- > 0)
                        {
                            if (CPVoltage >= 8.37 && CPVoltage <= 9.59)
                                break;
                            Thread.Sleep(300);
                            CPVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage;
                        }
                        dic.Add(item, CPVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "启动急停后", "CP电压(V)", "8.37", "9.59");

                    //按下1s内电流下降至5A一下
                    //OscilloscopeCursorPosition_CrashStop(2, RatedCurrent);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1);//CP动作时间
                    //ACDownTime(testWorkParam.lstIDs, 1, 30, 2);//AC回路动作时间
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);

                    Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Dictionary<int, string> Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (RatedCurrent / (Convert.ToDouble(itmp.Value) / 1000.0)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    //ProcessDataTmp(dd, "启动急停", "急停下降时间(ms)", "0", "1000", dImgs);
                    ProcessDataTmp(dd, "启动急停", "电流下降速率(A/s)", "200", "-", dImgs);


                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    SetCPRersh_EUDC();
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public void OscilloscopeCursorPosition_CrashStop(int chnelNum, double tCompareValue)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, chnelNum);

            Dictionary<int, double> Position = new Dictionary<int, double>();
            ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, tCompareValue, 1, false, false, ref Position);
            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position, true);


            Dictionary<int, double> Position2 = new Dictionary<int, double>();
            ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, 5, 1, false, false, ref Position2);
            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position2, false);

            Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, chnelNum, ref OscTime_Tmp);

            Dictionary<int, string> Data_Tmp = GetOSCTime(OscTime_Tmp);
            Dictionary<int, string> dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "启动急停", "急停下降时间(ms)", "0", "1000", dImgs);
        }

        public override void ProcessData()
        {

        }


    }
}
