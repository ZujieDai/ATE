using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucTrialResult : UserControl
    {
        public ucTrialResult()
        {
            InitializeComponent();
            SystemEvent.SendTestResultToUIEvent += SystemEvent_SendTestResultToUIEvent;
        }

        private void SystemEvent_SendTestResultToUIEvent(DataModel.DataBaseModel.TrialDataModel TrialData, int chargerID = 1, bool isClear = false, int TrialIndex = -1)
        {
            if (isClear)
            {
                lblResult.Text = "待测";
                lblResult.ForeColor = Color.Black;
                picResult.Image = null;
                return;
            }
            if (TrialData.TrialResult == DataModel.EnumModel.EmTrialResult.Pass)
            {
                if (lblResult.Text == "FAIL")
                {
                    return;
                }
                lblResult.Text = "PASS";
                lblResult.ForeColor = Color.Green;
                picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.大绿勾;
            }
            else
            {
                lblResult.Text = "FAIL";
                lblResult.ForeColor = Color.Red;
                picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.大红叉;
            }
        }
    }
}
