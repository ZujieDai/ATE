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
    /// 美标直流老化测试流程（深圳HY华源专用）
    /// </summary>
    public class CZ_SXHY_AgingTest_DCUA : BusinessBase
    {
        public CZ_SXHY_AgingTest_DCUA(int trialType) { TrialType = trialType; }

        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError;


        public override void InitializeParams()
        {
            Init();
            //
            //老化电压(V)=220|老化电流(A)=10|老化时间(分)=30|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            AgingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            AgingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            AgingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            IntervalTime = Convert.ToDouble(strParams[3].Split('=')[1]);
            VoltError = Convert.ToDouble(strParams[4].Split('=')[1]);
            CurrError = Convert.ToDouble(strParams[5].Split('=')[1]);
        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                byte[] x = new byte[0];
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // SetCPReresh();
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


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                if (!CheckSwipingCard_DC_SinglePart(testWorkParam.lstIDs))
                {
                    return;
                }

                Thread.Sleep(5000);

                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, 200, true, AgingVolt);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, AgingCurr, true, AgingVolt);
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile(string.Format("设置带载电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                SetLoadPara(testWorkParam.lstIDs, AgingVolt - 15, AgingCurr, AgingVolt, AgingCurr);//华源的负载默认降15V即可
                Thread.Sleep(1000);
                SetLoadDCON(testWorkParam.lstIDs);
                Thread.Sleep(1000 * 20);
                Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingVoltage;
                    if (voltage == 0)
                    {
                        Thread.Sleep(500);
                        voltage = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingVoltage;
                    }
                    dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                    double current = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingCurrent;
                    if (current == 0)
                    {
                        Thread.Sleep(500);
                        current = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingCurrent;
                    }
                    dicAgingCurr.Add(itmp, current.ToString("F2"));
                }
                ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", AgingVolt), "1秒时桩实际电压(V)", ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", AgingCurr), "1秒时桩实际电流(V)", ((1.0 - CurrError / 100) * AgingCurr).ToString(), ((1.0 + CurrError / 100) * AgingCurr).ToString());




                Stopwatch sw = new Stopwatch();
                sw.Start();
                int count = 0;
                while (sw.ElapsedMilliseconds / 1000 <= AgingTime * 60)
                {
                    dicAgingVolt.Clear();
                    dicAgingCurr.Clear();
                    Thread.Sleep(Convert.ToInt32(IntervalTime * 1000));
                    count++;
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingVoltage;
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingCurrent;
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_USA_DC_StateData[itmp].ChargingCurrent;
                        }
                        dicAgingCurr.Add(itmp, current.ToString("F2"));
                    }
                    int time = count * Convert.ToInt32(IntervalTime) + 1;
                    ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", AgingVolt), string.Format("{0}秒时桩实际电压(V)", time), ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                    ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", AgingCurr), string.Format("{0}秒时桩实际电流(A)", time), ((1.0 - CurrError / 100) * AgingCurr).ToString(), ((1.0 + CurrError / 100) * AgingCurr).ToString());
                }




            }
        }
        public override void ProcessData()
        {

        }
    }
}
