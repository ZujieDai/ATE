using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标研测直流：初始化阶段电压检查
    /// </summary>
    public class CCS2_RT_DC_InitializationVoltageCheck : BusinessBase
    {
        public CCS2_RT_DC_InitializationVoltageCheck(int type)
        {
            TrialType = type;
        }

        private int trlTimeOut_S = 10;

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
                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
                Thread.Sleep(300);
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

                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    bool[] Ks = new bool[24];
                    Dictionary<int, string> dic = new Dictionary<int, string>();

                    if (Customer.Equals("XJ"))//XJ客户不做第一种情况
                    {
                        //BMS受限最低100V外侧电压
                        SendNoticeToUIAndTxtFile("正在设置外侧电压0V");
                        Ks = new bool[24];
                        Ks[0] = true;//DC+DC-控制
                        Ks[1] = true;//CC信号控制
                        Ks[2] = true;//CP信号控制
                        Ks[4] = true;//PE信号控制
                                     //Ks[14] = true;//输出过压控制
                                     //ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 50, Ks.ToArray(), 0, 0, "0");
                        ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
                        Thread.Sleep(300);
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);
                        Thread.Sleep(2000);

                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value.ChargingVoltage;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "初始化阶段（外侧电压小于60V）", "外侧电压(V)", "-", "-");

                        SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                        //检测能否刷卡
                        //WaitSwipingCard(testWorkParam.lstIDs, 3);
                        SystemEvent.SendWaitSwipingCard(lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);
                        //CountDownTimeInfo("判断充电机是否能充电倒计时", 60, 0);

                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "初始化阶段（外侧电压小于60V）", "充电电压(V)", (MaxAllowChargeVoltage * 0.98).ToString(), (MaxAllowChargeVoltage * 1.02).ToString());

                        SendNoticeToUIAndTxtFile("关闭导引中...");
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);

                        SendNoticeToUIAndTxtFile("恢复BMS互操设置中...");
                        SetCPRersh_EUDC();
                    }


                    SendNoticeToUIAndTxtFile("正在设置外侧电压100V");
                    Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    Ks[14] = true;//输出过压控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 100, Ks.ToArray(), 0, 0, "0");
                    Thread.Sleep(3000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.FirstOrDefault().Value.ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "初始化阶段（外侧电压大于60V）", "外侧电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    //检测能否刷卡
                    //WaitSwipingCard(testWorkParam.lstIDs, 3);
                    SystemEvent.SendWaitSwipingCard(lstIDs, MaxAllowChargeVoltage, LstChargerInfo[0].ChargerType, 0);
                    //CountDownTimeInfo("判断充电机是否能充电倒计时", 60, 0);

                    string state = AllEquipStateData.DicBMS_EU_DC_StateData.First().Value.SystemState;
                    if (ChangeBMSChargeStatus_EU_DC(state) >= 20)
                        ProcessDataResult(testWorkParam.lstIDs, state, "充电状态", false, "初始化阶段（外侧电压大于60V）");
                    else
                        ProcessDataResult(testWorkParam.lstIDs, state, "充电状态", true, "初始化阶段（外侧电压大于60V）");

                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("恢复BMS互操设置中...");
                    SetCPRersh_EUDC();
                }
            }
            catch (Exception ex) { SendException(ex); }

        }

        public override void ProcessData()
        {

        }
    }
}
