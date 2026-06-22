using SaiTer.ATE.Controls;
using SaiTer.ATE.Manage;
using Sunny.UI;
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

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmBMSVersion : UIForm
    {
        public ControlsListManager EquipMentControl = XmlInfoAndAssembly.GetInstance()._EquipMentControl;
        public List<int> lstChargerID = new List<int>();
        private static FrmBMSVersion Instance = null;
        private string[] classNames;
        /// <summary>
        /// 设备操作窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmBMSVersion GetInstance(List<int> _lstChargerID, string[] classNames = null)
        {
            if (Instance == null)
            {
                Instance = new FrmBMSVersion();
                Instance.SetCustomerLogo();
            }
            Instance.lstChargerID = _lstChargerID;
            Instance.classNames = classNames == null ? new string[] { "emtBMS_GB_DC" } : classNames;
            Instance.Activate();
            return Instance;
        }

        private void SetCustomerLogo()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.ToString().Trim().Equals("TPK"))
            {
                //pictureBox1.Size = new Size(400, 60);
                //pictureBox1.Dock = DockStyle.Fill;
                this.Icon = Properties.Resources.TPK_Icon;
            }
        }

        private FrmBMSVersion()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadVersion();
        }

        private void FrmBMSVersion_Load(object sender, EventArgs e)
        {
            LoadVersion();
        }

        private void LoadVersion()
        {
            EquipMentControl.BMS.BMSGetVersion(lstChargerID, out string SoftwareVersion, out string FlowNumber, classNames);
            lblSoftware.Text = SoftwareVersion;
            lblFlow.Text = FlowNumber;
        }

        private void FrmBMSVersion_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
