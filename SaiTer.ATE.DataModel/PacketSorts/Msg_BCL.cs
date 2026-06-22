using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BCL : MsgCommon
    {
        private string MsgHeadLine = "电池充电需求";

        private string TestVoltReq= "电压需求";
        private string TestCurrentReq = "电流需求";

        private string TestMode = "01";

        private string ChargeModeV = "恒压充电";
        private string ChargeModeI = "恒流充电";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string reqV = DecodeVoltReq(arr[i++], arr[i++]);
                text = Function.TextAddColonSpace(TestVoltReq, reqV);

                string reqI = DecodeCurrentReq(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestCurrentReq, reqI);

                string chargeMode = DecodeChargeMode(arr[i++]);
                text += chargeMode;

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
        private string DecodeVoltReq(string low,string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeCurrentReq(string low, string high)
        {
            int cur = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(cur, 10) - 400;
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "A";
        }
        private string DecodeChargeMode(string str)
        {
            if (str == TestMode)
                return ChargeModeV;
            else
                return ChargeModeI;
        }
    }
}
