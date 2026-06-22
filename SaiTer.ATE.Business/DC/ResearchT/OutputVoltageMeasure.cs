 using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电压测量误差
    /// </summary>
    public class OutputVoltageMeasure : BusinessBase
    {
        string ItemFlow;
        int trlTimeOut_S = 30;
        double Norm = 5;
        int CheckTime = 1;
        int TestTime = 1;

        public OutputVoltageMeasure(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // 判定准则(±V)=5|检测时间(S)=1|测试时间(S)=1
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 3)
            {
                Norm = double.Parse(strParams[0].Split('=')[1]);
                CheckTime = Convert.ToInt32(double.Parse(strParams[1].Split('=')[1]));
                TestTime = Convert.ToInt32(double.Parse(strParams[2].Split('=')[1]));
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            // 模拟插拔枪
            SetCPReresh();
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
                    //设置测试条件
                    SetConditionValues();

                    double DMaxAllowChargeVoltage = MaxAllowChargeVoltage;
                    //SetLoadDCOFF(testWorkParam.lstIDs);
                    double[] Voltage = new double[3];
                    double Current = RatedCurrent * 0.5;
                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = MidAllowChargeVoltage;
                    Voltage[2] = MaxAllowChargeVoltage;

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, MinAllowChargeVoltage, Current + 20))
                    {
                        return;
                    }
                    Thread.Sleep(2000);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MinAllowChargeVoltage, Current + 20, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);
                    //WaitDCVoltage(testWorkParam.lstIDs, MaxAllowChargeVoltage);

                    for (int i = 0; i < 3; i++)
                    {
                        if (IsRLoad(Voltage[i], Current))
                        {
                            //d1 = new Dictionary<int, string>();
                            //d2 = new Dictionary<int, string>();
                            //foreach (int item in testWorkParam.lstIDs)
                            //{
                            //    d1.Add(item, Voltage[i].ToString("F2"));
                            //    d2.Add(item, Current.ToString("F2"));
                            //}
                            //ProcessDataTmp(d1, "充电设置", "需求电压(V)", "-", "-");
                            //ProcessDataTmp(d2, "充电设置", "需求电流(A)", "-", "-");

                            SetLoadDCOFF(testWorkParam.lstIDs);
                            System.Threading.Thread.Sleep(2000);
                            int index = i + 1;

                            d1 = new Dictionary<int, string>();
                            d2 = new Dictionary<int, string>();
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current + 20, true, 390);
                            WaitDCVoltage(testWorkParam.lstIDs, Voltage[i]);
                            Thread.Sleep(3 * 1000);

                            double CheckVoltage = Voltage[i];
                            int timeout2 = 50;
                            double ChargingVolt = 0;
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                timeout2 = 50;
                                while (timeout2-- > 0)
                                {
                                    ChargingVolt = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                                    if (ChargingVolt >= Voltage[i] - Norm && ChargingVolt <= Voltage[i] + Norm)
                                    {
                                        break;
                                    }
                                    System.Threading.Thread.Sleep(100);
                                }
                                d1.Add(item, ChargingVolt.ToString("F2"));
                            }

                            SendNoticeToUIAndTxtFile("开启负载中...");
                            SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 20, Current, Voltage[i], Current);
                            SetLoadDCON(testWorkParam.lstIDs);
                            SendNoticeToUIAndTxtFile("等待输出电流稳定...");
                            WaitDCCurrentWithTime(testWorkParam.lstIDs, Current, 35);
                            Thread.Sleep(3 * 1000);

                            d3 = new Dictionary<int, string>();
                            foreach(int item in testWorkParam.lstIDs)
                            {
                                double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                                d3.Add(item, current.ToString("F2"));
                            }
                            ProcessDataTmp(d3, "充电中（恒压）", "实际输出电流(A)", "-", "-");

                            int timeout = 50;
                            double Voltage_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                            while (timeout-- > 0)
                            {
                                Voltage_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSVolt;
                                if (Voltage_Analyzer >= Voltage[i] - Norm && Voltage_Analyzer <= Voltage[i] + Norm)
                                {
                                    break;
                                }
                                System.Threading.Thread.Sleep(100);
                            }
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d2.Add(item, Voltage_Analyzer.ToString("F2"));
                            }
                            ProcessDataTmp(d2, "充电中（恒压）", "实际输出电压(V)", (Voltage[i] - Norm).ToString("F2"), (Voltage[i] + Norm).ToString("F2"));
                            ProcessDataTmp(d1, "充电中（恒压）", "报文中输出电压(V)", (Voltage[i] - Norm).ToString("F2"), (Voltage[i] + Norm).ToString("F2"));

                            if (TestTime != 0)
                            {
                                System.Threading.Thread.Sleep(TestTime * 1000);
                            }
                            if (i == 1)
                            {
                                System.Threading.Thread.Sleep(6000);
                            }

                            var dicResult = new Dictionary<int, string>();
                            //输出电压测量误差不应超过±5 V
                            double errorVolt = ChargingVolt - Voltage_Analyzer;
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                dicResult.Add(item, errorVolt.ToString("F2"));
                            }
                            ProcessDataTmp(dicResult, "充电中（恒压）", "输出电压测量误差(V)", (- Norm).ToString(), Norm.ToString());
                        }
                    }

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
       
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
