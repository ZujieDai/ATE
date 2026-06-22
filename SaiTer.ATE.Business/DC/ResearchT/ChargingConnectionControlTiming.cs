using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Configuration;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电连接控制时序  包括B.5流程测试  和  B.6时序测试  _国标研测
    /// </summary>
    public class ChargingConnectionControlTiming : BusinessBase
    {
        public ChargingConnectionControlTiming(int trialType) { TrialType = trialType; }

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
                if (ControlEquipMent.ResistanceLoad == null)
                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 4, 4, true, "DC", "5000", "5000", Channel1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4
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

                //先设置负载参数和并机，减少充电过程时间
                var list = GetKStatus16_Charging_DC();
                list[3 + 16] = true;//DC+、DC-
                list[2 + 16] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, list.ToArray());
                Thread.Sleep(300);
                SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 10, DemandCurrent);
                Thread.Sleep(300);
                SendNoticeToUIAndTxtFile($"启动负载");
                SetLoadDCON(testWorkParam.lstIDs);

                SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                InitOscillograph();
                SendNoticeToUIAndTxtFile("等待录波仪滚动");
                Thread.Sleep(3 * 1000);
                if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                {
                    return;
                }
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                //Thread.Sleep(200);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                //Thread.Sleep(200);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, DemandVoltage);
                //Thread.Sleep(200);
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //MessgaeInfo(true, "请刷卡充电!", true);
                //int timeout = 200;
                //while (timeout-- > 0)
                //{
                //    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                //    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                //        continue;
                //    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                //    if (ALLCanCharge)
                //    {
                //        break;
                //    }

                //    System.Threading.Thread.Sleep(1000);
                //}
                //MessgaeInfo(false, "请刷卡充电!");
                ////ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);

                SetConditionValues();

                int loadTime = 11;
                SendNoticeToUIAndTxtFile($"带载{loadTime}秒");
                SetLoadDCON(testWorkParam.lstIDs);
                Thread.Sleep(loadTime * 1000);

                SendNoticeToUIAndTxtFile("读取CC1和CC2电压中...");
                Dictionary<int, string> dicCC1Value = new Dictionary<int, string>();
                int timeout = 30;
                while (timeout-- > 0)
                {
                    dicCC1Value = ControlEquipMent.Oscillograph.GetChannelValue(3, false, 0);
                    if (dicCC1Value[testWorkParam.lstIDs[0]] != "0")
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                Dictionary<int, string> dicCC2Value = new Dictionary<int, string>();
                timeout = 30;
                while (timeout-- > 0)
                {
                    dicCC2Value = ControlEquipMent.Oscillograph.GetChannelValue(5, false, 0);
                    if (dicCC2Value[testWorkParam.lstIDs[0]] != "0")
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                //dicCC1Value = ControlEquipMent.Oscillograph.GetChannelValue(3, false, 0);
                //dicCC2Value = ControlEquipMent.Oscillograph.GetChannelValue(5, false, 0);

                //可能导致充电时间过长，时序图不完整
                if (Customer != null && Customer.Equals("DH"))
                {
                    CountDownTimeInfo("请确认电子锁能可靠锁止 \r\n注:勾选上为可靠锁止", 5, 2);
                    foreach (var item in DicManualVerifyResult)
                    {
                        if (item.Value)
                            ProcessDataResult(testWorkParam.lstIDs, "可靠锁止", "应可靠锁止", item.Value, "正常充电");
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "未锁止", "应可靠锁止", item.Value, "正常充电");
                    }
                }

                SendNoticeToUIAndTxtFile("关闭负载,停止充电...");
                //SetLoadDCOFF(testWorkParam.lstIDs);
                //环形回馈载关闭导引太慢，影响时序图
                SetLoadDCOFF(testWorkParam.lstIDs, true, true);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                //ControlEquipMent.Oscillograph.Oscillograph_IsRun(false);
                //Thread.Sleep(5 * 1000);
                //等待负载电流下降
                WaitDCLoadOFF(lstIDs, 5);
                Thread.Sleep(2000);

                // ??这一段是什么意思?
                if (TrialType == (int)EmTrialType.充电连接控制时序研发B6)
                {
                    double K3K4Value;
                    timeout = 100;
                    while (timeout-- > 0)
                    {
                        K3K4Value = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(2, false, 0)));
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
                dicImagePath = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                ProcessDataTmp(dicCC1Value, "充电状态流程", "CC1电压", "3.7", "4.3", dicImagePath);
                ProcessDataTmp(dicCC2Value, "充电状态流程", "CC2电压", "5.7", "6.3");


                if (TrialType == (int)EmTrialType.充电连接控制时序研发B6)
                {
                    string sState = "充电连接控制过程";
                    ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    SendNoticeToUIAndTxtFile("读取辅源首个6V上升沿时间点中...");
                    double K3K4Time1 = GetTriggerTime_Single2_NoCursor(2, false, 0, 6, 0, false, false, 0.05) * 10;
                    dic.Add(1, (K3K4Time1 > 80 ? 0 : K3K4Time1).ToString("F1"));
                    //ProcessDataTmp(dic, "开辅源K3K4", "...", "-", "-");
                    ProcessDataTmp(dic, sState, "辅源首个6V上升沿时间点", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "开辅源K3K4");


                    SendNoticeToUIAndTxtFile("读取CHM桩握手首个电平上升沿时间点中...");
                    double CHMTime1 = GetTriggerTime_Single2_NoCursor(15, true, 17, 1, 0, false, false, 0.05) * 10;
                    CHMTime1 = CHMTime1 > 80 ? 0 : CHMTime1;
                    dic[1] = CHMTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CHM--->", K3K4Time1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CHM桩握手首个电平上升沿时间点", K3K4Time1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CHM--->", true, "...");



                    SendNoticeToUIAndTxtFile("读取BHM车辆握手电压首个电平100上升沿时间点中...");
                    double BHMTime1 = GetTriggerTime_Single2_NoCursor(15, true, 9, 100, 0, false, false, 0.05) * 10;
                    BHMTime1 = BHMTime1 > 80 ? 0 : BHMTime1;
                    dic[1] = BHMTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BHM---", CHMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BHM车辆握手电压首个电平100上升沿时间点", CHMTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BHM---", true, "...");

                    SendNoticeToUIAndTxtFile("读取K1K2前端高压，首个200V下降沿时间点中...");
                    double K1K2Time1 = GetTriggerTime_Single2_NoCursor(4, false, 0, 400, 1, false, false, 0.05) * 10;
                    K1K2Time1 = K1K2Time1 > 80 ? 0 : K1K2Time1;
                    dic[1] = K1K2Time1.ToString("F1");
                    //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "首个泄放电压时间点(K1K2前端高压)", BHMTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "泄放电压");


                    SendNoticeToUIAndTxtFile("读取K1K2-sig 首个6V上升沿时间点中...");
                    double K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    double K1K2SigTime1 = 0;
                    if (K1K2s2 > 2)
                    {
                        K1K2SigTime1 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 0, false, false, 0.05) * 10;
                    }
                    else
                    {
                        K1K2SigTime1 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 1, false, false, 0.05) * 10;
                    }
                    K1K2SigTime1 = K1K2SigTime1 > 80 ? 0 : K1K2SigTime1;
                    dic[1] = K1K2SigTime1.ToString("F1");
                    //ProcessDataTmp(dic, "闭合K1K2绝缘检测ok后断开K1K2", "...", BHMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2绝缘检测ok后断开时间点", K1K2Time1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "...", true, "闭合K1K2绝缘检测ok后断开K1K2");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "---CRM-00--->", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-00--->", true, "...");

                    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=2时间点中...");
                    double PGNTime1 = GetTriggerTime_Single3(15, true, 11, 2, false) * 10;
                    dic[1] = PGNTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BRM---", K1K2SigTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=2时间点(BRM)", K1K2SigTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRM---", true, "...");



                    SendNoticeToUIAndTxtFile("读取CRM充电机辨识首个电平1上升沿时间点中...");
                    double CRMTime1 = GetTriggerTime_Single2_NoCursor(15, true, 18, 1, 0, false, false, 0.05) * 10;
                    dic[1] = CRMTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CRM-AA--->", PGNTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CRM-AA 充电机辨识首个电平上升沿时间点", PGNTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRM-AA--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=6时间点中...");
                    double PGNTime2 = GetTriggerTime_Single3(15, true, 11, 6, false) * 10;
                    dic[1] = PGNTime2.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BCP---", CRMTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=6时间点(BCP)", CRMTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BCP---", true, "...");


                    SendNoticeToUIAndTxtFile("读取CTS桩时间年首个电平1上升沿时间点中");
                    double CTSTime1 = GetTriggerTime_Single2_NoCursor(15, true, 19, 1, 0, false, false, 0.05) * 10;
                    dic[1] = CTSTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CTS--->", PGNTime2.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CTS桩时间年首个电平上升沿时间点", PGNTime2.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CTS--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取CML电压首个200上升沿时间点中");
                    double CMLTime1 = GetTriggerTime_Single2_NoCursor(15, true, 20, 200, 0, false, false, 0.05) * 10;
                    dic[1] = CMLTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CML--->", CTSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CML电压首个200上升沿时间点", CTSTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CML--->", true, "...");


                    SendNoticeToUIAndTxtFile("读取直流高压第二个150V电平上升沿时间点中");
                    //double VoltageTime1 = OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 0, false, false, 0.05, 2) * 10;
                    //double VoltageTime1 = GetTriggerTime_Single2_NoCursor(4, false, 0, 150, 0, false, false, 0.05, true, 30) * 10;
                    double VoltageTime1 = OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 0, false, true, 0.05, 2, 30) * 10;
                    dic[1] = VoltageTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "闭合K5K6", CMLTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "闭合K5K6时间点", CMLTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "闭合K5K6", true, "...");
                    dic[1] = " ";
                    //ProcessDataTmp(dic, "...", "绝缘监视", "-", "-");
                    ProcessDataTmp(dic, sState, "绝缘监视", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "绝缘监视", true, "...");


                    SendNoticeToUIAndTxtFile("读取BRO电池准备就绪首个电平1上升沿时间点中...");
                    double BROTime1 = GetTriggerTime_Single2_NoCursor(15, true, 12, 1, 0, false, false, 0.05) * 10;
                    dic[1] = BROTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BRO-AA---", VoltageTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BRO电池准备就绪首个电平上升沿时间点", VoltageTime1.ToString("F1"), "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "<---BRO-AA---", true, "...");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "---CRO-00--->", "-", "-");
                    //ProcessDataResult(testWorkParam.lstIDs, dic[testWorkParam.lstIDs[0]], "---CRO-00--->", true, "...");



                    SendNoticeToUIAndTxtFile("读取K1K2-sig第二个6V上升沿时间点中...");
                    K1K2s2 = Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    double K1K2SigTime2 = 0;
                    if (K1K2s2 > 2)
                    {
                        K1K2SigTime2 = OscillographInstrument_Points_Single_Multiple2(7, false, 0, 6, 1, false, false, 0.05, 2) * 10;
                    }
                    else
                    {
                        K1K2SigTime2 = OscillographInstrument_Points_Single_Multiple2(7, false, 0, 6, 0, false, false, 0.05, 2) * 10;
                    }
                    dic[1] = K1K2SigTime2.ToString("F1");
                    //ProcessDataTmp(dic, "预充后闭合K1K2", "...", BROTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2-sig预充后闭合时间点", BROTime1.ToString("F1"), "-");



                    SendNoticeToUIAndTxtFile("读取CRO充电机准备就绪首个电平1上升沿时间点中...");
                    double CROTime1 = GetTriggerTime_Single2_NoCursor(15, true, 13, 1, 0, false, false, 0.05) * 10;
                    dic[1] = CROTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CRO-AA--->", K1K2SigTime2.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CRO充电机准备就绪首个电平上升沿时间点", K1K2SigTime2.ToString("F1"), "-");



                    SendNoticeToUIAndTxtFile("读取多包标识报文PGN首个=17时间点中...");
                    double PGNTime3 = GetTriggerTime_Single3(15, true, 11, 17, false) * 10;
                    dic[1] = PGNTime3.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BCS---", CROTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "多包标识报文PGN首个=17时间点(BCS)", CROTime1.ToString("F1"), "-");


                    SendNoticeToUIAndTxtFile("读取BCL电流需求首个电平上升沿时间点中...");
                    //double BCLTime1 = GetTriggerTime_Single2_NoCursor(15, true, 1, -100, 1, false, false, 0.05) * 10;
                    double BCLTime1 = GetTriggerTime_Single2_NoCursor(15, true, 1, -250, 1, false, false, 0.05) * 10;
                    dic[1] = BCLTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BCL---", PGNTime3.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BCL电流时间点", PGNTime3.ToString("F1"), "-");


                    dic[1] = " ";
                    ProcessDataTmp(dic, sState, "调节电流", "-", "-");


                    SendNoticeToUIAndTxtFile("读取CCS桩充电电流首个电平上升沿时间点中...");
                    double CCSTime1 = GetTriggerTime_Single2_NoCursor(15, true, 2, -100, 0, false, false, 0.05) * 10;
                    dic[1] = CCSTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CCS--->", BCLTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CCS桩充电电流首个电平上升沿时间点", BCLTime1.ToString("F1"), "-");


                    SendNoticeToUIAndTxtFile("读取BMS停充首个电平1下降沿时间点中...");
                    double BMSTime1 = GetTriggerTime_Single2_NoCursor(15, true, 3, 1, 0, false, false, 0.05, false, 30) * 10;
                    dic[1] = BMSTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BST/CST-->", CCSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BMS停充首个电平1下降沿时间点(BST/CST)", CCSTime1.ToString("F1"), "-");



                    SendNoticeToUIAndTxtFile("读取BSD结束中止电量首个电平1上升沿时间点中...");
                    double BSDTime1 = GetTriggerTime_Single2_NoCursor(15, true, 7, 1, 0, false, false, 0.05, false, 30) * 10;
                    dic[1] = BSDTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "<---BSD---", BMSTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "BSD结束中止电量首个电平上升沿时间点", BMSTime1.ToString("F1"), "-");


                    SendNoticeToUIAndTxtFile("读取CSD结束统计电能首个电平1上升沿时间点中...");
                    double CSDTime1 = GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05, false, 30) * 10;
                    dic[1] = CSDTime1.ToString("F1");
                    //ProcessDataTmp(dic, "...", "---CSD--->", BSDTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "CSD结束统计电能首个电平上升沿时间点", BSDTime1.ToString("F1"), "-");


                    SendNoticeToUIAndTxtFile("读取K1K2-sig第二个开关断开(6V)时间点中...");
                    K1K2s2 = System.Math.Abs(Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0)));
                    double K1K2SigTime3 = 0;
                    if (K1K2s2 > 2)
                    {
                        //K1K2SigTime3 = OscillographInstrument_Points_Single_Multiple2(7, false, 0, 6, 0, false, false, 0.05, 2) * 10;
                        K1K2SigTime3 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 0, false, false, 0.05, true, 30) * 10;
                    }
                    else
                    {
                        //K1K2SigTime3 = OscillographInstrument_Points_Single_Multiple2(7, false, 0, 6, 1, false, false, 0.05, 2) * 10;
                        K1K2SigTime3 = GetTriggerTime_Single2_NoCursor(7, false, 0, 6, 1, false, false, 0.05, true, 30) * 10;
                    }
                    dic[1] = K1K2SigTime3.ToString("F1");
                    //ProcessDataTmp(dic, "断开K1/K2", "...", CSDTime1.ToString(), "-");
                    ProcessDataTmp(dic, sState, "K1K2-sig第二个开关断开时间点", CSDTime1.ToString("F1"), "-");




                    SendNoticeToUIAndTxtFile("读取高压第二个100V下降沿时间点中...");
                    //double VoltageTime2 = OscillographInstrument_Points_Single_Multiple2(4, false, 0, 150, 1, false, false, 0.05, 2) * 10;
                    double VoltageTime2 = GetTriggerTime_Single2_NoCursor(4, false, 0, 150, 1, false, false, 0.05, true, 30) * 10;
                    dic[1] = VoltageTime2.ToString("F1");
                    //ProcessDataTmp(dic, "泄放电压", "...", K1K2SigTime3.ToString(), "-");
                    ProcessDataTmp(dic, sState, "第二个泄放电压时间点", K1K2SigTime3.ToString("F1"), "-");


                    SendNoticeToUIAndTxtFile("读取辅源6V下降沿时间点中...");
                    double K3K4Time2 = GetTriggerTime_Single2_NoCursor(2, false, 0, 9, 1, false, false, 0.05, true) * 10;
                    dic[1] = K3K4Time2.ToString("F1");
                    //ProcessDataTmp(dic, "断开K3/K4", "...", VoltageTime2.ToString(), "-");
                    ProcessDataTmp(dic, sState, "辅源6V下降沿时间点(断开K3/K4)", VoltageTime2.ToString("F1"), "-");
                }


                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);

            }
        }
        public override void ProcessData()
        {

        }





        /// <summary>
        ///录波仪初始化
        /// </summary>
        private void InitOscillograph()
        {
            string customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if(customer != null && customer.Equals("DH"))
                ControlEquipMent.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 8, 16, true, "5000", "CSD-energy", 0, "181DF456", "Auto", 16, 16, 0, 0, "0.1", "0", "kWh", "9", "-1", "YELLow", true, false, false, false);//CAN子通道8
            if (ControlEquipMent.ResistanceLoad == null)
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 4, 4, true, "DC", "FULL", "5000", Channel1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4
            if (TrialType == (int)EmTrialType.充电连接控制时序研发B5)
            {
                ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
                ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 16, true);
                ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);

                SetChannelOpenInit();

                channelopen[0] = true;//1通道
                channelopen[1] = true;//2通道
                channelopen[2] = true;//3通道
                channelopen[3] = true;//4通道
                channelopen[4] = true;//5通道
                channelopen[6] = true;//7通道
                channelopen[7] = true;//8通道

                canchannelopen[6] = true;//CAN 7通道
                canchannelopen[7] = true;//CAN 8通道
                canchannelopen[8] = true;//CAN 9通道

                canchannelopen[11] = true;//CAN 12通道
                canchannelopen[12] = true;//CAN 13通道
                canchannelopen[16] = true;//CAN 17通道
                canchannelopen[17] = true;//CAN 18通道

                ControlEquipMent.Oscillograph.Oscillograph_TimeBase("6");
            }
            //B6 流程测试  默认刚测过B5,录波仪参数已经设置过,省去设置参数的时间
            else if (TrialType == (int)EmTrialType.充电连接控制时序研发B6)
            {
                ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 16, true);
                ControlEquipMent.Oscillograph.Oscillograph_IsRun(true);
                channelopen[0] = true;//1通道
                channelopen[1] = true;//2通道
                channelopen[2] = true;//3通道
                channelopen[3] = true;//4通道
                channelopen[4] = true;//5通道
                channelopen[6] = true;//7通道
                channelopen[7] = true;//8通道
                canchannelopen[0] = true;//CAN 1通道
                canchannelopen[1] = true;//CAN 2通道
                canchannelopen[2] = true;//CAN 4通道     
                canchannelopen[6] = true;//CAN 7通道
                canchannelopen[7] = true;//CAN 8通道
                canchannelopen[8] = true;//CAN 9通道          
                canchannelopen[10] = true;//CAN 11通道    
                canchannelopen[11] = true;//CAN 12通道
                canchannelopen[12] = true;//CAN 13通道
                canchannelopen[16] = true;//CAN 17通道
                canchannelopen[17] = true;//CAN 18通道           
                canchannelopen[18] = true;//CAN 19通道
                canchannelopen[19] = true;//CAN 20通道
            }

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph.Oscillograph_TimeBase("6");
            if (Customer != null && Customer.Equals("CJB"))
                OscillographInstrument_SetTrigger(6, 2, 0, "FALL", false, 98);
            else
                OscillographInstrument_SetTrigger(6, 2, 0, "FALL", false, 95);
            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "30", "-4");
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
