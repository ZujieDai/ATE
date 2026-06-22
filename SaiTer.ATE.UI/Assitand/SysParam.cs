using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SaiTer.ATE.UI.Assitand
{
    /// <summary>
    /// 系统参数配置
    /// </summary>
    [DefaultProperty("Name")]
    public class SysParam
    {
        #region ---Field region---
        private static SysParam _SysParam = null;
        //检测完一个测试项目后，是否需要刷新CP信号模拟拔枪再插枪
        private bool _IsNeedRereshCP = false;
        //模拟拔枪后再插枪中间等待的时间(单位：秒)
        private double _RefreshCPWaitTime = 5;
        //检测完一个测试项目后，是否需要重新上电压
        private bool _IsNeedRereshVoltage = false;

        //系统最大支持同时测试的枪位数量
        private int _MaxChargerCount = 1;
        //所有测试项全都有数据后是否自动保存数据到正式库
        private bool _IsAutoSaveTrialData;

        //电阻负载单三相并机继电器序号（闭合此继电器切换到三相、断开切换到单相）  此值小于0代表不具备此功能
        private int _RelayIndex = -999;

        //示波器1号通道探头比
        private int _Channle1 = 500;
        private int _Channle2 = 500;//示波器2号通道探头比
        private int _Channle3 = 500;//示波器3号通道探头比
        private int _Channle4 = 500;


        //设备支持的充电桩类型
        private string _ChargerType = "国标直流充电枪|欧标直流充电枪";


        #endregion
        #region ---Constructor---
        /// <summary>
        /// 构造函数
        /// </summary>
        private SysParam()
        {
        }

        public static SysParam GetInstance()
        {
            if (_SysParam == null)
            {
                _SysParam = new SysParam();
            }
            return _SysParam;
        }
        #endregion

        #region ---Property region---

        [DisplayName("是否需要模拟拔、插枪"), Category("单个故障测试项完成后的动作"), Description("true-代表需要   false-不需要"), Browsable(true)]
        public bool IsNeedRereshCP
        {
            get { return _IsNeedRereshCP; }
            set { _IsNeedRereshCP = value; }
        }

        [DisplayName("模拟拔枪后再插枪中间等待时间(单位:秒)"), Category("单个故障测试项完成后的动作"), Description("true-代表需要   false-不需要"), Browsable(true)]
        public double RefreshCPWaitTime
        {
            get { return _RefreshCPWaitTime; }
            set { _RefreshCPWaitTime = value; }
        }
        [DisplayName("是否需要重新上电"), Category("单个故障测试项完成后的动作"), Description("true-代表需要   false-不需要"), Browsable(true)]
        public bool IsNeedRereshVoltage
        {
            get { return _IsNeedRereshVoltage; }
            set { _IsNeedRereshVoltage = value; }
        }
        [DisplayName("最大枪位数量"), Category("该系统最大支持同时测试的枪位数量"), Description("可以支持几个枪同时测功能就填几"), Browsable(true)]
        public int MaxChargerCount
        {
            get { return _MaxChargerCount; }
            set { _MaxChargerCount = value; }
        }
        [DisplayName("自动保存数据"), Category("所有测试项全都有数据后是否自动保存数据到正式库"), Description("true-代表需要   false-不需要"), Browsable(true)]
        public bool IsAutoSaveTrialData
        {
            get { return _IsAutoSaveTrialData; }
            set { _IsAutoSaveTrialData = value; }
        }
        [DisplayName("闭合此继电器切换到三相、断开切换到单相"), Category("电阻负载单三相并机继电器序号"), Description("此值小于0代表不具备此功能"), Browsable(true)]
        public int RelayIndex
        {
            get { return _RelayIndex; }
            set { _RelayIndex = value; }
        }
        [DisplayName("1通道"), Category("示波器通道探头比"), Description("例如变比为500：1, 就填500"), Browsable(true)]
        public int Channle1
        {
            get { return _Channle1; }
            set { _Channle1 = value; }
        }
        [DisplayName("2通道"), Category("示波器通道探头比"), Description("例如变比为500：1, 就填500"), Browsable(true)]
        public int Channle2
        {
            get { return _Channle2; }
            set { _Channle2 = value; }
        }
        [DisplayName("3通道"), Category("示波器通道探头比"), Description("例如变比为500：1, 就填500"), Browsable(true)]
        public int Channle3
        {
            get { return _Channle3; }
            set { _Channle3 = value; }
        }
        [DisplayName("4通道"), Category("示波器通道探头比"), Description("例如变比为500：1, 就填500"), Browsable(true)]
        public int Channle4
        {
            get { return _Channle4; }
            set { _Channle4 = value; }
        }

        [DisplayName("设备支持检测的充电桩类型"), Category("多种桩类型用|隔开"), Description("例如：国标直流充电枪|欧标直流充电枪"), Browsable(true)]
        public string ChargerType
        {
            get { return _ChargerType; }
            set { _ChargerType = value; }
        }
        /// <summary>
        /// 读取XML
        /// </summary>
        /// <param name="path"></param>
        public void ReadXml(string path)
        {

            XDocument _XDoc = XDocument.Load(path);

            //模拟拔、插枪
            _IsNeedRereshCP = bool.Parse(_XDoc.Descendants("System").Single().Elements("IsNeedRefreshCP").Single().Attribute("value").Value);
            _RefreshCPWaitTime = double.Parse(_XDoc.Descendants("System").Single().Elements("RefreshCPWaitTime").Single().Attribute("value").Value);

            //重新上电
            _IsNeedRereshVoltage = bool.Parse(_XDoc.Descendants("System").Single().Elements("IsNeedRereshVoltage").Single().Attribute("value").Value);
            //最大同测枪数量
            _MaxChargerCount = int.Parse(_XDoc.Descendants("System").Single().Elements("MaxChargerCount").Single().Attribute("value").Value);
            //所有测试项全都有数据后是否自动保存数据到正式库
            _IsAutoSaveTrialData = bool.Parse(_XDoc.Descendants("System").Single().Elements("IsAutoSaveTrialData").Single().Attribute("value").Value);
            //电阻负载单三相并机继电器序号
            _RelayIndex = int.Parse(_XDoc.Descendants("System").Single().Elements("RelayIndex").Single().Attribute("value").Value);
            //示波器1号通道探头比
            _Channle1 = int.Parse(_XDoc.Descendants("System").Single().Elements("Channle1").Single().Attribute("value").Value);
            _Channle2 = int.Parse(_XDoc.Descendants("System").Single().Elements("Channle2").Single().Attribute("value").Value);
            _Channle3 = int.Parse(_XDoc.Descendants("System").Single().Elements("Channle3").Single().Attribute("value").Value);
            _Channle4 = int.Parse(_XDoc.Descendants("System").Single().Elements("Channle4").Single().Attribute("value").Value);
            //支持的枪类型
            _ChargerType = _XDoc.Descendants("System").Single().Elements("ChargerType").Single().Attribute("value").Value;


        }
        /// <summary>
        /// 保存XML
        /// </summary>
        /// <param name="path"></param>
        /// <param name="_ClSysParam"></param>
        public void SaveXml(string path, SysParam _SysParam)
        {

            XDocument _XDoc = XDocument.Load(path);
            _XDoc.Descendants("IsNeedRefreshCP").Single().SetAttributeValue("value", _SysParam.IsNeedRereshCP);
            _XDoc.Descendants("IsNeedRereshVoltage").Single().SetAttributeValue("value", _SysParam.IsNeedRereshVoltage);

            _XDoc.Descendants("MaxChargerCount").Single().SetAttributeValue("value", _SysParam.MaxChargerCount);
            _XDoc.Descendants("IsAutoSaveTrialData").Single().SetAttributeValue("value", _SysParam.IsAutoSaveTrialData);

            _XDoc.Descendants("RelayIndex").Single().SetAttributeValue("value", _SysParam.RelayIndex);
            _XDoc.Descendants("Channle1").Single().SetAttributeValue("value", _SysParam.Channle1);
            _XDoc.Descendants("Channle2").Single().SetAttributeValue("value", _SysParam.Channle2);
            _XDoc.Descendants("Channle3").Single().SetAttributeValue("value", _SysParam.Channle3);
            _XDoc.Descendants("Channle4").Single().SetAttributeValue("value", _SysParam.Channle4);

            _XDoc.Descendants("ChargerType").Single().SetAttributeValue("value", _SysParam.ChargerType);

            _XDoc.Save(path);

        }

        #endregion
    }
}
