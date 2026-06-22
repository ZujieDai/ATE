using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_BHM : MsgCommon
    {
        private string MsgHeadLine = "车辆握手"; 

        private string TestConn = "最高允许充电电压";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver BMSGB_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text;

            try
            {
                string[] arr = Function.SplitMsgData(content);
                int i = 0;
                string low = arr[i++];
                string high = arr[i++];
                int volt = BaseConvert.HexStr2Int32(high + low);
                double result = Function.ShrinkKeepOffset(volt, 10, 1, 0);
                text = result.ToString("f1") + "V";
                switch (BMSGB_Ver)
                {
                    case ESGBDC_Ver.GBDC_2015:
                        TestConn = "最高允许充电电压";
                        break;
                    case ESGBDC_Ver.GBDC_2023:
                        TestConn = "车辆端绝缘监测允许总电压";
                        break;
                }
                text = Function.TextAddColonSpace(TestConn, text);

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
