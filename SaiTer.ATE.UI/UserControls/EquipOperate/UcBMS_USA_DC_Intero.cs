using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.UI.Assitand.PrjUI;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcBMS_USA_DC_Intero : UcEquipOperateBase
    {
        bool isRead = false;
        public UcBMS_USA_DC_Intero()
        {
            InitializeComponent();
        }

        private void UcBMS_USA_DC_Intero_Load(object sender, EventArgs e)
        {
            GetChargerID();

            //可能绝缘档位是别的
            var insulationStrs = EquipmentConfigManage.GetConfigParams(3, "EU_BMS_Insulation", "Text", "");
            if(insulationStrs != null)
            {
                string[] insulationTexts = insulationStrs[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                rbtn_1.Text = $"DC+对地{insulationTexts[0]}KΩ";
                rbtn_2.Text = $"DC+对地{insulationTexts[1]}KΩ";
                rbtn_3.Text = $"DC+对地{insulationTexts[2]}KΩ";
                rbtn_4.Text = $"DC+对地{insulationTexts[3]}KΩ";
                rbtn_5.Text = $"DC+对地{insulationTexts[4]}KΩ";
                rbtn_6.Text = $"DC+对地{insulationTexts[5]}KΩ";
                rbtn_7.Text = $"DC+对地{insulationTexts[6]}KΩ";
                rbtn_9.Text = $"DC-对地{insulationTexts[0]}KΩ";
                rbtn_10.Text = $"DC-对地{insulationTexts[1]}KΩ";
                rbtn_11.Text = $"DC-对地{insulationTexts[2]}KΩ";
                rbtn_12.Text = $"DC-对地{insulationTexts[3]}KΩ";
                rbtn_13.Text = $"DC-对地{insulationTexts[4]}KΩ";
                rbtn_14.Text = $"DC-对地{insulationTexts[5]}KΩ";
                rbtn_15.Text = $"DC-对地{insulationTexts[6]}KΩ";
            }

            //可能是减配版本，需要屏蔽开关和R2/R3设置
            var kStageStrs = EquipmentConfigManage.GetConfigParams(3, "EU_BMS_KStage", "EU_BMS", "");
            if (kStageStrs != null)
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

                //是否显示R2/R3设置
                if (!string.IsNullOrEmpty(kStageStrs[1]))
                {
                    if (kStageStrs[1].Contains("R2R3Set=0"))
                    {
                        btnSet_R.Visible = false;
                        btnRead_R.Visible = false;
                        txtR2.Visible = false;
                        txtR3.Visible = false;
                        uiLabel1.Visible = false;
                        uiLabel3.Visible = false;
                    }
                }
            }
        }

        private void btnSet_R_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_SetResistance(lstChargerID, Convert.ToUInt16(txtR2.Text), Convert.ToUInt16(txtR3.Text), new string[] { "emtBMS_USA_DC" });
        }

        private void btnRead_R_Click(object sender, EventArgs e)
        {
            ushort r2 = 0, r3 = 0;
            EquipMentControl.BMS.BMS_GetResistance(lstChargerID, ref r2, ref r3, new string[] { "emtBMS_USA_DC" });
            txtR2.Text = r2.ToString();
            txtR3.Text = r3.ToString();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            isRead = true;
            var dic = EquipMentControl.BMS.BMSGetKState_EU_DC(lstChargerID, out double voltage, new string[] { "emtBMS_USA_DC" });
            List<int> lstKStage = dic[lstChargerID[0]];
            if (lstKStage == null || lstKStage.Count < 13)
                return;

            cmb_17.Checked = lstKStage[0] == 1;
            cmb_18.Checked = lstKStage[1] == 1;
            cmb_19.Checked = lstKStage[2] == 1;
            cmb_20.Checked = lstKStage[3] == 1;
            cmb_21.Checked = lstKStage[4] == 1;
            cmb_22.Checked = lstKStage[5] == 1;
            cmb_23.Checked = lstKStage[6] == 1;
            cmb_24.Checked = lstKStage[7] == 1;
            cmb_25.Checked = lstKStage[8] == 1;
            cmb_26.Checked = lstKStage[9] == 1;
            cmb_27.Checked = lstKStage[10] == 1;
            //DC+绝缘阻值档位：
            switch (lstKStage[11])
            {
                case 0:
                    rbtn_0.Checked = true;
                    break;
                case 1:
                    rbtn_1.Checked = true;
                    break;
                case 2:
                    rbtn_2.Checked = true;
                    break;
                case 3:
                    rbtn_3.Checked = true;
                    break;
                case 4:
                    rbtn_4.Checked = true;
                    break;
                case 5:
                    rbtn_5.Checked = true;
                    break;
                case 6:
                    rbtn_6.Checked = true;
                    break;
                case 7:
                    rbtn_7.Checked = true;
                    break;
                default:
                    rbtn_0.Checked = true;
                    break;
            }
            //DC-绝缘阻值档位：
            switch (lstKStage[12])
            {
                case 0:
                    rbtn_8.Checked = true;
                    break;
                case 1:
                    rbtn_9.Checked = true;
                    break;
                case 2:
                    rbtn_10.Checked = true;
                    break;
                case 3:
                    rbtn_11.Checked = true;
                    break;
                case 4:
                    rbtn_12.Checked = true;
                    break;
                case 5:
                    rbtn_13.Checked = true;
                    break;
                case 6:
                    rbtn_14.Checked = true;
                    break;
                case 7:
                    rbtn_15.Checked = true;
                    break;
                default:
                    rbtn_8.Checked = true;
                    break;
            }
            txtVolt.Text = voltage.ToString();
            isRead = false;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            SetKState();
        }

        private void rbtn_CheckedChanged(object sender, EventArgs e)
        {
            SetKState();
        }

        private void SetKState()
        {
            if (isRead)
                return;

            double.TryParse(txtVolt.Text, out double voltage);
            List<bool> list = new List<bool>();
            for (int i = 0; i < 24; i++)
                list.Add(false);
            list[0] = cmb_17.Checked;
            list[1] = cmb_18.Checked;
            list[2] = cmb_19.Checked;
            list[3] = cmb_20.Checked;
            list[4] = cmb_21.Checked;
            list[5] = cmb_22.Checked;
            list[6] = cmb_23.Checked;
            list[14] = cmb_24.Checked;
            list[15] = cmb_25.Checked;
            list[22] = cmb_26.Checked;
            list[23] = cmb_27.Checked;
            int DCPlus = 0, DCMinus = 0;
            foreach (Control ctrl in tpnl1.Controls)
            {
                UIRadioButton r = ctrl as UIRadioButton;
                if (r.Checked)
                {
                    DCPlus = Convert.ToInt32(r.Name.Split('_')[1]);
                    break;
                }
            }
            foreach (Control ctrl in tpnl2.Controls)
            {
                UIRadioButton r = ctrl as UIRadioButton;
                if (r.Checked)
                {
                    DCMinus = Convert.ToInt32(r.Name.Split('_')[1]) - 8;
                    break;
                }
            }
            EquipMentControl.BMS.BMSSetKState_EU_DC(lstChargerID, voltage, list.ToArray(), DCPlus, DCMinus, "", new string[] { "emtBMS_USA_DC" });
        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            FrmBMSVersion frm = FrmBMSVersion.GetInstance(lstChargerID, new string[] { "emtBMS_USA_DC" });
            frm.Show();
        }

        private void btn_SetEA_UA_Click(object sender, EventArgs e)
        {
            if (btn_SetEA_UA.Text.Contains("美标"))
            {
                EquipMentControl.BMS.BMSSetHCAC(lstChargerID, EmChargerType.Charger_USA_DC, new string[] { "emtBMS_USA_DC" });
                btn_SetEA_UA.Text = "切换欧标";
            }
            else
            {
                EquipMentControl.BMS.BMSSetHCAC(lstChargerID, EmChargerType.Charger_EUR_DC, new string[] { "emtBMS_USA_DC" });
                btn_SetEA_UA.Text = "切换美标";
            }
        }
    }
}
