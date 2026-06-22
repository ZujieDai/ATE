using SaiTer.ATE.DataModel.EquipStateData;
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
    /// <summary>
    /// 交流源——爱科赛博
    /// </summary>
    public class emtACSource_AKSB : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        private static object SynLock = new object();
        public emtACSource_AKSB(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            int waitTime = 2000;
            // 需要先开机
            byte[] WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x02, 0xDB, 0x95 };
            SendData(WriteBuffer);
            Thread.Sleep(waitTime);

            //byte[] WriteBuffer = SetBuffer(0, 255);
            // 启动输出（运行）
            WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x03, 0x1A, 0x55 };
            SendData(WriteBuffer);
            Thread.Sleep(waitTime);

            // 输出吸合（接通）
            WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x08, 0x5B, 0x92 };
            SendData(WriteBuffer);
            Thread.Sleep(waitTime);

            ////以下因为交流源上电后需要发送两边启动命令，这里暂时发两遍
            // 需要先开机
            ////WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x02, 0xDB, 0x95 };
            ////SendData(WriteBuffer);
            ////Thread.Sleep(1000);

            //////byte[] WriteBuffer = SetBuffer(0, 255);
            ////// 启动输出
            ////WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x03, 0x1A, 0x55 };
            ////SendData(WriteBuffer);
            ////Thread.Sleep(1000);

            ////// 输出吸合
            ////WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x08, 0x5B, 0x92 };
            ////SendData(WriteBuffer);
            ////Thread.Sleep(1000);
        }

        public override void ACSource_OFF()
        {
            // 输出断开
            byte[] WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x09, 0x9A, 0x52 };
            SendData(WriteBuffer);
            Thread.Sleep(1000);

            //不用退出运行状态
            //WriteBuffer = new byte[8] { 0x01, 0x06, 0x09, 0x09, 0x00, 0x04, 0x5B, 0x97 };
            //SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            Byte[] writeBuffer;
            try
            {
                UInt32 temp1 = 0;
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x10, 0x0C, 0x0F, 0x00, 0x02, 0x04 };//前缀
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
                writeBuffer = new byte[] { 0x01, 0x10, 0x0C, 0x0F, 0x00, 0x02, 0x04, 0x00, 0x00, 0xC3, 0x50, 0xB6, 0x23 };//前缀
            }
            SendData(writeBuffer);
        }

        public override void ACSource_SetVolt(double Volt)
        {
            Byte[] writeBuffer;
            try
            {
                UInt32 temp1 = 0;
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x10, 0x0C, 0x00, 0x00, 0x02, 0x04 };//前缀
                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                temp1 = Convert.ToUInt32(Volt * 100);
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
                writeBuffer = new byte[] { 0x01, 0x10, 0x0C, 0x00, 0x00, 0x02, 0x04, 0x00, 0x00, 0x55, 0xF0, 0x99, 0xBB };//前缀
            }

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
            //StateData.ACSourceName = "爱科赛博";
            while (true)
            {

                if (AutoReadData)
                {
                    byte[] RevMsgData = null;

                    byte[] WriteBufferVolt = GetBuffer(0x003C, 6);//电压
                    byte[] WriteBufferFreq = GetBuffer(0x005C, 2);  //频率
                                                                    //
                    byte[] WriteBufferCurrent = GetBuffer(0x0062, 6);//电流

                    if (EquipMentPort != null)
                    {

                        for (int i = 0; i < ReConnNum; i++)
                        {
                            //DataBuf.Clear();
                            RevEquipMentData();
                            Thread.Sleep(50);

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
                        SendMsgToFile("交流源发送数据：" + strTemp);
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

        private ACSource_StateData GetStateData(byte[] buffer, string buffType, ACSource_StateData StateData)
        {
            try
            {
                StateData.ChargerID = this.ChargerID;
                //StateData.ACSourceName = "博奥斯";
                if (buffer != null && buffer.Length >= 8)
                {
                    int temp = 0;
                    switch (buffType)
                    {
                        case "Volt":
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                            StateData.Volt = Convert.ToSingle(temp) / 100;
                            temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                            StateData.Volt_B = Convert.ToSingle(temp) / 100;
                            temp = Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                            StateData.Volt_C = Convert.ToSingle(temp) / 100;
                            break;
                        case "Cur":
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                            StateData.Current = Convert.ToSingle(temp) / 100;
                            temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                            StateData.Current_B = Convert.ToSingle(temp) / 100;
                            temp = Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                            StateData.Current_C = Convert.ToSingle(temp) / 100;
                            break;
                        case "Freq":
                            temp = Convert.ToInt32(buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                            StateData.Freq = Convert.ToSingle(temp) / 1000;
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
