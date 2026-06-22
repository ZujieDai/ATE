using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    class CommonCAN
    {
        public static MutiPackage MutiPackage=new MutiPackage();//多包报文
        public static ValueManager ValueManager=new ValueManager();//add for 时间增量
        public enum TransitState
        {
            Ready,
            Finish,
            Reject
        }
        /// <summary>
        /// 为了兼容原来的报文解析，把16进制byte转换成ASCII型byte
        /// </summary>
        /// <param name="cont"></param>
        /// <returns></returns>
        public static EachFrameModel HexToASCIIByte(EachFrameModel cont)
        {
            EachFrameModel tmp = new EachFrameModel();
            string sstr = "";
            //首先把所有的16进制byte数组转换成16进制字符串
            for (int i = 0; i < cont.Buffer.Count; i++)
            {
                sstr += BaseConvert.ByteToHexLen2(cont.Buffer[i]);
            }
            //把字符串转换成ASCII值数组
            for (int i = 0; i < sstr.Length; i++)
            {
                if (i == 0)//开始标志
                {
                    tmp.Buffer.Add(0x7E);
                }
                else if (i == 1)
                {

                }
                else if (i == sstr.Length - 6)//结束标志
                {
                    tmp.Buffer.Add(0x0D);
                }
                else if (i == sstr.Length - 5)
                {

                }
                else
                {
                    tmp.Buffer.Add(Encoding.Default.GetBytes(sstr.Substring(i, 1).ToUpper())[0]);
                }
            }
            tmp.Len = cont.Len * 2;
            tmp.Cmd = cont.Cmd;

            return tmp;
        }
    }
}
