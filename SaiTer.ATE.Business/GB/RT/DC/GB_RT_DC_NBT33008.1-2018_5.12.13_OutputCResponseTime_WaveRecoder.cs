using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试:输出电流响应时间(录波板)  //GB_RT_DC_NBT33008.1-2018_5.12.13_OutputCResponseTime_WaveRecoder
    /// </summary>
    public class GB_RT_DC_OutputCResponseTime_WaveRecoder : BusinessBase
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
        public GB_RT_DC_OutputCResponseTime_WaveRecoder(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //BMS需求电压设置(V)=400|回馈负载电流设置(A)=100|小于等于20A下降电流(A)=20|大于20A下降电流(A)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 4)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                MinusCurrent1 = double.Parse(strParams[2].Split('=')[1]);
                MinusCurrent2 = double.Parse(strParams[3].Split('=')[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            SetLoadDCOFF(lstIDs);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);


            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s

            SetCPReresh();
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

                    Dictionary<int, string> ddC1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        ddC1.Add(item, MinusCurrent1.ToString("F2"));
                    }
                    Dictionary<int, string> ddC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        ddC2.Add(item, MinusCurrent1.ToString("F2"));
                    }



                    SendNoticeToUIAndTxtFile("开启导引中");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    Thread.Sleep(5 * 1000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);

                    #region 第一个点
                    var dic2 = new Dictionary<int, string>();
                    var dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整前(小于等于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整前(小于等于20A下降电流)", "充电电流(A)", "-", "-");
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorsOpen(true);
                    double Level = -(DemandCurrent - MinusCurrent1 / 2);
                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000); //加延迟防止变化太快
                    SendNoticeToUIAndTxtFile("下发指令改变电流点1...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent - MinusCurrent1, false, 390);

                    //等待电流启动
                    WaitDCCurrentChangeWithTime(testWorkParam.lstIDs, DemandCurrent, DemandCurrent - MinusCurrent1, 35);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);


                    //读取录波板数据
                    double Time_BCL_Current = 0;
                    double Time_OutputCurrent = 0;
                    WaveData CH_BCL_Current = new WaveData();
                    WaveData CH_OutputCurrent = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 130, 0, ref CH_BCL_Current, "BCLCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");

                    double Time_BCLStart = 0;//获取BCL的发送时间
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BCL_Current, true, DemandCurrent - MinusCurrent1, ref Time_BCLStart);//这个是上升波形
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BCL_Current, false, DemandCurrent - MinusCurrent1, ref Time_BCL_Current, Time_BCLStart);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputCurrent, false, DemandCurrent - (MinusCurrent1 / 2), ref Time_OutputCurrent);
                    SendNoticeToUIAndTxtFile("时间数据：" + Time_BCLStart + "," + Time_BCL_Current + "," + Time_OutputCurrent);

                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_BCL_Current);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputCurrent);//设置光标2
                    double Time_Response = Math.Abs(Time_BCL_Current - Time_OutputCurrent);

                    double DownCurrent = (DemandCurrent - MinusCurrent1) * 0.99;
                    SystemEvent.MessageInfo(true, "判断结果中...");

                    Dictionary<int, string> dTime = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        dTime.Add(item, (Time_Response).ToString("F2"));
                    }

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整后(小于等于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整后(小于等于20A下降电流)", "充电电流(A)", "-", "-");
                    Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    double checktime1 = MinusCurrent1 / 20.0;
                    ProcessDataTmp(dTime, "调整后(小于等于20A下降电流)", "时间差(ms)", "0", (checktime1 * 1000).ToString(), dImgs);
                    SystemEvent.MessageInfo(false, "判断结果中...");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SetCPReresh();

                    #region 第二个点


                    SendNoticeToUIAndTxtFile("开启导引中");

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, 390);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("启动充电中");
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    Thread.Sleep(5 * 1000);

                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 10, DemandCurrent + 10, DemandVoltage - 5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, DemandCurrent, 35);

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整前(大于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整前(大于20A下降电流)", "充电电流(A)", "-", "-");

                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(3 * 1000); //加延迟防止变化太快

                    SendNoticeToUIAndTxtFile("下发指令改变电流点2...");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent - MinusCurrent2, false, 390);

                    //等待电流启动
                    WaitDCCurrentChangeWithTime(testWorkParam.lstIDs, DemandCurrent, DemandCurrent - MinusCurrent2, 35);

                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(500);


                    //读取录波板数据
                    Time_BCL_Current = 0;
                    Time_OutputCurrent = 0;
                    CH_BCL_Current = new WaveData();
                    CH_OutputCurrent = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 130, 0, ref CH_BCL_Current, "BCLCurrent");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");

                    Time_BCLStart = 0;//获取BCL的发送时间
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_BCL_Current, true, DemandCurrent - MinusCurrent2, ref Time_BCLStart);//这个是上升波形
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_BCL_Current, false, DemandCurrent - MinusCurrent2, ref Time_BCL_Current, Time_BCLStart);
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_OutputCurrent, false, DemandCurrent - (MinusCurrent2 / 2), ref Time_OutputCurrent);
                    SendNoticeToUIAndTxtFile("时间数据："+Time_BCLStart+","+ Time_BCL_Current + ","+ Time_OutputCurrent);

                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_BCL_Current);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_OutputCurrent);//设置光标2
                    Time_Response = Math.Abs(Time_BCL_Current - Time_OutputCurrent);

                    SystemEvent.MessageInfo(true, "判断结果中...");

                    double checktime = MinusCurrent2 / 20;

                    dic2 = new Dictionary<int, string>();
                    dicC2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dicC2.Add(item, DCCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(dic2, "调整后(大于20A下降电流)", "充电电压(V)", "-", "-");
                    ProcessDataTmp(dicC2, "调整后(大于20A下降电流)", "充电电流(A)", "-", "-");
                    Dictionary<int, string> dImgs2 = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                    Dictionary<int, string> dTime2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dTime2.Add(item, (Time_Response).ToString("F2"));
                    }
                    ProcessDataTmp(dTime2, "调整后(大于20A下降电流)", "时间差(ms)", "0", (checktime * 1000).ToString(), dImgs2);

                    SystemEvent.MessageInfo(false, "判断结果中...");
                    #endregion

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    SetLoadDCOFF(testWorkParam.lstIDs);

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
