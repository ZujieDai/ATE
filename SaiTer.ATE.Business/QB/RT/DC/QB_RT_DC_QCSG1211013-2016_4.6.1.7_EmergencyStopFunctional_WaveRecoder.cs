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
    /// 企标研测直流：急停功能试验
    /// </summary>
    public class QB_RT_DC_EmergencyStopFunctional_WaveRecoder : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间

        public QB_RT_DC_EmergencyStopFunctional_WaveRecoder(int type) { TrialType = type; }


        public override void InitEquiMent()
        {
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
        }

        public override void InitializeParams()
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
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
                SetCPReresh();
                List<bool> Ks = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
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

                #region 启动急停开关

                CountDownTimeInfo("请检查充电机应安装急停装置，且具备防止误操作的防护措施。\r\n（勾选枪号为Pass）", 60, 2);
                ProcessDataConnect("充电机应安装急停装置和防止误操作的防护措施", "是否具备");
                if (!DicManualVerifyResult.First().Value)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("启动充电");

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage.ToString("F2"));
                }
                ProcessDataTmp(d1, "充电过程", "输出电压(V)", "-", "-");

                //设置测试条件
                SetConditionValues();

                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(500);


                string info1 = "", info2 = "", info3 = "", sState = "";
                info1 = "请按下急停按钮";
                info2 = "请按下充电桩急停按钮,然后点击确认或倒计时结束后自动判断";
                info3 = "请恢复急停按钮";
                sState = "急停功能试验";

                SendNoticeToUIAndTxtFile(info1);
                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(5000);

                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(500);

                //读取录波板数据
                double Time_EStop = 0;
                double Time_K1K2 = 0;
                WaveData CH_EStop = new WaveData();
                WaveData CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 7, ref CH_EStop, "EStop");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, true, 5, ref Time_EStop);
                double EStop_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_EStop, 5);
                if (EStop_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, false, 6, ref Time_EStop);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_EStop, true, 6, ref Time_EStop);
                }
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_EStop);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                double Time_Stop = Math.Abs(Time_EStop - Time_K1K2);

                //读取卡点时间
                Dictionary<int, double> dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "启动急停信号", "K1K2断开时间(ms)", "0", trlTimeOut_S.ToString(), dImgs);

                //南网增加电压下降时间  1000ms
                WaveData CH_OutputVoltage = new WaveData();
                double Time_OutputVoltage = 0;
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputVoltage, false, 60, ref Time_OutputVoltage);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_EStop);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputVoltage);//设置光标2
                Time_Stop = Math.Abs(Time_EStop - Time_OutputVoltage);
                //读取卡点时间
                dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "启动急停信号", "输出电压下降时间(ms)", "0", "1000", dImgs);


                Dictionary<int, string> dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    dicData.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                }
                ProcessDataTmp(dicData, sState, "直流输出电压值(V)", "0", "20");

                //CountDownTimeInfo("请人工判断充电机动力电源是否断开。\r\n（勾选枪号为Pass）", 60, 2);

                if (Customer.Equals("XJ"))
                {
                    CountDownTimeInfo("请人工判断充电机动力电源是否断开。\r\n（勾选枪号为Pass）", 30, 2);
                }
                else
                {
                    CountDownTimeInfo("请人工判断充电机动力电源是否断开。\r\n（勾选枪号为Pass）", 60, 2);
                }
                ProcessData();

                CountDownTimeInfo(info3, 60, 0);

                #endregion

                #region 通信故障
                SetCPReresh();

                //启动示波器
                //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                if (!CheckSwipingCard(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 20))
                {
                    return;
                }

                //这里要检测电压下降时间，不能用回馈负载
                if (ControlEquipMent.FeedbackLoad == null
                    && ControlEquipMent.StarLoopFeedbackLoad == null
                    && ControlEquipMent.LoopFeedbackLoad == null)
                {
                    SendNoticeToUIAndTxtFile("带载中...");
                    SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, 25, LstChargerInfo[0].NominalVoltage, 20);
                    Thread.Sleep(2000);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent(testWorkParam.lstIDs, 20);
                }
                Thread.Sleep(3000);



                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                //模拟不发送报文
                List<bool> Ks = GetKStatus16_Charging_DC();
                Ks[31] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                Thread.Sleep(1000);//等待发送报文后直接断开CAN
                Ks[20] = false;
                Ks[21] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                Thread.Sleep(5000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(3000);


                //读取录波板数据
                double Time_CANSingle = 0;
                WaveData CH_CANSingle = new WaveData();
                CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 37, 2, ref CH_CANSingle, "CANSingle");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CANSingle, true, 0.5, ref Time_CANSingle);
                K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CANSingle);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                Time_Stop = Math.Abs(Time_CANSingle - Time_K1K2);


                //读取卡点时间
                dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "通信故障", "K1K2断开时间(ms)", "0", trlTimeOut_S.ToString(), dImgs);



                //南网增加电压下降时间  1000ms
                CH_OutputVoltage = new WaveData();
                Time_OutputVoltage = 0;
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputVoltage, false, 60, ref Time_OutputVoltage);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CANSingle);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputVoltage);//设置光标2
                Time_Stop = Math.Abs(Time_CANSingle - Time_OutputVoltage);
                //读取卡点时间
                dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "通信故障", "输出电压下降时间(ms)", "0", "1000", dImgs);

                //断线后的电压
                Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "通信故障", "通信故障后输出电压(V)", "0", "20");

                Ks[31] = false;
                Ks[20] = true;
                Ks[21] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                SetLoadDCOFF(testWorkParam.lstIDs);

                #endregion


                #region 控制导引故障（CC1断线）
                SetCPReresh();

                if (!CheckSwipingCard(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 20))
                {
                    return;
                }


                SendNoticeToUIAndTxtFile("带载中...");
                SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, 25, LstChargerInfo[0].NominalVoltage, 20);
                Thread.Sleep(2000);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, 20);
                Thread.Sleep(3000);



                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                //模拟CC1断线
                SendNoticeToUIAndTxtFile("模拟控制导引故障...");
                Ks = GetKStatus16_Charging_DC();

                Ks[22] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                Thread.Sleep(5000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);


                SendNoticeToUIAndTxtFile("计算K1K2断开时间...");
                //读取录波板数据
                double Time_CC1 = 0;
                Time_K1K2 = 0;
                WaveData CH_CC1 = new WaveData();
                CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CC1, true, 5, ref Time_CC1);
                K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                Time_Stop = Math.Abs(Time_CC1 - Time_K1K2);


                //读取卡点时间
                dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "连接检测信号断开", "K1K2断开时间(ms)", "0", trlTimeOut_S.ToString(), dImgs);

                SendNoticeToUIAndTxtFile("计算输出电压下降时间...");
                //南网增加电压下降时间  1000ms
                CH_OutputVoltage = new WaveData();
                Time_OutputVoltage = 0;
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputVoltage, false, 60, ref Time_OutputVoltage);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputVoltage);//设置光标2
                Time_Stop = Math.Abs(Time_CC1 - Time_OutputVoltage);
                //读取卡点时间
                dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dd, "连接检测信号断开", "输出电压下降时间(ms)", "0", "1000", dImgs);

                //断线后的电压
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "连接检测信号断开", "断线后输出电压(V)", "0", "20");

                Ks[22] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                SetLoadDCOFF(testWorkParam.lstIDs);

                #endregion
            }

        }
        public override void ProcessData()
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

                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = "急停功能试验|动力电源是否断开|-|-|是|报表(勿删)";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = "急停功能试验|动力电源是否断开|-|-|否|报表(勿删)";
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
