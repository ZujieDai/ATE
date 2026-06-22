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

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 用程控板控制辅源负载
    /// </summary>
    public class emtAuxiliaryLoadCtrl : EquipMentBase
    {
        private AuxiliaryLoadCtrl_StateData StateData = new AuxiliaryLoadCtrl_StateData();

        public emtAuxiliaryLoadCtrl(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("辅源负载");
        }
        /// <summary>
        /// 设置初始16个继电器全断开状态
        /// </summary>
        /// <returns></returns>
        private List<bool> ConditionState()
        {
            List<bool> lstConditionState = new List<bool>();
            for (int i = 0; i < 16; i++)
            {
                lstConditionState.Add(false);
            }
            return lstConditionState;
        }


        /// <summary>
        /// 取消所有状态
        /// </summary>
        public override void CancelAllState()
        {            
            List<bool> lstConditionState = ConditionState();
            byte[] buffer = GetBuffer(lstConditionState);            
            lock (objLock)
            {
                SendData(buffer);
            }
        }
        /// <summary>
        /// 设置12V辅源过压
        /// </summary>
        public override void Set12VoltOver()
        {
            List<bool> lstConditionState = ConditionState();
            lstConditionState[0] = true;
            byte[] buffer = GetBuffer(lstConditionState);
            lock (objLock)
            {
                SendData(buffer);
            }
        }
        /// <summary>
        /// 设置24V辅源过压
        /// </summary>
        public override void Set24VoltOver()
        {
            List<bool> lstConditionState = ConditionState();
            lstConditionState[1] = true;
            byte[] buffer = GetBuffer(lstConditionState);
            lock (objLock)
            {
                SendData(buffer);
            }
        }

        /// <summary>
        /// 设置辅源短路
        /// </summary>
        public override void SetShortCircuite()
        {
            List<bool> lstConditionState = ConditionState();
            lstConditionState[2] = true;
            byte[] buffer = GetBuffer(lstConditionState);
            lock (objLock)
            {
                SendData(buffer);
            }
        }

        /// <summary>
        /// 设置12V辅源电流参数(1-16A范围，步进1A)
        /// </summary>
        public override void Set12VCurrent(int current)
        {
            List<bool> lstConditionState = ConvertToIntToRelayStates_12V(current);
            byte[] buffer = GetBuffer(lstConditionState);
            lock (objLock)
            {
                SendData(buffer);
            }
        }

        /// <summary>
        /// 设置24V辅源电流参数（2-14A范围，步进2A）
        /// </summary>
        public override void Set24VCurrent(int current)
        {
            List<bool> lstConditionState = ConvertToIntToRelayStates_24V(current);
            byte[] buffer = GetBuffer(lstConditionState);
            lock (objLock)
            {
                SendData(buffer);
            }
        }
        private List<bool> ConvertToIntToRelayStates_12V(int value)
        {
            List<bool> relayStates = new List<bool>();

            // 前三位置为 fasle
            for (int i = 0; i < 3; i++)
            {
                relayStates.Add(false);
            }

            // 中间四位对应二进制数字，使用掩码判断状态
            for (int i = 0; i < 4; i++)
            {
                bool isSwitchOn = (value & (1 << i)) != 0;
                relayStates.Add(isSwitchOn);
            }

            // 后面九位默认为断开，置为 false
            for (int i = 0; i < 9; i++)
            {
                relayStates.Add(false);
            }
            return relayStates;
        }

        private List<bool> ConvertToIntToRelayStates_24V(int value)
        {
            List<bool> relayStates = new List<bool>();

            // 前2位置为 fasle
            for (int i = 0; i < 2; i++)
            {
                relayStates.Add(false);
            }

            // 中间四位对应二进制数字，使用掩码判断状态
            for (int i = 0; i < 4; i++)
            {
                bool isSwitchOn = (value & (1 << i)) != 0;
                relayStates.Add(isSwitchOn);
            }

            // 后面10位默认为断开，置为 false
            for (int i = 0; i < 10; i++)
            {
                relayStates.Add(false);
            }
            return relayStates;
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
        public override void ReadAuxiliaryLoadCtrl_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicAuxiliaryLoadCtrl_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicAuxiliaryLoadCtrl_StateData.Add(ChargerID, StateData);
            }
            byte[] WriteBuffer = { 0x01, 0x03, 0x00, 0x05, 0x00, 0x01, 0x94, 0x0B };
            while (true)
            {
                if (AutoReadData)
                {
                    byte[] RevMsgData = null;
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            lock (objLock)
                            {
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null && RevMsgData.Length > 6)
                            {
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    lock (objLock)
                                    {
                                        if (AutoReadData)
                                        {
                                            EquipMentPort.SendData(WriteBuffer);
                                            RevMsgData = RevEquipMentData();
                                        }
                                    }
       
                                }
                                if (RevMsgData != null && RevMsgData.Length > 6)
                                {
                                    StateData = GetStateData(RevMsgData);
                                    SystemEvent.SendConnectState(true, this);
                                    SystemEvent.SendMonitorMessage(StateData);
                                }
                                else
                                    SystemEvent.SendConnectState(false, this);

                            }
                            else
                            {
                                StateData = new AuxiliaryLoadCtrl_StateData();
                                StateData.ChargerID = ChargerID;
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                        StateData = new AuxiliaryLoadCtrl_StateData();
                        StateData.ChargerID = ChargerID;
                        SystemEvent.SendMonitorMessage(StateData);
                        SystemEvent.SendConnectState(false, this);

                    }
                    Thread.Sleep(300);
                }
            }
        }





        public AuxiliaryLoadCtrl_StateData GetStateData(byte[] buff)
        {
            AuxiliaryLoadCtrl_StateData data = new AuxiliaryLoadCtrl_StateData();
            data.ChargerID = ChargerID;
            List<bool> result = new List<bool>(8);
            try
            {
                if (buff == null || buff.Length != 6)
                {
                    return data;
                }

                byte x = buff[4];//8-1

                for (int i = 0; i < 8; i++)
                {
                    byte y = Convert.ToByte(Math.Pow(2, i));
                    if ((x | y) == x)
                    {
                        result[i] = true;
                    }
                }
                if (result[0])
                {
                    data.VoltOver_12V = "过压";
                }
                else
                {
                    data.VoltOver_12V = "正常";
                }

                if (result[1])
                {
                    data.VoltOver_24V = "过压";
                }
                else
                {
                    data.VoltOver_24V = "正常";
                }

                if (result[2])
                {
                    data.ShortCircuite = "短路";
                }
                else
                {
                    data.ShortCircuite = "正常";
                }

                if (AllEquipStateData.DicBMS_DC_StateData[ChargerID].APSVoltage <= 18)
                {
                    data.AuxiCurrent_12V = (x >> 3) & 0b00001111;
                    data.AuxiCurrent_24V = 0;
                }
                else
                {
                    data.AuxiCurrent_12V = 0;
                    data.AuxiCurrent_24V = (x >> 2) & 0b00001111;
                }
                return data;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return data;
            }
        }
    }
}
