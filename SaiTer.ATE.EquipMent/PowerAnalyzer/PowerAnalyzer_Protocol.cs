using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 功率分析仪协议
    /// </summary>
    public class PowerAnalyzer_Protocol
    {
        ///青岛信创PA6500功率分析仪 /////////////////////////////////////////////
        ///
        public byte[] PA6500Bytes0 = { 0x01, 0x03, 0x17, 0x10, 0x00, 0x0A, 0xC1, 0xBC };//包括CRC16   效率值
        public byte[] PA6500Bytes1 = { 0x01, 0x03, 0x11, 0x00, 0x00, 0x0A, 0xC0, 0xF1 };//包括CRC16   1通道数据
        //public byte[] PA6500Bytes1 = { 0x01, 0x03, 0x11, 0x00, 0x00, 0x1E, 0xC0, 0xFE };//包括CRC16   1通道数据
        public byte[] PA6500Bytes2 = { 0x01, 0x03, 0x12, 0x00, 0x00, 0x0A, 0xC0, 0xB5 };//包括CRC16   2通道
        //public byte[] PA6500Bytes2 = { 0x01, 0x03, 0x12, 0x00, 0x00, 0x1E, 0xC0, 0xBA };//包括CRC16   2通道
        public byte[] PA6500Bytes3 = { 0x01, 0x03, 0x13, 0x00, 0x00, 0x0A, 0xC1, 0x49 };//包括CRC16
        //public byte[] PA6500Bytes3 = { 0x01, 0x03, 0x13, 0x00, 0x00, 0x1E, 0xC1, 0x46 };//包括CRC16
        public byte[] PA6500Bytes4 = { 0x01, 0x03, 0x14, 0x00, 0x00, 0x0A, 0xC0, 0x3D };//包括CRC16
        public byte[] PA6500Bytes5 = { 0x01, 0x03, 0x15, 0x00, 0x00, 0x0A, 0xC1, 0xC1 };//包括CRC16
        public byte[] PA6500Bytes6 = { 0x01, 0x03, 0x16, 0x00, 0x00, 0x0A, 0xC1, 0x85 };//包括CRC16    6通道
        public byte[] PA6500Bytes7 = { 0x01, 0x03, 0x17, 0x00, 0x00, 0x0A, 0xC0, 0x79 };//包括CRC16    总
        public byte[] PA6500Bytes8 = { 0x01, 0x03, 0x11, 0x19, 0x00, 0x0A, 0x11, 0x36 };//包括CRC16   1通道电能数据
        public byte[] PA6500Bytes9 = { 0x01, 0x03, 0x12, 0x19, 0x00, 0x0A, 0x11, 0x72 };//包括CRC16   2通道电能数据
        public byte[] PA6500Bytes10 = { 0x01, 0x03, 0x13, 0x19, 0x00, 0x0A, 0x10, 0x8E };//包括CRC16   3通道电能数据
        public byte[] PA6500Bytes11 = { 0x01, 0x03, 0x11, 0x15, 0x00, 0x02, 0xD0, 0xF3 };//包括CRC16   1通道数据
        public byte[] PA6500Bytes12 = { 0x01, 0x03, 0x12, 0x15, 0x00, 0x02, 0xD0, 0xB7 };//包括CRC16   2通道数据
        public byte[] PA6500Bytes13 = { 0x01, 0x03, 0x13, 0x15, 0x00, 0x02, 0xD1, 0x4B };//包括CRC16   3通道数据

        //public byte[] PA6500BytesSetIntegral1 = { 0x01, 0x06, 0x21, 0x00, 0x00, 0x01, 0x42, 0x36 };
        //public byte[] PA6500BytesSetIntegral2 = { 0x01, 0x06, 0x22, 0x00, 0x00, 0x01, 0x42, 0x72 };
        //public byte[] PA6500BytesSetIntegral3 = { 0x01, 0x06, 0x23, 0x00, 0x00, 0x01, 0x43, 0x8E };
        public byte[] PA6500BytesSetIntegral1 = { 0x01, 0x06, 0x21, 0x00, 0x00, 0x00, 0x83, 0xF6 };
        public byte[] PA6500BytesSetIntegral2 = { 0x01, 0x06, 0x22, 0x00, 0x00, 0x00, 0x83, 0xB2 };
        public byte[] PA6500BytesSetIntegral3 = { 0x01, 0x06, 0x23, 0x00, 0x00, 0x00, 0x82, 0x4E };
        public byte[] PA6500BytesSetIntegral123 = { 0x01, 0x06, 0x20, 0x14, 0x00, 0x15, 0x03, 0xC1 };


        /// <summary>
        /// 设置123通道积分状态
        /// </summary>
        /// <param name="iState">0：清零   1：开始    2：停止</param>
        /// <returns></returns>
        public byte[] PA6500SetIntegral123(int iState)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x06);
            bytestmp.Add(0x20);
            bytestmp.Add(0x14);
            bytestmp.Add(0x00);
            StringBuilder sbtmp = new StringBuilder();
            sbtmp.Append("00");
            if (iState > 2) iState = 2;
            for(int i = 0; i < 3; i++)
            {
                sbtmp.Append(Convert.ToString(iState, 2).PadLeft(2, '0'));
            }
            bytestmp.Add((byte)Convert.ToInt16(sbtmp.ToString(),2));
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        /// <summary>
        /// 设置通道量程
        /// </summary>
        /// <param name="iCH"></param>
        /// <param name="iRange">以下挡位对应状态（从0开始）：5mA 10mA 20mA 50mA 100mA 200mA 500mA 1A Auto</param>
        /// <returns></returns>
        public byte[] PA6500SetChannelRange(int iCH,int iRange)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x06);
            bytestmp.Add((byte)(0x20 + iCH));
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(iRange >> 8));
            bytestmp.Add((byte)iRange);
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        /// <summary>
        /// 设置通道变比
        /// </summary>
        /// <param name="iCH"></param>
        /// <param name="isU"></param>
        /// <param name="iRatio"></param>
        /// <returns></returns>
        public byte[] PA6500SetChannelRatio(int iCH,bool isU,int iRatio)
        {
            iRatio = iRatio * 10;//实际要把数据放大10倍
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x06);
            bytestmp.Add((byte)(0x20 + iCH));
            if(isU)
            {
                bytestmp.Add(0x0A);
            }
            else
            {
                bytestmp.Add(0x0B);
            }
            bytestmp.Add((byte)(iRatio >> 8));
            bytestmp.Add((byte)iRatio);
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        public byte[] PA6500ReadDcComponentVoltage(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x21);
            bytestmp.Add(0x00);
            bytestmp.Add(0x02);
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        public byte[] PA6500ReadDcComponentCurrent(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x23);
            bytestmp.Add(0x00);
            bytestmp.Add(0x02);
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        public byte[] PA6500ReadFreq(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x0A);
            bytestmp.Add(0x00);
            bytestmp.Add(0x02);
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        public byte[] PA6500ReadCurrentHarmonicValue_50(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x2D);
            bytestmp.Add(0x00);
            bytestmp.Add(0x32);//50个寄存器
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        public byte[] PA6500ReadVoltageHarmonicValue_50(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x5F);
            bytestmp.Add(0x00);
            bytestmp.Add(0x32);//50个寄存器
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }
        public byte[] PA6500SetHarmonicState(int iCH, bool isON)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x06);
            bytestmp.Add((byte)(0x20 + iCH));
            bytestmp.Add(0x07);
            bytestmp.Add(0x00);
            if (isON)
            {
                bytestmp.Add(0x01);
            }
            else
            {
                bytestmp.Add(0x00);
            }
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }


        public byte[] PA6500ReadCurrentHarmonic_Total(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x29);
            bytestmp.Add(0x00);
            bytestmp.Add(0x02);//50个寄存器
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }


        public byte[] PA6500ReadVoltageHarmonic_Total(int iCH)
        {
            List<byte> bytestmp = new List<byte>();
            bytestmp.Add(0x01);
            bytestmp.Add(0x03);
            bytestmp.Add((byte)(0x10 + iCH));
            bytestmp.Add(0x2B);
            bytestmp.Add(0x00);
            bytestmp.Add(0x02);//50个寄存器
            //校验码
            bytestmp.AddRange(GetModbusCrc16(bytestmp.ToArray()));

            return bytestmp.ToArray();
        }

        /*************************************************************************
        * CRC16校验方法
        * ***********************************************************************/
        public static byte[] GetModbusCrc16(byte[] bytes)
        {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF;// 预置一个值为 0xFFFF 的 16 位寄存器

            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01;// 多项式码 0xA001

            for (int i = 0; i < bytes.Length; i++)
            {
                crcRegister_L = (byte)(crcRegister_L ^ bytes[i]);

                for (int j = 0; j < 8; j++)
                {
                    byte tempCRC_H = crcRegister_H;
                    byte tempCRC_L = crcRegister_L;

                    crcRegister_H = (byte)(crcRegister_H >> 1);
                    crcRegister_L = (byte)(crcRegister_L >> 1);
                    // 高位右移前最后 1 位应该是低位右移后的第 1 位：如果高位最后一位为 1 则低位右移后前面补 1
                    if ((tempCRC_H & 0x01) == 0x01)
                    {
                        crcRegister_L = (byte)(crcRegister_L | 0x80);
                    }

                    if ((tempCRC_L & 0x01) == 0x01)
                    {
                        crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                        crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                    }
                }
            }

            return new byte[] { crcRegister_L, crcRegister_H };
        }


        public PowerAnalyzer_StateData GetStateData(byte[] buffer, int buffType, ref PowerAnalyzer_StateData StateData, int chargerID)
        {
            try
            {
                StateData.ChargerID = chargerID;
                if (buffer == null)
                {
                    return StateData;
                }
                switch (buffType)
                {
                    case 0:
                        string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Efficiency = DataParse(data);
                        break;
                    case 1:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel1RMSVolt = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.Channel1RMSCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.Channel1Power = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.Channel1PowerFactor = DataParse(data);
                        data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                        StateData.Channel1ReactivePower = DataParse(data) / 1000;
                        //data = buffer[59].ToString("x2") + buffer[60].ToString("x2") + buffer[61].ToString("x2") + buffer[62].ToString("x2");
                        //StateData.Channel1ElectricEnergy = DataParse(data);
                        break;
                    case 2:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel2RMSVolt = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.Channel2RMSCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.Channel2Power = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.Channel2PowerFactor = DataParse(data);
                        data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                        StateData.Channel2ReactivePower = DataParse(data) / 1000;
                        //data = buffer[59].ToString("x2") + buffer[60].ToString("x2") + buffer[61].ToString("x2") + buffer[62].ToString("x2");
                        //StateData.Channel2ElectricEnergy = DataParse(data);
                        break;
                    case 3:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel3RMSVolt = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.Channel3RMSCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.Channel3Power = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.Channel3PowerFactor = DataParse(data);
                        data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                        StateData.Channel3ReactivePower = DataParse(data) / 1000;
                        //data = buffer[59].ToString("x2") + buffer[60].ToString("x2") + buffer[61].ToString("x2") + buffer[62].ToString("x2");
                        //StateData.Channel3ElectricEnergy = DataParse(data);
                        break;
                    case 4:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel4RMSVolt = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.Channel4RMSCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.Channel4Power = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.Channel4PowerFactor = DataParse(data);
                        data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                        StateData.Channel4ReactivePower = DataParse(data) / 1000;
                        break;
                    case 5:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel5RMSVolt = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.Channel5RMSCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.Channel5Power = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.Channel5PowerFactor = DataParse(data);
                        data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                        StateData.Channel5ReactivePower = DataParse(data) / 1000;
                        break;
                    //case 6:
                    //    data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                    //    StateData.Channel6RMSVolt = DataParse(data);
                    //    data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                    //    StateData.Channel6RMSCurrent = DataParse(data);
                    //    data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                    //    StateData.Channel6Power = DataParse(data);
                    //    data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                    //    StateData.Channel6PowerFactor = DataParse(data);
                    //    data = buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2");
                    //    StateData.Channel6ReactivePower = DataParse(data);
                    //    break;
                    case 7:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.TotalVoltage = DataParse(data);
                        data = buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2");
                        StateData.TotalCurrent = DataParse(data) / 1000;
                        data = buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2");
                        StateData.TotalPower = DataParse(data) / 1000;
                        data = buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2");
                        StateData.TotalPowerFactor = DataParse(data);
                        break;
                    case 8:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel1ElectricEnergy = DataParse(data);
                        break;
                    case 9:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel2ElectricEnergy = DataParse(data);
                        break;
                    case 10:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel3ElectricEnergy = DataParse(data);
                        break;
                    case 11:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel1ElectricEnergy = DataParse(data);
                        //SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "通道1电能量："+ StateData.Channel1ElectricEnergy.ToString());
                        break;
                    case 12:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel2ElectricEnergy = DataParse(data);
                        //SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "通道2电能量：" + StateData.Channel2ElectricEnergy.ToString());
                        break;  
                    case 13:
                        data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                        StateData.Channel3ElectricEnergy = DataParse(data);
                        //SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "通道3电能量：" + StateData.Channel3ElectricEnergy.ToString());
                        break;
                    default:
                        break;
                }
                return StateData;
            }
            catch { return StateData; }
        }

        private double DataParse(string value)
        {
            UInt32 x = Convert.ToUInt32(value, 16);
            double fy = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
            ////浮点数转16进制
            //byte[] bytes = BitConverter.GetBytes(fy);
            fy = Math.Round(fy, 3);
            return fy;
        }
    }
}
