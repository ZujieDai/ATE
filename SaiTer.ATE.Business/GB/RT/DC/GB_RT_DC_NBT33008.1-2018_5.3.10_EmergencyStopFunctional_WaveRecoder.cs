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
    /// 国标研测直流：急停功能（录波板）
    /// </summary>
    public class GB_RT_DC_EmergencyStopFunctional_WaveRecoder : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间

        public GB_RT_DC_EmergencyStopFunctional_WaveRecoder(int type) { TrialType = type; }


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

                //CountDownTimeInfo("请检查充电机应安装急停装置，且具备防止误操作的防护措施。\r\n（勾选枪号为Pass）", 60, 2);
                if (Customer.Equals("XJ"))
                {
                    CountDownTimeInfo("请检查充电机应安装急停装置，且具备防止误操作的防护措施。\r\n（勾选枪号为Pass）", 30, 2);
                }
                else
                {
                    CountDownTimeInfo("请检查充电机应安装急停装置，且具备防止误操作的防护措施。\r\n（勾选枪号为Pass）", 60, 2);
                }
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
