using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BCS : MsgCommon
    {
        private string MsgHeadLine = "电池充电总状态";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestChargeVolt = "充电电压测量值";
        private string TestChargeCurrent = "充电电流测量值";
        private string TestMaxBatVolt = "最高单体动力蓄电池电压";
        private string TestBatNum = "最高单体动力蓄电池电压所在组号";
        private string TestSoc = "当前荷电状态SOC";
        private string TestRemainChargeTime = "估算剩余充电时间";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;

            try
            {
                string[] arr = Function.SplitMsgData(content);
                int i = 0;

                string chargeVolt = DecodeChargeVolt(arr[i++], arr[i++]);
                text = Function.TextAddColonSpace(TestChargeVolt, chargeVolt);

                string chargeI = DecodeChargeI(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestChargeCurrent, chargeI);

                string low = arr[i++];
                string high = arr[i++];
                string maxV = DecodeMaxBatVolt(low, high);
                text += Function.TextAddColonSpace(TestMaxBatVolt, maxV);

                string batNum = DecodeBatNum(high);
                text += Function.TextAddColonSpace(TestBatNum, batNum);

                string soc = DecodeSoc(arr[i++]);
                text += Function.TextAddColonSpace(TestSoc, soc);

                string remainTime = DecodeRemainTime(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestRemainChargeTime, remainTime);

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
        private string DecodeChargeVolt(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(volt, 10, 1, 0);
            return result.ToString("f1") + "V";
        }
        private string DecodeChargeI(string low, string high)
        {
            int current = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(current, 10, 1, 400);
            return result.ToString("f1") + "A";
        }
        private string DecodeMaxBatVolt(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            int volt = val & 0x0fff;
            double result = Function.ShrinkKeepOffset(volt, 100, 2, 0);
            return result.ToString("f2") + "V";
        }
        private string DecodeBatNum(string high)
        {
            int val = BaseConvert.HexStr2Int32(high);
            int result = (val & 0xf0) >> 4;
            return result.ToString();

        }
        private string DecodeSoc(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            return val.ToString() + "%";
        }
        private string DecodeRemainTime(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            if (val > 600)
                val = 600;
            return val.ToString() + "(min)";
        }
    }
}
