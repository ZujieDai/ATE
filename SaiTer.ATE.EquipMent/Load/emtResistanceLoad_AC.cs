using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.EquipMent.Load;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-交流电阻负载
    /// </summary>
    public class emtResistanceLoad_AC : EquipMentBase
    {
        private static object SyncLock = new object();
        private ResisLoad_Protocol LoadPro = new ResisLoad_Protocol();
        public bool isWriting;  //如果是多通道共用一个IP地址，则需要改为Static
        public readonly object lockWirte = new object();
        //(交直流)
        private byte[] ConnectRequestBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x02 };// 上位机发送连接请求
        private byte[] RequestDataBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x03 };// 上位机请求参数
        private byte[] ResponseTemBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x01, 0x06 };// 上位机响应温度信息
        private byte[] ResponseAlarmBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x01, 0x07 };// 上位机响应告警
        private byte[] RVersionMessageBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x08 };//上位机请求版本信息
        private byte[] RSystemMessage_ACBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x11, 0x01, 0x92, 0x16 };//上位机请求系统实时信息  11   包括CRC和尾部

        public ResisLoad_StateData StateData = new ResisLoad_StateData();

        public emtResistanceLoad_AC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流电阻负载");
        }

        public override void ResisLoad_ON()
        {
            byte[] writeBuffer = LoadPro.LoadSet(0x05, 0x01);
            SendData_ResistanceLoad(writeBuffer);
        }

        public override void ResisLoad_OFF()
        {
            byte[] writeBuffer = LoadPro.LoadSet(0x05, 0x02);
            SendData_ResistanceLoad(writeBuffer);
        }

        public override void SetResisLoadVoltCurr(double volt, double curr)
        {
            byte[] writeBuffer = LoadPro.LoadSetCurrent(0x04, 0x02, curr);//交流
            SendData_ResistanceLoad(writeBuffer);
        }

        public override void SetResisLoadVoltCurr(double volt, double curr, int Rate = 100)
        {
            byte[] writeBuffer = LoadPro.LoadSetCurrent(0x04, 0x02, curr, Rate);//交流
            SendData_ResistanceLoad(writeBuffer);
        }

        public override void SetResisLoadPower(double volt, double power)
        {
            byte[] writeBuffer = LoadPro.LoadSetCurrent(0x04, 0x04, power);//交流
            SendData_ResistanceLoad(writeBuffer);
        }

        //public override void SetResisLoadVolt(double volt)
        //{

        //}
        private bool SendData_ResistanceLoad(byte[] WriteBuffer)
        {
            try
            {
                if (!isWriting)
                {
                    lock (lockWirte)
                    {
                        if (!isWriting)
                        {
                            isWriting = true;
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
                                    if (RevMsgData != null)
                                    {
                                        List<byte> nTemp = RevMsgData.ToList();

                                        if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                        {
                                            EquipMentPort.SendData(WriteBuffer);
                                        }
                                    }
                                    else
                                    {
                                        Thread.Sleep(50);
                                        EquipMentPort.SendData(WriteBuffer);
                                        RevMsgData = RevEquipMentData();
                                        if (RevMsgData == null)
                                        {
                                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                            SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                        }
                                        continue;
                                    }
                                }
                                AutoReadData = true;
                                Thread.Sleep(300);
                                return true;
                            }
                            else
                            {
                                SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                                AutoReadData = true;
                                Thread.Sleep(300);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                Thread.Sleep(300);
                return false;
            }
            finally
            {
                isWriting = false;
            }
            Thread.Sleep(10);
            return false;
        }

        public override void ReadResisLoad_State_AC()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicResisLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicResisLoad_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                if (AutoReadData)
                {
                    try
                    {
                        byte[] RevMsgData = null;
                        // byte[] WriteBuffer = new byte[8];
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(RSystemMessage_ACBuff).Replace('-', ' ');
                                // SendMsgToFile(EquipMentName + "发送数据---------->：" + strTemp);
                                if (isWriting)
                                    continue;
                                lock (lockWirte)
                                {
                                    if (isWriting)
                                        continue;
                                    EquipMentPort.SendData(RSystemMessage_ACBuff);
                                        RevMsgData = RevEquipMentData();
                                }
                                // string Temp = BitConverter.ToString(RevMsgData).Replace('-', ' ');
                                // SendMsgToFile(EquipMentName + "收到数据<----------：" + Temp);
                                if (RevMsgData != null)
                                {
                                    if (CheckOut.CheckLoadCrc(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        lock (lockWirte)
                                        {
                                            if (isWriting)
                                                continue;
                                            EquipMentPort.SendData(RSystemMessage_ACBuff);
                                            RevMsgData = RevEquipMentData();
                                        }
                                    }

                                    if (RevMsgData != null)
                                    {
                                        StateData = LoadPro.GetResisLoad_AC_StateData(RevMsgData, this.ChargerID);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                }
                                else
                                {
                                    // SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new ResisLoad_StateData();
                                    StateData.ChargerID = this.ChargerID;
                                    StateData.emType = EmChargerType.Charger_GB_AC;
                                    StateData.EquipName = "交流电阻负载";
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                            StateData = new ResisLoad_StateData();
                            StateData.ChargerID = this.ChargerID;
                            StateData.emType = EmChargerType.Charger_GB_AC;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                    }
                    catch (Exception ex) { Log.Log.LogException(ex); }
                }
                Thread.Sleep(300);
            }
        }


    }
}
