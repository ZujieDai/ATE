using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BHM : MsgCommon
    {
        private string MsgHeadLine = "车辆握手"; 

        private string TestConn = "最高允许充电电压";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;

            try
            {
                string[] arr = Function.SplitMsgData(content);
                int i = 0;
                string low = arr[i++];
                string high = arr[i++];
                int volt = BaseConvert.HexStr2Int32(high + low);
                double result = Function.ShrinkKeepOffset(volt, 10, 1, 0);
                text = result.ToString("f1") + "V";
                text = Function.TextAddColonSpace(TestConn, text);

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

    }
}
