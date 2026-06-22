using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
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
        private string TestSelf = "自检故障（包括绝缘检测、短路测试、粘连检测等自检过程中的故障）";
        private string TestPreCharge = "预充故障（包括预充电压不匹配，预充失败等故障）";

        private string TestCurrent = "电流匹配";
        private string TestVolt = "电压";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
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
                string state = Function.MatchState(result, SPN.StateName.SPN3521_12);
                text += Function.TextAddColonSpace(TestAchievedSetPause, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3521_34);
                text += Function.TextAddColonSpace(TestManPause, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3521_56);
                text += Function.TextAddColonSpace(TestTroublePause, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3521_78);
                text += Function.TextAddColonSpace(TestBMSPause, state);


                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_12);
                text += Function.TextAddColonSpace(TestEqTotalTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_34);
                text += Function.TextAddColonSpace(TestConn, state);

                result = BaseConvert.GetBitsFromHex(val, 4, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_56);
                text += Function.TextAddColonSpace(TestInnerTemp, state);

                result = BaseConvert.GetBitsFromHex(val, 6, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_78);
                text += Function.TextAddColonSpace(TestTransfer, state);

                str = arr[i++];//两个字节的数据需要调换位置
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_9);
                text += Function.TextAddColonSpace(TestEmergencyStop, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3522_11);
                text += Function.TextAddColonSpace(TestOther, state);


                str = arr[i++];
                val = BaseConvert.HexStr2Int32(str);

                result = BaseConvert.GetBitsFromHex(val, 0, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3523_12);
                text += Function.TextAddColonSpace(TestCurrent, state);

                result = BaseConvert.GetBitsFromHex(val, 2, 2);
                state = Function.MatchState(result, SPN.StateName.SPN3523_34);
                text += Function.TextAddColonSpace(TestVolt, state);
                switch (eSGBDC_Ver)
                {
                    case ESGBDC_Ver.GBDC_2015:
                        break;
                    case ESGBDC_Ver.GBDC_2023:
                        result = BaseConvert.GetBitsFromHex(val, 4, 2);
                        state = Function.MatchState(result, SPN.StateName.SPN3522_13);
                        text += Function.TextAddColonSpace(TestSelf, state);

                        result = BaseConvert.GetBitsFromHex(val, 6, 2);
                        state = Function.MatchState(result, SPN.StateName.SPN3522_15);
                        text += Function.TextAddColonSpace(TestPreCharge, state);
                        break;
                }
                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
            }
            return model;
        }
    }
}
