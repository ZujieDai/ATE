using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

/*
 * 注：赛特新回馈负载
 *     一个负载设备有多个通道，每个通道连接一个导引。 如果是双导引各连一个负载通道，对于硬件层面只有一个负载设备，
 *     但是对于软件层面，则视为两个负载设备，每个通道作为一个独立的设备。把枪ID作为通道号。
 *     因此，1号枪导引必须连接1通道， 2号导引必须连接2通道。否则会出现控制错误
 * 
 */
namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-回馈负载
    /// </summary>
    public class emtFeedbackLoad : EquipMentBase
    {
        private static object SynLockLoad = new object();   //多通道的消息会串
        public FeedbackLoad_StateData StateData = new FeedbackLoad_StateData();
        public emtFeedbackLoad(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("回馈负载");
        }

        public override void SetFeedbackLoadParams(double voltage, double current)
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            int IntVoltage = Convert.ToInt32(voltage) * 1000;//单位mV
            byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };
            int IntCurrent = Convert.ToInt32(current) * 1000;//单位mA
            byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x11,//帧长度高
                0x80,//
                0x00,
                0x22,
                0x01
            };
            ReturnbyteSource[6] = (byte)chanel;//用桩编号作为通道号
            ReturnbyteSource.AddRange(voltBytes);
            ReturnbyteSource.AddRange(currBytes);

            byte[] WriteBuffer = ReturnbyteSource.ToArray();

            byte[] temp = new byte[WriteBuffer.Length - 1];

            Array.Copy(WriteBuffer, 1, temp, 0, WriteBuffer.Length - 1);
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);
            WriteBuffer = ReturnbyteSource.ToArray();

            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);

            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }


        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public override void FeedbackLoad_ON()
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01, 0x00, 0x00 };
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x01 };
            temp[5] = (byte)chanel;
            WriteBuffer[6] = (byte)chanel;//用桩编号作为通道号
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);

            string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }
        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public override void FeedbackLoad_OFF()
        {
            isOpenFeedBackLoadVoltDiff = false;
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)chanel;//用桩编号作为通道号
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x23, 0x01, 0x02 };
            temp[5] = (byte)chanel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }
        public override void FeedbackLoad_NoParallel()
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x02, 0x00, 0x00 };
            WriteBuffer[6] = (byte)chanel;
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x02 };
            temp[5] = (byte)chanel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        public override void FeedbackLoad_Parallel()
        {
            int chanel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out chanel))
            {
                chanel = ChargerID;//用桩编号作为通道号
            }
            else if (ChargerID == chanel)
                chanel = 1;

            byte[] WriteBuffer = new byte[10] { 0x68, 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x01, 0x00, 0x00 };
            WriteBuffer[6] = (byte)chanel;//用桩编号作为通道号
            byte[] temp = new byte[7] { 0x00, 0x0A, 0x80, 0x00, 0x28, 0x01, 0x01 };
            temp[5] = (byte)chanel;
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
            WriteBuffer[8] = CheckSumByte[0];
            WriteBuffer[9] = CheckSumByte[1];

            SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);

            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
            //SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }
        private bool SendData_FeedbackLoad(byte[] WriteBuffer, out byte[] RevMsgData)
        {
            RevMsgData = null;
            try
            {
                AutoReadData = false;
                Thread.Sleep(30);  //防止消息串了
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        //byte[] RevMsgData = null;
                        lock (SynLockLoad)
                        {
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();
                        }
                        if (RevMsgData == null)
                        {
                            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                            SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                            continue;
                        }
                        else
                            return true;
                    }
                    return false;
                }
                else
                {
                    SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
            finally
            {
                AutoReadData = true;
            }
        }

        public override void WriteFeedbackLoad_BMSInfo()
        {
            //如果负载没有预充继电器，需要上位机发送导引电压给负载
            if (ConfigurationManager.AppSettings["isHaveLoadControl"] == null)
                return;
            bool notLoadControl = ConfigurationManager.AppSettings["isHaveLoadControl"].ToString() == "0";

            if (notLoadControl)
            {
                while (true)
                {
                    try
                    {
                        if (AutoReadData)
                        {
                            AutoReadData = false;
                            EmChargerType? chargerType;
                            ChargerInfoManage.SelectChargerInfo(out List<ChargerInfoModel> lstChargerInfo);
                            if (lstChargerInfo != null && lstChargerInfo.Count > 0)
                            {
                                chargerType = lstChargerInfo.First().ChargerType;
                            }
                            else
                                chargerType = null;
                            if (chargerType != null)
                            {
                                //需要给负载下位机发送导引的实时电压电流，用于控制负载继电器
                                double BMSVolt = 0, BMSCurr = 0;
                                switch (chargerType)
                                {
                                    case EmChargerType.Charger_GB_DC:
                                        if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(ChargerID))
                                        {
                                            AutoReadData = true;
                                            break;
                                        }
                                        BMSVolt = AllEquipStateData.DicBMS_DC_StateData[ChargerID].ChargingVoltage;
                                        BMSCurr = AllEquipStateData.DicBMS_DC_StateData[ChargerID].ChargingCurrent;
                                        break;
                                    case EmChargerType.Charger_EUR_DC:
                                        if (!AllEquipStateData.DicBMS_EU_DC_StateData.ContainsKey(ChargerID))
                                        {
                                            AutoReadData = true;
                                            break;
                                        }
                                        BMSVolt = AllEquipStateData.DicBMS_EU_DC_StateData[ChargerID].ChargingVoltage;
                                        BMSCurr = AllEquipStateData.DicBMS_EU_DC_StateData[ChargerID].ChargingCurrent;
                                        break;
                                }
                                List<byte> BMSWriteBuffer = new List<byte> { 0x68, 0x00, 0x12, 0x80, 0x00, 0x2E, (byte)ChargerID };
                                int IntVoltage = Convert.ToInt32(BMSVolt * 1000);//单位mV
                                byte[] voltBytes = new byte[4] { (byte)(IntVoltage >> 24), (byte)(IntVoltage >> 16), (byte)(IntVoltage >> 8), (byte)IntVoltage };
                                int IntCurrent = Convert.ToInt32(BMSCurr * 1000);//单位mA
                                byte[] currBytes = new byte[4] { (byte)(IntCurrent >> 24), (byte)(IntCurrent >> 16), (byte)(IntCurrent >> 8), (byte)IntCurrent };
                                BMSWriteBuffer.AddRange(voltBytes);
                                BMSWriteBuffer.AddRange(currBytes);
                                BMSWriteBuffer.Add(isOpenFeedBackLoadVoltDiff ? (byte)1 : (byte)0); //是否开启导引电压差控制
                                BMSWriteBuffer.AddRange(CheckOut.GetModbusCrc16_High_Right(BMSWriteBuffer.Skip(1).ToArray()));
                                if (EquipMentPort != null)
                                {
                                    //SendMsgToFile(EquipMentName + "发送数据：" + BitConverter.ToString(BMSWriteBuffer.ToArray()).Replace('-', ' '));
                                    //SendData_FeedbackLoad(BMSWriteBuffer.ToArray(), out byte[] RevMsgData);
                                    EquipMentPort.SendData(BMSWriteBuffer.ToArray());
                                    Thread.Sleep(50);
                                    //SendMsgToFile(EquipMentName + "接收数据：" + BitConverter.ToString(RevMsgData).Replace('-', ' '));
                                }
                            }
                            AutoReadData = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log.LogException(ex);
                        AutoReadData = true;
                    }
                    Thread.Sleep(200);
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
                        //byte[] RevMsgData = null;
                        byte[] WriteBuffer = { 0x68, 0x00, 0x09, 0x80, 0x00, 0x21, (byte)ChargerID, 0x00, 0x00 };
                        byte[] temp = { 0x00, 0x09, 0x80, 0x00, 0x21, (byte)ChargerID };
                        byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(temp);//CRC校验函数。
                        WriteBuffer[7] = CheckSumByte[0];
                        WriteBuffer[8] = CheckSumByte[1];
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
                                SendData_FeedbackLoad(WriteBuffer, out byte[] RevMsgData);
                                if (RevMsgData != null)
                                {
                                    if (RevMsgData.Length > 18)
                                    {
                                        int index = RevMsgData.ToList().IndexOf(0x68);
                                        if (index >= 0)
                                        {
                                            byte[] RevMsgData1 = new byte[18];
                                            Array.Copy(RevMsgData, index, RevMsgData1, 0, RevMsgData1.Length - 1);
                                            RevMsgData = RevMsgData1;
                                        }
                                    }
                                    if (RevMsgData[6] != ChargerID)
                                    {
                                        continue;
                                    }
                                    byte[] revTemp = new byte[RevMsgData.Length - 1];
                                    Array.Copy(RevMsgData, 1, revTemp, 0, RevMsgData.Length - 1);

                                    //if (CheckOut.CheckModbusCrc16_High_Right(revTemp) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    //{
                                    //    EquipMentPort.SendData(WriteBuffer);
                                    //    RevMsgData = RevEquipMentData();
                                    //}
                                    //if (CheckOut.CheckModbusCrc16_High_Right(revTemp))
                                    //{
                                    StateData = GetStateData(RevMsgData);
                                    if (StateData.Voltage > 2000)
                                        continue;
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(true, this);
                                    //}
                                    //else
                                    //{
                                    //    StateData = new FeedbackLoad_StateData();
                                    //    StateData.ChargerID = this.ChargerID;
                                    //    SystemEvent.SendMonitorMessage(StateData);
                                    //    SystemEvent.SendConnectState(false, this);
                                    //}
                                }
                                else if(i == ReConnNum - 1)
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
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    StateData = new FeedbackLoad_StateData();
                    StateData.ChargerID = this.ChargerID;
                    SystemEvent.SendMonitorMessage(StateData);
                    SystemEvent.SendConnectState(false, this);
                }
                Thread.Sleep(300);
            }
        }


        private FeedbackLoad_StateData GetStateData(byte[] buff)
        {
            FeedbackLoad_StateData state = new FeedbackLoad_StateData();
            state.ChargerID = ChargerID;

            if (buff.Length == 18)
            {
                if (buff[6] != ChargerID)
                {
                    return state;
                }
                state.Chanel = buff[6];


                int temp = Convert.ToInt32(buff[7].ToString("x2") + buff[8].ToString("x2") + buff[9].ToString("x2") + buff[10].ToString("x2"), 16);
                state.Voltage = Convert.ToSingle(temp) / 1000;
                temp = Convert.ToInt32(buff[11].ToString("x2") + buff[12].ToString("x2") + buff[13].ToString("x2") + buff[14].ToString("x2"), 16);
                state.Current = Convert.ToSingle(temp) / 1000;
            }

            return state;
        }
    }
}
