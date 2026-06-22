using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public static class BaseConvert
    {
        public static byte[] HexStringToBytes(string str)
        {
            try
            {
                if (str == null || str == string.Empty)
                {
                    return null;
                }

                str = str.Trim();

                int i;
                string[] hexStrs = str.Split(' ');

                byte[] buf = new byte[hexStrs.Length];

                for (i = 0; i < hexStrs.Length; i++)
                {
                    buf[i] = Convert.ToByte(hexStrs[i], 16);
                }
                return buf;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] CutArrs(byte[] src, int fromIndex, int takeLen)
        {
            byte[] arr = src.Skip(fromIndex).Take(takeLen).ToArray();
            return arr;
        }
        public static byte[] CutLists(List<byte> lists, int fromIndex, int len)
        {
            byte[] arr = lists.Skip(fromIndex).Take(len).ToArray();
            return arr;
        }
        public static string[] CutArrs(string[] src, int fromIndex, int takeLen)
        {
            string[] arr = src.Skip(fromIndex).Take(takeLen).ToArray();
            return arr;
        }
        public static List<byte> CutLists2Lists(List<byte> lists, int fromIndex, int len)
        {
            List<byte> arr = new List<byte>();
            arr = lists.GetRange(fromIndex, len);
            return arr;
        }
        public static string AsciiBytes2String(byte[] buf)
        {
            return Encoding.GetEncoding("ascii").GetString(buf);
        }
        public static string AsciiBytes2String(List<byte> buf)
        {
            byte[] arr = buf.ToArray();
            return Encoding.GetEncoding("ascii").GetString(arr);
        }
        public static int HexStr2Int32(string hex)
        {
            return Convert.ToInt32(hex, 16);
        }
        public static int Ascii2Int32(byte ascii)
        {
            if (ascii == 'a' || ascii == 'A')
                return 10;
            else if (ascii == 'b' || ascii == 'B')
                return 11;
            else if (ascii == 'c' || ascii == 'C')
                return 12;
            else if (ascii == 'd' || ascii == 'D')
                return 13;
            else if (ascii == 'e' || ascii == 'e')
                return 14;
            else if (ascii == 'f' || ascii == 'F')
                return 14;
            else
            {
                if (ascii >= '0' && ascii <= '9')
                {
                    return ascii - '0';
                }
                return 0;
            }
        }

        public static string AsciiBytes2String(byte[] bytes, int fromIndex, int len)
        {
            byte[] arr = CutArrs(bytes, fromIndex, len);
            return Encoding.GetEncoding("ascii").GetString(arr);
        }
        public static string AsciiBytes2String(List<byte> arr, int fromIndex, int len)
        {
            byte[] bytes = CutLists(arr, fromIndex, len);
            return Encoding.GetEncoding("ascii").GetString(bytes);
        }

        public static string HexString2AsciiString(string hex)  //"30" -> "0"
        {
            byte byte1 = Convert.ToByte(hex, 16);
            string ascii = Encoding.ASCII.GetString(new byte[] { byte1 });
            return ascii;
        }
        public static string HexString2AsciiString(string[] hexs)  //"32", "46" -> "2F"
        {
            List<byte> lists = new List<byte>();
            byte byte1;
            foreach (string hex in hexs)
            {
                byte1 = Convert.ToByte(hex, 16);
                lists.Add(byte1);
            }
            byte[] bytes = lists.ToArray();
            string ascii = Encoding.ASCII.GetString(bytes);
            return ascii;
        }
        //public static int TwoDecimal2Int32(byte byteH, byte byteL)
        //{

        //    int ten = BaseConvert.Ascii2Int32(byteH);
        //    int single = BaseConvert.Ascii2Int32(byteL);

        //    int len = ten * 10 + single;
        //    return len;
        //}
        public static int TwoBytes2Int32(string total, int low, int high)
        {
            string sLow = total.Substring(low, 2);
            string sHigh = total.Substring(high, 2);
            string sTotal = sHigh + sLow;

            return Convert.ToInt32(sTotal, 16);
        }
        public static string GetBitsFromHex(int hex, int startIndex, int len)
        {
            try
            {
                string bin = Convert.ToString(hex, 2);
                bin = bin.PadLeft(8, '0');
                return bin.Substring(8 - startIndex - len, len);
            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public static string GetBytesToString(byte[] buf)
        {
            string bs = "";
            for (int i = 0; i < buf.Length; i++)
            {
                bs = bs + ByteToHexLen2(buf[i]);
            }
            return bs;
        }
        /// <summary>
        /// 将byte转换成16进制字符串,并保持两位的长度：0x0C ====>>>0C
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ByteToHexLen2(byte num)
        {
            string hexstr = "00";
            hexstr = Convert.ToString(num, 16).PadLeft(2, '0').ToUpper();
            return hexstr;
        }
        public static int ConvertHexToInt32(byte[] buf)
        {
            string val = "";
            for (int i = 0; i < buf.Length; i++)
            {
                val += ByteToHexLen2(buf[i]);
            }

            return Convert.ToInt32(val, 16);
        }
        public static int Convert4AsciiToInt32(byte[] buf)
        {
            string val = BaseConvert.AsciiBytes2String(buf);
            //string low = val.Substring(0,2);
            //string high = val.Substring(2, 2);
            //return Convert.ToInt32(high + low, 16);
            return Convert.ToInt32(val, 16);
        }
        public static string Int32ToHexStr(int num)//100 -> "64"
        {
            string text = string.Empty;
            while (num > 0)
            {
                int low = num & 0x0f;
                text = Convert.ToString(low, 16) + text;
                num >>= 4;
            }
            return text;
        }
        public static string Byte2HexStr(byte num)
        {
            string text = string.Empty;
            while (num > 0)
            {
                int low = num & 0x0f;
                text = Convert.ToString(low, 16) + text;
                num >>= 4;
            }
            return text;
        }
        public static string HexStr2BinaryStr(int binLen, string hex)
        {
            try
            {
                int val = Convert.ToInt32(hex, 16);
                string binary = Convert.ToString(val, 2).PadLeft(8, '0');
                return binary;
            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public static string Str2Hex2AsciiStr(string str)
        {
            byte[] result =ConvertCharToBytesUseFormat(str);

            str = "";
            for (int i = 0; i < result.Length; i++)
            {
                str += Convert.ToString(result[i], 16).PadLeft(2, '0');
            }
            return str;
        }
        public static byte[] ConvertCharToBytesUseFormat(string text)
        {
            byte[] result = System.Text.Encoding.Default.GetBytes(text);
            return result;
        }
    }
}
