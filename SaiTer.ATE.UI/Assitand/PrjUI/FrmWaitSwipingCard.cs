using SaiTer.ATE.Business;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.UserControls;
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

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    /// <summary>
    /// 等待刷卡倒计时窗体
    /// </summary>
    public partial class FrmWaitSwipingCard : UIForm
    {
        private List<int> lstIDs = new List<int>();
        private static FrmWaitSwipingCard Instance = null;
        //private AllEquipStateData EquipStateData = AllEquipStateData.GetInstance();
        private BusinessManage businessManage = BusinessManage.GetInstance();
        private int downTime = 300;//倒计时时间（秒）
        private EmChargerType ChargerType = 0;
        private double tBMSDemandVoltage;//导引需求电压
        /// <summary>
        /// 弹窗类型 0：等待刷卡  1：插枪检测     2:只检测刷卡（有PWM波）
        /// </summary>
        private int PopupType = 0;//弹窗类型 0：等待刷卡  1：插枪检测    2:只检测刷卡（有PWM波） 3:欧标直流检测刷卡,4:国标直流只检测是否刷卡
        public FrmWaitSwipingCard(EmChargerType chargerType)
        {
            InitializeComponent();
            this.timer1.Interval = 980;
            ChargerType = chargerType;

            //增加按钮提示
            UIToolTip utt = new UIToolTip();
            utt.BackColor = Color.LightGoldenrodYellow;
            utt.ForeColor= Color.Red;
            utt.SetToolTip(btnOK, "刷卡后充电桩无法启动充电，请点击确认按钮！！！");

        }

        public static FrmWaitSwipingCard GetInstance(EmChargerType BMSType)
        {
            if (Instance == null)
                Instance = new FrmWaitSwipingCard(BMSType);
            Instance.TopMost = true;
            return Instance;
        }
        private void frmWaitSwipingCard_Load(object sender, EventArgs e)
        {
            foreach (var item in businessManage.lstChargerInfo)
            {
                switch (item.ChargerId)
                {
                    case 1:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox1.Visible = false;
                            checkBox1.Checked = true;
                        }
                        else
                        {
                            checkBox1.Visible = item.IsCheck;
                            checkBox1.Checked = !item.IsCheck;
                        }
                        break;
                    case 2:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox2.Visible = false;
                            checkBox2.Checked = true;
                        }
                        else
                        {
                            checkBox2.Visible = item.IsCheck;
                            checkBox2.Checked = !item.IsCheck;
                        }
                        break;
                    case 3:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox3.Visible = false;
                            checkBox3.Checked = true;
                        }
                        else
                        {
                            checkBox3.Visible = item.IsCheck;
                            checkBox3.Checked = !item.IsCheck;
                        }
                        break;
                    case 4:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox4.Visible = false;
                            checkBox4.Checked = true;
                        }
                        else
                        {
                            checkBox4.Visible = item.IsCheck;
                            checkBox4.Checked = !item.IsCheck;
                        }
                        break;
                    case 5:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox5.Visible = false;
                            checkBox5.Checked = true;
                        }
                        else
                        {
                            checkBox5.Visible = item.IsCheck;
                            checkBox5.Checked = !item.IsCheck;
                        }
                        break;
                    case 6:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox6.Visible = false;
                            checkBox6.Checked = true;
                        }
                        else
                        {
                            checkBox6.Visible = item.IsCheck;
                            checkBox6.Checked = !item.IsCheck;
                        }
                        break;
                    case 7:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox7.Visible = false;
                            checkBox7.Checked = true;
                        }
                        else
                        {
                            checkBox7.Visible = item.IsCheck;
                            checkBox7.Checked = !item.IsCheck;
                        }
                        break;
                    case 8:
                        if (!lstIDs.Contains(item.ChargerId))
                        {
                            checkBox8.Visible = false;
                            checkBox8.Checked = true;
                        }
                        else
                        {
                            checkBox8.Visible = item.IsCheck;
                            checkBox8.Checked = !item.IsCheck;
                        }
                        break;
                }
            }
            SetCustomerLogo();
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

        public bool WaitSwipingCard(List<int> lstIDs, double Voltage, int type)
        {
            try
            {
                this.lstIDs = lstIDs;
                foreach (var item in businessManage.lstChargerInfo)
                {
                    switch (item.ChargerId)
                    {
                        case 1:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox1.Visible = false;
                                checkBox1.Checked = true;
                            }
                            else
                            {
                                checkBox1.Visible = item.IsCheck;
                                checkBox1.Checked = !item.IsCheck;
                            }
                            break;
                        case 2:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox2.Visible = false;
                                checkBox2.Checked = true;
                            }
                            else
                            {
                                checkBox2.Visible = item.IsCheck;
                                checkBox2.Checked = !item.IsCheck;
                            }
                            break;
                        case 3:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox3.Visible = false;
                                checkBox3.Checked = true;
                            }
                            else
                            {
                                checkBox3.Visible = item.IsCheck;
                                checkBox3.Checked = !item.IsCheck;
                            }
                            break;
                        case 4:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox4.Visible = false;
                                checkBox4.Checked = true;
                            }
                            else
                            {
                                checkBox4.Visible = item.IsCheck;
                                checkBox4.Checked = !item.IsCheck;
                            }
                            break;
                        case 5:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox5.Visible = false;
                                checkBox5.Checked = true;
                            }
                            else
                            {
                                checkBox5.Visible = item.IsCheck;
                                checkBox5.Checked = !item.IsCheck;
                            }
                            break;
                        case 6:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox6.Visible = false;
                                checkBox6.Checked = true;
                            }
                            else
                            {
                                checkBox6.Visible = item.IsCheck;
                                checkBox6.Checked = !item.IsCheck;
                            }
                            break;
                        case 7:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox7.Visible = false;
                                checkBox7.Checked = true;
                            }
                            else
                            {
                                checkBox7.Visible = item.IsCheck;
                                checkBox7.Checked = !item.IsCheck;
                            }
                            break;
                        case 8:
                            if (!lstIDs.Contains(item.ChargerId))
                            {
                                checkBox8.Visible = false;
                                checkBox8.Checked = true;
                            }
                            else
                            {
                                checkBox8.Visible = item.IsCheck;
                                checkBox8.Checked = !item.IsCheck;
                            }
                            break;
                    }
                }
                tBMSDemandVoltage = Voltage;
                PopupType = type;
                timer1.Start();
                //是否加载自动检测
                bool isAutoTest = false;
                string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
                if (strAutoTest != null)
                {
                    isAutoTest = bool.Parse(strAutoTest);
                }
                if (isAutoTest)
                {
                    this.Size = new Size(3, 3);
                    this.Location = new Point(1920, 1080);
                    this.StartPosition = FormStartPosition.Manual;
                }
                this.ShowDialog();
                this.Activate();
                return true;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("正在中止线程"))
                {
                    Log.Log.LogException(ex);
                }
                return false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (downTime > 0)
                {
                    downTime -= 1;

                    if (this.lblTime.InvokeRequired)
                    {
                        if (PopupType == 0)
                        {
                            this.lblTime.Invoke(new SetTimerEventHander(WaiteSwipingCard), downTime);
                        }
                        else if (PopupType == 2)
                        {
                            this.lblTime.Invoke(new SetTimerEventHander(WaiteSwipingCard_Only), downTime);
                        }
                        else if (PopupType == 3)
                        {
                            this.lblTime.Invoke(new SetTimerEventHander(WaiteSwipingCard_Only_EuropeDC), downTime);
                        }
                        else if (PopupType ==4)
                        {
                            this.lblTime.Invoke(new SetTimerEventHander(WaiteSwipingCard_Only_ChinaDC), downTime);
                        }
                        else
                        {
                            this.lblTime.Invoke(new SetTimerEventHander(CheckChargerIn), downTime);
                        }

                    }
                    else
                    {

                        if (PopupType == 0)
                        {
                            WaiteSwipingCard(downTime);
                        }
                        else if (PopupType == 2)
                        {
                            WaiteSwipingCard_Only(downTime);
                        }
                        else if (PopupType == 3)
                        {
                            WaiteSwipingCard_Only_EuropeDC(downTime);
                        }
                        else if(PopupType==4)
                        {
                            WaiteSwipingCard_Only_ChinaDC(downTime);
                        }
                        else
                        {
                            CheckChargerIn(downTime);
                        }
                    }
                }
                else
                {
                    timer1.Stop();
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("正在中止线程"))
                {
                    Log.Log.LogException(ex);
                }
            }

        }
        private delegate void SetTimerEventHander(int time);

        private int ChargingTime = 0;       //记录启动次数
        private int WaitingTime = 0;        //等待时间超过10s
        private void WaiteSwipingCard(int time)
        {
            this.Text = "等待刷卡";
            this.label2.Text = "请给以下充电桩刷卡上电：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();

            if (ChargerType == EmChargerType.Charger_GB_DC)
            {
                if (GetChinaState(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState) > 1)
                {
                    this.Text = "等待电压稳定";
                    this.label2.Text = "请耐心等待充电桩电压稳定：";
                }
                if (checkBox1.Visible && AllEquipStateData.DicBMS_DC_StateData[1].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage<= tBMSDemandVoltage+50)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && AllEquipStateData.DicBMS_DC_StateData[2].ChargingState.Contains("充电中")
                   && AllEquipStateData.DicBMS_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[3].ChargingState.Contains("充电中")
                  && AllEquipStateData.DicBMS_DC_StateData[3].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[4].ChargingState.Contains("充电中")
                  && AllEquipStateData.DicBMS_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox4.Checked = true;
                }
                if (checkBox5.Visible && AllEquipStateData.DicBMS_DC_StateData[5].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[5].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[5].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox5.Checked = true;
                }
                if (checkBox6.Visible && AllEquipStateData.DicBMS_DC_StateData[6].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[6].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[6].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox6.Checked = true;
                }
                if (checkBox7.Visible && AllEquipStateData.DicBMS_DC_StateData[7].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[7].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[7].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox7.Checked = true;
                }
                if (checkBox8.Visible && AllEquipStateData.DicBMS_DC_StateData[8].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[8].ChargingVoltage + 50 >= tBMSDemandVoltage && AllEquipStateData.DicBMS_DC_StateData[8].ChargingVoltage <= tBMSDemandVoltage + 50)
                {
                    checkBox8.Checked = true;
                }
            }
            else if (ChargerType == EmChargerType.Charger_GB_AC ||
                ChargerType == EmChargerType.Charger_USA_AC ||
                ChargerType == EmChargerType.Charger_EUR_AC)
            {
                if (checkBox1.Visible && AllEquipStateData.DicBMS_AC_StateData[1].SystemState.Contains("充电中")
                       && AllEquipStateData.DicBMS_AC_StateData[1].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[2].SystemState.Contains("充电中")
                   && AllEquipStateData.DicBMS_AC_StateData[2].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && AllEquipStateData.DicBMS_AC_StateData[3].SystemState.Contains("充电中")
                  && AllEquipStateData.DicBMS_AC_StateData[3].PhaseA_Voltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && AllEquipStateData.DicBMS_AC_StateData[4].SystemState.Contains("充电中")
                  && AllEquipStateData.DicBMS_AC_StateData[4].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox4.Checked = true;
                }
            }
            else if (ChargerType == EmChargerType.Charger_EUR_DC ||
                 ChargerType == EmChargerType.Charger_USA_DC)
            {
                if (GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState) > 1)
                {
                    this.Text = "等待电压稳定";
                    this.label2.Text = "请耐心等待充电桩电压稳定：";
                }
                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("SZHY") && ChargerType == EmChargerType.Charger_USA_DC)
                {
                    if (checkBox1.Visible && (AllEquipStateData.DicBMS_USA_DC_StateData[2].SystemState.Contains("CurrentDemandReq")
                        || AllEquipStateData.DicBMS_USA_DC_StateData[2].SystemState.Contains("CurrentDemandRes"))
                    && AllEquipStateData.DicBMS_USA_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox1.Checked = true;
                    }
                    else if (checkBox1.Visible && (AllEquipStateData.DicBMS_USA_DC_StateData[1].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_USA_DC_StateData[1].SystemState.Contains("CurrentDemandRes"))
                        && AllEquipStateData.DicBMS_USA_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox1.Checked = true;
                    }
                    if (checkBox2.Visible
                        && (AllEquipStateData.DicBMS_USA_DC_StateData[2].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_USA_DC_StateData[2].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_USA_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox2.Checked = true;
                    }
                    if (checkBox3.Visible
                       && (AllEquipStateData.DicBMS_USA_DC_StateData[3].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_USA_DC_StateData[3].SystemState.Contains("CurrentDemandRes"))
                      && AllEquipStateData.DicBMS_USA_DC_StateData[3].ChargingVoltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                    {
                        checkBox3.Checked = true;
                    }
                    if (checkBox4.Visible
                       && (AllEquipStateData.DicBMS_USA_DC_StateData[4].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_USA_DC_StateData[4].SystemState.Contains("CurrentDemandRes"))
                      && AllEquipStateData.DicBMS_USA_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox4.Checked = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && ChargerType == EmChargerType.Charger_USA_DC)
                    {
                        string state = AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState;
                        if (checkBox1.Visible && (state.Contains("CurrentDemandReq") || state.Contains("CurrentDemandRes"))
                        && AllEquipStateData.DicBMS_EU_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                        {
                            checkBox1.Checked = true;
                        }
                    }
                    else if(!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && ChargerType == EmChargerType.Charger_EUR_DC)
                    {
                        string state = AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState;
                        if (checkBox1.Visible && (state.Contains("CurrentDemandReq") || state.Contains("CurrentDemandRes"))
                            && AllEquipStateData.DicBMS_EU_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                        {
                            checkBox1.Checked = true;
                        }
                        else if(state.Contains("Not start") || state.Contains("WaitingForCharging"))
                        {
                            if (WaitingTime > 10 /*&& (!ChargingState.Contains("Not start") && !ChargingState.Contains("WaitingForCharging"))*/)  //历史状态已经超过了待机，当前状态为待机则肯定是起桩失败了
                            {
                                if (ChargingTime >= 3)
                                {
                                    timer1.Stop();
                                    this.Dispose();
                                    this.Close();
                                    return;
                                }
                                ChargingTime++;
                                WaitingTime = 0;
                                SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $" 启动充电失败，重新启动（{ChargingTime}/3）...");
                                var bms = BusinessManage.GetInstance()._xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.BMS;
                                SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 模拟插拔枪中...");
                                bool[] Ks = new bool[24];
                                Ks[0] = true;//DC+DC-控制
                                Ks[1] = true;//CC信号控制
                                Ks[2] = false;//CP信号控制
                                Ks[4] = true;//PE信号控制
                                bms.BMSSetKState_EU_DC(new List<int>() { 1 }, 390, Ks, 0, 0);
                                System.Threading.Thread.Sleep(2000);
                                Ks[2] = true;//CP信号控制
                                bms.BMSSetKState_EU_DC(new List<int>() { 1 }, 390, Ks.ToArray(), 0, 0);
                                System.Threading.Thread.Sleep(2000);
                                SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 停止充电...");
                                bms.BMS_OFF(new List<int>() { 1 });
                                System.Threading.Thread.Sleep(2000);
                                SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 开始充电...");
                                bms.BMS_ON(new List<int>() { 1 });
                            }
                            else
                            {
                                if (state.Contains("Not start") || state.Contains("WaitingForCharging"))
                                    WaitingTime++;
                            }
                        }
                    }
                    else if (checkBox1.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState.Contains("CurrentDemandRes"))
                            && AllEquipStateData.DicBMS_EU_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox1.Checked = true;
                    }
                    if (checkBox2.Visible
                        && (AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_EU_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox2.Checked = true;
                    }
                    if (checkBox3.Visible
                       && (AllEquipStateData.DicBMS_EU_DC_StateData[3].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_EU_DC_StateData[3].SystemState.Contains("CurrentDemandRes"))
                      && AllEquipStateData.DicBMS_EU_DC_StateData[3].ChargingVoltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                    {
                        checkBox3.Checked = true;
                    }
                    if (checkBox4.Visible
                       && (AllEquipStateData.DicBMS_EU_DC_StateData[4].SystemState.Contains("CurrentDemandReq")
                            || AllEquipStateData.DicBMS_EU_DC_StateData[4].SystemState.Contains("CurrentDemandRes"))
                      && AllEquipStateData.DicBMS_EU_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage)
                    {
                        checkBox4.Checked = true;
                    }
                }
            }
            //else if (ChargerType == EmChargerType.Charger_USA_AC)
            //{
            //    if (checkBox1.Visible && AllEquipStateData.DicBMS_AC_StateData[1].SystemState.Contains("充电中")
            //           && AllEquipStateData.DicBMS_AC_StateData[1].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
            //    {
            //        checkBox1.Checked = true;
            //    }
            //    if (checkBox2.Visible
            //        && AllEquipStateData.DicBMS_AC_StateData[2].SystemState.Contains("充电中")
            //       && AllEquipStateData.DicBMS_AC_StateData[2].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
            //    {
            //        checkBox2.Checked = true;
            //    }
            //    if (checkBox3.Visible
            //       && AllEquipStateData.DicBMS_AC_StateData[3].SystemState.Contains("充电中")
            //      && AllEquipStateData.DicBMS_AC_StateData[3].PhaseA_Voltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
            //    {
            //        checkBox3.Checked = true;
            //    }
            //    if (checkBox4.Visible
            //       && AllEquipStateData.DicBMS_AC_StateData[4].SystemState.Contains("充电中")
            //      && AllEquipStateData.DicBMS_AC_StateData[4].PhaseA_Voltage + 50 >= tBMSDemandVoltage)
            //    {
            //        checkBox4.Checked = true;
            //    }
            //}
            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked 
                && checkBox5.Checked && checkBox6.Checked && checkBox7.Checked && checkBox8.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }

        /// <summary>
        /// 只检测刷卡交流（有PWM波即可）
        /// </summary>
        /// <param name="time"></param>
        private void WaiteSwipingCard_Only(int time)
        {
            this.Text = "等待刷卡";
            this.label2.Text = "请给以下充电桩刷卡：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();

            if (ChargerType == EmChargerType.Charger_GB_DC )
            {
                if (GetChinaState(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState) > 1)
                {
                    this.Text = "等待电压稳定";
                    this.label2.Text = "请耐心等待充电桩电压稳定：";
                }
                if (checkBox1.Visible && AllEquipStateData.DicBMS_DC_StateData[1].ChargingState.Contains("充电中")
                        && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                { 
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && AllEquipStateData.DicBMS_DC_StateData[2].ChargingState.Contains("充电中")
                   && AllEquipStateData.DicBMS_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[3].ChargingState.Contains("充电中")
                  && AllEquipStateData.DicBMS_DC_StateData[3].ChargingVoltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[4].ChargingState.Contains("充电中")
                  && AllEquipStateData.DicBMS_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox4.Checked = true;
                }
            }
            else if (ChargerType == EmChargerType.Charger_GB_AC ||
                ChargerType == EmChargerType.Charger_USA_AC ||
                ChargerType == EmChargerType.Charger_EUR_AC)
            {
                if (checkBox1.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[1].CPFrequency >= 900)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[2].CPFrequency >= 900)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && AllEquipStateData.DicBMS_AC_StateData[3].CPFrequency >= 900)
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && AllEquipStateData.DicBMS_AC_StateData[4].CPFrequency >= 900)
                {
                    checkBox4.Checked = true;
                }
                if (checkBox5.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[5].CPFrequency >= 900)
                {
                    checkBox5.Checked = true;
                }
                if (checkBox6.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[6].CPFrequency >= 900)
                {
                    checkBox6.Checked = true;
                }
                if (checkBox7.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[7].CPFrequency >= 900)
                {
                    checkBox7.Checked = true;
                }
                if (checkBox8.Visible
                    && AllEquipStateData.DicBMS_AC_StateData[8].CPFrequency >= 900)
                {
                    checkBox8.Checked = true;
                }
            }
            else if (ChargerType == EmChargerType.Charger_EUR_DC ||
                 ChargerType == EmChargerType.Charger_USA_DC)
            {
                if (checkBox1.Visible && AllEquipStateData.DicBMS_EU_DC_StateData[1].CPFrequency >= 900)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible && AllEquipStateData.DicBMS_EU_DC_StateData[2].CPFrequency >= 900)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible && AllEquipStateData.DicBMS_EU_DC_StateData[3].CPFrequency >= 900)
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible && AllEquipStateData.DicBMS_EU_DC_StateData[4].CPFrequency >= 900)
                {
                    checkBox4.Checked = true;
                }
            }
            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked
                && checkBox5.Checked && checkBox6.Checked && checkBox7.Checked && checkBox8.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }


        /// <summary>
        /// 只检测刷卡欧标直流
        /// </summary>
        /// <param name="time"></param>
        private void WaiteSwipingCard_Only_EuropeDC(int time)
        {
            this.Text = "等待刷卡";
            this.label2.Text = "请给以下充电桩刷卡：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();

            int State1 = checkBox1.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState):-1;

            int State2 = checkBox2.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState) : -1;
            int State3 = checkBox3.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[3].SystemState):-1;
            int State4 = checkBox4.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[4].SystemState):-1;
            int State5 = checkBox5.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[5].SystemState) : -1;
            int State6 = checkBox6.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[6].SystemState) : -1;
            int State7 = checkBox7.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[7].SystemState) : -1;
            int State8 = checkBox8.Visible ? GetEuropeState(AllEquipStateData.DicBMS_EU_DC_StateData[8].SystemState) : -1;



            if (checkBox1.Visible && State1 > 2 && State1 <= 21)
            {
                checkBox1.Checked = true;
            }
            if (checkBox2.Visible && State2 > 2 && State2 <= 21)
            {
                checkBox2.Checked = true;
            }
            if (checkBox3.Visible && State3 > 2 && State3 <= 21)
            {
                checkBox3.Checked = true;
            }
            if (checkBox4.Visible && State4 > 2 && State4 <= 21)
            {
                checkBox4.Checked = true;
            }
            if (checkBox5.Visible && State5 > 2 && State5 <= 21)
            {
                checkBox5.Checked = true;
            }
            if (checkBox6.Visible && State6 > 2 && State6 <= 21)
            {
                checkBox6.Checked = true;
            }
            if (checkBox7.Visible && State7 > 2 && State7 <= 21)
            {
                checkBox7.Checked = true;
            }
            if (checkBox8.Visible && State8 > 2 && State8 <= 21)
            {
                checkBox8.Checked = true;
            }

            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked
                && checkBox5.Checked && checkBox6.Checked && checkBox7.Checked && checkBox8.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }

        /// <summary>
        /// 只检测刷卡美标直流
        /// </summary>
        /// <param name="time"></param>
        private void WaiteSwipingCard_Only_USADC(int time)
        {
            this.Text = "等待刷卡";
            this.label2.Text = "请给以下充电桩刷卡：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();

            int State1 = checkBox1.Visible ? GetEuropeState(AllEquipStateData.DicBMS_USA_DC_StateData[1].SystemState) : -1;

            int State2 = checkBox2.Visible ? GetEuropeState(AllEquipStateData.DicBMS_USA_DC_StateData[2].SystemState) : -1;
            int State3 = checkBox3.Visible ? GetEuropeState(AllEquipStateData.DicBMS_USA_DC_StateData[3].SystemState) : -1;
            int State4 = checkBox4.Visible ? GetEuropeState(AllEquipStateData.DicBMS_USA_DC_StateData[4].SystemState) : -1;



            if (checkBox1.Visible && State1 > 2 && State1 <= 21)
            {
                checkBox1.Checked = true;
            }
            if (checkBox2.Visible && State2 > 2 && State2 <= 21)
            {
                checkBox2.Checked = true;
            }
            if (checkBox3.Visible && State3 > 2 && State3 <= 21)
            {
                checkBox3.Checked = true;
            }
            if (checkBox4.Visible && State4 > 2 && State4 <= 21)
            {
                checkBox4.Checked = true;
            }

            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }


        /// <summary>
        /// 只检测刷卡欧标直流
        /// </summary>
        /// <param name="time"></param>
        private void WaiteSwipingCard_Only_ChinaDC(int time)
        {
            this.Text = "等待刷卡";
            this.label2.Text = "请给以下充电桩刷卡：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();

            int State1 = checkBox1.Visible ? GetChinaState(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState) : -1;

            int State2 = checkBox2.Visible ? GetChinaState(AllEquipStateData.DicBMS_DC_StateData[2].ChargingState) : -1;
            int State3 = checkBox3.Visible ? GetChinaState(AllEquipStateData.DicBMS_DC_StateData[3].ChargingState) : -1;
            int State4 = checkBox4.Visible ? GetChinaState(AllEquipStateData.DicBMS_DC_StateData[4].ChargingState) : -1;
            int State5 = checkBox5.Visible ? GetEuropeState(AllEquipStateData.DicBMS_DC_StateData[5].ChargingState) : -1;
            int State6 = checkBox6.Visible ? GetEuropeState(AllEquipStateData.DicBMS_DC_StateData[6].ChargingState) : -1;
            int State7 = checkBox7.Visible ? GetEuropeState(AllEquipStateData.DicBMS_DC_StateData[7].ChargingState) : -1;
            int State8 = checkBox8.Visible ? GetEuropeState(AllEquipStateData.DicBMS_DC_StateData[8].ChargingState) : -1;


            if (checkBox1.Visible && State1 >3 && State1 <= 9)
            {
                checkBox1.Checked = true;
            }
            if (checkBox2.Visible && State2 > 3 && State2 <= 9)
            {
                checkBox2.Checked = true;
            }
            if (checkBox3.Visible && State3 > 3 && State3 <= 9)
            {
                checkBox3.Checked = true;
            }
            if (checkBox4.Visible && State4 > 3 && State4 <= 9)
            {
                checkBox4.Checked = true;
            }
            if (checkBox5.Visible && State5 > 3 && State5 <= 9)
            {
                checkBox5.Checked = true;
            }
            if (checkBox6.Visible && State6 > 3 && State6 <= 9)
            {
                checkBox6.Checked = true;
            }
            if (checkBox7.Visible && State7 > 3 && State7 <= 9)
            {
                checkBox7.Checked = true;
            }
            if (checkBox8.Visible && State8 > 3 && State8 <= 9)
            {
                checkBox8.Checked = true;
            }

            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked
                && checkBox5.Checked && checkBox6.Checked && checkBox7.Checked && checkBox8.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }
        private int GetChinaState(string ChargingState)
        {
            try
            {
                if (ChargingState == "空闲状态")
                {
                    return 0;
                }

                if (ChargingState == "等待辅源")
                {
                    return 1;
                }
                if (ChargingState == "等待握手报文")
                {
                    return 2;
                }
                if (ChargingState == "等待辨识报文SPN2560=0x00")
                {
                    return 3;
                }
                if (ChargingState == "等待辨识报文SPN2560=0x01")
                {
                    return 4;


                }
                if (ChargingState == "等待CTS、CML报文")
                {
                    return 5;
                }
                if (ChargingState == "等待CRO_00报文")
                {
                    return 6;
                }
                if (ChargingState == "等待CRO_AA报文")
                {
                    return 7;
                }
                if (ChargingState == "等待充电开始")
                {
                    return 8;
                }
                if (ChargingState == "充电中")
                {
                    return 9;
                }
                if (ChargingState == "等待充电的中止报文")
                {
                    return 10;
                }
                if (ChargingState == "等待充电机充电统计")
                {
                    return 11;
                }
                if (ChargingState == "完成接收充电数据统计")
                {
                    return 12;
                }
                if (ChargingState == "充电结束")
                {
                    return 13;
                }
                if (ChargingState == "未定义")
                {
                    return -1;
                }
                return -1;
            }
            catch
            {

            }
            return -1;
        }

        private int GetEuropeState(string ChargingState)
        {
            try
            {
                if (ChargingState == "Not start")
                {
                    return 0;
                }
                if (ChargingState == "WaitingForCharging")
                {
                    return 1;
                }
                if (ChargingState == "SLAC")
                {
                    return 2;
                }
                if (ChargingState == "SDP")
                {
                    return 3;
                }
                if (ChargingState == "SessionSetupReq")
                {
                    return 4;
                }
                if (ChargingState == "SessionSetupRes")
                {
                    return 5;
                }
                if (ChargingState == "SessionDiscoveryReq")
                {
                    return 6;
                }
                if (ChargingState == "ServiceDiscoveryRes")
                {
                    return 7;
                }
                if (ChargingState == "ServicePaymentSlecionReq")
                {
                    return 8;
                }
                if (ChargingState == "ServicePaymentSelectionRes")
                {
                    return 9;
                }
                if (ChargingState == "ContractAuthenticationReq")
                {
                    return 10;
                }
                if (ChargingState == "ContractAuthenticationRes")
                {
                    return 11;
                }
                if (ChargingState == "ChargeParameterDiscoveryReq")
                {
                    return 12;
                }
                if (ChargingState == "ChargeParameterDiscoveryRes")
                {
                    return 13;
                }
                if (ChargingState == "PowerDeliveryReq")
                {
                    return 14;
                }
                if (ChargingState == "PowerDeliveryRes")
                {
                    return 15;
                }
                if (ChargingState == "CableCheckReq")
                {
                    return 16;
                }
                if (ChargingState == "CableCheckRes")
                {
                    return 17;
                }
                if (ChargingState == "PreChargeReq")
                {
                    return 18;
                }
                if (ChargingState == "PreChargeRes")
                {
                    return 19;
                }
                if (ChargingState == "CurrentDemandReq")
                {
                    return 20;
                }
                if (ChargingState == "CurrentDemandRes")
                {
                    return 21;
                }
        
            }
            catch
            {

            }
            return -1;
        }

        private void CheckChargerIn(int time)
        {
            this.Text = "插枪检测";
            this.label2.Text = "请将以下位置充电枪插入导引：";
            this.lblTime.ForeColor = Color.Fuchsia;
            this.lblTime.Text = time.ToString();


            if (ChargerType == EmChargerType.Charger_GB_DC ||
                 ChargerType == EmChargerType.Charger_USA_DC)
            {
                if (checkBox1.Visible && AllEquipStateData.DicBMS_DC_StateData[1].ChargingState == "充电中"
                        && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && AllEquipStateData.DicBMS_DC_StateData[2].ChargingState == "充电中"
                   && AllEquipStateData.DicBMS_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[3].ChargingState == "充电中"
                  && AllEquipStateData.DicBMS_DC_StateData[3].ChargingVoltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && AllEquipStateData.DicBMS_DC_StateData[4].ChargingState == "充电中"
                  && AllEquipStateData.DicBMS_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox4.Checked = true;
                }
                if (checkBox5.Visible && AllEquipStateData.DicBMS_DC_StateData[5].ChargingState == "充电中"
                        && AllEquipStateData.DicBMS_DC_StateData[5].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox5.Checked = true;
                }
                if (checkBox6.Visible && AllEquipStateData.DicBMS_DC_StateData[6].ChargingState == "充电中"
                        && AllEquipStateData.DicBMS_DC_StateData[6].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox6.Checked = true;
                }
                if (checkBox7.Visible && AllEquipStateData.DicBMS_DC_StateData[7].ChargingState == "充电中"
                        && AllEquipStateData.DicBMS_DC_StateData[7].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox7.Checked = true;
                }
                if (checkBox8.Visible && AllEquipStateData.DicBMS_DC_StateData[8].ChargingState == "充电中"
                        && AllEquipStateData.DicBMS_DC_StateData[8].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox8.Checked = true;
                }
            }
            else if (ChargerType == EmChargerType.Charger_GB_AC ||
                ChargerType == EmChargerType.Charger_USA_AC ||
                ChargerType == EmChargerType.Charger_EUR_AC)
            {
                if (checkBox1.Visible && AllEquipStateData.DicBMS_AC_StateData[1].ConnectState == "已连接")
                {
                    checkBox1.Checked = true;
                }

                if (checkBox2.Visible && AllEquipStateData.DicBMS_AC_StateData[2].ConnectState == "已连接")

                {
                    checkBox2.Checked = true;
                }

                if (checkBox3.Visible && AllEquipStateData.DicBMS_AC_StateData[3].ConnectState == "已连接")
                {
                    checkBox3.Checked = true;
                }

                if (checkBox4.Visible && AllEquipStateData.DicBMS_AC_StateData[4].ConnectState == "已连接")
                {
                    checkBox4.Checked = true;
                }

            }
            else if (ChargerType == EmChargerType.Charger_EUR_DC)
            {
                if (checkBox1.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_DC_StateData[1].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox1.Checked = true;
                }
                if (checkBox2.Visible
                    && (AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[2].SystemState.Contains("CurrentDemandRes"))
                   && AllEquipStateData.DicBMS_DC_StateData[2].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox2.Checked = true;
                }
                if (checkBox3.Visible
                   && (AllEquipStateData.DicBMS_EU_DC_StateData[3].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[3].SystemState.Contains("CurrentDemandRes"))
                  && AllEquipStateData.DicBMS_DC_StateData[3].ChargingVoltage + 50 >= Convert.ToSingle(tBMSDemandVoltage))
                {
                    checkBox3.Checked = true;
                }
                if (checkBox4.Visible
                   && (AllEquipStateData.DicBMS_EU_DC_StateData[4].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[4].SystemState.Contains("CurrentDemandRes"))
                  && AllEquipStateData.DicBMS_DC_StateData[4].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox4.Checked = true;
                }
                if (checkBox5.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[5].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[5].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_DC_StateData[5].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox5.Checked = true;
                }
                if (checkBox6.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[6].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[6].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_DC_StateData[6].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox6.Checked = true;
                }
                if (checkBox7.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[7].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[7].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_DC_StateData[7].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox7.Checked = true;
                }
                if (checkBox8.Visible && (AllEquipStateData.DicBMS_EU_DC_StateData[8].SystemState.Contains("CurrentDemandReq") || AllEquipStateData.DicBMS_EU_DC_StateData[8].SystemState.Contains("CurrentDemandRes"))
                       && AllEquipStateData.DicBMS_DC_StateData[8].ChargingVoltage + 50 >= tBMSDemandVoltage)
                {
                    checkBox8.Checked = true;
                }
            }
            //else if (ChargerType == EmChargerType.Charger_USA_AC)
            //{
            //    value = AllEquipStateData.DicBMS_AC_StateData[1].CCResistance;
            //    if (checkBox1.Visible && (value >= 1.23 && value <= 1.82))
            //    {
            //        checkBox1.Checked = true;
            //    }
            //    if (checkBox2.Visible && (value >= 1.23 && value <= 1.82))

            //    {
            //        checkBox2.Checked = true;
            //    }
            //    if (checkBox3.Visible && (value >= 1.23 && value <= 1.82))
            //    {
            //        checkBox3.Checked = true;
            //    }
            //    if (checkBox4.Visible && (value >= 1.23 && value <= 1.82))
            //    {
            //        checkBox4.Checked = true;
            //    }
            //}

            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked
                && checkBox5.Checked && checkBox6.Checked && checkBox7.Checked && checkBox8.Checked)
            {
                timer1.Stop();
                this.Dispose();
                this.Close();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Dispose();
            this.Close();
        }
    }
}
