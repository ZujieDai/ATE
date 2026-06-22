using SaiTer.ATE.EquipMent;
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
    public partial class UcZJLeakageCurrentTester : UcEquipOperateBase
    {
        /// <summary>
        /// 中佳漏电测试仪操作面板
        /// </summary>
        public UcZJLeakageCurrentTester()
        {
            InitializeComponent();
        }


        private void UcZJLeakageCurrentTester_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void cmbModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbModel.SelectedIndex == 0)
            {
                lblACDC.Text = "AC";
                cmbAngle.Visible = false;
                //lblS3.Visible = false;
                //switchS3.Visible = false;
                btnS3OFf.Visible = false;
                btnS3.Visible = false;
                cmbFunction.Items.Clear();
                cmbFunction.Items.Add("测试动作电流");
                cmbFunction.Items.Add("闭合电流测动作时间");
                cmbFunction.Items.Add("突然电流测动作时间");
                cmbFunction.Text = "测试动作电流";
                btnCurrent.Visible = false;

                /////交流模式
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 11, 0);
                /////测试动作电流
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 12, 0);


            }
            else
            {
                lblACDC.Text = "DC";
                cmbAngle.Visible = true;
                //lblS3.Visible = true;
                // switchS3.Visible = true;
                btnS3.Visible = true;
                btnS3OFf.Visible = true;
                cmbFunction.Items.Clear();
                cmbFunction.Items.Add("测试动作电流");
                cmbFunction.Items.Add("突然电流测动作时间");
                cmbFunction.Text = "测试动作电流";
                /////脉动直流模式
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 11, 1);
                /////测试动作电流
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 13, 0);
                //角度：0度
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 18, 0);
            }
        }

        private void cmbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbModel.Text == "脉动直流模式")
            {
                if (cmbFunction.Text == "测试动作电流")
                {
                    btnCurrent.Visible = false;
                    /////测试动作电流
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 13, 0);
                }
                else if (cmbFunction.Text == "突然电流测动作时间")
                {
                    btnCurrent.Visible = true;
                    /////突然电流测动作时间
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 13, 1);
                }
            }
            else if (cmbModel.Text == "交流模式")
            {
                if (cmbFunction.Text == "测试动作电流")
                {
                    btnCurrent.Visible = false;
                    /////测试动作电流
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 12, 0);
                }
                else if (cmbFunction.Text == "闭合电流测动作时间")
                {
                    btnCurrent.Visible = true;
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 12, 1);
                }
                else if (cmbFunction.Text == "突然电流测动作时间")
                {
                    btnCurrent.Visible = true;
                    EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 12, 2);
                }
            }
        }

        private void cmbAngle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAngle.Text == "0度")
            {
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 18, 0);
            }
            else if (cmbAngle.Text == "90度")
            {
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 18, 1);
            }
            else if (cmbAngle.Text == "135度")
            {
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 18, 2);
            }
        }
        private void rbtnPhaseA_CheckedChanged(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 40, 0);
        }

        private void rbtnPhaseB_CheckedChanged(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 40, 1);
        }

        private void rbtnPhaseC_CheckedChanged(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 40, 2);
        }

        private void switchS1_ValueChanged(object sender, bool value)
        {
            int type = switchS1.Active ? 1 : 0;
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 45, type);
        }

        private void switchS2_ValueChanged(object sender, bool value)
        {
            int type = switchS2.Active ? 1 : 0;
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 46, type);
        }

        private void switchS3_ValueChanged(object sender, bool value)
        {
            int type = switchS3.Active ? 1 : 0;
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 34, type);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            btnSet.Enabled = false;
            //参数设置
            if (cmbModel.Text == "脉动直流模式")
            {
                //DC额定电流In(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 15, Convert.ToInt32(Convert.ToDouble(txtParam1.Text) * 10));
                Thread.Sleep(30);
                //DC-In倍数/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 17, Convert.ToInt32(Convert.ToDouble(txtParam2.Text) * 10));
                Thread.Sleep(30);

                ////////DC动作电流上限(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 23, Convert.ToInt32(Convert.ToDouble(txtParam3.Text) * 10));
                Thread.Sleep(30);

                ////////DC动作电流下限(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 24, Convert.ToInt32(Convert.ToDouble(txtParam4.Text) * 10));
                Thread.Sleep(30);

                ////////DC动作时间上限(ms)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 25, Convert.ToInt32(Convert.ToDouble(txtParam5.Text) * 10));
                Thread.Sleep(30);

                ////////DC动作时间下限(ms)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 26, Convert.ToInt32(Convert.ToDouble(txtParam6.Text) * 10));
                Thread.Sleep(30);

                ////////DC电流上升率(mA/s)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 27, Convert.ToInt32(Convert.ToDouble(txtParam7.Text) * 10));
                Thread.Sleep(30);

                ////////DC不动作电流输出时间(s)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 28, Convert.ToInt32(Convert.ToDouble(txtParam8.Text) * 10 * 1000));
                Thread.Sleep(30);
            }
            else if (cmbModel.Text == "交流模式")
            {
                ////////AC额定电流In(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 14, Convert.ToInt32(Convert.ToDouble(txtParam1.Text) * 10));
                Thread.Sleep(30);

                ////////AC-In倍数/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 16, Convert.ToInt32(Convert.ToDouble(txtParam2.Text) * 10));
                Thread.Sleep(30);

                ////////AC动作电流上限(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 19, Convert.ToInt32(Convert.ToDouble(txtParam3.Text) * 10));
                Thread.Sleep(30);

                ////////AC动作电流下限(mA)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 20, Convert.ToInt32(Convert.ToDouble(txtParam4.Text) * 10));
                Thread.Sleep(30);

                ////////AC动作时间上限(ms)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 21, Convert.ToInt32(Convert.ToDouble(txtParam5.Text) * 10));
                Thread.Sleep(30);

                ////////AC动作时间下限(ms)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 22, Convert.ToInt32(Convert.ToDouble(txtParam6.Text) * 10));
                Thread.Sleep(30);

                ////////AC电流上升率(mA/s)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 10, Convert.ToInt32(Convert.ToDouble(txtParam7.Text) * 10));
                Thread.Sleep(30);

                ////////AC不动作电流输出时间(s)/////////
                EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 28, Convert.ToInt32(Convert.ToDouble(txtParam8.Text) * 10 * 1000));
                Thread.Sleep(30);
            }
            btnSet.Enabled = true;

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 44, 1);//复位
        }

        private void btnCurrent_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 41, 1);
        }

        private void btnS1_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 45, 1);
        }

        private void btnS2_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 46, 1);
        }

        private void btnS3_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 34, 1);
        }

        private void btnS3OFf_Click(object sender, EventArgs e)
        {
            EquipMentControl.LeakageCurrent.LeakageCurrent_SetParams(lstChargerID, 34, 0);
        }
    }
}
