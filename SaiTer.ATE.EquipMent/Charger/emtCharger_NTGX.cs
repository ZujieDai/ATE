using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.EquipStateData;
using System.Runtime.InteropServices.ComTypes;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备 - 充电桩模拟器（目前适用于南通GX）
    /// </summary>
    public class emtCharger_NTGX : EquipMentBase
    {
        public int Iindex = 0;
        private static object SynLockCharger = new object();
        public Charger_NTGX_StateData chargerStateData = new Charger_NTGX_StateData();

        /// <summary>
        /// BMS协议
        /// </summary>
        Protocol_Charger_NTGX procotol_Charger = new Protocol_Charger_NTGX();

        public emtCharger_NTGX(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("充电桩模拟器");
        }

        public override void ChargerStart()
        {
            byte[] WriteBuffer = procotol_Charger.Charger_Start_Stop(true);
            EquipMentPort.SendData(WriteBuffer);
        }

        public override void ChargerStop()
        {
            byte[] WriteBuffer = procotol_Charger.Charger_Start_Stop(false);
            EquipMentPort.SendData(WriteBuffer);
        }

        public override void LoadStart_Charger()
        {
            byte[] WriteBuffer = procotol_Charger.Load_Start_Stop(true);
            EquipMentPort.SendData(WriteBuffer);
        }

        public override void LoadStop_Charger()
        {
            byte[] WriteBuffer = procotol_Charger.Load_Start_Stop(false);
            EquipMentPort.SendData(WriteBuffer);
        }

        public override void SetLoadParam_Charger(double dVoltage, double dCurrent)
        {
            byte[] WriteBuffer = procotol_Charger.SetLoadParam_Charger(dVoltage, dCurrent);
            EquipMentPort.SendData(WriteBuffer);
        }


        public override void ReadCharger_StateData()
        {

            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicChargerNTGXCtrl_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicChargerNTGXCtrl_StateData.Add(ChargerID, chargerStateData);
            }
            int TimeOutCount = 0;//超时次数                
            while (true)
            {
                try
                {
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = null;
                        byte[] WriteBuffer1 = procotol_Charger.ReadStateData();

                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(WriteBuffer1).Replace('-', ' ');
                                SendMsgToFile("充电桩模拟器发送数据：" + strTemp);
                                lock (SynLockCharger)
                                {
                                    EquipMentPort.SendData(WriteBuffer1);
                                    RevMsgData = RevEquipMentData();
                                }

                                if (RevMsgData != null)
                                {
                                    strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                                    SendMsgToFile("充电桩模拟器接收到数据：" + strTemp);
                                    byte[] revTemp = new byte[RevMsgData.Length];
                                    Array.Copy(RevMsgData, 0, revTemp, 0, RevMsgData.Length);
                                    if (!procotol_Charger.CheckModbusCrc16_High_Right(RevMsgData))
                                    {

                                        if (TimeOutCount >= 3)
                                        {
                                            chargerStateData = new Charger_NTGX_StateData();
                                            chargerStateData.ChargerID = this.ChargerID;
                                            SystemEvent.SendMonitorMessage(chargerStateData);
                                            SystemEvent.SendConnectState(false, this);
                                            //SendMsgToFile("803");
                                        }
                                        else
                                        {
                                            TimeOutCount++;
                                        }


                                        continue;
                                    }

                                    SendMsgToFile("充电桩模拟器报文验证成功！");
                                    chargerStateData = procotol_Charger.GetStateData(RevMsgData);//解析数据
                                    chargerStateData.ChargerID = this.ChargerID;
                                    SystemEvent.SendMonitorMessage(chargerStateData);
                                    SystemEvent.SendConnectState(true, this);
                                    TimeOutCount = 0;
                                }
                                else
                                {

                                    SendMsgToFile("充电桩模拟器失败，没返回数据！");
                                    if (TimeOutCount >= 3)
                                    {
                                        chargerStateData = new Charger_NTGX_StateData();
                                        chargerStateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(chargerStateData);
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
                            SendMsgToFile("充电桩模拟器通道对象不存在，请检查通道");
                            chargerStateData = new Charger_NTGX_StateData();
                            chargerStateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(chargerStateData);
                        }
                        Thread.Sleep(300);
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    chargerStateData = new Charger_NTGX_StateData();
                    chargerStateData.ChargerID = this.ChargerID;
                    SystemEvent.SendConnectState(false, this);
                    SystemEvent.SendMonitorMessage(chargerStateData);
                }
            }
        }

    }

    class Protocol_Charger_NTGX
    {
        byte SOI = 0x7E;
        int iLen = 0;
        byte Add = 0xFF;
        byte CMDType = 0x01;
        byte CMD = 0x01;
        List<byte> Datas = new List<byte>();
        byte EOI = 0x0D;
        byte[] CHKSUN;

        List<byte> FrameMsg = new List<byte>();

        public byte[] GetFrameMsg()
        {
            FrameMsg.Clear();
            iLen = Datas.Count + 8;
            FrameMsg.Add(SOI);
            FrameMsg.Add((byte)iLen);
            FrameMsg.Add(Add);
            FrameMsg.Add(CMDType);
            FrameMsg.Add(CMD);
            FrameMsg.AddRange(Datas);
            FrameMsg.Add(EOI);
            FrameMsg.AddRange(GetModbusCrc16_High_Right(FrameMsg.ToArray()));

            return FrameMsg.ToArray();
        }

        public byte[] ReadStateData()
        {
            Datas.Clear();
            CMDType = 0x01;
            CMD = 0x01;

            return GetFrameMsg();
        }

        public byte[] SetCMLParam(double MaxU, double MinU, double MaxI, double MinI)
        {
            Datas.Clear();
            CMDType = 0x03;
            CMD = 0x15;
            int iMaxU = (int)(MaxU * 10);
            Datas.Add((byte)(iMaxU >> 8));
            Datas.Add((byte)(iMaxU));

            int iMinU = (int)(MinI * 10);
            Datas.Add((byte)(iMinU >> 8));
            Datas.Add((byte)(iMinU));

            int iMaxI = (int)(MaxI * 10);
            Datas.Add((byte)(iMaxI >> 8));
            Datas.Add((byte)(iMaxI));

            int iMinI = (int)(MinI * 10);
            Datas.Add((byte)(iMinI >> 8));
            Datas.Add((byte)(iMinI));


            return GetFrameMsg();
        }

        public byte[] Charger_Start_Stop(bool isStart)
        {
            Datas.Clear();
            CMDType = 0x03;
            CMD = 0x30;
            Datas.Add(0x00);
            Datas.Add(0x00);
            Datas.Add(0x00);
            if (isStart)
            {
                Datas.Add(0x01);
            }
            else
            {
                Datas.Add(0x00);
            }

            return GetFrameMsg();
        }


        public byte[] Load_Start_Stop(bool isStart)
        {
            Datas.Clear();
            CMDType = 0x03;
            CMD = 0xA2;
            //启停两个字节
            Datas.Add(0x00);
            if (isStart)
            {
                Datas.Add(0x01);
            }
            else
            {
                Datas.Add(0x00);
            }

            return GetFrameMsg();
        }

        public byte[] SetLoadParam_Charger(double dVoltage, double dCurrent)
        {
            Datas.Clear();
            CMDType = 0x03;
            CMD = 0xA3;
            Datas.Add(0x00);//模式默认两个字节
            Datas.Add(0x00);

            int iVoltage = (int)(dVoltage * 10);
            Datas.Add((byte)(iVoltage >> 8));
            Datas.Add((byte)(iVoltage));

            int iCurrent = (int)(dCurrent * 10);
            Datas.Add((byte)(iCurrent >> 8));
            Datas.Add((byte)(iCurrent));

            return GetFrameMsg();
        }


        public byte[] CANMsgAutoUpLoad_ON_OFF(bool isAutoUpload)
        {
            Datas.Clear();
            CMDType = 0x03;
            CMD = 0x50;
            if (isAutoUpload)
            {
                Datas.Add(0x01);
            }
            else
            {
                Datas.Add(0x02);
            }

            return GetFrameMsg();
        }



        public Charger_NTGX_StateData GetStateData(byte[] buff)
        {
            Charger_NTGX_StateData state = new Charger_NTGX_StateData();

            if (buff.Length >= 34)
            {
                int temp = Convert.ToInt32(buff[5].ToString("x2") + buff[6].ToString("x2"), 16);
                state.ChargingVoltage = Convert.ToSingle(temp) / 10;
                temp = Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2"), 16);
                state.ChargingCurrent = Convert.ToSingle(temp) / 10;

                temp = Convert.ToInt32(buff[23].ToString("x2") + buff[24].ToString("x2"), 16);//充电阶段
                state.ChargingState = GetChargingState(temp);
            }

            return state;
        }

        private string GetChargingState(int iState)
        {
            string sState = "";
            switch (iState)
            {
                case 0:
                    sState = "空闲";
                    break;
                case 1:
                    sState = "等待绝缘监测 ";
                    break;
                case 2:
                    sState = "等待BMS握手信号";
                    break;
                case 3:
                    sState = "等待BMS配置信息";
                    break;
                case 4:
                    sState = "等待BMS充电准备报文 ";
                    break;
                case 5:
                    sState = "等待BROAA准备报文";
                    break;
                case 6:
                    sState = "绝缘监测";
                    break;
                case 7:
                    sState = "等待充电启动 ";
                    break;
                case 8:
                    sState = "充电中";
                    break;
                case 9:
                    sState = "等待BMS中止充电报文";
                    break;
                case 10:
                    sState = "等待BMS充电统计";
                    break;
                case 11:
                    sState = "收到BMS充电统计 ";
                    break;
                case 12:
                    sState = "充电结束";
                    break;
                default:
                    sState = "空闲";
                    break;
            }
            sState = iState.ToString() + sState;
            return sState;
        }


        public  byte[] GetModbusCrc16_High_Right(byte[] bytes)
        {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF;// 预置一个值为 0xFFFF 的 16 位寄存器

            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01;// 多项式码 0xA001

            for (int i = 0; i < bytes.Length; i++)
            {
                crcRegister_L = (byte)(crcRegister_L ^ bytes[i]);

                for (int j = 0; j < 8; j++)
                {
                    byte tempCRC_H = crcRegister_H;
                    byte tempCRC_L = crcRegister_L;

                    crcRegister_H = (byte)(crcRegister_H >> 1);
                    crcRegister_L = (byte)(crcRegister_L >> 1);
                    // 高位右移前最后 1 位应该是低位右移后的第 1 位：如果高位最后一位为 1 则低位右移后前面补 1
                    if ((tempCRC_H & 0x01) == 0x01)
                    {
                        crcRegister_L = (byte)(crcRegister_L | 0x80);
                    }

                    if ((tempCRC_L & 0x01) == 0x01)
                    {
                        crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                        crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                    }
                }
            }

            return new byte[] { crcRegister_L, crcRegister_H };
        }

        public  bool CheckModbusCrc16_High_Right(byte[] tbytes)
        {
            // TCP通讯可能末尾填充了很多为0的数组凑够256个字节
            if (tbytes != null && tbytes.Length == 256)
            {
                // 找到结尾的字节
                int lastNonZeroIndex = -1;
                for (int i = tbytes.Length - 1; i >= 0; i--)
                {
                    if (tbytes[i] != 0x00)
                    {
                        lastNonZeroIndex = i;
                        break;
                    }
                }

                // 如果找到了非零字节，则截取有效数据
                if (lastNonZeroIndex != -1 && lastNonZeroIndex != tbytes.Length)
                {
                    byte[] trimmedBytes = new byte[lastNonZeroIndex + 1];
                    Array.Copy(tbytes, trimmedBytes, trimmedBytes.Length);
                    tbytes = trimmedBytes;
                }
            }
            bool isCheck = false;
            byte[] crc16 = new byte[2];

            try
            {
                if (tbytes.Count() > 3)
                {
                    crc16[0] = tbytes[tbytes.Length - 2];
                    crc16[1] = tbytes[tbytes.Length - 1];
                }
                byte[] cRC16Buffer = GetModbusCrc16_High_Right(tbytes.Take(tbytes.Length - 2).ToArray());
                if ((cRC16Buffer[0] == crc16[0]) && (cRC16Buffer[1] == crc16[1]))
                {
                    isCheck = true;
                }
            }
            catch
            {
            }
            return isCheck;
        }


    }
}
