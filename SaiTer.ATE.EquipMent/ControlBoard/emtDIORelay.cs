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
    public class emtDIORelay : EquipMentBase
    {
        public object Locker = new object();
        public emtDIORelay(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("继电器");
        }

        public override void ReadControlBoard_StateData()
        {
            try
            {
                SystemEvent.SendConnectState(false, this);
                while (true)
                {
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = new byte[7];
                        // byte[] WriteBuffer = new byte[8];
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                byte[] buffer = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x84, 0x0A };
                                lock (Locker)
                                {
                                    if (AutoReadData)
                                    {
                                        RevEquipMentData();
                                        EquipMentPort.SendData(buffer);
                                        byte[] temp = RevEquipMentData();
                                        if (temp != null && temp.Length >= 7)
                                        {
                                            Array.Copy(temp, RevMsgData, 7);
                                        }

                                    }
                                }
                                if (RevMsgData != null && RevMsgData.Length > 6)
                                {
                                    if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        lock (Locker)
                                        {
                                            if (AutoReadData)
                                            {
                                                RevEquipMentData();
                                                EquipMentPort.SendData(buffer);
                                                byte[] temp = RevEquipMentData();
                                                if (temp != null && temp.Length >= 7)
                                                {
                                                    Array.Copy(temp, RevMsgData, 7);
                                                }
                                            }
                                        }
                                    }

                                    if (RevMsgData != null && RevMsgData.Length > 6 && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                        SystemEvent.SendConnectState(true, this);
                                    else
                                        SystemEvent.SendConnectState(false, this);
                                }
                                else
                                {
                                    // SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                            SystemEvent.SendConnectState(false, this);
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void SetRelaySwitch(uint Register, bool OnOff)
        {
            List<byte> writeBuff = new List<byte>();
            writeBuff.Add(0x01);
            writeBuff.Add(0x06);
            writeBuff.Add((byte)(Register >> 8));
            writeBuff.Add((byte)Register);
            writeBuff.Add(0x00);
            writeBuff.Add((byte)(OnOff ? 0x01 : 0x00));
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff.ToArray());//CRC校验函数。
            writeBuff.AddRange(CheckSumByte.ToArray());

            string strTemp = BitConverter.ToString(writeBuff.ToArray()).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
            AutoReadData = false;
            Thread.Sleep(100);  // 会和读取状态的包粘连导致控制不了继电器
            lock (Locker)
            {
                EquipMentPort.SendData(writeBuff.ToArray());
            }
            AutoReadData = true;
        }

        public override List<bool> ReadRelaySwitch(int StratIndex, int RelayCount)
        {
            List<bool> result = new List<bool>();
            List<byte> writeBuff = new List<byte>
            {
                0x01,
                0x03,
                ((byte)(StratIndex >> 8)),
                (byte)StratIndex,
                ((byte)(RelayCount >> 8)),
                (byte)RelayCount
            };
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff.ToArray());//CRC校验函数。
            writeBuff.AddRange(CheckSumByte.ToArray());

            AutoReadData = false;
            lock (Locker)
            {
                EquipMentPort.SendData(writeBuff.ToArray());
                byte[] RevMsgData = RevEquipMentData();
                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                {
                    for (int i = 3; i < RevMsgData[2] + 3; i += 2)
                        result.Add(RevMsgData[i + 1] == 0x01);
                }
            }
            AutoReadData = true;
            return result;
        }
    }
}
