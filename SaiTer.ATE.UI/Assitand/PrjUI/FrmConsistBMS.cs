using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.UserControls;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmConsistBMS : UIForm
    {
        private string H00 = "00";
        private string H01 = "01";
        private string H10 = "10";
        private Thread timeThread;
        private int timeCount = 20;
        private static FrmConsistBMS Instance = null;
        public ControlsListManager EquipMentControl = XmlInfoAndAssembly.GetInstance()._EquipMentControl;
        public List<int> lstChargerID;
        public FrmConsistBMS()
        {
            InitializeComponent();
            tabPage1.Parent = null;
            tabPage2.Parent = null;
            this.TopMost = true;

        }
        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static FrmConsistBMS GetInstance(List<int> _lstChargerID)
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmConsistBMS();
            }
            Instance.lstChargerID = _lstChargerID;

            return Instance;
        }


        private void Djs()
        {
            Thread.Sleep(1000);//等待窗体加载完成
            while (true)
            {
                if (!this.Visible)
                    return;
                timeCount--;
                Updatelbltime(timeCount);
                if (timeCount < 1)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            //SetBMS();
            this.Hide();

        }
        private void Updatelbltime(int tc)
        {
            Action aysnc = delegate ()
              {
                  lbl_timeCount.Text = tc.ToString();
              };
            this.BeginInvoke(aysnc);
        }

        private void ShowMessageBox(string text)
        {
            ThreadPool.QueueUserWorkItem(a =>
            {
                MessageBox.Show(text);
            }, null);
        }

        public void SetBMS(bool isShow = true)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                { SetNMyBMS(isShow); }));
            }
            else
            {
                SetNMyBMS(isShow);
            }
        }
        public void SetNMyBMS(bool isShow = true)
        {
            try
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                if (isShow)
                    this.Show();

                this.Activate();
                if (IsInputMatchOk() == false)
                {
                    ShowMessageBox("输入的数值不合法！请输入正确的数值！");
                    return;
                }

                SettingCharging data = new SettingCharging();

                data.ReqV = tbReqV.Text;
                data.ReqI = tbReqI.Text;
                data.ChargeMode = rdoChargI.Checked == true ? "02" : "01";
                data.BCLPeriod = tbBCLPeriod.Text;

                data.MeasureV = tbMeasureV.Text;
                data.MeasureI = tbMeasureI.Text;
                data.CurSOC = tbSoc.Text;
                data.RemainTime = tbRemainTime.Text;
                data.MaxSingleBatV = tbMaxSingleBatV.Text;
                data.MaxSingleBatGrpNum = tbMaxSingleBatGrpNum.Text;
                data.BCSPeriod = tbBCSPeriod.Text;

                data.MaxSingleBatVNum = tbMaxSingleBatVNum.Text;
                data.MaxBatTemp = tbMaxBatTemp.Text;
                data.MaxTempDetectionNum = tbMaxTempDetectionNum.Text;
                data.MinBatTemp = tbMinBatTemp.Text;
                data.MinTempDetectionNum = tbMinTempDetectionNum.Text;
                data.BSMPeriod = tbBSMPeriod.Text;

                data.BitStateSingleV = GetPanelState(pnlSingleOverV);
                data.BitStateSOC = GetPanelState(pnlSocHighLow);
                data.BitStateOverI = GetPanelState(pnlBatOverI);
                data.BitStateOverTemp = GetPanelState(pnlOverTemp);
                data.BitStateInsulate = GetPanelState(pnlInsulateState);
                data.BitStateConnState = GetPanelState(pnlConnState);
                data.BitStateChargePermit = GetPanelState(pnlChargePermit);

                //BST 中止原因
                data.AchievedSOC = GetPanelState(pnlAchievedSoc);
                data.AchievedTotalV = GetPanelState(pnlAchievedTotalV);
                data.AchievedSingleV = GetPanelState(pnlAchievedSingleV);
                data.BmsPause = GetPanelState(pnlBmsPause);

                //故障原因
                data.InsulateTrouble = GetPanelState(pnlInsulateTrouble);
                data.OutputConnTrouble = GetPanelState(pnlOutputConnTrouble);
                data.BmsConnTempTrouble = GetPanelState(pnlBmsConnTempTrouble);
                data.ChargeConnTrouble = GetPanelState(pnlChargeConnTrouble);
                data.BatOverTempTrouble = GetPanelState(pnlBatOverTempTrouble);
                data.Detection2Trouble = GetPanelState(pnlDetection2Trouble);
                data.RelayTrouble = GetPanelState(pnlRelayTrouble);
                data.OtherTrouble = GetPanelState(pnlOtherTrouble);

                data.OverIError = GetPanelState(pnlOverIError);
                data.UnusualVError = GetPanelState(pnlUnusualVError);
                data.BSTPeriod = tbBSTPeriod.Text;

                //错误原因 
                EquipMentControl.BMS.BMSDC_SetAllParameter(lstChargerID, data);
                Thread.Sleep(50);
                //Prj.Prj.SendProtocolManager.SendChargingSet(data);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "窗体异常日志");

            }
            timeCount = 20;
            timeThread = new Thread(Djs);
            timeThread.Start();
        }

        public void SetDt(int TestID)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetMyDtHander(SetMyDt), new object[] { TestID });
                this.Invoke(new Action(() =>
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                    this.Activate();
                    this.ShowDialog();
                }));
            }
            else
            {
                SetMyDt(TestID);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Activate();
                this.ShowDialog();
            }
        }

        private delegate void SetMyDtHander(int TestID);

        public void SetMyDt(int TestID)
        {
            switch (TestID)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    break;
                case 18:
                    radioButton36.Checked = true;
                    groupBox1.Enabled = false;
                    groupBox6.Enabled = false;
                    tabPage3.Parent = null;
                    break;
                case 19:
                case 20:
                case 21:
                    break;
                case 22:
                    radioButton60.Checked = true;
                    tabPage4.Parent = null;
                    break;
                case 23:
                    radioButton3.Checked = true;
                    tabPage4.Parent = null;
                    break;
                case 24:
                    radioButton5.Checked = true;
                    tbcSetCharging.Enabled = false;//这条只有一种情况，不允许修改
                    tabPage4.Parent = null;
                    break;
                case 25:
                    radioButton30.Checked = true;
                    tabPage3.Parent = null;
                    break;
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                    break;
                case 38:
                    radioButton36.Checked = true;
                    groupBox1.Enabled = false;
                    groupBox6.Enabled = false;
                    tabPage3.Parent = null;
                    break;
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                    break;
                case 103://正常状态3页面展示
                    tabPage4.Parent = null;
                    break;
                case 104://正常状态4页面展示
                    tabPage3.Parent = null;
                    break;





            }
            timeCount = 20;
            timeThread = new Thread(Djs);
            timeThread.Start();
        }

        private void btn_SetBMS_Click(object sender, EventArgs e)
        {
            timeThread.Abort();
            SetBMS();
            this.Hide();
        }
        private bool IsInputMatchOk()
        {
            bool isMatch = true;
            isMatch &= IsDouble(tbReqV.Text);
            isMatch &= IsDouble(tbReqI.Text);
            isMatch &= IsInt(tbRemainTime.Text);
            isMatch &= IsInt(tbBCLPeriod.Text);

            isMatch &= IsDouble(tbMeasureV.Text);
            isMatch &= IsDouble(tbMeasureI.Text);
            isMatch &= IsInt(tbSoc.Text);
            isMatch &= IsInt(tbRemainTime.Text);
            isMatch &= IsDouble(tbMaxSingleBatV.Text);
            isMatch &= IsInt(tbMaxSingleBatGrpNum.Text);
            isMatch &= IsInt(tbBCSPeriod.Text);

            isMatch &= IsInt(tbMaxSingleBatVNum.Text);
            isMatch &= IsInt(tbMaxBatTemp.Text);
            isMatch &= IsInt(tbMaxTempDetectionNum.Text);
            isMatch &= IsInt(tbMinBatTemp.Text);
            isMatch &= IsInt(tbMinTempDetectionNum.Text);
            isMatch &= IsInt(tbBSMPeriod.Text);

            isMatch &= IsInt(tbBSTPeriod.Text);

            return isMatch;

        }
        private string GetPanelState(Panel pnl)
        {
            foreach (Control c in pnl.Controls)
            {
                if (c is RadioButton)
                {
                    RadioButton btn = ((RadioButton)c);
                    if (btn.Checked == true)
                    {
                        if (btn.Text == "正常" || btn.Text == "未达到" || btn.Text == "禁止"
                            || btn.Text == "OK" || btn.Text == "Not  achieve" || btn.Text == "Prohibit")
                            return H00;
                        else if (btn.Text == "不可信" || btn.Text == "过低" || btn.Text == "Distrust" || btn.Text == "too low")
                            return H10;
                        else
                            return H01;
                    }
                }
            }
            return H10;
        }
        public bool IsDouble(string str)
        {
            double num = 0;
            return double.TryParse(str, out num);
        }
        public bool IsInt(string str)
        {
            //匹配数字（0~9）
            //$表示字符串结尾
            int num = 0;
            return int.TryParse(str, out num);
        }
        private Point mouseOffset;
        private bool isMouseDown = false;
        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label14_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;
            if (e.Button == MouseButtons.Left)
            {
                xOffset = this.Location.X - System.Windows.Forms.Cursor.Position.X;
                yOffset = this.Location.Y - System.Windows.Forms.Cursor.Position.Y;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void label14_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }

        private void label14_MouseUp(object sender, MouseEventArgs e)
        {
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = false;
                }
            }
        }
    }
}

