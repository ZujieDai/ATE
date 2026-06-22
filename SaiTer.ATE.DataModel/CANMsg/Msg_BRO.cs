using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BRO : MsgCommon
    {
        private string MsgHeadLine = "电池充电准备就绪状态";

        private string TestNotReady = "BMS未完成充电准备";
        private string TestBeReady = "BMS完成充电准备";
        private string TestInvalid = "无效";

        private string JudgeBeReady = "AA";
        private string JudgeInvalid = "FF";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            try
            {
                string[] arr = PacketHelper.SplitMsgData(content);
                string val = arr[0].ToUpper();
                if (val == JudgeBeReady)
                    text = TestBeReady;
                else if (val == JudgeInvalid)
                    text = TestInvalid;
                else
                    text = TestNotReady;
                model.ConsistMsg.SPN2829 = val;

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
