using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using SaiTer.ATE.PortManage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 检测业务基类
    /// </summary>
    public abstract partial class BusinessBase
    {

        protected double MaxOutputPower = 160; //最大输出功率(KW)
        protected double MaxAllowChargeVoltage = 1000; //最高允许充电总电压(V)
        protected double MinAllowChargeVoltage = 200; //最低允许充电总电压(V)
        protected double MidAllowChargeVoltage = 600; //最高和最低加起来除以2
        protected double MaxAllowChargeCurrent = 250; //最高允许充电总电流(A)
        protected double StorageBatteryVoltage = 290;//车辆动力蓄电池当前电池电压(V)
        protected double ChargeVoltageMeasure = 290;// 充电电压测量值(V)
        protected double BatteryVoltage = 290;//电池电压(V)
        protected double RatedCurrent = 160;//额定电流 最大输出功率除以最高允许充电总电压
        protected double MidAllowChargeCurrent = 120;//额定电流 最大输出功率除以MidAllowChargeVoltage
        protected double RatedMinVoltage = 475;//恒功率最低电压点
        protected double RatedMidVoltage = 475;//恒功率中间电压
        protected double ModuleMinimumVoltage = 300;//电源模块恒功率最小电压(V)
        protected double CWHightVoltL = 500;//恒功率高电压段下限(V)
        protected double CWHightVoltH = 1000;//恒功率高电压段上限(V)
        protected double CWLowerVoltL = 300;//恒功率低电压段下限(V)
        protected double CWLowerVoltH = 500;//恒功率低电压段上限(V)
        protected double BatteryVoltage_EU = 390;//电池电压(V)


        public string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
        //惠州TB的直流测试系统，响河交流源性能问题导致投载240kW得先投160kW等待3s再投240
        public string Is240kWRestrict = ConfigurationManager.AppSettings["Is240kWRestrict"];
        public bool isAutoTest;
        protected int LoadWaitTime_s = Convert.ToInt32(CommAppConfigXMLOperation.GetValue("LoadWaitTime_s", "5"));


        /// <summary>
        /// 倒计时弹窗结果
        /// </summary>
        public bool CountDownResult { get; set; }
        /// <summary>
        /// 人工确认结果(枪位号，合格结论)
        /// </summary>
        public Dictionary<int, bool> DicManualVerifyResult { get; set; } = new Dictionary<int, bool>();
        /// <summary>
        /// 弹窗等待用户输入的数据
        /// </summary>
        public string InputData { get; set; }
        /// <summary>
        /// 所有设备的实时状态数据
        /// </summary>
        public AllEquipStateData EquipStateData = AllEquipStateData.GetInstance();
        /// <summary>
        /// 试验类型
        /// </summary>
        public int TrialType
        {
            get;
            set;
        }

        /// <summary>
        /// 设备操作对象
        /// </summary>
        public ControlsListManager ControlEquipMent
        {
            get;
            set;
        }
        /// <summary>
        /// 超时计时器
        /// </summary>
        public Stopwatch _StopWatch = new Stopwatch();
        /// <summary>
        /// 工作流程参数
        /// </summary>
        public TestWorkParam testWorkParam = new TestWorkParam();
        /// <summary>
        /// 试验结果集
        /// </summary>
        public List<TrialDataModel> LstTrialData
        {
            get;
            set;
        }
        /// <summary>
        /// 设备返回结果队列
        /// </summary>
        public Queue<StResultData> ResultData = new Queue<StResultData>();
        /// <summary>
        /// 对象锁
        /// </summary>
        private static object SLock = new object();

        private List<ChargerInfoModel> _LstChargerInfo;
        /// <summary>
        /// 充电枪信息集
        /// </summary>
        public List<ChargerInfoModel> LstChargerInfo
        {
            get { return _LstChargerInfo; }
            set
            {
                _LstChargerInfo = value;
                ParseChargerInfo(value);
            }
        }

        private List<PortBase> _LstPortInfo;
        /// <summary>
        /// 端口信息集
        /// </summary>
        public List<PortBase> LstPortInfo
        {
            get { return _LstPortInfo; }
            set
            {
                _LstPortInfo = value;
            }
        }
        private StTrialItem _TrialItem;
        /// <summary>
        /// 检测试验项
        /// </summary>
        public StTrialItem TrialItem
        {
            get { return _TrialItem; }
            set
            {
                _TrialItem = value;
                ParseSchemeInfo(value);
            }
        }

        /// <summary>
        /// 通道1探头变比
        /// </summary>
        public string Channel1 = "500";
        public string Channel2 = "200";
        public string Channel3 = "1";
        public string Channel4 = "500";
        /// <summary>
        /// 检测完一个测试项目后，是否需要刷新CP信号模拟拔枪再插枪
        /// </summary>
        public bool IsNeedRereshCP = false;
        /// <summary>
        /// 模拟插拔枪中间间隔时间
        /// </summary>
        public double RefreshCPWaitTime = 1;
        /// <summary>
        /// 检测完一个测试项目后，是否需要重新上电压
        /// </summary>
        public bool IsNeedRereshVoltage = false;
        /// <summary>
        /// 所有测试项全都有数据后是否自动保存数据到正式库
        /// </summary>
        public bool IsAutoSaveTrialData = false;
        /// <summary>
        /// 电阻负载单三相并机继电器序号（闭合此继电器切换到三相、断开切换到单相）  此值小于0代表不具备此功能
        /// </summary>
        public int RelayIndex = -999;

        #region-------------------------充电枪信息解析------------------------------
        /// <summary>
        /// 解析充电枪信息
        /// </summary>
        /// <param name="chargerInfo">充电枪信息集合</param>
        private void ParseChargerInfo(List<ChargerInfoModel> chargerInfo)
        {
            try
            {
                LstTrialData = new List<TrialDataModel>();
                MaxAllowChargeVoltage = chargerInfo[0].NominalVoltage;
                BatteryVoltage = MaxAllowChargeVoltage > 390 ? 390 : MaxAllowChargeVoltage - 10;
                MinAllowChargeVoltage = chargerInfo[0].MinAllowChargeVoltage;
                MaxAllowChargeCurrent = chargerInfo[0].MaxAllowChargeCurrent;
                MaxOutputPower = chargerInfo[0].MaxOutputPower;
                RatedCurrent = chargerInfo[0].NominalCurrent;
                CWHightVoltL = chargerInfo[0].CWHightVoltL;
                CWHightVoltH = chargerInfo[0].CWHightVoltH;
                CWLowerVoltL = chargerInfo[0].CWLowerVoltL;
                CWLowerVoltH = chargerInfo[0].CWLowerVoltH;
                MidAllowChargeVoltage = (MaxAllowChargeVoltage + MinAllowChargeVoltage) / 2; //最高和最低加起来除以2
                MidAllowChargeCurrent = Math.Round(MaxOutputPower * 1000 / MidAllowChargeVoltage, 2);   //最大输出功率除以MidAllowChargeVoltage
                RatedMinVoltage = Math.Round(MaxOutputPower * 1000 / MaxAllowChargeCurrent, 2);
                RatedMidVoltage = (MaxAllowChargeVoltage + RatedMinVoltage) / 2;
                //ModuleMinimumVoltage = Math.Round(MaxOutputPower * 1000 / MaxAllowChargeCurrent, 2);    //最大输出功率除以最大电流


                for (int i = 0; i < chargerInfo.Count; i++)
                {
                    TrialDataModel _TrialData = new TrialDataModel();
                    if (chargerInfo[i] != null)
                    {
                        _TrialData.ChargerId = chargerInfo[i].ChargerId;
                        _TrialData.BarCode = chargerInfo[i].BarCode;
                        _TrialData.IsCheck = chargerInfo[i].IsCheck;
                        _TrialData.ReCheckCount = chargerInfo[i].ReCheckCount;
                        _TrialData.RES1 = chargerInfo[i].RES1;
                        _TrialData.RES2 = chargerInfo[i].RES2;
                        _TrialData.TrialType = (EmTrialType)TrialType;
                        if (chargerInfo[i].IsCheck)
                        {

                            _TrialData.IsCheck = true;
                            _TrialData.TrialResult = EmTrialResult.Wait;
                        }
                        else
                        {
                            _TrialData.IsCheck = false;
                            _TrialData.TrialResult = chargerInfo[i].CheckResult;
                        }
                        LstTrialData.Add(_TrialData);

                        if (!testWorkParam.lstIDs.Contains(chargerInfo[i].ChargerId))
                        {
                            testWorkParam.lstIDs.Add(chargerInfo[i].ChargerId);
                        }

                    }
                    else
                    {
                        SendNoticeToUIAndTxtFile("充电枪信息集合中有为空的信息");
                    }
                }

                //自动化产线属性
                string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
                if (strAutoTest != null)
                {
                    isAutoTest = bool.Parse(strAutoTest);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion

        #region---------------------方案信息解析-------------------------
        /// <summary>
        /// 解析检测项目参数
        /// </summary>
        /// <param name="TrialItemInfo">方案</param>
        private void ParseSchemeInfo(StTrialItem TrialItemInfo)
        {
            try
            {
                if (TrialItemInfo.ItemParams != null)
                {

                }
                else
                {

                }

                if (TrialItemInfo.TrialMethod != null)
                {

                }
                else
                {

                }
                if (TrialItemInfo.ResultParams != null)
                {

                }
                else
                { }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        #endregion
        /// <summary>
        /// 是否继续检测
        /// </summary>
        public static bool isCheckContinue = true;
        /// <summary>
        /// 开始试验方法
        /// </summary>
        public abstract void ExecuteMethod();

        /// <summary>
        /// 设备初始化
        /// </summary> 
        public abstract void InitEquiMent();

        /// <summary>
        /// 参数初始化
        /// </summary>
        public abstract void InitializeParams();

        /// <summary>
        /// 处理数据
        /// </summary>
        public abstract void ProcessData();
        /// <summary>
        /// 4个枪的ID号，用于初始化设备
        /// </summary>
        public List<int> lstIDs = new List<int>() { 1, 2, 3, 4 };

        /// <summary>
        /// 测试数据序号
        /// </summary>
        public int iIndex = 1;

        /// <summary>
        /// 测试条件临时变量
        /// </summary>
        public Dictionary<int, string> d1 = new Dictionary<int, string>();
        public Dictionary<int, string> d2 = new Dictionary<int, string>();
        public Dictionary<int, string> d3 = new Dictionary<int, string>();
        public Dictionary<int, string> d4 = new Dictionary<int, string>();
        public Dictionary<int, string> d5 = new Dictionary<int, string>();

        /// <summary>
        /// 初始化变量（每个测试项开始时调用）
        /// </summary>
        public void Init()
        {
            iIndex = 1;
            d1.Clear();
            d2.Clear();
            d3.Clear();
            d4.Clear();
            d5.Clear();
        }

        /// <summary>
        /// 检定结果发送到UI
        /// </summary>
        /// <param name="TrialData"></param>
        public void SendTrialDataToUI(TrialDataModel TrialData)
        {
            SystemEvent.SendTrialResultToUI(TrialData);
            SystemEvent.SendDataMessageToUI(TrialData);
        }
        /// <summary>
        /// 保存试验结果
        /// </summary>
        public void SaveTrialResult()
        {
            try
            {
                iIndex = 1;
                List<TrialDataModel> tempTrial = new List<TrialDataModel>();
                bool isOK = false;

                byte TotalResult = 0;
                TotalResult = 0;
                for (int i = 0; i < LstTrialData.Count; i++)
                {
                    if (LstTrialData[i].IsCheck)
                    {
                        if (LstTrialData[i].TrialResult != EmTrialResult.Pass)
                        {
                            LstTrialData[i].TrialName = TrialItem.ItemName;
                            LstTrialData[i].TrialResult = EmTrialResult.Fail;
                        }
                        if (LstTrialData[i].PKID == 0)
                        {
                            LstTrialData[i].PKID = LstChargerInfo.Find(c => c.BarCode == LstTrialData[i].BarCode).PKID;
                        }
                        LstTrialData[i].SaveTime = System.DateTime.Now.ToString();
                        LstTrialData[i].SchemeID = TrialItem.SchemeID;
                        LstTrialData[i].SchemeName = TrialItem.SchemeName;
                        LstTrialData[i].Remarks = TrialItem.TrialOrder.ToString();
                        tempTrial.Add(LstTrialData[i]);
                        if (LstTrialData[i].TrialResult == EmTrialResult.Fail)
                        {
                            int j = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                            if (j >= 0)
                            {
                                LstChargerInfo[j].CheckResult = LstTrialData[i].TrialResult;
                            }
                        }
                        //SystemEvent.SendTrialResultToUI(LstTrialData[i]);
                        tempTrial.Find(s => s.ChargerId == LstTrialData[i].ChargerId).TrialCondition = LstTrialData[i].TrialCondition;
                        List<TrialDataModel> lstDataTmp = new List<TrialDataModel>();
                        isOK = TrialItemDataTmpManage.SelectTrialDataTmp(LstTrialData[i].BarCode, LstTrialData[i].TrialType, LstTrialData[i].TrialName, LstTrialData[i].ChargerId, out lstDataTmp);
                        int k = tempTrial.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                        if (isOK && lstDataTmp.Count > 0)
                        {
                            int index = lstDataTmp.FindIndex(s => s.TrialResult == EmTrialResult.Fail);
                            if (index >= 0) //这里先不控制信号灯，多个枪同时测试会造成灯闪烁
                            {

                                tempTrial[k].TrialFinalResult = EmTrialResult.Fail;
                                TotalResult += 1;

                              //  ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Red);
                            }
                            else
                            {
                                //if (lstDataTmp.All(s => s.TrialResult == EmTrialResult.NA))
                                //{

                                //    tempTrial[k].TrialFinalResult = EmTrialResult.NA;
                                //    ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Green);
                                //}
                                //else
                                //{
                                    tempTrial[k].TrialFinalResult = EmTrialResult.Pass;

                                 //   ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Green);
                                //}
                            }
                        }
                        else
                        {
                            tempTrial[k].TrialFinalResult = EmTrialResult.Fail;
                           // ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Red);
                        }
                        if (ControlEquipMent.ControlBoard != null)
                            Thread.Sleep(500);
                        ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Yellow);

                        //可能没有测试出ItemData的数据，异常结束导致只保存ItemResult，这时候要增加一条数据保证最终结果正常
                        if (lstDataTmp.Count == 0) 
                        {
                            ProcessDataResult(new List<int>() { LstTrialData[i].ChargerId }, "未测试出数据", "-", false);
                        }
                    }
                }
                if (TotalResult > 0)
                {
                    ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Red);
                    SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Fail);
                }
                else
                {
                    ControlEquipMent.ControlBoard?.SetLightColor(EmLightColor.Green);
                    SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Pass);
                }

                //一个检测项下的多个检测点数据已经保存进了分项数据表 ，这里只需要保存试验项总结论数据进临时库

                //数据保存到临时表中
                isOK = TrialItemResultTmpManage.SaveTrialFinalData(tempTrial);//这里保存数据入临时库

                if (!isOK && tempTrial.Count > 0)
                {
                    SendNoticeToUIAndTxtFile(tempTrial[0].TrialType.ToString() + "试验项总结论数据保存到数据库失败！");
                }


            }
            catch (Exception ex)
            {
                string AlarmReason = TrialItem.ItemName + ",试验数据保存本地异常，重做该实验";
                SendNoticeToUIAndTxtFile(AlarmReason);
                SendException(ex);
            }
        }

        /// <summary>
        /// 设置测试条件
        /// </summary>
        public void SetConditionValue(string sName, Dictionary<int, string> data)
        {
            try
            {
                for (int i = 0; i < LstTrialData.Count; i++)
                {
                    if (LstTrialData[i].IsCheck)
                    {
                        string conditionValue = data.ContainsKey(LstTrialData[i].ChargerId) ? data[LstTrialData[i].ChargerId] : data.First().Value;
                        if (LstTrialData[i].TrialCondition != "" && LstTrialData[i].TrialCondition != null)
                        {
                            LstTrialData[i].TrialCondition = LstTrialData[i].TrialCondition + "|" + LanguageManager.GetByKey(sName) + "=" + conditionValue;
                        }
                        else
                        {
                            LstTrialData[i].TrialCondition = LanguageManager.GetByKey(sName) + "=" + conditionValue;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string AlarmReason = TrialItem.ItemName + ",设置测试条件异常";
                SendNoticeToUIAndTxtFile(AlarmReason);
                SendException(ex);
            }
        }

        /// <summary>
        /// 设置测试条件
        /// </summary>
        public void SetConditionValues()
        {
            //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
            {
                //int key = testWorkParam.lstIDs[i];
                int key = testWorkParam.lstIDs.First();
                float volt = 0, freq = 0;
                if (AllEquipStateData.DicACSource_StateData != null && AllEquipStateData.DicACSource_StateData.Count > 0)
                {
                    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    {
                        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    }
                    volt = AllEquipStateData.DicACSource_StateData[key].Volt;
                    freq = AllEquipStateData.DicACSource_StateData[key].Freq;
                }
                else if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                {
                    volt = (float)AllEquipStateData.DicBMS_AC_StateData[key].PhaseA_Voltage;
                }
                else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    volt = (float)AllEquipStateData.DicBMS_DC_StateData[key].ChargingVoltage;
                }
                if (d1.ContainsKey(key))
                    d1[key] = volt.ToString();
                else
                    d1.Add(key, volt.ToString());
                if (d2.ContainsKey(key))
                    d2[key] = volt.ToString();
                else
                    d2.Add(key, freq.ToString());
            }
            SetConditionValue("供电电压(V)", d1);
            SetConditionValue("供电频率(Hz)", d2);
        }



        /// <summary>
        /// 保存检测项里面多个检测点的数据
        ///  一个检测点保存一次
        /// 有多少个检测点，就保存几条数据
        /// </summary>
        public void SaveTrialData(TrialDataModel TrialData)
        {
            try
            {
                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("HYQCP"))
                {
                    var charger = LstChargerInfo.FirstOrDefault();
                    TrialData.RES1 = charger?.RES1;
                }
                TrialItemDataTmpManage.SaveTrialData(TrialData);
            }
            catch (Exception ex) { SendException(ex); }
        }
        /// <summary>
        /// 打印日志到UI和TXT文件
        /// </summary>
        /// <param name="Message"></param>
        public void SendNoticeToUIAndTxtFile(string Message)
        {
            Log.Log.LogMessage(Message, "检测流程日志");
            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Message);
        }

        /// <summary>
        /// 打印异常日志到TXT文件
        /// </summary>
        /// <param name="ex"></param>
        public void SendException(Exception ex)
        {
            Log.Log.LogException(ex, "业务异常日志");
        }
        /// <summary>
        /// 发送当前试验结束
        /// </summary>
        public void SendMessageEndThisTrial()
        {

        }
        /// <summary>
        /// 倒计时提示
        /// </summary>
        /// <param name="info">提示信息</param>
        /// <param name="time">时间(S)</param>
        /// <param name="type">提示类型 0-纯倒计时提示信息。 1-倒计时等待选择  2-一般检测等人工确认 倒计时等待选择枪位结论  3-等待用户输入数据 4-等待刷卡(主要用于有可能刷卡无法上电的情况),5.只有提示信息无确认,6,和1一样只是隐藏了时间</param>
        /// <param name="tag">输入数据的默认值</param>
        public void CountDownTimeInfo(string info, int time, int type, string tag = "")
        {
            SystemEvent.SendCountDownTimer(info, time, type, tag);
        }
        /// <summary>
        /// 普通弹窗
        /// </summary>
        /// <param name="IsShow"></param>
        /// <param name="info"></param>
        ///  <param name="Confrom">确认按钮默认不显示</param>
        public void MessgaeInfo(bool IsShow, string info, bool Confrom = false)
        {
            SystemEvent.MessageInfo(IsShow, info, Confrom);
        }
        /// <summary>
        /// 弹窗等待刷卡
        /// </summary>
        /// <param name="lstIDs">需要刷卡的充电桩编号集合</param>
        /// <param name="type">//弹窗类型 0：等待刷卡  1：插枪检测    2:只检测刷卡（有PWM波） 3:欧标直流检测刷卡,4:国标直流只检测是否刷卡</param>
        public void WaitSwipingCard(List<int> lstIDs, int type)
        {
            SystemEvent.SendWaitSwipingCard(lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].ChargerType, type);
        }
        /// <summary>
        /// 开始启动注册事件
        /// </summary>
        public void StartEvent()
        {
            SystemEvent.EquipMentResultEvent += SystemEvent_EquipMentResultEvent;
            SystemEvent.SendCountDownTimerResultEvent += SystemEvent_SendCountDownTimerResultEvent;
            SystemEvent.SendManualVerifyResultEvent += SystemEvent_SendManualVerifyResultEvent;
            SystemEvent.SendInputDataEvent += SystemEvent_SendInputDataEvent;
        }

        private void SystemEvent_SendInputDataEvent(string value)
        {
            InputData = value;
        }

        private void SystemEvent_SendManualVerifyResultEvent(Dictionary<int, bool> dicReuslt)
        {
            DicManualVerifyResult = dicReuslt;
        }

        private void SystemEvent_SendCountDownTimerResultEvent(bool result)
        {
            CountDownResult = result;
        }

        /// <summary>
        /// 取消注册的事件
        /// </summary>
        public void StopEvent()
        {
            try
            {
                SystemEvent.EquipMentResultEvent -= SystemEvent_EquipMentResultEvent;
                SystemEvent.SendCountDownTimerResultEvent -= SystemEvent_SendCountDownTimerResultEvent;
                SystemEvent.SendManualVerifyResultEvent -= SystemEvent_SendManualVerifyResultEvent;
                SystemEvent.SendInputDataEvent -= SystemEvent_SendInputDataEvent;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        /// <summary>
        /// 接收设备返回数据方法
        /// </summary>
        /// <param name="st">返回的数据结构</param>
        public void SystemEvent_EquipMentResultEvent(StResultData st)
        {
            lock (SLock)
            {
                ResultData.Enqueue(st);
            }
        }


        /// <summary>
        /// 获取示波器光标之间的时间差
        /// </summary>
        /// <param name="dtmp"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetOSCTime(Dictionary<int, double[]> dtmp)
        {
            Dictionary<int, string> ds = new Dictionary<int, string>();
            try
            {
                foreach (var item in dtmp)
                {
                    if (item.Value != null)
                    {
                        ds.Add(item.Key, Math.Abs(item.Value[0]).ToString());
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return ds;
        }

        /// <summary>
        /// 获取示波器光标Y之间的电平差
        /// </summary>
        /// <param name="dtmp"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetOSCY(Dictionary<int, double[]> dtmp)
        {
            Dictionary<int, string> ds = new Dictionary<int, string>();
            try
            {
                foreach (var item in dtmp)
                {
                    if (item.Value != null)
                    {
                        ds.Add(item.Key, Math.Abs(item.Value[4]).ToString());
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return ds;
        }

        /// <summary>
        /// 保存测试数据
        /// </summary>
        /// <param name="datas">数据</param>
        /// <param name="sState">状态</param>
        /// <param name="sName">名称</param>
        /// <param name="minValue">下限</param>
        /// <param name="maxValue">上限</param>
        /// <param name="dImages">截图</param>
        public void ProcessDataTmp(Dictionary<int, string> datas, string sState, string sName, string minValue, string maxValue, Dictionary<int, string> dImages = null)
        {
            try
            {
                List<int> lstIDs = datas.Keys.ToList();
                foreach (var item in lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    bool bSx = false;//上限是否合格
                    bool bXx = false;//下限是否合格

                    double dData = 0;
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    double dSx = 0;//上限    
                    double dXx = 0;//下限
                    if (double.TryParse(maxValue, out dSx))
                    {
                        if (double.TryParse(datas[LstChargerInfo[i].ChargerId], out dData))
                        {
                            //dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                            if (dData <= dSx)
                            {
                                bSx = true;
                            }
                        }
                        else
                        {
                            //安规会出现>50000这种结果
                            if(datas[LstChargerInfo[i].ChargerId].IndexOf(">") > -1)
                            {
                                if (double.TryParse(datas[LstChargerInfo[i].ChargerId].Replace(">", ""), out dData))
                                {
                                    dData++;
                                    if (dData <= dSx)
                                    {
                                        bSx = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (maxValue.Trim() == "*" || maxValue.Trim() == "-")
                    {
                        bSx = true;
                    }
                    if (double.TryParse(minValue, out dXx))
                    {
                        if (double.TryParse(datas[LstChargerInfo[i].ChargerId], out dData))
                        {
                            //dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                            if (dData >= dXx)
                            {
                                bXx = true;
                            }
                        }
                        else
                        {
                            //安规会出现>50000这种结果
                            if (datas[LstChargerInfo[i].ChargerId].IndexOf(">") > -1)
                            {
                                if (double.TryParse(datas[LstChargerInfo[i].ChargerId].Replace(">", ""), out dData))
                                {
                                    dData++;
                                    if (dData >= dXx)
                                    {
                                        bXx = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (minValue.Trim() == "*" || minValue.Trim() == "-")//星号可以不判断直接合格
                    {
                        bXx = true;
                    }
                    if (minValue.Trim().Equals("-") && maxValue.Trim().Equals("-"))
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.NA;
                    }
                    else
                    {
                        if (bSx && bXx)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                    }

                    string data = datas[LstChargerInfo[i].ChargerId];
                    //if (Customer != null && Customer.Equals("HYQCP") && double.TryParse(datas[LstChargerInfo[i].ChargerId], out double measure) && measure != 0)
                    //    data = measure.ToString("F2");
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果
                    LstTrialData[i].ExtentData = sState
                        + "|" + sName
                        + "|" + minValue
                        + "|" + maxValue
                        + "|" + data
                        + "|" + sbtmp.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    if (!sState.Contains("**"))//带特定符号的数据不保存
                    {
                        SaveTrialData(LstTrialData[k]);
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }

        public void ProcessDataTmpThis(List<int> lstIDs, Dictionary<int, string> datas, string sState, string sName, string minValue, string maxValue, Dictionary<int, string> dImages = null)
        {
            try
            {
                foreach (var item in lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    bool bSx = false;//上限是否合格
                    bool bXx = false;//下限是否合格

                    double dData = 0;
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    double dSx = 0;//上限    
                    double dXx = 0;//下限
                    if (double.TryParse(maxValue, out dSx))
                    {
                        dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                        if (dData <= dSx)
                        {
                            bSx = true;
                        }
                    }
                    else if (maxValue.Trim() == "*" || maxValue.Trim() == "-")
                    {
                        bSx = true;
                    }
                    if (double.TryParse(minValue, out dXx))
                    {
                        dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                        if (dData >= dXx)
                        {
                            bXx = true;
                        }
                    }
                    else if (minValue.Trim() == "*" || minValue.Trim() == "-")//星号可以不判断直接合格
                    {
                        bXx = true;
                    }
                    if (bSx && bXx)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果
                    LstTrialData[i].ExtentData = sState
                        + "|" + sName
                        + "|" + minValue
                        + "|" + maxValue
                        + "|" + datas[LstChargerInfo[i].ChargerId].ToString()
                        + "|" + sbtmp.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    if (!sState.Contains("**"))//带特定符号的数据不保存
                    {
                        SaveTrialData(LstTrialData[k]);
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            iIndex++;
        }

        #region =====================检测插枪、等待刷卡上电==========================
        /// <summary>
        /// 交流充电桩插枪检测
        /// </summary>
        /// <param name="lstIDs">需要检测的枪位ID集合</param>
        public bool CheckChargerIn(List<int> lstIDs)
        {
            if (!isAutoTest)
                WaitSwipingCard(testWorkParam.lstIDs, 1);
            else
                //等待充电桩开机
                Thread.Sleep(2000);
            for (int i = 0; i < lstIDs.Count; i++)
            {
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC || LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[i]].ConnectState != "已连接")
                    {
                        SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪插枪状态，该枪停止检测");
                        lstIDs.Remove(lstIDs[i]);
                    }
                }
            }
            if (lstIDs.Count <= 0)
            {
                SendNoticeToUIAndTxtFile("所有枪位都未检测到插枪状态，测试结束！");
                return false;
            }
            return true;
        }


        public bool CheckSwipingCard(List<int> lstIDs, double BMSVolt = 0, double BMSCurrent = 0, double MaxVolt = 0, bool type = true)
        {
            bool result = false;
            SendNoticeToUIAndTxtFile("检测刷卡上电状态");
            try
            {
                ControlEquipMent.ACSource?.ACSource_ON(lstIDs); //??
                BMSVolt = BMSVolt == 0 ? LstChargerInfo[0].NominalVoltage : BMSVolt;
                BMSCurrent = BMSCurrent == 0 ? LstChargerInfo[0].MaxAllowChargeCurrent : BMSCurrent;
                MaxVolt = MaxVolt == 0 ? LstChargerInfo[0].NominalVoltage : MaxVolt;
                switch (LstChargerInfo[0].ChargerType)
                {
                    case EmChargerType.Charger_GB_AC:
                    case EmChargerType.Charger_EUR_AC:
                        CheckSwiping_AC(ref lstIDs);
                        break;
                    case EmChargerType.Charger_USA_AC:
                        CheckSwiping_USA_AC(ref lstIDs);
                        break;
                    case EmChargerType.Charger_GB_DC:
                        CheckSwiping_GB_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                    case EmChargerType.Charger_EUR_DC:
                    case EmChargerType.Charger_USA_DC:
                        CheckSwiping_EUR_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                    case EmChargerType.Charger_JP_DC:
                        CheckSwiping_JP_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                }

                //有至少一个桩正常刷卡起桩了 
                if (lstIDs.Count > 0)
                {
                    result = true;
                }
                if(!result)
                {
                    //ProcessDataResult(testWorkParam.lstIDs, "-", "无法启动充电", false, $"设置BMS电压{BMSVolt}V,电流{BMSCurrent}A");
                    d1 = new Dictionary<int, string>();
                    var dicResult = new Dictionary<int, EmTrialResult>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, "-");
                        dicResult.Add(item, EmTrialResult.Fail);
                    }
                    ProcessDataResults(testWorkParam.lstIDs, d1, "无法启动充电", dicResult, $"设置BMS电压{BMSVolt}V,电流{BMSCurrent}A");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }

        /// <summary>
        /// 刷卡检测（国欧美导引分开）
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="BMSVolt"></param>
        /// <param name="BMSCurrent"></param>
        /// <param name="MaxVolt"></param>
        /// <returns></returns>
        public bool CheckSwipingCard_DC_SinglePart(List<int> lstIDs, double BMSVolt = 0, double BMSCurrent = 0, double MaxVolt = 0, bool type = true)
        {
            bool result = false;
            SendNoticeToUIAndTxtFile("检测刷卡上电状态");
            try
            {
                ControlEquipMent.ACSource?.ACSource_ON(lstIDs);
                BMSVolt = BMSVolt == 0 ? LstChargerInfo[0].NominalVoltage : BMSVolt;
                BMSCurrent = BMSCurrent == 0 ? LstChargerInfo[0].MaxAllowChargeCurrent : BMSCurrent;
                MaxVolt = MaxVolt == 0 ? LstChargerInfo[0].NominalVoltage : MaxVolt;
                switch (LstChargerInfo[0].ChargerType)
                {
                    case EmChargerType.Charger_GB_AC:
                    case EmChargerType.Charger_EUR_AC:
                    case EmChargerType.Charger_USA_AC:
                        CheckSwiping_AC(ref lstIDs);
                        break;
                    case EmChargerType.Charger_GB_DC:
                        CheckSwiping_GB_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                    case EmChargerType.Charger_EUR_DC:
                        CheckSwiping_EUR_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                    case EmChargerType.Charger_USA_DC:
                        CheckSwiping_USA_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt);
                        break;
                    case EmChargerType.Charger_JP_DC:
                        CheckSwiping_JP_DC(ref lstIDs, BMSVolt, BMSCurrent, MaxVolt, type);
                        break;
                }

                //有至少一个桩正常刷卡起桩了 
                if (lstIDs.Count > 0)
                {
                    result = true;
                }
                if (!result)
                {
                    //ProcessDataResult(lstIDs, "-", "无法启动充电", false, $"设置BMS电压{LstChargerInfo[0].NominalVoltage}V,电流250A");
                    ProcessDataResult(lstIDs, "-", "无法启动充电", false, "-");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }
        /// <summary>
        /// 交流桩检测刷卡
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        private void CheckSwiping_AC(ref List<int> lstIDs)
        {
            //该逻辑当前仅用于HY，后续可改为通用
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            Task task = null;
            if (Customer != null && Customer.Equals("HYQCP"))
            {
                task = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(30 * 1000);
                    //如果30s后仍没有启动充电则模拟插拔枪并闭合开关S
                    if (!token.IsCancellationRequested)
                    {
                        SetCPReresh();
                        Thread.Sleep(100);
                        //模拟CP恢复，S2断开
                        var Ks = GetKStatus16_Charging();
                        ControlEquipMent.BMS.BMS_SetKState(this.lstIDs, Ks);
                    }
                }, token);
            }

            string BMSInfo = AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].SystemState;
            if (!BMSInfo.Contains("充电中"))
            {
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(200);
            }
            //可能因为丢帧导致判断充电状态失误，改为无论如何都启动一下
            ControlEquipMent.BMS.BMS_ON(lstIDs);

            //检测是否充电
            WaitSwipingCard(testWorkParam.lstIDs, 0);
            //取消重启刷卡流程的逻辑
            cts.Cancel();

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicBMS_AC_StateData[lstIDs[i]].PhaseA_Voltage;
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }

        }

        /// <summary>
        /// 美标交流桩检测刷卡（必须在状态B1刷卡）
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        private void CheckSwiping_USA_AC(ref List<int> lstIDs)
        {
            string BMSInfo = AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].SystemState;
            if (!BMSInfo.Contains("充电中"))
            {
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }

            //检测是否发波，美标枪没有C1状态
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            //B2状态闭合S2开关并等待电压稳定
            ControlEquipMent.BMS.BMS_ON(lstIDs);
            Thread.Sleep(6000);
            ////检测是否充电
            //WaitSwipingCard(testWorkParam.lstIDs, 0);     //可能会导致PWM发波了但是电压没输出然后测试异常

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicBMS_AC_StateData[lstIDs[i]].PhaseA_Voltage;
                if (Voltage < 50)
                {
                    Thread.Sleep(3000);
                    Voltage = AllEquipStateData.DicBMS_AC_StateData[lstIDs[i]].PhaseA_Voltage;
                    if (Voltage < 50)
                    {
                        SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                        lstIDs.Remove(lstIDs[i]);
                    }
                }
            }

        }

        private void CheckSwiping_GB_DC(ref List<int> lstIDs, double BMSVolt, double BMSCurrent, double MaxVolt, bool type)
        {
            if (lstIDs.Count < 1)
                return;
            Thread.Sleep(1000);//重要延时，勿删     防止上一个测试项目结束后,导引状态改变有延迟     

            //群充可能是一个终端一个条码两把枪
            //string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
            //if (strIsGroupC != null)
            //{
            //    bool isGroupCharger = Convert.ToBoolean(strIsGroupC);
            //    if (isGroupCharger)
            //    {
            //        for (int i = 0; i < LstChargerInfo.Count; i++)
            //        {
            //            if (LstChargerInfo[i].RES2 == "1")
            //            {
            //                lstIDs.Add(lstIDs.Max() + 1);
            //            }
            //        }
            //    }
            //}
            if (BMSVolt < 390 && !(Customer != null && Customer.Contains("DH")))
                BatteryVoltage = BMSVolt - 10;
            else
                BatteryVoltage = 390;
            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstIDs[0]].ChargingState;
            if (!BMSInfo.Contains("充电中"))
            {
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, MaxVolt);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, BatteryVoltage);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BatteryVoltage + 10, MaxVolt, MaxAllowChargeCurrent);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BMSVolt, BMSCurrent, type, BMSVolt);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(lstIDs);
                Thread.Sleep(1000);
            }
            else
            {
                ControlEquipMent.BMS.SetParameter(lstIDs, BMSVolt, BMSCurrent, type, BMSVolt);
            }
            if (IsNeedRereshCP)
            {
                SendNoticeToUIAndTxtFile("发送控制导引断线，模拟拔枪 ");
                foreach (int chargerId in lstIDs)
                {
                    if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                    {
                        BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                        if ("等待辅源".Contains(BMSInfo))
                        {
                            var Ks = GetKStatus16_Charging_DC();
                            Ks[22] = false;
                            ControlEquipMent.BMS.BMSSetKState_DC(new List<int>() { chargerId }, 1000, BatteryVoltage, Ks.ToArray());
                        }
                    }
                }
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile("恢复控制导引连接，模拟插枪 ");
                foreach (int chargerId in lstIDs)
                {
                    if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                    {
                        BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                        if ("等待辅源".Contains(BMSInfo))
                        {
                            var Ks = GetKStatus16_Charging_DC();
                            ControlEquipMent.BMS.BMSSetKState_DC(new List<int>() { chargerId }, 1000, BatteryVoltage, Ks.ToArray());
                        }
                    }
                }
            }
            //检测是否充电
            //WaitSwipingCard(lstIDs, 0);
            SystemEvent.SendWaitSwipingCard(lstIDs, BMSVolt, LstChargerInfo[0].ChargerType, 0);
            for (int i = lstIDs.Count - 1; i >= 0; i--)
            {
                double Voltage = AllEquipStateData.DicBMS_DC_StateData[lstIDs[i]].ChargingVoltage;
                if (Voltage < MinAllowChargeVoltage - 20)
                {
                    Thread.Sleep(2000);
                    Voltage = AllEquipStateData.DicBMS_DC_StateData[lstIDs[i]].ChargingVoltage;
                    if (Voltage < MinAllowChargeVoltage - 20)
                    {
                        SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                        ProcessDataResult(new List<int>() { lstIDs[i] }, "-", "无法启动充电", false);
                        lstIDs.Remove(lstIDs[i]);
                    }
                }
            }
        }

        public void ChangeKS_EU_DC(List<int> lstKS, out bool[] Ks, out int DCPlus, out int DCMinus)
        {
            Ks = new bool[24];
            Ks[0] = lstKS[0] == 1;//DC+DC-控制
            Ks[1] = lstKS[1] == 1;//CC信号控制
            Ks[2] = lstKS[2] == 1;//CP信号控制
            Ks[3] = lstKS[3] == 1;//转换板电源控制
            Ks[4] = lstKS[4] == 1;//PE控制
            Ks[5] = lstKS[5] == 1;//CP接120欧对地控制
            Ks[6] = lstKS[6] == 1;//电子锁控制
            Ks[14] = lstKS[7] == 1;//输出过压
            Ks[15] = lstKS[8] == 1;//停止报文
            Ks[22] = lstKS[9] == 1;//CP信号断开
            Ks[23] = lstKS[10] == 1;//CP二极管短接
            DCPlus = lstKS[11];//DC+绝缘阻值档位
            DCMinus = lstKS[12];//DC-绝缘阻值档位
        }

        public void CheckSwiping_EUR_DC(ref List<int> lstIDs, double BMS_U, double BMS_I, double MaxU, bool type)
        {
            //该逻辑当前仅用于DH，后续可改为通用
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            Task task = null;
            if (Customer != null && Customer.Equals("DH"))
            {
                task = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(45 * 1000);
                    //如果30s后仍没有启动充电则模拟插拔枪并闭合开关S
                    if (!token.IsCancellationRequested)
                    {
                        string BMSState = AllEquipStateData.DicBMS_EU_DC_StateData.First().Value.SystemState;
                        if (ChangeBMSChargeStatus_EU_DC(BMSState) > 3)
                        {
                            Thread.Sleep(60 * 1000); 
                            if (!token.IsCancellationRequested)
                            {
                                SetCPRersh_EUDC();
                                Thread.Sleep(100);
                                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                            }
                            return;
                        }
                        SetCPRersh_EUDC();
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }, token);
            }

            Thread.Sleep(1000);//重要延时，勿删
            string BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState;
            var lstKS = ControlEquipMent.BMS.BMSGetKState_EU_DC(lstIDs, out double batteryVolt).First().Value;
            if(lstKS == null || lstKS.Count < 13)
                lstKS = ControlEquipMent.BMS.BMSGetKState_EU_DC(lstIDs, out batteryVolt).First().Value;
            if (!BMSInfo.Contains("CurrentDemandReq") && !BMSInfo.Contains("CurrentDemandRes"))
            {
                ChangeKS_EU_DC(lstKS, out bool[] Ks, out int DCPlus, out int DCMinus);
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                if (lstKS == null || lstKS.Count < 13)
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                else
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), lstKS[11], lstKS[12], "0");
                Thread.Sleep(200);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs,15, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].MaxAllowChargeCurrent);
                //Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, 390, MaxU, MaxAllowChargeCurrent);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BMS_U, BMS_I, type, 390);
                Thread.Sleep(200);

            }

            BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState;

            if (!BMSInfo.Contains("充电中") && !BMSInfo.Contains("CurrentDemandReq") && !BMSInfo.Contains("CurrentDemandRes"))
            {
                if (AllEquipStateData.DicBMS_EU_DC_StateData != null && AllEquipStateData.DicBMS_EU_DC_StateData.Count > 0)
                {
                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    if (lstKS == null || lstKS.Count < 13)
                        ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                    else
                        ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), lstKS[11], lstKS[12], "0");
                }
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(5000);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                //Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, 390, MaxU, MaxAllowChargeCurrent);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BMS_U, BMS_I, type, 390);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(lstIDs);
            }
            else
            {
                ControlEquipMent.BMS.SetParameter(lstIDs, BMS_U, BMS_I, type, 390);

            }

            //检测是否充电
            //WaitSwipingCard(testWorkParam.lstIDs, 0);
            SystemEvent.SendWaitSwipingCard(lstIDs, BMS_U, LstChargerInfo[0].ChargerType, 0);
            //取消重启刷卡流程的逻辑
            cts.Cancel();

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[i]].ChargingVoltage;
                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                    Voltage = AllEquipStateData.DicBMS_EU_DC_StateData[2].ChargingVoltage;
                if (Voltage < MinAllowChargeVoltage - 20)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }
        }

        public void CheckSwiping_USA_DC(ref List<int> lstIDs, double BMS_U, double BMS_I, double MaxU)
        {
            Thread.Sleep(1000);//重要延时，勿删
            string BMSInfo = AllEquipStateData.DicBMS_USA_DC_StateData[lstIDs[0]].SystemState;
            if (!BMSInfo.Contains("CurrentDemandReq") && !BMSInfo.Contains("CurrentDemandRes"))
            {
                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                Thread.Sleep(200);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs,15, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].MaxAllowChargeCurrent);
                //Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, 390, MaxU, MaxAllowChargeCurrent);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(lstIDs, BMS_U, BMS_I, true, BMS_U);
                Thread.Sleep(200);

            }

            BMSInfo = AllEquipStateData.DicBMS_USA_DC_StateData[lstIDs[0]].SystemState;

            if (!BMSInfo.Contains("充电中") && !BMSInfo.Contains("CurrentDemandReq") && !BMSInfo.Contains("CurrentDemandRes"))
            {
                if (AllEquipStateData.DicBMS_USA_DC_StateData != null && AllEquipStateData.DicBMS_USA_DC_StateData.Count > 0)
                {
                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                }
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(200);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                //Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxU, MaxAllowChargeCurrent);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMS_U, BMS_I, true, BMS_U);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(lstIDs);
            }
            else
            {
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMS_U, BMS_I, true, BMS_U);

            }

            //检测是否充电
            //WaitSwipingCard(testWorkParam.lstIDs, 0);
            SystemEvent.SendWaitSwipingCard(lstIDs, BMS_U, LstChargerInfo[0].ChargerType, 0);

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicBMS_USA_DC_StateData[lstIDs[i]].ChargingVoltage;
                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                    Voltage = AllEquipStateData.DicBMS_USA_DC_StateData[2].ChargingVoltage;
                if (Voltage < MinAllowChargeVoltage - 20)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }
        }

        private void CheckSwiping_JP_DC(ref List<int> lstIDs, double BMSVolt, double BMSCurrent, double MaxVolt, bool type)
        {
            if (lstIDs.Count < 1)
                return;
            Thread.Sleep(1000);//重要延时，勿删     防止上一个测试项目结束后,导引状态改变有延迟     

            string BMSInfo = AllEquipStateData.DicBMS_JP_DC_StateData[lstIDs[0]].SystemState;
            if (!BMSInfo.Contains("正在充电"))
            {
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, 390);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMSSetParameter_JP_DC(lstIDs, BMSVolt, BMSCurrent, MaxVolt);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(lstIDs);
                Thread.Sleep(1000);
            }
            else
            {
                ControlEquipMent.BMS.BMSSetParameter_JP_DC(lstIDs, BMSVolt, BMSCurrent, MaxVolt);
            }
            if (IsNeedRereshCP)
            {
                SendNoticeToUIAndTxtFile("发送控制导引断线，模拟拔枪 ");
                foreach (int chargerId in lstIDs)
                {
                    if (AllEquipStateData.DicBMS_JP_DC_StateData.ContainsKey(chargerId))
                    {
                        ControlEquipMent.BMS.BMS_DC_SetControl(new List<int>() { chargerId }, 0x8A, false, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                    }
                }
                Thread.Sleep(3000);
                SendNoticeToUIAndTxtFile("恢复控制导引连接，模拟插枪 ");
                foreach (int chargerId in lstIDs)
                {
                    if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                    {
                        ControlEquipMent.BMS.BMS_DC_SetControl(new List<int>() { chargerId }, 0x8A, true, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                        ControlEquipMent.BMS.BMS_DC_SetControl(new List<int>() { chargerId }, 0x86, true, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                        byte[] DCBMSBitS = new byte[4];
                        DCBMSBitS[2] = 1;
                        DCBMSBitS[3] = 1;
                        ControlEquipMent.BMS.BMSSetKState_DC(new List<int>() { chargerId }, 0x87, DCBMSBitS, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                        ControlEquipMent.BMS.BMS_DC_SetControl(new List<int>() { chargerId }, 0x88, true, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                        ControlEquipMent.BMS.BMS_DC_SetControl(new List<int>() { chargerId }, 0x8A, true, new string[] { "emtBMS_JP_DC" });
                        Thread.Sleep(300);
                    }
                }
            }
            //检测是否充电
            //WaitSwipingCard(lstIDs, 0);
            SystemEvent.SendWaitSwipingCard(lstIDs, BMSVolt, LstChargerInfo[0].ChargerType, 0);
            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicBMS_JP_DC_StateData[lstIDs[i]].ChargingVoltage;
                if (Voltage < MinAllowChargeVoltage - 20)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }
        }


        //
        public bool CheckCharging_NTGX_CN(List<int> lstIDs)
        {
            bool result = false;
            SendNoticeToUIAndTxtFile("检测储能充电桩充电状态");
            try
            {
                //ControlEquipMent.ACSource?.ACSource_ON(lstIDs);
                Check_NTGX_Charging(ref lstIDs);

                //有至少一个桩正常刷卡起桩了 
                if (lstIDs.Count > 0)
                {
                    result = true;
                }
                if (!result)
                {
                    ProcessDataResult(lstIDs, "-", "无法启动充电", false, "-");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }


        private void Check_NTGX_Charging(ref List<int> lstIDs)
        {
            if (lstIDs.Count < 1)
                return;
            Thread.Sleep(1000);//重要延时，勿删     防止上一个测试项目结束后,导引状态改变有延迟     

            string ChargerInfo = AllEquipStateData.DicChargerNTGXCtrl_StateData[lstIDs[0]].ChargingState;
            if (!ChargerInfo.Contains("充电中"))
            {
                ControlEquipMent.ChargerCtrl.ChargerStop(lstIDs);
                Thread.Sleep(200);
                ControlEquipMent.ChargerCtrl.ChargerStop(lstIDs);
                Thread.Sleep(2000);
                ControlEquipMent.ChargerCtrl.ChargerStart(lstIDs);
                Thread.Sleep(200);
            }
            //检测是否充电
            int iWaitTime_s = 100;//等待时间100s
            bool bState = true;//是否都已经充电
            while(iWaitTime_s-- > 0)
            {
                SendNoticeToUIAndTxtFile("等待检测倒计时(s)：" + iWaitTime_s.ToString());
                for (int i = 0; i < lstIDs.Count; i++)
                {
                    double Voltage = AllEquipStateData.DicChargerNTGXCtrl_StateData[lstIDs[i]].ChargingVoltage;
                    if (AllEquipStateData.DicChargerNTGXCtrl_StateData[lstIDs[i]].ChargingState.Contains("充电中")
                        || Voltage > 150)
                    {
                        bState &= true;
                    }
                    else
                    {
                        bState = false;
                    }
                }

                if(bState)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicChargerNTGXCtrl_StateData[lstIDs[i]].ChargingVoltage;
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }

            //充电状态后，等待稳定
            if(lstIDs.Count > 0)
            {
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 检测放电
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public bool CheckDisCharge_NTGX_CN(List<int> lstIDs)
        {
            bool result = false;
            SendNoticeToUIAndTxtFile("检测储能充电桩放电状态");
            try
            {
                Check_NTGX_DisCharge(ref lstIDs);

                //有至少一个桩正常刷卡起桩了 
                if (lstIDs.Count > 0)
                {
                    result = true;
                }
                if (!result)
                {
                    ProcessDataResult(lstIDs, "-", "无法启动放电", false, "-");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }
        private void Check_NTGX_DisCharge(ref List<int> lstIDs)
        {
            if (lstIDs.Count < 1)
                return;
            Thread.Sleep(1000);//重要延时，勿删     防止上一个测试项目结束后,导引状态改变有延迟     

            //string ChargerInfo = AllEquipStateData.DicChargerNTGXCtrl_StateData[lstIDs[0]].ChargingState;
            //if (!ChargerInfo.Contains("充电中"))
            //{
            //    ControlEquipMent.ChargerCtrl.ChargerStop(lstIDs);
            //    Thread.Sleep(200);
            //    ControlEquipMent.ChargerCtrl.ChargerStart(lstIDs);
            //    Thread.Sleep(200);
            //}
            //检测是否充电
            int iWaitTime_s = 100;//等待时间100s
            bool bState = true;//是否都已经充电
            MessgaeInfo(true, "请操作充电桩放电", false);
            while (iWaitTime_s-- > 0)
            {
                bState = true;
                SendNoticeToUIAndTxtFile("等待检测倒计时(s)：" + iWaitTime_s.ToString());
                for (int i = 0; i < lstIDs.Count; i++)
                {
                    double Voltage = AllEquipStateData.DicPowerAnalyzer_StateData[lstIDs[i]].Channel1RMSVolt;
                    double Voltage2 = AllEquipStateData.DicPowerAnalyzer_StateData[lstIDs[i]].Channel4RMSVolt;
                    if (Voltage>=100
                        || Voltage2 >= 200)
                    {
                        bState &= true;
                    }
                    else
                    {
                        bState = false;
                    }
                }

                if (bState)
                {
                    break;
                }

                Thread.Sleep(1000);
            }
            MessgaeInfo(false, "请操作充电桩放电", false);

            for (int i = 0; i < lstIDs.Count; i++)
            {
                double Voltage = AllEquipStateData.DicPowerAnalyzer_StateData[lstIDs[i]].Channel1RMSVolt;
                double Voltage2 = AllEquipStateData.DicPowerAnalyzer_StateData[lstIDs[i]].Channel4RMSVolt;
                if (Voltage < 100 && Voltage2 < 200)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[i] + "号枪处于放电状态，该枪停止检测");
                    lstIDs.Remove(lstIDs[i]);
                }
            }
        }

        #endregion

        #region ---------------------直流BMS函数-------------------------


        /// <summary>
        /// 负载合并
        /// </summary>
        public void CombineControlResistance()
        {
            try
            {

                ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
                string CombineIndex = ConfigurationManager.AppSettings["CombineIndex"].ToString().ToUpper();
                int index = -1;
                int.TryParse(CombineIndex, out index);

                if (index >= 1)
                {

                    List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[index - 1] = true;
                    ControlEquipMent.ControlBoard?.ControlResistanceSetRelay(list);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }


        }
        /// <summary>
        /// 负载单个
        /// </summary>
        public void SingleControlResistance()
        {
            try
            {
                string Customer = ConfigurationManager.AppSettings["CombineIndex"].ToString().ToUpper();
                int index = -1;
                int.TryParse(Customer, out index);

                if (index >= 1)
                {
                    List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    Thread.Sleep(500);
                    list[index - 1] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }



        }
        /// <summary>
        /// 充电中的各个开关
        /// </summary>
        public List<bool> GetKStatus16_Charging_DC()
        {
            List<bool> Ks = new List<bool>();
            for (int i = 0; i < 32; i++)
            {
                Ks.Add(false);
            }
            Ks[0] = true;//DC+对地不导电
            Ks[8] = true;//DC-对地不导电
            Ks[17] = true;//PE
            Ks[20] = true;//S+
            Ks[21] = true;//S-
            Ks[22] = true;//CC1
            Ks[23] = true;//CC2
            Ks[24] = true;//A+
            Ks[25] = true;//A-
            Ks[27] = true;//开关S
            //18DC+,19DC-,26true输出过压控制,31true停止发送报文
            return Ks;
        }

        /// <summary>
        /// 充电中的各个开关
        /// </summary>
        public List<bool> GetKStatus16_Charging_EU_DC()
        {
            List<bool> Ks = new List<bool>();
            for (int i = 0; i < 24; i++)
            {
                Ks.Add(false);
            }
            Ks[0] = true;//DC+DC-控制
            Ks[1] = true;//CC信号控制
            Ks[2] = true;//CP信号控制
            Ks[4] = true;//PE信号控制
            Ks[6] = true;//电子锁
            return Ks;
        }

        /// <summary>
        /// CP断线直流欧标
        /// </summary>
        public void SetCPRersh_EUDC()
        {
            //测试结束之后testWorkParam.lstIDs已经被清空了，不能用
            List<int> lstIDs = new List<int>();
            if (testWorkParam.lstIDs.Count < 1)
                lstIDs = this.lstIDs;
            else
                lstIDs = testWorkParam.lstIDs;

            string BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState;
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD"))
                BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState;
            if (BMSInfo == "CurrentDemandReq" || BMSInfo == "CurrentDemandRes")
            {
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(1000);
            }
            //else
            {
                SendNoticeToUIAndTxtFile("关闭导引中...");
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                Thread.Sleep(4000);
                SendNoticeToUIAndTxtFile("模拟拔枪中...");
                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = false;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), 0, 0, "0");
                System.Threading.Thread.Sleep((int)RefreshCPWaitTime * 1000);
                if (Customer != null && Customer.ToString().ToUpper().Contains("DH"))
                    Thread.Sleep(13000);
                SendNoticeToUIAndTxtFile("模拟插枪中...");
                Ks[2] = true;//CP信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, 390, Ks.ToArray(), 0, 0, "0");
            }
        }

        public void SetCPRersh_JPDC()
        {
            string BMSInfo = AllEquipStateData.DicBMS_JP_DC_StateData[lstIDs[0]].SystemState;
            string Customer = ConfigurationManager.AppSettings["Customer"];

            SendNoticeToUIAndTxtFile("关闭导引中...");
            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            Thread.Sleep(4000);
            SendNoticeToUIAndTxtFile("模拟拔枪中...");
            ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8A, false, new string[] { "emtBMS_JP_DC" });
            //System.Threading.Thread.Sleep((int)RefreshCPWaitTime * 1000);
            System.Threading.Thread.Sleep(3000);
            SendNoticeToUIAndTxtFile("模拟插枪中...");
            ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8A, true, new string[] { "emtBMS_JP_DC" });
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// CP断线模拟插拔枪--直流欧标
        /// </summary>
        public void SetCPRersh_EUDCALL()
        {
            string BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs[0]].SystemState;
            //if (BMSInfo == "CurrentDemandReq" || BMSInfo == "CurrentDemandRes")
            //可能故障停充后状态No Start也需要BMS OFF       --DZJ 2024/10/31
            {
                SendNoticeToUIAndTxtFile("关闭导引中...");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
            }
            SendNoticeToUIAndTxtFile("模拟拔枪中...");
            bool[] Ks = new bool[24];
            Ks[0] = true;   //DC+DC-控制
            Ks[1] = true;   //CC信号控制
            Ks[2] = false;  //CP信号控制
            Ks[4] = true;   //PE信号控制
            ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0, "0");
            System.Threading.Thread.Sleep((int)RefreshCPWaitTime * 1000);
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (Customer != null && Customer.ToString().ToUpper().Contains("DH"))
                Thread.Sleep(13000);
            SendNoticeToUIAndTxtFile("模拟插枪中...");
            Ks[2] = true;   //CP信号控制
            ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0, "0");
            //ZD的欧标桩需要时间等待绝缘检测（插拔枪后），否则会启动失败
            if (Customer != null && Customer.ToString().ToUpper().Contains("ZD"))
                Thread.Sleep(4000);
        }
        /// <summary>
        /// 判断是否全部枪符合所需状态
        /// </summary>
        /// <param name="status">充电状态的文本</param>
        /// <returns></returns>
        public bool JudgeBMSChargerStatus(string status)
        {
            bool isStatus = true;
            var bmsData = AllEquipStateData.DicBMS_DC_StateData.Values.ToArray();
            // 如果枪ID集合中没有任何一个与BMS相对应，直接返回false
            if (bmsData.Where(bms => testWorkParam.lstIDs.Contains(bms.ChargerID)).ToArray().Length < 1)
                return false;
            foreach (var item in bmsData)
            {
                // 没有插枪的导引不做判断
                if (!testWorkParam.lstIDs.Contains(item.ChargerID))
                    continue;
                // 如果其中一个枪不符合状态，直接返回false
                if (item.ChargingState != status)
                {
                    isStatus = false;
                    break;
                }
            }
            return isStatus;
        }
        public static int ChangeBMSChargeStatus(string value)
        {

            if (value == "空闲状态")
            {
                return 0;
            }

            if (value == "等待辅源")
            {
                return 1;
            }
            if (value == "等待握手报文")
            {
                return 2;
            }
            if (value == "等待辨识报文SPN2560=0x00")
            {
                return 3;
            }
            if (value == "等待辨识报文SPN2560=0x01")
            {
                return 4;
            }
            if (value == "等待CTS、CML报文")
            {
                return 5;
            }
            if (value == "等待CRO_00报文")
            {
                return 6;
            }
            if (value == "等待CRO_AA报文")
            {
                return 7;
            }
            if (value == "等待充电开始")
            {
                return 8;
            }
            if (value == "充电中")
            {
                return 9;
            }
            if (value == "等待充电的中止报文")
            {
                return 10;
            }
            if (value == "等待充电机充电统计")
            {
                return 11;
            }
            if (value == "完成接收充电数据统计")
            {
                return 12;
            }
            if (value == "充电结束")
            {
                return 13;
            }
            if (value == "未定义")
            {
                return -1;
            }
            return -1;
        }
        public static int ChangeBMSChargeStatus_EU_DC(string value)
        {
            int itmp = -1;
            switch(value)
            {
                case "Not start":
                    itmp = 0;
                    break;
                case "WaitingForCharging":
                    itmp = 1;
                    break;
                case "SLAC":
                    itmp = 2;
                    break;
                case "SDP":
                    itmp = 3;
                    break;
                case "SessionSetupReq":
                    itmp = 4;
                    break;
                case "SessionSetupRes":
                    itmp = 5;
                    break;
                case "SessionDiscoveryReq":
                    itmp = 6;
                    break;
                case "ServiceDiscoveryRes":
                    itmp = 7;
                    break;
                case "ServicePaymentSlecionReq":
                    itmp = 8;
                    break;
                case "ServicePaymentSelectionRes":
                    itmp = 9;
                    break;
                case "ContractAuthenticationReq":
                    itmp = 10;
                    break;
                case "ContractAuthenticationRes":
                    itmp = 11;
                    break;
                case "ChargeParameterDiscoveryReq":
                    itmp = 12;
                    break;
                case "ChargeParameterDiscoveryRes":
                    itmp = 13;
                    break;
                case "PowerDeliveryReq":
                    itmp = 14;
                    break;
                case "PowerDeliveryRes":
                    itmp = 15;
                    break;
                case "CableCheckReq":
                    itmp = 16;
                    break;
                case "CableCheckRes":
                    itmp = 17;
                    break;
                case "PreChargeReq":
                    itmp = 18;
                    break;
                case "PreChargeRes":
                    itmp = 19;
                    break;
                case "CurrentDemandReq":
                    itmp = 20;
                    break;
                case "CurrentDemandRes":
                    itmp = 21;
                    break;
                case "WeldingDetectionReq":
                    itmp = 22;
                    break;
                case "WeldingDetectionRes":
                    itmp = 23;
                    break;
                case "SessionStopReq":
                    itmp = 24;
                    break;
                case "SessionStopRes":
                    itmp = 25;
                    break;

            }
            return itmp;
        }

        public static int ChangeBMSChargeStatus_JP_DC(string ChargingState)
        {
            try
            {
                if (ChargingState == "空闲状态")
                {
                    return 0;
                }
                if (ChargingState == "等待充电")
                {
                    return 1;
                }
                if (ChargingState == "等待H108、H109")
                {
                    return 2;
                }
                if (ChargingState == "锁止判断")
                {
                    return 3;
                }
                if (ChargingState == "绝缘检测")
                {
                    return 4;
                }
                if (ChargingState == "准备充电")
                {
                    return 5;
                }
                if (ChargingState == "电池检测")
                {
                    return 6;
                }
                if (ChargingState == "正在充电")
                {
                    return 7;
                }
                if (ChargingState == "输出停止")
                {
                    return 8;
                }
                if (ChargingState == "焊接检测")
                {
                    return 10;
                }
                if (ChargingState == "d2信号断开")
                {
                    return 11;
                }
                if (ChargingState == "d1信号断开")
                {
                    return 12;
                }
                if (ChargingState == "充电解锁")
                {
                    return 13;
                }
                if (ChargingState == "充电结束")
                {
                    return 14;
                }
                if (ChargingState == "未定义")
                {
                    return -1;
                }
                return -1;
            }
            catch
            {

            }
            return -1;
        }
        #endregion

        #region ---------------------交流BMS函数-------------------------

        /// <summary>
        /// 充电中的各个开关
        /// </summary>
        public List<bool> GetKStatus16_Charging()
        {
            List<bool> Ks = new List<bool>();
            for (int i = 0; i < 16; i++)
            {
                Ks.Add(false);
            }
            Ks[0] = true;//开关S2
            Ks[2] = true;//CC
            Ks[3] = true;//CP
            Ks[5] = true;//PE
            Ks[9] = true;
            //Alarm0 A枪开关S
            //Alarm1 A枪风扇控制
            //Alarm2 A枪CC断开控制
            //Alarm3 A枪CP断开控制
            //Alarm4 A枪CP接地控制
            //Alarm5 A枪PE断开控制
            //Alarm6 A枪CC电阻检测控制
            //Alarm7 A枪定时充电控制
            //Alarm8 漏电控制
            //Alarm9 电子锁控制
            //Alarm10 预留
            //Alarm11 预留
            //Alarm12 预留
            //Alarm13 预留
            //Alarm14 预留
            //Alarm15 二极管短接控制


            return Ks;
        }
        /// <summary>
        /// 模拟CP断线再恢复。模拟拔枪插枪
        /// </summary>
        public void SetCPReresh()
        {
            //测试结束之后testWorkParam.lstIDs已经被清空了，不能用
            List<int> lstIDs = new List<int>();
            if (testWorkParam.lstIDs.Count < 1)
                lstIDs = this.lstIDs;
            else
                lstIDs = testWorkParam.lstIDs;

            if (LstChargerInfo.First().ChargerType == EmChargerType.Charger_EUR_DC || LstChargerInfo.First().ChargerType == EmChargerType.Charger_USA_DC)
            {
                SetCPRersh_EUDC();
                return;
            }

            List<bool> Ks = GetKStatus16_Charging_DC();

            if (IsNeedRereshCP)
            {

                //受直流桩特性影响，充电状态有很多种可能性，此处多做很多冗余判断，以保证覆盖所有情况
                SendNoticeToUIAndTxtFile("发送控制导引断线，模拟拔枪 ");
                if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                {
                    Ks = GetKStatus16_Charging();
                    Ks[3] = false;
                    ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                    Thread.Sleep(1000);
                }
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    foreach (int chargerId in lstIDs)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                        {
                            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                            if (!"充电中".Equals(BMSInfo))
                            {
                                Ks = GetKStatus16_Charging_DC();
                                Ks[22] = false;
                                ControlEquipMent.BMS.BMSSetKState_DC(new List<int>() { chargerId}, 1000, BatteryVoltage, Ks.ToArray());
                            }
                        }
                    }
                }
            }
            if (IsNeedRereshVoltage)
            {
                ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
            }
            if (IsNeedRereshCP)
            {

                Thread.Sleep(Convert.ToInt32(RefreshCPWaitTime * 1000));
                SendNoticeToUIAndTxtFile("恢复控制导引连接，模拟插枪 ");
                ControlEquipMent.ACSource?.ACSource_ON(lstIDs);
                if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                {
                    //模拟CP恢复，S2断开
                    Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                }
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    foreach (int chargerId in lstIDs)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                        {
                            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                            if (!"充电中".Equals(BMSInfo))
                            {
                                Ks = GetKStatus16_Charging_DC();
                                ControlEquipMent.BMS.BMSSetKState_DC(new List<int>() { chargerId }, 1000, BatteryVoltage, Ks.ToArray());
                            }
                        }
                    }
                }
                Thread.Sleep(500);

            }
            if (IsNeedRereshVoltage)
            {
                if (!IsNeedRereshCP)
                {
                    Thread.Sleep(3000);
                }
                ControlEquipMent.ACSource?.ACSource_ON(lstIDs);
            }
        }

        public void PullCharger(List<int> lstIDs)
        {
            SendNoticeToUIAndTxtFile("发送控制导引断线，模拟拔枪 ");
            foreach (int chargerId in lstIDs)
            {
                if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                {
                    List<int> lstCharger = new List<int>() { chargerId };
                    string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                    if (!"等待辅源".Contains(BMSInfo))
                    {
                        ControlEquipMent.BMS.BMS_OFF(lstCharger);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.BMS_ON(lstCharger);
                        Thread.Sleep(1000);
                    }
                    var Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstCharger, 1000, BatteryVoltage, Ks.ToArray());
                }
            }
        }

        public void InsertCharger(List<int> lstIDs)
        {
            SendNoticeToUIAndTxtFile("恢复控制导引连接，模拟插枪 ");
            foreach (int chargerId in lstIDs)
            {
                if (AllEquipStateData.DicBMS_DC_StateData.ContainsKey(chargerId))
                {
                    List<int> lstCharger = new List<int>() { chargerId };
                    string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[chargerId]?.ChargingState;
                    if (!"等待辅源".Contains(BMSInfo))
                    {
                        ControlEquipMent.BMS.BMS_OFF(lstCharger);
                        Thread.Sleep(2000);
                        ControlEquipMent.BMS.BMS_ON(lstCharger);
                        Thread.Sleep(1000);
                    }
                    //var Ks = GetKStatus16_Charging_DC();
                    //插枪之前可能模拟了输出过压或者闭合了别的开关，应该先读取不能直接设置默认值
                    var Ks = ControlEquipMent.BMS.BMSGetKState_DC(lstIDs, out double R4, out double batteryVolt)[chargerId];
                    Ks[22] = true;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstCharger, R4, batteryVolt, Ks.ToArray());
                }
            }
        }
        #endregion

        #region --------------------电阻负载相关函数----------------------
        /// <summary>
        /// 设置电阻负载电压电流并启动
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="voltage"></param>
        /// <param name="current"></param>
        protected void SetResisLoadVolCurrAndStart(List<int> lstIDs, double voltage, double current, bool isOFF = true)
        {
            try
            {
                if (RelayIndex < 0) //不需要软件控制负载单、三相并机功能
                {
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, voltage, current);
                    Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            //需要软件控制负载单、三相并机功能
            if (isOFF)
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
            if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
            {
                if (ControlEquipMent.ControlBoard.DitEquipMentBase.Where(e => e.Value.EquipMentClassName.Equals("emtDIORelay")).ToArray().Length > 0)
                {
                    string isControlDIO_AC = ConfigurationManager.AppSettings["isControlDIO_AC"];
                    foreach (int chargerId in lstIDs)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData.ContainsKey(chargerId))
                        {
                            List<int> lstCharger = new List<int>() { chargerId };
                            if (isControlDIO_AC != null && Convert.ToBoolean(isControlDIO_AC) && !Customer.Contains("YT"))
                            {
                                //TS是Y1三相，Y2单相
                                if (new string[] { "TS", "SKY" }.Contains(Customer))
                                {
                                    if (AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseA_Voltage > 70
                                      && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseB_Voltage < 50
                                      && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseC_Voltage < 50)
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 0, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 1, true);
                                    }
                                    else
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 1, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 0, true);
                                    }
                                }
                                else
                                    ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 2, true);
                            }
                            ////2025-12-12-2--DZJ 惠州TB所有都是Y3三相Y4单相
                            //else if(Customer != null && Customer.Contains("TB"))
                            //{
                            //    if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage > 70
                            //      && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseB_Voltage < 50
                            //      && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseC_Voltage < 50)
                            //    {
                            //        ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                            //        Thread.Sleep(1000);
                            //        ControlEquipMent.ControlBoard.SetRelaySwitch(2, true);
                            //    }
                            //    else
                            //    {
                            //        ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                            //        Thread.Sleep(1000);
                            //        ControlEquipMent.ControlBoard.SetRelaySwitch(3, true);
                            //    }
                            //}
                            //拨码测试不能充电，但是会尝试启动负载
                            else if (AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseA_Voltage < 50
                                  && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseB_Voltage < 50
                                  && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseC_Voltage < 50)
                            {
                                //不做任何动作
                            }
                            else
                            {
                                if (AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseA_Voltage > 50
                                  && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseB_Voltage > 50
                                  && AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseC_Voltage > 50)
                                {
                                    if (Customer != null && new string[] { "TS", "SKY" }.Contains(Customer))
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 1, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 0, true);
                                    }
                                    else
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 3, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 2, true);
                                    }
                                }
                                else
                                {
                                    if (Customer != null && new string[] { "TS", "SKY" }.Contains(Customer))
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 0, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 1, true);
                                    }
                                    else
                                    {
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 2, false);
                                        Thread.Sleep(1000);
                                        ControlEquipMent.ControlBoard.SetRelaySwitch(lstCharger, 3, true);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<bool> lstRelay = ControlEquipMent.ControlBoard.ControlBoardReadState();

                    if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage > 70
                          && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseB_Voltage < 50
                          && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseC_Voltage < 50)
                    {

                        lstRelay[RelayIndex] = true;
                    }
                    else
                    {
                        lstRelay[RelayIndex] = false;
                    }
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                    Thread.Sleep(300);
                }
            }


            ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, voltage, current);
            Thread.Sleep(1000);
            ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs);
        }


        #endregion

        #region 适配电阻负载和回馈载

        /// <summary>
        /// 负载开启命令
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="IsFirstFeedbackLoad">是否优先回馈载</param>
        public void SetLoadDCON(List<int> lstIDs, bool IsFirstFeedbackLoad = true)
        {
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (LoadWaitTime_s != 0)
            {
                Thread.Sleep(LoadWaitTime_s * 1000);//XJ客户桩得模块启动较慢，这里增加5秒延时。
            }
            if (Customer != null && Customer.Contains("GJ"))
            {
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();

                if (list[8])
                {
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs);

                    List<int> lstIDs2 = new List<int> { 2 };
                    System.Threading.Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs2);
                    System.Threading.Thread.Sleep(1000);
                    return;
                }
            }

            if (Customer != null && (Customer.Contains("GX")))
            {
                if (ControlEquipMent.ChargerCtrl != null)
                {
                    SendNoticeToUIAndTxtFile("充电桩模拟器启动负载...");
                    ControlEquipMent.ChargerCtrl.LoadStart_Charger(lstIDs);
                    return;
                }
            }
            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            if (IsFirstFeedbackLoad)
            {
                if (ControlEquipMent.FeedbackLoad != null)
                {
                    Thread.Sleep(1000);     //可能设置和启动间隔太短会导致回馈载挂了
                    ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(lstIDs);
                    if (Customer != null && (Customer.Contains("XJ")))//XJ客户回馈负载多通道响应问题，这里多发一次
                    {
                        ControlEquipMent.FeedbackLoad?.FeedbackLoad_ON(lstIDs);
                    }
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
                else if (ControlEquipMent.ResistanceLoad != null)
                {
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
            }
            else
            {
                if (ControlEquipMent.ResistanceLoad != null)
                {
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(lstIDs);
                }
                else if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad.FeedbackLoad_ON(lstIDs);
                    if (Customer != null && (Customer.Contains("XJ")))//XJ客户回馈负载多通道响应问题，这里多发一次
                    {
                        ControlEquipMent.FeedbackLoad?.FeedbackLoad_ON(lstIDs);
                    }
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
            }
        }
        /// <summary>
        /// 负载关闭命令
        /// <param name="lstIDs"></param>
        /// <param name="IsFirstFeedbackLoad">是否优先回馈载</param>
        /// </summary>
        public void SetLoadDCOFF(List<int> lstIDs, bool IsFirstFeedbackLoad = true, bool isQuick = false)
        {
            //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            //if (Customer != null && Customer.Contains("GJ"))
            //{
            //    bool[] State = new bool[16];
            //    State[6] = false;
            //    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(State.ToList());
            //}


            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer != null && Customer.Contains("GJ"))
            {
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();

                if (list[8])
                {
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);

                    List<int> lstIDs2 = new List<int> { 2 };
                    System.Threading.Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs2);
                    System.Threading.Thread.Sleep(1000);
                    return;
                }
            }


            if (Customer != null && (Customer.Contains("JCZT")))
            {
                CountDownTimeInfo("请关闭负载后点击确认", 9999, 1);
                return;
            }


            if (Customer != null && (Customer.Contains("GX")))
            {
                if (ControlEquipMent.ChargerCtrl != null)
                {
                    SendNoticeToUIAndTxtFile("充电桩模拟器停止负载...");
                    ControlEquipMent.ChargerCtrl.LoadStop_Charger(lstIDs);
                    return;
                }
            }

            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            if (IsFirstFeedbackLoad)
            {
                if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    if (Customer != null && (Customer.Contains("XJ")))//XJ客户回馈负载多通道响应问题，这里多发一次
                    {
                        ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    }
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    ControlEquipMent.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                    if (!isQuick)
                        LoopFeedbackLoad_NoParallel(lstLoadChannel, channel);
                }
                else if (ControlEquipMent.ResistanceLoad != null)
                {
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_OFF(lstIDs);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    ControlEquipMent.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                    if (!isQuick)
                        StarLoopFeedbackLoad_NoParallel(lstLoadChannel);
                }
                ControlEquipMent.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                ControlEquipMent.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
            }
            else
            {
                if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    if (Customer != null && (Customer.Contains("XJ")))//XJ客户回馈负载多通道响应问题，这里多发一次
                    {
                        ControlEquipMent.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                    }
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    ControlEquipMent.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                    if (!isQuick)
                        LoopFeedbackLoad_NoParallel(lstLoadChannel);
                }
                else if (ControlEquipMent.ResistanceLoad != null)
                {
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_OFF(lstIDs);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    ControlEquipMent.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                    if (!isQuick)
                        LoopFeedbackLoad_NoParallel(lstLoadChannel);
                }
                ControlEquipMent.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                ControlEquipMent.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
            }
            if (!isQuick)
            {
                //等待负载电流下降
                WaitDCLoadOFF(lstIDs, 5);
                Thread.Sleep(2000);
                //CJB环形回馈载电流消失运行灯灭了风扇还在转，此时没有完全停止再启动会异常
                if(Customer != null && Customer.Equals("CJB"))
                {
                    Thread.Sleep(15 * 1000);
                }
            }
        }



        /// <summary>
        /// 全部负载关闭命令
        /// </summary>
        //public void SetLoadDCOFFALL(List<int> lstIDs)
        //{
        //    ControlEquipMent.FeedbackLoad.FeedbackLoad_OFF(lstIDs);
        //    ControlEquipMent.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, 2);
        //    ControlEquipMent.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, 2);
        //    ControlEquipMent.ResistanceLoad?.ResistanceLoad_OFF(lstIDs);
        //}

        /// <summary>
        /// 设置负载电压电流参数优先使用回馈载再是电阻载
        /// </summary>
        /// <param name="Voltage1"></param>
        /// <param name="Current1"></param>
        /// <param name="Voltage2"></param>
        /// <param name="Current2"></param>
        /// <param name="IsFirstFeedbackLoad">是否优先回馈载</param>
        public void SetLoadPara(List<int> lstIDs, double Voltage1, double Current1, double Voltage2, double Current2, bool IsFirstFeedbackLoad = true, bool isParallel = true)
        {
            //惠州TB的直流测试系统，响河交流源性能问题导致投载240kW得先投160kW等待3s再投240
            if (Is240kWRestrict != null && Is240kWRestrict.Equals("1") && Voltage1 * Current1 >= 240000 * 0.95)
            {
                ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(lstIDs, Voltage1, 160000 / Voltage1);
                Thread.Sleep(500);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, 160000 / Voltage1);
                Thread.Sleep(1000 * 3);
            }
            //环形回馈载的压差不能太小
            if (ControlEquipMent.LoopFeedbackLoad != null && Voltage1 > MaxAllowChargeVoltage - 15)
                Voltage1 = MaxAllowChargeVoltage - 15;
            //Current1 = Current1 > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Math.Round(Current1, 2);    //回馈负载不需要限制
            Current2 = Current2 > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : Math.Round(Current2, 2);
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer != null && Customer.Contains("GJ"))
            {
                double VoltageMAX = Current2 / 0.12;
                //double Power = Voltage2 * Current2 / 1000; 
                if (VoltageMAX > (Voltage2 - 5))
                {
                    List<bool> State = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    State[6] = true;
                    State[8] = true;
                    ControlEquipMent.ControlBoard?.ControlResistanceSetRelay(State);
                    List<int> lstIDs2 = new List<int> { 2 };

                    Double NewCurrent = Current2 / 2;

                    Double CheckCCurrent = Voltage2 * 0.12;

                    NewCurrent = NewCurrent > CheckCCurrent ? CheckCCurrent : NewCurrent;


                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, Voltage2, NewCurrent);
                    System.Threading.Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs2, Voltage2, NewCurrent);
                    System.Threading.Thread.Sleep(1000);
                    return;

                    //    double Power1 = (Voltage2 * 0.12 * Voltage2) / 1000;

                    //    double Power2 = Power - Power1;
                    //    double NewCurrent1 = Voltage1 * 0.12;
                    //    double NewCurrent2 = (Power2 / Voltage2) * 1000;


                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, Voltage2, NewCurrent1);

                    //    List<int> lstIDs2 = new List<int>{ 2};

                    //    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs2, Voltage2, NewCurrent2);
                    //    return;
                }
                else
                {
                    List<bool> State = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    State[6] = true;
                    ControlEquipMent.ControlBoard?.ControlResistanceSetRelay(State);
                }

            }

            if (Customer != null && (Customer.Contains("JCZT")))
            {
                if (Current2 == 0)
                {
                    CountDownTimeInfo("请关闭负载。", 9999, 0);

                }
                else
                {
                    Current2 = RetainDecimals<double>(Current2);
                    CountDownTimeInfo("请操作负载输出电压为;" + Voltage2 + ";" + "电流为:" + Current2 + "后再点击确认。", 9999, 0);

                }
                return;
            }

            if (Customer != null && (Customer.Contains("GX")))
            {
                if (ControlEquipMent.ChargerCtrl != null)
                {
                    SendNoticeToUIAndTxtFile("充电桩模拟器设置负载参数【" + Voltage1.ToString() + "V, " + Current1.ToString() + "A】");
                    ControlEquipMent.ChargerCtrl.SetLoadParam_Charger(lstIDs, Voltage1, Current1);
                    return;
                }
            }

            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            if (IsFirstFeedbackLoad)
            {
                if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(lstIDs, Voltage1, Current1);
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    if (isParallel)
                    {
                        if (Voltage1 >= 500)
                            LoopFeedbackLoad_Parallel(lstIDs, Voltage1 * Current1);
                        else
                            LoopFeedbackLoad_Parallel(lstIDs, Voltage1, Current1);
                    }
                    ControlEquipMent.LoopFeedbackLoad?.SetLoopFeedbackLoadParams(lstIDs, channel, Voltage1, Current1);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    if (isParallel)
                    {
                        StarLoopFeedbackLoad_Parallel(lstIDs, Voltage1 * Current1);
                    }

                    ControlEquipMent.StarLoopFeedbackLoad?.SetLoopFeedbackLoadParams(lstIDs, channel, Voltage1, Current1);
                }
                else
                {
                    //添加间隔，不然可能启动不了
                    if (ControlEquipMent.ResistanceLoad.DitEquipMentBase.FirstOrDefault().Value.GetType() == typeof(emtResistanceLoad_MultiChannel_DC))
                        Thread.Sleep(1500);
                    ControlEquipMent.ResistanceLoad?.SetResisLoadVolCurr(lstIDs, Voltage2, Current2);
                }
            }
            else
            {
                if (ControlEquipMent.ResistanceLoad != null)
                {
                    //添加间隔，不然可能启动不了
                    if (ControlEquipMent.ResistanceLoad.DitEquipMentBase.FirstOrDefault().Value.GetType() == typeof(emtResistanceLoad_MultiChannel_DC))
                        Thread.Sleep(1500);
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, Voltage2, Current2);
                }
                else if (ControlEquipMent.FeedbackLoad != null)
                {
                    ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(lstIDs, Voltage1, Current1);
                }
                else if (ControlEquipMent.LoopFeedbackLoad != null)
                {
                    if (isParallel)
                    {
                        if (Voltage1 >= 500)
                            LoopFeedbackLoad_Parallel(lstIDs, Voltage1 * Current1);
                        else
                            LoopFeedbackLoad_Parallel(lstIDs, Voltage1, Current1);
                    }
                    ControlEquipMent.LoopFeedbackLoad?.SetLoopFeedbackLoadParams(lstIDs, channel, Voltage1, Current1);
                }
                else if (ControlEquipMent.StarLoopFeedbackLoad != null)
                {
                    if (isParallel)
                    {
                        StarLoopFeedbackLoad_Parallel(lstIDs, Voltage1 * Current1);
                    }
                    ControlEquipMent.StarLoopFeedbackLoad?.SetLoopFeedbackLoadParams(lstIDs, channel, Voltage1, Current1);
                }
            }
        }

        /// <summary>
        /// 是否为电阻载系统柜
        /// </summary>
        /// <returns></returns>
        public bool IsResistanceLoad()
        {
            if (ControlEquipMent.ResistanceLoad != null)
                return true;
            else return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Voltage2"></param>
        /// <param name="Current2"></param>
        /// <param name="HasDouble">默认有两个电阻载</param>
        /// <returns></returns>
        public bool IsRLoad(double Voltage2, double Current2, bool HasDouble = true)
        {
            //KS的电阻载是全档位支持
            if(Customer != null && Customer.Equals("KS"))
                return true;
            if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
            {
                return true;
            }
            else
            {
                if (ControlEquipMent.ResistanceLoad != null)
                {
                    double VoltageMAX = 150;
                    if (HasDouble)
                    {
                        VoltageMAX = Current2 / 0.24;

                    }
                    else
                    {
                        VoltageMAX = Current2 / 0.12;
                    }

                    if ((Voltage2 + 5) >= VoltageMAX)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 控制交流源启停
        public void ACSourceON(List<int> lstIDs)
        {
            string ACSourceOutputR = ConfigurationManager.AppSettings["ACSourceOutputR"];
            if (ControlEquipMent.ACSource != null)
                ControlEquipMent.ACSource.ACSource_ON(lstIDs);
            else if (!string.IsNullOrEmpty(ACSourceOutputR) && int.TryParse(ACSourceOutputR, out int r_index) && r_index > -1)
                ControlEquipMent.ControlBoard.SetRelaySwitch((uint)r_index, true);
            else
                CountDownTimeInfo("请打开充电机的交流电输入", 999, 0);
        }
        public void ACSourceOFF(List<int> lstIDs)
        {
            string ACSourceOutputR = ConfigurationManager.AppSettings["ACSourceOutputR"];
            if (ControlEquipMent.ACSource != null)
                ControlEquipMent.ACSource.ACSource_OFF(lstIDs);
            else if (!string.IsNullOrEmpty(ACSourceOutputR) && int.TryParse(ACSourceOutputR, out int r_index) && r_index > -1)
            {
                ControlEquipMent.ControlBoard.SetRelaySwitch((uint)r_index, false);
            }
            else
                CountDownTimeInfo("请断开充电机的交流电输入", 999, 0);
        }
        #endregion

        #region 等待交流源电压和直流DC电压
        /// <summary>
        /// 判断交流源改变后是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Voltage">电压</param>
        public void SetACSource(List<int> lstIDs, double Voltage)
        {
            if (ControlEquipMent.ACSource == null)
                return;
            // 客户的交流源没有提供协议只能手动修改交流源
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if(Customer != null && Customer.Equals("JCZT"))
            {
                CountDownTimeInfo($"请手动设置交流源电压：{Voltage}(V)", 999, 0);
            }
            else
            {
                ControlEquipMent.ACSource?.ACSource_SetVolt(lstIDs, Voltage);
                SendNoticeToUIAndTxtFile("设置交流源电压：" + Voltage);
                SendNoticeToUIAndTxtFile("等待交流源稳定倒计时");
                int timeout = 50;
                while (timeout-- > 0)
                {
                    double ACVoltageB = AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt_B;
                    double ACVoltage = AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt;

                    if (ACVoltageB < 10)
                    {
                        break;
                    }
                    if (ACVoltage <= (Voltage + 5) && ACVoltage >= (Voltage - 5))
                    {
                        break;
                    }
                    SendNoticeToUIAndTxtFile("等待交流源稳定倒计时:" + timeout);
                    if (timeout < 0)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 判断直流电压是否稳定到设置
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Voltage">电压</param>
        public bool WaitDCVoltage(List<int> lstIDs, double Voltage, int timeout = 80)
        {
            SendNoticeToUIAndTxtFile($"等待导引电压稳定至{Voltage}V...");
            while (timeout-- > 0)
            {
                //double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[lstIDs.First()].ChargingVoltage;
                List<BMS_DC_StateData> dicData = AllEquipStateData.DicBMS_DC_StateData.Values.ToList().FindAll(d => lstIDs.Contains(d.ChargerID)).ToList();
                List<double> lstDCVolt = dicData.Select(s => s.ChargingVoltage).ToList();
                bool isAllInRange = lstDCVolt.All(v => v >= Voltage * 0.9 && v <= Voltage * 1.02);

                if (isAllInRange)
                {
                    return true;
                }
                //SystemEvent.SendLogMessage("等待导引电压稳定倒计时： " + timeout + "秒 ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            return false;
        }

        /// <summary>
        /// 判断直流电压是否稳定到设置
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Voltage">电压</param>
        public bool WaitDCVoltage_EU_DC(List<int> lstIDs, double Voltage, int timeout = 80)
        {
            SendNoticeToUIAndTxtFile($"等待导引电压稳定至{Voltage}V...");
            while (timeout-- > 0)
            {
                double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs.First()].ChargingVoltage;

                if (DCVoltage >= Voltage * 0.9 && DCVoltage <= Voltage * 1.02)
                {
                    return true;
                }
                //SystemEvent.SendLogMessage("等待导引电压稳定倒计时： " + timeout + "秒 ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            return false;
        }
        public bool WaitDCVoltage_JP_DC(List<int> lstIDs, double Voltage, int timeout = 80)
        {
            SendNoticeToUIAndTxtFile($"等待导引电压稳定至{Voltage}V...");
            while (timeout-- > 0)
            {
                double DCVoltage = AllEquipStateData.DicBMS_JP_DC_StateData[lstIDs.First()].ChargingVoltage;

                if (DCVoltage >= Voltage * 0.9 && DCVoltage <= Voltage * 1.02)
                {
                    return true;
                }
                //SystemEvent.SendLogMessage("等待导引电压稳定倒计时： " + timeout + "秒 ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            return false;
        }

        /// <summary>
        /// 判断交流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        public void WaitACVoltage(List<int> lstIDs, double Voltage, int timeout = 80)
        {
            while (timeout-- > 0)
            {
                double DCVoltage = AllEquipStateData.DicBMS_AC_StateData[lstIDs.First()].PhaseA_Voltage;

                if (DCVoltage >= Voltage * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 判断交流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        public void WaitACCurrent(List<int> lstIDs, double Current)
        {
            int timeout = 80;
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_AC_StateData[lstIDs.First()].PhaseA_Current;

                if (DCCurrent >= Current * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        public void WaitDCCurrent(List<int> lstIDs, double Current)
        {
            SendNoticeToUIAndTxtFile($"等待导引电流稳定{Current}A");
            int timeout = 80;
            while (timeout-- > 0)
            {
                //double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[lstIDs.First()].ChargingCurrent;
                List<BMS_DC_StateData> dicData = AllEquipStateData.DicBMS_DC_StateData.Values.ToList().FindAll(d => lstIDs.Contains(d.ChargerID)).ToList();
                List<double> lstDCVolt = dicData.Select(s => s.ChargingCurrent).ToList();
                bool isAllInRange = lstDCVolt.All(c => c >= Current * 0.9);

                if (isAllInRange)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        public void WaitDCCurrent_EU_DC(List<int> lstIDs, double Current)
        {
            SendNoticeToUIAndTxtFile($"等待导引电流稳定{Current}A");
            int timeout = 80;
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_EU_DC_StateData[lstIDs.First()].ChargingCurrent;

                if (DCCurrent >= Current * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        public void WaitDCCurrent_JP_DC(List<int> lstIDs, double Current, int timeout = 80)
        {
            SendNoticeToUIAndTxtFile($"等待导引电流稳定{Current}A");
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_JP_DC_StateData[lstIDs.First()].ChargingCurrent;

                if (DCCurrent >= Current * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        /// <param name="timeout">时间</param>
        public void WaitDCCurrentWithTime(List<int> lstIDs, double Current, int timeout, double precision = 0.9)
        {
            //XJ项目的大功率桩启动较慢，可能达到40到60s的延时，这里增加延时
            if(Customer.Contains("XJ"))
            {
                timeout = 65;
            }
            SendNoticeToUIAndTxtFile($"等待导引电流{Current}A稳定...");
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent;
                if (DCCurrent >= Current * precision)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            //等待功率分析仪采集电流
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 判断直流电流(电压)变化是否已经到达目标值
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="CurrentStart">电流起始值</param>
        /// <param name="CurrentEnd">电流目标值</param>
        /// <param name="timeout"></param>
        public void WaitDCCurrentChangeWithTime(List<int> lstIDs,double CurrentStart, double CurrentEnd, int timeout)
        {
            //XJ项目的大功率桩启动较慢，可能达到40到60s的延时，这里增加延时
            if(Customer.Contains("XJ"))
            {
                timeout = 65;
            }
            SendNoticeToUIAndTxtFile("等待导引电流稳定...");
            int iCnt = 0;
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent;
                if (CurrentStart > CurrentEnd)//这个下降
                {
                    if (DCCurrent < (CurrentStart + CurrentEnd) / 2)
                    {
                        iCnt++;
                    }
                    else
                    {
                        iCnt = 0;
                    }
                }
                else//上升
                {
                    if (DCCurrent > (CurrentStart + CurrentEnd) / 2)
                    {
                        iCnt++;
                    }
                    else
                    {
                        iCnt = 0;
                    }
                }

                if (timeout < 0
                    || iCnt > 3)//产生的次数要大于3
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            //等待功率分析仪采集电流
            Thread.Sleep(1000);
        }
        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        /// <param name="timeout">时间</param>
        public void WaitDCCurrentWithTimeEU_DC(List<int> lstIDs, double Current, int timeout)
        {
            //XJ项目的大功率桩启动较慢，可能达到40到60s的延时，这里增加延时
            if (Customer.Contains("XJ"))
            {
                timeout = 65;
            }
            SendNoticeToUIAndTxtFile("等待导引电流稳定...");
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent;

                if (DCCurrent >= Current * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            //等待功率分析仪采集电流
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        /// <param name="timeout">时间</param>
        public void WaitDCCurrentWithTime_JP_DC(List<int> lstIDs, double Current, int timeout)
        {
            SendNoticeToUIAndTxtFile("等待导引电流稳定...");
            while (timeout-- > 0)
            {
                double DCCurrent = AllEquipStateData.DicBMS_JP_DC_StateData[LstChargerInfo[0].ChargerId].ChargingCurrent;

                if (DCCurrent >= Current * 0.9)
                {
                    break;
                }
                //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                if (timeout < 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            //等待功率分析仪采集电流
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 判断直流带载是否稳定
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Current">电流</param>
        public void WaitDCLoadOFF(List<int> lstIDs, double Current)
        {
            if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
            {
                int timeout = 80;
                while (timeout-- > 0)
                {
                    double DCCurrent = AllEquipStateData.DicBMS_DC_StateData[lstIDs.First()].ChargingCurrent;

                    if (DCCurrent <= Current)
                    {
                        break;
                    }
                    //SystemEvent.SendLogMessage("等待导引电流稳定倒计时： " + timeout + "秒   \r\t  \r\t ");
                    if (timeout < 0)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region 功分数据读取
        public Dictionary<int, string> GetPowerAnalyzerVoltage(List<int> lstIDs, double min, double max, int time = 20)
        {
            Dictionary<int, string> dicV = new Dictionary<int, string>();
            foreach (var item in lstIDs)
            {
                double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                while (time-- > 0)
                {
                    if (DCVoltage >= min && DCVoltage <= max)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                    DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt;
                }
                dicV.Add(item, DCVoltage.ToString("F2"));
            }
            return dicV;
        }

        public Dictionary<int, string> GetPowerAnalyzerCurrent(List<int> lstIDs, double min, double max, int time = 20)
        {
            Dictionary<int, string> dicC = new Dictionary<int, string>();
            foreach (var item in lstIDs)
            {
                double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                while(time -- > 0)
                {
                    if (DCCurrent >= min && DCCurrent <= max)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                    DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                }
                dicC.Add(item, DCCurrent.ToString("F2"));
            }
            return dicC;
        }
        #endregion

        #region 结果判断

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="value"></param>
        /// <param name="sname"></param>
        /// <param name="Result"></param>
        public void ProcessDataResult(List<int> lstIDs, string value, string sname, bool? Result, string sState = "")
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (Result == null)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.NA;
                        LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                    }
                    else
                    {
                        if (Result == true)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                        }
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="value"></param>
        /// <param name="sname"></param>
        /// <param name="Result"></param>
        public void ProcessDataResults(List<int> lstIDs, string value, string sname, Dictionary<int, bool> Result, string sState = "")
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    //if (Result[LstChargerInfo[i].ChargerId] == null)
                    //{
                    //    LstTrialData[k].TrialResult = EmTrialResult.NA;
                    //    LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                    //}
                    //else
                    {
                        if (Result[LstChargerInfo[i].ChargerId] == true)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|报表(勿删)";
                        }
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        public void ProcessDataResults(List<int> lstIDs, Dictionary<int, string> value, string sname, Dictionary<int, bool> Result, string sState = "")
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (Result[LstChargerInfo[i].ChargerId] == true)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value[LstChargerInfo[i].ChargerId] + "|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value[LstChargerInfo[i].ChargerId] + "|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        public void ProcessDataResults(List<int> lstIDs, Dictionary<int, string> value, string sname, Dictionary<int, EmTrialResult> Result, string sState = "", string min = "-", string max = "-", Dictionary<int, string> dImages = null)
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    StringBuilder sbtmp = new StringBuilder();
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }

                    LstTrialData[k].TrialResult = Result[item];
                    LstTrialData[k].ExtentData = $"{sState}|{sname}|{min}|{max}|{value[LstChargerInfo[i].ChargerId]}|{sbtmp}";
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="value"></param>
        /// <param name="sname"></param>
        /// <param name="Result"></param>
        public void ProcessDataResult(List<int> lstIDs, string min, string max, string value, string sname, bool? Result, string sState = "")
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (Result == null)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.NA;
                        LstTrialData[k].ExtentData = sState + "|" + sname + "|"+ min + "|" + max + "|" + value + "|报表(勿删)";
                    }
                    else
                    {
                        if (Result == true)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|" + min + "|" + max + "|" + value + "|报表(勿删)";
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|" + min + "|" + max + "|" + value + "|报表(勿删)";
                        }
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        public void ProcessDataResult(List<int> lstIDs, Dictionary<int, string> dImages, string value, string sname, bool? Result, string sState = "")
        {
            try
            {
                if (string.IsNullOrEmpty(sState))
                    sState = TrialItem.ItemName;
                foreach (var item in lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    StringBuilder sbtmp = new StringBuilder();
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    if (Result == null)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.NA;
                        LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|" + sbtmp.ToString();
                    }
                    else
                    {
                        if (Result == true)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|" + sbtmp.ToString();
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            LstTrialData[k].ExtentData = sState + "|" + sname + "|-|-|" + value + "|" + sbtmp.ToString();
                        }
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">结果值</param>
        /// <param name="dImages"></param>
        /// <param name="sname">数据名称</param>
        /// <param name="correctvalue">正确的值</param>
        public void ProcessDataIsNor(string value, Dictionary<int, string> dImages, string sname, string correctvalue, string itemName = "")
        {
            try
            {

                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    if (value == correctvalue)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = (string.IsNullOrEmpty(itemName) ? TrialItem.ItemName : itemName) + "|" + sname + "|-|-|" + value + "|" + sbtmp.ToString();
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = (string.IsNullOrEmpty(itemName) ? TrialItem.ItemName : itemName) + "|" + sname + "|-|-|" + value + "|" + sbtmp.ToString();
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 判断是否报警结果
        /// </summary>
        public void ProcessDataWarn(string sname)
        {
            try
            {

                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + sname + "|-|-|是|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + sname + "|-|-|否|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        /// <summary>
        /// 判断是否正常解锁
        /// </summary>
        public void ProcessDataConnect(string status = null, string sname = null)
        {
            try
            {
                if(string.IsNullOrEmpty(status))
                    status = TrialItem.ItemName;
                if (string.IsNullOrEmpty(sname))
                    sname = "正常解锁";
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].ItemName = iIndex.ToString();
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                    if (DicManualVerifyResult[item])
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = status + $"|{sname}|-|-|是|报表(勿删)";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = status + $"|{sname}|-|-|否|报表(勿删)";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
        #endregion 

        #region 测试流程函数
        /// <summary>
        /// 欧标获取电压误差限
        /// </summary>
        /// <param name="dU">设定电压</param>
        /// <returns></returns>
        public List<double> GetErrLimit_U_CCS2_DC(double dU)
        {
            List<double> dlimits = new List<double>();
            dlimits.Add(dU * 0.98);
            dlimits.Add(dU * 1.02);
            return dlimits;
        }

        /// <summary>
        /// 欧标获取电流误差限
        /// </summary>
        /// <param name="dI">设定电流</param>
        /// <returns></returns>
        public List<double> GetErrLimit_I_CCS2_DC(double dI)
        {
            List<double> dlimits = new List<double>();
            if (dI > 50)
            {
                dlimits.Add(dI * 0.95);
                dlimits.Add(dI * 1.05);
            }
            else
            {
                dlimits.Add(dI - 2.5);
                dlimits.Add(dI + 2.5);
            }
            return dlimits;
        }

        /// <summary>
        /// 获取电流调整时间
        /// </summary>
        /// <param name="dI"></param>
        /// <returns></returns>
        public List<double> GetCurrentAdjustTime_CCS2_DC(double dStart,double dEnd)
        {
            double dValue = Math.Abs(dStart - dEnd);
            List<double> dlimits = new List<double>();
            if (dValue > 20)
            {
                dlimits.Add(0);
                dlimits.Add((dValue / 20) * 1000);
            }
            else
            {
                dlimits.Add(0);
                dlimits.Add(1000);
            }
            return dlimits;
        }


        #endregion
        /// <summary>
        /// BMS打开CAN回复
        /// </summary>
        /// <param name="IsOpen"></param>
       public void BMSCanOpenRev(bool IsOpen)
        {
            if (IsOpen)
            {
                // . 找到 1 号 BMS
                var bmsDevice = ControlEquipMent?.BMS?.DitEquipMentBase?.Values.FirstOrDefault(e => e is emtBMS_GB_DC && e.ChargerID == 1);
                if (bmsDevice == null || !(bmsDevice is emtBMS_GB_DC bmsGbDc))
                    return;

                bmsGbDc.CANDATA.Clear();
            }
            if (IsOpen)
            SendNoticeToUIAndTxtFile("下位机打开CAN接收");
            else
                SendNoticeToUIAndTxtFile("下位机关闭CAN接收");

            ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, IsOpen);

        }
        /// <summary>
        /// 导引中获取CAN 报文
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetCANByType(string type)
        {
            try
            {
                // 1. 获取设备快照
                var deviceList = ControlEquipMent?.BMS?.DitEquipMentBase?.Values.ToList();
                if (deviceList == null || !deviceList.Any())
                    return "";

                // 2. 找到 1 号 BMS
                var bmsDevice = deviceList.FirstOrDefault(e => e is emtBMS_GB_DC && e.ChargerID == 1);
                if (bmsDevice == null || !(bmsDevice is emtBMS_GB_DC bmsGbDc))
                    return "";

                // 3. 取 CAN 列表快照（防止集合修改报错）
                var canList = bmsGbDc.CANDATA.ToList();
                if (!canList.Any())
                    return "";

                // 4. 【重点】找到最后一条（最新）匹配的报文
                var latestData = canList.LastOrDefault(c => !string.IsNullOrEmpty(c.MsgText) && c.MsgText.Contains(type));

                return latestData?.MsgData ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 采集判断CP上升下降沿的时间
        /// </summary>
        public void CollectionCPPwm(string CPVoltMin, string CPVoltMax, string CPNegativeMin, string CPNegativeMax, string CPDutyMin, string CPDutyMax, string CPPWMUpMax, string CPPWMDownMax, string sState = "状态3’", string timebase = "400")
        {
            int sleepTime = 50;
            //有的示波器测量值只能添加4个
            if ((Customer != null && Customer.Equals("HYQCP")) || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.SingleOrDefault(e => e.GetType().Name.Equals("emtTekOscilloscope_MDO34")) != null)
            {
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                Thread.Sleep(sleepTime);
                //Oscilloscope_Measure_Initialize之后示波器的波形会清空
                Thread.Sleep(3000);
                bool isRun = false;
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadRun(testWorkParam.lstIDs, ref isRun);
                //不停止采样不准
                if (isRun)
                {
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(5000);
                }
            }

            var dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            Thread.Sleep(1000);
            var Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
            ProcessDataTmp(Data_Tmp, sState, "CP正电压(V)", CPVoltMin.ToString(), CPVoltMax.ToString(), dImgs);
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
            ProcessDataTmp(Data_Tmp, sState, "CP负电压(V)", CPNegativeMin.ToString(), CPNegativeMax.ToString());
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
            if (CPNegativeMin == "-")
                ProcessDataTmp(Data_Tmp, sState, "CP频率(Hz)", "-", "-");
            else
                ProcessDataTmp(Data_Tmp, sState, "CP频率(Hz)", "970", "1030");
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
            ProcessDataTmp(Data_Tmp, sState, "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

            //上升沿下降沿触发闭合，关闭滤波功能
            var lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
            lstConditionState[15] = true;
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
            Thread.Sleep(300);

            //有的示波器测量值只能添加4个
            ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");
            Thread.Sleep(300);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
            Thread.Sleep(1000);
            if ((Customer != null && Customer.Equals("HYQCP")) || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.SingleOrDefault(e => e.GetType().Name.Equals("emtTekOscilloscope_MDO34")) != null)
            {
                //滤波需要重新录波
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");//低值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//上升沿
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//下降沿
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(5000);
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            Thread.Sleep(1000);
            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
            var dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
            }
            //状态3'应该是上升时间最大值=7，如果反映问题就改
            if (CPPWMUpMax == "-")
                ProcessDataTmp(dd, sState, "CP上升时间(us)", "-", CPPWMUpMax.ToString());
            else
                ProcessDataTmp(dd, sState, "CP上升时间(us)", "0", CPPWMUpMax.ToString());

            Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
            }
            if (CPPWMDownMax == "-")
                ProcessDataTmp(dd, sState, "CP下降时间(us)", "-", CPPWMDownMax.ToString(), dImgs);
            else
                ProcessDataTmp(dd, sState, "CP下降时间(us)", "0", CPPWMDownMax.ToString(), dImgs);
            //恢复滤波功能
            lstConditionState[15] = false;
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);

            if ((Customer != null && Customer.Equals("HYQCP")) || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.SingleOrDefault(e => e.GetType().Name.Equals("emtTekOscilloscope_MDO34")) != null)
            {
                //滤波需要重新录波
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, timebase, "0");//低值
                Thread.Sleep(sleepTime);
            }
        }

        /// <summary>
        /// 巴特沃斯低通滤波器计算
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fc"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Tuple<double[], double[]> ButterworthLowPass(double fs, double fc, int order)
        {
            // Normalize the cutoff frequency to the Nyquist frequency (half the sampling rate)
            double wc = 2 * Math.PI * fc / fs;
            double tanWc = Math.Tan(wc / 2);

            // Calculate the poles of the analog prototype filter
            double[] poles = new double[order];
            for (int i = 0; i < order; i++)
            {
                double theta = Math.PI * (2 * i + 1) / (2 * order);
                poles[i] = -Math.Sin(theta) / Math.Cos(theta);
            }

            // Convert poles to biquad coefficients using bilinear transform
            double[] bCoefficients = new double[3]; // Biquad numerator coefficients
            double[] aCoefficients = new double[3]; // Biquad denominator coefficients

            if (order == 2)
            {
                // For a second-order filter, we have one pair of complex conjugate poles
                double poleReal = poles[0];
                double poleImag = Math.Sqrt(1 - poleReal * poleReal);

                double K = 2 / tanWc;
                double denom = 1 + K * poleReal + K * K;

                bCoefficients[0] = K * K / denom;
                bCoefficients[1] = 2 * bCoefficients[0];
                bCoefficients[2] = bCoefficients[0];

                aCoefficients[0] = 1;
                aCoefficients[1] = 2 * (K * K - 1) / denom;
                aCoefficients[2] = (1 - K * poleReal + K * K) / denom;
            }
            else
            {
                //throw new NotImplementedException("Only 2nd order filters are supported in this example.");
                //可以考虑使用Accord.NET框架来简化滤波器的设计过程
            }

            return Tuple.Create(bCoefficients, aCoefficients);
        }

        /// <summary>
        /// 示波器数据进行数字滤波，带宽1k（仅支持二阶滤波）
        /// </summary>
        /// <param name="chnelNum">通道号</param>
        /// <returns></returns>
        public Dictionary<int, double[]> DigitalFilter(int chnelNum)
        {
            Dictionary<int, double[]> retDatas = new Dictionary<int, double[]>();
            // 设定参数
            double samplingRate = 44100; // 假设采样率为44.1kHz
            double cutoffFrequency = 1000; // 截止频率为1kHz
            int filterOrder = 2; // 滤波器阶数

            // 计算Butterworth滤波器系数
            var coefficients = ButterworthLowPass(samplingRate, cutoffFrequency, filterOrder);
            var filter = new IirFilter(coefficients.Item1, coefficients.Item2);

            // 输入数据
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, chnelNum);
            foreach (int item in testWorkParam.lstIDs)
            {
                double[] signal = datas[item];
                // 应用滤波器到信号
                double[] filteredSignal = new double[signal.Length];
                for (int i = 0; i < signal.Length; i++)
                {
                    filteredSignal[i] = filter.ProcessSample(signal[i]);
                }

                retDatas.Add(item, filteredSignal);
            }
            return retDatas;
        }


        #region 测试项目参数加载

        public Dictionary<string, string> LoadTrialItemParams(string sParam)
        {
            try
            {
                if (sParam == null && sParam.Trim() == "")
                {
                    return null;
                }
                string[] sPs = sParam.Split('|');
                Dictionary<string, string> dParams = new Dictionary<string, string>();
                foreach (string sP in sPs)
                {
                    string[] stmps = sP.Split('=');
                    if (stmps.Length > 1)
                    {
                        if (stmps[0].Trim() != "" && stmps[1].Trim() != "")
                        {
                            dParams.Add(stmps[0], stmps[1]);
                        }
                    }
                }

                return dParams;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public double ConvertStringToDouble(string sParam)
        {
            double dtmp = 0;
            double.TryParse(sParam,out dtmp);
            return dtmp;
        }

        public string GetDicValue(Dictionary<string, string> Dics, string sKey, string sDefaultValue = null)
        {
            try
            {
                if (Dics[sKey] == null || Dics[sKey].Trim() == "")
                {
                    return sDefaultValue;
                }
                return Dics[sKey];
            }
            catch (Exception ex)
            {
                return sDefaultValue;
            }
        }

        private void SendStartCharger()
        {
            if (LstChargerInfo.Count < 1) return;
            EquipMentCMD cmd = new EquipMentCMD();
            cmd.EquipMentModel = LstChargerInfo[0].ProductModel;
            cmd.CMDType = 0;//启动命令
            List<EquipMentCMD> cmds = new List<EquipMentCMD>();
            List<CANMsgModel> cANMsgs = new List<CANMsgModel>();
            if (EquipMentCMDManage.SelectEquipMentCMD_EquipMentType_Model(cmd.EquipMentType, cmd.EquipMentModel, out cmds))
            {
                if (cmds != null && cmds.Count > 0)//有命令才执行
                {
                    foreach (EquipMentCMD emc in cmds)
                    {
                        CANMsgModel cmm = new CANMsgModel();
                        cmm.MsgID = emc.CMDID_Can;
                        cmm.MsgData = emc.CMDContent;
                        cANMsgs.Add(cmm);
                    }
                    //SaiTer.ATE.ui ChargerControl.ChargerControl_CAN.Send_Message(cANMsgs);
                }
            }

        }

        #endregion

        /// <summary>
        /// HEX 字符串转字节数组
        /// </summary>
        /// <param name="hexString">HEX字符串（支持大小写、空格/短横线分隔）</param>
        /// <returns>转换后的字节数组</returns>
        /// <exception cref="ArgumentException">格式不合法时抛出</exception>
        public  byte[] HexToByteArray(string hexString)
        {
            // 1. 空值和空字符串处理
            if (string.IsNullOrEmpty(hexString))
                return Array.Empty<byte>();

            // 2. 清理无关字符（空格、短横线）
            string cleanHex = Regex.Replace(hexString, @"[\s\-]", "");

            // 3. 校验长度是否为偶数（2个字符=1个字节）
            if (cleanHex.Length % 2 != 0)
                throw new ArgumentException("HEX字符串长度必须为偶数", nameof(hexString));

            // 4. 校验是否为合法十六进制字符
            if (!Regex.IsMatch(cleanHex, @"^[0-9A-Fa-f]+$"))
                throw new ArgumentException("HEX字符串包含非法字符", nameof(hexString));

            // 5. LINQ 核心逻辑：按每2个字符分组并转换为字节
            var byteArray = Enumerable.Range(0, cleanHex.Length / 2) // 生成索引：0,1,2...（数量=字节数）
                .Select(i =>
                {
                    // 截取当前索引对应的2个字符
                    string hexPair = cleanHex.Substring(i * 2, 2);
                    // 转换为字节（支持大小写）
                    return Convert.ToByte(hexPair, 16);
                })
                .ToArray(); // 转为字节数组

            return byteArray;
        }

    }
}
