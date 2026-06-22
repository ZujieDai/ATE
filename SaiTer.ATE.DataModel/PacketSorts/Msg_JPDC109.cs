using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_JPDC109 : MsgCommon
    {
        private string MsgHead1 = "CHAdeMO控制协议号";
        private string MsgHead2 = "当前输出电压";
        private string MsgHead3 = "当前充电电流";
        private string MsgHead4 = "状态/故障标志";
        private string MsgHead5 = "剩余充电时间(10s)";
        private string MsgHead6 = "剩余充电时间(min)";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);

            try
            {
                string sTmp ="Ver."+ Function.DecodeCommon1Byte(arr[0]);
                text = Function.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = Function.DecodeCommon2Byte(arr[2], arr[1]) + "V";
                text += Function.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[3]) + "A";
                text += Function.TextAddColonSpace(MsgHead3, sTmp) + "  ";

                sTmp = "";
                text += Function.TextAddColonSpace(MsgHead4, sTmp) + "  ";
                sTmp = Function.DecodeCommon1Byte(arr[5]);
                sTmp= Convert.ToString(int.Parse(sTmp), 2).PadLeft(8, '0');
                text += GetStateString(1, sTmp.Substring(7, 1)) + "  ";
                text += GetStateString(2, sTmp.Substring(6, 1)) + "  ";
                text += GetStateString(3, sTmp.Substring(5, 1)) + "  ";
                text += GetStateString(4, sTmp.Substring(4, 1)) + "  ";
                text += GetStateString(5, sTmp.Substring(3, 1)) + "  ";
                text += GetStateString(6, sTmp.Substring(2, 1)) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[6])+"s";
                text += Function.TextAddColonSpace(MsgHead5, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[7]) + "min";
                text += Function.TextAddColonSpace(MsgHead6, sTmp) + "  ";

                model.MsgText = text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }

        public string GetStateString(int staNum, string state)
        {
            string sta = "";
            string[] strTitle = "充电器状态,充电器故障,充电连接锁,电池不兼容,充电系统故障,充电停止控制".Split(',');
            //standby,normal,open,compatible,normal,operating
            string[] str0 = "待机,正常,打开,兼容,正常,运行".Split(',');
            //charging,fault,locked,incompatible,malfunction,stopped or stop charging
            string[] str1 = "充电,故障,锁定,不兼容,故障,停止或停止充电".Split(',');

            if (state == "0")
            {
                sta = strTitle[staNum - 1] + ":" + str0[staNum - 1];
            }
            else
            {
                sta = strTitle[staNum - 1] + ":" + str1[staNum - 1];
            }

            return sta;
        }

    }
}
