using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 能量传输阶段测试
    /// </summary>
    public class GB_RT_DC_2025_ChargingStageTest_WaveRecoder : BusinessBase
    {
        string ItemFlow = "";
        int trlTimeOut_S = 30;
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        public GB_RT_DC_2025_ChargingStageTest_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent >= 30 ? LstChargerInfo[0].NominalCurrent : 30;
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
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

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    BMSDemandVolt = BMSDemandVolt > LstChargerInfo[0].NominalVoltage * 0.5 ? LstChargerInfo[0].NominalVoltage * 0.5 : BMSDemandVolt;
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    ResiLoadCurrent = ResiLoadCurrent >= MaxAllowChargeCurrent ? ResiLoadCurrent - 5 : ResiLoadCurrent;
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
                    WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent, BMSDemandVolt, ResiLoadCurrent);
                    double meauseVolt = ControlEquipMent.ResistanceLoad != null ? BMSDemandVolt : BMSDemandVolt - 20;
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);

                    ProcessDataResult(testWorkParam.lstIDs, BMSDemandVolt.ToString(), "设定充电电压需求值(V)", null, "能量传输阶段");
                    ProcessDataResult(testWorkParam.lstIDs, ResiLoadCurrent.ToString(), "设定充电电流需求值(A)", null, "能量传输阶段");

                    WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "能量传输阶段", "实际充电电压测量值(V)", (meauseVolt * 0.95).ToString("F2"), (meauseVolt * 0.105).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "能量传输阶段", "实际充电电流测量值(A)", (ResiLoadCurrent * 0.9).ToString("F2"), (ResiLoadCurrent * 1.1).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    //控制变化
                    //BMSDemandVolt = BMSDemandVolt * 0.5;
                    //有点桩改变需求电压小于当前充电机电压会直接过压保护，例如江门AK
                    BMSDemandVolt = BMSDemandVolt + 250 > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSDemandVolt + 250;
                    meauseVolt = ControlEquipMent.ResistanceLoad != null ? BMSDemandVolt : BMSDemandVolt - 20;
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
                    WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);

                    ProcessDataResult(testWorkParam.lstIDs, BMSDemandVolt.ToString(), "设定充电电压需求值(V)", null, "能量传输阶段");
                    ProcessDataResult(testWorkParam.lstIDs, ResiLoadCurrent.ToString(), "设定充电电流需求值(A)", null, "能量传输阶段");

                    WaitDCCurrent(testWorkParam.lstIDs, 20);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "实际充电电压测量值(V)", (meauseVolt * 0.95).ToString("F2"), (meauseVolt * 1.05).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "实际充电电流测量值(A)", "18.00", "22.00");

                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    //读取录波板数据
                    WaveData CH_OutputVoltage = new WaveData();
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_CC1 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");

                    var dImages = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                    CountDownTimeInfo("请确认电子锁是否可靠锁止。\r\n注：勾选上为可靠锁止", 20, 2);
                    ProcessDataConnect("阶段信息", "是否可靠锁止");

                    Dictionary<int, EmTrialResult> chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingState == "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                    Dictionary<int, string> commResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "异常");
                    ProcessDataResults(testWorkParam.lstIDs, commResult, "通讯状态", chargeStateResult, "阶段信息", "正常", "正常", dImages);

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].ItemName = ItemFlow;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;

                if (ItemFlow == "充电锁止状态")
                {
                    if (item.Value)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止|-|-|是|报表(勿删)";

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止|-|-|否|报表(勿删)";
                    }
                }
                else
                {
                    bool CanCharge = false;
                    int timeout = 10;
                    string BMSInfo;
                    while (timeout-- > 0)
                    {
                        BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstIDs[0]].ChargingState;
                        if (BMSInfo.Contains("充电中"))
                        {
                            CanCharge = true;
                            break;
                        }
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(5000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    //读取录波板数据
                    WaveData CH_OutputVoltage = new WaveData();
                    WaveData CH_OutputCurrent = new WaveData();
                    WaveData CH_CC1 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");

                    var dImages = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    StringBuilder sbtmp = new StringBuilder();
                    sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    if (CanCharge)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = ItemFlow + "|是否可以充电|-|-|是|" + sbtmp.ToString();
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = ItemFlow + "|是否可以充电|-|-|否|" + sbtmp.ToString();
                    }
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     

                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
