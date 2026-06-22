using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 多通道电阻负载（使用回馈载协议）
    /// </summary>
    public class emtResistanceLoad_MultiChannel_AC : EquipMentBase
    {
        private static object SynLockLoad = new object();
        public ResisLoad_MultiChannel_AC_StateData StateData = new ResisLoad_MultiChannel_AC_StateData();
        public emtResistanceLoad_MultiChannel_AC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("多通道") + LanguageManager.GetByKey("电阻负载");
        }

        public override void SetResisLoadVoltCurr(double voltage, double current)
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
            ReturnbyteSource[6] = (byte)ChargerID;//用桩编号作为通道号
            ReturnbyteSource.AddRange(voltBytes);
            ReturnbyteSource.AddRange(currBytes);

            byte[] WriteBuffer = ReturnbyteSource.ToArray();

            byte[] temp = new byte[WriteBuffer.Length - 1];

            Array.Copy(WriteBuffer, 1, temp, 0, WriteBuffer.Length - 1);
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);
            WriteBuffer = ReturnbyteSource.ToArray();

            SendData_ResisLoad(WriteBuffer);

            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }


        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void ResisLoad_ON()
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01, 0x00, 0x00 };
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01 };
            temp[5] = (byte)ChargerID;
            WriteBuffer[6] = (byte)ChargerID;//用桩编号作为通道号
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_ResisLoad(WriteBuffer);

            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public override void ResisLoad_OFF()
        {
            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)ChargerID;//用桩编号作为通道号
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02 };
            temp[5] = (byte)ChargerID;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_ResisLoad(WriteBuffer);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        public override void ResisLoad_NoParallel()
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["ResisLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)chanel;
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x02 };
            temp[5] = (byte)chanel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_ResisLoad(WriteBuffer);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        public override void ResisLoad_Parallel()
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["ResisLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x01, 0x00, 0x00 };
            WriteBuffer[6] = (byte)chanel;//用桩编号作为通道号
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x01 };
            temp[5] = (byte)chanel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_ResisLoad(WriteBuffer);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        private bool SendData_ResisLoad(byte[] WriteBuffer)
        {
            lock (SynLockLoad)
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
                            EquipMentPort.SendData(WriteBuffer);
                            byte[] RevMsgData = RevEquipMentData();
                            if (RevMsgData == null)
                            {
                                SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                continue;
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
        }

        public override void ReadResisLoad_State_AC()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                try
                {
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = null;
                        byte[] WriteBuffer = { 0x68, 0x00, 0x09, 0x80, 0x00, 0x21, (byte)ChargerID, 0x00, 0x00 };
                        byte[] temp = { 0x00, 0x09, 0x80, 0x00, 0x21, (byte)ChargerID };
                        byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
                        WriteBuffer[7] = CheckSumByte[0];
                        WriteBuffer[8] = CheckSumByte[1];
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
                                if (RevMsgData != null)
                                {
                                    byte[] revTemp = new byte[RevMsgData.Length - 1];
                                    Array.Copy(RevMsgData, 1, revTemp, 0, RevMsgData.Length - 1);
                                    //if (CheckOut.CheckModbusCrc16_High_Right(revTemp) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    //{
                                    //    EquipMentPort.SendData(WriteBuffer);
                                    //    RevMsgData = RevEquipMentData();
                                    //}
                                    if (CheckOut.CheckModbusCrc16_High_Right(revTemp))
                                    {
                                        StateData = GetStateData(RevMsgData);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                    else
                                    {
                                        StateData = new ResisLoad_MultiChannel_AC_StateData();
                                        StateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(false, this);
                                    }
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new ResisLoad_MultiChannel_AC_StateData();
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
                            StateData = new ResisLoad_MultiChannel_AC_StateData();
                            StateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                        Thread.Sleep(300);
                    }

                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    StateData = new ResisLoad_MultiChannel_AC_StateData();
                    StateData.ChargerID = this.ChargerID;
                    SystemEvent.SendMonitorMessage(StateData);
                    SystemEvent.SendConnectState(false, this);
                    Thread.Sleep(300);
                }
            }
        }


        private ResisLoad_MultiChannel_AC_StateData GetStateData(byte[] buff)
        {
            ResisLoad_MultiChannel_AC_StateData state = new ResisLoad_MultiChannel_AC_StateData();
            state.ChargerID = ChargerID;

            if (buff.Length >= 18)
            {
                if (buff[6] != ChargerID)
                {
                    return state;
                }
                state.Chanel = buff[6];


                int temp = Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2") + buff[9].ToString("x2") + buff[10].ToString("x2"), 16);
                state.Voltage = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[11].ToString("x2") + buff[12].ToString("x2") + buff[13].ToString("x2") + buff[14].ToString("x2"), 16);
                state.Current = Convert.ToSingle(temp) / 1000;
            }

            return state;
        }
    }
}
