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
    /// <summary>
    /// 国标研测直流：甩负载试验
    /// </summary>
    public class GB_RT_DC_LoadDumpTest : BusinessBase
    {
        public GB_RT_DC_LoadDumpTest(int type)
        {
            TrialType = type;
        }
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 60;

        public override void InitEquiMent()
        {
            OscilloscopeInputCurrentOvershootE();
        }
        /// <summary>
        /// 突卸载测试
        /// </summary>
        public void OscilloscopeInputCurrentOvershootE()
        {
            int time = 50;
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20M", Channel1, "Output_V", "1M", "V", false, "100", "-4");
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "DC", "20M", Channel2, "Output_I", "1M", "A", false, "50", "-2");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "AC", "20M", Channel3, "Input_I", "1M", "A", false, "100", "0");//通道3设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(lstIDs, 4, false, "AC", "20M", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "1000", "0");//设置滚动，时基和触发延时
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//初始化测量
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "MAX", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "TOP", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(time);
        }

        public override void InitializeParams()
        {
            Init();

            //BMS需求电压(V)=750|BMS需求电流(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
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


                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }
                    Thread.Sleep(2000);//

                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent - 10, DemandVoltage, DemandCurrent - 5);
                    SetLoadDCON(testWorkParam.lstIDs);

                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.9 || DCVoltage > DemandVoltage * 1.1)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "正常充电", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "正常充电", "充电电流(A)", "-", "-");

                    Double DemandCurrent2 = MaxOutputPower / MaxAllowChargeVoltage;
                    DemandCurrent2 = DemandCurrent * 0.1;
                    Thread.Sleep(5 * 1000);
                    SendNoticeToUIAndTxtFile("正在设置示波器触发中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(lstIDs, 0, "FALL", "DC", "", "", 2, (DemandCurrent / 2).ToString(), "Single");
                    Thread.Sleep(10 * 1000);

                    SendNoticeToUIAndTxtFile("设备改变负载至10%，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 5, DemandVoltage - 5, DemandCurrent2);
                    Thread.Sleep(2000);

                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent2 * 0.9 || DCCurrent > DemandCurrent2 * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dicC2, "甩负载时", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("判断结果中...");
                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Dictionary<int, string> Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);
                    double MaxVolt = Convert.ToDouble(Data_Tmp.First().Value);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 1);
                    double RmsVolt = Convert.ToDouble(Data_Tmp.First().Value);
                    d1 = new Dictionary<int, string>();
                    foreach(int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, (MaxVolt - RmsVolt).ToString("F2"));
                    }
                    ProcessDataTmp(d1, "甩负载时", "电压过冲值(V)", "-", "50", dImgs);
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d2.Add(item, ((MaxVolt - RmsVolt) / RmsVolt * 100).ToString("F2"));
                    }
                    ProcessDataTmp(d2, "甩负载时", "电压过冲比(%)", "-", "10");

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
