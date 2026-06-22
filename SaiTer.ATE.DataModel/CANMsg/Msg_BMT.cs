using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BMT : MsgCommon
    {
        private string MsgHeadLine = "动力蓄电池温度";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestBatTemp = "动力蓄电池温度";

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
                    string result = DecodeBatTemp(arr[i]);
                    string testBatTemp = TestBatTemp + (i+1).ToString();
                    text += PacketHelper.TextAddColonSpace(testBatTemp, result);
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
        private string DecodeBatTemp(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) - 50;
            return val.ToString() + "摄氏度";
        }
    }
}
