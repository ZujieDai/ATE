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
    /// 欧标研测直流：测量电流和电压
    /// </summary>
    public class CCS2_RT_DC_MeasuringCurrentAndVoltage : BusinessBase
    {
        int trlTimeOut_S = 30;
        double Norm = 5;

        public CCS2_RT_DC_MeasuringCurrentAndVoltage(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            //判定准则(±V)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1)
            {
                Norm = double.Parse(strParams[0].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {

        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
                // 模拟插拔枪
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
                    Voltage[0] = MinAllowChargeVoltage;
                    Voltage[1] = MidAllowChargeVoltage;
                    Voltage[2] = MaxAllowChargeVoltage;
                    double[] Current = new double[2];
                    Current[0] = 50 < RatedCurrent * 0.3 ? 50 : RatedCurrent * 0.3;
                    Current[1] = RatedCurrent * 0.5;

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (IsRLoad(Voltage[i], Current[j]))
                            {
                                d1 = new Dictionary<int, string>();
                                d2 = new Dictionary<int, string>();
                                d3 = new Dictionary<int, string>();

                                ControlEquipMent.BMS.SetParameter(lstIDs, Voltage[i], Current[j] + 10, true, Voltage[i]);
                                WaitDCVoltage_EU_DC(testWorkParam.lstIDs, Voltage[i]);
                                Thread.Sleep(5 * 1000);

                                //d1 = new Dictionary<int, string>();
                                //d2 = new Dictionary<int, string>();
                                //foreach (int item in testWorkParam.lstIDs)
                                //{
                                //    d1.Add(item, Voltage[i].ToString("F2"));
                                //    d2.Add(item, Current.ToString("F2"));
                                //}
                                //ProcessDataTmp(d1, "充电设置", "需求电压(V)", "-", "-");
                                //ProcessDataTmp(d2, "充电设置", "需求电流(A)", "-", "-");

                                SendNoticeToUIAndTxtFile("开启负载中...");
                                SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 20, Current[j], Voltage[i], Current[j]);
                                SetLoadDCON(testWorkParam.lstIDs);
                                SendNoticeToUIAndTxtFile("等待输出电流稳定...");
                                WaitDCCurrent_EU_DC(testWorkParam.lstIDs, Current[j]);
                                Thread.Sleep(5 * 1000);

                                double CheckVoltage = Voltage[i];
                                int timeout2 = 50;
                                foreach (int item in testWorkParam.lstIDs)
                                {
                                    timeout2 = 50;
                                    double ChargingVolt = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                    while (timeout2-- > 0)
                                    {
                                        if (ChargingVolt >= Voltage[i] - Norm && ChargingVolt <= Voltage[i] + Norm)
                                        {
                                            break;
                                        }
                                        System.Threading.Thread.Sleep(300);
                                        ChargingVolt = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                    }
                                    d1.Add(item, ChargingVolt.ToString("F2"));
                                }
                                ProcessDataTmp(d1, $"需求电压{Voltage[i]}V，需求电流{Current[j]}A", "输出电压(V)", (Voltage[i] - Norm).ToString("F2"), (Voltage[i] + Norm).ToString("F2"));

                                d3 = new Dictionary<int, string>();
                                foreach (int item in testWorkParam.lstIDs)
                                {
                                    timeout2 = 50;
                                    double current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                    while (timeout2-- > 0)
                                    {
                                        if(Current[j] < 50)
                                        {
                                            if (current >= Current[j] - 1 && current <= Current[j] + 1)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (current >= Current[j] * 0.98 && current <= Current[j] * 1.02)
                                            {
                                                break;
                                            }
                                        }
                                        System.Threading.Thread.Sleep(300);
                                        current = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                    }
                                    d3.Add(item, current.ToString("F2"));
                                }
                                if (Current[j] < 50)
                                    ProcessDataTmp(d3, $"需求电压{Voltage[i]}V，需求电流{Current[j]}A", "输出电流(A)", (Current[j] - 1).ToString(), (Current[j] + 1).ToString());
                                else
                                    ProcessDataTmp(d3, $"需求电压{Voltage[i]}V，需求电流{Current[j]}A", "输出电流(A)", (Current[j] * 0.98).ToString(), (Current[j] * 1.02).ToString());


                                SetLoadDCOFF(testWorkParam.lstIDs);
                                System.Threading.Thread.Sleep(2000);
                            }
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
