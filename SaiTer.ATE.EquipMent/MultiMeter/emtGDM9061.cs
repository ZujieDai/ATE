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
    /// 设备-固纬GDM9061万用表
    /// </summary>
    public class emtGDM9061 : EquipMentBase
    {
        private MultiMeter_StateData StateData = new MultiMeter_StateData();
        public emtGDM9061(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("万用表");
        }

        public override void Read_MultiMeterState()
        {
            SystemEvent.SendConnectState(false, this);
            TCPClient tcp = EquipMentPort as TCPClient;

            if (!AllEquipStateData.MultiMeter_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.MultiMeter_StateData.Add(ChargerID, StateData);
            }
           
            while (true)
            {

                if (EquipMentPort != null)
                {
                    if (tcp.blConntOk)
                    {
                        try
                        {

                            if (this.AutoReadData)
                            {
                                //tcp.VisaNS.Write("CONF:VOLT:DC 1000,MAX\n
                                
                                tcp.VisaNS.Write("VAL1?\n");
                                string ReturnString = tcp.VisaNS.ReadString(16).Trim(new char[] { '\n', '\r' }); 
                                StateData.OutPutVoltage = Convert.ToDouble(CommonOscilloscope.BackTryDecimalParse(ReturnString));
                                StateData.ChargerID = ChargerID;
                                SystemEvent.SendMonitorMessage(StateData);
                                SystemEvent.SendConnectState(true, this);
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
                                string tPort = "TCPIP0::" + EquipMentPort.Ipaddress + "::4000::SOCKET";
                                tcp.VisaNS = (MessageBasedSession)ResourceManager.GetLocalManager().Open(tPort);
                                tcp.blConntOk = true;
                                SystemEvent.SendConnectState(true, this);
                                tcp.VisaNS.Write("CONF:VOLT:AC\n");
                                System.Threading.Thread.Sleep(50);
                            }

                        }
                        catch (Exception ex)
                        {
                            // Log.Log.LogException(ex); //网络断开或者IP不对才会报这个错，UI会显示断开，不用写日志
                            tcp.blConntOk = false;
                            SystemEvent.SendConnectState(false, this);
                            StateData.ChargerID = ChargerID;
                            SystemEvent.SendMonitorMessage(StateData);
                        }
                    }
                }

                Thread.Sleep(300);

            }
        }
    }
}
