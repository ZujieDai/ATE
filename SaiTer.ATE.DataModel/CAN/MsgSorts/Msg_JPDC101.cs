using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_JPDC101 : MsgCommon
    {
        private string MsgHead1 = "最大充电时间(s)";
        private string MsgHead2 = "最大充电时间(min)";
        private string MsgHead3 = "预计充电时间(min)";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);

            try
            {
                string sTmp = Function.DecodeCommon1Byte(arr[1]);//返回的数据单位是10s
                sTmp = (int.Parse(sTmp) * 10).ToString() + "s";
                text = Function.TextAddColonSpace(MsgHead1, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[2]) + "min";
                text += Function.TextAddColonSpace(MsgHead2, sTmp) + "  ";

                sTmp = Function.DecodeCommon1Byte(arr[3]) + "min";
                text += Function.TextAddColonSpace(MsgHead3, sTmp) + "  ";


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
