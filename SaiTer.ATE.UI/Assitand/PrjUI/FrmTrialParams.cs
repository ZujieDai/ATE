using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
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
using static Sunny.UI.IdentityCard;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmTrialParams : Form
    {
        private string _TrialMethod = "";
        private BusinessManage BCM = BusinessManage.GetInstance();
        private static string UserType = "管理员";
        public string TrialMethod
        {
            get { return _TrialMethod; }
            set
            {
                _TrialMethod = value;
                rtxtTrialMethod.Text = value;
            }
        }

        private string _DecideStandard = "";
        public string DecideStandard
        {
            get { return _DecideStandard; }
            set
            {
                _DecideStandard = value;
                rtxtDecideStandard.Text = value;
            }
        }


        private string _SchemeName = "Default";

        public string SchemeName
        {
            get { return _SchemeName; }
            set { _SchemeName = value; }
        }

        private string _TrialName = "Default";

        public string TrialName
        {
            get { return _TrialName; }
            set
            {
                _TrialName = value;
                this.Text = value;
                lbl_Title.Text = value;
            }
        }

        private EmTrialType _TrialType = EmTrialType.Null;

        public EmTrialType TrialType
        {
            get { return _TrialType; }
            set { _TrialType = value; }
        }

        private List<string> _LstParams = new List<string>();
        public List<string> LstParams
        {
            get { return _LstParams; }
            set
            {
                _LstParams = value;
                SetParams(value);
            }
        }
        private List<StTrialItem> _LstItemsClone = new List<StTrialItem>();

        public List<StTrialItem> LstItemsClone
        {
            get { return _LstItemsClone; }
            set { _LstItemsClone = value; }
        }
        private static FrmTrialParams p = null;
        /// <summary>
        /// 试验参数窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmTrialParams GetInstance(string trialMethod, string decideStandard, List<string> lstParams, string userType)
        {
            UserType = userType;
            if (p == null || p.IsDisposed)
            {
                p = new FrmTrialParams();
                p.SetCustomerLogo();
            }
            else
            {
                if (UserType == "管理员")
                {
                    p.btnOK.Visible = true;
                }
            }
           

            p.TrialMethod = trialMethod;
            p.DecideStandard = decideStandard;
            p.LstParams = lstParams;
            p.Activate();
            return p;
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

        private FrmTrialParams()
        {
            InitializeComponent();
            if (UserType != "管理员")
            {
                btnOK.Visible = false;
            }
        }

        private void SetParams(List<string> lstParams)
        {
            try
            {
                fPnlParams.Controls.Clear();
                foreach (string param in lstParams)
                {
                    string[] str = param.Split('=');
                    if (str.Length >= 2)
                    {
                        Label lblParams = new Label();
                        lblParams.AutoSize = true;
                        lblParams.Text = str[0];
                        lblParams.Margin = new Padding(10, 8, 2, 0);
                        lblParams.Anchor = AnchorStyles.Right;
                        fPnlParams.Controls.Add(lblParams);
                        UITextBox txtParams = new UITextBox();
                        txtParams.Text = str[1];
                        txtParams.Type = str[0].Contains("文本") ? UITextBox.UIEditType.String : UITextBox.UIEditType.Double;
                        txtParams.MaximumEnabled = true;
                        txtParams.MinimumEnabled = true;
                        txtParams.Minimum = -200000;
                        txtParams.Maximum = 200000;
                        txtParams.Anchor = AnchorStyles.Left;
                        txtParams.Margin = new Padding(10, 8, 2, 0);
                        fPnlParams.Controls.Add(txtParams);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in fPnlParams.Controls)
            {
                if (item.GetType() == typeof(Label))
                {
                    Label lbl = (Label)item;
                    sb.Append(lbl.Text);
                    sb.Append("=");
                }
                else if (item.GetType() == typeof(UITextBox))
                {
                    UITextBox txt = (UITextBox)item;
                    sb.Append(txt.Text);
                    sb.Append("|");
                }
            }
            string strParams = sb.ToString().TrimEnd('|');
            int result = TrialItemsManage.UpdateTrialScheme(_SchemeName, _TrialName, _TrialType, strParams, "test");
            if (result == 0)
            {
                MessageBox.Show("保存失败！");
            }
            else
            {
                BCM.GetTrialScheme(_SchemeName);
                FrmMain frm = FrmMain.GetInstance(_SchemeName);
                frm.lstTestItems = BCM.lstTrialItemsInfo;
                _LstItemsClone.ForEach(item =>
                {
                    if (item.TrialType == _TrialType && item.ItemName == _TrialName && item.SchemeName == _SchemeName)
                    {
                        item.ResultParams = strParams;
                    }
                });

                SystemEvent.SendDataGridViewItems(_LstItemsClone, true);
                //MessageBox.Show("保存成功！");
                UIMessageTip.ShowOk("保存成功！");
                this.Activate();
            }
        }

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;
            if (e.Button == MouseButtons.Left)
            {
                xOffset = this.Location.X - System.Windows.Forms.Cursor.Position.X;
                yOffset = this.Location.Y - System.Windows.Forms.Cursor.Position.Y;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }
        private void lblTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }
        private void lblTitle_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }
    }
}
