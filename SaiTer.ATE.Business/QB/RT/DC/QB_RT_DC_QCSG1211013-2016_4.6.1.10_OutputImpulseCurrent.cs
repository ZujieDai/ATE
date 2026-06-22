using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 企标研测直流：输出电流过冲
    /// </summary>
    public class QB_RT_DC_OutputImpulseCurrent : BusinessBase
    {
        public QB_RT_DC_OutputImpulseCurrent(int trialType) { TrialType = trialType; }
        private double DemandVoltage = 400;
        private double DemandCurrent = 20;

        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
        bool[] channelopen = new bool[8];
        bool[] canchannelopen = new bool[20];

        /// <summary>
        ///录波仪初始化
        /// </summary>
        private void InitOscillograph()
        {
            try
            {
                ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Auto");
                ControlEquipMent.Oscillograph.Oscillograph_CursorsOpen(false);
                SetChannelOpenInit();
                channelopen[0] = true;//1通道
                channelopen[3] = true;//4通道
                channelopen[6] = true;//7通道
                channelopen[7] = true;//8通道

                SetChannel(channelopen, canchannelopen);
                ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(1, "10", "");
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(4, "100", "-5");
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_SetGear(8, "100", "-5");
                ControlEquipMent.Oscillograph?.Oscillograph_TimeBase("0.5");
                ControlEquipMent.Oscillograph?.Oscillograph_IsRun(true);
                ControlEquipMent.Oscillograph?.Oscillograph_MEASureOpen(true);
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
                ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("MAXimum", 1, false, 0);
                ControlEquipMent.Oscillograph?.Oscillograph_AddMeasure("MINimum", 1, false, 0);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

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
        public override void InitializeParams()
        {
            Init();
            dicImagePath = new Dictionary<int, string>();
            //BMS需求电压(V)=400|BMS需求电流(A)=20
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(1);
                SetCPReresh();
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
                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                InitOscillograph();

                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, false, DemandVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(100);

                SystemEvent.MessageInfo(true, "请刷卡充电...");

                double K1K2sig = Convert.ToDouble(OscillographInstrumentReadValue(7, false, 0));
                double K1K2s2 = Math.Abs(K1K2sig);


                if (K1K2s2 < 2)
                {
                    OscillographInstrument_SetTrigger(6, 7, 0, "RISE", false, 50, "Auto");
                }
                else
                {
                    OscillographInstrument_SetTrigger(6, 7, 0, "FALL", false, 50, "Auto");
                }
                int timeout = 400;
                while (timeout-- > 0)
                {
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState);
                    if (state >= 5)
                    {
                        ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Single");
                        break;
                    }
                    if (timeout < 0)
                    {
                        SendNoticeToUIAndTxtFile("刷卡失败,测试结束");
                        return;
                    }
                    Thread.Sleep(100);
                }

                timeout = 400;
                while (timeout-- > 0)
                {
                    string state = AllEquipStateData.DicBMS_DC_StateData[1].ChargingState;
                    if (state.Contains("充电中"))
                    {
                        break;
                    }
                    if (timeout < 0)
                    {
                        SendNoticeToUIAndTxtFile("刷卡失败,测试结束");
                        return;
                    }
                    Thread.Sleep(100);
                }
                SystemEvent.MessageInfo(false, "");
                Thread.Sleep(100);
                if (!ReadTriggerTypeOscillograph(30))
                {
                    ProcessDataResult(testWorkParam.lstIDs, "触发失败", "K1K2状态变化", false);
                }
                SetConditionValues();

                Thread.Sleep(2000);
                dicImagePath = ControlEquipMent.Oscillograph.OscillographSaveScreen();

                SendNoticeToUIAndTxtFile("判断结果中...");
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic = ControlEquipMent.Oscillograph.Oscillograph_ReadMeasure("MAXimum", 1, false, 0);
                ProcessDataTmp(dic, "输出冲击电流", "电流MAX值", "-", "20", dicImagePath);


                dic = ControlEquipMent.Oscillograph.Oscillograph_ReadMeasure("MINimum", 1, false, 0);
                ProcessDataTmp(dic, "输出冲击电流", "电流Min值", "-20", "-");
            }
        }

        public override void ProcessData()
        {

        }
    }
}
