using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_MutiPackageText : MsgCommon
    {
        private string MsgHeadLine = "多包报文";
        //private string TestLastPckg = "该报文为最后一包";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver BMSGB_Ver)
        {
            CanMsgRich model = new CanMsgRich();
            string text;
            int i = 0;
            string[] arr = Function.SplitMsgData(content);
            try
            {
                //model.IsLastPackage = false;

                string sCurPckgCnt = arr[i++];
                int curPckgCnt = Convert.ToInt32(sCurPckgCnt, 16);
    
                if (curPckgCnt <CommonCAN.MutiPackage.GetCountPlan())
                {
                    text = string.Format("该报文为第{0}包", curPckgCnt) + Punctuation.Space;
                }
                else
                {
                    //model.IsLastPackage = true;
                    return model;
                }
                model.MsgText = Function.AppendTextToMsgHead(symbol, MsgHeadLine)+ text;
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
