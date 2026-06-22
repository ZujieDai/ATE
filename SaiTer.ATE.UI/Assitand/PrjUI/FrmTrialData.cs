using NPOI.OpenXmlFormats.Spreadsheet;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.UserControls;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    /// <summary>
    /// 试验数据查询窗体
    /// </summary>
    public partial class FrmTrialData : UIForm
    {
        //是否需要自定义模板 
        public static bool isCustomTemplate { get ; set ;}
        private static FrmTrialData Instance = null;
        private BusinessManage BCM = BusinessManage.GetInstance();
        string strPKID = "";
        string schemeName = "";
        string Customer;
        /// <summary>
        /// 试验数据查询窗体
        /// </summary>
        public static FrmTrialData GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmTrialData();
            }
            Instance.Activate();
            return Instance;
        }
        private FrmTrialData()
        {
            InitializeComponent();

        }
        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                string sWhere = "";
                if (ckbBarCode.Checked)
                {
                    sWhere = " And BarCode = '" + txtBarCode.Text + "'";
                }
                if (ckbRES.Checked)
                {
                    sWhere = " And RES1 like '%" + txtRES.Text + "%'";
                }
                if (ckbTime.Checked)
                {
                    if (sWhere != "")
                    {
                        sWhere += "and SaveTime < '" + dtpEndTime.Value + "'and SaveTime >'" + dtpStartTime.Value + "'";
                    }
                    else
                    {
                        sWhere = "And SaveTime < '" + dtpEndTime.Text + "' and SaveTime >'" + dtpStartTime.Text + "'";
                    }
                }
                System.Data.DataTable dt = TrialItemDataTmpManage.SelectTrialDataWhereSQL(sWhere);
                InitDgvContent(dt);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void InitDgvContent(System.Data.DataTable dtData)
        {
            try
            {
                dgv1.Rows.Clear();
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        dgv1.Rows.Add();
                        dgv1.Rows[i].Cells["cmbSelect"].Value = false;
                        dgv1.Rows[i].Cells["PKID"].Value = dtData.Rows[i]["PKID"].ToString();
                        dgv1.Rows[i].Cells["BarCode"].Value = dtData.Rows[i]["BarCode"].ToString();
                        dgv1.Rows[i].Cells["SchemeName"].Value = dtData.Rows[i]["SchemeName"].ToString();
                        dgv1.Rows[i].Cells["TrialResult"].Value = dtData.Rows[i]["TrialResult"].ToString();
                        dgv1.Rows[i].Cells["SaveTime"].Value = dtData.Rows[i]["SaveTime"].ToString();
                        if (RES1.Visible)
                            dgv1.Rows[i].Cells["RES1"].Value = dtData.Rows[i]["RES1"].ToString();
                    }
                    //DisableSortMode();
                    dgv1.Sort(dgv1.Columns["PKID"], ListSortDirection.Descending);

                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        dgv1.Rows[i].Cells["clmNo"].Value = (i + 1).ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 禁用排序功能
        /// </summary>
        private void DisableSortMode()
        {

            for (int i = 0; i < this.dgv1.Columns.Count; i++)
            {
                this.dgv1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

        }

        private void FrmTrialData_Load(object sender, EventArgs e)
        {
            Customer = ConfigurationManager.AppSettings["Customer"];
            isCustomTemplate = Convert.ToBoolean(ConfigurationManager.AppSettings["isCustomTemplate"]);
            dtpEndTime.Value = DateTime.Now;
            if (isCustomTemplate)
            {
                comboBox_TestReportSelect.Visible = true;
                if (Customer.Equals("KS"))
                {
                    comboBox_TestReportSelect.Items.AddRange(new string[] { "生成系统默认模板", "生成KS模板" });
                }
                comboBox_TestReportSelect.SelectedIndex = 1;
            }
            else
            {
                comboBox_TestReportSelect.Visible = false;
                comboBox_TestReportSelect.Items.AddRange(new string[] { });
                //comboBox_TestReportSelect.SelectedIndex = -1;
            }
            
            if (!string.IsNullOrEmpty(Customer) && Customer.Equals("HYQCP"))
            {
                dataGridViewTextBoxColumn2.HeaderText = "条码/编号";

                BarCode.HeaderText = "条码/样品编号";
                RES1.HeaderText = "委托单编号";
                RES1.Visible = true;
                //panel1.Width = 411;
                ckbBarCode.Text = "按条码/样品编号：";
                ckbBarCode.Width = 155;
                ckbBarCode.Location = new System.Drawing.Point(ckbBarCode.Location.X - 30, ckbBarCode.Location.Y);
                txtBarCode.Location = new System.Drawing.Point(185, txtBarCode.Location.Y);
                btnQuery.Location = new System.Drawing.Point(btnQuery.Location.X + 60, btnQuery.Location.Y);
                btnExport.Location = new System.Drawing.Point(btnExport.Location.X + 60, btnExport.Location.Y);
                ckbRES.Width = 110;
                ckbRES.Location = new System.Drawing.Point(878, ckbRES.Location.Y);
                ckbRES.Text = "委托单编号：";
                txtRES.Location = new System.Drawing.Point(998, ckbRES.Location.Y);
                ckbRES.Visible = true;
                txtRES.Visible = true;
            }
            else
            {
                RES1.Visible = false;
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

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {

                List<string> lstPKID = new List<string>();
                Dictionary<long, List<string>> dicData = new Dictionary<long, List<string>>();
                for (int i = 0; i < dgv1.Rows.Count; i++)
                {
                    if ((bool)dgv1.Rows[i].Cells["cmbSelect"].Value)
                    {
                        long pkid = Convert.ToInt64(dgv1.Rows[i].Cells["PKID"].Value);
                        if (!lstPKID.Contains(pkid.ToString()))
                        {
                            lstPKID.Add(pkid.ToString());
                        }
                        if (dicData.ContainsKey(pkid))
                        {
                            dicData[pkid].Add(dgv1.Rows[i].Cells["SchemeName"].Value.ToString());
                        }
                        else
                        {
                            List<string> lstSchemeName = new List<string>();
                            lstSchemeName.Add(dgv1.Rows[i].Cells["SchemeName"].Value.ToString());
                            dicData.Add(pkid, lstSchemeName);
                        }
                    }
                }
                if (dicData.Count == 0)
                {
                    MessageBox.Show("请选中测试数据");
                    return;
                }

                foreach (var item in dicData)
                {
                    string strSchemeNames = "";
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        strSchemeNames += "'" + item.Value[i] + "',";
                    }
                    strSchemeNames = strSchemeNames.TrimEnd(',');

                    List<TrialDataModel> lstTrialData = TrialItemDataTmpManage.GetTrialDataFromPkidAndSchemeName(item.Key.ToString(), strSchemeNames);
                    ChargerInfoModel chargerInfo = ChargerInfoManage.GetChargerInfoFromFomalTable(item.Key.ToString());

                    string wordPath = "";
                    bool result=false;
                    //选择生成默认模板或是KS模板
                    if (isCustomTemplate)
                    {
                        if (Customer.Equals("KS"))
                        {
                            if (comboBox_TestReportSelect.SelectedIndex == 0)
                                result = ExportReport.CreateFile(lstTrialData, chargerInfo, item.Value, ref wordPath);
                            else if (comboBox_TestReportSelect.SelectedIndex == 1)
                                result = ExportReport.CreateFileToKS(lstTrialData, chargerInfo, item.Value, ref wordPath);
                        }
                    }
                    else
                        result = ExportReport.CreateFile(lstTrialData, chargerInfo, item.Value, ref wordPath);
                    if (result)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = wordPath;
                        startInfo.Arguments = wordPath;
                        try
                        {
                            Process.Start(startInfo);
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                    }
                    else
                    {
                        MessageBox.Show("报告导出失败");
                    }
                }

                //List<TrialDataModel> lstTrialData = TrialItemDataTmpManage.GetTrialDataFromPkidAndSchemeName(strPKIDs, strSchemeNames);


                //string wordPath = "";
                //bool result = ExportReport.CreateFile(lstTrialData, BCM.lstChargerInfo, ref wordPath);
                //if (result)
                //{
                //    ProcessStartInfo startInfo = new ProcessStartInfo();
                //    startInfo.FileName = wordPath;
                //    startInfo.Arguments = wordPath;
                //    try
                //    {
                //        Process.Start(startInfo);
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //}
                //else
                //{
                //    MessageBox.Show("报告导出失败");
                //}
            }
            catch { }
        }

        private void dgv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            try
            {
                strPKID = dgv1.Rows[e.RowIndex].Cells[2].Value.ToString();
                schemeName = dgv1.Rows[e.RowIndex].Cells[4].Value.ToString();
                List<TrialDataModel> lstData = TrialItemResultTmpManage.GetTrialResultFromPKID(strPKID, null, schemeName);

                if (lstData == null)
                {
                    return;
                }
                dgvTrialItems.Rows.Clear();
                dgvTrialData.Rows.Clear();
                for (int i = 0; i < lstData.Count; i++)
                {
                    dgvTrialItems.Rows.Add(1);
                    dgvTrialItems.Rows[i].Cells[0].Value = (i + 1).ToString();//序号
                    dgvTrialItems.Rows[i].Cells[1].Value = lstData[i].TrialName;
                    dgvTrialItems.Rows[i].Cells[1].Tag = (int)lstData[i].TrialType;

                    if (lstData[i].TrialFinalResult == EmTrialResult.Pass || lstData[i].TrialFinalResult == EmTrialResult.NA)
                    {
                        dgvTrialItems.Rows[i].Cells[2].Value = global::SaiTer.ATE.UI.Properties.Resources.pass;
                    }
                    else if (lstData[i].TrialFinalResult == EmTrialResult.Fail)
                    {
                        dgvTrialItems.Rows[i].Cells[2].Value = global::SaiTer.ATE.UI.Properties.Resources.fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }

        private void dgvTrialItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            try
            {
                int TrialType = (int)dgvTrialItems.Rows[e.RowIndex].Cells[1].Tag;
                List<TrialDataModel> lstData = TrialItemResultTmpManage.GetTrialDataFromPKIDAndTrialType(strPKID, TrialType);
                if (lstData == null)
                {
                    return;
                }
                dgvTrialData.Rows.Clear();
                for (int i = 0; i < lstData.Count; i++)
                {
                    string[] datas = lstData[i].Data2.Split("|");
                    dgvTrialData.Rows.Add(1);

                    for (int j = 0; j < 6; j++)
                    {                        
                        if (j == 5)
                        {
                            dgvTrialData.Rows[i].Cells[j].Value = lstData[i].TrialResult.ToString();
                        }
                        else
                        {
                            dgvTrialData.Rows[i].Cells[j].Value = datas[j];
                        }
                    }
                    if (datas.Length == 6)
                    {
                        dgvTrialData.Rows[i].Cells[5].Tag = datas[5];

                        if (!datas[5].Contains("Image"))
                        {
                            DataGridViewButtonCell cell = (DataGridViewButtonCell)dgvTrialData[6, i];
                            cell.UseColumnTextForButtonValue = false;
                        }
                    }

                    else
                    {
                        DataGridViewButtonCell cell = (DataGridViewButtonCell)dgvTrialData[6, i];
                        cell.UseColumnTextForButtonValue = false;
                    }

                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void dgvTrialData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 6)
            {
                try
                {
                    if (!dgvTrialData.Rows[e.RowIndex].Cells[5].Tag.ToString().Contains("Image"))
                    {
                        return;
                    }
                    string path = "";

                    path = System.Environment.CurrentDirectory + "\\" + dgvTrialData.Rows[e.RowIndex].Cells[5].Tag.ToString();
                    if (path.Contains(".jpg") || path.Contains(".bmp"))
                    {
                        FrmOscilloscopeImage frm = FrmOscilloscopeImage.GetInstance();

                        frm.StartPosition = FormStartPosition.CenterScreen;
                        frm.Text = "示波器截图";
                        frm.pictureBox1.Image = Image.FromFile(path);
                        frm.Show();
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                }
            }
        }
    }
}
