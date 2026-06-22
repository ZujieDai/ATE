using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class GETDateWave
    {
        private string _Direction = "读取";
        public List<CanMsgRich> DecodePackage(List<byte> Buffer, ESGBDC_Ver eSGBDC_Ver)
        {
            List<CanMsgRich> canMsgRiches = new List<CanMsgRich>();
            EachFrameModel temp2 = new EachFrameModel();
            temp2.Buffer = Buffer;

            EachFrameModel package = new EachFrameModel();
            package = CommonCAN.HexToASCIIByte(temp2);
            try
            {
                List<byte> buf = Buffer;
                int MsgCount = 0;
                MsgCount = (buf.Count - 12) / 17;
                byte[] MsgContent = new byte[17];//报文内容
                int dlc;
                int MsgTime = 0;//绝对时间
                string msgId;
                string msgData;
                string flowId;

                //Prj.Prj.MainController.icount = Prj.Prj.MainController.icount + MsgCount;
                CanMsgRich canGridRich;
                for (int i = 0; i < MsgCount; i++)
                {
                    //报文内容：报文时间ms（4）+报文ID（4）+数据长度（1）+报文内容（8）
                    MsgContent = BaseConvert.CutLists(buf, i * 17 + 10, 17);
                    MsgTime = DecodeMsgTime(MsgContent);
                    msgId = DecodeMsgId(MsgContent);
                    dlc = DecodeDlc(MsgContent);
                    msgData = DecodeMsgData(MsgContent);
                    flowId = Function.GetMsgFlow(msgId);    //"1CECF456" -> PACKAGE_READY

                    List<byte> contentBytes = new List<byte>();
                    for (int j = 0; j < msgData.Length; j++)
                    {
                        contentBytes.Add(Encoding.Default.GetBytes(msgData.Substring(j, 1).ToUpper())[0]);
                    }

                    MsgManager msgManager = new MsgManager();
                    canGridRich = msgManager.DecodeMsgData(flowId, contentBytes,eSGBDC_Ver);
                    canGridRich.Direction = this._Direction;
                    canGridRich.Dlc = dlc;


                    if (CommonCAN.ValueManager.FirstCreateTime.Year == 2000)
                    {
                        CommonCAN.ValueManager.FirstCreateTime = DateTime.Now;
                        CommonCAN.ValueManager.ALTime_ms = MsgTime;
                    }
                    int TimeSpan = MsgTime - CommonCAN.ValueManager.ALTime_ms;//获取间隔时间
                    CommonCAN.ValueManager.ALTime_ms = MsgTime;//间隔时间是和上一帧报文的间隔时间，所以等间隔时间算出来了再赋值绝对时间
                    if (CommonCAN.ValueManager.PreMsgCreateTime.Year == 2000)
                    {
                        if (CommonCAN.ValueManager.FirstCreateTime.Year != 2000)
                        {
                            CommonCAN.ValueManager.PreMsgCreateTime = CommonCAN.ValueManager.FirstCreateTime;
                        }
                        else
                        {
                            CommonCAN.ValueManager.PreMsgCreateTime = DateTime.Now;
                        }
                        canGridRich.CreateTime = CommonCAN.ValueManager.PreMsgCreateTime;
                    }
                    else
                    {
                        canGridRich.CreateTime = CommonCAN.ValueManager.PreMsgCreateTime.AddMilliseconds(TimeSpan);
                    }

                    canGridRich.Id = msgId;
                    canGridRich.MsgData = Function.AppendSpaceIn2Char(msgData, dlc);
                    canGridRich.CreateTimestamp = canGridRich.CreateTime.ToString(TextFormat.Date);

                    //时间增量
                    if (CommonCAN.ValueManager.IsFirstMsg()) //说明该报文为第一条报文,add for 实时时间
                    {
                        if (canGridRich.ConsistMsg.MsgName != "UNDEFINED")//第一条报文，且不为非标，下位机发送的第一条非标时间不准确，舍弃
                        {
                            canGridRich.TimeIncrement = "00:00:00 0";
                            CommonCAN.ValueManager.FirstCreateTime = canGridRich.CreateTime;
                            canGridRich.SpanTime = 0;
                        }
                    }
                    else
                    {
                        TimeSpan span = canGridRich.CreateTime - CommonCAN.ValueManager.PreMsgCreateTime;
                        canGridRich.TimeIncrement = span.Hours.ToString().PadLeft(2, '0') + Punctuation.Colon
                                            + span.Minutes.ToString().PadLeft(2, '0') + Punctuation.Colon
                                            + span.Seconds.ToString().PadLeft(2, '0') +Punctuation.Space
                                            + span.Milliseconds.ToString("f0");
                        canGridRich.SpanTime = (canGridRich.CreateTime - CommonCAN.ValueManager.FirstCreateTime).TotalMilliseconds;
                    }

                    CommonCAN.ValueManager.PreMsgCreateTime = canGridRich.CreateTime;

                    canGridRich.ConsistMsg.Dlc = dlc;
                    canGridRich.ConsistMsg.CreateTimestamp = canGridRich.CreateTimestamp;
                    canGridRich.ConsistMsg.MsgData = canGridRich.MsgData;

                    canMsgRiches.Add(canGridRich);
                }
            }
            catch (Exception ex)
            {
           
            }
            return canMsgRiches;
        }
        private int DecodeMsgTime(byte[] buf)
        {
            int xh = 0;
            byte[] data = BaseConvert.CutArrs(buf, 0, 4);   //截取时间间隔
            string strHex = BaseConvert.GetBytesToString(data);
            //xh = Convert.ToInt32(strHex, 16);
            xh = (int)Convert.ToUInt32(strHex, 16);
            return xh;
        }
        private int DecodeDlc(byte[] buf)
        {
            byte[] dlcs = BaseConvert.CutArrs(buf, 8, 1);
            int dlc = Convert.ToInt32(dlcs[0].ToString(), 16);
            return dlc;
        }
        private string DecodeMsgId(byte[] buf)
        {
            byte[] idBytes = BaseConvert.CutArrs(buf, 4, 4);   //截取byte
            string msgId = BaseConvert.GetBytesToString(idBytes);
            return msgId;
        }
        private string DecodeMsgData(byte[] buf)
        {
            byte[] msgBytes = BaseConvert.CutArrs(buf, 9, 8);   //截取byte
            string msgData = BaseConvert.GetBytesToString(msgBytes);
            return msgData;
        }
    }
}
