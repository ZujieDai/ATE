using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    public class emtBMS_USA_DC_Msg : EquipMentBase
    {
        /// <summary>
        /// 用于接收欧标导引的报文，只需要接收就行了
        /// </summary>
        public emtBMS_USA_DC_Msg(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = $"{LanguageManager.GetByKey("美标直流导引BMS")} {LanguageManager.GetByKey("报文")}";
        }

        public override void ReadBMS_StateData()
        {
            while (true)
            {
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        byte[] RevData = RevEquipMentData();
                        if (RevData != null)
                        {
                            Dictionary<int, string> dicMsg = new Dictionary<int, string>();
                            string msg = ASCIIEncoding.UTF8.GetString(RevData);
                            dicMsg.Add(ChargerID, msg);
                            SystemEvent.RevEUMsg(dicMsg);
                        }
                        else
                        {
                        }

                    }
                }
                Thread.Sleep(5);
            }
        }

        public override void SendEUMsg(string EUMsg)
        {
            byte[] writeBuffer = GetBuffer(EUMsg, "\r\n", "\r\n");
            string strResult = string.Empty;
            if (EquipMentPort != null)
            {
                //DataBuf.Clear();
                RevEquipMentData();
                string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                // SendMsgToFile("安规发送数据：" + strTemp);
                EquipMentPort.SendData(writeBuffer);
            }
        }
    }
}
