using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_Undefined : MsgCommon
    {
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver BMSGB_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            try
            {
                model.MsgText = "blank:未定义报文";
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
