using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class ucTMLeakageCurrentTester : UcEquipOperateBase
    {
        public ucTMLeakageCurrentTester()
        {
            InitializeComponent();
        }

        private void ucTMLeakageCurrentTester_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btnSet1_Click(object sender, EventArgs e)
        {
            try
            {
                int sleepTime = 50;
                if (rbtnSingle.Checked)
                {
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 32, 1);//断点  单断点
                }
                else
                {
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 32, 2);//断点  电弧断点
                }
                Thread.Sleep(sleepTime);
                if (rbtnCurrentStart.Checked)
                {
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 41, 1);//电流起
                }
                else
                {
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 41, 2);//波形起
                }
                Thread.Sleep(sleepTime);
                int time = Convert.ToInt32(Convert.ToDouble(txtVATime.Text) * 1000);
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 42, time);//V/A间隔时间
                Thread.Sleep(sleepTime);
                time = Convert.ToInt32(Convert.ToDouble(txtCurrentTime.Text) * 1000);
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 33, time);//电流施加时间
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnSet2_Click(object sender, EventArgs e)
        {
            try
            {
                int sleepTime = 50;
                int CurrentValue = Convert.ToInt32(Convert.ToDouble(txtStartCurrent.Text) * 10);
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 1, CurrentValue);// 起始电流
                Thread.Sleep(sleepTime);

                CurrentValue = Convert.ToInt32(Convert.ToDouble(txtLeakCurrent.Text) * 10);
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 2, CurrentValue);// 剩余电流
                Thread.Sleep(sleepTime);

                int CurrentWave = cmbCurrentWave.SelectedIndex + 1;
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 4, CurrentWave);// 电流波形
                Thread.Sleep(sleepTime);

                int SourceVolt = cmbVolt.SelectedIndex + 1;
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 3, CurrentWave);// 电源电压
                Thread.Sleep(sleepTime);

                int Location = cmbLocation.SelectedIndex + 1;
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 6, Location);// 施加位置
                Thread.Sleep(sleepTime);

                int Superposition = cmbSuperposition.SelectedIndex + 1;
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 7, Superposition);// 叠加DC
                Thread.Sleep(sleepTime);

                int Type = cmbType.SelectedIndex + 1;
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 8, Type);// 试品类型
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnVolt_Click(object sender, EventArgs e)
        {
            int value = 0;
            if (btnVolt.Text == "启动电压")
            {
                value = 1;
                btnVolt.Text = "停止电压";
            }
            else if (btnVolt.Text == "停止电压")
            {
                btnVolt.Text = "启动电压";
                value = 0;
            }
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 9, value);// 启动、停止电压
            Thread.Sleep(50);
        }

        private void btnCurrent_Click(object sender, EventArgs e)
        {
            int value = 0;
            if (btnCurrent.Text == "启动电流")
            {
                value = 1;
                btnCurrent.Text = "停止电流";
            }
            else if (btnCurrent.Text == "停止电流")
            {
                btnCurrent.Text = "启动电流";
                value = 0;
            }
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 10, value);// 启动、停止电流
            Thread.Sleep(50);
        }

        private void btnSet3_Click(object sender, EventArgs e)
        {
            int TestType = 0;
            switch (cmbTestType.Text)
            {
                case "脱扣电流":
                    TestType = 1;
                    break;
                case "S2突现时间":
                    TestType = 2;
                    break;
                case "S1突现时间":
                    TestType = 7;
                    break;
                case "闭合时间":
                    TestType = 3;
                    break;
            }
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 34, TestType);// 测试类型
            Thread.Sleep(50);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 35, 1);// 启动测试
        }

        private void btnResult_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dicResult = EquipMentControl.LeakageCurrent.LeakageCurrent_ReadData(lstChargerID, 15, 3);// 查询结果
            if (dicResult[lstChargerID[0]] == null)
            {
                return;
            }
            if (dicResult[lstChargerID[0]].Contains("mA"))
            {
                lblResult.Text = "脱扣电流：" + dicResult[lstChargerID[0]];
            }
            else if (dicResult[lstChargerID[0]].Contains("ms"))
            {
                lblResult.Text = "脱扣时间：" + dicResult[lstChargerID[0]];
            }
            else
            {
                lblResult.Text = dicResult[lstChargerID[0]];
            }

        }
    }
}
