using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_CCS : MsgCommon
    {
        private string MsgHeadLine = "充电机充电状态";

        private string TestOutputVolt;
        private string TestOutputCurrent;
        private string TestChargeTimeSum = "累计充电时间";
        private string TestChargePermit = "充电允许";

        private string Permit = "允许";
        private string Pause = "暂停";

        private string TestPause = "00";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
        {
            switch (eSGBDC_Ver)
            {
                case ESGBDC_Ver.GBDC_2015:
                    TestOutputVolt = "电压输出值";
                    TestOutputCurrent = "电流输出值";
                    break;
                case ESGBDC_Ver.GBDC_2023:
                    TestOutputVolt = "车辆接口当前电压测量值/V";
                    TestOutputCurrent = "车辆接口当前电流测量值/A";
                    break;
            }
            CanMsgRich model = new CanMsgRich();

            string text = string.Empty;
            string[] arr = Function.SplitMsgData(content);
            int i = 0;
            try
            {
                string outputV = DecodeOutputVolt(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestOutputVolt, outputV);

                string outputI= DecodeOutputCurrent(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestOutputCurrent, outputI);

                string chargeT = DecodeChargeTime(arr[i++], arr[i++]);
                text += Function.TextAddColonSpace(TestChargeTimeSum, chargeT);


                string val= arr[i++].ToUpper();
                string permitText;
                string chargePermit = DecodeChargePermit(val,out permitText);
                model.ConsistMsg.SPN3929 = permitText;

                text += Function.TextAddColonSpace(TestChargePermit, chargePermit);

                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
            }
            return model;
        }
        private string DecodeOutputVolt(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(volt, 10, 1, 0);
            return result.ToString("f1") + "V";
        }
        private string DecodeOutputCurrent(string low, string high)
        {
            int current = BaseConvert.HexStr2Int32(high + low);
            double result = Function.ShrinkKeepOffset(current, 10, 1, ((int)MsgConfig.mCurrentRefrence));
            return result.ToString("f1") + "A";
        }
        private string DecodeChargeTime(string low, string high)
        {
            int result = BaseConvert.HexStr2Int32(high + low);
            return result.ToString() + "(min)";
        }
        private string DecodeChargePermit(string str,out string permitText)
        {
            int result = BaseConvert.HexStr2Int32(str);
            string bits = BaseConvert.GetBitsFromHex(result, 0, 2);
            permitText = bits;
            if (bits == TestPause)
            {
                return Pause;
            }
            else
            {
                return Permit;
            }
    
        }
    }
}
