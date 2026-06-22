using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 电表——中创智合
    /// </summary>
    public class emtElectricMeter_ZH4041 : EquipMentBase
    {
        public byte iAdd = 1;
        private object locker = new object();

        public emtElectricMeter_ZH4041(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "ZH" + LanguageManager.GetByKey("电表");
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

        public override double EM_GetTotalPower_ZH()
        {
            double totalPower = -1;
            int JcqAdd = 0x000C;
            int JcqCount = 0x01;
            AutoReadData = false;
            Thread.Sleep(200);
            try
            {
                List<bool> result = new List<bool>();

                //byte[] WriteBuffer = new byte[] { 0x01, 0x03, 0x00, 0x0C, 0x00, 0x01, 0x44, 0x09 };
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
                    List<byte> getBuffer = RevMsgData.ToList().GetRange(3, 2);
                    byte[] buffer = getBuffer.ToArray();
                    Array.Reverse(buffer);
                    uint aa = BitConverter.ToUInt16(buffer, 0);
                    //有符号,值=DATA/10000*3*电压量程*电流量程（电流单位mA需要除1000）
                    totalPower = aa / 10000.0 * 3 * 400.0 * 100.0 / 1000.0;
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
                        List<byte> getBuffer = RevMsgData.ToList().GetRange(3, 2);
                        byte[] buffer = getBuffer.ToArray();
                        Array.Reverse(buffer);
                        uint aa = BitConverter.ToUInt16(buffer, 0);
                        totalPower = aa / 10000.0 * 3 * 400.0 * 100.0 / 1000.0;
                    }
                }
                AutoReadData = true;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return totalPower;

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
                    SendMsgToFile("电子负载发送数据：" + strTemp);
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

        public override void Read_EMState()
        {
            try
            {
                SystemEvent.SendConnectState(false, this);
                int JcqAdd = 0x000C;
                int JcqCount = 0x01;
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
                                    EquipMentPort.SendData(WriteBuffer);
                                    byte[] RevMsgData = RevEquipMentData();
                                    if (RevMsgData != null && CheckOut.CheckModBus_RTU_CRC16(RevMsgData))
                                    {
                                        strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                    else
                                    {
                                        SystemEvent.SendConnectState(false, this);
                                    }
                                }
                                else
                                {
                                    SystemEvent.SendConnectState(false, this);
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
