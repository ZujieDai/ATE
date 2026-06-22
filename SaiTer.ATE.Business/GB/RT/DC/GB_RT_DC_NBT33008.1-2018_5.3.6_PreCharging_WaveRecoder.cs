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
    /// 预充电功能试验(新录波板)
    /// </summary>
    public class GB_RT_DC_PreCharging_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 60;

        /// <summary>
        /// 下降的电流差值需要小于等于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent1 = 20;
        /// <summary>
        /// 下降的电流差值需要大于20A-小于DemandCurrent*0.8
        /// </summary>
        Double MinusCurrent2 = 40;

        double BatteryVoltage = 390;

        WaveData wdtmp = new WaveData();//存储录波板数据的临时变量
        public GB_RT_DC_PreCharging_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {

            SendNoticeToUIAndTxtFile("设备初始化中...");


            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道
            channelopen[6] = true;//7通道
            channelopen[7] = true;//8通道


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

            SetCPReresh();
        }

        public override void InitializeParams()
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
                    ProcessDataTmp(dic, "充电设置", "电压需求(V)", "-", "-");
                    ProcessDataTmp(dicC, "充电设置", "电流需求(A)", "-", "-");
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, BatteryVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "充电设置", "报文电池电压(V)", "-", "-");



                    SendNoticeToUIAndTxtFile("开启导引中");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVoltage);   //互操蓄电池电压
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BatteryVoltage, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                    Thread.Sleep(500);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(300);


                    //启动录波板录波
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);

                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 20 * 200;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state >= 5 && state <= 7)
                        {
                            //启动录波板录波(到了固定阶段重新录波)
                            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                            break;
                        }
                        int residuetime = timeout / 20;
                        SendNoticeToUIAndTxtFile("刷卡剩余倒计时:" + residuetime);
                        System.Threading.Thread.Sleep(50);
                    }

                    MessgaeInfo(false, "请刷卡充电!");

                    timeout = 20 * 200;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                        if (state == 9)
                        {
                            //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                            break;
                        }
                        int residuetime = timeout / 20;
                        SendNoticeToUIAndTxtFile("等到到达充电中:" + residuetime);
                        System.Threading.Thread.Sleep(50);

                    }


                    //停止录波板录波
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);

                    //设置测试条件
                    SetConditionValues();


                    Thread.Sleep(6000);
                    double DownCurrent = (DemandCurrent) * 0.99;
                    SendNoticeToUIAndTxtFile("判断结果中...");


                    double Y1 = 0;
                    //Y1 = BatteryVoltage;
                    //WaveData CH_BCPVoltage = new WaveData();
                    //ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 20, 0, ref CH_BCPVoltage, "BCPVoltage");//读取数字通道数据（BCP电池电压）
                    //DataAnalysis_WaveRecoder.GetBCPBatteryVolt(wdtmp, ref Y1);
                    WaveData CH_OutputVoltage = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");//读取1通道的波形数据(前端电压)，这里目前用充电电压代替
                    DataAnalysis_WaveRecoder.GetPreChargeVolt(CH_OutputVoltage, BatteryVoltage, ref Y1);

                    double Y2 = 0;
                    WaveData CH_OutputFrontVoltage = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 6, ref CH_OutputFrontVoltage, "OutputFrontVoltage");//读取1通道的波形数据(前端电压)，这里目前用充电电压代替
                    DataAnalysis_WaveRecoder.GetPreChargeVolt(CH_OutputFrontVoltage, Y1 , ref Y2);
                    double Dvalue = 0;
                    Dvalue = Y1 - Y2;
                    Dvalue = RetainDecimals<double>(Dvalue);

                    WaveData CH_K1K2State = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2State, "K1K2State");

                    //保存图片
                    Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                    Dictionary<int, string> dY1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dY1.Add(item, Y1.ToString("F2"));
                    }

                    Dictionary<int, string> dY2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dY2.Add(item, Y2.ToString("F2"));
                    }

                    Dictionary<int, string> dY1Y2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dY1Y2.Add(item, Dvalue.ToString("F2"));
                    }


                    ProcessDataTmp(dY1, "闭合K5和K6后", "电池电压", "-", "-");
                    ProcessDataTmp(dY2, "闭合K5和K6后", "K1K2前端电压", " - ", "-");
                    ProcessDataTmp(dY1Y2, "闭合K5和K6后", "Y1和Y2差值", " 1 ", "10", dImgs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");
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
