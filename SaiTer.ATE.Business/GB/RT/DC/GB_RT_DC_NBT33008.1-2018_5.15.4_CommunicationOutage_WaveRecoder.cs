using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    /// 通讯中断测试（录波板）
    /// </summary>
    public class GB_RT_DC_CommunicationOutage_WaveRecoder : BusinessBase
    {
        public GB_RT_DC_CommunicationOutage_WaveRecoder(int trialType) { TrialType = trialType; }
        private double DemandVoltage = 400;
        private double DemandCurrent = 30;
        /// <summary>
        /// 测试流程阶段
        /// </summary>
        private int ItemFlowIndex = 1;
        private string crm_state = "";
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();

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
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

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


                SendNoticeToUIAndTxtFile("启动BMS，等待刷卡起桩");
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                ////Thread.Sleep(1000 * 10);
                //ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Single");
                SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
                Thread.Sleep(1000 * 40);

                SBitS[31] = true;//停止发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                Thread.Sleep(500);


                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(15000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                WaveData CH_CEM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 37, 2, ref CH_CEM, "CEM_BCL");
                double Time_CEM = 0;
                DataAnalysis_WaveRecoder.GetCANMsgTime(CH_CEM, 1, true, ref Time_CEM);//获取CEM出现的地方
                int timeout = 20;
                //double CEM = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CEM, CH_CEM.LinePoints_Y.Count - 2);//获取CEM最后的值
                double CEM = DataAnalysis_WaveRecoder.GetWavePointVave(CH_CEM, (int)(Time_CEM + 5));//获取CEM的值,XJ客户那里的会重新变为0，这里做一下处理



                ItemFlowIndex = 1;
                crm_state = "通讯中断";
                ProcessData();
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic.Add(1, CEM.ToString());
                ProcessDataTmp(dic, "通讯中断", "CEM报文", "1", "1");



                //读取录波板数据
                double Time_K1K2 = 0;
                WaveData CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CEM);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                double Time_Stop = Math.Abs(Time_CEM - Time_K1K2);

                SendNoticeToUIAndTxtFile("计算第一次测试结果...");

                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                Thread.Sleep(200);
                CountDownTimeInfo("请确认电子锁能正常解锁。\r\n注：勾选上为可以正常解锁", 20, 2);
                ProcessDataConnect();

                dic[1] = (Time_Stop / 1000).ToString();
                ProcessDataTmp(dic, "通讯中断", "K1K2断开时间(s)", "0", "10", dicImagePath);


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                ControlEquipMent.Oscillograph.Oscillograph_TimeBase("20");
                ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

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

                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                dicImagePath = null;
                crm_state = "CRM-1";
                ItemFlowIndex = 2;
                TestFlow(20);


                crm_state = "CRM-2";
                ItemFlowIndex = 3;
                TestFlow(20);

                crm_state = "CRM-3";
                ItemFlowIndex = 4;
                TestFlow(20);


                crm_state = "CRM-4";
                ItemFlowIndex = 5;
                TestFlow(40);

                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                CH_CEM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 37, 2, ref CH_CEM, "CEM_BCL");
                CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                WaveData CH_Voltage = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_Voltage, "ChargingVoltage");


                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                crm_state = "第四次重连结束";
                ItemFlowIndex = 6;
                CountDownTimeInfo("请给桩刷卡上电，刷卡后点击确认按钮，或者等待倒计时结束。\r\n（注:刷卡前请勿拔插枪。合格桩此处应【不允许充电】）", 50, 2);
                Thread.Sleep(2000);
                ProcessData();
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            }
        }

        private void TestFlow(int time)
        {
            SendNoticeToUIAndTxtFile($"开始第{ItemFlowIndex - 1}次超时重连。");
            SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
            //Thread.Sleep(5000);
            SBitS[31] = true;//停止发送报文
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            Thread.Sleep(3000);
            if (ItemFlowIndex >= 2 && ItemFlowIndex <= 4)
            {
                SendNoticeToUIAndTxtFile("发送恢复通讯指令");
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
            }
            SendNoticeToUIAndTxtFile($"等待{time}秒恢复充电");
            Thread.Sleep(1000 * time);
            ProcessData();
            //Thread.Sleep(1000);

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

    }
}
