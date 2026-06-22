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
    /// 国标产测直流：充电控制状态试验
    /// </summary>
    public class GB_PT_DC_ChargingControlState : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public GB_PT_DC_ChargingControlState(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent >= 30 ? LstChargerInfo[0].NominalCurrent : 30;
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
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

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
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电流(A)", (ResiLoadCurrent * 0.9).ToString("F2"), (ResiLoadCurrent * 1.1).ToString("F2"));

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电压(V)", (BMSDemandVolt - 20).ToString("F2"), (BMSDemandVolt + 20).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000);

                    //控制变化
                    //BMSDemandVolt = BMSDemandVolt * 0.5;
                    //有点桩改变需求电压小于当前充电机电压会直接过压保护，例如江门AK
                    BMSDemandVolt = BMSDemandVolt + 250 > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : BMSDemandVolt + 250;
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
                    WaitDCVoltage(testWorkParam.lstIDs, BMSDemandVolt);
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    Thread.Sleep(1000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, 20);
                    Thread.Sleep(1000 * 3);//等待回馈负载电流稳定

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        dic.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电控制", "充电电流(A)", "18.00", "22.00");

                    dic.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "调整充电输出", "充电电压(V)", (BMSDemandVolt * 0.9).ToString("F2"), (BMSDemandVolt * 1.1).ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(300);

                    CountDownTimeInfo("请人工检查充电机应按照制造商声明的方式手动设定充电参数，并实施充电启停操作。\r\n注：勾选上为可以正常解锁", 20, 2);
                    //ProcessDataConnect("应能手动设定充电参数，并实施充电启停操作", "是否具备");
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "是", "应能手动设定充电参数，并实施充电启停操作", true, "是否具备");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "否", "应能手动设定充电参数，并实施充电启停操作", true, "是否具备");
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
