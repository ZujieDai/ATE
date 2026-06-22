using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.MES.HuiZhou_TB
{
    public partial class FrmSetMESPara_TB : UIForm
    {
        public FrmSetMESPara_TB()
        {
            InitializeComponent();

            var tBHttp = TBHttpMES.GetInstance();
            switch_Run.Active = tBHttp.IsPost;
            txtUrl.Text = tBHttp.Url;
            txtEMP.Text = tBHttp.EMP;
            txtRes.Text = tBHttp.Res;
            txtMachineNo.Text = tBHttp.MachineNo;
            txtFixture.Text = tBHttp.Fixture;
            cmbPostWay.SelectedIndex = tBHttp.PostWay;
        }

        private static FrmSetMESPara_TB Instance = null;
        /// <summary>
        /// 设备操作窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmSetMESPara_TB GetInstance()
        {
            if (Instance == null)
                Instance = new FrmSetMESPara_TB();
            Instance.Activate();
            return Instance;
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("MES接口URL不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEMP.Text))
            {
                MessageBox.Show("工号/账户不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtRes.Text))
            {
                MessageBox.Show("工序不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMachineNo.Text))
            {
                MessageBox.Show("资源（设备编码）不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtFixture.Text))
            {
                MessageBox.Show("治具编码不能为空！");
                return;
            }

            using (var stream = File.Open("MESSet.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                string content = $"Url={txtUrl.Text}|EMP={txtEMP.Text}|Res={txtRes.Text}|MachineNo={txtMachineNo.Text}|Fixture={txtFixture.Text}|PostWay={cmbPostWay.SelectedIndex}|IsPost={switch_Run.Active}";
                var array = UTF8Encoding.Default.GetBytes(content);
                stream.Write(array, 0, array.Length);
            }
            this.Hide();
        }

        private void FrmSetMESPara_TB_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }
    }
}
