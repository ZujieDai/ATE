using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_JPDC102 : MsgCommon
    {
        private string MsgHead1 = "CHAdeMO控制协议号";
        private string MsgHead2 = "目标电池电压";
        private string MsgHead3 = "充电电流需求";
        private string MsgHead4 = "故障标志";
        private string MsgHead5 = "状态标志";
        private string MsgHead6 = "收费率";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = PacketHelper.SplitMsgData(content);

            try
            {
                string sTmp ="Ver."+ PacketHelper.DecodeCommon1Byte(arr[0]);
                text = PacketHelper.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon2Byte(arr[2], arr[1]) + "V";
                text += PacketHelper.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon1Byte(arr[3]) + "A";
                text += PacketHelper.TextAddColonSpace(MsgHead3, sTmp) + "  ";

                sTmp = "";
                text += PacketHelper.TextAddColonSpace(MsgHead4, sTmp) + "  ";
                sTmp = PacketHelper.DecodeCommon1Byte(arr[4]);
                sTmp = Convert.ToString(int.Parse(sTmp), 2).PadLeft(8, '0');
                text += GetFaultString(1, sTmp.Substring(7, 1)) + "  ";
                text += GetFaultString(2, sTmp.Substring(6, 1)) + "  ";
                text += GetFaultString(3, sTmp.Substring(5, 1)) + "  ";
                text += GetFaultString(4, sTmp.Substring(4, 1)) + "  ";
                text += GetFaultString(5, sTmp.Substring(3, 1)) + "  ";

                sTmp = "";
                text += PacketHelper.TextAddColonSpace(MsgHead5, sTmp) + "  ";
                sTmp = PacketHelper.DecodeCommon1Byte(arr[5]);
                sTmp = Convert.ToString(int.Parse(sTmp), 2).PadLeft(8, '0');
                text += GetStateString(1, sTmp.Substring(7, 1)) + "  ";
                text += GetStateString(2, sTmp.Substring(6, 1)) + "  ";
                text += GetStateString(3, sTmp.Substring(5, 1)) + "  ";
                text += GetStateString(4, sTmp.Substring(4, 1)) + "  ";
                text += GetStateString(5, sTmp.Substring(3, 1)) + "  ";



                sTmp = PacketHelper.DecodeCommon1Byte(arr[6]) + "%";
                text += PacketHelper.TextAddColonSpace(MsgHead6, sTmp) + "  ";

                model.MsgText = text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }

        public string GetFaultString(int staNum, string state)
        {
            string sta = "";
            string[] strTitle = "电池过压,电池欠压,电池电流偏差误差,电池高温,电池电压偏差误差".Split(',');
            //standby,normal,open,compatible,normal,operating
            string str0 = "正常";
            //charging,fault,locked,incompatible,malPacketHelper,stopped or stop charging
            string str1 = "故障";

            if (state == "0")
            {
                sta = strTitle[staNum - 1] + ":" + str0;
            }
            else
            {
                sta = strTitle[staNum - 1] + ":" + str1;
            }

            return sta;
        }

        public string GetStateString(int staNum, string state)
        {
            string sta = "";
            string[] strTitle = "车辆充电启用状态,车辆换档位置,充电系统故障,车辆状态,充电前正常停止请求".Split(',');
            //standby,normal,open,compatible,normal,operating
            string[] str0 = "禁用,停车位置,正常,电动汽车接触器闭合或焊接检测期间,没有请求".Split(',');
            //charging,fault,locked,incompatible,malPacketHelper,stopped or stop charging
            string[] str1 = "启用,其他位置,故障,电动汽车接触器断开或焊接检测终止,停止请求".Split(',');

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
