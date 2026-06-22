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
    /// 自检阶段测试(录波板)
    /// </summary>
    public class GB_RT_DC_SelfCheckPhaseTest_WaveRecoder : BusinessBase
    {
        private int SelfCheckPhaseType;
        private string StatusName = "";
        private double HandshakeUpper = 150;    //握手电压超上限
        private double HandshakeNoraml = 500;   //握手电压正常
        private double HandshakeLower = 1050;   //握手电压超下限
        private double ErrorBatteryUt = 100;    //非正常电池端电压
        private double NormalBatteryUt = 390;   //正常电池端电压
        //private double wsdy握手电压 = 750;
        //private double dcdy电池电压 = 390;//自检阶段测试<10V握手电压低于充电机下限默认150V, >=10V时, 电池电压默认100V
        //private double dzfzdl电子负载电流 = 5000;
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();

        public GB_RT_DC_SelfCheckPhaseTest_WaveRecoder(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();

            //正常电池端电压(V)=390|非正常电池端电压(V)=100|握手电压超输出电压上限值(V)=1050|握手电压在输出电压范围内值(V)=750|握手电压超输出电压下限值(V)=150
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 5)
            {
                NormalBatteryUt = Convert.ToDouble(strParams[0].Split('=')[1]);
                ErrorBatteryUt = Convert.ToDouble(strParams[1].Split('=')[1]);
                HandshakeUpper = Convert.ToDouble(strParams[2].Split('=')[1]);
                HandshakeNoraml = Convert.ToDouble(strParams[3].Split('=')[1]);
                HandshakeLower = Convert.ToDouble(strParams[4].Split('=')[1]);
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
                //过压控制开关
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(100);
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
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
                for (int i = 0; i < 6; i++)
                {
                    SetCPReresh();
                    SelfCheckPhaseType = i;
                    TrialMethod(i);
                }
            }
        }

        /// <summary>
        /// 0.正常电池电压--报文最高允许充电电压超上限
        /// 1.正常电池电压--报文最高允许充电电压范围内
        /// 2.正常电池电压--报文最高允许充电电压超下限
        /// 3.非正常电池电压--报文最高允许充电电压超上限
        /// 4.非正常电池电压--报文最高允许充电电压范围内
        /// 5.非正常电池电压--报文最高允许充电电压超下限
        /// </summary>
        /// <param name="type"></param>
        private void TrialMethod(int type)
        {
            switch (type)
            {
                case 0:
                    StatusName = "正常电池电压（报文最高允许充电电压范围内）";
                    break;
                case 1:
                    StatusName = "正常电池电压（报文最高允许充电电压超上限）";
                    break;
                case 2:
                    StatusName = "正常电池电压（报文最高允许充电电压超下限）";
                    break;
                case 3:
                    StatusName = "非正常电池电压（报文最高允许充电电压范围内）";
                    break;
                case 4:
                    StatusName = "非正常电池电压（报文最高允许充电电压超上限）";
                    break;
                case 5:
                    StatusName = "非正常电池电压（报文最高允许充电电压超下限）";
                    break;
            }
            SendNoticeToUIAndTxtFile("关闭BMS");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            SetCPReresh();

            //InitOscillograph(type);

            SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(100);
            //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, dcdy电池电压, LstChargerInfo[0].NominalVoltage, 250);
            //Thread.Sleep(100);
            //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, wsdy握手电压, 250, false, wsdy握手电压);
            //Thread.Sleep(100);
            //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            //Thread.Sleep(100);
            //ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
            //Thread.Sleep(100);

            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            Thread.Sleep(100);
            if (type != 0 && type != 1 && type != 2)
            {
                //过压控制开关
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, NormalBatteryUt, kstate.ToArray());
                Thread.Sleep(100);

                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, ErrorBatteryUt);
                Thread.Sleep(100);
            }
            else
            {
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, NormalBatteryUt);
                Thread.Sleep(100);
            }
            if (type == 1 || type == 4)
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeUpper);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, NormalBatteryUt, HandshakeUpper, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeUpper, MaxAllowChargeCurrent, false, HandshakeUpper);
                Thread.Sleep(100);
            }
            else if (type == 0 || type == 3)
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeNoraml);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, NormalBatteryUt, HandshakeNoraml, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeNoraml, MaxAllowChargeCurrent, false, HandshakeNoraml);
                Thread.Sleep(100);
            }
            else
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeLower);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, NormalBatteryUt, HandshakeLower, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HandshakeLower, MaxAllowChargeCurrent, false, HandshakeLower);
                Thread.Sleep(100);
            }
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            Thread.Sleep(100);
            ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
            Thread.Sleep(100);


            d1 = new Dictionary<int, string>();
            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                //外侧电压具体是模拟过压后的导引充电电压
                d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
            }
            //SetConditionValue("K1K2外侧电压(V)", d1);
            ProcessDataTmp(d1, StatusName, "绝缘检测前外侧电压(V)", "-", "-");

            try
            {
                SendNoticeToUIAndTxtFile("启动录波板");
                //启动录波板
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                Thread.Sleep(2500);
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


                Thread.Sleep(30000);


                //停止录波板
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);

                WaveData CH_OutputVoltage = new WaveData();
                WaveData CH_BHM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref CH_OutputVoltage, "OutputVoltage");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 2, 1, ref CH_BHM, "BHM_Voltage");
                WaveData CH_K1K2State = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2State, "K1K2State");
                WaveData CH_CRM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 3, 1, ref CH_CRM, "CRM");
                WaveData CH_BRM = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 7, 1, ref CH_BRM, "BRM");

                dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                Thread.Sleep(1000 * 20);
                SendNoticeToUIAndTxtFile("判断充电状态");
                ProcessData();


                if (type == 0)
                {
                    Dictionary<int, string> dic = new Dictionary<int, string>();

                    SendNoticeToUIAndTxtFile("读取BHM报文时间点...");
                    //读取录波板数据
                    double Time_BHM = 0;
                    DataAnalysis_WaveRecoder.GetCANMsgTime(CH_BHM, 100, true, ref Time_BHM);
                    dic.Add(testWorkParam.lstIDs[0], (Time_BHM).ToString());
                    ProcessDataTmp(dic, StatusName, "BHM报文时间(ms)", "0", "-");


                    SendNoticeToUIAndTxtFile("读取输出稳定绝缘电压的值...");

                    //BHM报文电压(V)
                    double BHM_Voltage = DataAnalysis_WaveRecoder.GetWavePointVave(CH_BHM, (int)Time_BHM);
                    dic.Clear();
                    dic.Add(testWorkParam.lstIDs[0], BHM_Voltage.ToString());
                    ProcessDataTmp(dic, StatusName, "BHM报文电压(V)", "-", "-");


                    //输出稳定绝缘电压的值(V)
                    dic.Clear();
                    dic.Add(testWorkParam.lstIDs[0], DataAnalysis_WaveRecoder.GetWavePointMaxVave(CH_OutputVoltage).ToString());
                    double Y1 = LstChargerInfo[0].NominalVoltage >= BHM_Voltage ? BHM_Voltage : LstChargerInfo[0].NominalVoltage;
                    ProcessDataTmp(dic, StatusName, "输出稳定绝缘电压的值(V)", "-", (1.05 * Y1).ToString());


                    SendNoticeToUIAndTxtFile("读取K1和K2进行绝缘检测时间点...");
                    double K1K2s2 = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2State, 5);//用来判定K1K2的状态是上升还是下降
                    double Time_K1K2State = 0;
                    if (K1K2s2 > 2)
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2State, false, 6, ref Time_K1K2State);
                    }
                    else
                    {
                        DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2State, true, 6, ref Time_K1K2State);
                    }
                    dic.Clear();
                    dic.Add(testWorkParam.lstIDs[0], (Time_K1K2State).ToString());
                    ProcessDataTmp(dic, StatusName, "闭合K1和K2进行绝缘检测时间点(ms)", (Time_BHM ).ToString(), "-");


                    SendNoticeToUIAndTxtFile("读取CRM报文时间点...");
                    double Time_CRM = 0;//CRM报文时间点
                    DataAnalysis_WaveRecoder.GetCANMsgTime(CH_CRM, 100, true, ref Time_CRM);
                    dic.Clear();
                    dic.Add(testWorkParam.lstIDs[0], (Time_CRM ).ToString());
                    ProcessDataTmp(dic, StatusName, "CRM报文时间点(ms)", (Time_K1K2State).ToString(), "-");

                    ////BRM是车辆报文，这里不予判断
                    //SendNoticeToUIAndTxtFile("读取多包标识报文PGN...");
                    //double Time_BRM = 0;//CRM报文时间点
                    //DataAnalysis_WaveRecoder.GetCANMsgTime(CH_BRM, 0.5, true, ref Time_BRM);
                    //dic.Clear();
                    //dic.Add(testWorkParam.lstIDs[0], (Time_BRM ).ToString());
                    //ProcessDataTmp(dic, StatusName, "BRM报文时间点(ms)", (Time_CRM ).ToString(), "-");


                    CountDownTimeInfo("确认充电中充电枪插头可靠被锁止。\r\n(注:勾选上为可靠锁止)", 20, 2);
                    dic.Clear();
                    foreach (var item in DicManualVerifyResult)
                    {
                        dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                    }
                    ProcessDataTmp(dic, StatusName, "应可靠锁止", "-", "-");
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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

                    if (SelfCheckPhaseType == 0 || SelfCheckPhaseType == 1)
                    {
                        if (state > 4 && state <= 9)//>=3  or  >4 ????
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            //result = "允许充电";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            //result = "不允许充电";
                        }
                        //界面展示的数据项格式
                        //状态|数据名称|测量值|上限|下限|结果      
                        //自检阶段测试 - 开始前 >= 10V不正常电池电压时握手电压超上限
                        LstTrialData[i].ExtentData = StatusName
                        + "|" + "当前充电阶段（充电机应允许充电）"
                            + "|" + "-"
                            + "|" + "-"
                            + "|" + AllEquipStateData.DicBMS_DC_StateData[item].ChargingState
                            + "|" + sbtmp.ToString();
                    }
                    else
                    {
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
                        LstTrialData[i].ExtentData = StatusName
                        + "|" + "当前充电阶段（充电机应不允许充电）"
                            + "|" + "-"
                            + "|" + "-"
                            + "|" + AllEquipStateData.DicBMS_DC_StateData[item].ChargingState
                            + "|" + sbtmp.ToString();
                    }
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
