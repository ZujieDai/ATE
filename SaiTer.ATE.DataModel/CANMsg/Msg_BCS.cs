using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
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
                string[] arr = PacketHelper.SplitMsgData(content);
                int i = 0;

                string chargeVolt = DecodeChargeVolt(arr[i++], arr[i++]);
                text = PacketHelper.TextAddColonSpace(TestChargeVolt, chargeVolt);

                string chargeI = DecodeChargeI(arr[i++], arr[i++]);
                text += PacketHelper.TextAddColonSpace(TestChargeCurrent, chargeI);

                string low = arr[i++];
                string high = arr[i++];
                string maxV = DecodeMaxBatVolt(low, high);
                text += PacketHelper.TextAddColonSpace(TestMaxBatVolt, maxV);

                string batNum = DecodeBatNum(high);
                text += PacketHelper.TextAddColonSpace(TestBatNum, batNum);

                string soc = DecodeSoc(arr[i++]);
                text += PacketHelper.TextAddColonSpace(TestSoc, soc);

                string remainTime = DecodeRemainTime(arr[i++], arr[i++]);
                text += PacketHelper.TextAddColonSpace(TestRemainChargeTime, remainTime);

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
        private string DecodeChargeVolt(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double result = PacketHelper.ShrinkKeepOffset(volt, 10, 1, 0);
            return result.ToString("f1") + "V";
        }
        private string DecodeChargeI(string low, string high)
        {
            int current = BaseConvert.HexStr2Int32(high + low);
            double result = PacketHelper.ShrinkKeepOffset(current, 10, 1, 400);
            return result.ToString("f1") + "A";
        }
        private string DecodeMaxBatVolt(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            int volt = val & 0x0fff;
            double result = PacketHelper.ShrinkKeepOffset(volt, 100, 2, 0);
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
