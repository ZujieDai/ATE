using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using SixLabors.Fonts;
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

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucChargerInfo : UserControl
    {
        //待检充电枪集合
        public List<int> m_LstChargerInfos = null;
        private BusinessManage BCM;
        private CustomToolTip customToolTip;
        private bool isToolTipVisible = false;
        private ucChargerInfo()
        {
            InitializeComponent();
            customToolTip = new CustomToolTip();
        }

        private static ucChargerInfo Instance = null;

        public static ucChargerInfo GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ucChargerInfo();
            }
            return Instance;
        }
        private void ucChargerInfo_Load(object sender, EventArgs e)
        {
            BCM = BusinessManage.GetInstance();
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;

            SetChargerInfo_Invoke();
            GetChargerInfos();
        }
        private void SystemEvent_SendChangeLanguageEvent()
        {
            for (int i = 0; i < this.Controls.Count; i++)
            {
                Controls[i].Text = LanguageManager.GetByKey(this.Name + "." + Controls[i].Name);
            }
            this.chbCharger1.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "1";
            this.chbCharger2.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "2";
            this.chbCharger3.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "3";
            this.chbCharger4.Text = LanguageManager.GetByKey(this.Name + "." + chbCharger1.Name) + "4";
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
            switch (BCM.lstChargerInfo.Count)
            {
                case 0:
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel4.Visible = false;
                    panel3.Visible = false;
                    break;
                case 1:
                    panel1.Visible = true;
                    panel2.Visible = false;
                    panel4.Visible = false;
                    panel3.Visible = false;
                    break;
                case 2:
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel4.Visible = false;
                    panel3.Visible = false;
                    break;
                case 3:
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel4.Visible = true;
                    panel3.Visible = false;
                    break;
                case 4:
                    panel1.Visible = true;
                    panel2.Visible = true;
                    panel4.Visible = true;
                    panel3.Visible = true;
                    break;
            }
        }

        private void chbCharger1_CheckedChanged(object sender, EventArgs e)
        {
            chbCharger2.Checked = chbCharger1.Checked;
            GetChargerInfos();
        }

        private void chbCharger2_CheckedChanged(object sender, EventArgs e)
        {
            GetChargerInfos();
        }

        private void chbCharger3_CheckedChanged(object sender, EventArgs e)
        {
            GetChargerInfos();
        }

        private void chbCharger4_CheckedChanged(object sender, EventArgs e)
        {
            GetChargerInfos();
        }

        /// <summary>
        /// 返回待检定充电枪(选中的)集合
        /// </summary>
        public List<int> GetChargerInfos()
        {

            m_LstChargerInfos = new List<int>();
            m_LstChargerInfos.Clear();
            if (chbCharger1.Visible && this.chbCharger1.Checked)
            {
                // 天津ZD的美标直流导引是欧标导引2
                //string Customer = ConfigurationManager.AppSettings["Customer"];
                //if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD"))
                //{
                //    ChargerInfoModel chargerInfoModel = BCM.lstChargerInfo.FirstOrDefault(s => s.ChargerId == 2);
                //    if (chargerInfoModel != null && chargerInfoModel.ChargerType == EmChargerType.Charger_USA_DC)
                //        m_LstChargerInfos.Add(2);
                //    else
                //        m_LstChargerInfos.Add(1);
                //}
                //else
                    m_LstChargerInfos.Add(1);
            }
            if (chbCharger2.Visible && this.chbCharger2.Checked)
            {
                m_LstChargerInfos.Add(2);
            }
            if (chbCharger3.Visible && this.chbCharger3.Checked)
            {
                m_LstChargerInfos.Add(3);
            }
            if (chbCharger4.Visible && this.chbCharger4.Checked)
            {
                m_LstChargerInfos.Add(4);
            }

            return m_LstChargerInfos;

        }

        private void chbAllTestItems_CheckedChanged(object sender, EventArgs e)
        {
            SystemEvent.SetAllTestItemsCheck(chbAllTestItems.Checked);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            chbCharger1.Checked = !chbCharger1.Checked;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            chbCharger2.Checked = !chbCharger2.Checked;

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            chbCharger3.Checked = !chbCharger3.Checked;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            chbCharger4.Checked = !chbCharger4.Checked;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            //控件名称为默认的 pictureBox1, pictureBox4,别改
            int chargerID = Convert.ToInt32((sender as PictureBox).Name.ToString().Substring(10));

            string tooltipText = GetTableDataAsString(chargerID);

            if (!isToolTipVisible)
            {
                customToolTip.ToolTipText = tooltipText;
                customToolTip.Show(tooltipText, pictureBox1, 30, 30); // 坐标为显示的位置
                isToolTipVisible = true;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            // 延迟检查鼠标是否真的离开了PictureBox,防止循环触发导致闪烁
            Task.Delay(100).ContinueWith(t =>
            {
                if (!pictureBox1.ClientRectangle.Contains(pictureBox1.PointToClient(Control.MousePosition)))
                {
                    Invoke(new Action(() =>
                    {
                        customToolTip.Hide(pictureBox1);
                        isToolTipVisible = false;
                    }));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
    }
    /// <summary>
    /// 扩展ToolTip控件,来自代码小天才YF.Zhang
    /// </summary>
    public class CustomToolTip : ToolTip
    {
        SizeF boundSize = new SizeF(220, 105);

        public string ToolTipText { get; set; }
        public CustomToolTip()
        {
            this.OwnerDraw = true;
            this.Popup += new PopupEventHandler(this.OnCustomPopup);
            this.Draw += new DrawToolTipEventHandler(this.OnCustomDraw);

            
        }

        private void OnCustomPopup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedWindow is Control crl)
            {
                using (Graphics graphics = crl.CreateGraphics())
                {
                    using (System.Drawing.Font font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Regular))
                        boundSize = graphics.MeasureString(ToolTipText, font);
                }
            }
            // 在这里调整 ToolTip 的大小
            e.ToolTipSize = new Size((int)boundSize.Width, (int)boundSize.Height);
        }

        private void OnCustomDraw(object sender, DrawToolTipEventArgs e)
        {
            //便签背景色
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 224)), e.Bounds);

            // 自定义边框颜色
            using (Pen borderPen = new Pen(Color.FromArgb(0, 92, 140), 3)) // 可以更改颜色和线条宽度
            {
                e.Graphics.DrawRectangle(borderPen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }
            // 自定义字体和大小绘制文本
            using (System.Drawing.Font font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Regular))
            {
                e.Graphics.DrawString(e.ToolTipText, font, Brushes.Black, new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }
    }
}
