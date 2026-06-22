using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
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
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                for (i = 0; i < arr.Length - 1; i += 2)
                {
                    string low = arr[i];
                    string high = arr[i+1];
                    string testBatV= "#" + (i+1).ToString() + TestBatV;
                    string batV = DecodeSingleBatV(low, high);
                    text += Function.TextAddColonSpace(testBatV, batV);

                    string grp = DecodeBatGroupNum(high);
                    text+= Function.TextAddColonSpace(TestBatGrp, grp);
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
