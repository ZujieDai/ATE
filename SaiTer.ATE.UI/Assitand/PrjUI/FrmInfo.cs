using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    /// <summary>
    /// 弹窗提示信息（倒计时或者选择）
    /// </summary>
    public partial class FrmInfo : UIForm
    {
        private BusinessManage businessManage = BusinessManage.GetInstance();
        private static FrmInfo Instance = null;
        private int downTime = 0;//倒计时时间
        private AllEquipStateData EquipStateData = AllEquipStateData.GetInstance();
        private Dictionary<int, bool> DicManualVerifyResult = new Dictionary<int, bool>();//一般检查等人工确认的结果
        int InfoType = 0;//提示类型 0-纯倒计时提示信息。 1-倒计时等待选择
        private System.Timers.Timer InfoTimer = new System.Timers.Timer();
        public FrmInfo()
        {
            InitializeComponent();
            this.BringToFront();

            timer1.Interval = 10;
            timer1.Start();
            this.TopLevel = true;
            this.InfoTimer.Interval = 980;
            InfoTimer.Elapsed += InfoTimer_Elapsed;
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void InfoTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (downTime > 0)
                {
                    downTime -= 1;
                    if (InfoType == 4)//刷卡上电,用于测试时,桩在特定状态下,有可能刷卡后无法上电,避免倒计时无限等待.
                    {
                        bool isOK = false;
                        foreach (var item in AllEquipStateData.DicBMS_AC_StateData)
                        {
                            if (item.Value.SystemState.Contains("充电中") && item.Value.CPFrequency >= 950 && item.Value.CPFrequency <= 1050)
                            {
                                isOK = true;
                            }
                            else
                            {
                                isOK = false;
                            }
                        }
                        if (isOK)
                        {
                            InfoTimer.Stop();
                            this.Dispose();
                            this.Close();
                        }
                    }
                    if (this.lblTime.InvokeRequired)
                    {
                        this.lblTime.Invoke(new SetTimerEventHander(SetTime), downTime);
                    }
                    else
                    {
                        SetTime(downTime);
                    }
                }
                else
                {
                    if (InfoType == 1)
                    {
                        SystemEvent.SendCountDownTimerResult(true);
                    }
                    else if (InfoType == 2)
                    {
                        SystemEvent.SendManualVerifyResult(DicManualVerifyResult);
                    }
                    else if (InfoType == 3)
                    {
                        SystemEvent.SendInputData(txtInput.Text);
                    }
                    else if(InfoType==5)
                    {
                        SystemEvent.SendCountDownTimerResult(true);
                    }
                    else if (InfoType == 6)
                    {
                        SystemEvent.SendInputData($"{uiTextBox_inTime1.Text}-{uiTextBox_inTime2.Text}-{uiTextBox_inTime3.Text} {uiTextBox_inTime4.Text}:{uiTextBox_inTime5.Text}:{uiTextBox_inTime6.Text}");
                    }

                    if (this.IsHandleCreated)
                    {
                        this.Invoke(new Action(() =>
                        {
                            InfoTimer.Stop();
                            this.Dispose();
                            this.Close();
                        }));
                    }
                    else
                    {
                        InfoTimer.Stop();
                        this.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                SystemEvent.SendCountDownTimerResult(false);
                if (this.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        InfoTimer.Stop();
                        this.Close();
                    }));
                }
                else
                {
                    InfoTimer.Stop();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static FrmInfo GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmInfo();
                Instance.SetCustomerLogo();
            }
            Instance.TopMost = true;
            return Instance;
        }

        private void SetCustomerLogo()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.ToString().Trim().Equals("TPK"))
            {
                //pictureBox1.Size = new Size(400, 60);
                //pictureBox1.Dock = DockStyle.Fill;
                this.Icon = Properties.Resources.TPK_Icon;
            }
        }

        /// <summary>
        /// 弹窗提示
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="time">时间</param>
        /// <param name="type">提示类型 0-纯倒计时提示信息。 1-倒计时等待选择   2-倒计时等待选择枪位结论 3-等待输入数据</param>
        /// <param name="tag">输入数据的默认值</param>
        public void CountDown(string message, int time, int type, string tag)
        {
            InfoType = type;
            this.lblInfo.Text = message;
            this.lblTime.Text = time.ToString();
            this.txtInput.Text = tag.ToString();
            downTime = time;
            if (type == 0 || type == 2 || type == 4)
            {
                pnlChargerID.Visible = true;
                pnlInputInfo.Visible = false;
                txtInput.Visible = false;
                panel_timeInput.Visible = false;
                btnOK.Visible = true;
                btnTrue.Visible = false;
                btnFalse.Visible = false;
            }
            else if (type == 1)
            {
                pnlChargerID.Visible = true;
                pnlInputInfo.Visible = false;
                panel_timeInput.Visible = false;
                btnOK.Visible = false;
                btnTrue.Visible = true;
                btnFalse.Visible = true;
            }
            else if (type == 3)
            {
                pnlChargerID.Visible = false;
                panel_timeInput.Visible = false;
                pnlInputInfo.Visible = true;
                
                btnOK.Visible = true;
                btnTrue.Visible = false;
                btnFalse.Visible = false;
            }
            else if (type == 5)
            {
                pnlInputInfo.Visible = false;
                panel_timeInput.Visible = false;
                pnlChargerID.Visible = true;
                txtInput.Visible = false;
                btnOK.Visible = false;
                btnTrue.Visible = false;
                btnFalse.Visible = false;
            }
            else if (InfoType == 6)
            {
                pnlChargerID.Visible = false;
                pnlInputInfo.Visible = false;
                txtInput.Visible = false;
                btnOK.Visible = true;
                btnTrue.Visible = false;
                btnFalse.Visible = false;

                string sDateTime = DateTime.Now.AddSeconds(20).ToString("yyyy-MM-dd HH:mm:ss");
                string[] t = sDateTime.Split(' ');
                string[] t1 = t[0].Split('-');
                string[] t2 = t[1].Split(':');
                uiTextBox_inTime1.Text = t1[0];
                uiTextBox_inTime2.Text = t1[1];
                uiTextBox_inTime3.Text = t1[2];
                uiTextBox_inTime4.Text = t2[0];
                uiTextBox_inTime5.Text = t2[1];
                uiTextBox_inTime6.Text = t2[2];
            }
            //  timer1.Start();
            InfoTimer.Start();

            if (this.InvokeRequired)
            {
                this.Invoke(new SetShowDialogHander(ShowThisDialog));
            }
            else
            {
                ShowDialog();
            }

        }
        private delegate void SetShowDialogHander();
        private void ShowThisDialog()
        {
            this.ShowDialog();
        }


        private delegate void SetTimerEventHander(int time);
        private void SetTime(int time)
        {
            this.lblTime.Text = time.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (InfoType == 2)
            {
                SystemEvent.SendManualVerifyResult(DicManualVerifyResult);
            }
            else if (InfoType == 3)
            {
                SystemEvent.SendInputData(txtInput.Text);
            }
            else if (InfoType == 6)
            {
                SystemEvent.SendInputData($"{uiTextBox_inTime1.Text}-{uiTextBox_inTime2.Text}-{uiTextBox_inTime3.Text} {uiTextBox_inTime4.Text}:{uiTextBox_inTime5.Text}:{uiTextBox_inTime6.Text}");
            }
            InfoTimer.Stop();
            this.Dispose();
            this.Close();
        }

        private void btnTrue_Click(object sender, EventArgs e)
        {
            if (InfoType == 1)
            {
                SystemEvent.SendCountDownTimerResult(true);
            }
            else if (InfoType == 2)
            {
                SystemEvent.SendManualVerifyResult(DicManualVerifyResult);
            }
            InfoTimer.Stop();
            this.Dispose();
            this.Close();
        }

        private void btnFalse_Click(object sender, EventArgs e)
        {
            SystemEvent.SendCountDownTimerResult(false);
            InfoTimer.Stop();
            this.Dispose();
            this.Close();
        }

        private void FrmInfo_Load(object sender, EventArgs e)
        {
            DicManualVerifyResult.Clear();
            txtInput.Text = "";
            if (InfoType == 2)
            {
                pnlChargerID.Enabled = true;
            }
            else
            {
                pnlChargerID.Enabled = false;
            }
            if ((InfoType == 0 || InfoType == 1 || InfoType == 2))
            {
                pnlChargerID.Visible = true;
                pnlInputInfo.Visible = false;
            }
            else
            {
                pnlChargerID.Visible = false;
                pnlInputInfo.Visible = true;
            }
            foreach (var item in businessManage.lstChargerInfo)
            {
                if(!item.IsCheck)
                    continue;

                DicManualVerifyResult.Add(item.ChargerId, item.IsCheck);
                switch (item.ChargerId)
                {
                    case 1:
                        checkBox1.Visible = item.IsCheck;
                        checkBox1.Checked = item.IsCheck;
                        break;
                    case 2:
                        checkBox2.Visible = item.IsCheck;
                        checkBox2.Checked = item.IsCheck;
                        break;
                    case 3:
                        checkBox3.Visible = item.IsCheck;
                        checkBox3.Checked = item.IsCheck;
                        break;
                    case 4:
                        checkBox4.Visible = item.IsCheck;
                        checkBox4.Checked = item.IsCheck;
                        break;
                    case 5:
                        checkBox5.Visible = item.IsCheck;
                        checkBox5.Checked = item.IsCheck;
                        break;
                    case 6:
                        checkBox6.Visible = item.IsCheck;
                        checkBox6.Checked = item.IsCheck;
                        break;
                    case 7:
                        checkBox7.Visible = item.IsCheck;
                        checkBox7.Checked = item.IsCheck;
                        break;
                    case 8:
                        checkBox8.Visible = item.IsCheck;
                        checkBox8.Checked = item.IsCheck;
                        break;
                }
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (DicManualVerifyResult.ContainsKey(1))
            {
                DicManualVerifyResult[1] = checkBox1.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(2))
            {
                DicManualVerifyResult[2] = checkBox2.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(3))
            {
                DicManualVerifyResult[3] = checkBox3.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(4))
            {
                DicManualVerifyResult[4] = checkBox4.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(5))
            {
                DicManualVerifyResult[5] = checkBox5.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(6))
            {
                DicManualVerifyResult[6] = checkBox6.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(7))
            {
                DicManualVerifyResult[7] = checkBox7.Checked;
            }
            if (DicManualVerifyResult.ContainsKey(8))
            {
                DicManualVerifyResult[8] = checkBox8.Checked;
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }
    }
}
