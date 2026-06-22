using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SaiTer.ATE.DataModel.ACTest;

namespace SaiTer.ATE.Business
{

    public class TPK_EVPlugLockFunction : BusinessBase
    {


        public TPK_EVPlugLockFunction(int type) { TrialType = type; }

        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError;


        public override void InitializeParams()
        {
            Init();
            //
            //老化电压(V)=750|老化电流(A)=100|老化时间(分)=30|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5
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

                //ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, AgingCurr + 10, true, AgingVolt);
                AgingVolt = AgingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt;
                if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr + 10, MaxAllowChargeVoltage, false))
                {
                    return;
                }

                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile("充电中,请检测电子锁是否闭合");
                // SendNoticeToUIAndTxtFile(string.Format("设置BMS电压{0}V,带载电流{1}A，等待负载稳定", AgingVolt, AgingCurr));
                //if (AgingVolt == MaxAllowChargeVoltage)
                //    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 10, AgingCurr, AgingVolt - 5, AgingCurr);  // 原AgingCurr + 20
                //else
                //    SetLoadPara(testWorkParam.lstIDs, AgingVolt - 20, AgingCurr, AgingVolt - 20, AgingCurr); // 原AgingCurr + 20
                //Thread.Sleep(1000);
                //SetLoadDCON(testWorkParam.lstIDs);
                //WaitDCCurrentWithTime(testWorkParam.lstIDs, AgingCurr, 40);
                Thread.Sleep(1000 * 2);

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
                ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", AgingVolt), "桩实际电压(V)", ((1.0 - VoltError / 100) * AgingVolt).ToString(), ((1.0 + VoltError / 100) * AgingVolt).ToString());
                // ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", AgingCurr), "桩实际电流(V)", ((1.0 - CurrError / 100) * AgingCurr).ToString(), ((1.0 + CurrError / 100) * AgingCurr).ToString());


                CountDownTimeInfo("请确认电子锁是否上锁\r\n注：勾选上为合格。", 99, 2);
                ProcessDataResults(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult, "枪锁止功能测试");

                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);  //关闭BMS和负载
              //  SetLoadDCOFF(testWorkParam.lstIDs);  //没有带载 只需关闭BMS
                Thread.Sleep(500);


            }
        }
        public override void ProcessData()
        {

        }
    }
}
