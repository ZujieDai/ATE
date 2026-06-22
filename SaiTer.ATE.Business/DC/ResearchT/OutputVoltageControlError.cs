using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    ///  研发测试:输出电压控制误差测试
    /// </summary>

    public class OutputVoltageControlError : BusinessBase
    {

        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 20;
        /// <summary>
        /// 需求电流2(A)
        /// </summary>

        Double ErrorPercentage = 5;


        public OutputVoltageControlError(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
            SetCPReresh();
            CombineControlResistance();
        }

        public override void InitializeParams()
        {
            Init();
            DemandVoltage = MinAllowChargeVoltage;

            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1)
            {
                ErrorPercentage = double.Parse(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                DemandVoltage = double.Parse(strParams[1].Split('=')[1]);
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

                    //Dictionary<int, string> dicV = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{

                    //    dicV.Add(item, DemandVoltage.ToString("F2"));
                    //}
                    //ProcessDataTmp(dicV, TrialItem.ItemName + "充电设置", "电压需求(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    DemandCurrent = RatedCurrent * 0.5;

                    string info = $"需求电压{DemandVoltage}V，电流{DemandCurrent + 20}A";
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile($"设置BMS需求电压{DemandVoltage}V，需求电流{DemandCurrent + 20}A，请稍候...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent + 20, true, DemandVoltage);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //Thread.Sleep(2 * 1000);
                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);
                    Thread.Sleep(5 * 1000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SendNoticeToUIAndTxtFile($"设置负载需求电压{DemandVoltage - 10}V，需求电流{DemandCurrent}A，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent, DemandVoltage, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 20);

                    SendNoticeToUIAndTxtFile("等待电压稳定中...");

                    Thread.Sleep(10 * 1000);



                    #region 测试过程
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCCurrent < DemandCurrent * 0.9 || DCCurrent > DemandCurrent * 1.1)
                            {
                                DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic, info, "直流输出电流(A)", "-", "-");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        int timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (DCVoltage < DemandVoltage * 0.99 || DCVoltage > DemandVoltage * 1.02)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, info, "直流输出电压(V)", "-", "-");

                    Dictionary<int, string> dErrorRate = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double VoDC = Convert.ToDouble(dic[item]);
                        double ErrorRate = System.Math.Abs((VoDC - DemandVoltage) / DemandVoltage);
                        ErrorRate = ErrorRate * 100;
                        dErrorRate.Add(item, ErrorRate.ToString("F2"));
                    }


                    ProcessDataTmp(dErrorRate, info, "电压误差(%)", "0", ErrorPercentage.ToString());
                    #endregion
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






    }
}
