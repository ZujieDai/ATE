using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.EquipMent;
using System.Configuration;
using System.Collections;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标研测交流：充电连接控制时序
    /// </summary>
    public class GB_RT_AC_ChargingConnectionControlTiming : BusinessBase
    {
        bool isPlugCharger = false;     //是否即插即充
        int waitTime = 50;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储

        public GB_RT_AC_ChargingConnectionControlTiming(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();

            //是否为即插即充(0是刷卡1是即插即充)=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                isPlugCharger = Convert.ToDouble(strParams[0].Split('=')[1]) == 1;
            }
        }

        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
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
                                //测试时间|电压|电流|CP电压|CP频率|CP占空比
                                LstTrialData[i].ExtentData = DateTime.Now.ToString()
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + "";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                SetConditionValues();

                try
                {
                    OscilloscopeSet1();

                    ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);
                    TrialMethon("", 0);

                    OscilloscopAllScreen();

                    //断开CP,S2
                    List<bool> Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(5000);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);

                    Ks = GetKStatus16_Charging();
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(5000);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(2000);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(3000);

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "-", "充电整体时序（具备S2开关）", null);

                    SetCPReresh();


                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "0");//低值
                    Thread.Sleep(50);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    //断开CP,S2
                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);


                    Ks = GetKStatus16_Charging();
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(5000);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    Thread.Sleep(1000);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(3000);

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "-", "充电整体时序（不具备S2开关）", null);
                }

                catch (Exception ex)
                {
                    SendException(ex);
                }
            }
        }

        private void TrialMethon(string sState, double diff)
        {
            #region 时序1.1
            //断开CP,S2
            List<bool> Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "10", "0");
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "12", "Auto");
            Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0.002");
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.5");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(1000);
            //Data_Tmp.Clear();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    double CPVoltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
            //    int waiteTime = 25;
            //    while (waiteTime-- > 0)
            //    {
            //        CPVoltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
            //        if (CPVoltage < 11.2)
            //        {
            //            Thread.Sleep(1000);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], CPVoltage.ToString());
            //}
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序1.1(状态1)", "CP电压(V)", "11.2", "12.8", dImgs);

            #region 状态1->状态2(1.1)
            //TimingChangeTime_AC(testWorkParam.lstIDs, "A1", "B1");
            //设置触发
            SendNoticeToUIAndTxtFile("设置示波器触发");
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "10.0", "Single");
            Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0.002");
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.6");//低值
            Thread.Sleep(waitTime);

            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(2000);

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //CPPWMUpTime(testWorkParam.lstIDs, 3, 11.7, 1, 1);
            //CPPWMDownTime(testWorkParam.lstIDs, 3, 9.2 + diff, 2, 2);
            ////读取卡点时间
            //ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            //Data_Tmp = GetOSCTime(OscTime_Tmp);
            //var dd = new Dictionary<int, string>();
            //foreach (var itmp in Data_Tmp)
            //{
            //    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            //}
            //dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            //ProcessDataTmp(dd, sState, "状态1->状态2(1.1)时间(ms)", "-", "-", dImgs);
            #endregion

            if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
            {
                Thread.Sleep(2000);
            }
            else
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(1000);
            }
            //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            //Thread.Sleep(1000);
            //Data_Tmp.Clear();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            if (isPlugCharger)
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            else
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序1.1(状态2)", "CP电压(V)", "8.2", "9.8", dImgs);
            #endregion

            #region 时序1.2
            //断开CP,S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "12", "Auto");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(3000);
            //Data_Tmp.Clear();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    double CPVoltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
            //    int waiteTime = 25;
            //    while (waiteTime-- > 0)
            //    {
            //        CPVoltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
            //        if (CPVoltage < 11.2)
            //        {
            //            Thread.Sleep(1000);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], CPVoltage.ToString());
            //}
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(2000);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            ProcessDataTmp(Data_Tmp, "时序1.2(状态1)", "CP电压(V)", "11.2", "12.8", dImgs);

            #region 状态1->状态3(1.2)
            //TimingChangeTime_AC(testWorkParam.lstIDs, "A1", "B1");
            //设置触发
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "7.0", "Single");
            Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "10", "0.05");//低值
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0.002");
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.6");//低值
            Thread.Sleep(waitTime);

            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(1000);

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = true;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //CPPWMUpTime(testWorkParam.lstIDs, 3, 11.7, 1, 1);
            //CPPWMDownTime(testWorkParam.lstIDs, 3, 9.2 + diff, 2, 2);
            ////读取卡点时间
            //ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            //Data_Tmp = GetOSCTime(OscTime_Tmp);
            //var dd = new Dictionary<int, string>();
            //foreach (var itmp in Data_Tmp)
            //{
            //    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            //}
            //dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            //ProcessDataTmp(dd, sState, "状态1->状态2(1.1)时间(ms)", "-", "-", dImgs);
            #endregion

            if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
            {
                Thread.Sleep(2000);
            }
            else
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(1000);
            }
            if (isPlugCharger)
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            else
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序1.2(状态3)", "CP电压(V)", "5.2", "6.8", dImgs);
            #endregion

            #region 时序2.1
            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "0", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);

            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);
            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(4000);
            //Data_Tmp.Clear();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序2.1(状态2’)", "CP电压(V)", "8.2", "9.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序2.1(状态2’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序2.1(状态2’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序2.1(状态2’)", "CP占空比(%)", "3", "97");
            sState = "时序2.1(状态2’)";
            CollectionCPPwm("8.2", "9.8", "-12.6", "-11.4", "3", "97", "10", "13", sState);

            #region 状态2->状态1(2.1)
            //TimingChangeTime_AC(testWorkParam.lstIDs, "B1", "A1");

            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(3000);
            //断开CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //CPPWMUpTime(testWorkParam.lstIDs, 3, 11.7, 1, 1);
            //CPPWMUpTime(testWorkParam.lstIDs, 3, 9.2 + diff, 2, 1);
            ////读取卡点时间
            //ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            //Data_Tmp = GetOSCTime(OscTime_Tmp);
            //dd = new Dictionary<int, string>();
            //foreach (var itmp in Data_Tmp)
            //{
            //    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            //}
            //dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            //ProcessDataTmp(dd, sState, "状态2->状态1(2.1)时间(ms)", "-", "-", dImgs);
            #endregion

            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(2000);
            //Data_Tmp.Clear();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序2.1(状态1)", "CP电压(V)", "11.2", "12.8", dImgs);

            //当进入状态1的100ms后5S内，充电设备的锁止装置必须解锁供电插头
            int time = 100;
            while(time-- > 0)
            {
                double freq = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPFrequency;
                if (freq > 0)
                    Thread.Sleep(200);
                else
                    break;
            }
            CountDownTimeInfo("当进入状态1的100ms后5S内，充电设备的锁止装置必须解锁供电插头。\r\n(注:勾选上为已解锁)", 20, 2);
            if (DicManualVerifyResult.First().Value)
                ProcessDataResult(testWorkParam.lstIDs, "已解锁", "锁止装置解锁", true, $"时序2.1(状态1)");
            else
                ProcessDataResult(testWorkParam.lstIDs, "未解锁", "锁止装置解锁", false, $"时序2.1(状态1)");
            #endregion

            SetCPReresh();

            #region 时序2.2
            var dd = new Dictionary<int, string>();
            if (CheckSwipingCard(testWorkParam.lstIDs))
            {
                //等待稳定
                Thread.Sleep(1000);
                #region 状态3'->状态1 断开连接，T19-T20≤100ms
                OscilloscopeCPDiconnect();

                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(3000);

                //断开CP,S2
                Ks = GetKStatus16_Charging();
                Ks[0] = false;
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(3000);

                //Data_Tmp = new Dictionary<int, string>();
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
                //}
                //ProcessDataTmp(Data_Tmp, "时序2.2(状态1’->状态1)", "PWM输出延时关断(CP占空比)", "3", "97");

                if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
                {
                    Thread.Sleep(1500);
                }
                else
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                }
                sState = "时序2.2(PWM输出延时关断)";
                var dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                var Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                ProcessDataTmp(Data_Tmp, sState, "CP正电压(V)", "11.2", "12.8", dImgs);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                ProcessDataTmp(Data_Tmp, sState, "CP负电压(V)", "-12.6", "-11.4");
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                ProcessDataTmp(Data_Tmp, sState, "CP频率(Hz)", "970", "1030");
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                ProcessDataTmp(Data_Tmp, sState, "CP占空比(%)", "3", "97");

                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile("正在分析示波器数据");
                //读取分析数据
                //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                //Thread.Sleep(1000);
                CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1);//CP动作时间
                ACDownTime(testWorkParam.lstIDs, 1, 55, 2);//AC回路动作时间

                //读取卡点时间
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                Data_Tmp = GetOSCTime(OscTime_Tmp);
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, "时序2.2(状态3’->状态1)", "S1开关切换时间(ms)", "0", "100", dImgs);

                //断线后的电压
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    int timeout = 20;
                    while (timeout-- > 0)
                    {
                        if (volt > 0 && volt < 20)
                            break;
                        Thread.Sleep(300);
                        volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    }
                    Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                }
                ProcessDataTmp(Data_Tmp, "时序2.2(状态3’->状态1)", "断开后输出电压(V)", "0", "20");

                CountDownTimeInfo("当进入状态1的100ms后5S内，充电设备的锁止装置必须解锁供电插头。\r\n(注:勾选上为已解锁)", 20, 2);
                if (DicManualVerifyResult.First().Value)
                    ProcessDataResult(testWorkParam.lstIDs, "已解锁", "锁止装置解锁", true, $"时序2.2(状态1)");
                else
                    ProcessDataResult(testWorkParam.lstIDs, "未解锁", "锁止装置解锁", false, $"时序2.2(状态1)");
                #endregion
            }
            #endregion

            #region 时序3.1
            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);

            //断开CP,S2（从状态1到状态2，保证即插即充也可以测试）
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "10.0", "Single");
            Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "10", "0.05");//低值
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0.002");
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.6");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(2000);

            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            if (ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).First().Value)
            {
                Thread.Sleep(5000);
            }
            else
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(1000);
            }

            if (isPlugCharger)
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            else
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序3.1(状态2)", "CP电压(V)", "8.2", "9.8", dImgs);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序3.1(状态2)", "CP占空比(%)", "0", "0");

            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "0", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(2000);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序3.1(状态2’)", "CP电压(V)", "8.2", "9.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序3.1(状态2’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序3.1(状态2’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序3.1(状态2’)", "CP占空比(%)", "3", "97");
            CollectionCPPwm("8.2", "9.8", "-12.6", "-11.4", "3", "97", "10", "13", "时序3.1(状态2’)");
            #endregion

            #region 时序3.2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "7.0", "Single");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "10", "0.05");//低值
            //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0.002");
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.6");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(1000);

            Ks = GetKStatus16_Charging();
            Ks[0] = true;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(4500);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(2000);
            if (isPlugCharger)
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            else
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(Data_Tmp, "时序3.2(状态3)", "CP电压(V)", "5.2", "6.8", dImgs);
            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序3.2(状态3)", "CP占空比(%)", "0", "0");

            //防止即插即充已经进入了充电状态
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            OscilloscopeS2Close();

            SendNoticeToUIAndTxtFile("启动示波器，闭合导引S2开关，开始充电");
            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(3500);

            Ks = GetKStatus16_Charging();
            Ks[0] = true;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            //闭合开关S2，启动充电
            WaitSwipingCard(testWorkParam.lstIDs, 0);

            Thread.Sleep(4000);
            SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");
            //泰克示波器需要等待5秒以上才可以绘制出波形图
            if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                Thread.Sleep(15000);
            else
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            //卡点
            CPPWMDownTime(testWorkParam.lstIDs, 3, -9.2, 1, 2);//判断CP变化时刻
            ACUpTime(testWorkParam.lstIDs, 1, 60, 2);//判断交流电压变化时刻

            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "时序3.2(状态3->状态3’)", "K1K2闭合时间(ms)", "0", "3000", dImgs);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "0", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(5000);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序3.2(状态3’)", "CP电压(V)", "5.2", "6.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序3.2(状态3’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序3.2(状态3’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序3.2(状态3’)", "CP占空比(%)", "6", "97");
            CollectionCPPwm("5.2", "6.8", "-12.6", "-11.4", "3", "97", "10", "13", "时序3.2(状态3’)");
            Data_Tmp = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                int timeout = 20;
                while (timeout-- > 0)
                {
                    if (volt > 50)
                        break;
                    Thread.Sleep(300);
                    volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                }
                Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
            }
            ProcessDataTmp(Data_Tmp, "时序3.2(状态3’)", "输出电压(V)", "50", "-");
            #endregion
            SetCPReresh();

            #region 时序4
            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(500);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "0", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(3000);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序4(状态2’)", "CP电压(V)", "8.2", "9.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序4(状态2’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序4(状态2’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序4(状态2’)", "CP占空比(%)", "6", "97");
            CollectionCPPwm("8.2", "9.8", "-12.6", "-11.4", "3", "97", "10", "13", "时序4(状态2’)");

            OscilloscopeS2Close();
            Thread.Sleep(2000);
            Ks = GetKStatus16_Charging();
            Ks[0] = true;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(3000);
            SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");
            //泰克示波器需要等待5秒以上才可以绘制出波形图
            if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                Thread.Sleep(15000);
            else
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            //卡点
            CPPWMDownTime_Tmp(testWorkParam.lstIDs, 3, 8, 1);//判断CP变化时刻
            ACUpTime(testWorkParam.lstIDs, 1, 50, 2);//判断交流电压变化时刻

            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "时序4(状态2’->状态3’)", "K1K2闭合时间(ms)", "0", "3000", dImgs);

            OscilloscopeSet1();
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "0", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");//低值
            Thread.Sleep(waitTime);
            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(2000);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序4(状态3’)", "CP电压(V)", "5.2", "6.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序4(状态3’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序4(状态3’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序4(状态3’)", "CP占空比(%)", "6", "97");
            //ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "");    //隐藏
            CollectionCPPwm("5.2", "6.8", "-12.6", "-11.4", "3", "97", "7", "13", "时序4(状态3’)");
            Data_Tmp = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                int timeout = 20;
                while (timeout-- > 0)
                {
                    if (volt > 50)
                        break;
                    Thread.Sleep(300);
                    volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                }
                Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
            }
            ProcessDataTmp(Data_Tmp, "时序4(状态3’)", "输出电压(V)", "50", "-");
            #endregion

            #region 时序5
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "40", "0");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(2000);
            SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent / 2);
            WaitACCurrent(testWorkParam.lstIDs, LstChargerInfo[0].NominalCurrent / 2);
            Thread.Sleep(1500);

            //Data_Tmp = new Dictionary<int, string>();
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            //{
            //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
            //}
            //ProcessDataTmp(Data_Tmp, "时序5(状态3’)", "CP电压(V)", "5.2", "6.8");
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ProcessDataTmp(Data_Tmp, "时序5(状态3’)", "CP负电压(V)", "-12.6", "-11.4");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            //ProcessDataTmp(Data_Tmp, "时序5(状态3’)", "CP频率(Hz)", "970", "1030");
            //Data_Tmp.Clear();
            //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            //ProcessDataTmp(Data_Tmp, "时序5(状态3’)", "CP占空比(%)", "6", "97");
            //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            //Thread.Sleep(2000);
            CollectionCPPwm("5.2", "6.8", "-12.6", "-11.4", "3", "97", "7", "13", "时序5(状态3’)");
            Data_Tmp = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                int timeout = 20;
                while(timeout-- > 0)
                {
                    if (volt > 50)
                        break;
                    Thread.Sleep(300);
                    volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                }
                Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
            }
            ProcessDataTmp(Data_Tmp, "时序5(状态3’)", "输出电压(V)", "50", "-");

            ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
            if(AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs.FirstOrDefault()].PhaseA_Current >= 5)
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
            #endregion

            #region 时序8.1
            OscilloscopeS2Open();

            CheckSwipingCard(testWorkParam.lstIDs);
            Thread.Sleep(2000);

            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(3000);

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(4500);

            //读取分析数据
            //泰克示波器需要等待5秒以上才可以绘制出波形图
            if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                Thread.Sleep(4000);
            else
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
            Thread.Sleep(1000);
            CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1);//CP动作时间
            ACDownTime(testWorkParam.lstIDs, 1, 60, 2);//AC回路动作时间

            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, itmp.Value.ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "时序8.1(状态3’->状态2’)", "断开回路时间(ms)", "0", "100", dImgs);
            #endregion
        }

        private void OscilloscopeSet1()
        {
            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "1.0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Auto");//上升边沿触发，自动
            Thread.Sleep(waitTime);

            //设置时基400ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "40", "0");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
            //ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
            //添加测量值
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 3);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
            Thread.Sleep(waitTime);
        }

        private void OscilloscopeCPDiconnect()
        {
            int waitTime = 50;
            //初始化示波器
            SendNoticeToUIAndTxtFile("设置示波器1、2、3、4号通道");
            //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.7");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            //有的示波器测量值只能添加4个
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
            Thread.Sleep(waitTime);
            //设置触发
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "50", 3, "10.8", "Single");//超时触发，泰克示波器在设备层做了触发模式转换
            Thread.Sleep(waitTime);
            //设置时基400ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
            Thread.Sleep(waitTime);
        }

        private void OscilloscopeS2Close()
        {
            int waitTime = 50;
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            //ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
            //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
            ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
            Thread.Sleep(waitTime);
            //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 1, "100", "Single");//上升边沿触发，自动
            Thread.Sleep(waitTime);
            //设置时基400ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "-1.5");//低值
            Thread.Sleep(waitTime);
        }

        private void OscilloscopeS2Open()
        {
            int waitTime = 50;
            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "FULL", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            //3通道 打开  DC耦合   带宽  探头比   标签      阻抗   电压  反向通道  纵坐标档位  纵坐标位置
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
            Thread.Sleep(waitTime);
            //设置时基
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1");//滚动  时基mS   延时
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");


            foreach (var item in ControlEquipMent.Oscilloscope.DitEquipMentBase)
            {
                if (item.Value.GetType() == typeof(DLMOscilloscope))
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8.5", "Single");//上升边沿触发，自动
                }
                else if (item.Value.GetType() == typeof(emtTekOscilloscope))
                {
                    //超时触发，泰克示波器在设备层做了触发模式转换
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "FALL", "DC", "EDGE", "50", 1, "100", "Auto");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    //设置时基
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1.2");//滚动  时基mS   延时
                }
                else if (item.Value.GetType() == typeof(SDSOscilloscope))
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Auto");//上升边沿触发，自动
                }
            }
        }

        private void OscilloscopAllScreen()
        {
            int waitTime = 50;
            //初始化示波器
            SendNoticeToUIAndTxtFile("设置示波器1、2、3、4号通道");
            ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            //设置时基400ms
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "2000", "0");//低值
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
            Thread.Sleep(1000);
        }

        public override void ProcessData()
        {

        }

    }
}
