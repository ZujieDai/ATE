using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电连接控制时序
    /// </summary>
    public class CCS2_PT_DC_ChargingConnectControlSeq : BusinessBase
    {
        public CCS2_PT_DC_ChargingConnectControlSeq(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
        int trlTimeOut_S = 0;


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
                SendNoticeToUIAndTxtFile("恢复互操作中...");
                SetCPRersh_EUDC();
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

                    //插拔枪之后主板需要反应时间才会有正常的CP电压
                    SendNoticeToUIAndTxtFile("设备正在停止充电，请稍候...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                    //插枪完毕
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPVoltage;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "插枪完毕", "CP电压(V)", "8.37", "9.59");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPFrequency;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "插枪完毕", "CP频率(Hz)", "0", "5");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPDutyCycle;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "插枪完毕", "CP占空比(%)", "0", "5");



                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent+10 , true, 390);
                    Thread.Sleep(1000);


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent+10, true, 390);
                    Thread.Sleep(5000);

                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent, true, LstChargerInfo[0].NominalVoltage);
                    //Thread.Sleep(10*1000);//

                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//


                        SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                        SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, BMSDemandCurrent, BMSDemandVolt, BMSDemandCurrent);
                        SetLoadDCON(testWorkParam.lstIDs);


                        CountDownTimeInfo("等待带载稳定", 20, 0);



                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.ChargingVoltage;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "充电电压(V)", GetErrLimit_U_CCS2_DC(BMSDemandVolt)[0].ToString(), GetErrLimit_U_CCS2_DC(BMSDemandVolt)[1].ToString());
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.ChargingCurrent;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "充电电流(A)", GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[1].ToString());
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPVoltage;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "CP电压(V)", "5", "7");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPFrequency;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "CP频率(Hz)", "970", "1030");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPDutyCycle;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "CP占空比(%)", "4", "6");

                    //停止充电
                    SendNoticeToUIAndTxtFile("设备正在停止充电，请稍候...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(5000);
                    SendNoticeToUIAndTxtFile("设备正在采集数据，请稍候...");
                    Thread.Sleep(3000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.ChargingVoltage;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "停止充电", "充电电压(V)", "0", "20");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.ChargingCurrent;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "停止充电", "充电电流(A)", "0","2");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPVoltage;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "停止充电", "CP电压(V)", "8", "10");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPFrequency;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "停止充电", "CP频率(Hz)", "0", "5");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value?.CPDutyCycle;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "停止充电", "CP占空比(%)", "0", "5");



                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
            BMSDemandCurrent = double.Parse(strParams[1].Split('=')[1]);


        }

        public override void ProcessData()
        {

        }


    }
}
