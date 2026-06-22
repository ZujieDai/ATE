using NationalInstruments.VisaNS;
using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage;
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
    /// 设备——横河功率计WT333E
    /// </summary>
    public class emtWT333E : EquipMentBase
    {
        public PowerAnalyzer_StateData stateData = new PowerAnalyzer_StateData();
        public bool isInit = false;//是否初始化
        public byte[] Msgs = null;

        public emtWT333E(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("功率分析仪");
        }

        public byte[] GetMsg(string sCMD)
        {
            try
            {
                List<byte> msgList = new List<byte>();
                msgList.AddRange(Encoding.UTF8.GetBytes(sCMD));
                msgList.Add(0x0A);//结束符
                return msgList.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void SetInit()
        {
            string scmd = "";
            scmd = ":NUMERIC:FORMAT ASCII";//设置数据输出格式
            Msgs=GetMsg(scmd);
            EquipMentPort.SendData(Msgs);
            Thread.Sleep(200);
            scmd = ":NUMERIC:NORMAL:PRESET 2";//预设数据输出项
            Msgs = GetMsg(scmd);
            EquipMentPort.SendData(Msgs);
            Thread.Sleep(200);
            scmd = ":NUMERIC:NORMAL:NUMBER 40";//设置数据输出数量40
            Msgs = GetMsg(scmd);
            EquipMentPort.SendData(Msgs);
            Thread.Sleep(200);
            isInit = true;//初始化完成
            SendMsgToFile(EquipMentName + "WT333E设备初始化");
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
                try
                {
                    if (AutoReadData)
                    {

                        byte[] RevMsgData = null;
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                //Rs232通讯方式
                                byte[] bytes = { 0x01, 0x04, 0x01, 0x01, 0x00, 0x02, 0x21, 0xF7 };

                                if(!isInit)
                                {
                                    SetInit();//初始化
                                }

                                string sCMD = ":NUMERIC:NORMAL:VALUE?";
                                bytes=GetMsg(sCMD);
                                EquipMentPort.SendData(bytes);
                                RevMsgData = RevEquipMentData();

                                if (RevMsgData != null /*&& CheckOut.CheckModbusCrc16_High_Right(RevMsgData)*/)
                                {

                                    stateData = new PowerAnalyzer_StateData();
                                    stateData.ChargerID = this.ChargerID;
                                    string sRetString=Encoding.ASCII.GetString(RevMsgData);
                                    string[] sDatas = sRetString.Split(',');
                                    List<double> dDatas = new List<double>();
                                    int iindex = 1;
                                    double dtmp = 0;
                                    //StringBuilder sbtmp = new StringBuilder();
                                    foreach (string sData in sDatas)
                                    {
                                        dtmp = (double)ContentEDataChangeNum_D(sData);
                                        dDatas.Add(dtmp);
                                        //sbtmp.Append(sData+"==");
                                        SetPAData(ref stateData, iindex, dtmp);
                                        iindex++;
                                    }

                                    SystemEvent.SendMonitorMessage(stateData);
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
                catch (Exception e) { Log.Log.LogException(e); }
            }
        }
        public void SetPAData(ref PowerAnalyzer_StateData pad,int iindex,double dData)
        {
            switch(iindex)
            {
                case 1:
                    pad.Channel1RMSVolt = dData;
                    break;
                case 2:
                    pad.Channel1RMSCurrent = dData;
                    break;
                case 3:
                    pad.Channel1Power = dData/1000;
                    break;
                case 5:
                    pad.Channel1ReactivePower = dData / 1000;
                    break;
                case 6:
                    pad.Channel1PowerFactor = dData;
                    break;
                case 7:
                    //pad.Channel1ElectricEnergy = dData;//目前没有
                    break;

                case 11:
                    pad.Channel2RMSVolt = dData;
                    break;
                case 12:
                    pad.Channel2RMSCurrent = dData;
                    break;
                case 13:
                    pad.Channel2Power = dData / 1000;
                    break;
                case 15:
                    pad.Channel2ReactivePower = dData / 1000;
                    break;
                case 16:
                    pad.Channel2PowerFactor = dData;
                    break;
                case 17:
                    //pad.Channel2ElectricEnergy = dData;//目前没有
                    break;

                case 21:
                    pad.Channel3RMSVolt = dData;
                    break;
                case 22:
                    pad.Channel3RMSCurrent = dData;
                    break;
                case 23:
                    pad.Channel3Power = dData / 1000;
                    break;
                case 25:
                    pad.Channel3ReactivePower = dData / 1000;
                    break;
                case 26:
                    pad.Channel3PowerFactor = dData;
                    break;
                case 27:
                    //pad.Channel3ElectricEnergy = dData;//目前没有
                    break;

                case 31:
                    pad.TotalVoltage = dData;
                    break;
                case 32:
                    pad.TotalCurrent = dData;
                    break;
                case 33:
                    pad.TotalPower = dData / 1000;
                    break;
                case 35:
                    //pad.tot = dData;
                    break;
                case 36:
                    pad.TotalPowerFactor = dData;
                    break;
                case 37:
                    //pad.Channel3ElectricEnergy = dData;//目前没有
                    break;
            }
        }

        public  decimal ContentEDataChangeNum_D(string GetStr)
        {
            string StrReturn = "";
            Decimal dData = 0;//十进制的数值

            if (GetStr.Contains("E") || GetStr.Contains("e"))
            {
                dData = BackTryDecimalParse(GetStr);//9.512E+00，把字符串转换成数字

                StrReturn = dData.ToString();
            }

            try
            {
                return Convert.ToDecimal(StrReturn);
            }
            catch
            {
                return 0;
            }


        }
        /// <param name="GNumFloat"></param>
        /// <returns></returns>
        //因为防止报错，所以使用了TryParse，而没有用Parse
        //字符串转数字。
        public Decimal BackTryDecimalParse(string GNumFloat)
        {
            GNumFloat = GNumFloat.Trim(new char[] { '\n', '\r', 'S', 's', 'H', 'h', 'Z', 'z', 'V', 'A', 'v', 'a', '%' });
            //string BackString = "";
            Decimal BacknumFloat;
            Decimal numFloat;
            Decimal.TryParse(GNumFloat, out numFloat);
            Decimal.TryParse(GNumFloat, System.Globalization.NumberStyles.Float, null, out numFloat);
            BacknumFloat = numFloat;

            return BacknumFloat;
        }


        private double DataParse(byte[] RevMsgData, int startIndex, int count)
        {
            //string data = RevMsgData[3].ToString("x2") + RevMsgData[4].ToString("x2") + RevMsgData[5].ToString("x2") + RevMsgData[6].ToString("x2");
            string data = "";
            for (int i = startIndex; i < count + startIndex; i++)
            {
                data += RevMsgData[i].ToString("x2");
            }
            UInt32 x = Convert.ToUInt32(data, 16);
            double fy = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
            //浮点数转16进制
            fy = Math.Round(fy, 3);
            return fy;
        }
    }
}
