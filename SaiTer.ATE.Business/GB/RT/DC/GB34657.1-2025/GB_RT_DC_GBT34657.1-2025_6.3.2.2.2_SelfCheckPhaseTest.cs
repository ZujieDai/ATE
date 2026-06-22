using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    /// 自检阶段测试(录波板)
    /// </summary>
    public class GB_RT_DC_2025_SelfCheckPhaseTest : BusinessBase
    {
        private int SelfCheckPhaseType;
        private string StatusName = "";
        //private double HandshakeUpper = 150;    //握手电压超上限
        //private double HandshakeNoraml = 500;   //握手电压正常
        //private double HandshakeLower = 1050;   //握手电压超下限
        //private double ErrorBatteryUt = 100;    //非正常电池端电压
        //private double NormalBatteryUt = 390;   //正常电池端电压
        //private double wsdy握手电压 = 750;
        //private double dcdy电池电压 = 390;//自检阶段测试<10V握手电压低于充电机下限默认150V, >=10V时, 电池电压默认100V
        //private double dzfzdl电子负载电流 = 5000;
        private Dictionary<int, string> dicImagePath = new Dictionary<int, string>();

        public GB_RT_DC_2025_SelfCheckPhaseTest(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
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
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
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
                for (int i = 0; i < 2; i++)
                {
                    SelfCheckPhaseType = i;
                    TrialMethod(i);
                }
            }
        }

        /// <summary>
        /// 0.正常电池电压--报文最高允许充电电压范围内
        /// 1.正常电池电压--报文最高允许充电电压超上限
        /// </summary>
        /// <param name="type"></param>
        private void TrialMethod(int type)
        {

            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                double HandshakeUpper = LstChargerInfo[i].NominalVoltage;    //握手电压超上限

                double NormalBatteryUt = 390;   //正常电池端电压

                List<bool> Ks = GetKStatus16_Charging_DC();
                SendNoticeToUIAndTxtFile("关闭BMS");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
                SetCPReresh();
                Ks[26] = false;//关闭输出过压控制

                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                Thread.Sleep(1000);
                //绝缘前设置握手电压和充电电流
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 30, HandshakeUpper, LstChargerInfo[i].NominalCurrent);
                Thread.Sleep(100);

                //Ks[16] = true;
                Ks[26] = true;//26输出过压控制，XJ现场不输出电压无法检测到电池反接

                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, HandshakeUpper, 30, Ks.ToArray());

                d1 = new Dictionary<int, string>();

                WaitDCVoltage(lstIDs, 30,10);
                d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));

                SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                //Ks[16] = true;
              

                //根据不同的type设置不同的握手电压,以及相应的BHM和BCP报文最高允许充电电压
                switch (type)
                {
                    case 0:
                        StatusName = "正常电池电压（报文最高允许充电电压范围内）";
                        // 1. 握手阶段
                        ControlEquipMent.BMS.SetParameter(lstIDs, HandshakeUpper);
                        Thread.Sleep(200);

                        // 2. 参数配置
                        ControlEquipMent.BMS.SetParameter(lstIDs, 390, HandshakeUpper, 50);

                        Thread.Sleep(200);
                        // 3. 充电需求
                        ControlEquipMent.BMS.SetParameter(lstIDs, 500, 50, true, 390);
                        Thread.Sleep(200);
                        break;

                    case 1:
                        StatusName = "正常电池电压（报文最高允许充电电压超上限）";
                        // 1. 握手阶段
                        ControlEquipMent.BMS.SetParameter(lstIDs, HandshakeUpper + 500);
                        Thread.Sleep(200);

                        // 2. 参数配置
                        ControlEquipMent.BMS.SetParameter(lstIDs, 390, HandshakeUpper, 50);

                        Thread.Sleep(200);
                        // 3. 充电需求
                        ControlEquipMent.BMS.SetParameter(lstIDs, 500, 50, true, 390);
                        Thread.Sleep(200);


                        break;
                }

                ProcessDataTmp(d1, StatusName, "绝缘检测前外侧电压(V)", "0", "60");


                try
                {

                    SendNoticeToUIAndTxtFile("启动录波板");
                    //启动录波板
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    SendNoticeToUIAndTxtFile("等待刷卡");

                    MessgaeInfo(true, "请刷卡充电!，进入充电状态自动关闭!");


                    if (!WaitChargingSrat(200))
                    {
                        SendNoticeToUIAndTxtFile("200S未能进入充电！！");
                    }
                    //停止录波板 
                    SendNoticeToUIAndTxtFile("停止录波板");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                    MessgaeInfo(false, "请刷卡充电!，进入充电状态自动关闭!");




                    // 读取充电电压波形
                    WaveData outputVoltageWave = new WaveData();
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref outputVoltageWave, "充电电压");


                    double z1 = 0;
                    double z2 = 0;

                    DataAnalysis_WaveRecoder.GetDCSingleTime(outputVoltageWave, true, HandshakeUpper - 5, ref z1);
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(outputVoltageWave, false, HandshakeUpper - 10, ref z1, z1);
                    DataAnalysis_WaveRecoder.GetDCSingleTime_M(outputVoltageWave, false, 60, ref z2, z1);
                    SetWaveRecorderCursor(z1, z2);
                    var dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
                    // 读取时间差
                    var cursorTimes = ControlEquipMent.WaveRecoderCtrl.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
                    var timeDiffData = cursorTimes.ToDictionary(k => k.Key, v => v.Value.ToString("F0"));

                    ProcessDataTmp(timeDiffData, StatusName, "绝缘电压下降时间", "1", "1000", dicImagePath);

                    Dictionary<int, string> dic = new Dictionary<int, string>();


                    SendNoticeToUIAndTxtFile("读取输出稳定绝缘电压的值...");
                    //输出稳定绝缘电压的值(V)
                    dic.Clear();
                    dic.Add(testWorkParam.lstIDs[0], DataAnalysis_WaveRecoder.GetWavePointMaxVave(outputVoltageWave).ToString());

                    ProcessDataTmp(dic, StatusName, "输出稳定绝缘电压的值(V)", "-", "-");
                    VerifyProtectionLogic(lstIDs, StatusName);

                    CountDownTimeInfo("确认充电中充电枪插头可靠被锁止。\r\n(注:勾选上为可靠锁止)", 20, 2);
                    dic.Clear();
                    foreach (var item in DicManualVerifyResult)
                    {
                        dic.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                    }
                    ProcessDataTmp(dic, StatusName, "应可靠锁止", "-", "-");



                }
                catch (Exception ex) { Log.Log.LogException(ex); }
            }
        }
        /// <summary>
        /// 设置录波光标
        /// </summary>
        private void SetWaveRecorderCursor(double timeStart, double timeEnd)
        {
            if (ControlEquipMent.WaveRecoderCtrl == null) return;

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, timeStart);
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, timeEnd);
        }
        private void VerifyProtectionLogic(List<int> chargerIds, string testScene)
        {
            string testTitle = $"{testScene}";
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();

            if (bmsData == null)
            {
                SendNoticeToUIAndTxtFile("BMS数据获取失败，测试中止");
                return;
            }
            // 3. C1/C2 断开（电压＜60V）
            var c1c2Result = new Dictionary<int, bool> { { 1, bmsData.ChargingVoltage > 60 } };
            ProcessDataResults(chargerIds, c1c2Result[1] ? "闭合" : "断开", "闭合", c1c2Result, testTitle + " C1C2状态");

            // 4. S3/S4 断开（电压＜5V）
            var s3s4Result = new Dictionary<int, bool> { { 1, bmsData.APSVoltage > 5 } };
            ProcessDataResults(chargerIds, s3s4Result[1] ? "闭合" : "断开", "闭合", s3s4Result, testTitle + " S3S4状态");


            var commResult = new Dictionary<int, bool> { { 1, bmsData.ChargingVoltage > 60 } };
            ProcessDataResults(chargerIds, "正常" , "正常", commResult, testTitle + " 通讯状态");

        }
        private bool WaitChargingSrat(int timeout)
        {
            for (int i = 0; i < timeout; i++)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value).First().Value.ChargingState;

                if (bmsData == "充电中")
                {
                    SendNoticeToUIAndTxtFile("充电桩以进入充电");

                    return true;

                }

                System.Threading.Thread.Sleep(1000);
            }
            SendNoticeToUIAndTxtFile("充电桩以进入充电超时"+ timeout);

            return false;

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


                    if (dicImagePath != null&& dicImagePath.ContainsKey(i))
                    {
                        sbtmp.Append(dicImagePath[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    string result = "";
                    var ChargingState = AllEquipStateData.DicBMS_DC_StateData.First().Value.ChargingState;
                    int state = ChangeBMSChargeStatus(ChargingState);

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
                        + "|" + ChargingState
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
 
        }

    }
}
