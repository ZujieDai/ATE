using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CHM : MsgCommon
    {
        private string MsgHeadLine = "充电机握手";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();           

            string text;

            try
            {
                string version = DecodeVersion(content);
                model.ConsistMsg.Version = version;

                text = "充电机通信协议版本号:" + version;

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }
        private string DecodeVersion(List<byte> content)
        {
            int i = 0;

            string verH = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            int H = Convert.ToInt32(verH, 16);

            string verL_00FF = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            string verL_FF00 = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            string verL = verL_FF00 + verL_00FF;
            int L = Convert.ToInt32(verL, 16);

            string ver = "V" + H.ToString() + "." + L.ToString();
            return ver;
        }
    }
}
