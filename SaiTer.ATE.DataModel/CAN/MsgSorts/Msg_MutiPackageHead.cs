using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_MutiPackageHead:MsgCommon
    {
        private string TestBRM = "BMS和车辆辨识报文";
        private string TestBCP = "动力蓄电池充电参数";
        private string TestBCS = "电池充电总状态";
        private string TestBMV = "单体动力蓄电池电压";
        private string TestBSP = "动力蓄电池预留报文";
        private string TestBMT = "动力蓄电池温度报文";
        private string TestUndefined = "Undefined CMD";

        private string TestEnd = "多包报文 首报文";
        private string TestCnt = "总共包数";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver BMSGB_Ver)
        {
            CanMsgRich model = new CanMsgRich();
            string text = string.Empty;
            try
            {
                switch (symbol)
                {
                    case CanMsgId.BCP:
                        text = TestBCP;
                        break;
                    case CanMsgId.BRM:
                        text = TestBRM;
                        break;
                    case CanMsgId.BCS:
                        text = TestBCS;
                        break;
                    case CanMsgId.BMV:
                        text = TestBMV;
                        break;
                    case CanMsgId.BSP:
                        text = TestBSP;
                        break;
                    case CanMsgId.BMT:
                        text = TestBMT;
                        break;
                    default:
                        text = TestUndefined;
                        break;
                }
                int cnt = CommonCAN.MutiPackage.GetCountPlan();
                model.MsgText = Function.AppendTextToMsgHead(symbol, text) + TestEnd + Punctuation.Space
                    + TestCnt + Punctuation.Colon + cnt.ToString();
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";

                return model;
            }
    
        }
    }
}
