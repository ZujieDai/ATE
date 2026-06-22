using Newtonsoft.Json.Linq;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SaiTer.ATE.DataModel.Consist;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 研发测试：直流输出电流响应时间测试（录波板采集）
    /// 功能：测试充电桩输出电流在指令下发后的响应速度，通过录波板采集波形计算时间差
    /// </summary>
    public class GB_RT_DC_2025_OutputCResponseTime_WaveRecoder : BusinessBase
    {
        #region 常量与测试配置参数
        /// <summary>
        /// 单轮测试超时时间（秒）
        /// </summary>
        private readonly int _trialTimeoutSeconds = 30;

        /// <summary>
        /// 测试目标电压（默认750V）
        /// </summary>
        private double _demandVoltage = 750;

        /// <summary>
        /// 初始输出电流（默认40A）
        /// </summary>
        private double _demandCurrent = 40;

        /// <summary>
        /// 第一级下降目标电流（≤20A档位，默认35A）
        /// </summary>
        private double _minusCurrent1 = 35;

        /// <summary>
        /// 第二级下降目标电流（>20A档位，默认10A）
        /// </summary>
        private double _minusCurrent2 = 10;

        /// <summary>
        /// 录波板采样率（固定1KHz）
        /// </summary>
        private const int SamplingRate = 1;

        /// <summary>
        /// 电流稳定判断连续次数
        /// </summary>
        private const int WaitStableCount = 3000;

        /// <summary>
        /// 电流读取间隔（毫秒）
        /// </summary>
        private const int ReadIntervalMs = 1000;

        /// <summary>
        /// 电流允许误差比例（3%）
        /// </summary>
        private const double CurrentToleranceRatio = 0.03;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">测试类型</param>
        public GB_RT_DC_2025_OutputCResponseTime_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 初始化测试参数（读取配置、重置变量）
        /// </summary>
        public override void InitializeParams()
        {
            try
            {
                Init();
                ParseTestParameters();
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"参数初始化异常：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化测试设备（负载、BMS、录波板）
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            // 关闭直流负载
            SetLoadDCOFF(testWorkParam.lstIDs);
            // 关闭BMS导引
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            // 设置录波板采样率
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, SamplingRate);

            SendNoticeToUIAndTxtFile("设备初始化完成");
        }

        /// <summary>
        /// 测试主执行入口（统一调度初始化、设备、流程、结果）
        /// </summary>
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                if (_demandCurrent < 40)
                {
                    SystemEvent.MessageInfo(true, "起始电流不能小于40A,请修改参数设置");
                    return;
                }
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // 保存测试结果
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                // 发送测试结束消息
                SendMessageEndThisTrial();
            }
        }

        /// <summary>
        /// 测试项主流程（循环执行测试、超时判断、设备刷新）
        /// </summary>
        private void StartItemFlow()
        {
            SendNoticeToUIAndTxtFile($"开始{TrialItem.ItemName}--------------------------->");
            _StopWatch.Restart();

            try
            {
                while (true)
                {
                    // 刷新待测试充电桩列表
                    RefreshTestChargers();

                    // 无待测试设备则退出
                    if (!testWorkParam.lstIDs.Any())
                        break;

                    // 测试超时处理
                    if (IsTestTimeout())
                    {
                        HandleTimeoutResult();
                        break;
                    }

                    // 执行核心测试流程
                    ExecuteTestProcess();
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                SendNoticeToUIAndTxtFile($"测试流程异常：{ex.Message}");
            }
            finally
            {
                // 安全停止所有设备
                SafeStopEquipment();
            }
        }

        #region 核心测试流程
        /// <summary>
        /// 执行电流响应测试完整流程
        /// </summary>
        private void ExecuteTestProcess()
        {
            // 刷卡校验（电压/电流安全检查）
            if (!CheckSwipingCard(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, 750, false))
                return;

            Thread.Sleep(5000);

            // 启动负载输出
            StartLoad(_demandVoltage, _demandCurrent);
            // 等待电流稳定在初始值
            WaitDCCurrent(_demandCurrent, _demandCurrent, 10);
           // 第一个测试点：≤20A 下降电流测试
            RunCurrentResponseTest(_minusCurrent1, "调整后(小于等于20A下降电流)", 1000);

            // 第二个测试点：>20A 下降电流测试（计算理论响应时间）
            double checkTime1 = (_minusCurrent1 - _minusCurrent2) / 20.0;
            RunCurrentResponseTest(_minusCurrent2, "调整后(大于20A下降电流)", (int)(checkTime1 * 1000), (int)checkTime1);
        }

        /// <summary>
        /// 执行单步电流响应测试（录波+指令+计算）
        /// </summary>
        /// <param name="targetCurrent">目标电流</param>
        /// <param name="testItemName">测试项名称</param>
        /// <param name="maxTimeMs">最大允许响应时间</param>
        /// <param name="waitTimeout">稳定等待超时</param>
        private void RunCurrentResponseTest(double targetCurrent, string testItemName, int maxTimeMs, int waitTimeout = 5)
        {
            try
            {


              

                SendNoticeToUIAndTxtFile("录波板启动录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(3000);

                // 下发BMS电流调整指令
                SendNoticeToUIAndTxtFile($"下发指令改变电流至{targetCurrent}A");
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, targetCurrent, false, 390);

                // 等待输出电流达到目标值
                WaitDCCurrent(_demandCurrent, targetCurrent, waitTimeout);

                SendNoticeToUIAndTxtFile("录波板停止录波...");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(500);

                // 从录波数据计算响应时间
                var responseTime = CalculateResponseTime(targetCurrent, testItemName);
                SendNoticeToUIAndTxtFile($"响应时间：{responseTime:F2} ms");

                // 保存测试结果与截图
                SaveTestResult(responseTime, testItemName, maxTimeMs);
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"测试点[{targetCurrent}A]异常：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 从录波波形中计算 BCL指令时间 与 实际输出电流时间 的差值（响应时间）
        /// </summary>
        /// <param name="minusCurrent">目标电流</param>
        /// <returns>响应时间（ms）</returns>
        private double CalculateResponseTime(double minusCurrent, string testItemName)
        {
            WaveData chBclCurrent = new WaveData();
            WaveData chOutputCurrent = new WaveData();

            // 读取录波通道：130通道=BCL需求电流，2通道=实际输出电流
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 130, 0, ref chBclCurrent, "BCL需求电流");
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref chOutputCurrent, "充电电流");

            double timeBclStart = 0;
            double timeBclEnd = 0;
            double timeOutput = 0;

            // 解析波形：获取BCL电流变化起始点
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(chBclCurrent, false, minusCurrent, ref timeBclStart, 1000);

            // 解析波形：获取BCL电流变化结束点
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(chBclCurrent, false, minusCurrent, ref timeBclEnd, timeBclStart);

            // 解析波形：获取实际输出电流到达目标值的时间
            DataAnalysis_WaveRecoder.GetDCSingleTime(chOutputCurrent, false, minusCurrent, ref timeOutput);

            // 异常数据保护
            if (timeBclStart == 1000 && timeBclEnd == 1000)
            {
                timeOutput = timeBclEnd = 0;
            }
            
            SendNoticeToUIAndTxtFile($"时间数据：BclStart={timeBclStart}，BclCurrent={timeBclEnd}，OutputCurrent={timeOutput}");

            ProcessDataTmp(new Dictionary<int, string> { { 1, chBclCurrent.LinePoints_Y[1000].ToString() } }, testItemName, "BCL当前值", "-", "-");

            // 处理数据并上传
            ProcessDataTmp(new Dictionary<int, string> { { 1, minusCurrent.ToString() } }, testItemName, "BCL目标值", "-", "-");

            // 设置录波光标用于查看波形
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, timeBclEnd);
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, timeOutput);

            // 响应时间 = 指令结束时间 - 实际输出响应时间
            return Math.Abs(timeBclEnd - timeOutput);
        }

        /// <summary>
        /// 保存测试结果（时间、判定、截图）
        /// </summary>
        private void SaveTestResult(double responseTime, string testItem, int maxTime)
        {
            SystemEvent.MessageInfo(true, "判断结果中...");

            // 组装时间结果
            var timeDic = testWorkParam.lstIDs.ToDictionary(id => id, _ => responseTime.ToString("F2"));
            // 保存录波截图
            var screenDic = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            // 处理数据并上传
            ProcessDataTmp(timeDic, testItem, "时间差(ms)", "1", maxTime.ToString(), screenDic);

            SystemEvent.MessageInfo(false, "判断完成");
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 解析外部传入的测试参数（电压、电流、降流值）
        /// </summary>
        private void ParseTestParameters()
        {
            if (string.IsNullOrWhiteSpace(TrialItem.ResultParams))
                return;

            var paramPairs = TrialItem.ResultParams.Split('|');
            if (paramPairs.Length < 4)
                return;
            _demandVoltage = ParseParameter(paramPairs[0]);
            _demandCurrent = ParseParameter(paramPairs[1]);
            _minusCurrent1 = _demandCurrent-5;
            _minusCurrent2 = _minusCurrent1-30;
        }

        /// <summary>
        /// 解析单个参数（key=value格式）
        /// </summary>
        private double ParseParameter(string paramStr)
        {
            var parts = paramStr.Split('=');
            return parts.Length >= 2 && double.TryParse(parts[1], out double value) ? value : 0;
        }

        /// <summary>
        /// 刷新待测试充电桩ID列表
        /// </summary>
        private void RefreshTestChargers()
        {
            testWorkParam.lstIDs.Clear();
            var waitChargers = LstTrialData.Where(t => t.IsCheck && t.TrialResult == EmTrialResult.Wait).ToList();

            foreach (var item in waitChargers)
            {
                if (!testWorkParam.lstIDs.Contains(item.ChargerId))
                    testWorkParam.lstIDs.Add(item.ChargerId);
            }
        }

        /// <summary>
        /// 判断测试是否超时
        /// </summary>
        private bool IsTestTimeout()
        {
            return _StopWatch.Elapsed.TotalSeconds > _trialTimeoutSeconds;
        }

        /// <summary>
        /// 超时处理：标记测试失败
        /// </summary>
        private void HandleTimeoutResult()
        {
            var waitItems = LstTrialData.Where(t => t.IsCheck && t.TrialResult == EmTrialResult.Wait).ToList();
            foreach (var item in waitItems)
            {
                item.TrialResult = EmTrialResult.Fail;
                item.TrialValue = ((int)_StopWatch.Elapsed.TotalSeconds).ToString();
                var charger = LstChargerInfo.FirstOrDefault(c => c.ChargerId == item.ChargerId);
                if (charger != null)
                    item.PKID = charger.PKID;

                item.ExtentData = "-|-|-|-|null";
                SendTrialDataToUI(item);
            }
        }

        /// <summary>
        /// 启动直流负载输出
        /// </summary>
        private void StartLoad(double voltage, double current)
        {
            SendNoticeToUIAndTxtFile("开启负载中...");
            SetLoadPara(testWorkParam.lstIDs, voltage - 10, current + 10, voltage - 10, current);
            Thread.Sleep(200);
            SetLoadDCON(testWorkParam.lstIDs);
        }

        /// <summary>
        /// 安全停止设备（关闭负载+关闭BMS）
        /// </summary>
        private void SafeStopEquipment()
        {
            try
            {
                SendNoticeToUIAndTxtFile("关闭负载中!");
                SetLoadDCOFF(testWorkParam.lstIDs);

                SendNoticeToUIAndTxtFile("关闭导引中!");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"设备停止异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 等待直流电流到达目标值并稳定
        /// </summary>
        /// <param name="currentStart">起始电流</param>
        /// <param name="currentEnd">目标电流</param>
        /// <param name="timeout">超时时间</param>
        public void WaitDCCurrent(double currentStart, double currentEnd, int timeout)
        {
            SendNoticeToUIAndTxtFile("等待导引电流稳定...");
            int stableCount = 0;

            while (timeout-- > 0)
            {
                double realCurrent = GetChargingCurrent();
                bool isTargetReached = CheckCurrentReached(realCurrent, currentStart, currentEnd);

                // 连续达标则计数+1，否则重置
                stableCount = isTargetReached ? stableCount + 1 : 0;

                // 达到稳定次数或超时则退出
                if (timeout < 0 || stableCount > WaitStableCount)
                    break;

                Thread.Sleep(ReadIntervalMs);
            }
        }

        /// <summary>
        /// 获取当前充电电流
        /// </summary>
        private double GetChargingCurrent()
        {
            if (LstChargerInfo.Count == 0) return 0;
            var chargerId = LstChargerInfo[0].ChargerId;
            return AllEquipStateData.DicBMS_DC_StateData.TryGetValue(chargerId, out var state)
                ? state.ChargingCurrent
                : 0;
        }

        /// <summary>
        /// 判断电流是否达到目标区间（允许3%误差）
        /// </summary>
        private bool CheckCurrentReached(double realCurrent, double start, double end)
        {
            if (start > end)
                // 降流模式：实际电流 ≤ 目标电流*1.03
                return realCurrent <= end * (1 + CurrentToleranceRatio);
            else
                // 升流模式：实际电流 ≥ 目标电流*0.97
                return realCurrent >= end * (1 - CurrentToleranceRatio);
        }
        #endregion
       
        /// <summary>
        /// 数据处理（本业务未实现）
        /// </summary>
        public override void ProcessData()
        {
            // 业务未实现
        }
    }
}