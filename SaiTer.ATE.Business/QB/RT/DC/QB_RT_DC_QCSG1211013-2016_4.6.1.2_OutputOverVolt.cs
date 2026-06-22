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
    /// 企标研测直流：输出过压保护试验
    /// </summary>
    public class QB_RT_DC_OutputOverVolt : BusinessBase
    {
        double OutputVoltage = 800;
        double BmsDemandVoltage = 220;

        public QB_RT_DC_OutputOverVolt(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //过压参考值(V)=800
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            BmsDemandVoltage = 500;
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
                SetCPReresh();
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
                                //测试时间|输入电压|输出电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //开始检测流程
                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SetConditionValues();

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BmsDemandVoltage, 250, true, BmsDemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BmsDemandVoltage, LstChargerInfo[0].ChargerType, 0);
                //检查刷卡上电
                double Voltage = 0;
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    Voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingVoltage;
                }
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[0] + "号枪处于充电状态，该枪停止检测");
                    return;
                }

                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                }
                ProcessDataTmp(d1, "过压保护前正常充电", "充电电压(V)", "-", "-");

                SendNoticeToUIAndTxtFile("模拟输出过压");
                List<bool> Ks = GetKStatus16_Charging_DC();

                Ks[26] = true;
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, OutputVoltage);
                Thread.Sleep(3000);
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, OutputVoltage, Ks.ToArray());

                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double outputVolt = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                    int timeout = 100;
                    while (timeout-- > 0)
                    {
                        double outputVoltNew = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        if (outputVoltNew > outputVolt)
                            outputVolt = outputVoltNew;
                    }
                    d1.Add(item, outputVolt.ToString("F2"));
                }
                ProcessDataTmp(d1, "模拟过压时", "直流输出电压(V)", "-", "-");

                Thread.Sleep(2000);

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 30, 2);
                ProcessDataConnect("应发出告警提示", "是否有告警提示");


                Thread.Sleep(1000 * 10);
                // 断开过压开关
                List<bool> Ks1 = GetKStatus16_Charging_DC();
                Ks1[26] = false;
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(3000);
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks1.ToArray());

                Thread.Sleep(1000 * 5);

                //采集数据
                var Data_Volt = new Dictionary<int, string>();
                //var Data_Current = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                    int timeout = 20;
                    while (timeout-- > 0)
                    {
                        if (voltage < 30)
                        {
                            break;
                        }
                        voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                        Thread.Sleep(100);
                    }
                    Data_Volt.Add(testWorkParam.lstIDs[i], voltage.ToString());
                }
                ProcessDataTmp(Data_Volt, "保护后桩应停止充电", "保护后桩输出电压(V)", "0", "50");
                //这里不能采集电压，因为我们自己会有模拟的过压电压，所以只需要判断充电状态即可。
                string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState;
                if (chargerState != "充电中")
                {
                    ProcessDataResult(testWorkParam.lstIDs, "是", "是否切断直流输出", true, "保护后桩应停止充电");
                    //Thread.Sleep(1000);
                    //ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", true, "保护后桩应停止充电");
                }
                else
                {
                    ProcessDataResult(testWorkParam.lstIDs, "否", "是否切断直流输出", false, "保护后桩应停止充电");
                    //Thread.Sleep(1000);
                    //ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", false, "保护后桩应停止充电");
                }
                //ProcessDataTmp(Data_Current, "保护后桩应停止充电", "充电电流(A)", "0", "5");

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 30, 2);
                ProcessDataConnect("保护后桩应发出告警", "是否有告警提示");
            }
        }


        public override void ProcessData()
        {

        }
    }
}
