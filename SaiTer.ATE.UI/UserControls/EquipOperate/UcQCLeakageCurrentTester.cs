using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using Sunny.UI;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcQCLeakageCurrentTester : UcEquipOperateBase
    {
        public UcQCLeakageCurrentTester()
        {
            InitializeComponent();
            cbb_param_addCurrentType.SelectedIndex = 0;
            cbb_param_currentFreq.SelectedIndex = 0;
            cbb_param_currentNP.SelectedIndex = 0;
            cbb_param_interruptType.SelectedIndex = 0;
            cbb_param_loadLine.SelectedIndex = 0;
            cbb_param_TestType.SelectedIndex = 0;
            cbb_param_waveType.SelectedIndex = 0;
            SystemEvent.SendMonitorMessageEvent += SystemEvent_SendMonitorMessageEvent;
        }

        private void SystemEvent_SendMonitorMessageEvent(object monitorData)
        {
            if (monitorData is QCLeakageCurrent_StateData stateData)   //刷新数据 
            {
                if (this.InvokeRequired)
                {
                    Action SetData = delegate { SetQCLeakageCurrent_Enabled(stateData); };
                    this.Invoke(SetData);
                }
                else
                {
                    SetQCLeakageCurrent_Enabled(stateData);
                }
            }
        }

        private void SetQCLeakageCurrent_Enabled(QCLeakageCurrent_StateData stateDate)
        {
            if (stateDate.RunTime != 1)
            {
                SetParamControlEnable(true);
            }
        }

        private void UcQCLeakageCurrentTester_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btn_Set_parameters_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.Leakage_SetParameters(lstChargerID,
                cbb_param_TestType.SelectedIndex + 1,
                cbb_param_waveType.SelectedIndex,
                cbb_param_currentFreq.SelectedIndex,
                cbb_param_interruptType.SelectedIndex,
                cbb_param_loadLine.SelectedIndex,
                Convert.ToInt32(txt_param_outCurrent.Text),
                cbb_param_addCurrentType.SelectedIndex,
                Convert.ToInt32(txt_param_addCurrent.Text),
                Convert.ToInt32(txt_param_enableCurrentTime.Text),
                Convert.ToInt32(txt_param_startCurrent.Text),
                Convert.ToInt32(txt_param_endCurrent.Text),
                Convert.ToInt32(txt_param_testTime.Text),
                cbb_param_currentNP.SelectedIndex);
        }

        private void btn_StartTest_Click(object sender, EventArgs e)
        {
            try
            {
                SetParamControlEnable(false); // 启动测试后，先关闭参数控件的使能
                int _testType = cbb_param_TestType.SelectedIndex + 1;
                int _snapTime = Convert.ToInt32(txt_param_enableSnapTime.Text);
                EquipMentControl.LeakageCurrent.Leakage_StartTest(lstChargerID, _testType, _snapTime);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void SetParamControlEnable(bool bEnable)
        {
            try
            {
                SetEnabled(cbb_param_TestType, bEnable);
                SetEnabled(cbb_param_waveType, bEnable);
                SetEnabled(cbb_param_currentFreq, bEnable);
                SetEnabled(cbb_param_interruptType, bEnable);
                SetEnabled(cbb_param_loadLine, bEnable);
                SetEnabled(txt_param_outCurrent, bEnable);
                SetEnabled(cbb_param_addCurrentType, bEnable);
                SetEnabled(txt_param_addCurrent, bEnable);
                SetEnabled(txt_param_enableCurrentTime, bEnable);
                SetEnabled(txt_param_startCurrent, bEnable);
                SetEnabled(txt_param_endCurrent, bEnable);
                SetEnabled(txt_param_testTime, bEnable);
                SetEnabled(cbb_param_currentNP, bEnable);
                SetEnabled(txt_param_enableSnapTime, bEnable);
                SetEnabled(txt_param_StepDelayTime, bEnable);

                SetEnabled(btn_Set_parameters, bEnable);
                SetEnabled(btn_StartTest, bEnable);
            }
            catch { }
        }

        private void SetEnabled(Control control, bool bEnable)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => { control.Enabled = bEnable; }));
            }
            else
            {
                control.Enabled = bEnable;
            }
        }
    }
}
