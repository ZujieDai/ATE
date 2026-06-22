using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections;
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
    /// 设备- 艾诺交流源
    /// </summary>
    public class emtACSource_AN : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        public static object SynLock = new object();
        public emtACSource_AN(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            if (AllEquipStateData.DicACSource_StateData[ChargerID].Volt < 50)
            {
                byte[] WriteBuffer = { 0x7B, 0x00, 0x08, 0x01, 0x0F, 0xFF, 0x17, 0x7D };
                SendData(WriteBuffer);
            }
        }

        public override void ACSource_OFF()
        {
            byte[] WriteBuffer = { 0x7B, 0x00, 0x08, 0x01, 0x0F, 0x00, 0x18, 0x7D };
            SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            List<byte> bytes = new List<byte>();
            byte[] temp = { 0x7B, 0x00, 0x0C, 0x01, 0x5A, 0x50, 0x08, 0x98 };
            bytes.AddRange(temp);
            byte[] byteArray = BitConverter.GetBytes((short)(freq * 10));
            Array.Reverse(byteArray);
            bytes.AddRange(byteArray);
            byte checkSum = CalcCheckSum(bytes.ToArray());
            bytes.Add(checkSum);
            bytes.Add(0x7D);
            SendData(bytes.ToArray());
        }
        public override void ACSource_SetVolt(double Volt)
        {
            List<byte> bytes = new List<byte>();
            byte[] temp = { 0x7B, 0x00, 0x0C, 0x01, 0x5A, 0x50 };
            bytes.AddRange(temp);
            byte[] byteArray = BitConverter.GetBytes((short)(Volt * 10));
            Array.Reverse(byteArray);
            bytes.AddRange(byteArray);
            bytes.AddRange(new byte[2] { 0x01, 0xF4 });
            byte checkSum = CalcCheckSum(bytes.ToArray());
            bytes.Add(checkSum);
            bytes.Add(0x7D);
            SendData(bytes.ToArray());
        }

        public override void ACSource_DisConnect()
        {

        }


        private byte CalcCheckSum(byte[] data)
        {
            byte sum = 0;
            for (int i = 1; i < data.Length; i++)
            {
                sum += data[i];
            }
            return sum;
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
                    byte[] WriteBuffer = { 0x7B, 0x00, 0x08, 0x01, 0xF0, 0xAE, 0xA7, 0x7D };


                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile("交流源发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                if (!CheckBuffer(RevMsgData))
                                {
                                    lock (SynLock)
                                    {
                                        EquipMentPort.SendData(WriteBuffer);
                                        RevMsgData = RevEquipMentData();
                                    }
                                }

                                stateData = GetStateData(RevMsgData, StateData);
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
        }
        /// <summary>
        /// 校验报文
        /// </summary>
        /// <param name="tbytes"></param>
        /// <returns></returns>
        public bool CheckBuffer(byte[] tbytes)
        {
            try
            {
                if (tbytes[0] != 0x7B || tbytes[tbytes.Length - 1] != 0x7D)
                    return false;

                byte[] lengthBytes = new byte[2];
                Array.Copy(tbytes, 1, lengthBytes, 0, 2);
                int length = BitConverter.ToInt16(lengthBytes, 0);
                if (length != tbytes.Length)
                    return false;


                int sum = 0;
                for (int i = 1; i < tbytes.Length - 2; i++)
                {
                    sum += tbytes[i];
                }
                if (sum != tbytes[tbytes.Length - 2])
                    return false;
                return true;
            }
            catch
            {
                return false;
            }

        }


        private ACSource_StateData GetStateData(byte[] buffer, ACSource_StateData StateData)
        {
            StateData.ChargerID = this.ChargerID;
            if (buffer == null)
            {
                return StateData;
            }
            try
            {
                StateData.Volt = Convert.ToSingle(Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16)) / 10;
                StateData.Current = Convert.ToSingle(Convert.ToInt32(buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16)) / 10;
                StateData.Freq = Convert.ToSingle(Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2"), 16)) / 10;

                StateData.Volt_B = Convert.ToSingle(Convert.ToInt32(buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16)) / 10;
                StateData.Current_B = Convert.ToSingle(Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2"), 16)) / 10;

                StateData.Volt_C = Convert.ToSingle(Convert.ToInt32(buffer[27].ToString("x2") + buffer[28].ToString("x2"), 16)) / 10;
                StateData.Current_C = Convert.ToSingle(Convert.ToInt32(buffer[29].ToString("x2") + buffer[30].ToString("x2"), 16)) / 10;

                return StateData;
            }
            catch { return StateData; }
        }

    }
}
