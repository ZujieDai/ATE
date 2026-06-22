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
    /// 欧标研测直流：急停
    /// </summary>
    public class CCS2_RT_DC_EmergencyShutdown : BusinessBase
    {
        public CCS2_RT_DC_EmergencyShutdown(int type)
        {
            TrialType = type;
        }
        int trlTimeOut_S = 0;

        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
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
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(3000);

                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 4, 0, "0");
                    Thread.Sleep(5000);

                    CountDownTimeInfo("请判断是否有绝缘保护", 999, 1);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    var dicVolt = new Dictionary<int, string>();
                    string state = AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState;
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        int timeout = 50;
                        double voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        while (timeout-- > 0)
                        {
                            if (voltage > 20)
                            {
                                voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                                Thread.Sleep(100);
                            }
                        }
                        dicVolt.Add(item, voltage.ToString("F2"));
                    }
                    ProcessDataTmp(dicVolt, "绝缘故障", "直流输出电压(V)", "0", "20");
                    if (ChangeBMSChargeStatus_EU_DC(state) < 20)
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "紧急停止", "桩充电状态", true, "绝缘故障");
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "未能停充", "桩充电状态", false, "绝缘故障");
                    }


                    ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
