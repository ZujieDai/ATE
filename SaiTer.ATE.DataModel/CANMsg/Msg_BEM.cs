using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BEM : MsgCommon
    {
        private string MsgHeadLine = "BMS错误报文";

        private string TestRecognize00 = "接收SPN2560=00的充电机辨识报文";
        private string TestRecognizeAA = "接收SPN2560=AA的充电机辨识报文";
        private string TestSync = "接收充电机的时间同步和充电机最大输出能力报文";
        private string TestReady = "接收充电机准备报文";
        private string TestState= "接收充电机状态报文";
        private string TestPauseCharge = "接收充电机中止充电报文";
        private string TestEqSummary = "接收充电机充电统计报文";


        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = PacketHelper.SplitMsgData(content);
            int i = 0;
            try
            {
                string str = arr[i++];
                int val = BaseConvert.HexStr2Int32(str);

                string result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3901 = result;
                string state = PacketHelper.MatchState(result, SPN.StateName.SPN3901);
                text += PacketHelper.TextAddColonSpace(TestRecognize00, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3902);
                model.ConsistMsg.SPN3902 = result;
                text += PacketHelper.TextAddColonSpace(TestRecognizeAA, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3903 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3903);
                text += PacketHelper.TextAddColonSpace(TestSync, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3904 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3904);
                text += PacketHelper.TextAddColonSpace(TestReady, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3905 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3905);
                text += PacketHelper.TextAddColonSpace(TestState, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3906 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3906);
                text += PacketHelper.TextAddColonSpace(TestPauseCharge, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3907 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3907);
                text += PacketHelper.TextAddColonSpace(TestEqSummary, state);

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