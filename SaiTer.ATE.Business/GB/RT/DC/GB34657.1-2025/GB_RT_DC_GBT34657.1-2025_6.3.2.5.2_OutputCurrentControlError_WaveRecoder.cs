using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电流控制误差测试（录波版）
    /// 标准：GB/T 34657.1-2025  6.3.2.5.2
    /// 测试编号：TC_DA.EVSE_P_EnergyTransfer.Charging_003
    /// 测试内容：能量传输阶段恒流输出误差 ΔI = IM - IO
    /// 合格依据：NB/T 33001 输出电流误差要求
    /// </summary>
    public class GB_RT_DC_2025_OutputCurrentControlError_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 60;
        private const int DefaultVoltage = 750;
        private const int HighVoltage = 750;
        private const int CurrentStableTime = 5000;   // 电流稳定时间
        private const int DefaultCurrent = 15;         // 默认测试电流
        #endregion

        #region 私有变量
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        private double MAXVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_OutputCurrentControlError_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 基类重写
        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            Init();
            _tempTestData.Clear();
            _bmsDemandVoltage = LstChargerInfo?.FirstOrDefault()?.NominalVoltage ?? DefaultVoltage;

            string[] strParams = TrialItem.ResultParams.Split('|');
            //if (strParams.Length >= 1)
            //{
            //    ElectronicLoadCurrent = double.Parse(strParams[0].Split('=')[1]);
            //}
            if (strParams.Length >= 1)
            {
               MAXVoltage = double.Parse(strParams[0].Split('=')[1]);
               
            }
        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                if (MAXVoltage < 30)
                {
                    SystemEvent.MessageInfo(true, "设置参数 大电流值需要大于等于30A\r\n请修改后进行测试",true);
                    return;
                }
                InitEquiMent();
                StartTestFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                ResetEquipmentState();
                SaveTrialResult();
                SendNoticeToUIAndTxtFile($"{TrialItem.ItemName} 结束 ------------------>");
                SendMessageEndThisTrial();
            }
        }

        public override void ProcessData()
        {
        }
        #endregion

        #region 测试主流程
        private void StartTestFlow()
        {
            SendNoticeToUIAndTxtFile($"开始 {TrialItem.ItemName} ------------------>");
            _StopWatch.Restart();

            while (true)
            {
                List<int> pendingIds = GetPendingChargerIds();
                if (pendingIds.Count == 0)
                    break;

                if (IsTestTimeout())
                {
                    HandleTimeout(pendingIds);
                    break;
                }

                ExecuteCurrentErrorTest(pendingIds);
            }
        }

        /// <summary>
        /// 执行电流误差测试（多档位恒流）
        /// </summary>
        private void ExecuteCurrentErrorTest(List<int> chargerIds)
        {
            try
            {
                SetCPReresh();

                Thread.Sleep(200);
                SetConditionValues();
                // 1. 握手阶段
                ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage);
                Thread.Sleep(200);

                // 2. 参数配置
                ControlEquipMent.BMS.SetParameter(chargerIds, 390, HighVoltage, 50);

                Thread.Sleep(200);
                // 3. 充电需求
                ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage, 50, false, 390);
                Thread.Sleep(200);

                ControlEquipMent.BMS.BMS_ON(chargerIds);
                // 等待刷卡进入充电
                WaitForSwipeCard(chargerIds, 100);

                if (!WaitDCVoltage(lstIDs, 750, 60))
                {
                    Dictionary<int, bool> Result = new Dictionary<int, bool>
            { { 1, false } };

                    ProcessDataResults(chargerIds, "充电桩未输出需求电压", "充电桩未输出需求电压", Result);
                    return;
                }
                // 启动录波
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                Thread.Sleep(200);


                // 测试点1：小电流（<30A）
                TestCurrentPoint(chargerIds, 10);

                // 测试点2：中电流
                TestCurrentPoint(chargerIds, MAXVoltage);


                // 停止充电 & 负载
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(3000);

                ControlEquipMent.BMS.BMS_OFF(chargerIds);
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        /// <summary>
        /// BMS 充电参数初始化
        /// </summary>
        private void InitBMSChargeParams(List<int> chargerIds, double targetCurrent)
        {
          

            
        }
        #endregion

        #region 电流点测试（核心）
        /// <summary>
        /// 单个电流点误差测试
        /// </summary>
        private void TestCurrentPoint(List<int> chargerIds, double targetCurrent)
        {
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

            string testTitle = targetCurrent<30? $"恒流输出<30A": $"恒流输出>30A";
            SendNoticeToUIAndTxtFile($"===== 开始 {testTitle} 测试 =====");
            // 3. 充电需求
            ControlEquipMent.BMS.SetParameter(chargerIds, HighVoltage, targetCurrent, false, 390);

            Thread.Sleep(200);
            SendNoticeToUIAndTxtFile($"BMS 设置需求电流 IO = {targetCurrent} A");

            // 开启负载，稳定电流
            SendNoticeToUIAndTxtFile("开启负载，等待电流稳定...");
            SetLoadPara(testWorkParam.lstIDs, 750 - 20, targetCurrent, 750-20, targetCurrent);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrentWithTime(testWorkParam.lstIDs, targetCurrent, 30);
            Thread.Sleep(CurrentStableTime);

            // 停止录波
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 读取实测电流
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            double measureCurrent = bmsData.ChargingCurrent;
            double errorCurrent = measureCurrent - targetCurrent; // ΔI = IM - IO

            // NB/T 33001 电流合格判据
            double maxAllowedError;
            if (targetCurrent >= 30)
                maxAllowedError = targetCurrent * 0.01;       // ≥30A：±1%
            else
                maxAllowedError = 0.3;                        // <30A：±0.3A

            // 日志输出
            SendNoticeToUIAndTxtFile($"需求电流 IO = {targetCurrent:F2} A");
            SendNoticeToUIAndTxtFile($"实测电流 IM = {measureCurrent:F2} A");
            SendNoticeToUIAndTxtFile($"电流误差 ΔI = {errorCurrent:F2} A");
            SendNoticeToUIAndTxtFile($"最大允许误差 = ±{maxAllowedError:F2} A");

            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1, $" {targetCurrent:F2}" } }, testTitle, "需求电流 IO", "-", "-");
            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1, $" {measureCurrent:F2}" } }, testTitle, "实测电流 IM", "-", "-");
            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1, $" {errorCurrent:F2}" } }, testTitle, "电流误差 ΔI", "-", "-");
           
            // 构造结果
            var resultData = new Dictionary<int, string>
            {
                { 1, measureCurrent.ToString("F2") },
            };
                
            // 上报结果
            ProcessDataTmp(resultData, testTitle, targetCurrent >= 30? "实测电流(1%)" : $"实测电流±{maxAllowedError:F2}",
                (targetCurrent - maxAllowedError).ToString("F2"),
                (targetCurrent + maxAllowedError).ToString("F2"));
            WaveData waveData2 = new WaveData();


            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(lstIDs, 2, ref waveData2, "充电电流");

            // 保存波形截图
            var screenImg = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(new Dictionary<int, string> { { 1, "" } }, testTitle, "波形截图", "-", "-", screenImg);


        }
        #endregion

        #region 工具方法
        private List<int> GetPendingChargerIds()
        {
            testWorkParam.lstIDs.Clear();
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                if (!testWorkParam.lstIDs.Contains(item.ChargerId))
                    testWorkParam.lstIDs.Add(item.ChargerId);
            }
            return testWorkParam.lstIDs;
        }

        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > TestTimeoutSeconds;
        }

        private void HandleTimeout(List<int> ids)
        {
            foreach (var item in LstTrialData.Where(x => x.IsCheck && x.TrialResult == EmTrialResult.Wait))
            {
                item.TrialResult = EmTrialResult.Fail;
                item.TrialValue = _StopWatch.Elapsed.Seconds.ToString();
                var charger = LstChargerInfo.FirstOrDefault(x => x.ChargerId == item.ChargerId);
                if (charger != null)
                    item.PKID = charger.PKID;
                item.ExtentData = "-|-|-|-|null";
                SendTrialDataToUI(item);
            }
        }

        private void ResetEquipmentState()
        {
            var kState = GetKStatus16_Charging_DC();
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, HighVoltage, DefaultVoltage, kState.ToArray());
            Thread.Sleep(300);
        }

        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡启动充电...");
            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = timeoutSec;

            while (timeout-- > 0)
            {
                var bms = AllEquipStateData.DicBMS_DC_StateData
                    .FirstOrDefault(x => ids.Contains(x.Value.ChargerID)).Value;

                if (bms != null && ChangeBMSChargeStatus(bms.ChargingState) >= 9)
                    break;

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion
    }
}