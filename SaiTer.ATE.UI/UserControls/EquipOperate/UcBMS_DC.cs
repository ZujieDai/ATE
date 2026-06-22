using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcBMS_DC : UcEquipOperateBase
    {
        public UcBMS_DC()
        {
            InitializeComponent();
        }

        private void UcBMS_DC_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }



        private void btnSet1_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.SetParameter(lstChargerID, Convert.ToDouble(txt1.Text), 410 , new string[] { "emtBMS_GB_DC" });
        }

        private void btnSet2_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.SetParameter(lstChargerID, Convert.ToDouble(txt2.Text), Convert.ToDouble(txt3.Text), Convert.ToDouble(txt4.Text), new string[] { "emtBMS_GB_DC" });
        }

        private void btnSet3_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.SetParameter(lstChargerID, Convert.ToDouble(txt6.Text), Convert.ToDouble(txt7.Text), rbtnVolt.Checked, Convert.ToDouble(txt5.Text), true, new string[] { "emtBMS_GB_DC" });
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_ON(lstChargerID, new string[] { "emtBMS_GB_DC" });
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_OFF(lstChargerID, new string[] { "emtBMS_GB_DC" });
        }
    }
}
