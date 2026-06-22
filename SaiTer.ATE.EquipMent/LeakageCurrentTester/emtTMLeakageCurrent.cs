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
using System.Net;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-拓勉漏电流测试仪
    /// </summary>
    public class emtTMLeakageCurrent : EquipMentBase
    {
        public ZJLeakageCurrent_StateData stateData = new ZJLeakageCurrent_StateData();
        private LeakageCurrent_Protocol protocol = new LeakageCurrent_Protocol();
        private static object SynLock = new object();
        public emtTMLeakageCurrent(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "TM" + " " + LanguageManager.GetByKey("剩余电流保护测试仪");
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
            byte[] WriteBuffer = protocol.GetBuffer(address, param, 0x03);
            byte[] RevMsgData;
            SendData(WriteBuffer, out RevMsgData);
            data = GetData(RevMsgData);
            return data;
        }

        private string GetData(byte[] buffer)
        {
            string data = "";
            try
            {
                if (buffer == null)
                {
                    return null;
                }

                int tempdata = 0;
                int Testtype = 0;//测试类型
                int Test_result = 0;//测试结果
                if (buffer[1] == 3)//读命令
                {

                    tempdata = buffer[2];
                    Testtype = buffer[5] * 256 + buffer[6];
                    Test_result = buffer[7] * 256 + buffer[8];


                    if (Testtype == 1)//脱扣电流
                    {
                        if (Test_result == 65535)//超时
                        {
                            data = "脱扣电流超时";
                        }
                        else//脱扣电流值
                        {
                            data = Test_result / 10 + "." + Test_result % 10 + "mA";
                        }
                    }
                    else if (Testtype == 2)//S2突现时间&不驱动时间
                    {
                        if (Test_result == 65535)//不驱动时间：误动
                        {
                            data = "不驱动误动";
                        }
                        else if (Test_result == 65534)//不驱动时间：超时
                        {
                            data = "不驱动超时";
                        }
                        else if (Test_result == 65533)//S2突现时间：超时
                        {
                            data = "S2超时";
                        }
                        else//S2突现时间：脱扣时间值
                        {
                            data = Test_result / 10 + "." + Test_result % 10 + "ms";
                        }
                    }
                    else if (Testtype == 7)//S1突现时间
                    {
                        if (Test_result == 65535)//超时
                        {
                            data = "S1超时";
                        }
                        else//S1突现时间：脱扣时间值
                        {
                            data = Test_result / 10 + "." + Test_result % 10 + "ms";
                        }
                    }
                    else if (Testtype == 3)//闭合时间
                    {
                        if (Test_result == 65535)//超时
                        {
                            data = "闭合超时";
                        }
                        else//闭合时间：脱扣时间值
                        {
                            if (Test_result == 1)
                            {
                                data = Test_result / 10 + "." + Test_result % 10 + "ms";
                            }
                            else
                            {
                                data = Test_result / 10 + "." + Test_result % 10 + "ms";
                            }
                        }

                    }

                }
            }
            catch (Exception e) { Log.Log.LogException(e); }
            return data;
        }

        private bool SendData(byte[] WriteBuffer, out byte[] RevMsgData)
        {
            lock (SynLock)
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

                            RevEquipMentData();


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
                                    RevMsgData = RevEquipMentData();

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
        }

        public override void LeakageCurrent_ReadState()
        {
            SystemEvent.SendConnectState(false, this);
            LeakageCurrent_SetParams(37, 1);//跳转到主程序（联机指令）

            while (true)
            {

                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = protocol.GetBuffer(0x6A, 0x0B, 3);//读FLASH

                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            lock (SynLock)
                            {
                                RevEquipMentData();
                                string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                List<byte> nTemp = RevMsgData.ToList();
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                {
                                    lock (SynLock)
                                    {
                                        EquipMentPort.SendData(WriteBuffer);
                                        RevMsgData = RevEquipMentData();
                                    }
                                }
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                LeakageCurrent_SetParams(37, 1);//跳转到主程序（联机指令）
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                        LeakageCurrent_SetParams(37, 1);//跳转到主程序（联机指令）
                        SystemEvent.SendConnectState(false, this);
                    }
                    Thread.Sleep(300);
                }
            }
        }

    }
}
