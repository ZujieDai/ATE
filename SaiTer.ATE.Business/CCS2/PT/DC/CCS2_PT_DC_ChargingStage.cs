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
    /// 充电阶段测试	
    /// </summary>
    public class CCS2_PT_DC_ChargingStage : BusinessBase
    {
        public CCS2_PT_DC_ChargingStage(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private double BMSVoltage = 500;//电压
        private double BMSCurrent = 20;//
        private double BMSVoltage2 = 500;//电压
        private double BMSCurrent2 = 20;//电流
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        private double ErrorVoltageRate = 0.05;
        private double ErrorCurrentRate = 0.05;
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
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    //设置电压电流启动BMS


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltage, BMSCurrent, true, BMSMeasureVoltage);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    //检测插枪
                    //WaitSwipingCard(testWorkParam.lstIDs, 1);


                    //检测刷卡
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BMSVoltage, LstChargerInfo[0].ChargerType, 0);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    Thread.Sleep(5000);
                    SetLoadPara(testWorkParam.lstIDs, BMSVoltage - 20, BMSCurrent - 5, BMSVoltage, BMSCurrent - 5);

                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    int timeout = 60;
                    while (timeout-- > 0)
                    {
                        bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= (BMSCurrent - 5) * 0.85);
                        if (StabilizeCurrent)
                        {
                            break;
                        }
                        SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                        System.Threading.Thread.Sleep(1000);
                    }

                    Thread.Sleep(5000);




                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "检测充电中电压1", "充电电压(V)", (BMSVoltage * (1 - ErrorVoltageRate)).ToString(), (BMSVoltage * (1 + ErrorVoltageRate)).ToString());


                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent;
                        dic.Add(item, DCCurrent.GetValueOrDefault().ToString("F2"));
                    }

                    ProcessDataTmp(dicC, "检测充电中电流1", "充电电流(A)", ((BMSCurrent - 5) * (1 - ErrorCurrentRate)).ToString(), ((BMSCurrent - 5) * (1 + ErrorCurrentRate)).ToString());

                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("调整电压电流需求中...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltage2, BMSCurrent2, true, BMSMeasureVoltage);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");



                    SetLoadPara(testWorkParam.lstIDs, BMSVoltage2 - 20, BMSCurrent2 - 5, BMSVoltage2, BMSCurrent2 - 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");

                    while (timeout-- > 0)
                    {
                        bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= BMSCurrent2 * 0.85);
                        if (StabilizeCurrent)
                        {
                            break;
                        }

                        SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                        System.Threading.Thread.Sleep(1000);
                    }
                    Thread.Sleep(10 * 1000);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("判断充电状态中");
                    ProcessData();


                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic2, "检测充电中电压2", "充电电压(V)", (BMSVoltage2 * (1 - ErrorVoltageRate)).ToString(), (BMSVoltage2 * (1 + ErrorVoltageRate)).ToString());


                    Dictionary<int, string> dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent;
                        dic.Add(item, DCCurrent.GetValueOrDefault().ToString("F2"));
                    }

                    ProcessDataTmp(dicC2, "检测充电中电流2", "充电电流(A)", ((BMSCurrent2 - 5) * (1 - ErrorCurrentRate)).ToString(), ((BMSCurrent2 - 5) * (1 + ErrorCurrentRate)).ToString());


                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);



                    //SendNoticeToUIAndTxtFile("关闭BMS中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }



                //ChargeState = AllEquipStateData.DicBMS_DC_StateData.Any(kvp => kvp.Value.ChargingState != "充电中");


                //Ks = GetKStatus16_Charging_DC();
                //Ks[22] = false;


                //ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());

                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}





                //double BMSDemandVoltageMin = BMSVoltage - 10;
                //double BMSDemandVoltageMax = BMSVoltage + 10;
                //int timeout = 1000;

                //while (timeout-- > 0)
                //{
                //    SendNoticeToUIAndTxtFile("等到充电机到达充电中...");
                //    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //    {

                //        int count = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => kvp.Value.ChargingState.Contains("充电中"));


                //        if (!ChargeState)
                //        {
                //            break;
                //        }
                //    }

                //    System.Threading.Thread.Sleep(100);
                //}

                //SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                //ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, BMSDemandVolt - 20, 20);
                //Thread.Sleep(500);
                //ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(testWorkParam.lstIDs);
                //SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定

            }
            catch (Exception ex) { SendException(ex); }

        }

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
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

            if (strParams.Length >= 6)
            {
                BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                //TestTime = Convert.ToInt32(strParams[2].Split('=')[1]);

            }
        }

        public override void ProcessData()
        {
            try
            {

                foreach (var item in testWorkParam.lstIDs)
                {

                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    string State = AllEquipStateData.DicBMS_EU_DC_StateData[LstTrialData[k].ChargerId].SystemState;
                    if (State == "CurrentDemandRes" || State == "CurrentDemandReq")
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = TrialItem.ItemName + "|充电状态|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
