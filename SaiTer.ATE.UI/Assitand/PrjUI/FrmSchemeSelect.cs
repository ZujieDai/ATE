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

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmSchemeSelect : UIForm
    {
        private string schemeName = null;
        private BusinessManage BCM = null;
        public string UserType = null;
        bool isAutoTest = false;
        public int SchemeIndex { get => cmbScheme.SelectedIndex; set => cmbScheme.SelectedIndex = value; }

        private FrmSchemeSelect()
        {
            InitializeComponent();
            string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
            if (strAutoTest != null)
            {
                isAutoTest = bool.Parse(strAutoTest);
            }
        }
        private static FrmSchemeSelect Instance = null;

        public static FrmSchemeSelect GetInstance()
        {
            if (Instance == null)
                Instance = new FrmSchemeSelect();
            Instance.Activate();
            return Instance;
        }
        private void FrmSchemeSelect_Load(object sender, EventArgs e)
        {
            LoadParams();
            SetCustomerLogo();
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

        public void LoadParams()
        {
            List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
            List<ChargerInfoModel> lstChargerInfo = new List<ChargerInfoModel>();
            try
            {
                if (ChargerInfoManage.SelectChargerInfo(out lstChargerInfo))
                {
                    if (lstChargerInfo.Count > 0)
                        schemeName = lstChargerInfo[0].SchemeName;
                }

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
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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
                ConfirmScheme();
                this.Close();
            }
            catch (Exception ex)
            { Log.Log.LogException(ex); }
        }

        public void ConfirmScheme()
        {
            BCM = BusinessManage.GetInstance();
            BCM.GetTrialScheme(cmbScheme.Text);
            bool isOK = ChargerInfoManage.UpdateChargerInfo(BCM.lstChargerInfo, cmbScheme.Text);
            if (isOK)
            {
                //重新选择方案后， 删除原有方案的数据
                BCM.LoadChargerInfo();
                ucCheckData uc = ucCheckData.GetInstance();
                uc.DataCollection_Init(BCM.lstTrialItemsInfo.Count, BCM.lstChargerInfo.Count);
                //for (int i = 0; i < BCM.lstChargerInfo.Count; i++)
                //{
                //    TrialItemDataTmpManage.DeleteTrialData(BCM.lstChargerInfo[i].PKID.ToString());
                //    TrialItemResultTmpManage.DeleteTrialData(BCM.lstChargerInfo[i].PKID.ToString());
                //}
                //SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Wait);
            }
            BCM.AddTrialData();     //加载测试历史数据
            SystemEvent.SendDataGridViewItems(BCM.lstTrialItemsInfo, false, cmbScheme.Text);
            ucTestItems temp = ucTestItems.GetInstance(BCM.lstTrialItemsInfo.Count, "管理员");
            temp.lstItems.ForEach(item => { item.SchemeName = cmbScheme.Text; });
            temp.lstItemsClone.ForEach(item => { item.SchemeName = cmbScheme.Text; });
            temp.IsInitialize = false;  //切换方案时会更新测试状态
            SystemEvent.SendDataGridViewItems(temp.lstItemsClone, true);
        }

        private void FrmSchemeSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
