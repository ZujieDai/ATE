using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static SaiTer.ATE.DataModel.SwitchSource;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 正常充电结束测试(录波板)
    /// 符合GB/T34657.1—2025 充电结束阶段测试要求
    /// </summary>
    public class GB_RT_DC_2025_NormalChargingEndTest_WaveRecoder : BusinessBase
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
        public GB_RT_DC_2025_NormalChargingEndTest_WaveRecoder(int type)
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
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        public override void InitEquiMent()
        {
            LogNotice("设备初始化中...");

            // 统一停止BMS
            ControlEquipMent.BMS.BMS_OFF(lstIDs);

            // 设置录波板采样率
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, WaveRecorderSamplingRate);

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

                    // 基础设备状态重置
                    ResetTestEquipmentState();

                    // 执行车端中止充电测试
                    ExecuteVehicleSideStopChargingTest();

                    // 执行桩端主动停充测试
                    ExecutePileSideActiveStopChargingTest();

                    //// 关闭负载
                    //ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(testWorkParam.lstIDs);
                    //LogNotice("关闭负载完成!");
                }
            }
            catch (Exception ex)
            {
                LogError($"测试流程执行异常：{ex.Message}", ex);
            }
        }

        #region 核心测试流程
        /// <summary>
        /// 执行车端中止充电测试
        /// </summary>
        private void ExecuteVehicleSideStopChargingTest()
        {

            LogNotice("开始执行车端中止充电测试");

            LogNotice("启动充电中");

            // 开启导引并刷卡验证
            if (!StartGuidanceAndCheckCard()) return;

            // 充电中
            ExecuteVehicleStopChargingInProgressTest();

            // 充电结束
            ExecuteVehicleStopChargingAfterCompleteTest();


            // 刷新充电枪状态
            RefreshChargingGunState();

            LogNotice("车端中止充电测试执行完成");
        }

        /// <summary>
        /// 执行桩端主动停充测试
        /// </summary>
        private void ExecutePileSideActiveStopChargingTest()
        {
            LogNotice("开始执行桩端主动停充测试");

            // 开启导引并刷卡验证
            if (!StartGuidanceAndCheckCard()) return;

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
            //获取继电器状态
            ReadWaveRecorderChannelData();

            // 保存录波截图
            var chargingStatusImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            // 读取并记录接触器状态
            RecordContactorStatus("桩中止充电(充电中)", chargingStatusImgs);

            // 等待手动停止充电
            WaitManualStopCharging();
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 停止录波并分析数据
            //ReadWaveRecorderChannelData();  
            // 保存录波截图
            chargingStatusImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            // 分析电压变化时间
            double timeStart = 0, timeEnd = 0;
            AnalyzeVoltageChangeTime(ReadWaveRecorderChannelData()[EChannelType.充电电压], ref timeStart, ref timeEnd);

            // 设置录波板光标
            SetWaveRecorderCursor(timeStart, timeEnd);

            Thread.Sleep(1000);

            // 再次读取接触器状态
            RecordContactorStatus("桩中止充电(充电结束)", chargingStatusImgs);

            // 重置设备状态
            ResetTestEquipmentState();

            // 分析并记录时间差数据
            AnalyzeAndRecordTimeDifference("主动终止充电");

            // 确认电子锁解锁状态
            ConfirmElectronicLockUnlockStatus();

            // 处理连接数据
            ProcessDataConnect("主动停止充电");

            // 关闭导引
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            LogNotice("关闭导引完成!");

            // 刷新充电枪状态
            RefreshChargingGunState();

            LogNotice("桩端主动停充测试执行完成");
        }
        #endregion

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
        /// 开启导引并检查刷卡状态
        /// </summary>
        /// <returns>是否成功</returns>
        private bool StartGuidanceAndCheckCard()
        {

            LogNotice("开启导引中");

            if (!CheckSwipingCard(testWorkParam.lstIDs,_demandVoltage, DefaultDemandCurrent, 750))
            {
                LogError("刷卡验证失败，终止测试");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行充电中
        /// </summary>
        private void ExecuteVehicleStopChargingInProgressTest()
        {
            // 启动录波板
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
            Thread.Sleep(3000);
            ReadWaveRecorderChannelData();
            // 保存录波截图
            var chargingStatusImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            // 读取并记录接触器状态

            RecordContactorStatus("车端中止充电(充电中)", chargingStatusImgs);
            Thread.Sleep(1000);
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);

            LogNotice("充电中车端中止测试完成");
        }

        /// <summary>
        /// 执行充电结束车端中止测试
        /// </summary>
        private void ExecuteVehicleStopChargingAfterCompleteTest()
        {
            // 启动录波板
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
            // 停止充电
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            // 等待电压降至0
            WaitDCVoltage(testWorkParam.lstIDs, 0);
            LogNotice("关闭导引完成!");

            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 分析录波数据
            
            double timeStart = 0, timeEnd = 0;
            AnalyzeVoltageChangeTime(ReadWaveRecorderChannelData()[EChannelType.充电电压], ref timeStart, ref timeEnd);

            // 设置录波板光标
            SetWaveRecorderCursor(timeStart, timeEnd);
            // 保存录波截图并停止录波
            var chargingStatusImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);


            // 读取并记录接触器状态
            RecordContactorStatus("车端中止充电(充电结束)", chargingStatusImgs);

            // 分析并记录时间差数据
            AnalyzeAndRecordTimeDifference("车端中止充电");

            // 确认电子锁解锁状态
            ConfirmElectronicLockUnlockStatus();

            // 处理连接数据
            ProcessDataConnect("车端中止充电");

            LogNotice("充电结束车端中止测试完成");
        }

  
        /// <summary>
        /// 读取录波板通道数据
        /// </summary>
        /// <returns>录波数据</returns>
        private Dictionary<EChannelType, WaveData> ReadWaveRecorderChannelData()
        {
            var waveData1 = new WaveData();
            var waveData2 = new WaveData();
            var waveData3 = new WaveData();
            var waveData4 = new WaveData();
            var waveData5 = new WaveData();
            var waveData6 = new WaveData();
            var waveData8 = new WaveData();

            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData1, "充电电压");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref waveData2, "充电电流");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref waveData3, "辅源电压");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref waveData4, "CC1电压");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 5, ref waveData5, "CC2电压");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 6, ref waveData6, "前端电压");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref waveData8, "C1C2");
            return new Dictionary<EChannelType, WaveData> 
            {
                { EChannelType.充电电压,waveData1}, 
                { EChannelType.充电电流, waveData2 },
                { EChannelType.辅源电压, waveData3 },
                { EChannelType.CC1电压, waveData4 },
                { EChannelType.CC2电压, waveData5 },
                { EChannelType.前端电压, waveData6 },
                { EChannelType.C1C2, waveData8},

            };
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
        /// 分析电压变化时间
        /// </summary>
        /// <param name="waveData">录波数据</param>
        /// <param name="timeStart">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        private void AnalyzeVoltageChangeTime(WaveData waveData, ref double timeStart, ref double timeEnd)
        {
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveData, false, _demandVoltage - VoltageJudgeOffset, ref timeStart);
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveData, false, VoltageJudgeOffset, ref timeEnd);

            // 处理时间重叠问题
            if (Math.Abs(timeStart - timeEnd) < double.Epsilon)
            {
                timeEnd++;
                LogWarning("电压变化时间重叠，已自动调整结束时间");
            }
        }

        /// <summary>
        /// 设置录波板光标
        /// </summary>
        /// <param name="timeStart">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        private void SetWaveRecorderCursor(double timeStart, double timeEnd)
        {
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, timeStart);
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, timeEnd);
        }

        /// <summary>
        /// 记录接触器状态
        /// </summary>
        /// <param name="testType">测试类型</param>
        /// <param name="imgs">截图信息</param>
        private void RecordContactorStatus(string testType, Dictionary<int, string> imgs)
        {
            Dictionary<int, bool> c1c2Status = new Dictionary<int, bool>();
            Dictionary<int, bool> s3s4Status = new Dictionary<int, bool>();
            double c1c2Voltage = -1;
            double s3s4Voltage = -1;

            switch (testType)
            {
                case "桩中止充电(充电中)":
                    // 读取C1C2状态
                     c1c2Voltage = AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage;
                    c1c2Status.Add(1, c1c2Voltage > SafeVoltageThreshold);
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Status.First().Value ? "闭合" + c1c2Voltage : "断开" + c1c2Voltage, "闭合", c1c2Status, "C1C2状态" + testType);

                    s3s4Voltage = AllEquipStateData.DicBMS_DC_StateData[1].APSVoltage;
                    s3s4Status.Add(1, s3s4Voltage > AuxPowerVoltageThreshold);
                    ProcessDataResults(testWorkParam.lstIDs, s3s4Status.First().Value ? "闭合" + s3s4Voltage : "断开" + s3s4Voltage, "闭合", s3s4Status, "S3S4状态" + testType);
                    break;
                case "桩中止充电(充电结束)":
                    // 读取C1C2状态
                    c1c2Voltage = AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage;
                    c1c2Status.Add(1, c1c2Voltage < SafeVoltageThreshold);
                    s3s4Voltage = AllEquipStateData.DicBMS_DC_StateData[1].APSVoltage;
                    s3s4Status.Add(1, s3s4Voltage < AuxPowerVoltageThreshold);
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Status.First().Value ? "断开 " + c1c2Voltage : "闭合 " + c1c2Voltage, "断开", c1c2Status, "C1C2状态" + testType);
                    ProcessDataResults(testWorkParam.lstIDs, s3s4Status.First().Value ? "断开 " + s3s4Voltage : "闭合 " + s3s4Voltage, "断开", s3s4Status, "S3S4状态" + testType);
                    break;
                case "车端中止充电(充电中)":
                    // 读取C1C2状态
                    c1c2Voltage = AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage;
                    c1c2Status.Add(1, c1c2Voltage > SafeVoltageThreshold);
                    s3s4Voltage = AllEquipStateData.DicBMS_DC_StateData[1].APSVoltage;
                    s3s4Status.Add(1, s3s4Voltage > AuxPowerVoltageThreshold);
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Status.First().Value ? "闭合 " + c1c2Voltage : "断开 " + c1c2Voltage, "闭合", c1c2Status, "C1C2状态" + testType);
                    ProcessDataResults(testWorkParam.lstIDs, s3s4Status.First().Value ? "闭合 "+ s3s4Voltage : "断开 " + s3s4Voltage, sname: "闭合", s3s4Status, "S3S4状态" + testType);
                    break;
                case "车端中止充电(充电结束)":
                    // 读取C1C2状态
                    c1c2Voltage = AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage;
                    c1c2Status.Add(1, c1c2Voltage < SafeVoltageThreshold);
                    s3s4Voltage = AllEquipStateData.DicBMS_DC_StateData[1].APSVoltage;
                    s3s4Status.Add(1, s3s4Voltage < AuxPowerVoltageThreshold);
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Status.First().Value ? "断开 " + c1c2Voltage : "闭合 "+ c1c2Voltage, "断开", c1c2Status, "C1C2状态" + testType);
                    ProcessDataResults(testWorkParam.lstIDs, s3s4Status.First().Value ? "断开 " + s3s4Voltage : "闭合 " + s3s4Voltage, "断开", s3s4Status, "S3S4状态" + testType);
                    break;
            }
          

        }

        /// <summary>
        /// 分析并记录时间差数据
        /// </summary>
        /// <param name="testType">测试类型</param>
        private void AnalyzeAndRecordTimeDifference(string testType)
        {
            LogNotice("判断结果中...");

            // 读取光标数据
            var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            var timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            // 处理时间差数据
            ProcessDataTmp(timeDiffData, testType, "电压停止时间差(ms)", "0", "1000", timeDiffImgs);

            // 计算电压变化率
            var voltageChangeRateData = _tempTestData.ToDictionary(
                kv => kv.Key,
                kv => (_demandVoltage / (Convert.ToDouble(kv.Value) / 1000)).ToString()
            );

            // 处理电压变化率数据
            ProcessDataTmp(voltageChangeRateData, testType,
                testType.Contains("车端") ? "电压停止时间差(V/ms)" : "电压停止时间差(V/s)",
                "0", testType.Contains("车端") ? "1000" : "-");
        }

        /// <summary>
        /// 等待手动停止充电
        /// </summary>
        private void WaitManualStopCharging()
        {
            var timeout =   99999;
            CountDownTimeInfo("请手动控制桩停止充电再点击确认或者是。", timeout, 1);
        }

        /// <summary>
        /// 确认电子锁解锁状态
        /// </summary>
        private void ConfirmElectronicLockUnlockStatus()
        {
            var timeout = Customer.Equals("XJ") ? 30 : 999;
            CountDownTimeInfo("请确认车辆插头电子锁应能正确解锁!\r\n注：勾选上为可以正确解锁", timeout, 2);
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