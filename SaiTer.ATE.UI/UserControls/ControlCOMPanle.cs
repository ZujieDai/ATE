using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ControlCOMPanle : UserControl
    {
        public ControlCOMPanle()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 名称
        /// </summary>
        private string _BackName;
        /// <summary>
        /// 
        /// </summary>
        public string BackName
        {
            get { return _BackName; }
            set {

                _BackName = value;
                this.label1.Text = value;
            }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
           
        }

        private void ControlCOMPanle_Load(object sender, EventArgs e)
        {
            this.label1.Text = this.Name.ToString();
        }
    }
}
