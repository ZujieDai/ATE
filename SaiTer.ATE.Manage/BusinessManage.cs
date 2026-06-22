using Newtonsoft.Json;
using SaiTer.ATE.Business;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using SaiTer.ATE.MES;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Manage
{
    public class BusinessManage
    {
        /// <summary>
        /// 是否首次连接使用泰克示波器2020B
        /// </summary>
        private bool isFirst_tek2020B = true;
        /// <summary>
        /// 是否为自动化产线
        /// </summary>
        private bool isAutoTest = false;
        /// <summary>
        /// 安规测试项
        /// </summary>
        private List<EmTrialType> lstSafetyTrialTypes;

        /// <summary>
        /// 倒计时弹窗结果
        /// </summary>
        public bool CountDownResult { get; set; }
        /// <summary>
        /// 录波仪点数
        /// </summary>
        int ReadPointsOscillographInstrument = 500000;
        ///// <summary>
        ///// 当前枪所使用的测试方案名称
        ///// </summary>
        // public string SchemeName = "";
        /// <summary>
        /// 当前正在检测的项目在表格中的顺序号
        /// </summary>
        public int TrialItemOrderID = -1;
        /// <summary>
        /// 当前在检的试验项
        /// </summary>
        public StTrialItem NowTrialScheme;
        /// <summary>
        /// 检测项目信息集合
        /// </summary>
        public List<StTrialItem> lstTrialItemsInfo = new List<StTrialItem>();
        /// <summary>
        /// 待测充电枪信息集合
        /// </summary>
        public List<ChargerInfoModel> lstChargerInfo = null;
        /// <summary>
        /// 业务管理对象
        /// </summary>
        private static BusinessManage _BusinessControlManage = null;
        /// <summary>
        /// 配置文件和反射对象
        /// </summary>
        public XmlInfoAndAssembly _xmlInfoAssembly = null;

        /// <summary>
        /// 界面操作对象
        /// </summary>
        public InterfaceOperate _interfaceOperate = null;

        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static BusinessManage GetInstance()
        {
            if (_BusinessControlManage == null)
            {
                _BusinessControlManage = new BusinessManage();
            }
            return _BusinessControlManage;
        }

        /// <summary>
        /// 关闭全部设备
        /// </summary>
        bool ExitAllE = false;

        //录波仪测试项需要初始化
        private int[] OscillographEmTrialTypes = new int[] { 1220002, 1220003, 1220004, 1220005, 1220006, 3220042 };

        /// <summary>
        /// 构造函数
        /// </summary>
        private BusinessManage()
        {
            try
            {
                //实例配制文件读取类和反射类
                _xmlInfoAssembly = XmlInfoAndAssembly.GetInstance();
                _xmlInfoAssembly.SetConfigInfoToAssembly();
                // 对业务对象赋值相对应的操作
                _xmlInfoAssembly.SetBusinessControl();
                //加载业务类
                _interfaceOperate = InterfaceOperate.GetInstance(_xmlInfoAssembly);
                //lstChargerInfo = _xmlInfoAssembly.lstCharger;
                //lstChargerInfo = lstChargerInfo.OrderBy(s => s.ChargerId).ToList();
                LoadChargerInfo();
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.SetChargerInfo(lstChargerInfo); 
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 加载试验数据到界面显示
        /// </summary>
        /// <returns></returns>
        public void AddTrialData()
        {
            try
            {
                List<TrialDataModel> lstTrialData = TrialItemDataTmpManage.GetTrialData_ALL(lstChargerInfo);
                //if (lstTrialData != null && lstTrialData.Count > 0)
                {
                    SystemEvent.SendTrialDataToUIEvent(lstTrialData);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public void GetTrialScheme(string SchemeName)
        {
            lstTrialItemsInfo.Clear();
            TrialItemsManage.GetTrialSchemeFromSchemeName(SchemeName, ref lstTrialItemsInfo);
        }



        /// <summary>
        /// 加载充电枪信息
        /// </summary>
        public void LoadChargerInfo()
        {
            try
            {
                if (ChargerInfoManage.SelectChargerInfo(out lstChargerInfo))
                {
                    lstChargerInfo = lstChargerInfo.OrderBy(s => s.ChargerId).ToList();
                }
                else
                {
                    Log.Log.LogException("加载充电枪信息错误");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 开始多个项目检测
        /// </summary>
        /// <param name="lstTrialScheme">方案集合</param>
        public async void StartTrialTest(List<StTrialItem> lstTrialScheme, CancellationTokenSource cst = null)
        {
            List<int> lstID = GetListChargerID();

            try
            {
                //可能需要切换欧美标的枪号，因为交流的国欧标分开主板了
                //var bmsAddressStrs = EquipmentConfigManage.GetConfigParams((int)lstChargerInfo.First().ChargerType, "Equip_Address", "BMS");
                //if(bmsAddressStrs!= null)
                //{
                //    var bmsList = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.DitEquipMentBase.Where(kv => kv.Value is emtBMS_AC).Select(kv => kv.Value as emtBMS_AC).ToList();
                //    foreach(var equip_bms in bmsList)
                //    {
                //        foreach(string[] address in bmsAddressStrs)
                //        {
                //            //找到对应枪号的导引，检查地址是否一致，不一致的需要修改
                //            if(address[1].Split('=')[1] == equip_bms.ChargerID.ToString())
                //            {
                //                if (address[0].Equals(equip_bms.EquipMentPort.PortName + "_" + equip_bms.EquipMentPort.PortParams))
                //                {
                //                    equip_bms.EquipMentPort.Close();

                //                }

                //            }
                //        }
                //    }
                //}
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Yellow);
                SystemEvent.SendTrialResult(EmTrialResult.Testing);
                Thread.Sleep(300);
                lstSafetyTrialTypes = new List<EmTrialType>() {
                         EmTrialType.绝缘电阻_输入对输出 ,
                         EmTrialType.绝缘电阻_输入对地 ,
                         EmTrialType.绝缘电阻_输出对地,
                         EmTrialType.交流耐压_输入对输出,
                         EmTrialType.交流耐压_输入对地 ,
                         EmTrialType.交流耐压_输出对地,
                         EmTrialType.直流耐压_输入对输出 ,
                         EmTrialType.直流耐压_输入对地 ,
                         EmTrialType.直流耐压_输出对地 ,
                         EmTrialType.接地试验1,
                         EmTrialType.接地试验2,
                         EmTrialType.接地试验3,
                         EmTrialType.绝缘电阻,
                         EmTrialType.交流耐压,
                         EmTrialType.直流耐压,
                         EmTrialType.接地试验,
                         EmTrialType.GB_PT_DC_InsulationResistance,
                         EmTrialType.GB_PT_DC_DielectricStrength,
                         EmTrialType.GB_PT_DC_EarthingConductor1,
                         EmTrialType.GB_PT_DC_EarthingConductor2,
                         EmTrialType.GB_PT_DC_EarthingConductor3,

                };


                //第一个测试项不是安规项就启动交流源
                if (lstTrialScheme.Count > 0 && !lstSafetyTrialTypes.Contains(lstTrialScheme[0].TrialType))
                {
                    if (lstChargerInfo[0].ChargerType != EmChargerType.Charger_NTGX_CN && _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource != null)//储能的不用开交流源
                    {
                        SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 正在启动交流源输出，请等待。");
                        double voltage = 0;
                        if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            voltage = lstChargerInfo[0].NominalVoltage;
                        }
                        else
                        {
                            voltage = 220;
                        }
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_SetVolt(lstID, voltage);
                        Thread.Sleep(1000);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_ON(lstID);
                        // Thread.Sleep(5000);
                        if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                        {
                            //等待直流桩开机
                            Thread.Sleep(5000);
                        }
                    }
                }
                else
                {
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                }
                if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                {
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS?.BMSSetHCAC(lstID, lstChargerInfo[0].ChargerType);
                }

                //欧美标直流桩需要切换
                if ((AllEquipStateData.DicBMS_EU_DC_StateData != null && AllEquipStateData.DicBMS_EU_DC_StateData.Count > 0)
                    || (AllEquipStateData.DicBMS_USA_DC_StateData != null && AllEquipStateData.DicBMS_USA_DC_StateData.Count > 0))
                {
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS?.BMSSetHCAC(lstID, lstChargerInfo[0].ChargerType);
                }

                if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC
                    || lstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC
                    || lstChargerInfo[0].ChargerType == EmChargerType.Charger_NACS_DC)
                {
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS?.BMSSetHCAC(lstID, lstChargerInfo[0].ChargerType);
                }
                string isControlDIO_AC = ConfigurationManager.AppSettings["isControlDIO_AC"];
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if ((Customer != null && (Customer.Contains("LJ") || Customer.Contains("HYQCP")))
                    || (isControlDIO_AC != null && Convert.ToBoolean(isControlDIO_AC)))
                {
                    //PE控制默认闭合
                    var lstRelay = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.ControlBoardReadState(lstID);
                    lstRelay[14] = true;
                    //TS国标CP采样闭合K13，欧标CP采样断开K13
                    if (Customer.Contains("TS"))
                    {
                        //勾选测试国标枪
                        if (lstChargerInfo.Find(s => s.IsCheck && s.ChargerType == EmChargerType.Charger_GB_AC) != null)
                            lstRelay[12] = true;
                        else
                            lstRelay[12] = false;
                        //TS是默认断开继电器是PE未断线
                        lstRelay[14] = false;
                    }
                    else if(Customer.Contains("SKY"))
                    {
                        //SKY是默认断开继电器是PE未断线
                        lstRelay[14] = false;
                    }
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.ControlResistanceSetRelay(lstID, lstRelay);

                    if (isFirst_tek2020B && Customer.Contains("HYQCP"))
                    {
                        var tek2020B = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscilloscope;
                        if (tek2020B != null && tek2020B.DitEquipMentBase.Values.FirstOrDefault(e => e is emtTekOscilloscope) != null)
                        {
                            //交流输出电流需要校准调零
                            tek2020B.Oscilloscope_AutoZero(lstID, 2);
                            isFirst_tek2020B = false;
                        }
                    }
                }
                else if (Customer != null && Customer.Contains("GJ"))
                {
                    if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                    {
                        bool[] State = new bool[16];
                        State[5] = true;
                        State[6] = true;
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.ControlResistanceSetRelay(State.ToList());
                    }
                    else
                    {
                        bool[] State = new bool[16];
                        State[6] = true;
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.ControlResistanceSetRelay(State.ToList());
                    }


                }
                else if (Customer != null && Customer.Contains("HK"))
                {
                    bool[] State = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.ControlBoardReadState().ToArray();
                    State[8] = true;
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.ControlResistanceSetRelay(State.ToList());
                }
                //惠州TB新安规仪SE7441需要先检查初始化
                else if (Customer != null && Customer.Contains("TB"))
                {
                    var safety = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Safety;
                    if (safety != null && safety.DitEquipMentBase.First().Value is emtSafety_SE7441) 
                    {
                        SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 正在检查安规仪的测试方案，请等待。");
                        //通过测试方案获取安规方案的信息
                        string schemeName = lstChargerInfo.FirstOrDefault()?.SchemeName;
                        List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
                        SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo);
                        int SchemeId = lstSchemeInfo.FirstOrDefault(s => s.SchemeName.Equals(schemeName)).SchemeID;
                        var EquipmentConfig_Scheme = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441") 
                            && s.ConfigType.Equals("Safety_Scheme") && s.ChargerType == SchemeId);
                        if(EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Params") && s.ChargerType == SchemeId) == null)
                        {
                            SystemEvent.SendCountDownTimer("请在设备操作界面先保存安规配置后再进行测试！", 99999, 0);
                            EndTrial();
                            SystemEvent.SetUIButton(true);
                            return;
                        }
                        if (safety.SafetyInit(lstID, EquipmentConfig_Scheme.Params1, EquipmentConfig_Scheme.Params2))
                            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 安规仪初始化成功。");
                        //else
                        //    SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 安规仪初始化失败。");
                        safety.SafetySetParam(lstID, "FL " + EquipmentConfig_Scheme.Params1, "\n", "\n", 0);
                    }
                }
                else if (Customer != null && Customer.Contains("GX"))
                {
                    if (lstChargerInfo[0].ChargerType != 0
                        && lstChargerInfo[0].ChargerType != EmChargerType.Charger_NTGX_CN)//目前充电桩固定中盛板Y2闭合其他断开，后面储能测试还需要添加Y1闭合其他断开
                    {
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(0, false);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(1, true);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(2, false);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(3, false);
                    }
                    else
                    {
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(0, true);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(1, false);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(2, false);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetRelaySwitch(3, false);
                    }
                }
                else if(Customer != null && Customer.Contains("YKR"))
                {
                    //YKR需要通过插枪信号判断负载的投切
                    _xmlInfoAssembly._EquipMentControl.FeedbackLoad.FeedbackLoad_BMSON(lstID);
                }

                var bms = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS;
                if (bms != null && bms.DitEquipMentBase.Values.FirstOrDefault(e => e is emtBMS_AC) != null && !Customer.Equals("TB"))
                {
                    bms.BMS_SetResistance(lstID, 1300, 2740);
                }
                if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC && AllEquipStateData.DicBMS_EU_DC_StateData != null
                    && AllEquipStateData.DicBMS_EU_DC_StateData.Count > 0 && AllEquipStateData.DicBMS_EU_DC_StateData.ContainsKey(lstID[0]))
                {
                    string BMSInfo = AllEquipStateData.DicBMS_EU_DC_StateData[lstID[0]]?.SystemState;

                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.SetParameter(lstID, 390, lstChargerInfo[0].NominalVoltage, lstChargerInfo[0].MaxAllowChargeCurrent);

                    if (!string.IsNullOrEmpty(BMSInfo) && !BMSInfo.Contains("CurrentDemandReq") && !BMSInfo.Contains("CurrentDemandRes"))
                    {

                        bool[] Ks = new bool[24];
                        Ks[0] = true;//DC+DC-控制
                        Ks[1] = true;//CC信号控制
                        Ks[2] = false;//CP信号控制
                        Ks[4] = true;//PE信号控制
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMSSetKState_EU_DC(lstID, 390, Ks.ToArray(), 0, 0, "0");



                        Thread.Sleep(500);
                        Ks[2] = true;//CP信号控制
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMSSetKState_EU_DC(lstID, 390, Ks.ToArray(), 0, 0, "0");


                    }
                }
                // 模拟插拔枪
                if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC && _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS != null)
                {
                    SetCPReresh();
                }


                int SafetyIndex = lstTrialScheme.FindIndex(s => lstSafetyTrialTypes.Contains(s.TrialType));

                int OscillographInstrumentIndex = lstTrialScheme.FindIndex(c => (Convert.ToInt32(c.TrialType) >= 50000 && Convert.ToInt32(c.TrialType) < 59999)
                || OscillographEmTrialTypes.Contains(Convert.ToInt32(c.TrialType)));
                //开始前先清空检测方案表格右边对应的检定结果
                for (int i = 0; i < lstTrialScheme.Count; i++)
                {
                    int TrialIndex = lstTrialItemsInfo.FindIndex(s => s.TrialType == lstTrialScheme[i].TrialType && s.ItemName == lstTrialScheme[i].ItemName);
                    for (int j = 0; j < lstChargerInfo.Count; j++)
                    {
                        if (lstChargerInfo[j].IsCheck)
                            SystemEvent.SendTrialResultToUI(new TrialDataModel(), lstChargerInfo[j].ChargerId, true, TrialIndex);
                    }
                }
                //重新检测时，是否删除之前检测所有的临时数据
                string strDeleteData = ConfigurationManager.AppSettings["isDeleteData"];
                if (strDeleteData != null)
                {
                    bool isDeleteData = bool.Parse(strDeleteData);
                    if (isDeleteData)
                    {
                        for (int i = 0; i < lstChargerInfo.Count; i++)
                        {
                            TrialItemResultTmpManage.DeleteTrialData(lstChargerInfo[i].PKID.ToString());
                            TrialItemDataTmpManage.DeleteTrialData(lstChargerInfo[i].PKID.ToString());
                        }
                    }
                }
                //自动化产线属性
                string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
                if (strAutoTest != null)
                {
                    isAutoTest = bool.Parse(strAutoTest);
                }
                for (int i = 0; i < lstTrialScheme.Count; i++)
                {
                    //安全模式下停止检测
                    if (cst != null && cst.Token.IsCancellationRequested)
                        break;

                    int index = lstTrialItemsInfo.FindIndex(s => s.TrialType == lstTrialScheme[i].TrialType && s.ItemName == lstTrialScheme[i].ItemName);
                    if (index >= 0)
                    {
                        TrialItemOrderID = index;
                        SystemEvent.SwitchCheckItemIndex(index);
                    }
                    //查找安规测试项
                    if (i == SafetyIndex)
                    {
                        if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource != null)
                        {
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                            string info = "请断开充电桩输入动力线缆，\r\n确认与安规设备接线符合要求后，点击【确定】按钮继续";
                            SystemEvent.SendCountDownTimer(info, 99999, 0);
                        }
                    }
                    //查找录波仪测试项
                    if (i == OscillographInstrumentIndex)
                    {
                        SystemEvent.SendCountDownTimerResultEvent += SystemEvent_SendCountDownTimerResultEvent;
                        SystemEvent.SendCountDownTimer("请确认已增加K1K2线圈电压检测，K1K2前端高压，急停外部接线（需要接线请点击是，已接线请点否）", 20, 1);
                        if (CountDownResult)
                        {
                            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "关闭交流源，开始接线...");
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                            SystemEvent.SendCountDownTimer("请增加K1K2线圈电压检测，K1K2前端高压，急停外部接线\r\n确认与设备接线符合要求后，点击【确定】按钮继续", 99999, 0);
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_ON(lstID);
                            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "接线完成，开启交流源...");
                        }
                        SystemEvent.SendCountDownTimerResultEvent -= SystemEvent_SendCountDownTimerResultEvent;

                        string info = "是否需要进行录波仪全局初始化？注：此过程耗时约3分钟，如果上一次初始化过后没有手动修改过录波仪设置，可以跳过这一步";
                        SystemEvent.SendCountDownTimerResultEvent += SystemEvent_SendCountDownTimerResultEvent;
                        SystemEvent.SendCountDownTimer(info, 20, 1);
                        if (CountDownResult)
                        {
                            Console.WriteLine("开始全局录波仪初始化" + DateTime.Now);
                            SystemEvent.MessageInfo(true, "正在初始化录波仪全局配置，请耐心等待...");
                            OscillographInstrumentInitialization();
                            SystemEvent.MessageInfo(false, "");
                            Console.WriteLine("录波仪初始化完成" + DateTime.Now);
                        }
                        SystemEvent.SendCountDownTimerResultEvent -= SystemEvent_SendCountDownTimerResultEvent;
                    }
                    //判断哪些测试项需要初始化录波仪档位
                    if ((int)lstTrialScheme[0].TrialType >= 50000 && (int)lstTrialScheme[0].TrialType <= 59999)
                    {
                        SystemEvent.MessageInfo(true, "正在初始化录波仪档位和位置配置，请耐心等待...");
                        OscillographInstrumentInitGearPosition();
                        SystemEvent.MessageInfo(false, "");
                    }
                    //待机功耗测试项需要动作继电器
                    if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.DitEquipMentBase.Where(e => e.Value.EquipMentClassName.Equals("emtDIORelay")).ToArray().Length > 0)
                    {
                        if ((Customer.Contains("LJ") || Customer.Contains("HYQCP") || Customer.Contains("WR")) || (isControlDIO_AC != null && Convert.ToBoolean(isControlDIO_AC)))
                        {
                            //TS没有待机功耗，Y1Y2为单三相切换
                            if (!Customer.Equals("TS") && !Customer.Equals("SKY"))
                            {
                                //查看是否需要待机功耗切换
                                string txt = ConfigurationManager.AppSettings["DIORelayText"];
                                if (string.IsNullOrEmpty(txt) || txt.Contains("待机功耗"))
                                {
                                    if (!Customer.Contains("YTZL_ACDC"))//YTZL交直流一体测试系统这里没有操作
                                    {
                                        //深圳LJ单开K5：ST-PTAC-GB导引模块带载接触器闭合(K5与中盛的DIO模块的输出Y3或者Y4互锁
                                        //SystemEvent.SendCountDownTimer("请确认当前负载已经关闭，带载控制程控板有损坏设备的风险", 999, 0);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ResistanceLoad.ResistanceLoad_OFF(lstID);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMS_OFF(lstID);
                                        Thread.Sleep(200);
                                        var list = _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.ControlBoardReadState(lstID);
                                        list[4] = false;
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.ControlResistanceSetRelay(list);
                                        Thread.Sleep(500);
                                    }

                                    //深圳LJ待机功耗测试需要操作DIO继电器
                                    //1.Y1控制KM3、KM6接触器闭合(待机功耗测试)，X1是KM3、KM6闭合反馈
                                    //2.Y2控制KM1~2、KM4~5接触器闭合(除待机功耗测试外),X2是KM1~2、KM4~5闭合反馈
                                    if (lstTrialScheme[i].TrialType == EmTrialType.待机功耗测试)
                                    {
                                        //先关闭交流源再切换继电器
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource.ACSource_OFF(lstID);
                                        Thread.Sleep(1000);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, false);
                                        Thread.Sleep(300);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(2, false);
                                        Thread.Sleep(300);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(3, false);
                                        Thread.Sleep(300);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, true);
                                        Thread.Sleep(300);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource.ACSource_ON(lstID);
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, false);
                                        Thread.Sleep(300);
                                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, true);
                                    }
                                }
                                if (string.IsNullOrEmpty(txt) || txt.Contains("源输入"))
                                {
                                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(2, true);
                                }
                            }
                        }
                        //ZLX除了交流源相关测试，别的测试项都得用电网输入，因为交流源的功率小
                        else if (Customer.Contains("ZLX"))
                        {
                            if(lstTrialScheme[i].TrialType == EmTrialType.GB_PT_DC_InputOverVoltProtection || 
                                lstTrialScheme[i].TrialType == EmTrialType.GB_PT_DC_InputUnderVoltProtection ||
                                lstTrialScheme[i].TrialType == EmTrialType.GB_PT_DC_PowerInputLost)
                            {
                                //Y1电网 Y2交流源
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, false);
                                Thread.Sleep(300);
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, true);
                                Thread.Sleep(300);
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource.ACSource_ON(lstID);
                                Thread.Sleep(1000);
                            }
                            else if(i - 1 >= 0)
                            {
                                if (lstTrialScheme[i - 1].TrialType == EmTrialType.GB_PT_DC_InputOverVoltProtection ||
                                lstTrialScheme[i - 1].TrialType == EmTrialType.GB_PT_DC_InputUnderVoltProtection ||
                                lstTrialScheme[i - 1].TrialType == EmTrialType.GB_PT_DC_PowerInputLost)
                                {
                                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, false);
                                    Thread.Sleep(300);
                                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, true);
                                    Thread.Sleep(300);
                                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, false);
                                Thread.Sleep(300);
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, true);
                                Thread.Sleep(300);
                                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    if (!StartScheme(lstTrialScheme[i]))
                    {
                        bool isReTestOK = false;
                        //if (isAutoTest)
                        //{
                        //    for (int r = 0; r < 2; r++)
                        //    {
                        //        try
                        //        {
                        //            Log.Log.LogMessage($"测试未通过，重测第{r + 1}次");
                        //            isReTestOK = StartScheme(lstTrialScheme[i]);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            Log.Log.LogException($"测试未通过，重测第{r + 1}次出现异常", ex);
                        //        }
                        //    }
                        //}
                        if (!isReTestOK && isAutoTest)
                        {
                            Log.Log.LogMessage("检测出现异常，停止检测");
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Red);
                            Thread.Sleep(500);
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Close);

                            break;
                        }
                    }
                }
                SystemEvent.SetUIButton(false, "false");//禁用停止检测按钮
                for (int i = 0; i < lstChargerInfo.Count; i++)
                {
                    string testRes;
                    if (CheckChargerFinalResult(lstChargerInfo[i].PKID))
                    {
                        lstChargerInfo[i].CheckResult = EmTrialResult.Pass;
                        testRes = "1";
                    }
                    else
                    {
                        lstChargerInfo[i].CheckResult = EmTrialResult.Fail;
                        testRes = "2";
                    }
                    if (isAutoTest)
                    {
                        string testData = "";
                        //需要从数据库查询
                        List<TrialDataModel> LstTrialData = TrialItemDataTmpManage.GetTrialData_ALL(lstChargerInfo.FindAll(c => c.PKID == lstChargerInfo[i].PKID));
                        bool isFirst_testData = true;
                        foreach (var item in LstTrialData)
                        {
                            if (isFirst_testData)
                                isFirst_testData = false;
                            else
                                testData += ",";
                            string status = item.ExtentData.Split('|')[0];
                            string name = item.ExtentData.Split('|')[1];
                            string value = item.ExtentData.Split('|')[4];
                            string sMin = item.ExtentData.Split('|')[2];
                            string sMax = item.ExtentData.Split('|')[3];
                            string sResult = (item.TrialResult == EmTrialResult.Pass || item.TrialResult == EmTrialResult.NA) ? "PASS" : "FAIL";

                            //直流测试难修改去掉一段，不然字节过长
                            string PostMESMode = ConfigurationManager.AppSettings["PostMESMode"];
                            if (PostMESMode != null && PostMESMode.Contains("1"))
                                testData += $"{status}({name}):{value}:{sMin}~{sMax}:{sResult}";
                            else if (item.TrialType >= EmTrialType.绝缘电阻_输入对输出 && item.TrialType <= EmTrialType.接地试验)
                                testData += $"{item.TrialName}:{value}:{sMin}~{sMax}:{sResult}";
                            else
                                testData += $"{item.TrialName}_{status}({name}):{value}:{sMin}~{sMax}:{sResult}";
                        }
                        string json = JsonConvert.SerializeObject(new
                        {
                            MasterMaterialBarCode = TBHttpMESAutomation.GetInstance().MasterMaterialBarCode,
                            Data = new
                            {
                                TestProgramName = TBHttpMESAutomation.GetInstance().TestProgramName,
                                TestData = testData,
                                TestResult = testRes
                            }
                        });
                        //HttpBase.HttpPostAsyn(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/testresult", json);
                        HttpBase.HttpPost(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/testresult", json);
                        //MessageBox.Show(res);
                        SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 已发送测试结果");
                    }
                }


                if (lstTrialScheme.Count > 0)
                {
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS?.BMS_OFF(lstID);
                    if (!Customer.Equals("XJ") && !Customer.Equals("DH") && !Customer.Equals("CJB"))//XJ客户做完了不断电源
                    {
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_OFF(lstID);
                    }
                    Thread.Sleep(200);
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Close);

                }

                //bool.TryParse(ConfigurationManager.AppSettings["isTestQuality"], out bool isTestQuality);
                //if (isTestQuality)
                {
                    SystemEvent.SendTrialState(EmTrialState.End);
                    for (int i = 0; i < lstChargerInfo.Count; i++)
                    {
                        ChargerInfoManage.UpdateChargerTrialResult(lstChargerInfo[i].PKID.ToString());
                    }
                }
                EndTrial();
                //SystemEvent.SetUIButton(true);
            }
            catch (Exception ex)
            {
                try
                {
                    //此处用以解决按下停止按钮中止线程后引发的BUG,未找到更好的办法之前，勿轻易修改此处逻辑
                    SystemEvent.SetUIButton(false);//禁用除停止检测以外的按钮   
                    SystemEvent.SetUIButton(false, "false");//禁用停止检测按钮
                    SystemEvent.MessageInfo(false, "");
                    if (!ex.Message.Contains("正在中止线程"))
                    {
                        Thread.Sleep(200);
                        Log.Log.LogException(ex);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Red);
                        Thread.Sleep(500);
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard?.SetLightColor(EmLightColor.Close);
                    }

                    SystemEvent.SendTrialResult(EmTrialResult.Wait);

                    EndTrial();
                    //SystemEvent.SetUIButton(true);
                    //SystemEvent.SetUIButton(true, "true");
                    bool.TryParse(ConfigurationManager.AppSettings["isTestQuality"], out bool isTestQuality);
                    if (isTestQuality)
                    {
                        SystemEvent.SendTrialState(EmTrialState.End);
                        for (int i = 0; i < lstChargerInfo.Count; i++)
                        {
                            ChargerInfoManage.UpdateChargerTrialResult(lstChargerInfo[i].PKID.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Log.LogException(e);
                }
            }

        }
        private void SystemEvent_SendCountDownTimerResultEvent(bool result)
        {
            CountDownResult = result;
        }

        /// <summary>
        /// 挡位初始化
        /// </summary>
        public void OscillographInstrumentInitGearPosition()
        {

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_TimeBase("0.1");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(1, "40", "-4");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(2, "4", "-4");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(3, "2", "-2");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(4, "250", "0");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(5, "2", "-2");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(6, "5", "0");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(7, "5", "0");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear(8, "250", "0");//
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 1, "50", "-450");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 2, "50", "-450");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 3, "90", "-10");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 4, "9", "-1");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 5, "9", "-1");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 6, "9", "-1");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 7, "110", "-90");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_SetGear_CAN(15, 8, "1", "0");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetGroup3(1, 1, false);
        }
        private void OscillographInstrumentInitialization()
        {
            List<int> lstIDs = GetListChargerID();

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_TimeBase("0.1");
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.OscillographIDefalut();//示波器初始化
            System.Threading.Thread.Sleep(3000);

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Remove_All_Channel(1);//示波器初始化
                                                                                                        // System.Threading.Thread.Sleep(200);
            //电阻载不需要滤波，滤波会使得电压电流下降变慢
            if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ResistanceLoad != null)
            {
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 4, 4, true, "DC", "FULL", "5000", _xmlInfoAssembly._systemXmlInfo.Channle1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4

                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "FULL", "5000", _xmlInfoAssembly._systemXmlInfo.Channle2, "DC-out-I", "A", false, "40", "-4", 0, 0, 0, "200", "BLUE", true, false, false, false);//通道1
            }
            else
            {
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 4, 4, true, "DC", "5000", "5000", _xmlInfoAssembly._systemXmlInfo.Channle1, "DC-out-V", "V", false, "250", "0", 0, 0, 0, "0", "DBLue", true, false, false, false);//通道4

                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "50", "5000", _xmlInfoAssembly._systemXmlInfo.Channle2, "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200", "BLUE", true, false, false, false);//通道1
            }
            //通道1输出电流的变比可能是300或者录波仪不支持的档位，需要计算
            var oscillographCH1Config = EquipmentConfigManage.GetConfigParams(1, "Oscillograph_STRain_LSCale", "Oscillograph", "");
            if (oscillographCH1Config != null && oscillographCH1Config.Length >= 3)
            {
                SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 开始设置录波仪通道1线性标尺");
                if (double.TryParse(oscillographCH1Config[0], out double AVALue))
                {
                    if (double.TryParse(oscillographCH1Config[1], out double BVALue))
                    {
                        int probe = Convert.ToInt32(_xmlInfoAssembly._systemXmlInfo.Channle2) / (int)AVALue;
                        if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ResistanceLoad != null)
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "FULL", "5000", probe.ToString(), "DC-out-I", "A", false, "40", "-4", 0, 0, 0, "200", "BLUE", true, false, false, false);//通道1
                        else
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 1, 1, true, "DC", "50", "5000", probe.ToString(), "DC-out-I", "A", false, "40", "-4", 1, 1, 2, "200", "BLUE", true, false, false, false);//通道1
                        _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_STRain_LSCale(1, "AXB", AVALue, BVALue, "A", "FLOating");
                    }
                }
            }

            // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 2, 2, true, "DC", "5000", "5000", "1", "K3K4-AUX-V", "V", false, "4", "-4", 0, 0, 0, "0", "BGReen", true, false, false, false);//通道2
                                                                                                                                                                                                                                                  // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 3, 3, true, "DC", "5000", "5000", "1", "CC1", "V", false, "2", "-2", 0, 0, 0, "0", "CYAN", true, false, false, false);//通道3 
                                                                                                                                                                                                                                               // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 5, 5, true, "DC", "5000", "5000", "1", "CC2", "V", false, "2", "-2", 0, 0, 0, "0", "GRAY", true, false, false, false);//通道5
                                                                                                                                                                                                                                         // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 6, 6, true, "DC", "5000", "5000", "1", "E-stop", "V", false, "5", "0", 0, 0, 0, "0", "GREen", true, false, false, false);//通道6
            // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 7, 7, true, "DC", "5000", "5000", "1", "K1K2-sig", "V", false, "5", "0", 0, 0, 0, "0", "LBLue", true, false, false, false);//通道7
            //System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_Channel_Set(lstIDs, 8, 8, true, "DC", "5000", "5000", "500", "K1K2-front-V", "V", false, "250", "0", 0, 0, 0, "0", "LGReen", true, false, false, false);//通道8
            //System.Threading.Thread.Sleep(200);

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCan_Channel(15, "250000", true, false);
            // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 1, 9, true, "5000", "BCL-I", 0, "181056F4", "Auto", 16, 16, 0, 0, "0.1", "-400", "A", "50", "-450", "MAGenta", true, false, false, false);//CAN子通道1
                                                                                                                                                                                                                                                                 // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 2, 10, true, "5000", "CCS-I", 0, "1812F456", "Auto", 16, 16, 0, 0, "0.1", "-400", "A", "50", "-450", "MGReen", true, false, false, false);//CAN子通道2
                                                                                                                                                                                                                                                                 // System.Threading.Thread.Sleep(200);
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 3, 11, true, "5000", "BST-all", 0, "101956F4", "Auto", 0, 32, 0, 0, "1", "0", "", "90", "-10", "ORANge", true, false, false, false);//CAN子通道3


            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 4, 12, true, "5000", "CST-V-error", 0, "101AF456", "Auto", 26, 2, 0, 0, "1", "0", "", "9", "-1", "PINK", true, false, false, false);//CAN子通道4

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 5, 13, true, "5000", "CST-fault", 0, "101AF456", "Auto", 4, 2, 0, 0, "1", "0", "", "9", "-1", "PURPle", true, false, false, false);//CAN子通道5
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 6, 14, true, "5000", "CST-fault-con", 0, "101AF456", "Auto", 10, 2, 0, 0, "1", "0", "", "9", "-1", "RED", true, false, false, false);//CAN子通道6
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 7, 15, true, "5000", "BSD-SOC", 0, "181C56F4", "Auto", 0, 8, 0, 0, "1", "0", "%", "110", "-90", "SPINk", true, false, false, false);//CAN子通道7
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 8, 16, true, "5000", "CSD-energy", 0, "181DF456", "Auto", 16, 16, 0, 0, "0.1", "0", "kWh", "1", "0", "YELLow", true, false, false, false);//CAN子通道8
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 9, 17, true, "5000", "BHM-V", 0, "182756F4", "Auto", 0, 16, 0, 0, "0.1", "0", "V", "1200", "-300", "BLUE", true, false, false, false);//CAN子通道9
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 10, 18, true, "5000", "CRM-NUM", 0, "1801F456", "Auto", 8, 32, 0, 0, "1", "0", "", "90", "-10", "BGReen", true, false, false, false);//CAN子通道10
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 11, 19, true, "5000", "BMS-CAN-PGN", 0, "1CEC56F4", "Auto", 48, 8, 0, 0, "1", "0", "", "32", "-8", "CYAN", true, false, false, false);//CAN子通道11
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 12, 20, true, "5000", "BRO-AA", 0, "100956F4", "Auto", 0, 8, 0, 0, "1", "0", "", "190", "-10", "DBLue", true, false, false, false);//CAN子通道12
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 13, 21, true, "5000", "CRO-AA", 0, "100AF456", "Auto", 0, 8, 0, 0, "1", "0", "", "190", "-10", "GRAY", true, false, false, false);//CAN子通道13
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 14, 22, true, "5000", "CST-bms-stop", 0, "101AF456", "Auto", 6, 2, 0, 0, "1", "0", "", "9", "-1", "GREen", true, false, false, false);//CAN子通道14
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 15, 23, true, "5000", "CST-bms-SOC", 0, "101AF456", "Auto", 0, 2, 0, 0, "1", "0", "", "9", "-1", "LBLue", true, false, false, false);//CAN子通道15
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 16, 24, true, "5000", "CEM-BCL-timeout", 0, "081FF456", "Auto", 18, 2, 0, 0, "1", "0", "", "9", "-1", "LGReen", true, false, false, false);//CAN子通道16
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 17, 25, true, "5000", "CHM-verison", 0, "1826F456", "Auto", 0, 24, 0, 0, "1", "0", "", "400", "-100", "MAGenta", true, false, false, false);//CAN子通道16
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 18, 26, true, "5000", "CRM-AA", 0, "1801F456", "Auto", 0, 8, 0, 0, "1", "0", "", "190", "-10", "MGReen", true, false, false, false);//CAN子通道16
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 19, 27, true, "5000", "CTS-year", 0, "1807F456", "Auto", 40, 4, 0, 0, "1", "0", "year", "9", "-1", "ORANge", true, false, false, false);//CAN子通道16
            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetCanChild_Channel(15, 20, 28, true, "5000", "CML-max-V", 0, "1808F456", "Auto", 0, 16, 0, 0, "0.1", "0", "", "1200", "-300", "PINK", true, false, false, false);//CAN子通道16
                                                                                                                                                                                                                                                                 // System.Threading.Thread.Sleep(200);


            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_MEASureOpen(true);

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_SetGroup(1, 1, 0);

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.OscillographIDefalut_Show();

            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Oscillograph?.Oscillograph_StorageDepth(ReadPointsOscillographInstrument.ToString());


        }

        private void EndTrial()
        {
            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 测试项结束，关闭所有设备");

            int time = 15;
            OFFAllEquip();
            while (time-- > 0)
            {
                if (ExitAllE)
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            if (_xmlInfoAssembly._systemXmlInfo.IsAutoSaveTrialData)
            {
                SaveFormalData();
            }
        }
        private bool CheckChargerFinalResult(long PKID)
        {
            bool result = false;
            try
            {
                List<string> lstResult = TrialItemResultTmpManage.GetAllTrialResult(PKID.ToString());
                int index = lstResult.FindIndex(s => s.ToUpper().Equals("FAIL"));
                if (index < 0)
                {
                    SystemEvent.SendTrialResult(EmTrialResult.Pass);
                    result = true;
                }
                else
                {
                    SystemEvent.SendTrialResult(EmTrialResult.Fail);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return result;
        }


        /// <summary>
        /// 保存检测数据到正式表中并生成报告
        /// </summary>
        /// <returns>是否保存成功</returns>
        private void SaveFormalData()
        {

            try
            {
                /*
                 * 1、先根据当前所使用的方案名称查出TrialItems表中所有的检测项
                 * 2、遍历TrialItemDataTemp表，当前ChargerID,BarCode,SchemeID，SchemeName下，判断TrialType,TrialName是否包含了步骤1中的所有检测项
                 * 3、如果不包含，则跳过。   如果全部包含，则将这些数据全部转移到正式表中
                 *         
                 * */

                List<StTrialItem> lstTrialItemsInfo = new List<StTrialItem>();
                TrialItemsManage.GetTrialSchemeFromSchemeName(lstChargerInfo[0].SchemeName, ref lstTrialItemsInfo);
                bool isFinish = true;
                if (!isAutoTest)
                {
                    for (int i = 0; i < lstChargerInfo.Count; i++)
                    {

                        for (int k = 0; k < lstTrialItemsInfo.Count; k++)
                        {
                            List<TrialDataModel> lstTrialData = new List<TrialDataModel>();
                            TrialItemDataTmpManage.SelectTrialDataWhereSchemeName(lstChargerInfo[i].BarCode, lstTrialItemsInfo[k].TrialType, lstTrialItemsInfo[k].ItemName, lstChargerInfo[i].ChargerId, lstTrialItemsInfo[k].SchemeName, out lstTrialData);
                            if (lstTrialData.Count <= 0)
                            {
                                isFinish = false;
                                break;
                            }
                        }
                    }
                }
                if (isFinish)//所有枪位所有测试项都有数据，保存到正式库 
                {
                    Dictionary<long, List<string>> dicData = new Dictionary<long, List<string>>();
                    foreach (var item in lstChargerInfo)
                    {
                        List<string> lstPKID = new List<string>();
                        for (int i = 0; i < lstChargerInfo.Count; i++)
                        {
                            long pkid = lstChargerInfo[i].PKID;
                            if (!lstPKID.Contains(pkid.ToString()))
                            {
                                lstPKID.Add(pkid.ToString());
                            }
                            if (dicData.ContainsKey(pkid))
                            {
                                dicData[pkid].Add(lstChargerInfo[i].SchemeName);
                            }
                            else
                            {
                                List<string> lstSchemeName = new List<string>();
                                lstSchemeName.Add(lstChargerInfo[i].SchemeName);
                                dicData.Add(pkid, lstSchemeName);
                            }

                        }
                    }
                    SystemEvent.SaveTrialData();

                    foreach (var item in dicData)
                    {
                        string strSchemeNames = "";
                        for (int i = 0; i < item.Value.Count; i++)
                        {
                            strSchemeNames += "'" + item.Value[i] + "',";
                        }
                        strSchemeNames = strSchemeNames.TrimEnd(',');

                        List<TrialDataModel> lstTrialData = TrialItemDataTmpManage.GetTrialDataFromPkidAndSchemeName(item.Key.ToString(), strSchemeNames);
                        ChargerInfoModel chargerInfo = ChargerInfoManage.GetChargerInfoFromFomalTable(item.Key.ToString());

                        if (lstTrialData == null || lstTrialData.Count == 0)
                        {
                            return;
                        }
                        string wordPath = "";

                        bool result = ExportReport.CreateFile(lstTrialData, chargerInfo, item.Value, ref wordPath);

                    }

                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }
        /// <summary>
        /// 关闭所有设备
        /// </summary>
        public void OFFAllEquip()
        {

            Task task = new Task(OFFAllEquipItem);
            task.Start();
        }

        private void OFFAllEquipItem()
        {
            try
            {
                //List<int> lstIDs = new List<int> { 1, 2, 3, 4 };
                List<int> lstIDs = lstChargerInfo.Select( c => c.ChargerId).ToList() ;
                // 导引1和导引n对调
                int channel;
                string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
                if (!int.TryParse(strChanel, out channel))
                {
                    channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
                }
                else if (lstIDs.FirstOrDefault() == channel)
                    channel = 1;

                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                ExitAllE = false;
                if (_xmlInfoAssembly._EquipMentControl.BMS != null)
                {
                    List<bool> Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    _xmlInfoAssembly._EquipMentControl.BMS.BMS_SetKState(lstIDs, Ks);
                }
                _xmlInfoAssembly._EquipMentControl.ResistanceLoad?.ResistanceLoad_OFF(lstIDs);
                _xmlInfoAssembly._EquipMentControl.FeedbackLoad?.FeedbackLoad_OFF(lstIDs);
                _xmlInfoAssembly._EquipMentControl.LoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                _xmlInfoAssembly._EquipMentControl.StarLoopFeedbackLoad?.LoopFeedbackLoad_OFF(lstIDs, channel);
                _xmlInfoAssembly._EquipMentControl.BMS?.BMS_OFF(lstIDs);
                if (Customer.Contains("YKR"))
                {
                    _xmlInfoAssembly._EquipMentControl.FeedbackLoad.FeedbackLoad_BMSOFF(lstIDs);
                }
                if (!Customer.Equals("XJ") && !Customer.Equals("DH") && !Customer.Equals("CJB"))//XJ客户目前不断开交流电源
                {
                    _xmlInfoAssembly._EquipMentControl.ACSource?.ACSource_OFF(lstIDs);
                    _xmlInfoAssembly._EquipMentControl.ACSource?.ACSource_DisConnect(lstIDs);
                }
                if (Customer.Contains("ZLX"))
                {
                    //断开输入，Y1电网 Y2交流源
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(0, false);
                    Thread.Sleep(300);
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ControlBoard.SetRelaySwitch(1, false);
                    Thread.Sleep(300);
                }
                _xmlInfoAssembly._EquipMentControl.Safety?.SafetyOFF(lstIDs);
                _xmlInfoAssembly._EquipMentControl.AuxiliaryLoadCtrl?.CancelAllState(lstIDs);
            }
            catch(Exception ex)
            {
                Log.Log.LogException(ex);
            }

            SystemEvent.SetUIButton(true);
            SystemEvent.SetUIButton(true, "true");
            SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 检测结束，系统空闲状态");
            ExitAllE = true;
        }

        #region ---------------------交流BMS函数-------------------------

        /// <summary>
        /// 充电中的各个开关
        /// </summary>
        private List<bool> GetKStatus16_Charging()
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

        #endregion
        public List<int> GetListChargerID()
        {
            List<int> lstID = new List<int>();
            for (int i = 0; i < lstChargerInfo.Count; i++)
            {
                if (lstChargerInfo[i].IsCheck)
                {
                    lstID.Add(lstChargerInfo[i].ChargerId);
                }
            }
            return lstID;
        }
        /// <summary>
        /// 开始单项方案
        /// </summary>
        /// <param name="trialType"></param>
        public bool StartScheme(StTrialItem TrialScheme)
        {
            try
            {
                NowTrialScheme = TrialScheme;
                if (lstChargerInfo != null && lstChargerInfo.Count > 0)
                {
                    if (_xmlInfoAssembly._businessAssmeblyManager != null && _xmlInfoAssembly._businessAssmeblyManager.Sessions.ContainsKey((int)TrialScheme.TrialType))
                    {
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstChargerInfo = lstChargerInfo;
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstPortInfo = PortManage.GetInstance()._portAssmeblyManager.Sessions.Select(p => p.Value).ToList();
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].TrialItem = TrialScheme;
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].TrialType = (int)TrialScheme.TrialType;
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstTrialData.ForEach(x => { x.TrialName = TrialScheme.ItemName; });
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].StartEvent();
                        if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS != null)
                            SystemEvent.CreateCANExcelName(TrialScheme.ItemName);   // 清空CAN报文
                        //_xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].SendNoticeToUIAndTxtFile("关闭负载中!");
                        List<int> lstID = GetListChargerID();
                        if (AllEquipStateData.DicResisLoad_StateData.Count > 0 || AllEquipStateData.DicFeedbackLoad_StateData.Count > 0 || AllEquipStateData.DicLoopFeedbackLoad_StateData.Count > 0)
                            _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].SetLoadDCOFF(lstID);

                        SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Testing);
                        ////开始前先清空检测方案表格右边对应的检定结果
                        //for (int i = 0; i < _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstTrialData.Count; i++)
                        //{
                        //    SystemEvent.SendTrialResultToUI(_xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstTrialData[i]);
                        //}
                        //开始前把安规的循环读连接状态关掉
                        if (lstSafetyTrialTypes.Contains(TrialScheme.TrialType))
                        {
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Safety.PauseReadSafetyStateData(lstID, true);
                        }
                        //开始一个测试项之前， 把临时表里这个测试项的数据删了
                        foreach(var trialData in _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstTrialData)
                        {
                            //勾选的枪号才进行清空数据
                            if (trialData.IsCheck)
                            {
                                TrialItemDataTmpManage.DeleteTrialData(trialData);
                                TrialItemResultTmpManage.DeleteTrialFinalData(trialData);
                            }
                        }
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].ExecuteMethod();
                        _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].StopEvent();
                        //测试后把安规的循环读连接状态打开
                        if (lstSafetyTrialTypes.Contains(TrialScheme.TrialType))
                        {
                            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.Safety.PauseReadSafetyStateData(lstID, false);
                        }
                        if (_xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS != null)
                            SystemEvent.CreateCANExcel();   // 保存CAN报文
                        if (_xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].isAutoTest)
                        {
                            return _xmlInfoAssembly._businessAssmeblyManager.Sessions[(int)TrialScheme.TrialType].LstTrialData.First(x =>  x.TrialName == TrialScheme.ItemName).TrialFinalResult == EmTrialResult.Pass;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 重置枪信息
        /// </summary>
        /// <param name="lstCheckCharger">选中枪编号</param>
        public void ResetChargerInfo(List<int> lstCheckCharger)
        {
            try
            {
                for (int i = 0; i < lstChargerInfo.Count; i++)
                {
                    lstChargerInfo[i].IsCheck = false;
                }
                for (int j = 0; j < lstCheckCharger.Count; j++)
                {
                    int index = lstChargerInfo.FindIndex(s => s.ChargerId == lstCheckCharger[j]);
                    if (index >= 0)
                    {
                        lstChargerInfo[index].IsCheck = true;
                        lstChargerInfo[index].CheckResult = EmTrialResult.Wait;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);

            }
        }
        public void SetCPReresh()
        {
            //安规测试没有导引
            if (AllEquipStateData.DicBMS_AC_StateData == null && AllEquipStateData.DicBMS_DC_StateData == null)
                return;

            List<int> lstIDs = GetListChargerID();
            List<bool> Ks = GetKStatus16_Charging_DC();

            //受直流桩特性影响，充电状态有很多种可能性，此处多做很多冗余判断，以保证覆盖所有情况

            if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
            {
                Ks = GetKStatus16_Charging();
                Ks[3] = false;
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMS_SetKState(lstIDs, Ks);
                Thread.Sleep(1000);
            }
            else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
            {
                string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstChargerInfo[0].ChargerId].ChargingState;
                if (!BMSInfo.Contains("充电中"))
                {


                    Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                }
            }



            _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.ACSource?.ACSource_ON(lstIDs);
            if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
            {
                //模拟CP恢复，S2断开
                Ks = GetKStatus16_Charging();
                Ks[0] = false;
                _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMS_SetKState(lstIDs, Ks);
            }
            else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
            {
                string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[lstChargerInfo[0].ChargerId].ChargingState;
                if (!BMSInfo.Contains("充电中"))
                {
                    Ks = GetKStatus16_Charging_DC();
                    _xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                }
            }
            Thread.Sleep(500);

        }
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
            //26true输出过压控制,31true停止发送报文
            return Ks;
        }
    }
}
