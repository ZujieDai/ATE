using SaiTer.ATE.DataModel.EnumModel;
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
    /// <summary>
    /// 设备 - 美标直流BMS导引
    /// </summary>
    public class emtBMS_USA_DC : EquipMentBase
    {
        private static object SynLockBMS = new object();
        public BMS_USA_DC_StateData bmsStateData = new BMS_USA_DC_StateData();
        public emtBMS_USA_DC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("美标直流导引BMS");
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
            byte[] WriteBuffer = procotol.BMSSetType_EU_USA(type);
            SendMsgToFile("切换设备类型报文：" + BitConverter.ToString(WriteBuffer));
            lock (SynLockBMS)
            {
                SendData(WriteBuffer);
                Thread.Sleep(50);
            }
            AutoReadData = true;
        }

        /// <summary>
        /// 赛特BMS设置需求参数
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="type">true - 恒压  false - 恒流</param>
        /// <param name="bmsCurrent">充电电压测量值</param>
        public override void BMSSetParameter(Double bmsVolt, Double bmsCurrent, bool type, double measureVoltcanCharge, bool canCharge = true, InsulationState insulationState = InsulationState.正常)
        {
            //bmsCurrent = bmsCurrent >= 250 ? 250 : bmsCurrent;
            //byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, bmsCurrent, type, measureVolt);
            byte[] WriteBuffer = procotol.BMSSetPara2_EU_DC(100, 85, bmsVolt, bmsCurrent, bmsVolt * 0.8);
            SendData(WriteBuffer);
        }

        // 国标专用
        //public override void BMSSetParameter(Double bmsVolt)
        //{

        //    byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt);
        //    SendData(WriteBuffer);
        //}

        public override void BMSSetParameter(Double bmsVolt, Double maxVolt, double maxCurrent)
        {
            //bmsCurrent = bmsCurrent >= 250 ? 250 : bmsCurrent;
            //byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, bmsCurrent, measureVolt);
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(15, maxCurrent, maxVolt);   //默认值为15
            SendData(WriteBuffer);
        }
        public override void BMSSetPara1_EU_DC(List<int> para1, double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(para1, RESS_SoC, MaxCurrent, MaxVoltage);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara1_EU_DC(double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(RESS_SoC, MaxCurrent, MaxVoltage);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara2_EU_DC(double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt)
        {
            byte[] WriteBuffer = procotol.BMSSetPara2_EU_DC(FullSOC, BulkSOC, TargetVolt, TargetCurrent, ReadyVolt);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara3_EU_DC(double FullSOCRemainTime, double BulkSOCRemainTime)
        {
            byte[] WriteBuffer = procotol.BMSSetPara3_EU_DC(FullSOCRemainTime, BulkSOCRemainTime);
            SendData(WriteBuffer);
        }

        public override List<double> BMSGetParameter_EU_DC(byte tComm)
        {
            List<double> result = new List<double>();
            AutoReadData = false;

            byte[] WriteBuffer = procotol.ParameterCommand_EU_DC(tComm);
            WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();

                if (CheckOut.CheckCrc_EU(RevMsgData))
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    result = procotol.ParameterRead_EU_DC(tComm, RevMsgData);
                }

            }
            AutoReadData = true;
            return result;
        }

        public override void BMSSetBatteryVoltage(double batteryVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetBatteryVoltage(batteryVoltage);
            SendData(WriteBuffer);
        }

        public override void BMS_GetResistance(ref ushort R2, ref ushort R3)
        {
            byte[] RevMsgData = null;
            byte[] WriteBuffer = procotol.BMS_GetResistance();
            if (EquipMentPort != null)
            {
                WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);
                EquipMentPort.SendData(WriteBuffer);
                RevMsgData = RevEquipMentData();

                if (RevMsgData != null)
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    procotol.BMS_GetResistance(RevMsgData, ref R2, ref R3);
                }
            }
        }

        public override void BMS_SetResistance(UInt16 tR2, UInt16 tR3)
        {
            byte[] WriteBuffer = procotol.BMS_SetResistance(tR2, tR3);
            SendData(WriteBuffer);
        }

        //public override void BMSSetKState_DC(double resistance, double batteryVoltage, bool[] bs)
        //{
        //    byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(resistance, batteryVoltage, bs);
        //    SendData(WriteBuffer);
        //}

        public override void BMSSetKState_EU_DC(double batteryVoltage, bool[] bs, int DCPlus, int DCMinus, string reserved)
        {
            byte[] WriteBuffer = procotol.EUExchangeSetStrAllSend(batteryVoltage, bs, DCPlus, DCMinus, reserved);
            SendData(WriteBuffer);
        }

        public override List<int> BMSGetKState_EU_DC(out double BMSVolatage)
        {
            List<int> result = new List<int>();
            BMSVolatage = 0;
            AutoReadData = false;
            byte[] WriteBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x9a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x40, 0xBB };
            WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();

                if (RevMsgData != null)
                {
                    string strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                    SendMsgToFile("赛特直流BMS收到数据：" + strTemp);
                }
                if (CheckOut.CheckCrc_EU(RevMsgData))
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    string strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                    SendMsgToFile("赛特直流BMS，CRC校验通过：" + strTemp);
                    result = procotol.ParseKState_EU_DC(RevMsgData, out BMSVolatage);
                }
            }

            AutoReadData = true;
            return result;
        }

        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            if (EquipMentPort != null)
            {
                WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);   //使用欧标的命令格式
                for (int i = 0; i < ReConnNum; i++)
                {
                    lock (SynLockBMS)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        //SendMsgToFile("赛特直流BMS发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                        byte[] RevMsgData = RevEquipMentData();

                        if (RevMsgData != null)
                        {
                            //if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                            if (CheckOut.CheckCrc_EU(RevMsgData) == false)      //欧标的CRC校验不一样
                            {
                                EquipMentPort.SendData(WriteBuffer);
                            }
                        }
                        else
                        {
                            SendMsgToFile("赛特直流BMS设置参数信息失败，BMS没返回数据！");
                            continue;
                        }
                    }
                }
                AutoReadData = true;
                return true;
            }
            else
            {
                SendMsgToFile("赛特直流BMS通道对象不存在，请检查赛特直流BMS通道");
                AutoReadData = true;
                return false;
            }
        }

        public override void BMS_OFF()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(false);// 停止充电
            SendData(WriteBuffer);
        }
        public override void BMS_ON()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(true);// 开始充电
            SendData(WriteBuffer);
        }

        public override void ReadBMS_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicBMS_USA_DC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicBMS_USA_DC_StateData.Add(ChargerID, bmsStateData);
            }
            try
            {
                while (true)
                {

                    if (AutoReadData)
                    {

                        byte[] RevMsgData = null;
                        byte[] WriteBuffer = procotol.BMSReadData(EmChargerType.Charger_EUR_DC);
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                lock (SynLockBMS)
                                {
                                    //DataBuf.Clear();//清空函数经常报索引越界错误
                                    RevEquipMentData();
                                    Thread.Sleep(20);
                                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                    WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);
                                    //SendMsgToFile("赛特直流BMS发送数据：" + strTemp);
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (RevMsgData != null)
                                {
                                    //TCP通讯会填满256个字节
                                    if (RevMsgData.Length > 64)
                                        RevMsgData = RevMsgData.Take(64).ToArray();
                                    //if (!CheckCrc(RevMsgData))
                                    if (!CheckOut.CheckCrc_EU(RevMsgData))
                                    {
                                        bmsStateData = new BMS_USA_DC_StateData();
                                        bmsStateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(bmsStateData);
                                        SystemEvent.SendConnectState(false, this);
                                        continue;
                                    }
                                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                                    bmsStateData = procotol.GetBMS_USA_DC_StateData(RevMsgData, this.ChargerID);
                                    SystemEvent.SendMonitorMessage(bmsStateData);
                                    SystemEvent.SendConnectState(true, this);
                                }
                                else
                                {
                                    // SendMsgToFile("读赛特直流BMS失败，BMS没返回数据！");
                                    bmsStateData = new BMS_USA_DC_StateData();
                                    bmsStateData.ChargerID = this.ChargerID;
                                    SystemEvent.SendMonitorMessage(bmsStateData);
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }

                            }
                        }
                        else
                        {
                            SendMsgToFile("赛特直流BMS通道对象不存在，请检查赛特直流BMS通道");
                            bmsStateData = new BMS_USA_DC_StateData();
                            bmsStateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(bmsStateData);
                        }

                        Thread.Sleep(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
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
            lock (SynLockBMS)
            {
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
            }
            AutoReadData = true;
        }

        //只有国标和日标支持
        //public override void BMS_DC_SetControl(byte tComm, bool tisTrue)
        //{
        //    //直流CC1通断控制 下发0x88设置（桩模拟器R3/车模拟器R4）
        //    //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
        //    //直流开关S通断控制 下发0x8A设置（桩模拟器R3/车模拟器R4）

        //    //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
        //    //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
        //    //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
        //    //充电桩报文设置读取 下发0x50设置 ）True X4 = 01，代表设置启动BMS报文数据读取; false X4 = 00，代表关闭BMS报文数据读取；

        //    byte[] writeBuffer = procotol.BMSSetControl(tComm, tisTrue);
        //    SendData(writeBuffer);
        //}

    }
    /// <summary>
    /// 设备 - 欧标直流BMS导引
    /// </summary>
    public class emtBMS_USA_DC111 : EquipMentBase
    {
        private static object SynLockBMS = new object();
        public BMS_EU_DC_StateData bmsStateData = new BMS_EU_DC_StateData();
        public emtBMS_USA_DC111(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("美标直流导引BMS");
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
            lock (SynLockBMS)
            {
                SendData(WriteBuffer);
                Thread.Sleep(50);
            }
            AutoReadData = true;
        }

        /// <summary>
        /// 赛特BMS设置需求参数
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="type">true - 恒压  false - 恒流</param>
        /// <param name="bmsCurrent">充电电压测量值</param>
        public override void BMSSetParameter(Double bmsVolt, Double bmsCurrent, bool type, double measureVolt, bool canCharge = true, InsulationState insulationState = InsulationState.正常)
        {
            //bmsCurrent = bmsCurrent >= 250 ? 250 : bmsCurrent;
            //byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, bmsCurrent, type, measureVolt);
            byte[] WriteBuffer = procotol.BMSSetPara2_EU_DC(100, 85, bmsVolt, bmsCurrent, bmsVolt * 0.8);
            SendData(WriteBuffer);
        }

        //public override void BMSSetParameter(Double bmsVolt)
        //{

        //    byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt);
        //    SendData(WriteBuffer);
        //}

        public override void BMSSetParameter(Double bmsVolt, Double maxVolt, double maxCurrent)
        {
            //bmsCurrent = bmsCurrent >= 250 ? 250 : bmsCurrent;
            //byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, bmsCurrent, measureVolt);
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(15, maxCurrent, maxVolt);   //默认值为15
            SendData(WriteBuffer);
        }
        public override void BMSSetPara1_EU_DC(List<int> para1, double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(para1, RESS_SoC, MaxCurrent, MaxVoltage);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara1_EU_DC(double RESS_SoC, double MaxCurrent, double MaxVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetPara1_EU_DC(RESS_SoC, MaxCurrent, MaxVoltage);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara2_EU_DC(double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt)
        {
            byte[] WriteBuffer = procotol.BMSSetPara2_EU_DC(FullSOC, BulkSOC, TargetVolt, TargetCurrent, ReadyVolt);
            SendData(WriteBuffer);
        }
        public override void BMSSetPara3_EU_DC(double FullSOCRemainTime, double BulkSOCRemainTime)
        {
            byte[] WriteBuffer = procotol.BMSSetPara3_EU_DC(FullSOCRemainTime, BulkSOCRemainTime);
            SendData(WriteBuffer);
        }

        public override List<double> BMSGetParameter_EU_DC(byte tComm)
        {
            List<double> result = new List<double>();
            AutoReadData = false;

            byte[] WriteBuffer = procotol.ParameterCommand_EU_DC(tComm);
            WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();

                if (CheckOut.CheckCrc_EU(RevMsgData))
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    result = procotol.ParameterRead_EU_DC(tComm, RevMsgData);
                }

            }
            AutoReadData = true;
            return result;
        }

        public override void BMSSetBatteryVoltage(double batteryVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetBatteryVoltage(batteryVoltage);
            SendData(WriteBuffer);
        }

        public override void BMS_GetResistance(ref ushort R2, ref ushort R3)
        {
            byte[] RevMsgData = null;
            byte[] WriteBuffer = procotol.BMS_GetResistance();
            if (EquipMentPort != null)
            {
                WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);
                EquipMentPort.SendData(WriteBuffer);
                RevMsgData = RevEquipMentData();

                if (RevMsgData != null)
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    procotol.BMS_GetResistance(RevMsgData, ref R2, ref R3);
                }
            }
        }

        public override void BMS_SetResistance(UInt16 tR2, UInt16 tR3)
        {
            byte[] WriteBuffer = procotol.BMS_SetResistance(tR2, tR3);
            SendData(WriteBuffer);
        }

        //只有国标有
        //public override void BMSSetKState_DC(double resistance, double batteryVoltage, bool[] bs)
        //{
        //    byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(resistance, batteryVoltage, bs);
        //    SendData(WriteBuffer);
        //}

        public override void BMSSetKState_EU_DC(double batteryVoltage, bool[] bs, int DCPlus, int DCMinus, string reserved)
        {
            byte[] WriteBuffer = procotol.EUExchangeSetStrAllSend(batteryVoltage, bs, DCPlus, DCMinus, reserved);
            SendData(WriteBuffer);
        }

        public override List<int> BMSGetKState_EU_DC(out double BMSVolatage)
        {
            List<int> result = new List<int>();
            BMSVolatage = 0;
            AutoReadData = false;
            byte[] WriteBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x9a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x40, 0xBB };
            WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();

                if (CheckOut.CheckCrc_EU(RevMsgData))
                {
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    result = procotol.ParseKState_EU_DC(RevMsgData, out BMSVolatage);
                }
            }

            AutoReadData = true;
            return result;
        }

        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            if (EquipMentPort != null)
            {
                WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);   //使用欧标的命令格式
                for (int i = 0; i < ReConnNum; i++)
                {
                    lock (SynLockBMS)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        //SendMsgToFile("赛特直流BMS发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                        byte[] RevMsgData = RevEquipMentData();

                        if (RevMsgData != null)
                        {
                            //if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                            if (CheckOut.CheckCrc_EU(RevMsgData) == false)      //欧标的CRC校验不一样
                            {
                                EquipMentPort.SendData(WriteBuffer);
                            }
                        }
                        else
                        {
                            SendMsgToFile("赛特直流BMS设置参数信息失败，BMS没返回数据！");
                            continue;
                        }
                    }
                }
                AutoReadData = true;
                return true;
            }
            else
            {
                SendMsgToFile("赛特直流BMS通道对象不存在，请检查赛特直流BMS通道");
                AutoReadData = true;
                return false;
            }
        }

        public override void BMS_OFF()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(false);// 停止充电
            SendData(WriteBuffer);
        }
        public override void BMS_ON()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(true);// 开始充电
            SendData(WriteBuffer);
        }

        public override void BMSGetVersion(out string SoftwareVersion, out string FlowNumber)
        {
            AutoReadData = false;
            Thread.Sleep(1000);

            SoftwareVersion = "";
            FlowNumber = "";
            byte[] WriteBuffer = procotol.BMSRead(0x01, 0x03);
            //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                WriteBuffer = CheckOut.ToASCII_EU(WriteBuffer);
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
                        if (!CheckOut.CheckCrc_EU(RevMsgData))
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
                    RevMsgData = CheckOut.Array_CharToHex(RevMsgData);
                    if (RevMsgData != null && RevMsgData.Length > 12 && RevMsgData[3] == 0x41 && RevMsgData[4] == 0x03)
                    {
                        procotol.GetVersion(RevMsgData, out SoftwareVersion, out FlowNumber);
                    }
                }
            }
            AutoReadData = true;
        }

        //只有国标和日标有
        //public override void BMS_DC_SetControl(byte tComm, bool tisTrue)
        //{
        //    //直流CC1通断控制 下发0x88设置（桩模拟器R3/车模拟器R4）
        //    //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
        //    //直流开关S通断控制 下发0x8A设置（桩模拟器R3/车模拟器R4）

        //    //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
        //    //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
        //    //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
        //    //充电桩报文设置读取 下发0x50设置 ）True X4 = 01，代表设置启动BMS报文数据读取; false X4 = 00，代表关闭BMS报文数据读取；

        //    byte[] writeBuffer = procotol.BMSSetControl(tComm, tisTrue);
        //    SendData(writeBuffer);
        //}

    }
}
