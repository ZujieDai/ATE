using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_CEM : MsgCommon
    {
        private string MsgHeadLine = "充电机错误报文";

        private string TestRecognize= "接收BMS和车辆的辨识报文";
        private string TestChargePara = "接收电池充电参数报文";
        private string TestBmsReady = "接收BMS完成充电准备报文";
        private string TestChargeState = "接收电池充电总状态报文";
        private string TestChargeReq = "接收电池充电要求报文";
        private string TestBmsPauseCharge = "接收BMS中止充电报文";
        private string TestBmsSummary = "接收BMS充电统计报文";


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
                model.ConsistMsg.SPN3921 = result;
                string state = PacketHelper.MatchState(result, SPN.StateName.SPN3921);
                text += PacketHelper.TextAddColonSpace(TestRecognize, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3922 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3922);
                text += PacketHelper.TextAddColonSpace(TestChargePara, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3923 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3923);
                text += PacketHelper.TextAddColonSpace(TestBmsReady, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3924 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3924);
                text += PacketHelper.TextAddColonSpace(TestChargeState, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                model.ConsistMsg.SPN3925 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3925);
                text += PacketHelper.TextAddColonSpace(TestChargeReq, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                model.ConsistMsg.SPN3926 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3926);
                text += PacketHelper.TextAddColonSpace(TestBmsPauseCharge, state);

                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                model.ConsistMsg.SPN3927 = result;
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3927);
                text += PacketHelper.TextAddColonSpace(TestBmsSummary, state);

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