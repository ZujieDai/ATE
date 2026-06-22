using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Ink;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-交流BMS
    /// </summary>
    public class emtBMS_AC : EquipMentBase
    {
        private static object SynLockBMS = new object();
        public BMS_AC_StateData StateData = new BMS_AC_StateData();
        public emtBMS_AC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流导引BMS");
        }


        /// <summary>
        /// BMS协议
        /// </summary>
        BMS_Protocol procotol = new BMS_Protocol();


        /// <summary>
        /// 三标导引切换
        /// </summary>
        /// <param name="type">充电桩类型</param>
        public override void BMSSetHCAC(EmChargerType type)
        {
            AutoReadData = false;
            byte[] WriteBuffer = procotol.BMSSetHCAC(type);
            SendData(WriteBuffer);
            Thread.Sleep(50);
            AutoReadData = true;
        }



        public override void BMS_OFF()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(false);// 停止充电
            SendData(WriteBuffer);
        }
        public override void BMS_ON()
        {
            // byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(true, false, true, true, false, true, false, false); //false S2开关闭合,交流A枪开启    //true CP闭合， BIT9 电子锁控制解锁 false
            byte[] WriteBuffer = procotol.BMSSetONOFF(true);// 启动充电

            SendData(WriteBuffer);
        }
        public override void BMS_SetResistance(UInt16 tR2, UInt16 tR3)
        {
            byte[] WriteBuffer = procotol.GBResistanceSend(tR2, tR3);
            SendData(WriteBuffer);
        }
        public override void BMS_SetS2State(bool bs)
        {
            byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(bs, false, true, true, false, true, false, false); //false S2开关闭合,交流A枪开启    //true CP闭合， BIT9 电子锁控制解锁 false
            SendData(WriteBuffer);
        }

        public override void BMS_SetKState(List<bool> bs)
        {
            byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(bs); //false S2开关闭合,交流A枪开启    //true CP闭合， BIT9 电子锁控制解锁 false
            SendData(WriteBuffer);
        }

        public override List<bool> BMS_GetKState()
        {
            AutoReadData = false;
            Thread.Sleep(300);
            List<bool> result = new List<bool>();

            byte[] WriteBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x46, 0x39 };
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
            EquipMentPort.SendData(WriteBuffer);
            byte[] RevMsgData = RevEquipMentData();
            result = procotol.ParseKState(RevMsgData);
            AutoReadData = true;

            return result;
        }

        public override void BMSClearEnergy()
        {
            AutoReadData = false;
            Thread.Sleep(300);

            byte[] WriteBuffer = procotol.BMSClearEnergy_AC();
            EquipMentPort.SendData(WriteBuffer);

            AutoReadData = true;
        }

        public override double BMSGetEnergy()
        {
            double electricenergy = 0;
            lock (SynLockBMS)
            {
                AutoReadData = false;
                Thread.Sleep(300);
                try
                {
                    byte[] WriteBuffer = procotol.BMSGetEnergy_AC();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    SendMsgToFile("交流BMS发送读电量指令：" + strTemp);
                    RevEquipMentData();
                    EquipMentPort.SendData(WriteBuffer);
                    Thread.Sleep(300);
                    byte[] RevMsgData = RevEquipMentData();
                    if (RevMsgData == null)
                    {
                        SendMsgToFile("交流BMS读取电量失败，BMS没返回数据,再次发送读电量指令！");

                        strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        SendMsgToFile("交流BMS第二次发送读电量指令：" + strTemp);
                        RevEquipMentData();
                        EquipMentPort.SendData(WriteBuffer);
                        Thread.Sleep(300);
                        RevMsgData = RevEquipMentData();
                    }

                    if (RevMsgData != null && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                    {
                        strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                        //是返回的电量报文
                        if (RevMsgData[4] == 0x25 && RevMsgData.Length >= 56)
                        {

                            SendMsgToFile("交流BMS收到电量数据：" + strTemp);
                            List<byte> getBuffer = RevMsgData.ToList().GetRange(41, 4);
                            byte[] returnBuffer2 = ExChangeListByte_NoCRC(getBuffer);
                            string electricenergy1string = DecodeValue1000000(returnBuffer2);

                            electricenergy = Convert.ToDouble(electricenergy1string);
                        }
                        else
                        {
                            SendMsgToFile("交流BMS读取电量失败，BMS返回错误数据！");
                            electricenergy = -999;
                        }
                    }
                    else
                    {
                        SendMsgToFile("交流BMS读取电量失败，BMS没返回数据！");
                        electricenergy = -999;
                    }

                }
                catch (Exception ex) { Log.Log.LogException(ex); }
                AutoReadData = true;
            }
            return electricenergy;
        }

        public override void BMSGetTempRH(out double Temp, out double RH)
        {
            lock (SynLockBMS)
            {
                List<bool> result = new List<bool>();
                Temp = 0;
                RH = 0;

                AutoReadData = false;
                Thread.Sleep(200);

                byte[] WriteBuffer = procotol.BMSRead(0x01, 0x6d);
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                RevEquipMentData();
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(200);
                byte[] RevMsgData = RevEquipMentData();
                if (RevMsgData != null && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                {
                    Temp = ((RevMsgData[8] << 0) | (RevMsgData[7] << 8) | (RevMsgData[6] << 16) | (RevMsgData[5] << 24)) / 1000.0;
                    RH = ((RevMsgData[12] << 0) | (RevMsgData[11] << 8) | (RevMsgData[10] << 16) | (RevMsgData[9] << 24)) / 1000.0;
                }
                AutoReadData = true;
            }
        }

        public Byte[] ExChangeListByte_NoCRC(List<byte> ReturnbyteSource)
        {
            try
            {
                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验

                return writeBuff;
            }
            catch
            {
                return null;
            }
        }


        public string DecodeValue1000000(byte[] buffer)
        {
            int tmp = Convert.ToInt32(buffer[0].ToString("x2") + buffer[1].ToString("x2") + buffer[2].ToString("x2") + buffer[3].ToString("x2"), 16);
            double electricenergy = ((double)tmp / (double)1000000);
            return electricenergy.ToString("F6");
        }


        public override void ReadBMS_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicBMS_AC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicBMS_AC_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {

                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = procotol.BMSReadData(EmChargerType.Charger_GB_AC);
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
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                }
                                StateData = procotol.GetBMS_AC_StateData(RevMsgData, this.ChargerID);
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                                break;
                            }
                            else
                            {
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                StateData = new BMS_AC_StateData();
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
                        StateData = new BMS_AC_StateData();
                        StateData.ChargerID = this.ChargerID;
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(StateData);
                    }
                    Thread.Sleep(300);
                }
                Thread.Sleep(5);
            }
        }

        public override void BMSGetVersion(out string SoftwareVersion, out string FlowNumber)
        {
            AutoReadData = false;
            Thread.Sleep(1000);

            SoftwareVersion = "";
            FlowNumber = "";
            byte[] WriteBuffer = procotol.BMSRead(0x01, 0x03);
            //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
            EquipMentPort.SendData(WriteBuffer);
            //Thread.Sleep(100);
            byte[] RevMsgData = RevEquipMentData();
            int index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
            if (index >= 0 && RevMsgData.Length > index + 16 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0x03)
                RevMsgData = RevMsgData.Skip(index).Take(16).ToArray();
            if (RevMsgData != null)
            {
                for (int i = 0; i < 30; i++)
                {
                    if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                    {
                        EquipMentPort.SendData(WriteBuffer);
                        //Thread.Sleep(1000);
                        RevMsgData = RevEquipMentData();
                        index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
                        if (index >= 0 && RevMsgData.Length > index + 16 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0x03)
                        {
                            RevMsgData = RevMsgData.Skip(index).Take(16).ToArray();
                            break;
                        }
                    }
                    else
                        break;
                    Thread.Sleep(200);
                }
                if (RevMsgData != null && RevMsgData.Length > 12 && RevMsgData[3] == 0x41 && RevMsgData[4] == 0x03)
                {
                    procotol.GetVersion(RevMsgData, out SoftwareVersion, out FlowNumber);
                }
            }
            AutoReadData = true;
        }
    }
}
