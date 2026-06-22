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
    /// 多通道直流电阻负载
    /// </summary>
    public class emtResistanceLoad_MultiChannel_DC : EquipMentBase
    {
        private emtResistanceLoad_MultiChannel_DC_Protocol LoadPro = new emtResistanceLoad_MultiChannel_DC_Protocol();
        private static object SynLockLoad = new object();
        public ResisLoad_MultiChannel_DC_StateData StateData = new ResisLoad_MultiChannel_DC_StateData();

        public static bool isWriting;
        public readonly static object lockWirte = new object();


        public emtResistanceLoad_MultiChannel_DC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("多通道直流") + LanguageManager.GetByKey("电阻负载");
        }
        public override void SetResisLoadVoltCurr(double voltage, double current)
        {
            if (!isWriting)
            {
                lock (lockWirte)
                {
                    if (!isWriting)
                    {
                        isWriting = true;
                        try
                        {
                            //发送需求前，先申请容量，容量可以大一点
                            double dPower_W = (voltage + 20) * (current + 5);//需求容量，单位W
                            byte[] writeBuffer = LoadPro.Load_RequestCapacity(dPower_W);
                            //SendData(writeBuffer);
                            SendData_FeedbackLoad(writeBuffer, out byte[] revMsgData);
                            string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);

                            //发送需求
                            writeBuffer = LoadPro.Load_SetParams(voltage, current);
                            //SendData(writeBuffer);
                            SendData_FeedbackLoad(writeBuffer, out revMsgData);

                            strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);

                            //添加间隔，不然可能启动不了
                            //Thread.Sleep(1500);
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
        }

        public override void ResisLoad_ON()
        {
            if (!isWriting)
            {
                lock (lockWirte)
                {
                    if (!isWriting)
                    {
                        isWriting = true;
                        try
                        {
                            byte[] writeBuffer = LoadPro.Load_ON();
                            //SendData(writeBuffer);
                            SendData_FeedbackLoad(writeBuffer, out byte[] revMsgData);
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
        }

        public override void ResisLoad_OFF()
        {
            if (!isWriting)
            {
                lock (lockWirte)
                {
                    if (!isWriting)
                    {
                        isWriting = true;
                        try
                        {
                            //先关闭负载
                            byte[] writeBuffer = LoadPro.Load_OFF();
                            //SendData(writeBuffer);
                            SendData_FeedbackLoad(writeBuffer, out byte[] revMsgData);

                            //再释放容量
                            writeBuffer = LoadPro.Load_ReleaseCapacity();
                            //SendData(writeBuffer);
                            SendData_FeedbackLoad(writeBuffer, out revMsgData);
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
        }


        public override void ReadResisLoad_State_DC()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData.Add(ChargerID, StateData);
            }
            LoadPro.SetChannel(this.ChargerID);//初始化通道编号
            while (true)
            {
                try
                {
                    Thread.Sleep(300);
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = null;
                        byte[] WriteBuffer = LoadPro.Load_GetStateData();
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                //lock (SynLockLoad)
                                //{
                                //    EquipMentPort.SendData(WriteBuffer);
                                //    RevMsgData = RevEquipMentData();
                                //}
                                bool ret = SendData_FeedbackLoad(WriteBuffer, out RevMsgData, false);
                                if (RevMsgData != null)
                                {
                                    //头部的不算
                                    byte[] revTemp = new byte[RevMsgData.Length - 1];
                                    Array.Copy(RevMsgData, 1, revTemp, 0, RevMsgData.Length - 1);
                                    if (LoadPro.CheckModbusCrc16_High_Right(revTemp))
                                    {
                                        StateData = GetStateData(RevMsgData);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                        //SendMsgToFile(EquipMentName +StateData.ChargerID.ToString()+ ">>>>>数据更新");
                                    }
                                    else if (!ret && !isWriting)
                                    {
                                        //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                        StateData = new ResisLoad_MultiChannel_DC_StateData();
                                        StateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(false, this);
                                        continue;
                                    }
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new ResisLoad_MultiChannel_DC_StateData();
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
                            StateData = new ResisLoad_MultiChannel_DC_StateData();
                            StateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    StateData = new ResisLoad_MultiChannel_DC_StateData();
                    StateData.ChargerID = this.ChargerID;
                    SystemEvent.SendMonitorMessage(StateData);
                    SystemEvent.SendConnectState(false, this);
                    Thread.Sleep(300);
                }
            }
        }


        private ResisLoad_MultiChannel_DC_StateData GetStateData(byte[] buff)
        {
            ResisLoad_MultiChannel_DC_StateData state = new ResisLoad_MultiChannel_DC_StateData();
            //state.ChargerID = ChargerID;

            if (buff.Length >= 18)
            {
                int temp = Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2") + buff[9].ToString("x2") + buff[10].ToString("x2"), 16);
                state.Voltage = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[11].ToString("x2") + buff[12].ToString("x2") + buff[13].ToString("x2") + buff[14].ToString("x2"), 16);
                state.Current = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[6].ToString("x2") , 16);
                state.ChargerID = (int)Convert.ToSingle(temp);
            }

            return state;
        }

        private bool SendData_FeedbackLoad(byte[] WriteBuffer, out byte[] RevMsgData, bool isWrite = true)
        {
            RevMsgData = null;
            lock (SynLockLoad)
            {
                try
                {
                    //优先写入
                    if (isWriting && !isWrite)
                        return true;
                    if (isWrite)
                        AutoReadData = false;
                    Thread.Sleep(300);
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();
                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                            strTemp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                            SendMsgToFile(EquipMentName + "响应数据：" + strTemp);
                            if (RevMsgData == null)
                            {
                                SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                continue;
                            }
                        }
                        if (isWrite)
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


    }


    public class emtResistanceLoad_MultiChannel_DC_Protocol
    {
        byte bStart = 0x68;
        int iLen = 0;
        byte bTAdd = 0x81;//目标地址
        byte bSAdd = 0x80;//源地址
        int iCMD = 0x0001;
        byte bChannel = 0x01;//通道编号


        /// <summary>
        /// 设置通道编号
        /// </summary>
        /// <param name="iChargerID"></param>
        public void SetChannel(int iChargerID)
        {
            try
            {
                int iChannelOffset = Convert.ToInt32(ConfigurationManager.AppSettings["iChannelOffset"]);
                int iChannel = iChargerID + iChannelOffset;
                bChannel = (byte)iChannel;
            }
            catch (Exception ex)
            {

            }
        }
        public byte[] GetModbusCrc16_High_Right(byte[] bytes)
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

        public bool CheckModbusCrc16_High_Right(byte[] tbytes)
        {
            // TCP通讯可能末尾填充了很多为0的数组凑够256个字节
            if (tbytes != null && tbytes.Length == 256)
            {
                // 找到结尾的字节
                int lastNonZeroIndex = -1;
                for (int i = tbytes.Length - 1; i > 0; i--)
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


        public byte[] GetFrameMsg(List<byte> bDatas)
        {
            List<byte> FrameMsg = new List<byte>();
            iLen = bDatas.Count + 8;
            FrameMsg.Add(bStart);
            FrameMsg.Add((byte)(iLen>>8));
            FrameMsg.Add((byte)(iLen ));
            FrameMsg.Add(bSAdd);
            FrameMsg.Add((byte)(iCMD >> 8));
            FrameMsg.Add((byte)(iCMD));
            FrameMsg.AddRange(bDatas);
            var crcData = FrameMsg.Skip(1).ToArray();
            FrameMsg.AddRange(GetModbusCrc16_High_Right(crcData));   //头部不算

            return FrameMsg.ToArray();
        }

        /// <summary>
        /// 申请容量
        /// </summary>
        /// <param name="dPower">容量，单位W</param>
        /// <returns></returns>
        public byte[] Load_RequestCapacity(double dPower)
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0021;
            int iPower = (int)dPower;
            bDatas.Add(bChannel);
            bDatas.Add((byte)(iPower >> 24));
            bDatas.Add((byte)(iPower >> 16));
            bDatas.Add((byte)(iPower >> 8));
            bDatas.Add((byte)(iPower));
            return GetFrameMsg(bDatas);
        }

        /// <summary>
        /// 释放容量
        /// </summary>
        /// <returns></returns>
        public byte[] Load_ReleaseCapacity()
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0022;
            bDatas.Add(bChannel);
            return GetFrameMsg(bDatas);
        }


        public byte[] Load_ON()
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0024;
            bDatas.Add(bChannel);
            bDatas.Add(0x01);
            return GetFrameMsg(bDatas);
        }

        public byte[] Load_OFF()
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0024;
            bDatas.Add(bChannel);
            bDatas.Add(0x02);
            return GetFrameMsg(bDatas);
        }

        /// <summary>
        /// 设置需求
        /// </summary>
        /// <param name="isCC">是否恒流</param>
        /// <param name="dVoltage">电压</param>
        /// <param name="dCurrent">电流</param>
        /// <returns></returns>
        public byte[] Load_SetParams(double dVoltage, double dCurrent)
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0023;
            bDatas.Add(bChannel);
            int itmp = (int)(dVoltage * 1000);//电压
            bDatas.Add((byte)(itmp >> 24));
            bDatas.Add((byte)(itmp >> 16));
            bDatas.Add((byte)(itmp >> 8));
            bDatas.Add((byte)(itmp));
            itmp = (int)(dCurrent * 1000);//电流
            bDatas.Add((byte)(itmp >> 24));
            bDatas.Add((byte)(itmp >> 16));
            bDatas.Add((byte)(itmp >> 8));
            bDatas.Add((byte)(itmp));
            return GetFrameMsg(bDatas);
        }

        public byte[] Load_GetVersion()
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0028;
            return GetFrameMsg(bDatas);
        }

        public byte[] Load_GetStateData()
        {
            List<byte> bDatas = new List<byte>();
            iCMD = 0x0025;
            bDatas.Add(bChannel);
            return GetFrameMsg(bDatas);
        }



    }
}
