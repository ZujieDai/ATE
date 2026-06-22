using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-安规
    /// </summary>
    public class emtSafety : EquipMentBase
    {
        private static object SynLock = new object();
        private bool isPause;

        public override void PauseReadSafetyStateData(bool IsPause)
        {
            isPause = IsPause;
        }

        public emtSafety(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("安规");

        }
        public override void Safety_OFF()
        {
            SafetySetParam("FUNC: TEST OFF", "\r\n", "\r\n");
        }


        public override void SafetySetParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    byte[] writeBuffer = GetBuffer(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag);

                    if (EquipMentPort != null)
                    {
                        //DataBuf.Clear();
                        RevEquipMentData();
                        string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                        // SendMsgToFile("安规发送数据：" + strTemp);
                        EquipMentPort.SendData(writeBuffer);
                        RevEquipMentData();
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        public override bool SafetyReadParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag, ref string strResult)
        {
            try
            {
                this.AutoReadData = false;
                RevEquipMentData();//先取出已有的数据扔掉
                byte[] writeBuffer = GetBuffer(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag);

                if (EquipMentPort != null)
                {
                    lock (SynLock)
                    {
                        //DataBuf.Clear();
                        RevEquipMentData();
                        string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                        // SendMsgToFile("安规发送数据：" + strTemp);
                        EquipMentPort.SendData(writeBuffer);
                        byte[] RevMsgData = RevEquipMentData();
                        if (RevMsgData != null)
                        {
                            //strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                            //SendMsgToFile("安规接收数据：" + strTemp);
                            strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                            SendMsgToFile("安规接收数据：" + strResult);
                            SystemEvent.SendConnectState(true, this);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                SendExMsg(ex);
                return false;
            }
            finally { this.AutoReadData = true; }
        }


        public override void ReadSafetyStateData()
        {
            try
            {
                SystemEvent.SendConnectState(false, this);
                while (true)
                {
                    if (this.AutoReadData && !isPause)
                    {
                        byte[] writeBuffer = GetBuffer("*idn?", "\r\n", "\r\n");
                        string strResult = string.Empty;
                        if (EquipMentPort != null)
                        {
                            byte[] RevMsgData = null;
                            lock (SynLock)
                            {
                                //DataBuf.Clear();
                                RevEquipMentData();
                                string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                                // SendMsgToFile("安规发送数据：" + strTemp);
                                EquipMentPort.SendData(writeBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                SystemEvent.SendConnectState(false, this);
                            }
                            // SystemEvent.SendMonitorMessage(strResult);
                        }
                        else
                        {
                            SystemEvent.SendConnectState(false, this);
                            //SendMsgToFile("安规通道对象不存在，请检查安规通道");
                        }
                        Thread.Sleep(300);
                    }
                    Thread.Sleep(5);
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);

            }
        }


    }
}
