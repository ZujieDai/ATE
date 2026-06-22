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
    /// 欧标研测直流：输出冲击电流
    /// </summary>
    public class CCS2_RT_DC_TurnOnInrushCurrentDL : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压(V)
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流(A)
        /// </summary>
        Double DemandCurrent = 60;
        /// <summary>
        /// 判定准则(A)
        /// </summary>
        Double ErrorValue = 2;


        public CCS2_RT_DC_TurnOnInrushCurrentDL(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Auto");

            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            bool[] channelopen = new bool[8];
            bool[] canchannelopen = new bool[20];
            channelopen[0] = true;//1通道
            channelopen[3] = true;//4通道

            SetChannel(channelopen, canchannelopen);
            ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.1");

            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);

            ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "1", "");

            ControlEquipMent.Oscillograph?.Oscillograph_MEASureOpen(true);
            ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Min", 1, false, 0);
            ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("Max", 1, false, 0);
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');

            //BMS需求电压设置(V)=745|BMS需求电流设置(A)=50|判定准则(A)=2
            if (strParams.Length >= 3)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
                DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
                ErrorValue = double.Parse(strParams[2].Split('=')[1]);
            }
            DemandCurrent = DemandCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : DemandCurrent;
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "50", "5000", Channel2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200Hz", "BLUE", true, false, false, false);//通道1
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
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


                    ControlEquipMent.Oscillograph?.Oscillograph_Channel_Set(testWorkParam.lstIDs, 1, 1, true, "DC", "300", "5000", Channel2, "DC-out-I", "A", false, "1", "0", 1, 1, 2, "100Hz", "BLUE", true, false, false, false);//通道1
                    ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                    OscillographInstrument_SetTrigger(300, 4, 0, "RISE", false, 50, "Auto");
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(200);
                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, MaxAllowChargeCurrent, true, 390);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3000);

                    MessgaeInfo(true, "请刷卡充电!");
                    int timeout = 300;
                    while (timeout-- > 0)
                    {

                        int state = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState);
                        if (state >= 18)
                        {
                            ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Single");
                            MessgaeInfo(false, "请刷卡充电!");
                            break;
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    Thread.Sleep(5000);

                    SendNoticeToUIAndTxtFile("读取录波仪电流最大值...");
                    Thread.Sleep(3000);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscillograph?.OscillographSaveScreen();

                    var dic = new Dictionary<int, string>();
                    foreach (int i in testWorkParam.lstIDs)
                    {
                        double Max = OscillographInstrumentReadMeasure("Max", 1, false, 0);
                        dic.Add(i, Max.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "接触器闭合涌入电流", "正向输入电流最大值(A)", "-", "2", dImgs);
                    dic = new Dictionary<int, string>();
                    foreach (int i in testWorkParam.lstIDs)
                    {
                        double Min = OscillographInstrumentReadMeasure("Min", 1, false, 0);
                        dic.Add(i, Min.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "接触器闭合涌入电流", "负向输入电流最大值(A)", "-2", "-");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
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

        }
    }
}
