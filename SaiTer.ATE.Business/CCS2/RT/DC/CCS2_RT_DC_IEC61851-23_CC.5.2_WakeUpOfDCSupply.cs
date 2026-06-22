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
    /// 欧标研测直流：电动汽车的直流电源唤醒
    /// </summary>
    public class CCS2_RT_DC_WakeUpOfDCSupply : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public CCS2_RT_DC_WakeUpOfDCSupply(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            //string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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

        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();

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
                ////是否全部有结论
                //if (testWorkParam.lstIDs.Count <= 0) break;
                ////是否超时
                //if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
                //{
                //    for (int i = 0; i < LstTrialData.Count; i++)
                //    {
                //        if (LstTrialData[i].IsCheck)
                //        {
                //            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                //            {
                //                LstTrialData[i].TrialResult = EmTrialResult.Fail;
                //                LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                //                int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                //                LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                //                //界面展示的数据项格式
                //                //
                //                LstTrialData[i].ExtentData = "-|-|-|-|null";
                //                SendTrialDataToUI(LstTrialData[i]);
                //            }
                //        }
                //    }
                //    break;
                //}

                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                //添加测试条件
                SetConditionValues();


                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, BMSDemandVolt, 250);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, true, BMSDemandVolt);
                Thread.Sleep(200);
                //提示刷卡并等待两分钟再闭合开关S
                int timeout = 120;
                //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                MessgaeInfo(true, "请刷卡后，等待两分钟!", true);
                while (timeout-- > 0)
                {
                    if (timeout % 10 == 0 && timeout != 0)
                    {
                        SendNoticeToUIAndTxtFile($"剩余等待时间：{timeout}秒");
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                MessgaeInfo(false, "");

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSDemandVolt);
                Thread.Sleep(2000);

                timeout = 50;
                foreach (int item in testWorkParam.lstIDs)
                {
                    double Voltage_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                    while (timeout-- > 0)
                    {
                        if (Voltage_Analyzer >= MaxAllowChargeVoltage * 0.9 && Voltage_Analyzer <= MaxAllowChargeVoltage * 1.02)
                        {
                            break;
                        }
                        Voltage_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                        System.Threading.Thread.Sleep(100);
                    }
                    Data_Tmp.Add(item, Voltage_Analyzer.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, "电动汽车的直流电源唤醒", "充电电压(V)", (MaxAllowChargeVoltage * 0.95).ToString("F2"), (MaxAllowChargeVoltage * 1.05).ToString("F2"));


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
