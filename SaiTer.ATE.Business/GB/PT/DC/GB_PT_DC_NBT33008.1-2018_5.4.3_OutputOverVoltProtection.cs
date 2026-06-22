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
    /// 国标产测直流：输出过压保护试验
    /// </summary>
    public class GB_PT_DC_OutputOverVoltProtection : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double OutputVoltage = 800;
        double BmsDemandVoltage = 500;
        double FBLoadMaxVoltage = 1000;//回馈负载最大电压(V)=1000

        public GB_PT_DC_OutputOverVoltProtection(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            //过压参考值(V)=800
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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
                SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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
                                //测试时间|输入电压|输出电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //开始检测流程
                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                SetConditionValues();

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BmsDemandVoltage, 250, true, BmsDemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BmsDemandVoltage, LstChargerInfo[0].ChargerType, 0);
                //检查刷卡上电
                double Voltage = 0;
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    Voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingVoltage;
                }
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[0] + "号枪处于充电状态，该枪停止检测");
                    return;
                }

                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                {
                    SendNoticeToUIAndTxtFile(string.Format("调整需求电压为{0}V", BmsDemandVoltage));
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, 480, 20, 500, 20);
                }
                Thread.Sleep(10000);    //等待负载稳定
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                }
                ProcessDataTmp(d1, "过压保护前正常充电", "充电电压(V)", "-", "-");

                if ((ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null) && OutputVoltage <= FBLoadMaxVoltage)
                {
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    if (Customer.Contains("XJ"))//XJ这里的回馈负载只有1000V，所以这里要用高压源
                    {
                        SendNoticeToUIAndTxtFile("模拟输出过压");
                        List<bool> Ks = GetKStatus16_Charging_DC();

                        Ks[26] = true;
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, OutputVoltage);
                        Thread.Sleep(500);
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, OutputVoltage, Ks.ToArray());
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("发送输出电压异常值：" + OutputVoltage + "V，等待输出稳定。");
                        SetLoadPara(testWorkParam.lstIDs, OutputVoltage, 20, OutputVoltage, 20);
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCVoltage(testWorkParam.lstIDs, OutputVoltage, 30);
                    }
                }
                else
                {
                    SendNoticeToUIAndTxtFile("模拟输出过压");
                    List<bool> Ks = GetKStatus16_Charging_DC();

                    Ks[26] = true;
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, OutputVoltage);
                    Thread.Sleep(3000);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, OutputVoltage, Ks.ToArray());
                }

                Thread.Sleep(5000);//等待输出稳定
                //ControlEquipMent.FeedbackLoad?.SetFeedbackLoadParams(testWorkParam.lstIDs, OutputVoltage, 20);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    double outputVolt = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                    int timeout = 100;
                    while (timeout-- > 0)
                    {
                        double outputVoltNew = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        if (outputVoltNew > outputVolt)
                            outputVolt = outputVoltNew;
                    }
                    d1.Add(item, outputVolt.ToString("F2"));
                }
                ProcessDataTmp(d1, "模拟过压时", "直流输出电压(V)", "-", "-");

                Thread.Sleep(2000);
                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                //ProcessDataConnect("应发出告警提示", "是否有告警提示");
                d1 = new Dictionary<int, string>();
                var dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, DicManualVerifyResult[item] ? "是" : "否");
                    dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, "是否有告警提示", dicResult, "应发出告警提示");


                Thread.Sleep(1000 * 10);
                // 关闭回馈负载
                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                {
                    if (Customer.Contains("XJ"))//XJ这里的回馈负载只有1000V，所以这里要用高压源
                    {
                        SendNoticeToUIAndTxtFile("恢复正常电压：" + (BmsDemandVoltage - 20) + "V，等待直流输出稳定。");
                        List<bool> Ks = GetKStatus16_Charging_DC();

                        Ks[26] = false;
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BmsDemandVoltage - 20);
                        Thread.Sleep(3000);
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BmsDemandVoltage - 20, Ks.ToArray());
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("恢复正常电压：" + (BmsDemandVoltage - 20) + "V，等待直流输出稳定。");
                        SetLoadPara(testWorkParam.lstIDs, BmsDemandVoltage - 20, 20, OutputVoltage - 50, 20);

                        Thread.Sleep(500);
                        SetLoadDCOFF(testWorkParam.lstIDs);
                    }
                }
                // 断开过压开关
                else
                {
                    List<bool> Ks1 = GetKStatus16_Charging_DC();
                    Ks1[26] = false;
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                    Thread.Sleep(3000);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks1.ToArray());
                }
                Thread.Sleep(500);
                SetLoadDCOFF(testWorkParam.lstIDs);

                //WaitDCVoltage(testWorkParam.lstIDs, BmsDemandVoltage);
                Thread.Sleep(1000 * 5);

                //采集数据
                //ProcessDataResult(testWorkParam.lstIDs, BmsDemandVoltage.ToString(), "BMS需求电压(V)", true, "");
                //ProcessDataResult(testWorkParam.lstIDs, OutputVoltage.ToString(), "过压参考值(V)", true);
                var Data_Volt = new Dictionary<int, string>();
                //var Data_Current = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                    int timeout = 20;
                    while (timeout-- > 0)
                    {
                        if (voltage < 30)
                        {
                            break;
                        }
                        voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                        Thread.Sleep(100);
                    }
                    Data_Volt.Add(testWorkParam.lstIDs[i], voltage.ToString());
                }
                ProcessDataTmp(Data_Volt, "保护后桩应停止充电", "保护后桩输出电压(V)", "0", "50");
                //这里不能采集电压，因为我们自己会有模拟的过压电压，所以只需要判断充电状态即可。
                //string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState;
                //if (chargerState != "充电中")
                //{
                //    ProcessDataResult(testWorkParam.lstIDs, "是", "是否切断直流输出", true, "保护后桩应停止充电");
                //}
                //else
                //{
                //    ProcessDataResult(testWorkParam.lstIDs, "否", "是否切断直流输出", false, "保护后桩应停止充电");
                //}
                d1 = new Dictionary<int, string>();
                dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState) != 9 ? "是" : "否");
                    dicResult.Add(item, ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, "是否切断直流输出", dicResult, "保护后桩应停止充电");

                //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                //Thread.Sleep(1000);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
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
                    double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
                    string strResult = "未停机";
                    if (volate < 50)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "已停机";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //测试时间|BMS需求电压|输出电压|是否停机|测试结果     
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|" + strResult;
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
