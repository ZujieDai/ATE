using NPOI.SS.Formula.Functions;
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
    /// 设备- 固特交流源    未完成
    /// </summary>
    public class emtACSource_GT : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        public static object SynLock = new object();
        public emtACSource_GT(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
        }

        public override void ACSource_ON()
        {
            if (AllEquipStateData.DicACSource_StateData[ChargerID].Volt < 50)
            {
                byte[] WriteBuffer = GetBuffer(0x0009, 0x0001, 6);
                SendData(WriteBuffer);
            }
        }

        public override void ACSource_OFF()
        {
            byte[] WriteBuffer = GetBuffer(0x0009, 0x0002, 6);
            SendData(WriteBuffer);
        }


        public override void ACSource_SetFreq(double freq)
        {
            //无此指令
        }
        public override void ACSource_SetVolt(double Volt)
        {

            //List<byte> lstBytes = new List<byte>() { 0x01, 0x10, 0x00, 0x00, 0x00, 0x0A, 0x14, 0x00, 0xBE, 0x03, 0x01, 0x01, 0x08, 0x00, 0xB0, 0x03, 0x53, 0x01, 0x0F, 0x00, 0x11, 0x03, 0x03, 0x03, 0xE8, 0x00, 0x00 };
            //byte[] data = ConvertIntToByteArray(Convert.ToInt32(Volt));
            //lstBytes[7] = data[0];
            //lstBytes[8] = data[1];
            //byte[] writeBuff = lstBytes.ToArray();//把前面压入的数据转成成数组，校验
            //byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
            //lstBytes.AddRange(CheckSumByte);
            //SendData(lstBytes.ToArray());
            double voltNow = AllEquipStateData.DicACSource_StateData[1].Volt;
            double pecent = Math.Abs(Volt - voltNow) / voltNow;
            if (pecent > 0.1)
            {
                int tempVolt = 0;
                if (voltNow> Volt)
                {
                    tempVolt = Convert.ToInt32(voltNow - Math.Abs(Volt - voltNow) / 2);
                }
                else
                {
                    tempVolt = Convert.ToInt32(voltNow + Math.Abs(Volt - voltNow) / 2);
                }
                 
                byte[] buffer = GetBuffer(0x0000, Convert.ToInt32(tempVolt), 0x06);
                SendData(buffer);
                Thread.Sleep(3000);
            }

            byte[] WriteBuffer = GetBuffer(0x0000, Convert.ToInt32(Volt), 0x06);
            SendData(WriteBuffer);
            Thread.Sleep(1000);

        }

        public override void ACSource_DisConnect()
        {

        }

        private byte[] GetBuffer(int addr, int value, byte cmd)
        {
            try
            {
                List<byte> lstBytes = new List<byte>();
                lstBytes.Add(1);
                lstBytes.Add(cmd);
                lstBytes.AddRange(ConvertIntToByteArray(addr));

                lstBytes.AddRange(ConvertIntToByteArray(value));
                byte[] writeBuff = lstBytes.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
                lstBytes.AddRange(CheckSumByte);//把校验压入list
                byte[] WriteBuffer = lstBytes.ToArray();//返回要发送的字节
                return WriteBuffer;
            }
            catch { return null; }
        }

        private byte[] ConvertIntToByteArray(int value)
        {
            string hexString = value.ToString("X4"); // 将十六进制数转换为四位的十六进制字符串
            byte[] byteArray = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                string byteString = hexString.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(byteString, 16);
            }
            return byteArray;
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
                    byte[] WriteBuffer = GetBuffer(0x0000, 10, 4);



                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile("交流源发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                List<byte> nTemp = RevMsgData.ToList();
                                if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
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

            AutoReadData = false;
            Thread.Sleep(500);
            if (EquipMentPort != null)
            {
                for (int i = 0; i < ReConnNum; i++)
                {
                    DataBuf.Clear();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    // SendMsgToFile("交流源发送数据：" + strTemp);
                    lock (SynLock)
                    {
                        EquipMentPort.SendData(WriteBuffer);
                        byte[] RevMsgData = RevEquipMentData();
                        if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                        {
                            EquipMentPort.SendData(WriteBuffer);
                        }
                        else
                            break;
                    }
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


        private ACSource_StateData GetStateData(byte[] buffer, ACSource_StateData StateData)
        {
            try
            {

                StateData.ChargerID = this.ChargerID;
                if (buffer != null && CheckOut.CheckModbusCrc16_High_Right(buffer) && buffer.Length >= 22)
                {
                    int temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2"), 16);
                    StateData.Volt = Convert.ToSingle(temp) / 10;
                    temp = Convert.ToInt32(buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                    StateData.Current = Convert.ToSingle(temp) / 10;

                    temp = Convert.ToInt32(buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                    StateData.Volt_B = Convert.ToSingle(temp) / 10;
                    temp = Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2"), 16);
                    StateData.Current_B = Convert.ToSingle(temp) / 10;

                    temp = Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2"), 16);
                    StateData.Volt_C = Convert.ToSingle(temp) / 10;
                    temp = Convert.ToInt32(buffer[21].ToString("x2") + buffer[22].ToString("x2"), 16);
                    StateData.Current_C = Convert.ToSingle(temp) / 10;
                }
                StateData.Freq = 50;
                return StateData;
            }
            catch { return StateData; }
        }

    }
}
