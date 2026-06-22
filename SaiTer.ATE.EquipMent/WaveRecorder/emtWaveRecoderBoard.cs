using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
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
    /// <summary>
    /// 设备-赛特录波板
    /// </summary>
    public class emtWaveRecoderBoard : EquipMentBase
    {
        public emtWaveRecoderBoard(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("录波板");

            SetImagePath();
        }


        public override void ReadWaveRecoderBoard_StateData()
        {
            SystemEvent.SendConnectState(false, this);

            while (true)
            {

                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = GetBuffer("GETCANDATA", null, null);
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();

                            if (RevMsgData != null)
                            {                               
                                SystemEvent.SendConnectState(true, this);
                            }
                            //else
                            //{
                            //    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");                             
                            //    SystemEvent.SendConnectState(false, this);
                            //    continue;
                            //}

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");                      
                        SystemEvent.SendConnectState(false, this);                     
                    }
                    Thread.Sleep(300);
                }
            }
        }
    }
}
