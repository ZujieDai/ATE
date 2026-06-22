using System;
using System.Collections.Generic;

namespace SaiTer.ATE.DataModel
{
    public class Msg_CST : MsgCommon
    {
        private string MsgHeadLine = "充电机中止充电";

        private string TestAchievedSetPause = "达到充电机设定条件中止";
        private string TestManPause = "人工中止";
        private string TestTroublePause = "故障中止";
        private string TestBMSPause = "BMS主动中止";

        private string TestEqTotalTemp = "充电机温度";
        private string TestConn = "充电机连接器";
        private string TestInnerTemp = "充电机内部温度";
        private string TestTransfer = "电能传送";
        private string TestEmergencyStop = "充电急停";
        private string TestOther = "其他";

        private string TestCurrent = "电流匹配";
        private string TestVolt = "电压";
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
                string state = PacketHelper.MatchState(result, SPN.StateName.SPN3521_12);
                text += PacketHelper.TextAddColonSpace(TestAchievedSetPause, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3521_34);
                text += PacketHelper.TextAddColonSpace(TestManPause, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3521_56);
                text += PacketHelper.TextAddColonSpace(TestTroublePause, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3521_78);
                text += PacketHelper.TextAddColonSpace(TestBMSPause, state);

                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_9);
                text += PacketHelper.TextAddColonSpace(TestEmergencyStop, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_11);
                text += PacketHelper.TextAddColonSpace(TestOther, state);


                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_12);
                text += PacketHelper.TextAddColonSpace(TestEqTotalTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_34);
                text += PacketHelper.TextAddColonSpace(TestConn, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_56);
                text += PacketHelper.TextAddColonSpace(TestInnerTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3522_78);
                text += PacketHelper.TextAddColonSpace(TestTransfer, state);


                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3523_12);
                text += PacketHelper.TextAddColonSpace(TestCurrent, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = PacketHelper.MatchState(result, SPN.StateName.SPN3523_34);
                text += PacketHelper.TextAddColonSpace(TestVolt, state);

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
