using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备 - 国标直流BMS导引
    /// </summary>
    public class emtBMS_GB_DC : EquipMentBase
    {
        ESGBDC_Ver eSGBDC_Ver;

        public int Iindex = 0;
        private object SynLockBMS = new object();
        public BMS_DC_StateData bmsStateData = new BMS_DC_StateData();
        private Stopwatch swCANStop = new Stopwatch();

        private AutoResetEvent waiter = new AutoResetEvent(true);
        string TestItemName = "null";
        string testTime = "";

        ProtocolParser parser = new ProtocolParser();

        /// <summary>
        /// BMS协议
        /// </summary>
        BMS_Protocol procotol = new BMS_Protocol();

        public emtBMS_GB_DC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("国标直流导引BMS");
            SystemEvent.CreateCANExcelNameEvent += SystemEventCreateCANExcelName;
            SystemEvent.CreateCANExcelEvent += SystemEventCreateCANExcel;
            // 获取BMS协议版本
            BMSProtocol_VersionMange bMSProtocol_VersionMange = new BMSProtocol_VersionMange();
            eSGBDC_Ver = bMSProtocol_VersionMange.SelectVersion();
            // 注册事件处理程序

            parser.CAN_DataReceived += Parser_CAN_DataReceived;
            SystemEvent.CanProtocolVersionSW += BMSControl_CanProtocolVersionSW;
        }

        #region --------------------BMS读写函数------------------------
        /// <summary>
        /// 三标导引切换
        /// </summary>
        /// <param name="type">充电桩类型</param>
        public override void BMSSetHCAC(EmChargerType type)
        {
            lock (SynLockBMS)
            {
                AutoReadData = false;
                byte[] WriteBuffer = procotol.BMSSetHCAC(type);
                SendBMSData(WriteBuffer);
                Thread.Sleep(100);
                AutoReadData = true;
            }
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
            byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, bmsCurrent, type, measureVolt, canCharge, insulationState);
            SendBMSData(WriteBuffer);
        }

        public override void BMSSetParameter(Double bmsVolt, double BatteryTotalVolt)
        {

            byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, BatteryTotalVolt);
            SendBMSData(WriteBuffer);
        }

        public override void BMSSetParameter(Double bmsVolt, Double measureVolt, double bmsCurrent)
        {
            //bmsCurrent = bmsCurrent >= 250 ? 250 : bmsCurrent;
            byte[] WriteBuffer = procotol.BMSSetPara(bmsVolt, measureVolt, bmsCurrent);
            SendBMSData(WriteBuffer);
        }
        public override void BMSSetBatteryVoltage(double batteryVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetBatteryVoltage(batteryVoltage);
            SendBMSData(WriteBuffer);
        }

        public override void BMSSetResistance(double resistance)
        {
            byte[] WriteBuffer = procotol.BMSSetResistance(resistance);
            SendBMSData(WriteBuffer);
        }

        public override void BMSReadCombine_DC(out bool isON)
        {
            lock (SynLockBMS)
            {
                AutoReadData = false;
                byte[] WriteBuffer = procotol.GBExchangeReadCombine();
                parser.OtherDataQueue.Clear();
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(200);
                byte[] RevMsgData = null;
                if (parser.OtherDataQueue.Count > 0)
                {
                    RevMsgData = parser.OtherDataQueue.Dequeue();
                }

                isON = procotol.ParseCombineKState_DC(RevMsgData);
                AutoReadData = true;
            }
        }

        public override void BMSSetCombine_DC(bool isON)
        {
            byte[] WriteBuffer = procotol.GBExchangeSetCombine(isON);
            SendBMSData(WriteBuffer);
        }

        public override void BMSSetKState_DC(double resistance, double batteryVoltage, bool[] bs)
        {
            byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(resistance, batteryVoltage, bs);
            SendBMSData(WriteBuffer);
        }

        public override void BMSReadLeakageResistance_DC(out int DCUpON, out int DCDownON, out double DCUpResistance, out double DCDownResistance)
        {
            lock (SynLockBMS)
            {
                AutoReadData = false;
                parser.IsPause = true;
                try
                {
                    Thread.Sleep(200);
                    byte[] WriteBuffer = procotol.GBReadLeakageResistance();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    SendMsgToFile("直流BMS发送读绝缘阻值指令：" + strTemp);
                    parser.ClearBuffer();
                    EquipMentPort.SendData(WriteBuffer);
                    Thread.Sleep(200);
                    byte[] RevMsgData = parser.GetBuffer().ToArray();
                    strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                    SendMsgToFile("直流BMS接收读绝缘阻值指令：" + strTemp);
                    procotol.ParseLeakageResistance(RevMsgData, out DCUpON, out DCDownON, out DCUpResistance, out DCDownResistance);
                }
                catch (Exception ex) { Log.Log.LogException(ex); DCUpON = 0; DCDownON = 0; DCUpResistance = 0; DCDownResistance = 0; }
                finally
                {
                    parser.IsPause = false;
                    AutoReadData = true;
                }
            }
        }

        public override void BMSSetLeakageResistance_DC(int DCUpON, int DCDownON, double DCUpResistance, double DCDownResistance)
        {
            byte[] WriteBuffer = procotol.GBSetLeakageResistance(DCUpON, DCDownON, DCUpResistance, DCDownResistance);
            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            SendMsgToFile("直流BMS发送设置绝缘阻值指令：" + strTemp);
            SendBMSData(WriteBuffer);
        }

        public override List<bool> BMSGetKState_DC(out double R4Resistance, out double BMSVolatage)
        {
            lock (SynLockBMS)
            {
                List<bool> result = new List<bool>();
                R4Resistance = 0;
                BMSVolatage = 0;


                AutoReadData = false;
                Thread.Sleep(200);

                byte[] WriteBuffer = new byte[] { 0x7e, 0x00, 0xff, 0x01, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x40, 0xBB };
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                parser.OtherDataQueue.Clear();
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(200);
                byte[] RevMsgData = null;
                if (parser.OtherDataQueue.Count > 0)
                {
                    RevMsgData = parser.OtherDataQueue.Dequeue();
                }

                result = procotol.ParseKState_DC(RevMsgData, out R4Resistance, out BMSVolatage);
                AutoReadData = true;

                return result;
            }
        }



        public override void BMS_OFF()
        {
            byte[] WriteBuffer = procotol.BMSSetONOFF(false);// 停止充电
            SendBMSData(WriteBuffer);
        }

        public override void BMS_ON()
        {
            byte[] WriteBuffer;
            WriteBuffer = procotol.BMSReadBCSUseReqOrRealValue();
            byte[] RevMsgData = SendBMSData(WriteBuffer);
            bool isRealValue = false;
            if (RevMsgData != null && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
            {
                isRealValue = RevMsgData[5] == 0x01 ? true : false;
            }
            if (!isRealValue)
            {
                WriteBuffer = procotol.BMSSetBCSUseReqOrRealValue(true); // 默认发实测值
                SendBMSData(WriteBuffer);
                Thread.Sleep(2000);
            }

            WriteBuffer = procotol.BMSSetONOFF(true);// 开始充电
            SendBMSData(WriteBuffer);
        }

        public override void BMSSetConstAndInspectionError(double ElecConstant, double InspecError)
        {
            byte[] WriteBuffer = procotol.BMSSetConstAndInspectionError(ElecConstant, InspecError);
            SendBMSData(WriteBuffer);
        }

        public override void BMSClearError()
        {
            byte[] WriteBuffer = procotol.BMSClearError();
            SendBMSData(WriteBuffer);
        }
        public override void BMSClearEnergy()
        {
            byte[] WriteBuffer = procotol.BMSClearEnergy();
            SendBMSData(WriteBuffer);
        }
        public override double BMSGetEnergy()
        {
            double electricenergy = 0;
            lock (SynLockBMS)
            {
                try
                {
                    byte[] WriteBuffer = procotol.BMSGetEnergy();
                    //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    //SendMsgToFile("直流BMS发送读电量指令：" + strTemp);
                    parser.OtherDataQueue.Clear();
                    EquipMentPort.SendData(WriteBuffer);
                    Thread.Sleep(300);
                    byte[] RevMsgData = null;
                    if (parser.OtherDataQueue.Count > 0)
                    {
                        RevMsgData = parser.OtherDataQueue.Dequeue();
                    }
                    //byte[] RevMsgData = DataBuf.Dequeue();
                    else
                    {
                        SendMsgToFile("直流BMS读取电量失败，BMS没返回数据,再次发送读电量指令！");

                        //strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        //SendMsgToFile("直流BMS第二次发送读电量指令：" + strTemp);
                        parser.OtherDataQueue.Clear();
                        EquipMentPort.SendData(WriteBuffer);
                        Thread.Sleep(300);
                        if (parser.OtherDataQueue.Count > 0)
                        {
                            RevMsgData = parser.OtherDataQueue.Dequeue();
                        }
                    }

                    if (RevMsgData != null && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                    {
                        //strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                        //是返回的电量报文
                        if (RevMsgData[4] == 0x64 && RevMsgData.Length >= 12)
                        {

                            //SendMsgToFile("直流BMS收到电量数据：" + strTemp);
                            List<byte> getBuffer = RevMsgData.ToList().GetRange(5, 4);
                            byte[] returnBuffer2 = ExChangeListByte_NoCRC(getBuffer);
                            string electricenergy1string = DecodeValue1000000(returnBuffer2);

                            electricenergy = Convert.ToDouble(electricenergy1string);
                        }
                        else
                        {
                            SendMsgToFile("直流BMS读取电量失败，BMS返回错误数据！");
                            electricenergy = -999;
                        }
                    }
                    else
                    {
                        SendMsgToFile("直流BMS读取电量失败，BMS没返回数据！");
                        electricenergy = -999;
                    }

                }
                catch (Exception ex) { Log.Log.LogException(ex); }


            }
            return electricenergy;
        }

        public override double[] BMSGetError(string ElectricConstant16, string InspectionError16)
        {
            double[] BMSError = new double[3];
            lock (SynLockBMS)
            {

                AutoReadData = false;

                try
                {
                    List<bool> result = new List<bool>();

                    byte[] WriteBuffer = procotol.BMSReadError(ElectricConstant16, InspectionError16);
                    EquipMentPort.SendData(WriteBuffer);
                    parser.OtherDataQueue.Clear();
                    Thread.Sleep(300);
                    byte[] RevMsgData = null;
                    if (parser.OtherDataQueue.Count > 0)
                    {
                        RevMsgData = parser.OtherDataQueue.Dequeue();
                    }
                    if (RevMsgData != null && CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                    {
                        List<byte> getBuffer = RevMsgData.ToList().GetRange(21, 4);
                        byte[] returnBuffer2 = ExChangeListByte_NoCRC(getBuffer);
                        string errorstring = DecodeValue1000000(returnBuffer2);
                        decimal error1 = Convert.ToDecimal(errorstring);

                        List<byte> getBuffer2 = RevMsgData.ToList().GetRange(25, 4);
                        byte[] returnBuffer3 = ExChangeListByte_NoCRC(getBuffer2);
                        string errorstring2 = DecodeValue1000000(returnBuffer3);
                        decimal error2 = Convert.ToDecimal(errorstring2);



                        getBuffer2 = RevMsgData.ToList().GetRange(29, 4);
                        returnBuffer3 = ExChangeListByte_NoCRC(getBuffer2);
                        errorstring2 = DecodeValue1000000(returnBuffer3);
                        decimal error3 = Convert.ToDecimal(errorstring2);
                        BMSError[0] = (double)error1;
                        BMSError[1] = (double)error2;
                        BMSError[2] = (double)error3;
                    }
                    AutoReadData = true;
                }
                catch (Exception ex) { Log.Log.LogException(ex); }

            }
            return BMSError;


        }

        public override void BMS_DC_SetControl(byte tComm, bool tisTrue)
        {
            //直流CC1通断控制 下发0x88设置（桩模拟器R3/车模拟器R4）
            //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
            //直流开关S通断控制 下发0x8A设置（桩模拟器R3/车模拟器R4）

            //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
            //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
            //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
            //充电桩报文设置读取 下发0x50设置 ）True X4 = 01，代表设置启动BMS报文数据读取; false X4 = 00，代表关闭BMS报文数据读取；

            byte[] writeBuffer = procotol.BMSSetControl(tComm, tisTrue);
            SendBMSData(writeBuffer);
        }

        public override void BMSGetVersion(out string SoftwareVersion, out string FlowNumber)
        {
            lock (SynLockBMS)
            {
                AutoReadData = false;
                Thread.Sleep(1000);

                SoftwareVersion = "";
                FlowNumber = "";
                byte[] WriteBuffer = procotol.BMSRead(0x01, 0x03);
                parser.OtherDataQueue.Clear();
                //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(300);

                byte[] RevMsgData = null;
                if (parser.OtherDataQueue.Count > 0)
                {
                    RevMsgData = parser.OtherDataQueue.Dequeue();
                }
                if (RevMsgData == null)
                {
                    AutoReadData = true;
                    return;
                }
                int index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
                if (index >= 0 && RevMsgData.Length > index + 16 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0x03)
                    RevMsgData = RevMsgData.Skip(index).Take(16).ToArray();
                if (RevMsgData != null)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                        {
                            parser.OtherDataQueue.Clear();
                            //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                            EquipMentPort.SendData(WriteBuffer);
                            Thread.Sleep(300);
                            //Thread.Sleep(1000);
                            if (parser.OtherDataQueue.Count > 0)
                            {
                                RevMsgData = parser.OtherDataQueue.Dequeue();
                            }
                            if (RevMsgData == null)
                            {
                                AutoReadData = true;
                                return;
                            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingCharging">所有的参数设置</param>
        public override void BMSSetALLParameter(SettingCharging settingCharging)
        {
            byte[] writeBuffer = procotol.BMSProtocolChargeData(settingCharging);
            SendBMSData(writeBuffer);
        }

        /// <summary>
        /// 获得DN2009测试项辅源停止时间，目前只有公牛下位机适配了这个功能
        /// </summary>
        /// <returns></returns>
        public override void GetK3K4StopTime(out int K3K4Time)
        {
            AutoReadData = false;
            Thread.Sleep(1000);
            K3K4Time = -999;

            byte[] RevMsgData = null;
            byte[] WriteBuffer = procotol.BMSGetK3K4Time();
            lock (SynLockBMS)
            {
                parser.OtherDataQueue.Clear();
                //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(100);

                if (parser.OtherDataQueue.Count > 0)
                {
                    RevMsgData = parser.OtherDataQueue.Dequeue();
                }
                if (RevMsgData == null)
                {
                    AutoReadData = true;
                    return;
                }
            }
            int index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
            if (index >= 0 && RevMsgData.Length > index + 4 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0x73)
                RevMsgData = RevMsgData.Skip(index).Take(12).ToArray();
            if (RevMsgData != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                    {
                        lock (SynLockBMS)
                        {
                            parser.OtherDataQueue.Clear();
                            //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                            EquipMentPort.SendData(WriteBuffer);
                            Thread.Sleep(300);
                            //Thread.Sleep(1000);
                            if (parser.OtherDataQueue.Count > 0)
                            {
                                RevMsgData = parser.OtherDataQueue.Dequeue();
                            }
                            if (RevMsgData == null)
                            {
                                AutoReadData = true;
                                return;
                            }
                        }
                        index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
                        if (index >= 0 && RevMsgData.Length > index + 12 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0x73)
                        {
                            RevMsgData = RevMsgData.Skip(index).Take(16).ToArray();
                            break;
                        }
                    }
                    else
                        break;
                    Thread.Sleep(200);
                }

                if (RevMsgData != null && RevMsgData.Length >= 12 && RevMsgData[3] == 0x41 && RevMsgData[4] == 0x73)
                {
                    procotol.GetK3K4Time(RevMsgData, out K3K4Time);
                }
            }
            else
            {

            }
            AutoReadData = true;
        }
        /// <summary>
        /// BMS 协议一致性测试设置
        /// </summary>
        /// <param name="byte0"></param>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="byte3"></param>
        /// <param name="byte4"></param>
        /// <param name="byte5"></param>
        /// <param name="byte6"></param>
        /// <param name="byte7"></param>
        public override void BMSProtocolConsistency(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)
        {
            byte[] writeBuffer = procotol.BMSProtocolConsistency(byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7);// BMS 协议一致性测试设置
            SendBMSData(writeBuffer);
        }


        private byte[] SendBMSData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            byte[] RevMsgData = null;
            lock (SynLockBMS)
            {
                Thread.Sleep(250);
                try
                {
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile("直流BMS发送数据：" + strTemp);
                            parser.OtherDataQueue.Clear();
                            EquipMentPort.SendData(WriteBuffer);
                            Thread.Sleep(300);
                            if (parser.OtherDataQueue.Count > 0)
                            {
                                RevMsgData = parser.OtherDataQueue.Dequeue();
                                break;
                            }
                            else
                            {
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    parser.OtherDataQueue.Clear();
                                    EquipMentPort.SendData(WriteBuffer);
                                    Thread.Sleep(300);
                                    if (parser.OtherDataQueue.Count > 0)
                                    {
                                        RevMsgData = parser.OtherDataQueue.Dequeue();
                                    }
                                    else
                                    {
                                        SendMsgToFile("直流BMS发送数据：" + strTemp);
                                        SendMsgToFile("直流BMS设置参数信息失败，BMS没返回数据！");
                                        continue;
                                    }
                                }
                                else
                                {
                                    strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                                    //SendMsgToFile("直流BMS收到数据：" + strTemp);
                                    break;
                                }
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile("赛特直流BMS通道对象不存在，请检查赛特直流BMS通道");
                    }
                }
                catch (Exception ex) { AutoReadData = true; Log.Log.LogException(ex); }
            }
            AutoReadData = true;
            return RevMsgData;
        }

        #endregion



        #region -------------------CAN相关-------------------
        private void Parser_CAN_DataReceived(object sender, ProtocolDataEventArgs e)
        {
            if (e.ProtocolData.Length > 0)
            {
                Dictionary<int, DataModel.CAN.CanMsgRich> dicPacket = new Dictionary<int, DataModel.CAN.CanMsgRich>();
                DataModel.CAN.GETCANDATA gETCANDATA = new DataModel.CAN.GETCANDATA();
                try
                {
                    byte[] data = e.ProtocolData.ToArray();
                    if (data != null)
                    {
                        SENDCAN = true;
                        DataModel.CAN.CanMsgRich canMsgRiche = gETCANDATA.DecodePackage2(data.ToList(), eSGBDC_Ver);
                        canMsgRiche.ObjectNo = canMsgRiche.ConsistMsg.ObjectNo = Iindex++;
                        if (canMsgRiche?.Id != null && canMsgRiche?.Id != "")
                        {
                            CANDATA.Add(canMsgRiche);
                            isCANStop = false;
                            swCANStop.Restart();
                            dicPacket.Add(ChargerID, canMsgRiche);
                            SystemEvent.RevCANPacket(dicPacket);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                }
            }

        }
        private void BMSControl_CanProtocolVersionSW(ESGBDC_Ver ver)
        {
            eSGBDC_Ver = ver;
        }
        private void SystemEventCreateCANExcel()
        {
            try
            {
                if (CANDATA != null)
                {
                    if (CANDATA.Count <= 0)
                    {
                        return;
                    }
                    int index = 0;
                    Queue<string> Q = new Queue<string>();
                    Q.Enqueue("序号");
                    Q.Enqueue("接收时间");
                    Q.Enqueue("报文时间");
                    Q.Enqueue("时间增量");
                    Q.Enqueue("帧ID");
                    Q.Enqueue("帧长度DLC");
                    Q.Enqueue("有效数据");
                    Q.Enqueue("报文翻译");
                    string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");
                    string path = $"{System.AppDomain.CurrentDomain.BaseDirectory}CAN报文(测试项)\\{GetNowLongData}\\{TestItemName + testTime}.xls";
                    NPIOUtil.CreateExcelFile(path, ChargerID, Q);
                    NpioOperation.ExcelWR.OpenExceWorkBook(NpioOperation.ExcelSaveFileName, NpioOperation.ExcelSaveSheetName);
                    for (int i = 0; i < CANDATA.Count; i++)
                    {
                        index++;
                        var DATA = CANDATA[i];
                        NpioOperation.ExcelWR.WriteExceWorkBook(++NpioOperation.RowNum, index.ToString(),
                            DATA.CreateTime.ToString(), DATA.CreateTimestamp,
                            DATA.TimeIncrement, ChargerID.ToString(), DATA.Dlc.ToString(), DATA.MsgData, DATA.MsgText);
                    }
                    NpioOperation.ExcelWR.CloseExceWorkBook(NpioOperation.ExcelSaveFileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }

        private void SystemEventCreateCANExcelName(string testItemName)
        {
            try
            {
                Iindex = 0;
                swCANStop.Restart();
                CANDATA?.Clear();
                TestItemName = testItemName;
                testTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(":", "∶");
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        #region CAN报文处理 
        public override byte[] RevCANPacket()
        {
            if (CANDataBuf.Count > 0)
            {
                byte[] data = new byte[12048];
                data = CANDataBuf.Dequeue();
                //SystemEvent.RevCANPacket(data);
                SetCANRunState();
                return data;
            }
            return null;
        }
        #endregion

        #region CAN读取设置
        int CANTimeOut
        {
            get;
            set;
        }
        bool SENDCAN = false;
        private void SetCANRunState()
        {
            isCANStop = false;
            SENDCAN = true;
            CANTimeOut = 0;
        }
        private bool SETCANStopState()
        {
            if (!SENDCAN)
            {
                CANTimeOut += 1;
                Console.WriteLine("EquipMentName:" + EquipMentName + $"ChargerID:{ChargerID}" + "；增加:" + CANTimeOut);
            }
            if (CANTimeOut >= 5)
            {
                Console.WriteLine("CANTimeOut:" + CANTimeOut);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public override void SetProtocolTime()
        {
            byte[] writeBuffer = procotol.BMSSetProtocolTime();
            SendBMSData(writeBuffer);
        }

        #endregion


        public override void RevData(byte[] RevData)
        {
            //接收到的数据全部转到协议解析队列
            parser.AddBytesToBuffer(RevData);
        }

        /// <summary>
        /// 转换List<byte>为byte[]无CRC校验
        /// </summary>
        /// <param name="ReturnbyteSource"></param>
        /// <returns></returns>
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

        public override void ReadBMSChargingData(out double ChargingVolt, out double ChargingCurrent)
        {
            AutoReadData = false;
            ChargingVolt = 0;
            ChargingCurrent = 0;
            try
            {
                byte[] RevMsgData = null;
                byte[] WriteBuffer = procotol.BMSReadCCSData();
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
                parser.OtherDataQueue.Clear();
                EquipMentPort.SendData(WriteBuffer);
                Thread.Sleep(200);
                if (parser.OtherDataQueue.Count > 0)
                {
                    RevMsgData = parser.OtherDataQueue.Dequeue();
                }

                procotol.ParseChargingData(RevMsgData, out ChargingVolt, out ChargingCurrent);
            }
            catch(Exception ex)
            {
                Log.Log.LogException(ex);
            }
            AutoReadData = true;
        }

        public override void ReadBMS_StateData()
        {

            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicBMS_DC_StateData.Add(ChargerID, bmsStateData);
            }
            int TimeOutCount = 0;//超时次数                
            while (true)
            {
                try
                {
                    if (!isCANStop)  // 如果长时间未接收到CAN报文把标志位置为false，接收到CAN报文会刷新计时器
                    {
                        if (swCANStop.ElapsedMilliseconds > 1500)
                            isCANStop = true;
                    }
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = null;
                        byte[] WriteBuffer1 = procotol.BMSReadData(EmChargerType.Charger_GB_DC);

                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {

                                //DataBuf.Clear();//清空函数经常报索引越界错误
                                parser.StateDataQueue.Clear();
                                Thread.Sleep(20);
                                //string strTemp = BitConverter.ToString(WriteBuffer1).Replace('-', ' ');
                                //SendMsgToFile("BMS_GB_DC发送数据：" + strTemp);
                                lock (SynLockBMS)
                                {
                                    EquipMentPort.SendData(WriteBuffer1);
                                }
                                Thread.Sleep(200);
                                if (parser.StateDataQueue.Count > 0)//读状态数据
                                {
                                    RevMsgData = parser.StateDataQueue.Dequeue();
                                }

                                if (RevMsgData != null)
                                {
                                    //strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                                    //SendMsgToFile("BMS_GB_DC接收到数据：" + strTemp);
                                    if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                    {
                                        if (TimeOutCount >= 3)
                                        {
                                            bmsStateData = new BMS_DC_StateData();
                                            bmsStateData.ChargerID = this.ChargerID;
                                            SystemEvent.SendMonitorMessage(bmsStateData);
                                            SystemEvent.SendConnectState(false, this);
                                            //SendMsgToFile("803");
                                        }
                                        else
                                        {
                                            TimeOutCount++;
                                        }


                                        continue;
                                    }

                                    bmsStateData = procotol.GetBMS_DC_StateData(RevMsgData, this.ChargerID);
                                    SystemEvent.SendMonitorMessage(bmsStateData);
                                    SystemEvent.SendConnectState(true, this);
                                    TimeOutCount = 0;
                                }
                                else
                                {
                                    SendMsgToFile("直流BMS失败，BMS没返回数据！");
                                    if (TimeOutCount >= 3)
                                    {
                                        bmsStateData = new BMS_DC_StateData();
                                        bmsStateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(bmsStateData);
                                        SystemEvent.SendConnectState(false, this);
                                        //SendMsgToFile("829");
                                    }
                                    else
                                    {
                                        TimeOutCount++;
                                    }

                                    continue;
                                }
                            }
                        }
                        else
                        {
                            SendMsgToFile("直流BMS通道对象不存在，请检查直流BMS通道");
                            bmsStateData = new BMS_DC_StateData();
                            bmsStateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            //SendMsgToFile("847");
                            SystemEvent.SendMonitorMessage(bmsStateData);
                        }
                    }
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                }
            }
        }

    }
}