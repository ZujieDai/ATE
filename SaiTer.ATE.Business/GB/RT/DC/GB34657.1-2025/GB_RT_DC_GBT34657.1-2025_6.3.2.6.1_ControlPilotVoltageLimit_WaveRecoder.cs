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
    /// 控制导引电压限值测试
    /// </summary>
    public class GB_RT_DC_2025_ControlPilotVoltageLimit_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 3;

        public GB_RT_DC_2025_ControlPilotVoltageLimit_WaveRecoder(int type)
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
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    // 握手
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 750);
                    Thread.Sleep(200);

                    // 参数配置
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, 750, 3);
                    Thread.Sleep(200);

                    // 充电需求
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 750, 3, true, 390);

                    #region 充电前

                    #region 限值内测试

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    SetR4Value(3.65, 4.37);

                    //开始充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    // 等待刷卡
                    WaitForSwipeCard( 200);
                    WaitDCVoltage(testWorkParam.lstIDs, 750, 10);
                    var bmsData = GetBmsState();
                    if (bmsData == null) return;

                    // 1. 充电状态验证
                    var isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1]? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，限值内测试", "检测点1电压设定范围", "3.65", "4.37");

                    // 2. CC1电压记录
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，限值内测试", "CC1电压", "-", "-");


                    var c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 8 }
            };
                    string c1c2Text = c1c2State[1] ? "断开" : "闭合";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");



                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    #endregion


                    #region 超上限值测试
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    SetR4Value(4.8, 10);

                    // 等待刷卡
                    CountDownTimeInfo("充电前超上限 充电桩不能进行启动充电？（勾选为不能充电）", 999, 2);
                    ProcessDataConnect("充电前，超上限", "充电桩不能进行启动充电");
                    bmsData = GetBmsState();
                    if (bmsData == null) return;
                  
                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "不允许充电" : "允许充电", "不允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，超上限", "检测点1电压设定范围", "4.8", "999");

                    // 2. CC1电压记录
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，超上限", "CC1电压", "-", "-");


                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80 }
            };
                    c1c2Text = c1c2State[1] ? "断开" : "闭合";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "断开", c1c2State, $"C1C2状态");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);
                    ProcessDataConnect("应发出告警提示", "是否有告警提示");


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    #endregion


                    #region 超下限值测试
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    SetR4Value(0, 3.2);

                    // 等待刷卡
                    CountDownTimeInfo("充电前，超下限 充电桩不能进行启动充电？（勾选为不能充电）", 999, 2);
                    ProcessDataConnect("充电前，超下限", "充电桩不能进行启动充电");
                    bmsData = GetBmsState();
                    if (bmsData == null) return;

                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "不允许充电" : "允许充电", "不允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，超下限", "检测点1电压设定范围", "0", "3.2");

                    // 2. CC1电压记录
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，超下限", "CC1电压", "-", "-");


                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80 }
            };
                    c1c2Text = c1c2State[1] ? "断开" : "闭合";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "断开", c1c2State, $"C1C2状态");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);
                    ProcessDataConnect("应发出告警提示", "是否有告警提示");


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);
                    #endregion


                    #region 车端电阻最值测试(1030Ω)
                  
                    //充电前，检测点1电压小于【3.2 V】
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1030);

                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(200);
                    Thread.Sleep(2000);
                    bmsData = GetBmsState();
                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    // 2. CC1电压记录
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，车端电阻最值测试(1030Ω)", "检测点1电压、CC1电压", "3.65", "4.37");


                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80 }
            };
                    c1c2Text = c1c2State[1] ?  "闭合":"断开";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);


                    #endregion


                    #region 车端电阻最值测试(970Ω)

                    //充电前，检测点1电压小于【3.2 V】
                    //ManualConfirmationShow("请手动调整R4阻值，使R4的阻值为【970Ω Ω】,调整完毕后请点击确定按钮！！！");
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 970);


                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(200);
                    bmsData = GetBmsState();
                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电前，车端电阻最值测试(970Ω)", "检测点1电压、CC1电压", "3.65", "4.37");

                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80 }
            };
                    c1c2Text = c1c2State[1] ?   "闭合":"断开";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);
                    #endregion

                    #endregion

                    SetR4Value(3.65, 4.37);
                    Thread.Sleep(1000);

                    #region 充电中

                    #region 限值内测试
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(200);
                    Thread.Sleep(2000);

                    SetR4Value(3.65, 4.37);

                    bmsData = GetBmsState();
                    if (bmsData == null) return;

                    // 1. 充电状态验证
                     isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电中，限值内测试", "检测点1电压设定范围", "3.65", "4.37");


                     c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80 }
            };
                     c1c2Text = c1c2State[1] ? "闭合" : "断开";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");



                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    #endregion



                    #region 超上限值测试
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);


                    // 等待刷卡
                    WaitForSwipeCard(200);
                    Thread.Sleep(3000);

                    SetR4Value(4.8, 10);

                    WaitDCVoltage(testWorkParam.lstIDs, 0, 10);
                    bmsData = GetBmsState();
                    if (bmsData == null) return;

                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "停止充电" : "不允许充电", "停止充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电中，超上限", "检测点1电压设定范围", "3.8", "4.2");


                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80 }
            };
                    c1c2Text = c1c2State[1] ?  "断开":"闭合";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "断开", c1c2State, $"C1C2状态");



                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    #endregion

                    SetR4Value(3.65, 4.37);

                    #region 超下限值测试
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //// 启动录波
                    //SendNoticeToUIAndTxtFile("录波板启动录波...");
                    //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_Start(testWorkParam.lstIDs);


                    // 等待刷卡
                    WaitForSwipeCard(200);
                    Thread.Sleep(2000);

                    SetR4Value(4.8, 10);

                    WaitDCVoltage(testWorkParam.lstIDs, 0, 10);
                    bmsData = GetBmsState();
                    if (bmsData == null) return;

                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "停止充电" : "不允许充电", "停止充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电中，超下限", "检测点1电压设定范围", "4.8", "999");

                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage < 80 }
            };
                    c1c2Text = c1c2State[1] ? "断开" : "闭合";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "断开", c1c2State, $"C1C2状态");



                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(5000);

                    #endregion

                    #region 车端电阻最值测试(1030Ω)

                    SetR4Value(3.65, 4.37);
                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(200);

                    Thread.Sleep(2000);

                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1030);
                    Thread.Sleep(3000);
                    bmsData = GetBmsState();
                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电中，车端电阻最值测试(1030Ω)", "检测点1电压、CC1电压", "3.65", "4.37");



                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80 }
            };
                    c1c2Text = c1c2State[1] ? "闭合" : "断开";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    WaitDCVoltage(lstIDs, 0, 5);

                    #endregion


                    #region 车端电阻最值测试(970Ω)

                    // 启动导引
                    SendNoticeToUIAndTxtFile("开启导引中...");
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    // 等待刷卡
                    WaitForSwipeCard(200);
                    //充电前，检测点1电压小于【3.2 V】
                    //ManualConfirmationShow("请手动调整R4阻值，使R4的阻值为【1030 Ω】,调整完毕后请点击确定按钮！！！");
                    Thread.Sleep(2000);

                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1030);
                    Thread.Sleep(3000);
                    bmsData = GetBmsState();
                    // 1. 充电状态验证
                    isCharging = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80}
            };

                    ProcessDataResults(testWorkParam.lstIDs, isCharging[1] ? "允许充电" : "不允许充电", "允许充电", isCharging, $"充电状态");
                    //读取数据，判断结果
                    //检测点1电压测量值
                    ProcessDataTmp(new Dictionary<int, string> { { 1, bmsData.CC1Voltage.ToString() } }, "充电中，车端电阻最值测试(970Ω)", "检测点1电压、CC1电压", "3.65", "4.37");


                    c1c2State = new Dictionary<int, bool>
            {
                { 1, bmsData.ChargingVoltage > 80 }
            };
                    c1c2Text = c1c2State[1] ? "闭合" : "断开";
                    ProcessDataResults(testWorkParam.lstIDs, c1c2Text, "闭合", c1c2State, $"C1C2状态");


                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    WaitDCVoltage(lstIDs, 0, 5);
                    #endregion
                  

                    #endregion

                }

            }
            catch (Exception ex) 
            { Log.Log.LogException(ex); }

        }

        /// <summary>
        /// 等待刷卡
        /// </summary>
        private void WaitForSwipeCard( int timeoutSec)
        {
            SendNoticeToUIAndTxtFile("等待刷卡...");
            MessgaeInfo(true, "请刷卡充电!", true);

            int remainingTime = timeoutSec;
            while (remainingTime-- > 0)
            {
                var bmsState = GetBmsState();
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
        }

        /// <summary>
        /// 获取BMS状态
        /// </summary>
        private BMS_DC_StateData GetBmsState()
        {
            return AllEquipStateData.DicBMS_DC_StateData
                .FirstOrDefault(x => testWorkParam.lstIDs.Contains(x.Value.ChargerID))
                .Value;
        }

        /// <summary>
        /// 设置R4阻值
        /// </summary>
        /// <param name="vdown">cc1电压值下限</param>
        /// <param name="vup">cc1电压值上限</param>
        public void SetR4Value(double vdown, double vup)
        {
            //设置R4阻值
            double[] stdv = { 4.0, 4.9, 3.1 };//cc1电压标准值
            int[] stdR = { 1000, 2500, 400 };//R4对应的阻值
            int stdRV = 1000;

            for (int i = 0; i < stdv.Length; i++)
            {
                if (stdv[i] > vdown && stdv[i] <= vup)
                {
                    stdRV = stdR[i];
                    break;
                }
            }
            ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, Convert.ToDouble(stdRV));
            Thread.Sleep(300);

        }




        public override void ProcessData()
        {

        }

    }
}
