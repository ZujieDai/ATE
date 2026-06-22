using NationalInstruments.VisaNS;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TmctlAPINet;

namespace SaiTer.ATE.PortManage
{
    public abstract class PortBase
    {
        /// <summary>
        /// 发送数据委托
        /// </summary>
        /// <param name="RevBuff">缓存数据</param>
        public delegate void DteRevBuffData(byte[] RevBuff);
        /// <summary>
        /// 发送接收数据事件
        /// </summary>
        public event DteRevBuffData SendBuffDataEvent;
        /// <summary>
        /// 发送数据方法
        /// </summary>
        /// <param name="BuffData">缓存数据</param>
        public void AddBuffData(byte[] BuffData)
        {
            if (SendBuffDataEvent != null)
            {
                SendBuffDataEvent(BuffData);
            }
        }
        /// <summary>
        /// 发送错误日志
        /// </summary>
        /// <param name="strErrMsg"></param>
        public void SendErrMsg(Exception strErrMsg)
        {
            Log.Log.LogException(strErrMsg, "异常消息");

        }
        public void LogMsg(string strLog)
        {
            Log.Log.LogMessage(strLog, "通讯消息");

        }

        /// <summary>
        /// 端口名称
        /// </summary>
        public string PortName
        {
            get;
            set;
        }
        /// <summary>
        /// 串口波特率
        /// </summary>
        public string PortParams
        {
            get;
            set;
        }
        /// <summary>
        /// NIVISA(泰克/鼎阳示波器、固纬万用表等设备)
        /// </summary>
        public MessageBasedSession VisaNS
        {
            get;
            set;
        }

        /// <summary>
        /// 横河示波器
        /// </summary>
        public TMCTL DLMOscilloscope
        {
            get;
            set;
        }
        /// <summary>
        /// 端口所属设备类名
        /// </summary>
        public string EquiqMentClassName
        {
            get;
            set;
        }
        /// <summary>
        /// 远端Ip地址
        /// </summary>
        public string Ipaddress
        {
            get;
            set;
        }
        /// <summary>
        /// 本地网络端口号
        /// </summary>
        public int LocalPort
        {
            get;
            set;
        }
        /// <summary>
        /// 远端网络端口号
        /// </summary>
        public int RemotePort
        {
            get;
            set;
        }
        /// <summary>
        /// 发送数据方法
        /// </summary>
        /// <param name="SendBuff"></param>
        public abstract void SendData(byte[] SendBuff);
        /// <summary>
        /// 打开端口方法
        /// </summary>
        /// <returns></returns>
        public abstract bool Open();
        /// <summary>
        /// 关闭端口方法
        /// </summary>
        /// <returns></returns>
        public abstract bool Close();

    }
}
