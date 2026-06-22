using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XPCar.Common;
using XPCar.Prj.Model;
using static XPCar.Common.KeyConst;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CRM : MsgCommon
    {
        private string MsgHeadLine = "充电机辨识";

        private string TestReco = "辨识结果";
        private string TestEqNum = "充电机编号";
        private string TestAreaNum = "充电机所在区域编码";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            int i = 0;

            try
            {
                string[] arr = Function.SplitMsgData(content);

                string recognizeVal = arr[i++].ToUpper();
  
                string recognizedText = RecogizedText(recognizeVal);

                model.ConsistMsg.SPN2560 = recognizeVal;

                text += TestReco + Punctuation.Colon + recognizedText + Punctuation.Space;

                text += TestEqNum + Punctuation.Colon + EQNum(arr) + Punctuation.Space;

                text += TestAreaNum + Punctuation.Colon + AreaNum(arr) + Punctuation.Space;

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;

                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }

        private bool IsRecognized(string recognize)
        {
            if (recognize.ToUpper() == "AA")
            {
                return true;
            }
            else
                return false;
        }
        private string RecogizedText(string str)
        {
            if (str == "AA")
                return "BMS能辨识";
            else
                return "BMS不能辨识";
        }

        private string EQNum(string[] arr)
        {
            string text = arr[4] + arr[3] + arr[2] + arr[1];
            int eqnum = Convert.ToInt32(text, 16);
            return eqnum.ToString();

        }
        private string AreaNum(string[] arr)
        {
            //int i = 0;
            string area1 = arr[5];
            string area2 = arr[6];
            string area3 = arr[7];
            //string area = area1 + area2 + area3;
            string area = BaseConvert.HexString2AsciiString(area1)
                + BaseConvert.HexString2AsciiString(area2)
                + BaseConvert.HexString2AsciiString(area3);
            area = Function.RandomCodeReplaceBySpace(area);
            return area;
        }
    }
}
