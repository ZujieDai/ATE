using SaiTer.ATE.DataModel;
using SaiTer.ATE.Manage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcResisLoad : UcEquipOperateBase
    {
        public UcResisLoad()
        {
            InitializeComponent();
        }

        private void UcResisLoad_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnSetVoltCur_Click(object sender, EventArgs e)
        {
            BusinessManage BCM = BusinessManage.GetInstance();
            //    double Current = 0;
            //    if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
            //    {
            //        if (AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseA_Voltage > 70
            //              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseB_Voltage > 70
            //              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseC_Voltage > 70)
            //        {
            //            Current = Convert.ToDouble(txtCurrent.Text);
            //        }
            //        else
            //        {
            //            Current = Convert.ToDouble(txtCurrent.Text) / 3;
            //        }
            //    }

            try
            {
                //防止配置文件没有此项或者值不对
                int index = BCM._xmlInfoAssembly._systemXmlInfo.RelayIndex - 1;
                if (index < 0) //不需要软件控制负载单、三相并机功能
                {
                    EquipMentControl.ResistanceLoad.SetResisLoadVolCurr(lstChargerID, Convert.ToDouble(txtVolt.Text), Convert.ToDouble(txtCurrent.Text));
                    Thread.Sleep(1000);
                    EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            string Customer = ConfigurationManager.AppSettings["Customer"];
            //2026/4/23 硬件确认不需要停止负载再进行需求修改
            //if (Customer == null || !Customer.ToString().ToUpper().Contains("HYQCP"))
            //    EquipMentControl.ResistanceLoad.ResistanceLoad_OFF(lstChargerID);
            //需要软件控制负载单、三相并机功能
            if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
            {
                if (EquipMentControl.ControlBoard.DitEquipMentBase.Where(eq => eq.Value.EquipMentClassName.Equals("emtDIORelay")).ToArray().Length > 0)
                {
                    string isControlDIO_AC = ConfigurationManager.AppSettings["isControlDIO_AC"];
                    if (isControlDIO_AC != null && Convert.ToBoolean(isControlDIO_AC))
                    {
                        //TS是Y1三相，Y2单相
                        if (new string[] { "TS", "SKY" }.Contains(Customer))
                        {
                            if (AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseA_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseB_Voltage < 50
                              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseC_Voltage < 50)
                            {
                                EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID}, 0, false);
                                Thread.Sleep(1000);
                                EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 1, true);
                            }
                            else
                            {
                                EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 1, false);
                                Thread.Sleep(1000);
                                EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 0, true);
                            }
                        }
                        else
                            EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 2, true);
                    }
                    else
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseA_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseB_Voltage < 50
                              && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseC_Voltage < 50)
                        {
                            EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 2, false);
                            Thread.Sleep(1000);
                            EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 3, true);
                        }
                        else
                        {
                            EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 3, false);
                            Thread.Sleep(1000);
                            EquipMentControl.ControlBoard.SetRelaySwitch(new List<int> { this.ChargerID }, 2, true);
                        }
                    }
                }
                else
                {
                    int relayIndex = BCM._xmlInfoAssembly._systemXmlInfo.RelayIndex - 1;
                    //List<bool> lstRelay = new List<bool>();

                    //for (int i = 0; i < 16; i++)
                    //{
                    //    lstRelay.Add(false);
                    //}
                    //lstRelay[0] = true;
                    List<bool> lstRelay = EquipMentControl.ControlBoard.ControlBoardReadState();

                    if (AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseA_Voltage > 70
                          && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseB_Voltage < 50
                          && AllEquipStateData.DicBMS_AC_StateData[lstChargerID[0]].PhaseC_Voltage < 50)
                    {
                        //三相闭合
                        lstRelay[relayIndex] = true;

                    }
                    else
                    {
                        //单相断开
                        lstRelay[relayIndex] = false;
                    }
                    EquipMentControl.ControlBoard.ControlResistanceSetRelay(lstRelay);
                    Thread.Sleep(300);
                }
            }

            EquipMentControl.ResistanceLoad.SetResisLoadVolCurr(lstChargerID, Convert.ToDouble(txtVolt.Text), Convert.ToDouble(txtCurrent.Text));
            //Thread.Sleep(1000);
            //EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
        }

        private void btnCurrent_Click(object sender, EventArgs e)
        {

        }

        private void btnFreq_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.SetResisLoadPower(lstChargerID, Convert.ToDouble(txtVolt.Text), Convert.ToDouble(txtPower.Text));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.ResistanceLoad_OFF(lstChargerID);
        }
    }
}
