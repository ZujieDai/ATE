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
    /// 国标直流研测：充电机自检
    /// </summary>
    public class GB_RT_DC_ChargerDevSelfCheck : BusinessBase
    {
        private int SelfCheckPhaseType;
        private string StatusName = "";
        public GB_RT_DC_ChargerDevSelfCheck(int trialType) { TrialType = trialType; }
        private double ErrorBatteryUt = 100;    //非正常电池端电压
        private double NormalBatteryUt = 390;   //正常电池端电压
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();
        /// <summary>
        /// 短路装置
        /// </summary>
        int ShortCircuitChargeID = 2;
        List<int> MylstIDs = new List<int>();

        public void OscilloscopeInit()
        {
            int time = 50;
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20M", Channel1, "Output_V", "1M", "V", false, "300", "0");
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, false, "DC", "20M", Channel2, "Output_I", "1M", "A", false, "50", "0");//通道2设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "AC", "20M", Channel3, "Input_I", "1M", "A", false, "100", "0");//通道3设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope?.Oscilloscope_Channel_Set(lstIDs, 4, false, "AC", "20M", Channel4, "Input_AC_V", "1M", "V", false, "100", "0");//通道4设置
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "1000", "0");//设置滚动，时基和触发延时
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//初始化测量
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 1);
            Thread.Sleep(time);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(time);
        }

        public override void InitializeParams()
        {
            Init();

            //正常电池端电压(V)=390|非正常电池端电压(V)=100
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                NormalBatteryUt = Convert.ToDouble(strParams[0].Split('=')[1]);
                ErrorBatteryUt = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
            MylstIDs = new List<int>();
            string[] strItems = TrialItem.ItemParams.Split('|');
            if (strItems.Length >= 1)
            {
                double value = double.Parse(strItems[0].Split('=')[1]);
                ShortCircuitChargeID = Convert.ToInt32(value);
                MylstIDs.Add(ShortCircuitChargeID);
            }
        }

        public override void InitEquiMent()
        {
            OscilloscopeInit();
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
                for (int i = 0; i < 4; i++)
                {
                    SetCPReresh();
                    if (i == 3)
                    {
                        StatusName = "短路";

                        CountDownTimeInfo("请人工确认是否具备短路保护功能,\r\n （勾选上代表具有）", 100, 2);
                        if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
                        {
                            ProcessDataResult(testWorkParam.lstIDs, "是", "是否具备短路保护功能", true, "短路");
                        }
                        else
                        {
                            ProcessDataResult(testWorkParam.lstIDs, "否", "是否具备短路保护功能", false, "短路");
                            continue;
                        }

                        CountDownTimeInfo("请确认充电枪插头插入到短路装置!!!", 99999, 0);

                        SendNoticeToUIAndTxtFile("开启导引中");

                        if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(ShortCircuitChargeID))
                        {
                            Dictionary<int, string> datas = new Dictionary<int, string>();
                            datas.Add(ShortCircuitChargeID, "不存在该枪号");
                            ProcessDataTmpThis(MylstIDs, datas, TrialItem.ItemName, "结果", "-", "-");
                            return;
                        }

                        ControlEquipMent.BMS.BMS_OFF(MylstIDs);
                        Thread.Sleep(200);
                        ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage);
                        Thread.Sleep(200);
                        ControlEquipMent.BMS.SetParameter(MylstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                        Thread.Sleep(200);
                        ControlEquipMent.BMS.SetParameter(MylstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);

                        SendNoticeToUIAndTxtFile("设置示波器触发模式");
                        ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, "200", "Single");

                        d1 = new Dictionary<int, string>();
                        for (int j = 0; j < MylstIDs.Count; j++)
                        {
                            //外侧电压具体是模拟过压后的导引充电电压
                            d1.Add(MylstIDs[j], AllEquipStateData.DicBMS_DC_StateData[MylstIDs[j]].ChargingVoltage.ToString("F2"));
                        }
                        ProcessDataTmp(d1, StatusName, "绝缘检测前外侧电压(V)", "-", "-");

                        CountDownTimeInfo("请手动设置短路测试仪模拟短路故障!!!", 999, 0);
                        Thread.Sleep(200);
                        ControlEquipMent.BMS.BMS_ON(MylstIDs);

                        Thread.Sleep(2000);
                        MessgaeInfo(true, "请刷卡充电!", true);
                        int timeout = 200;
                        while (timeout-- > 0)
                        {
                            var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => MylstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                            if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                                continue;
                            bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                            if (ALLCanCharge)
                            {
                                break;
                            }

                            System.Threading.Thread.Sleep(1000);
                        }
                        MessgaeInfo(false, "请刷卡充电!");
                        WaitDCVoltage(MylstIDs, LstChargerInfo[0].NominalVoltage);
                        Thread.Sleep(5000);

                        SendNoticeToUIAndTxtFile("判断充电状态");
                        ProcessData();

                        SendNoticeToUIAndTxtFile("关闭导引中!");
                        ControlEquipMent.BMS.BMS_OFF(MylstIDs);

                        CountDownTimeInfo("请恢复充电机正常！！", 999, 0);
                        CountDownTimeInfo("请确认充电枪插头插回到之前导引装置上!!!", 99999, 0);
                    }
                    else
                    {
                        SelfCheckPhaseType = i;
                        TrialMethod(i);
                    }
                }
            }
        }

        private void TrialMethod(int type)
        {
            switch (type)
            {
                case 0:
                    StatusName = "外侧电压低于60V DC";
                    break;
                case 1:
                    StatusName = "外侧电压大于60V DC";
                    break;
                case 2:
                    StatusName = "触点粘连";
                    break;
                case 3:
                    StatusName = "短路";
                    break;
            }
            SendNoticeToUIAndTxtFile("关闭BMS,设置录波仪参数");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            SetCPReresh();

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
            if (type == 1)
            {
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, ErrorBatteryUt);
                Thread.Sleep(100);
                //过压控制开关
                var kstate = GetKStatus16_Charging_DC();
                kstate[26] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, NormalBatteryUt, kstate.ToArray());
                Thread.Sleep(100);
            }
            else
            {
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, NormalBatteryUt);
                Thread.Sleep(100);
            }

            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
            Thread.Sleep(100);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, NormalBatteryUt, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
            Thread.Sleep(100);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent, false, MaxAllowChargeVoltage);
            Thread.Sleep(100);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            Thread.Sleep(100);
            ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
            Thread.Sleep(100);

            try
            {
                SendNoticeToUIAndTxtFile("设置示波器触发模式");
                ControlEquipMent.Oscilloscope?.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, "200", "Single");
                if (type == 2)
                    CountDownTimeInfo("请人工模拟粘连！！", 999, 0);
                else if (type == 3)
                    CountDownTimeInfo("请人工短路！！", 999, 0);

                d1 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    //外侧电压具体是模拟过压后的导引充电电压
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                }
                ProcessDataTmp(d1, StatusName, "绝缘检测前外侧电压(V)", "-", "-");

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

                if (type < 2)
                {
                    SendNoticeToUIAndTxtFile("等待触发中...");
                    timeout = 15;
                    bool istrigger = false;
                    while (timeout-- > 0)
                    {
                        istrigger = ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs).FirstOrDefault().Value;
                        if (istrigger)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }
                dicImagePath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                if(type == 0)
                    Thread.Sleep(1000 * 6);
                Thread.Sleep(1000 * 4);
                SendNoticeToUIAndTxtFile("判断充电状态");
                ProcessData();

                if (type >= 2)
                {
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(200);
                    CountDownTimeInfo("请恢复充电机正常！！", 999, 0);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
            try
            {
                var lstChargerIDs = SelfCheckPhaseType == 3 ? MylstIDs : testWorkParam.lstIDs;
                foreach (var item in lstChargerIDs)
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

                    if (SelfCheckPhaseType == 0)
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
