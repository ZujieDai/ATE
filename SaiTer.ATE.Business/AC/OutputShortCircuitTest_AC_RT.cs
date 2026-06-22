using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
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
    public class OutputShortCircuitTest_AC_RT : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        /// <summary>
        /// 继电器序号
        /// </summary>
        int mYRelayIndex = 0;

        public OutputShortCircuitTest_AC_RT(int type)
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
            string[] strParams = TrialItem.ItemParams.Split('|');

            if (strParams.Length >= 1)
            {
                double index = double.Parse(strParams[0].Split('=')[1]);
                mYRelayIndex = Convert.ToInt32(index) - 1;
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

                    #region 充电前

                    SendNoticeToUIAndTxtFile("闭合短路继电器");
                    //List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                    List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[mYRelayIndex] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);

                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    //CheckSwipingCard(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    CountDownTimeInfo("请刷卡充电!", 100, 2);

                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);

                    ProcessDataTmp1("充电前短路");

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        //dic.Add(item, DCVoltage.ToString("F2"));
                        double ACVoltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                        dic.Add(item, ACVoltage.ToString("F2"));
                    }


                    ProcessDataTmpThis(testWorkParam.lstIDs, dic, TrialItem.ItemName, "充电电压(V)", "0", "20");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[mYRelayIndex] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    #endregion
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //模拟故障
                    SendNoticeToUIAndTxtFile("闭合短路继电器");
                    //List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                    list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[mYRelayIndex] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);

                    Thread.Sleep(5000);

                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);

                    ProcessDataTmp1("充电中短路");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmpThis(testWorkParam.lstIDs, dic, TrialItem.ItemName, "充电电压(V)", "0", "20");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);

                    list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[mYRelayIndex] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);


                    #region 充电中




                    #endregion


                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {

                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                Thread.Sleep(500);
                list[mYRelayIndex] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
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

        public void ProcessDataTmp1(string sName)
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
                double volate = 0;
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                {
                    volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
                }
                else
                {
                    volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                }

                string stmp = "是";
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    stmp = "是";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    stmp = "否";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = sName + "|是否报警|-|-|" + stmp;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
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
        public void ProcessDataTmpThis(List<int> lstIDs, Dictionary<int, string> datas, string sState, string sName, string minValue, string maxValue, Dictionary<int, string> dImages = null)
        {
            try
            {
                foreach (var item in lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    bool bSx = false;//上限是否合格
                    bool bXx = false;//下限是否合格

                    double dData = 0;
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    double dSx = 0;//上限    
                    double dXx = 0;//下限
                    if (double.TryParse(maxValue, out dSx))
                    {
                        dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                        if (dData <= dSx)
                        {
                            bSx = true;
                        }
                    }
                    else if (maxValue.Trim() == "*" || maxValue.Trim() == "-")
                    {
                        bSx = true;
                    }
                    if (double.TryParse(minValue, out dXx))
                    {
                        dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                        if (dData >= dXx)
                        {
                            bXx = true;
                        }
                    }
                    else if (minValue.Trim() == "*" || minValue.Trim() == "-")//星号可以不判断直接合格
                    {
                        bXx = true;
                    }
                    if (bSx && bXx)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果
                    LstTrialData[i].ExtentData = sState
                        + "|" + sName
                        + "|" + minValue
                        + "|" + maxValue
                        + "|" + datas[LstChargerInfo[i].ChargerId].ToString()
                        + "|" + sbtmp.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    if (!sState.Contains("**"))//带特定符号的数据不保存
                    {
                        SaveTrialData(LstTrialData[k]);
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }
    }
}
