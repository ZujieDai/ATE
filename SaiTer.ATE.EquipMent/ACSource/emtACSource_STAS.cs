using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备 - 博奥斯交流源
    /// </summary>
    public class emtACSource_STAS : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        private static object SynLock = new object();
        public emtACSource_STAS(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {

            //byte[] WriteBuffer = SetBuffer(0, 255);
            byte[] WriteBuffer = { 0x01, 0x06, 0x00, 0x00, 0xFF, 0x00, 0xC8, 0x3A };
            SendData(WriteBuffer);

        }

        public override void ACSource_OFF()
        {
            //byte[] WriteBuffer = SetBuffer(1, 255);
            byte[] WriteBuffer = { 0x01, 0x06, 0x00, 0x01, 0xFF, 0x00, 0x99, 0xFA };
            SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            byte[] WriteBuffer = SetBuffer(11, Convert.ToUInt16(freq * 10));
            SendData(WriteBuffer);
        }
        public override void ACSource_SetVolt(double Volt)
        {
            byte[] WriteBuffer = SetBuffer(10, Convert.ToUInt16(Volt * 10));

            SendData(WriteBuffer);
        }

        public override void ACSource_DisConnect()
        {
            //byte[] WriteBuffer = SetBuffer();

            //SendData(WriteBuffer);
        }



        public override void ACSource_ReadState()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicACSource_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicACSource_StateData.Add(ChargerID, stateData);
            }
            ACSource_StateData StateData = new ACSource_StateData();
            //StateData.ACSourceName = "博奥斯";
            while (true)
            {

                if (AutoReadData)
                {
                    byte[] RevMsgData = null;
                    byte[] AC60_33600Bytes = { 0x01, 0x03, 0x00, 0x02, 0x00, 0x15, 0x25, 0xC5 };//读取个21寄存器，包括CRC

                    byte[] WriteBufferVolt = GetBuffer(20, 3);//电压
                    byte[] WriteBufferDisplay = { 0x01, 0x03, 0x00, 0x11, 0x00, 0x03, 0x55, 0xCE }; //标志位
                    byte[] WriteBufferFreq = GetBuffer(11, 0);  //频率
                                                                //
                    byte[] WriteBufferCurrent = GetBuffer(2, 3);//电流

                    if (EquipMentPort != null)
                    {

                        for (int i = 0; i < ReConnNum; i++)
                        {
                            //DataBuf.Clear();
                            RevEquipMentData();
                            Thread.Sleep(50);
                            //lock (SynLock)
                            //{
                            //    EquipMentPort.SendData(AC60_33600Bytes);
                            //    RevMsgData = RevEquipMentData();
                            //}

                            string strTemp = BitConverter.ToString(WriteBufferVolt).Replace('-', ' ');
                            //SendMsgToFile("交流源发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                EquipMentPort.SendData(WriteBufferVolt);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {

                                stateData = GetStateData(RevMsgData, "Volt", StateData);
                                SystemEvent.SendMonitorMessage(stateData);

                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferDisplay);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (RevMsgData != null && RevMsgData.Length >= 11)
                                {
                                    byte display = (byte)RevMsgData[4];
                                    switch (display)
                                    {
                                        case 0x00:
                                            resolution = 1000;
                                            break;
                                        case 0x0F:
                                            resolution = 100;
                                            break;
                                        case 0xFF:
                                            resolution = 10;
                                            break;
                                    }
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferCurrent);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "Cur", StateData);
                                SystemEvent.SendMonitorMessage(stateData);

                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferFreq);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "Freq", StateData);
                                SystemEvent.SendMonitorMessage(stateData);


                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile("读交流源状态数据失败");
                                stateData = new ACSource_StateData();
                                stateData.ChargerID = this.ChargerID;
                                SystemEvent.SendMonitorMessage(stateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile("交流源通道对象不存在，请检查");
                        stateData = new ACSource_StateData();
                        stateData.ChargerID = this.ChargerID;
                        //stateData.ACSourceName = "博奥斯";
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(stateData);
                    }
                    Thread.Sleep(300);
                }
            }
        }

        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            Thread.Sleep(500);
            lock (SynLock)
            {
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        // SendMsgToFile("交流源发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile("交流源通道对象不存在，请检查交流源通道");
                    AutoReadData = true;
                    return false;
                }
            }
        }
        /// <summary>
        /// 校验报文
        /// </summary>
        /// <param name="tbytes"></param>
        /// <returns></returns>
        public bool CheckBuffer(byte[] tbytes)
        {
            bool isCheck = false;
            try
            {
                if (tbytes[tbytes.Length - 2] == 0x0D && tbytes[tbytes.Length - 1] == 0x0A)
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }

        int resolution = 10;//电流显示分辨率

        private ACSource_StateData GetStateData(byte[] buffer, string buffType, ACSource_StateData StateData)
        {
            try
            {
                StateData.ChargerID = this.ChargerID;
                //StateData.ACSourceName = "博奥斯";
                if (buffer != null && buffer.Length >= 11)
                {
                    int temp = 0;
                    switch (buffType)
                    {
                        case "Volt":
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Volt = Convert.ToSingle(temp) / 10;
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Volt_B = Convert.ToSingle(temp) / 10;
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Volt_C = Convert.ToSingle(temp) / 10;
                            break;
                        case "Cur":
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Current = Convert.ToSingle(temp) / resolution;
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Current_B = Convert.ToSingle(temp) / resolution;
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2"), 16);
                            StateData.Current_C = Convert.ToSingle(temp) / resolution;
                            break;
                        case "Freq":
                            StateData.Freq = Convert.ToSingle(temp) / 10;
                            break;

                    }
                }

                return StateData;
            }
            catch { return StateData; }
        }
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="tRegAdd"></param>
        /// <param name="tData"></param>
        /// <returns></returns>
        public byte[] SetBuffer(UInt16 tRegAdd, UInt16 tData)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x06 };//前缀

            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add((byte)(tRegAdd >> 8));//寄存器地址高位
            ReturnbyteSource.Add((byte)tRegAdd);//寄存器地址低位

            ReturnbyteSource.Add((byte)(tData >> 8));//写入的数据 高位
            ReturnbyteSource.Add((byte)tData);//写入的数据 低位

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="tRegAdd"></param>
        /// <param name="tData"></param>
        /// <returns></returns>
        public byte[] GetBuffer(UInt16 tRegAdd, UInt16 tData)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x03 };//前缀

            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add((byte)(tRegAdd >> 8));//寄存器地址高位
            ReturnbyteSource.Add((byte)tRegAdd);//寄存器地址低位

            ReturnbyteSource.Add((byte)(tData >> 8));//写入的数据 高位
            ReturnbyteSource.Add((byte)tData);//写入的数据 低位

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

    }
}
