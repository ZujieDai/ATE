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
using System.Timers;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmMesage : UIForm
    {
        private static FrmMesage Instance = null;
        public FrmMesage()
        {
            InitializeComponent();
            this.BringToFront();
            Control.CheckForIllegalCrossThreadCalls = false;

            this.TopLevel = true;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }



        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static FrmMesage GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmMesage();
                Instance.SetCustomerLogo();
            }
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

        public void SetMessageInfo(bool IsShow, string Info,bool Confrim=false)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetShowDialogHander(ShowThisDialog), new object[] { IsShow, Info,Confrim });
            }
            else
            {
                ShowThisDialog(IsShow, Info, Confrim);
            }
        }

        public bool GetVisvible()
        {

            if (Instance != null)
            {
                bool result = false;
                if (this.InvokeRequired)
                {
                    this.Invoke((new Action(() => { result = this.Visible; })));
                }
                else
                {

                    result = this.Visible;
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        private delegate void SetShowDialogHander(bool IsShow, string Info, bool Confrim);

        private void ShowThisDialog(bool IsShow, string Info, bool Confrim)
        {
            if (IsShow)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                Tips.Text = Info;
                this.button1.Visible = Confrim;
                this.Show();

                //this.Invalidate(); // 标记控件为需要重绘    
                //this.Refresh();
                this.Activate();
            }
            else
            {
                this.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
