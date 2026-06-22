using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BSD : MsgCommon
    {
        private string MsgHeadLine = "BMS统计数据";

        private string TestSoc= "中止荷电状态SOC";
        private string TestMinVolt= "动力蓄电池单体最低电压";
        private string TestMaxVolt = "动力蓄电池单体最高电压";
        private string TestMinTemp = "动力蓄电池最低温度";
        private string TestMaxTemp = "动力蓄电池最高温度";


        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string soc = DecodeSoc(arr[i++]);
                text += Function.TextAddColonSpace(TestSoc, soc);

                string minVolt = DecodeVolt(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMinVolt, minVolt);

                string maxVolt = DecodeVolt(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestMaxVolt, maxVolt);

                string minTemp = DeocdeTemp(arr[i++]);
                text += Function.TextAddColonSpace(TestMinTemp, minTemp);

                string maxTemp = DeocdeTemp(arr[i++]);
                text += Function.TextAddColonSpace(TestMaxTemp, maxTemp);

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
        private string DecodeSoc(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            return val.ToString() + "%";
        }
        private string DecodeVolt(string low, string high)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(val, 100, 2, 0);
            return result.ToString("f2") + "V";
        }
        private string DeocdeTemp(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) - 50;
            return val.ToString() + "摄氏度";
        }
    }
}
