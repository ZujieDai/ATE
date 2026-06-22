using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出电压控制误差测试（录波版）
    /// 标准：GB/T 34657.1-2025  6.3.2.5.1
    /// 测试编号：TC_DA.EVSE_P_EnergyTransfer.Charging_002
    /// 测试内容：能量传输阶段恒压输出误差 ΔU = UM - UO
    /// 合格依据：NB/T 33001 输出电压误差要求
    /// </summary>
    public class GB_RT_DC_2025_OutputVoltageControlError_WaveRecoder : BusinessBase
    {
        #region 常量
        private const int TestTimeoutSeconds = 60;
        private const int DefaultVoltage = 390;
        private const int HighVoltage = 750;
        private const int VoltageStableTime = 10000;   // 电压稳定等待时间

        #endregion

        #region 私有变量
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        private double _bmsDemandVoltage;
        #endregion

        #region 构造
        public GB_RT_DC_2025_OutputVoltageControlError_WaveRecoder(int type)
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
        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
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

                ExecuteVoltageErrorTest(pendingIds);
            }
        }

        /// <summary>
        /// 执行电压误差测试（多档位可扩展）
        /// </summary>
        private void ExecuteVoltageErrorTest(List<int> chargerIds)
        {
            try
            {
                //CheckSwipingCard(lstIDs, 300, 3, 750);

                Thread.Sleep(200);
                SetConditionValues();
                // 握手
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, HighVoltage);
                Thread.Sleep(200);

                // 参数配置
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, 750, 3);
                Thread.Sleep(200);

                // 充电需求
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 300, 3, true, 390);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(chargerIds);



                SendNoticeToUIAndTxtFile("等待刷卡");



                if (!WaitChargingSrat(100))
                {
                    SendNoticeToUIAndTxtFile("100S未能进入充电！！");
                }



                Thread.Sleep(200);
                // 测试点1：下限附近电压
                TestVoltagePoint(chargerIds, 300);

                // 测试点2：额定/典型电压
                TestVoltagePoint(chargerIds, 500);

                // 测试点3：上限附近电压
                TestVoltagePoint(chargerIds, 750);

                ControlEquipMent.BMS.BMS_OFF(chargerIds);

            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        private bool WaitChargingSrat(int timeout)
        {

            MessgaeInfo(true, "请刷卡充电!，进入充电状态自动关闭!");
            for (int i = 0; i < timeout; i++)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value).First().Value.ChargingState;

                if (bmsData == "充电中")
                {
                    SendNoticeToUIAndTxtFile("充电桩以进入充电");
                    //关闭提示信息
                    MessgaeInfo(false, "请刷卡充电!，进入充电状态自动关闭!");
                    return true;

                }

                Thread.Sleep(1000);
            }
            //关闭提示信息
            MessgaeInfo(false, "请刷卡充电!，进入充电状态自动关闭!");
            SendNoticeToUIAndTxtFile("充电桩以进入充电超时" + timeout);

            return false;

        }

        #endregion

        #region 电压点测试（核心）
        /// <summary>
        /// 单个电压点误差测试
        /// </summary>
        private void TestVoltagePoint(List<int> chargerIds, double targetVoltage)
        {
            string testTitle = $"恒压输出{targetVoltage}V";
            SendNoticeToUIAndTxtFile($"===== 开始 {testTitle} 测试 =====");
            
            // 充电需求
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, targetVoltage, 3, true,390);
         

            SendNoticeToUIAndTxtFile($"BMS 设置需求电压 UO = {targetVoltage} V");
            // 启动录波
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_ON(chargerIds);
            Thread.Sleep(200);
            MessgaeInfo(true, $"等待电压稳定 {VoltageStableTime / 1000}s...");
            // 等待稳定
            SendNoticeToUIAndTxtFile($"等待电压稳定 {VoltageStableTime / 1000}s...");
            WaitDCVoltage(testWorkParam.lstIDs, targetVoltage, VoltageStableTime);
            MessgaeInfo(false, $"等待电压稳定 {VoltageStableTime / 1000}s...");
            Thread.Sleep(2000);
            // 停止录波
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

            // 读取录波电压（通道1一般为输出电压）
            WaveData outputWave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(chargerIds, 1, ref outputWave, "输出电压");

            // 获取实测电压平均值
            BMS_DC_StateData bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.FirstOrDefault();
            double measureVoltage = bmsData.ChargingVoltage;
            double errorVoltage = measureVoltage - targetVoltage; // ΔU = UM - UO

            SendNoticeToUIAndTxtFile($"需求电压 UO = {targetVoltage:F2} V");
            SendNoticeToUIAndTxtFile($"实测电压 UM = {measureVoltage:F2} V");
            SendNoticeToUIAndTxtFile($"电压误差 ΔU = {errorVoltage:F2} V");
            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1, $" {targetVoltage:F2}" } }, testTitle, "需求电压 UO", "-", "-");
            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1, $" {measureVoltage:F2}" } }, testTitle, "实测电压 UM", "-", "-");
            // 上报结果
            ProcessDataTmp(new Dictionary<int, string> { { 1,$" {errorVoltage:F2}" } }, testTitle, "电压误差 ΔU", "-", "-");

            // 构造结果
            var resultData = new Dictionary<int, string>
            {
                { 1, measureVoltage.ToString("F2") },
            };


            // 上报结果
            ProcessDataTmp(resultData, testTitle, "实测电压(误差0.5%)", (targetVoltage * 0.995).ToString(), (targetVoltage * 1.005).ToString());
            // 保存截图
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