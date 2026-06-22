using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电流测量误差
    /// </summary>
    public class OutputCurrentMeasure : BusinessBase
    {
        string ItemFlow;
        int trlTimeOut_S = 30;
        double Norm = 1.5;
        int CheckTime = 1;
        int TestTime = 1;

        public OutputCurrentMeasure(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // 判定准则(±%)=1.5|检测时间(S)=1|测试时间(S)=1
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

                    double[] Current = new double[3];
                    Current[0] = RatedCurrent * 0.2;
                    Current[1] = RatedCurrent * 0.5;
                    Current[2] = MidAllowChargeCurrent;

                    for (int i = 0; i < Current.Length; i++)
                    {
                        if (Current[i] > MaxAllowChargeCurrent)
                            Current[i] = MaxAllowChargeCurrent;
                    }
                    //SetLoadDCOFF(testWorkParam.lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();
                    for (int i = 0; i < 3; i++)
                    {
                        if (IsRLoad(MidAllowChargeVoltage, Current[i]))
                        {
                            d1 = new Dictionary<int, string>();
                            d2 = new Dictionary<int, string>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, MidAllowChargeVoltage.ToString("F2"));
                                d2.Add(item, Current[i].ToString("F2"));
                            }
                            ProcessDataTmp(d1, "充电设置", "需求电压(V)", "-", "-");
                            ProcessDataTmp(d2, "充电设置", "需求电流(A)", "-", "-");


                            SetLoadDCOFF(testWorkParam.lstIDs);
                            SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MidAllowChargeVoltage, Current[i], false, 390);
                            //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                            //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, MidAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);
                            WaitDCVoltage(testWorkParam.lstIDs, MidAllowChargeVoltage);
                            Thread.Sleep(500);


                            SendNoticeToUIAndTxtFile("开启负载中...");
                            SetLoadPara(testWorkParam.lstIDs, MidAllowChargeVoltage - 20, Current[i], MidAllowChargeVoltage, Current[i]);
                            SetLoadDCON(testWorkParam.lstIDs);
                            WaitDCCurrentWithTime(testWorkParam.lstIDs, Current[i], 35);
                            Thread.Sleep(3 * 1000);

                            double Current_CCS = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingVoltage;
                            d1 = new Dictionary<int, string>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                int timeout = 20;
                                while (timeout-- > 0)
                                {
                                    if (Current_CCS < Current[i] * (1 - Norm / 100) || Current_CCS > Current[i] * (1 + Norm / 100))
                                    {
                                        Current_CCS = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                                        Thread.Sleep(100);
                                        continue;
                                    }
                                    break;
                                }
                                d1.Add(item, Current_CCS.ToString("F2"));
                            }
                            ProcessDataTmp(d1, "充电中（恒流）", "报文中输出电流(A)", (Current[i] * 0.8).ToString(), "-");
                            Thread.Sleep(1000);

                            SendNoticeToUIAndTxtFile("等待通道4均方根电流稳定...");
                            double Current_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                int timeout = 35;
                                while (timeout-- > 0)
                                {
                                    Current_Analyzer = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                    if (Current_Analyzer > Current[i] * 0.9)
                                    {
                                        break;
                                    }

                                    System.Threading.Thread.Sleep(1000);
                                }
                            }

                            if (Current_Analyzer < Current[i] * 0.8)
                            {
                                SendNoticeToUIAndTxtFile("带载未稳定");
                            }
                            d1 = new Dictionary<int, string>();
                            foreach (int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, Current_Analyzer.ToString("F2"));
                            }
                            //ProcessDataTmp(d1, "充电中（恒流）", "实际输出电流(A)", (Current[i] * (1 - Norm / 100)).ToString("F2"), (Current[i] * (1 + Norm / 100)).ToString("F2"));
                            ProcessDataTmp(d1, "充电中（恒流）", "实际输出电流(A)", (Current[i] * 0.8).ToString(), "-");
                            Thread.Sleep(1000);

                            //if (TestTime != 0)
                            //{
                            //    System.Threading.Thread.Sleep(TestTime * 1000);
                            //}
                            //if (i == 1)
                            //{
                            //    System.Threading.Thread.Sleep(6000);
                            //}

                            //输出电流测量误差不应超过±（1.5 % *IM + 1）A
                            double errorCurrent = Norm / 100 * Current_Analyzer + 1;
                            d1 = new Dictionary<int, string>();
                            foreach(int item in testWorkParam.lstIDs)
                            {
                                d1.Add(item, (Current_CCS - Current_Analyzer).ToString("F2"));
                            }
                            ProcessDataTmp(d1, "充电中（恒流）", "输出电流测量误差(A)", (-errorCurrent).ToString("F2"), errorCurrent.ToString("F2"));
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
