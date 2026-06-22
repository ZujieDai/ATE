using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_JPDC101 : MsgCommon
    {
        private string MsgHead1 = "最大充电时间(s)";
        private string MsgHead2 = "最大充电时间(min)";
        private string MsgHead3 = "预计充电时间(min)";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = PacketHelper.SplitMsgData(content);

            try
            {
                string sTmp = PacketHelper.DecodeCommon1Byte(arr[1]);//返回的数据单位是10s
                sTmp = (int.Parse(sTmp) * 10).ToString() + "s";
                text = PacketHelper.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon1Byte(arr[2]) + "min";
                text += PacketHelper.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon1Byte(arr[3]) + "min";
                text += PacketHelper.TextAddColonSpace(MsgHead3, sTmp) + "  ";


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

    }
}
