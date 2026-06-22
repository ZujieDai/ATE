using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;
namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BST : MsgCommon
    {
        private string MsgHeadLine = "BMS中止充电";

        private string TestSocTarget = "SOC目标值";
        private string TestBatVSet = "总电压的设定值";
        private string TestSingleVSet = "单体电压的设定值";
        private string TestEqPause = "充电机主动中止";

        private string TestInsulateProblem = "绝缘故障";
        private string TestConnOverTemp = "输出连接器温度";
        private string TestBmsOutputConn = "BMS元件、输出连接器温度";
        private string TestChargeConn = "充电连接器";
        private string TestBatTemp = "电池组温度";
        private string TestRelay = "高压继电器";
        private string TestDetectPoint2 = "检测点2电压";
        private string TestOther = "其他";
        private string TestCurrent = "电流";
        private string TestVolt = "电压";


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
                string state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3511_12);
                text += Function.TextAddColonSpace(TestSocTarget, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3511_34);
                text += Function.TextAddColonSpace(TestBatVSet, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3511_56);
                text += Function.TextAddColonSpace(TestSingleVSet, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3511_78);
                text += Function.TextAddColonSpace(TestEqPause, state);

                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_9);
                text += Function.TextAddColonSpace(TestBatTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_11);
                text += Function.TextAddColonSpace(TestRelay, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_13);
                text += Function.TextAddColonSpace(TestDetectPoint2, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_15);
                text += Function.TextAddColonSpace(TestOther, state);

                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_12);
                text += Function.TextAddColonSpace(TestInsulateProblem, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_34);
                text += Function.TextAddColonSpace(TestConnOverTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_56);
                text += Function.TextAddColonSpace(TestBmsOutputConn, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3512_78);
                text += Function.TextAddColonSpace(TestChargeConn, state);


                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3513_12);
                text += Function.TextAddColonSpace(TestCurrent, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, KeyConst.SPN.StateName.SPN3513_34);
                text += Function.TextAddColonSpace(TestVolt, state);

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
