using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NationalInstruments.Visa;

namespace SaiTer.ATE.PortManage.PortType
{
    /// <summary>
    /// TCP客户端操作类
    /// </summary>
    public class TCPClient : PortBase
    {
        /// <summary>
        /// 远程端点
        /// </summary>
        private IPEndPoint LongIpEnd = null;
        /// <summary>
        /// socket实例
        /// </summary>
        private Socket CLSocket = null;
        /// <summary>
        /// 收到的数据实际长度
        /// </summary>
        private int RcvLen = 0;
        /// <summary>
        /// 接收缓冲区最大长度
        /// </summary>
        private const int RCVBUFLEN = 256;
        /// <summary>
        /// 接收缓冲区
        /// </summary>
        private byte[] m_softBuff = new byte[RCVBUFLEN];
        /// <summary>
        /// 接收数据子线程
        /// </summary>
        private Thread thdSoft;
        /// <summary>
        /// 连接是否成功的标志
        /// </summary>
        public bool blConntOk = false;
        /// <summary>
        /// 控制通信通断
        /// </summary>
        public bool isCanConnect = true;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        public TCPClient(int type)
        {

        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="SendBuff"></param>
        public override void SendData(byte[] SendBuff)
        {             //设置目标设备IP、Port----------------------------------------
            try
            {
                RcvLen = 0;
                if (!CLSocket.Connected && isCanConnect)
                {
                    Open();
                }
                else
                {
                    CLSocket.Send(SendBuff);
                }
                //CLSocket.Send(SendBuff);
            }
            catch (Exception e)
            {
                SendErrMsg(e);
            }
        }
        /// <summary>
        /// 打开连接
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            try
            {
                CLSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                LongIpEnd = new IPEndPoint(IPAddress.Parse(Ipaddress), RemotePort);

                //  CLSocket.Connect(LongIpEnd);//YF

                //异步方式进行连接的远程服务器的IP地址和端口号  YF
                IAsyncResult result = CLSocket.BeginConnect(LongIpEnd,
                    null,
                    null);
                result.AsyncWaitHandle.WaitOne(500);
                //CLSocket.Connect(LongIpEnd);
                blConntOk = CLSocket.Connected;
                if (blConntOk)
                {
                    thdSoft = new Thread(new ThreadStart(SoftListen));
                    //设置为后台

                    thdSoft.IsBackground = true;
                    thdSoft.Start();
                }
                return true;
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
                return false;
            }
        }

        /// <summary>
        /// 监听
        /// </summary>
        private void SoftListen()
        {
            int nLen = 0;
            byte[] rcvBuff = new byte[12048];
            try
            {
                for (int n = 0; n < 12048; n++) rcvBuff[n] = 0;

                while (true)
                {
                    if(!CLSocket.Connected)
                    {
                        blConntOk = false;
                        break;
                    }

                    //获得 主模块 发送过来的数据包-----
                    nLen = CLSocket.Receive(rcvBuff);

                    if (nLen > 0)
                    {
                        m_softBuff = rcvBuff;
                        RcvLen = nLen;
                        //AddBuffData(rcvBuff);
                        AddBuffData(rcvBuff.Skip(0).Take(RcvLen).ToArray());
                    }
                    Thread.Sleep(5);
                }
            }
            catch (Exception Ex)
            {
                CLSocket.Close();
                blConntOk = false;
                //SendErrMsg(Ex);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public override bool Close()
        {
            try
            {
                //if (blConntOk == true)
                //{
                //    if (thdSoft.IsAlive == true)
                //    {
                        blConntOk = false;
                        CLSocket.Close();
                        thdSoft.Abort();
                        Thread.Sleep(250);
                        //CLSocket.Shutdown(SocketShutdown.Both);
                        //CLSocket.Close(3);
                //    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
                return false;
            }
        }
    }
}
