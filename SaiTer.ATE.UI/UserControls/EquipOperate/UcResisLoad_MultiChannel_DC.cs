using SaiTer.ATE.DataModel;
using SaiTer.ATE.Manage;
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
    public partial class UcResisLoad_MultiChannel_DC : UcEquipOperateBase
    {
        public UcResisLoad_MultiChannel_DC()
        {
            InitializeComponent();
        }

        private void UcResisLoad_MultiChannel_DC_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnSetVoltCur_Click(object sender, EventArgs e)
        {
            try
            {
                EquipMentControl.ResistanceLoad.SetResisLoadVolCurr(lstChargerID, Convert.ToDouble(txtVolt.Text), Convert.ToDouble(txtCurrent.Text));
                //Thread.Sleep(1000);
                //EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
            }
            catch(Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {
            try
            {
                EquipMentControl.ResistanceLoad.ResistanceLoad_OFF(lstChargerID);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
