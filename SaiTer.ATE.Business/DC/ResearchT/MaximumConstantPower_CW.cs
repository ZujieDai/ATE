using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研测:最大恒功率（恒功率段）
    /// </summary> 
    public class MaximumConstantPower_CW: BusinessBase
    {
        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        double Error = 1;
        /// <summary>
        /// 继电器序号
        /// </summary>
        int mYRelayIndex = 0;

        public MaximumConstantPower_CW(int type)
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
            //±误差(%)=0.5
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length > 0 && strParams[0].Split('=').Length > 1)
            {
                Error = Convert.ToDouble(strParams[0].Split('=')[1]);
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

                    SendNoticeToUIAndTxtFile("开启并机中");
                    CombineControlResistance();
                    SendNoticeToUIAndTxtFile("启动充电中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("开启负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);

                    //标准中的点，电流扩展区域恒功率电压
                    double[] Current = new double[3];
                    double[] Voltage = new double[3];
                    Voltage[0] = MaxAllowChargeVoltage;
                    Voltage[1] = (MaxOutputPower * 1000 / MaxAllowChargeCurrent + MaxAllowChargeVoltage) / 2;
                    Voltage[2] = MaxOutputPower * 1000 / MaxAllowChargeCurrent;
                    Voltage = RetainDecimals<double>(Voltage);
                    Current[0] = (MaxOutputPower * 1000) / MaxAllowChargeVoltage;
                    Current[1] = (MaxOutputPower * 1000) / Voltage[1];
                    Current[2] = MaxAllowChargeCurrent;
                    Current = RetainDecimals<double>(Current);
                    Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                    TrialMethon(Voltage, Current);

                    var IsCWRate = ConfigurationManager.AppSettings["IsCWRate"];
                    if (IsCWRate != null && Convert.ToBoolean(IsCWRate))
                    {
                        //高电压段
                        Current = new double[2];
                        Voltage = new double[2];
                        Voltage[0] = (CWHightVoltH + CWHightVoltL) / 2;
                        Voltage[1] = CWHightVoltL;
                        Voltage = RetainDecimals<double>(Voltage);
                        Current[0] = (MaxOutputPower * 1000) / Voltage[0];
                        Current[1] = MaxAllowChargeCurrent;
                        Current = RetainDecimals<double>(Current);
                        Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                        TrialMethon(Voltage, Current);

                        // 低电压段
                        Current = new double[3];
                        Voltage = new double[3];
                        Voltage[0] = CWLowerVoltH;
                        Voltage[1] = (CWLowerVoltH + CWLowerVoltL) / 2;
                        Voltage[2] = CWLowerVoltL;
                        Voltage = RetainDecimals<double>(Voltage);
                        Current[0] = (MaxOutputPower * 1000) / CWLowerVoltH;
                        Current[1] = (MaxOutputPower * 1000) / Voltage[1];
                        Current[2] = MaxAllowChargeCurrent;
                        Current = RetainDecimals<double>(Current);
                        Current = CompareMaximum(Current, MaxAllowChargeCurrent);

                        TrialMethon(Voltage, Current);
                    }

                    SendNoticeToUIAndTxtFile("取消负载并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        private void TrialMethon(double[] Voltage, double[] Current)
        {

            for (int i = 0; i < Voltage.Length; i++)
            {
                if (IsRLoad(Voltage[i], Current[i]))
                {
                    if(AllEquipStateData.DicBMS_DC_StateData.First().Value.ChargingVoltage < 40)
                    {
                        if (!CheckSwipingCard(testWorkParam.lstIDs))
                        {
                            break;
                        }
                    }
                    SendNoticeToUIAndTxtFile("设置导引参数中");
                    double volt = Voltage[i];
                    if (Voltage[i] + 20 < MaxAllowChargeVoltage)
                    {
                        volt = Voltage[i] + 20;
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, volt, Current.Max(), false, 390);
                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, Voltage[i], Current.Max(), false, 390);
                    }
                    string info = $"输出电压点{Voltage[i]}V，输出电流点{Current[i]}A";
                    ProcessDataResult(testWorkParam.lstIDs, volt.ToString(), "BMS需求电压(V)", true, info);

                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, volt, MaxAllowChargeCurrent, false, 390);
                    WaitDCVoltage(testWorkParam.lstIDs, volt);

                    if (Voltage[i] + 20 < MaxAllowChargeVoltage)
                    {
                        SetLoadPara(testWorkParam.lstIDs, Voltage[i], Current[i] + 20, volt, Current[i]);
                    }
                    else
                    {
                        SetLoadPara(testWorkParam.lstIDs, Voltage[i] - 10, Current[i] + 20, Voltage[i], Current[i]);
                    }
                    //SetLoadPara(testWorkParam.lstIDs, volt - 20, Current[i] + 20, volt, Current[i]);
                    Thread.Sleep(300);
                    SetLoadDCON(testWorkParam.lstIDs);

                    WaitDCCurrentWithTime(testWorkParam.lstIDs, Current[i], 50);
                    Thread.Sleep(3 * 1000);

                    SendNoticeToUIAndTxtFile("判断结果中");
                    //先通过功率等待稳定
                    d1 = new Dictionary<int, string>();
                    Dictionary<int, string> dicPower = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                        if(MaxOutputPower * 1000 <= Voltage[i] * Current[i])
                        {
                            int timeout = 15;
                            while (timeout-- > 0)
                            {
                                if (MaxOutputPower * 1000 > Voltage[i] * Current[i])
                                {
                                    break;
                                }
                                //else if ((Voltage[i] + 20) * Current[i] / 1000.0 > MaxOutputPower)
                                //{
                                //    if (DCPower >= (MaxAllowChargeVoltage - 20) * Current[i] / 1000.0 * (1 - Error / 100.0))
                                //        break;
                                //}
                                else
                                {
                                    if (DCPower >= MaxOutputPower * (1 - Error / 100.0))
                                        break;
                                }
                                Thread.Sleep(1000);
                                DCPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                            }
                        }
                        dicPower.Add(item, DCPower.ToString("F2"));
                        d1.Add(item, MaxOutputPower.ToString("F2"));
                    }

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, info, "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC, info, "充电电流(A)", "-", "-");
                    //ProcessDataTmp(dic, info, "测充电电压(V)", (volt * 0.95).ToString(), (volt * 1.05).ToString());

                    double OutputPower = Convert.ToDouble(dicPower[LstChargerInfo[0].ChargerId]);

                    //if (OutputPower >= MaxOutputPower * 0.99)
                    //{
                    ProcessDataTmp(d1, info, "额定功率恒功率(kW)", "-", "-");
                    if (MaxOutputPower * 1000 > Voltage[i] * Current[i])
                        ProcessDataTmp(dicPower, info, "充电功率(kW)", "-", "-");
                    //else if((Voltage[i] + 20) * Current[i] / 1000.0 > MaxOutputPower)
                    //    ProcessDataTmp(dicPower, info, "充电功率(kW)", ((MaxAllowChargeVoltage - 20) * Current[i] / 1000.0 * (1 - Error / 100.0)).ToString("F2"), "-");
                    else
                        ProcessDataTmp(dicPower, info, "充电功率(kW)", (MaxOutputPower * (1 - Error / 100.0)).ToString(), "-");
                    //}
                    //else
                    //{
                    //if (i == 2)
                    //    ProcessDataTmp(dicC, info, "充电电流(A)", (MaxAllowChargeCurrent * 0.99).ToString(), "-");
                    //}

                    SendNoticeToUIAndTxtFile("关闭负载中");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2 * 1000);
                }

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
