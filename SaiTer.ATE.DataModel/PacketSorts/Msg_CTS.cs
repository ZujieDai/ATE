using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CTS : MsgCommon
    {
        private string MsgHeadLine = "充电机发送时间同步信息";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);
            try
            {
                text = DecodeSyncDatetime(arr);

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
        private string DecodeSyncDatetime(string[] arr)
        {
            return string.Format("{0}年{1}月{2}日{3}时{4}分{5}秒", arr[6] + arr[5], arr[4], arr[3], arr[2], arr[1], arr[0]);
            //int year = BaseConvert.HexStr2Int32(arr[6] + arr[5]);
            //int month = BaseConvert.HexStr2Int32(arr[4]);
            //int day = BaseConvert.HexStr2Int32(arr[3]);
            //int hour = BaseConvert.HexStr2Int32(arr[2]);
            //int minute = BaseConvert.HexStr2Int32(arr[1]);
            //int second = BaseConvert.HexStr2Int32(arr[0]);

            //return string.Format("{0}年{1}月{2}日{3}时{4}分{5}秒",
            //                    year.ToString(),
            //                    month.ToString(),
            //                    day.ToString(),
            //                    hour.ToString(),
            //                    minute.ToString(),
            //                    second.ToString());
        }
    }
}
