using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SaiTer.ATE.MES;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    /// <summary>
    /// 弹窗提示信息（倒计时或者选择）
    /// </summary>
    public partial class FrmAutoTest : UIForm
    {
        TBHttpMESAutomation httpMES;
        public FrmAutoTest()
        {
            InitializeComponent();
            this.BringToFront();

            this.StartPosition = FormStartPosition.CenterParent; // 居中显示
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            httpMES.Stop();
            this.Close();
            this.Dispose();
        }

        private void FrmAutoTest_Load(object sender, EventArgs e)
        {
            httpMES = TBHttpMESAutomation.GetInstance();
            httpMES.Run();
        }
    }
}
