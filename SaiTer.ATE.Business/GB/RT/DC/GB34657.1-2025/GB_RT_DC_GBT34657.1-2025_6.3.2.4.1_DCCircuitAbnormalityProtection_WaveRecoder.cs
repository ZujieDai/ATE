using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 直流供电回路异常保护测试(录波板)
    /// 符合GB/T34657.1—2025 充电结束阶段测试要求
    /// </summary>
    public class GB_RT_DC_2025_DCCircuitAbnormalityProtection_WaveRecoder : BusinessBase
    {
        #region 常量定义（统一管理硬编码值）
        /// <summary>
        /// 测试超时时间(秒)
        /// </summary>
        private const int TestTimeOutSeconds = 30;

        /// <summary>
        /// 默认需求电压(V)
        /// </summary>
        private const double DefaultDemandVoltage = 500;

        /// <summary>
        /// 默认负载电流(A)
        /// </summary>
        private const int DefaultDemandCurrent = 30;

        /// <summary>
        /// 安全电压阈值(V) - 符合GB/T34657.1—2025要求
        /// </summary>
        private const double SafeVoltageThreshold = 60;

        /// <summary>
        /// 辅源电压判定阈值(V)
        /// </summary>
        private const double AuxPowerVoltageThreshold = 8;

        /// <summary>
        /// 电压判定偏移值(V)
        /// </summary>
        private const double VoltageJudgeOffset = 5;

        /// <summary>
        /// 电压泄放等待最大次数(每次1秒)
        /// </summary>
        private const int VoltageDischargeMaxWaitTimes = 80;

        /// <summary>
        /// 录波板采样率(1k/s)
        /// </summary>
        private const int WaveRecorderSamplingRate = 1;
        #endregion

        #region 私有字段
        /// <summary>
        /// 需求电压
        /// </summary>
        private double _demandVoltage;
        private double _demandCurrent;

        /// <summary>
        /// 临时测试数据
        /// </summary>
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();

        /// <summary>
        /// 示波器光标卡点时间
        /// </summary>
        private readonly Dictionary<int, double[]> _oscCursorTimes = new Dictionary<int, double[]>();
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">试验类型</param>
        public GB_RT_DC_2025_DCCircuitAbnormalityProtection_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 初始化参数
        /// </summary>
        public override void InitializeParams()
        {
            Init();

            // 解析测试参数：BMS需求电压设置(V)=500|负载电流设置(A)=30
            if (!string.IsNullOrEmpty(TrialItem.ResultParams))
            {
                var paramParts = TrialItem.ResultParams.Split('|');
                if (paramParts.Length >= 2)
                {
                    _demandVoltage = ParseParamValue(paramParts[0], DefaultDemandVoltage);
                    // 如需解析电流可启用：var demandCurrent = ParseParamValue(paramParts[1], DefaultDemandCurrent);
                }
                else
                {
                    _demandVoltage = DefaultDemandVoltage;
                }
            }
            else
            {
                _demandVoltage = DefaultDemandVoltage;
            }

            LogNotice($"参数初始化完成，需求电压：{_demandVoltage}V");

            SetCPReresh();
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        public override void InitEquiMent()
        {
            LogNotice("设备初始化中...");

            // 统一停止BMS
            ControlEquipMent.BMS.BMS_OFF(lstIDs);

            // 模拟插拔枪
            RefreshChargingGunState();

            LogNotice("设备初始化完成");
        }

        /// <summary>
        /// 执行测试方法
        /// </summary>
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                ExecuteTestFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
                LogError($"测试执行异常：{ex.Message}", ex);
            }
            finally
            {
                // 恢复K状态
                ResetKState();

                // 保存试验结果
                SaveTrialResult();

                // 日志记录
                LogNotice($"{TrialItem.ItemName}结束---------------------->");

                // 发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        /// <summary>
        /// 执行测试流程
        /// </summary>
        private void ExecuteTestFlow()
        {
            try
            {
                LogNotice($"开始{TrialItem.ItemName}--------------------------->");

                _StopWatch.Reset();
                _StopWatch.Start();

                while (true)
                {
                    // 筛选待测试的充电桩ID
                    RefreshTestChargerIds();

                    // 所有测试完成则退出循环
                    if (testWorkParam.lstIDs.Count <= 0) break;

                    // 超时处理
                    if (IsTestTimeout())
                    {
                        HandleTestTimeout();
                        break;
                    }

                    #region 车桩通信参数配置
                    Thread.Sleep(200);
                    SendNoticeToUIAndTxtFile("设置BMS参数，并启动充电");

                    // 配置BMS基础参数（标称电压）
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);

                    // 配置BMS辅助参数（390V/标称电压/250A）
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);

                    // 配置BMS充电需求参数（核心需求电压/电流）
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, 30, true, _demandVoltage);
                    Thread.Sleep(300);
                    #endregion

                    #region 接触器C1和C2发生粘连故障
                    string sName = "接触器C1和C2发生粘连故障";
                    CountDownTimeInfo("请模拟接触器C1和C2发生粘连故障，完成后点击确定!", 99999, 0);

                    // 启动BMS
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(200);

                    var isChargeReady = WaitChargeReady(100);
                    if (!isChargeReady)
                    {
                        SendNoticeToUIAndTxtFile("充电就绪超时，测试终止");
                        MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                    }
                    Thread.Sleep(3000);
                    //检测点1电压值
                    Dictionary<int, string> CC1VoltageValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(CC1VoltageValue, sName, "检测点1电压值CC1电压", "-", "-");

                    // C1C2状态
                    Dictionary<int, EmTrialResult> chargeC1C2Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    Dictionary<int, string> C1C2Result = chargeC1C2Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, C1C2Result, "C1C2状态", chargeC1C2Result, sName, "断开", "断开");

                    //S3S4状态
                    Dictionary<int, EmTrialResult> chargeS3S4Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < 5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    Dictionary<int, string> S3S4Result = chargeS3S4Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, S3S4Result, "S3S4状态", chargeS3S4Result, sName, "断开", "断开");

                    //是否启动充电
                    Dictionary<int, EmTrialResult> chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    Dictionary<int, string> stateResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
                    ProcessDataResults(testWorkParam.lstIDs, stateResult, "充电状态", chargeStateResult, sName, "停止充电", "停止充电");

                    //通讯是否正常（有检测到刷卡就是通讯正常，没检测到就是不正常）
                    Dictionary<int, EmTrialResult> chargeReadyResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => isChargeReady ? EmTrialResult.Pass : EmTrialResult.Fail);
                    Dictionary<int, string> readyResult = chargeReadyResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "不正常" : "正常");
                    ProcessDataResults(testWorkParam.lstIDs, readyResult, "通讯状态", chargeReadyResult, sName, "不正常", "不正常");

                    //电子锁是否正常
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);                   
                    ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "正常解锁" : "不能解锁", "电子锁状态", DicManualVerifyResult, sName);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    // 基础设备状态重置
                    ResetTestEquipmentState();
                    #endregion

                    #region 直流供电回路发生短路故障
                    sName = "直流供电回路发生短路故障";
                    CountDownTimeInfo("请模拟直流供电回路发生短路故障，完成后点击确定!", 99999, 0);
                    // 启动BMS
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(200);

                     isChargeReady = WaitChargeReady(100);
                    if (!isChargeReady)
                    {
                        SendNoticeToUIAndTxtFile("充电就绪超时，测试终止");
                        MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                    }
                    Thread.Sleep(5000);
                    //检测点1电压值
                    CC1VoltageValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(CC1VoltageValue, sName, "检测点1电压值CC1电压", "-", "-");

                    // C1C2状态
                    chargeC1C2Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    C1C2Result = chargeC1C2Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, C1C2Result, "C1C2状态", chargeC1C2Result, sName, "断开", "断开");

                    //S3S4状态
                    chargeS3S4Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < 5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    S3S4Result = chargeS3S4Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, S3S4Result, "S3S4状态", chargeS3S4Result, sName, "断开", "断开");

                    //是否启动充电
                    chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    stateResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
                    ProcessDataResults(testWorkParam.lstIDs, stateResult, "充电状态", chargeStateResult, sName, "停止充电", "停止充电");

                    //通讯是否正常（有检测到刷卡就是通讯正常，没检测到就是不正常）
                    chargeReadyResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => isChargeReady ? EmTrialResult.Pass : EmTrialResult.Fail);
                    readyResult = chargeReadyResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "不正常" : "正常");
                    ProcessDataResults(testWorkParam.lstIDs, readyResult, "通讯状态", chargeReadyResult, sName, "不正常", "不正常");

                    //通讯是否正常
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
                    ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "正常解锁" : "不能解锁", "电子锁状态", DicManualVerifyResult, sName);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    // 基础设备状态重置
                    ResetTestEquipmentState();

                    #endregion

                    #region 车辆侧充电回路电压异常(如模拟电池侧电压绝对值超过60VDC)
                    sName = "车辆侧充电回路电压异常";
                    var Ks = GetKStatus16_Charging_DC();
                    Ks[26] = true;//26输出过压控制，
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 100, Ks.ToArray());
                    Thread.Sleep(200);
                    // 启动BMS
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(200);

                     isChargeReady = WaitChargeReady(100);
                    if (!isChargeReady)
                    {
                        SendNoticeToUIAndTxtFile("充电就绪超时，测试终止");
                        MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                        return;
                    }
                    Thread.Sleep(5000);
                    //检测点1电压值
                    CC1VoltageValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(CC1VoltageValue, sName, "检测点1电压值CC1电压", "-", "-");

                    // C1C2状态
                    chargeC1C2Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    C1C2Result = chargeC1C2Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, C1C2Result, "C1C2状态", chargeC1C2Result, sName, "断开", "断开");

                    //S3S4状态
                    chargeS3S4Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < 5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    S3S4Result = chargeS3S4Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, S3S4Result, "S3S4状态", chargeS3S4Result, sName, "断开", "断开");

                    //是否启动充电
                    chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    stateResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
                    ProcessDataResults(testWorkParam.lstIDs, stateResult, "充电状态", chargeStateResult, sName, "停止充电", "停止充电");

                    //通讯是否正常（有检测到刷卡就是通讯正常，没检测到就是不正常）
                    chargeReadyResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => isChargeReady ? EmTrialResult.Pass : EmTrialResult.Fail);
                    readyResult = chargeReadyResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "不正常" : "正常");
                    ProcessDataResults(testWorkParam.lstIDs, readyResult, "通讯状态", chargeReadyResult, sName, "不正常", "不正常");

                    //通讯是否正常
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
                    ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "正常解锁" : "不能解锁", "电子锁状态", DicManualVerifyResult, sName);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    // 基础设备状态重置
                    ResetTestEquipmentState();
                    Ks[26] = false;//26输出过压控制，
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 100, Ks.ToArray());
                    #endregion

                    #region 车辆端绝缘监测允许总电压低于直流供电设备最低充电电压
                    sName = "车辆端绝缘监测允许总电压低于直流供电设备最低充电电压";
                    ControlEquipMent.BMS.SetParameter(lstIDs, 50);
                    Thread.Sleep(100);
                    // 启动BMS
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(200);

                    isChargeReady = WaitChargeReady(100);
                    if (!isChargeReady)
                    {
                        SendNoticeToUIAndTxtFile("充电就绪超时，测试终止");
                        MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                        return;
                    }
                    Thread.Sleep(5000);
                    //检测点1电压值
                    CC1VoltageValue = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(CC1VoltageValue, sName, "检测点1电压值CC1电压", "-", "-");

                    // C1C2状态
                    chargeC1C2Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    C1C2Result = chargeC1C2Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, C1C2Result, "C1C2状态", chargeC1C2Result, sName, "断开", "断开");

                    //S3S4状态
                    chargeS3S4Result = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < 5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    S3S4Result = chargeS3S4Result.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(testWorkParam.lstIDs, S3S4Result, "S3S4状态", chargeS3S4Result, sName, "断开", "断开");

                    //是否启动充电
                    chargeStateResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => ChangeBMSChargeStatus(x.Value.ChargingState) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    stateResult = chargeStateResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "停止充电" : "允许充电");
                    ProcessDataResults(testWorkParam.lstIDs, stateResult, "充电状态", chargeStateResult, sName, "停止充电", "停止充电");

                    //通讯是否正常（有检测到刷卡就是通讯正常，没检测到就是不正常）
                    chargeReadyResult = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => isChargeReady ? EmTrialResult.Pass : EmTrialResult.Fail);
                    readyResult = chargeReadyResult.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "不正常" : "正常");
                    ProcessDataResults(testWorkParam.lstIDs, readyResult, "通讯状态", chargeReadyResult, sName, "不正常", "不正常");

                    //通讯是否正常
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
                    ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "正常解锁" : "不能解锁", "电子锁状态", DicManualVerifyResult, sName);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    // 基础设备状态重置
                    ResetTestEquipmentState();

                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogError($"测试流程执行异常：{ex.Message}", ex);
            }
        }

     

        #region 辅助方法
        /// <summary>
        /// 解析参数值
        /// </summary>
        /// <param name="paramStr">参数字符串</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>解析后的值</returns>
        private double ParseParamValue(string paramStr, double defaultValue)
        {
            try
            {
                var parts = paramStr.Split('=');
                if (parts.Length == 2 && double.TryParse(parts[1], out var value))
                {
                    return value;
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 刷新待测试充电桩ID列表
        /// </summary>
        private void RefreshTestChargerIds()
        {
            testWorkParam.lstIDs.Clear();

            foreach (var trialData in LstTrialData)
            {
                if (trialData.IsCheck && trialData.TrialResult == EmTrialResult.Wait &&
                    !testWorkParam.lstIDs.Contains(trialData.ChargerId))
                {
                    testWorkParam.lstIDs.Add(trialData.ChargerId);
                }
            }
        }

        /// <summary>
        /// 判断测试是否超时
        /// </summary>
        /// <returns>是否超时</returns>
        private bool IsTestTimeout()
        {
            return _StopWatch.ElapsedMilliseconds / 1000 > TestTimeOutSeconds;
        }

        /// <summary>
        /// 处理测试超时
        /// </summary>
        private void HandleTestTimeout()
        {
            LogWarning($"测试超时（{TestTimeOutSeconds}秒），标记为失败");

            foreach (var trialData in LstTrialData)
            {
                if (trialData.IsCheck && trialData.TrialResult == EmTrialResult.Wait)
                {
                    trialData.TrialResult = EmTrialResult.Fail;
                    trialData.TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();

                    var chargerIndex = LstChargerInfo.FindIndex(s => s.ChargerId == trialData.ChargerId);
                    if (chargerIndex >= 0)
                    {
                        trialData.PKID = LstChargerInfo[chargerIndex].PKID;
                    }

                    trialData.ExtentData = "-|-|-|-|null";
                    SendTrialDataToUI(trialData);
                }
            }
        }

        /// <summary>
        /// 重置测试设备状态
        /// </summary>
        private void ResetTestEquipmentState()
        {
            SetLoadDCOFF(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            // 模拟插拔枪
            Thread.Sleep(200);
            RefreshChargingGunState();
            LogNotice("测试设备状态已重置");
        }

        /// <summary>
        /// 重置K状态
        /// </summary>
        private void ResetKState()
        {
            var ks = GetKStatus16_Charging_DC();
            ks[22] = true;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, ks.ToArray());
            LogNotice("K状态已重置");
        }
        /// <summary>
        /// 等待充电就绪（BMS状态变为充电中）
        /// </summary>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>是否就绪</returns>
        private bool WaitChargeReady(int timeout)
        {
            SendNoticeToUIAndTxtFile($"等待充电就绪，超时{timeout}秒");
            MessgaeInfo(true, "请刷卡后等待系统自动判断...");
            while (timeout-- > 0)
            {
                // 筛选当前测试设备的BMS状态
                var bmsData = AllEquipStateData.DicBMS_DC_StateData
                    .Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID))
                    .ToDictionary(bms => bms.Key, bms => bms.Value);

                if (bmsData.Count < 1 || bmsData.Values.FirstOrDefault() == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // 打印当前充电状态（调试用）
                Console.WriteLine($"当前BMS充电状态：{bmsData.First().Value.ChargingState}");

                // 所有设备充电状态变为9（可充电）则就绪
                bool allCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 2);
                if (allCanCharge)
                {
                    MessgaeInfo(false, "");
                    SendNoticeToUIAndTxtFile("所有设备充电就绪");
                    return true;
                }

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
            SendNoticeToUIAndTxtFile("充电就绪超时");
            return false;
        }
        /// <summary>
        /// 开启导引并检查刷卡状态
        /// </summary>
        /// <returns>是否成功</returns>
        private bool StartGuidanceAndCheckCard()
        {
            LogNotice("开启导引中");

            if (!CheckSwipingCard(testWorkParam.lstIDs))
            {
                LogError("刷卡验证失败，终止测试");
                return false;
            }

            return true;
        }
        /// <summary>
        /// 批量标记测试结果
        /// </summary>
        /// <param name="result">结果类型</param>
        /// <param name="value">结果值</param>
        private void MarkTrialResult(EmTrialResult result, string value)
        {
            foreach (var trialData in LstTrialData.Where(d => d.IsCheck && d.TrialResult == EmTrialResult.Wait))
            {
                trialData.TrialResult = result;
                trialData.TrialValue = value;
                int k = LstChargerInfo.FindIndex(s => s.ChargerId == trialData.ChargerId);
                trialData.PKID = LstChargerInfo[k].PKID;
                trialData.ExtentData = "null|null|null|null|null";
                SendTrialDataToUI(trialData);
            }
        }

        enum EChannelType
        {
            充电电压=1,
            充电电流=2,
            辅源电压=3,
            CC1电压=4,
            CC2电压=5,
            前端电压=6,
            C1C2=8
        }



        /// <summary>
        /// 刷新充电枪状态（原SetCPReresh方法）
        /// </summary>
        private void RefreshChargingGunState()
        {
            SetCPReresh();
        }

        /// <summary>
        /// 日志记录（统一UI和文件输出）
        /// </summary>
        /// <param name="message">日志信息</param>
        private void LogNotice(string message)
        {
            SendNoticeToUIAndTxtFile(message);
        }

        /// <summary>
        /// 错误日志记录
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常对象</param>
        private void LogError(string message, Exception ex = null)
        {
            SendNoticeToUIAndTxtFile($"【错误】{message}");
            if (ex != null)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 警告日志记录
        /// </summary>
        /// <param name="message">警告信息</param>
        private void LogWarning(string message)
        {
            SendNoticeToUIAndTxtFile($"【警告】{message}");
        }
        #endregion

        /// <summary>
        /// 处理数据（空实现保持兼容）
        /// </summary>
        public override void ProcessData()
        {
        }
    }
}