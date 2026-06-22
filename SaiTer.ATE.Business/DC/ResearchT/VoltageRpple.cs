using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
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
    /// 国标直流研发:电压纹波
    /// </summary>
    public class VoltageRpple : BusinessBase
    {

        int Count = 0;

        /// <summary>
        /// 带载余量(A)=10
        /// </summary>
        Double Aallowance = 10;
        /// <summary>
        /// 判定准则(±%)
        /// </summary>
        double ErrorRate =1;
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>, Dictionary<int, string>)> VoltageLists1 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>, Dictionary<int, string>)> VoltageLists2 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>, Dictionary<int, string>)> VoltageLists3 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();
        /// <summary>
        /// 电压需求
        /// </summary>
        List<double> BMSVoltage = new List<double>();
        /// <summary>
        /// 电源模块恒功率最小电压(V)
        /// </summary>
        Double ModuleMinimumVoltage = 300;

        int trlTimeOut_S = 30;

        /// <summary>
        /// 测试时间
        /// </summary>
        int TestTime = 1;
        /// <summary>
        /// 输入电压85%额定值(V)
        /// </summary>
        double ACVoltage1 = 187;
        /// <summary>
        /// 输入电压100%额定值(V)
        /// </summary>
        double ACVoltage2 = 220;

        /// <summary>
        /// 输入电压115%额定值(V)
        /// </summary>
        double ACVoltage3 = 253;

        public VoltageRpple(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
            SetCPReresh();
            SendNoticeToUIAndTxtFile("示波器初始化中");
            OscilloscopeVoltageStabilizationAccuracy();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            Count = 0;
            VoltageLists1 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();
            VoltageLists2 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();
            VoltageLists3 = new List<(string, Dictionary<int, string>, Dictionary<int, string>)>();

            if (strParams.Length >= 7)
            {
                //输入电压85%额定值(V)=187|输入电压100%额定值(V)=220|输入电压115%额定值(V)=253|判定准则(±%)=1|测试时间(S)=1|带载余量(A)=10|电源模块恒功率最小电压(V)=300
                ACVoltage1 = double.Parse(strParams[0].Split('=')[1]);
                ACVoltage2 = double.Parse(strParams[1].Split('=')[1]);
                ACVoltage3 = double.Parse(strParams[2].Split('=')[1]);
                ErrorRate = double.Parse(strParams[3].Split('=')[1]) ;
                double time = double.Parse(strParams[4].Split('=')[1]);
                Aallowance = double.Parse(strParams[5].Split('=')[1]);
                TestTime = int.Parse(time.ToString());
                ModuleMinimumVoltage = double.Parse(strParams[6].Split('=')[1]);
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
                SetACSource(lstIDs, 220);
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
                    SetConditionValues();
                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("开启并机继电器");
                    CombineControlResistance();

                    SendNoticeToUIAndTxtFile("开启负载并机");
                    if (ControlEquipMent.FeedbackLoad != null)
                    {
                        ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    }
                    else
                    {

                    }
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);


                    List<double> BMSCurrent = new List<double>();
                    double DCurrent = 0;
                    DCurrent = Convert.ToDouble(((MaxOutputPower * 1000) / ModuleMinimumVoltage).ToString("f2"));
                    DCurrent = DCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DCurrent;
                    DCurrent = RetainDecimals<double>(DCurrent);
                    BMSCurrent.Add(DCurrent);

                    DCurrent = Convert.ToDouble(((MaxOutputPower * 1000) / MidAllowChargeVoltage).ToString("f2"));
                    DCurrent = DCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DCurrent;
                    DCurrent = RetainDecimals<double>(DCurrent);
                    BMSCurrent.Add(DCurrent);

                    DCurrent = Convert.ToDouble(((MaxOutputPower * 1000) / MaxAllowChargeVoltage).ToString("f2"));
                    DCurrent = DCurrent >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DCurrent;
                    DCurrent = RetainDecimals<double>(DCurrent);
                    BMSCurrent.Add(DCurrent);



                    BMSVoltage.Add(MinAllowChargeVoltage);
                    BMSVoltage.Add(MidAllowChargeVoltage);
                    BMSVoltage.Add(MaxAllowChargeVoltage);



                    double[] Current = new double[3];
                    Current[0] = 0;
                    double MaxCurrent = (MaxOutputPower * 1000) / MaxAllowChargeVoltage;
                    double MidCurrent = MaxCurrent / 2;

                    Current[1] = MidCurrent;



                    Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / ModuleMinimumVoltage).ToString("f2"));

                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                    Current = RetainDecimals<double>(Current);







                    List<double> ACVoltage = new List<double>();
                    ACVoltage.Add(ACVoltage1);
                    ACVoltage.Add(ACVoltage2);
                    ACVoltage.Add(ACVoltage3);


                    SendNoticeToUIAndTxtFile("启动充电中");

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);

                    OscilloscopeVoltageStabilizationAccuracy(BMSVoltage[0]);
                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < Current.Length; k++)
                        {
                            VoltageAccuracy(BMSVoltage[0], BMSCurrent[0], Current[k], ACVoltage[j], k);
                        }

                    }


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("启动充电中");
                    SetCPReresh();
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, true, 390);

                    Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / MidAllowChargeVoltage).ToString("f2"));
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);
                    OscilloscopeVoltageStabilizationAccuracy(BMSVoltage[1]);
                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < Current.Length; k++)
                        {
                            VoltageAccuracy(BMSVoltage[1], BMSCurrent[1], Current[k], ACVoltage[j], k);
                        }

                    }
                    Current[2] = Convert.ToDouble(((MaxOutputPower * 1000) / MaxAllowChargeVoltage).ToString("f2"));
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);
                    OscilloscopeVoltageStabilizationAccuracy(BMSVoltage[2]);
                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < Current.Length; k++)
                        {
                            VoltageAccuracy(BMSVoltage[2], BMSCurrent[2], Current[k], ACVoltage[j], k);
                        }

                    }

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent > 1)
                    {
                        SetLoadDCOFF(testWorkParam.lstIDs);
                    }


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("判断结果中!");
                    JudgeResult(VoltageLists1);

                    JudgeResult(VoltageLists2);

                    JudgeResult(VoltageLists3);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        private void JudgeResult(List<(string, Dictionary<int, string>, Dictionary<int, string>)> value)
        {
            if (value.Count <= 0)
            {
                return;
            }


            for (int i = 0; i < value.Count; i++)
            {
                ProcessDataTmp(value[i].Item2, value[i].Item1, "电压纹波因数", "0", ErrorRate.ToString(),value[i].Item3);
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


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|否";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }


        public void VoltageAccuracy(double BMSVoltageSet, double BMSCurrentSet, double CurrentSet, double ACVoltageSet, int CurrentIndex)
        {
            try
            {
                Count++;

                if (IsRLoad(BMSVoltageSet, CurrentSet))
                {
                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent > 1)
                    {
                        SetLoadDCOFF(testWorkParam.lstIDs);
                    }
                    if ((Count - 1) % 3 == 0)
                    {

                        SetACSource(testWorkParam.lstIDs, ACVoltageSet);
                    }



                    CurrentSet = CurrentSet >= MaxAllowChargeCurrent ? MaxAllowChargeCurrent : CurrentSet;
                    CurrentSet = CurrentIndex == 2 ? (CurrentSet - Aallowance) : CurrentSet;

                    if (Count == 1 || Count == 19 || Count == 10)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltageSet, BMSCurrentSet, true, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, BMSVoltageSet);
                    }

                    Thread.Sleep(4000);

                    if (CurrentSet > 0)
                    {

                        SetLoadPara(testWorkParam.lstIDs, BMSVoltageSet - 20, CurrentSet, BMSVoltageSet, CurrentSet - 4);

                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent < 4)
                        {
                            SetLoadDCON(testWorkParam.lstIDs);
                        }
                    }
                    else
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingCurrent > 1)
                        {
                            SetLoadDCOFF(testWorkParam.lstIDs);
                        }
                    }




                    int Time = TestTime + 15;
                    SendNoticeToUIAndTxtFile("点位运行时间:" + Time + "s");

                    if (TestTime != 0)
                    {
                        Thread.Sleep(TestTime * 1000);
                    }



                    WaitDCCurrent(testWorkParam.lstIDs, CurrentSet-4);


                    System.Threading.Thread.Sleep(15 * 1000);
                    if (Count == 10 || Count == 19)
                    {
                        SendNoticeToUIAndTxtFile("延时20s");
                        System.Threading.Thread.Sleep(20 * 1000);
                    }
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        Dictionary<int, string> RipplewaveS = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs,"PKPK",1);
                        double Ripplewave = double.Parse(RipplewaveS[item]);
                      for(int i=0;i<20;i++)
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                            if (Ripplewave > 0)
                            {
                                break;
                            }
                            Thread.Sleep(100);
                            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                            RipplewaveS = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1);
                            Ripplewave = double.Parse(RipplewaveS[item]);
               
                        
                        }
                        double ResultError = Convert.ToDouble(Ripplewave / DCVoltage / 2);
                        ResultError = System.Math.Abs(RetainDecimals<double>(ResultError));
                        ResultError = ResultError * 100;
                        dic.Add(item, ResultError.ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    if (BMSVoltageSet == BMSVoltage[0])
                    {
                        VoltageLists1.Add(("充电电压:" + BMSVoltage[0] + ";带载电流:" + CurrentSet , dic,dImgs));
                    }
                    if (BMSVoltageSet == BMSVoltage[1])
                    {
                        VoltageLists2.Add(("充电电压:" + BMSVoltage[1] + ";带载电流:" + CurrentSet, dic, dImgs));
                    }
                    if (BMSVoltageSet == BMSVoltage[2])
                    {
                        VoltageLists3.Add(("充电电压:" + BMSVoltage[2] + ";带载电流:" + CurrentSet , dic, dImgs));
                    }
                    System.Threading.Thread.Sleep(2000);

                }




            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public void OscilloscopeVoltageStabilizationAccuracy()//示波器电压纹波
        {

            try
            {



                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "5", "0");//通道1设置2
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "50", "-2");//通道2设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置

                System.Threading.Thread.Sleep(100);

                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//初始化测量
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 1);
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);
                System.Threading.Thread.Sleep(100);


                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



        }
        public void ProcessData(bool Status)
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
                    LstTrialData[k].ItemName = iIndex.ToString();
                    string State = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingState;

                    if (Status)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = TrialItem.ItemName + "|充电状态|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public void OscilloscopeVoltageStabilizationAccuracy(double Voltage)//示波器电压纹波
        {

            try
            {


                System.Threading.Thread.Sleep(500); //
                if (Voltage == 200)
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", Channel1, "Output_DC_Ripple", "1", "V", false, "5", "0");

                    System.Threading.Thread.Sleep(100);
                }
               else  if (Voltage == 600)
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", Channel1, "Output_DC_Ripple", "1", "V", false, "5", "0");
                    System.Threading.Thread.Sleep(100);
                }
                else if (Voltage == 1000)
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", Channel1, "Output_DC_Ripple", "1", "V", false, "5", "0");
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20", Channel1, "Output_DC_Ripple", "1", "V", false, "5", "0");
                    System.Threading.Thread.Sleep(100);
                }

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }

    }
}
