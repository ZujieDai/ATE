using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Configuration;
using SaiTer.ATE.DataModel.CAN;
using static System.Windows.Forms.AxHost;
using Sunny.UI.Win32;
using System.Collections;
using System.Drawing.Imaging;

namespace SaiTer.ATE.EquipMent
{
    public class emtFeedbackLoad_DC_ST : EquipMentBase
    {
        private emtFeedbackLoad_DC_ST_Protocol LoadPro = new emtFeedbackLoad_DC_ST_Protocol();
        private static object SynLockLoad = new object();
        public FeedbackLoad_StateData StateData = new FeedbackLoad_StateData();


        public emtFeedbackLoad_DC_ST(int type)
        {
            //LoadPro.SetAdd(this.ChargerID);//设置通道编号,这里还没赋值，不能调用
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("回馈负载");
        }
        public override void SetFeedbackLoadParams(double voltage, double current)
        {
            //发送需求前，先申请容量，容量可以大一点
            double dPower_W = (voltage + 20) * (current + 5);//需求容量，单位W
            byte[] writeBuffer = LoadPro.Load_RequestCapacity(dPower_W);
            //SendData(writeBuffer);
            SendData_FeedbackLoad(writeBuffer, out byte[] RevMsgData);
            string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);

            //发送需求
            writeBuffer = LoadPro.Load_SetParams(true, voltage, current);
            //SendData(writeBuffer);
            SendData_FeedbackLoad(writeBuffer, out RevMsgData);

            strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
            SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
        }

        public override void FeedbackLoad_ON()
        {
            byte[] writeBuffer = LoadPro.Load_ON();
            //SendData(writeBuffer);
            SendData_FeedbackLoad(writeBuffer, out byte[] RevMsgData);
        }

        public override void FeedbackLoad_OFF()
        {
            //先关闭负载
            byte[] writeBuffer = LoadPro.Load_OFF();
            //SendData(writeBuffer);
            SendData_FeedbackLoad(writeBuffer, out byte[] RevMsgData);

            //再释放容量
            writeBuffer = LoadPro.Load_ReleaseCapacity();
            //SendData(writeBuffer);
            SendData_FeedbackLoad(writeBuffer, out RevMsgData);

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


        public override void ReadFeedbackLoad_StateData()
        {
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicFeedbackLoad_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicFeedbackLoad_StateData.Add(ChargerID, StateData);
            }
            LoadPro.SetAdd(this.ChargerID);//初始化通道编号
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
                                //lock (SynLockLoad)
                                //{
                                //    EquipMentPort.SendData(WriteBuffer);
                                //    RevMsgData = RevEquipMentData();
                                //}
                                SendData_FeedbackLoad(WriteBuffer, out RevMsgData);
                                if (RevMsgData != null)
                                {
                                    byte[] revTemp = new byte[RevMsgData.Length];
                                    Array.Copy(RevMsgData, 0, revTemp, 0, RevMsgData.Length);


                                    StateData = GetStateData(RevMsgData);
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(true, this);

                                    //if (LoadPro.CheckSum(revTemp))
                                    //{
                                    //    StateData = GetStateData(RevMsgData);
                                    //    SystemEvent.SendMonitorMessage(StateData);
                                    //    SystemEvent.SendConnectState(true, this);
                                    //    //SendMsgToFile(EquipMentName +StateData.ChargerID.ToString()+ ">>>>>数据更新");
                                    //}
                                    //else
                                    //{
                                    //    StateData = new FeedbackLoad_StateData();
                                    //    StateData.ChargerID = this.ChargerID;
                                    //    SystemEvent.SendMonitorMessage(StateData);
                                    //    SystemEvent.SendConnectState(false, this);
                                    //}
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
                Thread.Sleep(300);
            }
        }


        private FeedbackLoad_StateData GetStateData(byte[] buff)
        {
            FeedbackLoad_StateData state = new FeedbackLoad_StateData();
            state.ChargerID = ChargerID;
            try
            {
                List<emtFeedbackLoad_DC_ST_MsgStructure> msgs = GetMsgs(buff);
                if (msgs.Count > 0)
                {
                    foreach(emtFeedbackLoad_DC_ST_MsgStructure msg in msgs)
                    {
                        if(msg.bCMD==0x42)//CMD符合要求
                        {
                            if(msg.bDatas.Count > 0)//数据长度符合要求
                            {
                                int temp = Convert.ToInt32(msg.bDatas[20].ToString("x2") + msg.bDatas[21].ToString("x2") 
                                    + msg.bDatas[22].ToString("x2") + msg.bDatas[23].ToString("x2"), 16);
                                state.Voltage = Convert.ToSingle(temp) / 1000;
                                temp = Convert.ToInt32(msg.bDatas[24].ToString("x2") + msg.bDatas[25].ToString("x2")
                                    + msg.bDatas[26].ToString("x2") + msg.bDatas[27].ToString("x2"), 16);
                                state.Current = -Convert.ToSingle(temp) / 1000;


                                return state;
                            }
                        }
                    }
                    //if (buff.Length >= 32)
                    //{
                    //    int temp = Convert.ToInt32(buff[25].ToString("x2") + buff[26].ToString("x2") + buff[27].ToString("x2") + buff[28].ToString("x2"), 16);
                    //    state.Voltage = Convert.ToSingle(temp) / 1000;
                    //    temp = Convert.ToInt32(buff[29].ToString("x2") + buff[30].ToString("x2") + buff[31].ToString("x2") + buff[32].ToString("x2"), 16);
                    //    state.Current = Convert.ToSingle(temp) / 1000;
                    //}
                }
            }
            catch(Exception ex)
            {

            }

            return state;
        }

        private int _State = 0;
        /// <summary>
        /// 将所有的字节分解为多包报文
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private List<emtFeedbackLoad_DC_ST_MsgStructure> GetMsgs(byte[] buff)
        {
            List<emtFeedbackLoad_DC_ST_MsgStructure> msgs = new List<emtFeedbackLoad_DC_ST_MsgStructure>();
            emtFeedbackLoad_DC_ST_MsgStructure model = new emtFeedbackLoad_DC_ST_MsgStructure();
            List<byte> buf = buff.ToList();
            int tmpLen = 0;

            for (int i = 0; i < buf.Count; i++)
            {
                byte v = buf[i];

                #region 帧格式判断
                switch (_State)
                {
                    case 0:
                        if (v == 0x68)
                        {
                            _State = 30;
                            model = new emtFeedbackLoad_DC_ST_MsgStructure();
                            model.FrameMsg.Add(v);
                            model.bStart = v;
                        }
                        break;
                    case 30:
                        if (v == ChargerID)//地址和枪号关联，筛选了其他的通道报文
                        {
                            _State = 31;
                            model.FrameMsg.Add(v);
                            model.bTAdd = v;
                        }
                        else
                        {
                            _State = -30;
                        }
                        break;
                    case 31:
                        if (v == 0x80)
                        {
                            _State = 32;
                            model.FrameMsg.Add(v);
                            model.bSAdd = v;
                        }
                        else
                        {
                            _State = -31;
                        }
                        break;
                    case 32://CMD
                        _State = 33;
                        model.FrameMsg.Add(v);
                        model.bCMD = v;
                        break;
                    case 33://数据长度
                        _State = 34;
                        model.FrameMsg.Add(v);
                        model.iLen = v;
                        tmpLen = v;
                        break;
                    case 34://报文内容
                        model.FrameMsg.Add(v);
                        model.bDatas.Add(v);
                        tmpLen--;
                        if (tmpLen <= 0)
                        {
                            _State = 35;
                        }
                        break;
                    case 35://校验码
                        if (v == LoadPro.GetCheckSum(model.FrameMsg.ToArray()))
                        {
                            model.FrameMsg.Add(v);
                            model.bCheckCode = v;
                            _State = 100;
                        }
                        else
                        {
                            _State = -35;
                        }
                        break;
                }
                #endregion


                if (_State <= 0)
                {
                    model = new emtFeedbackLoad_DC_ST_MsgStructure();
                    _State = 0;
                }
                else if (_State == 100 || _State == 101)
                {
                    msgs.Add(model);
                    model = new emtFeedbackLoad_DC_ST_MsgStructure();
                    _State = 0;
                }
            }

            return msgs;
        }

    }

        public class emtFeedbackLoad_DC_ST_MsgStructure
        {
            public byte bStart = 0x68;
            public int iLen = 0;
            public byte bTAdd = 0x80;//目标地址
            public byte bSAdd = 0x01;//源地址
            public byte bCMD = 0x01;
            public List<byte> bDatas = new List<byte>();
            public byte bCheckCode = 0x00;//校验码
            public List<byte> FrameMsg = new List<byte>();//总的报文包
        }


    public class emtFeedbackLoad_DC_ST_Protocol
    {
        byte bStart = 0x68;
        int iLen = 0;
        byte bTAdd = 0x80;//目标地址
        byte bSAdd = 0x01;//源地址
        byte bCMD = 0x01;
        List<byte> bDatas = new List<byte>();

        List<byte> FrameMsg = new List<byte>();

        /// <summary>
        /// 设置通道编号
        /// </summary>
        /// <param name="iChargerID"></param>
        public void SetAdd(int iChargerID)
        {
            try
            {
                int iChannelOffset = Convert.ToInt32(ConfigurationManager.AppSettings["iChannelOffset"]);
                int iAdd = iChargerID + iChannelOffset;
                bSAdd = (byte)iAdd;
            }
            catch(Exception ex)
            {

            }
        }


        public byte GetCheckSum(byte[] bytes)
        {
            int iSum = 0x00;

            for (int i = 1; i < bytes.Length; i++)//从第二个字节开始
            {
                iSum+=bytes[i];
            }

            return (byte)iSum;
        }

        public byte GetCheckXor(byte[] bytes)
        {
            byte CheckCode = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                CheckCode ^= bytes[i];
            }
            return CheckCode;
        }

        public bool CheckSum(byte[] tbytes)
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
            byte bSum = 0x00;

            try
            {
                if (tbytes.Count() > 6)
                {
                    bSum = tbytes[tbytes.Length - 1];
                }
                byte bCheckSum = GetCheckSum(tbytes.Take(tbytes.Length - 1).ToArray());
                if (bSum == bCheckSum)
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
            iLen = bDatas.Count + 6;
            FrameMsg.Add(bStart);
            FrameMsg.Add(bTAdd);
            FrameMsg.Add(bSAdd);
            FrameMsg.Add(bCMD);
            FrameMsg.Add((byte)iLen);
            FrameMsg.AddRange(bDatas);
            FrameMsg.Add(GetCheckSum(FrameMsg.ToArray()));

            return FrameMsg.ToArray();
        }

        /// <summary>
        /// 申请容量
        /// </summary>
        /// <param name="dPower">容量，单位W</param>
        /// <returns></returns>
        public byte[] Load_RequestCapacity(double dPower)
        {
            bDatas.Clear();
            bCMD = 0x01;
            int iPower=(int)dPower;
            bDatas.Add((byte)(iPower >> 24));
            bDatas.Add((byte)(iPower >> 16));
            bDatas.Add((byte)(iPower >> 8));
            bDatas.Add((byte)(iPower));
            return GetFrameMsg();
        }

        /// <summary>
        /// 释放容量
        /// </summary>
        /// <returns></returns>
        public byte[] Load_ReleaseCapacity()
        {
            bDatas.Clear();
            bCMD = 0x02;
            return GetFrameMsg();
        }


        public byte[] Load_ON()
        {
            bDatas.Clear();
            bCMD = 0x03;
            bDatas.Add(0x01);
            return GetFrameMsg();
        }

        public byte[] Load_OFF()
        {
            bDatas.Clear();
            bCMD = 0x03;
            bDatas.Add(0x02);
            return GetFrameMsg();
        }

        /// <summary>
        /// 设置需求
        /// </summary>
        /// <param name="isCC">是否恒流</param>
        /// <param name="dVoltage">电压</param>
        /// <param name="dCurrent">电流</param>
        /// <returns></returns>
        public byte[] Load_SetParams(bool isCC,double dVoltage, double dCurrent)
        {
            bDatas.Clear();
            bCMD = 0x04;
            if (isCC)
            {
                bDatas.Add(0x02);//恒流
            }
            else
            {
                bDatas.Add(0x01);//恒压
            }
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
            return GetFrameMsg();
        }

        public byte[] Load_GetVersion()
        {
            bDatas.Clear();
            bCMD = 0x40;
            return GetFrameMsg();
        }

        public byte[] Load_GetStateData()
        {
            bDatas.Clear();
            bCMD = 0x42;
            return GetFrameMsg();
        }



    }
}
