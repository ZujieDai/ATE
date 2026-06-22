
using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
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

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucConnectState : UserControl
    {
        private EquipDescripe equipDescripe = EquipDescripe.GetInstance();

        public List<EquipMentBase> lstEquipment = new List<EquipMentBase>();
        //静态实例
        private static ucConnectState Instance = null;
        public ucConnectState()
        {
            InitializeComponent();
            SystemEvent.SentConnectStateEvent += SystemEvent_SentConnectStateEvent;
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;
        }

        private void SystemEvent_SentConnectStateEvent(bool isConnect, object obj)
        {
            if (dgvState.InvokeRequired)
                dgvState.Invoke(new SentConnectStateEventHandler(LoadingInvoked), isConnect, obj);
            else
                LoadingInvoked(isConnect, obj);
        }
        private void SystemEvent_SendChangeLanguageEvent()
        {
            for (int i = 0; i < this.dgvState.ColumnCount; i++)
            {
                dgvState.Columns[i].HeaderText = LanguageManager.GetByKey(this.Name + "." + dgvState.Columns[i].Name);
            }
        }
        private delegate void SentConnectStateEventHandler(bool isConnect, object obj);
        private void LoadingInvoked(bool isConnect, object obj)
        {
            try
            {
                EquipMentBase equip = (EquipMentBase)obj;
                int index = lstEquipment.FindIndex(s => s.EquipMentClassName == equip.EquipMentClassName && s.ChargerID == equip.ChargerID);
                if (index == -1)
                {
                    lstEquipment.Add(equip);
                    this.dgvState.Rows.Add(1);
                    dgvState.Rows[lstEquipment.Count - 1].Cells["clmName"].Value = GetEquipMentName(equip);
                    dgvState.Rows[lstEquipment.Count - 1].Cells["clmName"].Tag = equip.EquipMentClassName;
                    dgvState.Rows[lstEquipment.Count - 1].Cells["clmState"].Tag = equip.ChargerID;
                    dgvState.Rows[lstEquipment.Count - 1].Cells["clmCom"].Value = equip.EquipMentPort.PortName;
                    dgvState.Rows[lstEquipment.Count - 1].Cells["clmBaudRate"].Value = equip.EquipMentPort.PortParams.ToString();
                    if (isConnect)
                    {
                        dgvState.Rows[lstEquipment.Count - 1].Cells["clmState"].Value = global::SaiTer.ATE.UI.Properties.Resources.pass;
                    }
                    else
                    {
                        dgvState.Rows[lstEquipment.Count - 1].Cells["clmState"].Value = global::SaiTer.ATE.UI.Properties.Resources.fail;
                    }
                }
                for (int i = 0; i < dgvState.Rows.Count; i++)
                {
                    if (dgvState.Rows[i].Cells["clmName"].Tag.ToString() == equip.EquipMentClassName
                        && (int)dgvState.Rows[i].Cells["clmState"].Tag == equip.ChargerID)
                    {
                        if (isConnect)
                        {
                            dgvState.Rows[i].Cells["clmState"].Value = global::SaiTer.ATE.UI.Properties.Resources.pass;
                        }
                        else
                        {
                            dgvState.Rows[i].Cells["clmState"].Value = global::SaiTer.ATE.UI.Properties.Resources.fail;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static ucConnectState GetInstance()
        {
            if (Instance == null)
                Instance = new ucConnectState();
            return Instance;
        }
        private string GetEquipMentName(EquipMentBase equip)
        {
            string name = "";// equipDescripe.DicEquipDescripe.GetValueOrDefault(equip.EquipMentClassName, "");           
            equipDescripe.DicEquipDescripe.TryGetValue(equip.EquipMentClassName, out name);
            name += equip.ChargerID.ToString();

            return name;
        }
    }
}
