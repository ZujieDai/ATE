using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 互操作_绝缘故障测试
    /// 标准：GB_RT_DC_GBT34657.1-2025_6.3.2.4.2_InsulationDetection
    /// </summary>
    public class GB_RT_DC_2025_InsulationDetection_WaveRecoder : BusinessBase
    {
        #region 常量定义（消除魔法值）
        private const int TestTimeoutSeconds = 30;
        private const int DefaultVoltage = 390;
        private const int VoltageThreshold60 = 60;
        private const int VoltageThreshold5 = 5;
        private const int VoltageThreshold20 = 20;

        private const double VoltageTolerance95 = 0.95;
        private const double VoltageTolerance105 = 1.05;
        private const double VoltageTolerance90 = 0.9;
        private const double VoltageTolerance110 = 1.1;
        private const double VoltageTolerance99 = 0.99;
        private const double VoltageTolerance101 = 1.01;
        #endregion

        #region 私有字段
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        private double _resistiveLoadCurrent;
        private bool _is2015Charger;
        #endregion

        #region 构造函数
        public GB_RT_DC_2025_InsulationDetection_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 重写基类方法
        public override void InitEquiMent()
        {
            // 设备初始化逻辑（如需要可在此扩展）
        }

        public override void InitializeParams()
        {
            Init();
            _tempTestData.Clear();

            // 初始化测试参数
            _bmsDemandVoltage = LstChargerInfo?.FirstOrDefault()?.NominalVoltage ?? 0;
            _resistiveLoadCurrent = LstChargerInfo?.FirstOrDefault()?.NominalCurrent ?? 0;
        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartInsulationTestFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // 确保设备状态重置
                ResetEquipmentState();

                // 保存试验结果
                SaveTrialResult();
                SendNoticeToUIAndTxtFile($"{TrialItem.ItemName}结束---------------------->");

                // 发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public override void ProcessData()
        {
            // 数据处理逻辑（如需要可在此扩展）
        }
        #endregion

        #region 核心测试流程
        /// <summary>
        /// 启动绝缘检测测试流程
        /// </summary>
        private void StartInsulationTestFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile($"开始{TrialItem.ItemName}--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();

                while (true)
                {
                    // 获取待测试的充电机ID列表
                    var pendingChargerIds = GetPendingChargerIds();

                    // 退出条件：无待测试ID
                    if (pendingChargerIds.Count <= 0) break;

                    // 超时处理
                    if (IsTestTimeout())
                    {
                        HandleTestTimeout(pendingChargerIds);
                        break;
                    }

                    // 执行核心测试步骤
                    ExecuteCoreTestSteps(pendingChargerIds);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// 执行核心测试步骤
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        private void ExecuteCoreTestSteps(List<int> chargerIds)
        {
            // 基础设备控制
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            SetConditionValues();

            // 2. 判断充电机类型（2015款）
            _is2015Charger = CheckIf2015Charger(chargerIds);

            // 3. 选择合适的绝缘电阻档位并执行测试（100Ω/V ≤ 绝缘电阻/充电电压 ≤ 500Ω/V）
            if (!SelectResistanceGearAndTest(chargerIds, true)) return;

            // 4. 选择合适的绝缘电阻档位并执行测试（绝缘电阻/充电电压 ≤ 100Ω/V）
            if (!SelectResistanceGearAndTest(chargerIds, false)) return;

            // 5. 车辆检测绝缘异常测试（不可信状态）
            ExecuteVehicleInsulationAbnormalTest(chargerIds, InsulationState.不可信);

            // 6. 车辆检测绝缘异常测试（异常状态）
            ExecuteVehicleInsulationAbnormalTest(chargerIds, InsulationState.异常);

        }
        #endregion

        #region 辅助方法 - 测试准备与基础控制
        /// <summary>
        /// 获取待测试的充电机ID列表
        /// </summary>
        /// <returns>待测试ID列表</returns>
        private List<int> GetPendingChargerIds()
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

            return testWorkParam.lstIDs;
        }

        /// <summary>
        /// 检查测试是否超时
        /// </summary>
        /// <returns>是否超时</returns>
        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;
        }

        /// <summary>
        /// 处理测试超时
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        private void HandleTestTimeout(List<int> chargerIds)
        {
            foreach (var trialData in LstTrialData)
            {
                if (trialData.IsCheck && trialData.TrialResult == EmTrialResult.Wait)
                {
                    trialData.TrialResult = EmTrialResult.Fail;
                    trialData.TrialValue = _StopWatch.Elapsed.Seconds.ToString();

                    var chargerInfo = LstChargerInfo.FirstOrDefault(c => c.ChargerId == trialData.ChargerId);
                    if (chargerInfo != null)
                    {
                        trialData.PKID = chargerInfo.PKID;
                    }

                    trialData.ExtentData = "-|-|-|-|null";
                    SendTrialDataToUI(trialData);
                }
            }
        }

        /// <summary>
        /// 重置设备状态
        /// </summary>
        private void ResetEquipmentState()
        {
            var kState = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, DefaultVoltage, kState.ToArray());
            Thread.Sleep(300);
        }

        /// <summary>
        /// 检查是否为2015款充电机
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <returns>是否为2015款</returns>
        private bool CheckIf2015Charger(List<int> chargerIds)
        {
            if (!chargerIds.Any()) return false;

            var chargerNums = ConfigurationManager.AppSettings["ChargerNum_2015"]
                ?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if(chargerNums.Length == 0) return false;
            else if(chargerNums.Length == 1&& chargerNums[0] == "true") return true;
            else if(chargerNums.Length == 1 && chargerNums[0] == "false") return false;

            return false;
        }
        #endregion

        #region 辅助方法 - 具体测试步骤
        /// <summary>
        /// 模拟K1/K2外侧电压>10V测试
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        private void ExecuteVoltageOver10VTest(List<int> chargerIds)
        {
            // 启动BMS并设置电池电压
            ControlEquipMent.BMS.BMS_ON(chargerIds);
            Thread.Sleep(1000);
            ControlEquipMent.BMS.BMSSetBatteryVoltage(chargerIds, 100);
            Thread.Sleep(1000);

            // 设置过压控制开关
            var kState = GetKStatus16_Charging_DC();
            kState[26] = true;
            ControlEquipMent.BMS.BMSSetKState_DC(chargerIds, 1000, DefaultVoltage, kState.ToArray());
            Thread.Sleep(3000);

            // 记录外侧电压数据
            var voltageData = new Dictionary<int, string>();
            foreach (var id in chargerIds)
            {
                voltageData.Add(id, AllEquipStateData.DicBMS_DC_StateData[id].ChargingVoltage.ToString("F2"));
            }
            ProcessDataTmp(voltageData, "绝缘前模拟外侧电压>10V", "绝缘检测前外侧电压(V)", "-", "-");

            // 等待刷卡充电
            WaitForSwipingCard(chargerIds);

            // 验证充电状态
            VerifyChargingState(chargerIds, "绝缘前模拟外侧电压>10V");

            // 人工确认告警
            VerifyManualAlarm(chargerIds, "绝缘前模拟外侧电压>10V");

            // 恢复BMS状态
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
            Thread.Sleep(100);

            // 重置过压控制开关
            kState = GetKStatus16_Charging_DC();
            kState[26] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(chargerIds, 1000, DefaultVoltage, kState.ToArray());
            Thread.Sleep(100);

            // 重置电池电压
            ControlEquipMent.BMS.BMSSetBatteryVoltage(chargerIds, DefaultVoltage);
            Thread.Sleep(100);
        }

        /// <summary>
        /// 等待刷卡充电
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        private void WaitForSwipingCard(List<int> chargerIds)
        {
            foreach (var id in chargerIds)
            {
                SendNoticeToUIAndTxtFile($"等待刷卡(枪{id})");
                int timeout = 100;
                MessgaeInfo(true, $"请刷卡充电!(枪{id})", true);

                while (timeout-- > 0)
                {
                    if (ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[id].ChargingState) > 4)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }

                MessgaeInfo(false, "");
            }
        }

        /// <summary>
        /// 验证充电状态
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="testScenario">测试场景</param>
        private void VerifyChargingState(List<int> chargerIds, string testScenario)
        {
            var stateData = new Dictionary<int, string>();
            var resultData = new Dictionary<int, EmTrialResult>();

            foreach (var id in chargerIds)
            {
                var state = AllEquipStateData.DicBMS_DC_StateData[id].ChargingState;
                stateData.Add(id, state);
                resultData.Add(id, ChangeBMSChargeStatus(state) != 9 ? EmTrialResult.Pass : EmTrialResult.Fail);
            }

            ProcessDataResults(chargerIds, stateData, "充电状态", resultData, testScenario);
        }

        /// <summary>
        /// 验证人工告警确认
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="testScenario">测试场景</param>
        private void VerifyManualAlarm(List<int> chargerIds, string testScenario)
        {
            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);

            var alarmData = new Dictionary<int, string>();
            var resultData = new Dictionary<int, EmTrialResult>();

            foreach (var id in chargerIds)
            {
                alarmData.Add(id, DicManualVerifyResult[id] ? "有告警" : "未告警");
                resultData.Add(id, DicManualVerifyResult[id] ? EmTrialResult.Pass : EmTrialResult.Fail);
            }

            ProcessDataResults(chargerIds, alarmData, "应发出告警提示", resultData, testScenario);
        }

        /// <summary>
        /// 选择电阻档位并执行测试
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="isNormalResistance">是否为正常电阻范围（100-500Ω/V）</param>
        /// <returns>是否测试成功</returns>
        private bool SelectResistanceGearAndTest(List<int> chargerIds, bool isNormalResistance)
        {
            int kStatus1 = 0, kStatus2 = 0;
            string resistanceValue = string.Empty;

            // 根据充电机类型和电阻范围选择档位
            if (!SelectResistanceGear(isNormalResistance, ref kStatus1, ref kStatus2, ref resistanceValue))
            {
                // 未找到合适档位，标记测试结果
                MarkNoSuitableGear(chargerIds);
                return false;
            }

            // 执行具体的漏电测试
            ExecuteLeakageTests(chargerIds, kStatus1, kStatus2, resistanceValue, isNormalResistance);

            return true;
        }

        /// <summary>
        /// 选择绝缘电阻档位
        /// </summary>
        /// <param name="isNormalResistance">是否为正常电阻范围</param>
        /// <param name="kStatus1">K状态1</param>
        /// <param name="kStatus2">K状态2</param>
        /// <param name="resistanceValue">电阻值</param>
        /// <returns>是否选择成功</returns>
        private bool SelectResistanceGear(bool isNormalResistance, ref int kStatus1, ref int kStatus2, ref string resistanceValue)
        {
            if (_is2015Charger)
            {
                return Select2015ChargerResistanceGear(isNormalResistance, ref kStatus1, ref kStatus2, ref resistanceValue);
            }
            else
            {
                return SelectStandardChargerResistanceGear(isNormalResistance, ref kStatus1, ref kStatus2, ref resistanceValue);
            }
        }

        /// <summary>
        /// 选择2015款充电机电阻档位
        /// </summary>
        private bool Select2015ChargerResistanceGear(bool isNormalResistance, ref int kStatus1, ref int kStatus2, ref string resistanceValue)
        {
            double ratio;
            double lowerBound = isNormalResistance ? 100 * VoltageTolerance105 : 0;
            double upperBound = isNormalResistance ? 500 * VoltageTolerance95 : 100 * VoltageTolerance105;

            // 600KΩ
            ratio = 600000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 7; kStatus2 = 15; resistanceValue = "600KΩ"; return true;
            }

            // 300KΩ
            ratio = 300000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 6; kStatus2 = 14; resistanceValue = "300KΩ"; return true;
            }

            // 200KΩ
            ratio = 200000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 5; kStatus2 = 13; resistanceValue = "200KΩ"; return true;
            }

            // 100KΩ
            ratio = 100000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 4; kStatus2 = 12; resistanceValue = "100KΩ"; return true;
            }

            // 75KΩ
            ratio = 75000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                //kStatus1 = 3; kStatus2 = 11; resistanceValue = "75KΩ"; return true;
                kStatus1 = 2; kStatus2 = 10; resistanceValue = "20KΩ"; return true;
            }

            // 20KΩ
            ratio = 20000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 2; kStatus2 = 10; resistanceValue = "20KΩ"; return true;
            }

            // 15.3KΩ
            ratio = 15300 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 1; kStatus2 = 9; resistanceValue = "15.3KΩ"; return true;
            }

            return false;
        }

        /// <summary>
        /// 选择标准充电机电阻档位
        /// </summary>
        private bool SelectStandardChargerResistanceGear(bool isNormalResistance, ref int kStatus1, ref int kStatus2, ref string resistanceValue)
        {
            double ratio;
            double lowerBound = isNormalResistance ? 100 * VoltageTolerance105 : 0;
            double upperBound = isNormalResistance ? 500 * VoltageTolerance95 : 100 * VoltageTolerance95;

            // 300KΩ
            ratio = 300000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 7; kStatus2 = 15; resistanceValue = "300KΩ"; return true;
            }

            // 100KΩ
            ratio = 100000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 6; kStatus2 = 14; resistanceValue = "100KΩ"; return true;
            }

            // 75KΩ
            ratio = 75000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                //kStatus1 = 5; kStatus2 = 13; resistanceValue = "75KΩ"; return true;
                 kStatus1 = 4; kStatus2 = 12; resistanceValue = "33KΩ"; return true;
            }

            // 33KΩ
            ratio = 33000 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 4; kStatus2 = 12; resistanceValue = "33KΩ"; return true;
            }

            // 29.7KΩ
            ratio = 29700 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 3; kStatus2 = 11; resistanceValue = "29.7KΩ"; return true;
            }

            // 24.8KΩ
            ratio = 24800 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 2; kStatus2 = 10; resistanceValue = "24.8KΩ"; return true;
            }

            // 22.9KΩ
            ratio = 22900 / _bmsDemandVoltage;
            if (ratio >= lowerBound && ratio <= upperBound)
            {
                kStatus1 = 1; kStatus2 = 9; resistanceValue = "22.9KΩ"; return true;
            }

            return false;
        }

        /// <summary>
        /// 执行漏电测试
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="kStatus1">K状态1</param>
        /// <param name="kStatus2">K状态2</param>
        /// <param name="resistanceValue">电阻值</param>
        /// <param name="canCharge">是否允许充电</param>
        private void ExecuteLeakageTests(List<int> chargerIds, int kStatus1, int kStatus2, string resistanceValue, bool canCharge)
        {
            // DC+非对称漏电测试
            SendNoticeToUIAndTxtFile($"设置DC+非对称漏电{resistanceValue}");
            var kState = GetKStatus16_Charging_DC();
            kState[0] = false;
            kState[kStatus1] = true;
            TrialMethod($"DC+非对称漏电{resistanceValue}", canCharge ? "应能充电" : "应不能充电", "应能告警", kState, canCharge);

            // DC-非对称漏电测试
            SendNoticeToUIAndTxtFile($"设置DC-非对称漏电{resistanceValue}");
            kState = GetKStatus16_Charging_DC();
            kState[8] = false;
            kState[kStatus2] = true;
            TrialMethod($"DC-非对称漏电{resistanceValue}", canCharge ? "应能充电" : "应不能充电", "应能告警", kState, canCharge);

            // DC+、DC-对称漏电测试
            SendNoticeToUIAndTxtFile($"设置DC-、DC+对称漏电{resistanceValue}");
            kState = GetKStatus16_Charging_DC();
            kState[0] = false;
            kState[kStatus1] = true;
            kState[8] = false;
            kState[kStatus2] = true;
            TrialMethod($"DC+、DC-对称漏电{resistanceValue}", canCharge ? "应能充电" : "应不能充电", "应能告警", kState, canCharge);
        }

        /// <summary>
        /// 标记未找到合适档位
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        private void MarkNoSuitableGear(List<int> chargerIds)
        {
            var data = new Dictionary<int, string>();
            var results = new Dictionary<int, EmTrialResult>();

            foreach (var id in chargerIds)
            {
                data.Add(id, "未找到合适档位");
                results.Add(id, EmTrialResult.NA);
            }

            ProcessDataResults(chargerIds, data, "应发出告警提示", results, "绝缘前模拟外侧电压>10V");
        }

        /// <summary>
        /// 执行车辆绝缘异常测试
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="insulationState">绝缘状态</param>
        private void ExecuteVehicleInsulationAbnormalTest(List<int> chargerIds, InsulationState insulationState)
        {
           
            // 启动充电
            ChargingStart();

            // 启动BMS
            ControlEquipMent.BMS.BMS_ON(chargerIds);
            Thread.Sleep(200);
            MessgaeInfo(true, "请刷卡充电!", true);
            // 等待充电就绪
            bool isChargeReady = WaitChargeReady(200);
            MessgaeInfo(false, "请刷卡充电!", true);

            if (!isChargeReady)
            {
                string stateDesc = insulationState == InsulationState.不可信 ? "不可信" : "异常";
                SendNoticeToUIAndTxtFile($"充电就绪超时，{stateDesc}状态测试终止");
                MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                return;
            }

            // 模拟车辆绝缘异常
            var nominalVoltage = LstChargerInfo[0].NominalVoltage;
            ControlEquipMent.BMS.SetParameter(chargerIds, nominalVoltage, 30, true, nominalVoltage, true, null, insulationState);
            Thread.Sleep(300);
            ControlEquipMent.BMS.SetParameter(chargerIds, nominalVoltage, 30, true, nominalVoltage, true, null, insulationState);
            Thread.Sleep(300); 
            ControlEquipMent.BMS.SetParameter(chargerIds, nominalVoltage, 30, true, nominalVoltage, true, null, insulationState);
            Thread.Sleep(5000);
            // 验证测试结果
            VerifyVehicleInsulationTestResults(chargerIds, insulationState);
            ControlEquipMent.BMS.SetParameter(chargerIds, nominalVoltage, 30, true, nominalVoltage, true, null,  InsulationState.正常);

            // 关闭BMS
            Thread.Sleep(2000);
            ControlEquipMent.BMS.BMS_OFF(chargerIds);
        }

        /// <summary>
        /// 验证车辆绝缘异常测试结果
        /// </summary>
        /// <param name="chargerIds">充电机ID列表</param>
        /// <param name="insulationState">绝缘状态</param>
        private void VerifyVehicleInsulationTestResults(List<int> chargerIds, InsulationState insulationState)
        {
            string stateDesc = insulationState == InsulationState.不可信 ? "不可信" : "异常";
            string testScenario = $"车辆检测绝缘异常({stateDesc})";
            // 获取BMS数据
            var bmsData = AllEquipStateData.DicBMS_DC_StateData.First().Value;
            switch (insulationState)
            {
                case InsulationState.异常:
                    // 1. 验证充电状态
                    var chargingState = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    var chargingResult = chargingState.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "未充电" : "允许充电");
                    ProcessDataResults(chargerIds, chargingResult, "充电状态", chargingState, testScenario, "未充电", "未充电");

                    // 2. 记录CC1电压值
                    var cc1Voltage = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(cc1Voltage, testScenario, "检测点1电压值CC1电压", "-", "-");

                    // 3. 验证C1C2状态
                    var c1c2State = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage < VoltageThreshold60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    var c1c2Result = c1c2State.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(chargerIds, c1c2Result, "C1C2状态", c1c2State, testScenario, "断开", "断开");

                    // 4. 验证S3S4状态
                    var s3s4State = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage < VoltageThreshold5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    var s3s42Result = s3s4State.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "断开" : "闭合");
                    ProcessDataResults(chargerIds, s3s42Result, "S3S4状态", s3s4State, testScenario, "断开", "断开");

                    // 5. 验证通讯状态
                    //var cst = GetCANByType("CST");
                    var connResult = chargingState.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "不正常");
                    ProcessDataResults(chargerIds, connResult, "通讯状态", chargingState, testScenario, "正常", "正常");

                    // 6. 人工验证电子锁状态
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常 解锁 \r\n注:勾选上为可解锁", 20, 2);
                    var lockerState = DicManualVerifyResult.ToDictionary(x => x.Key, x => x.Value ? EmTrialResult.Pass : EmTrialResult.Fail);
                    var lockerResult = DicManualVerifyResult.ToDictionary(x => x.Key, x => x.Value ? "解锁" : "锁止");
                    ProcessDataResults(chargerIds, lockerResult, "电子锁状态", lockerState, testScenario, "解锁", "解锁");
                    break;

                case InsulationState.不可信:  
                    // 1. 验证充电状态
                    chargingState = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage >= 60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    chargingResult = chargingState.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "允许充电" : "未充电");
                    ProcessDataResults(chargerIds, chargingResult, "充电状态", chargingState, testScenario, "允许充电", "允许充电");

                    // 2. 记录CC1电压值
                    cc1Voltage = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(c => c.Key, c => c.Value.CC1Voltage.ToString());
                    ProcessDataTmp(cc1Voltage, testScenario, "检测点1电压值CC1电压", "-", "-");

                    // 3. 验证C1C2状态
                    c1c2State = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.ChargingVoltage >= VoltageThreshold60 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    c1c2Result = c1c2State.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "闭合" : "断开");
                    ProcessDataResults(chargerIds, c1c2Result, "C1C2状态", c1c2State, testScenario, "闭合", "闭合");

                    // 4. 验证S3S4状态
                    s3s4State = AllEquipStateData.DicBMS_DC_StateData.ToDictionary(x => x.Key, x => x.Value.APSVoltage >= VoltageThreshold5 ? EmTrialResult.Pass : EmTrialResult.Fail);
                    s3s42Result = s3s4State.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "闭合" : "断开");
                    ProcessDataResults(chargerIds, s3s42Result, "S3S4状态", s3s4State, testScenario, "闭合", "闭合");

                    // 5. 验证通讯状态
                    //var cst = GetCANByType("CST");
                    connResult = chargingState.ToDictionary(x => x.Key, x => x.Value == EmTrialResult.Pass ? "正常" : "不正常");
                    ProcessDataResults(chargerIds, connResult, "通讯状态", chargingState, testScenario, "正常", "正常");

                    // 6. 人工验证电子锁状态
                    SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                    CountDownTimeInfo("请确认电子锁能正常 锁止 \r\n注:勾选上为可锁止", 20, 2);
                    lockerState = DicManualVerifyResult.ToDictionary(x => x.Key, x => x.Value ? EmTrialResult.Pass : EmTrialResult.Fail);
                    lockerResult = DicManualVerifyResult.ToDictionary(x => x.Key, x => x.Value ? "锁止" : "解锁");
                    ProcessDataResults(chargerIds, lockerResult, "电子锁状态", lockerState, testScenario, "锁止", "锁止");
                    break;

            }
          
        }
        #endregion

        #region 原有方法优化
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

                var chargerInfo = LstChargerInfo.FirstOrDefault(c => c.ChargerId == trialData.ChargerId);
                if (chargerInfo != null)
                {
                    trialData.PKID = chargerInfo.PKID;
                }

                trialData.ExtentData = "null|null|null|null|null";
                SendTrialDataToUI(trialData);
            }
        }

        /// <summary>
        /// 等待充电就绪（BMS状态变为充电中）
        /// </summary>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>是否就绪</returns>
        private bool WaitChargeReady(int timeout)
        {
            SendNoticeToUIAndTxtFile($"等待充电就绪，超时{timeout}秒");

            while (timeout-- > 0)
            {
                // 筛选当前测试设备的BMS状态
                var bmsData = AllEquipStateData.DicBMS_DC_StateData
                    .Where(c => testWorkParam.lstIDs.Contains(c.Key))
                    .ToDictionary(bms => bms.Key, bms => bms.Value);

                if (bmsData.Count == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // 所有设备充电状态变为9（可充电）则就绪
                bool allCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                if (allCanCharge)
                {
                    SendNoticeToUIAndTxtFile("所有设备充电就绪");
                    return true;
                }

                Thread.Sleep(1000);
            }

            SendNoticeToUIAndTxtFile("充电就绪超时");
            return false;
        }

        /// <summary>
        /// 执行具体的试验方法
        /// </summary>
        /// <param name="stateDesc">状态描述</param>
        /// <param name="itemName1">项目名称1</param>
        /// <param name="itemName2">项目名称2</param>
        /// <param name="kStates">K状态列表</param>
        /// <param name="canCharge">是否允许充电</param>
        private void TrialMethod(string stateDesc, string itemName1, string itemName2, List<bool> kStates, bool canCharge = true)
        {
            SetCPReresh();  // 模拟插拔枪

            SendNoticeToUIAndTxtFile("开始设置充电参数");
            var nominalVoltage = LstChargerInfo[0].NominalVoltage;

            // 设置BMS参数
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, nominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DefaultVoltage, nominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, nominalVoltage, 250, true, nominalVoltage);
            Thread.Sleep(200);

            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, DefaultVoltage, kStates.ToArray());

            // 处理每个充电机的测试
            foreach (var chargerId in testWorkParam.lstIDs)
            {
                ProcessSingleChargerTest(chargerId, stateDesc, nominalVoltage);
            }

            // 验证充电结果
            VerifyChargingResults(stateDesc, itemName1, canCharge);

            // 人工确认告警
            VerifyManualAlarm(testWorkParam.lstIDs, stateDesc, itemName2);

            // 重置设备状态
            var resetKState = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, DefaultVoltage, resetKState.ToArray());
            Thread.Sleep(300);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        /// <summary>
        /// 处理单个充电机的测试
        /// </summary>
        /// <param name="chargerId">充电机ID</param>
        /// <param name="stateDesc">状态描述</param>
        /// <param name="nominalVoltage">额定电压</param>
        private void ProcessSingleChargerTest(int chargerId, string stateDesc, double nominalVoltage)
        {
            double insulationVolt = 0;
            int timeout = 300;

            // 等待刷卡充电
            MessgaeInfo(true, $"请刷卡充电!(枪{chargerId})", true);
            while (timeout-- > 0)
            {
                int chargingState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingState);
                if (chargingState >= 2 && chargingState < 9)
                {
                    insulationVolt = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingVoltage;
                    break;
                }
                Thread.Sleep(1000);
            }
            MessgaeInfo(false, "");

            // 等待进入充电状态
            MessgaeInfo(true, $"枪{chargerId}等待充电中...", true);
            timeout = 600;
            while (timeout-- > 0)
            {
                int bmsState = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingState);
                if (bmsState <= 5)
                {
                    double newVolt = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingVoltage;
                    insulationVolt = Math.Max(newVolt, insulationVolt);
                }

                if (bmsState == 9 || bmsState == 13 || bmsState == 0)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            MessgaeInfo(false, "");

            // 等待泄放
            Thread.Sleep(2000);

            // 记录测试数据
            var voltageData = new Dictionary<int, string>
            {
                { chargerId, nominalVoltage.ToString() }
            };
            ProcessDataTmp(voltageData, stateDesc, "车辆通信握手报文的最高允许充电电压(V)", "-", "-");

            voltageData = new Dictionary<int, string>
            {
                { chargerId, insulationVolt.ToString() }
            };
            ProcessDataTmp(voltageData, stateDesc, "绝缘电压(V)",
                (nominalVoltage * VoltageTolerance99).ToString("F2"),
                (nominalVoltage * VoltageTolerance101).ToString("F2"));
        }

        /// <summary>
        /// 验证充电结果
        /// </summary>
        /// <param name="stateDesc">状态描述</param>
        /// <param name="itemName">项目名称</param>
        /// <param name="canCharge">是否允许充电</param>
        private void VerifyChargingResults(string stateDesc, string itemName, bool canCharge)
        {
            var voltageData = new Dictionary<int, string>();

            foreach (var chargerId in testWorkParam.lstIDs)
            {
                double voltage = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingVoltage;
                int timeout = 50;

                if (canCharge)
                {
                    // 正常充电：电压在额定电压90%-110%之间
                    while (timeout-- > 0 && (voltage < _bmsDemandVoltage * VoltageTolerance90 || voltage > _bmsDemandVoltage * VoltageTolerance110))
                    {
                        voltage = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingVoltage;
                        Thread.Sleep(100);
                    }
                    ProcessDataTmp(new Dictionary<int, string> { { chargerId, voltage.ToString("F2") } },
                        stateDesc, "直流输出电压(V)", "-", "-");
                }
                else
                {
                    // 不能充电：电压应≤20V
                    while (timeout-- > 0 && voltage > VoltageThreshold20)
                    {
                        voltage = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingVoltage;
                        Thread.Sleep(100);
                    }
                    ProcessDataTmp(new Dictionary<int, string> { { chargerId, voltage.ToString("F2") } },
                        stateDesc, "直流输出电压(V)", "0", VoltageThreshold20.ToString());
                }

                voltageData.Add(chargerId, voltage.ToString("F2"));
            }

            // 验证充电状态结果
            var stateData = new Dictionary<int, string>();
            var resultData = new Dictionary<int, EmTrialResult>();

            foreach (var chargerId in testWorkParam.lstIDs)
            {
                string state = AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingState;
                string chargeStatus = state == "充电中" ? "能充电" : "不能充电";

                stateData.Add(chargerId, chargeStatus);
                resultData.Add(chargerId, canCharge
                    ? (state == "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail)
                    : (state != "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail));
            }

            ProcessDataResults(testWorkParam.lstIDs, stateData, itemName, resultData, stateDesc);
        }

        /// <summary>
        /// 验证人工告警确认（重载方法）
        /// </summary>
        private void VerifyManualAlarm(List<int> chargerIds, string testScenario, string itemName)
        {
            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);

            var alarmData = new Dictionary<int, string>();
            var resultData = new Dictionary<int, EmTrialResult>();

            foreach (var id in chargerIds)
            {
                alarmData.Add(id, DicManualVerifyResult[id] ? "有告警" : "未告警");
                resultData.Add(id, DicManualVerifyResult[id] ? EmTrialResult.Pass : EmTrialResult.Fail);
            }

            ProcessDataResults(chargerIds, alarmData, itemName, resultData, testScenario);
        }
        #endregion
    }
}