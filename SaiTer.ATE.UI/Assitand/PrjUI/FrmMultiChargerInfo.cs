using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.UserControls;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaiTer.ATE.MES;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmMultiChargerInfo : UIForm
    {
        public string SchemeName { get; set; }

        List<UcChagerInfo> ucChagerInfos = new List<UcChagerInfo>();
        private BusinessManage BCM = BusinessManage.GetInstance();
        /// <summary>
        /// 系统支持同时测试的最大枪位数量
        /// </summary>
        private int MaxChargerCount = 0;

        /// <summary>
        /// 设备支持的桩类型
        /// </summary>
        private string[] ChargerType = null;

        private List<ChargerInfoModel> lstChargerInfo = new List<ChargerInfoModel>();

        public FrmMultiChargerInfo()
        {
            InitializeComponent();

            MaxChargerCount = BCM._xmlInfoAssembly._systemXmlInfo.MaxChargerCount;
            ChargerType = BCM._xmlInfoAssembly._systemXmlInfo.ChargerType;
        }

        #region Window Function
        private static FrmMultiChargerInfo Instance = null;
        /// <summary>
        /// 充电枪信息录入窗体单例
        /// </summary>
        public static FrmMultiChargerInfo GetInstance()
        {
            try
            {
                if (Instance == null || Instance.IsDisposed)
                {
                    Instance = new FrmMultiChargerInfo();

                    #region 初始化
                    List<UIPage> pages = new List<UIPage>();
                    Instance.ucChagerInfos = new List<UcChagerInfo>();
                    for (int i = 0; i < Instance.MaxChargerCount; i++)
                    {
                        UIPage page = new UIPage();
                        page.Text = "枪" + (i + 1);
                        UcChagerInfo chagerInfo = new UcChagerInfo();
                        chagerInfo.Dock = DockStyle.Fill;
                        chagerInfo.Name = "cgi" + i;
                        chagerInfo.SetChargerType(Instance.cmbType.Text);
                        Instance.ucChagerInfos.Add(chagerInfo);
                        page.Controls.Add(chagerInfo);
                        pages.Add(page);
                    }
                    Instance.tbcChargers.AddPages(pages.ToArray());
                    #endregion
                }

                Instance.Show();
                Instance.Activate();
                return Instance;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                Instance.Dispose();
                Instance.Close();
                return null;
            }
        }

        private void FrmMultiChargerInfo_Load(object sender, EventArgs e)
        {
            //充电枪类型
            cmbType.Items.Clear();
            foreach (var item in ChargerType)
            {
                cmbType.Items.Add(item);
            }
            if(cmbType.Items.Count > 0)
            {
                cmbType.SelectedIndex = 0;
            }
            //测试方案
            List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
            if (SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo))
            {
                cmbScheme.Items.Clear();
                foreach (var item in lstSchemeInfo)
                {
                    cmbScheme.Items.Add(item.SchemeName);
                }
            }
            //枪信息
            lstChargerInfo = Clone<ChargerInfoModel>(BCM.lstChargerInfo);
            if (lstChargerInfo.Count > 0)
            {
                //_SchemeName = lstChargerInfo[0].SchemeName;
                cmbScheme.Text = lstChargerInfo[0].SchemeName;
                cmbType.Text = EnumHelper.GetEnumDescription<EmChargerType>(lstChargerInfo[0].ChargerType);
                txtName.Text = lstChargerInfo[0].ProductName;
                txtModel.Text = lstChargerInfo[0].ProductModel;
                txtOperater.Text = lstChargerInfo[0].Operater;
                txtAuditor.Text = lstChargerInfo[0].Auditor;
                cmbScheme.Text = lstChargerInfo[0].SchemeName;
            }
            for(int i = 0; i < MaxChargerCount; i++)
            {
                if (lstChargerInfo.Count == 0)
                    break;
                int index = lstChargerInfo.FindIndex(c => c.ChargerId == i + 1);
                bool isChecked = index >= 0;
                ucChagerInfos[i].BindingChargerInfo(cmbType.Text, isChecked, isChecked ? lstChargerInfo[index] : null);
                //群充双枪
                if (index >= 0 && lstChargerInfo[index].RES2 == "1")
                {
                    ucChagerInfos[i + 1].BindingChargerInfo(cmbType.Text, isChecked, isChecked ? lstChargerInfo[index] : null);
                    i++;
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

        private void FrmChargerInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }

        private Point mouseOffset;
        private bool isMouseDown = false;
        private void lbl_Title_MouseDown(object sender, MouseEventArgs e)
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

        private void lbl_Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = System.Windows.Forms.Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }

        private void lbl_Title_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }
        #endregion

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //this.Close();
            this.Hide();
        }

        private void btnCommit_Click_Async(object sender, EventArgs e)
        {
            try
            {
                //if (MESUtils.IsMES_TB())
                //{
                //    Action actCommint = new Action(() => btnCommit_Click(sender, e));
                //    TBHttpMES tBHttp = TBHttpMES.GetInstance();
                //    if (chbChargerOne.Visible && chbChargerOne.Checked)
                //    {
                //        CheckSN(tBHttp, 1, actCommint);
                //    }
                //    if (chbChargerTwo.Visible && chbChargerTwo.Checked)
                //    {
                //        CheckSN(tBHttp, 2, actCommint);
                //    }
                //}
                //else
                    btnCommit_Click(sender, e);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            try
            {
                //if (MessageBox.Show("提交枪信息可能会丢失未保存的已检数据。请确认检测数据已保存！", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                //{
                //    return;
                //}
                if (!CheckParams())
                {
                    ShowWarningTip("带*号参数不能为空！");
                    return;
                }

                string barCode = "";
                for (int i = 0; i < MaxChargerCount; i++)
                {
                    if (ucChagerInfos[i].isChecked)
                    {
                        if (chbTwoCharger.Checked)
                        {
                            if (i % 2 == 0)
                                barCode += " '" + ucChagerInfos[i].Barcode.Trim() + "A' ,";
                            else
                                barCode += " '" + ucChagerInfos[i - 1].Barcode.Trim() + "B' ,";
                        }
                        else
                            barCode += " '" + ucChagerInfos[i].Barcode.Trim() + "' ,";
                    }
                }
                barCode = barCode.TrimEnd(',');

                long pkid = 0;
                if (ChargerInfoManage.SelectIsHasChargerInfo(barCode, ref pkid))
                {
                    if (MessageBox.Show("该条码已经保存过数据，是否对此桩再次检测？", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    {
                        return;
                    }
                }


                lstChargerInfo.Clear();
                int index = 1;
                for (int i = 0; i < MaxChargerCount; i++)
                {
                    if (!ucChagerInfos[i].isChecked)
                        continue;
                    ChargerInfoModel model = new ChargerInfoModel();
                    //string strPKID = pkid == 0 ? DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') +
                    //                    DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') +
                    //                    index.ToString().PadLeft(3, '0') : pkid.ToString();
                    string strPKID = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') +
                                        DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') +
                                        index.ToString().PadLeft(3, '0');
                    index++;
                    if (ucChagerInfos[i].isChecked)
                    {
                        if (string.IsNullOrEmpty(ucChagerInfos[i].Barcode))
                        {
                            if (!chbTwoCharger.Checked || (chbTwoCharger.Checked && i % 2 == 0))
                            {
                                ShowWarningTip("条码/编号不能为空！");
                                return;
                            }
                        }
                        // 天津ZD的美标直流导引是欧标导引2
                        //string Customer = ConfigurationManager.AppSettings["Customer"];
                        //if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && cmbType.Text.Contains("美标直流"))
                        //    model.ChargerId = 2;
                        //else
                        model.ChargerId = i + 1;
                        if (chbTwoCharger.Checked)
                        {
                            if (i % 2 == 0)
                                model.BarCode = ucChagerInfos[i].Barcode + "A";
                            else
                                model.BarCode = ucChagerInfos[i - 1].Barcode + "B";
                        }
                        else
                            model.BarCode = ucChagerInfos[i].Barcode;
                    }
                    model.PKID = Convert.ToInt64(strPKID);
                    model.ProductName = txtName.Text;
                    model.ProductModel = txtModel.Text;
                    string chargerType = cmbType.Text;
                    if (cmbScheme.Text.Contains("安规"))
                    {
                        if (cmbType.Text.Contains("交流"))
                        {
                            chargerType = "国标交流充电枪";
                        }
                        else if (cmbType.Text.Contains("直流"))
                        {
                            chargerType = "国标直流充电枪";
                        }
                    }
                    model.ChargerType = EnumHelper.GetEnumByDescription<EmChargerType>(cmbType.Text);
                    model.Operater = txtOperater.Text;
                    model.Auditor = txtAuditor.Text;
                    model.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    model.SchemeName = cmbScheme.Text;
                    model.NominalVoltage = Convert.ToDouble(ucChagerInfos[i].NormalVolt);
                    model.NominalCurrent = Convert.ToDouble(ucChagerInfos[i].RateCurrent);
                    model.Frequency = Convert.ToDouble(ucChagerInfos[i].Freq);
                    model.MaxOutputPower = Convert.ToDouble(ucChagerInfos[i].MaxPower);
                    model.MaxAllowChargeCurrent = Convert.ToDouble(ucChagerInfos[i].MaxCurrent);
                    model.MinAllowChargeVoltage = Convert.ToDouble(ucChagerInfos[i].MinVolt);
                    model.RES1 = txtRES.Text;
                    model.RES2 = chbTwoCharger.Checked ? "1" : "0";

                    lstChargerInfo.Add(model);
                }


                bool isOK = ChargerInfoManage.InsertChargerInfo(lstChargerInfo);
                if (isOK)
                {
                    BCM.LoadChargerInfo();
                    BCM._xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.UpdateChargerInfo(lstChargerInfo);
                    ucWaitTestChagers ucWaitTestChagers = ucWaitTestChagers.GetInstance();
                    ucWaitTestChagers.SetChargerInfo_Invoke();
                    ucWaitTestChagers.GetChargerInfos();
                    ucCheckData checkData = ucCheckData.GetInstance();
                    checkData.AutoLoadCharger();
                    SystemEvent.SwitchCheckItemIndex(-999);//清空表格内的检测结论
                    //UpdateTrialParams(cmbScheme.Text);
                    UIMessageTip.ShowOk("保存成功");
                    BCM.GetTrialScheme(cmbScheme.Text);
                    SystemEvent.SendDataGridViewItems(BCM.lstTrialItemsInfo, false, cmbScheme.Text);
                    // SystemEvent.SendTrialResultToUI(new DataModel.DataBaseModel.TrialDataModel(), -999, true, -1);
                    SystemEvent.SendTrialResult(EmTrialResult.Wait);
                    this.Hide();

                }
                else
                {
                    ShowWarningTip("保存失败");
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                ShowWarningTip("保存失败");
            }
        }

        /// <summary>
        /// 验证输入的参数
        /// </summary>
        /// <returns></returns>
        private bool CheckParams()
        {
            bool result = true;

            try
            {
                if (string.IsNullOrEmpty(cmbType.Text))
                {
                    result = false;
                }
                if (string.IsNullOrEmpty(cmbScheme.Text))
                {
                    result = false;
                }
                for (int i = 0; i < MaxChargerCount; i++)
                {
                    if(!ucChagerInfos[i].CheckParams(cmbType.Text))
                    {
                        result = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbScheme.Text.Contains("安规"))
            {
                //for (int i = 0; i < cmbScheme.Items.Count; i++)
                //{
                //    schemeName[i] = cmbScheme.Items[i].ToString();
                //}
                if (cmbType.Text.Contains("直流"))
                {
                    for (int i = 0; i < cmbScheme.Items.Count; i++)
                    {
                        if (cmbScheme.Items[i].ToString().Contains("直流"))
                        {
                            cmbScheme.Text = cmbScheme.Items[i].ToString();
                            break;
                        }
                    }
                }
                else if (cmbType.Text.Contains("交流"))
                {
                    for (int i = 0; i < cmbScheme.Items.Count; i++)
                    {
                        if (cmbScheme.Items[i].ToString().Contains("交流"))
                        {
                            cmbScheme.Text = cmbScheme.Items[i].ToString();
                            break;
                        }
                    }
                }
            }

            foreach(var item in ucChagerInfos)
            {
                item.SetChargerType(cmbType.Text);
            }
        }

        /// <summary>
        /// 克隆一个集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The list.</param>
        /// <returns>List{``0}.</returns>
        public static List<T> Clone<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }

        private void chbTwoCharger_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 1; i < MaxChargerCount; i+=2)
            {
                ucChagerInfos[i].pnlBarcodeVisible = !chbTwoCharger.Checked;
            }
        }
    }
}
