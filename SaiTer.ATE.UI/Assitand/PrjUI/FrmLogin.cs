using SaiTer.ATE.DataModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.UI;
using Sunny.UI;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;


namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmLogin : UILoginForm
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            SetCustomerLogo();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (UserName.Trim() == "" || Password.Trim() == "")
            {
                ShowWarningTip(LanguageManager.GetByKey("用户名和密码不能为空") + "！！！");
                return;
            }
            string userType = "";
            if (LoginService.Login(UserName, Password, ref userType))
            {
                UIMessageTip.ShowOk(UserName + LanguageManager.GetByKey("登录成功"));
                this.Hide();
                // ShowWaitForm("系统加载中，请稍候........");
                //FrmMain fm = new FrmMain();
                //fm.UserType = userType;
                //HideWaitForm();
                //fm.Show();

                FrmSchemeSelect_Start fm = FrmSchemeSelect_Start.GetInstance();
                fm.UserType = LanguageManager.GetByKey(userType);
                fm.Show();

            }
            else
            {
                ShowErrorTip(LanguageManager.GetByKey("用户名或密码不正确") + "！！！");
            }

        }

        private void SetCustomerLogo()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.ToString().Trim().Equals("TPK"))
            {
                pictureBox1.Image = Properties.Resources.TPK_Logo;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.Visible = false;
                //pictureBox1.Size = new Size(400, 60);
                //pictureBox1.Dock = DockStyle.Fill;
                this.Icon = Properties.Resources.TPK_Icon;
                this.SubText = "深圳市托普科实业有限公司";
                lblSubText.Text = "深圳市托普科实业有限公司";
                lklbl_wz.Text = "www.topsmt.cn";
            }
        }

        private void lklbl_wz_Click(object sender, EventArgs e)
        {
            Process.Start(lklbl_wz.Text);
        }
    }
}
