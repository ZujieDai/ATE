using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_ErrorPayment : BusinessBase
    {

        public TPK_ErrorPayment(int type)
        {
            TrialType = type;
        }


        private double ChargingVolt, ChargingCurr, ChargingTime;


        public override void InitializeParams()
        {
            Init();

            //充电电压(V)=750|充电电流(A)=100|充电时间(分)=2
            string[] strParams = TrialItem.ResultParams.Split('|');
            ChargingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            ChargingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            ChargingTime = Convert.ToDouble(strParams[2].Split('=')[1]);

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
                // ControlEquipMent.FeedbackLoad.FeedbackLoad_NoParallel(lstIDs);
                // SetCPReresh();
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭负载");
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
                //设置测试条件
                SetConditionValues();

                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, ChargingVolt, ChargingCurr + 10, true, ChargingVolt);   //需求BMS充电参数设置
                if (!CheckSwipingCard(testWorkParam.lstIDs, ChargingVolt, ChargingCurr + 10, MaxAllowChargeVoltage, false))
                {
                    return;
                }

                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile(string.Format("设置BMS电压{0}V,带载电流{1}A，等待负载稳定", ChargingVolt, ChargingCurr));
                if (ChargingVolt == MaxAllowChargeVoltage)
                    SetLoadPara(testWorkParam.lstIDs, ChargingVolt - 10, ChargingCurr, ChargingVolt - 5, ChargingCurr);  // 原AgingCurr + 20
                else
                    SetLoadPara(testWorkParam.lstIDs, ChargingVolt - 20, ChargingCurr, ChargingVolt - 20, ChargingCurr); // 原AgingCurr + 20
                Thread.Sleep(1000);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrentWithTime(testWorkParam.lstIDs, ChargingCurr, 40);
                Thread.Sleep(1000 * 3);

                Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                foreach (var itmp in testWorkParam.lstIDs)
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
                ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", ChargingVolt), "桩实际电压(V)", (ChargingVolt * 0.9).ToString(), (ChargingVolt * 1.1).ToString());
                ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", ChargingCurr), "桩实际电流(V)", (ChargingCurr * 0.9).ToString(), (ChargingCurr * 1.1).ToString());






                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds / 1000 <= ChargingTime * 60)   //充电时间
                {
                }

                dicAgingVolt.Clear();
                dicAgingCurr.Clear();

                foreach (var itmp in testWorkParam.lstIDs)
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
                ProcessDataTmp(dicAgingVolt, string.Format("充电{0}分钟", ChargingTime), "桩实际电压(V)", (ChargingVolt * 0.9).ToString(), (ChargingVolt * 1.1).ToString());
                ProcessDataTmp(dicAgingCurr, string.Format("设定{0}分钟", ChargingTime), "桩实际电流(V)", (ChargingCurr * 0.9).ToString(), (ChargingCurr * 1.1).ToString());

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(1000 * 3);
                CountDownTimeInfo("请确认充电中扣费金额（费率、电价、时段）结算是否正确。\r\n注：勾选上为合格。", 99, 2);
                ProcessDataResults(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult, "扣费测试");
                Thread.Sleep(500);


            }
        }
        public override void ProcessData()
        {

        }
    }
}





