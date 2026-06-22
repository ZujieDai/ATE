using SaiTer.ATE.Controls;
using SaiTer.ATE.Manage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcEquipOperateBase : UserControl
    {
        public UcEquipOperateBase()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 所有设备控制集合
        /// </summary>
        public ControlsListManager EquipMentControl = XmlInfoAndAssembly.GetInstance()._EquipMentControl;
        public List<int> lstChargerID = new List<int>();
        /// <summary>
        /// 该设备对应的充电枪编号
        /// </summary>
        public int ChargerID { get; set; }

        public void GetChargerID()
        {
            string str = this.Parent.Text;
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", "");
            ChargerID = Convert.ToInt32(str);
            if (!lstChargerID.Contains(ChargerID))
            {
                lstChargerID.Add(ChargerID);
            }
        }
    }
}
