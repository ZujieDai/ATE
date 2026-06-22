using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI;
using SaiTer.ATE.UI.UserControls;
using SaiTer.ATE.UI.UserControls.EquipOperate;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmEquipOperate : UIForm
    {
        private XmlInfoAndAssembly _XmlInfoAndAssembly = XmlInfoAndAssembly.GetInstance();
        private ucConnectState _ucConnectState = ucConnectState.GetInstance();
        public FrmEquipOperate()
        {
            InitializeComponent();
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;
            IniControlClass();
        }

        private void SystemEvent_SendChangeLanguageEvent()
        {
            this.Text = LanguageManager.GetByKey("FrmEquipOperate");
        }

        private static FrmEquipOperate Instance = null;
        /// <summary>
        /// 设备操作窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmEquipOperate GetInstance()
        {
            if (Instance == null)
                Instance = new FrmEquipOperate();
            Instance.Activate();
            return Instance;
        }

        private void FrmEquipOperate_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }
        private void FrmEquipOperate_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();
        }
        private void FrmEquipOperate_Load(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();      
            IniControlEquip();

            
            long x = stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();

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

        private void IniControlClass()
        {
            foreach (var item in _ucConnectState.lstEquipment)
            {
                if (!item.EquipMentName.Contains(LanguageManager.GetByKey("示波器")) && !item.EquipMentName.Contains(LanguageManager.GetByKey("功率分析仪")) &&
                    !item.EquipMentName.Contains(LanguageManager.GetByKey("万用表")) && //!item.EquipMentName.Contains(LanguageManager.GetByKey("录波板")) &&
                    !item.EquipMentName.Contains("电表") && !item.EquipMentName.Contains("录波仪") && !item.EquipMentName.Contains("临时"))
                {
                    string equipMentName = item.EquipMentName;
                    if(item.EquipMentName.Contains(LanguageManager.GetByKey("交流电阻负载")) || item.EquipMentName.Contains(LanguageManager.GetByKey("交流电阻负载")))
                    {
                        equipMentName = LanguageManager.GetByKey("电阻负载");
                    }
                    TabPage tpEquipClass = new TabPage(equipMentName);
                    bool isexist = false;

                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        if (item.EquipMentName.Contains(page.Text))
                        {
                            isexist = true;
                        }
                    }
                    if (!isexist)
                    {
                        tbMenu.TabPages.Add(tpEquipClass);
                        isexist = true;
                    }
                }
            }
            foreach (var item in _ucConnectState.lstEquipment)
            {
                foreach (TabPage page in tbMenu.TabPages)
                {
                    if (item.EquipMentName.Contains(page.Text))
                    {
                        TabControl tc = new TabControl();
                        tc.Dock = DockStyle.Fill;
                        page.Controls.Add(tc);
                    }
                }
            }
        }
        private void IniControlEquip()
        {
            foreach (var item in _ucConnectState.lstEquipment)
            {
                if (item.EquipMentClassName == "emtSafety")
                {
                    TabPage tp = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("安规"))
                        {
                            tb.TabPages.Add(tp);
                            UcSafety uc = new UcSafety();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtSafety_SE7441")
                {
                    TabPage tp = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("安规"))
                        {
                            tb.TabPages.Add(tp);
                            UcSafety_SE7441 uc = new UcSafety_SE7441();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtBMS_AC")
                {
                    TabPage tp = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("交流导引BMS"))
                        {
                            tb.TabPages.Add(tp);
                            UcBMS_AC uc = new UcBMS_AC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtBMS_GB_DC")
                {
                    TabPage tp1 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    TabPage tp2 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪互操作"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("国标直流导引BMS"))
                        {
                            tb.TabPages.Add(tp1);
                            UcBMS_DC uc = new UcBMS_DC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp1.Controls.Add(uc);


                            tb.TabPages.Add(tp2);
                            UcBMS_DC_Intero ucBms = new UcBMS_DC_Intero();
                            ucBms.ChargerID = ucBms.ChargerID;
                            ucBms.Dock = DockStyle.Fill;
                            tp2.Controls.Add(ucBms);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtBMS_EU_DC")
                {
                    TabPage tp1 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    TabPage tp2 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪互操作"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("欧标直流导引BMS"))
                        {
                            tb.TabPages.Add(tp1);
                            UcBMS_EU_DC uc = new UcBMS_EU_DC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp1.Controls.Add(uc);

                            tb.TabPages.Add(tp2);
                            UcBMS_EU_DC_Intero ucBms = new UcBMS_EU_DC_Intero();
                            ucBms.ChargerID = ucBms.ChargerID;
                            ucBms.Dock = DockStyle.Fill;
                            tp2.Controls.Add(ucBms);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtBMS_USA_DC")
                {
                    TabPage tp1 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    TabPage tp2 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪互操作"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("美标直流导引BMS"))
                        {
                            tb.TabPages.Add(tp1);
                            UcBMS_USA_DC uc = new UcBMS_USA_DC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp1.Controls.Add(uc);

                            tb.TabPages.Add(tp2);
                            UcBMS_USA_DC_Intero ucBms = new UcBMS_USA_DC_Intero();
                            ucBms.ChargerID = ucBms.ChargerID;
                            ucBms.Dock = DockStyle.Fill;
                            tp2.Controls.Add(ucBms);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtBMS_JP_DC")
                {
                    TabPage tp1 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪"));
                    TabPage tp2 = new TabPage(item.ChargerID + LanguageManager.GetByKey("号枪互操作"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text == LanguageManager.GetByKey("日标直流导引BMS"))
                        {
                            tb.TabPages.Add(tp1);
                            UcBMS_JP_DC uc = new UcBMS_JP_DC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp1.Controls.Add(uc);

                            tb.TabPages.Add(tp2);
                            UcBMS_JP_DC_Intero ucBms = new UcBMS_JP_DC_Intero();
                            ucBms.ChargerID = ucBms.ChargerID;
                            ucBms.Dock = DockStyle.Fill;
                            tp2.Controls.Add(ucBms);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtACSource"))
                {
                    //由于常德TM使用的稳压源无法做过压欠压，所以加了一个变频源进行控制
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.Equals("TM"))
                        if (item.ChargerID == 2)
                            continue;
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("交流源")))
                        {
                            tb.TabPages.Add(tp);
                            UcACSource uc = new UcACSource();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtResistanceLoad_AC"
                    || item.EquipMentClassName == "emtResistanceLoad_DC")
                {
                    string pageName = item.ChargerID.ToString() + LanguageManager.GetByKey("号枪") + "_";
                    pageName += item.EquipMentClassName.Contains("AC") ? LanguageManager.GetByKey("交流") : LanguageManager.GetByKey("直流");
                    TabPage tp = new TabPage(pageName);
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("电阻负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcResisLoad uc = new UcResisLoad();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName == "emtResistanceLoad_MultiChannel_AC")
                {
                    //string pageName = item.ChargerID.ToString() + LanguageManager.GetByKey("号枪") + "_";
                    //pageName += LanguageManager.GetByKey("交流");
                    string pageName = item.ChargerID.ToString() + LanguageManager.GetByKey("号枪");
                    TabPage tp = new TabPage(pageName);
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("电阻负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcResisLoad_MultiChannel_AC uc = new UcResisLoad_MultiChannel_AC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName =="emtResistanceLoad_MultiChannel_DC")
                {
                    string pageName = item.ChargerID.ToString() + LanguageManager.GetByKey("号枪") + "_";
                    pageName +=  LanguageManager.GetByKey("直流");
                    TabPage tp = new TabPage(pageName);
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("电阻负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcResisLoad_MultiChannel_DC uc = new UcResisLoad_MultiChannel_DC();
                            uc.ChargerID = item.ChargerID;
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }

                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtControlBoard"))
                {
                    //由于常德TM使用的稳压源无法做过压欠压，所以加了一个变频源进行控制
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.Equals("TM"))
                        if (item.ChargerID == 2)
                            continue;
                    //TabPage tp = new TabPage(LanguageManager.GetByKey("程控板"));
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("程控板")))
                        {
                            tb.TabPages.Add(tp);
                            UcControlBoard uc = new UcControlBoard();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtDIORelay"))
                {
                    //TabPage tp = new TabPage(LanguageManager.GetByKey("程控板"));
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("继电器")))
                        {
                            tb.TabPages.Add(tp);
                            UcDIORelay uc = new UcDIORelay();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtZJLeakageCurrent"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("剩余电流保护测试仪")))
                        {
                            tb.TabPages.Add(tp);
                            UcZJLeakageCurrentTester uc = new UcZJLeakageCurrentTester();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtTMLeakageCurrent"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("剩余电流保护测试仪")))
                        {
                            tb.TabPages.Add(tp);
                            ucTMLeakageCurrentTester uc = new ucTMLeakageCurrentTester();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtQCLeakageCurrent"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("剩余电流保护测试仪")))
                        {
                            tb.TabPages.Add(tp);
                            UcQCLeakageCurrentTester uc = new UcQCLeakageCurrentTester();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtElectronicLoad"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("电子负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcElectronicLoad uc = new UcElectronicLoad();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                //else if (item.EquipMentClassName.Contains("emtFeedbackLoad"))
                else if (item.EquipMentClassName=="emtFeedbackLoad"
                    || item.EquipMentClassName == "emtFeedbackLoad_SZHY"
                    || item.EquipMentClassName == "emtFeedbackLoad_DC_ST"
                    || item.EquipMentClassName == "emtFeedbackLoad_YKR")
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("回馈负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcFeedbackLoad uc = new UcFeedbackLoad();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtFeedbackLoad_AC"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("交流回馈负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcFeedbackLoadAC uc = new UcFeedbackLoadAC();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtLoopFeedbackLoad"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains( LanguageManager.GetByKey("回馈负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcLoopFeedbackLoad uc = new UcLoopFeedbackLoad();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtStarLoopFeedbackLoad"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("回馈负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcStarLoopFeedbackLoad uc = new UcStarLoopFeedbackLoad();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtAuxiliaryLoadCtrl"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("辅源负载")))
                        {
                            tb.TabPages.Add(tp);
                            UcAuxiliaryLoad uc = new UcAuxiliaryLoad();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtWaveRecoderBoard30"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("录波板")))
                        {
                            tb.TabPages.Add(tp);
                            UcWaveRecoder uc = new UcWaveRecoder();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }
                else if (item.EquipMentClassName.Contains("emtCharger_NTGX"))
                {
                    TabPage tp = new TabPage(item.ChargerID.ToString() + LanguageManager.GetByKey("号枪"));
                    foreach (TabPage page in tbMenu.TabPages)
                    {
                        TabControl tb = page.Controls[0] as TabControl;
                        if (page.Text.Contains(LanguageManager.GetByKey("充电桩模拟器")))
                        {
                            tb.TabPages.Add(tp);
                            UcCharger_NTGX uc = new UcCharger_NTGX();
                            uc.Dock = DockStyle.Fill;
                            tp.Controls.Add(uc);
                        }
                    }
                    continue;
                }

            }
        }



        private void tbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (tbMenu.SelectedTab.Text == "交流导引BMS")
            //{
            //    try
            //    {
            //        foreach (var item in tbMenu.SelectedTab.Controls)
            //        {
            //            TabControl tc = item as TabControl;
            //            foreach (var temp in tc.TabPages)
            //            {
            //                TabPage tp1 = temp as TabPage;
            //                UcBMS_AC uc = tp1.Controls[0] as UcBMS_AC;
            //                if (uc != null)
            //                {
            //                    uc.BMSGetKState();
            //                }
            //            }
            //        }

            //    }
            //    catch (Exception ex) { Log.Log.LogException(ex); }
            //}

        }
    }
}
