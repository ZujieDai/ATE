using Sunny.UI;
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
    public partial class UcBMS_JP_DC : UcEquipOperateBase
    {
        public UcBMS_JP_DC()
        {
            InitializeComponent();
        }

        private void UcBMS_JP_DC_Load(object sender, EventArgs e)
        {
            GetChargerID();
            cmbBatteryOverVolt.SelectedIndex = 0;
            cmbBatteryUnderVolt.SelectedIndex = 0;
            cmbBatteryCurrentError.SelectedIndex = 0;
            cmbBatteryTempHight.SelectedIndex = 0;
            cmbBatteryVoltError.SelectedIndex = 0;

            cmbChargingEnabled.SelectedIndex = 0;
            cmbShiftPosition.SelectedIndex = 0;
            cmbSystemFault.SelectedIndex = 0;
            cmbVehicleStatus.SelectedIndex = 0;
            cmbNormalStop.SelectedIndex = 0;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_ON(lstChargerID, new string[] { "emtBMS_JP_DC" });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_OFF(lstChargerID, new string[] { "emtBMS_JP_DC" });
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                var dic = EquipMentControl.BMS.BMSGetData_JP_DC(lstChargerID, out int[] ErrorSign, out int[] StateSign, new string[] { "emtBMS_JP_DC" });
                foreach (int CID in lstChargerID)
                {
                    var data = dic[CID];
                    if (data != null && data.Count == 10)
                    {
                        txtMinBatteryVolt.Text = data[0].ToString();
                        txtMaxBatteryVolt.Text = data[1].ToString();
                        txtChargingRateConst.Text = data[2].ToString();
                        txtMaxChargingTime_S.Text = data[3].ToString();
                        txtMaxChargingTime_M.Text = data[4].ToString();
                        txtChargingET.Text = data[5].ToString();
                        txtCHAdeMONumber.Text = data[6].ToString();
                        txtTargetBatteryVolt.Text = data[7].ToString();
                        txtChargingCurrent.Text = data[8].ToString();
                        txtChargingRate.Text = data[9].ToString();

                        cmbBatteryOverVolt.SelectedIndex = ErrorSign[0];
                        cmbBatteryUnderVolt.SelectedIndex = ErrorSign[1];
                        cmbBatteryCurrentError.SelectedIndex = ErrorSign[2];
                        cmbBatteryTempHight.SelectedIndex = ErrorSign[3];
                        cmbBatteryVoltError.SelectedIndex = ErrorSign[4];

                        cmbChargingEnabled.SelectedIndex = StateSign[0];
                        cmbShiftPosition.SelectedIndex = StateSign[1];
                        cmbSystemFault.SelectedIndex = StateSign[2];
                        cmbVehicleStatus.SelectedIndex = StateSign[3];
                        cmbNormalStop.SelectedIndex = StateSign[4];
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                int[] ErrorSign = new int[] { cmbBatteryOverVolt.SelectedIndex, cmbBatteryUnderVolt.SelectedIndex, cmbBatteryCurrentError.SelectedIndex, cmbBatteryTempHight.SelectedIndex,
                    cmbBatteryVoltError.SelectedIndex, 0, 0, 0 };
                int[] StateSign = new int[] { cmbChargingEnabled.SelectedIndex, cmbShiftPosition.SelectedIndex, cmbSystemFault.SelectedIndex, cmbVehicleStatus.SelectedIndex,
                    cmbNormalStop.SelectedIndex, 0, 0, 0 };
                EquipMentControl.BMS.BMSSetData_JP_DC(lstChargerID, txtMinBatteryVolt.Text, txtMaxBatteryVolt.Text, txtChargingRateConst.Text, txtMaxChargingTime_S.Text, txtMaxChargingTime_M.Text,
                    txtChargingET.Text, txtCHAdeMONumber.Text, txtTargetBatteryVolt.Text, txtChargingCurrent.Text, ErrorSign, StateSign, txtChargingRate.Text, new string[] { "emtBMS_JP_DC" });
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
    }
}
