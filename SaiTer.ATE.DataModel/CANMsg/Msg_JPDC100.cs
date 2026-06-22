using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_JPDC100 : MsgCommon
    {
        private string MsgHead1 = "最大电池电压";
        private string MsgHead2 = "充电率参考常数";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = PacketHelper.SplitMsgData(content);

            try
            {
                string sTmp = PacketHelper.DecodeCommon2Byte(arr[5], arr[4]) + "V";
                text = PacketHelper.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon1Byte(arr[6]) + "%";
                text += PacketHelper.TextAddColonSpace(MsgHead2, sTmp) + "  ";


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
