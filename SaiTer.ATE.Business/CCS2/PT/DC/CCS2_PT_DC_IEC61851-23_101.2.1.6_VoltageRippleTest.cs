using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标产测直流：电压纹波
    /// </summary>
    public class CCS2_PT_DC_VoltageRippleTest : BusinessBase
    {
        public CCS2_PT_DC_VoltageRippleTest(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        Double ErrorRate = 0.05;

        public void OscilloscopeVoltageStabilizationAccuracy()//示波器电压纹波
        {
            int time = 50;
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "AC", "20M", Channel1, "Output_V", "1M", "V", false, "2.5", "0");//通道1设置2
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "DC", "20M", Channel2, "Output_I", "1M", "A", false, "100", "-2");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "AC", "20M", Channel3, "Input_I", "1M", "A", false, "100", "0");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//初始化测量
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "PKPK", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 3);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "", "", 1, (DemandVoltage * 0.95).ToString(), "Auto");
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(time);
        }

        public override void InitEquiMent()
        {
            OscilloscopeVoltageStabilizationAccuracy();
        }

        public override void InitializeParams()
        {
            //±误差(%) = 5
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                ErrorRate = Convert.ToDouble(strParams[0].Split('=')[1]) / 100;
            }
            DemandVoltage = MaxAllowChargeVoltage;
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    DemandCurrent = MaxOutputPower * 1000 / MaxAllowChargeVoltage;
                    DemandCurrent = CompareMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (ControlEquipMent.FeedbackLoad == null)
                    {
                        Double MaxCurrent1 = DemandVoltage * 0.12;

                        DemandCurrent = CompareMaximum(DemandCurrent, MaxCurrent1);
                    }
                    if (!CheckSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, DemandCurrent))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    #region 最高电压--100%
                    double CheckVoltage1 = DemandVoltage * (1 - ErrorRate);
                    double CheckVoltage2 = DemandVoltage * (1 + ErrorRate);


                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, MaxAllowChargeVoltage - 20, DemandCurrent - 5, MaxAllowChargeVoltage, DemandCurrent - 5);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, DemandCurrent - 5);
                    Thread.Sleep(3000);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Dictionary<int, string> PKPKS = new Dictionary<int, string>();
                    PKPKS = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1);

                    Dictionary<int, string> dicV = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicV, "负载--100%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());
                    ProcessDataTmp(PKPKS, "负载--100%", "电压纹波峰峰值", "0", "5", dImgs);

                    #endregion




                    #region 最高电压--50%
                    SendNoticeToUIAndTxtFile("开启示波器滚动中，请稍候...");


                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    DemandCurrent = MaxOutputPower * 1000 / MaxAllowChargeCurrent / 2;
                    DemandCurrent = CompareMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (ControlEquipMent.FeedbackLoad == null)
                    {
                        Double MaxCurrent1 = DemandVoltage * 0.12;

                        DemandCurrent = CompareMaximum(DemandCurrent, MaxCurrent1);
                    }

                    SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, DemandCurrent, true, 390);
                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");


                    SetLoadPara(testWorkParam.lstIDs, MaxAllowChargeVoltage - 20, DemandCurrent - 5, MaxAllowChargeVoltage, DemandCurrent - 5);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, DemandCurrent - 5);
                    Thread.Sleep(3000);

                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Dictionary<int, string> PKPKS2 = new Dictionary<int, string>();
                    PKPKS2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1);

                    Dictionary<int, string> dicV2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV2.Add(item, DCVoltage.ToString("F2"));
                    }

                    ProcessDataTmp(dicV2, "负载--50%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());
                    ProcessDataTmp(PKPKS2, "负载--50%", "电压纹波峰峰值", "0", "5", dImgs2);
                    #endregion



                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void ProcessData()
        {

        }

        public double CompareMaximum(double value, double maxvalue)
        {
            try
            {
                value = value >= maxvalue ? maxvalue : value;
                return value;
            }
            catch
            {
                return 0;
            }
        }
    }
}
