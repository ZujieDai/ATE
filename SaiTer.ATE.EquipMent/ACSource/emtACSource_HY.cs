using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 华源交流源
    /// </summary>
    public class emtACSource_HY : EquipMentBase
    {

        public ACSource_StateData stateData = new ACSource_StateData();
        public emtACSource_HY(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            byte[] WriteBuffer = GetBuffer("ON", "\r\n", "\r\n");
            SendData(WriteBuffer);
            SendMsgToFile("启动交流源输出");
        }

        public override void ACSource_OFF()
        {

            byte[] WriteBuffer = GetBuffer("OFF", "\r\n", "\r\n");
            SendData(WriteBuffer);
            SendMsgToFile("关闭交流源输出");
        }


        public override void ACSource_SetFreq(double freq)
        {
            byte[] WriteBuffer = GetBuffer("SFR " + freq.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
        }
        public override void ACSource_SetVolt(double Volt)
        {

            byte[] WriteBuffer = GetBuffer("SVOA " + Volt.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("SVOB " + Volt.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("SVOC " + Volt.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
        }

        public override void ACSource_SetVolt3(double VoltA, double VoltB, double VoltC)
        {
            byte[] WriteBuffer = GetBuffer("SVOA " + VoltA.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("SVOB " + VoltB.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("SVOC " + VoltC.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
        }

        public override void ACSource_SetAngle3(double AngleA, double AngleB, double AngleC)
        {

            byte[] WriteBuffer = GetBuffer("ANGLE_A " + AngleA.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("ANGLE_B " + AngleB.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
            Thread.Sleep(100);
            WriteBuffer = GetBuffer("ANGLE_C " + AngleC.ToString("F1"), "\r\n", "\r\n");
            SendData(WriteBuffer);
        }

        public override void ACSource_DisConnect()
        {
            //byte[] WriteBuffer = GetBuffer("END", "\r\n", "\r\n");

            //SendData(WriteBuffer);
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
                    byte[] WriteBufferVolt = GetBuffer("?VOA", "\r\n", "\r\n");
                    byte[] WriteBufferCurrent = GetBuffer("?IA", "\r\n", "\r\n");
                    byte[] WriteBufferFreq = GetBuffer("?FR", "\r\n", "\r\n");

                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBufferVolt).Replace('-', ' ');
                            //SendMsgToFile("交流源发送数据：" + strTemp);
                            EquipMentPort.SendData(WriteBufferVolt);
                            RevMsgData = RevEquipMentData();

                            if (RevMsgData != null)
                            {
                                List<byte> nTemp = RevMsgData.ToList();
                                if (!CheckBuffer(RevMsgData))
                                {
                                    EquipMentPort.SendData(WriteBufferVolt);
                                }

                                stateData = GetStateData(RevMsgData, "Volt", StateData);
                                SystemEvent.SendMonitorMessage(stateData);


                                EquipMentPort.SendData(WriteBufferCurrent);
                                RevMsgData = RevEquipMentData();
                                stateData = GetStateData(RevMsgData, "Cur", StateData);
                                SystemEvent.SendMonitorMessage(stateData);

                                EquipMentPort.SendData(WriteBufferFreq);
                                RevMsgData = RevEquipMentData();
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
            AutoReadData = false;
            Thread.Sleep(500);
            if (EquipMentPort != null)
            {
                for (int i = 0; i < ReConnNum; i++)
                {
                    DataBuf.Clear();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    // SendMsgToFile("交流源发送数据：" + strTemp);
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
                    case "Volt":
                        StateData.Volt = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;
                    case "Cur":
                        StateData.Current = Convert.ToSingle(Regex.Replace(str, @"[\r\n]", ""));
                        break;

                }
                return StateData;
            }
            catch { return StateData; }
        }

    }
}
