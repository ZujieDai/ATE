using NationalInstruments.VisaNS;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
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
    /// 设备-泰克DMM6500万用表
    /// </summary>
    public class emtDMM6500 : EquipMentBase
    {
        private MultiMeter_StateData StateData = new MultiMeter_StateData();
        public emtDMM6500(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("万用表");
        }


        public override void Read_MultiMeterState()
        {
            SystemEvent.SendConnectState(false, this);

            if (!AllEquipStateData.MultiMeter_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.MultiMeter_StateData.Add(ChargerID, StateData);
            }
            TCPClient tcp = EquipMentPort as TCPClient;
            if (tcp == null)
            {
                return;
            }
            tcp.Close();
            tcp.blConntOk = tcp.Open();
            while (true)
            {
                if (tcp.blConntOk)
                {
                    try
                    {
                        if (this.AutoReadData)
                        {
                            this.AutoReadData = false;
                            RevEquipMentData();
                            EquipMentPort.SendData(Encoding.ASCII.GetBytes(":READ?\n"));
                            Thread.Sleep(300);
                            byte[] RevMsgData = RevEquipMentData();
                            this.AutoReadData = true;
                            if (RevMsgData != null)
                            {
                                string strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                                StateData.OutPutVoltage = Convert.ToDouble(CommonOscilloscope.BackTryDecimalParse(strResult.Trim(new char[] { '\n', '\r' })));
                                StateData.ChargerID = ChargerID;
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
                            }
                        }
                    }
                    catch
                    {
                        StateData.ChargerID = ChargerID;
                        SystemEvent.SendMonitorMessage(StateData);
                        tcp.blConntOk = false;
                        SystemEvent.SendConnectState(false, this);
                    }

                }
                else
                {
                    try
                    {
                        if (this.AutoReadData)
                        {
                            tcp.Close();
                            tcp.blConntOk = tcp.Open();
                            System.Threading.Thread.Sleep(50);
                            EquipMentPort.SendData(Encoding.ASCII.GetBytes(":SENS:FUNC \"VOLT:DC\"\n"));
                            System.Threading.Thread.Sleep(50);
                            SystemEvent.SendConnectState(tcp.blConntOk, this);
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }

                Thread.Sleep(300);

            }
        }
    }
}
