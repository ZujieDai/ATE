using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标产测直流：保护接地连续性试验(录波板)
    /// </summary>
    public class GB_PT_DC_ProtectiveGroundingContinuance_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 750;
        /// <summary>
        /// 需求电流
        /// </summary>
        Double DemandCurrent = 60;

        public GB_PT_DC_ProtectiveGroundingContinuance_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {

            SendNoticeToUIAndTxtFile("设备初始化中...");

            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            //bool[] channelopen = new bool[8];
            //bool[] canchannelopen = new bool[20];
            //channelopen[0] = true;//1通道
            //channelopen[1] = true;//2通道
            //channelopen[2] = true;//3通道
            //channelopen[3] = true;//4通道
            //channelopen[6] = true;//4通道

            //canchannelopen[4] = true;//CAN5通道
            //canchannelopen[5] = true;//CAN6通道
            //canchannelopen[6] = true;//CAN7通道
            //canchannelopen[7] = true;//CAN8通道
            //SetChannel(channelopen, canchannelopen);
            //ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.2");

            ////ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(testWorkParam.lstIDs, 1, 1, true, "DC", "500", "5000", "200", "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200", "BLUE", true, false, false, false);//通道1

            //ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1)
            {
                DemandCurrent = double.Parse(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                DemandVoltage = double.Parse(strParams[1].Split('=')[1]);
            }

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
                List<bool> Ks = GetKStatus16_Charging_DC();
                Ks[22] = true;
                Ks[27] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
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
                    //设置测试条件


                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dic.Add(item, DemandVoltage.ToString("F2"));
                    }


                    Dictionary<int, string> dicC = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dicC.Add(item, DemandCurrent.ToString("F2"));
                    }




                    ProcessDataTmp(dic, TrialItem.ItemName, "电压需求(V)", "-", "-");

                    ProcessDataTmp(dicC, TrialItem.ItemName, "电流需求(A)", "-", "-");




                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("启动充电中");

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);

                    Thread.Sleep(2 * 1000);

                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);
                    SendNoticeToUIAndTxtFile("启动录波板...");
                    //OscillographInstrument_SetTrigger(5, 3, 0, "RISE", false, 20);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    Thread.Sleep(2 * 1000);

                    SendNoticeToUIAndTxtFile("发送PE断线中...");
                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    Ks[27] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());


                    Thread.Sleep(10000);
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);


                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("判断结果中!");


                    //读取录波板数据
                    double Time_CC1 = 0;
                    double Time_K1K2 = 0;
                    WaveData CH_CC1 = new WaveData();
                    WaveData CH_K1K2 = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CC1, true, 5, ref Time_CC1);
                    double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                    if (K1K2_Tmp > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                    }

                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                    double Time_Stop = Math.Abs(Time_CC1 - Time_K1K2);


                    //读取卡点时间
                    Dictionary<int, double> dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                    Dictionary<int, string> dd = new Dictionary<int, string>();

                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        dd.Add(itmp, AllEquipStateData.DicBMS_EU_DC_StateData[itmp].ChargingVoltage.ToString());
                    }

                    ProcessDataTmp(dd, TrialItem.ItemName, "故障后输出电压(V)", "0", "20");


                    dd = new Dictionary<int, string>();
                    foreach (var itmp in dTime)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图


                    ProcessDataTmp(dd, TrialItem.ItemName, "断开K1K2延时(ms)", "0", "100", dImgs);



                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState);

                    if (state > 4 && state <= 9)//>=3  or  >4 ????
                    {
                        ProcessDataResult(testWorkParam.lstIDs, AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState, "充电状态", false, "失去保护接地连续性后应停止充电");
                        //result = "允许充电";
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState, "充电状态", true, "失去保护接地连续性后应停止充电");
                        //result = "不允许充电";
                    }

                    CountDownTimeInfo("请确认电子锁能正常解锁。\r\n注：勾选上为可以正常解锁", 20, 2);
                    ProcessDataConnect("电子锁在直流60V以下后", "应正常解锁");


                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        string canData = GetCANByType("CST");
                        ProcessDataResult(testWorkParam.lstIDs, canData, "CST报文内容", true, "PE断线");
                    }

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }




        public override void ProcessData()
        {

        }

    }
}
