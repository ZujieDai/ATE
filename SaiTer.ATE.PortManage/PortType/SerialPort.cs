using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace SaiTer.ATE.PortManage.PortType
{
    public class SerialPort : PortBase
    {
        public System.IO.Ports.SerialPort _SerialPort; //串行端口资源
        private static object SerialPortLock = new object();
        #region ---- 辅助函数 ----

        /// <summary>
        /// 字符串 ==> Parity 
        /// </summary>
        /// <param name="tag">校验位标记</param>
        /// <returns>Parity枚举值</returns>
        public Parity GetParity(string str_Parity)
        {
            Parity parity = Parity.None;
            switch (str_Parity)
            {
                case "n":
                case "N":
                    parity = Parity.None;
                    break;
                case "e":
                case "E":
                    parity = Parity.Even;
                    break;
                case "o":
                case "O":
                    parity = Parity.Odd;
                    break;
                case "m":
                case "M":
                    parity = Parity.Mark;
                    break;
                case "s":
                case "S":
                    parity = Parity.Space;
                    break;
                default:
                    //错误
                    break;
            }
            return parity;
        }
        /// <summary>
        /// 字符串 ==> StopBits 
        /// </summary>
        /// <param name="tag">停止位标记</param>
        /// <returns>StopBits枚举值</returns>
        public StopBits GetStopBit(string str_StopBit)
        {
            StopBits stopBits = StopBits.One;
            switch (str_StopBit)
            {
                case "1":
                    stopBits = StopBits.One;
                    break;
                case "1.5":
                    stopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    stopBits = StopBits.Two;
                    break;
                case "n":
                    stopBits = StopBits.None;
                    break;
                default:
                    //错误
                    break;
            }
            return stopBits;
        }
        /// <summary>
        /// Parity ==> 字符串
        /// </summary>
        /// <param name="parity">校验位</param>
        /// <returns>Parity字符串</returns>
        public string GetStrParity(Parity parity)
        {
            string str_Parity = "";
            switch (parity)
            {
                case Parity.Even:
                    str_Parity = "e";
                    break;
                case Parity.Mark:
                    str_Parity = "m";
                    break;
                case Parity.None:
                    str_Parity = "n";
                    break;
                case Parity.Odd:
                    str_Parity = "o";
                    break;
                case Parity.Space:
                    str_Parity = "s";
                    break;
            }
            return str_Parity;
        }
        /// <summary>
        /// StopBits ==> 字符串
        /// </summary>
        /// <param name="stopBits">停止位</param>
        /// <returns>StopBits字符串</returns>
        public string GetStrStopBit(StopBits stopBits)
        {
            string str_StopBits = "";
            switch (stopBits)
            {
                case StopBits.None:
                    str_StopBits = "n";
                    break;
                case StopBits.One:
                    str_StopBits = "1";
                    break;
                case StopBits.OnePointFive:
                    str_StopBits = "1.5";
                    break;
                case StopBits.Two:
                    str_StopBits = "2";
                    break;
            }
            return str_StopBits;
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="i"></param>
        public SerialPort(int i)
        {
            this._SerialPort = new System.IO.Ports.SerialPort();
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            try
            {
                if (!_SerialPort.IsOpen)
                {
                    string[] vars = PortParams.Split(',');
                    if (vars.Length >= 4)
                    {
                        //设置串口参数
                        _SerialPort.PortName = PortName;
                        _SerialPort.BaudRate = Convert.ToInt32(vars[0]); //波特率
                        _SerialPort.Parity = this.GetParity(vars[1]);         //校验位
                        _SerialPort.DataBits = Convert.ToInt32(vars[2]); //数据位
                        _SerialPort.StopBits = this.GetStopBit(vars[3]);      //停止位
                    }
                    else if (vars.Length >= 1)
                    {
                        //设置串口参数
                        _SerialPort.PortName = PortName;
                        _SerialPort.WriteTimeout = 3000;
                        _SerialPort.BaudRate = Convert.ToInt32(vars[0]); //波特率
                    }

                    _SerialPort.Close();
                    if (!_SerialPort.IsOpen)
                    {
                        _SerialPort.Open();
                    }
                    _SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivetData);
                }



                return true;
            }
            catch (IOException ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                //SendErrMsg(String.Format("端口{0}未找到。错误信息：{1}", PortName, ex.Source));
                return false;
            }
            catch (UnauthorizedAccessException ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                //SendErrMsg(String.Format("端口{0}被其他程序占用。错误信息：{1}", PortName, ex.Source));
                return false;
            }
            catch (InvalidOperationException ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                //SendErrMsg(String.Format("端口{0}重复打开。错误信息：{1}", PortName, ex.Source));
                return false;
            }
            catch (Exception ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                return false;
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public override bool Close()
        {
            try
            {
                if (_SerialPort.IsOpen)
                {
                    _SerialPort.Close();
                }
                return true;
            }
            catch (InvalidOperationException ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                //SendErrMsg(String.Format("尝试关闭未被打开的端口{0}。错误信息：{1}", PortName, ex.Source));
                return false;
            }
            catch (Exception ExcepMsg)
            {
                SendErrMsg(ExcepMsg);
                return false;
            }
        }

        /// <summary>
        /// 串口发送数据
        /// </summary>
        /// <param name="SendBuff"></param>
        public override void SendData(byte[] SendBuff)
        {
            try
            {
                if (_SerialPort.IsOpen)
                {
                    //将发送缓存和接收缓存中的数据清空
                    //if (_SerialPort.BytesToRead > 0)
                    //{
                    //    _SerialPort.DiscardInBuffer();
                    //}
                    //if (_SerialPort.BytesToWrite > 0)
                    //{
                    //    _SerialPort.DiscardOutBuffer();
                    //}

                    if (SendBuff != null && SendBuff.Length != 0)
                    {
                        _SerialPort.Write(SendBuff, 0, SendBuff.Length); //写串口文件

                        //LogMsg("串口：" + _SerialPort.PortName.ToString() + "波特率：" + _SerialPort.BaudRate.ToString() + "写入数据：" + BitConverter.ToString(SendBuff));
                    }
                }
            }
            catch (InvalidOperationException Exception)
            {
                SendErrMsg(Exception);
                //SendErrMsg(String.Format("端口{0}未被打开，尝试向该文件写数据。错误信息：{1}", PortName, ex.Source));
            }
            catch (Exception Exception)
            {
                SendErrMsg(Exception);
                //SendErrMsg(String.Format("端口{0}发送数据错误。", PortName));
            }
        }

        /// <summary>
        /// 串口接收数据的事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReceivetData(object sender, SerialDataReceivedEventArgs e)
        {
            int _OutBufferLen = 0; //接收串口缓存区的数据长度
            int _RevDataLen = 0; //数据长度
            byte[] _RevDataBuff; //数据缓冲区

            try
            {
                if (_SerialPort.IsOpen)
                {
                    _OutBufferLen = _SerialPort.BytesToRead;
                    if (_OutBufferLen > 0)
                    {
                        _RevDataBuff = new byte[_OutBufferLen];
                        _RevDataLen = _SerialPort.Read(_RevDataBuff, 0, _OutBufferLen);
                        AddBuffData(_RevDataBuff);
                    }
                }
            }
            catch (InvalidOperationException Exception)
            {
                SendErrMsg(Exception);
                //SendErrMsg(String.Format("端口{0}未被打开，尝试读取该文件的数据。错误信息：{1}", PortName, ex.Source));
            }
        }
    }
}
