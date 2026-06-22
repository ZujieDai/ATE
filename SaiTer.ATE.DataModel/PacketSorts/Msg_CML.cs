using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CML : MsgCommon
    {
        private string MsgHeadLine = "充电机最大输出能力";

        private string TestMaxOutputV = "最高输出电压";
        private string TestMinOutputV = "最低输出电压";
        private string TestMaxOutputI = "最大输出电流";
        private string TestMinOutputI = "最小输出电流";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                //1.最高输出电压
                //string low = arr[i++];
                //string high = arr[i++];
                string maxV = DecodeMaxV(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMaxOutputV, maxV);

                //2.最低输出电压
                //low = arr[i++];
                //high = arr[i++];
                string minV = DecodeMinV(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMinOutputV, minV);

                //3.最大输出电流
                //low = arr[i++];
                //high = arr[i++];
                string maxI = DecodeMaxI(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMaxOutputI, maxI);

                //4.最小输出电流
                string minI = DecodeMinI(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMinOutputI, minI);

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
        private string DecodeMaxV(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeMinV(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeMaxI(string low, string high)
        {
            int eleCurrent = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(eleCurrent, 10) - 400;
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "A";
        }
        private string DecodeMinI(string low, string high)
        {
            int eleCurrent = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(eleCurrent, 10) - 400;
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "A";
        }
    }
}
