using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;


namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_BCL : MsgCommon
    {
        private string MsgHeadLine = "电池充电需求";

        private string TestVoltReq = "电压需求";
        private string TestCurrentReq = "电流需求";

        private string TestMode = "01";

        private string ChargeModeV = "恒压充电";
        private string ChargeModeI = "恒流充电";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text;
            //string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string reqV = DecodeVoltReq(content[0].ToString("x2"), content[1].ToString("x2"));
                text = Function.TextAddColonSpace(TestVoltReq, reqV);

                string reqI = DecodeCurrentReq(content[2].ToString("x2"), content[3].ToString("x2"));
                text += Function.TextAddColonSpace(TestCurrentReq, reqI);

                string chargeMode = DecodeChargeMode(content[5].ToString("x2"));
                text += chargeMode;

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";

            }
            return model;
        }
        private string DecodeVoltReq(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeCurrentReq(string low, string high)
        {
            int cur = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = MsgConfig.mCurrentRefrence - Function.ShrinkCntTimes(cur, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "A";
        }
        private string DecodeChargeMode(string str)
        {
            if (str == TestMode)
                return ChargeModeI;
            else
                return ChargeModeV;
        }
    }
}
