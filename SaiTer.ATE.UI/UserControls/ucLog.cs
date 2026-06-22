using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.DataModel;
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
using SaiTer.ATE.Manage;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucLog : UserControl
    {
        public ucLog()
        {
            InitializeComponent();
            string txt = "检测系统开启处于空闲状态";
            this.txtLog.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + LanguageManager.GetByKey(txt);
            Log.Log.LogMessage(txt, "检测流程日志");
            SystemEvent.SendLogMessageEvent += SystemEvent_SendLogMessageEvent;
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;
            //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            //if (Customer != null && Customer.Contains("HR"))//青岛HR， 这里的区别是HR是双枪同测。现在这个逻辑暂时不支持双枪
            BusinessManage BCM = BusinessManage.GetInstance();
            int Count = BCM._xmlInfoAssembly._systemXmlInfo.MaxChargerCount;
            if (Count >= 2)//现在这个逻辑暂时不支持双枪
            {
                //spc1.Visible = false;
                SystemEvent.SendTrialResultToUIEvent += SystemEvent_SendTrialResultToUIEvent;
            }
            else
            {
                SystemEvent.SendTrialResultToUIEvent += SystemEvent_SendTrialResultToUIEvent;
            }
            //spc1.SplitterDistance = 230;
            //spc2.SplitterDistance = 367;
        }

        private void SystemEvent_SendChangeLanguageEvent()
        {
            this.label2.Text = LanguageManager.GetByKey(this.Name + "." + "label2");
            lblResult.Text = LanguageManager.GetByKey("待测...");
        }

        private void SystemEvent_SendTrialResultToUIEvent(EmTrialResult emTrialResult)
        {
            if (lblResult.InvokeRequired)
            {
                lblResult.Invoke(new ResultEventHander(SetState), emTrialResult);
            }
            else
            {
                SetState(emTrialResult);
            }
        }

        private delegate void ResultEventHander(EmTrialResult emTrialResult);
        private void SetState(EmTrialResult emTrialResult)
        {
            switch (emTrialResult)
            {
                case EmTrialResult.Wait:
                    lblResult.Text = LanguageManager.GetByKey("待测...");
                    lblResult.ForeColor = Color.Black;
                    picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.未开始;
                    break;
                case EmTrialResult.Testing:
                    lblResult.Text = LanguageManager.GetByKey("检测中");
                    lblResult.ForeColor = Color.Black;
                    picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.测试中;
                    break;
                case EmTrialResult.Fail:
                    lblResult.Text = "FAIL";
                    lblResult.ForeColor = Color.Red;
                    picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.大红叉;
                    break;
                case EmTrialResult.Pass:
                    lblResult.Text = "PASS";
                    lblResult.ForeColor = Color.Green;
                    picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.大绿勾;
                    break;
            }
        }

        private void SystemEvent_SendLogMessageEvent(string logMsg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new LogEventHander(WriteLog), logMsg);
            }
            else
            {
                WriteLog(logMsg);
            }
        }

        private delegate void LogEventHander(string logMsg);

        private void WriteLog(string logMsg)
        {
            txtLog.AppendText(logMsg);
            txtLog.ScrollToCaret();
        }
    }
}
