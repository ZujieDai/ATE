using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BSP : MsgCommon
    {
        private string MsgHeadLine = "动力蓄电池预留字段";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestBatReserve = "动力蓄电池预留字段";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = PacketHelper.SplitMsgData(content);
            int i = 0;
            try
            {
                for (i = 0; i < arr.Length; i++)
                {
                    text += PacketHelper.TextAddColonSpace(TestBatReserve, (i + 1).ToString());
                }

                model.MsgText = PacketHelper.AppendTextToMsgHead(symbol, this.MsgHeadLine) + LastPckgText + Punctuation.Space + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }
        private string DecodeSingleBatV(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double result = PacketHelper.ShrinkKeepOffset(volt, 100, 2, 0);
            return result.ToString("f2") + "V";
        }
        private string DecodeBatGroupNum(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            int num = (val & 0xf000) >> 12;
            return num.ToString();
        }
    }
}
