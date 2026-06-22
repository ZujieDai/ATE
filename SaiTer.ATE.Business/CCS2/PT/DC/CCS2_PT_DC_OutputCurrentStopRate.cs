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
    /// 输出电流停止速率
    /// </summary>
    public class CCS2_PT_DC_OutputCurrentStopRate : BusinessBase
    {
        public CCS2_PT_DC_OutputCurrentStopRate(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;

        //private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
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
                    SendNoticeToUIAndTxtFile("关闭BMS中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPRersh_EUDC();

                    #region 被动中止
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, false, BMSDemandVolt);
                    //Thread.Sleep(1000);
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, MaxAllowChargeVoltage, false))
                    {
                        return;
                    }
                    Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 5, BMSDemandVolt, ResiLoadCurrent);

                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    //int timeout = 60;
                    //while (timeout-- > 0)
                    //{
                    //    bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                    //    if (StabilizeCurrent)
                    //    {
                    //        break;
                    //    }
                    //    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    //    System.Threading.Thread.Sleep(1000);
                    //}
                    //Thread.Sleep(2000);//等待回馈负载电流稳定
                    WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, ResiLoadCurrent, 50);
                    Thread.Sleep(3000);//等待回馈负载电流稳定


                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        int timeout = 20;
                        while(timeout-- > 0)
                        {
                            if (DCVoltage >= GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0] && DCVoltage <= GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1])
                                break;
                            Thread.Sleep(1000);
                            DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        }
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "被动中止", "充电电压(V)", GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0].ToString(), GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1].ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        int timeout = 20;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent >= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0] && DCCurrent <= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1])
                                break;
                            Thread.Sleep(1000);
                            DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        }
                        dic.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "被动中止", "充电电流(A)", GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1].ToString());

                    //初始化示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(300);//
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (10).ToString(), "Single");
                    Thread.Sleep(5000);//

                    SendNoticeToUIAndTxtFile("BMS正在主动中止充电");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                        Thread.Sleep(4000);

                    //OscilloscopeCursorPosition_StopRate(2, ResiLoadCurrent);//示波器卡点
                    ACDownTime(testWorkParam.lstIDs, 2, ResiLoadCurrent * 0.95, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);

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
                    ProcessDataTmp(dd, "被动中止", "电流停止时间(ms)", "-", "-", dImgs);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double dStopRateI = (ResiLoadCurrent - 5) / (Convert.ToDouble(dd[item]) / 1000);//算出停止速率
                        dic.Add(item, dStopRateI.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "被动中止", "电流停止速率(A/s)", "100", "-");

                    #endregion 被动中止

                    #region 主动中止
                    //主动中止停止速率
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, false, BMSDemandVolt);
                    Thread.Sleep(1000);
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, MaxAllowChargeVoltage))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");


                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 5, BMSDemandVolt, ResiLoadCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    ////timeout = 60;
                    ////while (timeout-- > 0)
                    ////{
                    ////    bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                    ////    if (StabilizeCurrent)
                    ////    {
                    ////        break;
                    ////    }
                    ////    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    ////    System.Threading.Thread.Sleep(1000);
                    ////}
                    ////Thread.Sleep(2000);//等待回馈负载电流稳定
                    WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, ResiLoadCurrent, 50);
                    Thread.Sleep(3000);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        int timeout = 20;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage >= GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0] && DCVoltage <= GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1])
                                break;
                            Thread.Sleep(1000);
                            DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        }
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "主动中止", "充电电压(V)", GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0].ToString(), GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1].ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        int timeout = 20;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent >= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0] && DCCurrent <= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1])
                                break;
                            Thread.Sleep(1000);
                            DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        }
                        dic.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "主动中止", "充电电流(A)", GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1].ToString());

                    //初始化示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);//
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (5).ToString(), "Single");
                    Thread.Sleep(2000);//



                    CountDownTimeInfo("请操作充电桩停止充电！", 999, 2);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    Thread.Sleep(3000);
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                        Thread.Sleep(4000);

                    //OscilloscopeCursorPosition_StopRate(2, ResiLoadCurrent);//示波器卡点
                    ACDownTime(testWorkParam.lstIDs, 2, ResiLoadCurrent * 0.95, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);

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
                    ProcessDataTmp(dd, "主动中止", "电流停止时间(ms)", "-", "-", dImgs);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double dStopRateI = (ResiLoadCurrent - 5) / (Convert.ToDouble(dd[item]) / 1000);//算出停止速率
                        dic.Add(item, dStopRateI.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "主动中止", "电流停止速率(A/s)", "100", "-");



                    SetCPRersh_EUDC();


                    #endregion 主动中止

                    #region 急停中止
                    //急停停止速率
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, false, BMSDemandVolt);
                    Thread.Sleep(1000);
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, MaxAllowChargeVoltage))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");


                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 5, BMSDemandVolt, ResiLoadCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    //timeout = 60;
                    //while (timeout-- > 0)
                    //{
                    //    bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                    //    if (StabilizeCurrent)
                    //    {
                    //        break;
                    //    }
                    //    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    //    System.Threading.Thread.Sleep(1000);
                    //}
                    //Thread.Sleep(2000);//等待回馈负载电流稳定
                    WaitDCCurrentWithTimeEU_DC(testWorkParam.lstIDs, ResiLoadCurrent, 50);
                    Thread.Sleep(3000);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        int timeout = 20;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage >= GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0] && DCVoltage <= GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1])
                                break;
                            Thread.Sleep(1000);
                            DCVoltage = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt).GetValueOrDefault();
                        }
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "急停", "充电电压(V)", GetErrLimit_U_CCS2_DC(BMSDemandVolt - 20)[0].ToString(), GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1].ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        int timeout = 20;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent >= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0] && DCCurrent <= GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1])
                                break;
                            Thread.Sleep(1000);
                            DCCurrent = (AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent).GetValueOrDefault();
                        }
                        dic.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "急停", "充电电流(A)", GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(ResiLoadCurrent)[1].ToString());

                    //初始化示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);//
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 2, (5).ToString(), "Single");
                    Thread.Sleep(2000);//



                    CountDownTimeInfo("请按下充电桩急停按钮，停止充电！", 999, 2);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                        Thread.Sleep(4000);

                    //OscilloscopeCursorPosition_StopRate(2, ResiLoadCurrent);//示波器卡点
                    ACDownTime(testWorkParam.lstIDs, 2, ResiLoadCurrent * 0.95, 1);
                    ACDownTime(testWorkParam.lstIDs, 2, 5, 2);

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
                    ProcessDataTmp(dd, "急停", "电流停止时间(ms)", "-", "-", dImgs);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double dStopRateI = (ResiLoadCurrent - 5) / (Convert.ToDouble(dd[item]) / 1000);//算出停止速率
                        dic.Add(item, dStopRateI.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "急停", "电流停止速率(A/s)", "200", "-");


                    CountDownTimeInfo("请恢复充电桩急停按钮！", 999, 2);
                    SetCPRersh_EUDC();


                    #endregion 急停中止




                }
            }
            catch (Exception ex) { SendException(ex); }

        }

        double BMSDemandVolt = 400;
        double ResiLoadCurrent = 10;
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
            OscilloscopeStopRate();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >= 1)
            {

                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));

            }
        }

        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string State = AllEquipStateData.DicBMS_EU_DC_StateData[LstTrialData[k].ChargerId].SystemState;
                    if (State == "CurrentDemandReq")
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = "充电状态" + "|是否解锁|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 输出电流停止速率
        /// </summary>
        public void OscilloscopeStopRate()
        {

            try
            {


                //ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "300", "0");//通道1设置2

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "50", "-1");//通道2设置

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置


                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//设置滚动，时基和触发延时


                ////添加测量值
                //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//1通道平均值
                //Thread.Sleep(50);
                //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//2通道平均值
                //Thread.Sleep(50);


                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);



            }
            catch
            {
                System.Threading.Thread.Sleep(1000);

            }



        }


        /// <summary>
        /// 电流停止时间卡点
        /// </summary>
        /// <param name="chnelNum"></param>
        /// <param name="tCompareValue">带载电流</param>
        public void OscilloscopeCursorPosition_StopRate(int chnelNum, double tCompareValue)
        {
            try
            {
                double dcv = tCompareValue-5;
                if (tCompareValue > 50)
                {
                    dcv = tCompareValue * 0.9;
                }
                Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, chnelNum);

                Dictionary<int, double> Position = new Dictionary<int, double>();
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, dcv, 1, false, false, ref Position);


                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position, true);


                Dictionary<int, double> Position2 = new Dictionary<int, double>();
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, 5, 1, false, false, ref Position2);


                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position2, false);


            }
            catch
            {


            }


        }
        ///// <summary>
        ///// 获取示波器光标之间的时间差
        ///// </summary>
        ///// <param name="dtmp"></param>
        ///// <returns></returns>
        //public Dictionary<int, string> GetOSCTime(Dictionary<int, double[]> dtmp)
        //{

        //    Dictionary<int, string> ds = new Dictionary<int, string>();
        //    try
        //    {
        //        foreach (var item in dtmp)
        //        {
        //            if (item.Value != null)
        //            {
        //                ds.Add(item.Key, Math.Abs(item.Value[0]).ToString());
        //            }

        //        }
        //    }
        //    catch (Exception ex) { Log.Log.LogException(ex); }

        //    return ds;
        //}
    }
}
