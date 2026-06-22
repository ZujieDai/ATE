using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 负载突降测试··
    /// </summary>
    public class GB_RT_DC_2025_LoadDump_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double _demandVoltage = 750;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double testCurrent = 20;

        public GB_RT_DC_2025_LoadDump_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {


            SendNoticeToUIAndTxtFile("设备初始化中...");


            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();

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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
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
                    if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
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
                                    //
                                    LstTrialData[i].ExtentData = "-|-|-|-|null";
                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    SetConditionValues();
                    // 握手
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage);
                    Thread.Sleep(200);

                    // 参数配置
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, _demandVoltage, 100);
                    Thread.Sleep(200);

                    // 充电需求
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, _demandVoltage, testCurrent, true, 390);

                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    // 启动录波
                    SendNoticeToUIAndTxtFile("录波板启动录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(testWorkParam.lstIDs, 200);

                    Thread.Sleep(1000);
                    WaitDCVoltage(lstIDs, _demandVoltage);

                    // 启动负载
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, _demandVoltage - 10, testCurrent, _demandVoltage - 10, testCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, testCurrent, 35);
                    Thread.Sleep(2000);
                    //模拟负载突降
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //等待20s动作，包括报文发送停止和数据刷新
                    WaitDCVoltage(lstIDs, 0,20);
                                        // 停止录波
                    SendNoticeToUIAndTxtFile("录波板停止录波...");
                    ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Stop(testWorkParam.lstIDs);

                    // 启动导引
                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    VerifyProtectionState();
                }

            }
            catch (Exception ex) 
            { Log.Log.LogException(ex); }

        }
        /// <summary>
        /// 获取BMS状态
        /// </summary>
        private BMS_DC_StateData GetBmsState()
        {
            return AllEquipStateData.DicBMS_DC_StateData
                .FirstOrDefault(x =>testWorkParam.lstIDs.Contains(x.Value.ChargerID))
                .Value;
        }
        /// <summary>
        /// 通用状态验证核心方法
        /// </summary>
        private void VerifyProtectionState()
        {
            // 读取电流波形
            var outputWave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 1, ref outputWave, "充电电压");
            // 读取电流波形
            outputWave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 2, ref outputWave, "充电电流");
            // 读取电流波形
            outputWave = new WaveData();
            ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 3, ref outputWave, "辅源电压");
            // 截图
            var screenImages = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);
            var bmsData = GetBmsState();
            if (bmsData == null) return;

            // 1. 充电状态验证
            var isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80 }
            };

            ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "暂停充电" : "充电中", "暂停充电", isCharging, $"充电状态");

            // 2. CC1电压记录
            ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "CC1电压", "CC1电压", "-", "-");

            var c1c2State = new Dictionary<int, bool>
            {
                { 1,  bmsData.ChargingVoltage < 80}
            };

            ProcessDataResults(testWorkParam.lstIDs, c1c2State[1] ? "断开" : "闭合", "断开", c1c2State, $"C1C2状态");

            // 4. S3/S4状态验证
            var s3s4State = new Dictionary<int, bool> { { 1, bmsData.APSVoltage < 8 } };

            ProcessDataResults(testWorkParam.lstIDs, s3s4State[1] ? "断开" : "闭合", "断开", s3s4State, $"S3S4状态");

            // 5. 通讯状态验证
            var commState = new Dictionary<int, bool> { { 1, bmsData.APSVoltage > 8 } };
            ProcessDataResults(testWorkParam.lstIDs, "正常", "正常", commState, $"通讯状态");

            // 解锁确认
            CountDownTimeInfo("请确认充电中充电枪插头可被解锁。\r\n(注:勾选上为可被解锁)", 60, 2);
            ProcessDataConnect("绝缘检测阶段前", "是否可解锁");
        }
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

        public override void ProcessData()
        {

        }

    }
}
