using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_SwipingCard : BusinessBase
    {

        public TPK_SwipingCard(int type)
        {
            TrialType = type;
        }

        private double ChargingVolt, ChargingCurr, ChargingTime, IntervalTime, VoltError, CurrError;


        public override void InitializeParams()
        {
            Init();
            //
            //充电电压(V)=750|充电电流(A)=60|老化时间(分)=5|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            ChargingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            ChargingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            ChargingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
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
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭导引BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭导引负载");
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

                // CountDownTimeInfo("请将充电桩辅源电压设置到12V ", 60, 2);      提示刷卡充电  不需要带载//  ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                // SetCPReresh();
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, ChargingVolt, 200, true, ChargingVolt);  //                                                                                            
                ChargingVolt = ChargingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : ChargingVolt;
                if (!CheckSwipingCard(testWorkParam.lstIDs, ChargingVolt, ChargingCurr, MaxAllowChargeVoltage, false))
                {
                    return;
                }
                SendNoticeToUIAndTxtFile("等待充电稳定");
                Thread.Sleep(2000);
                Dictionary<int, string> dicVolt = new Dictionary<int, string>();
                dicVolt.Clear();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    if (voltage == 0)
                    {
                        Thread.Sleep(500);
                        voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    }
                    dicVolt.Add(itmp, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, "刷卡启动充电", "充电电压(V)", (ChargingVolt - 50).ToString("F2"), (ChargingVolt + 50).ToString("F2"));

                SendNoticeToUIAndTxtFile("刷卡停止充电中。。。");
                CountDownTimeInfo("正常充电中，请刷卡停止充电 ", 99, 0);  //提示
                Thread.Sleep(1000);
                dicVolt.Clear();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    if (voltage == 0)
                    {
                        Thread.Sleep(500);
                        voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                    }
                    dicVolt.Add(itmp, voltage.ToString("F2"));

                }
                ProcessDataTmp(dicVolt, "刷卡停止充电", "充电电压(V)", "0", "20");
                Thread.Sleep(6000);



                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);




            }
        }
        public override void ProcessData()
        {

        }
    }
}
