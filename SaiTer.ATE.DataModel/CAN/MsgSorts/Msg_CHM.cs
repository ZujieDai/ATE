using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace SaiTer.ATE.DataModel.CAN.MsgSorts
{
    public class Msg_CHM : MsgCommon
    {
        private string MsgHeadLine = "充电机握手";
        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content, ESGBDC_Ver eSGBDC_Ver)
        {
            CanMsgRich model = new CanMsgRich();

            string text;

            try
            {
                string version = DecodeVersion(content);
                model.ConsistMsg.Version = version;

                text = "充电机通信协议版本号:" + version;
                model.MsgText = Function.AppendTextToMsgHead(symbol, this.MsgHeadLine) + text;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
            }
            return model;
        }
        /// <summary>
        /// 解析版本号
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string DecodeVersion(List<byte> content)
        {
            string ver = string.Empty;
            try
            {
                //string strContent = "";
                //foreach(var item in content)
                //{
                //    strContent += item.ToString();
                //}
                //Log.Log.LogMessage($"Count:{content.Count} {strContent}");
                content = content.Take(3).ToList();
                content.Reverse(); 
                ver = Encoding.UTF8.GetString(content.ToArray());
            }
            catch { }
            if (ver == "SC1")
            {
                MsgConfig.mCurrentRefrence = 3000;
            }
            else
            {
                MsgConfig.mCurrentRefrence = 400;
                byte[] tmp = new byte[] { content[0], content[1] };
                string verH = BaseConvert.GetBytesToString(tmp);
                int H = Convert.ToInt32(verH, 16);


                string verL = content[2].ToString("x2");
                int L = Convert.ToInt32(verL, 16);

                ver = "V" + H.ToString() + "." + L.ToString();
            }
            return ver;
        }
    }
}
