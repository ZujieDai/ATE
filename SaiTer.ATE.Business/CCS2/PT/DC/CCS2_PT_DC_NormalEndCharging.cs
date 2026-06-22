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
    /// 正常充电结束
    /// </summary>
    public class CCS2_PT_DC_NormalEndCharging : BusinessBase
    {
        public CCS2_PT_DC_NormalEndCharging(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;

        //private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
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
                    SendNoticeToUIAndTxtFile("关闭BMS中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPRersh_EUDC();


                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");



                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }



                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 5, BMSDemandVolt, ResiLoadCurrent);

                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    int timeout = 60;
                    while (timeout-- > 0)
                    {
                        bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                        if (StabilizeCurrent)
                        {
                            break;
                        }
                        SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                        System.Threading.Thread.Sleep(1000);
                    }
                    Thread.Sleep(2000);//等待回馈负载电流稳定





                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }

                    ProcessDataTmp(dic, "检测充电中电压1", "充电电压(V)", (BMSDemandVolt - 10).ToString(), (BMSDemandVolt + 10).ToString());


                    SendNoticeToUIAndTxtFile("BMS正在主动中止充电");

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("恢复互操命令中，请稍候...");

                    //设置电压电流启动BMS



                    CountDownTimeInfo("等待停止充电延时", TestTime, 0);


                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "BMS主动停止充电电压", "充电电压(V)", "0", "20");

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }



                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");


                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 5, BMSDemandVolt, ResiLoadCurrent - 5);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    timeout = 60;
                    while (timeout-- > 0)
                    {
                        bool StabilizeCurrent = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingCurrent >= ResiLoadCurrent * 0.85);
                        if (StabilizeCurrent)
                        {
                            break;
                        }
                        SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                        System.Threading.Thread.Sleep(1000);
                    }
                    Thread.Sleep(2000);//等待回馈负载电流稳定





                    Dictionary<int, string> dic3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic3.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }

                    ProcessDataTmp(dic3, "检测充电中电压2", "充电电压(V)", (BMSDemandVolt - 10).ToString(), (BMSDemandVolt + 10).ToString());



                    CountDownTimeInfo("请主动中止充电桩充电！", 999, 2);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



                    //设置电压电流启动BMS



                    CountDownTimeInfo("等待停止充电延时", TestTime, 0);


                    Dictionary<int, string> dic4 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        dic4.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic4, "充电桩主动停止充电电压", "充电电压(V)", "0", "20");
                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);




                    SendNoticeToUIAndTxtFile("恢复互操命令中，请稍候...");
                    SetCPRersh_EUDC();




                }
            }
            catch (Exception ex) { SendException(ex); }

        }

        double BMSDemandVolt = 400;
        double ResiLoadCurrent = 10;
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            //ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >= 1)
            {

                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));

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
