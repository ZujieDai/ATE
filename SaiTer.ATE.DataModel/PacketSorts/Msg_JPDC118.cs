using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_JPDC118 : MsgCommon
    {
        private string MsgHead1 = "扩展功能1";
        private string MsgHead2 = "可用输出电流（扩展）";
        private string MsgHead3 = "当前充电电流（扩展）";
        private string MsgHead4 = "大电流控制条件标志";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);

            try
            {
                string sTmp = "";
                text = Function.TextAddColonSpace(MsgHead1, sTmp) + "  ";
                sTmp = "";
                sTmp = Function.DecodeCommon1Byte(arr[0]);
                sTmp = Convert.ToString(int.Parse(sTmp), 2).PadLeft(8, '0');
                text += "动态控制：" + GetStateString1(sTmp.Substring(7, 1)) + "  ";
                text += "大电流控制：" + GetStateString1(sTmp.Substring(6, 1)) + "  ";

                sTmp = Function.DecodeCommon2Byte(arr[2], arr[1]) + "A";
                text += Function.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = Function.DecodeCommon2Byte(arr[3], arr[4]) + "A";
                text += Function.TextAddColonSpace(MsgHead3, sTmp) + "  ";

                sTmp = "";
                text = Function.TextAddColonSpace(MsgHead4, sTmp) + "  ";
                sTmp = Function.DecodeCommon1Byte(arr[5]);
                sTmp = Convert.ToString(int.Parse(sTmp), 2).PadLeft(8, '0');
                text += GetStateString(1, sTmp.Substring(7, 1)) + "  ";
                text += GetStateString(2, sTmp.Substring(6, 1)) + "  ";
                text += GetStateString(3, sTmp.Substring(5, 1)) + "  ";
                text += GetStateString(4, sTmp.Substring(4, 1)) + "  ";
                text += GetStateString(5, sTmp.Substring(3, 1)) + "  ";
                text += GetStateString(6, sTmp.Substring(2, 1)) + "  ";
                text += GetStateString(7, sTmp.Substring(1, 1)) + "  ";

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

        public string GetStateString1(string state)
        {
            string sta = "";
            string[] strTitle = "动态控制,大电流控制".Split(',');

            if (state == "0")
            {
                sta = "不兼容";
            }
            else
            {
                sta = "兼容";
            }
            return sta;
        }

        public string GetStateString(int staNum, string state)
        {
            string sta = "";
            string[] strTitle = ("操作条件,冷却功能（用于充电电缆）,限流功能（充电电缆）," +
                "冷却功能（用于充电接头）,限流功能（用于充电连接器）," +
                "过热保护（用于充电连接器）,可靠性设计（温度监测功能）").Split(',');
            //standby,normal,open,compatible,normal,operating
            string[] str0 = "标准,未操作,未安装,未操作,未安装,未安装,未应用".Split(',');
            //charging,fault,locked,incompatible,malfunction,stopped or stop charging
            string[] str1 = "特定,正在运行,已安装,正在运行,已安装,已安装,已应用".Split(',');

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
