using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_CRO : MsgCommon
    {
        private string MsgHeadLine = "充电机输出准备就绪状态";

        private string TestNotReady = "充电机未完成充电准备";
        private string TestBeReady = "充电机完成充电准备";
        private string TestInvalid = "无效";

        private string JudgeBeReady = "AA";
        private string JudgeInvalid = "FF";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            string[] arr = Function.SplitMsgData(content);
            try
            {
                string val = arr[0].ToUpper();
                model.ConsistMsg.SPN2830 = val;
                if (val == JudgeBeReady)
                    text = TestBeReady;
                else if (val == JudgeInvalid)
                    text = TestInvalid;
                else
                    text = TestNotReady;

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
