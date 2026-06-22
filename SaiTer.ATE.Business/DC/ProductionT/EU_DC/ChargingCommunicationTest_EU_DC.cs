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
    /// 欧标通讯测试
    /// </summary>
    public class ChargingCommunicationTest_EU_DC : BusinessBase
    {
        private int TestTime = 0;
        private int trlTimeOut_S = 5;
        Double DemandVoltage = 500;
        Double DemandCurrent = 40;
        string ChargeState = "SLAC";

        public ChargingCommunicationTest_EU_DC(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ItemParams.Split('|');
            DemandVoltage = LstChargerInfo[0].NominalVoltage;
            DemandCurrent = LstChargerInfo[0].NominalCurrent;

            //判断充电状态=SLAC
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                ChargeState = strParams[0].Split('=')[1];
            }
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

                    SendNoticeToUIAndTxtFile("开启导引中");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(200);
                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, DemandVoltage, Ks.ToArray(), 0, 0, "0");
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BatteryVoltage_EU, DemandVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, 250, true, DemandVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    bool isPass = false;
                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 1500;
                    while (timeout-- > 0)
                    {

                        string state = AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState;
                        if (state.Contains(ChargeState))
                        {
                            isPass = true;
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }
                        if(ChangeBMSChargeStatus_EU_DC(state) >= 20)
                        {
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }
                        System.Threading.Thread.Sleep(200);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    if (!isPass)
                    {
                        timeout = 300;
                        while (timeout-- > 0)
                        {

                            string state = AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState;
                            if (state.Contains(ChargeState))
                            {
                                isPass = true;
                                break;
                            }
                            //if (ChangeBMSChargeStatus_EU_DC(state) < 20)
                            //{
                            //    break;
                            //}
                            System.Threading.Thread.Sleep(200);
                        }
                    }

                    if (isPass)
                        ProcessDataResult(testWorkParam.lstIDs, "存在", ChargeState, true, "通讯测试");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "缺失", ChargeState, false, "通讯测试");

                    SendNoticeToUIAndTxtFile("关闭BMS中...");
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
