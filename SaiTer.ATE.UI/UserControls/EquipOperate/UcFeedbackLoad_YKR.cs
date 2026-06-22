using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    /// <summary>
    /// 回馈负载
    /// </summary>
    public partial class UcFeedbackLoad_YKR : UcEquipOperateBase
    {
        public UcFeedbackLoad_YKR()
        {
            InitializeComponent();
        }

        private void btnSetVolt_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.SetFeedbackLoadParams(lstChargerID, Convert.ToDouble(txtVolt.Text.Trim()), Convert.ToDouble(txtCurrent.Text.Trim()));
        }

        private void btnSetCurr_Click(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.FeedbackLoad_ON(lstChargerID);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.FeedbackLoad.FeedbackLoad_OFF(lstChargerID);

        }

        private void UcFeedbackLoad_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //并机
            EquipMentControl.FeedbackLoad.FeedbackLoad_Parallel(lstChargerID);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            //取消并机
            EquipMentControl.FeedbackLoad.FeedbackLoad_NoParallel(lstChargerID);
        }
    }
}
