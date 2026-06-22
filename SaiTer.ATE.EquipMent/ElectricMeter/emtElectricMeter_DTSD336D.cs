using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    public class emtElectricMeter_DTSD336D : EquipMentBase
    {
        public byte iAdd = 1;
        private object locker = new object();

        public emtElectricMeter_DTSD336D(int type)
        {
            this.AutoReadData = true ;
            this.EquipMentName = LanguageManager.GetByKey("电表");
        }

        public override double EM_GetKeyValue(int JcqAdd, int JcqCount, double XZoom)
        {
            double dtmp = -1;
            //AutoReadData = false;
            double[] BMSError = new double[3];
            try
            {
                List<bool> result = new List<bool>();

                byte[] WriteBuffer = GetMsgFrame_Read(JcqAdd, JcqCount);
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();
                if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                {
                    List<byte> getBuffer = RevMsgData.ToList().GetRange(4, JcqCount * 2);
                    byte[] buffer = getBuffer.ToArray();
                    Array.Reverse(buffer);
                    uint aa = BitConverter.ToUInt32(buffer, 0);
                    dtmp = aa * XZoom;

                }
                //AutoReadData = true;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return dtmp;

        }

        public override double EM_GetTotalPower()
        {
            double dtmp = -1;
            AutoReadData = false;
            Thread.Sleep(200);
            int JcqAdd = 0x17A;
            int JcqCount = 0x02;
            double XZoom = 0.001;
            try
            {
                List<bool> result = new List<bool>();

                byte[] WriteBuffer = GetMsgFrame_Read(JcqAdd, JcqCount);
                byte[] RevMsgData;
                //EquipMentPort.SendData(WriteBuffer);
                lock (locker)
                {
                    SendData(WriteBuffer);
                    RevMsgData = RevEquipMentData();
                }
                if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                {
                    List<byte> getBuffer = RevMsgData.ToList().GetRange(3, JcqCount * 2);
                    byte[] buffer = getBuffer.ToArray();
                    Array.Reverse(buffer);
                    uint aa = BitConverter.ToUInt32(buffer, 0);
                    dtmp = aa * XZoom;
                }
                else//失败重发一次
                {
                    //EquipMentPort.SendData(WriteBuffer);
                    lock (locker)
                    {
                        SendData(WriteBuffer);
                        RevMsgData = RevEquipMentData();
                    }
                    if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                    {
                        List<byte> getBuffer = RevMsgData.ToList().GetRange(3, JcqCount * 2);
                        byte[] buffer = getBuffer.ToArray();
                        Array.Reverse(buffer);
                        uint aa = BitConverter.ToUInt32(buffer, 0);
                        dtmp = aa * XZoom;
                    }
                }
                AutoReadData = true;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return dtmp;

        }

        public override double[] EM_GetVolt()
        {
            double dtmp = -1;
            AutoReadData = false;
            Thread.Sleep(200);
            int JcqAdd = 0x16E;
            int JcqCount = 0x0E;
            double XZoom = 0.0001;
            try
            {
                List<bool> result = new List<bool>();

                byte[] WriteBuffer = GetMsgFrame_Read(JcqAdd, JcqCount);
                byte[] RevMsgData;
                //EquipMentPort.SendData(WriteBuffer);
                lock (locker)
                {
                    SendData(WriteBuffer);
                    RevMsgData = RevEquipMentData();
                }
                if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                {
                    List<byte> getBuffer = RevMsgData.ToList().GetRange(7, 4);
                    byte[] buffer = getBuffer.ToArray();
                    Array.Reverse(buffer);
                    uint aa = BitConverter.ToUInt32(buffer, 0);
                    dtmp = aa * XZoom;
                }
                else//失败重发一次
                {
                    //EquipMentPort.SendData(WriteBuffer);
                    lock (locker)
                    {
                        SendData(WriteBuffer);
                        RevMsgData = RevEquipMentData();
                    }
                    if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                    {
                        List<byte> getBuffer = RevMsgData.ToList().GetRange(7, 4);
                        byte[] buffer = getBuffer.ToArray();
                        Array.Reverse(buffer);
                        uint aa = BitConverter.ToUInt32(buffer, 0);
                        dtmp = aa * XZoom;
                    }
                }
                AutoReadData = true;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return new double[3] { dtmp, dtmp, dtmp };
        }

        public override double[] EM_GetCurrent()
        {
            return base.EM_GetCurrent();
        }

        public override double[] EM_GetPower()
        {
            return base.EM_GetPower();
        }

        public override double[] EM_GetPowerFactor()
        {
            return base.EM_GetPowerFactor();
        }

        public override double[] EM_GetPhaseAngle()
        {
            return base.EM_GetPhaseAngle();
        }


        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            Thread.Sleep(500);
            if (EquipMentPort != null)
            {
                for (int i = 0; i < ReConnNum; i++)
                {
                    DataBuf.Clear();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    // SendMsgToFile("电子负载发送数据：" + strTemp);
                    EquipMentPort.SendData(WriteBuffer);
                }
                AutoReadData = true;
                return true;
            }
            else
            {
                SendMsgToFile("电表通道对象不存在，请检查电表通道");
                AutoReadData = true;
                return false;
            }
        }


        public byte[] GetMsgFrame_Read(int JcqAdd, int JcqCount)
        {
            List<byte> MsgFrame = new List<byte>();
            MsgFrame.Add(iAdd);//地址
            MsgFrame.Add(0x03);//功能码：读取
            MsgFrame.Add((byte)(JcqAdd >> 8));
            MsgFrame.Add((byte)(JcqAdd));//寄存器起始地址2个字节
            MsgFrame.Add((byte)(JcqCount >> 8));
            MsgFrame.Add((byte)(JcqCount));//寄存器长度2个字节
            MsgFrame.AddRange(CheckOut.ModBus_RTU_CRC16(MsgFrame.ToArray()));//添加校验码

            return MsgFrame.ToArray();
        }


        public override void Read_EMState()
        {
            try
            {
                SystemEvent.SendConnectState(false, this);
                int JcqAdd = 0x16E;
                int JcqCount = 0x0E;
                double XZoom = 0.0001;
                byte[] WriteBuffer = GetMsgFrame_Read(JcqAdd, JcqCount);
                while (true)
                {
                    if (this.AutoReadData)
                    {
                        lock (locker)
                        {
                            if (this.AutoReadData)
                            {
                                //byte[] writeBuffer = GetBuffer("*idn?", "\r\n", "\r\n");
                                string strResult = string.Empty;
                                if (EquipMentPort != null)
                                {
                                    DataBuf.Clear();
                                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                    // SendMsgToFile("安规发送数据：" + strTemp);
                                    EquipMentPort.SendData(WriteBuffer);
                                    byte[] RevMsgData = RevEquipMentData();
                                    if (RevMsgData != null)
                                    {
                                        strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                    else
                                    {
                                        SystemEvent.SendConnectState(false, this);
                                    }
                                    // SystemEvent.SendMonitorMessage(strResult);
                                }
                                else
                                {
                                    SystemEvent.SendConnectState(false, this);
                                    //SendMsgToFile("安规通道对象不存在，请检查安规通道");
                                }
                            }
                        }
                    }
                    Thread.Sleep(300);
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);

            }
        }


    }
}
