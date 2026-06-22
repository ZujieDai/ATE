using NationalInstruments.VisaNS;
using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备——功率分析仪WT5000
    /// </summary>
    public class emtWT5000 : EquipMentBase
    {
        public PowerAnalyzer_StateData stateData = new PowerAnalyzer_StateData();

        public emtWT5000(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("功率分析仪");
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

                    ushort[] RevMsgData = null;
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(101, 12);
                            //byte[] bytes = { 0x01, 0x04, 0x00, 0x71, 0x00, 0x18, 0xA0, 0x1B };  //读24个寄存器
                            //RevMsgData = RevEquipMentData();

                            if (RevMsgData != null)
                            {

                                //if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                //{
                                //    EquipMentPort.SendData(protocol.PA6500Bytes0);
                                //    RevMsgData = RevEquipMentData();
                                //}

                                stateData = new PowerAnalyzer_StateData();
                                stateData.ChargerID = this.ChargerID;
                                uint combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];                
                                stateData.Channel1RMSVolt = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[2] << 16) | RevMsgData[3];
                                stateData.Channel1RMSCurrent = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[4] << 16) | RevMsgData[5];
                                stateData.Channel1Power = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3) / 1000.0;
                                combinedValue = ((uint)RevMsgData[8] << 16) | RevMsgData[9];
                                stateData.Channel1ReactivePower = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[10] << 16) | RevMsgData[11];
                                stateData.Channel1PowerFactor = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

                                RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(201, 12);
                                combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];
                                stateData.Channel2RMSVolt = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[2] << 16) | RevMsgData[3];
                                stateData.Channel2RMSCurrent = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[4] << 16) | RevMsgData[5];
                                stateData.Channel2Power = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3) / 1000.0;
                                combinedValue = ((uint)RevMsgData[8] << 16) | RevMsgData[9];
                                stateData.Channel2ReactivePower = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[10] << 16) | RevMsgData[11];
                                stateData.Channel2PowerFactor = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

                                RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(301, 12);
                                combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];
                                stateData.Channel3RMSVolt = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[2] << 16) | RevMsgData[3];
                                stateData.Channel3RMSCurrent = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[4] << 16) | RevMsgData[5];
                                stateData.Channel3Power = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3) / 1000.0;
                                combinedValue = ((uint)RevMsgData[8] << 16) | RevMsgData[9];
                                stateData.Channel3ReactivePower = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[10] << 16) | RevMsgData[11];
                                stateData.Channel3PowerFactor = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

                                RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(401, 12);
                                combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];
                                stateData.Channel4RMSVolt = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[2] << 16) | RevMsgData[3];
                                stateData.Channel4RMSCurrent = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[4] << 16) | RevMsgData[5];
                                stateData.Channel4Power = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3) / 1000.0;
                                combinedValue = ((uint)RevMsgData[8] << 16) | RevMsgData[9];
                                stateData.Channel4ReactivePower = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[10] << 16) | RevMsgData[11];
                                stateData.Channel4PowerFactor = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

                                RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(3, 2);
                                combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];
                                stateData.Efficiency = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

                                RevMsgData = ((PortManage.PortType.ModbusTCPClient)EquipMentPort).ReadData(1001, 12);
                                combinedValue = ((uint)RevMsgData[0] << 16) | RevMsgData[1];
                                stateData.TotalVoltage = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[2] << 16) | RevMsgData[3];
                                stateData.TotalCurrent = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);
                                combinedValue = ((uint)RevMsgData[4] << 16) | RevMsgData[5];
                                stateData.TotalPower = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3) / 1000.0;
                                combinedValue = ((uint)RevMsgData[10] << 16) | RevMsgData[11];
                                stateData.TotalPowerFactor = Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(combinedValue), 0), 3);

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
        }


        private double DataParse(ushort[] RevMsgData, int startIndex, int count)
        {
            //string data = RevMsgData[3].ToString("x2") + RevMsgData[4].ToString("x2") + RevMsgData[5].ToString("x2") + RevMsgData[6].ToString("x2");
            string data = "";
            for(int i = startIndex; i < count + startIndex; i++)
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
