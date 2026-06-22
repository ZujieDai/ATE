using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-中佳漏电流测试仪
    /// </summary>
    public class emtZJLeakageCurrent : EquipMentBase
    {
        public ZJLeakageCurrent_StateData stateData = new ZJLeakageCurrent_StateData();
        private LeakageCurrent_Protocol protocol = new LeakageCurrent_Protocol();
        public emtZJLeakageCurrent(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "ZJ" + " " + LanguageManager.GetByKey("剩余电流保护测试仪");
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="param">参数</param>
        public override void LeakageCurrent_SetParams(int address, int param)
        {
            byte[] WriteBuffer = protocol.GetBuffer(address, param);
            byte[] RevMsgData;
            SendData(WriteBuffer, out RevMsgData);
        }



        public override string LeakageCurrent_ReadData(int address, int param)
        {
            string data = "";
            byte[] WriteBuffer = protocol.GetBuffer(address, param);
            byte[] RevMsgData;
            SendData(WriteBuffer, out RevMsgData);
            return data;
        }


        private bool SendData(byte[] WriteBuffer, out byte[] RevMsgData)
        {
            try
            {
                AutoReadData = false;
                Thread.Sleep(300);
                RevMsgData = new byte[] { };
                if (EquipMentPort != null)
                {

                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                        RevMsgData = RevEquipMentData();
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
                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
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
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                RevMsgData = new byte[] { };
                return false;
            }
        }

        public override void LeakageCurrent_ReadState()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicZJLeakageCurrent_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicZJLeakageCurrent_StateData.Add(ChargerID, stateData);
            }
            ZJLeakageCurrent_StateData StateData = new ZJLeakageCurrent_StateData();

            while (true)
            {

                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    // byte[] WriteBuffer = { 0x01, 0x03, 0x00, 0x20, 0x00, 0x0D, 0x85, 0xC5 };//读取个13寄存器，包括CRC
                    byte[] WriteBuffer = { 0x01, 0x03, 0x00, 0x20, 0x00, 0x0F, 0x04, 0x04 };//读取个15寄存器，包括CRC
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
                                List<byte> nTemp = RevMsgData.ToList();
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                }
                                StateData = protocol.GetZJLeakageCurrent_StateData(RevMsgData, this.ChargerID);
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                StateData = new ZJLeakageCurrent_StateData();
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
                        StateData = new ZJLeakageCurrent_StateData();
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
