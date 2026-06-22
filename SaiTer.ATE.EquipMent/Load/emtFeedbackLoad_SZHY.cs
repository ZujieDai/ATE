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
    /// 回馈负载，深圳HY项目客户提供的协议，使用ModbusTCP
    /// </summary>
    public class emtFeedbackLoad_SZHY : EquipMentBase
    {
        private static object SynLockLoad = new object();
        public FeedbackLoad_StateData StateData = new FeedbackLoad_StateData();
        LoadProtocool__SZHY Protocol = new LoadProtocool__SZHY();
        public emtFeedbackLoad_SZHY(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("回馈负载");
        }

        public override void SetFeedbackLoadParams(double voltage, double current)
        {
            byte[] WriteBuffer = Protocol.Set_U_I(voltage, current);
            SendData_FeedbackLoad(WriteBuffer);
        }


        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void FeedbackLoad_ON()
        {
            byte[] WriteBuffer = Protocol.Load_ON();
            SendData_FeedbackLoad(WriteBuffer);

        }
        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public override void FeedbackLoad_OFF()
        {
            byte[] WriteBuffer = Protocol.Load_OFF();
            SendData_FeedbackLoad(WriteBuffer);
        }

        /// <summary>
        /// 这个负载有国标BMS启动功能
        /// </summary>
        public override void BMS_ON()
        {
            byte[] WriteBuffer = Protocol.BMS_ON();
            SendData_FeedbackLoad(WriteBuffer);
        }

        public override void BMS_OFF()
        {
            byte[] WriteBuffer = Protocol.Load_OFF();
            SendData_FeedbackLoad(WriteBuffer);
        }

        private bool SendData_FeedbackLoad(byte[] WriteBuffer)
        {
            lock (SynLockLoad)
            {
                try
                {

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
                            if (RevMsgData == null)
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
                    return false;
                }
            }
        }

        public override void ReadFeedbackLoad_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicFeedbackLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicFeedbackLoad_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                try
                {
                    if (AutoReadData)
                    {
                        byte[] RevMsgData = null;
                        byte[] WriteBuffer = Protocol.Load_ReadInfo();
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                lock (SynLockLoad)
                                {
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (RevMsgData != null)
                                {
                                    byte[] revTemp = new byte[RevMsgData.Length];
                                    Array.Copy(RevMsgData, 0, revTemp, 0, RevMsgData.Length );
                                    if (CheckOut.CheckModbusCrc16_High_Right(revTemp))
                                    {
                                        StateData = GetStateData(RevMsgData);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                    else
                                    {
                                        StateData = new FeedbackLoad_StateData();
                                        StateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(false, this);
                                    }
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new FeedbackLoad_StateData();
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
                            StateData = new FeedbackLoad_StateData();
                            StateData.ChargerID = this.ChargerID;
                            SystemEvent.SendConnectState(false, this);
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                        Thread.Sleep(300);
                    }

                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    StateData = new FeedbackLoad_StateData();
                    StateData.ChargerID = this.ChargerID;
                    SystemEvent.SendMonitorMessage(StateData);
                    SystemEvent.SendConnectState(false, this);
                    Thread.Sleep(300);
                }
            }
        }


        private FeedbackLoad_StateData GetStateData(byte[] buff)
        {
            FeedbackLoad_StateData state = new FeedbackLoad_StateData();
            state.ChargerID = ChargerID;

            if (buff.Length >= 18 + 5)
            {


                int itmp = 0;
                itmp = Convert.ToInt32(buff[3].ToString("x2") + buff[4].ToString("x2"), 16);
                state.Voltage = Convert.ToSingle(itmp) / 10;
                itmp = Convert.ToInt32(buff[5].ToString("x2") + buff[6].ToString("x2"), 16);
                state.Current = Convert.ToSingle(itmp) / 10;
            }

            return state;
        }



    }

    class LoadProtocool__SZHY
    {
        byte bAdd = 0x01;
        byte bCMD_Read = 0x03;
        byte bCMD_Write = 0x06;
        byte bCMD_Writes = 0x10;
        int JcqAdd = 0x00;
        int JcqCount = 0;
        List<byte> bDatas = new List<byte>();
        List<byte> buff = new List<byte>();
        public byte[] GetFrameMsg_Read()
        {
            buff.Clear();
            buff.Add(bAdd);
            buff.Add(bCMD_Read);
            buff.Add((byte)(JcqAdd >> 8));
            buff.Add((byte)(JcqAdd));
            buff.Add((byte)(JcqCount >> 8));
            buff.Add((byte)(JcqCount));
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(buff.ToArray());//CRC校验函数。 
            buff.AddRange(CheckSumByte);

            return buff.ToArray();
        }

        /// <summary>
        /// 写多个寄存器
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrameMsg_Writes()
        {
            buff.Clear();
            buff.Add(bAdd);
            buff.Add(bCMD_Writes);
            buff.Add((byte)(JcqAdd >> 8));
            buff.Add((byte)(JcqAdd));
            buff.Add((byte)(JcqCount >> 8));
            buff.Add((byte)(JcqCount));
            buff.Add((byte)(JcqCount * 2));
            buff.AddRange(bDatas);
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(buff.ToArray());//CRC校验函数。 
            buff.AddRange(CheckSumByte);

            return buff.ToArray();
        }

        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrameMsg_Write_Single()
        {
            buff.Clear();
            buff.Add(bAdd);
            buff.Add(bCMD_Write);
            buff.Add((byte)(JcqAdd >> 8));
            buff.Add((byte)(JcqAdd));
            buff.AddRange(bDatas);
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(buff.ToArray());//CRC校验函数。 
            buff.AddRange(CheckSumByte);

            return buff.ToArray();
        }

        public byte[] BMS_ON()
        {
            bDatas.Clear();
            JcqAdd = 0x00;
            JcqCount = 1;
            bDatas.Add(0x00);
            bDatas.Add(0x01);

            return GetFrameMsg_Write_Single();
        }

        public byte[] Load_ON()
        {
            bDatas.Clear();
            JcqAdd = 0x00;
            JcqCount = 1;
            bDatas.Add(0x00);
            bDatas.Add(0x02);

            return GetFrameMsg_Write_Single();
        }

        public byte[] Load_OFF()
        {
            bDatas.Clear();
            JcqAdd = 0x00;
            JcqCount = 1;
            bDatas.Add(0x00);
            bDatas.Add(0x00);

            return GetFrameMsg_Write_Single();
        }

        public byte[] Set_U_I(double dU,double dI)
        {
            int iU = (int)(dU * 10);
            int iI = (int)(dI * 10);
            bDatas.Clear();
            JcqAdd = 0x01;
            JcqCount = 2;
            bDatas.Add((byte)(iU >> 8));
            bDatas.Add((byte)(iU ));
            bDatas.Add((byte)(iI >> 8));
            bDatas.Add((byte)(iI));

            return GetFrameMsg_Writes();
        }

        public byte[] Load_ReadInfo()
        {
            bDatas.Clear();
            JcqAdd = 0x06;
            JcqCount = 9;

            return GetFrameMsg_Read();
        }


    }

}
