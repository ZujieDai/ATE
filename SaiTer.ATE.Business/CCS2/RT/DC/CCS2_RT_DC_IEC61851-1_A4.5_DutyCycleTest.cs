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
    /// 欧标直流研测：占空比测试
    /// </summary>
    public class CCS2_RT_DC_DutyCycleTest : BusinessBase
    {
        double PwmMax = 96;
        double PwmMin = 10;
        Dictionary<int, string> DicPwm = new Dictionary<int, string>();
        int TrialFlowNum = 1;

        public CCS2_RT_DC_DutyCycleTest(int trialType) { TrialType = trialType; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //占空比下限(%)=10|占空比上限(%)=96
            string[] strParams = TrialItem.ResultParams.Split('|');
            PwmMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            PwmMin = Convert.ToDouble(strParams[0].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt < 50)
                    {
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    }
                }
                if (testWorkParam.lstIDs.Count > 0)
                {
                    SendNoticeToUIAndTxtFile("关闭示波器1、2、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    SendNoticeToUIAndTxtFile("开启示波器3号通道，并设置相应参数");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");//时基
                    Thread.Sleep(100);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        SendNoticeToUIAndTxtFile("所有枪位都未检测到插枪状态，测试结束！");
                        return;
                    }

                    double DemandVolt = MaxOutputPower * 1000 / MaxAllowChargeCurrent;
                    if (DemandVolt > MaxAllowChargeVoltage) DemandVolt = MaxAllowChargeVoltage;
                    if(DemandVolt < MinAllowChargeVoltage) DemandVolt = MinAllowChargeVoltage;
                    DemandVolt = Math.Round(DemandVolt);

                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVolt, MaxAllowChargeCurrent))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("设置示波器3号通道测量项为【CP占空比】");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);

                    Thread.Sleep(200);
                    SetLoadPara(testWorkParam.lstIDs, DemandVolt - 20, MaxAllowChargeCurrent + 10, DemandVolt - 5, MaxAllowChargeCurrent);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, MaxAllowChargeCurrent);
                    Thread.Sleep(2000);

                    DicPwm = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    Thread.Sleep(100);
                    SendNoticeToUIAndTxtFile("正在保存示波器截屏");
                    var dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(DicPwm, "最大电流测试", "CP占空比", "5", "96", dicPath);

                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage.ToString());
                    }
                    ProcessDataTmp(d1, "最大电流测试", "CP电压(V)", "5.47", "6.53");
                }
            }
        }
        public override void ProcessData()
        {
        }
    }
}
