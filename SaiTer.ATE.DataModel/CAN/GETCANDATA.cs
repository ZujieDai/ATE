using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaiTer.ATE.DataModel.CAN
{
    public class GETCANDATA

    {

        private string _Direction = "读取";
        public CanMsgRich DecodePackage(List<byte> Buffer, ESGBDC_Ver eSGBDC_Ver)
        {

            EachFrameModel temp2 = new EachFrameModel();
            temp2.Buffer = Buffer;

            EachFrameModel temp = new EachFrameModel();
            temp = CommonCAN.HexToASCIIByte(temp2);







            CanMsgRich canGridRich = new CanMsgRich();

            try
            {
                List<byte> buf = temp.Buffer;
                int dlc;
                dlc = DecodeDlc(buf);
                //dlc = 8;
                List<byte> contentBytes = new List<byte>();

                string msgId = CutMsgId(buf);
                string msgData = CutMsgData(buf, ref contentBytes);
                string flowId = Function.GetMsgFlow(msgId);    //"1CECF456" -> PACKAGE_READY
                //CommonCAN.MainController._DeviceConnect.STState = true;

                MsgManager msgManager = new MsgManager();

                canGridRich = msgManager.DecodeMsgData(flowId, contentBytes, eSGBDC_Ver);
                canGridRich.Direction = this._Direction;
                canGridRich.Dlc = dlc;
                //canGridRich.CreateTime = DateTime.Now;//add for 实时时间
                //canGridRich.CreateTime = DecodeDatetime(buf);
                if (CommonCAN.ValueManager.FirstCreateTime.Year == 2000)
                {
                    CommonCAN.ValueManager.FirstCreateTime = DateTime.Now;
                }


                int TimeSpan = DecodeTimeSpan(buf);//获取间隔时间
                canGridRich.CreateTime = DateTime.Now;
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
                //canGridRich.CreateTimestamp = "20200402 00:00:00 010";

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
                                        + span.Seconds.ToString().PadLeft(2, '0') + Punctuation.Space
                                        + span.Milliseconds.ToString("f0");
                    canGridRich.SpanTime = (canGridRich.CreateTime - CommonCAN.ValueManager.FirstCreateTime).TotalMilliseconds;

                }
                CommonCAN.ValueManager.PreMsgCreateTime = canGridRich.CreateTime;

                canGridRich.ConsistMsg.Dlc = dlc;
                canGridRich.ConsistMsg.CreateTimestamp = canGridRich.CreateTimestamp;
                canGridRich.ConsistMsg.MsgData = canGridRich.MsgData;




                canGridRich.ConsistMsg.ObjectNo = canGridRich.ObjectNo;


            }
            catch (Exception ex)
            {

            }


            return canGridRich;
        }


        public CanMsgRich DecodePackage2(List<byte> Buffer, ESGBDC_Ver eSGBDC_Ver)
        {
            CanMsgRich canGridRich = new CanMsgRich();

            try
            {
                List<byte> buf = Buffer;

                //移除0x7e前面的字节,确保是正确的格式
                byte[] pattern = new byte[] { 0x7E, 0x00, 0xFF, 0x43, 0x59 };
                int index = -1;
                // 查找指定的报文头模式
                for (int i = 0; i <= Buffer.Count - pattern.Length; i++)
                {
                    if (Buffer.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    {
                        index = i;
                        break;
                    }
                }

                // 如果找到匹配的模式，则移除模式之前的元素
                if (index > 0)
                {
                    Buffer.RemoveRange(0, index);
                }
                else if (index < 0)
                {
                    // 如果找不到匹配的模式，则清空列表
                    Buffer.Clear();
                    return null;
                }



                int dlc;
                dlc = buf[5];
                //dlc = 8;
                List<byte> contentBytes = new List<byte>();

                string msgId = CutMsgId(buf);
                string msgData = CutMsgData(buf, ref contentBytes);
                string flowId = Function.GetMsgFlow(msgId);    //"1CECF456" -> PACKAGE_READY


                MsgManager msgManager = new MsgManager();

                canGridRich = msgManager.DecodeMsgData(flowId, contentBytes, eSGBDC_Ver);
                canGridRich.Direction = this._Direction;
                canGridRich.Dlc = dlc;
                //canGridRich.CreateTime = DateTime.Now;//add for 实时时间
                //canGridRich.CreateTime = DecodeDatetime(buf);
                if (CommonCAN.ValueManager.FirstCreateTime.Year == 2000)
                {
                    CommonCAN.ValueManager.FirstCreateTime = DateTime.Now;
                }

                int TimeSpan = DecodeTimeSpan(buf);//获取间隔时间
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
                //canGridRich.CreateTimestamp = "20200402 00:00:00 010";

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
                                        + span.Seconds.ToString().PadLeft(2, '0') + Punctuation.Space
                                        + span.Milliseconds.ToString("f0");
                    canGridRich.SpanTime = (canGridRich.CreateTime - CommonCAN.ValueManager.FirstCreateTime).TotalMilliseconds;

                }
                CommonCAN.ValueManager.PreMsgCreateTime = canGridRich.CreateTime;

                canGridRich.ConsistMsg.Dlc = dlc;
                canGridRich.ConsistMsg.CreateTimestamp = canGridRich.CreateTimestamp;
                canGridRich.ConsistMsg.MsgData = canGridRich.MsgData;




                canGridRich.ConsistMsg.ObjectNo = canGridRich.ObjectNo;

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return canGridRich;
        }
        private int DecodeDlc(List<byte> buf)
        {
            byte[] dlcs = BaseConvert.CutLists(buf, 5, 1);
            string str = BaseConvert.AsciiBytes2String(dlcs);
            int dlc = BaseConvert.HexStr2Int32(str);
            return dlc;
        }
        private string CutMsgId(List<byte> buf)
        {
            byte[] idBytes = BaseConvert.CutLists(buf, 6, 4);   //截取byte  ???????YF.ZH

            // 将字节数组转换为十六进制字符串
            string msgId = BitConverter.ToString(idBytes).Replace("-", "");


            return msgId;
        }


        private string CutMsgData(List<byte> buf, ref List<byte> contentBytes)
        {
            contentBytes = BaseConvert.CutLists2Lists(buf, 10, 8);    //截取msg内容
            string msgData = BitConverter.ToString(contentBytes.ToArray()).Replace("-", "");
            return msgData;
        }
        private int DecodeTimeSpan(List<byte> buf)
        {
            int Tspan = 0;
            byte[] data = BaseConvert.CutLists(buf, 18, 2);   //截取时间间隔
            string strHex = BitConverter.ToString(data).Replace("-", "");
            //Tspan = Convert.ToInt32(strHex, 16);
            Tspan = (int)Convert.ToUInt32(strHex, 16);

            return Tspan;
        }
    }


    public class TextFormat
    {
        public const string Date = "yyyyMMdd HH:mm:ss fff";
    }
}
