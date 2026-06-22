using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流研发:稳流精度
    /// </summary>
    public class SteadyCurrent : BusinessBase
    {

        int Count = 0;
        /// <summary>
        /// 判定准则(±%)
        /// </summary>
        double ErrorRate = 0.05;
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>)> CurrentLists1 = new List<(string, Dictionary<int, string>)>();
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>)> CurrentLists2 = new List<(string, Dictionary<int, string>)>();
        /// <summary>
        /// 保存的测试电流
        /// </summary>
        List<(string, Dictionary<int, string>)> CurrentLists3 = new List<(string, Dictionary<int, string>)>();
        /// <summary>
        /// 需求电流
        /// </summary>
        double[] BMSCurrent = new double[3];

        int trlTimeOut_S = 30;

        /// <summary>
        /// 测试时间
        /// </summary>
        int TestTime = 1;
        /// <summary>
        /// 输入电压85%额定值(V)
        /// </summary>
        double ACVoltage1 = 187;
        /// <summary>
        /// 输入电压100%额定值(V)
        /// </summary>
        double ACVoltage2 = 220;

        /// <summary>
        /// 输入电压115%额定值(V)
        /// </summary>
        double ACVoltage3 = 253;

        public SteadyCurrent(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            Count = 0;
            CurrentLists1 = new List<(string, Dictionary<int, string>)>();
            CurrentLists2 = new List<(string, Dictionary<int, string>)>();
            CurrentLists3 = new List<(string, Dictionary<int, string>)>();
            string[] strParams = TrialItem.ResultParams.Split('|');


            if (strParams.Length >= 1)
            {
                ACVoltage1 = double.Parse(strParams[0].Split('=')[1]);
                ACVoltage2 = double.Parse(strParams[1].Split('=')[1]);
                ACVoltage3 = double.Parse(strParams[2].Split('=')[1]);
                ErrorRate = double.Parse(strParams[3].Split('=')[1]) / 100;
                double time = double.Parse(strParams[4].Split('=')[1]);
                TestTime = int.Parse(time.ToString());
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
                SetACSource(lstIDs, 220);
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
                    SetConditionValues();
                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("开启并机继电器");
                    CombineControlResistance();

                    SendNoticeToUIAndTxtFile("开启负载并机");

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);


                    List<double> BMSVoltage = new List<double>();//BMS需求电压
                    BMSVoltage.Add(MinAllowChargeVoltage);
                    BMSVoltage.Add(MidAllowChargeVoltage);
                    BMSVoltage.Add(MaxAllowChargeVoltage);

                    BMSCurrent[0] = RatedCurrent * 0.2;
                    BMSCurrent[1] = RatedCurrent * 0.5;
                    BMSCurrent[2] = RatedCurrent;

                    List<double> ACVoltage = new List<double>();
                    ACVoltage.Add(ACVoltage1);
                    ACVoltage.Add(ACVoltage2);
                    ACVoltage.Add(ACVoltage3);


                    SendNoticeToUIAndTxtFile("启动充电中");

                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            CurrentAccuracy(BMSVoltage[0], BMSCurrent[k], ACVoltage[j]);
                        }

                    }
                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            CurrentAccuracy(BMSVoltage[1], BMSCurrent[k], ACVoltage[j]);
                        }

                    }
                    for (int j = 0; j < ACVoltage.Count; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            CurrentAccuracy(BMSVoltage[2], BMSCurrent[k], ACVoltage[j]);
                        }

                    }

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent > 1)
                    {
                        SetLoadDCOFF(testWorkParam.lstIDs);
                    }

                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("判断结果中!");
                    JudgeResult(CurrentLists1);

                    JudgeResult(CurrentLists2);

                    JudgeResult(CurrentLists3);
                }





            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        private void JudgeResult(List<(string, Dictionary<int, string>)> value)
        {
            if (value.Count <= 0)
            {
                return;
            }
            double CheckCurrent1 = 0, CheckCurrent2 = 0;
            if (value.Count > 0)
            {
                CheckCurrent1 = Convert.ToDouble(value[value.Count / 2].Item2[LstChargerInfo[0].ChargerId]) * (1 - ErrorRate);
                CheckCurrent2 = Convert.ToDouble(value[value.Count / 2].Item2[LstChargerInfo[0].ChargerId]) * (1 + ErrorRate);

            }
            for (int i = 0; i < value.Count; i++)
            {
                ProcessDataTmp(value[i].Item2, value[i].Item1, "充电电流(A)", CheckCurrent1.ToString(), CheckCurrent2.ToString());
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


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否报警|-|-|否";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

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


        public void CurrentAccuracy(double BMSVoltageSet, double BMSCurrentSet, double ACVoltageSet)
        {
            try
            {
                Count++;

                if (IsRLoad(BMSVoltageSet, BMSCurrentSet))
                {
                    //if ((Count - 1) % 3 == 0)
                    //{
                        if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent > 1)
                        {
                            SetLoadDCOFF(testWorkParam.lstIDs);
                        }
                        SetACSource(testWorkParam.lstIDs, ACVoltageSet);
                    //}



                    if (Count >= 19)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltageSet, BMSCurrentSet, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, BMSVoltageSet);
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltageSet - 20, BMSCurrentSet + 20, BMSVoltageSet - 5, BMSCurrentSet);
                        Thread.Sleep(1000);

                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSVoltageSet + 20, BMSCurrentSet, false, 390);
                        WaitDCVoltage(testWorkParam.lstIDs, BMSVoltageSet + 20);
                        SetLoadPara(testWorkParam.lstIDs, BMSVoltageSet , BMSCurrentSet + 20, BMSVoltageSet - 5, BMSCurrentSet);
                    }
                    if (AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent < 4)
                    {
                        SetLoadDCON(testWorkParam.lstIDs);
                    }




                    SendNoticeToUIAndTxtFile("点位运行中:"+TestTime+"s");

                    if (TestTime != 0)
                    {
                        Thread.Sleep(TestTime * 1000);
                    }



                    WaitDCCurrent(testWorkParam.lstIDs, BMSCurrentSet);


                    if (Count == 1)
                    {
                        SendNoticeToUIAndTxtFile("延时10s");
                        Thread.Sleep(10 * 1000);
                    }

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dic.Add(item, DCCurrent.ToString("F2"));
                    }

                    if (BMSCurrentSet == BMSCurrent[0])
                    {
                        CurrentLists1.Add(("充电电压:" + BMSVoltageSet + ";充电电流:" + BMSCurrent[0] , dic));
                    }
                    if (BMSCurrentSet == BMSCurrent[1])
                    {
                        CurrentLists2.Add(("充电电压:" + BMSVoltageSet + ";充电电流:" + BMSCurrent[1] , dic));
                    }
                    if (BMSCurrentSet == BMSCurrent[2])
                    {
                        CurrentLists3.Add(("充电电压:" + BMSVoltageSet + ";充电电流:" + BMSCurrent[2] , dic));
                    }
                }




            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        public void ProcessData(bool Status)
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
                    string State = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingState;

                    if (Status)
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
