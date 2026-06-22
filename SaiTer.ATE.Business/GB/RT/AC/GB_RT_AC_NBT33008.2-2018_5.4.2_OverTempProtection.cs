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
    /// 国标交流研测：过温保护
    /// </summary>
    public class GB_RT_AC_OverTempProtection : BusinessBase
    {
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double OutputCurrent = 0;

        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储

        public GB_RT_AC_OverTempProtection(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            OutputCurrent = ResiLoadCurrent / 2;
        }

        public override void InitEquiMent()
        {
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

                //SetCPReresh();

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

        /// <summary>
        /// 测试流程
        /// </summary>
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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                //设置测试条件
                SetConditionValues();

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                int waitTime = 50;
                //需要计算输出电流的设定值，让示波器的输出电流保持在三格的范围
                string OutputCurrentScale = "50";
                if (OutputCurrent * 1.4 * 2 < 30)
                    OutputCurrentScale = "10";
                else if (OutputCurrent * 1.4 * 2 < 60)
                    OutputCurrentScale = "20";
                else if (OutputCurrent * 1.4 * 2 < 120)
                    OutputCurrentScale = "40";

                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "500", "0.3");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_Current", "50", "A", false, OutputCurrentScale, "2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "-1.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(waitTime);

                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "");
                Thread.Sleep(waitTime);
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                Thread.Sleep(waitTime);

                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "10", 1, "60", "Auto");//超时触发
                Thread.Sleep(waitTime);
                //设置时基400ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//低值
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(waitTime);

                SendNoticeToUIAndTxtFile("设备开启负载中，请稍候...");
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, BMSDemandVolt, OutputCurrent);
                WaitACCurrent(testWorkParam.lstIDs, OutputCurrent);
                Thread.Sleep(1000 * 5);
                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                d3 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                    int timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (volt < 50)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d1.Add(item, volt.ToString("F2"));
                    double current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (current < OutputCurrent * 0.8 || current > OutputCurrent * 1.2)
                        {
                            current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d2.Add(item, current.ToString("F2"));
                    double data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (data < 3 || data > 97)
                        {
                            data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d3.Add(item, data.ToString("F2"));
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(d1, "过温前充电状态", "充电电压(V)", "-", "-", dImgs);
                ProcessDataTmp(d2, "过温前充电状态", "充电电流(A)", "-", "-");
                ProcessDataTmp(d3, "过温前充电状态", "PWM占空比(%)", "-", "-");

                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "Alternating", "DC", "EDGE", "100", 1, "60", "Single");//超时触发
                Thread.Sleep(waitTime);

                CountDownTimeInfo("请人工模拟内部温度超过过温保护值", 999, 0);
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(3 * 1000);

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                d3 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                    int timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (volt < 50)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d1.Add(item, volt.ToString("F2"));
                    double current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (current < OutputCurrent * 0.8 || current > OutputCurrent * 1.2)
                        {
                            current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d2.Add(item, current.ToString("F2"));
                    double data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (data < 3 || data > 97)
                        {
                            data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d3.Add(item, data.ToString("F2"));
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(d1, "过温保护状态", "充电电压(V)", "-", "-", dImgs);
                ProcessDataTmp(d2, "过温保护状态", "充电电流(A)", "-", "-");
                ProcessDataTmp(d3, "过温保护状态", "PWM占空比(%)", "-", "-");
                CountDownTimeInfo("请检查充电桩应降低PWM占空比或切断交流供电回路，并发出告警提示。\r\n（勾选枪号则为Pass）", 999, 2);
                ProcessData();
                ProcessDataConnect("应发出告警提示", "是否有告警提示");
                CountDownTimeInfo("请人工恢复充电机内部温度到正常范围内。", 999, 0);

                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 2, "10", "Single");//上升沿触发
                Thread.Sleep(waitTime);

                if(AllEquipStateData.DicBMS_AC_StateData.First().Value.PhaseA_Voltage < 30)
                {
                    SetCPReresh();
                }

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                SendNoticeToUIAndTxtFile("设备开启负载中，请稍候...");
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent / 2);
                WaitACCurrent(testWorkParam.lstIDs, OutputCurrent);
                Thread.Sleep(1000 * 5);

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                d3 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                    int timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (volt < 50)
                        {
                            volt = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d1.Add(item, volt.ToString("F2"));
                    double current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (current < OutputCurrent * 0.8 || current > OutputCurrent * 1.2)
                        {
                            current = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d2.Add(item, current.ToString("F2"));
                    double data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                    timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (data < 3 || data > 97)
                        {
                            data = AllEquipStateData.DicBMS_AC_StateData[item].CPDutyCycle;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d3.Add(item, data.ToString("F2"));
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(d1, "过温恢复状态", "充电电压(V)", "-", "-", dImgs);
                ProcessDataTmp(d2, "过温恢复状态", "充电电流(A)", "-", "-");
                ProcessDataTmp(d3, "过温恢复状态", "PWM占空比(%)", "-", "-");

                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
        }

        public override void ProcessData()
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
                LstTrialData[k].ItemName = iIndex.ToString();


                //double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                //if (voltage >= 0 && voltage <= 20)
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = $"过温保护状态|是否降低PWM占空比或切断交流供电回路|-|-|是|报表(勿删)";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = $"过温保护状态|是否降低PWM占空比或切断交流供电回路|-|-|否|报表(勿删)";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
                iIndex++;
            }
        }
    }
}
