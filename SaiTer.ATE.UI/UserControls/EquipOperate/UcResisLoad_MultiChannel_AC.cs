using SaiTer.ATE.DataModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
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
    /// <summary>
    /// 多通道交流电阻负载（使用回馈载协议）
    /// </summary>
    public partial class UcResisLoad_MultiChannel_AC : UcEquipOperateBase
    {
        public UcResisLoad_MultiChannel_AC()
        {
            InitializeComponent();
        }

        private void UcResisLoad_MultiChannel_DC_Load(object sender, EventArgs e)
        {
            GetChargerID();
            if(ChargerInfoManage.SelectChargerInfo(out var lstChargerInfo))
            {
                txtVolt.Text = lstChargerInfo.FirstOrDefault()?.NominalVoltage.ToString();
            }
        }

        private void btnSetVoltCur_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.SetResisLoadVolCurr(lstChargerID, Convert.ToDouble(txtVolt.Text), Convert.ToDouble(txtCurrent.Text));
            //Thread.Sleep(1000);
            //EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.ResistanceLoad_ON(lstChargerID);
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {
            EquipMentControl.ResistanceLoad.ResistanceLoad_OFF(lstChargerID);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //并机
            EquipMentControl.ResistanceLoad.ResistanceLoad_Parallel(lstChargerID);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            //取消并机
            EquipMentControl.ResistanceLoad.ResistanceLoad_NoParallel(lstChargerID);
        }
    }
}
