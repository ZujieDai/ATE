using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaiTer.ATE.DataModel
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
                string[] arr = PacketHelper.SplitMsgData(content);
                int i = 0;
                string low = arr[i++];
                string high = arr[i++];
                int volt = BaseConvert.HexStr2Int32(high + low);
                double result = PacketHelper.ShrinkKeepOffset(volt, 10, 1, 0);
                text = result.ToString("f1") + "V";
                text = PacketHelper.TextAddColonSpace(TestConn, text);

                model.MsgText = PacketHelper.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }

    }
}
