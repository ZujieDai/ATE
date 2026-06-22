using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 交流BMS协议
    /// </summary>
    public class BMS_Protocol
    {
        public byte[] GBExchangeReadCombine()//BMS
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x69 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return WriteBuffer;
        }
        public byte[] GBExchangeSetCombine(bool isON)//BMS
        {
            int BYTE0 = isON ? 0x01 : 0x00;

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x69 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add((byte)BYTE0);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return WriteBuffer;
        }
        public byte[] GBExchangeSetStrAllSend(Double t1, Double t2, bool[] tBitS)//模拟桩或者BMS 为桩时t1为R3  t2为辅助电源电压 或者 为BMS时 t1为R4电阻值 t2为电池电压 
        {
            UInt16 temp1 = 0, temp2 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x80 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(t1);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 

            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp2);//0000  0000  // 

            byte DCPositive = 0;
            if (tBitS[0])
                DCPositive = 0x80;
            else if (tBitS[1])
                DCPositive = 0x40;
            else if (tBitS[2])
                DCPositive = 0x20;
            else if (tBitS[3])
                DCPositive = 0x10;
            else if (tBitS[4])
                DCPositive = 0x08;
            else if (tBitS[5])
                DCPositive = 0x04;
            else if (tBitS[6])
                DCPositive = 0x02;
            else if (tBitS[7])
                DCPositive = 0x01;
            ReturnbyteSource.Add(DCPositive);

            // DC-
            byte DCNegative = 0;
            if (tBitS[8])
                DCNegative = 0x80;
            else if (tBitS[9])
                DCNegative = 0x40;
            else if (tBitS[10])
                DCNegative = 0x20;
            else if (tBitS[11])
                DCNegative = 0x10;
            else if (tBitS[12])
                DCNegative = 0x08;
            else if (tBitS[13])
                DCNegative = 0x04;
            else if (tBitS[14])
                DCNegative = 0x02;
            else if (tBitS[15])
                DCNegative = 0x01;
            ReturnbyteSource.Add(DCNegative);

            UInt32 Temp = 0;
            for (int i = 0; i < 16; i++)
            {
                // 将bool值左移相应的位数并进行或运算
                //if (tBitS[i + 16])
                //    Temp |= (UInt32)1 << i;
                if (tBitS[i + 16])
                {

                    Temp = ((0x0001 | Temp) << 1);
                }
                else
                {

                    Temp = ((0xFFFFFFFE & Temp) << 1);
                }
            }
            Temp = (Temp >> 1);//上面多左移动了1位
            ReturnbyteSource.Add((byte)(Temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)Temp);//0000  0000
            //UInt64 Temp = 0;
            //foreach (bool tBit in tBitS)
            //{
            //    if (tBit)
            //    {

            //        Temp = ((0x0001 | Temp) << 1);
            //    }
            //    else
            //    {

            //        Temp = ((0xFFFFFFFE & Temp) << 1);
            //    }
            //}
            //Temp = (Temp >> 1);//上面多左移动了1位
            //ReturnbyteSource.Add((byte)(Temp >> 24));//0000  0000
            //ReturnbyteSource.Add((byte)(Temp >> 16));//0000  0000
            //ReturnbyteSource.Add((byte)(Temp >> 8));//0000  0000
            //ReturnbyteSource.Add((byte)Temp);//0000  0000

            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public byte[] GBExchangeSetStrAllSend(bool isBIT0, bool isBIT1, bool isBIT2, bool isBIT3, bool isBIT4, bool isBIT5, bool isBIT6, bool isBIT7)//BMS
        {
            //isBIT0 A枪开关S                isBIT1 A枪风扇控制  False         isBIT2 A枪CC断开控制 true                 isBIT3 A枪CP断开控制 true
            //isBIT4 A枪CP接地控制  False    isBIT5 A枪PE断开控制  true        isBIT6 A枪CC电阻检测控制  False           isBIT7 A枪定时充电控制 False  
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x20 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            Byte BYTE0 = 0;
            if (isBIT0)
            {
                BYTE0 = Convert.ToByte(0x80 | BYTE0);
            }
            if (isBIT1)
            {
                BYTE0 = Convert.ToByte(0x40 | BYTE0);
            }
            if (isBIT2)
            {
                BYTE0 = Convert.ToByte(0x20 | BYTE0);
            }
            if (isBIT3)
            {
                BYTE0 = Convert.ToByte(0x10 | BYTE0);
            }
            if (isBIT4)
            {
                BYTE0 = Convert.ToByte(0x08 | BYTE0);
            }
            if (isBIT5)
            {
                BYTE0 = Convert.ToByte(0x04 | BYTE0);
            }
            if (isBIT6)
            {
                BYTE0 = Convert.ToByte(0x02 | BYTE0);
            }
            if (isBIT7)
            {
                BYTE0 = Convert.ToByte(0x01 | BYTE0);
            }



            ReturnbyteSource.Add(BYTE0);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return WriteBuffer;
        }
        public byte[] GBExchangeSetStrAllSend(List<bool> bs)//BMS
        {
            if (bs.Count != 16) return new byte[] { 0x00, 0x00 };
            //isBIT0 A枪开关S                isBIT1 A枪风扇控制  False         isBIT2 A枪CC断开控制 true                 isBIT3 A枪CP断开控制 true
            //isBIT4 A枪CP接地控制  False    isBIT5 A枪PE断开控制  true        isBIT6 A枪CC电阻检测控制  False           isBIT7 A枪定时充电控制 False  
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x20 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            StringBuilder sbtmp = new StringBuilder();
            Byte BYTE0 = 0;
            Byte BYTE1 = 0;


            for (int i = 7; i >= 0; i--)
            {
                byte x = Convert.ToByte(Math.Pow(2, i));
                if (bs[7 - i])
                {
                    BYTE0 = Convert.ToByte(x | BYTE0);
                }
            }
            for (int i = 15; i >= 8; i--)
            {
                byte x = Convert.ToByte(Math.Pow(2, i - 8));
                if (bs[15 - i + 8])
                {
                    BYTE1 = Convert.ToByte(x | BYTE1);
                }
            }
            ReturnbyteSource.Add(BYTE0);//0000  0000
            ReturnbyteSource.Add(BYTE1);//0000  0000

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000VoltageRegulationDCEUVoltageRegulationDCEU
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return WriteBuffer;
        }
        public byte[] GBReadLeakageResistance()
        {
            UInt32 temp1 = 0, temp2 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x68, 0x86, 0x00, 0x19, 0x01, 0xff, 0x00, 0x00, 0x00, 0x03, 0x00, 0x01, 0x80, 0x4d };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.AddRange(new byte[9]);

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public void ParseLeakageResistance(byte[] buff, out int DCUpON, out int DCDownON, out double DCUpResistance, out double DCDownResistance)
        {
            try
            {
                DCUpON = Convert.ToInt32(buff[14]) & 0x01;
                DCDownON = (Convert.ToInt32(buff[14]) >> 1) & 0x01;
                int iIndex = 15;
                DCUpResistance = buff[iIndex] << 24 | buff[iIndex + 1] << 16 | buff[iIndex + 2] << 8 | buff[iIndex + 3];
                iIndex += 4;
                DCDownResistance = buff[iIndex] << 24 | buff[iIndex + 1] << 16 | buff[iIndex + 2] << 8 | buff[iIndex + 3];
            }
            catch { DCUpON = 0; DCDownON = 0; DCUpResistance = 0; DCDownResistance = 0; }
        }
        public byte[] GBSetLeakageResistance(int DCUpON, int DCDownON, double DCUpResistance, double DCDownResistance)
        {
            UInt32 temp1 = 0, temp2 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x68, 0x86, 0x00, 0x19, 0x01, 0xff, 0x00, 0x00, 0x00, 0x03, 0x00, 0x02, 0x80, 0x4d };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            byte DCWitch = 0;
            DCWitch = (byte)(DCUpON | DCDownON << 1);
            ReturnbyteSource.Add(DCWitch);

            temp1 = Convert.ToUInt32(DCUpResistance);
            ReturnbyteSource.Add((byte)(temp1 >> 24));
            ReturnbyteSource.Add((byte)(temp1 >> 16));
            ReturnbyteSource.Add((byte)(temp1 >> 8));
            ReturnbyteSource.Add((byte)temp1);

            temp2 = Convert.ToUInt32(DCDownResistance);
            ReturnbyteSource.Add((byte)(temp2 >> 24));
            ReturnbyteSource.Add((byte)(temp2 >> 16));
            ReturnbyteSource.Add((byte)(temp2 >> 8));
            ReturnbyteSource.Add((byte)temp2);

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        /// <summary>
        /// 欧标互操性设置
        /// </summary>
        /// <param name="volt">电池电压</param>
        /// <param name="tBitS">数据内容第123字节的二进制开关（24位）</param>
        /// <param name="DCPlus">DC+绝缘阻值档位</param>
        /// <param name="DCMinus">DC-绝缘阻值档位</param>
        /// <param name="reserved">数据内容第5字节预留</param>
        /// <returns></returns>
        public byte[] EUExchangeSetStrAllSend(double volt, bool[] tBitS, int DCPlus, int DCMinus, string reserved)
        {
            UInt16 temp1 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x9a };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(volt * 10);

            //充电接口通断控制
            UInt64 Temp = 0;
            foreach (bool tBit in tBitS)
            {
                if (tBit)
                {

                    Temp = (0x0001 | Temp) << 1;
                }
                else
                {

                    Temp = (0xFFFFFFFE & Temp) << 1;
                }

            }
            Temp >>= 1;//上面多左移动了1位
            ReturnbyteSource.Add((byte)(Temp >> 16));//0000  0000
            ReturnbyteSource.Add((byte)(Temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)Temp);//0000  0000
            ReturnbyteSource.Add((byte)DCPlus);
            ReturnbyteSource.Add(0x00);//reserved预留字节
            ReturnbyteSource.Add((byte)DCMinus);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//电池电压
            ReturnbyteSource.Add((byte)temp1);


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetONOFF(bool istrue)// 模拟桩或者BMS   istrue==true启动充电  
        {
            /*
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0xd0,0x02};//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list


            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
            */




            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x30 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            if (istrue)
            {
                ReturnbyteSource.Add(0x01); //启动充电
            }
            else
            {
                ReturnbyteSource.Add(0x00);//停止充电
            }


            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        /// <summary>
        /// 控制BCS电压电流参数实时值传输
        /// </summary>
        /// <param name="istrue">0：修改的参数值 1：传输采集的实时值</param>
        /// <returns></returns>
        public byte[] BMSReadBCSUseReqOrRealValue()//  istrue==true实测值  
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x16 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x00);//0000  0000   

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        /// <summary>
        /// 控制BCS电压电流参数实时值传输
        /// </summary>
        /// <param name="istrue">0：修改的参数值 1：传输采集的实时值</param>
        /// <returns></returns>
        public byte[] BMSSetBCSUseReqOrRealValue(bool istrue)//  istrue==true实测值  
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x16 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add((byte)(istrue ? 0x01 : 0x00));//0000  0000   

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        //交流
        public byte[] GBResistanceSend(UInt16 Resistance2, UInt16 Resistance3)//BMS
        {
            //isBIT0 A枪开关S                isBIT1 A枪风扇控制  False         isBIT2 A枪CC断开控制 true                 isBIT3 A枪CP断开控制 true
            //isBIT4 A枪CP接地控制  False    isBIT5 A枪PE断开控制  true        isBIT6 A枪CC电阻检测控制  False           isBIT7 A枪定时充电控制 False  
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x21 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add((Byte)(Resistance2 >> 8));//0000  0000
            ReturnbyteSource.Add((Byte)(Resistance2));//0000  0000
            ReturnbyteSource.Add((Byte)(Resistance3 >> 8));//0000  0000
            ReturnbyteSource.Add((Byte)(Resistance3));//0000  0000

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        /// <summary>
        /// 设置电表常数和工作误差校验圈数
        /// </summary>
        /// <returns></returns>
        public byte[] BMSSetConstAndInspectionError(double ElecConstant, double InspecError)
        {
            byte[] PrefixCode_Zero = new byte[] { 0x7e, 0x00, 0xff };
            byte[] PrefixCode2;
            byte[] PrefixCode;

            List<byte> ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero);
            int ElectricConstant = Convert.ToInt32(ElecConstant);
            int InspectionError = Convert.ToInt32(InspecError);
            string ElectricConstant16 = ElectricConstant.ToString("X");
            string InspectionError16 = InspectionError.ToString("X");
            PrefixCode = new byte[] { 0x00, 0x00, 0x00 };
            PrefixCode2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            ReturnbyteSource.Add(0x03);
            ReturnbyteSource.Add(0x66);
            ReturnbyteSource.AddRange(PrefixCode);
            byte[] addbyte = ConvertHexStringToBytes(ElectricConstant16);
            ReturnbyteSource.AddRange(addbyte);//电表1常数
            ReturnbyteSource.AddRange(PrefixCode);
            ReturnbyteSource.AddRange(addbyte);//电表2常数
            //ReturnbyteSource.AddRange(PrefixCode2);
            ReturnbyteSource.AddRange(PrefixCode);
            addbyte = ConvertHexStringToBytes(InspectionError16);//电表1圈数
            ReturnbyteSource.AddRange(addbyte);

            ReturnbyteSource.AddRange(PrefixCode);
            ReturnbyteSource.AddRange(addbyte);//电表2圈数
            //ReturnbyteSource.AddRange(PrefixCode2);
            //PrefixCode = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            // ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            ReturnbyteSource.Add(0x0d);//

            byte[] writeBuffer = ExChangeListByte(ReturnbyteSource);
            return writeBuffer;
        }

        public byte[] BMSGetEnergy_AC()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero_Send = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d };
            ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero_Send);

            return ExChangeListByte(ReturnbyteSource);

        }
        public byte[] BMSClearEnergy_AC()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero_Send = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x2C, 0x00, 0x00, 0x00, 0x01, 0x0d };
            ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero_Send);

            byte[] writeBuffer = ExChangeListByte(ReturnbyteSource);
            return writeBuffer;
        }
        public byte[] BMSGetEnergy()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero_Send = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d };
            ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero_Send);

            return ExChangeListByte(ReturnbyteSource);

        }
        public byte[] BMSClearEnergy()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero_Send = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x65, 0x00, 0x00, 0x00, 0x01, 0x0d };
            ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero_Send);

            byte[] writeBuffer = ExChangeListByte(ReturnbyteSource);
            return writeBuffer;
        }

        public byte[] BMSClearError()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero_Send = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x67, 0x00, 0x00, 0x00, 0x01, 0x0d };
            ReturnbyteSource = new List<byte>();
            ReturnbyteSource.AddRange(PrefixCode_Zero_Send);

            byte[] writeBuffer = ExChangeListByte(ReturnbyteSource);
            return writeBuffer;
        }
        public byte[] BMSReadError(string ElectricConstant16, string InspectionError16)
        {


            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode_Zero = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x66 };
            ReturnbyteSource.AddRange(PrefixCode_Zero);
            ReturnbyteSource.AddRange(new byte[40]);
            ReturnbyteSource.Add(0x0d);
            return ExChangeListByte(ReturnbyteSource);

            //List<byte> ReturnbyteSource = new List<byte>();
            //byte[] PrefixCode_Zero = new byte[] { 0x7e, 0x00, 0xff };
            //ReturnbyteSource.AddRange(PrefixCode_Zero);
            //byte[] PrefixCode = new byte[] { 0x00, 0x00, 0x00 };
            //byte[] PrefixCode2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //ReturnbyteSource.Add(0x01);
            //ReturnbyteSource.Add(0x66);
            //ReturnbyteSource.AddRange(PrefixCode);
            //byte[] addbyte = ConvertHexStringToBytes(ElectricConstant16);
            //ReturnbyteSource.AddRange(addbyte);
            //ReturnbyteSource.AddRange(PrefixCode2);
            //ReturnbyteSource.AddRange(PrefixCode);
            //addbyte = ConvertHexStringToBytes(InspectionError16);
            //ReturnbyteSource.AddRange(addbyte);
            //ReturnbyteSource.AddRange(PrefixCode2);
            //PrefixCode = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.AddRange(PrefixCode);
            //ReturnbyteSource.Add(0x0d);//
            //return ExChangeListByte(ReturnbyteSource);
        }
        /// <summary>
        /// 转换List<byte>为byte[]
        /// </summary>
        /// <param name="ReturnbyteSource"></param>
        /// <returns></returns>
        public static Byte[] ExChangeListByte(List<byte> ReturnbyteSource)
        {
            try
            {
                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                Byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return writeBuffer;
            }
            catch
            {
                return null;
            }

        }


        /// <summary>
        /// 三标导引切换
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public byte[] BMSSetHCAC(EmChargerType type)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0xd0 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            byte temp = 0x00;
            /*
             *  国标交流 设置0x00
                欧标交流 设置0x01
                美标交流 设置0x02
                国标直流 设置0x03
                欧标直流 设置0x04
                美标直流 设置0x05
                日标直流 设置0x06
               */
            switch (type)
            {
                case EmChargerType.Charger_GB_AC:
                    temp = 0x00;
                    break;
                case EmChargerType.Charger_EUR_AC:
                    temp = 0x01;
                    break;
                case EmChargerType.Charger_USA_AC:
                    temp = 0x02;
                    break;
                case EmChargerType.Charger_GB_DC:
                    temp = 0x03;
                    break;
                case EmChargerType.Charger_EUR_DC:
                    temp = 0x04;
                    break;
                case EmChargerType.Charger_USA_DC:
                case EmChargerType.Charger_NACS_DC:
                    temp = 0x05;
                    break;
                case EmChargerType.Charger_JP_DC:
                    temp = 0x06;
                    break;
            }
            ReturnbyteSource.Add(temp);
            byte[] bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d };
            ReturnbyteSource.AddRange(bytes);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        /// <summary>
        /// 导引切换（欧美标直流设置类型报文不一样）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public byte[] BMSSetType_EU_USA(EmChargerType type)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0xd0 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            byte temp = 0x00;
            /*
             *  国标交流 设置0x00
                欧标交流 设置0x01
                美标交流 设置0x02
                国标直流 设置0x03
                欧标直流 设置0x04
                美标直流 设置0x05
                日标直流 设置0x06
               */
            switch (type)
            {
                case EmChargerType.Charger_GB_AC:
                    temp = 0x00;
                    break;
                case EmChargerType.Charger_EUR_AC:
                    temp = 0x01;
                    break;
                case EmChargerType.Charger_USA_AC:
                    temp = 0x02;
                    break;
                case EmChargerType.Charger_GB_DC:
                    temp = 0x03;
                    break;
                case EmChargerType.Charger_EUR_DC:
                    temp = 0x04;
                    break;
                case EmChargerType.Charger_USA_DC:
                case EmChargerType.Charger_NACS_DC:
                    temp = 0x05;
                    break;
                case EmChargerType.Charger_JP_DC:
                    temp = 0x06;
                    break;
            }
            ReturnbyteSource.Add(temp);
            byte[] bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            ReturnbyteSource.AddRange(bytes);
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);

            //byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。
            //ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1, double BatteryTotalVolt)///BMS   t1 最高允许总电压   
        {       //充电及充电握手阶段报文设置
            UInt16 temp1 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x31 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //    //1)最高允许电压：精度0.1V，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000  //250ms   2）BHM报文周期：精度1ms，就是4000ms，即0x0fa0;

            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0x01);//0000  0000  
            ReturnbyteSource.Add(0x00);// 3）协议版本号：版本V1.1，即0x010100;


            ReturnbyteSource.Add(0x03);//0000  0000  4）电池类型：	01：铅酸电池
                                       // 02：镍氢电池
                                       //03：磷酸铁锂电池
                                       //04：锰酸锂电池
                                       //05：钴酸锂电池
                                       //06：三元材料电池
                                       //07：聚合物锂离子电池
                                       //08：钛酸锂电池
                                       //FF：其他电池


            ReturnbyteSource.Add(0x03);//0000  0000
            ReturnbyteSource.Add(0xE8);//0000  0000  100AH// 5）整车电池额定容量：（车体BMS上传值 1byte定点数），精度0.1，放大10倍取整数，如400AH电池就是4000，即0x0fa0； 


            //ReturnbyteSource.Add(0x10);//0000  0000
            //ReturnbyteSource.Add(0x04);//0000  0000  //410V    6）额定电池总电压：（车体BMS上传值 1byte定点数），精度0.1，放大10倍取整数，如500V，就是5000，即0x1388；
            temp1 = Convert.ToUInt16(BatteryTotalVolt * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));
            ReturnbyteSource.Add((byte)temp1);


            ReturnbyteSource.Add(0x42);//0000  0000
            ReturnbyteSource.Add(0x59);//0000  0000  // 
            ReturnbyteSource.Add(0x44);//0000  0000
            ReturnbyteSource.Add(0x20);//0000  0000  //BYD   7）电池生产厂商：4字节，标准ASCII码；


            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x01);//0000  0000  // 8）电池组序号：4字节；

            ReturnbyteSource.Add(0x21);//0000  0000
            ReturnbyteSource.Add(0x05);//0000  0000
            ReturnbyteSource.Add(0x1F);//0000  0000 2018.5.31    9）电池生产日期：年：1年/位，1985年偏移量，就是1990年即X21=0x05；
                                       // 月：1月 / 位，0月偏移量，就是5月即X22 = 0x05；
                                       //日：1日 / 位，0日偏移量，就是5日即X23 = 0x05；
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x02);//0000  0000 //  10）电池充电次数：1次 / 位；

            ReturnbyteSource.Add(0x01);//0000  0000         //11）产权标识：0x01代表车自有，0x00代表租赁；


            ReturnbyteSource.Add(0x00);//0000  0000  // 预留

            ReturnbyteSource.Add(0x53);
            ReturnbyteSource.Add(0x41);
            ReturnbyteSource.Add(0x49);
            ReturnbyteSource.Add(0x54);
            ReturnbyteSource.Add(0x45);
            ReturnbyteSource.Add(0x52);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x31);//SAITER00000000001   车辆识别码（VIN）

            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留


            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0xFA); //250ms BRM报文周期

            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1, Double t2, double t3)///BMS   t1 动力蓄电池当前电池电压   t2最高允许充电电压
        {       //参数配置阶段报文设置
            UInt16 temp1 = 0, temp2 = 0, temp3 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x32 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0xf4);//0000  0000  //5V    //1) 单体蓄电池最高允许充电电压：精度0.01V，0V偏移量，就是3.14V，即0x013a；



            temp3 = Convert.ToUInt16(t3 * 10);
            ReturnbyteSource.Add((byte)(temp3 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp3);//0000  0000  //    //4)最高允许充电电流：精度0.1A，就是400.0A，即0x0fa0;
            //ReturnbyteSource.Add(0x09);//0000  0000
            //ReturnbyteSource.Add(0xC4);//0000  0000  //200A   //2)最高允许充电电流：精度0.1A，就是400.0A，即0x0fa0; 07D0->09C4 2000修改成2500

            ReturnbyteSource.Add(0x07);//0000  0000
            ReturnbyteSource.Add(0xD0);//0000  0000  // 200kWh    3)蓄电池标称总能量：精度0.1kWh，就是400.kWh，即即0x0fa0；

            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp2);//0000  0000  //    //4)最高允许充电电压：精度0.1V，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(0x94);//0000  0000  //98℃  //5)最高允许温度：1℃/位，-50℃偏移，就是10℃，即0x3C；



            ReturnbyteSource.Add(0x03);//0000  0000
            ReturnbyteSource.Add(0x20);//0000  0000  //80%    //6)动力蓄电池荷电状态：0.1％/位，就是50%，即0x01f4； 

            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //    //7)	动力蓄电池当前电池电压：精度0.1V，就是400.0V，即0x0fa0;


            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0xF4);//0000  0000  //500ms    //  //8)BCP报文周期：精度1ms，就是60ms，即0x003c;


            ReturnbyteSource.Add(0xAA);//0000  0000  //  ////9)	车辆准备就绪状态：准备就绪0xAA，未准备就绪0x00，其他值无效;


            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000  //250ms  //10)BRO报文周期：精度1ms，就是60ms，即0x003c;


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        // 新增insulationState参数，默认正常
        public byte[] BMSSetPara(Double t1, Double t2, byte t3, Double t4, bool canCharger = true, InsulationState insulationState = InsulationState.正常)
        {
            t2 = System.Math.Abs(t2);
            //充电阶段报文设置
            UInt16 temp1 = 0, temp2 = 0, temp4 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x33 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);

            // 原有逻辑：电压需求、电流需求、恒压/恒流等（保留不变）
            //充电需求电压
            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));
            ReturnbyteSource.Add((byte)temp1);

            //充电需求电压
            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));
            ReturnbyteSource.Add((byte)temp2);

          
            ReturnbyteSource.Add(t3);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x32);

            temp4 = Convert.ToUInt16(t4 * 10);
            ReturnbyteSource.Add((byte)(temp4 >> 8));
            ReturnbyteSource.Add((byte)temp4);

            ReturnbyteSource.Add(0x01);
            ReturnbyteSource.Add(0x2C);
            ReturnbyteSource.Add(0xA1);
            ReturnbyteSource.Add(0x90);
            ReturnbyteSource.Add(0x1E);
            ReturnbyteSource.Add(0x07);
            ReturnbyteSource.Add(0xD0);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0xFA);
            ReturnbyteSource.Add(0x02);
            ReturnbyteSource.Add(0x5F);
            ReturnbyteSource.Add(0x03);
            ReturnbyteSource.Add(0x5A);

            // ===================== 核心修改：绝缘状态 + 充电允许 =====================
            ReturnbyteSource.Add(0x00); // 状态高位（固定00）
           

            //  00 FA 
            ReturnbyteSource.Add(0x00);//0000  0000

            byte statusByte = 0x00;
            // 1. 设置绝缘状态（bit0~bit1）
            statusByte |= (byte)insulationState;
            // 2. 设置充电允许（bit4~bit5）
            if (canCharger)
            {
                statusByte |= 0x10; // 0001 0000 → 充电允许位设为1
            }
            ReturnbyteSource.Add(statusByte); // 替换原有的0x10/0x00
           
            //00
            ReturnbyteSource.Add(0x00);//0000  0000    17）	BMS终止充电原因
            ReturnbyteSource.Add(0xFA);//0000  0000  250ms  	16）	BSM报文周期：精度1ms，就是60ms，即0x003c;
            //00 00
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  	18）	BMS终止充电故障原因：
            // 00
            ReturnbyteSource.Add(0x00);//0000  0000        19）	BMS终止充电错误原因：
            // 00 0A
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x0A);//0000  0000  	20）BST报文周期：精度1ms，就是60ms，即0x003c;

            ReturnbyteSource.Add(0x0D);

            // CRC校验（保留原有逻辑）
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public byte[] BMSSetPara(Double t1, Double t2, bool isTrue, Double t4, bool canCharger, InsulationState insulationState = InsulationState.正常)//BMS  t1 BMS需求电压设置(V)     t2 BMS需求电流设置(A)  isTrue=true恒压  t4充电电压测量值
        {
            if (isTrue)// 3）充电模式：0x01：恒压充电；0x02：恒流充电；
            { return BMSSetPara(t1, t2, 0x01, t4, canCharger, insulationState); }//恒压充电
            else
            { return BMSSetPara(t1, t2, 0x02, t4, canCharger, insulationState); }//恒流充电

        }

        public byte[] BMSSetPara1_EU_DC(List<int> para1, double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x97 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            //1   电动汽车已经准备好进行能量转移
            //4   电动汽车开始
            //1   接触器状态
            //1   接触器接通后充电启动和停止
            //1   充电完成标志
            int temp = para1[0] | para1[1] << 1 | para1[2] << 5 | para1[3] << 6 | para1[4] << 7;
            ReturnbyteSource.Add((byte)temp);
            //1   充满完成标志
            //1   充电完成标志
            //1   连接检测请求
            //4   EV支持的充电口类型
            //1   S2打开/关闭请求 0:打开，1:关闭
            temp = para1[5] | para1[6] << 1 | para1[7] << 2 | para1[8] << 3 | para1[9] << 7;
            ReturnbyteSource.Add((byte)temp);
            //1   入口锁请求
            //2   AAG 值匹配状态
            temp = para1[10] | para1[11] << 1;
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(RESS_SoC / 0.5);
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(MaxCurrent * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(MaxVoltage * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。

            ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara1_EU_DC(double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x97 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            //赋默认值
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            int temp = Convert.ToInt32(RESS_SoC / 0.5);
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(MaxCurrent * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(MaxVoltage * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。

            ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        /// <summary>
        /// 欧标充电数据设置2
        /// </summary>
        /// <param name="FullSOC"></param>
        /// <param name="BulkSOC"></param>
        /// <param name="TargetVolt"></param>
        /// <param name="TargetCurrent"></param>
        /// <param name="ReadyVolt"></param>
        /// <returns></returns>
        public byte[] BMSSetPara2_EU_DC(double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x98 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            //赋默认值
            ReturnbyteSource.Add((byte)Convert.ToInt32(FullSOC / 0.5));
            ReturnbyteSource.Add((byte)Convert.ToInt32(BulkSOC / 0.5));
            int temp = Convert.ToInt32(TargetVolt * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(TargetCurrent * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(ReadyVolt * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。

            ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara3_EU_DC(double FullSOCRemainTime, double BulkSOCRemainTime)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x99 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            //赋默认值
            int temp = Convert.ToInt32(FullSOCRemainTime);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            temp = Convert.ToInt32(BulkSOCRemainTime);
            ReturnbyteSource.Add((byte)(temp >> 8));
            ReturnbyteSource.Add((byte)temp);
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。

            ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetResistance(Double t1)//模拟桩或者BMS
        {
            //设置互操电阻阻值（桩模拟器R3/车模拟器R4）
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x81 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            temp = Convert.ToUInt16(t1);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetKState_DC(byte tComm, byte[] bs)//模拟桩或者BMS
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, tComm };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(bs[0]);//0000  0000   
            ReturnbyteSource.Add(bs[1]);//0000  0000
            ReturnbyteSource.Add(bs[2]);//0000  0000  
            ReturnbyteSource.Add(bs[3]);//0000  0000 

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMS_GetResistance()//模拟桩或者BMS
        {
            //设置互操电阻阻值
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x21 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public void BMS_GetResistance(byte[] buffer, ref ushort R2, ref ushort R3)
        {
            R2 = Convert.ToUInt16(buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
            R3 = Convert.ToUInt16(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16);
        }

        public byte[] BMS_SetResistance(Double t1, Double t2)//模拟桩或者BMS
        {
            //设置互操电阻阻值
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x21 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            temp = Convert.ToUInt16(t1);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            temp = Convert.ToUInt16(t2);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetBatteryVoltage(Double t1)//BMS 
        {
            //设置互操电池电压
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x82 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            temp = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        public byte[] BMSSetControl(byte tComm, bool isTrue)//模拟桩或者BMS  通断控制 true 1代表闭合， false 0代表断开
        {
            //直流CC1通断控制 下发0x88设置（桩模拟器R3/车模拟器R4）
            //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
            //直流开关S通断控制 下发0x8A设置（桩模拟器R3/车模拟器R4）

            //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
            //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
            //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
            //充电桩报文设置读取 下发0x50设置 ）	X4 = 01，代表设置启动BMS报文数据读取，X4 = 00，代表关闭BMS报文数据读取；
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(tComm);//0000  0000  

            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            if (tComm == 0x85 || tComm == 0x86)//DC=-和A+-是一起控制的
            {
                ReturnbyteSource.Add((byte)(isTrue ? 0x01 : 0x00));
            }
            else
            {
                ReturnbyteSource.Add(0x00);//0000  0000
            }
            if (isTrue)
            { ReturnbyteSource.Add(0x01); }// true 0x01代表闭合//X4

            else
            { ReturnbyteSource.Add(0x00); }// false 0x00代表断开//X4

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSProtocolConsistency(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)// BMS 协议一致性测试设置
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x70 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(byte0);//0000  0000   
            ReturnbyteSource.Add(byte1);//0000  0000
            ReturnbyteSource.Add(byte2);//0000  0000  
            ReturnbyteSource.Add(byte3);//
            ReturnbyteSource.Add(byte4);//0000  0000   
            ReturnbyteSource.Add(byte5);//0000  0000
            ReturnbyteSource.Add(byte6);//0000  0000  
            ReturnbyteSource.Add(byte7);//

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        public UInt16 HexStringToUInt16(string hex)
        {
            // 确保移除前缀"0x"（如果存在），并全部转为大写
            hex = hex.Replace("0X", "").Replace("0x", "").ToUpperInvariant();

            // 直接解析为16进制数
            return Convert.ToUInt16(hex, 16); // 或者 uint.Parse(hex, NumberStyles.HexNumber);
        }
        public byte[] HexStringToTwoBytes(string hex)
        {
            hex = hex.PadLeft(4, '0');

            byte[] bytes = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public byte[] CommonStrMultiply10(string text)
        {
            try
            {
                int num = (int)(Convert.ToDouble(text) * 10);
                string str = BaseConvert.Int32ToHexStr(num);
                string result = str.PadLeft(4, '0');

                byte[] bytes = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    bytes[i] = Convert.ToByte(result.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                return new byte[2] { 0x00, 0x00 };
            }
        }
        private byte[] EncodeMaxSingleBatV_Num(string v, string n)
        {
            try
            {
                int volt = (int)(Convert.ToDouble(v) * 100);
                int num = Convert.ToInt32(n);

                num = num << 12;
                int total = volt + num;
                string str = BaseConvert.Int32ToHexStr(total);
                string result = str.PadLeft(4, '0');

                byte[] bytes = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    bytes[i] = Convert.ToByte(result.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                return new byte[2] { 0x00, 0x00 };
            }
        }
        public byte[] EncodeCommon2HexStr(string text)
        {
            try
            {
                int num = Convert.ToInt32(text);
                string str = BaseConvert.Int32ToHexStr(num);
                string result = str.PadLeft(4, '0');
                byte[] bytes = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    bytes[i] = Convert.ToByte(result.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                return new byte[2] { 0x00, 0x00 };
            }
        }

        private byte EncodeCommonMinus(string str)
        {
            try
            {
                int volt = Convert.ToInt32(str) - 1;
                string result = BaseConvert.Int32ToHexStr(volt);
                result = result.PadLeft(2, '0');
                UInt16 temp1 = Convert.ToUInt16(result);
                byte value = (byte)temp1;
                return value;

            }
            catch (Exception ex)
            {
                return 0x00;
            }
        }
        private byte EncodeCommonPlus50(string str)
        {
            try
            {
                int data = Convert.ToInt32(str) + 50;
                string result = BaseConvert.Int32ToHexStr(data);
                result = result.PadLeft(2, '0');
                UInt16 temp1 = Convert.ToUInt16(result);
                byte value = (byte)temp1;
                return value;

            }
            catch (Exception ex)
            {
                return 0x00;
            }
        }
        private byte EncodeState(string bits1, string bits2, string bits3, string bits4)
        {
            try
            {
                string bits = bits4 + bits3 + bits2 + bits1;
                int num = Convert.ToInt32(bits, 2);
                string result = BaseConvert.Int32ToHexStr(num);
                result = result.PadLeft(2, '0');
                UInt16 temp1 = Convert.ToUInt16(result);
                byte value = (byte)temp1;
                return value;
            }
            catch (Exception ex)
            {
                return 0x00;
            }
        }

        public byte[] BMSProtocolChargeData(SettingCharging data)// BMS 设置充电完整参数
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x33 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            //UInt16 temp1 = Convert.ToUInt16(Convert.ToDouble(data.ReqV) * 10);
            //ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            //ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 1）电压需求：精度0.1V，就是400.0V，即0x0fa0;
            ReturnbyteSource.AddRange(CommonStrMultiply10(data.ReqV));


            //UInt16 temp2 = Convert.ToUInt16(Convert.ToDouble(data.ReqI) * 10);
            //ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            //ReturnbyteSource.Add((byte)temp2);//0000  0000  //  2）电流需求：精度0.1A，就是400.0V，即0x0fa0;
            ReturnbyteSource.AddRange(CommonStrMultiply10(data.ReqI));

            ReturnbyteSource.Add((byte)HexStringToUInt16(data.ChargeMode));//充电模式:0x01:恒压充电、恒流充电

            ReturnbyteSource.AddRange(HexStringToTwoBytes(data.BCLPeriod));//BCL报文周期，2位



            ReturnbyteSource.AddRange(CommonStrMultiply10(data.MeasureV));//充电电流测量值，2位


            ReturnbyteSource.AddRange(CommonStrMultiply10(data.MeasureI));//充电电压测量值，2位


            ReturnbyteSource.AddRange(EncodeMaxSingleBatV_Num(data.MaxSingleBatV, data.MaxSingleBatGrpNum));//最高单体电池电压及组号


            UInt16 temp3 = Convert.ToUInt16(data.CurSOC);

            ReturnbyteSource.Add((byte)temp3);//X14 当前荷电状态


            ReturnbyteSource.AddRange(CommonStrMultiply10(data.RemainTime));//估算剩余充电时间


            ReturnbyteSource.AddRange(HexStringToTwoBytes(data.BCSPeriod));            //X17-X18 BCS报文周期




            ReturnbyteSource.Add(EncodeCommonMinus(data.MaxSingleBatVNum));            //X19 单体最高电压编号




            ReturnbyteSource.Add(EncodeCommonPlus50(data.MaxBatTemp));            //X20 最高电池温度




            ReturnbyteSource.Add(EncodeCommonMinus(data.MaxTempDetectionNum));                       //X21 最高温度编号




            ReturnbyteSource.Add(EncodeCommonPlus50(data.MinBatTemp));                        //X22 最低电池温度






            ReturnbyteSource.Add(EncodeCommonMinus(data.MinTempDetectionNum));            //X23 最低温度编号




            //X24-X25 相关状态
            ReturnbyteSource.Add(EncodeState(data.BitStateSingleV,
                        data.BitStateSOC,
                        data.BitStateOverI,
                        data.BitStateOverTemp));

            ReturnbyteSource.Add(EncodeState(data.BitStateInsulate,
                                        data.BitStateConnState,
                                        data.BitStateChargePermit,
                                        "00"));

            //X26-X27 BSM报文周期
            ReturnbyteSource.AddRange(EncodeCommon2HexStr(data.BSMPeriod));

            //X28 终止充电原因
            ReturnbyteSource.Add(EncodeState(data.AchievedSOC,
                                    data.AchievedTotalV,
                                    data.AchievedSingleV,
                                    data.BmsPause));


            //X29 BMS终止充电故障原因
            ReturnbyteSource.Add(EncodeState(data.BatOverTempTrouble,
                    data.RelayTrouble,
                    data.Detection2Trouble,
                    data.OtherTrouble));
            //X30 
            ReturnbyteSource.Add(EncodeState(data.InsulateTrouble,
                    data.OutputConnTrouble,
                    data.BmsConnTempTrouble,
                    data.ChargeConnTrouble));

            //X31 BMS终止充电错误原因
            ReturnbyteSource.Add(EncodeState(data.OverIError,
                    data.UnusualVError,
                    "00",
                    "00"));

            //X32-X33 BST报文周期
            ReturnbyteSource.AddRange(EncodeCommon2HexStr(data.BSTPeriod));








            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        /// <summary>
        /// 读取充电桩充电阶段数据（CCS/CST）
        /// </summary>
        /// <returns></returns>
        public byte[] BMSReadCCSData()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x13 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            for (int i = 0; i < 16; i++)
            {
                ReturnbyteSource.Add(0x00);//0000  0000   
            }

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public void ParseChargingData(byte[] buffer, out double ChargingVolt, out double ChargingCurrent)
        {
            ChargingVolt = 0;
            ChargingCurrent = 0;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int temp = Convert.ToInt32(buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                    ChargingVolt = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16);
                    ChargingCurrent = Convert.ToDouble(temp) / 10;
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public byte[] BMSReadData(EmChargerType emChargerType)
        {
            byte[] BMSBuffer = null;
            switch (emChargerType)
            {
                case EmChargerType.Charger_GB_DC:
                    BMSBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x16, 0x96 };
                    break;

                case EmChargerType.Charger_EUR_DC:
                    BMSBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x06, 0x86 };
                    //BMSBuffer = new byte[] { 0x7e, 0x30, 0x30, 0x46, 0x46, 0x30, 0x31, 0x30, 0X34, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x0d, 0x43, 0x34 };
                    break;
                case EmChargerType.Charger_JP_DC:
                    BMSBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0xb2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0xe1, 0x8e };
                    break;
                case EmChargerType.Charger_GB_AC:
                    BMSBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0xe6, 0x99 };
                    break;

                default:
                    BMSBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x16, 0x96 };
                    break;
            }
            return BMSBuffer;
        }
        /// <summary>
        /// 解析交流BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_AC_StateData GetBMS_AC_StateData(byte[] buffer, int chargerID)
        {
            BMS_AC_StateData bms = new BMS_AC_StateData();
            bms.ChargerID = chargerID;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int temp = Convert.ToInt32(buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                    bms.PhaseA_Voltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16);
                    bms.PhaseB_Voltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                    bms.PhaseC_Voltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2"), 16);
                    bms.PhaseA_Current = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                    bms.PhaseB_Current = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2"), 16);
                    bms.PhaseC_Current = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16);
                    bms.ChargePower = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2"), 16);
                    bms.ChargeKwh = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[21].ToString("x2") + buffer[22].ToString("x2"), 16);
                    bms.CPDutyCycle = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[23].ToString("x2") + buffer[24].ToString("x2"), 16);
                    bms.CPVoltage = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[25].ToString("x2") + buffer[26].ToString("x2") + buffer[27].ToString("x2") + buffer[28].ToString("x2"), 16);
                    bms.CPFrequency = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[29].ToString("x2") + buffer[30].ToString("x2") + buffer[31].ToString("x2") + buffer[32].ToString("x2"), 16);// 29 30 31 32  美标CS电压   国标 CC电阻
                    bms.CCResistance = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[33].ToString("x2") + buffer[34].ToString("x2"), 16);
                    bms.AllowChargingCurrent = Convert.ToDouble(temp) / 100;
                    //temp = Convert.ToInt32(buffer[35].ToString("x2") + buffer[36].ToString("x2"), 16);
                    //bms.线缆额定电流 = Convert.ToDouble(temp) ;                
                    temp = Convert.ToInt32(buffer[37].ToString("x2") + buffer[38].ToString("x2"), 16);
                    bms.ChargerTemp = Convert.ToDouble(temp) / 100;
                    temp = Convert.ToInt32(buffer[39].ToString("x2"), 16);
                    switch (temp)
                    {
                        case 0:
                            bms.ConnectState = "未连接";
                            break;
                        case 1:
                            bms.ConnectState = "已连接";
                            break;
                        default:
                            bms.ConnectState = "未连接";
                            break;
                    }
                    temp = Convert.ToInt32(buffer[40].ToString("x2"), 16);
                    switch (temp)
                    {
                        case 0:
                            bms.SystemState = "充电过压";
                            break;
                        case 1:
                            bms.SystemState = "充电欠压";
                            break;
                        case 2:
                            bms.SystemState = "充电过流";
                            break;
                        case 3:
                            bms.SystemState = "待机中";
                            break;
                        case 4:
                            bms.SystemState = "充电中";
                            break;
                        case 5:
                            bms.SystemState = "24V故障";
                            break;
                        case 6:
                            bms.SystemState = "连接超时";
                            break;
                        case 7:
                            bms.SystemState = "电表故障";
                            break;
                        case 8:
                            bms.SystemState = "A相过压";
                            break;
                        case 9:
                            bms.SystemState = "B相过压";
                            break;
                        case 10:
                            bms.SystemState = "C相过压";
                            break;
                        case 11:
                            bms.SystemState = "A相过流";
                            break;
                        case 12:
                            bms.SystemState = "B相过流";
                            break;
                        case 13:
                            bms.SystemState = "C相过流";
                            break;
                        case 14:
                            bms.SystemState = "温度故障";
                            break;
                        case 15:
                            bms.SystemState = "急停";
                            break;
                        case 16:
                            bms.SystemState = "A枪CC电阻异常";
                            break;
                        case 17:
                            bms.SystemState = "B枪CC电阻异常";
                            break;
                        default:
                            bms.SystemState = "系统待机中";
                            break;
                    }
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (!string.IsNullOrEmpty(Customer) && Customer.Equals("TS") && buffer.Length > 85)
                    {
                        temp = Convert.ToInt32(buffer[77].ToString("x2") + buffer[78].ToString("x2") + buffer[79].ToString("x2") + buffer[80].ToString("x2"), 16);
                        bms.ChargePower = Convert.ToDouble(temp) / 10000;
                        temp = Convert.ToInt32(buffer[81].ToString("x2") + buffer[82].ToString("x2") + buffer[83].ToString("x2") + buffer[84].ToString("x2"), 16);
                        bms.ChargeKwh = Convert.ToDouble(temp) / 1000;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return bms;
        }

        /// <summary>
        /// 解析直流BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_DC_StateData GetBMS_DC_StateData(byte[] buffer, int chargerID)
        {

            BMS_DC_StateData bms = new BMS_DC_StateData();
            bms.ChargerID = chargerID;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int temp = Convert.ToInt32(buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                    bms.ChargingVoltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16);
                    bms.ChargingCurrent = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                    bms.DCPulsTemp = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2"), 16);
                    bms.DCMinusTemp = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                    bms.EnvironmentTemp = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2"), 16);
                    bms.CC1Voltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16);
                    bms.CC2Voltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2"), 16);
                    bms.APSVoltage = Convert.ToDouble(temp) / 10;
                    temp = Convert.ToInt32(buffer[21].ToString("x2"), 16);
                    switch (temp)
                    {
                        case 0:
                            bms.ChargingState = "空闲状态";
                            break;
                        case 1:
                            bms.ChargingState = "等待辅源";
                            break;
                        case 2:
                            bms.ChargingState = "等待握手报文";
                            break;
                        case 3:
                            bms.ChargingState = "等待辨识报文SPN2560=0x00";
                            break;
                        case 4:
                            bms.ChargingState = "等待辨识报文SPN2560=0x01";
                            break;
                        case 5:
                            bms.ChargingState = "等待CTS、CML报文";
                            break;
                        case 6:
                            bms.ChargingState = "等待CRO_00报文";
                            break;
                        case 7:
                            bms.ChargingState = "等待CRO_AA报文";
                            break;
                        case 8:
                            bms.ChargingState = "等待充电开始";
                            break;
                        case 9:
                            bms.ChargingState = "充电中";
                            break;
                        case 10:
                            bms.ChargingState = "等待充电的中止报文";
                            break;
                        case 11:
                            bms.ChargingState = "等待充电机充电统计";
                            break;
                        case 12:
                            bms.ChargingState = "完成接收充电数据统计";
                            break;
                        case 13:
                            bms.ChargingState = "充电结束"; ;
                            break;
                        default:
                            bms.ChargingState = "空闲状态";
                            break;
                    }

                }
            }
            catch (Exception e) { }
            return bms;
        }

        /// <summary>
        /// 解析欧标直流BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_EU_DC_StateData GetBMS_EU_DC_StateData(byte[] buffer, int chargerID)
        {

            BMS_EU_DC_StateData bms = new BMS_EU_DC_StateData();
            bms.ChargerID = chargerID;
            try
            {
                int iIndex = 5;
                bms.ChargingVoltage = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                bms.ChargingCurrent = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 1000;
                iIndex += 4;
                bms.ChargingPower = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 10000;
                iIndex += 4;
                bms.ChargingQuantity = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 100;
                iIndex += 4;
                bms.CPDutyCycle = (buffer[iIndex] << 8 | buffer[iIndex + 1]) / 100;
                iIndex += 2;
                bms.CPVoltage = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 100;
                iIndex += 2;
                bms.CPFrequency = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 100;
                iIndex += 4;
                bms.ChargerTemp = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                int temp = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                switch (temp)
                {
                    case 0:
                        bms.SystemState = "Not start";
                        break;
                    case 1:
                        bms.SystemState = "WaitingForCharging";
                        break;
                    case 2:
                        bms.SystemState = "SLAC";
                        break;
                    case 3:
                        bms.SystemState = "SDP";
                        break;
                    case 4:
                        bms.SystemState = "SessionSetupReq";
                        break;
                    case 5:
                        bms.SystemState = "SessionSetupRes";
                        break;
                    case 6:
                        bms.SystemState = "SessionDiscoveryReq";
                        break;
                    case 7:
                        bms.SystemState = "ServiceDiscoveryRes";
                        break;
                    case 8:
                        bms.SystemState = "ServicePaymentSlecionReq";
                        break;
                    case 9:
                        bms.SystemState = "ServicePaymentSelectionRes";
                        break;
                    case 10:
                        bms.SystemState = "ContractAuthenticationReq";
                        break;
                    case 11:
                        bms.SystemState = "ContractAuthenticationRes";
                        break;
                    case 12:
                        bms.SystemState = "ChargeParameterDiscoveryReq";
                        break;
                    case 13:
                        bms.SystemState = "ChargeParameterDiscoveryRes"; ;
                        break;
                    case 14:
                        bms.SystemState = "PowerDeliveryReq";
                        break;
                    case 15:
                        bms.SystemState = "PowerDeliveryRes";
                        break;
                    case 16:
                        bms.SystemState = "CableCheckReq";
                        break;
                    case 17:
                        bms.SystemState = "CableCheckRes";
                        break;
                    case 18:
                        bms.SystemState = "PreChargeReq";
                        break;
                    case 19:
                        bms.SystemState = "PreChargeRes";
                        break;
                    case 20:
                        bms.SystemState = "CurrentDemandReq";
                        break;
                    case 21:
                        bms.SystemState = "CurrentDemandRes";
                        break;
                    case 22:
                        bms.SystemState = "WeldingDetectionReq";
                        break;
                    case 23:
                        bms.SystemState = "WeldingDetectionRes";
                        break;
                    case 24:
                        bms.SystemState = "SessionStopReq";
                        break;
                    case 25:
                        bms.SystemState = "SessionStopRes";
                        break;
                    default:
                        bms.SystemState = "Undefined";
                        break;
                }
                iIndex++;
                bms.ErrorMessage = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);

            }
            catch (Exception e) { }
            return bms;
        }

        /// <summary>
        /// 解析欧标直流BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_USA_DC_StateData GetBMS_USA_DC_StateData(byte[] buffer, int chargerID)
        {

            BMS_USA_DC_StateData bms = new BMS_USA_DC_StateData();
            bms.ChargerID = chargerID;
            try
            {
                int iIndex = 5;
                bms.ChargingVoltage = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                bms.ChargingCurrent = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 1000;
                iIndex += 4;
                bms.ChargingPower = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 10000;
                iIndex += 4;
                bms.ChargingQuantity = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 100;
                iIndex += 4;
                bms.CPDutyCycle = (buffer[iIndex] << 8 | buffer[iIndex + 1]) / 100;
                iIndex += 2;
                bms.CPVoltage = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 100;
                iIndex += 2;
                bms.CPFrequency = (float)(buffer[iIndex] << 24 | buffer[iIndex + 1] << 16 | buffer[iIndex + 2] << 8 | buffer[iIndex + 3]) / 100;
                iIndex += 4;
                bms.ChargerTemp = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                int temp = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                switch (temp)
                {
                    case 0:
                        bms.SystemState = "Not start";
                        break;
                    case 1:
                        bms.SystemState = "WaitingForCharging";
                        break;
                    case 2:
                        bms.SystemState = "SLAC";
                        break;
                    case 3:
                        bms.SystemState = "SDP";
                        break;
                    case 4:
                        bms.SystemState = "SessionSetupReq";
                        break;
                    case 5:
                        bms.SystemState = "SessionSetupRes";
                        break;
                    case 6:
                        bms.SystemState = "SessionDiscoveryReq";
                        break;
                    case 7:
                        bms.SystemState = "ServiceDiscoveryRes";
                        break;
                    case 8:
                        bms.SystemState = "ServicePaymentSlecionReq";
                        break;
                    case 9:
                        bms.SystemState = "ServicePaymentSelectionRes";
                        break;
                    case 10:
                        bms.SystemState = "ContractAuthenticationReq";
                        break;
                    case 11:
                        bms.SystemState = "ContractAuthenticationRes";
                        break;
                    case 12:
                        bms.SystemState = "ChargeParameterDiscoveryReq";
                        break;
                    case 13:
                        bms.SystemState = "ChargeParameterDiscoveryRes"; ;
                        break;
                    case 14:
                        bms.SystemState = "PowerDeliveryReq";
                        break;
                    case 15:
                        bms.SystemState = "PowerDeliveryRes";
                        break;
                    case 16:
                        bms.SystemState = "CableCheckReq";
                        break;
                    case 17:
                        bms.SystemState = "CableCheckRes";
                        break;
                    case 18:
                        bms.SystemState = "PreChargeReq";
                        break;
                    case 19:
                        bms.SystemState = "PreChargeRes";
                        break;
                    case 20:
                        bms.SystemState = "CurrentDemandReq";
                        break;
                    case 21:
                        bms.SystemState = "CurrentDemandRes";
                        break;
                    case 22:
                        bms.SystemState = "";
                        break;
                    case 23:
                        bms.SystemState = "";
                        break;
                    case 24:
                        bms.SystemState = "";
                        break;
                    case 25:
                        bms.SystemState = "";
                        break;
                    default:
                        bms.SystemState = "";
                        break;
                }
                iIndex++;
                bms.ErrorMessage = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);

            }
            catch (Exception e) { }
            return bms;
        }

        /// <summary>
        /// 解析日标直流BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_JP_DC_StateData GetBMS_JP_DC_StateData(byte[] buffer, int chargerID)
        {

            BMS_JP_DC_StateData bms = new BMS_JP_DC_StateData();
            bms.ChargerID = chargerID;
            try
            {
                int iIndex = 5;
                bms.ChargingVoltage = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                bms.ChargingCurrent = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                bms.ChargerTemp = (float)(buffer[iIndex] << 8 | buffer[iIndex + 1]) / 10;
                iIndex += 2;
                bms.Version = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                iIndex += 1;
                bms.Signal_d1 = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                iIndex += 1;
                bms.Signal_d2 = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                iIndex += 1;
                bms.Signal_k = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                iIndex += 1;
                bms.ProxDetect = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                iIndex += 1;
                int temp = Convert.ToInt32(buffer[iIndex].ToString("x2"), 16);
                switch (temp)
                {
                    case 0:
                        bms.SystemState = "空闲状态";
                        break;
                    case 1:
                        bms.SystemState = "等待充电";
                        break;
                    case 2:
                        bms.SystemState = "等待H108、H109";
                        break;
                    case 3:
                        bms.SystemState = "锁止判断";
                        break;
                    case 4:
                        bms.SystemState = "绝缘检测";
                        break;
                    case 5:
                        bms.SystemState = "准备充电";
                        break;
                    case 6:
                        bms.SystemState = "电池检测";
                        break;
                    case 7:
                        bms.SystemState = "正在充电";
                        break;
                    case 8:
                        bms.SystemState = "输出停止";
                        break;
                    case 9:
                        bms.SystemState = "输出停止";
                        break;
                    case 10:
                        bms.SystemState = "焊接检测";
                        break;
                    case 11:
                        bms.SystemState = "d2信号断开";
                        break;
                    case 12:
                        bms.SystemState = "d1信号断开";
                        break;
                    case 13:
                        bms.SystemState = "充电解锁"; ;
                        break;
                    case 14:
                        bms.SystemState = "充电结束";
                        break;
                    default:
                        bms.SystemState = "";
                        break;
                }

            }
            catch (Exception e) { }
            return bms;
        }

        public bool ParseCombineKState_DC(byte[] buff)
        {
            try
            {
                return Convert.ToInt32(buff[5]) == 1;
            }
            catch { return false; }
        }

        public List<bool> ParseKState(byte[] buff)
        {
            try
            {
                if (buff == null) return null;

                List<bool> list = new List<bool>();
                byte state = buff[5];
                byte calcByte = 0x01;
                for (int i = 7; i >= 0; i--)
                {
                    list.Add(Convert.ToByte(state >> i & 0x01) == calcByte);
                }
                state = buff[6];
                for (int i = 7; i >= 0; i--)
                {
                    list.Add(Convert.ToByte(state >> i & 0x01) == calcByte);
                }
                return list;
            }
            catch { return null; }
        }

        public List<bool> ParseKState_DC(byte[] buff, out double R4Resistance, out double BMSVolatage)
        {
            R4Resistance = 0;
            BMSVolatage = 0;
            try
            {
                if (buff == null || buff.Length != 16)
                {
                    R4Resistance = 0;
                    BMSVolatage = 0;
                    return null;
                }
                R4Resistance = Convert.ToInt32(buff[5].ToString("x2") + buff[6].ToString("x2"), 16);
                BMSVolatage = Convert.ToDouble(Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2"), 16)) / 10;
                List<bool> list = new List<bool>();
                int state = 0;
                switch (buff[9])
                {
                    case 0x80:
                        state = 0;
                        break;
                    case 0x40:
                        state = 1;
                        break;
                    case 0x20:
                        state = 2;
                        break;
                    case 0x10:
                        state = 3;
                        break;
                    case 0x08:
                        state = 4;
                        break;
                    case 0x04:
                        state = 5;
                        break;
                    case 0x02:
                        state = 6;
                        break;
                    case 0x01:
                        state = 7;
                        break;
                }
                for(int i = 0; i < 8; i++)
                {
                    list.Add(i == state);
                }

                state = 0;
                switch (buff[10])
                {
                    case 0x80:
                        state = 0;
                        break;
                    case 0x40:
                        state = 1;
                        break;
                    case 0x20:
                        state = 2;
                        break;
                    case 0x10:
                        state = 3;
                        break;
                    case 0x08:
                        state = 4;
                        break;
                    case 0x04:
                        state = 5;
                        break;
                    case 0x02:
                        state = 6;
                        break;
                    case 0x01:
                        state = 7;
                        break;
                }
                for (int i = 0; i < 8; i++)
                {
                    list.Add(i == state);
                }

                byte calcByte = 0x01;
                state = buff[11];

                for (int i = 7; i >= 0; i--)
                {
                    list.Add(Convert.ToByte(state >> i & 0x01) == calcByte);
                }
                state = buff[12];

                for (int i = 7; i >= 0; i--)
                {
                    list.Add(Convert.ToByte(state >> i & 0x01) == calcByte);
                }
                return list;
            }
            catch { return null; }
        }
        public List<int> ParseKState_EU_DC(byte[] buff, out double BMSVolatage)
        {
            BMSVolatage = 0;
            try
            {
                if (buff == null || buff.Length != 13)
                {
                    BMSVolatage = 0;
                    return null;
                }
                List<int> list = new List<int>();
                int iIndex = 5;
                //DC+DC-控制(bit7):
                //1:开启    0:关闭
                //CC信号控制(bit6):
                //1:开启    0:关闭
                //CP信号控制(bit5):
                //1:开启    0:关闭
                //转换板电源控制(bit4):
                //1:开启    0:关闭
                //PE控制(bit3):
                //1:开启    0:关闭
                //CP接120欧对地控制(bit2):
                //1:开启    0:关闭
                //电子锁控制(bit1):
                //1:开启    0:关闭
                list.Add((buff[iIndex] >> 7) & 0x01);
                list.Add((buff[iIndex] >> 6) & 0x01);
                list.Add((buff[iIndex] >> 5) & 0x01);
                list.Add((buff[iIndex] >> 4) & 0x01);
                list.Add((buff[iIndex] >> 3) & 0x01);
                list.Add((buff[iIndex] >> 2) & 0x01);
                list.Add((buff[iIndex] >> 1) & 0x01);
                iIndex++;
                //输出过压(bit1):
                //1:过压    0:正常
                //停止报文(bit0):
                //1:开启    0:关闭
                list.Add((buff[iIndex] >> 1) & 0x01);
                list.Add((buff[iIndex] >> 0) & 0x01);
                iIndex++;
                //CP信号断开(bit1):
                //1:开启    0:关闭
                //CP二极管短接(bit0):
                //  1:开启    0:关闭
                list.Add((buff[iIndex] >> 1) & 0x01);
                list.Add((buff[iIndex] >> 0) & 0x01);
                iIndex++;
                //DC+绝缘阻值档位：
                //0：对地不漏电
                //1：对地22.9kΩ
                //2：对地24.8kΩ
                //3：对地29.7kΩ
                //4：对地33kΩ
                //5：对地75kΩ
                //6：对地100kΩ
                //7：对地300kΩ
                list.Add(buff[iIndex]);
                iIndex++;
                //预留
                iIndex++;
                //DC-绝缘阻值档位：
                //0：对地不漏电
                //1：对地22.9kΩ
                //2：对地24.8kΩ
                //3：对地29.7kΩ
                //4：对地33kΩ
                //5：对地75kΩ
                //6：对地100kΩ
                //7：对地300kΩ
                list.Add(buff[iIndex]);
                iIndex++;
                BMSVolatage = (double)(buff[iIndex] << 8 | buff[iIndex + 1]) / 10;
                return list;
            }
            catch { return null; }
        }
        public byte[] ParameterCommand_EU_DC(byte tComm)//    
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(tComm);//0000  0000   

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x0d);//
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000

            byte[] CheckSumByte = CheckOut.ToASCII_EU(ReturnbyteSource.ToArray());//CRC校验函数。

            ReturnbyteSource.AddRange(CheckSumByte);

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public List<double> ParameterRead_EU_DC(byte tComm, byte[] buff)//    
        {
            int index = 5;
            List<double> ReturnSource = new List<double>();
            switch (tComm)
            {
                case 0x90:
                    //0   1   PLC状态
                    //1   1   EVCC-SECC SDP 连接状态
                    //2   4   EVSE状态
                    //6   1   由EVSE完成收费请求
                    //7   1   EVSE进程状态
                    ReturnSource.Add((buff[index] >> 0) & 0x01);
                    ReturnSource.Add((buff[index] >> 1) & 0x01);
                    ReturnSource.Add((buff[index] >> 2) & 0x0F);
                    ReturnSource.Add((buff[index] >> 6) & 0x01);
                    ReturnSource.Add((buff[index] >> 7) & 0x01);
                    index++;
                    //8   2   EVSE隔离状态
                    //10  4   充电器连接器类型支持EVSE
                    //14  2   EVSE给EV的通知
                    ReturnSource.Add((buff[index] >> 0) & 0x03);
                    ReturnSource.Add((buff[index] >> 2) & 0x0F);
                    ReturnSource.Add((buff[index] >> 6) & 0x03);
                    index++;
                    //16  1   供电状态
                    //17  1   SECC (GQ模块)收费完成请求
                    //18  3   CP状态
                    //21  1   S2开关状态
                    //22  2   PD状态
                    ReturnSource.Add((buff[index] >> 0) & 0x01);
                    ReturnSource.Add((buff[index] >> 1) & 0x01);
                    ReturnSource.Add((buff[index] >> 2) & 0x07);
                    ReturnSource.Add((buff[index] >> 0) & 0x01);
                    ReturnSource.Add((buff[index] >> 0) & 0x03);
                    index++;
                    //24  7   PWM占空比
                    //31  1   锁状态
                    ReturnSource.Add((buff[index] >> 0) & 0x7F);
                    ReturnSource.Add((buff[index] >> 7) & 0x01);
                    index++;
                    //32  8   平均衰减增益
                    //40  8   PLC错误码
                    //48  8   EVSE-EV充电步骤
                    //56  8   进行EVSE通知的时间限制
                    ReturnSource.Add(buff[index]);
                    index++;
                    ReturnSource.Add(buff[index]);
                    index++;
                    ReturnSource.Add(buff[index]);
                    index++;
                    ReturnSource.Add(buff[index]);
                    break;
                case 0x91:
                    //0   16  EVSE最大输出电压
                    //16  16  EVSE最大输出电流
                    //32  16  EVSE当前输出电压
                    //48  16  EVSE当前输出电流
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    break;
                case 0x92:
                    //0   16  EVSE的最小电压
                    //16  16  EVSE的最小电流
                    //32  16  EVSE的最大功率
                    //48  2   锁状态
                    //50  2   状态锁定报警
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) * 10);
                    index += 2;
                    ReturnSource.Add(buff[index] & 0x03);
                    ReturnSource.Add(buff[index] >> 2 & 0x03);
                    break;
                case 0x97:
                    //1   电动汽车已经准备好进行能量转移
                    //4   电动汽车开始
                    //1   接触器状态
                    //1   接触器接通后充电启动和停止
                    //1   充电完成标志
                    ReturnSource.Add(buff[index] & 0x01);
                    ReturnSource.Add(buff[index] >> 1 & 0x0F);
                    ReturnSource.Add(buff[index] >> 5 & 0x01);
                    ReturnSource.Add(buff[index] >> 6 & 0x01);
                    ReturnSource.Add(buff[index] >> 7 & 0x01);
                    index++;
                    //1   充满完成标志
                    //1   充电完成标志
                    //1   连接检测请求
                    //4   EV支持的充电口类型
                    //1   S2打开/关闭请求
                    ReturnSource.Add(buff[index] & 0x01);
                    ReturnSource.Add(buff[index] >> 1 & 0x01);
                    ReturnSource.Add(buff[index] >> 2 & 0x01);
                    ReturnSource.Add(buff[index] >> 3 & 0x0F);
                    ReturnSource.Add(buff[index] >> 7 & 0x01);
                    index++;
                    //1   入口锁请求
                    //2   AAG 值匹配状态
                    ReturnSource.Add(buff[index] & 0x01);
                    ReturnSource.Add((buff[index] >> 1 & 0x03));
                    index++;
                    //8   RESS SoC值
                    ReturnSource.Add(buff[index] * 0.5);
                    index++;
                    //16  BMS最大电流值
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    //16  BMS最大电压值
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    break;
                case 0x98:
                    //0   8   Full SoC值
                    //8   8   Bulk SoC值
                    //16  16  EV目标需求电压
                    //32  16  EV目标需求电流
                    //48  16  预充充电电压
                    ReturnSource.Add(buff[index] * 0.5);
                    index++;
                    ReturnSource.Add(buff[index] * 0.5);
                    index++;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]) / 10);
                    break;
                case 0x99:
                    //0   16  剩余时间到满SoC
                    //16  16  剩余时间到Bulk SoC
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]));
                    index += 2;
                    ReturnSource.Add((buff[index] << 8 | buff[index + 1]));
                    break;
            }
            return ReturnSource;
        }
        /// <summary>
        /// 16进制字符串转byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                hexString = hexString.PadLeft(hexString.Length + 1, '0');
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return returnBytes;
            }
            else
            {
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return returnBytes;
            }

        }

        public byte[] BMSRead(byte type, byte command)//BMS 
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, type, command };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSGetK3K4Time()//BMS 
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x73 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public byte[] BMSSetProtocolTime()
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x33 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            var dtNow = DateTime.Now;
            ReturnbyteSource.Add((byte)(dtNow.Year >> 8));
            ReturnbyteSource.Add((byte)dtNow.Month);
            ReturnbyteSource.Add((byte)dtNow.Day);
            ReturnbyteSource.Add((byte)dtNow.Hour);
            ReturnbyteSource.Add((byte)dtNow.Minute);
            ReturnbyteSource.Add((byte)dtNow.Second);
            ReturnbyteSource.Add((byte)(dtNow.Millisecond >> 8));
            ReturnbyteSource.Add((byte)dtNow.Millisecond);

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public void GetVersion(byte[] buff, out string SoftwareVersion, out string FlowNumber)
        {
            SoftwareVersion = "";
            SoftwareVersion += $"V{buff[5]}.{buff[6]}{buff[7]} ";
            SoftwareVersion += $"{buff[8] << 8| buff[9]}{buff[10]}{buff[11]}";
            FlowNumber = Convert.ToInt32(buff[12]).ToString("X2");
        }

        public void GetK3K4Time(byte[] buff,out int K3K4Time)
        {
            try
            {
                if(buff.Length>10)
                {
                    // 大端序排列
                    K3K4Time = (buff[8] << 0) |
                           (buff[7] << 8) |
                           (buff[6] << 16) |
                           (buff[5] << 24);

                }else
                {
                    K3K4Time =-999;
                }
            }
            catch
            {
                K3K4Time = -999;
            }

        }

        public List<int> BMSGetData_JP_DC(byte[] buff, out int[] ErrorSign, out int[] StateSign)
        {
            List<int> ReturnSource = new List<int>();
            int iIndex = 7;
            ReturnSource.Add(buff[iIndex] << 8 | buff[iIndex + 1]);
            iIndex += 2;
            ReturnSource.Add(buff[iIndex] << 8 | buff[iIndex + 1]);
            iIndex += 2;
            ReturnSource.Add(buff[iIndex]);
            iIndex += 3;
            ReturnSource.Add(buff[iIndex]);
            iIndex++;
            ReturnSource.Add(buff[iIndex]);
            iIndex++;
            ReturnSource.Add(buff[iIndex]);
            iIndex += 5;
            ReturnSource.Add(buff[iIndex]);
            iIndex++;
            ReturnSource.Add(buff[iIndex] << 8 | buff[iIndex + 1]);
            iIndex += 2;
            ReturnSource.Add(buff[iIndex]);
            iIndex++;
            List<int> lstSign = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                lstSign.Add((buff[iIndex] >> i) & 0x01);
            }
            ErrorSign = lstSign.ToArray();
            iIndex++;
            lstSign = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                lstSign.Add((buff[iIndex] >> i) & 0x01);
            }
            StateSign = lstSign.ToArray();
            iIndex++;
            ReturnSource.Add(buff[iIndex]);

            return ReturnSource;
        }

        public byte[] BMSSetData_JP_DC(string MinBatteryVolt, string MaxBatteryVolt, string ChargingRateConst, string MaxChargingTime_S, string MaxChargingTime_M,
            string ChargingET, string CHAdeMONumber, string TargetBatteryVolt, string ChargingCurrent, int[] ErrorSign, int[] StateSign, string ChargingRate)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0xb1 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)(Convert.ToInt32(MinBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(MinBatteryVolt));
            ReturnbyteSource.Add((byte)(Convert.ToInt32(MaxBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(MaxBatteryVolt));
            ReturnbyteSource.Add((byte)Convert.ToInt32(ChargingRateConst));
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)(Convert.ToInt32(MaxChargingTime_S) / 10));
            ReturnbyteSource.Add((byte)Convert.ToInt32(MaxChargingTime_M));
            ReturnbyteSource.Add((byte)Convert.ToInt32(ChargingET));
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)Convert.ToInt32(CHAdeMONumber));
            ReturnbyteSource.Add((byte)(Convert.ToInt32(TargetBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(TargetBatteryVolt));
            ReturnbyteSource.Add((byte)Convert.ToInt32(ChargingCurrent));
            int Temp = 0;
            for(int i = 0; i < 8; i++)
            {
                Temp |= ErrorSign[i] << i;
            }
            ReturnbyteSource.Add((byte)Temp);
            Temp = 0;
            for (int i = 0; i < 8; i++)
            {
                Temp |= StateSign[i] << i;
            }
            ReturnbyteSource.Add((byte)Temp);
            ReturnbyteSource.Add((byte)Convert.ToInt32(ChargingRate));
            ReturnbyteSource.Add(0x00);

            ReturnbyteSource.Add(0x0d);//结束
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        public byte[] BMSSetData_JP_DC(SettingCharging_JP_DC data)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0xb1 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)(Convert.ToInt32(data.MinBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.MinBatteryVolt));
            ReturnbyteSource.Add((byte)(Convert.ToInt32(data.MaxBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.MaxBatteryVolt));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.ChargingRateConst));
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)(Convert.ToInt32(data.MaxChargingTime_s) / 10));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.MaxChargingTime_m));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.EstimatedChargingTime_m));
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.CHAdeMOProtocolNumber));
            ReturnbyteSource.Add((byte)(Convert.ToInt32(data.TargetBatteryVolt) >> 8));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.TargetBatteryVolt));
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.ChargingCurrent));
            int Temp = 0;
            for (int i = 0; i < 8; i++)
            {
                Temp |= data.FaultFlags[i] << i;
            }
            ReturnbyteSource.Add((byte)Temp);
            Temp = 0;
            for (int i = 0; i < 8; i++)
            {
                Temp |= data.StateFlags[i] << i;
            }
            ReturnbyteSource.Add((byte)Temp);
            ReturnbyteSource.Add((byte)Convert.ToInt32(data.ChargingRate));
            ReturnbyteSource.Add(0x00);

            ReturnbyteSource.Add(0x0d);//结束
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public List<bool> ParseKState_JP_DC(byte[] RecvData, out int DCPRState, out int DCMRState, out double BatteryVolt)
        {
            List<bool> KState = new List<bool>();
            for(int i = 0; i < 8;i++)
            {
                KState.Add(((RecvData[5] >> i) & 0x01) == 1);
            }
            for (int i = 0; i < 2; i++)
            {
                KState.Add(((RecvData[6] >> i) & 0x01) == 1);
            }
            DCPRState = RecvData[8];
            DCMRState = RecvData[10];
            BatteryVolt = (RecvData[11] << 8 | RecvData[12]) / 10;
            return KState;
        }

    }

}
