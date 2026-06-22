using SaiTer.ATE.Business;
using SaiTer.ATE.Controls;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.Manage
{
    /// <summary>
    /// 配置文件信息和反射集合
    /// </summary>
    public class XmlInfoAndAssembly
    {
        /// <summary>
        /// 控制类型
        /// </summary>
        public int ControlType
        {
            set;
            get;
        }
        /// <summary>
        /// 业务实际对象集合
        /// </summary>
        public AssemblyManager<BusinessBase> _businessAssmeblyManager = null;//业务类
        /// <summary>
        /// 控制类集
        /// </summary>
        public ControlsManage _ControlsManage = null;
        /// <summary>
        /// 开放设备控制
        /// </summary>
        public ControlsListManager _EquipMentControl = null;
        /// <summary>
        /// 检测项目集合
        /// </summary>
        public List<StTrialItem> lstTrialItems = new List<StTrialItem>();

        /// <summary>
        /// 充电枪信息
        /// </summary>
        public List<ChargerInfoModel> lstCharger = new List<ChargerInfoModel>();

        private static XmlInfoAndAssembly _XmlInfoAndAssembly = null;

        /// <summary>
        /// 系统配制文件
        /// </summary>
        public SystemXmlInfo _systemXmlInfo = null;
        /// <summary>
        /// 单例配置文件和反射对象集合
        /// </summary>
        /// <returns></returns>
        public static XmlInfoAndAssembly GetInstance()
        {
            if (_XmlInfoAndAssembly == null)
            {
                _XmlInfoAndAssembly = new XmlInfoAndAssembly();
            }
            return _XmlInfoAndAssembly;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlInfoAndAssembly()
        {
            LoadAssmebly();
            _systemXmlInfo = SystemXmlInfo.Instance();
            // SetConfigInfoToAssembly();//如果需要系统配置文件则添加此代码YF2023-3-23
        }
        /// <summary>
        /// 从DLL加载业务对象
        /// </summary>
        public void LoadAssmebly()
        {
            try
            {

                //需要反射的业务类名集合
                Dictionary<int, string> classNames = new Dictionary<int, string>();

                //加载业务类名配置文件
                XDocument _XDoc = XDocument.Load("xml\\TrialTypeItems.xml");
                foreach (XElement item in _XDoc.Descendants("items").Elements("item"))
                {
                    if (!classNames.ContainsKey(Convert.ToInt32(item.Attribute("Value").Value)))
                    {
                        classNames.Add(Convert.ToInt32(item.Attribute("Value").Value), item.Attribute("ClassName").Value.ToString());
                    }
                }

                //反射业务类
                _businessAssmeblyManager = new AssemblyManager<BusinessBase>("SaiTer.ATE.Business", classNames, "");
                //实例化控制对象
                _ControlsManage = new ControlsManage();

                //对业务类的设备控制属性赋值
                foreach (KeyValuePair<int, BusinessBase> item in _businessAssmeblyManager.Sessions)
                {

                    //对业务的控制属性赋值
                    if (_businessAssmeblyManager.Sessions[item.Key] != null)
                    {
                        _businessAssmeblyManager.Sessions[item.Key].ControlEquipMent = _ControlsManage.ControlAssmeblyManager;
                        _EquipMentControl = _ControlsManage.ControlAssmeblyManager;
                        break;

                    }
                    else
                    {
                        //记录日志("当前试验类型为：" + item.Key.ToString() + "试验加载失败，请检查配置！");
                        Log.Log.LogMessage("当前试验类型为：" + item.Key.ToString() + "试验加载失败，请检查配置！");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                //界面显示：("加载系统组件失败！请检查配置文件！");
            }
        }

        /// <summary>
        /// 将配置文件信息加载到反射类中去
        /// </summary>
        public void SetConfigInfoToAssembly()
        {
            foreach (KeyValuePair<int, BusinessBase> item in _businessAssmeblyManager.Sessions)
            {
                _businessAssmeblyManager.Sessions[item.Key].Channel1 = _systemXmlInfo.Channle1;
                _businessAssmeblyManager.Sessions[item.Key].Channel2 = _systemXmlInfo.Channle2;
                _businessAssmeblyManager.Sessions[item.Key].Channel3 = _systemXmlInfo.Channle3;
                _businessAssmeblyManager.Sessions[item.Key].Channel4 = _systemXmlInfo.Channle4;
                _businessAssmeblyManager.Sessions[item.Key].RelayIndex = _systemXmlInfo.RelayIndex - 1;
                _businessAssmeblyManager.Sessions[item.Key].IsAutoSaveTrialData = _systemXmlInfo.IsAutoSaveTrialData;
                _businessAssmeblyManager.Sessions[item.Key].IsNeedRereshCP = _systemXmlInfo.IsNeedRefreshCP;
                _businessAssmeblyManager.Sessions[item.Key].RefreshCPWaitTime = _systemXmlInfo.RefreshCPWaitTime;
                _businessAssmeblyManager.Sessions[item.Key].IsNeedRereshVoltage = _systemXmlInfo.IsNeedRereshVoltage;
            }
        }

        /// <summary>
        /// 配置业务对应控制方式
        /// </summary>
        public void SetBusinessControl()
        {
            foreach (KeyValuePair<int, BusinessBase> item in _businessAssmeblyManager.Sessions)
            {
                try
                {
                    if (_businessAssmeblyManager.Sessions[item.Key] != null)
                    {
                        _businessAssmeblyManager.Sessions[item.Key].ControlEquipMent = _ControlsManage.ControlAssmeblyManager;
                        _EquipMentControl = _ControlsManage.ControlAssmeblyManager;

                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                }
            }
        }
    }
}
