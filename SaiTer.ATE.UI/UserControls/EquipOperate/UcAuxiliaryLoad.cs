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
    public partial class UcAuxiliaryLoad : UcEquipOperateBase
    {

        public UcAuxiliaryLoad()
        {
            InitializeComponent();
        }
        private void UcAuxiliaryLoad_Load(object sender, EventArgs e)
        {
            GetChargerID();

            //dgvTip.BringToFront();
            dgvTip.AddRow("1", "0.98");
            dgvTip.AddRow("2", "1.96");
            dgvTip.AddRow("4", "3.60");
            dgvTip.AddRow("8", "6.71");
        }
        private void btnSet_Click(object sender, EventArgs e)
        {

            try
            {
                if (rtbnCancel.Checked)
                {
                    EquipMentControl.AuxiliaryLoadCtrl.CancelAllState(lstChargerID);
                }
                else if (rbtn12V.Checked)
                {
                    EquipMentControl.AuxiliaryLoadCtrl.Set12VoltOver(lstChargerID);
                }
                else if (rbtn24V.Checked)
                {
                    EquipMentControl.AuxiliaryLoadCtrl.Set24VoltOver(lstChargerID);
                }
                else if (rbtnShort.Checked)
                {
                    EquipMentControl.AuxiliaryLoadCtrl.SetShortCircuite(lstChargerID);
                }
                else if (rbtn12VCurrent.Checked)
                {
                    int current = Convert.ToInt32(txt12V.Text);
                    EquipMentControl.AuxiliaryLoadCtrl.Set12VCurrent(lstChargerID, current);
                }
                else if (rbtn24VCurrent.Checked)
                {
                    int current = Convert.ToInt32(cmbCurrent24V.Text);
                    EquipMentControl.AuxiliaryLoadCtrl.Set24VCurrent(lstChargerID, current);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


    }
}
