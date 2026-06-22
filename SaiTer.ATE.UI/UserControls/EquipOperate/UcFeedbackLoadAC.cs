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
    public partial class UcFeedbackLoadAC : UcEquipOperateBase
    {
        public UcFeedbackLoadAC()
        {
            InitializeComponent();
        }

        private void btnSetVolt_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.SetFeedbackLoadParams(lstChargerID, Convert.ToDouble(txtVolt.Text.Trim()), Convert.ToDouble(txtCurrent.Text.Trim()));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.FeedbackLoad_ON(lstChargerID);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.FeedbackLoad_OFF(lstChargerID);
        }

        private void UcFeedbackLoadAC_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

    }
}
