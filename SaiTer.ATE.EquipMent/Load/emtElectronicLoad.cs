using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 直流电子负载
    /// </summary>
    public class emtElectronicLoad : EquipMentBase
    {
        private static object locker = new object();
        private ElectronicLoad_StateData StateData = new ElectronicLoad_StateData();
        public emtElectronicLoad(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("电子负载");
        }

        private byte[] WriteBuffer = { 0xAA, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09 };//包括CRC

        public override void ElectronicLoad_OFF()
        {
            byte[] buff = ElectronicLoadSet(0x21, 0x00);
            SendData(buff);
        }

        public override void ElectronicLoad_ON()
        {
            byte[] buff = ElectronicLoadSet(0x21, 0x01);
            SendData(buff);
        }

        public override void SetElectronicLoadParams(byte tCom, byte tOperate)
        {
            byte[] buff = ElectronicLoadSet(tCom, tOperate);
            SendData(buff);
        }

        public override void SetElectronicLoadParams(byte tCom, uint tOperate)
        {
            byte[] buff = ElectronicLoadSet(tCom, tOperate);
            SendData(buff);
        }

        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            Thread.Sleep(500);
            //lock (locker)
            {
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        SendMsgToFile("电子负载发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                        Thread.Sleep(300);  //防止死机
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile("电子负载通道对象不存在，请检查交流源通道");
                    AutoReadData = true;
                    return false;
                }
            }
        }

        public override void ReadElectronicLoad_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicElectronicLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicElectronicLoad_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                if (AutoReadData)
                {
                    lock (locker)
                    {
                        if (AutoReadData)
                        {
                            byte[] RevMsgData = null;
                            if (EquipMentPort != null)
                            {
                                for (int i = 0; i < ReConnNum; i++)
                                {
                                    DataBuf.Clear();
                                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                    //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();

                                    if (RevMsgData != null)
                                    {
                                        StateData = GetStateData(RevMsgData);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                        //if (RevMsgData != null && CheckOut.CheckElectronicLoadCrc(RevMsgData))
                                        //{
                                        //    StateData = GetStateData(RevMsgData);
                                        //    SystemEvent.SendMonitorMessage(StateData);
                                        //    SystemEvent.SendConnectState(true, this);
                                        //}
                                        //else
                                        //{
                                        //    StateData = new ElectronicLoad_StateData();
                                        //    StateData.ChargerID = ChargerID;
                                        //    SystemEvent.SendMonitorMessage(StateData);
                                        //    SystemEvent.SendConnectState(false, this);
                                        //    continue;
                                        //}
                                    }
                                    else
                                    {
                                        StateData = new ElectronicLoad_StateData();
                                        StateData.ChargerID = ChargerID;
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(false, this);
                                        continue;
                                    }

                                }
                            }
                            else
                            {
                                SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                                StateData = new ElectronicLoad_StateData();
                                StateData.ChargerID = ChargerID;
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(false, this);

                            }
                            Thread.Sleep(300);
                        }
                    }
                }
            }
        }




        public byte[] ElectronicLoadSet(byte tCom, byte tOperate)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0xAA, 0x00 };//前缀

            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(tCom);//0000  0000
            ReturnbyteSource.Add(tOperate);//0000  0000
            for (int i = 0; i < 21; i++)
            {
                ReturnbyteSource.Add(0x00);//0000  0000
            }
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.ElectronicLoadCrc(writeBuff);//CRC校验函数。
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        public byte[] ElectronicLoadSet(byte tCom, UInt32 tOperate)
        {
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0xAA, 0x00 };//前缀

            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(tCom);//0000  0000
            ReturnbyteSource.Add((Byte)(tOperate));//0000  0000
            ReturnbyteSource.Add((Byte)(tOperate >> 8));//0000  0000
            ReturnbyteSource.Add((Byte)(tOperate >> 16));//0000  0000
            ReturnbyteSource.Add((Byte)(tOperate >> 24));//0000  0000
            for (int i = 0; i < 18; i++)
            {
                ReturnbyteSource.Add(0x00);//0000  0000
            }
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.ElectronicLoadCrc(writeBuff);//CRC校验函数。
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }



        public ElectronicLoad_StateData GetStateData(byte[] buff)
        {
            ElectronicLoad_StateData data = new ElectronicLoad_StateData();
            data.ChargerID = ChargerID;
            try
            {
                if (buff == null || buff.Length < 26)
                {
                    return data;
                }
                //第四到第七字节  实际输入电压值(低字节在前,高字节在后)
                int temp = Convert.ToInt32(buff[6].ToString("x2") + buff[5].ToString("x2") + buff[4].ToString("x2") + buff[3].ToString("x2"), 16);
                data.InputVoltage = Convert.ToSingle(temp) / 1000;
                data.InputCurrent = Convert.ToSingle(Convert.ToInt32(buff[10].ToString("x2") + buff[9].ToString("x2") + buff[8].ToString("x2") + buff[7].ToString("x2"), 16)) / 10000;
                data.InputPower = Convert.ToSingle(Convert.ToInt32(buff[14].ToString("x2") + buff[13].ToString("x2") + buff[12].ToString("x2") + buff[11].ToString("x2"), 16)) / 1000;


                if ((buff[15] & 0x04) == 0x04)
                {
                    data.OperateState1 = "远端控制模式";
                }
                else
                {
                    data.OperateState1 = "本地控制模式";
                }
                if ((buff[15] & 0x08) == 0x08)
                {
                    data.OperateState2 = "ON状态";
                }
                else
                {
                    data.OperateState2 = "OFF状态";
                }


                if ((buff[16] & 0x80) == 0x80)
                {
                    data.SystemState1 = "定电压";
                }
                else if ((buff[16] & 0x40) == 0x40)
                {
                    data.SystemState1 = "定电流";
                }
                else if ((buff[17] & 0x01) == 0x01)
                {
                    data.SystemState2 = "定功率";
                }
                else if ((buff[17] & 0x02) == 0x02)
                {
                    data.SystemState2 = "定电阻";
                }

                return data;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return data;
            }
        }
    }
}
