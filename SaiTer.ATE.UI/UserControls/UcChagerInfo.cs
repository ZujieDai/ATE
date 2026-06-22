using SaiTer.ATE.DataModel;
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

namespace SaiTer.ATE.UI.UserControls
{
    public partial class UcChagerInfo : UserControl
    {
        #region Param
        public bool pnlBarcodeVisible { get => pnlBarcode.Visible; set => pnlBarcode.Visible = value; }
        public bool isChecked { get => chbCharger.Checked; set => chbCharger.Checked = value; }
        public string Barcode { get => txtBarCode.Text; set => txtBarCode.Text = value; }
        public string NormalVolt { get => txtMaxVoltage.Text; set => txtMaxVoltage.Text = value; }
        public string RateCurrent { get => txtRateCurrent.Text; set => txtRateCurrent.Text = value; }
        public string MaxPower { get => txtMaxPower.Text; set => txtMaxPower.Text = value; }
        public string MinVolt { get => txtMinVolt.Text; set => txtMinVolt.Text = value; }
        public string Freq { get => txtFreq.Text; set => txtFreq.Text = value; }
        public string MaxCurrent { get => txtMaxCurr.Text; set => txtMaxCurr.Text = value; }

        #endregion

        public UcChagerInfo()
        {
            InitializeComponent();

            string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
            if (strIsGroupC != null)
            {
                bool isGroupCharger = Convert.ToBoolean(strIsGroupC);
                if (isGroupCharger)
                {
                    chbCharger.Text = "终端条码/编号：";
                }
            }
        }

        public void SetChargerType(string ChargerTypeName)
        {
            if (ChargerTypeName.Contains("直流"))
            {
                if (pnlDC.Visible)
                    return;
                label1.Visible = false;
                lblVolt.Visible = false;
                txtRateCurrent.Maximum = 500;
                txtRateCurrent.MaxLength = 3;
                txtRateCurrent.Text = "160";
                txtMaxVoltage.Maximum = 1500;
                txtMaxVoltage.Text = "1000";
                txtMaxPower.Text = "160";
                pnlDC.Visible = true;
            }
            else
            {
                if (!pnlDC.Visible)
                    return;
                label1.Visible = true;
                lblVolt.Visible = true;
                txtRateCurrent.Maximum = 64;
                txtRateCurrent.Text = "32";
                txtRateCurrent.MaxLength = 2;
                txtMaxVoltage.Maximum = 240;
                txtMaxVoltage.Text = "220";
                pnlDC.Visible = false;
            }
        }

        public void BindingChargerInfo(string ChargerTypeName, bool isChecked, ChargerInfoModel ChargerInfo)
        {
            chbCharger.Checked = isChecked;
            if (isChecked)
            {
                txtBarCode.Text = ChargerInfo.BarCode;
                if (ChargerTypeName.Contains("直流"))
                {
                    pnlDC.Visible = true;
                    txtMaxPower.Text = ChargerInfo.MaxOutputPower.ToString();
                    txtMaxCurr.Text = ChargerInfo.MaxAllowChargeCurrent.ToString();
                    txtMinVolt.Text = ChargerInfo.MinAllowChargeVoltage.ToString();
                }
                else
                {
                    pnlDC.Visible = false;
                }

                txtMaxVoltage.Text = ChargerInfo.NominalVoltage.ToString();
                txtRateCurrent.Text = ChargerInfo.NominalCurrent.ToString();
                txtFreq.Text = ChargerInfo.Frequency.ToString();
            }
        }

        public bool CheckParams(string ChargerTypeName)
        {
            if(!chbCharger.Checked || !pnlBarcodeVisible)
                return true;
            if (string.IsNullOrEmpty(txtMaxVoltage.Text))
                return false;
            if (string.IsNullOrEmpty(txtRateCurrent.Text))
                return false;
            if (string.IsNullOrEmpty(txtFreq.Text))
                return false;
            if (ChargerTypeName.Contains("直流"))
            {
                if (string.IsNullOrEmpty(txtMaxPower.Text))
                    return false;
                if (string.IsNullOrEmpty(txtMaxCurr.Text))
                    return false;
                if (string.IsNullOrEmpty(txtMinVolt.Text))
                    return false;
            }
            return true;
        }

        private void txtMaxVoltage_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMaxVoltage.Text))
                {
                    lblVolt.Text = "0 V";
                }
                else
                {
                    lblVolt.Text = (Convert.ToDouble(txtMaxVoltage.Text) * 1.732).ToString() + " V";
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

    }
}
