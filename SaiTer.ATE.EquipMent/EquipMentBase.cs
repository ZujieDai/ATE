using NationalInstruments.VisaNS;
using NPOI.Util;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace SaiTer.ATE.EquipMent
{

    /// <summary>
    /// 设备基类
    /// </summary>
    public abstract partial class EquipMentBase
    {
        /// <summary>
        /// 通信超时（失败）次数
        /// </summary>
        public int TimeOutCount = 0;
        /// <summary>
        /// 报文通信状态（不包含通信通道的状态，只涉及报文收发解析）
        /// </summary>
        public bool MsgState = false;

        protected static object objLock = new object();
         
       
        /// <summary>
        /// 接收缓存
        /// </summary>
        public Queue<byte[]> DataBuf = new Queue<byte[]>();
        /// <summary>
        /// 计时器
        /// </summary>
        public Stopwatch _Stopwatch = new Stopwatch();
        private PortBase _PortBase;
        /// <summary>
        /// 通讯通道
        /// </summary>
        public PortBase EquipMentPort
        {
            get { return _PortBase; }
            set
            {
                _PortBase = value;
                StartRevDataEvent();
            }
        }


        /// <summary>
        /// 通讯超时时间
        /// </summary>
        public int OutTimes
        {
            get;
            set;
        }
        /// <summary>
        /// 充电枪位编号
        /// </summary>
        public int ChargerID
        {
            get;
            set;
        }
        /// <summary>
        /// 是否开启自动读取实时数据
        /// </summary>
        public bool AutoReadData
        {
            get;
            set;
        }
        /// <summary>
        /// 设备管控枪位号
        /// </summary>
        public List<int> EquipManageChargerId
        {
            get;
            set;
        }
        /// <summary>
        /// 通讯设备类名(例：emtSafty)
        /// </summary>
        public string EquipMentClassName
        {
            get;
            set;
        }
        /// <summary>
        /// 设备中文描述名称（例：安规）
        /// </summary>
        public string EquipMentName
        {
            get;
            set;
        }

        /// <summary>
        /// 设置报文通信状态
        /// </summary>
        /// <param name="bState">报文通信状态</param>
        /// <param name="MaxTimeOutCount">最大允许通信超时次数，默认5</param>
        /// <returns>判定通信结果</returns>
        public bool SetCommunicationState(bool bState, int MaxTimeOutCount = 5)
        {
            if (!bState)
            {
                TimeOutCount++;
                if (TimeOutCount >= MaxTimeOutCount)//超过一定的次数才判定通信异常
                {
                    TimeOutCount = MaxTimeOutCount;
                    MsgState = false;
                }
            }
            else
            {
                TimeOutCount = 0;
                MsgState = true;
            }
            return MsgState;
        }

        /// <summary>
        /// 重复通讯次数
        /// </summary>
        public int ReConnNum
        {
            get;
            set;
        }
        /// <summary>
        /// 发送监视器
        /// </summary>
        /// <param name="MonitorData">设备返回实时状态数据</param>
        public void SendMonitorData(object MonitorData)
        {
            SystemEvent.SendMonitorMessage(MonitorData);
        }
        /// <summary>
        /// 发送日志到文件
        /// </summary>
        /// <param name="Msg">日志信息</param>
        public void SendMsgToFile(string Msg)
        {
            Log.Log.LogMessage("[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]" + Msg, "设备日志_" + ChargerID + "号枪" + EquipMentName);
        }
        /// <summary>
        /// 发送异常到文件
        /// </summary>
        /// <param name="ex">异常信息</param>
        public void SendExMsg(Exception ex)
        {
            Log.Log.LogException(ex, "设备异常日志");
        }
        /// <summary>
        /// 检查返回报文是否符合协议
        /// </summary>
        /// <param name="RevData"></param>
        /// <returns></returns>
        public int CheckRevData(byte[] RevData)
        {
            return 0;
        }

        /// <summary>
        /// 开始接收事件
        /// </summary>
        /// 

        public void StartRevDataEvent()
        {
            _PortBase.SendBuffDataEvent += new PortBase.DteRevBuffData(RevData);
        }
        /// <summary>
        /// 停止接收事件
        /// </summary>
        public void StopRevDataEvent()
        {
            if (_PortBase != null)
            {
                _PortBase.SendBuffDataEvent -= RevData;
            }
        }
        // public virtual void RevData(byte[] RevData) { }
        public virtual void RevData(byte[] RevData)
        {
            byte[] byttemp = new byte[RevData.Length];
            byttemp = RevData;
            DataBuf.Enqueue(byttemp);
        }
        /// <summary>
        /// 查询设备端口返回的数据
        /// </summary>
        public byte[] RevEquipMentData_Old()
        {
            try
            {
                //byte[] temp = new byte[12048];
                List<byte> lstTmp = new List<byte>();
                _Stopwatch.Reset();
                _Stopwatch.Start();
                int len = 0; ;
                while (_Stopwatch.ElapsedMilliseconds < OutTimes)
                {
                    if (DataBuf.Count > 0)
                    {
                        byte[] RevTemp;
                        RevTemp = DataBuf.Dequeue();
                        if (RevTemp != null)
                        {
                            //Array.Copy(RevTemp, 0, temp, len, RevTemp.Length);
                            lstTmp.AddRange(RevTemp);
                            len += RevTemp.Length;
                        }

                    }
                    System.Threading.Thread.Sleep(5);
                }
                if (len > 0)
                {
                    byte[] data = new byte[len];
                    //Array.Copy(temp, 0, data, 0, len);
                    Array.Copy(lstTmp.ToArray(), 0, data, 0, len);
                    return data;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);
                return null;
            }
            finally
            {
                _Stopwatch.Stop();
            }
        }

        /// <summary>
        /// 查询设备端口返回的数据
        /// </summary>
        public byte[] RevEquipMentData()
        {
            try
            {
                List<byte> lstTmp = new List<byte>();
                DateTime dtRev = DateTime.Now;
                int len = 0; ;
                while (dtRev.AddMilliseconds(OutTimes) > DateTime.Now)//当有两个重复发报文的时候有问题，这里直接用时间比较法。
                {
                    if (DataBuf != null && DataBuf.Count > 0)
                    {
                        byte[] RevTemp;
                        RevTemp = DataBuf.Dequeue();
                        if (RevTemp != null)
                        {
                            lstTmp.AddRange(RevTemp);
                            len += RevTemp.Length;
                        }
                    }
                    System.Threading.Thread.Sleep(5);
                }
                if (len > 0)
                {
                    byte[] data = new byte[len];
                    Array.Copy(lstTmp.ToArray(), 0, data, 0, len);
                    return data;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);
                SendMsgToFile(EquipMentName + "接收报文异常: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 字符串转报文数组
        /// </summary>
        /// <param name="AsciiSendStr"></param>
        /// <param name="AsciiOutEndFlag"></param>
        /// <param name="AsciiInEndFlag"></param>
        /// <returns></returns>
        public byte[] GetBuffer(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag)
        {
            List<byte> LstSendBuffer = new List<byte>();
            byte[] tSendBytes = System.Text.Encoding.ASCII.GetBytes(AsciiSendStr);
            byte[] tEndBytes = null;
            if (!string.IsNullOrEmpty(AsciiOutEndFlag))
            {
                tEndBytes = System.Text.Encoding.ASCII.GetBytes(AsciiOutEndFlag);
            }
            byte[] tInEndBytes = null;
            if (!string.IsNullOrEmpty(AsciiInEndFlag))
            {
                tInEndBytes = System.Text.Encoding.ASCII.GetBytes(AsciiInEndFlag);
            }
            LstSendBuffer.AddRange(tSendBytes);
            if (tEndBytes != null)
            {
                LstSendBuffer.AddRange(tEndBytes);
            }
            else if (tInEndBytes != null)
            {
                LstSendBuffer.AddRange(tInEndBytes);
            }
            byte[] writeBuffer = LstSendBuffer.ToArray();
            return writeBuffer;
        }
        protected bool SendData(byte[] WriteBuffer)
        {
            try
            {
                bool ret = true;
                AutoReadData = false;
                Thread.Sleep(OutTimes + 10);
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        EquipMentPort.SendData(WriteBuffer);
                        byte[] RevMsgData = RevEquipMentData();
                        if (RevMsgData == null)
                        {
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();
                            if (RevMsgData == null)
                            {
                                Thread.Sleep(200);
                                SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                ret = false;
                                continue;
                            }
                        }
                    }
                    AutoReadData = true;
                    return ret;
                }
                else
                {
                    SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                    AutoReadData = true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                AutoReadData = true;
                Log.Log.LogException(ex);
                return false;
            }
        }

        #region ----------------------交流源----------------------
        /// <summary>
        /// 关闭交流源
        /// </summary>
        public virtual void ACSource_OFF() { }
        /// <summary>
        /// 启动交流源
        /// </summary>
        public virtual void ACSource_ON() { }
        /// <summary>
        /// 设置交流源电压
        /// </summary>
        /// <param name="Volt">电压值</param>        
        public virtual void ACSource_SetVolt(double Volt) { }

        /// <summary>
        /// 设置交流源频率
        /// </summary>
        /// <param name="freq">频率</param>        
        public virtual void ACSource_SetFreq(double freq) { }
        /// <summary>
        /// 思普交流源断开远程控制
        /// </summary>
        public virtual void ACSource_DisConnect() { }
        /// <summary>
        /// 读取交流源实时状态数据
        /// </summary>
        public virtual void ACSource_ReadState() { }
        /// <summary>
        /// 设置三相电压
        /// </summary>
        /// <param name="VoltA">电压A</param>
        /// <param name="VoltB">电压B</param>
        /// <param name="VoltC">电压C</param>
        public virtual void ACSource_SetVolt3(double VoltA, double VoltB, double VoltC) { }

        /// <summary>
        /// 设置三相相位角
        /// </summary>
        /// <param name="AngleA">A相相位角</param>
        /// <param name="AngleB">B相相位角</param>
        /// <param name="AngleC">C相相位角</param>
        public virtual void ACSource_SetAngle3(double AngleA, double AngleB, double AngleC) { }

        /// <summary>
        /// 设置缺相电压
        /// </summary>
        public virtual void ACSource_SetOpenPhase() { }
        #endregion

        #region --------------------Safety 安规--------------------

        /// <summary>
        /// 初始化安规测试方案
        /// </summary>
        /// <param name="lstIDs">充电枪ID集合</param>
        public virtual bool SafetyInit(string SchemeID, string SchemeName, bool isSave = false) { return false; }
        public virtual void Safety_OFF() { }
        public virtual void SafetySetParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag) { }

        public virtual bool SafetyReadParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag, ref string strData) { return true; }

        /// <summary>
        /// 读BMS实时状态数据
        /// </summary>
        public virtual void ReadSafetyStateData() { }

        public virtual void PauseReadSafetyStateData(bool IsPause) { }
        #endregion


        #region --------------------ControlBoard 程控板--------------------
        /// <summary>
        /// 程控板连接状态
        /// </summary>
        public virtual void ReadControlBoard_StateData() { }
        /// <summary>
        /// 继电器吸合状态
        /// </summary>
        /// <returns></returns>
        public virtual List<bool> ControlBoardReadState() { return null; }

        /// <summary>
        /// 设置继电器
        /// </summary>
        public virtual void ControlResistanceSetRelay(List<bool> lstConditionState) { }
        /// <summary>
        /// 读继电器状态
        /// </summary>
        public virtual void ReadControlBoardCondition() { }
        /// <summary>
        /// 设置三色灯状态
        /// </summary>
        /// <param name="color">颜色</param>
        public virtual void SetLightColor(EmLightColor color) { }
        #endregion

        #region --------------DIO继电器--------------
        /// <summary>
        /// 设置寄存器开关
        /// </summary>
        /// <param name="Register">寄存器地址</param>
        /// <param name="OnOff">通道控制</param>
        public virtual void SetRelaySwitch(uint Register, bool OnOff) { }

        /// <summary>
        /// 读取寄存器开关
        /// </summary>
        /// <param name="StratIndex">起始位置</param>
        /// <param name="RelayCount">寄存器数量</param>
        /// <returns></returns>
        public virtual List<bool> ReadRelaySwitch(int StratIndex, int RelayCount) { return null; }
        #endregion
        /// <summary>
        /// 固纬9061万用表实时状态数据
        /// </summary>
        public virtual void Read_MultiMeterState() { }
    }
}