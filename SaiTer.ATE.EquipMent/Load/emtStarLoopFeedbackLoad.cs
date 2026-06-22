using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

/*
 * 注：赛特新回馈负载  手拉手环式通道设计
 *    
 * 
 */



namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-环式回馈负载
    /// </summary>
    public class emtStarLoopFeedbackLoad : EquipMentBase
    {
        private static object SynLockLoad = new object();
        public LoopFeedbackLoad_StateData StateData = new LoopFeedbackLoad_StateData();
        public emtStarLoopFeedbackLoad(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("回馈负载");
        }

        public override void SetLoopFeedbackLoadParams(int channel, double voltage, double current)
        {
            int IntVoltage = Convert.ToInt32(voltage) * 1000;//单位mV
            byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };
            int IntCurrent = Convert.ToInt32(current) * 1000;//单位mA
            byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x11,//帧长度高
                0x80,//
                0x00,
                0x22,
                0x01
            };
            ReturnbyteSource[6] = (byte)channel;
            ReturnbyteSource.AddRange(voltBytes);
            ReturnbyteSource.AddRange(currBytes);

            byte[] WriteBuffer = ReturnbyteSource.ToArray();

            byte[] temp = new byte[WriteBuffer.Length - 1];

            Array.Copy(WriteBuffer, 1, temp, 0, WriteBuffer.Length - 1);
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);
            WriteBuffer = ReturnbyteSource.ToArray();

            SendData_FeedbackLoad(WriteBuffer);

        }


        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void LoopFeedbackLoad_ON(int channel)
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01, 0x00, 0x00 };
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01 };
            temp[5] = (byte)channel;
            WriteBuffer[6] = (byte)channel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer);

        }
        /// <summary>
        /// 关闭回馈负载
        /// </summary>
        public override void LoopFeedbackLoad_OFF(int Channel)
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)Channel;
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02 };
            temp[5] = (byte)Channel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer);

        }
        public override void LoopFeedbackLoad_NoParallel(int Channel)
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x29, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)Channel;
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x29, 0x01, 0x02 };
            temp[5] = (byte)Channel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer);
        }

        public override void LoopFeedbackLoad_Parallel(int Channel)
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x29, 0x01, 0x01, 0x00, 0x00 };
            WriteBuffer[6] = (byte)Channel;//用桩编号作为通道号
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x29, 0x01, 0x01 };
            temp[5] = (byte)Channel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer);
        }
        private bool SendData_FeedbackLoad(byte[] WriteBuffer)
        {
            try
            {
                AutoReadData = false;
                Thread.Sleep(300);
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        lock (SynLockLoad)
                        {
                            EquipMentPort.SendData(WriteBuffer);
                            byte[] RevMsgData = RevEquipMentData();

                            if (RevMsgData == null)
                            {
                                SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                continue;
                            }
                        }
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                    AutoReadData = true;
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }

        }

        public override void ReadFeedbackLoad_StateData()
        {
            LoopFeedbackLoad_StateData state = new LoopFeedbackLoad_StateData();
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicLoopFeedbackLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicLoopFeedbackLoad_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {

                if (AutoReadData)
                {
                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = { 0x68, 0x00, 0x18, 0x80, 0x00, 0x21, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0xCF, 0x8E };
                    byte[] WriteBuffer2 = { 0x68, 0x00, 0x18, 0x80, 0x00, 0x2B, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0xE9, 0x2C };
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            lock (SynLockLoad)
                            {
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null && (RevMsgData[0] == 0x68 && RevMsgData[2] == 0xA8 && RevMsgData[3] == 0x81))
                            {

                                StateData = GetStateData(ref state, RevMsgData, 1);
                                lock (SynLockLoad)
                                {
                                    EquipMentPort.SendData(WriteBuffer2);
                                    RevMsgData = RevEquipMentData();
                                    StateData = GetStateData(ref state, RevMsgData, 2);
                                }
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                StateData = new LoopFeedbackLoad_StateData();
                                StateData.ChargerID = this.ChargerID;
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                        StateData = new LoopFeedbackLoad_StateData();
                        StateData.ChargerID = this.ChargerID;
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(StateData);
                    }
                    Thread.Sleep(300);
                }

            }
        }


        private LoopFeedbackLoad_StateData GetStateData(ref LoopFeedbackLoad_StateData state, byte[] buff, int num)
        {

            state.ChargerID = ChargerID;
            if (buff == null)
            {
                return state;
            }
            if (num == 1)
            {
                int temp = Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2") + buff[9].ToString("x2") + buff[10].ToString("x2"), 16);
                state.Voltage_1 = Convert.ToDouble(temp) / 1000;
                temp = Convert.ToInt32(buff[11].ToString("x2") + buff[12].ToString("x2") + buff[13].ToString("x2") + buff[14].ToString("x2"), 16);
                state.Current_1 = Convert.ToSingle(temp) / 1000;
                state.RunState_1 = buff[15] == 1 ? "运行" : "停止";

                state.Voltage_2 = Convert.ToDouble(Convert.ToInt32(buff[16 + 1].ToString("x2") + buff[17 + 1].ToString("x2") + buff[18 + 1].ToString("x2") + buff[19 + 1].ToString("x2"), 16)) / 1000;
                state.Current_2 = Convert.ToDouble(Convert.ToInt32(buff[20 + 1].ToString("x2") + buff[21 + 1].ToString("x2") + buff[22 + 1].ToString("x2") + buff[23 + 1].ToString("x2"), 16)) / 1000;
                state.RunState_2 = buff[25] == 1 ? "运行" : "停止";

                state.Voltage_3 = Convert.ToDouble(Convert.ToInt32(buff[25 + 2].ToString("x2") + buff[26 + 2].ToString("x2") + buff[27 + 2].ToString("x2") + buff[28 + 2].ToString("x2"), 16)) / 1000;
                state.Current_3 = Convert.ToDouble(Convert.ToInt32(buff[29 + 2].ToString("x2") + buff[30 + 2].ToString("x2") + buff[31 + 2].ToString("x2") + buff[32 + 2].ToString("x2"), 16)) / 1000;
                state.RunState_3 = buff[35] == 1 ? "运行" : "停止";

                state.Voltage_4 = Convert.ToDouble(Convert.ToInt32(buff[3 + 34].ToString("x2") + buff[3 + 35].ToString("x2") + buff[3 + 36].ToString("x2") + buff[3 + 37].ToString("x2"), 16)) / 1000;
                state.Current_4 = Convert.ToDouble(Convert.ToInt32(buff[3 + 38].ToString("x2") + buff[3 + 39].ToString("x2") + buff[3 + 40].ToString("x2") + buff[3 + 41].ToString("x2"), 16)) / 1000;
                state.RunState_4 = buff[3 + 42] == 1 ? "运行" : "停止";

                state.Voltage_5 = Convert.ToDouble(Convert.ToInt32(buff[4 + 43].ToString("x2") + buff[4 + 44].ToString("x2") + buff[4 + 45].ToString("x2") + buff[4 + 46].ToString("x2"), 16)) / 1000;
                state.Current_5 = Convert.ToDouble(Convert.ToInt32(buff[4 + 47].ToString("x2") + buff[4 + 48].ToString("x2") + buff[4 + 49].ToString("x2") + buff[4 + 50].ToString("x2"), 16)) / 1000;
                state.RunState_5 = buff[4 + 51] == 1 ? "运行" : "停止";

                state.Voltage_6 = Convert.ToDouble(Convert.ToInt32(buff[5 + 52].ToString("x2") + buff[5 + 53].ToString("x2") + buff[5 + 54].ToString("x2") + buff[5 + 55].ToString("x2"), 16)) / 1000;
                state.Current_6 = Convert.ToDouble(Convert.ToInt32(buff[5 + 56].ToString("x2") + buff[5 + 57].ToString("x2") + buff[5 + 58].ToString("x2") + buff[5 + 59].ToString("x2"), 16)) / 1000;
                state.RunState_6 = buff[5 + 60] == 1 ? "运行" : "停止";

                state.Voltage_7 = Convert.ToDouble(Convert.ToInt32(buff[6 + 61].ToString("x2") + buff[6 + 62].ToString("x2") + buff[6 + 63].ToString("x2") + buff[6 + 64].ToString("x2"), 16)) / 1000;
                state.Current_7 = Convert.ToDouble(Convert.ToInt32(buff[6 + 65].ToString("x2") + buff[6 + 66].ToString("x2") + buff[6 + 67].ToString("x2") + buff[6 + 68].ToString("x2"), 16)) / 1000;
                state.RunState_7 = buff[6 + 69] == 1 ? "运行" : "停止";

                state.Voltage_8 = Convert.ToDouble(Convert.ToInt32(buff[7 + 70].ToString("x2") + buff[7 + 71].ToString("x2") + buff[7 + 72].ToString("x2") + buff[7 + 73].ToString("x2"), 16)) / 1000;
                state.Current_8 = Convert.ToDouble(Convert.ToInt32(buff[7 + 74].ToString("x2") + buff[7 + 75].ToString("x2") + buff[7 + 76].ToString("x2") + buff[7 + 77].ToString("x2"), 16)) / 1000;
                state.RunState_8 = buff[7 + 78] == 1 ? "运行" : "停止";

                state.Voltage_9 = Convert.ToDouble(Convert.ToInt32(buff[8 + 79].ToString("x2") + buff[8 + 80].ToString("x2") + buff[8 + 81].ToString("x2") + buff[8 + 82].ToString("x2"), 16)) / 1000;
                state.Current_9 = Convert.ToDouble(Convert.ToInt32(buff[8 + 83].ToString("x2") + buff[8 + 84].ToString("x2") + buff[8 + 85].ToString("x2") + buff[8 + 86].ToString("x2"), 16)) / 1000;
                state.RunState_9 = buff[8 + 87] == 1 ? "运行" : "停止";

                state.Voltage_10 = Convert.ToDouble(Convert.ToInt32(buff[9 + 88].ToString("x2") + buff[9 + 89].ToString("x2") + buff[9 + 90].ToString("x2") + buff[9 + 91].ToString("x2"), 16)) / 1000;
                state.Current_10 = Convert.ToDouble(Convert.ToInt32(buff[9 + 92].ToString("x2") + buff[9 + 93].ToString("x2") + buff[9 + 94].ToString("x2") + buff[9 + 95].ToString("x2"), 16)) / 1000;
                state.RunState_10 = buff[9 + 96] == 1 ? "运行" : "停止";

                state.Voltage_11 = Convert.ToDouble(Convert.ToInt32(buff[10 + 97].ToString("x2") + buff[10 + 98].ToString("x2") + buff[10 + 99].ToString("x2") + buff[10 + 100].ToString("x2"), 16)) / 1000;
                state.Current_11 = Convert.ToDouble(Convert.ToInt32(buff[10 + 101].ToString("x2") + buff[10 + 102].ToString("x2") + buff[10 + 103].ToString("x2") + buff[10 + 104].ToString("x2"), 16)) / 1000;
                state.RunState_11 = buff[10 + 105] == 1 ? "运行" : "停止";

                state.Voltage_12 = Convert.ToDouble(Convert.ToInt32(buff[11 + 106].ToString("x2") + buff[11 + 107].ToString("x2") + buff[11 + 108].ToString("x2") + buff[11 + 109].ToString("x2"), 16)) / 1000;
                state.Current_12 = Convert.ToDouble(Convert.ToInt32(buff[11 + 110].ToString("x2") + buff[11 + 111].ToString("x2") + buff[11 + 112].ToString("x2") + buff[11 + 113].ToString("x2"), 16)) / 1000;
                state.RunState_12 = buff[11 + 114] == 1 ? "运行" : "停止";

                state.Voltage_13 = Convert.ToDouble(Convert.ToInt32(buff[12 + 115].ToString("x2") + buff[12 + 116].ToString("x2") + buff[12 + 117].ToString("x2") + buff[12 + 118].ToString("x2"), 16)) / 1000;
                state.Current_13 = Convert.ToDouble(Convert.ToInt32(buff[12 + 119].ToString("x2") + buff[12 + 120].ToString("x2") + buff[12 + 121].ToString("x2") + buff[12 + 122].ToString("x2"), 16)) / 1000;
                state.RunState_13 = buff[12 + 123] == 1 ? "运行" : "停止";

                state.Voltage_14 = Convert.ToDouble(Convert.ToInt32(buff[13 + 124].ToString("x2") + buff[13 + 125].ToString("x2") + buff[13 + 126].ToString("x2") + buff[13 + 127].ToString("x2"), 16)) / 1000;
                state.Current_14 = Convert.ToDouble(Convert.ToInt32(buff[13 + 128].ToString("x2") + buff[13 + 129].ToString("x2") + buff[13 + 130].ToString("x2") + buff[13 + 131].ToString("x2"), 16)) / 1000;
                state.RunState_14 = buff[13 + 132] == 1 ? "运行" : "停止";

                state.Voltage_15 = Convert.ToDouble(Convert.ToInt32(buff[14 + 133].ToString("x2") + buff[14 + 134].ToString("x2") + buff[14 + 135].ToString("x2") + buff[14 + 136].ToString("x2"), 16)) / 1000;
                state.Current_15 = Convert.ToDouble(Convert.ToInt32(buff[14 + 137].ToString("x2") + buff[14 + 138].ToString("x2") + buff[14 + 139].ToString("x2") + buff[14 + 140].ToString("x2"), 16)) / 1000;
                state.RunState_15 = buff[14 + 141] == 1 ? "运行" : "停止";

                state.Voltage_16 = Convert.ToDouble(Convert.ToInt32(buff[15 + 142].ToString("x2") + buff[15 + 143].ToString("x2") + buff[15 + 144].ToString("x2") + buff[15 + 145].ToString("x2"), 16)) / 1000;
                state.Current_16 = Convert.ToDouble(Convert.ToInt32(buff[15 + 146].ToString("x2") + buff[15 + 147].ToString("x2") + buff[15 + 148].ToString("x2") + buff[15 + 149].ToString("x2"), 16)) / 1000;
                state.RunState_16 = buff[15 + 150] == 1 ? "运行" : "停止";
            }
            else if (num == 2)
            {
                state.Parallet_1 = buff[7];
                state.Parallet_2 = buff[9];
                state.Parallet_3 = buff[11];
                state.Parallet_4 = buff[13];
                state.Parallet_5 = buff[15];
                state.Parallet_6 = buff[17];
                state.Parallet_7 = buff[19];
                state.Parallet_8 = buff[21];
                state.Parallet_9 = buff[23];
                state.Parallet_10 = buff[25];
                state.Parallet_11 = buff[27];
                state.Parallet_12 = buff[29];
                state.Parallet_13 = buff[31];
                state.Parallet_14 = buff[33];
                state.Parallet_15 = buff[35];
                state.Parallet_16 = buff[37];

            }



            return state;
        }
    }
}
