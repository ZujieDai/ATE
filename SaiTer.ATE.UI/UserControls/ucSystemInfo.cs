using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.UI.Assitand.PrjUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucSystemInfo : UserControl
    {

        private int _TotalCharger;

        /// <summary>
        /// 当日已测桩总数
        /// </summary>
        public int TotalCharger
        {
            get { return _TotalCharger; }
            set
            {
                _TotalCharger = value;
                dgv.Rows[3].Cells[1].Value = value;
                dgv.Rows[5].Cells[1].Value = value - _PassCharger;
                dgv.Rows[6].Cells[1].Value = ((Convert.ToDouble(_PassCharger) / Convert.ToDouble(_TotalCharger)) * 100).ToString("F2");

            }
        }

        private int _PassCharger;

        /// <summary>
        /// 当日合格桩总数
        /// </summary>
        public int PassCharger
        {
            get { return _PassCharger; }
            set
            {
                _PassCharger = value;
                dgv.Rows[4].Cells[1].Value = value;
                dgv.Rows[5].Cells[1].Value = _TotalCharger - value;
                dgv.Rows[6].Cells[1].Value = ((Convert.ToDouble(_PassCharger) / Convert.ToDouble(_TotalCharger)) * 100).ToString("F2");
            }
        }
        private static ucSystemInfo Instance = null;

        public static ucSystemInfo GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new ucSystemInfo();
            }
            return Instance;
        }

        private ucSystemInfo()
        {
            InitializeComponent();
            Init();
        }


        private void ucSystemInfo_Load(object sender, EventArgs e)
        {
            dgv.Rows[3].Cells[1].Value = _TotalCharger;
            dgv.Rows[4].Cells[1].Value = _PassCharger;
            dgv.Rows[5].Cells[1].Value = _TotalCharger - _PassCharger;
            if (_TotalCharger == 0)
            {
                dgv.Rows[6].Cells[1].Value = 100;
            }
            else
            {
                dgv.Rows[6].Cells[1].Value = ((Convert.ToDouble(_PassCharger) / Convert.ToDouble(_TotalCharger)) * 100).ToString("F2");
            }
            SystemEvent.SendTrialStateEvent += SystemEvent_SendTrialStateEvent;
        }

        private void SystemEvent_SendTrialStateEvent(EmTrialState emTrialState)
        {
            try
            {
                switch (emTrialState)
                {
                    case EmTrialState.Start:
                        dgv.Rows[0].Cells[1].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        dgv.Rows[1].Cells[1].Value = "";
                        dgv.Rows[2].Cells[1].Value = "";
                        break;
                    case EmTrialState.End:
                        dgv.Rows[1].Cells[1].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        TimeSpan ts = DateTime.Parse(dgv.Rows[1].Cells[1].Value.ToString()) - DateTime.Parse(dgv.Rows[0].Cells[1].Value.ToString());
                        dgv.Rows[2].Cells[1].Value = ts.ToString();
                        break;
                }
            }
            catch(Exception ex) { Log.Log.LogException(ex); }
        }

        private void Init()
        {
            dgv.Rows.Clear();
            dgv.Rows.Add(7);
            dgv.Rows[0].Cells[0].Value = LanguageManager.GetByKey("当前测试开始时间");
            dgv.Rows[1].Cells[0].Value = LanguageManager.GetByKey("当前测试结束时间");
            dgv.Rows[2].Cells[0].Value = LanguageManager.GetByKey("当前测试耗时");
            dgv.Rows[3].Cells[0].Value = LanguageManager.GetByKey("当日测试桩总数量");
            dgv.Rows[4].Cells[0].Value = LanguageManager.GetByKey("当日合格桩数量");
            dgv.Rows[5].Cells[0].Value = LanguageManager.GetByKey("当日不合格桩数量");
            dgv.Rows[6].Cells[0].Value = LanguageManager.GetByKey("当日合格率(%)");
        }

    }
}
