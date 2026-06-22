using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_SingleCharging : BusinessBase  //功率分配 单枪测试
    {
        public TPK_SingleCharging(int trialType) { TrialType = trialType; }


        int GunID = 1;
        List<int> MylstIDs = new List<int>();


        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError;


        public override void InitializeParams()
        {
            Init();
          
            //充电电压(V)=750|充电电流(A)=10|老化时间(分)=5|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5|枪号=1
            string[] strParams = TrialItem.ResultParams.Split('|');
            AgingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            AgingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            AgingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            IntervalTime = Convert.ToDouble(strParams[3].Split('=')[1]);
            VoltError = Convert.ToDouble(strParams[4].Split('=')[1]);
            CurrError = Convert.ToDouble(strParams[5].Split('=')[1]);
            if (strParams.Length > 6)
            {
                GunID = (int)Convert.ToDouble(strParams[6].Split('=')[1]);
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
                // ControlEquipMent.FeedbackLoad.FeedbackLoad_NoParallel(lstIDs);
                // SetCPReresh();
                SetLoadDCOFF(MylstIDs);  //设置关闭负载
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

                MylstIDs.Clear();
                MylstIDs.Add(GunID);





                //设置测试条件
                SetConditionValues();

                //  ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);

                SetCPReresh();   // 模拟插拔枪
                Thread.Sleep(2000); //延时2S  再提示刷卡  CheckSwipingCard 刷卡函数

                AgingVolt = AgingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt;
                if (!CheckSwipingCard(MylstIDs, AgingVolt, AgingCurr + 10, MaxAllowChargeVoltage, false))   //刷卡  带参数的刷卡函数  
                {
                    return;
                }
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, 200, true, AgingVolt);
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile(string.Format("设置带载电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                if (AgingVolt == MaxAllowChargeVoltage)
                    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 10, AgingCurr + 20, AgingVolt - 5, AgingCurr);  // 原AgingCurr + 20
                else
                    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 20, AgingCurr + 20, AgingVolt - 20, AgingCurr); // 原AgingCurr + 20
                Thread.Sleep(1000);
                SendNoticeToUIAndTxtFile("开启负载");
                SetLoadDCON(MylstIDs);    //开启负载
                SendNoticeToUIAndTxtFile("等待电流稳定");
                WaitDCCurrentWithTime(MylstIDs, AgingCurr, 40);   //等待电流稳定
                Thread.Sleep(1000 * 5);

                Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                foreach (var itmp in MylstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;  //获取充电电压 
                    if (voltage == 0)
                    {
                        Thread.Sleep(500);
                        voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    }
                    dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                    double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;   //获取充电电流
                    if (current == 0)
                    {
                        Thread.Sleep(500);
                        current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
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
                    foreach (var itmp in MylstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        if (voltage == 0)
                        {
                            Thread.Sleep(500);
                            voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                        }
                        dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                        double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                        if (current == 0)
                        {
                            Thread.Sleep(500);
                            current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
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