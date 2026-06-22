using SaiTer.ATE.IDAL;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SaiTer.ATE.DataModel.EnumModel;
using System.Runtime.InteropServices.ComTypes;

namespace SaiTer.ATE.Manage
{
    public class EquipMentsManage
    {
        // public Dictionary<int, AssemblyManager<EquipMentBase>> DitEquipMentAssmeblyManager = null;
        public AssemblyManager<EquipMentBase> EquipMentAssmeblyManager = null;
        /// <summary>
        /// 线程集合
        /// </summary> 
        private List<Thread> LstThread = new List<Thread>();
        string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();

        public EquipMentsManage()
        {
            LoadAssmebly();
            StartThreadReadData();
        }
        /// <summary>
        /// 反射设备类
        /// </summary>
        private void LoadAssmebly()
        {
            try
            {
                // DitEquipMentAssmeblyManager = new Dictionary<int, AssemblyManager<EquipMentBase>>();
                //加载配置文件
                XDocument _XDoc = XDocument.Load("xml\\EquipMentManage.xml");
                //实例化端口管理
                PortManage Port = PortManage.GetInstance();
                //
                foreach (XElement EquipMents in _XDoc.Descendants("EquipMentConfig").Elements("EquipMents"))
                {
                    Dictionary<int, string> classNames = new Dictionary<int, string>();

                    //类名集合
                    foreach (XElement item in EquipMents.Elements("Equip"))
                    {
                        classNames.Add(Convert.ToInt32(item.Attribute("ID").Value), item.Attribute("EquipMentName").Value.ToString());
                    }
                    //反射设备类
                    EquipMentAssmeblyManager = new AssemblyManager<EquipMentBase>("SaiTer.ATE.EquipMent", classNames, "");
                    //将设备类属性赋值
                    foreach (KeyValuePair<int, string> item in classNames)
                    {
                        if (EquipMentAssmeblyManager.Sessions.ContainsKey(item.Key))
                        {
                            EquipMentAssmeblyManager.Sessions[item.Key].EquipMentClassName = item.Value;
                        }
                    }

                    foreach (XElement item in EquipMents.Elements("Equip"))
                    {
                        string equipMentName = item.Attribute("EquipMentName").Value;
                        int ID = Convert.ToInt32(item.Attribute("ID").Value);
                        if (Port._portAssmeblyManager.Sessions.ContainsKey(ID))
                        {
                            EquipMentAssmeblyManager.Sessions[ID].EquipMentPort = Port._portAssmeblyManager.Sessions[ID];
                        }
                        //重复通讯次数
                        EquipMentAssmeblyManager.Sessions[ID].ReConnNum = int.Parse(item.Attribute("ReConnNum").Value.ToString());
                        //设备超时时间
                        EquipMentAssmeblyManager.Sessions[ID].OutTimes = int.Parse(item.Attribute("OutTime").Value.ToString());
                        //添加设备管理充电枪位
                        EquipMentAssmeblyManager.Sessions[ID].EquipManageChargerId = new List<int>();
                        //  EquipMentAssmeblyManager.Sessions[ID].ChargerID = int.Parse(item.Attribute("ChargerId").Value.ToString());
                        string[] StrTemp = item.Attribute("ChargerId").Value.ToString().Split('|');
                        EquipMentAssmeblyManager.Sessions[ID].ChargerID = int.Parse(StrTemp[0]);
                        for (int i = 0; i < StrTemp.Length; i++)
                        {
                            EquipMentAssmeblyManager.Sessions[ID].EquipManageChargerId.Add(int.Parse(StrTemp[i]));
                        }

                        if (EquipMentAssmeblyManager.Sessions[ID] is emtBMS_GB_DC bms)
                        {
                            //关闭CAN报文发送(下位机会主动发送CAN报文，所以要关闭。上位机只做请求访问)
                            bms.BMS_DC_SetControl(0x50, false);
                        }
                    }
                    //多类别设备对象集合
                    // DitEquipMentAssmeblyManager.Add(0, EquipMentAssmeblyManager);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "配置文件加载异常");
            }
        }
        public void StartThreadReadData()
        {
            ReadData();
        }

        /// <summary>
        /// 开启线程读所有设备实时数据
        /// </summary>
        public void ReadData()
        {
            if (EquipMentAssmeblyManager.Sessions.Count > 0)
            {
                foreach (KeyValuePair<int, EquipMentBase> item in EquipMentAssmeblyManager.Sessions)
                {
                    //加个间隔，防止顺序出错
                    Thread.Sleep(100);
                    if (item.Value.GetType() == typeof(emtSafety) || item.Value.GetType() == typeof(emtSafety_SE7441))
                    {
                        ThreadStart WorkStart = delegate
                        {
                            item.Value.ReadSafetyStateData();
                        };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtQCLeakageCurrent))
                    {
                        QCLeakageCurrent_StateData StateData = new QCLeakageCurrent_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(StateData);
                        ThreadStart WorkStart = delegate { item.Value.LeakageCurrent_ReadState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtZJLeakageCurrent))
                    {
                        ZJLeakageCurrent_StateData StateData = new ZJLeakageCurrent_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(StateData);
                        ThreadStart WorkStart = delegate { item.Value.LeakageCurrent_ReadState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtTMLeakageCurrent))
                    {

                        ThreadStart WorkStart = delegate { item.Value.LeakageCurrent_ReadState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_AC))
                    {
                        BMS_AC_StateData bmsStateData = new BMS_AC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(bmsStateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_GB_DC))
                    {
                        BMS_DC_StateData bmsStateData = new BMS_DC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(bmsStateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_EU_DC))
                    {
                        BMS_EU_DC_StateData bmsStateData = new BMS_EU_DC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(bmsStateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_EU_DC_Msg))
                    {
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_USA_DC))
                    {
                        BMS_USA_DC_StateData bmsStateData = new BMS_USA_DC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(bmsStateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }                    
                    else if (item.Value.GetType() == typeof(emtBMS_USA_DC_Msg))
                    {
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtBMS_JP_DC))
                    {
                        BMS_JP_DC_StateData bmsStateData = new BMS_JP_DC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(bmsStateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadBMS_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(SDSOscilloscope)
                        || item.Value.GetType() == typeof(emtTekOscilloscope)
                        || item.Value.GetType() == typeof(DLMOscilloscope)
                        || item.Value.GetType() == typeof(emtTekOscilloscope_MDO34)
                        || item.Value.GetType() == typeof(emtKSOscilloscope_3000X)
                        || item.Value.GetType() == typeof(emtRIGOLOscilloscope_MSO5000))
                    {
                        //SDSOscilloscope_StateData stateData = new SDSOscilloscope_StateData
                        //{
                        //    ChargerID = item.Value.ChargerID,
                        //};
                        //SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.Read_OscilloscopeState(); };
                        StartThread(WorkStart);

                        continue;
                    }

                    else if (item.Value.GetType() == typeof(emtACSource_SPWY)
                        || item.Value.GetType() == typeof(emtACSource_STAS)
                        //|| item.Value.GetType() == typeof(emtACSource_CtrlBoard)
                        || item.Value.GetType() == typeof(emtACSource_HY)
                        || item.Value.GetType() == typeof(emtACSource_XH)
                        || item.Value.GetType() == typeof(emtACSource_GT)
                        || item.Value.GetType() == typeof(emtACSource_AN)
                        || item.Value.GetType() == typeof(emtACSource_AKSB)
                        )
                    {
                        ACSource_StateData stateData = new ACSource_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ACSource_ReadState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtControlBoard))
                    {
                        ThreadStart WorkStart = delegate { item.Value.ReadControlBoard_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtDIORelay))
                    {
                        ThreadStart WorkStart = delegate { item.Value.ReadControlBoard_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtResistanceLoad_AC))
                    {
                        ResisLoad_StateData stateData = new ResisLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            emType = EmChargerType.Charger_GB_AC,
                            EquipName = "交流电阻负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadResisLoad_State_AC(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtFeedbackLoad_AC))
                    {
                        FeedbackLoadAC_StateData stateData = new FeedbackLoadAC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            emType = EmChargerType.Charger_GB_AC,
                            EquipName = "交流回馈负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadFeedbackLoad_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtResistanceLoad_DC))
                    {
                        ResisLoad_StateData stateData = new ResisLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            emType = EmChargerType.Charger_GB_DC,
                            EquipName = "直流电阻负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadResisLoad_State_DC(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtResistanceLoad_MultiChannel_AC))
                    {
                        ResisLoad_MultiChannel_AC_StateData stateData = new ResisLoad_MultiChannel_AC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            EquipName = "电阻负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadResisLoad_State_AC(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtResistanceLoad_MultiChannel_DC))
                    {
                        ResisLoad_MultiChannel_DC_StateData stateData = new ResisLoad_MultiChannel_DC_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            EquipName = "直流电阻负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadResisLoad_State_DC(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtFeedbackLoad)
                        ||item.Value.GetType()==typeof(emtFeedbackLoad_DC_ST)
                        || item.Value.GetType() == typeof(emtFeedbackLoad_YKR))
                    {
                        FeedbackLoad_StateData stateData = new FeedbackLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadFeedbackLoad_StateData(); };
                        StartThread(WorkStart);
                        ThreadStart BMSInfoWorkStart = delegate { item.Value.WriteFeedbackLoad_BMSInfo(); };
                        StartThread(BMSInfoWorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtLoopFeedbackLoad) || item.Value.GetType() == typeof(emtStarLoopFeedbackLoad))
                    {
                        LoopFeedbackLoad_StateData stateData = new LoopFeedbackLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadFeedbackLoad_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtFeedbackLoad_SZHY))
                    {
                        FeedbackLoad_StateData stateData = new FeedbackLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            EquipName = "回馈负载"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadFeedbackLoad_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtPA6500) || item.Value.GetType() == typeof(emtWT333E) || item.Value.GetType() == typeof(emtWT5000) || item.Value.GetType() == typeof(emtPA6000))
                    {
                        PowerAnalyzer_StateData stateData = new PowerAnalyzer_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadPA6500_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtElectronicLoad))
                    {
                        ElectronicLoad_StateData stateData = new ElectronicLoad_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadElectronicLoad_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtGDM9061) || item.Value.GetType() == typeof(emtDMM6500))
                    {
                        MultiMeter_StateData stateData = new MultiMeter_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.Read_MultiMeterState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtWaveRecoderBoard)
                        || item.Value.GetType() == typeof(emtWaveRecoderBoard30))
                    {

                        ThreadStart WorkStart = delegate { item.Value.ReadWaveRecoderBoard_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtElectricMeter_DTSD336D)
                        || item.Value.GetType() == typeof(emtElectricMeter_ZH4041))
                    {

                        ThreadStart WorkStart = delegate { item.Value.Read_EMState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtOscillographInstrument))
                    {
                        ThreadStart WorkStart = delegate { item.Value.Read_OscillographState(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtAuxiliaryLoadCtrl))
                    {
                        AuxiliaryLoadCtrl_StateData stateData = new AuxiliaryLoadCtrl_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadAuxiliaryLoadCtrl_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                    else if (item.Value.GetType() == typeof(emtCharger_NTGX))
                    {
                        Charger_NTGX_StateData stateData = new Charger_NTGX_StateData
                        {
                            ChargerID = item.Value.ChargerID,
                            EquipName = "充电桩模拟器"
                        };
                        SystemEvent.SendMonitorMessage(stateData);
                        ThreadStart WorkStart = delegate { item.Value.ReadCharger_StateData(); };
                        StartThread(WorkStart);
                        continue;
                    }
                }
            }

        }

        private void StartThread(ThreadStart WorkStart)
        {
            Thread thread = new Thread(WorkStart);
            thread.Start();
            LstThread.Add(thread);
        }
    }
}
