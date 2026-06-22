using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    public static class CommonOscilloscope
    {
        public static decimal ContentEDataChangeNum_D(string GetStr)
        {
            string StrReturn = "";
            Decimal dData = 0;//十进制的数值

            if (GetStr.Contains("E") || GetStr.Contains("e"))
            {

                dData = BackTryDecimalParse(GetStr);//9.512E+00，把字符串转换成数字

                //dData = Convert.ToDecimal(Decimal.Parse(str2.ToString(), System.Globalization.NumberStyles.Float));
                //StrReturn = dData.ToString("#0.000");
                StrReturn = dData.ToString();
            }

            try
            {
                return Convert.ToDecimal(StrReturn);
            }
            catch
            {
                return 0;
            }


        }
        /// //////////////////////////////////////////////////////////////////////////

        /// <param name="GNumFloat"></param>
        /// <returns></returns>
        //因为防止报错，所以使用了TryParse，而没有用Parse
        //字符串转数字。
        public static Decimal BackTryDecimalParse(string GNumFloat)
        {
            GNumFloat = GNumFloat.Trim(new char[] { '\n', '\r', 'S', 's', 'H', 'h', 'Z', 'z', 'V', 'A', 'v', 'a', '%' });
            //string BackString = "";
            Decimal BacknumFloat;
            Decimal numFloat;
            Decimal.TryParse(GNumFloat, out numFloat);
            Decimal.TryParse(GNumFloat, System.Globalization.NumberStyles.Float, null, out numFloat);
            BacknumFloat = numFloat;

            return BacknumFloat;
        }

        public static Decimal ContentEDataChangeDec(string GetStr, int chnnelNum)
        {
            // Decimal StrReturn = 0;
            Decimal dData = 0;//十进制的数值

            if (GetStr.Contains("E") || GetStr.Contains("e"))
            {

                dData = BackTryDecimalParse(GetStr);//9.512E+00，把字符串转换成数字

                //dData = Convert.ToDecimal(Decimal.Parse(str2.ToString(), System.Globalization.NumberStyles.Float));
                // StrReturn = dData.ToString("#0.000");
                if (chnnelNum != 3)//示波器通道1是交流电压，通道2是交流电流，通道3是CP直流电压；通道3不需要去绝对值
                {
                    if (dData < 0)
                    { dData = -1 * dData; }//取绝对值
                }
            }

            return dData;

        }

        public static Decimal ContentEDataChangeDec(string GetStr)
        {
            // Decimal StrReturn = 0;
            Decimal dData = 0;//十进制的数值

            if (GetStr.Contains("E") || GetStr.Contains("e"))
            {

                dData = BackTryDecimalParse(GetStr);//9.512E+00，把字符串转换成数字

                //if (dData < 0)
                //{ dData = -1 * dData; }//取绝对值

            }

            return dData;

        }

        public static string RemoveNonNumericCharacters(string inputString)
        {
            string resultString = "";

            for (int i = 0; i < inputString.Length; i++)
            {
                if (Char.IsDigit(inputString[i]))
                {
                    resultString += inputString[i];
                }
            }

            return resultString;
        }
    }
}
