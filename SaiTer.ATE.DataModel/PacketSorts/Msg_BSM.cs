using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BSM : MsgCommon
    {
        private string MsgHeadLine = "动力蓄电池状态信息";

        private string TestMaxBatNum = "最高单体动力蓄电池电压所在编号";
        private string TestMaxBatTemp = "最高动力蓄电池温度";
        private string TestMaxTempNum = "最高温度检测点编号";
        private string TestMinBatTemp = "最低动力蓄电池温度";
        private string TestMinBatNum = "最低动力蓄电池温度检测点编号";
        private string TestBatV = "单体动力蓄电池电压";
        private string TestSoc = "整车动力蓄电池荷电状态SOC";
        private string TestBatChargeI = "动力蓄电池充电过电流";
        private string TestBatTooHigh = "动力蓄电池温度过高";
        private string TestBatInsulate = "动力蓄电池绝缘状态";
        private string TestBatConn = "动力蓄电池组输出连接器连接状态";

        private string TestChargePermit = "充电允许";
        private string vSPN3090 = "";
        private string vSPN3091 = "";
        private string vSPN3092 = "";
        private string vSPN3093 = "";
        private string vSPN3094 = "";
        private string vSPN3095 = "";
        private string vSPN3096 = "";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                //1.最高单体动力蓄电池电压所在编号
                string num = DecodeMaxBatNum(arr[i++]);
                text += Function.TextAddColonSpace(TestMaxBatNum, num);

                //2.最高动力蓄电池温度
                string maxTemp = DecodeMaxBatTemp(arr[i++]);
                text += Function.TextAddColonSpace(TestMaxBatTemp, maxTemp);

                //3.最高温度检测点编号
                string maxNum = DecodeMaxTempNum(arr[i++]);
                text += Function.TextAddColonSpace(TestMaxTempNum, maxNum);

                //4.最低动力蓄电池温度
                string minTemp = DecodeMinBatTemp(arr[i++]);
                text += Function.TextAddColonSpace(TestMinBatTemp, minTemp);

                //5.最低动力蓄电池温度检测点编号
                string minNum = DecodeMinBatNum(arr[i++]);
                text += Function.TextAddColonSpace(TestMinBatNum, minNum);

                //6.单体动力蓄电池电压
                string str1 = arr[i++];

                string batV = DecodeBatV(str1);
                model.ConsistMsg.SPN3090 = vSPN3090;
                text += Function.TextAddColonSpace(TestBatV, batV);

                //7.整车动力蓄电池荷电状态SOC
                string soc = DecodeSoc(str1);
                model.ConsistMsg.SPN3091 = vSPN3091;
                text += Function.TextAddColonSpace(TestSoc, soc);

                //8.动力蓄电池充电过电流
                string chargeI = DecodeBatChargeI(str1);
                model.ConsistMsg.SPN3092 = vSPN3092;
                text += Function.TextAddColonSpace(TestBatChargeI, chargeI);

                //9.动力蓄电池温度过高
                string batTooHigh = DecodeBatTooHigh(str1);
                model.ConsistMsg.SPN3093 = vSPN3093;
                text += Function.TextAddColonSpace(TestBatTooHigh, batTooHigh);

                str1 = arr[i++];
                //10.动力蓄电池绝缘状态
                string batInsulate = DecodeBatInsulate(str1);
                model.ConsistMsg.SPN3094 = vSPN3094;
                text += Function.TextAddColonSpace(TestBatInsulate, batInsulate);

                //11.动力蓄电池组输出连接器连接状态
                string conn = DecodeBatConn(str1);
                model.ConsistMsg.SPN3095 = vSPN3095;
                text += Function.TextAddColonSpace(TestBatConn, conn);

                //12.充电允许
                string permit = DecodeChargePermit(str1);
                model.ConsistMsg.SPN3096 = vSPN3096;
                text += Function.TextAddColonSpace(TestChargePermit, permit);

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
        private string DecodeMaxBatNum(string str)
        {
            int num = BaseConvert.HexStr2Int32(str) + 1;
            return num.ToString();
        }
        private string DecodeMaxBatTemp(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) - 50;
            return val.ToString();
        }
        private string DecodeMaxTempNum(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) + 1;
            return val.ToString();
        }
        private string DecodeMinBatTemp(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) - 50;
            return val.ToString();
        }
        private string DecodeMinBatNum(string str)
        {
            int val = BaseConvert.HexStr2Int32(str) + 1;
            return val.ToString();
        }
        private string DecodeBatV(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 0, 2);
            vSPN3090 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3090);
            return text;
        }
        private string DecodeSoc(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 2, 2);
            vSPN3091 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3091);
            return text;
        }
        private string DecodeBatChargeI(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 4, 2);
            vSPN3092 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3092);
            return text;
        }
        private string DecodeBatTooHigh(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 6, 2);
            vSPN3093 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3093);
            return text;
        }
        private string DecodeBatInsulate(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 0, 2);
            vSPN3094 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3094);
            return text;
        }
        private string DecodeBatConn(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 2, 2);
            vSPN3095 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3095);
            return text;
        }
        private string DecodeChargePermit(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            string result = BaseConvert.GetBitsFromHex(val, 4, 2);
            vSPN3096 = result;
            string text = Function.MatchState(result, KeyConst.SPN.StateName.SPN3096);
            return text;
        }
    }
}
