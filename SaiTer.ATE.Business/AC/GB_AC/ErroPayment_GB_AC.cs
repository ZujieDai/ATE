using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标交流  付费金额误差   包含示值误差、时钟误差
    /// </summary>
    internal class ErroPayment_GB_AC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        double 充电桩等级;//充电桩等级
        double 工作误差校验圈数;//工作误差校验圈数
        double 电表常数;//电表常数
        double 起始电量;
        double 结束电量;//kWh
        double 单价;//圆/kWh
        double 显示付费金额;
        double 最小金额误差;
        double 示值误差测试时间;//分钟
        double 时钟示值误差时间;//秒

        DateTime 起始时间;
        DateTime 结束时间;
        public ErroPayment_GB_AC(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //电能表常数=100|工作误差校验圈数=4|充电桩等级=1|示值误差测试时间(分钟)=3|最小金额误差(圆)=0.01|时钟示值误差时间(s)=5
            if (strParams.Length >= 6)
            {
                电表常数 = Convert.ToDouble(strParams[0].Split('=')[1]);
                工作误差校验圈数 = Convert.ToDouble(strParams[1].Split('=')[1]);
                充电桩等级 = Convert.ToDouble(strParams[2].Split('=')[1]);
                示值误差测试时间 = Convert.ToDouble(strParams[3].Split('=')[1]);
                最小金额误差 = Convert.ToDouble(strParams[4].Split('=')[1]);
                时钟示值误差时间 = Convert.ToDouble(strParams[5].Split('=')[1]);
                BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
                ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            }
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
                SetCPReresh();
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

                    // 功率分析仪输入、输出电流采样切换
                    var listState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    listState[8] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listState);
                    Thread.Sleep(500);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    CountDownTimeInfo("请输入测试前电能(kWh)", 999, 3);
                    起始电量 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请输入单价(元/kWh)", 999, 3);
                    单价 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    try
                    {
                        起始时间 = Convert.ToDateTime(InputData);
                    }
                    catch
                    {
                        CountDownTimeInfo("输入的时间格式不正确  \r\n  请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        起始时间 = Convert.ToDateTime(InputData);
                    }
                    DateTime SystemTimeBegin = DateTime.Now.AddSeconds(-5);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent);
                    Thread.Sleep(500);
                    var powerAnalyzerData = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]];
                    double BMSEnergy_Start = powerAnalyzerData.Channel1ElectricEnergy + powerAnalyzerData.Channel2ElectricEnergy + powerAnalyzerData.Channel3ElectricEnergy;
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    while (st.ElapsedMilliseconds / 1000 <= 30)
                    {
                        double CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                        if (CheckCurrent >= ResiLoadCurrent * 0.9 && CheckCurrent <= ResiLoadCurrent * 1.1)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    st.Stop();
                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    st.Reset();
                    st.Start();
                    SystemEvent.SendLogMessage("\r\n  ");
                    while (st.ElapsedMilliseconds < (Convert.ToInt32(示值误差测试时间 * 60) + 2) * 1000)
                    {
                        int t = (Convert.ToInt32(示值误差测试时间 * 60) + 2) - (int)st.ElapsedMilliseconds / 1000;
                        if (t % 20 == 0)
                        {
                            SystemEvent.SendLogMessage("正在充电中，剩余时间 " + t + "秒   \r\t  \r\t ");
                        }
                        Thread.Sleep(1000);
                    }
                    st.Stop();
                    SendNoticeToUIAndTxtFile("关闭负载");
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("读取标准电能中...");
                    double BMSEnergy_End = powerAnalyzerData.Channel1ElectricEnergy + powerAnalyzerData.Channel2ElectricEnergy + powerAnalyzerData.Channel3ElectricEnergy;
                    double BMSEnergy = BMSEnergy_End - BMSEnergy_Start;
                    //int Count = 1;
                    //while (Count++ <= 5)
                    //{
                    //    if (BMSEnergy == 0 || BMSEnergy == -999)
                    //    {
                    //        Thread.Sleep(1000);
                    //        SendNoticeToUIAndTxtFile(string.Format("第{0}次读取结束标准电能为{1},错误数据,重新读取数据", Count, BMSEnergy.ToString()));
                    //        BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);
                    //    }
                    //    else
                    //    {
                    SendNoticeToUIAndTxtFile(string.Format("读取结束标准电能为{0}", BMSEnergy.ToString()));

                    //        break;
                    //    }
                    //}
                    CountDownTimeInfo("请输入测试后电能(kWh)", 999, 3);
                    结束电量 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请输入充电机显示付费金额(元)", 999, 3);
                    显示付费金额 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    结束时间 = Convert.ToDateTime(InputData);
                    DateTime SystemTimeEnd = DateTime.Now.AddSeconds(-5);

                    double ChargerEnergy = 结束电量 - 起始电量;
                    double DisPlayError = System.Math.Abs(((ChargerEnergy - BMSEnergy) / BMSEnergy) * 100);
                    double Payment = ChargerEnergy * 单价;//充电桩应有的付费金额
                    double PayMentError = System.Math.Abs(显示付费金额 - Payment);

                    TimeSpan ChargerTime = 结束时间 - 起始时间;
                    TimeSpan SystemTime = SystemTimeEnd - SystemTimeBegin;
                    double TimeError = System.Math.Abs(SystemTime.TotalMilliseconds / 1000 - ChargerTime.TotalMilliseconds / 1000);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(LstChargerInfo[0].ChargerId, BMSEnergy.ToString());
                    ProcessDataTmp(dic, "充电电量", "导引电量(kWh)", "0", "999");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, ChargerEnergy.ToString("F4"));
                    ProcessDataTmp(dic, "充电电量", "充电桩电量(kWh)", "0", "999");

                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, DisPlayError.ToString("F2"));
                    ProcessDataTmp(dic, "充电电量", "示值误差(%)", "0", 充电桩等级.ToString());

                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, PayMentError.ToString("F2"));
                    ProcessDataTmp(dic, "付费金额", "金额示值误差(圆)", "0", 最小金额误差.ToString());

                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, TimeError.ToString("F2"));
                    ProcessDataTmp(dic, "时钟示值", "时钟示值误差(秒)", "0", 时钟示值误差时间.ToString());

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        public override void ProcessData()
        {

        }
    }
}
