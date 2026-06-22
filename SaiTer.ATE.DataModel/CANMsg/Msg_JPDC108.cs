using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_JPDC108 : MsgCommon
    {
        private string MsgHead1 = "电动汽车接触器焊接检测支持的标识符";
        private string MsgHead2 = "可用输出电压";
        private string MsgHead3 = "可用输出电流";
        private string MsgHead4 = "阈值电压";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = PacketHelper.SplitMsgData(content);

            try
            {
                string sTmp = "Ver." + PacketHelper.DecodeCommon1Byte(arr[0]);
                text = PacketHelper.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon2Byte(arr[2], arr[1]) + "V"; 
                text += PacketHelper.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon1Byte(arr[3]) + "A";
                text += PacketHelper.TextAddColonSpace(MsgHead3, sTmp) + "  ";

                sTmp = PacketHelper.DecodeCommon2Byte(arr[5], arr[4]) + "V";
                text += PacketHelper.TextAddColonSpace(MsgHead4, sTmp) + "  ";

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
