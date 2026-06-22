using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.PortManage.PortType
{
    public delegate List<byte> ReceiveEvent(List<byte> RcvBytes);

    /// <summary>
    /// UDP通讯类，用于A+程序通讯
    /// </summary>
    public class UdpIpPort
    {
        #region EquipPort（旧代码的结构）
        public bool IsOpen;
        public List<byte> LstRcvBytes;
        protected int ReadBytesCount;
        protected int ReceiveReadCount;

        protected bool IsReceived;
        protected ReceiveEvent bcRcvEvent;
        protected byte[] bcEndFlag;
        protected ReceiveEvent cRcvEvent;

        protected int DelayTime;
        protected byte[] cOutEndFlag;

        protected List<byte> ReceiveEvent1(List<byte> RcvBytes)
        {

            if (LstRcvBytes == null)
            {
                LstRcvBytes = new List<byte>();

            }
            else
            { LstRcvBytes.Clear(); }
            LstRcvBytes.AddRange(RcvBytes);

            return LstRcvBytes;
        }
        #endregion

        Socket SocketClient = null;

        IPEndPoint LocatePoint = null;//本地端口号

        IPEndPoint RemotePoint = null;//远程端口号
        Thread ThreadClient = null;
        string LocalIp = "127.0.0.1:9000";
        string RemoteIp = "127.0.0.1:9980";

        public UdpIpPort()
        {
            IsOpen = false;
        }
        public void Connect()
        {
            try
            {
                ClosePort();
                Thread.Sleep(500);
                String[] LocalStringS = LocalIp.Split(':');
                if (LocalStringS.Length >= 2)
                {

                    IPAddress locateIp = IPAddress.Parse(LocalStringS[0]);//本地IP地址

                    LocatePoint = new IPEndPoint(locateIp, Convert.ToInt32(LocalStringS[1]));//本地端口 9000

                    SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    SocketClient.Bind(LocatePoint);

                    ThreadClient = new Thread(ReceiveMsg); //监听创建好后，就开始接收信息，并创建一个线程
                    ThreadClient.IsBackground = true;//指示该线程为后台线程。后台线程将会随着主线程的退出而退出
                    ThreadClient.Start();
                    IsReceive = true;
                    IsOpen = true;


                }

                String[] RemoteStringS = RemoteIp.Split(':');
                if (RemoteStringS.Length >= 2)
                {
                    IPAddress remoteIp = IPAddress.Parse(RemoteStringS[0]);//远程的IP地址,使用本地的IP地址
                    RemotePoint = new IPEndPoint(remoteIp, Convert.ToInt32(RemoteStringS[1]));//远程的IP端口号

                }


            }
            catch (Exception eex)
            {
                ClosePort();
            }
        }

        public void Connect(string tRemoteIp)
        {
            RemoteIp = tRemoteIp;
            Connect();
        }
        public void Connect(string tLocalIp, string tRemoteIp)
        {

            LocalIp = tLocalIp;
            RemoteIp = tRemoteIp;
            Connect();
        }
        ////////////////////////////
        int ErrNum = 0;
        bool IsReceive = true;
        void ReceiveMsg()
        {
            EndPoint RvPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {


                try
                {

                    List<byte> tmpLstBytes = new List<byte>();
                    while (IsReceive)
                    {



                        byte[] buf = new byte[1024 * 1024 * 1];
                        int length = 0;
                        try
                        {
                            length = SocketClient.ReceiveFrom(buf, ref RvPoint);
                            if (length > 0)
                            {
                                if (ReadBytesCount != 0)
                                {
                                    if (length >= ReadBytesCount)
                                    {
                                        byte[] tmpbuf = new byte[ReadBytesCount];
                                        Array.Copy(buf, tmpbuf, ReadBytesCount);
                                        cRcvEvent(tmpbuf.ToList<byte>());
                                        IsReceived = true;
                                    }
                                }
                                else
                                {
                                    if (cOutEndFlag != null)
                                    {


                                        byte[] tmpbuf = new byte[length];
                                        Array.Copy(buf, tmpbuf, length);
                                        tmpLstBytes.AddRange(tmpbuf);
                                        if (tmpbuf[length - 1] == cOutEndFlag[cOutEndFlag.Length - 1] && tmpbuf[length - 2] == cOutEndFlag[cOutEndFlag.Length - 2])
                                        {
                                            cRcvEvent(tmpLstBytes);
                                            tmpLstBytes.Clear();
                                            IsReceived = true;
                                        }


                                    }
                                    else
                                    {
                                        byte[] tmpbuf = new byte[length];
                                        Array.Copy(buf, tmpbuf, length);
                                        cRcvEvent(tmpbuf.ToList<byte>());
                                        IsReceived = true;
                                    }

                                }
                            }

                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(10);
                        }


                        //////          
                    }
                }
                catch (Exception ex)
                {
                    IsReceive = false;
                }
                Thread.Sleep(20);
            }
        }

        public void ClosePort()
        {
            try
            {
                if (SocketClient != null)
                {

                    SocketClient.Close();
                    SocketClient.Dispose();
                }
                if (ThreadClient != null)
                    ThreadClient.Abort();
            }
            catch
            {

            }
            IsReceive = false;
            IsOpen = false;

        }

        public void Send(byte[] SendBytes)
        {

            try
            {
                ReadBytesCount = 0;
                //A+端口号9980  ，AST9000端口是9000

                if (SocketClient != null && SendBytes.Length != 0)
                {
                    SocketClient.SendTo(SendBytes, RemotePoint);
                }
                ErrNum = 0;
            }
            catch
            {
                ErrNum++;
                if (ErrNum >= 3)
                {
                    ClosePort();
                }
            }
            //



        }
        public bool Send(string AsciiStr)
        {
            byte[] cSendBytes = System.Text.Encoding.Default.GetBytes(AsciiStr);
            this.Send(cSendBytes);
            return true;
        }

        public List<byte> SendRecieve(byte[] tSendBytes, int tReadBytesCount, int tTimeOut)
        {

            if (LstRcvBytes == null)
            { LstRcvBytes = new List<byte>(); }
            else
            { LstRcvBytes.Clear(); }
            ReadBytesCount = tReadBytesCount;
            if (tTimeOut == 0 && DelayTime > 0)
            {
                tTimeOut = DelayTime;
            }
            cRcvEvent = ReceiveEvent1;
            IsReceived = false;
            try
            {
                List<byte> LstSend = new List<byte>();
                LstSend.AddRange(tSendBytes);
                SocketClient.SendTo(LstSend.ToArray(), RemotePoint);
                ErrNum = 0;
            }
            catch
            {
                ErrNum++;
                if (ErrNum >= 3)
                {
                    ClosePort();
                }
                return new List<byte>();
            }

            int wTime = 0;
            while (wTime < tTimeOut && !IsReceived)
            {
                System.Threading.Thread.Sleep(10);
                wTime += 10;
            }

            ReadBytesCount = 0;
            if (bcRcvEvent != null)
                cRcvEvent = bcRcvEvent;

            if (!IsReceived)
            {
                return new List<byte>();
            }
            return LstRcvBytes;
        }

        public string SendRecieve(string AsciiSendStr, int tReadBytesCount, int tTimeOut)
        {
            byte[] tSendBytes = System.Text.Encoding.ASCII.GetBytes(AsciiSendStr);
            List<byte> RcvBytes = SendRecieve(tSendBytes, tReadBytesCount, tTimeOut);

            if (RcvBytes.Count == 0)
            {
                IsOpen = false;//
                return "NULL";
            }
            else
            {
                return System.Text.Encoding.ASCII.GetString(RcvBytes.ToArray());
            }
        }

        public List<byte> SendRecieve(byte[] tSendBytes, byte[] tOutEndFlag, byte[] tInEndFlag, int tTimeOut)
        {

            ReadBytesCount = 0;
            if (LstRcvBytes == null)
            { LstRcvBytes = new List<byte>(); }
            else
            { LstRcvBytes.Clear(); }
            if (tTimeOut == 0 && DelayTime > 0)
            {
                tTimeOut = DelayTime;
            }

            if (tOutEndFlag != null)
                cOutEndFlag = tOutEndFlag;
            cRcvEvent = ReceiveEvent1;
            IsReceived = false;


            try
            {
                List<byte> LstSend = new List<byte>();
                LstSend.AddRange(tSendBytes);
                if (tInEndFlag != null)
                {
                    LstSend.AddRange(tInEndFlag);
                }
                SocketClient.SendTo(LstSend.ToArray(), RemotePoint);
                ErrNum = 0;
            }
            catch
            {
                ErrNum++;
                if (ErrNum >= 3)
                {
                    ClosePort();
                }

                return new List<byte>();
            }
            int wTime = 0;
            while (wTime < tTimeOut && !IsReceived)
            {
                System.Threading.Thread.Sleep(10);
                wTime += 10;
            }

            if ((tOutEndFlag != null))
            {
                if (bcEndFlag != null)
                { cOutEndFlag = bcEndFlag; }
                else
                {
                    cOutEndFlag = null;
                }

            }
            if (bcRcvEvent != null)
                cRcvEvent = bcRcvEvent;

            if (!IsReceived)
            {
                return new List<byte>();
            }
            return LstRcvBytes;
        }

        /// <summary>
        /// 发送并等待接收
        /// </summary>
        /// <param name="AsciiSendStr">发送的字节</param>
        /// <param name="AsciiOutEndFlag">校验回复的结尾是否符合</param>
        /// <param name="AsciiInEndFlag"></param>
        /// <param name="tTimeOut"></param>
        /// <returns></returns>
        public string SendRecieve(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag, int tTimeOut)
        {
            byte[] tSendBytes = System.Text.Encoding.ASCII.GetBytes(AsciiSendStr);

            byte[] tEndBytes = null;
            if (!string.IsNullOrEmpty(AsciiOutEndFlag))
            {
                tEndBytes = System.Text.Encoding.ASCII.GetBytes(AsciiOutEndFlag);
            }

            byte[] tInEndBytes = null;
            if (!string.IsNullOrEmpty(AsciiInEndFlag))
                tInEndBytes = System.Text.Encoding.ASCII.GetBytes(AsciiInEndFlag);


            List<byte> RcvBytes = SendRecieve(tSendBytes, tEndBytes, tInEndBytes, tTimeOut);

            if (RcvBytes.Count == 0)
            {
                IsOpen = false;//
                return "NULL";
            }
            else
            {
                return System.Text.Encoding.ASCII.GetString(RcvBytes.ToArray());
            }
        }
        public void Receive(byte[] tOutEndFlag, ReceiveEvent tRcvEvent)
        {
            ReadBytesCount = 0;
            if (tOutEndFlag != null)
            {
                cOutEndFlag = tOutEndFlag;
                bcEndFlag = tOutEndFlag;
            }
            cRcvEvent = tRcvEvent;
            bcRcvEvent = tRcvEvent;
            IsReceived = false;
        }
        public void Receive(string tOutEndFlag, ReceiveEvent tRcvEvent)
        {
            ReadBytesCount = 0;
            byte[] tEndBytes = null;
            if (tOutEndFlag != "")
                tEndBytes = System.Text.Encoding.ASCII.GetBytes(tOutEndFlag);
            this.Receive(tEndBytes, tRcvEvent);
            IsReceived = false;
        }

    }
}
