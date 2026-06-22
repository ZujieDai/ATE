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

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcDIORelay : UcEquipOperateBase
    {
        bool isAuto = true;
        int relayCount = 4;
        public UcDIORelay()
        {
            InitializeComponent();
        }

        private void UcDIORelay_Load(object sender, EventArgs e)
        {
            string txt = ConfigurationManager.AppSettings["DIORelayText"];
            if (txt != null)
            {
                lblText.Text = txt;
            }
        }

        private void chbS_1_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                for (uint i = 1; i <= relayCount; i++)
                {
                    foreach (var item in pnlRelay.Controls)
                    {
                        if (item.GetType() == typeof(UICheckBox))
                        {
                            UICheckBox cb = (UICheckBox)item;
                            if (cb.Name == "chbS_" + i)
                            {
                                EquipMentControl.ControlBoard.SetRelaySwitch(i - 1, cb.Checked);
                                Thread.Sleep(300);
                            }
                        }
                    }
                }
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            isAuto = false;
            List<bool> bools = EquipMentControl.ControlBoard.ReadRelaySwitch(0, 4);

            if (bools.Count > 0)
            {
                for (int i = 1; i <= relayCount; i++)
                {
                    foreach (var item in pnlRelay.Controls)
                    {
                        UICheckBox cb = item as UICheckBox;
                        if (cb.Text.TrimStart('Y') == i.ToString())
                        {
                            cb.Checked = bools[i - 1];
                        }
                    }
                }
            }
            isAuto = true;
        }
    }
}
