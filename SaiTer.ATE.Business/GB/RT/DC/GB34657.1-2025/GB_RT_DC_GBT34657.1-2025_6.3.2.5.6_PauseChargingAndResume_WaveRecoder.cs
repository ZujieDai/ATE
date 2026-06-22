using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SaiTer.ATE.EquipMent.BMS_Protocol;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 暂停充电及恢复测试（34657.1-2025 A类 6.3.2.5.6）
    /// </summary>
    public class GB_RT_DC_2025_InrushCurrent_WaveRecoder : BusinessBase
    {
        #region 常量定义
        private const int DefaultTrialTimeoutSeconds = 30;
        private const int DefaultWaitTimeoutSeconds = 40;
        private const int SwipeCardTimeoutSeconds = 200;
        private const int StabilizeWaitMilliseconds = 5000;
        private const int ShortWaitMilliseconds = 500;
        private const int UnlockCheckCountdownSeconds = 15;
        private const int ContinueWaitSeconds = 60;

        // 阈值常量
        private const double VoltageStopThreshold = 10;
        private const double VoltageClosedThreshold = 60;
        private const double ApsVoltageNormalThreshold = 5;
        private const double VoltageChargingThreshold = 80;
        #endregion

        #region 私有变量
        private double _demandVoltage = 500;
        private double _demandCurrent = 3;
        private bool _isCarStop;
        #endregion

        #region 构造函数
        public GB_RT_DC_2025_InrushCurrent_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 初始化
        public override void InitializeParams()
        {
            Init();

            // 参数格式：BMS需求电压设置(V)=400|回馈负载电流设置(A)=100|小于等于20A下降电流(A)=20|大于20A下降电流(A)=60
            var paramArray = TrialItem.ResultParams.Split('|');
            if (paramArray.Length >= 2)
            {
                _demandVoltage = ParseParameterValue(paramArray[0]);
                _demandCurrent = ParseParameterValue(paramArray[1]);
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            SetLoadDCOFF(lstIDs);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);

            // 设置录波板采样率为1k/s
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);

            SetCPReresh();
        }
        #endregion

        #region 执行入口
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
                SaveTrialResult();
                SendNoticeToUIAndTxtFile($"{TrialItem.ItemName} 结束 ---------------------->");
                SendMessageEndThisTrial();
            }
        }
        #endregion

        #region 测试主流程
        private void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile($"开始 {TrialItem.ItemName} --------------------------->");
                _StopWatch.Restart();

                while (true)
                {
                    // 获取待测试通道
                    testWorkParam.lstIDs = LstTrialData
                        .Where(d => d.IsCheck && d.TrialResult == EmTrialResult.Wait)
                        .Select(d => d.ChargerId)
                        .Distinct()
                        .ToList();

                    // 所有通道测试完成
                    if (!testWorkParam.lstIDs.Any())
                        break;

                    // 测试超时处理
                    if (_StopWatch.Elapsed.TotalSeconds > DefaultTrialTimeoutSeconds)
                    {
                        HandleTestTimeout();
                        break;
                    }

                    // 执行充电桩主动暂停充电测试
                    RunChargingPileStopTest();
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        #endregion

        #region 核心测试方法
        /// <summary>
        /// 执行电流停止速率测试
        /// </summary>
        private void RunChargingPileStopTest()
        {
            try
            {
                // BMS参数配置
                ConfigureBmsParameters();

                // 启动导引
                SendNoticeToUIAndTxtFile("开启导引中...");
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                // 启动录波
                SendNoticeToUIAndTxtFile("录波板启动录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                // 等待刷卡
                WaitForSwipeCard(testWorkParam.lstIDs, SwipeCardTimeoutSeconds);

                // 状态验证
                VerifyChargingState(_isCarStop);

                // 执行停止操作
                ExecuteStopOperation();

                // 等待恢复
                WaitForRecovery();

                // 验证停止后状态
                VerifyStoppedState(_isCarStop);
            }
            finally
            {
                CleanupTestResources();
            }
        }
        #endregion

        #region 等待方法
        /// <summary>
        /// 等待刷卡
        /// </summary>
        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡...");
            MessgaeInfo(true, "请刷卡充电!", true);

            int remainingTime = timeoutSec;
            while (remainingTime-- > 0)
            {
                var bmsState = GetBmsState(ids);
                if (bmsState == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                int chargeState = ChangeBMSChargeStatus(bmsState.ChargingState);
                if (chargeState >= 9)
                    break;

                Thread.Sleep(1000);
            }
            
            MessgaeInfo(false, string.Empty);
            WaitDCVoltage(lstIDs, 500);
        }

        /// <summary>
        /// 等待充电桩手动停止
        /// </summary>
        private void WaitForChargingPileStop(List<int> ids)
        {
            SendNoticeToUIAndTxtFile("请在充电桩中点击暂停充电");
            MessgaeInfo(true, "请在充电桩中点击暂停充电! 点击确认", true);

            WaitForVoltageZero(ids, DefaultWaitTimeoutSeconds);

            MessgaeInfo(false, string.Empty);
        }

        /// <summary>
        /// 等待车辆主动停止
        /// </summary>
        private void WaitForCarStop(List<int> ids)
        {
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, true, 390, false);
            Thread.Sleep(200);

            SendNoticeToUIAndTxtFile("等待车辆停止充电...");
            WaitForVoltageZero(ids, DefaultWaitTimeoutSeconds);

            MessgaeInfo(false, string.Empty);
            CountDownTimeInfo("等待60秒，自动继续...", ContinueWaitSeconds, 0);
        }
        #endregion

        #region 数据处理
        public override void ProcessData()
        {
            // 业务逻辑由专用验证方法实现
        }
        #endregion

        #region 通用验证方法（消除4个重复方法）
        /// <summary>
        /// 统一验证充电状态（合并原4个重复方法）
        /// </summary>
        private void VerifyChargingState(bool isCarStopMode)
        {
            string title = isCarStopMode ? "车辆暂停前状态" : "充电桩暂停前状态";
            bool isChargingExpected = true;

            VerifyProtectionState(title, isChargingExpected);
        }

        private void VerifyStoppedState(bool isCarStopMode)
        {
            string title = isCarStopMode ? "车辆暂停后状态" : "充电桩暂停后状态";
            bool isChargingExpected = false;

            VerifyProtectionState(title, isChargingExpected);
        }

        /// <summary>
        /// 通用状态验证核心方法
        /// </summary>
        private void VerifyProtectionState(string testTitle, bool expectCharging)
        {
            var bmsData = GetBmsState(testWorkParam.lstIDs);
            if (bmsData == null) return;

            // 1. 充电状态验证
            var isCharging = new Dictionary<int, bool>
            {
                { 1, expectCharging ? bmsData.ChargingVoltage > VoltageChargingThreshold : bmsData.ChargingVoltage < VoltageStopThreshold }
            };
            string chargeStatusText = expectCharging ? "充电中" : "暂停充电";
            ProcessDataResults(testWorkParam.lstIDs, chargeStatusText, chargeStatusText, isCharging, $"{testTitle} 充电状态");

            // 2. CC1电压记录
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, testTitle, "CC1电压", "-", "-");

            // 3. C1/C2状态验证
            bool c1c2Expected = expectCharging;
            var c1c2State = new Dictionary<int, bool>
            {
                { 1, c1c2Expected ? bmsData.ChargingVoltage > VoltageClosedThreshold : bmsData.ChargingVoltage < VoltageClosedThreshold }
            };
            string c1c2Text = c1c2Expected ? "闭合" : "断开";
            ProcessDataResults(testWorkParam.lstIDs, c1c2Text, c1c2Text, c1c2State, $"{testTitle} C1C2状态");

            // 4. S3/S4状态验证
            var s3s4State = new Dictionary<int, bool> { { 1, bmsData.APSVoltage > ApsVoltageNormalThreshold } };
            string s3s4Text = expectCharging ? "闭合" : "断开";
            ProcessDataResults(testWorkParam.lstIDs, s3s4Text, "闭合", s3s4State, $"{testTitle} S3S4状态");

            // 5. 通讯状态验证
            var commState = new Dictionary<int, bool> { { 1, bmsData.APSVoltage > ApsVoltageNormalThreshold } };
            ProcessDataResults(testWorkParam.lstIDs, commState[1]?"正常":"异常", "正常", commState, $"{testTitle} 通讯状态");

            // 解锁确认
            CountDownTimeInfo("请确认充电中充电枪插头可被解锁。\r\n(注:勾选上为可被解锁)", UnlockCheckCountdownSeconds, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否可解锁");
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 解析参数值
        /// </summary>
        private double ParseParameterValue(string paramStr)
        {
            if (string.IsNullOrWhiteSpace(paramStr) || !paramStr.Contains("="))
                return 0;

            return double.TryParse(paramStr.Split('=')[1], out double value) ? value : 0;
        }

        /// <summary>
        /// 获取BMS状态
        /// </summary>
        private BMS_DC_StateData GetBmsState(List<int> ids)
        {
            return AllEquipStateData.DicBMS_DC_StateData
                .FirstOrDefault(x => ids.Contains(x.Value.ChargerID))
                .Value;
        }

        /// <summary>
        /// 等待电压为0
        /// </summary>
        private void WaitForVoltageZero(List<int> ids, int timeoutSec)
        {
            int remainingTime = timeoutSec;
            while (remainingTime-- > 0)
            {
                var bmsState = GetBmsState(ids);
                if (bmsState != null && bmsState.ChargingVoltage == 0)
                    break;

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 处理测试超时
        /// </summary>
        private void HandleTestTimeout()
        {
            foreach (var data in LstTrialData.Where(d => d.IsCheck && d.TrialResult == EmTrialResult.Wait))
            {
                data.TrialResult = EmTrialResult.Fail;
                data.TrialValue = _StopWatch.Elapsed.TotalSeconds.ToString("F0");

                var charger = LstChargerInfo.FirstOrDefault(s => s.ChargerId == data.ChargerId);
                if (charger != null)
                    data.PKID = charger.PKID;

                data.ExtentData = "-|-|-|-|null";
                SendTrialDataToUI(data);
            }
        }

        /// <summary>
        /// 配置BMS参数
        /// </summary>
        private void ConfigureBmsParameters()
        {
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, true, 390);
        }

        /// <summary>
        /// 启动负载设备
        /// </summary>
        private void StartLoadDevice()
        {
            SendNoticeToUIAndTxtFile("开启负载中...");
            SetLoadPara(testWorkParam.lstIDs, _demandVoltage - 10, _demandCurrent + 10, _demandVoltage - 10, _demandCurrent);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrentWithTime(testWorkParam.lstIDs, _demandCurrent, 35);
            Thread.Sleep(StabilizeWaitMilliseconds);
        }

        /// <summary>
        /// 执行停止操作
        /// </summary>
        private void ExecuteStopOperation()
        {
            if (_isCarStop)
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, false, 390);
                WaitForCarStop(testWorkParam.lstIDs);
            }
            else
            {
                WaitForChargingPileStop(testWorkParam.lstIDs);
            }
        }

        /// <summary>
        /// 等待恢复
        /// </summary>
        private void WaitForRecovery()
        {
            CountDownTimeInfo("等待60秒，自动继续...", ContinueWaitSeconds, 0);

            if (_isCarStop)
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, true, 390);
            }
            else
            {
                MessgaeInfo(true, "请在充电桩中点击继续充电! 点击确认", true);
            }

            WaitDCVoltage(testWorkParam.lstIDs, _demandVoltage);
            MessgaeInfo(false, string.Empty);
        }

        /// <summary>
        /// 清理测试资源
        /// </summary>
        private void CleanupTestResources()
        {
            // 停止录波
            SendNoticeToUIAndTxtFile("录波板停止录波...");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
            Thread.Sleep(ShortWaitMilliseconds);


            // 关闭导引
            SendNoticeToUIAndTxtFile("关闭导引中...");
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            SetCPReresh();
        }
        #endregion
    }
}