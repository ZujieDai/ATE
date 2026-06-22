using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    ///输出冲击电流  直流测试(录波板,源自南网企标，目前河南产测使用)  
    /// </summary>
    public class GB_PT_DC_OutputImpulseCurrent_WaveRecoder_XJ_QB : BusinessBase
    {
        public GB_PT_DC_OutputImpulseCurrent_WaveRecoder_XJ_QB(int trialType) { TrialType = trialType; }
        private double DemandVoltage = 400;
        private double DemandCurrent = 20;

        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
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



                SendNoticeToUIAndTxtFile("录波板启动录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                int timeout = 400;
                while (timeout-- > 0)
                {
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState);
                    if (state >= 5)
                    {
                        //ControlEquipMent.Oscillograph.Oscillograph_TriggerTypeSet("Single");
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

                Thread.Sleep(5000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                //if (!ReadTriggerTypeOscillograph(30))
                //{
                //    ProcessDataResult(testWorkParam.lstIDs, "触发失败", "K1K2状态变化", false);
                //}
                SetConditionValues();


                //读取录波板数据
                WaveData CH_OutputVoltage = new WaveData();
                WaveData CH_OutputCurrent = new WaveData();
                WaveData CH_K1K2 = new WaveData();
                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref CH_OutputCurrent, "OutputCurrent");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                double Time_K1K2 = 0;
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }

                Thread.Sleep(2000);
                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图

                SendNoticeToUIAndTxtFile("判断结果中...");
                //double dMax = DataAnalysis_WaveRecoder.GetWavePointMaxVave(CH_OutputCurrent);
                double dMax = DataAnalysis_WaveRecoder.GetWavePointMaxVave(CH_OutputCurrent, (int)Time_K1K2 + 100);//要取第二次到充电阶段的数据
                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    dic.Add(item, dMax.ToString("F2"));
                }

                ProcessDataTmp(dic, "输出冲击电流", "电流MAX值", "-", "20", dicImagePath);


                double dMin = DataAnalysis_WaveRecoder.GetWavePointMinVave(CH_OutputCurrent);
                dic = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    dic.Add(item, dMax.ToString("F2"));
                }
                ProcessDataTmp(dic, "输出冲击电流", "电流Min值", "-20", "-");


            }
        }
        public override void ProcessData()
        {

        }



    }
}
