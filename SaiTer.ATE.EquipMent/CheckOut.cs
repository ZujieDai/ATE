using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 协议校验
    /// </summary>
    public class CheckOut
    {
        ////////////////////////////吉事励交流源////////////////////////////////////////////////////
        public static Byte ACSourceCrc(byte[] pDataIn)
        {
            UInt16 sum = 0;
            UInt32 i;
            for (i = 0; i < pDataIn.Length; i++)
            {
                sum = Convert.ToUInt16(sum + pDataIn[i]);
            }

            return (Byte)sum;
        }

        public static bool CheckACSourceCrc(byte[] tbytes)
        {
            bool isCheck = false;
            byte crc16 = new byte();

            try
            {
                crc16 = tbytes[tbytes.Length - 1];


                byte cRC16Buffer = ACSourceCrc(tbytes.Take(tbytes.Length - 1).ToArray());
                if (cRC16Buffer == crc16)
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }

        /////////////////////////////赛特交直流-模拟桩和BMS////////////////////////////////////////////////////
        public static byte[] GetModbusCrc16_High_Right(byte[] bytes)
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

        public static bool CheckModbusCrc16_High_Right(byte[] tbytes)
        {
            // TCP通讯可能末尾填充了很多为0的数组凑够256个字节
            if (tbytes != null && tbytes.Length == 256)
            {
                // 找到结尾的字节
                int lastNonZeroIndex = -1;
                for (int i = tbytes.Length - 1; i > 0; i--)
                {
                    if (tbytes[i] != 0x00)
                    {
                        lastNonZeroIndex = i;
                        break;
                    }
                }

                // 如果找到了非零字节，则截取有效数据
                if (lastNonZeroIndex != -1 && lastNonZeroIndex != tbytes.Length)
                {
                    byte[] trimmedBytes = new byte[lastNonZeroIndex + 1];
                    Array.Copy(tbytes, trimmedBytes, trimmedBytes.Length);
                    tbytes = trimmedBytes;
                }
            }
            bool isCheck = false;
            byte[] crc16 = new byte[2];

            try
            {
                if (tbytes.Count()>3)
                {
                    crc16[0] = tbytes[tbytes.Length - 2];
                    crc16[1] = tbytes[tbytes.Length - 1];                    
                }
                byte[] cRC16Buffer = GetModbusCrc16_High_Right(tbytes.Take(tbytes.Length - 2).ToArray());
                if ((cRC16Buffer[0] == crc16[0]) && (cRC16Buffer[1] == crc16[1]))
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }

        /////////////////////////////赛特交直流负载/////////////////////////////
        public static byte LoadCrc(byte[] SetByte)
        {
            UInt16 sum = 0;
            UInt32 i;

            for (i = 4; i < SetByte.Length; i++)//前4个字节去掉
            {
                sum = Convert.ToUInt16(sum + SetByte[i]);
            }

            return (Byte)sum;

        }
        public static bool CheckLoadCrc(byte[] SetByte)
        {
            bool isCheck = false;

            byte CheckSumByte = 0x00;

            UInt16 sum = 0;
            UInt32 i;

            if (SetByte.Length > 4)

            {
                for (i = 4; i < SetByte.Length - 2; i++)//前4个字节和后2个字节去掉
                {
                    sum = Convert.ToUInt16(sum + SetByte[i]);
                }
                CheckSumByte = (Byte)sum;
                if (CheckSumByte == SetByte[SetByte.Length - 2])//倒数第2个字节是校验和
                {
                    isCheck = true;
                }
            }
            return isCheck;

        }


        /// /////////////////////////艾德克斯电子负载/////////////////////////////////////
        public static byte ElectronicLoadCrc(byte[] SetByte)
        {
            UInt16 sum = 0;
            UInt32 i;

            for (i = 0; i < SetByte.Length; i++)//
            {
                sum = Convert.ToUInt16(sum + SetByte[i]);
            }

            return (Byte)sum;

        }
        public static bool CheckElectronicLoadCrc(byte[] SetByte)
        {
            bool isCheck = false;

            byte CheckSumByte = 0x00;

            UInt16 sum = 0;
            UInt32 i;

            if (SetByte.Length > 0)

            {
                for (i = 0; i < SetByte.Length - 1; i++)//最后1个字节去掉
                {
                    sum = Convert.ToUInt16(sum + SetByte[i]);
                }
                CheckSumByte = (Byte)sum;
                if (CheckSumByte == SetByte[SetByte.Length - 1])//倒数第1个字节是校验和
                {
                    isCheck = true;
                }
            }
            return isCheck;

        }


        ////////////////////////////博奥斯AC60-33600变频交电源,雅达DTSD3366D交流电能表//////////////////////////////////////////////
        ////////////////////////////中佳 剩余电流保护装置动作特性测试仪(漏电测试仪)ABX-R2-A/////////////////////////////////////////////
        /// /////////////////////////////博奥斯STLD-40KW-AT-R回馈负载设备///////////////////////////////////////////////////
        /////////////////////////////青岛信创PA6500功率分析仪////////////////////////////////////////////////
        public static byte[] ModBus_RTU_CRC16(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;

                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
                byte lo = (byte)(crc & 0x00FF);         //低位置

                return new byte[] { lo, hi };
            }
            return new byte[] { 0, 0 };
        }

        public static bool CheckModBus_RTU_CRC16(byte[] tbytes)
        {
            bool isCheck = false;
            byte[] crc16 = new byte[2];

            try
            {
                crc16[0] = tbytes[tbytes.Length - 2];//低位在前
                crc16[1] = tbytes[tbytes.Length - 1];//高位在后

                byte[] cRC16Buffer = ModBus_RTU_CRC16(tbytes.Take(tbytes.Length - 2).ToArray());
                if ((cRC16Buffer[0] == crc16[0]) && (cRC16Buffer[1] == crc16[1]))
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }



        /////////////////////////华源 ERL_DC60KW回馈负载设备/////////////////////////////////
        public static byte ERL_DC60KWCrc(byte[] SetByte)
        {
            UInt16 sum = 0;
            UInt32 i;

            for (i = 2; i < SetByte.Length; i++)//前2个字节去掉
            {
                sum = Convert.ToUInt16(sum + SetByte[i]);
            }

            return (Byte)sum;

        }

        public static bool CheckERL_DC60KWCrc(byte[] SetByte)
        {
            bool isCheck = false;

            byte CheckSumByte = 0x00;

            UInt16 sum = 0;
            UInt32 i;

            if (SetByte.Length > 0)

            {
                for (i = 2; i < SetByte.Length - 1; i++)//前2个字节和后1个字节去掉
                {
                    sum = Convert.ToUInt16(sum + SetByte[i]);
                }
                CheckSumByte = (Byte)sum;
                if (CheckSumByte == SetByte[SetByte.Length - 1])//倒数第1个字节是校验和
                {
                    isCheck = true;
                }
            }
            return isCheck;

        }



        /////////////////////////////其他//////////////////////////////////////////////////////////////////////
        public static UInt16 CRCCCITT(List<byte> pDataIn, UInt16 iLenIn)
        {
            UInt16 wTemp = 0;
            UInt16 wCRC = 0x0;
            UInt16 i;
            UInt16 j;

            for (i = 0; i < iLenIn; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    //  wTemp = ((pDataIn[i] << j) & 0x80 ) ^ ((wCRC & 0x8000) >> 8);      
                    wTemp = ((UInt16)((pDataIn[i] << j) & 0x80));
                    wTemp ^= (UInt16)((wCRC & 0x8000) >> 8);
                    wCRC <<= 1;
                    if (wTemp != 0)
                    {
                        wCRC ^= 0x1021;
                    }
                }
            }

            return wCRC;
        }
        public static UInt16 CRC(byte[] pDataIn, UInt32 iLenIn)
        {
            UInt16 wTemp = 0;
            UInt16 wCRC = 0x0;
            UInt32 i;
            UInt16 j;
            for (i = 0; i < iLenIn; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    //  wTemp = ((pDataIn[i] << j) & 0x80 ) ^ ((wCRC & 0x8000) >> 8);      
                    wTemp = ((UInt16)((pDataIn[i] << j) & 0x80));
                    wTemp ^= (UInt16)((wCRC & 0x8000) >> 8);
                    wCRC <<= 1;
                    if (wTemp != 0)
                    {
                        wCRC ^= 0x1021;
                    }
                }
            }

            return wCRC;
        }

        #region 欧标命令格式转换
        private static byte HexToChar(byte bHex)
        {
            byte desc = 0;

            if ((bHex >= 0) && (bHex <= 9))
                desc = (byte)(bHex + 0x30);
            else if ((bHex >= 0xA) && (bHex <= 0xF))
                desc = (byte)(bHex + 0x37);

            return desc;
        }

        private static List<byte> Array_HexToChar(List<byte> bHexs)
        {
            List<byte> bytes = new List<byte>();
            foreach (byte b in bHexs)
            {
                bytes.Add(HexToChar((byte)((b >> 4) & 0x0F)));
                bytes.Add(HexToChar((byte)((b) & 0x0F)));
            }

            return bytes;
        }

        /// <summary>
        /// 获取CRC
        /// </summary>
        /// <param name="bytes">待CRC校验数据</param>
        /// <returns>返回CRC数组</returns>
        private static ushort GetCrc_EU(byte[] bytes)
        {
            int sum = 0;
            foreach (byte b in bytes)
            {
                sum += b;
            }

            sum = 0xffff - sum + 0x0001;
            sum = (ushort)(0x00ff & sum);

            string sLow = Convert.ToString(sum % 16, 16).ToUpper();  //0x0d -> "0x0D"
            string sHigh = Convert.ToString(sum / 16, 16).ToUpper();

            byte[] ascii1 = Encoding.ASCII.GetBytes(sHigh);     //"0x0D" -> 0x44
            byte[] ascii2 = Encoding.ASCII.GetBytes(sLow);

            return (ushort)(ascii1[0] << 8 | ascii2[0]);
        }

        /// <summary>
        /// 校验CRC
        /// </summary>
        /// <param name="tbytes">待CRC校验数据</param>
        /// <returns>true/false</returns>
        public static bool CheckCrc_EU(byte[] tbytes)
        {
            try
            {
                if (tbytes == null)
                    return false;
                return ((ushort)(tbytes[tbytes.Length - 2] << 8 | tbytes[tbytes.Length - 1]) == GetCrc_EU(tbytes.Take(tbytes.Length - 2).ToArray()));
            }
            catch { }
            return false;
        }

        public static byte[] ToASCII_EU(byte[] bytes)
        {
            List<byte> list = bytes.ToList();
            list.RemoveAt(0);   //移除SOI
            list.RemoveRange(list.Count - 3, 3);    //移除CRC和EOI
            List<byte> WriteBuffer = new List<byte>();
            WriteBuffer.Add(0x7E);  //SOI
            WriteBuffer.AddRange(Array_HexToChar(list));
            WriteBuffer.Add(0x0D);  //EOI

            //转欧标发送格式
            ushort curCRC = GetCrc_EU(WriteBuffer.ToArray()); // CRC
            WriteBuffer.AddRange(new byte[2] { (byte)(curCRC >> 8), (byte)curCRC });

            return WriteBuffer.ToArray();
        }

        /// <summary>
        /// ASCII转HEX字节数组
        /// </summary>
        /// <param name="bChars"></param>
        /// <returns>返回不含0X0D和CRC的数组</returns>
        public static byte[] Array_CharToHex(byte[] bChars)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(bChars[0]);
            for (int i = 1; i < bChars.Length - 3; i += 2)
            {
                bytes.Add(Char2ToHex(bChars[i], bChars[i + 1]));
            }

            return bytes.ToArray();
        }

        private static byte Char2ToHex(byte bChar1, byte bChar2)
        {
            return (byte)(CharToHex(bChar1) << 4 | CharToHex(bChar2));
        }

        private static byte CharToHex(byte bChar)
        {
            byte desc = 0;

            if ((bChar >= '0') && (bChar <= '9'))
                desc = (byte)(bChar - 0x30);
            else if ((bChar >= 'a') && (bChar <= 'f'))
                desc = (byte)(bChar - 0x57);
            else if ((bChar >= 'A') && (bChar <= 'F'))
                desc = (byte)(bChar - 0x37);

            return desc;
        }

        public static byte[] EuropeBMS(byte[] tbytes)
        {
            try
            {
                if (tbytes.Length >= 0XFF)
                    return tbytes;


                string set = System.Text.Encoding.ASCII.GetString(tbytes);
                string temp = set.Substring(1, set.Length - 4);//去掉头帧0x7e 和0X0D  CRC16
                Byte[] rebytes = new byte[temp.Length / 2 + 4];
                rebytes[0] = 0x7e;
                int k = 1;
                for (int i = 0; i < temp.Length; i = i + 2)
                {
                    rebytes[k++] = Convert.ToByte(temp.Substring(i, 2), 16);//转换成16进制

                }
                return rebytes;
            }
            catch
            {
                return tbytes;
            }

        }
        #endregion

    }
}
