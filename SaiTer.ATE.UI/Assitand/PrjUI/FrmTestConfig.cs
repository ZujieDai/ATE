using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmTestConfig : UIForm
    {
        public FrmTestConfig()
        {
            InitializeComponent();
            InitForm();
        }
        private void InitForm()
        {
            try
            {
                txt_LoadWaitTime_s.Text = CommAppConfigXMLOperation.GetValue("LoadWaitTime_s", "1");
            }
            catch(Exception ex)
            {

            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Dispose();
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            try
            {
                bool bret=CommAppConfigXMLOperation.SetValue("LoadWaitTime_s",txt_LoadWaitTime_s.Text);
                if(bret)
                {
                    ShowSuccessTip("参数保存成功,请重启软件！！！");
                }
                else
                {
                    ShowErrorTip("参数保存失败！！！");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
