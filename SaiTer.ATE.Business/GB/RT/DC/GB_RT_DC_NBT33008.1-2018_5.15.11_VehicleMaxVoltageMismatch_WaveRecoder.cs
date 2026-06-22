using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
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
    /// 车辆最高允许充电总电压不匹配试验（录波板）
    /// </summary>
    public class GB_RT_DC_VehicleMaxVoltageMismatch_WaveRecoder : BusinessBase
    {
        public GB_RT_DC_VehicleMaxVoltageMismatch_WaveRecoder(int trialType) { TrialType = trialType; }
        private double wsdy握手电压 = 750;
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
        public override void InitializeParams()
        {
            Init();

            //自检阶段测试-开始前>=10V不正常电池电压时握手电压超限  三种情况
            //握手电压超上限值(V)=1050|电池电压Ut-error(V)=100
            //握手电压正常值(V)=500|电池电压Ut-error(V)=100
            //握手电压超下限值(V)=150|电池电压Ut-error(V)=100




            //自检阶段测试 < 10V握手电压低于充电机下限 / 车辆最高允许充电总电压不匹配试验  两种情况
            //握手电压超上限值(V)=1050       
            //握手电压超下限值(V)=150

            wsdy握手电压 = LstChargerInfo[0].NominalVoltage;

            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                wsdy握手电压 = Convert.ToDouble(strParams[0].Split('=')[1]);
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
                ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(8);
                //过压控制开关
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(100);
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);

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

                SetConditionValues();

                SendNoticeToUIAndTxtFile("关闭BMS");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                SetCPReresh();//模拟插拔枪

                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, wsdy握手电压);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, wsdy握手电压, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);



                try
                {
                    SendNoticeToUIAndTxtFile("启动录波板");
                    //启动录波板
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("等待刷卡");
                    int timeout = 30;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) > 4 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9); // >=3 or >4??
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    Thread.Sleep(1000 * 15);

                    //停止录波板
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    Thread.Sleep(1000);

                    WaveData CH_OutputVoltage = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");

                    double MaxVoltage = DataAnalysis_WaveRecoder.GetWavePointMaxVave(CH_OutputVoltage);//最大电压

                    dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("判断充电状态");
                    ProcessData();

                    //CountDownTimeInfo("请人工确认是否有告警！\r\n 勾选上代表有告警", 100, 2); 
                    if (Customer.Equals("XJ"))
                    {
                        CountDownTimeInfo("请人工确认是否有告警！\r\n 勾选上代表有告警", 30, 2);
                    }
                    else
                    {
                        CountDownTimeInfo("请人工确认是否有告警！\r\n 勾选上代表有告警", 100, 2);
                    }
                    string statusName = TrialItem.ItemName.Split('-').Length > 1 ? TrialItem.ItemName.Split('-')[1] : TrialItem.ItemName;
                    if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "有告警", statusName, true, "是否有告警");
                    }
                    else
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "未告警", statusName, false, "是否有告警");
                    }


                }

                catch (Exception ex) { Log.Log.LogException(ex); }
            }
        }
        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();


                    if (dicImagePath != null)
                    {
                        sbtmp.Append(dicImagePath[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }


                    string result = "";

                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);

                    if (state > 4 && state <= 9)//>=3  or  >4 ????
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        //result = "允许充电";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        //result = "不允许充电";
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果      
                    //自检阶段测试 - 开始前 >= 10V不正常电池电压时握手电压超上限
                    string statusName = TrialItem.ItemName.Split('-').Length > 1 ? TrialItem.ItemName.Split('-')[1] : TrialItem.ItemName;
                    LstTrialData[i].ExtentData = statusName
                        + "|" + "当前充电阶段"
                        + "|" + "-"
                        + "|" + "-"
                        + "|" + AllEquipStateData.DicBMS_DC_StateData[item].ChargingState
                        + "|" + sbtmp.ToString();

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);

                    SaveTrialData(LstTrialData[k]);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }

    }
}
