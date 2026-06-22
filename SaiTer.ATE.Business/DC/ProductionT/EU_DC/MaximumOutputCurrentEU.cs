using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
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
    /// 最大输出电流测试
    /// </summary>
    public class MaximumOutputCurrentEU : BusinessBase
    {
        private int TestTime = 0;
        private int trlTimeOut_S = 5;
        private double ErrorCurrentRate = 0.05;
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        Double DemandVoltage = 500;
        Double DemandCurrent = 40;

        public MaximumOutputCurrentEU(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >= 2)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                //BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                //BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                //ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                //ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));
                ErrorCurrentRate = Convert.ToDouble(strParams[1].Split('=')[1]) / 100;
            }
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

        private void StartItemFlow()
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
                    SetConditionValues();
                    if (ControlEquipMent.FeedbackLoad != null)
                    {
                        DemandVoltage = (MaxOutputPower / MaxAllowChargeCurrent) * 1000;
                        DemandCurrent = MaxAllowChargeCurrent;
                    }
                    else
                    {
                        DemandVoltage = LstChargerInfo[0].NominalVoltage;
                        double DemandCurrentNew = (MaxOutputPower / DemandVoltage) * 1000;
                        DemandCurrent = DemandVoltage * 0.24;
                        DemandCurrent = DemandCurrent > DemandCurrentNew ? DemandCurrentNew : DemandCurrent;
                    }
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, 390);
                    //Thread.Sleep(10 * 1000);

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    Thread.Sleep(2000);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 5, DemandVoltage, DemandCurrent);
                    Thread.Sleep(500);
                   SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    int timeout = 60;
                    while (timeout-- > 0)
                    {
                        bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                        if(StabilizeCurrent)
                        {
                            break;
                        }
                        SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                        System.Threading.Thread.Sleep(1000);
                    }
                    Thread.Sleep(5000);//等待回馈负载电流稳定

                    CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);
                    SendNoticeToUIAndTxtFile("判断结果中");




                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }




                    ProcessDataTmp(dicC2, "最大充电电流", "充电电流(A)", (DemandCurrent * (1-ErrorCurrentRate)).ToString(), (DemandCurrent * (1 + ErrorCurrentRate)).ToString());


                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //SendNoticeToUIAndTxtFile("关闭BMS中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
