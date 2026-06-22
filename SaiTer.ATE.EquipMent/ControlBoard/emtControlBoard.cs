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
using System.IO.Ports;
using System.Configuration;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-程控板
    /// </summary>
    public class emtControlBoard : EquipMentBase
    {
        public static object Locker = new object();
        public emtControlBoard(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("程控板");
        }

        public override void ControlResistanceSetRelay(List<bool> lstConditionState)
        {
            //byte[] buffer = GetBuffer(lstConditionState);
            //SendData(buffer);

            //先全部关闭
            List<bool> lst_tmp = new List<bool>();
            for(int i=0; i<lstConditionState.Count; i++)
            {
                if (i < 4)//4以下的都是控制的灯
                {
                    lst_tmp.Add(lstConditionState[i]);
                }
                else
                {
                    lst_tmp.Add(false);
                }
            }
            byte[] buffer = GetBuffer(lstConditionState);
            SendData(buffer);

            //再开启单个开关
            for (int i = 0; i < lstConditionState.Count; i++)
            {
                if (i > 3)//只控制4以上的继电器
                {
                    if (lstConditionState[i])//只有打开的开关执行打开动作
                    {
                        buffer = SetSwich(i, lstConditionState[i]);
                        SendData(buffer);
                    }
                }
            }
        }

        public override List<bool> ControlBoardReadState()
        {
            AutoReadData = false;
            byte[] buffer = { 0x01, 0x03, 0x00, 0x05, 0x00, 0x01, 0x94, 0x0B };
            List<bool> result = new List<bool>();
            for (int i = 0; i < 16; i++)
            {
                result.Add(false);
            }
            lock (Locker)
            {
                Thread.Sleep(100);
                EquipMentPort.SendData(buffer);
                byte[] RevMsgData = RevEquipMentData();
                if (RevMsgData == null)
                {
                    Thread.Sleep(100);
                    EquipMentPort.SendData(buffer);
                    RevMsgData = RevEquipMentData();
                }
                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                {
                    byte x1 = RevMsgData[3];//16-9
                    byte x2 = RevMsgData[4];//8-1

                    for (int i = 8; i < 16; i++)
                    {
                        byte y = Convert.ToByte(Math.Pow(2, i - 8));
                        if ((x1 | y) == x1)
                        {
                            result[i] = true;
                        }
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        byte y = Convert.ToByte(Math.Pow(2, i));
                        if ((x2 | y) == x2)
                        {
                            result[i] = true;
                        }
                    }
                }
                else
                    SendMsgToFile(EquipMentName + "通道数据读取失败");
            }
            AutoReadData = true;
            return result;
        }


        public override void SetLightColor(EmLightColor color)
        {
            //List<bool> lstConditionState = new List<bool>();
            //for (int i = 0; i < 16; i++)
            //{
            //    lstConditionState.Add(false);
            //}
            List<bool> lstConditionState = ControlBoardReadState();
            switch (color)
            {
                case EmLightColor.Yellow:
                    lstConditionState[0] = true;
                    lstConditionState[1] = false;
                    lstConditionState[2] = false;
                    lstConditionState[3] = false;
                    break;
                case EmLightColor.Green:
                    lstConditionState[0] = false;
                    lstConditionState[1] = true;
                    lstConditionState[2] = false;
                    lstConditionState[3] = false;
                    break;
                case EmLightColor.Red:
                    lstConditionState[0] = false;
                    lstConditionState[1] = false;
                    lstConditionState[2] = true;
                    string isAlarm = ConfigurationManager.AppSettings["isAlarm"];

                    if (isAlarm != null && isAlarm.ToUpper().Equals("TRUE"))
                    {
                        //亮红灯时同时蜂鸣报警
                        lstConditionState[3] = true;
                    }

                    break;
                case EmLightColor.Alarm:
                    lstConditionState[3] = true;
                    break;
                default:
                    lstConditionState[0] = false;
                    lstConditionState[1] = false;
                    lstConditionState[2] = false;
                    lstConditionState[3] = false;
                    break;
            }
            byte[] buffer = GetBuffer(lstConditionState);
            SendData(buffer);
            Thread.Sleep(500);
        }

        private byte[] GetBuffer(List<bool> lstConditionState)
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x06, 0x00, 0x05 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            Byte BYTE0 = 0;
            Byte BYTE1 = 0;
            for (int i = 0; i < 8; i++)
            {
                byte x = Convert.ToByte(Math.Pow(2, i));
                if (lstConditionState[i])
                {
                    BYTE0 = Convert.ToByte(x | BYTE0);
                }
            }
            for (int i = 8; i < 16; i++)
            {
                byte x = Convert.ToByte(Math.Pow(2, i - 8));
                if (lstConditionState[i])
                {
                    BYTE1 = Convert.ToByte(x | BYTE1);
                }
            }
            ReturnbyteSource.Add(BYTE1);//0000  0000
            ReturnbyteSource.Add(BYTE0);//0000  0000


            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。 
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;


        }

        private byte[] SetSwich(int iindex,bool bState)
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x06};//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            int iJcq = iindex + 0x200;
            ReturnbyteSource.Add((byte)(iJcq >> 8));
            ReturnbyteSource.Add((byte)(iJcq));
            ReturnbyteSource.Add(0x00);
            if(bState)
            {
                ReturnbyteSource.Add(0x01);
            }
            else
            {
                ReturnbyteSource.Add(0x00);
            }

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。 
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            lock (Locker)
            {
                Thread.Sleep(100);
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        //Thread.Sleep(80);//施加间隔
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                        byte[] RevMsgData = RevEquipMentData();
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
        }

        public byte[] ControlResistanceSetRelay(string tS8Condition, string tS7Condition, string tS6Condition, string tS5Condition, string tS4Condition, string tS3Condition, string tS2Condition, string tS1Condition)
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x06, 0x00, 0x05 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            Byte BYTE0 = 0;

            if (tS8Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x80 | BYTE0);
            }
            if (tS7Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x40 | BYTE0);
            }
            if (tS6Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x20 | BYTE0);
            }
            if (tS5Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x10 | BYTE0);
            }
            if (tS4Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x08 | BYTE0);
            }
            if (tS3Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x04 | BYTE0);
            }
            if (tS2Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x02 | BYTE0);
            }
            if (tS1Condition == "闭合")
            {
                BYTE0 = Convert.ToByte(0x01 | BYTE0);
            }
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(BYTE0);//0000  0000


            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。 
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;


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
                                byte[] buffer = { 0x01, 0x03, 0x00, 0x05, 0x00, 0x01, 0x94, 0x0B };
                                lock (Locker)
                                {
                                    Thread.Sleep(100);
                                    if (AutoReadData)
                                    {
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
                                    if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        lock (Locker)
                                        {
                                            Thread.Sleep(100);
                                            if (AutoReadData)
                                            {
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
                                    {
                                        SystemEvent.SendConnectState(true, this);
                                        return;//刷新了状态后就不循环读取了
                                    }
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
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            //通过串口连接状态判断
            //while (true)
            //{
            //    if (EquipMentPort != null)
            //    {
            //        SaiTer.ATE.PortManage.PortType.SerialPort sp = EquipMentPort as SaiTer.ATE.PortManage.PortType.SerialPort;
            //        if (sp != null && sp._SerialPort.IsOpen)
            //        {
            //            SystemEvent.SendConnectState(true, this);
            //        }
            //        else
            //        {
            //            SystemEvent.SendConnectState(false, this);
            //        }
            //    }
            //    else
            //    {
            //        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
            //        SystemEvent.SendConnectState(false, this);
            //    }
            //    Thread.Sleep(300);
            //}
        }

        public static byte[] RemoveTrailingZeros(byte[] input)
        {
            if (input == null || input.Length == 0)
                return input;

            int nonZeroIndex = input.Length - 1;
            while (nonZeroIndex >= 0 && input[nonZeroIndex] == 0)
            {
                nonZeroIndex--;
            }

            // nonZeroIndex 现在指向最后一个非零元素的索引（或-1，如果数组全为零）
            // 截取从数组开始到 nonZeroIndex + 1（包含）的所有元素
            return input.Take(nonZeroIndex + 1).ToArray();
        }
    }
}

