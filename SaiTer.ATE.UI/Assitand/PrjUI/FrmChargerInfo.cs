using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.Manage;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.UI.UserControls;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Business;
using System.Management.Instrumentation;
using static Sunny.UI.IdentityCard;
using System.Runtime.CompilerServices;
using SaiTer.ATE.MES;
using System.Threading;
using static System.Net.WebRequestMethods;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmChargerInfo : UIForm
    {
        private string _SchemeName = "";
        private bool isCommiting = false;
        string IsCWRate;
        bool isAutoTest = false;


        public string SchemeName
        {
            get { return _SchemeName; }
            set
            {
                _SchemeName = value;
                cmbScheme.Text = value;

            }
        }
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
        private FrmChargerInfo()
        {
            InitializeComponent();
            MaxChargerCount = BCM._xmlInfoAssembly._systemXmlInfo.MaxChargerCount;
            ChargerType = BCM._xmlInfoAssembly._systemXmlInfo.ChargerType;
            IsCWRate = ConfigurationManager.AppSettings["IsCWRate"];
        }
        private static FrmChargerInfo Instance = null;
        /// <summary>
        /// 充电枪信息录入窗体单例
        /// </summary>
        public static FrmChargerInfo GetInstance()
        {
            try
            {
                if (Instance == null || Instance.IsDisposed)
                {
                    Instance = new FrmChargerInfo();
                }

                Instance.Show();
                Instance.Activate();
                Instance.txtBarCode1.Focus();
                Instance.txtBarCode1.SelectionStart = 0;  //设置起始位置 
                Instance.txtBarCode1.SelectionLength = Instance.txtBarCode1.Text.Length;  //设置长度
                Instance.txtBarCode1.ScrollToCaret();
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


        private void FrmChargerInfo_Load(object sender, EventArgs e)
        {
            string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
            if (strAutoTest != null)
            {
                isAutoTest = bool.Parse(strAutoTest);
            }

            cmbType.Items.Clear();

            foreach (var item in ChargerType)
            {
                cmbType.Items.Add(item);
            }
            try
            {
                switch (MaxChargerCount)
                {
                    case 1:
                        panel1.Visible = true;
                        break;
                    case 2:
                        panel1.Visible = true;
                        panel2.Visible = true;
                        break;
                    case 3:
                        panel1.Visible = true;
                        panel2.Visible = true;
                        panel3.Visible = true;
                        break;
                    case 4:
                        panel1.Visible = true;
                        panel2.Visible = true;
                        panel3.Visible = true;
                        panel4.Visible = true;
                        break;
                }


                lstChargerInfo = Clone<ChargerInfoModel>(BCM.lstChargerInfo);
                List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
                if (SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo))
                {
                    cmbScheme.Items.Clear();
                    foreach (var item in lstSchemeInfo)
                    {
                        cmbScheme.Items.Add(item.SchemeName);
                    }
                }
                for (int i = 0; i < lstChargerInfo.Count; i++)
                {
                    switch (lstChargerInfo[i].ChargerId)
                    {
                        case 1:
                            chbChargerOne.Checked = true;
                            txtBarCode1.Text = lstChargerInfo[i].BarCode;
                            break;
                        case 2:
                            chbChargerTwo.Checked = true;
                            txtBarCode2.Text = lstChargerInfo[i].BarCode;
                            break;
                        case 3:
                            chbChargerThree.Checked = true;
                            txtBarCode3.Text = lstChargerInfo[i].BarCode;
                            break;
                        case 4:
                            chbChargerFour.Checked = true;
                            txtBarCode4.Text = lstChargerInfo[i].BarCode;
                            break;
                    }
                }
                
                if (lstChargerInfo.Count > 0)
                {
                    _SchemeName = lstChargerInfo[0].SchemeName;
                    cmbScheme.Text = lstChargerInfo[0].SchemeName;
                    cmbType.Text = EnumHelper.GetEnumDescription<EmChargerType>(lstChargerInfo[0].ChargerType);
                    if (cmbType.Text.Contains("直流"))
                    {
                        pnlDC.Visible = true;
                        txtMaxPower.Text = lstChargerInfo[0].MaxOutputPower.ToString();
                        txtMaxCurr.Text= lstChargerInfo[0].MaxAllowChargeCurrent.ToString();
                        txtMinVolt.Text = lstChargerInfo[0].MinAllowChargeVoltage.ToString();

                        //恒功率段
                        if (IsCWRate != null && Convert.ToBoolean(IsCWRate))
                        {
                            bool isVisible = true;
                            labHightVoltRange.Visible = isVisible;
                            txtHightVoltL.Visible = isVisible;
                            txtHightVoltH.Visible = isVisible;
                            uiLabel22.Visible = isVisible;
                            labLowVoltRange.Visible = isVisible;
                            txtLowerVlotL.Visible = isVisible;
                            txtLowerVoltH.Visible = isVisible;
                            uiLabel25.Visible = isVisible;
                            //额定电流
                            uiLabel11.Visible = !isVisible;
                            txtRateCurrent.Visible = !isVisible;
                            //频率
                            //lblFrequency.Visible = !isVisible;
                            //txtFreq.Visible = !isVisible;
                            lblFrequency.Location = new Point(7, 255);
                            txtFreq.Location = new Point(178, 255);
                        }
                    }
                    else
                    {
                        pnlDC.Visible = false;
                    }
                    txtName.Text = lstChargerInfo[0].ProductName;
                    txtModel.Text = lstChargerInfo[0].ProductModel;
                    txtOperater.Text = lstChargerInfo[0].Operater;
                    txtAuditor.Text = lstChargerInfo[0].Auditor;
                    cmbScheme.Text = lstChargerInfo[0].SchemeName;

                    txtMaxVoltage.Text = lstChargerInfo[0].NominalVoltage.ToString();
                    txtRateCurrent.Text = lstChargerInfo[0].NominalCurrent.ToString();
                    txtFreq.Text = lstChargerInfo[0].Frequency.ToString();
                }
                
                if (cmbScheme.Text.Contains("安规"))
                {
                    pnlParams.Visible = false;
                }
                else
                {
                    pnlParams.Visible = true;
                }

                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("XJ"))
                {
                    lblRES.Text = "合同编号：";
                    lblRES.Visible = true;
                    txtRES.Visible = true;
                }
                else if (!string.IsNullOrEmpty(Customer) && Customer.Equals("HYQCP"))
                {
                    panel1.Width = 411;
                    chbChargerOne.Text = "1号枪条码/样品编号：";
                    chbChargerOne.Width = 195;
                    txtBarCode1.Location = new Point(195, txtBarCode1.Location.Y);
                    lbl1.Location = new Point(382, lbl1.Location.Y);
                    lblRES.Text = "委托单编号：";
                    lblRES.Visible = true;
                    txtRES.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
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
                if (string.IsNullOrEmpty(txtRateCurrent.Text))
                {
                    result = false;
                }
                if (string.IsNullOrEmpty(txtMaxVoltage.Text))
                {
                    result = false;
                }
                if (string.IsNullOrEmpty(txtFreq.Text))
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
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

        private void CheckSN(TBHttpMES tBHttp, int ChargerNo, Action actCommint)
        {
            bool isOK = false;
            Thread th = new Thread(() =>
            {
                try
                {
                    if (isCommiting)
                    {
                        ShowWarningTip("提交中，请稍等！");
                        return;
                    }
                    isCommiting = true;
                    isOK = tBHttp.CheckSN(txtBarCode1.Text.Trim(), out string result);
                    this.Invoke(new Action(() =>
                    {
                        if (!isOK)
                        {
                            ShowWarningTip($"{ChargerNo}号枪SN检查：{result}", 3000);
                            txtBarCode1.Focus();
                            txtBarCode1.SelectionStart = 0;  //设置起始位置 
                            txtBarCode1.SelectionLength = txtBarCode1.Text.Length;  //设置长度
                            txtBarCode1.ScrollToCaret();
                            return;
                        }
                        actCommint.Invoke();
                    }));
                }
                catch(Exception ex) { Log.Log.LogException(ex); }
                finally
                {
                    isCommiting = false;
                }
            });
            th.Start();
        }

        public void SetChargerInfo(string barcode, int schemeIndex)
        {
            List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
            if (cmbScheme.Items == null || cmbScheme.Items.Count == 0)
            {
                if (SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo))
                {
                    cmbScheme.Items.Clear();
                    foreach (var item in lstSchemeInfo)
                    {
                        cmbScheme.Items.Add(item.SchemeName);
                    }
                }
            }
            txtBarCode1.Text = barcode;
            cmbScheme.SelectedIndex = schemeIndex;
            if (isAutoTest)
            {
                if (lstSchemeInfo == null || lstSchemeInfo.Count < 1)
                    SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo);
                var schemeInfo = lstSchemeInfo.FirstOrDefault(s => s.SchemeName.Equals(cmbScheme.Text));
                txtMaxVoltage.Text = schemeInfo.RES1;
                txtRateCurrent.Text = schemeInfo.RES2;
                cmbType.Text = schemeInfo.RES3;
                //ChargerInfoManage.UpdateChargerInfo(BCM.lstChargerInfo, schemeInfo.RES1, schemeInfo.RES2, schemeInfo.RES3);
            }
            btnCommit_Click_Async(btnCommit, null);
        }

        private void btnCommit_Click_Async(object sender, EventArgs e)
        {
            try
            {
                //自动化产线属性
                if (isAutoTest)
                {
                    btnCommit_Click(sender, e);
                }
                else
                {
                    if (MESUtils.IsMES_TB())
                    {
                        Action actCommint = new Action(() => btnCommit_Click(sender, e));
                        TBHttpMES tBHttp = TBHttpMES.GetInstance();
                        if (chbChargerOne.Visible && chbChargerOne.Checked)
                        {
                            CheckSN(tBHttp, 1, actCommint);
                        }
                        if (chbChargerTwo.Visible && chbChargerTwo.Checked)
                        {
                            CheckSN(tBHttp, 2, actCommint);
                        }
                    }
                    else
                        btnCommit_Click(sender, e);
                }
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
                if (chbChargerOne.Visible && chbChargerOne.Checked)
                {
                    barCode += " '" + txtBarCode1.Text.Trim() + "' ,";
                }
                if (chbChargerTwo.Visible && chbChargerTwo.Checked)
                {
                    barCode += " '" + txtBarCode2.Text.Trim() + "' ,";
                }
                if (chbChargerThree.Visible && chbChargerThree.Checked)
                {
                    barCode += " '" + txtBarCode1.Text.Trim() + "' ,";
                }
                if (chbChargerFour.Visible && chbChargerFour.Checked)
                {
                    barCode += " '" + txtBarCode4.Text.Trim() + "' ,";
                }
                barCode = barCode.TrimEnd(',');

                long pkid = 0;
                if (ChargerInfoManage.SelectIsHasChargerInfo(barCode, ref pkid) && !isAutoTest)
                {
                    if (MessageBox.Show("该条码已经保存过数据，是否对此桩再次检测？", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    {
                        return;
                    }
                }


                lstChargerInfo.Clear();
                int index = 1;
                for (int i = 1; i <= 4; i++)
                {
                    ChargerInfoModel model = new ChargerInfoModel();
                    //string strPKID = pkid == 0 ? DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') +
                    //                    DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') +
                    //                    index.ToString().PadLeft(3, '0') : pkid.ToString();
                    string strPKID = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') +
                                        DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0') +
                                        index.ToString().PadLeft(3, '0');
                    index++;
                    if (i == 1 && chbChargerOne.Checked)
                    {
                        if (txtBarCode1.Text.Trim() == "")
                        {
                            ShowWarningTip("条码/编号不能为空！");
                            return;
                        }
                        // 天津ZD的美标直流导引是欧标导引2
                        //string Customer = ConfigurationManager.AppSettings["Customer"];
                        //if (!string.IsNullOrEmpty(Customer) && Customer.Equals("ZD") && cmbType.Text.Contains("美标直流"))
                        //    model.ChargerId = 2;
                        //else
                            model.ChargerId = 1;
                        model.BarCode = txtBarCode1.Text.Trim();
                    }

                    else if (i == 2 && chbChargerTwo.Checked)
                    {
                        if (txtBarCode2.Text.Trim() == "")
                        {
                            ShowWarningTip("条码/编号不能为空！");
                            return;
                        }
                        model.ChargerId = 2;
                        model.BarCode = txtBarCode2.Text.Trim();
                    }

                    else if (i == 3 && chbChargerThree.Checked)
                    {
                        if (txtBarCode3.Text.Trim() == "")
                        {
                            ShowWarningTip("条码/编号不能为空！");
                            return;
                        }
                        model.ChargerId = 3;
                        model.BarCode = txtBarCode3.Text.Trim();
                    }

                    else if (i == 4 && chbChargerFour.Checked)
                    {
                        if (txtBarCode4.Text.Trim() == "")
                        {
                            ShowWarningTip("条码/编号不能为空！");
                            return;
                        }
                        model.ChargerId = 4;
                        model.BarCode = txtBarCode4.Text.Trim();
                    }
                    else
                    {
                        continue;
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
                    model.NominalVoltage = Convert.ToDouble(txtMaxVoltage.Text);
                    model.NominalCurrent = Convert.ToDouble(txtRateCurrent.Text);
                    model.Frequency = Convert.ToDouble(txtFreq.Text);
                    model.MaxOutputPower = Convert.ToDouble(txtMaxPower.Text);
                    model.MaxAllowChargeCurrent = Convert.ToDouble(txtMaxCurr.Text);
                    model.MinAllowChargeVoltage = Convert.ToDouble(txtMinVolt.Text);
                    model.RES1 = txtRES.Text;

                    if(IsCWRate != null && Convert.ToBoolean(IsCWRate))
                    {
                        model.NominalCurrent = model.MaxOutputPower * 1000 / model.NominalVoltage;
                        model.CWHightVoltL = Convert.ToDouble(txtHightVoltL.Text);
                        model.CWHightVoltH = Convert.ToDouble(txtHightVoltH.Text);
                        model.CWLowerVoltL = Convert.ToDouble(txtLowerVlotL.Text);
                        model.CWLowerVoltH = Convert.ToDouble(txtLowerVoltH.Text);
                    }

                    lstChargerInfo.Add(model);
                }


                bool isOK = ChargerInfoManage.InsertChargerInfo(lstChargerInfo);
                if (isOK)
                {
                    BCM.LoadChargerInfo();
                    BCM._xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.UpdateChargerInfo(lstChargerInfo);
                    //ucChargerInfo chargerInfo = ucChargerInfo.GetInstance();
                    //chargerInfo.SetChargerInfo_Invoke(); 
                    //chargerInfo.GetChargerInfos();
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
        /// 更新枪信息后，同时更新过压、欠压、过流测试项的参数👍
        /// </summary>
        private void UpdateTrialParams(string schemeName)
        {
            try
            {
                string strParams = "";
                foreach (var item in BCM.lstTrialItemsInfo)
                {
                    if (item.TrialType == EmTrialType.输入过压保护测试)
                    {
                        //过压值(V)=275.00|BMS电压需求值(V)=220.00
                        string Voltage = lstChargerInfo[0].NominalVoltage.ToString("F2");
                        string overVoltage = "0";
                        if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            overVoltage = (lstChargerInfo[0].NominalVoltage * 1.27272).ToString("F2");
                        }
                        else
                        {
                            overVoltage = "280";
                        }
                        strParams = string.Format("过压值(V)={0}|BMS电压需求值(V)={1}", overVoltage, Voltage);

                        TrialItemsManage.UpdateTrialScheme(schemeName, item.ItemName, item.TrialType, strParams, "test");
                        continue;
                    }
                    if (item.TrialType == EmTrialType.输入欠压保护测试)
                    {
                        //欠压值(V)=100.00|BMS电压需求值(V)=220.00
                        string Voltage = lstChargerInfo[0].NominalVoltage.ToString("F2");
                        string overVoltage = "0";
                        if (lstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                            lstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            overVoltage = (lstChargerInfo[0].NominalVoltage / 1.27272).ToString("F2");
                        }
                        else
                        {
                            overVoltage = "180";
                        }

                        strParams = string.Format("过压值(V)={0}|BMS电压需求值(V)={1}", overVoltage, Voltage);
                        TrialItemsManage.UpdateTrialScheme(schemeName, item.ItemName, item.TrialType, strParams, "test");
                        continue;
                    }
                    if (item.TrialType == EmTrialType.输出过流测试)
                    {
                        //输出过流值(A)=20.00|正常电流值(A)=16.00
                        string Current = lstChargerInfo[0].NominalCurrent.ToString("F2");
                        string overCurrent = (lstChargerInfo[0].NominalCurrent * 1.25).ToString("F2");
                        strParams = string.Format("输出过流值(A)={0}|正常电流值(A)={1}", overCurrent, Current);
                        TrialItemsManage.UpdateTrialScheme(schemeName, item.ItemName, item.TrialType, strParams, "test");
                        continue;
                    }
                }

                BCM.GetTrialScheme(schemeName);
                FrmMain frm = FrmMain.GetInstance(schemeName);
                frm.lstTestItems = BCM.lstTrialItemsInfo;
                FrmTrialParams frmTrialParam = FrmTrialParams.GetInstance("", "", new List<string>(), "");
                //SystemEvent.SendDataGridViewItems(frmTrialParam.LstItemsClone, true);
                SystemEvent.SendDataGridViewItems(BCM.lstTrialItemsInfo, false);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //this.Close();
            this.Hide();
        }

        private void FrmChargerInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }

        private void chbChargerOne_CheckedChanged(object sender, EventArgs e)
        {
            lbl1.Visible = chbChargerOne.Checked;
        }

        private void chbChargerTwo_CheckedChanged(object sender, EventArgs e)
        {
            lbl2.Visible = chbChargerTwo.Checked;
        }

        private void txtMaxVoltage_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMaxVoltage.Text))
                {
                    lblVolt.Text = "0 V";
                }
                else
                {
                    lblVolt.Text = (Convert.ToDouble(txtMaxVoltage.Text) * 1.732).ToString() + " V";
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //string[] schemeName = new string[cmbScheme.Items.Count];

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
                if (cmbType.Text.Contains("直流"))
                {
                    label1.Visible = false;
                    lblVolt.Visible = false;
                    txtRateCurrent.Maximum = 2000;
                    txtRateCurrent.MaxLength = 4;
                    txtMaxVoltage.Maximum = 1500;
                    txtMaxVoltage.Text = "1000";
                    pnlDC.Visible = true;

                    //恒功率段
                    if (IsCWRate != null && Convert.ToBoolean(IsCWRate))
                    {
                        bool isVisible = true;
                        labHightVoltRange.Visible = isVisible;
                        txtHightVoltL.Visible = isVisible;
                        txtHightVoltH.Visible = isVisible;
                        uiLabel22.Visible = isVisible;
                        labLowVoltRange.Visible = isVisible;
                        txtLowerVlotL.Visible = isVisible;
                        txtLowerVoltH.Visible = isVisible;
                        uiLabel25.Visible = isVisible;
                        //额定电流
                        uiLabel11.Visible = !isVisible;
                        txtRateCurrent.Visible = !isVisible;
                        //频率
                        //lblFrequency.Visible = !isVisible;
                        //txtFreq.Visible = !isVisible;
                        lblFrequency.Location = new Point(7, 255);
                        txtFreq.Location = new Point(178, 255);
                    }
                }
                else
                {
                    label1.Visible = true;
                    lblVolt.Visible = true;
                    txtRateCurrent.Maximum = 64;
                    txtRateCurrent.MaxLength = 2;
                    txtMaxVoltage.Maximum = 240;
                    pnlDC.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void cmbScheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbScheme.Text.Contains("安规"))
            {
                pnlParams.Visible = false;
            }
            else
            {
                pnlParams.Visible = true;
            }
        }
    }
}
