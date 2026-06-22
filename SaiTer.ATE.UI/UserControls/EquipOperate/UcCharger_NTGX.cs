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
    public partial class UcCharger_NTGX : UcEquipOperateBase
    {
        public UcCharger_NTGX()
        {
            InitializeComponent();
        }

        private void UcCharger_NTGX_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.ChargerCtrl.ChargerStart(lstChargerID);
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {
            EquipMentControl.ChargerCtrl.ChargerStop(lstChargerID);
        }

        private void btnSetLoadParam_Click(object sender, EventArgs e)
        {
            double dU = Convert.ToDouble(txtVolt.Text);
            double dI = Convert.ToDouble(txtCurrent.Text);
            EquipMentControl.ChargerCtrl.SetLoadParam_Charger(lstChargerID, dU, dI);
        }

        private void btnLoadStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.ChargerCtrl.LoadStart_Charger(lstChargerID);
        }

        private void btnLoadStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.ChargerCtrl.LoadStop_Charger(lstChargerID);
        }
    }
}
