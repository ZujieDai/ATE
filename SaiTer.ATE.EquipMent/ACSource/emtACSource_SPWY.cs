using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备- 思普交流源
    /// </summary>
    public class emtACSource_SPWY : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        public static object SynLock = new object();
        public emtACSource_SPWY(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            // byte[] WriteBuffer = new byte[] { 0x53, 0x01 };
            if (AllEquipStateData.DicACSource_StateData[ChargerID].Volt < 50)
            {
                byte[] WriteBuffer = GetBuffer("ON", "\r\n", "\r\n");
                SendData(WriteBuffer);
            }
        }

        public override void ACSource_OFF()
        {
            // byte[] WriteBuffer = new byte[] { 0x53, 0x00 };
            byte[] WriteBuffer = GetBuffer("OFF", "\r\n", "\r\n");
            SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            // byte[] WriteBuffer = new byte[] { 0x53, 0x00 };
            byte[] WriteBuffer = GetBuffer("FR " + freq.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
        }
        public override void ACSource_SetVolt(double Volt)
        {
            //List<byte> ls = new List<byte>();
            //ls.AddRange(new byte[] { 0x52 });
            //int iU = Convert.ToInt32(Volt);
            //if (iU > 300) iU = 300;
            //if (iU < 100) iU = 100;
            //byte[] byte_i = BitConverter.GetBytes(iU);
            //ls.Add(byte_i[1]);
            //ls.Add(byte_i[0]);
            ////思普交流源设置电压参数报文格式   0x52 指令   后面两个字节为电压值 高8位加低8位  如 220v  指令为52  00  DC
            //byte[] WriteBuffer = ls.ToArray();
            byte[] WriteBuffer = GetBuffer("VSA " + Volt.ToString("F1"), "\r\n", "\r\n");

            SendData(WriteBuffer);
        }

        public override void ACSource_DisConnect()
        {
            byte[] WriteBuffer = GetBuffer("END", "\r\n", "\r\n");

            SendData(WriteBuffer);
        }



        public override void ACSource_ReadState()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicACSource_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicACSource_StateData.Add(ChargerID, stateData);
            }
            ACSource_StateData StateData = new ACSource_StateData();
            while (true)
            {

                if (AutoReadData)
                {
                    byte[] RevMsgData = null;
                    byte[] WriteBufferVolt_A = GetBuffer("?RVO", "\r\n", "\r\n");
                    byte[] WriteBufferVolt_B = GetBuffer("?SVO", "\r\n", "\r\n");
                    byte[] WriteBufferVolt_C = GetBuffer("?TVO", "\r\n", "\r\n");
                    byte[] WriteBufferCurrent_A = GetBuffer("?RAM", "\r\n", "\r\n");
                    byte[] WriteBufferCurrent_B = GetBuffer("?SAM", "\r\n", "\r\n");
                    byte[] WriteBufferCurrent_C = GetBuffer("?TAM", "\r\n", "\r\n");
                    byte[] WriteBufferFreq = GetBuffer("?FR", "\r\n", "\r\n");

                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBufferVolt_A).Replace('-', ' ');
                            //SendMsgToFile("交流源发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                EquipMentPort.SendData(WriteBufferVolt_A);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                List<byte> nTemp = RevMsgData.ToList();
                                if (!CheckBuffer(RevMsgData))
                                {
                                    lock (SynLock)
                                    {
                                        EquipMentPort.SendData(WriteBufferVolt_A);
                                        RevMsgData = RevEquipMentData();
                                    }
                                }
                                stateData = GetStateData(RevMsgData, "VoltA", StateData);
                                SystemEvent.SendMonitorMessage(stateData);




                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferVolt_B);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "VoltB", StateData);
                                SystemEvent.SendMonitorMessage(stateData);




                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferVolt_C);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "VoltC", StateData);
                                SystemEvent.SendMonitorMessage(stateData);





                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferCurrent_A);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "CurA", StateData);
                                SystemEvent.SendMonitorMessage(stateData);



                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferCurrent_B);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "CurB", StateData);
                                SystemEvent.SendMonitorMessage(stateData);




                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferCurrent_C);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "CurC", StateData);
                                SystemEvent.SendMonitorMessage(stateData);





                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(WriteBufferFreq);
                                    RevMsgData = RevEquipMentData();
                                }
                                stateData = GetStateData(RevMsgData, "Freq", StateData);
                                SystemEvent.SendMonitorMessage(stateData);

                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile("读交流源状态数据失败");
                                stateData = new ACSource_StateData();
                                stateData.ChargerID = this.ChargerID;
                                SystemEvent.SendMonitorMessage(stateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile("交流源通道对象不存在，请检查");
                        stateData = new ACSource_StateData();
                        stateData.ChargerID = this.ChargerID;
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(stateData);
                    }
                    Thread.Sleep(300);
                }
            }
        }

        private bool SendData(byte[] WriteBuffer)
        {
            lock (SynLock)
            {
                AutoReadData = false;
                Thread.Sleep(500);
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        // SendMsgToFile("思普交流源发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile("交流源通道对象不存在，请检查交流源通道");
                    AutoReadData = true;
                    return false;
                }
            }
        }
        /// <summary>
        /// 校验报文
        /// </summary>
        /// <param name="tbytes"></param>
        /// <returns></returns>
        public bool CheckBuffer(byte[] tbytes)
        {
            bool isCheck = false;
            try
            {
                if (tbytes[tbytes.Length - 2] == 0x0D && tbytes[tbytes.Length - 1] == 0x0A)
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }


        private ACSource_StateData GetStateData(byte[] buffer, string buffType, ACSource_StateData StateData)
        {
            try
            {
                StateData.ChargerID = this.ChargerID;
                string str = Encoding.ASCII.GetString(buffer.ToArray());
                switch (buffType)
                {
                    case "Freq":
                        StateData.Freq = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "VoltA":
                        StateData.Volt = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "VoltB":
                        StateData.Volt_B = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "VoltC":
                        StateData.Volt_C = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "CurA":
                        StateData.Current = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "CurB":
                        StateData.Current_B = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "CurC":
                        StateData.Current_C = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                }
                return StateData;
            }
            catch { return StateData; }
        }

    }
}
