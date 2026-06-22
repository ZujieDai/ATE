using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using SaiTer.ATE.InterFace;
using System.Linq;
using System.Security.Policy;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 通讯中断   通信中断  直流研发测试
    /// </summary>
    public class CommunicationOutage_R : BusinessBase
    {
        public CommunicationOutage_R(int trialType) { TrialType = trialType; }
        private double DemandVoltage = 400;
        private double DemandCurrent = 30;
        /// <summary>
        /// 测试流程阶段
        /// </summary>
        private int ItemFlowIndex = 1;
        private string crm_state = "";
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();

        double R4, BatteryVolt;
        List<bool> SBitS;
        public override void InitializeParams()
        {
            Init();
            dicImagePath = new Dictionary<int, string>();
            //BMS需求电压(V)=400|BMS需求电流(A)=30
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
            SBitS = GetKStatus16_Charging_DC();
        }
        public override void InitEquiMent()
        {
            Init();
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
                SBitS = ControlEquipMent.BMS.BMSGetKState_DC(lstIDs, out R4, out BatteryVolt)[testWorkParam.lstIDs[0]];
                SBitS[18] = false;//DC+
                SBitS[19] = false;//DC-
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, SBitS.ToArray());

                SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }
        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
        {
            SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");


            _StopWatch.Reset();
            _StopWatch.Start();
            while (true)
            {
                #region  ------  此部分代码保留,作用可忽略---------------


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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                #endregion

                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                InitOscillograph();
                SetConditionValues();

                SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                //if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                //{
                //    return;
                //}
                CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                //Thread.Sleep(1000 * 10);
                ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
                Thread.Sleep(1000 * 40);

                SBitS[18] = true;//DC+
                SBitS[19] = true;//DC-
                SBitS[31] = true;//停止发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                Thread.Sleep(500);

                SendNoticeToUIAndTxtFile("CEM充电需求超时错误值");
                int timeout = 20;
                double CEM = Convert.ToDouble(OscillographInstrumentReadValue(15, true, 16));
                while (timeout-- > 0)
                {
                    if (CEM == 0)
                    {
                        CEM = Convert.ToDouble(OscillographInstrumentReadValue(15, true, 16));
                        if (CEM == 1)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }

                ItemFlowIndex = 1;
                crm_state = "通讯中断";
                ProcessData();
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic.Add(1, CEM.ToString());
                ProcessDataTmp(dic, "通讯中断", "CEM报文", "1", "1");

                SendNoticeToUIAndTxtFile("等待触发中...");
                timeout = 35;
                bool istrigger = false;
                while (timeout-- > 0)
                {
                    istrigger = ControlEquipMent.Oscillograph.Oscillograph_ReadTrigger().FirstOrDefault().Value == 0;
                    if (istrigger)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                SendNoticeToUIAndTxtFile("计算第一次测试结果...");
                //double position1 = GetTriggerTime_Single3(15, true, 16, 1, false);
                GetTriggerTime_Single2(15, true, 16, 1, 0, false, true, 0.05);
                double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                double time = 0;
                if (K1K2s2 > 2)
                {
                    time = GetTriggerTime_Single2(7, false, 0, 6, 0, false, false, 0.05);
                    //position2 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 0, false, false, 0.05);//2为辅助电源，7为K1K2状态
                }
                else
                {
                    time = GetTriggerTime_Single2(7, false, 0, 6, 1, false, false, 0.05);
                    //position2 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 1, false, false, 0.05);//2为辅助电源，7为K1K2状态
                }

                //double time = 0;

                //if (position1 == 10 && position2 == 10)
                //{
                //    time = System.Math.Abs(GetTriggerTime_Position(1, false, 0, -5, 5));
                //    time = 10 * 10;
                //}
                //else
                //{
                //    time = System.Math.Abs(GetTriggerTime_Position(1, false, 0, position1 - 5, position2 - 5));
                //}

                dicImagePath = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                Thread.Sleep(200);
                ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                CountDownTimeInfo("请确认电子锁能正常解锁。\r\n注：勾选上为可以正常解锁", 5, 2);
                ProcessDataConnect();

                dic[1] = time.ToString();
                ProcessDataTmp(dic, "通讯中断", "K1K2断开时间(s)", "0", "10", dicImagePath);


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
                SBitS[18] = false;//DC+
                SBitS[19] = false;//DC-
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("20");
                ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                Thread.Sleep(5000);

                SetCPReresh();
                SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                timeout = 300;
                MessgaeInfo(true, "请刷卡充电!", true);
                while (timeout-- > 0)
                {
                    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                        continue;
                    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                    if (ALLCanCharge)
                    {
                        MessgaeInfo(false, "请刷卡充电!");
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                MessgaeInfo(false, "请刷卡充电!");
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);

                dicImagePath = null;
                crm_state = "CRM-1";
                ItemFlowIndex = 2;
                if (!TestFlow(20))
                    return;


                crm_state = "CRM-2";
                ItemFlowIndex = 3;
                if (!TestFlow(20))
                    return;

                crm_state = "CRM-3";
                ItemFlowIndex = 4;
                if (!TestFlow(20))
                    return;


                crm_state = "CRM-4";
                ItemFlowIndex = 5;
                if (!TestFlow(40))
                    return;

                dicImagePath = ControlEquipMent.Oscillograph?.OscillographSaveScreen();
                crm_state = "第四次重连结束";
                ItemFlowIndex = 6;
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(2000);
                CountDownTimeInfo("请给桩刷卡上电，刷卡后点击确认按钮，或者等待倒计时结束。\r\n（注:刷卡前请勿拔插枪。合格桩此处应【不允许充电】）", 50, 2);
                Thread.Sleep(2000);
                ProcessData();

            }
        }
        private bool TestFlow(int time)
        {
            SendNoticeToUIAndTxtFile($"开始第{ItemFlowIndex - 1}次超时重连。");
            string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs.First()].ChargingState;
            if (chargerState != "充电中")
            {
                SendNoticeToUIAndTxtFile("目前充电状态异常，请检查充电状态，即将退出该项目，开始下一个测试项目！！！");
                ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态异常", false, crm_state);
                return false;
            }
            SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
            //Thread.Sleep(5000);
            SBitS = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out R4, out BatteryVolt)[testWorkParam.lstIDs[0]];
            SBitS[18] = true;//DC+
            SBitS[19] = true;//DC-
            SBitS[31] = true;//停止发送报文
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            Thread.Sleep(3000);
            if (ItemFlowIndex >= 2 && ItemFlowIndex <= 4)
            {
                SendNoticeToUIAndTxtFile("发送恢复通讯指令");
                SBitS = ControlEquipMent.BMS.BMSGetKState_DC(testWorkParam.lstIDs, out R4, out BatteryVolt)[testWorkParam.lstIDs[0]];
                SBitS[18] = false;//DC+
                SBitS[19] = false;//DC-
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
            }
            SendNoticeToUIAndTxtFile($"等待{time}秒恢复充电");
            Thread.Sleep(1000 * time);
            ProcessData();
            //Thread.Sleep(1000);
            return true;
        }



        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();


                    if (dicImagePath != null)
                    {
                        sbtmp.Append(dicImagePath[item]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }


                    string result = "";

                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    string info = "";
                    if (ItemFlowIndex == 1 || ItemFlowIndex >= 5)
                    {
                        if (state >= 7 && state <= 10)//准备开始充电
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            result = "允许充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            result = "不允许充电";
                        }
                        info = "应不允许充电";
                    }
                    else
                    {
                        if (state == 9)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            result = "已恢复充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            result = "未恢复充电";
                        }
                        info = "应恢复充电";
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果      

                    LstTrialData[i].ExtentData = crm_state
                    + "|" + info
                    + "|" + "-"
                    + "|" + "-"
                    + "|" + result
                    + "|" + sbtmp.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);

                    SaveTrialData(LstTrialData[k]);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }
        /// <summary>
        ///录波仪初始化
        /// </summary>
        private void InitOscillograph()
        {
            try
            {
                ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");
                ControlEquipMent.Oscillograph?.Oscillograph_CursorsSet("XY");
                OscillographInstrument_SetTrigger(6, 2, 0, "FALL", false, 90, "Auto");

                SetChannelOpenInit();
                channelopen[0] = true;//1通道
                channelopen[1] = true;//2通道
                channelopen[3] = true;//4通道
                channelopen[6] = true;//7通道
                channelopen[7] = true;//8通道

                canchannelopen[10] = true;//CAN 11通道
                canchannelopen[11] = true;//CAN 12通道
                canchannelopen[12] = true;//CAN 13通道
                canchannelopen[15] = true;//CAN 16通道

                SetChannel(channelopen, canchannelopen);

                ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("5");
                ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        bool[] channelopen = new bool[8];
        bool[] canchannelopen = new bool[20];
        public void SetChannelOpenInit()
        {
            for (int i = 0; i < channelopen.Length; i++)
            {
                channelopen[i] = false;
            }
            for (int i = 0; i < canchannelopen.Length; i++)
            {
                canchannelopen[i] = false;
            }
        }
    }
}
