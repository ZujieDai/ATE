using SaiTer.ATE.DataModel.EnumModel;
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
    /// 欧标产测直流：电流纹波试验
    /// </summary>
    public class CCS2_PT_DC_CurrentRippleTest : BusinessBase
    {
        public CCS2_PT_DC_CurrentRippleTest(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 20;
        double DemandVoltage1 = 400;//额定功率电压(V)
        double DemandVoltage2 = 400;//电流最大时电压(V)

        Double ErrorRate = 5;
        Double ErrorVoltage = 5;


        public void OscilloscopeCurrentRipple()
        {
            int time = 50;
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_Ripple", "1M", "V", false, "1", "-2");//通道1设置2
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "AC", "20M", Channel2, "Output_AC_I", "1M", "A", false, "5", "0");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, false, "AC", "20M", Channel4, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//初始化测量
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "PKPK", 2);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(time);
        }

        public override void InitEquiMent()
        {
            OscilloscopeCurrentRipple();
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            //if (strParams.Length >= 2)
            //{

            //    //DemandVoltage1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            //    DemandVoltage2 = Convert.ToDouble(strParams[1].Split('=')[1]);
            //}
            DemandVoltage1 = MaxAllowChargeVoltage;
            DemandVoltage2 = MinAllowChargeVoltage;
        }


        public override void ExecuteMethod()
        {
            try
            {
                SetCPRersh_EUDCALL();
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


                    double DemandCurrent1 = MaxOutputPower * 1000 / DemandVoltage1;
                    double DemandCurrent2 = MaxAllowChargeCurrent; ;
                    DemandCurrent1 = RetainDecimals<double>(DemandCurrent1);
                    DemandCurrent1 = CompareMaximum(DemandCurrent1, MaxAllowChargeCurrent);

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    DemandCurrent = MaxOutputPower * 1000 / MaxAllowChargeVoltage;
                    DemandCurrent = CompareMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (ControlEquipMent.FeedbackLoad == null)
                    {
                        Double MaxCurrent1 = DemandVoltage * 0.12;

                        DemandCurrent = CompareMaximum(DemandCurrent, MaxCurrent1);
                    }
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage1, DemandCurrent1))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    #region 最大功率
                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 10, DemandCurrent1 + 5, DemandVoltage1, DemandCurrent1);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待带载稳定...");
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, DemandCurrent1);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    Dictionary<int, string> PKPKS = new Dictionary<int, string>();
                    PKPKS = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 2);
                    ProcessDataTmp(PKPKS, "最大功率", "电流Ipp(A)", "0", "9", dImgs);
                    #endregion




                    #region 最大电流
                    if (IsRLoad(DemandVoltage2, DemandCurrent2))
                    {
                        SendNoticeToUIAndTxtFile("开启示波器滚动中，请稍候...");
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                        SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage1 - 10, 5, DemandVoltage1, 5);
                        Thread.Sleep(3000);
                        SetLoadDCOFF(testWorkParam.lstIDs);

                        SendNoticeToUIAndTxtFile("设备正在调整导引需求中...");
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage2, DemandCurrent2, true, 390);
                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                        SetLoadPara(testWorkParam.lstIDs, DemandVoltage2 - 10, DemandCurrent2 + 5, DemandVoltage2, DemandCurrent2);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrent_EU_DC(testWorkParam.lstIDs, DemandCurrent2);

                        Dictionary<int, string> dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                        Dictionary<int, string> PKPKS2 = new Dictionary<int, string>();
                        PKPKS2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 2);
                        ProcessDataTmp(PKPKS2, "最大电流时", "电流Ipp(A)", "0", "9", dImgs2);
                    }
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
