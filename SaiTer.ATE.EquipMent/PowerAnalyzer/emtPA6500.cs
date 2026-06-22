using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using NPOI.SS.Formula.Functions;
using System.Configuration;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    ///  设备 - 信创功率分析仪
    /// </summary>
    public class emtPA6500 : EquipMentBase
    {
        private static object SynLock = new object();
        public PowerAnalyzer_StateData stateData = new PowerAnalyzer_StateData();
        private PowerAnalyzer_Protocol protocol = new PowerAnalyzer_Protocol();
        public emtPA6500(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("功率分析仪");
        }

        public override void IntegralClear()
        {
            IntegralStop();
            lock (SynLock)
            {
                byte[] buff = { 0x01, 0x06, 0x24, 0x00, 0x00, 0x00, 0x83, 0x3A };//4通道清零积分
                SendData(buff);
            }
        }

        public override void IntegralStart()
        {
            lock (SynLock)
            {
                byte[] buff = { 0x01 ,0x06 ,0x24 ,0x00 ,0x00 ,0x01 ,0x42 ,0xFA };//4通道开始积分
                SendData(buff);
            }
        }

        public override void IntegralStop()
        {
            lock (SynLock)
            {
                byte[] buff = { 0x01, 0x06, 0x24, 0x00, 0x00, 0x02, 0x02, 0xFB };//4通道停止积分
                SendData(buff);
            }
        }

        public override void Integral123Start(int iState)
        {
            lock (SynLock)
            {
                PowerAnalyzer_Protocol pp = new PowerAnalyzer_Protocol();
                //SendData(pp.PA6500BytesSetIntegral1);
                //Thread.Sleep(100);
                //SendData(pp.PA6500BytesSetIntegral2);
                //Thread.Sleep(100);
                //SendData(pp.PA6500BytesSetIntegral3);
                SendData(pp.PA6500SetIntegral123(iState));
            }
        }

        public override void SetChannelRatio(int iCH, bool isU, int iRatio)
        {
            lock (SynLock)
            {
                PowerAnalyzer_Protocol pp = new PowerAnalyzer_Protocol();
                SendData(pp.PA6500SetChannelRatio(iCH, isU, iRatio));
                Thread.Sleep(100);
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && (Customer.Contains("HK")))//目前只有北京HK的才需要改量程
                {
                    if (iRatio <= 30)//目前用于待机功耗HK
                    {
                        SendData(pp.PA6500SetChannelRange(iCH, 1));//设置通道量程：目前默认5mA
                    }
                    else
                    {
                        SendData(pp.PA6500SetChannelRange(iCH, 3));//设置通道量程：目前默认50mA
                    }
                }
                else if(Customer != null && (Customer.Contains("YTZL_ACDC")))
                {
                    if (iRatio == 1)//目前用于待机功耗HK
                    {
                        SendData(pp.PA6500SetChannelRange(iCH, 6));//设置通道量程：目前默认500mA
                    }
                    else
                    {
                        SendData(pp.PA6500SetChannelRange(iCH, 3));//设置通道量程：目前默认50mA
                    }
                }

                Thread.Sleep(100);
            }
        }

        public override double ReadIntegralValue()
        {
          
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = { 0x01, 0x03, 0x14, 0x19, 0x00, 0x02, 0x10, 0x3C };//4通道积分回读
                //byte[] buff = { 0x01, 0x03, 0x14, 0x19, 0x00, 0x01, 0x50, 0x3D };//4通道积分回读
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double fy = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                fy = Math.Round(fy, 2);
                return fy;
            }
        }

        public override double ReadCurrentHarmonicValue() 
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = { 0x01, 0x03, 0x14, 0x2D, 0x00, 0x02, 0x51, 0xF2 };//4通道电流谐波含量
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double fy = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                fy = Math.Round(fy, 2);
                return fy;
            }
        }

        public override double ReadDcComponentVoltage(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadDcComponentVoltage(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                dv = Math.Round(dv, 2);
                return dv;
            }
        }

        public override double ReadDcComponentCurrent(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadDcComponentCurrent(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                dv = Math.Round(dv/1000, 2);//这里的电流单位是mA 转换为A
                return dv;
            }
        }

        public override double ReadFreq(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadFreq(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                dv = Math.Round(dv, 2);//这里的电流单位是mA 转换为A
                return dv;
            }
        }

        public override List<double> ReadCurrentHarmonicValue_50(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadCurrentHarmonicValue_50(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                List<double> lstV = new List<double>();
                if (buffer.Length > 103)
                {
                    for (int i = 0; i < 50; i++)//这里固定的是50个数据
                    {
                        string data = buffer[(i * 2) + 3].ToString("x2") + buffer[(i * 2) + 4].ToString("x2");
                        UInt32 x = Convert.ToUInt32(data, 16);
                        //double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                        double dv = (double)x;
                        dv = Math.Round(dv / 100, 2);//这里数据要缩小100倍
                        lstV.Add(dv);
                    }
                }
                return lstV;
            }
        }

        public override List<double> ReadVoltageHarmonicValue_50(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadVoltageHarmonicValue_50(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                List<double> lstV = new List<double>();
                if (buffer.Length > 103)
                {
                    for (int i = 0; i < 50; i++)//这里固定的是50个数据
                    {
                        string data = buffer[(i * 2) + 3].ToString("x2") + buffer[(i * 2) + 4].ToString("x2");
                        UInt32 x = Convert.ToUInt32(data, 16);
                        //double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                        double dv = (double)x;
                        dv = Math.Round(dv / 100, 2);//这里数据要缩小100倍
                        lstV.Add(dv);
                    }
                }
                return lstV;
            }
        }

        public override void SetHarmonicState(int iCH, bool isON)
        {
            lock (SynLock)
            {
                SendData(protocol.PA6500SetHarmonicState(iCH, isON));
            }
        }


        public override double ReadCurrentHarmonic_Total(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadCurrentHarmonic_Total(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                dv = Math.Round(dv, 2);
                return dv;
            }
        }

        public override double ReadVoltageHarmonic_Total(int iCH)
        {
            Thread.Sleep(100);
            lock (SynLock)
            {
                RevEquipMentData();
                byte[] buff = protocol.PA6500ReadVoltageHarmonic_Total(iCH);
                EquipMentPort.SendData(buff);
                byte[] buffer = RevEquipMentData();
                string data = buffer[3].ToString("x2") + buffer[4].ToString("x2") + buffer[5].ToString("x2") + buffer[6].ToString("x2");
                UInt32 x = Convert.ToUInt32(data, 16);
                double dv = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                ////浮点数转16进制
                //byte[] bytes = BitConverter.GetBytes(fy);
                dv = Math.Round(dv, 2);
                return dv;
            }
        }

        public override void ReadPA6500_StateData()
        {
            stateData.ChargerID = ChargerID;
            SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicPowerAnalyzer_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicPowerAnalyzer_StateData.Add(ChargerID, stateData);
            }
            PowerAnalyzer_StateData StateData = new PowerAnalyzer_StateData();
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
                            // string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                EquipMentPort.SendData(protocol.PA6500Bytes0);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {

                                //if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                //{
                                //    EquipMentPort.SendData(protocol.PA6500Bytes0);
                                //    RevMsgData = RevEquipMentData();
                                //}
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 0, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes1);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 1, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes2);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 2, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes3);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 3, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes4);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 4, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes5);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 5, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes6);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 6, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                //lock (SynLock)
                                //{
                                //    EquipMentPort.SendData(protocol.PA6500Bytes7);
                                //    RevMsgData = RevEquipMentData();
                                //}
                                //if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                //{
                                //    protocol.GetStateData(RevMsgData, 7, ref StateData, ChargerID);
                                //    SystemEvent.SendMonitorMessage(StateData);
                                //}
                                //返回数据为空
                                StateData.TotalVoltage = (StateData.Channel1RMSVolt + StateData.Channel2RMSVolt + StateData.Channel3RMSVolt) / 3;
                                StateData.TotalCurrent = (StateData.Channel1RMSCurrent + StateData.Channel2RMSCurrent + StateData.Channel3RMSCurrent) / 3;
                                StateData.TotalPower = StateData.Channel1Power + StateData.Channel2Power + StateData.Channel3Power;
                                StateData.TotalPowerFactor = (StateData.Channel1PowerFactor + StateData.Channel2PowerFactor + StateData.Channel3PowerFactor) / 3;
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes8);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 8, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes9);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 9, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes10);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 10, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }

                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes11);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 11, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }

                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes12);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 12, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }

                                lock (SynLock)
                                {
                                    EquipMentPort.SendData(protocol.PA6500Bytes13);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
                                {
                                    protocol.GetStateData(RevMsgData, 13, ref StateData, ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                }
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                stateData = new PowerAnalyzer_StateData();
                                stateData.ChargerID = this.ChargerID;
                                SystemEvent.SendMonitorMessage(stateData);
                                SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                        stateData = new PowerAnalyzer_StateData();
                        SystemEvent.SendConnectState(false, this);
                        SystemEvent.SendMonitorMessage(StateData);
                    }
                    Thread.Sleep(100);
                }
            }
        }
    }
}
