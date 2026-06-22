using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 正常充电结束测试
    /// </summary>
    public class NormalEndCharge_TBAuto : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double CPDutyMin, CPDutyMax, CPFreqError, CPVoltError;

        double CPPWMUpMax = 10;
        double CPPWMDownMax = 13;

        public NormalEndCharge_TBAuto(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            //占空比下限(%)=2.00|占空比上限(%)=96.00|频率误差(Hz)=30|正向电平误差(V)=0.6|上升时间上限(μs)=10|下降时间上限(μs)=13|桩类型（1为刷卡桩，否则为0）=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
            CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            CPFreqError = Convert.ToDouble(strParams[2].Split('=')[1]);
            CPVoltError = Convert.ToDouble(strParams[3].Split('=')[1]);
            CPPWMUpMax = Convert.ToDouble(strParams[4].Split('=')[1]);
            CPPWMDownMax = Convert.ToDouble(strParams[5].Split('=')[1]);
            IsCardCharg = double.Parse(strParams[6].Split('=')[1]) == 1;
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

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

                if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                {
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                }
                SetConditionValues();

                //是否为三相充电桩
                bool isSanX = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 20
                    && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseB_Voltage > 20
                    && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseC_Voltage > 20;

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                {
                    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                    int i = 500;
                    while (i-- > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                        {
                            //双重判断
                            Thread.Sleep(100);
                            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                                break;
                        }
                        Thread.Sleep(100);
                    }
                }

                //Thread.Sleep(2000);
                int timeOut = 30;
                while (timeOut > 0)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle == 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage < 20)
                    {
                        break;
                    }
                    Thread.Sleep(300);
                    timeOut--;
                }

                int timeout = 10;
                SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                //示波器采集不准
                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                    timeout = 10;
                    while (timeout-- > 0)
                    {
                        if (cpVolt >= 9 - CPVoltError && cpVolt <= 9 + CPVoltError)
                            break;
                        Thread.Sleep(200);
                        cpVolt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                    }
                    Data_Tmp.Add(testWorkParam.lstIDs[i], cpVolt.ToString());
                }
                ProcessDataTmp(Data_Tmp, "充电结束", "CP正电压(V)", (9 - CPVoltError).ToString(), (9 + CPVoltError).ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > -11.2 || Convert.ToDouble(Data_Tmp.First().Value) < -12.8)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);
                    }
                    else
                        break;
                }
                if (Convert.ToDouble(Data_Tmp.First().Value) < -10.8 && Convert.ToDouble(Data_Tmp.First().Value) > -11.2)
                {
                    foreach (var item in Data_Tmp.Keys)
                    {
                        Data_Tmp[item] = (Convert.ToDouble(Data_Tmp[item]) - 0.3).ToString();
                    }
                }
                ProcessDataTmp(Data_Tmp, "充电结束", "CP负电压(V)", "-12.8", "-11.2");

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > 1000 + CPFreqError || Convert.ToDouble(Data_Tmp.First().Value) < 1000 - CPFreqError)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "充电结束", "CP频率(Hz)", (1000 - CPFreqError).ToString(), (1000 + CPFreqError).ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) > CPDutyMax || Convert.ToDouble(Data_Tmp.First().Value) < CPDutyMin)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    }
                    else
                        break;
                }
                ProcessDataTmp(Data_Tmp, "充电结束", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                //CPPWMUpMax = 7;
                Dictionary<int, string> dd = new Dictionary<int, string>();
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMUpMax)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);
                    }
                    else
                        break;
                }
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "充电结束", "CP上升时间(us)", "0", CPPWMUpMax.ToString());

                //CPPWMDownMax = 13;
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                timeout = 50;
                while (timeout-- > 0)
                {
                    if (Convert.ToDouble(Data_Tmp.First().Value) * 1000000 > CPPWMDownMax)
                    {
                        Thread.Sleep(100);
                        Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);
                    }
                    else
                        break;
                }
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "充电结束", "CP下降时间(us)", "0", CPPWMDownMax.ToString());
                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    timeout = 100;
                    double voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    while (timeout-- > 0)
                    {
                        if (voltage >= 0 && voltage <= 30)
                        {
                            Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                            break;
                        }
                        Thread.Sleep(100);
                        voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    }
                    // 导引的读不准通过电表读取
                    if (voltage < 0 || voltage > 30)
                    {
                        Data_Tmp[testWorkParam.lstIDs[i]] = ControlEquipMent.ElectricMeter.EM_GetVolt(testWorkParam.lstIDs).First().Value.First().ToString("F2");
                    }
                }
                ProcessDataTmp(Data_Tmp, "充电结束", "输出电压(V)", "0", "30");

                //此时充电已经停止，可以设置单三相切换（TB的交流源都是要用DIO继电器控制单三相输出）
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1500);
                if (isSanX)
                {
                    ControlEquipMent.ControlBoard.SetRelaySwitch(2, true);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                    Thread.Sleep(300);
                }
                else
                {
                    ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(3, true);
                    Thread.Sleep(300);
                }
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                Thread.Sleep(1000);
            }
        }
        public override void ProcessData()
        {
           
        }
    }
}
