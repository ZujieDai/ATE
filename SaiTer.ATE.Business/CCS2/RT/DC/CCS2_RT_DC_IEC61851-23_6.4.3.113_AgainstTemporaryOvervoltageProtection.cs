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
    /// 欧标研测直流：防止暂时过电压
    /// </summary>
    public class CCS2_RT_DC_AgainstTemporaryOvervoltageProtection : BusinessBase
    {
        public CCS2_RT_DC_AgainstTemporaryOvervoltageProtection(int type)
        {
            TrialType = type;
        }

        //private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        double BMSDemandVolt = 750;
        double ResiLoadCurrent = 250;
        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            BMSDemandVolt = MaxAllowChargeVoltage;
            ResiLoadCurrent = MaxAllowChargeCurrent;
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


                    //打开DC+DC-对地采样
                    var kslist = ControlEquipMent.ControlBoard.ControlBoardReadState(testWorkParam.lstIDs);
                    kslist[9] = true;
                    kslist[10] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(testWorkParam.lstIDs, kslist);

                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel3, "Output_Current", "50", "V", false, "50", "-3");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "MAX", 1);
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(waitTime);

                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "2");//低值
                    Thread.Sleep(waitTime);

                    //设置触发
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 1, MaxAllowChargeVoltage.ToString(), "Auto");//上升边沿触发，自动
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "7.5", 2, (ResiLoadCurrent / 2).ToString(), "Auto");//上升边沿触发，自动
                    Thread.Sleep(waitTime);
                    SendNoticeToUIAndTxtFile("启动示波器，闭合导引S2开关，开始充电");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent))
                    {
                        return;
                    }

                    //bool[] Ks = new bool[24];
                    //Ks[0] = true;//DC+DC-控制
                    //Ks[1] = true;//CC信号控制
                    //Ks[2] = true;//CP信号控制
                    //Ks[4] = true;//PE信号控制
                    //ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0);
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, BMSDemandVolt, 250);
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, true, BMSDemandVolt);
                    //Thread.Sleep(200);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //MessgaeInfo(true, "请刷卡充电!");
                    //int timeout = 60;
                    //while (timeout-- > 0)
                    //{

                    //    int state = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState);
                    //    if (state >= 12)
                    //    {
                    //        MessgaeInfo(false, "请刷卡充电!");
                    //        ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    //        break;
                    //    }
                    //    System.Threading.Thread.Sleep(1000);
                    //}
                    //MessgaeInfo(false, "请刷卡充电!");
                    //WaitDCVoltage_EU_DC(testWorkParam.lstIDs, BMSDemandVolt);
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    Thread.Sleep(2000);


                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 10, ResiLoadCurrent - 10, BMSDemandVolt, ResiLoadCurrent - 5);
                    Thread.Sleep(300);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, ResiLoadCurrent - 10);
                    Thread.Sleep(5000);//等待回馈负载电流稳定

                    //double? chargingVoltage = 0;
                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{
                    //    int time = 100;
                    //    while(time-- > 0)
                    //    {
                    //        chargingVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                    //        if (chargingVoltage > BMSDemandVolt * 0.95)
                    //            break;
                    //        Thread.Sleep(100);
                    //    }
                    //    dic.Add(item, chargingVoltage.GetValueOrDefault().ToString("F2"));
                    //}

                    var dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Dictionary<int, string> Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);
                    //输出电压不能超过110%或者低于5s
                    if (Convert.ToDouble(Data_Tmp.FirstOrDefault().Value) < BMSDemandVolt * 1.1)
                        ProcessDataTmp(Data_Tmp, "防止暂时过电压", "充电电压(V)", "-", (BMSDemandVolt * 1.1).ToString(), dImgs);
                    else
                    {
                        ProcessDataTmp(Data_Tmp, "防止暂时过电压", "充电电压(V)", (BMSDemandVolt * 1.1).ToString(), "-");
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                        Thread.Sleep(2000);
                        ACUpTime(testWorkParam.lstIDs, 1, BMSDemandVolt * 1.1, 1);
                        ACDownTime(testWorkParam.lstIDs, 1, BMSDemandVolt, 2);

                        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
                        //读取卡点时间
                        ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                        Data_Tmp = GetOSCTime(OscTime_Tmp);
                        Dictionary<int, string> dd = new Dictionary<int, string>();
                        foreach (var itmp in Data_Tmp)
                        {
                            dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                        }
                        ProcessDataTmp(dd, "防止暂时过电压", "过压时间(ms)", "0", "5000", dImgs);
                    }
                }
            }
            catch (Exception ex) { SendException(ex); }
            finally
            {
                //关闭DC+DC-对地采样
                var kslist = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
                kslist[9] = false;
                kslist[10] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstIDs, kslist);
                SetLoadDCOFF(lstIDs);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
            }

        }

        public override void ProcessData()
        {
        }
    }
}
