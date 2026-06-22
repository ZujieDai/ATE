using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_MutiPackageReady : MsgCommon
    {
        private string TestReady = "充电机准备接收多包报文";
        private string TestEnd= "充电机完成接收多包报文";
        private string TestReject = "充电机拒绝接收多包报文";

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver BMSGB_Ver)
        {
            CanMsgRich model = new CanMsgRich();
            string text;
            try
            {
                CommonCAN.TransitState ack = GetTransitAck(content);
                if (ack == CommonCAN.TransitState.Ready)
                {
                    text = this.TestReady;
                    model.ConsistMsg.IsPackageReady = 1;
                }
                else if (ack == CommonCAN.TransitState.Finish)
                {
                    model.ConsistMsg.IsPackageEnd = 1;//最后一包
                    text = this.TestEnd;
                }
                else
                {
                    model.ConsistMsg.IsPackageReady = 2;//拒绝接收
                    text = this.TestReject;
                }
                model.MsgText = Function.AppendTextToMsgHead(symbol, text);
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";

                return model;
            }
        }

        private CommonCAN.TransitState GetTransitAck(List<byte> content)
        {
            byte readyOrFinish = content[0];

            if (readyOrFinish == 0x11)
                return CommonCAN.TransitState.Ready;
            else if (readyOrFinish == 0x13)
            {
                return CommonCAN.TransitState.Finish;
            }
            else
                return CommonCAN.TransitState.Reject;
        }
    }
}
