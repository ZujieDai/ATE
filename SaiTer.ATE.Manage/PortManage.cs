using NationalInstruments.VisaNS;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.PortManage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ResourceManager = NationalInstruments.VisaNS.ResourceManager;

namespace SaiTer.ATE.Manage
{
    public class PortManage
    {
        /// <summary>
        /// 设备管理对象集合
        /// </summary>
        public AssemblyManager<PortBase> _portAssmeblyManager = null;
        private static PortManage _PortManage = null;
        /// <summary>
        /// 构造函数 
        /// </summary>
        private PortManage()
        {
            LoadAssmebly();
        }
        /// <summary>
        /// 单列
        /// </summary>
        /// <returns></returns>
        public static PortManage GetInstance()
        {
            if (_PortManage == null)
            {
                _PortManage = new PortManage();
            }
            return _PortManage;
        }
        /// <summary>
        /// 反射端口类
        /// </summary>
        private void LoadAssmebly()
        {
            try
            {
                //类名集合
                Dictionary<int, string> classNames = new Dictionary<int, string>();
                //加载端口配置文件
                XDocument _XDoc = XDocument.Load("xml\\EquipmentManage.xml");

                //将类名添加到集合
                foreach (XElement item in _XDoc.Descendants("EquipMents").Elements("Equip"))
                {
                    classNames.Add(Convert.ToInt32(item.Attribute("ID").Value), item.Attribute("ComType").Value.ToString());
                }


                //反射端口类
                _portAssmeblyManager = new AssemblyManager<PortBase>("SaiTer.ATE.PortManage", classNames, "PortType");
                //循环赋值属性并打开端口
                foreach (XElement item in _XDoc.Descendants("EquipMents").Elements("Equip"))
                {
                    int id = Convert.ToInt32(item.Attribute("ID").Value);
                    _portAssmeblyManager.Sessions[id].EquiqMentClassName = item.Attribute("EquipMentName").Value.ToString();

                    if (item.Attribute("ComType").Value.ToString() == "SerialPort")//串口发送方式
                    {

                        _portAssmeblyManager.Sessions[id].PortName = item.Attribute("PortNum").Value.ToString();
                        foreach (var temp in _portAssmeblyManager.Sessions)
                        {
                            if (_portAssmeblyManager.Sessions[id].PortName == temp.Value.PortName)
                            {
                                _portAssmeblyManager.Sessions[id] = temp.Value;
                                break;
                            }
                        }



                        _portAssmeblyManager.Sessions[id].PortParams = item.Attribute("PortParams").Value.ToString();
                        _portAssmeblyManager.Sessions[id].Open();
                    }
                    else if (item.Attribute("ComType").Value.ToUpper().Contains("TCPCLIENT"))//TCP客户端
                    {
                        _portAssmeblyManager.Sessions[id].PortParams = item.Attribute("PortNum").Value.ToString();
                        _portAssmeblyManager.Sessions[id].Ipaddress = item.Attribute("PortNum").Value.ToString();
                        _portAssmeblyManager.Sessions[id].PortName = item.Attribute("PortParams").Value.ToString();
                        _portAssmeblyManager.Sessions[id].RemotePort = Convert.ToInt32(item.Attribute("PortParams").Value);
                        string strName = item.Attribute("EquipMentName").Value.ToUpper();
                        if (!strName.Contains("TEK") && !strName.Contains("SDS") && !strName.Contains("DL") && !strName.Contains("GDM") && !strName.Contains("RIGOL"))//泰克/鼎阳/横河/普源示波器/横河录波仪调用厂方动态库，此处不连接
                        {
                            _portAssmeblyManager.Sessions[id].Open();

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
