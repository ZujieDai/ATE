using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BMV : MsgCommon
    {
        private string MsgHeadLine = "单体动力蓄电池电压";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestBatV = "单体动力蓄电池电压";
        private string TestBatGrp = "电池分组号";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = PacketHelper.SplitMsgData(content);
            int i = 0;
            try
            {
                for (i = 0; i < arr.Length - 1; i += 2)
                {
                    string low = arr[i];
                    string high = arr[i+1];
                    string testBatV= "#" + (i+1).ToString() + TestBatV;
                    string batV = DecodeSingleBatV(low, high);
                    text += PacketHelper.TextAddColonSpace(testBatV, batV);

                    string grp = DecodeBatGroupNum(high);
                    text+= PacketHelper.TextAddColonSpace(TestBatGrp, grp);
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
