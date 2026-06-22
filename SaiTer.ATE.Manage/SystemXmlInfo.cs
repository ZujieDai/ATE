using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.Manage
{
    public class SystemXmlInfo
    {
        #region   属性
        /// <summary>
        /// 示波器1通道探头比
        /// </summary>
        public string Channle1
        {
            set;
            get;
        }
        /// <summary>
        /// 示波器2通道探头比
        /// </summary>
        public string Channle2
        {
            set;
            get;
        }
        /// <summary>
        /// 示波器3通道探头比
        /// </summary>
        public string Channle3
        {
            set;
            get;
        }
        /// <summary>
        /// 示波器4通道探头比
        /// </summary>
        public string Channle4
        {
            set;
            get;
        }


        /// <summary>
        /// 该系统最大支持同时测试的枪位数量
        /// </summary>
        public int MaxChargerCount
        {
            set;
            get;
        }
        /// <summary>
        /// 检测完一个测试项目后，是否需要刷新CP/CC1信号模拟拔枪再插枪
        /// </summary>
        public bool IsNeedRefreshCP
        {
            set; get;
        }
        /// <summary>
        /// 模拟拔枪后再插枪中间等待的时间(单位：秒)
        /// </summary>
        public double RefreshCPWaitTime { get; set; }
        /// <summary>
        /// 检测完一个测试项目后，是否需要重新上电压 
        /// </summary>
        public bool IsNeedRereshVoltage
        {
            set; get;
        }
        /// <summary>
        /// 所有测试项全都有数据后是否自动保存数据到正式库
        /// </summary>
        public bool IsAutoSaveTrialData
        {
            set; get;
        }


        /// <summary>
        /// 电阻负载单三相并机继电器序号（闭合此继电器切换到三相、断开切换到单相)
        /// </summary>
        public int RelayIndex
        {
            set;
            get;
        }

        /// <summary>
        /// 设备支持的充电桩类型
        /// </summary>
        public string[] ChargerType
        {
            set; get;
        }

        #endregion

        private static SystemXmlInfo _systemXmlInfo = null;
        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static SystemXmlInfo Instance()
        {
            if (_systemXmlInfo == null)
            {
                _systemXmlInfo = new SystemXmlInfo();
            }
            return _systemXmlInfo;
        }
        private SystemXmlInfo()
        {
            Channle1 = "500";
            Channle2 = "200";
            Channle3 = "1";
            Channle4 = "500";

            IsNeedRefreshCP = true;
            IsNeedRereshVoltage = false;
            MaxChargerCount = 1;
            RelayIndex = -999;
            ChargerType = new string[] { "国标直流充电枪", "欧标直流充电枪" };
            RefreshCPWaitTime = 5;

            ReadSystemConfig();
        }

        /// <summary>
        /// 读取系统配置
        /// </summary>
        public void ReadSystemConfig()
        {
            try
            {
                //加载系统配置文件
                XDocument _XDoc = XDocument.Load(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + ConfigurationManager.AppSettings["System"].ToString());
                if (_XDoc != null)
                {

                    foreach (XElement item in _XDoc.Descendants("System").Elements("IsNeedRefreshCP"))
                    {
                        IsNeedRefreshCP = bool.Parse(item.Attribute("value").Value.ToString());
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("RefreshCPWaitTime"))
                    {
                        RefreshCPWaitTime = double.Parse(item.Attribute("value").Value.ToString());
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("IsNeedRereshVoltage"))
                    {
                        IsNeedRereshVoltage = bool.Parse(item.Attribute("value").Value.ToString());
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("MaxChargerCount"))
                    {
                        MaxChargerCount = int.Parse(item.Attribute("value").Value.ToString());
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("IsAutoSaveTrialData"))
                    {
                        IsAutoSaveTrialData = bool.Parse(item.Attribute("value").Value.ToString());
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("RelayIndex"))
                    {
                        RelayIndex = int.Parse(item.Attribute("value").Value.ToString());
                    }

                    foreach (XElement item in _XDoc.Descendants("System").Elements("Channle1"))
                    {
                        Channle1 = item.Attribute("value").Value.ToString();
                    }
                    //加载加密机地址
                    foreach (XElement item in _XDoc.Descendants("System").Elements("Channle2"))
                    {
                        Channle2 = item.Attribute("value").Value.ToString();

                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("Channle3"))
                    {
                        Channle3 = item.Attribute("value").Value.ToString();
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("Channle4"))
                    {
                        Channle4 = item.Attribute("value").Value.ToString();
                    }
                    foreach (XElement item in _XDoc.Descendants("System").Elements("ChargerType"))
                    {
                        ChargerType = item.Attribute("value").Value.ToString().Split('|');
                    }

                }
                else
                {
                    Log.Log.LogMessage("读取配置文件失败！");
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
