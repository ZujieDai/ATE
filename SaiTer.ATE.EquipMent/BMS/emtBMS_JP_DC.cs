using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.BMS;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备 - 日标直流BMS导引
    /// </summary>
    public class emtBMS_JP_DC : EquipMentBase
    {
        private static object SynLockBMS = new object();
        public BMS_JP_DC_StateData bmsStateData = new BMS_JP_DC_StateData();
        public emtBMS_JP_DC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("日标直流导引BMS");
        }
        /// <summary>
        /// BMS协议
        /// </summary>
        BMS_Protocol procotol = new BMS_Protocol();

        public override void ReadBMS_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicBMS_JP_DC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicBMS_JP_DC_StateData.Add(ChargerID, bmsStateData);
            }
            while (true)
            {
                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = procotol.BMSReadData(EmChargerType.Charger_JP_DC);
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            lock (SynLockBMS)
                            {
                                //DataBuf.Clear();//清空函数经常报索引越界错误
                                RevEquipMentData();
                                //SendMsgToFile("赛特直流BMS发送数据：" + strTemp);
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                int index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
                                if (index >= 0 && RevMsgData.Length > index + 20 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0xB2)
                                    RevMsgData = RevMsgData.Skip(index).Take(20).ToArray();
                                //if (!CheckCrc(RevMsgData))
                                if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    bmsStateData = new BMS_JP_DC_StateData();
                                    bmsStateData.ChargerID = this.ChargerID;
                                    SystemEvent.SendMonitorMessage(bmsStateData);
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }
                                bmsStateData = procotol.GetBMS_JP_DC_StateData(RevMsgData, this.ChargerID);
                                SystemEvent.SendMonitorMessage(bmsStateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                // SendMsgToFile("读赛特直流BMS失败，BMS没返回数据！");
                                bmsStateData = new BMS_JP_DC_StateData();
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
                        bmsStateData = new BMS_JP_DC_StateData();
                        bmsStateData.ChargerID = this.ChargerID;
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(bmsStateData);
                    }

                    Thread.Sleep(300);
                }
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

        public override void BMSSetBatteryVoltage(double batteryVoltage)
        {
            byte[] WriteBuffer = procotol.BMSSetBatteryVoltage(batteryVoltage);
            SendData(WriteBuffer);
        }

        public override void BMSSetResistance(double resistance)
        {
            byte[] WriteBuffer = procotol.BMSSetResistance(resistance);
            SendData(WriteBuffer);
        }

        //只有国标有
        //public override void BMSSetKState_DC(double resistance, double batteryVoltage, bool[] bs)
        //{
        //    byte[] WriteBuffer = procotol.GBExchangeSetStrAllSend(resistance, batteryVoltage, bs);
        //    SendData(WriteBuffer);
        //}

        public override void BMS_DC_SetControl(byte tComm, bool tisTrue)
        {
            //直流CAN通断控制 下发0x87设置
            //直流D1通断控制 下发0x88设置 
            //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
            //直流开关S通断控制 下发0x8A设置（连接信号）

            //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
            //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
            //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
            //充电桩报文设置读取 下发0x50设置 ）True X4 = 01，代表设置启动BMS报文数据读取; false X4 = 00，代表关闭BMS报文数据读取；

            byte[] writeBuffer = procotol.BMSSetControl(tComm, tisTrue);
            SendData(writeBuffer);
        }

        public override void BMSSetKState_DC(byte tComm, byte[] bs)
        {
            byte[] WriteBuffer = procotol.BMSSetKState_DC(tComm, bs);
            SendData(WriteBuffer);
        }

        public override void BMSSetData_JP_DC(string MinBatteryVolt, string MaxBatteryVolt, string ChargingRateConst, string MaxChargingTime_S, string MaxChargingTime_M,
            string ChargingET, string CHAdeMONumber, string TargetBatteryVolt, string ChargingCurrent, int[] ErrorSign, int[] StateSign, string ChargingRate)
        {
            byte[] WriteBuffer = procotol.BMSSetData_JP_DC(MinBatteryVolt, MaxBatteryVolt, ChargingRateConst, MaxChargingTime_S, MaxChargingTime_M,
                ChargingET, CHAdeMONumber, TargetBatteryVolt, ChargingCurrent, ErrorSign, StateSign, ChargingRate);
            SendData(WriteBuffer);
        }

        public override List<int> BMSGetData_JP_DC(out int[] ErrorSign, out int[] StateSign)
        {
            AutoReadData = false;
            Thread.Sleep(1000);

            List<int> resData = new List<int>();
            ErrorSign = new int[8];
            StateSign = new int[8];
            byte[] WriteBuffer = procotol.BMSRead(0x01, 0xb1);
            //byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(WriteBuffer);//CRC校验函数。
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                //Thread.Sleep(100);
                byte[] RevMsgData = RevEquipMentData();
                if (RevMsgData != null)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                        {
                            EquipMentPort.SendData(WriteBuffer);
                            //Thread.Sleep(1000);
                            RevMsgData = RevEquipMentData();
                        }
                        else
                            break;
                        Thread.Sleep(200);
                    }
                    if (RevMsgData != null)
                    {
                        resData = procotol.BMSGetData_JP_DC(RevMsgData, out ErrorSign, out StateSign);
                    }
                }
            }
            AutoReadData = true;
            return resData;
        }

        public override List<bool> BMSGetKState_JP_DC(out int DCPRState, out int DCMRState, out double BatteryVolt)
        {
            List<bool> result = new List<bool>();
            DCPRState = 0; DCMRState = 0; BatteryVolt = 0;
            AutoReadData = false;
            byte[] WriteBuffer = procotol.BMSRead(0x01, 0xb6);
            lock (SynLockBMS)
            {
                EquipMentPort.SendData(WriteBuffer);
                byte[] RevMsgData = RevEquipMentData();

                int index = RevMsgData.ToList().FindIndex(b => b == 0x7e);
                if (index >= 0 && RevMsgData.Length > index + 16 && RevMsgData[index + 3] == 0x41 && RevMsgData[index + 4] == 0xB6)
                    RevMsgData = RevMsgData.Skip(index).Take(16).ToArray();
                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                {
                    result = procotol.ParseKState_JP_DC(RevMsgData, out DCPRState, out DCMRState, out BatteryVolt);
                }
            }

            AutoReadData = true;
            return result;
        }

        public override void BMSSetParameter_JP_DC(double bmsVolt, double bmsCurrent, double maxVolt)
        {
            SettingCharging_JP_DC data = new SettingCharging_JP_DC();
            data.MaxBatteryVolt = ((int)maxVolt).ToString();
            data.TargetBatteryVolt = ((int)bmsVolt).ToString();
            data.ChargingCurrent = ((int)bmsCurrent).ToString();
            byte[] WriteBuffer = procotol.BMSSetData_JP_DC(data);
            SendData(WriteBuffer);
        }


    }
}
