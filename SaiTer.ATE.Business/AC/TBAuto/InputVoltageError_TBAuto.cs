using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输入电压异常检测（包含输入过压、输入欠压）
    /// </summary>
    public class InputVoltageError_TBAuto : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double InputVoltage = 220;
        double NormalVoltage = 220;
        double WaitTime = 20;
        double RecoveryTime = 20;   //等待恢复时间(s)
        double ProtectVoltMin = 0, ProtectVoltMax = 20, RecoveryVoltMin = 220, RecoveryVoltMax = 260;//保护后输出电压下限(V)=0|保护后输出电压上限(V)=20|恢复后输出电压下限(V)=230|恢复后输出电压上限(V)=260

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public InputVoltageError_TBAuto(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //过压值(V)=230.00|恢复值(V)=220.00|桩类型（1为刷卡桩，否则为0）=0.00|保护后输出电压下限(V)=0|保护后输出电压上限(V)=20|恢复后输出电压下限(V)=230|恢复后输出电压上限(V)=260
            //欠压值(V)=180.00|恢复值(V)=220.00|桩类型（1为刷卡桩，否则为0）=0.00|保护后输出电压下限(V)=0|保护后输出电压上限(V)=20|恢复后输出电压下限(V)=180|恢复后输出电压上限(V)=220
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            NormalVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
            IsCardCharg = double.Parse(strParams[2].Split('=')[1]) == 1;
            if (strParams.Length > 3)
            {
                //RecoveryTime = Convert.ToDouble(strParams[3].Split('=')[1]);
                ProtectVoltMin = Convert.ToDouble(strParams[3].Split('=')[1]);
                ProtectVoltMax = Convert.ToDouble(strParams[4].Split('=')[1]);
                RecoveryVoltMin = Convert.ToDouble(strParams[5].Split('=')[1]);
                RecoveryVoltMax = Convert.ToDouble(strParams[6].Split('=')[1]);
            }
            // 惠州TB 260V也可以恢复充电
            //if (NormalVoltage > 220)
            //{
            //    NormalVoltage = 220;
            //}
            VoltageRate = Convert.ToDouble(TrialItem.ItemParams.Split('|')[0].Split('=')[1]);
            if (InputVoltage / NormalVoltage >= VoltageRate)
            {
                InputVoltage = NormalVoltage * VoltageRate;
            }
            //if (strParams.Length > 2)
            //{
            //    //等待断电时间(s)
            //    WaitTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            //}
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
                //SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + NormalVoltage + "|" + InputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }


                if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 || AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                {
                    //模拟CP恢复，S2闭合
                    var Ks = GetKStatus16_Charging();
                    ControlEquipMent.BMS.BMS_SetKState(this.lstIDs, Ks);
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    WaitSwipingCard(testWorkParam.lstIDs, 0);
                }

                ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, InputVoltage);
                SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                Thread.Sleep(1000);
                ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, InputVoltage);

                SetConditionValues();

                //采集数据
                Data_Tmp = new Dictionary<int, string>();
                Dictionary<int, string> dd = new Dictionary<int, string>();
                int count = 0;
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double outputVoltage = 0;
                    double inputVoltage = 0;
                    for (int j = 0; j < WaitTime * 5; j++)
                    {
                        outputVoltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        inputVoltage = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        if (dd.ContainsKey(testWorkParam.lstIDs[i]))
                        {
                            dd[testWorkParam.lstIDs[i]] = inputVoltage.ToString("F2");
                        }
                        else
                        {
                            dd.Add(testWorkParam.lstIDs[i], inputVoltage.ToString("F2"));
                        }
                        if (inputVoltage > InputVoltage * 0.8 && inputVoltage < InputVoltage * 1.1)
                        {
                            if (outputVoltage < ProtectVoltMax)
                            {
                                count++;
                                if (count >= 2)
                                {
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(200);
                    }
                    //voltage = 0;

                    Data_Tmp.Add(testWorkParam.lstIDs[i], outputVoltage.ToString("F2"));
                }
                if (TrialType == (int)EmTrialType.输入过压保护及恢复测试 || TrialType == (int)EmTrialType.输入过压保护测试)
                {
                    ProcessDataTmp(dd, "输入过压", "输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入过压", "输出电压(V)", ProtectVoltMin.ToString(), ProtectVoltMax.ToString());
                }
                else
                {
                    ProcessDataTmp(dd, "输入欠压", "输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入欠压", "输出电压(V)", ProtectVoltMin.ToString(), ProtectVoltMax.ToString());
                }
                SendNoticeToUIAndTxtFile("恢复正常电压：" + NormalVoltage + "V，等待交流源输出稳定。");
                ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, NormalVoltage);
                Thread.Sleep(1000);
                ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, NormalVoltage);
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                    CheckSwipingCard(testWorkParam.lstIDs);

                dd.Clear();
                //这个是有恢复测试的
                if (TrialType == (int)EmTrialType.输入过压保护及恢复测试
                    || TrialType == (int)EmTrialType.输入欠压保护及恢复测试)
                {

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        double inputVoltage = 0;
                        for (int j = 0; j < RecoveryTime * 5; j++)
                        {
                            inputVoltage = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                            if (dd.ContainsKey(testWorkParam.lstIDs[i]))
                            {
                                dd[testWorkParam.lstIDs[i]] = inputVoltage.ToString("F2");
                            }
                            else
                            {
                                dd.Add(testWorkParam.lstIDs[i], inputVoltage.ToString("F2"));
                            }
                            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            if (voltage < 80)
                            {
                                Thread.Sleep(200);
                            }
                            else
                            {
                                Thread.Sleep(200);
                                voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                                if (inputVoltage > NormalVoltage * 0.9 && inputVoltage < NormalVoltage * 1.05)
                                    if (voltage > RecoveryVoltMin && voltage < RecoveryVoltMax)
                                        break;
                            }
                        }

                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                    }
                    if (TrialType == (int)EmTrialType.输入过压保护及恢复测试)
                    {
                        ProcessDataTmp(dd, "过压恢复", "输入电压(V)", "-", "-");
                        ProcessDataTmp(Data_Tmp, "过压恢复", "输出电压(V)", RecoveryVoltMin.ToString("F2"), RecoveryVoltMax.ToString("F2"));
                    }
                    else
                    {
                        ProcessDataTmp(dd, "欠压恢复", "输入电压(V)", "-", "-");
                        ProcessDataTmp(Data_Tmp, "欠压恢复", "输出电压(V)", RecoveryVoltMin.ToString("F2"), RecoveryVoltMax.ToString("F2"));
                    }
                }
                ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, LstChargerInfo[0].NominalVoltage);
            }
        }


        public override void ProcessData()
        {

        }
    }
}
