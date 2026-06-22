using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    /// 企标研测直流：充电接口兼容性试验(完整版，包含多个测试项目)
    /// </summary>
    public class QB_RT_DC_ChargerControlCompatibility_WaveRecoder : BusinessBase
    {
        string ItemStatus;
        string ItemFlow;
        int trlTimeOut_S = 30;
        double DemandVoltage = 745;
        double DemandCurrent = 20;
        double NormalCC1 = 3.9;
        double Normal2CC1 = 3.9;
        double MinCC1 = 2.8;
        double MaxCC1 = 5;


        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;


        Dictionary<int, string> dicImagePath = new Dictionary<int, string>();


        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储


        List<bool> SBitS;
        private int ItemFlowIndex = 1;
        private string crm_state = "";

        public QB_RT_DC_ChargerControlCompatibility_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // BMS需求电压设置(V)=745|BMS需求电流设置(A)=20|CC1正常电压设置(V)=3.9|CC1正常电压设置2(V)=4.1|CC1异常小电压(V)=2.8|CC1异常大电压(V)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 6)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                NormalCC1 = double.Parse(strParams[2].Split('=')[1]);
                Normal2CC1 = double.Parse(strParams[3].Split('=')[1]);
                MinCC1 = double.Parse(strParams[4].Split('=')[1]);
                MaxCC1 = double.Parse(strParams[5].Split('=')[1]);
            }
            // 限制值的范围
            if (NormalCC1 < 3.65)
            {
                NormalCC1 = 3.65;
            }
            if (NormalCC1 > 4.37)
            {
                NormalCC1 = 4.37;
            }
            if (Normal2CC1 < 3.65)
            {
                Normal2CC1 = 3.65;
            }
            if (Normal2CC1 > 4.37)
            {
                Normal2CC1 = 4.37;
            }

            if (MinCC1 < 2)
            {
                MinCC1 = 2;
            }
            if (MinCC1 > 3.2)
            {
                MinCC1 = 3.2;
            }

            if (MaxCC1 < 4.8)
            {
                MaxCC1 = 4.8;
            }
            if (MinCC1 > 5.7)
            {
                MaxCC1 = 5.7;
            }

            //充电控制状态测试
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent >= 30 ? LstChargerInfo[0].NominalCurrent : 30;


            dicImagePath = new Dictionary<int, string>();

            SBitS = GetKStatus16_Charging_DC();
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");


            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

            // 模拟插拔枪
            SetCPReresh();
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
                ControlEquipMent.BMS.BMSSetResistance(lstIDs, 1000);
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
                //充电控制电压测试
                StartItemFlow_1();

                //充电控制状态测试
                StartItemFlow_2();

                //充电控制时序测试
                StartItemFlow_3();

                //绝缘异常测试
                StartItemFlow_4();

                //机械锁异常测试
                StartItemFlow_5();

                //通信异常测试
                StartItemFlow_6();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 充电控制电压测试
        /// </summary>
        public void StartItemFlow_1()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "充电控制电压测试" + "--------------------------->");
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
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    double Resistance1 = (NormalCC1) / (12 - 2 * NormalCC1) * 1000;//正常CC1
                    double Resistance1_2 = (Normal2CC1) / (12 - 2 * Normal2CC1) * 1000;//正常CC2
                    double Resistance2 = (MinCC1) / (12 - 2 * MinCC1) * 1000;//最小CC1
                    double Resistance3 = (MaxCC1) / (12 - 2 * MaxCC1) * 1000;//最大CC1
                    Resistance1 = RetainDecimals<double>(Resistance1, 1);
                    Resistance1_2 = RetainDecimals<double>(Resistance1_2, 1);
                    Resistance2 = RetainDecimals<double>(Resistance2, 1);
                    Resistance3 = RetainDecimals<double>(Resistance3, 1);
                    double Resistance4 = 1030;
                    double Resistance5 = 970;

                    #region 第一个点
                    SendNoticeToUIAndTxtFile("开启导引中，并设置正常CC1阻值1：" + Resistance1);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);
                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance1, "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("设置正常CC1阻值2：" + Resistance1_2);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1_2);
                    Thread.Sleep(3 * 1000);
                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance1_2, "CC1电压(V)", "3.65", "4.37");
                    Thread.Sleep(2 * 1000);
                    //Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", chargerState == "充电中", $"CC1正常电压{NormalCC1}充电中调整到正常电压{Normal2CC1}");

                    SendNoticeToUIAndTxtFile("设置正常CC1阻值1：" + Resistance1);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance1);
                    Thread.Sleep(5 * 1000);
                    //dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, $"CC1正常电压{Normal2CC1}充电中调整到正常电压{NormalCC1}", "是");
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", chargerState == "充电中", $"CC1正常电压{Normal2CC1}充电中调整到正常电压{NormalCC1}");
                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 第三个点
                    SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置异常CC1小电压，阻值：" + Resistance2);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance2);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    CountDownTimeInfo("CC1异常小电压，请检查充电桩是否连接!\r\n（勾上为可连接）", 999, 2);
                    ItemStatus = $"异常CC1小电压(阻值{Resistance2})";
                    ItemFlow = "充电枪连接";
                    ProcessData();
                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 第四个点CC1Min
                    SetCPReresh();
                    SendNoticeToUIAndTxtFile("设置异常CC1大电压，阻值：" + Resistance3);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance3);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    CountDownTimeInfo("CC1异常大电压，请检查充电桩是否连接!\r\n（勾上为可连接）", 999, 2);
                    ItemStatus = $"异常CC1大电压(阻值{Resistance3})";
                    ItemFlow = "充电枪连接";
                    ProcessData();

                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                    //ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);

                    #region 第五个点CC1Min-充电中
                    SetCPReresh();
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值1000", "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    System.Threading.Thread.Sleep(8 * 1000);
                    SendNoticeToUIAndTxtFile("设置到CC1异常小电压，阻值" + Resistance2);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance2);


                    Thread.Sleep(10000);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);

                    SendNoticeToUIAndTxtFile("判断结果中...");

                    //读取录波板数据
                    WaveData CH_OutputVoltage = new WaveData();
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_CC1 = new WaveData();
                    WaveData CH_CST = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST, "CST_FaultState");//连接器故障


                    //System.Threading.Thread.Sleep(1000 * 10);
                    var dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    string CSTFaultSon = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST, CH_CST.LinePoints_Y.Count - 1).ToString();
                    ProcessDataIsNor(CSTFaultSon, dImgs, "CST连接器异常", "1", $"CC1异常小电压(阻值{Resistance2})");
                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 第六个点CC1Min-充电中
                    SetCPReresh();
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值1000", "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    System.Threading.Thread.Sleep(8 * 1000);
                    SendNoticeToUIAndTxtFile("设置到CC1异常大电压，阻值" + Resistance3);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance3);


                    Thread.Sleep(10000);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);

                    SendNoticeToUIAndTxtFile("判断结果中...");

                    //读取录波板数据
                    CH_OutputVoltage = new WaveData();
                    CH_OutputCurrent = new WaveData();
                    CH_CC1 = new WaveData();
                    CH_CST = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST, "CST_FaultState");//连接器故障

                    //System.Threading.Thread.Sleep(1000 * 10);
                    dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    CSTFaultSon = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CST, CH_CST.LinePoints_Y.Count - 1).ToString();
                    ProcessDataIsNor(CSTFaultSon, dImgs, "CST连接器异常", "1", $"CC1异常大电压(阻值{Resistance3})");
                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 第七个点
                    SetCPReresh();

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("设置正常CC1电压，阻值：" + Resistance4);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance4);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance4, "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);


                    //读取录波板数据
                    CH_OutputVoltage = new WaveData();
                    CH_OutputCurrent = new WaveData();
                    CH_CC1 = new WaveData();
                    CH_CST = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST, "CST_FaultState");//连接器故障


                    dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, "是否充电中", "是"); chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "正常CC1阻值" + Resistance4, chargerState, chargerState == "充电中", "充电状态");
                    #endregion

                    SendNoticeToUIAndTxtFile("设备正在初始化中，请稍后...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    #region 第八个点
                    SetCPReresh();

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("设置正常CC1电压，阻值：" + Resistance5);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Resistance5);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double CC1 = AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage;
                        d1.Add(item, CC1.ToString());
                    }
                    ProcessDataTmp(d1, "正常CC1阻值" + Resistance5, "CC1电压(V)", "3.65", "4.37");

                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);

                    //读取录波板数据
                    CH_OutputVoltage = new WaveData();
                    CH_OutputCurrent = new WaveData();
                    CH_CC1 = new WaveData();
                    CH_CST = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 43, 2, ref CH_CST, "CST_FaultState");//连接器故障


                    dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    //ProcessDataIsNor(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState == "充电中" ? "是" : "否", dImgs, "是否充电中", "是");
                    chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.FirstOrDefault()].ChargingState;
                    ProcessDataResult(testWorkParam.lstIDs, dImgs, "正常CC1阻值" + Resistance4, chargerState, chargerState == "充电中", "充电状态");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            finally
            {
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "充电控制电压测试" + "结束--------------------------->");
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

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = ItemStatus + "|" + ItemFlow + "|-|-|是|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = ItemStatus + "|" + ItemFlow + "|-|-|否|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    SaveTrialData(LstTrialData[k]);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }


        /// <summary>
        /// 充电控制状态测试
        /// </summary>
        public void StartItemFlow_2()
        {

            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "充电控制状态测试" + "--------------------------->");
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
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

                    BMSDemandVolt = BMSDemandVolt > LstChargerInfo[0].NominalVoltage * 0.5 ? LstChargerInfo[0].NominalVoltage * 0.5 : BMSDemandVolt;
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
                    //WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(1000 * 3);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent, BMSDemandVolt, ResiLoadCurrent);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电流(A)", (ResiLoadCurrent * 0.9).ToString("F2"), (ResiLoadCurrent * 1.1).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电压(V)", (BMSDemandVolt - 20).ToString("F2"), (BMSDemandVolt + 20).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    //控制变化
                    //BMSDemandVolt = BMSDemandVolt * 0.5;
                    //有点桩改变需求电压小于当前充电机电压会直接过压保护，例如江门AK
                    BMSDemandVolt = BMSDemandVolt + 250 > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSDemandVolt + 250;
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, 30, true, BMSDemandVolt);
                    WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(1000 * 3);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, 20);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电流(A)", "18.00", "22.00");

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电压(V)", (BMSDemandVolt * 0.9).ToString("F2"), (BMSDemandVolt * 1.1).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);

                    if (TrialType == (int)EmTrialType.充电控制)
                    {
                        CountDownTimeInfo("请人工检查充电机应按照制造商声明的方式手动设定充电参数，并实施充电启停操作。\r\n注：勾选上为可以正常解锁", 20, 2);
                        //ProcessDataConnect("应能手动设定充电参数，并实施充电启停操作", "是否具备");
                        if (DicManualVerifyResult.First().Value)
                            ProcessDataResult(testWorkParam.lstIDs, "是", "应能手动设定充电参数，并实施充电启停操作", true, "是否具备");
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "否", "应能手动设定充电参数，并实施充电启停操作", true, "是否具备");
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "充电控制状态测试" + "结束--------------------------->");
            }
        }

        /// <summary>
        /// 充电控制时序测试
        /// </summary>
        public void StartItemFlow_3()
        {

            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "充电控制时序测试" + "--------------------------->");


                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
                    #region  ------  此部分代码保留,作用可忽略---------------


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

                                    LstTrialData[i].ExtentData = "null|null|null|null|null";

                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    #endregion

                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                    //InitOscillograph();
                    //SendNoticeToUIAndTxtFile("等待录波仪滚动");

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    MessgaeInfo(true, "请刷卡充电!", true);
                    int timeout = 200;
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);

                    SetConditionValues();

                    Dictionary<int, string> dicCC1Value = new Dictionary<int, string>();
                    Dictionary<int, string> dicCC2Value = new Dictionary<int, string>();
                    string CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                    string CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dicCC1Value.Add(item, CC1Value1);
                        dicCC2Value.Add(item, CC2Value1);
                    }

                    int loadTime = 15;
                    SendNoticeToUIAndTxtFile($"启动负载,并带载{loadTime}秒");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 10, DemandCurrent);
                    Thread.Sleep(300);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(loadTime * 1000);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭负载,停止充电...");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //检测辅源是否断开
                    if (TrialType == (int)EmTrialType.充电连接控制时序研发B6)
                    {
                        double K3K4Value;
                        timeout = 100;
                        while (timeout-- > 0)
                        {
                            K3K4Value = System.Math.Abs(AllEquipStateData.DicBMS_DC_StateData.First().Value.APSVoltage);
                            if (timeout-- <= 0)
                            {
                                break;
                            }
                            if (K3K4Value <= 1)
                            {
                                Thread.Sleep(6500);
                                break;
                            }
                            Thread.Sleep(100);
                        }
                    }

                    Thread.Sleep(5000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);


                    //读取录波板数据
                    SendNoticeToUIAndTxtFile("读取录波板数据...");
                    WaveData CH_OutputVoltage = new WaveData();
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_APSVoltage = new WaveData();
                    WaveData CH_CC1 = new WaveData();
                    WaveData CH_CC2 = new WaveData();
                    WaveData CH_FrontEndVoltage = new WaveData();
                    WaveData CH_K1K2 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref CH_APSVoltage, "APSVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1Voltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 5, ref CH_CC2, "CC2Voltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 6, ref CH_FrontEndVoltage, "FrontEndVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");


                    dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    ProcessDataTmp(dicCC1Value, "充电状态流程", "CC1电压", "3.7", "4.3", dicImagePath);
                    ProcessDataTmp(dicCC2Value, "充电状态流程", "CC2电压", "5.7", "6.3");

                    //这个CAN报文数据先放在后面，波形太多不好显示
                    WaveData CH_CHM = new WaveData();
                    WaveData CH_BHMMaxVoltage = new WaveData();
                    WaveData CH_CRMState = new WaveData();
                    WaveData CH_BCP = new WaveData();
                    WaveData CH_CTS = new WaveData();
                    WaveData CH_CML = new WaveData();
                    WaveData CH_BRO = new WaveData();
                    WaveData CH_CRO = new WaveData();
                    WaveData CH_BCS = new WaveData();
                    WaveData CH_BCL = new WaveData();
                    WaveData CH_CCS = new WaveData();
                    WaveData CH_BST = new WaveData();
                    WaveData CH_CST = new WaveData();
                    WaveData CH_BSD = new WaveData();
                    WaveData CH_CSD = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 1, 1, ref CH_CHM, "CHM");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 2, 1, ref CH_BHMMaxVoltage, "BHMMaxVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 3, 1, ref CH_CRMState, "CRMState");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 20, 1, ref CH_BCP, "BCP");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 24, 1, ref CH_CTS, "CTS");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 25, 1, ref CH_CML, "CML");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 29, 1, ref CH_BRO, "BRO");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 30, 1, ref CH_CRO, "CRO");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 132, 1, ref CH_BCS, "BCS");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 129, 1, ref CH_BCL, "BCL");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 137, 1, ref CH_CCS, "CCS");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 39, 0, ref CH_BST, "BST");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 42, 0, ref CH_CST, "CST");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 45, 0, ref CH_BSD, "BSD");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 52, 0, ref CH_CSD, "CSD");
                    SendNoticeToUIAndTxtFile("读取录波板数据完成");


                    string sState = "充电连接控制过程";
                    ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    SendNoticeToUIAndTxtFile("读取辅源首个6V上升沿时间点中...");
                    double K3K4Time1 = 0;
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_APSVoltage, true, 6, ref K3K4Time1);
                    dic.Add(1, K3K4Time1.ToString());
                    //ProcessDataTmp(dic, "开辅源K3K4", "...", "-", "-");
                    ProcessDataTmp(dic, sState, "辅源首个6V上升沿时间点", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "开辅源K3K4");


                    SendNoticeToUIAndTxtFile("读取CHM桩握手首个电平上升沿时间点中...");
                    double CHMTime1 = 0;
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CHM, true, 6, ref CHMTime1);
                    dic[1] = CHMTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CHM--->", K3K4Time1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CHM桩握手首个电平上升沿时间点", K3K4Time1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CHM--->", true, "...");



                    SendNoticeToUIAndTxtFile("读取BHM车辆握手电压首个电平100上升沿时间点中...");
                    double BHMTime1 = 0;
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BHMMaxVoltage, true, 100, ref BHMTime1);
                    dic[1] = BHMTime1.ToString();
                    //ProcessDataTmp(dic, "...", "<---BHM---", CHMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BHM车辆握手电压首个电平100上升沿时间点", CHMTime1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BHM---", true, "...");

                    SendNoticeToUIAndTxtFile("读取K1K2前端高压，首个60V下降沿时间点中...");
                    double K1K2Time1 = 0;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, false, 60, ref K1K2Time1, BHMTime1);
                    dic[1] = K1K2Time1.ToString();
                    //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "首个泄放电压时间点(K1K2前端高压)", BHMTime1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "泄放电压");


                    SendNoticeToUIAndTxtFile("读取K1K2-sig 首个6V上升沿时间点中...");
                    double K1K2SigTime1 = 0;
                    double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref K1K2SigTime1);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref K1K2SigTime1);
                    }
                    dic[1] = K1K2SigTime1.ToString();
                    //ProcessDataTmp(dic, "闭合K1K2绝缘检测ok后断开K1K2", "...", BHMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2绝缘检测ok后断开时间点", K1K2Time1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "闭合K1K2绝缘检测ok后断开K1K2");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "---CRM-00--->", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-00--->", true, "...");

                    ////BRM报文不分析
                    //    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=2时间点中...");
                    //    double PGNTime1 = GetTriggerTime_Single3(15, true, 11, 2, false) * 10;
                    //PGNTime1 = K1K2SigTime1 + 10;
                    //    dic[1] = PGNTime1.ToString();
                    //    //ProcessDataTmp(dic, "...", "<---BRM---", K1K2SigTime1.ToString(), "-");
                    //    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=2时间点", K1K2SigTime1.ToString(), "-");
                    //    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRM---", true, "...");



                    SendNoticeToUIAndTxtFile("读取CRM充电机辨识首个电平1上升沿时间点中...");
                    double CRMTime1 = 0;
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CRMState, true, 100, ref CRMTime1);
                    dic[1] = CRMTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CRM-AA--->", PGNTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CRM-AA 充电机辨识首个电平上升沿时间点", K1K2Time1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-AA--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=6时间点中...");
                    double PGNTime2 = 0;// GetTriggerTime_Single3(15, true, 11, 6, false) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BCP, true, 100, ref PGNTime2);
                    dic[1] = PGNTime2.ToString();
                    //ProcessDataTmp(dic, "...", "<---BCP---", CRMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=6时间点(BCP)", CRMTime1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BCP---", true, "...");

                    ////CTS没有数据，暂不做
                    //SendNoticeToUIAndTxtFile("读取CTS桩时间年首个电平1上升沿时间点中");
                    //double CTSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 19, 1, 0, false, false, 0.05) * 10;
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CTS, true, 1, ref CTSTime1, PGNTime2);
                    //dic[1] = CTSTime1.ToString();
                    ////ProcessDataTmp(dic, "...", "---CTS--->", PGNTime2.ToString(), "-");
                    //ProcessDataTmp(dic, sState, "CTS桩时间年首个电平上升沿时间点", PGNTime2.ToString(), "-");
                    ////ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CTS--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取CML电压首个200上升沿时间点中");
                    double CMLTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 20, 200, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CML, true, 10, ref CMLTime1, PGNTime2);
                    dic[1] = CMLTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CML--->", CTSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CML电压首个200上升沿时间点", PGNTime2.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CML--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取直流高压第二个150V电平上升沿时间点中");
                    double VoltageTime1 = 0;// OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 0, false, false, 0.05, 2) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, true, 150, ref VoltageTime1, PGNTime2);
                    dic[1] = VoltageTime1.ToString();
                    //ProcessDataTmp(dic, "...", "闭合K5K6", CMLTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "闭合K5K6时间点", CMLTime1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "闭合K5K6", true, "...");
                    dic[1] = " ";
                    //ProcessDataTmp(dic, "...", "绝缘监视", "-", "-");
                    ProcessDataTmp(dic, sState, "绝缘监视", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "绝缘监视", true, "...");


                    SendNoticeToUIAndTxtFile("读取BRO电池准备就绪首个电平1上升沿时间点中...");
                    double BROTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 12, 1, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BRO, true, 150, ref BROTime1, CMLTime1);
                    dic[1] = BROTime1.ToString();
                    //ProcessDataTmp(dic, "...", "<---BRO-AA---", VoltageTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BRO电池准备就绪首个电平上升沿时间点", VoltageTime1.ToString(), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRO-AA---", true, "...");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "---CRO-00--->", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRO-00--->", true, "...");



                    SendNoticeToUIAndTxtFile("读取K1K2-sig第二个6V上升沿时间点中...");
                    double K1K2SigTime2 = 0;
                    K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, false, 6, ref K1K2SigTime2, VoltageTime1);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, true, 6, ref K1K2SigTime2, VoltageTime1);
                    }
                    dic[1] = K1K2SigTime2.ToString();
                    //ProcessDataTmp(dic, "预充后闭合K1K2", "...", BROTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2-sig预充后闭合时间点", BROTime1.ToString(), "-");



                    SendNoticeToUIAndTxtFile("读取CRO充电机准备就绪首个电平1上升沿时间点中...");
                    double CROTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 13, 1, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CRO, true, 150, ref CROTime1, K1K2SigTime2);
                    dic[1] = CROTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CRO-AA--->", K1K2SigTime2.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CRO充电机准备就绪首个电平上升沿时间点", K1K2SigTime2.ToString(), "-");



                    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=17时间点中...");
                    double PGNTime3 = 0;// GetTriggerTime_Single3(15, true, 11, 17, false) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CRO, true, 150, ref PGNTime3, CROTime1);
                    dic[1] = PGNTime3.ToString();
                    //ProcessDataTmp(dic, "...", "<---BCS---", CROTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=17时间点(BCS)", CROTime1.ToString(), "-");


                    SendNoticeToUIAndTxtFile("读取BCL电流需求首个电平上升沿时间点中...");
                    double BCLTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 1, -250, 1, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BCL, true, 50, ref BCLTime1, CROTime1);
                    dic[1] = BCLTime1.ToString();
                    //ProcessDataTmp(dic, "...", "<---BCL---", PGNTime3.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BCL电流时间点", CROTime1.ToString(), "-");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "调节电流", "-", "-");


                    SendNoticeToUIAndTxtFile("读取CCS桩充电电流首个电平上升沿时间点中...");
                    double CCSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 2, -100, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CCS, true, 50, ref CCSTime1, CROTime1);
                    dic[1] = CCSTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CCS--->", BCLTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CCS桩充电电流首个电平上升沿时间点", BCLTime1.ToString(), "-");


                    SendNoticeToUIAndTxtFile("读取BMS停充首个电平1下降沿时间点中...");
                    double BMSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 3, 1, 1, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CST, true, 1, ref BMSTime1, CCSTime1);
                    dic[1] = BMSTime1.ToString();
                    //ProcessDataTmp(dic, "...", "<---BST/CST-->", CCSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BMS停充首个电平1下降沿时间点(BST/CST)", CCSTime1.ToString(), "-");



                    SendNoticeToUIAndTxtFile("读取BSD结束中止电量首个电平1上升沿时间点中...");
                    double BSDTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 7, 1, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BSD, true, 1, ref BSDTime1, BMSTime1);
                    dic[1] = BSDTime1.ToString();
                    //ProcessDataTmp(dic, "...", "<---BSD---", BMSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BSD结束中止电量首个电平上升沿时间点", BMSTime1.ToString(), "-");


                    SendNoticeToUIAndTxtFile("读取CSD结束统计电能首个电平1上升沿时间点中...");
                    double CSDTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CSD, true, 0.1, ref CSDTime1, BSDTime1);
                    dic[1] = CSDTime1.ToString();
                    //ProcessDataTmp(dic, "...", "---CSD--->", BSDTime1.ToString(), "-");
                    if (CSDTime1 != 0)//这里有的桩回过来的报文都是0，计算会不合格，这里做判断
                    {
                        ProcessDataTmp(dic, sState, "CSD结束统计电能首个电平上升沿时间点", BSDTime1.ToString(), "-");
                    }
                    else
                    {
                        CSDTime1 = BSDTime1;
                    }


                    SendNoticeToUIAndTxtFile("读取K1K2-sig第二个6V下降沿时间点中...");
                    double K1K2SigTime3 = 0;
                    K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, false, 6, ref K1K2SigTime3, CSDTime1);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, true, 6, ref K1K2SigTime3, CSDTime1);
                    }

                    dic[1] = K1K2SigTime3.ToString();
                    //ProcessDataTmp(dic, "断开K1/K2", "...", CSDTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2-sig第二个开关断开时间点", CSDTime1.ToString(), "-");




                    SendNoticeToUIAndTxtFile("读取高压第二个100V下降沿时间点中...");
                    //double VoltageTime2 = OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 1, false, false, 0.05, 2) * 10;
                    double VoltageTime2 = 0;// GetTriggerTime_Single2_NoCursor(4, false, 0, 150, 1, false, false, 0.05, true) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, false, 60, ref VoltageTime2, CSDTime1);
                    dic[1] = VoltageTime2.ToString();
                    //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime3.ToString(), "-");
                    ProcessDataTmp(dic, sState, "第二个泄放电压时间点", K1K2SigTime3.ToString(), "-");


                    SendNoticeToUIAndTxtFile("读取辅源6V下降沿时间点中...");
                    double K3K4Time2 = 0; //GetTriggerTime_Single2_NoCursor(2, false, 0, 9, 1, false, false, 0.05, true) * 10;
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_APSVoltage, false, 6, ref K3K4Time2, VoltageTime2);
                    dic[1] = K3K4Time2.ToString();
                    //ProcessDataTmp(dic, "断开K3/K4", "...", VoltageTime2.ToString(), "-");
                    ProcessDataTmp(dic, sState, "辅源6V下降沿时间点(断开K3/K4)", VoltageTime2.ToString(), "-");



                    CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
                    dic.Clear();
                    foreach (var item in DicManualVerifyResult)
                    {
                        dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                    }
                    ProcessDataTmp(dic, "正常充电", "应可靠锁止", "-", "-");




                    //流程结束,恢复BMS电压
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(100);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "充电控制时序测试" + "结束--------------------------->");
            }
        }



        /// <summary>
        /// 绝缘异常测试
        /// </summary>
        public void StartItemFlow_4()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "绝缘异常测试" + "--------------------------->");
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //添加测试条件
                    SetConditionValues();
                    //d1 = new Dictionary<int, string>();
                    //foreach (int item in testWorkParam.lstIDs)
                    //{
                    //    d1.Add(item, LstChargerInfo[0].NominalVoltage.ToString("F2"));
                    //}
                    //SetConditionValue("BMS需求电压(V)", d1);

                    #region 在绝缘检测前，模拟 K1 和 K2 外侧电压绝对值＞10 V，检查充电机应停止绝缘检测过程，并发出告警提示
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 100);
                    Thread.Sleep(1000);
                    //过压控制开关
                    var kstate = GetKStatus16_Charging_DC();
                    kstate[26] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(3000);

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        //外侧电压具体是模拟过压后的导引充电电压
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    //SetConditionValue("K1K2外侧电压(V)", d1);
                    ProcessDataTmp(d1, "绝缘前模拟外侧电压>10V", "绝缘检测前外侧电压(V)", "-", "-");


                    SendNoticeToUIAndTxtFile("等待刷卡");
                    int timeout = 100;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        if (ChangeBMSChargeStatus(bmsData.First().Value.ChargingState) > 4)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    }
                    if (ChangeBMSChargeStatus(d1.First().Value) != 9)
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "充电状态", d1.First().Value, EmTrialResult.Pass);
                    else
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "充电状态", d1.First().Value, EmTrialResult.Fail);

                    CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "应发出告警提示", "有告警", EmTrialResult.Pass);
                    }
                    else
                    {
                        ProcessDataResult("绝缘前模拟外侧电压>10V", "应发出告警提示", "未告警", EmTrialResult.Fail);
                    }

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    //过压控制开关
                    kstate = GetKStatus16_Charging_DC();
                    kstate[26] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(100);
                    #endregion

                    SendNoticeToUIAndTxtFile("设置DC+非对称漏电300KΩ");
                    List<bool> lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[7] = true;//DC+非对称漏电300K
                                        //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC+非对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile("设置DC-非对称漏电300KΩ");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[15] = true;//DC-非对称漏电300K
                                         //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC-非对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    SendNoticeToUIAndTxtFile("设置DC-、DC+对称漏电300K");
                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[7] = true;//DC+非对称漏电300K
                    lstKState[8] = false;
                    lstKState[15] = true;//DC-非对称漏电300K
                                         //lstKState[22] = false;//断开  模拟拔枪
                    TrialMethod("DC+、DC-对称漏电300KΩ", "应能充电", "应能告警", lstKState);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[2] = true;//DC+非对称漏电33K
                    TrialMethod("DC+非对称漏电33KΩ", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[8] = false;
                    lstKState[10] = true;//DC-非对称漏电33K
                    TrialMethod("DC-非对称漏电33KΩ", "应不能充电", "应能告警", lstKState, false);


                    lstKState = GetKStatus16_Charging_DC();
                    lstKState[0] = false;
                    lstKState[2] = true;//DC+非对称漏电33K
                    lstKState[8] = false;
                    lstKState[10] = true;//DC-非对称漏电33K
                    TrialMethod("DC+、DC-对称漏电33KΩ", "应不能充电", "应能告警", lstKState, false);


                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "output_DC_V", "1", "V", false, "200", "-2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "A", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "-0.6");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Time(testWorkParam.lstIDs, 1, 0, false);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "XY");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_Y(testWorkParam.lstIDs, 1, 0.987, 0.54);
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "40", 1, "60", "Auto");//下降边沿触发，单次文档上未60V
                    Thread.Sleep(waitTime);


                    SetCPReresh();  // 模拟插拔枪
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    timeout = 300;
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                        if (ALLCanCharge && AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage > 380)   //避免提前触发
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    // 等待进入充电状态
                    //Thread.Sleep(5500);     //避免提前触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    timeout = 60;
                    MessgaeInfo(true, "等待充电中...", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "等待充电中...");

                    Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");


                    ACDownTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 20, 1);//
                                                                                                  //string Customer = ConfigurationManager.AppSettings["Customer"];
                                                                                                  //if (Customer != null && Customer.Equals("DH"))
                                                                                                  //{
                                                                                                  //    Dictionary<int, double> records = new Dictionary<int, double>();
                                                                                                  //    records.Add(testWorkParam.lstIDs[0], 0.42);
                                                                                                  //    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, 1, records, false);
                                                                                                  //}
                                                                                                  //else
                    DCDownTime(testWorkParam.lstIDs, 1, 65, 2);//

                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);//读取卡点时间
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "绝缘电压", "泄放时间(ms)", "0", "1000", dImgs);
                }
            }
            catch (Exception ex) { SendException(ex); }
            finally
            {
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "绝缘异常测试" + "结束--------------------------->");
            }
        }

        private void TrialMethod(string sState, string ItemName1, string ItemName2, List<bool> lstKState, bool CanCharge = true)
        {
            SetCPReresh();  // 模拟插拔枪
            //if (!CheckSwipingCard(testWorkParam.lstIDs))
            //{
            //    return;
            //}
            ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(lstIDs);


            SendNoticeToUIAndTxtFile("录波板启动录波...");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
            Thread.Sleep(1000);

            double insulationVolt = 0;
            int timeout = 300;
            MessgaeInfo(true, "请刷卡充电!", true);
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 2 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                if (ALLCanCharge)
                {
                    insulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                    break;
                }

                System.Threading.Thread.Sleep(1000);
            }
            MessgaeInfo(false, "请刷卡充电!");

            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstKState.ToArray());


            // 等待进入充电状态
            MessgaeInfo(true, "等待充电中...", true);

            timeout = 100;
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 5 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                if (ALLCanCharge)
                {
                    insulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                    break;
                }

                System.Threading.Thread.Sleep(1000);
            }

            Thread.Sleep(1000);
            SendNoticeToUIAndTxtFile("录波板停止录波...");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
            Thread.Sleep(500);


            foreach (int item in testWorkParam.lstIDs)
            {
                timeout = 600;
                while (timeout-- > 0)
                {
                    int bmsState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    if (bmsState <= 6)
                    {
                        double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                    }
                    bool ALLCanCharge = bmsState == 9 || bmsState == 13 || bmsState == 0;  //进入充电中或充电结束阶段
                    if (ALLCanCharge)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
            MessgaeInfo(false, "等待充电中...");

            d1 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                d1.Add(item, LstChargerInfo[0].NominalVoltage.ToString());
            }
            ProcessDataTmp(d1, sState, "车辆通信握手报文的最高允许充电电压(V)", "-", "-");



            WaveData CH_OutputVoltage = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
            insulationVolt = DataAnalysis_WaveRecoder.GetWavePointMaxVave(CH_OutputVoltage);

            d1 = new Dictionary<int, string>();
            foreach (var item in testWorkParam.lstIDs)
            {
                d1.Add(item, insulationVolt.ToString());
            }
            //绝缘检测电压应符合 GB/T 18487.1—2015 中 B.3.3 的规定
            ProcessDataTmp(d1, sState, "绝缘电压(V)", (LstChargerInfo[0].NominalVoltage * 0.99).ToString("F2"), (LstChargerInfo[0].NominalVoltage * 1.01).ToString("F2"));

            d1 = new Dictionary<int, string>();
            d2 = new Dictionary<int, string>();
            var dicVolt = new Dictionary<int, string>();
            string state = AllEquipStateData.DicBMS_DC_StateData[1].ChargingState;
            if (CanCharge)
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage < LstChargerInfo[0].NominalVoltage * 0.9 || voltage > LstChargerInfo[0].NominalVoltage * 1.1)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                        else
                            break;
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "-", "-");
                if (state == "充电中")
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Fail);
                }
            }
            else
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage > 20)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "0", "20");
                if (state != "充电中")
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Fail);
                }
            }


            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
            d1.Clear();
            d2.Clear();
            if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
            {
                ProcessDataResult(sState, ItemName2, "有告警", EmTrialResult.Pass);
            }
            else
            {
                ProcessDataResult(sState, ItemName2, "未告警", EmTrialResult.Fail);
            }
            List<bool> lstK = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstK.ToArray());
            Thread.Sleep(300);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        private void ProcessDataResult(string CheckState, string ItemName, string strResult, EmTrialResult trialResult)
        {

            LstTrialData[0].BarCode = LstChargerInfo[0].BarCode;
            LstTrialData[0].TrialName = TrialItem.ItemName;
            LstTrialData[0].SchemeName = TrialItem.SchemeName;
            LstTrialData[0].SchemeID = TrialItem.SchemeID;
            LstTrialData[0].ItemName = iIndex.ToString();
            LstTrialData[0].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[0].ChargerId].ChargingVoltage;

            LstTrialData[0].TrialResult = trialResult;


            LstTrialData[0].PKID = LstChargerInfo[0].PKID;
            //界面展示的数据项格式
            //状态|测试结果     
            LstTrialData[0].ExtentData = CheckState + "|" + ItemName + "|-|-|" + strResult + "|" + "报表(勿删)";
            LstTrialData[0].Data2 = LstTrialData[0].ExtentData;
            LstTrialData[0].Data3 = TrialItem.TrialOrder.ToString();
            SendTrialDataToUI(LstTrialData[0]);
            SaveTrialData(LstTrialData[0]);
            iIndex++;

        }



        /// <summary>
        /// 机械锁异常测试
        /// </summary>
        public void StartItemFlow_5()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "机械锁异常测试" + "--------------------------->");
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
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

                    //ControlEquipMent.BMS.BMS_ON(lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    d1.Clear();
                    d2.Clear();
                    //设置测试条件
                    SetConditionValues();

                    ItemFlow = "正常充电状态";
                    CountDownTimeInfo("请确认车辆插头电子锁应可靠锁止\r\n勾选上代表正常锁止", 20, 2);
                    ProcessData_5();

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage.ToString());
                    }
                    ProcessDataTmp(d1, ItemFlow, "CC1(V)", "3.65", "4.37");



                    ItemFlow = "模拟电子锁故障";
                    CountDownTimeInfo("请模拟充电机故障停机，然后点击确认按钮", 100, 0);
                    CountDownTimeInfo("请确认车辆插头电子锁解锁\r\n勾选上代表正常解锁", 20, 2);
                    ProcessData_5();

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                    }
                    ProcessDataTmp(d1, ItemFlow, "解锁时车辆接口电压应降至60VDC以下", "-", "60");
                    CountDownTimeInfo("请恢复充电机故障，然后点击确认按钮", 100, 0);


                    ItemFlow = "检查电子锁装置应具备应急解锁功能";
                    CountDownTimeInfo("请检查电子锁装置是否具备应急解锁功能\r\n勾选上代表具备", 20, 2);
                    ProcessDataConnect(ItemFlow, "是否具备");
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "机械锁异常测试" + "结束--------------------------->");
            }
        }

        public  void ProcessData_5()
        {
            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].ItemName = ItemFlow;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;

                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    if (ItemFlow == "正常充电状态")
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止和加外力检查有效性\r\n|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否允许充电\r\n|-|-|否";
                    }

                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    if (ItemFlow == "正常充电状态")
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止和加外力检查有效性\r\n|-|-|否";
                    }
                    else
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否允许充电\r\n|-|-|是";
                    }
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     

                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);


            }

        }



        /// <summary>
        /// 通信异常测试
        /// </summary>
        public void StartItemFlow_6()
        {
            SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");


            _StopWatch.Reset();
            _StopWatch.Start();
            while (true)
            {
                #region  ------  此部分代码保留,作用可忽略---------------


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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                #endregion

                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                ////Thread.Sleep(1000 * 10);
                //ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Single");
                SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
                Thread.Sleep(1000 * 40);

                SBitS[31] = true;//停止发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                Thread.Sleep(500);


                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(15000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                WaveData CH_CEM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 37, 2, ref CH_CEM, "CEM_BCL");
                double Time_CEM = 0;
                DataAnalysis_WaveRecoder.GetCANMsgTime(CH_CEM, 1, true, ref Time_CEM);//获取CEM出现的地方
                int timeout = 20;
                //double CEM = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CEM, CH_CEM.LinePoints_Y.Count - 2);//获取CEM最后的值
                double CEM = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CEM, (int)(Time_CEM + 5));//获取CEM的值,XJ客户那里的会重新变为0，这里做一下处理



                ItemFlowIndex = 1;
                crm_state = "通讯中断";
                ProcessData_6();
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic.Add(1, CEM.ToString());
                ProcessDataTmp(dic, "通讯中断", "CEM报文", "1", "1");



                //读取录波板数据
                double Time_K1K2 = 0;
                WaveData CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CEM);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                double Time_Stop = Math.Abs(Time_CEM - Time_K1K2);

                SendNoticeToUIAndTxtFile("计算第一次测试结果...");

                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                Thread.Sleep(200);
                CountDownTimeInfo("请确认电子锁能正常解锁。\r\n注：勾选上为可以正常解锁", 20, 2);
                ProcessDataConnect();

                dic[1] = (Time_Stop / 1000).ToString();
                ProcessDataTmp(dic, "通讯中断", "K1K2断开时间(s)", "0", "10", dicImagePath);


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                ControlEquipMent.Oscillograph.Oscillograph_TimeBase("20");
                ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

                SetCPReresh();
                SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                timeout = 300;
                MessgaeInfo(true, "请刷卡充电!", true);
                while (timeout-- > 0)
                {
                    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                        continue;
                    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                    if (ALLCanCharge)
                    {
                        MessgaeInfo(false, "请刷卡充电!");
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                MessgaeInfo(false, "请刷卡充电!");
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);

                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                dicImagePath = null;
                crm_state = "CRM-1";
                ItemFlowIndex = 2;
                TestFlow(20);


                crm_state = "CRM-2";
                ItemFlowIndex = 3;
                TestFlow(20);

                crm_state = "CRM-3";
                ItemFlowIndex = 4;
                TestFlow(20);


                crm_state = "CRM-4";
                ItemFlowIndex = 5;
                TestFlow(40);

                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                CH_CEM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 37, 2, ref CH_CEM, "CEM_BCL");
                CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                WaveData CH_Voltage = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_Voltage, "ChargingVoltage");


                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                crm_state = "第四次重连结束";
                ItemFlowIndex = 6;
                CountDownTimeInfo("请给桩刷卡上电，刷卡后点击确认按钮，或者等待倒计时结束。\r\n（注:刷卡前请勿拔插枪。合格桩此处应【不允许充电】）", 50, 2);
                Thread.Sleep(2000);
                ProcessData_6();
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            }
        }
        private void TestFlow(int time)
        {
            SendNoticeToUIAndTxtFile($"开始第{ItemFlowIndex - 1}次超时重连。");
            SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
            //Thread.Sleep(5000);
            SBitS[31] = true;//停止发送报文
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            Thread.Sleep(3000);
            if (ItemFlowIndex >= 2 && ItemFlowIndex <= 4)
            {
                SendNoticeToUIAndTxtFile("发送恢复通讯指令");
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
            }
            SendNoticeToUIAndTxtFile($"等待{time}秒恢复充电");
            Thread.Sleep(1000 * time);
            ProcessData_6();
            //Thread.Sleep(1000);

        }

        public void ProcessData_6()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();


                    if (dicImagePath != null)
                    {
                        sbtmp.Append(dicImagePath[item]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }


                    string result = "";

                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    string info = "";
                    if (ItemFlowIndex == 1 || ItemFlowIndex >= 5)
                    {
                        if (state >= 7 && state <= 10)//准备开始充电
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            result = "允许充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            result = "不允许充电";
                        }
                        info = "应不允许充电";
                    }
                    else
                    {
                        if (state == 9)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            result = "已恢复充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            result = "未恢复充电";
                        }
                        info = "应恢复充电";
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果      

                    LstTrialData[i].ExtentData = crm_state
                    + "|" + info
                    + "|" + "-"
                    + "|" + "-"
                    + "|" + result
                    + "|" + sbtmp.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);

                    SaveTrialData(LstTrialData[k]);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }



    }
}
