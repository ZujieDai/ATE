using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// 设备 - 响河交流源
    /// </summary>
    public class emtACSource_XH : EquipMentBase
    {
        /*
         * 功能码
         *  10H:		写设置参数、控制电源输出停止
	        03H:		读设置参数
	        04H:		读测量数据、状态
         * 
         * 
         */


        public ACSource_StateData stateData = new ACSource_StateData();
        private static object SynLock = new object();
        public emtACSource_XH(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            byte[] WriteBuffer = { 0x01, 0x10, 0xB3, 0xB0, 0x00, 0x01, 0x02, 0x00, 0x00, 0x3E, 0xAB };
            SendData(WriteBuffer);
        }

        public override void ACSource_OFF()
        {
            byte[] WriteBuffer = { 0x01, 0x10, 0xB3, 0xB1, 0x00, 0x01, 0x02, 0x00, 0x00, 0x3F, 0x7A };
            SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            //byte[] WriteBuffer = SetBuffer(41007, Convert.ToUInt16(freq * 1000));
            //SendData(WriteBuffer);


            Byte[] writeBuffer;

            try
            {
                UInt32 temp1 = 0;
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x10, 0xA0, 0x2E, 0x00, 0x02, 0x04 };//前缀
                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                temp1 = Convert.ToUInt32(freq * 1000);
                ReturnbyteSource.Add((byte)(temp1 >> 24));//0000  0000
                ReturnbyteSource.Add((byte)(temp1 >> 16));//0000  0000  //// 
                ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000  //// 
                ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 

                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节

            }
            catch
            {
                writeBuffer = new byte[] { 0x01, 0x10, 0xA0, 0x28, 0x00, 0x02, 0x04, 0x00, 0x03, 0x5B, 0x60, 0xC3, 0x0E };//前缀

            }
            SendData(writeBuffer);
        }
        public override void ACSource_SetVolt(double Volt)
        {
            // byte[] WriteBuffer = SetBuffer(41001, Convert.ToUInt16(Volt * 1000));
            /* 设定电压100V指令：01 10 A0 28 00 02 04 00 01 86 A0 3B CE//
                0186A0 = 100000 / 1000 = 100.000V
                返回：01 10 A0 28 00 02 E3 C0
            */
            Byte[] writeBuffer;
            try
            {
                UInt32 temp1 = 0;
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x10, 0xA0, 0x28, 0x00, 0x02, 0x04 };//前缀
                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                temp1 = Convert.ToUInt32(Volt * 1000);
                ReturnbyteSource.Add((byte)(temp1 >> 24));//0000  0000
                ReturnbyteSource.Add((byte)(temp1 >> 16));//0000  0000  //// 
                ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000  //// 
                ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 

                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节

            }
            catch
            {
                writeBuffer = new byte[] { 0x01, 0x10, 0xA0, 0x28, 0x00, 0x02, 0x04, 0x00, 0x03, 0x5B, 0x60, 0xC3, 0x0E };//前缀
            }

            SendData(writeBuffer);
        }

        public override void ACSource_SetOpenPhase()
        {
            //设置C相电压为0
            Byte[] writeBuffer = new byte[] { 0x01, 0x10, 0xA0, 0x35, 0x00, 0x02, 0x04, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x18 };
            SendData(writeBuffer);
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

            while (true)
            {
                if (AutoReadData)
                {
                    byte[] RevMsgData = null;

                    byte[] WriteBufferVolt = { 0x01, 0x04, 0x8C, 0x9F, 0x00, 0x08, 0xEB, 0x72 };

                    if (EquipMentPort != null)
                    {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                //DataBuf.Clear();
                                RevEquipMentData();
                                Thread.Sleep(50);
                                string strTemp = BitConverter.ToString(WriteBufferVolt).Replace('-', ' ');

                            lock (SynLock)
                            {
                                EquipMentPort.SendData(WriteBufferVolt);
                                RevMsgData = RevEquipMentData();
                            }
                                if (RevMsgData != null)
                                {
                                    stateData = GetStateData(RevMsgData, StateData);
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
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(stateData);
                    }
                    Thread.Sleep(300);
                }
            }
        }

        private new bool SendData(byte[] WriteBuffer)
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



        private ACSource_StateData GetStateData(byte[] buffer, ACSource_StateData StateData)
        {
            try
            {
                StateData.ChargerID = this.ChargerID;

                if (buffer != null && buffer.Length >= 20)
                {
                    //注意旧版的系数是100
                    int temp = 0;
                    temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.Contains("LJ"))
                        StateData.Volt = Convert.ToSingle(temp) / 100;
                    else
                        StateData.Volt = Convert.ToSingle(temp) / 1000;
                    //通讯协议只有单相数据
                    StateData.Volt_B = StateData.Volt;
                    StateData.Volt_C = StateData.Volt;
                    temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                    StateData.Current = Convert.ToSingle(temp) / 1000;
                    //temp = Convert.ToInt32(buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16);
                    //StateData.Freq = Convert.ToSingle(temp);
                    StateData.Freq = 50;
                }
                return StateData;
            }
            catch { return StateData; }
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="tRegAdd"></param>
        /// <param name="tData"></param>
        /// <param name="ReadType">0X03-读设置参数，  0x04  读测量数据读测量数据、状态</param>
        /// <returns></returns>
        public byte[] GetBuffer(UInt16 tRegAdd, UInt16 tData, byte ReadType)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, ReadType };//前缀

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
