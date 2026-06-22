using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_JPDC100 : MsgCommon
    {
        private string MsgHead1 = "最大电池电压";
        private string MsgHead2 = "充电率参考常数";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);

            try
            {
                string sTmp = Function.DecodeCommon2Byte(arr[5], arr[4]) + "V";
                text = Function.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[6]) + "%";
                text += Function.TextAddColonSpace(MsgHead2, sTmp) + "  ";


                model.MsgText = text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";

            }
            return model;
        }

    }
}
