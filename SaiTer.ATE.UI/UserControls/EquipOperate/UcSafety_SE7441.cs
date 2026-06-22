using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcSafety_SE7441 : UcEquipOperateBase
    {
        BusinessManage BM;
        int SchemeId = 0;
        EquipmentConfigModel EquipmentConfig_Params;
        EquipmentConfigModel EquipmentConfig_Scheme;

        public UcSafety_SE7441()
        {
            InitializeComponent();
            BM = BusinessManage.GetInstance();
        }

        private void UcSafety_Load(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            GetChargerID();
            string schemeName = FrmMain.SchemeName;
            List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
            SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo);
            try
            {
                SchemeId = lstSchemeInfo.FirstOrDefault(s => s.SchemeName.Equals(schemeName)).SchemeID;

                EquipmentConfig_Params = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Params") && s.ChargerType == SchemeId);
                if (EquipmentConfig_Params != null)
                {
                    string[] IRParams = EquipmentConfig_Params.Params1.Split(';');
                    if (IRParams.Length >= 3)
                    {
                        string[] param_IR1 = IRParams[0].Split('|');
                        txtIR1_IRVoltage.Text = param_IR1[0].Split('=')[1];
                        txtIR1_HISET.Text = param_IR1[1].Split('=')[1];
                        txtIR1_LOSET.Text = param_IR1[2].Split('=')[1];
                        txtIR1_TestTime.Text = param_IR1[3].Split('=')[1];
                        txtIR1_ERU.Text = param_IR1[4].Split('=')[1];
                        txtIR1_ERD.Text = param_IR1[5].Split('=')[1];
                        txtIR1_EDE.Text = param_IR1[6].Split('=')[1];

                        string[] param_IR2 = IRParams[1].Split('|');
                        txtIR2_IRVoltage.Text = param_IR2[0].Split('=')[1];
                        txtIR2_HISET.Text = param_IR2[1].Split('=')[1];
                        txtIR2_LOSET.Text = param_IR2[2].Split('=')[1];
                        txtIR2_TestTime.Text = param_IR2[3].Split('=')[1];
                        txtIR2_ERU.Text = param_IR2[4].Split('=')[1];
                        txtIR2_ERD.Text = param_IR2[5].Split('=')[1];
                        txtIR2_EDE.Text = param_IR1[6].Split('=')[1];

                        string[] param_IR3 = IRParams[2].Split('|');
                        txtIR3_IRVoltage.Text = param_IR3[0].Split('=')[1];
                        txtIR3_HISET.Text = param_IR3[1].Split('=')[1];
                        txtIR3_LOSET.Text = param_IR3[2].Split('=')[1];
                        txtIR3_TestTime.Text = param_IR3[3].Split('=')[1];
                        txtIR3_ERU.Text = param_IR3[4].Split('=')[1];
                        txtIR3_ERD.Text = param_IR3[5].Split('=')[1];
                        txtIR3_EDE.Text = param_IR1[6].Split('=')[1];
                    }

                    string[] ACWParams = EquipmentConfig_Params.Params2.Split(';');
                    if (ACWParams.Length >= 3)
                    {
                        string[] param_ACW1 = ACWParams[0].Split('|');
                        txtACW_one.Text = param_ACW1[0].Split('=')[1];
                        txtACW_two.Text = param_ACW1[1].Split('=')[1];
                        txtACW_three.Text = param_ACW1[2].Split('=')[1];
                        txtACW_four.Text = param_ACW1[3].Split('=')[1];
                        txtACW_five.Text = param_ACW1[4].Split('=')[1];
                        txtACW_six.Text = param_ACW1[5].Split('=')[1];
                        txtACW_seven.Text = param_ACW1[6].Split('=')[1];
                        cmbACW_eight.SelectedIndex = Convert.ToInt32(param_ACW1[7].Split('=')[1]);

                        string[] param_ACW2 = ACWParams[1].Split('|');
                        txtACW2_one.Text = param_ACW2[0].Split('=')[1];
                        txtACW2_two.Text = param_ACW2[1].Split('=')[1];
                        txtACW2_three.Text = param_ACW2[2].Split('=')[1];
                        txtACW2_four.Text = param_ACW2[3].Split('=')[1];
                        txtACW2_five.Text = param_ACW2[4].Split('=')[1];
                        txtACW2_six.Text = param_ACW2[5].Split('=')[1];
                        txtACW2_seven.Text = param_ACW2[6].Split('=')[1];
                        cmbACW2_eight.SelectedIndex = Convert.ToInt32(param_ACW2[7].Split('=')[1]);

                        string[] param_ACW3 = ACWParams[2].Split('|');
                        txtACW3_one.Text = param_ACW3[0].Split('=')[1];
                        txtACW3_two.Text = param_ACW3[1].Split('=')[1];
                        txtACW3_three.Text = param_ACW3[2].Split('=')[1];
                        txtACW3_four.Text = param_ACW3[3].Split('=')[1];
                        txtACW3_five.Text = param_ACW3[4].Split('=')[1];
                        txtACW3_six.Text = param_ACW3[5].Split('=')[1];
                        txtACW3_seven.Text = param_ACW3[6].Split('=')[1];
                        cmbACW3_eight.SelectedIndex = Convert.ToInt32(param_ACW3[7].Split('=')[1]);
                    }

                    string[] DCWParams = EquipmentConfig_Params.Params3.Split(';');
                    if (DCWParams.Length >= 3)
                    {
                        string[] param_DCW1 = DCWParams[0].Split('|');
                        txtDCW_one.Text = param_DCW1[0].Split('=')[1];
                        txtDCW_two.Text = param_DCW1[1].Split('=')[1];
                        txtDCW_three.Text = param_DCW1[2].Split('=')[1];
                        txtDCW_four.Text = param_DCW1[3].Split('=')[1];
                        txtDCW_five.Text = param_DCW1[4].Split('=')[1];
                        txtDCW_six.Text = param_DCW1[5].Split('=')[1];
                        txtDCW_seven.Text = param_DCW1[6].Split('=')[1];

                        string[] param_DCW2 = DCWParams[1].Split('|');
                        txtDCW2_one.Text = param_DCW2[0].Split('=')[1];
                        txtDCW2_two.Text = param_DCW2[1].Split('=')[1];
                        txtDCW2_three.Text = param_DCW2[2].Split('=')[1];
                        txtDCW2_four.Text = param_DCW2[3].Split('=')[1];
                        txtDCW2_five.Text = param_DCW2[4].Split('=')[1];
                        txtDCW2_six.Text = param_DCW2[5].Split('=')[1];
                        txtDCW2_seven.Text = param_DCW2[6].Split('=')[1];

                        string[] param_DCW3 = DCWParams[2].Split('|');
                        txtDCW3_one.Text = param_DCW3[0].Split('=')[1];
                        txtDCW3_two.Text = param_DCW3[1].Split('=')[1];
                        txtDCW3_three.Text = param_DCW3[2].Split('=')[1];
                        txtDCW3_four.Text = param_DCW3[3].Split('=')[1];
                        txtDCW3_five.Text = param_DCW3[4].Split('=')[1];
                        txtDCW3_six.Text = param_DCW3[5].Split('=')[1];
                        txtDCW3_seven.Text = param_DCW3[6].Split('=')[1];
                    }
                    string[] GNDParams = EquipmentConfig_Params.Remark.Split(';');
                    if (GNDParams.Length >= 4)
                    {
                        string[] param_GND1 = GNDParams[0].Split('|');
                        txtGND1_one.Text = param_GND1[1].Split('=')[1];
                        txtGND1_two.Text = param_GND1[0].Split('=')[1];
                        txtGND1_three.Text = param_GND1[2].Split('=')[1];
                        txtGND1_four.Text = param_GND1[3].Split('=')[1];
                        txtGND1_five.Text = param_GND1[4].Split('=')[1];
                        cmbGND1_six.SelectedIndex = Convert.ToInt32(param_GND1[5].Split('=')[1]);

                        string[] param_GND2 = GNDParams[1].Split('|');
                        txtGND2_one.Text = param_GND2[1].Split('=')[1];
                        txtGND2_two.Text = param_GND2[0].Split('=')[1];
                        txtGND2_three.Text = param_GND2[2].Split('=')[1];
                        txtGND2_four.Text = param_GND2[3].Split('=')[1];
                        txtGND2_five.Text = param_GND2[4].Split('=')[1];
                        cmbGND2_six.SelectedIndex = Convert.ToInt32(param_GND2[5].Split('=')[1]);

                        string[] param_GND3 = GNDParams[2].Split('|');
                        txtGND3_one.Text = param_GND3[1].Split('=')[1];
                        txtGND3_two.Text = param_GND3[0].Split('=')[1];
                        txtGND3_three.Text = param_GND3[2].Split('=')[1];
                        txtGND3_four.Text = param_GND3[3].Split('=')[1];
                        txtGND3_five.Text = param_GND3[4].Split('=')[1];
                        cmbGND3_six.SelectedIndex = Convert.ToInt32(param_GND3[5].Split('=')[1]);

                        string[] param_GND4 = GNDParams[3].Split('|');
                        txtGND4_one.Text = param_GND4[1].Split('=')[1];
                        txtGND4_two.Text = param_GND4[0].Split('=')[1];
                        txtGND4_three.Text = param_GND4[2].Split('=')[1];
                        txtGND4_four.Text = param_GND4[3].Split('=')[1];
                        txtGND4_five.Text = param_GND4[4].Split('=')[1];
                        cmbGND4_six.SelectedIndex = Convert.ToInt32(param_GND4[5].Split('=')[1]);
                    }

                    btnTest.Enabled = true;
                }
                EquipmentConfig_Scheme = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Scheme") && s.ChargerType == SchemeId);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int pageIndex = tabControl1.SelectedIndex;
            btnTest.Enabled = false;

            EquipMentControl.Safety.SafetySetParam(lstChargerID, "FL " + EquipmentConfig_Scheme.Params1, "\n", "\n");
            Thread.Sleep(500);
            EquipMentControl.Safety.SafetySetParam(lstChargerID, $"SS 0{pageIndex + 1}", "\n", "\n");

            StartTest();
            btnTest.Enabled = true;
        }

        private void StartTest()
        {
            string lastRes = "";
            Thread.Sleep(500);
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "TEST", "\n", "\n");
            SystemEvent.EquipMentResultEvent += SystemEvent_EquipMentResultEvent;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                string res = EquipMentControl.Safety.SafetyReadResult(lstChargerID, $"TD?", "\n", "\n");
                Application.DoEvents();
                if (lastRes.Equals(res))
                    break;
                lastRes = res;
                Thread.Sleep(100);
            }

            EquipMentControl.Safety.SafetyReadParam(lstChargerID, $"RD {tabControl1.SelectedIndex + 1}?", "\n", "\n");
            Thread.Sleep(2000);
            SystemEvent.EquipMentResultEvent -= SystemEvent_EquipMentResultEvent;
            stopwatch.Stop();
            EquipMentControl.Safety.SafetyOFF(lstChargerID);
        }

        private void SystemEvent_EquipMentResultEvent(DataModel.Struct.StResultData st)
        {
            //01,IR,HI-LIMIT,500,>50000,0.5
            if (st.LstData != null && st.LstData.Count > 0)
            {
                string[] data = st.LstData[0].ToString().Split(',');
                if (data.Length >= 6)
                {
                    switch (tabControl1.SelectedIndex)
                    {
                        case 3:
                        case 4:
                        case 5:
                            txtTestFunction.Text = data[1];
                            txtTestResult.Text = data[2];
                            txtVolOrCurrent.Text = data[3];
                            txtCurrentOrResistance.Text = data[4];
                            txtTestOrRampTime.Text = data[6];
                            break;
                        default:
                            txtTestFunction.Text = data[1];
                            txtTestResult.Text = data[2];
                            txtVolOrCurrent.Text = data[3];
                            txtCurrentOrResistance.Text = data[4];
                            txtTestOrRampTime.Text = data[5];
                            break;
                    }
                    txtTestFunction.Text = data[1];
                    txtTestResult.Text = data[2];
                    txtVolOrCurrent.Text = data[3];
                    txtCurrentOrResistance.Text = data[4];
                    txtTestOrRampTime.Text = data[5];
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.Safety.SafetyOFF(lstChargerID);
        }

        private void btnSaveParam_Click(object sender, EventArgs e)
        {
            try
            {
                btnSaveParam.Enabled = false;
                #region IR Params 1
                if (!double.TryParse(txtIR1_IRVoltage.Text, out double IR_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_HISET.Text, out double IR_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_LOSET.Text, out double IR_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_TestTime.Text, out double IR_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_ERU.Text, out double IR_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_ERD.Text, out double IR_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR1_EDE.Text, out double IR_EDE))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region IR Params 2
                if (!double.TryParse(txtIR2_IRVoltage.Text, out double IR2_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_HISET.Text, out double IR2_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_LOSET.Text, out double IR2_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_TestTime.Text, out double IR2_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_ERU.Text, out double IR2_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_ERD.Text, out double IR2_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR2_EDE.Text, out double IR2_EDE))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region IR Params 3
                if (!double.TryParse(txtIR3_IRVoltage.Text, out double IR3_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_HISET.Text, out double IR3_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_LOSET.Text, out double IR3_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_TestTime.Text, out double IR3_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_ERU.Text, out double IR3_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_ERD.Text, out double IR3_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtIR3_EDE.Text, out double IR3_EDE))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region ACW Params 1
                if (!double.TryParse(txtACW_one.Text, out double ACW_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_two.Text, out double ACW_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_three.Text, out double ACW_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_four.Text, out double ACW_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_five.Text, out double ACW_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_six.Text, out double ACW_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW_seven.Text, out double ACW_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region ACW Params 2
                if (!double.TryParse(txtACW2_one.Text, out double ACW2_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_two.Text, out double ACW2_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_three.Text, out double ACW2_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_four.Text, out double ACW2_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_five.Text, out double ACW2_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_six.Text, out double ACW2_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW2_seven.Text, out double ACW2_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region ACW Params 3
                if (!double.TryParse(txtACW3_one.Text, out double ACW3_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_two.Text, out double ACW3_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_three.Text, out double ACW3_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_four.Text, out double ACW3_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_five.Text, out double ACW3_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_six.Text, out double ACW3_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtACW3_seven.Text, out double ACW3_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region DCW Params 1
                if (!double.TryParse(txtDCW_one.Text, out double DCW_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_two.Text, out double DCW_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_three.Text, out double DCW_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_four.Text, out double DCW_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_five.Text, out double DCW_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_six.Text, out double DCW_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW_seven.Text, out double DCW_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region DCW Params 2
                if (!double.TryParse(txtDCW2_one.Text, out double DCW2_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_two.Text, out double DCW2_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_three.Text, out double DCW2_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_four.Text, out double DCW2_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_five.Text, out double DCW2_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_six.Text, out double DCW2_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW2_seven.Text, out double DCW2_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region DCW Params 3
                if (!double.TryParse(txtDCW3_one.Text, out double DCW3_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_two.Text, out double DCW3_HISET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_three.Text, out double DCW3_LOSET))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_four.Text, out double DCW3_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_five.Text, out double DCW3_ERU))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_six.Text, out double DCW3_ERD))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtDCW3_seven.Text, out double DCW3_ARC))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region GND Params 1
                if (!double.TryParse(txtGND1_one.Text, out double GND_Current))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND1_two.Text, out double GND_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND1_three.Text, out double GND_EH))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND1_four.Text, out double GND_EL))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND1_five.Text, out double GND_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region GND Params 2
                if (!double.TryParse(txtGND2_one.Text, out double GND2_Current))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND2_two.Text, out double GND2_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND2_three.Text, out double GND2_EH))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND2_four.Text, out double GND2_EL))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND2_five.Text, out double GND2_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region GND Params 3
                if (!double.TryParse(txtGND3_one.Text, out double GND3_Current))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND3_two.Text, out double GND3_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND3_three.Text, out double GND3_EH))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND3_four.Text, out double GND3_EL))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND3_five.Text, out double GND3_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                #region GND Params 4
                if (!double.TryParse(txtGND4_one.Text, out double GND4_Current))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND4_two.Text, out double GND4_Volt))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND4_three.Text, out double GND4_EH))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND4_four.Text, out double GND4_EL))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                if (!double.TryParse(txtGND4_five.Text, out double GND4_TestTime))
                {
                    UIMessageTip.ShowError("保存失败！");
                    return;
                }
                #endregion
                // 保存数据库
                string param1 = $"IR电压值(V)={IR_Volt}|HISET电阻值(MΩ)={IR_HISET}|LOSET电阻值(MΩ)={IR_LOSET}|测试时间(S)={IR_TestTime}|缓升时间(S)={IR_ERU}|缓降时间(S)={IR_ERD}|延迟时间(S)={IR_EDE}"
                    + $";IR电压值(V)={IR2_Volt}|HISET电阻值(MΩ)={IR2_HISET}|LOSET电阻值(MΩ)={IR2_LOSET}|测试时间(S)={IR2_TestTime}|缓升时间(S)={IR2_ERU}|缓降时间(S)={IR2_ERD}|延迟时间(S)={IR2_EDE}"
                    + $";IR电压值(V)={IR3_Volt}|HISET电阻值(MΩ)={IR3_HISET}|LOSET电阻值(MΩ)={IR3_LOSET}|测试时间(S)={IR3_TestTime}|缓升时间(S)={IR3_ERU}|缓降时间(S)={IR3_ERD}|延迟时间(S)={IR3_EDE}";
                string param2 = $"交流耐压值(V)={ACW_Volt}|HISET电流值(mA)={ACW_HISET}|LOSET电流值(mA)={ACW_LOSET}|测试时间(S)={ACW_TestTime}|缓升时间(S)={ACW_ERU}|缓降时间(S)={ACW_ERD}|ARC电弧灵敏度={ACW_ARC}|输出频率(Hz)={cmbACW_eight.SelectedIndex}"
                    + $";交流耐压值(V)={ACW2_Volt}|HISET电流值(mA)={ACW2_HISET}|LOSET电流值(mA)={ACW2_LOSET}|测试时间(S)={ACW2_TestTime}|缓升时间(S)={ACW2_ERU}|缓降时间(S)={ACW2_ERD}|ARC电弧灵敏度={ACW2_ARC}|输出频率(Hz)={cmbACW2_eight.SelectedIndex}"
                    + $";交流耐压值(V)={ACW3_Volt}|HISET电流值(mA)={ACW3_HISET}|LOSET电流值(mA)={ACW3_LOSET}|测试时间(S)={ACW3_TestTime}|缓升时间(S)={ACW3_ERU}|缓降时间(S)={ACW3_ERD}|ARC电弧灵敏度={ACW3_ARC}|输出频率(Hz)={cmbACW3_eight.SelectedIndex}";
                string param3 = $"直流耐压值(V)={DCW_Volt}|HISET电流值(uA)={DCW_HISET}|LOSET电流值(uA)={DCW_LOSET}|测试时间(S)={DCW_TestTime}|缓升时间(S)={DCW_ERU}|缓降时间(S)={DCW_ERD}|ARC电弧灵敏度={DCW_ARC}"
                    + $";直流耐压值(V)={DCW2_Volt}|HISET电流值(uA)={DCW2_HISET}|LOSET电流值(uA)={DCW2_LOSET}|测试时间(S)={DCW2_TestTime}|缓升时间(S)={DCW2_ERU}|缓降时间(S)={DCW2_ERD}|ARC电弧灵敏度={DCW2_ARC}"
                    + $";直流耐压值(V)={DCW3_Volt}|HISET电流值(uA)={DCW3_HISET}|LOSET电流值(uA)={DCW3_LOSET}|测试时间(S)={DCW3_TestTime}|缓升时间(S)={DCW3_ERU}|缓降时间(S)={DCW3_ERD}|ARC电弧灵敏度={DCW3_ARC}";
                string param4 = $"接地电压值(V)={GND_Volt}|接地电流值(A)={GND_Current}|HISET电阻值(MΩ)={GND_EH}|LOSET电阻值(MΩ)={GND_EL}|测试时间(S)={GND_TestTime}|输出频率(Hz)={cmbGND1_six.SelectedIndex}"
                    + $";接地电压值(V)={GND2_Volt}|接地电流值(A)={GND2_Current}|HISET电阻值(MΩ)={GND2_EH}|LOSET电阻值(MΩ)={GND2_EL}|测试时间(S)={GND2_TestTime}|输出频率(Hz)={cmbGND2_six.SelectedIndex}"
                    + $";接地电压值(V)={GND3_Volt}|接地电流值(A)={GND3_Current}|HISET电阻值(MΩ)={GND3_EH}|LOSET电阻值(MΩ)={GND3_EL}|测试时间(S)={GND3_TestTime}|输出频率(Hz)={cmbGND3_six.SelectedIndex}"
                    + $";接地电压值(V)={GND4_Volt}|接地电流值(A)={GND4_Current}|HISET电阻值(MΩ)={GND4_EH}|LOSET电阻值(MΩ)={GND4_EL}|测试时间(S)={GND4_TestTime}|输出频率(Hz)={cmbGND4_six.SelectedIndex}";
                EquipmentConfigModel equipmentConfig = new EquipmentConfigModel()
                {
                    ChargerType = SchemeId,
                    ConfigType = "Safety_Params",
                    EquipmentName = "Safety_SE7441",
                    Params1 = param1,
                    Params2 = param2,
                    Params3 = param3,
                    Remark = param4
                };
                EquipmentConfigManage.InsertEquipConfigs(equipmentConfig);
                Application.DoEvents();
                // 操作安规设备，先删除再保存
                //EquipMentControl.Safety.SafetySetParam(lstChargerID, $"FD {EquipmentConfig_Scheme.Params1}", "\n", "\n");
                //Thread.Sleep(1000);
                //根据方案编号和方案名保存
                if (EquipMentControl.Safety.SafetyInit(new List<int> { 1 }, EquipmentConfig_Scheme.Params1, EquipmentConfig_Scheme.Params2, true))
                    UIMessageTip.ShowOk("保存成功！");
                else
                    UIMessageTip.ShowError("保存失败！");
            }
            catch (Exception ex) { Log.Log.LogException(ex); UIMessageTip.ShowError("保存失败！"); }
            finally
            {
                btnSaveParam.Enabled = true;
                btnTest.Enabled = true;
            }
        }
    }
}
