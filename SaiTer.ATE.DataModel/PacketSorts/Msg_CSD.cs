using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CSD : MsgCommon
    {
        private string MsgHeadLine = "充电机统计数据";

        private string TestChargeTime = "累计充电时间";
        private string TestEnergy = "输出能量";
        private string TestEqNum = "充电机编号";



        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string chargeT = DecodeChargeTime(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestChargeTime, chargeT);

                string energy = DecodeEnergy(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestEnergy, energy);

                string eqNum = DecodeEqNum(arr[i++], arr[i++], arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestEqNum, eqNum);

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
        private string DecodeChargeTime(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high+low);
            return val.ToString() + "(min)";
        }
        private string DecodeEnergy(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(val, 10, 1, 0);
            return result.ToString("f1") + "kW·h";
        }
        private string DecodeEqNum(string s1, string s2, string s3, string s4)
        {
            uint val = Convert.ToUInt32(s4 + s3 + s2 + s1, 16) + 1;
            return val.ToString();
        }
    }
}
