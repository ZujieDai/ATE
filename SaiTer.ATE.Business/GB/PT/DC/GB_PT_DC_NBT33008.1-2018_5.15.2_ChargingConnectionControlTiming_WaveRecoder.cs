using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.GB.PT.DC
{
    /// <summary>
    /// 充电连接控制时序(录波板)
    /// </summary>
    public class GB_PT_DC_ChargingConnectionControlTiming_WaveRecoder : BusinessBase
    {
        public GB_PT_DC_ChargingConnectionControlTiming_WaveRecoder(int trialType) { TrialType = trialType; }

        private double DemandVoltage = 500;
        private double DemandCurrent = 100;
        Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
        public override void InitializeParams()
        {
            Init();

            dicImagePath = new Dictionary<int, string>();
            //DemandVoltage = LstChargerInfo.First().NominalVoltage;
            //BMS需求电压(V)=500|BMS需求电流(A)=100
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
        }
        public override void InitEquiMent()
        {
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
                // SetCPReresh();
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
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                //InitOscillograph();
                //SendNoticeToUIAndTxtFile("等待录波仪滚动");

                SendNoticeToUIAndTxtFile("录波板启动录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(3 * 1000);
                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                MessgaeInfo(true, "请刷卡充电!", true);
                int timeout = 200;
                while (timeout-- > 0)
                {
                    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                        continue;
                    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                    if (ALLCanCharge)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
                MessgaeInfo(false, "请刷卡充电!");
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);

                SetConditionValues();

                Dictionary<int, string> dicCC1Value = new Dictionary<int, string>();
                Dictionary<int, string> dicCC2Value = new Dictionary<int, string>();
                string CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                string CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");
                foreach (var item in testWorkParam.lstIDs)
                {
                    dicCC1Value.Add(item, CC1Value1);
                    dicCC2Value.Add(item, CC2Value1);
                }

                int loadTime = 15;
                SendNoticeToUIAndTxtFile($"启动负载,并带载{loadTime}秒");
                SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 10, DemandCurrent);
                Thread.Sleep(300);
                SetLoadDCON(testWorkParam.lstIDs);
                Thread.Sleep(loadTime * 1000);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                SendNoticeToUIAndTxtFile("关闭负载,停止充电...");
                SetLoadDCOFF(testWorkParam.lstIDs);

                //检测辅源是否断开
                if (TrialType == (int)EmTrialType.充电连接控制时序研发B6)
                {
                    double K3K4Value;
                    timeout = 100;
                    while (timeout-- > 0)
                    {
                        K3K4Value = System.Math.Abs(AllEquipStateData.DicBMS_DC_StateData.First().Value.APSVoltage);
                        if (timeout-- <= 0)
                        {
                            break;
                        }
                        if (K3K4Value <= 1)
                        {
                            Thread.Sleep(6500);
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(5000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);


                //读取录波板数据
                SendNoticeToUIAndTxtFile("读取录波板数据...");
                WaveData CH_OutputVoltage = new WaveData();
                WaveData CH_OutputCurrent = new WaveData();
                WaveData CH_APSVoltage = new WaveData();
                WaveData CH_CC1 = new WaveData();
                WaveData CH_CC2 = new WaveData();
                WaveData CH_FrontEndVoltage = new WaveData();
                WaveData CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref CH_APSVoltage, "APSVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1Voltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 5, ref CH_CC2, "CC2Voltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 6, ref CH_FrontEndVoltage, "FrontEndVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");


                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                ProcessDataTmp(dicCC1Value, "充电状态流程", "CC1电压", "3.7", "4.3", dicImagePath);
                ProcessDataTmp(dicCC2Value, "充电状态流程", "CC2电压", "5.7", "6.3");

                //这个CAN报文数据先放在后面，波形太多不好显示
                WaveData CH_CHM = new WaveData();
                WaveData CH_BHMMaxVoltage = new WaveData();
                WaveData CH_CRMState = new WaveData();
                WaveData CH_BCP = new WaveData();
                WaveData CH_CTS = new WaveData();
                WaveData CH_CML = new WaveData();
                WaveData CH_BRO = new WaveData();
                WaveData CH_CRO = new WaveData();
                WaveData CH_BCS = new WaveData();
                WaveData CH_BCL = new WaveData();
                WaveData CH_CCS = new WaveData();
                WaveData CH_BST = new WaveData();
                WaveData CH_CST = new WaveData();
                WaveData CH_BSD = new WaveData();
                WaveData CH_CSD = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 1, 1, ref CH_CHM, "CHM");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 2, 1, ref CH_BHMMaxVoltage, "BHMMaxVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 3, 1, ref CH_CRMState, "CRMState");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 20, 1, ref CH_BCP, "BCP");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 24, 1, ref CH_CTS, "CTS");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 25, 1, ref CH_CML, "CML");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 29, 1, ref CH_BRO, "BRO");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 30, 1, ref CH_CRO, "CRO");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 132, 1, ref CH_BCS, "BCS");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 129, 1, ref CH_BCL, "BCL");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 137, 1, ref CH_CCS, "CCS");
                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 39, 0, ref CH_BST, "BST");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 42, 0, ref CH_CST, "CST");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 45, 0, ref CH_BSD, "BSD");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 52, 0, ref CH_CSD, "CSD");
                SendNoticeToUIAndTxtFile("读取录波板数据完成");


                string sState = "充电连接控制过程";
                ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
                Dictionary<int, string> dic = new Dictionary<int, string>();
                SendNoticeToUIAndTxtFile("读取辅源首个6V上升沿时间点中...");
                double K3K4Time1 = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_APSVoltage, true, 6, ref K3K4Time1);
                dic.Add(1, K3K4Time1.ToString());
                //ProcessDataTmp(dic, "开辅源K3K4", "...", "-", "-");
                ProcessDataTmp(dic, sState, "辅源首个6V上升沿时间点", "-", "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "开辅源K3K4");


                SendNoticeToUIAndTxtFile("读取CHM桩握手首个电平上升沿时间点中...");
                double CHMTime1 = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CHM, true, 6, ref CHMTime1);
                dic[1] = CHMTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CHM--->", K3K4Time1.ToString(), "-");
                ProcessDataTmp(dic, sState, "CHM桩握手首个电平上升沿时间点", K3K4Time1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CHM--->", true, "...");



                SendNoticeToUIAndTxtFile("读取BHM车辆握手电压首个电平100上升沿时间点中...");
                double BHMTime1 = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BHMMaxVoltage, true, 100, ref BHMTime1);
                dic[1] = BHMTime1.ToString();
                //ProcessDataTmp(dic, "...", "<---BHM---", CHMTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "BHM车辆握手电压首个电平100上升沿时间点", CHMTime1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BHM---", true, "...");

                SendNoticeToUIAndTxtFile("读取K1K2前端高压，首个60V下降沿时间点中...");
                double K1K2Time1 = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, false, 60, ref K1K2Time1, BHMTime1);
                dic[1] = K1K2Time1.ToString();
                //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "首个泄放电压时间点(K1K2前端高压)", BHMTime1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "泄放电压");


                SendNoticeToUIAndTxtFile("读取K1K2-sig 首个6V上升沿时间点中...");
                double K1K2SigTime1 = 0;
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref K1K2SigTime1);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref K1K2SigTime1);
                }
                dic[1] = K1K2SigTime1.ToString();
                //ProcessDataTmp(dic, "闭合K1K2绝缘检测ok后断开K1K2", "...", BHMTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "K1K2绝缘检测ok后断开时间点", K1K2Time1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "闭合K1K2绝缘检测ok后断开K1K2");


                dic[1] = " ";
                ProcessDataTmp(dic, sState, "---CRM-00--->", "-", "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-00--->", true, "...");

                ////BRM报文不分析
                //    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=2时间点中...");
                //    double PGNTime1 = GetTriggerTime_Single3(15, true, 11, 2, false) * 10;
                //PGNTime1 = K1K2SigTime1 + 10;
                //    dic[1] = PGNTime1.ToString();
                //    //ProcessDataTmp(dic, "...", "<---BRM---", K1K2SigTime1.ToString(), "-");
                //    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=2时间点", K1K2SigTime1.ToString(), "-");
                //    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRM---", true, "...");



                SendNoticeToUIAndTxtFile("读取CRM充电机辨识首个电平1上升沿时间点中...");
                double CRMTime1 = 0;
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CRMState, true, 100, ref CRMTime1);
                dic[1] = CRMTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CRM-AA--->", PGNTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "CRM-AA 充电机辨识首个电平上升沿时间点", K1K2Time1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-AA--->", true, "...");


                SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=6时间点中...");
                double PGNTime2 = 0;// GetTriggerTime_Single3(15, true, 11, 6, false) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BCP, true, 100, ref PGNTime2);
                dic[1] = PGNTime2.ToString();
                //ProcessDataTmp(dic, "...", "<---BCP---", CRMTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "多包标识报文PGN首个=6时间点(BCP)", CRMTime1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BCP---", true, "...");

                ////CTS没有数据，暂不做
                //SendNoticeToUIAndTxtFile("读取CTS桩时间年首个电平1上升沿时间点中");
                //double CTSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 19, 1, 0, false, false, 0.05) * 10;
                //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CTS, true, 1, ref CTSTime1, PGNTime2);
                //dic[1] = CTSTime1.ToString();
                ////ProcessDataTmp(dic, "...", "---CTS--->", PGNTime2.ToString(), "-");
                //ProcessDataTmp(dic, sState, "CTS桩时间年首个电平上升沿时间点", PGNTime2.ToString(), "-");
                ////ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CTS--->", true, "...");


                SendNoticeToUIAndTxtFile("读取CML电压首个200上升沿时间点中");
                double CMLTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 20, 200, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CML, true, 10, ref CMLTime1, PGNTime2);
                dic[1] = CMLTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CML--->", CTSTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "CML电压首个200上升沿时间点", PGNTime2.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CML--->", true, "...");


                SendNoticeToUIAndTxtFile("读取直流高压第二个150V电平上升沿时间点中");
                double VoltageTime1 = 0;// OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 0, false, false, 0.05, 2) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, true, 150, ref VoltageTime1, PGNTime2);
                dic[1] = VoltageTime1.ToString();
                //ProcessDataTmp(dic, "...", "闭合K5K6", CMLTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "闭合K5K6时间点", CMLTime1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "闭合K5K6", true, "...");
                dic[1] = " ";
                //ProcessDataTmp(dic, "...", "绝缘监视", "-", "-");
                ProcessDataTmp(dic, sState, "绝缘监视", "-", "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "绝缘监视", true, "...");


                SendNoticeToUIAndTxtFile("读取BRO电池准备就绪首个电平1上升沿时间点中...");
                double BROTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 12, 1, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BRO, true, 150, ref BROTime1, CMLTime1);
                dic[1] = BROTime1.ToString();
                //ProcessDataTmp(dic, "...", "<---BRO-AA---", VoltageTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "BRO电池准备就绪首个电平上升沿时间点", VoltageTime1.ToString(), "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRO-AA---", true, "...");


                dic[1] = " ";
                ProcessDataTmp(dic, sState, "---CRO-00--->", "-", "-");
                //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRO-00--->", true, "...");



                SendNoticeToUIAndTxtFile("读取K1K2-sig第二个6V上升沿时间点中...");
                double K1K2SigTime2 = 0;
                K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, false, 6, ref K1K2SigTime2, VoltageTime1);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, true, 6, ref K1K2SigTime2, VoltageTime1);
                }
                dic[1] = K1K2SigTime2.ToString();
                //ProcessDataTmp(dic, "预充后闭合K1K2", "...", BROTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "K1K2-sig预充后闭合时间点", BROTime1.ToString(), "-");



                SendNoticeToUIAndTxtFile("读取CRO充电机准备就绪首个电平1上升沿时间点中...");
                double CROTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 13, 1, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CRO, true, 150, ref CROTime1, K1K2SigTime2);
                dic[1] = CROTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CRO-AA--->", K1K2SigTime2.ToString(), "-");
                ProcessDataTmp(dic, sState, "CRO充电机准备就绪首个电平上升沿时间点", K1K2SigTime2.ToString(), "-");



                SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=17时间点中...");
                double PGNTime3 = 0;// GetTriggerTime_Single3(15, true, 11, 17, false) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CRO, true, 150, ref PGNTime3, CROTime1);
                dic[1] = PGNTime3.ToString();
                //ProcessDataTmp(dic, "...", "<---BCS---", CROTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "多包标识报文PGN首个=17时间点(BCS)", CROTime1.ToString(), "-");


                SendNoticeToUIAndTxtFile("读取BCL电流需求首个电平上升沿时间点中...");
                double BCLTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 1, -250, 1, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BCL, true, 50, ref BCLTime1, CROTime1);
                dic[1] = BCLTime1.ToString();
                //ProcessDataTmp(dic, "...", "<---BCL---", PGNTime3.ToString(), "-");
                ProcessDataTmp(dic, sState, "BCL电流时间点", CROTime1.ToString(), "-");


                dic[1] = " ";
                ProcessDataTmp(dic, sState, "调节电流", "-", "-");


                SendNoticeToUIAndTxtFile("读取CCS桩充电电流首个电平上升沿时间点中...");
                double CCSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 2, -100, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CCS, true, 50, ref CCSTime1, CROTime1);
                dic[1] = CCSTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CCS--->", BCLTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "CCS桩充电电流首个电平上升沿时间点", BCLTime1.ToString(), "-");


                SendNoticeToUIAndTxtFile("读取BMS停充首个电平1下降沿时间点中...");
                double BMSTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 3, 1, 1, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CST, true, 1, ref BMSTime1, CCSTime1);
                dic[1] = BMSTime1.ToString();
                //ProcessDataTmp(dic, "...", "<---BST/CST-->", CCSTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "BMS停充首个电平1下降沿时间点(BST/CST)", CCSTime1.ToString(), "-");



                SendNoticeToUIAndTxtFile("读取BSD结束中止电量首个电平1上升沿时间点中...");
                double BSDTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 7, 1, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BSD, true, 1, ref BSDTime1, BMSTime1);
                dic[1] = BSDTime1.ToString();
                //ProcessDataTmp(dic, "...", "<---BSD---", BMSTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "BSD结束中止电量首个电平上升沿时间点", BMSTime1.ToString(), "-");


                SendNoticeToUIAndTxtFile("读取CSD结束统计电能首个电平1上升沿时间点中...");
                double CSDTime1 = 0;// GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_CSD, true, 1, ref CSDTime1, BSDTime1);
                dic[1] = CSDTime1.ToString();
                //ProcessDataTmp(dic, "...", "---CSD--->", BSDTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "CSD结束统计电能首个电平上升沿时间点", BSDTime1.ToString(), "-");


                SendNoticeToUIAndTxtFile("读取K1K2-sig第二个6V下降沿时间点中...");
                double K1K2SigTime3 = 0;
                K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, false, 6, ref K1K2SigTime3, CSDTime1);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2, true, 6, ref K1K2SigTime3, CSDTime1);
                }

                dic[1] = K1K2SigTime3.ToString();
                //ProcessDataTmp(dic, "断开K1/K2", "...", CSDTime1.ToString(), "-");
                ProcessDataTmp(dic, sState, "K1K2-sig第二个开关断开时间点", CSDTime1.ToString(), "-");




                SendNoticeToUIAndTxtFile("读取高压第二个100V下降沿时间点中...");
                //double VoltageTime2 = OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 1, false, false, 0.05, 2) * 10;
                double VoltageTime2 = 0;// GetTriggerTime_Single2_NoCursor(4, false, 0, 150, 1, false, false, 0.05, true) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_OutputVoltage, false, 60, ref VoltageTime2, CSDTime1);
                dic[1] = VoltageTime2.ToString();
                //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime3.ToString(), "-");
                ProcessDataTmp(dic, sState, "第二个泄放电压时间点", K1K2SigTime3.ToString(), "-");


                SendNoticeToUIAndTxtFile("读取辅源6V下降沿时间点中...");
                double K3K4Time2 = 0; //GetTriggerTime_Single2_NoCursor(2, false, 0, 9, 1, false, false, 0.05, true) * 10;
                DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_APSVoltage, false, 6, ref K3K4Time2, VoltageTime2);
                dic[1] = K3K4Time2.ToString();
                //ProcessDataTmp(dic, "断开K3/K4", "...", VoltageTime2.ToString(), "-");
                ProcessDataTmp(dic, sState, "辅源6V下降沿时间点(断开K3/K4)", VoltageTime2.ToString(), "-");



                CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
                dic.Clear();
                foreach (var item in DicManualVerifyResult)
                {
                    dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                }
                ProcessDataTmp(dic, "正常充电", "应可靠锁止", "-", "-");




                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);

            }
        }
        public override void ProcessData()
        {

        }




    }
}
