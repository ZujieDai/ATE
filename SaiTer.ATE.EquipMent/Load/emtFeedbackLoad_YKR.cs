using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{

    /// <summary>
    /// 多通道回馈负载  采用ModBus通讯  最大支持 64通道
    /// </summary>

    public class emtFeedbackLoad_YKR : EquipMentBase
    {
        public readonly static object SynLockLoad = new object();
        public FeedbackLoad_StateData StateData = new FeedbackLoad_StateData();

        public static bool isWriting;
        public readonly static object lockWirte = new object();

        private readonly object _lockObject = new object();

        private const byte SlaveAddress = 0x01; //从机地址

        // 通道地址范围
        private const byte MIN_CHANNEL_ADDRESS = 0x01;
        private const byte MAX_CHANNEL_ADDRESS = 0x40;

        // 功能码
        private const byte READ_COILS = 0x01;
        private const byte READ_INPUT_STATUS = 0x02;
        private const byte READ_HOLDING_REGISTERS = 0x03;
        private const byte READ_INPUT_REGISTERS = 0x04;
        private const byte WRITE_SINGLE_COIL = 0x05;
        private const byte WRITE_SINGLE_REGISTER = 0x06;
        private const byte WRITE_MULTIPLE_COILS = 0x0F;
        private const byte WRITE_MULTIPLE_REGISTERS = 0x10;


        /// <summary>
        /// 工作模式 1恒压 2恒流
        /// </summary>
        public enum WorkMode
        {
            ConstantVoltage = 1,
            ConstantCurrent = 2
        }

        public emtFeedbackLoad_YKR(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("回馈负载");
        }

        #region 写入寄存器操作

        /// <summary>
        /// 写入单个寄存器
        /// </summary>
        public bool WriteSingleRegister(int channelAddress, byte registerAddress, ushort value)
        {
            byte[] request = new byte[6];
            request[0] = SlaveAddress;
            request[1] = WRITE_SINGLE_REGISTER;
            request[2] = (byte)channelAddress;
            request[3] = registerAddress;
            request[4] = (byte)(value >> 8);
            request[5] = (byte)value;

            //byte[] crc = CalculateCRC(request, 6);
            byte[] crc = CheckOut.GetModbusCrc16_High_Right(request);//CRC校验函数。
            byte[] fullRequest = new byte[8];
            Array.Copy(request, fullRequest, 6);
            fullRequest[6] = crc[0];
            fullRequest[7] = crc[1];

            //byte[] response = SendRequest(slaveAddress, fullRequest, WRITE_SINGLE_REGISTER);
            //return response != null;

            if (!isWriting)
            {
                lock (lockWirte)
                {
                    if (!isWriting)
                    {
                        isWriting = true;
                        try
                        {
                            SendData_FeedbackLoad(fullRequest, out byte[] RevMsgData);  //写入的先不做解析
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 写入多个寄存器
        /// </summary>
        public bool WriteMultipleRegisters(int channelAddress, ushort startAddress, byte[] values)
        {
            int byteCount = values.Length;
            int registerCount = values.Length / 2;
            List<byte> ReturnbyteSource = new List<byte>
            {
                SlaveAddress,                  //从机地址
                WRITE_MULTIPLE_REGISTERS,      //功能码
                (byte)channelAddress,          //通道号
                (byte)startAddress,            //功能
                (byte)(registerCount >> 8),
                (byte)(registerCount & 0xFF),
                (byte)(byteCount)
            };
            ReturnbyteSource.AddRange(values);

            //byte[] crc = CalculateCRC(request, request.Length);
            byte[] crc = CheckOut.GetModbusCrc16_High_Right(ReturnbyteSource.ToArray());//CRC校验函数。
            ReturnbyteSource.AddRange(crc);
            var WriteBuffer = ReturnbyteSource.ToArray();

            //byte[] response = SendRequest(slaveAddress, fullRequest, WRITE_MULTIPLE_REGISTERS);
            //return response != null;

            if (!isWriting)
            {
                lock (lockWirte)
                {
                    if (!isWriting)
                    {
                        isWriting = true;
                        try
                        {
                            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);  //写入的先不做解析
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 读取多个寄存器
        /// </summary>
        public bool ReadMultipleRegisters(int channelAddress, ushort startAddress, int registerCount, out byte[] RevMsgData)
        {
            RevMsgData = null;
            if (isWriting)
                return true;
            List<byte> ReturnbyteSource = new List<byte>
            {
                SlaveAddress,                  //从机地址
                READ_HOLDING_REGISTERS,      //功能码
                (byte)channelAddress,          //通道号
                (byte)startAddress,            //功能
                (byte)(registerCount >> 8),
                (byte)(registerCount & 0xFF)
            };

            //byte[] crc = CalculateCRC(request, request.Length);
            byte[] crc = CheckOut.GetModbusCrc16_High_Right(ReturnbyteSource.ToArray());//CRC校验函数。
            ReturnbyteSource.AddRange(crc);
            var WriteBuffer = ReturnbyteSource.ToArray();

            //byte[] response = SendRequest(slaveAddress, fullRequest, WRITE_MULTIPLE_REGISTERS);
            //return response != null;

            SendData_FeedbackLoad(WriteBuffer, out RevMsgData, false);  //写入的先不做解析
            return RevMsgData != null;
        }

        #endregion

        #region 外部调用方法
        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void FeedbackLoad_BMSON()
        {
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out int chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            ValidateChannelAddress((byte)chanel);
            ushort value = 1;
            WriteSingleRegister(chanel, 0x07, value);
        }

        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public override void FeedbackLoad_BMSOFF()
        {
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out int chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            ValidateChannelAddress((byte)chanel);
            ushort value = 0;
            WriteSingleRegister((byte)chanel, 0x07, value);

        }

        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void FeedbackLoad_ON()
        {
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out int chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            ValidateChannelAddress((byte)chanel);
            ushort value = 1;
            WriteSingleRegister(chanel, 0x06, value);
        }

        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public override void FeedbackLoad_OFF()
        {
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out int chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            ValidateChannelAddress((byte)chanel);
            ushort value = 0;
            WriteSingleRegister((byte)chanel, 0x06, value);

        }

        /// <summary>
        /// 设置工作参数
        /// </summary>
        /// <param name="voltage">需求电压（mV）</param>
        /// <param name="current">需求电流（mA）</param>
        public override void SetFeedbackLoadParams(double voltage, double current)
        {
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out int chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }

            ValidateChannelAddress((byte)chanel);
            ushort startAddress = 0x01;

            int IntVoltage = Convert.ToInt32(voltage) * 1000;//单位mV
            byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };
            int IntCurrent = Convert.ToInt32(current) * 1000;//单位mA
            byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };

            // 总共5个寄存器：工作模式(1) + 电压(2) + 电流(2)
            List<byte> values = new List<byte>();
            values.Add(0x00);
            values.Add(0x01);                           // 工作模式  默认先恒压吧 01 恒压 02恒流
            values.AddRange(voltBytes);
            values.AddRange(currBytes);

            WriteMultipleRegisters((byte)chanel, startAddress, values.ToArray());
        }

        public override void ReadFeedbackLoad_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicFeedbackLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicFeedbackLoad_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                try
                {
                    Thread.Sleep(300);
                    if (AutoReadData)
                    {
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                bool ret = ReadMultipleRegisters(ChargerID, 0xA1, 5, out byte[] RevMsgData);
                                if (RevMsgData != null)
                                {
                                    if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        ReadMultipleRegisters(ChargerID, 0xA1, 5, out RevMsgData);
                                    }
                                    StateData = GetStateData(RevMsgData);
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(true, this);
                                }
                                else if(!ret && !isWriting)
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new FeedbackLoad_StateData();
                                    StateData.ChargerID = this.ChargerID;
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                            StateData = new FeedbackLoad_StateData();
                            StateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    //StateData = new FeedbackLoad_StateData();
                    //StateData.ChargerID = this.ChargerID;
                    //SystemEvent.SendMonitorMessage(StateData);
                    //SystemEvent.SendConnectState(false, this);
                }
            }
        }


        private FeedbackLoad_StateData GetStateData(byte[] buff)
        {
            FeedbackLoad_StateData state = new FeedbackLoad_StateData();
            state.ChargerID = ChargerID;

            if (buff != null && buff.Length >= 15)
            {
                state.Chanel = this.ChargerID;
                int temp = Convert.ToInt32(buff[5].ToString("x2") + buff[6].ToString("x2") + buff[7].ToString("x2") + buff[8].ToString("x2"), 16);
                state.Voltage = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[9].ToString("x2") + buff[10].ToString("x2") + buff[11].ToString("x2") + buff[12].ToString("x2"), 16);
                state.Current = Convert.ToSingle(temp) / 1000;
            }

            return state;
        }


        #region 没有并机和取消并机
        public override void FeedbackLoad_NoParallel()
        {

        }

        public override void FeedbackLoad_Parallel()
        {

        }
        #endregion


        #endregion

        #region 调试

        /// <summary>
        /// 设置工作模式
        /// </summary>
        public bool SetWorkMode(byte channelAddress, WorkMode mode)
        {
            ValidateChannelAddress(channelAddress);
            return WriteSingleRegister(channelAddress, 0x01, (ushort)mode);
        }
        /// <summary>
        /// 设置需求电压 (mV)
        /// </summary>
        public bool SetVoltage(byte channelAddress, double voltage)
        {
            ValidateChannelAddress(channelAddress);

            // 电压需要2个寄存器 (4字节)
            int IntVoltage = Convert.ToInt32(voltage) * 1000;//单位mV
            byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };

            return WriteMultipleRegisters(channelAddress, 0x02, voltBytes);
        }

        /// <summary>
        /// 设置需求电流 (mA)
        /// </summary>
        public bool SetCurrent(byte channelAddress, double current)
        {
            ValidateChannelAddress(channelAddress);

            // 电流需要2个寄存器 (4字节)
            ushort[] values = new ushort[2];
            int IntCurrent = Convert.ToInt32(current) * 1000;//单位mA
            byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };

            return WriteMultipleRegisters(channelAddress, 0x04, currBytes);
        }

        /// <summary>
        /// 设置通道启停
        /// </summary>
        public bool SetChannelStartStop(byte channelAddress, bool start)
        {
            ValidateChannelAddress(channelAddress);
            ushort value = (ushort)(start ? 1 : 0);
            return WriteSingleRegister(channelAddress, 0x06, value);
        }

        /// <summary>
        /// 批量设置通道参数
        /// </summary>
        public bool SetChannelParameters(byte channelAddress, WorkMode mode, double voltage, double current, bool start)
        {
            ValidateChannelAddress(channelAddress);
            ushort startAddress = (ushort)((channelAddress << 8) | 0x01);

            // 总共5个寄存器：工作模式(1) + 电压(2) + 电流(2)
            int IntVoltage = Convert.ToInt32(voltage) * 1000;//单位mV
            byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };
            int IntCurrent = Convert.ToInt32(current) * 1000;//单位mA
            byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };

            // 总共5个寄存器：工作模式(1) + 电压(2) + 电流(2)
            List<byte> values = new List<byte>(1);
            values[0] = 0x01;                           // 工作模式  默认先恒压吧 01 恒压 02恒流
            values.AddRange(voltBytes);
            values.AddRange(currBytes);

            return WriteMultipleRegisters((byte)channelAddress, startAddress, values.ToArray());
        }

        #endregion

        #region 私有方法

        private byte[] CreateReadRequest(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
        {
            byte[] request = new byte[6];
            request[0] = slaveAddress;
            request[1] = functionCode;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)(startAddress & 0xFF);
            request[4] = (byte)(numberOfPoints >> 8);
            request[5] = (byte)(numberOfPoints & 0xFF);

            //byte[] crc = CalculateCRC(request, 6);
            byte[] crc = CheckOut.GetModbusCrc16_High_Right(request);//CRC校验函数。
            byte[] fullRequest = new byte[8];
            Array.Copy(request, fullRequest, 6);
            fullRequest[6] = crc[0];
            fullRequest[7] = crc[1];

            return fullRequest;
        }

        private bool SendData_FeedbackLoad(byte[] WriteBuffer, out byte[] RevMsgData, bool isWrite = true)
        {
            RevMsgData = null;
            lock (SynLockLoad)
            {
                try
                {
                    //优先写入
                    if (isWriting && !isWrite)
                        return true;
                    if (isWrite)
                        AutoReadData = false;
                    Thread.Sleep(300);
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();
                            if (RevMsgData == null)
                            {
                                SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                continue;
                            }
                        }
                        if (isWrite)
                            AutoReadData = true;
                        return true;
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
                    Log.Log.LogException(ex);
                    return false;
                }
            }
        }

        private void ValidateChannelAddress(byte channelAddress)
        {
            if (channelAddress < MIN_CHANNEL_ADDRESS || channelAddress > MAX_CHANNEL_ADDRESS)
            {
                //throw new ArgumentException($"通道地址必须在 {MIN_CHANNEL_ADDRESS} 到 {MAX_CHANNEL_ADDRESS} 之间");
                channelAddress = MIN_CHANNEL_ADDRESS;
            }
        }

        #endregion

        #region CRC计算

        public static byte[] CalculateCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF;

            for (int pos = 0; pos < length; pos++)
            {
                crc ^= data[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        private bool ValidateCRC(byte[] data)
        {
            if (data.Length < 2)
                return false;

            int dataLength = data.Length - 2;
            byte[] crcCalculated = CalculateCRC(data, dataLength);
            byte crcLow = data[dataLength];
            byte crcHigh = data[dataLength + 1];

            return crcCalculated[0] == crcLow && crcCalculated[1] == crcHigh;
        }

        #endregion

    }

    /// <summary>
    /// 通道数据类
    /// </summary>
    public class ChannelData
    {
        public emtFeedbackLoad_YKR.WorkMode WorkMode { get; set; }
        public ushort OutputVoltage { get; set; }  // mV
        public ushort OutputCurrent { get; set; }  // mA
        public ushort ChannelStatus { get; set; }  // 0:停止, 1:启动

        public override string ToString()
        {
            return $"模式: {WorkMode}, 电压: {OutputVoltage}mV, 电流: {OutputCurrent}mA, 状态: {(ChannelStatus == 1 ? "运行" : "停止")}";
        }
    }



    #region  先注释掉的方法

    /// <summary>
    /// 先注释
    /// </summary>
    /// <param name="channelAddress"></param>
    /// <exception cref="ArgumentException"></exception>
    //private byte[] SendRequest(byte expectedAddress, byte[] request, byte expectedFunctionCode)
    //{
    //    lock (_lockObject)
    //    {
    //        try
    //        {
    //            _serialPort.DiscardInBuffer();
    //            _serialPort.Write(request, 0, request.Length);

    //            // 等待响应
    //            Thread.Sleep(50);

    //            // 读取响应头
    //            byte[] header = new byte[3];
    //            int bytesRead = _serialPort.Read(header, 0, 3);

    //            if (bytesRead != 3)
    //                return null;

    //            // 检查地址和功能码
    //            if (header[0] != expectedAddress || header[1] != expectedFunctionCode)
    //                return null;

    //            // 根据功能码处理不同长度的响应
    //            byte[] response;
    //            if (expectedFunctionCode == READ_HOLDING_REGISTERS || expectedFunctionCode == READ_INPUT_REGISTERS)
    //            {
    //                int dataLength = header[2];
    //                response = new byte[3 + dataLength + 2]; // 头 + 数据 + CRC
    //                Array.Copy(header, response, 3);

    //                // 读取剩余数据
    //                _serialPort.Read(response, 3, dataLength + 2);
    //            }
    //            else if (expectedFunctionCode == WRITE_SINGLE_REGISTER)
    //            {
    //                response = new byte[8];
    //                Array.Copy(header, response, 3);
    //                _serialPort.Read(response, 3, 5);
    //            }
    //            else if (expectedFunctionCode == WRITE_MULTIPLE_REGISTERS)
    //            {
    //                response = new byte[8];
    //                Array.Copy(header, response, 3);
    //                _serialPort.Read(response, 3, 5);
    //            }
    //            else
    //            {
    //                return null;
    //            }

    //            // 验证CRC
    //            if (!ValidateCRC(response))
    //                return null;

    //            return response;
    //        }
    //        catch (TimeoutException)
    //        {
    //            return null;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"通信错误: {ex.Message}");
    //            return null;
    //        }
    //    }
    //}


    #endregion
}
