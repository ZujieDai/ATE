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
    /// 欧标研测直流：电池端过电压保护
    /// </summary>
    public class CCS2_RT_DC_BatteryOvervoltageProtection : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double MaxChargeVoltage = 750;
        Double StorageBatteryVoltage_New = 1010;


        public CCS2_RT_DC_BatteryOvervoltageProtection(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            //BMS需求电压设置(V)=750
            string[] strParams = TrialItem.ResultParams.Split('|');
            DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                Thread.Sleep(200);
                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BatteryVoltage_EU, MaxChargeVoltage, 250);
                Thread.Sleep(200);
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage))
                    {
                        return;
                    }
                    //Thread.Sleep(200);
                    //bool[] Ks = new bool[24];
                    //Ks[0] = true;//DC+DC-控制
                    //Ks[1] = true;//CC信号控制
                    //Ks[2] = true;//CP信号控制
                    //Ks[4] = true;//PE信号控制
                    //ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, StorageBatteryVoltage_New, Ks.ToArray(), 0, 0, "0");
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, StorageBatteryVoltage_New, MaxChargeVoltage, 250);
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    dic.Add(item, DemandVoltage.ToString("F2"));
                    //}
                    //Dictionary<int, string> dicC = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    dicC.Add(item, MaxChargeVoltage.ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, "充电设置", "BMS电压需求(V)", "-", "-");
                    //ProcessDataTmp(dicC, "充电设置", "最高允许充电总电压(V)", "-", "-");


                    //SendNoticeToUIAndTxtFile("判断能否充电中");
                    //MessgaeInfo(true, "请刷卡充电!");
                    //int timeout = 60;
                    //while (timeout-- > 0)
                    //{

                    //    int state = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState);
                    //    if (state >= 20)
                    //    {
                    //        MessgaeInfo(false, "请刷卡充电!");
                    //        break;
                    //    }
                    //    System.Threading.Thread.Sleep(1000);
                    //}
                    //MessgaeInfo(false, "请刷卡充电!");
                    //CheckSwipingCard(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("充电过程中更改最大电压限制");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, DemandVoltage - 200, MaxAllowChargeCurrent);
                    Thread.Sleep(5000);
                    CountDownTimeInfo("请确认输出电压超过车辆最大电压是否保护", 999, 1);

                    Dictionary<int, string> dicV = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dicV.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicV, $"输出电压超过车辆最大电压{DemandVoltage - 200}V", "充电电压(V)", "0", "20");


                    SendNoticeToUIAndTxtFile("关闭导引中");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
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
