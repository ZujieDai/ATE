using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using System.Xml.Linq;
using static NPOI.HSSF.Util.HSSFColor;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    ///  设备 - 周立功功率分析仪（TCP协议端口固定9988）
    /// </summary>
    public class emtPA6000 : EquipMentBase
    {
        private static object SynLock = new object();
        public PowerAnalyzer_StateData stateData = new PowerAnalyzer_StateData();
        private PowerAnalyzer_Protocol protocol = new PowerAnalyzer_Protocol();
        public emtPA6000(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("功率分析仪");
        }

        public byte[] GetMsgBytes(string sMsg)
        {
            sMsg = sMsg + ";";//添加结束符
            byte[] bMsg = new byte[sMsg.Length];
            bMsg = Encoding.ASCII.GetBytes(sMsg);
            return bMsg;
        }

        public override void IntegralClear()
        {
            IntegralStop();
            lock (SynLock)
            {
                string scmd = ":INTEGrate:RESet";
                byte[] buff = GetMsgBytes(scmd);
                SendData(buff);
            }
        }

        public override void IntegralStart()
        {
            lock (SynLock)
            {
                string scmd = ":INTEGrate:STARt";
                byte[] buff = GetMsgBytes(scmd);
                SendData(buff);
            }
        }

        public override void IntegralStop()
        {
            lock (SynLock)
            {
                string scmd = ":INTEGrate:STOP";
                byte[] buff = GetMsgBytes(scmd);
                SendData(buff);
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
        private string sReadDataMsg = ":NUMeric:NORMal:VALue?";//常用读取命令
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
                            int scalingState = 0;
                            byte[] buff;
                            DataBuf.Clear();
                            // string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            lock (SynLock)
                            {
                                //清空接收区
                                RevMsgData = RevEquipMentData();
                                //读取缩放功能开关
                                buff = GetMsgBytes(":INPUT:SCALING:STATE?");
                                EquipMentPort.SendData(buff);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                string sD = Encoding.ASCII.GetString(RevMsgData);
                                //string[] sDatas = sD.Split(',');
                                scalingState = Convert.ToInt32(sD.Substring(0, 1));
                            }
                            lock (SynLock)
                            {
                                //读取数据流程：先设置数据内容和条目（最多255个数据），再读取
                                ////命令1：:NUMeric:NORMal:ITEM1 Urms,1//第1个值，电压 Urms 值，第一通道
                                ////命令1：:NUMeric:NORMal:ITEM2 Irms,1//第2个值，电压 Irms 值，第一通道
                                ////命令1：:NUMeric:NORMal:ITEM3 Pnrm,1//第3个值，电压 Urms 值，第一通道
                                ////命令1：:NUMeric:NORMal:ITEM4 Qnrm,1//第4个值，电压 Urms 值，第一通道
                                ////命令1：:NUMeric:NORMal:ITEM5 LAMBDAnrm,1//第5个值，电压 Urms 值，第一通道
                                ////中间增加通道数据
                                ///
                                ////最后设置数据条目：:NUMeric[:NORMal]:VALue 255//最多255
                                buff = GetMsgBytes(sReadDataMsg);//目前采用默认数据255个
                                EquipMentPort.SendData(buff);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                GetStateData(RevMsgData, 0, ref StateData, ChargerID, scalingState);
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
                    Thread.Sleep(500);
                }
            }
        }

        public PowerAnalyzer_StateData GetStateData(byte[] buffer, int buffType, ref PowerAnalyzer_StateData StateData, int chargerID, int scalingState)
        {
            try
            {
                //设备返回的数据内容示例：
                //    1.2009e+000,8.9782e-003,1.9133e-003,1.0782e-002,1.0611e-002,INF,INF,0.0000e+000,
                //    0.0000e+000,2.6107e+000,-2.2042e+000,3.0576e-002,-1.4074e-002,2.1739e+000,
                //    3.4056e+000,5.9390e-001,5.0790e-003,-3.8887e-004,3.0164e-003,2.9912e-003,
                //    INF,INF,0.0000e+000,0.0000e+000,1.8188e+000,-1.1782e+000,2.1022e-002,-2.0512e-002,
                //    3.0624e+000,4.1390e+000,7.2305e-001,1.5456e-002,2.0767e-003,1.1176e-002,1.0981e-002,
                //    INF,INF,0.0000e+000,0.0000e+000,2.1588e+000,-2.3408e+000,5.7233e-003,-3.5962e-002,
                //    3.2373e+000,2.3267e+000,3.5347e+000,7.5894e-003,-1.5064e-003,2.6826e-002,2.6784e-002,
                //    INF,INF,0.0000e+000,0.0000e+000,7.9093e+000,-7.4255e+000,1.6333e-002,-2.8679e-002,2.2376e+000,
                //    3.7788e+000,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,8.3929e-001,
                //    9.8378e-003,3.6012e-003,2.4974e-002,2.4583e-002,INF,INF,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    4.6156e-001,7.9807e-003,-2.4922e-003,3.6835e-003,2.7125e-003,INF,INF,0.0000e+000,0.0000e+000,
                //    5.7666e-001,-1.3489e+000,2.9645e-002,-1.1986e-002,2.9226e+000,3.7145e+000,2.4085e-001,6.2744e-003,
                //    -6.2272e-005,1.5112e-003,1.5099e-003,INF,INF,0.0000e+000,0.0000e+000,9.4431e-001,-8.7615e-001,
                //    1.4860e-002,-2.3630e-002,3.9207e+000,3.7661e+000,1.4124e+000,7.2815e-003,-4.0608e-003,3.2021e-002,
                //    3.1006e-002,INF,INF,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,NAN,
                //    NAN,NAN,NAN

                if (buffer == null)
                {
                    return StateData;
                }
                StateData.ChargerID = chargerID;
                string sD = Encoding.ASCII.GetString(buffer);
                //string[] sDatas = sD.Split(',');
                string[] ss = sD.Split(',');
                List<string> sDatas = new List<string>();
                sDatas.AddRange(sD.Split(','));
                if (sDatas.Count < 70)
                {
                    return StateData;
                }
                //double powerMultiple = scalingState == 1 ? 1000 : 1;   //功率的倍数，1=打开缩放功能功率单位为W，倍数为1000；0=关闭缩放功能单位为kW，倍数为1
                double powerMultiple = 1000;
                //1通道
                StateData.Channel1RMSVolt = GetFormatData(sDatas[0]);
                StateData.Channel1RMSCurrent = GetFormatData(sDatas[1]);
                StateData.Channel1Power = GetFormatData(sDatas[2]) / powerMultiple;
                StateData.Channel1ReactivePower = GetFormatData(sDatas[4]) / powerMultiple;
                StateData.Channel1PowerFactor = GetFormatData(sDatas[5]);

                //2通道
                StateData.Channel2RMSVolt = GetFormatData(sDatas[15]);
                StateData.Channel2RMSCurrent = GetFormatData(sDatas[16]);
                StateData.Channel2Power = GetFormatData(sDatas[17]) / powerMultiple;
                StateData.Channel2ReactivePower = GetFormatData(sDatas[19]) / powerMultiple;
                StateData.Channel2PowerFactor = GetFormatData(sDatas[20]);

                //3通道
                StateData.Channel3RMSVolt = GetFormatData(sDatas[30]);
                StateData.Channel3RMSCurrent = GetFormatData(sDatas[31]);
                StateData.Channel3Power = GetFormatData(sDatas[32]) / powerMultiple;
                StateData.Channel3ReactivePower = GetFormatData(sDatas[34]) / powerMultiple;
                StateData.Channel3PowerFactor = GetFormatData(sDatas[35]);

                //4通道
                StateData.Channel4RMSVolt = GetFormatData(sDatas[45]);
                StateData.Channel4RMSCurrent = GetFormatData(sDatas[46]);
                StateData.Channel4Power = GetFormatData(sDatas[47]) / powerMultiple;
                StateData.Channel4ReactivePower = GetFormatData(sDatas[49]) / powerMultiple;
                StateData.Channel4PowerFactor = GetFormatData(sDatas[50]);

                //5通道
                //StateData.Channel5RMSVolt = GetFormatData(sDatas[60]);
                //StateData.Channel5RMSCurrent = GetFormatData(sDatas[61]);
                //StateData.Channel5Power = GetFormatData(sDatas[62]) / powerMultiple;
                //StateData.Channel5ReactivePower = GetFormatData(sDatas[64]) / powerMultiple;
                //StateData.Channel5PowerFactor = GetFormatData(sDatas[65]);

                //6通道
                //StateData.Channel6RMSVolt = GetFormatData(sDatas[75]);
                //StateData.Channel6RMSCurrent = GetFormatData(sDatas[76]);
                //StateData.Channel6Power = GetFormatData(sDatas[77]) / powerMultiple;
                //StateData.Channel6ReactivePower = GetFormatData(sDatas[79]) / powerMultiple;
                //StateData.Channel6PowerFactor = GetFormatData(sDatas[80]);

                //三相总
                StateData.TotalVoltage = GetFormatData(sDatas[75]);
                StateData.TotalCurrent = GetFormatData(sDatas[76]);
                StateData.TotalPower = GetFormatData(sDatas[77]) / powerMultiple;
                StateData.TotalPowerFactor = GetFormatData(sDatas[80]);
                return StateData;
            }
            catch { return StateData; }
        }
        private double GetFormatData(string sData)
        {
            double dtmp = 0;
            if(sData== "INF" || sData == "NAN")
            {
                return 0;
            }
            try
            {
                if (sData.Contains("E") || sData.Contains("e"))
                {
                    //9.512E+00，把字符串转换成数字
                    decimal ddd = 0;
                    sData = sData.Trim(new char[] { '\n', '\r', 'S', 's', 'H', 'h', 'Z', 'z', 'V', 'A', 'v', 'a', '%' });
                    decimal.TryParse(sData, System.Globalization.NumberStyles.Float, null, out ddd);
                    ddd = Math.Round(ddd, 4);//最大4位小数，小数位太多系统会用科学计数法
                    dtmp = (double)ddd;
                }
                else
                {
                    double.TryParse(sData, out dtmp);
                }
            }
            catch(Exception ex)
            {

            }
            return dtmp;
        }


        /// <summary>
        /// 设置缩放功能开关
        /// </summary>
        /// <param name="state">关闭=0，打开=1</param>
        public override void SetScalingState(int state)
        {
            byte[] buff;
            if (state == 1)
            {
                buff = GetMsgBytes(":INPUT:SCALING:STATE:ALL ON");
                EquipMentPort.SendData(buff);
            }
            else if(state == 0)
            {
                buff = GetMsgBytes(":INPUT:SCALING:STATE:ALL OFF");
                EquipMentPort.SendData(buff);
            }
        }
    }
}
