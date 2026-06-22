using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcSafety : UcEquipOperateBase
    {

        bool isStop = false;
        public UcSafety()
        {
            InitializeComponent();
        }
        private void UcSafety_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnTest_IR_Click(object sender, EventArgs e)
        {
            btnTest_IR.Enabled = false;

            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MAIN:FUNC MANU", "\r\n", "\r\n");
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:EDIT:MODE IR", "\r\n", "\r\n");
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:IR:VOLT " + txtIR_IRVoltage.Text.Trim(), "\r\n", "\r\n"); //IR电压，以 kV 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:IR:RHIS " + txtIR_HISET.Text.Trim(), "\r\n", "\r\n");//HISET 电阻值，以 MΩ
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:IR:RLOS " + txtIR_LOSET.Text.Trim(), "\r\n", "\r\n");//LOSET电阻值，以 MΩ
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:IR:TTIM " + txtIR_TestTime.Text.Trim(), "\r\n", "\r\n");// 测试时间 S
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:RTIM " + txtIR_RampTime.Text.Trim(), "\r\n", "\r\n");//斜坡时间（以秒为单位）
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:IR:REF " + txtIR_Reference.Text.Trim(), "\r\n", "\r\n");//IR参考值 MΩ
            int sleepTime = Convert.ToInt32(double.Parse(txtIR_TestTime.Text.Trim())) + Convert.ToInt32(double.Parse(txtIR_RampTime.Text.Trim()));//测试时间+斜坡时间
            StartTest(sleepTime);
            btnTest_IR.Enabled = true;

        }

        private void StartTest(int sleepTime)
        {
            isStop = false;
            System.Threading.Thread.Sleep(500);
            sleepTime += 5; //偏移值，确认获取最终结果
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "FUNC:TEST ON", "\r\n", "\r\n");
            SystemEvent.EquipMentResultEvent += SystemEvent_EquipMentResultEvent;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < sleepTime * 1000)
            {
                if (isStop)
                    break;
                EquipMentControl.Safety.SafetyReadParam(lstChargerID, "MEAS?", "\r\n", "\r\n");
                Application.DoEvents();
            }
            SystemEvent.EquipMentResultEvent -= SystemEvent_EquipMentResultEvent;
            stopwatch.Stop();
        }

        private void btnTest_ACW_Click(object sender, EventArgs e)
        {
            btnTest_ACW.Enabled = false;
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MAIN:FUNC MANU", "\r\n", "\r\n");
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:EDIT:MODE ACW", "\r\n", "\r\n");


            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:VOLT " + txtACW_one.Text.Trim(), "\r\n", "\r\n");//ACW 电压，以 kV
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:CHIS " + txtACW_two.Text.Trim(), "\r\n", "\r\n");// ACW HI SET 电流值（以毫安为单位）。
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:CLOS " + txtACW_three.Text.Trim(), "\r\n", "\r\n");//ACW LO SET 电流值，以毫安为单位。
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:TTIM " + txtACW_four.Text.Trim(), "\r\n", "\r\n");// ACW 测试时间，以秒为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:RTIM " + txtACW_five.Text.Trim(), "\r\n", "\r\n");//斜坡时间（以秒为单位）
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:REF " + txtACW_six.Text.Trim(), "\r\n", "\r\n");//ACW 参考值，以 mA 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:ARCC " + txtACW_seven.Text.Trim(), "\r\n", "\r\n"); //ACW ARC 电流值，以 mA 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:ACW:FREQ " + txtACW_eight.Text.Trim(), "\r\n", "\r\n");// ACW 测试频率，仪 Hz 计

            int sleepTime = Convert.ToInt32(double.Parse(txtACW_four.Text.Trim())) + Convert.ToInt32(double.Parse(txtACW_five.Text.Trim()));
            StartTest(sleepTime);
            btnTest_ACW.Enabled = true;

        }

        private void btnTest_DCW_Click(object sender, EventArgs e)
        {
            btnTest_DCW.Enabled = false;
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:EDIT:MODE DCW", "\r\n", "\r\n");

            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:VOLT " + txtDCW_one.Text.Trim(), "\r\n", "\r\n");//DCW 电压，以 kV 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:CHIS " + txtDCW_two.Text.Trim(), "\r\n", "\r\n");//DCW HI SET 电流值，以 mA 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:CLOS " + txtDCW_three.Text.Trim(), "\r\n", "\r\n");//DCW LO SET 电流值，以 mA 为单 位。
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:TTIM " + txtDCW_four.Text.Trim(), "\r\n", "\r\n");// DCW 测试时间，以秒为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:RTIM " + txtDCW_five.Text.Trim(), "\r\n", "\r\n");//斜坡时间（以秒为单位）
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:REF " + txtDCW_six.Text.Trim(), "\r\n", "\r\n");// DCW 参考值，以 mA 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:DCW:ARCC " + txtDCW_seven.Text.Trim(), "\r\n", "\r\n");//DCW ARC 电流值，以 mA 为单位。


            int sleepTime = Convert.ToInt32(double.Parse(txtDCW_four.Text.Trim())) + Convert.ToInt32(double.Parse(txtDCW_five.Text.Trim()));
            StartTest(sleepTime);
            btnTest_DCW.Enabled = true;
        }

        private void btnTest_GB_Click(object sender, EventArgs e)
        {
            btnTest_GB.Enabled = false;
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:EDIT:MODE GB", "\r\n", "\r\n");

            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:CURR " + txtGB_one.Text.Trim(), "\r\n", "\r\n");  // GB 电流，以 A
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:RHIS " + txtGB_two.Text.Trim(), "\r\n", "\r\n");//HI SET 电阻值，以 mΩ 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:RLOS " + txtGB_three.Text.Trim(), "\r\n", "\r\n");//LO SET 电阻值，以 mΩ 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:TTIM " + txtGB_four.Text.Trim(), "\r\n", "\r\n");//GB 测试时间，以秒为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:RTIM " + txtGB_five.Text.Trim(), "\r\n", "\r\n");//斜坡时间（以秒为单位）
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:REF " + txtGB_six.Text.Trim(), "\r\n", "\r\n");//GB 参考值，以 mΩ 为单位
            EquipMentControl.Safety.SafetySetParam(lstChargerID, "MANU:GB:FREQ " + txtGB_seven.Text.Trim(), "\r\n", "\r\n");// GB 测试频率，以 Hz 为单位
            int sleepTime = Convert.ToInt32(double.Parse(txtGB_four.Text.Trim())) + Convert.ToInt32(double.Parse(txtGB_five.Text.Trim()));
            StartTest(sleepTime);
            btnTest_GB.Enabled = true;
        }
        private void SystemEvent_EquipMentResultEvent(DataModel.Struct.StResultData st)
        {
            if (st.LstData != null && st.LstData.Count > 0)
            {
                string[] data = st.LstData[0].ToString().Split(',');
                if (data.Length >= 5)
                {
                    txtTestFunction.Text = data[0];
                    txtTestResult.Text = data[1];
                    txtVolOrCurrent.Text = data[2];
                    txtCurrentOrResistance.Text = data[3].Replace("ohm", "Ω");
                    txtTestOrRampTime.Text = data[4];
                    //测试已结束
                    if(new string[] { "PASS", "FAIL", "STOP"}.Contains(txtTestResult.Text.ToUpper().Trim()))
                        isStop = true;
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.Safety.SafetyOFF(lstChargerID);
        }
    }
}
