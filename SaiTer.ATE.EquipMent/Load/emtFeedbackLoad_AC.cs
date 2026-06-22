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
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent.Load;
using System.Drawing.Imaging;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-交流回馈负载
    /// </summary>
    public class emtFeedbackLoad_AC : EquipMentBase
    {
        private FeedbackLoadAC_Protocol LoadPro = new FeedbackLoadAC_Protocol();
        private static object SynLockLoad = new object();
        public FeedbackLoadAC_StateData StateData = new FeedbackLoadAC_StateData();


        public emtFeedbackLoad_AC(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流回馈负载");
        }
        public override void SetFeedbackLoadParams(double voltage, double current)
        {

            byte[] writeBuffer = LoadPro.Load_SetParams(current);
            SendData(writeBuffer);

            string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        public override void FeedbackLoad_ON()
        {
            byte[] writeBuffer = LoadPro.Load_ON();
            SendData(writeBuffer);
        }

        public override void FeedbackLoad_OFF()
        {
            byte[] writeBuffer = LoadPro.Load_OFF();
            SendData(writeBuffer);
        }


        public override void ReadFeedbackLoad_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicFeedbackLoadAC_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicFeedbackLoadAC_StateData.Add(ChargerID, StateData);
            }
            while (true)
            {
                try
                {
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
                                lock (SynLockLoad)
                                {
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (RevMsgData != null)
                                {
                                    byte[] revTemp = new byte[RevMsgData.Length];
                                    Array.Copy(RevMsgData, 0, revTemp, 0, RevMsgData.Length);
                                    if (LoadPro.CheckModbusCrc16_High_Right(revTemp))
                                    {
                                        StateData = GetStateData(RevMsgData);
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(true, this);
                                        //SendMsgToFile(EquipMentName +StateData.ChargerID.ToString()+ ">>>>>数据更新");
                                    }
                                    else
                                    {
                                        StateData = new FeedbackLoadAC_StateData();
                                        StateData.ChargerID = this.ChargerID;
                                        SystemEvent.SendMonitorMessage(StateData);
                                        SystemEvent.SendConnectState(false, this);
                                    }
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new FeedbackLoadAC_StateData();
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
                            StateData = new FeedbackLoadAC_StateData();
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
                    StateData = new FeedbackLoadAC_StateData();
                    StateData.ChargerID = this.ChargerID;
                    SystemEvent.SendMonitorMessage(StateData);
                    SystemEvent.SendConnectState(false, this);
                    Thread.Sleep(300);
                }
            }
        }


        private FeedbackLoadAC_StateData GetStateData(byte[] buff)
        {
            FeedbackLoadAC_StateData state = new FeedbackLoadAC_StateData();
            state.ChargerID = ChargerID;

            if (buff.Length >= 32)
            {
                int temp = Convert.ToInt32(buff[8].ToString("x2") + buff[9].ToString("x2") + buff[10].ToString("x2") + buff[11].ToString("x2"), 16);
                state.ActualVolt_A = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[12].ToString("x2") + buff[13].ToString("x2") + buff[14].ToString("x2") + buff[15].ToString("x2"), 16);
                state.ActualVolt_B = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[16].ToString("x2") + buff[17].ToString("x2") + buff[18].ToString("x2") + buff[19].ToString("x2"), 16);
                state.ActualVolt_C = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[20].ToString("x2") + buff[21].ToString("x2") + buff[22].ToString("x2") + buff[23].ToString("x2"), 16);
                state.ActualCurrent_A = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[24].ToString("x2") + buff[25].ToString("x2") + buff[26].ToString("x2") + buff[27].ToString("x2"), 16);
                state.ActualCurrent_B = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[28].ToString("x2") + buff[29].ToString("x2") + buff[30].ToString("x2") + buff[31].ToString("x2"), 16);
                state.ActualCurrent_C = Convert.ToSingle(temp) / 1000;
            }

            return state;
        }


    }


    public class FeedbackLoadAC_Protocol
    {
        byte bStart = 0x68;
        int iLen = 0;
        byte bAdd = 0x00;
        int iCMD = 0x0000;
        byte bDataCMD = 0x00;
        List<byte> bDatas = new List<byte>();

        List<byte> FrameMsg = new List<byte>();


        public byte[] GetModbusCrc16_High_Right(byte[] bytes)
        {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF;// 预置一个值为 0xFFFF 的 16 位寄存器

            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01;// 多项式码 0xA001

            for (int i = 1; i < bytes.Length; i++)//从第二个字节开始
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

        public byte[] GetFrameMsg()
        {
            FrameMsg.Clear();
            iLen = bDatas.Count + 9;
            FrameMsg.Add(bStart);
            FrameMsg.Add((byte)(iLen >> 8));
            FrameMsg.Add((byte)(iLen));
            FrameMsg.Add(bAdd);
            FrameMsg.Add((byte)(iCMD>>8));
            FrameMsg.Add((byte)(iCMD));
            FrameMsg.Add(bDataCMD);
            FrameMsg.AddRange(bDatas);
            FrameMsg.AddRange(GetModbusCrc16_High_Right(FrameMsg.ToArray()));

            return FrameMsg.ToArray();
        }

        public byte[] Load_ON()
        {
            bDatas.Clear();
            bAdd = 0x80;
            iCMD = 0x1013;
            bDataCMD = 0x01;
            bDatas.Add(0x01);
            return GetFrameMsg();
        }

        public byte[] Load_OFF()
        {
            bDatas.Clear();
            bAdd = 0x80;
            iCMD = 0x1013;
            bDataCMD = 0x01;
            bDatas.Add(0x02);
            return GetFrameMsg();
        }

        public byte[] Load_SetParams(double dCurrent)
        {
            bDatas.Clear();
            bAdd = 0x80;
            iCMD = 0x1011;
            bDataCMD = 0x01;
            int itmp = (int)(dCurrent * 1000);
            bDatas.Add((byte)(itmp >> 24));
            bDatas.Add((byte)(itmp >> 16));
            bDatas.Add((byte)(itmp >> 8));
            bDatas.Add((byte)(itmp ));
            return GetFrameMsg();
        }

        public byte[] Load_GetVersion()
        {
            bDatas.Clear();
            bAdd = 0x80;
            iCMD = 0x1001;
            bDataCMD = 0x01;
            return GetFrameMsg();
        }

        public byte[] Load_GetStateData()
        {
            bDatas.Clear();
            bAdd = 0x80;
            iCMD = 0x1012;
            bDataCMD = 0x01;
            return GetFrameMsg();
        }



    }
}
