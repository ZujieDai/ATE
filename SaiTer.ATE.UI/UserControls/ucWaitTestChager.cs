using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucWaitTestChager : UserControl
    {
        public string ChargerName { get => chbCharger.Text; set => chbCharger.Text = value; }
        public bool ChargerChecked { get => chbCharger.Checked; set => chbCharger.Checked = value; }

        public ucWaitTestChager()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            chbCharger.Checked = !chbCharger.Checked;
        }
    }
}
