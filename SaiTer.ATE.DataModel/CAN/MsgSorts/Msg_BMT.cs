using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_BMT : MsgCommon
    {
        private string MsgHeadLine = "动力蓄电池温度";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestBatTemp = "动力蓄电池温度";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
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

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + LastPckgText + Punctuation.Space + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
           
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
