using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 电压纹波
    /// </summary>
    public class CCS2_PT_DC_VoltageRipple : BusinessBase
    {
        public CCS2_PT_DC_VoltageRipple(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 20;



        Double ErrorRate = 5;
        Double ErrorVoltage = 5;


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

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    DemandCurrent = MaxOutputPower * 1000 / MaxAllowChargeVoltage;
                    DemandCurrent = CompareMaximum(DemandCurrent, MaxAllowChargeCurrent);
                    if (ControlEquipMent.FeedbackLoad == null)
                    {
                        Double MaxCurrent1 = DemandVoltage * 0.12;

                        DemandCurrent = CompareMaximum(DemandCurrent, MaxCurrent1);
                    }
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, DemandCurrent, true, 390);

                    //ControlEquipMent.BMS.BMS_ON(lstIDs);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, LstChargerInfo[0].NominalVoltage);






                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);






                    #region 最高电压-100%


                    double CheckVoltage1 = DemandVoltage * (1 - ErrorRate);
                    double CheckVoltage2 = DemandVoltage * (1 + ErrorRate);


                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");


                    SetLoadPara(testWorkParam.lstIDs, MaxAllowChargeVoltage - 20, DemandCurrent - 5, MaxAllowChargeVoltage, DemandCurrent - 5);
                    SetLoadDCON(testWorkParam.lstIDs);

                    CountDownTimeInfo("等待带载稳定", 20, 0);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    Dictionary<int, string> PKPKS = new Dictionary<int, string>();
                    PKPKS = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1);

                    Dictionary<int, string> dicV = new Dictionary<int, string>();

                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }



                    ProcessDataTmp(dicV, "负载-100%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());


                    ProcessDataTmp(PKPKS, "负载-100%", "电压纹波峰峰值", "0", "5", dImgs);

                    #endregion




                    #region 最高电压-50%

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

                    CountDownTimeInfo("等待带载稳定", 20, 0);

                    Dictionary<int, string> dImgs2 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> PKPKS2 = new Dictionary<int, string>();
                    PKPKS2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 1);

                    Dictionary<int, string> dicV2 = new Dictionary<int, string>();

                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV2.Add(item, DCVoltage.ToString("F2"));
                    }



                    ProcessDataTmp(dicV2, "负载-50%", "充电电压(V)", CheckVoltage1.ToString(), CheckVoltage2.ToString());


                    ProcessDataTmp(PKPKS2, "负载-50%", "电压纹波峰峰值", "0", "5", dImgs2);





                    #endregion



                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("开启示波器滚动中，请稍候...");


                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
            OscilloscopeVoltageStabilizationAccuracy();
        }


        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 2)
            {

                ErrorRate = Convert.ToDouble(strParams[0].Split('=')[1]) / 100;
                ErrorVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
            DemandVoltage = MaxAllowChargeVoltage;
        }

        public override void ProcessData()
        {

        }
        public void OscilloscopeVoltageStabilizationAccuracy()//示波器电压纹波
        {

            try
            {



                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "AC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "1", "0");//通道1设置2

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "100", "-2");//通道2设置

                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//初始化测量
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 1);

                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

            }
            catch
            {



            }




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
