using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 防逆流功能试验
    /// </summary>
    public class AntiBackflow : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        /// <summary>
        /// 继电器序号
        /// </summary>
        int mYRelayIndex = 0;



        public AntiBackflow(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //string[] strParams = TrialItem.ResultParams.Split('|');

            //if (strParams.Length >= 1)
            //{
            //    double index = double.Parse(strParams[0].Split('=')[1]);
            //    mYRelayIndex = Convert.ToInt32(index);
            //    mYRelayIndex = mYRelayIndex - 1;
            //}
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
                if (ControlEquipMent.FeedbackLoad == null)
                {
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
                }
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
                    SetConditionValues();

                    DemandVoltage = 500;
                    DemandCurrent = 20;
                    Double LoadVoltage = 490;
                    Double LoadCurrent = 10;
                    Double LoadVoltage2 = 700;
                    Double LoadCurrent2 = 10;


                    SendNoticeToUIAndTxtFile("启动充电中");

                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage, DemandCurrent))
                    {
                        return;
                    }

                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("初始化示波器...");
                    OscilloscopeAntiReflux();


                    //SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);
                    //WaitDCVoltage(testWorkParam.lstIDs, DemandVoltage);

                    SendNoticeToUIAndTxtFile("开启负载...");
                    OscilloscopeAntiReflux2();
                    SendNoticeToUIAndTxtFile("设置负载参数中...");
                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        SetLoadPara(testWorkParam.lstIDs, LoadVoltage, LoadCurrent, DemandVoltage, LoadCurrent2);
                        SendNoticeToUIAndTxtFile("开启负载...");
                        SetLoadDCON(testWorkParam.lstIDs);
                        WaitDCCurrentWithTime(testWorkParam.lstIDs, LoadCurrent, 40);

                    }
                    ControlEquipMent.Oscilloscope?.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");

                    if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    {
                        SetLoadPara(testWorkParam.lstIDs, LoadVoltage2, LoadCurrent2, LoadVoltage2, 0, true, false);
                        WaitDCVoltage(testWorkParam.lstIDs, LoadVoltage2);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("发送过压中...");
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, MaxAllowChargeVoltage + 300);
                        //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, LoadVoltage2);
                        Thread.Sleep(2000);
                        List<bool> Ks = GetKStatus16_Charging_DC();
                        Ks[26] = true;
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                    }
                    //if (ControlEquipMent.FeedbackLoad != null)
                    //{
                    //    WaitDCCurrent(testWorkParam.lstIDs, LoadCurrent2);
                    //}


                    ReadTriggerType(testWorkParam.lstIDs, 15);
                    SetLoadDCOFF(testWorkParam.lstIDs, false);



                    Dictionary<int, string> InDCImin = ControlEquipMent.Oscilloscope?.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MIN", 2);    //采集的是输入交流电流，最大值

                    Dictionary<int, string> dImgs1 = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);


                    Dictionary<int, string> DCImin = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = Convert.ToDouble(InDCImin[Convert.ToInt32(item)]);
                        DCImin.Add(item, DCCurrent.ToString("F2"));
                    }

                    ProcessDataTmp(DCImin, "防逆流功能", "直流回路反向电流(A)", "-0.5", "-", dImgs1);

                    CountDownTimeInfo("请确认桩是否报警\r\n注：勾选上为有报警提示", 20, 2);
                    ProcessData();

                    SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFFALL(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("关闭导引中!");

                    if (ControlEquipMent.FeedbackLoad == null && ControlEquipMent.LoopFeedbackLoad == null && ControlEquipMent.StarLoopFeedbackLoad == null)
                    {
                        var Ks = GetKStatus16_Charging_DC();
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                        ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        public override void ProcessData()
        {
            try
            {

                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = "防逆流功能" + "|是否报警|-|-|是|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = "防逆流功能" + "|是否报警|-|-|否|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public void ProcessData(bool Status)
        {
            try
            {

                foreach (var item in testWorkParam.lstIDs)
                {

                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    string State = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingState;

                    if (Status)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = "防逆流功能" + "|充电状态|-|-|" + State;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 示波器防逆流功能试验
        /// </summary>
        public void OscilloscopeAntiReflux()
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);

                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "150", "-3");//通道1设置
                System.Threading.Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "2", "-1");//通道2设置

                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道3设置
                System.Threading.Thread.Sleep(100);

                ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20M", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置

                System.Threading.Thread.Sleep(1000);

                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, "650", "Auto");
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "-0.3");//设置滚动，时基和触发延时

                System.Threading.Thread.Sleep(100);

                ControlEquipMent.Oscilloscope?.Oscilloscope_CursorPosition_X_Time(testWorkParam.lstIDs, 1, 0, true);
                System.Threading.Thread.Sleep(100);
                ControlEquipMent.Oscilloscope?.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "XY");//设置测量项为XY
                System.Threading.Thread.Sleep(2000);
                ControlEquipMent.Oscilloscope?.Oscilloscope_CursorPosition_Y(testWorkParam.lstIDs, 1, 1, 1.0833);
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(100);

                //回馈载杂波
                //if (ControlEquipMent.FeedbackLoad != null)
                //    ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 2);
                //else
                    ControlEquipMent.Oscilloscope?.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 2);
            }
            catch
            {

            }
        }
        public void OscilloscopeAntiReflux2()
        {
            try
            {
                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, "650", "Auto");
            }
            catch
            {

            }
        }

    }
}
