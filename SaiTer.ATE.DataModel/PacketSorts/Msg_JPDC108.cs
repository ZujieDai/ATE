using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
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
            string[] arr = Function.SplitMsgData(content);

            try
            {
                string sTmp = "Ver." + Function.DecodeCommon1Byte(arr[0]);
                text = Function.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = Function.DecodeCommon2Byte(arr[2], arr[1]) + "V"; 
                text += Function.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[3]) + "A";
                text += Function.TextAddColonSpace(MsgHead3, sTmp) + "  ";

                sTmp = Function.DecodeCommon2Byte(arr[5], arr[4]) + "V";
                text += Function.TextAddColonSpace(MsgHead4, sTmp) + "  ";

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
        
    }
}
