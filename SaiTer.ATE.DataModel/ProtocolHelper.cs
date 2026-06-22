using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public static class ProtocolHelper
    {

        public static byte[] CalCheckSum(List<byte> lists, ref byte check1, ref byte check2)
        {

            int sum = 0;
            foreach (byte b in lists)
            {
                sum += b;
            }
            string temp = Convert.ToString(sum, 2);
            temp = temp.Replace('1', '-').Replace('0', '1').Replace('-', '0');//按位取反

            int plus = Convert.ToInt32(temp, 2) + 1;
            plus = plus & 0xff;
            int low = plus & 0x0f;
            int high = (plus & 0xf0) >> 4;

            string sLow = Convert.ToString(low, 16).ToUpper();  //0x0d -> "0x0D"
            string sHigh = Convert.ToString(high, 16).ToUpper();

            byte[] ascii1 = Encoding.ASCII.GetBytes(sHigh);     //"0x0D" -> 0x44
            byte[] ascii2 = Encoding.ASCII.GetBytes(sLow);

            check1 = ascii1[0];
            check2 = ascii2[0];

            //return new byte[] { ascii1[0], ascii2[0] };
            return ConvertCharToBytes(sHigh + sLow);
        }

        public static bool CheckSum(List<byte> lists)
        {
            //return true;
            int len = lists.Count;
            if (len <= 2)
                return false;


            byte code1 = lists[len - 2];
            byte code2 = lists[len - 1];
            List<byte> buf = new List<byte>();
            buf = BaseConvert.CutLists2Lists(lists, 0, len - 2);
            byte[] Checkbuff = GetModbusCrc16(buf.ToArray());
            if (Checkbuff[0] == code1 && Checkbuff[1] == code2) return true;

            return false;
        }

        /*************************************************************************
        * CRC16校验方法
        * 
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


        //全部先转化为大写,再转换为byte
        public static byte[] ConvertCharToBytes(string text)
        {
            ////原来的===全部先转化为大写，再转为Ascii码
            //byte[] result = System.Text.Encoding.Default.GetBytes(text.ToUpper());
            //return result;

            text = text.ToUpper();
            byte[] result = new byte[text.Length / 2];
            for (int j = 0; j < text.Length / 2; j++)
            {
                result[j] = Convert.ToByte(text.Substring(j * 2, 2), 16);
            }
            return result;
        }

        //包含字母小写
        //"1fA0" -> 0x31 0x66 0x41 0x30
        public static byte[] ConvertCharToBytesUseFormat(string text)
        {
            byte[] result = System.Text.Encoding.Default.GetBytes(text);
            return result;
        }
        ////拼接2个数组
        //public static int AddValueToBuf(ref byte[] buf, byte[] content)
        //{
        //    buf = buf.Concat(content).ToArray();    
        //    return content.Length;
        //}

    }
}
