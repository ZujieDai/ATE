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
    /// <summary>
    /// 充电准备就绪测试
    /// </summary>
    public class ReadyToChargeEU : BusinessBase
    {
        public ReadyToChargeEU(int type)
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
                    //SendNoticeToUIAndTxtFile("关闭BMS中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(6000);
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }

                    ProcessDataTmp(dic, "检测充电中电压1", "充电电压(V)", (BMSDemandVolt-10).ToString(), (BMSDemandVolt +10).ToString());

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

            //if (strParams.Length >= 3)
            //{
            //    BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            //    BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            //    BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
            //    BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
            //    ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
            //    ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
            //    //TestTime = Convert.ToInt32(strParams[2].Split('=')[1]);

            //}
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
                    string State = AllEquipStateData.DicBMS_EU_DC_StateData[LstTrialData[k].ChargerId].SystemState;
                    if (State == "CurrentDemandReq")
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = "充电状态" + "|是否解锁|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
