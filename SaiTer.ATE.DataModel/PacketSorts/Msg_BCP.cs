using System;
using System.Collections.Generic;
using XPCar.Common;
using XPCar.Prj.Model;

namespace XPCar.Protocol.Decode.Msg.MsgSorts
{
    public class Msg_BCP : MsgCommon
    {
        private string MsgHeadLine = "动力蓄电池充电参数";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestBatMaxV = "单体动力蓄电池最高允许充电电压";
        private string TestBatMaxI = "最高允许充电电流";
        private string TestBatEnergy = "动力蓄电池标称总能量";
        private string TestChargeV = "最高允许充电总电压";
        private string TestMaxTemperature = "最高允许温度";
        private string TestBatSOC = "整车动力蓄电池荷电状态";
        private string TestCurBatV = "整车动力蓄电池当前电池电压";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();
            string text = string.Empty;
            int i = 0;
            string[] arr = Function.SplitMsgData(content);
            try
            {
                //1.单体动力蓄电池最高允许充电电压
                string sLow = arr[i++];
                string sHigh = arr[i++];
                string batMaxV = DecodeBatMaxV(sHigh, sLow);
                text += Function.TextAddColonSpace(TestBatMaxV, batMaxV);

                //2.最高允许充电电流
                sLow = arr[i++];   
                sHigh = arr[i++];
                string batMaxI = DecodeBatMaxI(sHigh, sLow);
                text += Function.TextAddColonSpace(TestBatMaxI, batMaxI);

                //3.动力蓄电池标称总能量
                sLow = arr[i++];
                sHigh = arr[i++];
                string batEnergy = DecodeBatEnergy(sHigh, sLow);
                text += Function.TextAddColonSpace(TestBatEnergy, batEnergy);

                //4.最高允许充电总电压
                sLow = arr[i++];
                sHigh = arr[i++];
                string chargeV = DecodeChargeV(sHigh, sLow);
                text += Function.TextAddColonSpace(TestChargeV, chargeV);

                //5.最高允许温度
                string sTemp = arr[i++];
                string maxTemp = DecodeMaxTemperature(sTemp);
                text += Function.TextAddColonSpace(TestMaxTemperature, maxTemp);

                //6.整车动力蓄电池荷电状态
                sLow = arr[i++];
                sHigh = arr[i++];
                string soc = DecodeBatSoc(sHigh, sLow);
                text += Function.TextAddColonSpace(TestBatSOC, soc);

                //7.整车动力蓄电池当前电池电压
                sLow = arr[i++];
                sHigh = arr[i++];
                string curBatV = DecodeCurBatV(sHigh, sLow);
                text += Function.TextAddColonSpace(TestCurBatV, curBatV);

                model.MsgText = Function.AppendTextToMsgHead(symbol, MsgHeadLine) + LastPckgText + KeyConst.Punctuation.Space + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return model;
            }
        }
        private string DecodeBatMaxV(string sHigh,string sLow)
        {
            int volt = BaseConvert.HexStr2Int32(sHigh + sLow);
            double shrinkV = Function.ShrinkCntTimes(volt,100);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 2);
            return result.ToString("f2") + "V";
        }
        private string DecodeBatMaxI(string sHigh, string sLow)
        {
            int volt = BaseConvert.HexStr2Int32(sHigh + sLow) - 4000;//偏移量
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "A";
        }
        private string DecodeBatEnergy(string sHigh, string sLow)
        {
            int energy = BaseConvert.HexStr2Int32(sHigh + sLow);
            double shrinkV = Function.ShrinkCntTimes(energy, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + @"kW·h";
        }
        private string DecodeChargeV(string sHigh, string sLow)
        {
            int volt = BaseConvert.HexStr2Int32(sHigh + sLow);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeMaxTemperature(string sTemp)
        {
            int result = BaseConvert.HexStr2Int32(sTemp) - 50;
            return result.ToString() + "摄氏度";
        }
        private string DecodeBatSoc(string sHigh, string sLow)
        {
            int state = BaseConvert.HexStr2Int32(sHigh + sLow);
            double shrinkV = Function.ShrinkCntTimes(state, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "%";
        }
        private string DecodeCurBatV(string sHigh, string sLow)
        {
            int volt = BaseConvert.HexStr2Int32(sHigh + sLow);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
    }
}
