
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 充电连接控制时序测试（录波板版）- 符合GB/T 34657.1-2025A类 6.3.2.3标准
    /// 核心流程：机械锁状态检测 → 车桩握手 → 参数配置 → 充电启停 → 录波数据分析 → 状态恢复
    /// </summary>
    public class GB_RT_DC_2025_ChargingConnectionControlTiming_WaveRecoder : BusinessBase
    {
        // 测试参数：需求电压/电流（可通过配置参数覆盖）
        private double _demandVoltage = 500;
        private double _demandCurrent = 30;
        // 录波图片路径字典（设备ID → 图片路径）
        private Dictionary<int, string> _dicImagePath = new Dictionary<int, string>();
        // 超时控制常量
        private const int MAX_TEST_TIMEOUT_SEC = 10;    // 整体测试超时
        private const int CHARGE_READY_TIMEOUT = 200;   // 充电就绪超时（秒）
        Dictionary<EMessageType, bool> _MessageOn=new Dictionary<EMessageType, bool>();
        private readonly double VoltageJudgeOffset=5;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="trialType">测试类型（研发/量产等）</param>
        public GB_RT_DC_2025_ChargingConnectionControlTiming_WaveRecoder(int trialType)
        {
            TrialType = trialType;
        }

        /// <summary>
        /// 初始化测试参数（从配置读取需求电压/电流）
        /// </summary>
        public override void InitializeParams()
        {
            Init();
            _dicImagePath = new Dictionary<int, string>();

            // 解析配置参数：格式 "BMS需求电压(V)=500|BMS需求电流(A)=100"
            if (!string.IsNullOrEmpty(TrialItem.ResultParams))
            {
                string[] strParams = TrialItem.ResultParams.Split('|');
                if (strParams.Length >= 2)
                {
                    _demandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                    _demandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                }
            }

            SendNoticeToUIAndTxtFile($"初始化测试参数完成：需求电压={_demandVoltage}V，需求电流={_demandCurrent}A");
        }



        /// <summary>
        /// 初始化测试设备（录波板采样率设置）
        /// </summary>
        public override void InitEquiMent()
        {
            // 设置录波板采样率为1k/s（1000次/秒）
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);
            SendNoticeToUIAndTxtFile("录波板初始化完成，采样率设置为1k/s");
        }

        /// <summary>
        /// 核心执行方法（封装异常处理和最终清理）
        /// </summary>
        public override void ExecuteMethod()
        {
            try
            {

                // 1. 初始化阶段
                InitializeParams();
                InitEquiMent();

                // 2. 执行核心测试流程
                StartItemFlow();
            }
            catch (Exception ex)
            {
                // 异常处理：记录并上报
                SendException(ex);
                SendNoticeToUIAndTxtFile($"测试执行异常：{ex.Message}");
            }
            finally
            {
                // 最终处理：状态恢复 + 结果保存 + 通知UI
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();

                // 确保BMS关闭，避免设备残留状态
                if (testWorkParam.lstIDs.Count > 0)
                {
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 核心测试流程（对齐GB/T 34657.1-2025A类时序）
        /// 流程：T0-T2机械锁检测 → 车桩握手 → 参数配置 → 充电启停 → 录波分析 → T19-T21状态复检
        /// </summary>
        public void StartItemFlow()
        {
            SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");

            // 初始化超时计时器
            _StopWatch.Reset();
            _StopWatch.Start();

            #region 前置检查：设备列表有效性 + 超时控制
            // 筛选待测试设备（状态为Wait的设备）
            testWorkParam.lstIDs.Clear();
            foreach (var trialData in LstTrialData)
            {
                if (trialData.IsCheck && trialData.TrialResult == EmTrialResult.Wait && !testWorkParam.lstIDs.Contains(trialData.ChargerId))
                {
                    testWorkParam.lstIDs.Add(trialData.ChargerId);
                }
            }

            // 无待测试设备，直接退出
            if (testWorkParam.lstIDs.Count <= 0)
            {
                SendNoticeToUIAndTxtFile("无待测试设备，流程结束");
                return;
            }

            // 整体测试超时判定
            if (_StopWatch.ElapsedMilliseconds / 1000 > MAX_TEST_TIMEOUT_SEC)
            {
                SendNoticeToUIAndTxtFile($"测试超时（>{MAX_TEST_TIMEOUT_SEC}秒），标记为失败");
                foreach (var trialData in LstTrialData.Where(d => d.IsCheck && d.TrialResult == EmTrialResult.Wait))
                {
                    trialData.TrialResult = EmTrialResult.Fail;
                    trialData.TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                    int k = LstChargerInfo.FindIndex(s => s.ChargerId == trialData.ChargerId);
                    trialData.PKID = LstChargerInfo[k].PKID;
                    trialData.ExtentData = "null|null|null|null|null";
                    SendTrialDataToUI(trialData);
                }
                return;
            }
            #endregion

            try
            {
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                #region #region 阶段1：充电枪机械锁初始状态检测（T0-T2）
                TestPhase_T0_T2_MechanicalLock();
                #endregion

                WaveData CC1waveData = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CC1waveData,"CC1电压");

                var dicImagePath = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                Dictionary<int, string> apsVoltageValue = new Dictionary<int, string>()
                {
                    { 1,""}
                };
                ProcessDataTmp(apsVoltageValue, "充电连接控制时序", "时序图", "-", "-", dicImagePath);
                SendNoticeToUIAndTxtFile("关闭BMS，准备录波板参数");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
                SetCPReresh();
                #region 阶段2：车桩通信参数配置
                Thread.Sleep(200);
                SendNoticeToUIAndTxtFile("设置BMS参数，并启动充电");

                // 配置BMS基础参数（标称电压）
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                Thread.Sleep(200);

                // 配置BMS辅助参数（390V/标称电压/250A）
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                Thread.Sleep(200);

                // 配置BMS充电需求参数（核心需求电压/电流）
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, _demandCurrent, true, _demandVoltage);
                Thread.Sleep(300);
                #endregion

                #region  阶段3：启动充电


                // 启动BMS
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(200);


                // 提示用户刷卡充电，并等待充电就绪
                MessgaeInfo(true, "请刷卡充电!", true);


                // T2'阶段：检测电子锁锁止状态
                TestPhaseT2Prime();


                //报文检测线程
                Task.Run(MessageON, cancellationTokenSource.Token);

                Task.Run(() =>
                {

                    #region T3 充电机闭合S3和S4,使低压辅助供电回路导通
                    WaitAPSUp(60);
                    ChackLokc();

                   var apsVoltageValue2 = new Dictionary<int, string>()
                {
                    {1,AllEquipStateData.DicBMS_DC_StateData.First().Value.APSVoltage.ToString()}
                };
                    ProcessDataTmp(apsVoltageValue2, "T3", "辅源电压", "11.2", "12.8");
                    #endregion
                });
              



                bool isChargeReady = WaitChargeReady(CHARGE_READY_TIMEOUT);

                MessgaeInfo(false, "请刷卡充电!");

                cancellationTokenSource.Cancel();

                if (!isChargeReady)
                {
                    SendNoticeToUIAndTxtFile("充电就绪超时，测试终止");
                    MarkTrialResult(EmTrialResult.Fail, "充电就绪超时");
                    cancellationTokenSource.Cancel();
                    return;
                }
                int loadTime = 10;
                SendNoticeToUIAndTxtFile($"启动负载,并带载{loadTime}秒");
                SetLoadPara(testWorkParam.lstIDs, _demandVoltage - 10, _demandCurrent + 10, _demandVoltage - 10, _demandCurrent);
                Thread.Sleep(300);
                SetLoadDCON(testWorkParam.lstIDs);
                Thread.Sleep(loadTime * 1000);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                SendNoticeToUIAndTxtFile("关闭负载,停止充电...");
                SetLoadDCOFF(testWorkParam.lstIDs);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);
                //读取录波板数据
                SendNoticeToUIAndTxtFile("读取录波板所有数据...");

                #endregion

                #region 阶段4：录波数据解析（T6-T18）
                double TimeStart = 0;
                TestPhaseT6_T9Prime( ref TimeStart);
                TestPhaseT10(ref TimeStart);

                TestPhaseT12_T13(ref TimeStart);
                
                TestPhaseT14_16(ref TimeStart);


                #endregion

                #region 阶段8：T19-T21 机械锁状态复检（对齐原流程）
                TestPhase_T19_T21_MechanicalLockCheck();
                #endregion

                #region 阶段9：人工验证（电子锁解锁）
                SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
                CountDownTimeInfo("请确认电子锁能正常解锁 \r\n注:勾选上为可正常解锁", 20, 2);
               var dicManualResult = new Dictionary<int, string>();
                foreach (var item in DicManualVerifyResult)
                {
                    dicManualResult.Add(item.Key, item.Value ? "可靠锁止" : "未锁止");
                }
                ProcessDataTmp(dicManualResult, "正常充电", "应可靠锁止", "-", "-");
                #endregion

                // 标记测试成功
                MarkTrialResult(EmTrialResult.Pass, "测试完成");
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"流程执行异常：{ex.Message}");
                MarkTrialResult(EmTrialResult.Fail, $"执行异常：{ex.Message}");
            }
            finally
            {
                // 最终恢复BMS状态
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 停止充电之后的电流,C1C2变化
        /// </summary>
        /// <param name="TimeStart"></param>
        private void TestPhaseT14_16(ref double TimeStart)
        {
            var Status = new Dictionary<int, bool>()
            {
                { 1,false}
            };
            WaveData CH_CST = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, 137, 0, ref CH_CST, "CST");
            if (CH_CST.LinePoints_Y.Count > 0)
                Status[1] = true;
            else
                Status[1]=false;
            ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "T14 是否收到CST报文");

            //获取充电电流
            WaveData Curr = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref Curr, "充电电流");

            //下降前的电流时间
            var CurrUPTimeStart = 0.0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(Curr, false, _demandCurrent-5, ref CurrUPTimeStart, TimeStart);

            //下降到5A电流时间
            var CurrUPTimeEnd = 0.0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(Curr, false,  5, ref CurrUPTimeEnd, CurrUPTimeStart);

            SetWaveRecorderCursor(CurrUPTimeStart, CurrUPTimeEnd);
            //读取光标数据
            var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            var timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            // 合格条件：下降时间 ≤ (充电电流-5)/20 * 1000 ms
            ProcessDataTmp(timeDiffData, "T14", "电流下降到5A的时间(ms)", "0", $"{Math.Abs((_demandCurrent - 5) / 20) * 1000}", timeDiffImgs);

            //获取充电电压
            WaveData Vol = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref Vol, "充电电压");

            Dictionary<int, bool> dicManualResult = new Dictionary<int, bool>()
            {
                {1,Vol.LinePoints_Y[(int)CurrUPTimeEnd]<60}
            };
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "断开" : "闭合", "断开", dicManualResult, "T15 C1C2状态");

            //下降前的电压时间
            var VolUPTimeStart = 0.0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(Vol, false, _demandCurrent - 5, ref VolUPTimeStart, TimeStart);

            //下降到5A电压时间
            var VolUPTimeEnd = 0.0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(Vol, false, 5, ref VolUPTimeEnd, VolUPTimeStart);

            SetWaveRecorderCursor(VolUPTimeStart, VolUPTimeEnd);

            //读取光标数据
            cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            // 合格条件：小于 1000 ms
            ProcessDataTmp(timeDiffData, "T16", "电压泄放时间(ms)", "0", $"1000", timeDiffImgs);

            //时间点移到电流都小于5A的时间
            TimeStart = CurrUPTimeEnd;
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
        /// 临时测试数据
        /// </summary>
        private readonly Dictionary<int, string> _tempTestData = new Dictionary<int, string>();
        /// <summary>
        /// 分析并记录时间差数据
        /// </summary>
        /// <param name="testType">测试类型</param>
        private void AnalyzeAndRecordTimeDifference(string testType)
        {


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
        /// 绝缘检测电压
        /// </summary>
        private void TestPhaseT6_T9Prime(ref double TimeStart)
        {

            #region 绝缘检测电压
            var dicResult = new Dictionary<int, string>()
            {{1,"0" } };


            WaveData waveData = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData);
            // 分析电压变化时间
            double timeStart = 0, timeEnd = 0;
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveData, true, (LstChargerInfo[0].NominalVoltage-5), ref timeStart);
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveData, false, _demandVoltage-10, ref timeEnd, timeStart);
            TimeStart = timeEnd;
            //处理时间重叠问题
            if (Math.Abs(timeStart - timeEnd) < double.Epsilon)
            {
                timeEnd++;
                LogWarning("电压变化时间重叠，已自动调整结束时间");
            }
            SetWaveRecorderCursor(timeStart, timeEnd);
            //读取光标数据
           var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            var timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);

            dicResult[1] = waveData.LinePoints_Y[ (int)timeStart].ToString();

            ProcessDataTmp(dicResult, "T7", "绝缘检测电压", (LstChargerInfo[0].NominalVoltage-7).ToString(), (LstChargerInfo[0].NominalVoltage + 7).ToString(), timeDiffImgs);

           

            Dictionary<int, bool> dicManualResult = new Dictionary<int, bool>()
            {
                {1, cursorTimes.Values.All(x => x > 80)}
            };
            dicResult[1]= timeStart.ToString();
            ProcessDataTmp(dicResult, "T7", "C1C2变化时刻(ms)", "-", "-");

            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "闭合" : "断开", "闭合", dicManualResult, "T7绝缘检测 C1C2状态");
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "闭合" : "断开", "闭合", dicManualResult, "T8绝缘检测 C1C2状态");
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "闭合" : "断开", "闭合", dicManualResult, "T9绝缘检测 C1C2状态");
            #endregion
        }

        /// <summary>
        /// T10阶段测试：C1C2断开状态 + 电压泄放时间检测（0<时间<1000ms为合格）
        /// </summary>
        private void TestPhaseT10(ref double TimeStart)
        {

            // C1C2状态：断开为合格
            var dicResult = new Dictionary<int, string>()
            {{1,"" } };


            WaveData waveData = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData);

            double timeStart = 0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveData, false, _demandVoltage - 5, ref timeStart, TimeStart);
            double timeEnd = 0;
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveData, false, 30, ref timeEnd, timeStart);
            SetWaveRecorderCursor(timeStart, timeEnd);
            //读取光标数据
            var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            var timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(timeDiffData, "T10", "泄放时间(ms)", "0", "1000", timeDiffImgs);


            Dictionary<int, bool> dicManualResult = new Dictionary<int, bool>()
            {
                {1,waveData.LinePoints_Y[(int)timeEnd]<60}
            };
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "断开" : "闭合", "断开", dicManualResult, "T10绝缘检测 C1C2状态");
            dicResult[1] = timeStart.ToString();
            ProcessDataTmp(dicResult, "T10", "C1C2变化时刻(ms)", "-", "-");
            //泄放结束后的时间
            TimeStart = timeEnd;

        }

        /// <summary>
        /// 阶段测试：C5C6开关状态检测（闭合为合格）充电开始
        /// </summary>
        private void TestPhaseT12_T13(ref double TimeStart)
        {

            double timeStart = 0;
            var CRO = ReadWaveDigitalChannel(30, 0, EMessageType.CRO);
            //CRO =AA 充电机准备就绪
            DataAnalysis_WaveRecoder.GetDCSingleTime(CRO.waveData, true, 100, ref timeStart);

            WaveData waveData = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref waveData,"充电电压");
            SetWaveRecorderCursor(timeStart, timeStart+1000);
            //读取光标数据
            var cursorTimes = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs) ?? new Dictionary<int, double>();
            var timeDiffData = cursorTimes.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value).ToString());

            // 保存时间差截图
            var timeDiffImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            var Vol = waveData.LinePoints_Y[(int)timeStart];
            Dictionary<int, bool> dicManualResult = new Dictionary<int, bool>()
            {
                {1,waveData.LinePoints_Y[(int)timeStart]>80}
            };
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "闭合" : "断开", "闭合", dicManualResult, "T12 C5C6状态");
            ProcessDataResults(testWorkParam.lstIDs, dicManualResult.First().Value ? "闭合" : "断开", "闭合", dicManualResult, "T13 C1C2状态");
            
            Dictionary<int, string> T13Vol = new Dictionary<int, string>()
            {
                {1,Vol.ToString()}
            };

            ProcessDataTmp(T13Vol, "T13", " 预充电压", $"{390 - 10}", $"{390 + 10}", timeDiffImgs);
            TimeStart = (timeStart + 2000);


        }
        private void ChackLokc()
        {
            // 检测电子锁状态（锁止为合格）
            SendNoticeToUIAndTxtFile("开始人工验证电子锁状态");
            CountDownTimeInfo("请确认电子锁能正常上锁 \r\n注:勾选上为可正常上锁", 20, 2);
            Dictionary<int, bool> dicManualResult = new Dictionary<int, bool>();
            foreach (var item in DicManualVerifyResult)
            {
                dicManualResult.Add(item.Key,item.Value);
            }
            ProcessDataResults(testWorkParam.lstIDs, DicManualVerifyResult.First().Value?"上锁":"未上锁", "上锁", dicManualResult, "T2'电子锁状态");
        }

        private void MessageON()
        {
            Dictionary<int, bool> Status = new Dictionary<int, bool>();
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (_MessageOn.Count == 5)
                {
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("全部报文正常");
                    break;
                }
                Thread.Sleep(200);
                var CHM = ReadWaveDigitalChannel(1, 1, EMessageType.CHM);
                var CRM = ReadWaveDigitalChannel(3, 2, EMessageType.CRM);
                var CRO = ReadWaveDigitalChannel(30, 0, EMessageType.CRO);
                var CML = ReadWaveDigitalChannel(25, 0, EMessageType.CML);
                var CCS = ReadWaveDigitalChannel(137, 0, EMessageType.CCS);


                if (CHM.waveData.LinePoints_Y!=null&& CHM.waveData.LinePoints_Y.Count > 0 && !_MessageOn.Keys.Contains(EMessageType.CHM))
                {
                    _MessageOn[CHM.MessageName] = true;
                    Status = new Dictionary<int, bool>()
                        {
                            { 1, true},
                        };
                    ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "是否收到" + CHM.MessageName);
                }
                if (CRM.waveData.LinePoints_Y != null && CRM.waveData.LinePoints_Y.Count > 0 && !_MessageOn.Keys.Contains(EMessageType.CRM))
                {
                    _MessageOn[CRM.MessageName] = true;
                    Status = new Dictionary<int, bool>()
                        {
                            { 1, true},
                        };
                    ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "是否收到" + CRM.MessageName);
                }
                if (CRO.waveData.LinePoints_Y != null && CRO.waveData.LinePoints_Y.Count > 0 && !_MessageOn.Keys.Contains(EMessageType.CRO))
                {
                    _MessageOn[CRO.MessageName] = true;
                    Status = new Dictionary<int, bool>()
                        {
                            { 1, true},
                        };
                    ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "是否收到" + CRO.MessageName);
                }
                if (CML.waveData.LinePoints_Y != null && CML.waveData.LinePoints_Y.Count > 0 && !_MessageOn.Keys.Contains(EMessageType.CML))
                {
                    _MessageOn[CML.MessageName] = true;
                    Status = new Dictionary<int, bool>()
                        {
                            { 1, true},
                        };
                    ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "是否收到" + CML.MessageName);
                }
                if (CCS.waveData.LinePoints_Y != null && CCS.waveData.LinePoints_Y.Count > 0 && !_MessageOn.Keys.Contains(EMessageType.CCS))
                {
                    _MessageOn[CCS.MessageName] = true;
                    Status = new Dictionary<int, bool>()
                        {
                            { 1, true},
                        };
                    ProcessDataResults(testWorkParam.lstIDs, "收到", "收到", Status, "是否收到" + CCS.MessageName);
                }

            }
        }

        private void TestPhaseT2Prime()
        {
            // 等待60秒，直到充电状态离开"等待低压辅助电源"

            SendNoticeToUIAndTxtFile("执行T2'阶段测试：检测电子锁锁止状态");
            for (int i = 0; i < 60; i++)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 1);
                if (ALLCanCharge)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
           
        }



        /// <summary>
        /// 变化区间
        /// </summary>
        /// <param name="waveData"></param>
        /// <param name="isRise"></param>
        /// <param name="StartValue"></param>
        /// <param name="EndValue"></param>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        private (double timeStart, double timeEnd) AnalyzeVoltageChangeTime(WaveData waveData,bool isRise, double StartValue, double EndValue)
        {

            var timeStart = 0.0; var timeEnd = 0.0;
           DataAnalysis_WaveRecoder.GetDCSingleTime(waveData, isRise, StartValue, ref timeStart);
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveData, isRise, EndValue, ref timeEnd);

            // 处理时间重叠问题
            if (Math.Abs(timeStart - timeEnd) < double.Epsilon)
            {
                timeEnd++;
                LogWarning("电压变化时间重叠，已自动调整结束时间");
            }
            return (timeStart, timeEnd);
        }
        /// <summary>
        /// 警告日志记录
        /// </summary>
        /// <param name="message">警告信息</param>
        private void LogWarning(string message)
        {
            SendNoticeToUIAndTxtFile($"【警告】{message}");
        }
        #region 私有方法：各测试阶段封装（对齐原Test_GB_DC_2025A流程）
        /// <summary>
        /// T0-T2阶段：机械锁状态 + CC1/CC2电压初始检测
        /// </summary>
        private void TestPhase_T0_T2_MechanicalLock()
        {
            var Charger = LstChargerInfo.First();
            double R4;
            double volt;
            int timeOut = 0;
            double T0CC1 = 0, T0CC2 = 0, T1CC1 = 0, T1CC2 = 0, T2CC1 = 0, T2CC2=0;
            //机械锁(开关S)状态
            Dictionary<int,bool> SwStatus=new Dictionary<int,bool>();

            Dictionary<int, List<bool>> dic = ControlEquipMent.BMS.BMSGetKState_DC(lstIDs, out R4, out volt, new string[] { "emtBMS_GB_DC" });
            SendNoticeToUIAndTxtFile("发送CC1、CC2断线！");
            List<bool> Ks = dic.First().Value;
            Ks[22] = false;
            Ks[23] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, Charger.NominalVoltage, 30, Ks.ToArray());

            

            #region T0阶段：断开机械锁，检测CC1电压（标称12V±0.8V）
            SendNoticeToUIAndTxtFile("T0阶段：断开机械锁，检测CC1电压");
            SystemEvent.MessageInfo(true, "请按住枪锁不松开!");

            while (timeOut < 100)
            {
                T0CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                T0CC2 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

                if (T0CC1 >=11.2 && T0CC1 <= 12.8 && T0CC2 >= 11.2 && T0CC2 <= 12.8)
                    break;
                timeOut++;
                Thread.Sleep(100);
            }
            // 此处可添加机械锁断开控制 + CC1电压检测逻辑
            // 示例：读取当前CC1电压并验证
            T0CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
            T0CC2 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

            bool isCC1T0Pass = CheckVoltageInRange(T0CC1.ToString(), 11.2, 12.8);


            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], T0CC1.ToString() } },
                "T0阶段", "检测点1,CC1电压(V)", "11.2", "12.8");
            SendNoticeToUIAndTxtFile($"T0阶段检测点1电压：{T0CC1.ToString()}V，判定：{(isCC1T0Pass ? "合格" : "不合格")}");

            bool isCC2T0Pass = CheckVoltageInRange(T0CC2.ToString(), 11.2, 12.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], T0CC2.ToString() } },
                "T0阶段", "CC2电压(V)", "11.2", "12.8");
            SendNoticeToUIAndTxtFile($"T0阶段CC2电压：{T0CC2.ToString()}V，判定：{(isCC2T0Pass ? "合格" : "不合格")}");

            SwStatus.Add(1, isCC1T0Pass);
            ProcessDataResults(testWorkParam.lstIDs, SwStatus.First().Value ? "断开" : "闭合", "机械锁(开关S)状态 断开", SwStatus, "T0阶段");

            #endregion

            #region T1阶段：松开机械锁，检测CC1电压（标称6V±0.8V）

            SendNoticeToUIAndTxtFile("发送CC1、CC2闭合！");
            Ks[22] = true;
            Ks[23] = true;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, Charger.NominalVoltage, 30, Ks.ToArray());
            timeOut = 0;
            while (timeOut < 10)
            {
                T1CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                T1CC2 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

                if (T1CC1 >= 5.2 && T1CC1 <= 6.8 && T1CC2 >= 5.2 && T1CC2 <= 6.8)
                    break;
                timeOut++;
                Thread.Sleep(1000);
            }
            SendNoticeToUIAndTxtFile("T1阶段：松开机械锁，检测CC1电压,检测CC2电压");

            bool isCC1T1Pass = CheckVoltageInRange(T1CC1.ToString(), 5.2, 6.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], T1CC1.ToString() } },
                "T1阶段", "CC1电压(V)", "5.2", "6.8");
            SendNoticeToUIAndTxtFile($"T1阶段CC1电压：{T1CC1}V，判定：{(isCC1T1Pass ? "合格" : "不合格")}");

            bool isCC2T1Pass = CheckVoltageInRange(T1CC2.ToString(), 5.2, 6.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], T1CC2.ToString() } },
                "T1阶段", "CC2电压(V)", "5.2", "6.8");
            SendNoticeToUIAndTxtFile($"T1阶段CC2电压：{T1CC2}V，判定：{(isCC2T1Pass ? "合格" : "不合格")}");

            SwStatus.Clear();
            SwStatus.Add(1, isCC1T1Pass);
            ProcessDataResults(testWorkParam.lstIDs, SwStatus.First().Value ? "闭合" : "断开 ", "机械锁(开关S)状态 闭合", SwStatus, "T1阶段");
            SystemEvent.MessageInfo(false, "请按住枪锁不松开!");

            #endregion

            #region T2阶段：闭合/松开机械锁，检测CC1/CC2电压
            SendNoticeToUIAndTxtFile("T2阶段：闭合/松开机械锁，检测CC1/CC2电压");
            SystemEvent.MessageInfo(true, "请松开枪锁!");

            timeOut = 0;
            while (timeOut < 5)
            {
                T2CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                T2CC2 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

                if (T2CC1 >= 3.2 && T2CC1 <= 4.8&& T2CC2 >= 5.2 && T2CC2 <= 6.8)
                    break;
                timeOut++;
                Thread.Sleep(1000);
            }
            SystemEvent.MessageInfo(false, "请松开枪锁!");

            string cc1VoltT2 = T2CC1.ToString();
            string cc2VoltT2 = T2CC2.ToString();
            bool isCC1T2Pass = CheckVoltageInRange(cc1VoltT2, 3.2, 4.8); // 标称4V±0.8V
            bool isCC2T2Pass = CheckVoltageInRange(cc2VoltT2, 5.2, 6.8); // 标称6V±0.8V

            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc1VoltT2 } },
                "T2阶段", "CC1电压(V)", "3.2", "4.8");
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc2VoltT2 } },
                "T2阶段", "CC2电压(V)", "5.2", "6.8");
            SendNoticeToUIAndTxtFile($"T2阶段CC1：{cc1VoltT2}V({(isCC1T2Pass ? "合格" : "不合格")})，CC2：{cc2VoltT2}V({(isCC2T2Pass ? "合格" : "不合格")})");
            
            SwStatus.Clear();
            SwStatus.Add(1, isCC1T0Pass);
            ProcessDataResults(testWorkParam.lstIDs, SwStatus.First().Value ? "闭合" : "断开", "机械锁(开关S)状态 闭合", SwStatus, "T2阶段");
            #endregion
        }

        /// <summary>
        /// T19-T21阶段：充电后机械锁状态复检
        /// </summary>
        private void TestPhase_T19_T21_MechanicalLockCheck()
        {
            SendNoticeToUIAndTxtFile("开始T19-T21阶段：充电后机械锁状态复检");

            int timeOut = 0;
            double cc1VoltT19 = 0, cc2VoltT19 = 0, cc1VoltT20 = 0, cc1VoltT21 = 0, cc2VoltT20 = 0, cc2VoltT21 = 0;
            var Charger = LstChargerInfo.First();
            //机械锁(开关S)状态
            Dictionary<int, bool> SwStatus = new Dictionary<int, bool>();

            #region T19阶段：按下机械锁，检测CC1/CC2电压
            SystemEvent.MessageInfo(true, "请按住枪锁不松开!");
            while (timeOut < 10)
            {
                cc1VoltT19 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                cc2VoltT19 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;
                if (cc1VoltT19 >= 5.2 && cc1VoltT19 <= 6.8)
                    break;
                timeOut++;
                Thread.Sleep(1000);
            }
            SendNoticeToUIAndTxtFile("T19阶段：按下机械锁，检测CC1/CC2电压");
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc1VoltT19.ToString() } },
                "T19阶段", "检测点1，CC1电压(V)", "5.2", "6.8");
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc2VoltT19.ToString() } },
                "T19阶段", "检测点2，CC2电压(V)", "5.2", "6.8");
            #endregion

            #region T20阶段：按下机械锁，检测CC1电压

            SendNoticeToUIAndTxtFile("发送CC1、CC2断开！");

            double R4;
            double volt;
            Dictionary<int, List<bool>> dic = ControlEquipMent.BMS.BMSGetKState_DC(lstIDs, out R4, out volt, new string[] { "emtBMS_GB_DC" });
            List<bool> Ks = dic.First().Value;
            Ks[22] = false;
            Ks[23] = false;
            ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, Charger.NominalVoltage, 30, Ks.ToArray());

            SendNoticeToUIAndTxtFile("T20阶段：断开机械锁，检测CC1电压");

            SystemEvent.MessageInfo(true, "请按住枪锁不松开!");
            timeOut = 0;
            while (timeOut < 10)
            {
                cc1VoltT20 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                cc2VoltT20 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

                if (cc1VoltT20 >= 11.2 && cc1VoltT20 <= 12.8 && cc2VoltT20 >= 11.2 && cc2VoltT20 <= 12.8)
                    break;
                timeOut++;
                Thread.Sleep(1000);
            }
            SystemEvent.MessageInfo(false, "请按住枪锁不松开!");

            // 此处可添加机械锁断开控制 + CC1电压检测逻辑
            // 示例：读取当前CC1电压并验证
            cc1VoltT20 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
            cc2VoltT20 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

            bool isCC1T0Pass = CheckVoltageInRange(cc1VoltT20.ToString(), 11.2, 12.8);


            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc1VoltT20.ToString() } },
                "T20阶段", "检测点1,CC1电压(V)", "11.2", "12.8");
            SendNoticeToUIAndTxtFile($"T20阶段检测点1电压：{cc1VoltT20.ToString()}V，判定：{(isCC1T0Pass ? "合格" : "不合格")}");

            bool isCC2T0Pass = CheckVoltageInRange(cc2VoltT20.ToString(), 11.2, 12.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc2VoltT20.ToString() } },
                "T20阶段", "CC2电压(V)", "11.2", "12.8");
            SendNoticeToUIAndTxtFile($"T20阶段CC2电压：{cc2VoltT20.ToString()}V，判定：{(isCC2T0Pass ? "合格" : "不合格")}");

            SwStatus.Add(1, isCC1T0Pass);
            ProcessDataResults(testWorkParam.lstIDs, SwStatus.First().Value ? "断开" : "闭合", "机械锁(开关S)状态 断开", SwStatus, "T20阶段");

            #endregion

            SystemEvent.MessageInfo(true, "请松开枪锁!");
            Thread.Sleep(1000);

            while (timeOut < 100)
            {
                cc1VoltT21 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                if (cc1VoltT21 >=5.2 && cc1VoltT21 <= 6.8)
                {
                    break;
                }
                timeOut++;
                Thread.Sleep(200);
            }
            #region T21阶段：松开机械锁，检测CC1电压


            timeOut = 0;
            while (timeOut < 10)
            {
                cc1VoltT21 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                cc2VoltT21 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage;

                if (cc1VoltT21 >= 5.2 && cc1VoltT21 <= 6.8 && cc2VoltT21 >= 11.2 && cc2VoltT21 <= 12.8)
                    break;
                timeOut++;
                Thread.Sleep(1000);
            }
            SendNoticeToUIAndTxtFile("T21阶段：松开机械锁，检测CC1电压,检测CC2电压");

            bool isCC1T1Pass = CheckVoltageInRange(cc1VoltT21.ToString(), 5.2, 6.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc1VoltT21.ToString() } },
                "T21阶段", "CC1电压(V)", "5.2", "6.8");
            SendNoticeToUIAndTxtFile($"T1阶段CC1电压：{cc1VoltT21}V，判定：{(isCC1T1Pass ? "合格" : "不合格")}");

            bool isCC2T1Pass = CheckVoltageInRange(cc2VoltT21.ToString(), 11.2, 12.8);
            ProcessDataTmp(new Dictionary<int, string> { { testWorkParam.lstIDs[0], cc2VoltT21.ToString() } },
                "T21阶段", "CC2电压(V)", "11.2", "12.8");
            SendNoticeToUIAndTxtFile($"T1阶段CC2电压：{cc2VoltT21}V，判定：{(isCC2T1Pass ? "合格" : "不合格")}");

            SwStatus.Clear();
            SwStatus.Add(1, isCC1T1Pass);
            ProcessDataResults(testWorkParam.lstIDs, SwStatus.First().Value ? "闭合" : "断开 ", "机械锁(开关S)状态 闭合", SwStatus, "T21阶段");
            SystemEvent.MessageInfo(false, "请松开枪锁!");
            #endregion

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
        /// 等待辅源电压下降（≤1V）
        /// </summary>
        /// <param name="timeout">超时次数（每次100ms）</param>
        /// <returns>是否检测到辅源断开</returns>
        private bool WaitAPSDrop(int timeout)
        {
            double apsVoltage;
            while (timeout-- > 0)
            {
                apsVoltage = Math.Abs(AllEquipStateData.DicBMS_DC_StateData.First().Value.APSVoltage);
                if (apsVoltage <= 1)
                {
                    Thread.Sleep(6500); // 稳定等待
                    SendNoticeToUIAndTxtFile($"辅源断开，电压：{apsVoltage}V");
                    return true;
                }
                Thread.Sleep(100);
            }
            SendNoticeToUIAndTxtFile("辅源断开检测超时");
            return false;
        }
        /// <summary>
        /// 等待辅源电压稳定（11V）
        /// </summary>
        /// <param name="timeout">超时次数（每次100ms）</param>
        /// <returns>是否检测到辅源断开</returns>
        private bool WaitAPSUp(int timeout)
        {
            for (int i = 0; i < timeout; i++)
            {
               var apsVoltage = Math.Abs(AllEquipStateData.DicBMS_DC_StateData.First().Value.APSVoltage);
                if (apsVoltage >= 11)
                {
                    SendNoticeToUIAndTxtFile($"辅源稳定，电压：{apsVoltage}V");
                    return true;
                }
                Thread.Sleep(200);
            }
            SendNoticeToUIAndTxtFile("辅源断开检测超时");
            return false;

        }
        /// <summary>
        /// 读取录波板模拟通道数据
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="waveData">波形数据对象</param>
        /// <param name="channelName">通道名称</param>
        private void ReadWaveAnalogChannel(int channelNo, ref WaveData waveData, string channelName)
        {
            try
            {
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, channelNo, ref waveData, channelName);
                SendNoticeToUIAndTxtFile($"读取模拟通道{channelNo}({channelName})完成");
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"读取模拟通道{channelNo}失败：{ex.Message}");
            }
        }

     

        /// <summary>
        /// 读取录波板数字通道数据（单个通道）
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="subChannel">子通道号</param>
        /// <param name="waveData">波形数据对象</param>
        /// <param name="channelName">通道名称</param>
        private (WaveData waveData, EMessageType MessageName)  ReadWaveDigitalChannel(int channelNo, int subChannel, EMessageType channelName)
        {
            WaveData waveData = new WaveData();
            try
            {
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadDigitalChannelData(testWorkParam.lstIDs, channelNo, subChannel, ref waveData, channelName.ToString());
                //SendNoticeToUIAndTxtFile($"读取数字通道{channelNo}-{subChannel}({channelName})完成");
                return (waveData, channelName);
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile($"读取数字通道{channelNo}-{subChannel}失败：{ex.Message}");
                return (waveData, channelName);

            }
        }
        /*
        /// <summary>
        /// 录波时序数据分析（对齐原Test_GB_DC_2025A的T3-T18分析）
        /// </summary>
        /// <param name="waveAPSVoltage">辅源电压波形</param>
        /// <param name="waveOutputVoltage">输出电压波形</param>
        /// <param name="waveK1K2">K1K2开关波形</param>
        private void AnalyzeWaveTimingData(WaveData waveAPSVoltage, WaveData waveOutputVoltage, WaveData waveK1K2)
        {
            SendNoticeToUIAndTxtFile("开始分析录波时序数据");
            string sState = "充电连接控制过程";
            Dictionary<int, string> dicResult = new Dictionary<int, string>();
            double tmpTime = 0;

            #region 辅源首个6V上升沿时间
            SendNoticeToUIAndTxtFile("读取辅源首个6V上升沿时间点中...");
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveAPSVoltage, true, 6, ref tmpTime);
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "辅源首个6V上升沿时间点", "-", "-");
            #endregion

            #region CHM桩握手首个电平上升沿时间
            SendNoticeToUIAndTxtFile("读取CHM桩握手首个电平上升沿时间点中...");
            WaveData waveCHM = new WaveData();
            ReadWaveDigitalChannel(1, 1, ref waveCHM, "CHM");
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveCHM, true, 6, ref tmpTime);
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "CHM桩握手首个电平上升沿时间点", dicResult[1], "-");
            #endregion

            #region BHM车辆握手电压首个100V上升沿时间
            SendNoticeToUIAndTxtFile("读取BHM车辆握手电压首个电平100上升沿时间点中...");
            WaveData waveBHMMaxVoltage = new WaveData();
            ReadWaveDigitalChannel(2, 1, ref waveBHMMaxVoltage, "BHMMaxVoltage");
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveBHMMaxVoltage, true, 100, ref tmpTime);
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "BHM车辆握手电压首个电平100上升沿时间点", dicResult[1], "-");
            #endregion

            #region K1K2前端高压60V下降沿时间（泄放电压）
            SendNoticeToUIAndTxtFile("读取K1K2前端高压，首个60V下降沿时间点中...");
            DataAnalysis_WaveRecoder.GetDCSingleTime_M(waveOutputVoltage, false, 60, ref tmpTime, tmpTime);
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "首个泄放电压时间点(K1K2前端高压)", dicResult[1], "-");
            #endregion

            #region K1K2-sig开关状态时间点
            SendNoticeToUIAndTxtFile("读取K1K2-sig 首个6V上升沿时间点中...");
            double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(waveK1K2, 5);
            if (K1K2_Tmp > 2)
            {
                DataAnalysis_WaveRecoder.GetDCSingleTime(waveK1K2, false, 6, ref tmpTime);
            }
            else
            {
                DataAnalysis_WaveRecoder.GetDCSingleTime(waveK1K2, true, 6, ref tmpTime);
            }
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "K1K2绝缘检测ok后断开时间点", dicResult[1], "-");
            #endregion

            #region CRM充电机辨识时间点
            SendNoticeToUIAndTxtFile("读取CRM充电机辨识首个电平1上升沿时间点中...");
            WaveData waveCRMState = new WaveData();
            ReadWaveDigitalChannel(3, 1, ref waveCRMState, "CRMState");
            DataAnalysis_WaveRecoder.GetDCSingleTime(waveCRMState, true, 100, ref tmpTime);
            dicResult[1] = tmpTime.ToString();
            ProcessDataTmp(dicResult, sState, "CRM-AA 充电机辨识首个电平上升沿时间点", dicResult[1], "-");
            #endregion

            #region 其他关键时序（省略重复逻辑，保留原有实现）
            // 如需完整实现，可参考原有代码补充BCP/CML/BRO/CRO/BCL/CCS/CST等时序分析
            #endregion

            SendNoticeToUIAndTxtFile("录波时序数据分析完成");
        }
        */
        /// <summary>
        /// 验证电压是否在指定范围内
        /// </summary>
        /// <param name="voltStr">电压字符串</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>是否在范围内</returns>
        private bool CheckVoltageInRange(string voltStr, double min, double max)
        {
            if (!double.TryParse(voltStr, out double volt))
            {
                return false;
            }
            return volt >= min && volt <= max;
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
        #endregion

        /// <summary>
        /// 数据处理（空实现，保留原有框架）
        /// </summary>
        public override void ProcessData()
        {
            // 保留原有空实现，兼容基类
        }
        public enum EMessageType
        {
            CHM,
            CRM,
            CCS,
            CML,
            CRO,
            BRM
        }
    }

}