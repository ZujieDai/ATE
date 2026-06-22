using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    /// <summary>
    /// 日标直流BMS互操作
    /// </summary>
    public partial class UcBMS_JP_DC_Intero : UcEquipOperateBase
    {
        bool isAuto = true;
        public UcBMS_JP_DC_Intero()
        {
            InitializeComponent();
        }

        private void UcBMS_JP_DC_Intero_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnR4_Click(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMSSetResistance(lstChargerID, Convert.ToDouble(txtR4.Text), new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void btnVolt_Click(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMSSetBatteryVoltage(lstChargerID, Convert.ToDouble(txtVolt.Text), new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void rbtn_0_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                byte[] DCBMSBitS = SetDCPRState();
                EquipMentControl.BMS.BMSSetKState_DC(lstChargerID, 0x83, DCBMSBitS, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void rbtn_8_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                byte[] DCBMSBitS = SetDCMRState();
                EquipMentControl.BMS.BMSSetKState_DC(lstChargerID, 0x84, DCBMSBitS, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private byte[] SetDCPRState()
        {
            byte index = 0;
            foreach (Control ctrl in tpnl1.Controls)
            {
                UIRadioButton r = ctrl as UIRadioButton;
                if (r.Checked)
                {
                    index = Convert.ToByte(r.Name.Split('_')[1]);
                }
            }
            return new byte[] { 0, 0, 0, index };
        }

        private byte[] SetDCMRState()
        {
            byte index = 0;
            foreach (Control ctrl in tpnl2.Controls)
            {
                UIRadioButton r = ctrl as UIRadioButton;
                if (r.Checked)
                {
                    index = (byte)(Convert.ToInt32(r.Name.Split('_')[1]) - 8);
                }
            }
            return new byte[] { 0, 0, 0, index };
        }

        private void cmb_17_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x85, cmb_17.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_18_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x86, cmb_18.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_19_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                byte[] DCBMSBitS = new byte[4];
                DCBMSBitS[2] = (byte)(cmb_19.Checked ? 1 : 0);
                DCBMSBitS[3] = (byte)(cmb_20.Checked ? 1 : 0);
                EquipMentControl.BMS.BMSSetKState_DC(lstChargerID, 0x87, DCBMSBitS, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_21_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x88, cmb_21.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_22_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x89, cmb_22.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_23_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x8A, cmb_23.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_24_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x8C, cmb_24.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void cmb_25_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMS_DC_SetControl(lstChargerID, 0x8D, cmb_24.Checked, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(300);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            double volt = 0;
            try
            {
                isAuto = false;
                Dictionary<int, List<bool>> dic = EquipMentControl.BMS.BMSGetKState_JP_DC(lstChargerID, out int DCPRState, out int DCMRState, out volt, new string[] { "emtBMS_JP_DC" });
                txtVolt.Text = volt.ToString();
                if (dic.Count == 0)
                {
                    isAuto = true;
                    return;
                }
                List<bool> lstKStage = dic[lstChargerID[0]];
                if (lstKStage == null)
                {
                    isAuto = true;
                    return;
                }
                foreach (Control ctrl in tpnl1.Controls)
                {
                    UIRadioButton r = ctrl as UIRadioButton;
                    if (Convert.ToInt32(r.Name.Split('_')[1]) == DCPRState)
                    {
                        r.Checked = true;
                    }
                }
                foreach (Control ctrl in tpnl2.Controls)
                {
                    UIRadioButton r = ctrl as UIRadioButton;
                    if (Convert.ToInt32(r.Name.Split('_')[1]) == DCMRState)
                    {
                        r.Checked = true;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    foreach (Control ctrl in tpnl3.Controls)
                    {
                        UICheckBox c = (UICheckBox)ctrl;
                        if (Convert.ToInt32(c.Name.Split('_')[1]) - 16 == i)
                        {
                            c.Checked = lstKStage[i];
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            isAuto = true;
        }
    }
}
