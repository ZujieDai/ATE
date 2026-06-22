using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
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
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                for (i = 0; i < arr.Length; i++)
                {
                    string result = DecodeBatTemp(arr[i]);
                    string testBatTemp = TestBatTemp + (i+1).ToString();
                    text += Function.TextAddColonSpace(testBatTemp, result);
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
        private string DecodeBatTemp(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) - 50;
            return val.ToString() + "摄氏度";
        }
    }
}
