using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.UserControls;
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
using static Sunny.UI.IdentityCard;
////
namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    /// <summary>
    /// 这个窗体仅用于程序启动时选择方案
    /// </summary>
    public partial class FrmSchemeSelect_Start : UIForm
    {
        private string schemeName = null;
        private BusinessManage BCM = null;
        public string UserType = null;
        private FrmSchemeSelect_Start()
        {
            InitializeComponent();
        }
        private static FrmSchemeSelect_Start Instance = null;

        public static FrmSchemeSelect_Start GetInstance()
        {
            if (Instance == null)
                Instance = new FrmSchemeSelect_Start();
            Instance.Activate();
            return Instance;
        }
        private void FrmSchemeSelect_Load(object sender, EventArgs e)
        {

            List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
            List<ChargerInfoModel> lstChargerInfo = new List<ChargerInfoModel>();
            try
            {
                if (ChargerInfoManage.SelectChargerInfo(out lstChargerInfo))
                {
                    schemeName = lstChargerInfo[0].SchemeName;
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            try
            {
                if (SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo))
                {
                    cmbScheme.Items.Clear();
                    foreach (var item in lstSchemeInfo)
                    {
                        cmbScheme.Items.Add(item.SchemeName);
                    }
                }
                if (schemeName == null)
                {
                    schemeName = cmbScheme.Items[0].ToString();
                }
                cmbScheme.Text = schemeName;
                SetCustomerLogo();
            }
            catch(Exception ex) { Log.Log.LogException(ex); }

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                FrmMain frm = FrmMain.GetInstance(cmbScheme.Text);
                if (UserType != null)
                {
                    frm.UserType = UserType;
                }

                frm.Show();
                BCM = BusinessManage.GetInstance();
                BCM.GetTrialScheme(cmbScheme.Text);
                SystemEvent.SendDataGridViewItems(BCM.lstTrialItemsInfo, false);
                bool isOK = ChargerInfoManage.UpdateChargerInfo(BCM.lstChargerInfo, cmbScheme.Text);
                if (isOK)
                {
                    BCM.LoadChargerInfo();                   
                }
                this.Close();
            }
            catch (Exception ex)
            { Log.Log.LogException(ex); }
        }

        private void FrmSchemeSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Close();
            Application.Exit();
        }
    }
}
