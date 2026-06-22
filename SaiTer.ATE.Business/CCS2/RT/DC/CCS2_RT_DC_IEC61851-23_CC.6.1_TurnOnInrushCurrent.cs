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
    /// 欧标研测直流：输出冲击电流
    /// </summary>
    public class CCS2_RT_DC_TurnOnInrushCurrent : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 60;
        /// <summary>
        /// 判定准则(A)
        /// </summary>
        Double ErrorValue = 2;


        public CCS2_RT_DC_TurnOnInrushCurrent(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');

            //BMS需求电压设置(V)=745|BMS需求电流设置(A)=50|判定准则(A)=2
            if (strParams.Length >= 3)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                ErrorValue = double.Parse(strParams[2].Split('=')[1]);
            }
            DemandCurrent = DemandCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;
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
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
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



                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, DemandVoltage, false))
                    {
                        return;
                    }
                    Thread.Sleep(3000);
                    OscilloscopeStartupOutputOvershoot(DemandCurrent);
                    Thread.Sleep(3000);


                    SendNoticeToUIAndTxtFile($"设置负载电压{DemandVoltage - 20}V，电流{DemandCurrent}A...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 20, DemandVoltage - 5, DemandCurrent, false);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadDCON(testWorkParam.lstIDs, false);

                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    Dictionary<int, int> dicTemp = new Dictionary<int, int>();
                    int count = 10;
                    while (count-- > 0)
                    {
                        SendNoticeToUIAndTxtFile("读取是否触发...");
                        dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                        if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                        {
                            ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent / 2).ToString(), "Single");

                            Thread.Sleep(100);
                            break;
                        }
                        else
                        { }

                    }
                    //System.Threading.Thread.Sleep(5000);

                    ReadTriggerType(testWorkParam.lstIDs, 15);

                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("停止示波器滚动中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("读取示波器电流最大值...");
                    Dictionary<int, string> InDCImaxs = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 2);     //采集的是输入交流电流，最大值
                    //Dictionary<int, string> InDCImaxs = new Dictionary<int, string>();
                    //InDCImaxs.Add(testWorkParam.lstIDs.First(), DigitalFilter(2).Values.First().Max().ToString());
                    Thread.Sleep(3000);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope?.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> dErrorValue = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double InDCImax = Convert.ToDouble(InDCImaxs[item]);
                        double errorValue = System.Math.Abs(InDCImax - DemandCurrent);
                        dErrorValue.Add(item, errorValue.ToString("F2"));
                    }

                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }

                    ProcessDataTmp(dic2, "输出冲击电流", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "输出冲击电流", "充电电流(A)", "-", "-");
                    ProcessDataTmp(InDCImaxs, "输出冲击电流", "输入电流最大值(A)", "-", "-");
                    ProcessDataTmp(dErrorValue, "输出冲击电流", "过冲测试结果(A)", "0", ErrorValue.ToString(), dImgs);


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }

        /// <summary>
        /// 示波器启动输出过冲
        /// </summary>
        public void OscilloscopeStartupOutputOvershoot(double DemandCurrent)
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_V", "1M", "V", false, "150", "-2");//通道1设置
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1M", "A", false, "10", "-3");//通道2设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道3设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 2, (DemandCurrent / 2).ToString(), "Single");
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0.1");//设置滚动，时基和触发延时
                System.Threading.Thread.Sleep(100);


                ControlEquipMent.Oscilloscope?.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");//设置测量项为XY
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//初始化测量
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 1);
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 2);
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
