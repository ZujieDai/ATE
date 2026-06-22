using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.UI.Assitand.PrjUI;
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
using System.Windows.Documents;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcBMS_AC : UcEquipOperateBase
    {
        public UcBMS_AC()
        {
            InitializeComponent();
        }
        bool isAuto = true;
        private void UcBMS_AC_Load(object sender, EventArgs e)
        {
            GetChargerID();
            btnGetKState.Enabled = false;
            isAuto = false;
            BMSGetKState();
            btnGetKState.Enabled = true;
        }
        private void SetKState(List<bool> lst)
        {
            try
            {
                isAuto = false;
                switch_K0.Active = lst[0];
                switch_K2.Active = lst[2];
                switch_K3.Active = lst[3];
                switch_K4.Active = lst[4];
                switch_K5.Active = lst[5];
                switch_K9.Active = lst[9];
                Thread.Sleep(100);
                isAuto = true;

            }
            catch { }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //深圳LJ单开K5：ST-PTAC-GB导引模块带载接触器闭合(K5与中盛的DIO模块的输出Y3或者Y4互锁
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer != null && Customer.Contains("LJ"))
            {
                SystemEvent.SendCountDownTimer("请确认当前负载已经关闭，带载控制程控板有损坏设备的风险", 999, 0);
                EquipMentControl.BMS.BMS_OFF(lstChargerID);
                Thread.Sleep(200);
                var list = EquipMentControl.ControlBoard.ControlBoardReadState(lstChargerID);
                list[4] = true;
                EquipMentControl.ControlBoard.ControlResistanceSetRelay(list);
                Thread.Sleep(500);
            }
            EquipMentControl.BMS.BMS_ON(lstChargerID);
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_OFF(lstChargerID);
        }

        private void switch_K1_ValueChanged(object sender, bool value)
        {
            if (isAuto)
            {
                BMSACStatus();
                EquipMentControl.BMS.BMS_SetKState(lstChargerID, BMSACstatus);
            }
        }

        List<bool> BMSACstatus = new List<bool>(16) { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

        public void BMSACStatus()
        {
            BMSACstatus[0] = switch_K0.Active;
            BMSACstatus[2] = switch_K2.Active;
            BMSACstatus[3] = switch_K3.Active;
            BMSACstatus[4] = switch_K4.Active;
            BMSACstatus[5] = switch_K5.Active;
            BMSACstatus[9] = switch_K9.Active;
            /*
            Ks[0] = true;//开关S2
            Ks[2] = true;//CC
            Ks[3] = true;//CP
            Ks[5] = true;//PE
            Ks[9] = true;//电子锁
            
            */

        }

        private void btnR3_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_SetResistance(lstChargerID, Convert.ToUInt16(txtR2.Text), Convert.ToUInt16(txtR3.Text));
        }




        private void btnSetCP_Click(object sender, EventArgs e)
        {
            try
            {
                pnlCP.Enabled = false;
                double CPVolt = Convert.ToDouble(txtCP.Text);
                double R3 = (CPVolt - 0.78) / ((12 - CPVolt) / 1000);
                R3 = (1000 * (CPVolt - 0.78)) / (12 - CPVolt);
                EquipMentControl.BMS.BMS_SetResistance(lstChargerID, 1300, Convert.ToUInt16(R3));
                pnlCP.Enabled = true;
                lblR3.Text = Convert.ToInt32(R3).ToString();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                pnlCP.Enabled = true;

            }
        }
        private void btnGetKState_Click(object sender, EventArgs e)
        {
            btnGetKState.Enabled = false;
            isAuto = false;
            BMSGetKState();
            btnGetKState.Enabled = true;
        }

        public void BMSGetKState()
        {
            try
            {
                Dictionary<int, List<bool>> dic = EquipMentControl.BMS.BMS_GetKState(lstChargerID);
                if (dic[lstChargerID[0]] != null)
                {
                    SetKState(dic[lstChargerID[0]]);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnUp001_Click(object sender, EventArgs e)
        {
            double CPVolt = Convert.ToDouble(txtCP.Text) + 0.01;
            SetCPVolt(CPVolt);
        }


        private void btnDown001_Click(object sender, EventArgs e)
        {
            double CPVolt = Convert.ToDouble(txtCP.Text) - 0.01;
            SetCPVolt(CPVolt);
        }

        private void btnUp01_Click(object sender, EventArgs e)
        {
            double CPVolt = Convert.ToDouble(txtCP.Text) + 0.1;
            SetCPVolt(CPVolt);
        }

        private void btnDown01_Click(object sender, EventArgs e)
        {
            double CPVolt = Convert.ToDouble(txtCP.Text) - 0.1;
            SetCPVolt(CPVolt);
        }
        private void SetCPVolt(double CPVolt)
        {
            try
            {                
                if (CPVolt > 11 || CPVolt < 3)
                {                    
                    return;
                }                
                txtCP.Text = CPVolt.ToString();
                double R3 = (CPVolt - 0.7) / ((12 - CPVolt) / 1000);
                pnlCP.Enabled = false;
                EquipMentControl.BMS.BMS_SetResistance(lstChargerID, 1300, Convert.ToUInt16(R3));
                pnlCP.Enabled = true;
                lblR3.Text = Convert.ToInt32(R3).ToString();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                pnlCP.Enabled = true;
            }
        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            FrmBMSVersion frm = FrmBMSVersion.GetInstance(lstChargerID, new string[] { "emtBMS_AC" });
            frm.Show();
        }
    }
}
