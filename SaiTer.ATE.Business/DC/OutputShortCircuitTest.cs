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
    /// 输出短路保护试验
    /// </summary>
    public class OutputShortCircuitTest : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        /// <summary>
        /// 继电器序号
        /// </summary>
        int mYRelayIndex = 0;
        /// <summary>
        /// 短路装置
        /// </summary>
        int ShortCircuitChargeID = 2;

        List<int> MylstIDs = new List<int>();

        public OutputShortCircuitTest(int type)
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
            MylstIDs = new List<int>();
            string[] strParams = TrialItem.ItemParams.Split('|');
            if (strParams.Length >= 2)
            {
                double value= double.Parse(strParams[0].Split('=')[1]);
                ShortCircuitChargeID = Convert.ToInt32(value);
                double index = double.Parse(strParams[1].Split('=')[1]);
                mYRelayIndex = Convert.ToInt32(index);
                mYRelayIndex = mYRelayIndex - 1;
                MylstIDs.Add(ShortCircuitChargeID);
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
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                list[mYRelayIndex] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
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

                    CountDownTimeInfo("请人工确认是否具备短路保护功能,\r\n （勾选上代表具有）", 100, 2);
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "是", "是否具备短路保护功能", true);
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "否", "是否具备短路保护功能", false);
                        return;
                    }

                    CountDownTimeInfo("请确认充电枪插头插入到短路装置!!!", 99999, 0);

                    SendNoticeToUIAndTxtFile("开启导引中");

                    if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(ShortCircuitChargeID))
                    {
                        Dictionary<int, string> datas = new Dictionary<int, string>();
                        datas.Add(ShortCircuitChargeID, "不存在该枪号");
                        ProcessDataTmpThis(MylstIDs,datas, TrialItem.ItemName, "结果", "-", "-");
                        return;
                    }
                    //if (!CheckSwipingCard(MylstIDs))
                    //{
                    //    return;
                    //}

                    ControlEquipMent.BMS.BMS_OFF(MylstIDs);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(MylstIDs);

                    Thread.Sleep(2000);
                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 200;
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => MylstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");
                    WaitDCVoltage(MylstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(5000);

                    //短路工装没有接负载通道，并且电阻载可以带载短路，但是回馈载损伤会很大
                    //SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, RatedCurrent - 5, LstChargerInfo[0].NominalVoltage - 5, RatedCurrent - 5);
                    //Thread.Sleep(800);
                    ////SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 20, Current[i] + 10, Voltage[i] - 10, Current[i]);
                    ////Thread.Sleep(800);
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //WaitDCCurrent(testWorkParam.lstIDs, RatedCurrent - 5);
                    //Thread.Sleep(2000);

                    //设置测试条件
                    SetConditionValues();
                    //Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
                    //keyValuePairs.Add()

                    //ProcessDataTmp(dic, TrialItem.ItemName, "充电电压(V)", "0", "20");

                    d1 = new Dictionary<int, string>();
                    //d2 = new Dictionary<int, string>();
                    foreach (int item in MylstIDs)
                    {
                        d1.Add(1, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                        //d2.Add(1, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "短路前充电数据", "直流输出电压(V)", "-", "-");
                    //ProcessDataTmp(d2, "短路前充电数据", "直流输出电流(V)", "-", "-");



                    if (mYRelayIndex < 0)//这里手动
                    {
                        CountDownTimeInfo("请手动模拟短路故障!!!", 999, 0);
                    }
                    else//控制程控板
                    {
                        SendNoticeToUIAndTxtFile("闭合短路继电器");
                        //List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                        List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                        Thread.Sleep(500);
                        list[mYRelayIndex] = true;
                        ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                    }

                    Thread.Sleep(5000);

                    d1 = new Dictionary<int, string>();
                    //d2 = new Dictionary<int, string>();
                    foreach (int item in MylstIDs)
                    {
                        d1.Add(1, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                        //d2.Add(1, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "短路后充电数据", "直流输出电压(V)", "-", "-");
                    //ProcessDataTmp(d2, "短路后充电数据", "直流输出电流(V)", "-", "-");

                    CountDownTimeInfo("检查充电机应自动进入恒流输出状态或切断直流输出，并发出告警提示!\r\n注：勾选上为符合合格条件PASS", 999, 2);
                    ProcessData();

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(MylstIDs);

                    if (mYRelayIndex < 0)//这里手动
                    {
                        CountDownTimeInfo("请恢复正常状态!!!", 999, 0);
                    }

                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in MylstIDs)
                    //{
                    //    double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    //    dic.Add(item, DCVoltage.ToString("F2"));
                    //}
                    //ProcessDataTmpThis(MylstIDs, dic, "输出短路保护", "充电电压(V)", "0", "20");
                }



                CountDownTimeInfo("请确认充电枪插头插回到之前导引装置上!!!", 99999, 0);

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

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = "短接充电机的直流输出端|是否进入恒流输出状态或切断直流输出|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = "短接充电机的直流输出端|是否进入恒流输出状态或切断直流输出|-|-|否";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

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
