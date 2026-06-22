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
    /// 设备-直流电阻负载
    /// </summary>
    public class emtResistanceLoad_DC : EquipMentBase
    {
        private ResisLoad_Protocol LoadPro = new ResisLoad_Protocol();
        //(交直流)
        private byte[] ConnectRequestBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x02 };// 上位机发送连接请求
        private byte[] RequestDataBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x03 };// 上位机请求参数
        private byte[] ResponseTemBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x01, 0x06 };// 上位机响应温度信息
        private byte[] ResponseAlarmBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x01, 0x07 };// 上位机响应告警
        private byte[] RVersionMessageBuff = new byte[] { 0x68, 0x08, 0x00, 0x68, 0x80, 0x08 };//上位机请求版本信息
        private byte[] RSystemMessage_DCBuff = new byte[] { 0x68, 0x09, 0x00, 0x68, 0x80, 0x11, 0x02, 0x93, 0x16 };//上位机请求系统实时信息  11   包括CRC和尾部

        public ResisLoad_StateData StateData = new ResisLoad_StateData() { emType = EmChargerType.Charger_GB_DC };

        public emtResistanceLoad_DC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("直流电阻负载");
        }

        public override void ResisLoad_ON()
        {
            byte[] writeBuffer = LoadPro.LoadSet(0x05, 0x01);
            SendData(writeBuffer);
        }

        public override void ResisLoad_OFF()
        {
            byte[] writeBuffer = LoadPro.LoadSet(0x05, 0x02);
            SendData(writeBuffer);
        }

        public override void SetResisLoadVoltCurr(double volt, double curr)
        {
            byte[] writeBuffer = LoadPro.DCLoadSetVoltageCurrent(volt, curr);//直流
            SendData(writeBuffer);

        }

        public override void SetResisLoadPower(double volt, double power)
        {
            byte[] writeBuffer = LoadPro.DCLoadSetVoltagePower(volt, power);//直流
            SendData(writeBuffer);

        }

        //public override void SetResisLoadVolt(double volt)
        //{

        //}
        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            if (EquipMentPort != null)
            {
                for (int i = 0; i < ReConnNum; i++)
                {
                    DataBuf.Clear();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    // SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                    EquipMentPort.SendData(WriteBuffer);
                    byte[] RevMsgData = RevEquipMentData();

                    if (RevMsgData != null)
                    {
                        if (CheckOut.CheckLoadCrc(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                        {
                            EquipMentPort.SendData(WriteBuffer);
                        }
                    }
                    else
                    {
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

        public override void ReadResisLoad_State_DC()
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

                    byte[] RevMsgData;
                    // byte[] WriteBuffer = new byte[8];
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(RSystemMessage_DCBuff).Replace('-', ' ');
                            // SendMsgToFile("电阻负载发送数据：" + strTemp);
                            EquipMentPort.SendData(RSystemMessage_DCBuff);
                            RevMsgData = RevEquipMentData();

                            if (RevMsgData != null)
                            {
                                if (CheckOut.CheckLoadCrc(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    EquipMentPort.SendData(RSystemMessage_DCBuff);
                                }
                                StateData = LoadPro.GetResisLoad_DC_StateData(RevMsgData, this.ChargerID);
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                StateData = new ResisLoad_StateData();
                                StateData.ChargerID = this.ChargerID;
                                StateData.emType = EmChargerType.Charger_GB_DC;
                                StateData.EquipName = "直流电阻负载";
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        SendMsgToFile("电阻负载通道对象不存在，请检查");
                        StateData = new ResisLoad_StateData();
                        StateData.emType = EmChargerType.Charger_GB_DC;
                        StateData.ChargerID = this.ChargerID;                      
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(StateData);
                    }
                    Thread.Sleep(300);
                }
            }
        }


    }
}
