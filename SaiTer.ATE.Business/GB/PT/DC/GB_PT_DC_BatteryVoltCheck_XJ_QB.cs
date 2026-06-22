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
    /// 测试项：车辆动力蓄电池电压检测(源自南网企标，目前河南产测使用)
    /// </summary>
    public class GB_PT_DC_BatteryVoltCheck_XJ_QB : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double MaxChargeVoltage = 750;

        Double StorageBatteryVoltage_New = 190;

        Double StorageBatteryVoltage_New2 = 1010;


        public GB_PT_DC_BatteryVoltCheck_XJ_QB(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
        }

        public override void InitializeParams()
        {
            Init();
            //车辆动力蓄电池当前电池电压(V)=190|车辆动力蓄电池当前电池电压2(V)=1010|BMS需求电压设置(V)=750|最高允许充电总电压(V)=750
            string[] strParams = TrialItem.ResultParams.Split('|');


            if (strParams.Length >= 2)
            {
                StorageBatteryVoltage_New = double.Parse(strParams[0].Split('=')[1]);
                StorageBatteryVoltage_New2 = double.Parse(strParams[1].Split('=')[1]);
            }
            if (strParams.Length >= 4)
            {
                DemandVoltage = double.Parse(strParams[2].Split('=')[1]);
                MaxChargeVoltage = double.Parse(strParams[3].Split('=')[1]);
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
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);
                SetCPReresh();
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
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, 200, true, 390);
                    ControlEquipMent.BMS.SetParameter(lstIDs, StorageBatteryVoltage_New, MaxChargeVoltage, 250);

                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dic.Add(item, DemandVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dicC.Add(item, MaxChargeVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电设置", "BMS电压需求(V)", "-", "-");
                    ProcessDataTmp(dicC, "充电设置", "最高允许充电总电压(V)", "-", "-");

                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 60;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state >= 3 && state <= 9)
                        {
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    SendNoticeToUIAndTxtFile("判断能否充电中");


                    timeout = 60;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state == 9)
                        {

                            break;
                        }
                        if (state <= 0 || state > 9)
                        {
                            break;
                        }
                        //SystemEvent.SendLogMessage("判断能否充电中倒计时： " + timeout + "秒   \r\t  \r\t ");
                        System.Threading.Thread.Sleep(1000);
                    }
                    Dictionary<int, string> dicV = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicV, $"蓄电池当前电池电压(V)={StorageBatteryVoltage_New}", "充电电压(V)", "0", "20");


                    SendNoticeToUIAndTxtFile("关闭导引中");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                    SetCPReresh();

                    SendNoticeToUIAndTxtFile("开启导引中");

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, 200, true, 390);
                    ControlEquipMent.BMS.SetParameter(lstIDs, StorageBatteryVoltage_New2, MaxChargeVoltage, 250);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    MessgaeInfo(true, "请刷卡充电!");
                    timeout = 60;
                    while (timeout-- > 0)
                    {
                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state >= 3 && state <= 9)
                        {
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    SendNoticeToUIAndTxtFile("判断能否充电中");


                    timeout = 60;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state == 9)
                        {

                            break;
                        }
                        if (state <= 0 || state > 9)
                        {

                            break;
                        }
                        SystemEvent.SendLogMessage("判断能否充电中倒计时： " + timeout + "秒   \r\t  \r\t ");
                        System.Threading.Thread.Sleep(1000);
                    }

                    System.Threading.Thread.Sleep(10 * 1000);

                    Dictionary<int, string> dicV2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dicV2.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicV2, $"蓄电池当前电池电压(V)={StorageBatteryVoltage_New2}", "充电电压(V)", "0", "20");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 999, 2);
                    ProcessDataConnect("应发出告警提示", "是否有告警提示");


                    SendNoticeToUIAndTxtFile("关闭导引中");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(2000);
                    SetCPReresh();

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


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = "蓄电池电压超过充电机范围" + "|是否报警|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = "蓄电池电压超过充电机范围" + "|是否报警|-|-|否";
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
                    LstTrialData[i].ExtentData = "蓄电池电压超过充电机范围" + "|充电状态|-|-|" + State;
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
