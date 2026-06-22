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
    /// 国标直流研发:直流输出回路短路检测功能试验
    /// </summary>
    public class CircuitShortFunction : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        ///// <summary>
        ///// 继电器序号
        ///// </summary>
        //int mYRelayIndex = 0;



        public CircuitShortFunction(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
            SetCPReresh();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');


            //if (strParams.Length >= 1)
            //{
            //    double index = double.Parse(strParams[0].Split('=')[1]);
            //    mYRelayIndex = Convert.ToInt32(index);
            //    mYRelayIndex = mYRelayIndex - 1;
            //}
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
                    SetConditionValues();

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    PullCharger(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("模拟输出短路");
                    CountDownTimeInfo("请模拟直流输出回路短路故障!", 99999, 0);

                    //List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                    //Thread.Sleep(500);
                    //list[mYRelayIndex] = true;
                    //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);

                    SendNoticeToUIAndTxtFile("开启导引中");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    InsertCharger(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("开始充电中...");
                    MessgaeInfo(true,"请刷卡充电!");

                    double insulationVolt = 0;  //绝缘电压
                    while (true)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state > 2 && state <= 9)
                        {
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }
                        if (state <= 0 || state > 9)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    int timeout = 100;
                    while (timeout-->0)
                    {
                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state > 3)
                        {
                            if (state < 5)
                            {
                                double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                                insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                            }
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    //采集绝缘阶段电压
                    timeout = 100;
                    while (timeout-- > 0)
                    {
                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state <= 5)
                        {
                            double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                            insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                        }
                        else
                            break;
                        System.Threading.Thread.Sleep(100);
                    }
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, insulationVolt.ToString());
                    }
                    ProcessDataTmp(d1, "直流输出回路短路检测试验", "绝缘电压(V)", "-", "-");


                    System.Threading.Thread.Sleep(2000);
                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 999, 2);
                    ProcessDataConnect("应发出告警提示", "是否有告警提示");

                    ProcessData();

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    CountDownTimeInfo("请将短路恢复到正常!", 99999, 0);


                    //ProcessData();
                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{
                    //    double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    //    dic.Add(item, DCVoltage.ToString("F2"));
                    //}


                    //ProcessDataTmp(dic, TrialItem.ItemName, "充电电压(V)", "0", "20");
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
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
                    string State = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingState;
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    if (state > 4 && state <= 9)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        State= "否";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        State= "是";
                    }

                    //if (Status)
                    //{
                    //    LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    //}
                    //else
                    //{
                    //    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    //}
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = "直流输出回路出现短路故障|是否停止充电过程|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
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
