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
using System.Configuration;

namespace SaiTer.ATE.Business
{
    public class ErrorPayment_GB_AC : BusinessBase
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
        double DemandCurrent = 32;//测试电流
        int DecimalLength = 2;//BMS读取电能保留几位小数

        int GunID = 1;
        List<int> MylstIDs = new List<int>();
        string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();

        DateTime 起始时间;
        DateTime 结束时间;
        public ErrorPayment_GB_AC(int type)
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
            //电能表常数=100|工作误差校验圈数=4|充电桩等级=1|示值误差测试时间(分钟)=3|最小金额误差(圆)=0.01|时钟示值误差时间(s)=5|测试电流(A)=32.00|保留几位小数=2
            电表常数 = Convert.ToDouble(strParams[0].Split('=')[1]);
            工作误差校验圈数 = Convert.ToDouble(strParams[1].Split('=')[1]);
            充电桩等级 = Convert.ToDouble(strParams[2].Split('=')[1]);
            示值误差测试时间 = Convert.ToDouble(strParams[3].Split('=')[1]);
            最小金额误差 = Convert.ToDouble(strParams[4].Split('=')[1]);
            时钟示值误差时间 = Convert.ToDouble(strParams[5].Split('=')[1]);
            if (strParams.Length > 6)
            {
                DemandCurrent = (int)Convert.ToDouble(strParams[6].Split('=')[1]);
            }
            if (strParams.Length > 7)
            {
                DecimalLength = (int)Convert.ToDouble(strParams[7].Split('=')[1]);
            }
            string[] itemParams = TrialItem.ItemParams.Split('|');
            if (itemParams.Length > 0 && itemParams[0].Split('=').Length > 1)
            {
                GunID = (int)Convert.ToDouble(itemParams[0].Split('=')[1]);
            }
            MylstIDs.Add(GunID);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                Task task = null;
                if (Customer != null && Customer.Equals("HYQCP"))
                {
                    task = Task.Factory.StartNew(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            try
                            {
                                DataModel.EquipStateData.BMS_AC_StateData StateData = new DataModel.EquipStateData.BMS_AC_StateData();
                                StateData.ChargerID = MylstIDs.FirstOrDefault();
                                StateData.SystemState = "-1";
                                double result = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, EmChargerType.Charger_GB_AC);
                                StateData.ChargeKwh = result;
                                SystemEvent.SendMonitorMessage(StateData);
                                Thread.Sleep(5000);
                            }
                            catch (Exception ex) { Log.Log.LogException(ex); }
                        }
                        ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                        Thread.Sleep(1000);
                        ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                        Thread.Sleep(300);
                        double BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, EmChargerType.Charger_GB_AC);
                        if (BMSEnergy > 0 || BMSEnergy == -999)
                            ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                    }, token);
                }
                StartItemFlow();
                cts.Cancel();
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
                    //设置测试条件
                    SetConditionValues();

                    //MylstIDs.Clear();
                    //MylstIDs.Add(GunID);

                    ControlEquipMent.BMS.BMSGetTempRH(MylstIDs, out double Temp, out double RH);
                    d1 = new Dictionary<int, string>
                        {
                            { GunID, Temp.ToString() }
                        };
                    ProcessDataTmp(d1, "计量信息", "温度(℃)", "-", "-");
                    d1 = new Dictionary<int, string>
                        {
                            { GunID, RH.ToString() }
                        };
                    ProcessDataTmp(d1, "计量信息", "湿度(%)", "-", "-");

                    if (GunID != 1)
                    {
                        // 关闭1号枪导引
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                        // 提示
                        CountDownTimeInfo($"请将充电枪插入计量导引中，枪ID={GunID}！！！", 999, 2);
                        if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(GunID))
                        {
                            Dictionary<int, string> datas = new Dictionary<int, string>();
                            datas.Add(GunID, "不存在该枪号");
                            ProcessDataTmpThis(MylstIDs, datas, TrialItem.ItemName, "结果", "-", "-");
                            return;
                        }

                        ControlEquipMent.BMS.BMS_OFF(MylstIDs);
                        Thread.Sleep(200);
                        ControlEquipMent.BMS.BMS_ON(MylstIDs);

                        Thread.Sleep(200);
                        WaitSwipingCard(testWorkParam.lstIDs, 0);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        if (!CheckSwipingCard(MylstIDs))
                        {
                            return;
                        }
                    }

                    SendNoticeToUIAndTxtFile("设备发送电量清零、并启动计算...");
                    ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                    Thread.Sleep(500);
                    //多发一遍有时候启动不了
                    ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("读取初始标准电能...");
                    var chargerType = LstChargerInfo.Find(c => c.ChargerId == GunID).ChargerType;
                    double BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, chargerType);


                    while (true)
                    {
                        if (BMSEnergy < 0.01 && BMSEnergy != -999)
                        {
                            SendNoticeToUIAndTxtFile(string.Format("初始标准电能为{0}，开始检测流程", BMSEnergy.ToString("F6")));

                            break;
                        }
                        else
                        {
                            SendNoticeToUIAndTxtFile(string.Format("初始标准电能为{0}，未正确清零，尝试重新清零（如一直无法清零，请停止检测并向管理员反馈）", BMSEnergy.ToString()));
                            ControlEquipMent.BMS.BMSClearEnergy(MylstIDs);
                            Thread.Sleep(500);
                            BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, chargerType);
                        }
                    }



                    CountDownTimeInfo("请输入测试前电能(kWh)", 999, 3);
                    起始电量 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请输入单价(元/kWh)", 999, 3);
                    单价 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 6);
                    try
                    {
                        起始时间 = Convert.ToDateTime(InputData);
                    }
                    catch
                    {
                        CountDownTimeInfo("输入的时间格式不正确  \r\n  请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 6);
                        起始时间 = Convert.ToDateTime(InputData);
                    }
                    DateTime SystemTimeBegin = DateTime.Now.AddSeconds(-5);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetResisLoadVolCurrAndStart(MylstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                    Thread.Sleep(2000);
                    Stopwatch st = new Stopwatch();
                    if (AllEquipStateData.DicPowerAnalyzer_StateData != null && AllEquipStateData.DicPowerAnalyzer_StateData.Count > 0)
                    {
                        st.Start();
                        while (st.ElapsedMilliseconds / 1000 <= 30)
                        {
                            double CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[1].Channel4RMSCurrent;

                            if (CheckCurrent >= ResiLoadCurrent * 0.9 && CheckCurrent <= ResiLoadCurrent * 1.1)
                            {
                                break;
                            }
                            Thread.Sleep(1000);
                        }
                        st.Stop();
                    }
                    else
                        Thread.Sleep(10 * 1000);

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
                    SetLoadDCOFF(MylstIDs);
                    SendNoticeToUIAndTxtFile("读取标准电能中...");
                    BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, chargerType);
                    int Count = 1;
                    while (Count++ <= 5)
                    {
                        if (BMSEnergy == 0 || BMSEnergy == -999)
                        {
                            Thread.Sleep(1000);
                            SendNoticeToUIAndTxtFile(string.Format("第{0}次读取结束标准电能为{1},错误数据,重新读取数据", Count, BMSEnergy.ToString()));
                            BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(MylstIDs, chargerType);
                        }
                        else
                        {
                            BMSEnergy = Math.Round(BMSEnergy, DecimalLength);
                            SendNoticeToUIAndTxtFile(string.Format("读取结束标准电能为{0}", BMSEnergy.ToString()));

                            break;
                        }
                        Thread.Sleep(300);
                    }
                    CountDownTimeInfo("请输入测试后电能(kWh)", 999, 3);
                    结束电量 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请输入充电机显示付费金额(元)", 999, 3);
                    显示付费金额 = Convert.ToDouble(InputData);
                    CountDownTimeInfo("请按格式输入充电机当前时间(yyyy-MM-dd HH:mm:ss)\r\n重要：为减少误差，请尽量输入点击确认按钮时的时间", 999, 6);
                    结束时间 = Convert.ToDateTime(InputData);
                    DateTime SystemTimeEnd = DateTime.Now.AddSeconds(-5);

                    double ChargerEnergy = 结束电量 - 起始电量;
                    double DisPlayError = System.Math.Abs(ChargerEnergy - BMSEnergy) / BMSEnergy * 100;
                    double Payment = ChargerEnergy * 单价;//充电桩应有的付费金额
                    double PayMentError = System.Math.Abs(显示付费金额 - Payment);

                    TimeSpan ChargerTime = 结束时间 - 起始时间;
                    TimeSpan SystemTime = SystemTimeEnd - SystemTimeBegin;
                    double TimeError = System.Math.Abs(SystemTime.TotalMilliseconds / 1000 - ChargerTime.TotalMilliseconds / 1000);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(1, BMSEnergy.ToString($"F{DecimalLength}"));
                    ProcessDataTmp(dic, "充电电量", "导引电量(kWh)", "0", "999");
                    dic.Clear();
                    dic.Add(1, ChargerEnergy.ToString($"F{DecimalLength}"));
                    ProcessDataTmp(dic, "充电电量", "充电桩电量(kWh)", "0", "999");

                    dic.Clear();
                    dic.Add(1, DisPlayError.ToString("F2"));
                    ProcessDataTmp(dic, "充电电量", "示值误差(%)", "0", 充电桩等级.ToString());

                    dic.Clear();
                    dic.Add(1, PayMentError.ToString("F2"));
                    ProcessDataTmp(dic, "付费金额", "金额示值误差(圆)", "0", 最小金额误差.ToString());

                    dic.Clear();
                    dic.Add(1, TimeError.ToString("F2"));
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
