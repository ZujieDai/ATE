
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.UI.Assitand.PrjUI;
using Sunny.UI;
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
    /// <summary>
    /// 直流BMS互操作
    /// </summary>
    public partial class UcBMS_DC_Intero : UcEquipOperateBase
    {
        BMSProtocol_VersionMange bMSProtocol_VersionMange;
        bool isAuto = true;
        string[] ChargerNum_Control;
        public UcBMS_DC_Intero()
        {
            InitializeComponent();
        }
        private void UcBMS_DC_Intero_Load(object sender, EventArgs e)
        {
            LoadVersion();

            GetChargerID();

            //可能绝缘档位是别的
            var insulationStrs = EquipmentConfigManage.GetConfigParams(1, "GB_BMS_Insulation", "Text", "");
            if (insulationStrs != null)
            {
                string[] insulationTexts = insulationStrs[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                rbtn_1.Text = $"DC+对地{insulationTexts[0]}kΩ";
                rbtn_2.Text = $"DC+对地{insulationTexts[1]}kΩ";
                rbtn_3.Text = $"DC+对地{insulationTexts[2]}kΩ";
                rbtn_4.Text = $"DC+对地{insulationTexts[3]}kΩ";
                rbtn_5.Text = $"DC+对地{insulationTexts[4]}kΩ";
                rbtn_6.Text = $"DC+对地{insulationTexts[5]}kΩ";
                if (insulationTexts.Length > 6)
                    rbtn_7.Text = $"DC+对地{insulationTexts[6]}kΩ";
                else
                    rbtn_7.Visible = false;
                rbtn_9.Text = $"DC-对地{insulationTexts[0]}kΩ";
                rbtn_10.Text = $"DC-对地{insulationTexts[1]}kΩ";
                rbtn_11.Text = $"DC-对地{insulationTexts[2]}kΩ";
                rbtn_12.Text = $"DC-对地{insulationTexts[3]}kΩ";
                rbtn_13.Text = $"DC-对地{insulationTexts[4]}kΩ";
                rbtn_14.Text = $"DC-对地{insulationTexts[5]}kΩ";
                if (insulationTexts.Length > 6)
                    rbtn_15.Text = $"DC-对地{insulationTexts[6]}kΩ";
                else
                    rbtn_15.Visible = false;
            }

            string[] ChargerNum = ConfigurationManager.AppSettings["ChargerNum_2015"] != null ? 
                ConfigurationManager.AppSettings["ChargerNum_2015"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            if (ChargerNum.Contains(ChargerID.ToString()))
            {
                rbtn_1.Text = "DC+对地15.3KΩ";
                rbtn_2.Text = "DC+对地20KΩ";
                rbtn_3.Text = "DC+对地75KΩ";
                rbtn_4.Text = "DC+对地100KΩ";
                rbtn_5.Text = "DC+对地200KΩ";
                rbtn_6.Text = "DC+对地300KΩ";
                rbtn_7.Text = "DC+对地600KΩ";
                rbtn_9.Text = "DC-对地15.3KΩ";
                rbtn_10.Text = "DC-对地20KΩ";
                rbtn_11.Text = "DC-对地75KΩ";
                rbtn_12.Text = "DC-对地100KΩ";
                rbtn_13.Text = "DC-对地200KΩ";
                rbtn_14.Text = "DC-对地300KΩ";
                rbtn_15.Text = "DC-对地600KΩ";
            }

            string[] ChargerNum_Normal = ConfigurationManager.AppSettings["ChargerNum_Normal"] != null ?
                ConfigurationManager.AppSettings["ChargerNum_Normal"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            if (ChargerNum_Normal.Contains(ChargerID.ToString()))
            {
                rbtn_1.Text = "DC+对地95KΩ";
                rbtn_2.Text = "DC+对地100KΩ";
                rbtn_3.Text = "DC+对地105KΩ";
                rbtn_4.Text = "DC+对地475KΩ";
                rbtn_5.Text = "DC+对地500KΩ";
                rbtn_6.Text = "DC+对地525KΩ";
                rbtn_7.Visible = false;
                rbtn_9.Text = "DC-对地95KΩ";
                rbtn_10.Text = "DC-对地100KΩ";
                rbtn_11.Text = "DC-对地105KΩ";
                rbtn_12.Text = "DC-对地475KΩ";
                rbtn_13.Text = "DC-对地500KΩ";
                rbtn_14.Text = "DC-对地525KΩ";
                rbtn_15.Visible = false;
            }

            //非档位设置绝缘故障，步进1k直接设置参数
            ChargerNum_Control = ConfigurationManager.AppSettings["ChargerNum_Control"] != null ?
                ConfigurationManager.AppSettings["ChargerNum_Control"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            if (ChargerNum_Control.Contains(ChargerID.ToString()))
            {
                rbtn_0.Visible = false;
                rbtn_1.Visible = false;
                rbtn_2.Visible = false;
                rbtn_3.Visible = false;
                rbtn_4.Visible = false;
                rbtn_5.Visible = false;
                rbtn_6.Visible = false;
                rbtn_7.Visible = false;
                rbtn_8.Visible = false;
                rbtn_9.Visible = false;
                rbtn_10.Visible = false;
                rbtn_11.Visible = false;
                rbtn_12.Visible = false;
                rbtn_13.Visible = false;
                rbtn_14.Visible = false;
                rbtn_15.Visible = false;
                //tpnl2.Controls.Add(cmbCombine, 0, 0);
                //并充和绝缘板之间可能有冲突
                cmbCombine.Visible = false;
                tpnl2.Controls.Add(cmb_16, 0, 0);
                tpnl1.Controls.Add(chbDCUp, 0, 0);
                tpnl1.Controls.Add(txtDCUp, 1, 0);
                tpnl1.Controls.Add(lblDCTip, 2, 0);
                tpnl1.Controls.Add(chbDCDown, 0, 1);
                tpnl1.Controls.Add(txtDCDown, 1, 1); 
                tpnl1.Controls.Add(btnSetDCResistance, 2, 1);
            }

            //2号枪之后可能是减配版本，需要屏蔽开关和R2/R3设置
            var kStageStrs = EquipmentConfigManage.GetConfigParams(1, "GB_BMS_KStage", "GB_BMS", "");
            if (kStageStrs != null && ChargerID != 1)
            {
                //互操开关是否有需要隐藏的
                if (!string.IsNullOrEmpty(kStageStrs[0]))
                {
                    string[] cmbNames = kStageStrs[0].Split('|');
                    foreach (Control ctrl in tpnl1.Controls)
                    {
                        if (cmbNames.Contains(ctrl.Name + "=0"))
                        {
                            ctrl.Visible = false;
                        }
                    }
                    foreach (Control ctrl in tpnl2.Controls)
                    {
                        if (cmbNames.Contains(ctrl.Name + "=0"))
                        {
                            ctrl.Visible = false;
                        }
                    }
                    foreach (Control ctrl in tpnl3.Controls)
                    {
                        if (cmbNames.Contains(ctrl.Name + "=0"))
                        {
                            ctrl.Visible = false;
                        }
                    }
                }
            }

            //YKR的3-8导引电池电压不可设置，需要隐藏
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (ChargerID > 2 && Customer != null && Customer.ToString().Equals("YKR"))
            {
                uiLabel2.Text = "电池电压：";
                txtBatteryVolt.Enabled = false;
            }
        }

        private void LoadVersion()
        {
            bMSProtocol_VersionMange= new BMSProtocol_VersionMange();
            try
            {
                cmbCanVer.Items.Clear();
                ESGBDC_Ver ver = bMSProtocol_VersionMange.SelectVersion();
                // 方法1.1：添加所有枚举值
                foreach (ESGBDC_Ver status in Enum.GetValues(typeof(ESGBDC_Ver)))
                {
                    cmbCanVer.Items.Add(status);
                }
                cmbCanVer.Text = ver.ToString();
                SystemEvent.CanProtocolVersionSW.Invoke(ver);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rbtn_0_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                isAuto = false;
                bool[] DCBMSBitS = SetKState();
                //DC+-一起控制
                if (sender is UICheckBox checkBox1)
                {
                    if (checkBox1.Name.Equals("cmb_18"))
                        DCBMSBitS[19] = checkBox1.Checked;
                    if (checkBox1.Name.Equals("cmb_19"))
                        DCBMSBitS[18] = checkBox1.Checked;
                    if (checkBox1.Name.Equals("cmb_24"))
                        DCBMSBitS[25] = checkBox1.Checked;
                    if (checkBox1.Name.Equals("cmb_25"))
                        DCBMSBitS[24] = checkBox1.Checked;
                }
                //开关S和CC1一样控制，开关S隐藏
                DCBMSBitS[27] = DCBMSBitS[22];
                EquipMentControl.BMS.BMSSetKState_DC(lstChargerID, Convert.ToDouble(txtR4.Text), Convert.ToDouble(txtBatteryVolt.Text), DCBMSBitS, new string[] { "emtBMS_GB_DC" });
                //Thread.Sleep(300);
                //DC+-一起控制
                if (sender is UICheckBox checkBox2)
                {
                    if (checkBox2.Name.Equals("cmb_18"))
                        cmb_19.Checked = checkBox2.Checked;
                    if (checkBox2.Name.Equals("cmb_19"))
                        cmb_18.Checked = checkBox2.Checked;
                    if (checkBox2.Name.Equals("cmb_24"))
                        cmb_25.Checked = checkBox2.Checked;
                    if (checkBox2.Name.Equals("cmb_25"))
                        cmb_24.Checked = checkBox2.Checked;
                }
                isAuto = true;
            }
        }

        private void cmbCombine_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                EquipMentControl.BMS.BMSSetCombine_DC(lstChargerID, cmbCombine.Checked, new string[] { "emtBMS_GB_DC" });
                Thread.Sleep(300);
            }
        }

        private bool[] SetKState()
        {
            bool[] DCBMSBitS = new bool[32];
            for (int i = 0; i < 32; i++)
            {
                DCBMSBitS[i] = false;
            }
            int index;
            foreach (Control ctrl in tpnl1.Controls)
            {
                if (ctrl.GetType() == typeof(UIRadioButton))
                {
                    UIRadioButton r = ctrl as UIRadioButton;
                    if (r.Checked)
                    {
                        index = Convert.ToInt32(r.Name.Split('_')[1]);
                        DCBMSBitS[index] = true;
                    }
                }
            }
            foreach (Control ctrl in tpnl2.Controls)
            {
                if (ctrl.GetType() == typeof(UIRadioButton))
                {
                    UIRadioButton r = ctrl as UIRadioButton;
                    if (r.Checked)
                    {
                        index = Convert.ToInt32(r.Name.Split('_')[1]);
                        DCBMSBitS[index] = true;
                    }
                }
                else if (ctrl.GetType() == typeof(UICheckBox))
                {
                    UICheckBox c = (UICheckBox)ctrl;
                    if (c.Checked)
                    {
                        index = Convert.ToInt32(c.Name.Split('_')[1]);
                        DCBMSBitS[index] = true;
                    }
                }
            }

            foreach (Control ctrl in tpnl3.Controls)
            {
                if (ctrl.GetType() == typeof(UICheckBox))
                {
                    UICheckBox c = (UICheckBox)ctrl;
                    if (c.Checked)
                    {
                        index = Convert.ToInt32(c.Name.Split('_')[1]);
                        DCBMSBitS[index] = true;
                    }
                }
            }
            return DCBMSBitS;
        }


        private void btnVolt_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMSSetResistance(lstChargerID, Convert.ToDouble(txtR4.Text), new string[] { "emtBMS_GB_DC" });
            Thread.Sleep(200);
            EquipMentControl.BMS.BMSSetBatteryVoltage(lstChargerID, Convert.ToDouble(txtBatteryVolt.Text), new string[] { "emtBMS_GB_DC" });
        }

        private void btnR4_Click(object sender, EventArgs e)
        {

            EquipMentControl.BMS.BMSSetResistance(lstChargerID, Convert.ToDouble(txtR4.Text), new string[] { "emtBMS_GB_DC" });
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            double R4 = 0;
            double volt = 0;
            try
            {
                isAuto = false;
                //绝缘档位为数值设置时，需要读取显示
                if (ChargerNum_Control.Contains(ChargerID.ToString()))
                {
                    EquipMentControl.BMS.BMSReadLeakageResistance_DC(lstChargerID, out int isDCUp, out int isDCDown, out double DCUpValue, out double DCDownValue, new string[] { "emtBMS_GB_DC" });
                    chbDCUp.Checked = isDCUp == 1 ? true : false;
                    chbDCDown.Checked = isDCDown == 1 ? true : false;
                    txtDCUp.Text = (DCUpValue / 1000).ToString();
                    txtDCDown.Text = (DCDownValue / 1000).ToString();
                }
                Dictionary<int, List<bool>> dic = EquipMentControl.BMS.BMSGetKState_DC(lstChargerID, out R4, out volt, new string[] { "emtBMS_GB_DC" });
                txtR4.Text = R4.ToString();
                txtBatteryVolt.Text = volt.ToString();
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
                for (int i = 0; i < 8; i++)
                {

                    foreach (Control ctrl in tpnl1.Controls)
                    {
                        if (ctrl.GetType() == typeof(UIRadioButton))
                        {
                            UIRadioButton r = ctrl as UIRadioButton;
                            if (Convert.ToInt32(r.Name.Split('_')[1]) == i)
                            {
                                r.Checked = lstKStage[i];
                            }
                        }
                    }
                }
                for (int i = 8; i < 17; i++)
                {
                    foreach (Control ctrl in tpnl2.Controls)
                    {
                        if (ctrl.GetType() == typeof(UIRadioButton))
                        {
                            UIRadioButton r = ctrl as UIRadioButton;
                            if (Convert.ToInt32(r.Name.Split('_')[1]) == i)
                            {
                                r.Checked = lstKStage[i];
                            }
                        }
                        else if (ctrl.GetType() == typeof(UICheckBox))
                        {
                            UICheckBox c = (UICheckBox)ctrl;
                            if (c.Name.IndexOf('_') > -1)
                            {
                                int.TryParse(c.Name.Split('_')[1], out int cIndex);
                                if (cIndex == i)
                                {
                                    c.Checked = lstKStage[i];
                                }
                            }
                        }
                    }
                }
                for (int i = 16; i < 32; i++)
                {
                    foreach (Control ctrl in tpnl3.Controls)
                    {
                        UICheckBox c = (UICheckBox)ctrl;
                        if (Convert.ToInt32(c.Name.Split('_')[1]) == i)
                        {
                            c.Checked = lstKStage[i];
                        }
                    }
                }
                EquipMentControl.BMS.BMSReadCombine_DC(lstChargerID, out bool isON, new string[] { "emtBMS_GB_DC" });
                cmbCombine.Checked = isON;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            finally
            {
                isAuto = true;
            }
        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            FrmBMSVersion frm = FrmBMSVersion.GetInstance(lstChargerID);
            frm.Show();
        }
        
        private void btnSetDCResistance_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMSSetLeakageResistance_DC(lstChargerID, chbDCUp.Checked ? 1 : 0, chbDCDown.Checked ? 1 : 0,
                Convert.ToDouble(txtDCUp.Text) * 1000, Convert.ToDouble(txtDCDown.Text) * 1000, new string[] { "emtBMS_GB_DC" });
        }

        private void cmbCanVer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ESGBDC_Ver selectedStatus = (ESGBDC_Ver)Enum.Parse(typeof(ESGBDC_Ver), cmbCanVer.SelectedItem.ToString());
                SystemEvent.CanProtocolVersionSW.Invoke(selectedStatus);
                if (bMSProtocol_VersionMange.InsertVersion(selectedStatus))
                {
                    MessageBox.Show("版本切换成功");

                }
                else
                {
                    MessageBox.Show("版本切换失败", "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }
    }
}
