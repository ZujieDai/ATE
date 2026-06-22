using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.UI.Assitand.PrjUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.DataModel;
using System.Windows.Markup.Localizer;
using Sunny.UI;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using System.IO.Ports;
using System.Configuration;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucTestItems : UserControl
    {
        public EmTrialType trialType = EmTrialType.Null;
        //静态实例
        private static ucTestItems Instance = null;

        private static int ChargerCount = 1;

        public static string UserType = "操作员";

        private bool isGroupCharger = false;
        public List<StTrialItem> lstItems = new List<StTrialItem>();
        public List<StTrialItem> lstItemsClone = new List<StTrialItem>();
        private List<TrialDataModel> LstTrialData = new List<TrialDataModel>();
        public bool IsInitialize = false;
        #region --------------数据事件委托---------
        /// <summary>
        /// 加载方案委托
        /// </summary>
        public delegate void OnLoadedEventHandler();

        /// <summary>
        /// 数据视图刷新委托
        /// </summary>
        /// <param name="rowIndex">当前行号</param>
        /// <param name="funcNo">功能代号</param>
        public delegate void DataViewRefreshedEventHandler(int rowIndex, EmTrialType funcNo, string itemName);

        /// <summary>
        /// 数据视图刷新委托
        /// </summary>
        /// <param name="Column0">当前行号</param>
        /// <param name="Column1">功能代号</param>
        public delegate void RunRefreshedEventHandler(string Column0, string Column1);

        /// <summary>
        /// 数据视图刷新委托
        /// </summary>
        /// <param name="rowIndex">当前行号</param>
        /// <param name="funcNo">功能代号</param>
        /// <param name="itemId">检定点ID</param>
        public delegate void DataDetailRefreshedEventHandler(int rowIndex, EmTrialType funcNo, int itemId);

        /// <summary>
        /// 数据视图刷新事件
        /// </summary>
        public event DataViewRefreshedEventHandler DataViewRefreshed;

        /// <summary>
        /// 数据视图刷新事件
        /// </summary>
        public event RunRefreshedEventHandler RunRefreshed;
        #endregion
        public ucTestItems()
        {
            InitializeComponent();
            SystemEvent.SendTestResultToUIEvent += SystemEvent_SendTestResultToUIEvent;
            SystemEvent.SetAllTestItemsCheckEvent += SystemEvent_SetAllTestItemsCheckEvent;
            SystemEvent.SwitchCheckItemIndexEvent += SystemEvent_SwitchCheckItemIndexEvent;
            SystemEvent.EventSendTrialDataToUI += SystemEvent_EventSendTrialDataToUI;
            //dgvTestItem.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            //dgvTestItem.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvTestItem.Columns["clmResult1"].DefaultCellStyle.NullValue = null;
            dgvTestItem.Columns["clmResult2"].DefaultCellStyle.NullValue = null;
            dgvTestItem.Columns["clmResult3"].DefaultCellStyle.NullValue = null;
            dgvTestItem.Columns["clmResult4"].DefaultCellStyle.NullValue = null;

            dgvTestItem.Columns["clmResult1"].Tag = true;
            dgvTestItem.Columns["clmResult2"].Tag = true;
            dgvTestItem.Columns["clmResult3"].Tag = true;
            dgvTestItem.Columns["clmResult4"].Tag = true;
            //群充不需要分别判断
            string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
            if (strIsGroupC != null)
            {
                isGroupCharger = Convert.ToBoolean(strIsGroupC);
            }
            if (!isGroupCharger)
            {
                switch (ChargerCount)
                {
                    case 2:
                        dgvTestItem.Columns["clmResult2"].Visible = true;
                        break;
                    case 3:
                        dgvTestItem.Columns["clmResult2"].Visible = true;
                        dgvTestItem.Columns["clmResult3"].Visible = true;
                        break;
                    case 4:
                        dgvTestItem.Columns["clmResult2"].Visible = true;
                        dgvTestItem.Columns["clmResult3"].Visible = true;
                        dgvTestItem.Columns["clmResult4"].Visible = true;
                        break;
                }
            }
        }
        //程序启动时加载已有的数据，显示合格状态
        private void SystemEvent_EventSendTrialDataToUI(List<TrialDataModel> lstTrialData)
        {
            LstTrialData = lstTrialData;
        }

        public delegate void SetSelectedIndexHanlder(int index);
        private void SystemEvent_SwitchCheckItemIndexEvent(int index)
        {
            if (dgvTestItem.InvokeRequired)
            {
                dgvTestItem.Invoke(new SetSelectedIndexHanlder(SetSelectedIndex), index);
            }
            else
            {
                SetSelectedIndex(index);
            }

        }
        private void SetSelectedIndex(int index)
        {
            if (index == -999)///清空检测结论
            {
                for (int i = 0; i < dgvTestItem.Rows.Count; i++)
                {
                    dgvTestItem.Rows[i].Cells["clmResult1"].Value = null;
                    dgvTestItem.Rows[i].Cells["clmResult2"].Value = null;
                    dgvTestItem.Rows[i].Cells["clmResult3"].Value = null;
                    dgvTestItem.Rows[i].Cells["clmResult4"].Value = null;
                }
            }
            else
            {
                this.dgvTestItem.ClearSelection();
                this.dgvTestItem.CurrentCell = null;
                this.dgvTestItem.Rows[index].Cells[2].Selected = true;
                dgvTestItem.SelectedIndex = index;
            }
        }
        private void SystemEvent_SetAllTestItemsCheckEvent(bool isCheckAll)
        {
            for (int i = 0; i < dgvTestItem.Rows.Count; i++)
            {
                dgvTestItem.Rows[i].Cells["clmCheck"].Value = isCheckAll;
            }
            UpdateLstCheckItems();
        }

        private void SystemEvent_SendTestResultToUIEvent(TrialDataModel TrialData = null, int chargerID = 1, bool isClear = false, int TrialIndex = -1)
        {

            string clmName = "clmResult";
            if (isClear)
            {
                if (chargerID == -999)//特殊数字，这里不触发
                {
                    return;
                }
                for (int j = 0; j < dgvTestItem.RowCount; j++)
                {
                    string cellName = isGroupCharger ? clmName + 1 : clmName + chargerID;
                    bool condition = false;
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.Contains("KLQ"))
                    {
                        condition = true;//新乡KLQ客户要求全部清除     
                    }
                    else
                    {
                        condition = j == TrialIndex; //通用方案是勾选上的才清除数据
                    }


                    if (condition)
                    {
                        dgvTestItem.Rows[j].Cells[cellName].Value = null;
                        dgvTestItem.Rows[j].Cells[cellName].Tag = true;
                        ucCheckData checkDataForm = ucCheckData.GetInstance();

                        checkDataForm.DataTable_Clear(j, new List<int> { chargerID });
                    }
                }
                return;
            }
            for (int j = 0; j < dgvTestItem.RowCount; j++)
            {
                //循环判断试验结果数据显示在表格哪一行
                if (TrialData.TrialType == (EmTrialType)dgvTestItem.Rows[j].Cells["clmItemName"].Tag
                    && TrialData.TrialName == dgvTestItem.Rows[j].Cells["clmItemName"].Value.ToString())
                {
                    string cellName = isGroupCharger ? clmName + 1 : clmName + TrialData.ChargerId;
                    if (TrialData.TrialResult == EmTrialResult.Pass || TrialData.TrialResult == EmTrialResult.NA)
                    {
                        //如果已经是FAIL状态，再次刷新的时候保持不变(此情况出现的状态： 一个测试项有多个测量点，其中某个点上发了FAIL，后面的点是PASS，此时一直保持FAIL状态)
                        if (dgvTestItem.Rows[j].Cells[cellName].Tag == null || (bool)dgvTestItem.Rows[j].Cells[cellName].Tag)
                        {
                            dgvTestItem.Rows[j].Cells[cellName].Value = global::SaiTer.ATE.UI.Properties.Resources.pass;
                            dgvTestItem.Rows[j].Cells[cellName].Tag = true;
                        }
                    }
                    else if (TrialData.TrialResult == EmTrialResult.Fail)
                    {
                        dgvTestItem.Rows[j].Cells[cellName].Value = global::SaiTer.ATE.UI.Properties.Resources.fail;
                        dgvTestItem.Rows[j].Cells[cellName].Tag = false;
                    }
                    else
                    {
                        dgvTestItem.Rows[j].Cells[cellName].Value = null;
                        dgvTestItem.Rows[j].Cells[cellName].Tag = true;
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static ucTestItems GetInstance(int chargerCount, string userType)
        {
            ChargerCount = chargerCount;
            UserType = userType;
            if (Instance == null)
            {
                Instance = new ucTestItems();
            }
            return Instance;
        }


        /// <summary>
        /// 加载方案
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="m_TreeNode"></param>
        public void LoadCheckItem(List<StTrialItem> LstTrialItems)
        {
            if (LstTrialItems != null)
            {
                if (this.dgvTestItem.InvokeRequired)
                {
                    this.dgvTestItem.Invoke(new SetDatagridViewEventHander(SetDataGridViewData), LstTrialItems);
                }
                else
                {
                    SetDataGridViewData(LstTrialItems);
                }
                lstItems = LstTrialItems;
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

        private delegate void SetDatagridViewEventHander(List<StTrialItem> LstTrialItems);

        private void SetDataGridViewData(List<StTrialItem> LstTrialItems)
        {
            try
            {
                lstItemsClone = Clone<StTrialItem>(lstItems);
                dgvTestItem.Rows.Clear();
                for (int i = 0; i < LstTrialItems.Count; i++)
                {
                    dgvTestItem.Rows.Add(1);
                    dgvTestItem.Rows[i].Cells["clmNum"].Value = (i + 1).ToString();
                    dgvTestItem.Rows[i].Cells["clmCheck"].Value = true;
                    dgvTestItem.Rows[i].Cells["clmItemName"].Value = LstTrialItems[i].ItemName;
                    dgvTestItem.Rows[i].Cells["clmItemName"].Tag = LstTrialItems[i].TrialType;
                }
                if (!IsInitialize)
                {
                    string clmName = "clmResult";
                    bool? isPass = true;
                    foreach (TrialDataModel trial in LstTrialData)
                    {
                        clmName = "clmResult" + (isGroupCharger ? "1" : trial.ChargerId.ToString());
                        for (int i = 0; i < dgvTestItem.Rows.Count; i++)
                        {
                            if (trial.TrialType == (EmTrialType)dgvTestItem.Rows[i].Cells["clmItemName"].Tag)
                            {
                                //超过4的列是没有的，现在都是总结果没有分枪
                                //if (trial.ChargerId > 4)
                                //    continue;
                                if (trial.TrialResult == EmTrialResult.Pass || trial.TrialResult == EmTrialResult.NA)
                                {
                                    //如果已经是FAIL状态，再次刷新的时候保持不变(此情况出现的状态： 一个测试项有多个测量点，其中某个点上发了FAIL，后面的点是PASS，此时一直保持FAIL状态)
                                    if (dgvTestItem.Rows[i].Cells[clmName].Tag == null || (bool)dgvTestItem.Rows[i].Cells[clmName].Tag)
                                    {
                                        dgvTestItem.Rows[i].Cells[clmName].Value = global::SaiTer.ATE.UI.Properties.Resources.pass;
                                        dgvTestItem.Rows[i].Cells[clmName].Tag = true;
                                    }

                                }
                                else if (trial.TrialResult == EmTrialResult.Fail)
                                {
                                    dgvTestItem.Rows[i].Cells[clmName].Value = global::SaiTer.ATE.UI.Properties.Resources.fail;
                                    dgvTestItem.Rows[i].Cells[clmName].Tag = false;
                                    isPass = false;
                                }
                                else
                                {
                                    dgvTestItem.Rows[i].Cells[clmName].Value = null;
                                    isPass = null;
                                }
                            }
                        }
                        IsInitialize = true;
                    }
                    if (LstTrialData == null || LstTrialData.Count == 0)
                        SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Wait);
                    else
                    {
                        switch (isPass)
                        {
                            case true:
                                SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Pass);
                                break;
                            case false:
                                SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Fail);
                                break;
                            case null:
                                SystemEvent.SendTrialResult(DataModel.EnumModel.EmTrialResult.Wait);
                                break;
                        }
                    }
                }
                DataViewRefreshed(0, trialType, this.dgvTestItem.Rows[0].Cells["clmItemName"].Value.ToString());
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void dgvTestItem_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                this.dgvTestItem.Rows[e.RowIndex].Cells["clmCheck"].Value = !(bool)this.dgvTestItem.Rows[e.RowIndex].Cells["clmCheck"].Value;
                UpdateLstCheckItems();
            }
            //else if (e.RowIndex >= 0 && e.ColumnIndex == 2)
            //{
            //    trialType = (EmTrialType)this.dgvTestItem.Rows[e.RowIndex].Cells["clmItemName"].Tag;
            //    DataViewRefreshed(e.RowIndex, trialType,this.dgvTestItem.Rows[e.RowIndex].Cells["clmItemName"].Value.ToString());
            //}


        }

        private void UpdateLstCheckItems()
        {
            lstItemsClone = Clone<StTrialItem>(lstItems);
            for (int i = 0; i < dgvTestItem.Rows.Count; i++)
            {
                bool isCheck = (bool)this.dgvTestItem.Rows[i].Cells["clmCheck"].Value;
                //勾选取消后，在待测试验集合中移除相应的项
                if (!isCheck)
                {
                    trialType = (EmTrialType)this.dgvTestItem.Rows[i].Cells["clmItemName"].Tag;
                    int index = lstItemsClone.FindIndex(s => s.TrialType == trialType && s.ItemName == this.dgvTestItem.Rows[i].Cells["clmItemName"].Value.ToString());
                    if (index >= 0)
                    {
                        lstItemsClone.RemoveAt(index);
                    }
                }
            }
            SystemEvent.SendDataGridViewItems(lstItemsClone, true);
        }

        private void dgvTestItem_SelectIndexChange(object sender, int index)
        {
            if (index >= 0)
            {
                trialType = (EmTrialType)this.dgvTestItem.Rows[index].Cells["clmItemName"].Tag;
                DataViewRefreshed(index, trialType, this.dgvTestItem.Rows[index].Cells["clmItemName"].Value.ToString());
            }
        }

        private void dgvTestItem_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //UIDataGridView dgv = (UIDataGridView)sender;
            //if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            //{
            //    e.PaintBackground(e.CellBounds, true);
            //    // ControlPaint.DrawCheckBox(e.Graphics, e.CellBounds.X + 1, e.CellBounds.Y + 1, e.CellBounds.Width - 5, e.CellBounds.Height - 5, (bool)e.FormattedValue ? ButtonState.Checked : ButtonState.Normal);
            //    ControlPaint.DrawCheckBox(e.Graphics, e.CellBounds.X + 1, e.CellBounds.Y + 1, e.CellBounds.Width - 5, e.CellBounds.Height - 5, ButtonState.Checked);

            //    e.Handled = true;
            //}
        }

        private void dgvTestItem_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 2)
            {
                try
                {
                    List<string> lstParams = new List<string>();
                    trialType = (EmTrialType)this.dgvTestItem.Rows[e.RowIndex].Cells["clmItemName"].Tag;
                    string method = lstItems[e.RowIndex].TrialMethod.ToString();
                    string standard = lstItems[e.RowIndex].DecideStandard.ToString();
                    foreach (string param in lstItems[e.RowIndex].ResultParams.Split('|'))
                    {
                        lstParams.Add(param);
                    }
                    FrmTrialParams frm = FrmTrialParams.GetInstance(method, standard, lstParams, UserType);
                    frm.SchemeName = lstItems[0].SchemeName;
                    frm.TrialType = trialType;
                    frm.LstItemsClone = lstItemsClone;
                    frm.TrialName = dgvTestItem.Rows[e.RowIndex].Cells["clmItemName"].Value.ToString();
                    frm.Show();
                }
                catch (Exception ex) { Log.Log.LogException(ex); }
            }
        }


    }
}
