using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 缓启动试验(录波板，源自南网企标，目前河南产测使用)
    /// </summary>
    public class GB_PT_DC_SlowStart_WaveRecoder_XJ_QB : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double BMSDemandVolt = 0;
        double BMSDemandCurrent = 30;
        public GB_PT_DC_SlowStart_WaveRecoder_XJ_QB(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
        }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            BMSDemandCurrent = LstChargerInfo[0].NominalCurrent;
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //添加测试条件
                    SetConditionValues();


                    SetCPReresh();  // 模拟插拔枪
                    ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(lstIDs);


                    SendNoticeToUIAndTxtFile("启动录波板...");
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    //MessgaeInfo(true, "请刷卡充电!", true);
                    //int timeout = 300;
                    //while (timeout-- > 0)
                    //{
                    //    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                    //    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    //        continue;
                    //    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) <= 9);
                    //    if (ALLCanCharge && AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage > 380)   //避免提前触发
                    //    {
                    //        break;
                    //    }

                    //    System.Threading.Thread.Sleep(1000);
                    //}
                    //MessgaeInfo(false, "");
                    //// 等待进入充电状态
                    //////Thread.Sleep(5500);     //避免提前触发
                    ////ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    //timeout = 60;
                    //MessgaeInfo(true, "等待充电中...", true);
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
                    //MessgaeInfo(false, "");

                    Thread.Sleep(10000);//有的模块启动较慢，这里加长一点
                    SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");

                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("停止录波板...");
                    ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    //XJ客户这里是采集的K1K2前端电压来分析的，不用带载
                    WaveData CH_K1K2 = new WaveData();
                    WaveData CH_K1K2Voltage = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 6, ref CH_K1K2Voltage, "OutputVoltage_Front");
                    //分多段 数据分析
                    double Time_Tmp = 0;
                    double Time_Start = 0;
                    double Time_End = 0;
                    //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2Voltage, true, BMSDemandVolt / 2, ref Time_Tmp);//获取第一个上升的点
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, false, 40, ref Time_Tmp, Time_Tmp);//获取第一个下降的点，这段是绝缘电压
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, true, 40, ref Time_Tmp, Time_Tmp);//获取第2个上升的点
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, false, 40, ref Time_Tmp, Time_Tmp);//获取第2个下降的点，这段是绝缘电压
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, true, 40, ref Time_Start, Time_Tmp);//获取第3个下降的点
                    //DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, true, BMSDemandVolt - 50, ref Time_End, Time_Start);//获取第3个上升的点

                    //这里获取起点和终点
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, true, BMSDemandVolt - 50, ref Time_Tmp, CH_K1K2Voltage.LinePoints_Y.Count,true);//倒序获取计算点
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, false, 60, ref Time_Start, Time_Tmp, true);//倒序获取起点，正序是上升，因为是倒序的，所以这里是下降趋势
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(CH_K1K2Voltage, true, BMSDemandVolt - 50, ref Time_End, Time_Start);//获取终点，这里是升序


                    double Time_Stop = Math.Abs(Time_End - Time_Start);
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_Start);//设置光标1
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_End);//设置光标2


                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        dd.Add(item, Time_Stop.ToString());
                    }
                    dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "缓启动", "直流输出上升时间(ms)", "1000", "8000", dImgs);


                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);//关闭导引
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
