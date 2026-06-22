using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.Properties;
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
    public partial class ucWaitTestChagers : UserControl
    {
        //待检充电枪集合
        public List<int> m_LstChargerInfos = null;
        private BusinessManage BCM;
        private CustomToolTip customToolTip;
        private bool isToolTipVisible = false;
        private List<ucWaitTestChager> lstWaitTestChager;

        private ucWaitTestChagers()
        {
            InitializeComponent();
            customToolTip = new CustomToolTip();
        }

        private static ucWaitTestChagers Instance = null;

        public static ucWaitTestChagers GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ucWaitTestChagers();
            }
            return Instance;
        }

        private void ucChargerInfo_Load(object sender, EventArgs e)
        {
            BCM = BusinessManage.GetInstance();
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;

            SetChargerInfo_Invoke();
            //GetChargerInfos();
        }
        private void SystemEvent_SendChangeLanguageEvent()
        {
            //for (int i = 0; i < this.Controls.Count; i++)
            //{
            //    Controls[i].Text = LanguageManager.GetByKey(this.Name + "." + Controls[i].Name);
            //}
            //this.chbCharger1.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "1";
            //this.chbCharger2.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "2";
            //this.chbCharger3.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "3";
            //this.chbCharger4.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "4";
        }
        public void SetChargerInfo_Invoke()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetChargerInfoHandler(SetChargerInfo));
            }
            else
            {
                SetChargerInfo();
            }
        }
        private delegate void SetChargerInfoHandler();
        public void SetChargerInfo()
        {
            lstWaitTestChager = new List<ucWaitTestChager>();
            flpChargers.Controls.Clear();
            for (int i = 0; i < BCM.lstChargerInfo.Count; i++)
            {
                ucWaitTestChager ucWaitTestChager = new ucWaitTestChager();
                ucWaitTestChager.ChargerName = LanguageManager.GetByKey("枪") + BCM.lstChargerInfo[i].ChargerId;
                ucWaitTestChager.pictureBox1.Tag = BCM.lstChargerInfo[i].ChargerId;
                ucWaitTestChager.pictureBox1.MouseEnter += (sender, e) =>
                {
                    int chargerID = (int)((PictureBox)sender).Tag;

                    string tooltipText = GetTableDataAsString(chargerID);

                    if (!isToolTipVisible)
                    {
                        customToolTip.ToolTipText = tooltipText;
                        customToolTip.Show(tooltipText, (PictureBox)sender, 30, 30); // 坐标为显示的位置
                        isToolTipVisible = true;
                    }
                };
                ucWaitTestChager.pictureBox1.MouseLeave += (object sender, EventArgs e) =>
                {
                    // 延迟检查鼠标是否真的离开了PictureBox,防止循环触发导致闪烁
                    Task.Delay(100).ContinueWith(t =>
                    {
                        if (!((PictureBox)sender).ClientRectangle.Contains(((PictureBox)sender).PointToClient(Control.MousePosition)))
                        {
                            Invoke(new Action(() =>
                            {
                                customToolTip.Hide((PictureBox)sender);
                                isToolTipVisible = false;
                            }));
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                };
                ucWaitTestChager.chbCharger.CheckedChanged += (object sender, EventArgs e) =>
                {
                    // bool iS2charger =FrmGroupChargerInfo.IS2charger;
                    //  FrmGroupChargerInfo.is2charger
                    //string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
                    //if (strIsGroupC != null)
                    //{
                    //    bool isGroupCharger = Convert.ToBoolean(strIsGroupC);
                    //    if (isGroupCharger)
                    //    {

                    //    }
                    //}

                    //int ChargerNum = Convert.ToInt32(((Control)sender).Text.Substring(1));
                    //if (BCM.lstChargerInfo[ChargerNum - 1].RES2 == "1")
                    //{
                    //    for(int r=0;r< lstWaitTestChager.Count/2;r++)
                    //    {
                    //        lstWaitTestChager[2*r+1].ChargerChecked = lstWaitTestChager[2*r].ChargerChecked;
                    //        //lstWaitTestChager[3].ChargerChecked = lstWaitTestChager[2].ChargerChecked;
                    //        //lstWaitTestChager[5].ChargerChecked = lstWaitTestChager[4].ChargerChecked;
                    //        //lstWaitTestChager[7].ChargerChecked = lstWaitTestChager[6].ChargerChecked;
                    //    }
                    //}

                    GetChargerInfos();
                };
                flpChargers.Controls.Add(ucWaitTestChager);
                lstWaitTestChager.Add(ucWaitTestChager);
            }
            GetChargerInfos();
        }

        private string GetTableDataAsString(int chargerID)
        {
            try
            {
                ChargerInfoModel chargerInfoModel = BCM.lstChargerInfo.First(s => s.ChargerId == chargerID);
                string[,] tableData = new string[,] { { " 枪   条  码      ", "" }, { " 类        型      ", "" }, { " 额定电压      ", "" }, { " 额定电流      ", "" }, { " 方案名称      ", "" } };
                tableData[0, 1] = chargerInfoModel.BarCode;
                tableData[1, 1] = EnumHelper.GetEnumDescription<EmChargerType>(chargerInfoModel.ChargerType);
                tableData[2, 1] = chargerInfoModel.NominalVoltage + " V";
                tableData[3, 1] = chargerInfoModel.NominalCurrent + " A";
                tableData[4, 1] = chargerInfoModel.SchemeName;
                int rows = tableData.GetLength(0);
                int cols = tableData.GetLength(1);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        sb.Append(tableData[i, j]);
                        //if (j < cols - 1) sb.Append("\t"); // 列分隔
                    }
                    if (i < rows - 1) sb.AppendLine(); // 行分隔
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 返回待检定充电枪(选中的)集合
        /// </summary>
        public List<int> GetChargerInfos()
        {
            m_LstChargerInfos = new List<int>();
            m_LstChargerInfos.Clear();
            foreach(var item in lstWaitTestChager)
            {
                if (item.ChargerChecked)
                {
                    int.TryParse(item.ChargerName.Replace(LanguageManager.GetByKey("枪"), ""), out int ChargerNum);
                    m_LstChargerInfos.Add(ChargerNum);
                }
            }

            return m_LstChargerInfos;
        }

        private void chbAllTestItems_CheckedChanged(object sender, EventArgs e)
        {
            SystemEvent.SetAllTestItemsCheck(chbAllTestItems.Checked);
        }

    }
}
