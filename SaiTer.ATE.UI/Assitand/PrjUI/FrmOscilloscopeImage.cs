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

namespace SaiTer.ATE.UI.Assitand
{
    public partial class FrmOscilloscopeImage : UIForm
    {
        private FrmOscilloscopeImage()
        {
            InitializeComponent();
        }
        private static FrmOscilloscopeImage Instance = null;

        public static FrmOscilloscopeImage GetInstance() 
        {
            if (Instance == null)
            {
                Instance = new FrmOscilloscopeImage();
                Instance.SetCustomerLogo();
            }
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


        private void FrmOscilloscopeImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }
    }
}
