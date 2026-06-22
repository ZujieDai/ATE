using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 漏电流保护测试（程控板控制电阻，判断状态即可，目前TB在用）
    /// </summary>
    public class LeakageCurrentTest_TB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电
        public LeakageCurrentTest_TB(int type) { TrialType = type; }
        int index1 = 0;//需要控制的继电器索引号
        int index2 = 0;//需要控制的继电器索引号
        //double StandbyPowerMax = 5;

        public override void InitializeParams()
        {
            //数据库参数格式
            //需要控制的继电器=7,8

            Init();
            try
            {
                //桩类型（1为刷卡桩，否则为0）=0|待机功耗上限(W)=5
                string[] strParams = TrialItem.ItemParams.Split('|');
                string[] relayIndex = strParams[0].Split('=')[1].Split(',');
                string[] strParams1 = TrialItem.ResultParams.Split('|');
                if (strParams1.Length > 0 && strParams1[0].Split('=').Length > 1)
                {
                    IsCardCharg = double.Parse(strParams1[0].Split('=')[1]) == 1;
                    //StandbyPowerMax = double.Parse(strParams1[1].Split('=')[1]);
                }
                index1 = Convert.ToInt32(double.Parse(relayIndex[0])) - 1;
                if (relayIndex.Length == 2)
                {
                    index2 = Convert.ToInt32(double.Parse(relayIndex[1])) - 1;
                }
                else
                {
                    index2 = index1;
                }
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile("请正确配置继电器参数");
                SendException(ex);
            }
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

                    SetConditionValues();

                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    WaitACVoltage(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 20);

                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 2);
                    

                    SendNoticeToUIAndTxtFile("模拟漏电流输出");
                    List<bool> lstRelay = new List<bool>();
                    for (int i = 0; i < 16; i++)
                    {
                        lstRelay.Add(false);
                    }
                    lstRelay[0] = true;
                    lstRelay[index1] = true;
                    lstRelay[index2] = true;
                    SendNoticeToUIAndTxtFile("控制K" + (index1 + 1).ToString() + "、" + (index2 + 1).ToString() + "闭合");

                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                    Thread.Sleep(200);
                    //防止下发失败
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);

                    Thread.Sleep(1000);

                    if (TrialType == (int)EmTrialType.漏电保护测试_程控板)
                    {
                        //断线后的电压
                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            int timeout = 100;
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
                            if (!Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                                Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        }

                        ProcessDataTmp(Data_Tmp, "漏电", "输出电压(V)", "0", "30");


                        #region 采集总是0，先去掉
                        //Dictionary<int, double> Power_Tmp = new Dictionary<int, double>();
                        //Power_Tmp = ControlEquipMent.ElectricMeter.EM_GetTotalPower(testWorkParam.lstIDs);
                        //Data_Tmp.Clear();
                        //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        //{
                        //    Data_Tmp.Add(testWorkParam.lstIDs[i], Power_Tmp[testWorkParam.lstIDs[i]].ToString());
                        //}
                        //ProcessDataTmp(Data_Tmp, "漏电", "输入功率(W)", "0", StandbyPowerMax.ToString());
                        #endregion
                    }
                    else if (TrialType == (int)EmTrialType.不动作漏电测试_程控板)
                    {
                        //断线后的电压
                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            int timeout = 100;
                            double voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            while (timeout-- > 0)
                            {
                                if (voltage >= 80)
                                {
                                    Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                                    break;
                                }
                                Thread.Sleep(100);
                                voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            }
                        }

                        ProcessDataTmp(Data_Tmp, "漏电", "输出电压(V)", "80", "-");
                        //Thread.Sleep(2000);
                        //Data_Tmp.Clear();
                        //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        //{
                        //    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Current.ToString("F2"));
                        //}
                        //ProcessDataTmp(Data_Tmp, "不动作漏电测试", "漏电后输出电流(A)", "1.8", "2.2");
                          

                    }


                    //恢复状态
                    SendNoticeToUIAndTxtFile("恢复正常状态");
                    lstRelay.Clear();
                    for (int i = 0; i < 16; i++)
                    {
                        lstRelay.Add(false);
                    }
                    lstRelay[0] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }



        public override void ProcessData()
        {

        }



    }
}
