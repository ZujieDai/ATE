using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
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
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string str = arr[i++];
                int val = BaseConvert.HexStr2Int32(str);

                string result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3901 = result;
                string state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3901);
                text += Function.TextAddColonSpace(TestRecognize00, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3902);
                model.ConsistMsg.SPN3902 = result;
                text += Function.TextAddColonSpace(TestRecognizeAA, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3903 = result;
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3903);
                text += Function.TextAddColonSpace(TestSync, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3904 = result;
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3904);
                text += Function.TextAddColonSpace(TestReady, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3905 = result;
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3905);
                text += Function.TextAddColonSpace(TestState, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3906 = result;
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3906);
                text += Function.TextAddColonSpace(TestPauseCharge, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3907 = result;
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3907);
                text += Function.TextAddColonSpace(TestEqSummary, state);

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