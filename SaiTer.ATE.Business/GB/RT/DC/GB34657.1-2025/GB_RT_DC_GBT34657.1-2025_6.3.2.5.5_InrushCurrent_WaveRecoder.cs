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
using static SaiTer.ATE.EquipMent.BMS_Protocol;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 冲击电流测试（34657.1-2025 A类 6.3.2.5.5）
    /// </summary>
    public class GB_RT_DC_2025_PauseChargingAndResume_WaveRecoder : BusinessBase
    {
        #region 常量与参数
        private readonly int _trialTimeoutSeconds = 30;
        private double _demandVoltage = 500;
        private double _demandCurrent = 3;
        #endregion

        #region 构造函数
        public GB_RT_DC_2025_PauseChargingAndResume_WaveRecoder(int type)
        {
            TrialType = type;
        }
        #endregion

        #region 初始化
        public override void InitializeParams()
        {
            Init();

            // 参数格式：BMS需求电压设置(V)=400|回馈负载电流设置(A)=100|小于等于20A下降电流(A)=20|大于20A下降电流(A)=60
            var strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 4)
            {
                _demandVoltage = double.Parse(strParams[0].Split('=')[1]);
                _demandCurrent = double.Parse(strParams[1].Split('=')[1]);
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
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + " 结束 ---------------------->");
                SendMessageEndThisTrial();
            }
        }
        #endregion

        #region 测试主流程
        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始 " + TrialItem.ItemName + " --------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();

                while (true)
                {
                    testWorkParam.lstIDs.Clear();
                    foreach (var data in LstTrialData.Where(d => d.IsCheck && d.TrialResult == EmTrialResult.Wait))
                    {
                        if (!testWorkParam.lstIDs.Contains(data.ChargerId))
                        {
                            testWorkParam.lstIDs.Add(data.ChargerId);
                        }
                    }

                    // 所有通道已完成
                    if (testWorkParam.lstIDs.Count <= 0)
                        break;

                    // 超时判断
                    if (_StopWatch.Elapsed.TotalSeconds > _trialTimeoutSeconds)
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
                        break;
                    }

                    #region 1. 充电桩主动停止充电 < 100A
                    RunChargingPileStopTest();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        #endregion

        #region 统一测试方法（核心抽离，消除重复代码）
        /// <summary>
        /// 统一执行电流停止速率测试
        /// </summary>
        /// <param name="isCarStop">是否车辆主动停止</param>
        /// <param name="isGreaterThan100A">是否使用大于100A电流</param>
        private void RunChargingPileStopTest()
        {
            double testCurrent =_demandCurrent;
            try
            {
                // BMS参数设置
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, testCurrent);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, testCurrent, false, 390);


                // 启动导引
                SendNoticeToUIAndTxtFile("开启导引中...");
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                // 启动录波
                SendNoticeToUIAndTxtFile("录波板启动录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                // 等待刷卡
                WaitForSwipeCard(testWorkParam.lstIDs, 200);

                //// 启动负载
                //SendNoticeToUIAndTxtFile("开启负载中...");
                //SetLoadPara(testWorkParam.lstIDs, _demandVoltage - 10, testCurrent + 10, _demandVoltage - 10, testCurrent);
                //SetLoadDCON(testWorkParam.lstIDs);
                //WaitDCCurrentWithTime(testWorkParam.lstIDs, testCurrent, 35);

                Thread.Sleep(13000);

                // 停止录波
                SendNoticeToUIAndTxtFile("录波板停止录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(500);

                // 读取电流波形
                var outputCurrentWave = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref outputCurrentWave, "充电电流");
                // 截图
                var screenImages = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

                // 获取最大电流
                var maxA = outputCurrentWave.LinePoints_Y.Max();
                var timeResult = new Dictionary<int, string> { { 1, maxA.ToString() } };
                // 判定标准
                ProcessDataTmp(timeResult, $"冲击电流测试", "冲击电流(A)", "0", "20", screenImages);

               

            }
            finally
            {
                // 关闭设备
                SendNoticeToUIAndTxtFile("关闭负载中...");
                SetLoadDCOFF(testWorkParam.lstIDs);

                SendNoticeToUIAndTxtFile("关闭导引中...");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                SetCPReresh();

            }
        }
        #endregion

        #region 等待方法
        private void WaitForSwipeCard(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡...");
            MessgaeInfo(true, "请刷卡充电!", true);

            int timeout = timeoutSec;
            while (timeout-- > 0)
            {
                var bmsState = AllEquipStateData.DicBMS_DC_StateData
                    .FirstOrDefault(x => ids.Contains(x.Value.ChargerID)).Value;

                if (bmsState == null) continue;

                int state = ChangeBMSChargeStatus(bmsState.ChargingState);
                if (state >= 9) break;

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }

        private void WaitForStop(List<int> ids, int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待充电桩停止充电");
            MessgaeInfo(true, "请在充电桩中点击停止充电!", true);

            int timeout = timeoutSec;
            while (timeout-- > 0)
            {
                var bmsState = AllEquipStateData.DicBMS_DC_StateData
                    .FirstOrDefault(x => ids.Contains(x.Value.ChargerID)).Value;

                if (bmsState == null) continue;

                int state = ChangeBMSChargeStatus(bmsState.ChargingState);
                if (state == 0) break;

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }

        private void WaitForCarStop(List<int> ids, int timeoutSec)
        {
            ControlEquipMent.BMS.BMS_OFF(ids);
            Thread.Sleep(200);
            SendNoticeToUIAndTxtFile("等待车辆停止充电...");

            int timeout = timeoutSec;
            while (timeout-- > 0)
            {
                var bmsState = AllEquipStateData.DicBMS_DC_StateData
                    .FirstOrDefault(x => ids.Contains(x.Value.ChargerID)).Value;

                if (bmsState == null) continue;

                int state = ChangeBMSChargeStatus(bmsState.ChargingState);
                if (state == 0) break;

                Thread.Sleep(1000);
            }

            MessgaeInfo(false, "");
        }
        #endregion

        #region 数据处理
        public override void ProcessData()
        {
            // 业务逻辑由 ProcessDataTmp 实现
        }
        #endregion
    }
}