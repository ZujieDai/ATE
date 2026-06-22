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

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcControlBoard : UcEquipOperateBase
    {
        /// <summary>
        /// 程控板
        /// </summary>
        public UcControlBoard()
        {
            InitializeComponent();
        }
        bool isAuto = true;
        private void UcControlBoard_Load(object sender, EventArgs e)
        {
            GetChargerID();
            string txt = ConfigurationManager.AppSettings["CtrlBoardText"];
            if (txt != null)
            {
                string SaftyControlNumber = ConfigurationManager.AppSettings["SaftyControlNumber"];
                if (SaftyControlNumber != null && int.TryParse(SaftyControlNumber, out int num) && ChargerID == num)
                {
                    lblText.Text = "S6、S8输入对地\r\nS5、S8输出对地\r\nS6、S7输入对输出\r\nS8、S9、S11接地电阻测试1\r\nS8、S9、S12接地电阻测试2\r\nS8、S9、S13接地电阻测试3";
                }
                else
                    lblText.Text = txt;
            }
        }

        private void chbS_1_CheckedChanged(object sender, EventArgs e)
        {
            if (isAuto)
            {
                List<bool> lstCondiotion = new List<bool>();
                for (int i = 1; i < 17; i++)
                {
                    foreach (var item in pnlRelay.Controls)
                    {
                        if (item.GetType() == typeof(UICheckBox))
                        {
                            UICheckBox cb = (UICheckBox)item;
                            if (cb.Name == "chbS_" + i)
                            {
                                lstCondiotion.Add(cb.Checked);
                            }
                        }
                    }
                }
                EquipMentControl.ControlBoard.ControlResistanceSetRelay(new List<int>() { ChargerID }, lstCondiotion);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            isAuto = false;
            List<bool> bools = EquipMentControl.ControlBoard.ControlBoardReadState(new List<int>() { ChargerID });

            if (bools.Count > 0)
            {
                for (int i = 1; i < 17; i++)
                {
                    foreach (var item in pnlRelay.Controls)
                    {
                        UICheckBox cb = item as UICheckBox;
                        if (cb.Text.TrimStart('S') == i.ToString())

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
