using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
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
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                for (i = 0; i < arr.Length; i++)
                {
                    text += Function.TextAddColonSpace(TestBatReserve, (i + 1).ToString());
                }

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + LastPckgText + KeyConst.Punctuation.Space + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }
        private string DecodeSingleBatV(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(volt, 100, 2, 0);
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
