using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电流调整时间
    /// </summary>
    public class CCS2_PT_DC_OutputCurrentAdjustTime : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 60;

        /// <summary>
        /// 下降的电流差值需要小于等于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent1 = 20;
        /// <summary>
        /// 下降的电流差值需要大于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent2 = 40;
        public CCS2_PT_DC_OutputCurrentAdjustTime(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //BMS需求电压设置(V)=400|回馈负载电流设置(A)=100|小于等于20A下降电流(A)=20|大于20A下降电流(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 4)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                MinusCurrent1 = double.Parse(strParams[2].Split('=')[1]);
                MinusCurrent2 = double.Parse(strParams[3].Split('=')[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");


            SetLoadDCOFF(lstIDs);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            OscilloscopeCurrentAdjustTime();
            SetCPReresh();
        }
        public void OscillographInstrumentStopRate_Gear(double demandCurrent)
        {
            try
            {
                if (demandCurrent <= 80)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "10", "");
                }
                if (demandCurrent > 80 && demandCurrent <= 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "20", "");
                }
                if (demandCurrent > 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "40", "");
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public void OscillographInstrumentStopRate_TimeBase(double Current)
        {
            try
            {
                if (Current <= 20)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");
                }
                if (Current >= 40 && Current <= 80)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
                }
                if (Current > 80 && Current <= 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("1");
                }
                if (Current > 160)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("2");
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
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
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dic.Add(item, DemandVoltage.ToString("F2"));
                    }

                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dicC.Add(item, DemandCurrent.ToString("F2"));
                    }

                    Dictionary<int, string> ddC1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        ddC1.Add(item, MinusCurrent1.ToString("F2"));
                    }
                    Dictionary<int, string> ddC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        ddC2.Add(item, MinusCurrent1.ToString("F2"));
                    }


                    //ProcessDataTmp(dic, TrialItem.ItemName + "充电设置", "电压需求(V)", "-", "-");
                    //ProcessDataTmp(dicC, TrialItem.ItemName + "充电设置", "电流需求(A)", "-", "-");
                    //ProcessDataTmp(ddC1, TrialItem.ItemName + "负载设置", "下降电流差值1(A)", "-", "-");
                    //ProcessDataTmp(ddC2, TrialItem.ItemName + "负载设置", "下降电流差值2(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("开启导引中");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    SendNoticeToUIAndTxtFile("设置充电电压：【" + DemandVoltage.ToString() + "】电流：【" + DemandCurrent.ToString() + "】");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, MaxAllowChargeVoltage))
                    {
                        return;
                    }


                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 10 , DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 50);
                    Thread.Sleep(3000);//等待数据稳定，仪器数据刷新

                    #region 第一个点
                    var dic2 = new Dictionary<int, string>();
                    var dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整前(小于等于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整前(小于等于20A下降电流)", "充电电流(A)", "-", "-");
                    double Level = (DemandCurrent - MinusCurrent1 / 2);
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("设置示波器触发中...");
                    Thread.Sleep(2000);//
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (Level).ToString(), "Single");
                    Thread.Sleep(2000);//
                    Thread.Sleep(3 * 1000); //加延迟防止变化太快
                    SendNoticeToUIAndTxtFile("下发指令改变电流点1...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent - MinusCurrent1, false, 390);
                    SendNoticeToUIAndTxtFile("设置充电电压：【" + DemandVoltage.ToString() + "】电流：【" + (DemandCurrent-MinusCurrent1).ToString() + "】");
                    Thread.Sleep(5000);//

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整后(小于等于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整后(小于等于20A下降电流)", "充电电流(A)", "-", "-");

                    //OscilloscopeCursorPosition_CurrentAdjustTime(2, DemandCurrent, DemandCurrent - MinusCurrent1);//示波器卡点
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - 2, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - MinusCurrent1 + 4, 2);

                    Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 2, ref OscTime_Tmp);
                    Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "小于等于20A下降电流", "电流调整时间(ms)", GetCurrentAdjustTime_CCS2_DC(DemandCurrent, DemandCurrent - MinusCurrent1)[0].ToString(),
                    GetCurrentAdjustTime_CCS2_DC(DemandCurrent, DemandCurrent - MinusCurrent1)[1].ToString(), dImgs);



                    SystemEvent.MessageInfo(false, "判断结果中...");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    SetCPRersh_EUDC();


                    #region 第二个点

                    SendNoticeToUIAndTxtFile("开启导引中");

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    SendNoticeToUIAndTxtFile("设置充电电压：【" + DemandVoltage.ToString() + "】电流：【" + (DemandCurrent).ToString() + "】");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, MaxAllowChargeVoltage))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent , DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, DemandCurrent, 60);
                    Thread.Sleep(3000);//等待数据稳定，仪器数据刷新

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整前(大于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整前(大于20A下降电流)", "充电电流(A)", "-", "-");
                    Level = (DemandCurrent - MinusCurrent2 / 2);

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("设置示波器触发中...");
                    Thread.Sleep(2000);//
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (Level).ToString(), "Single");
                    Thread.Sleep(2000);//
                    Thread.Sleep(3 * 1000); //加延迟防止变化太快

                    SendNoticeToUIAndTxtFile("下发指令改变电流点2...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent - MinusCurrent2, false, 390);
                    SendNoticeToUIAndTxtFile("设置充电电压：【" + DemandVoltage.ToString() + "】电流：【" + (DemandCurrent - MinusCurrent2).ToString() + "】");
                    Thread.Sleep(5000);//

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整后(大于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整后(大于20A下降电流)", "充电电流(A)", "-", "-");


                    //OscilloscopeCursorPosition_CurrentAdjustTime(2, DemandCurrent, DemandCurrent - MinusCurrent2);//示波器卡点
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - 2, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, DemandCurrent - MinusCurrent2 + 4, 2);

                    OscTime_Tmp = new Dictionary<int, double[]>();
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 2, ref OscTime_Tmp);
                    Data_Tmp = new Dictionary<int, string>();
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "大于20A下降电流", "电流调整时间(ms)", GetCurrentAdjustTime_CCS2_DC(DemandCurrent, DemandCurrent - MinusCurrent2)[0].ToString(),
                        GetCurrentAdjustTime_CCS2_DC(DemandCurrent, DemandCurrent - MinusCurrent2)[1].ToString(), dImgs);

                    SystemEvent.MessageInfo(false, "判断结果中...");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }




        public override void ProcessData()
        {

        }


        public void OscilloscopeCurrentAdjustTime()
        {
            try
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "300", "0");//通道1设置2

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "50", "-1");//通道2设置

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置

                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//设置滚动，时基和触发延时

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            }
            catch
            {
                System.Threading.Thread.Sleep(1000);

            }

        }

        public void OscilloscopeCursorPosition_CurrentAdjustTime(int chnelNum, double tStart, double dEnd)
        {
            try
            {
                Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, chnelNum);

                Dictionary<int, double> Position = new Dictionary<int, double>();
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, tStart - 5, 1, false, false, ref Position);

                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position, true);

                Dictionary<int, double> Position2 = new Dictionary<int, double>();
                double de = dEnd + 5;
                if (dEnd > 50)
                {
                    de = dEnd * 1.1;
                }
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, de, 1, false, false, ref Position2);

                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position2, false);
            }
            catch
            {


            }
        }

    }
}
