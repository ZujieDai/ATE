
using SaiTer.ATE.UI.Assitand.NodeView;
using SaiTer.ATE.UI.Assitand.PrjUI.ExtentClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.Assitand;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.UI;

namespace SaiTer.ATE.UI.PrjUI
{
    public partial class DataGridViewBase : UserControl
    {
        #region ---------------变量定义区----------------

        //当前选中的检定项序号
        public int nSelectedIndex = -1;
        //当前新增待检枪号
        private int m_ChargerId = 0;
        //检定结果数据临时仓库
        private ListNodes m_LstStorage = null;

        #endregion

        #region ---------------窗体属性---------------

        /// <summary>
        /// 当前选中的检定项序号
        /// </summary>
        public int SelectedItemIndex
        {
            get { return nSelectedIndex; }
            set { nSelectedIndex = value; }
        }
        #endregion

        #region ----------------构造函数-----------------
        public DataGridViewBase()
        {
            InitializeComponent();
            this.Load += new EventHandler(DataGridViewBase_Load);
            this.dgvDataView.RowsAdded += new DataGridViewRowsAddedEventHandler(dgvDataView_RowsAdded);
            this.dgvDataView.Scroll += new ScrollEventHandler(dgvDataView_Scroll);
        }

        #endregion

        #region ----------------窗体加载-----------------
        /// <summary>
        /// 加载数据视图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewBase_Load(object sender, EventArgs e)
        {
            string str = "枪号";
            dgvDataView.TopLeftHeaderCellText = LanguageManager.GetByKey(str);
            this.Dock = DockStyle.Fill;
            SystemEvent.SendChangeLanguageEvent += SystemEvent_SendChangeLanguageEvent;

        }
        private void SystemEvent_SendChangeLanguageEvent()
        {
            for (int i = 0; i < this.Controls.Count; i++)
            {
                Controls[i].Text = LanguageManager.GetByKey(this.Name + "." + Controls[i].Name);
            }
        }
        #endregion

        #region ----------------窗体事件-----------------


        /// <summary>
        /// 增加新行数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDataView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // dgvDataView.Rows[e.RowIndex].DefaultHeaderCellType = typeof(DataGridViewCheckBoxRowHeaderCell);
            //dgvDataView.Rows[e.RowIndex].HeaderCell.Value = string.Format("{0:D2}号枪", m_ChargerId);
            dgvDataView.Rows[e.RowIndex].Height = 40;
        }

        /// <summary>
        /// 增加单元格数据
        /// </summary>
        /// <param name="nItemId">当前需要增加的检定项序号</param>
        /// <param name="nColIndex">列号索引(从零开始的下标)</param>
        /// <param name="nRowIndex">行号索引(从零开始的下标)</param>
        /// <param name="strCellValue">单元格值</param>
        public virtual void dgvDataView_AddCellValue(int nItemId, int nColIndex, int nRowIndex, string strCellValue)
        {
            if (nColIndex < 0 || nColIndex >= dgvDataView.ColumnCount)
                return;
            if (nRowIndex < 0 || nRowIndex >= dgvDataView.RowCount)
                return;
            //此处要做判断，判断nSelectedIndex是否存在于数据字典中
            if (nItemId == nSelectedIndex)
            {
                dgvDataView[nColIndex, nRowIndex].Value = strCellValue;
                Dictionary_Add(nItemId, nColIndex, nRowIndex, strCellValue, false);
            }//if
        }
        public delegate void RefreshCellValueHandler(int id);
        /// <summary>
        /// 刷新单元格式数据
        /// </summary>
        /// <param name="nItemId"></param>
        public void dgvDataView_RefreshCellValue(int nItemId)
        {
            if (this.dgvDataView.InvokeRequired)
            {
                this.dgvDataView.Invoke(new RefreshCellValueHandler(Refresh), nItemId);
            }
            else
            {
                Refresh(nItemId);
            }
        }

        private void Refresh(int nItemId)
        {
            //多行数据
            dgvDataView.Rows.Clear();
            for (int i = 0; i < m_LstStorage[nItemId].Count; i++)
            {
                dgvDataView.Rows.Add();
                dgvDataView.Rows[i].HeaderCell.Value = m_LstStorage[nItemId][i][0].Column;

                for (int j = 0; j < dgvDataView.Rows[i].Cells.Count - 1; j++)
                {
                    string str = "报表(勿删)";
                    string TranslateStr = LanguageManager.GetByKey(str);
                    if (m_LstStorage[nItemId][i][j + 1].Column.ToString() == TranslateStr)
                    {
                        DataGridViewButtonCell cell = (DataGridViewButtonCell)dgvDataView[17, i];
                        cell.UseColumnTextForButtonValue = false;
                    }
                    dgvDataView.Rows[i].Cells[j].Value = m_LstStorage[nItemId][i][j + 1].Column;
                    if (dgvDataView.Rows[i].Cells[j].Value.ToString().ToUpper().Contains("PASS"))
                    {
                        dgvDataView.Rows[i].DefaultCellStyle.ForeColor = Color.Black;
                        dgvDataView.Rows[i].HeaderCell.Tag = true;

                    }
                    else if (dgvDataView.Rows[i].Cells[j].Value.ToString().ToUpper().Contains("FAIL"))
                    {
                        dgvDataView.Rows[i].DefaultCellStyle.ForeColor = Color.Red;
                        dgvDataView.Rows[i].HeaderCell.Tag = true;
                    }
                    else
                    {
                        dgvDataView.Rows[i].HeaderCell.Tag = null;
                    }
                }
            }
        }

        /// <summary>
        /// 增加一整行数据
        /// </summary>
        /// <param name="nItemId">当前需要增加的检定项序号</param>
        /// <param name="nRowIndex">行号索引(从零开始的下标)</param>
        /// <param name="arrRowValues">行值集合，各检定项的值排列见配置文档或检测业务类说明</param>
        /// <param name="color">字体颜色</param>
        /// <param name="isAddNewRow">true 数据插入新的一行    false  数据更新到已有的行</param>
        /// <param name="chargerID">这一行数据对应的枪编号</param>
        public void dgvDataView_AddRow(int nItemId, int nRowIndex, ArrayList arrRowValues, Color color, bool isAddNewRow, int chargerID)
        {
            if (nItemId == -999)///特殊数字， 清空所有数据
            {
                m_LstStorage.Clear();
                return;
            }

            #region --------------------一个检测项显示多行数据新代码-------------------
            int rowIndex = 0;


            for (int i = 0; i < m_LstStorage[nItemId].Count; i++)
            {
                if (m_LstStorage[nItemId][i].ChargerID == chargerID)
                {
                    rowIndex = i;
                }
            }
            if (m_LstStorage[nItemId][rowIndex][1].Column.ToString() != "")//数据仓库中该项目该表位第一条数据不为空
            {
                LstStorageSort(nItemId, chargerID);//插入一条数据
                rowIndex++;
            }
            // m_LstStorage[nItemId][nRowIndex][0].Column = chargerID.ToString() + "号枪";
            if (isAddNewRow)
            {

                if (dgvDataView.InvokeRequired)
                {
                    dgvDataView.Invoke(new InsertNewRow(InvokeInsert), nRowIndex, nItemId);
                }
                else
                {
                    InvokeInsert(nRowIndex, nItemId);
                }
                // m_LstStorage[nItemId].AddRow(chargerID);

            }
            nRowIndex = rowIndex;
            // bool isHad = false;
            for (int i = 0; i < arrRowValues.Count; i++)
            {
                if (nItemId == nSelectedIndex)
                {
                    string str = "报表(勿删)";
                    string TranslateStr = LanguageManager.GetByKey(str);
                    if (arrRowValues[i].ToString() == (TranslateStr))
                    {
                        DataGridViewButtonCell cell = (DataGridViewButtonCell)dgvDataView[17, nRowIndex];
                        cell.UseColumnTextForButtonValue = false;
                    }
                    //else
                    //{
                    //    if (isHad)
                    //    {
                    //        dgvDataView[i - 1, nRowIndex].Value = arrRowValues[i].ToString();
                    //    }
                    //    else
                    //    {
                    //        dgvDataView[i, nRowIndex].Value = arrRowValues[i].ToString();
                    //    }
                    //}
                    dgvDataView[i, nRowIndex].Value = arrRowValues[i].ToString();
                }
                Dictionary_Add(nItemId, i, nRowIndex, arrRowValues[i], isAddNewRow);
            }
            if (nItemId == nSelectedIndex)
            {
                dgvDataView.Rows[nRowIndex].DefaultCellStyle.ForeColor = color;
            }
            //dgvDataView.Rows[nRowIndex].HeaderCell.Tag = true;

            #endregion
        }

        delegate void InsertNewRow(int nRowIndex, int nItemID);
        private void InvokeInsert(int nRowIndex, int nItemID)
        {
            if (nSelectedIndex == nItemID)//当前选中的检测项=正在测试的项，才刷新ui数据，否则只更新数据仓库
            {
                dgvDataView.Rows.Insert(nRowIndex, 1);
                //dgvDataView.Rows[nRowIndex].HeaderCell.Value = dgvDataView.Rows[nRowIndex - 1].HeaderCell.Value;
                dgvDataView.Rows[nRowIndex].HeaderCell.Value = m_LstStorage[nItemID][nRowIndex][0].Column.ToString();
            }
        }
        /// <summary>
        /// 数据仓库插入一条数据并重新按照表位排序
        /// </summary>
        private void LstStorageSort(int index, int chargerID)
        {
            try
            {
                int rowIndex = 0;
                for (int i = 0; i < m_LstStorage[index].Count; i++)
                {
                    if (m_LstStorage[index][i][0].Column.ToString().Contains(chargerID.ToString()))
                    {
                        if (i < m_LstStorage[index].Count - 1)
                        {
                            //相邻两条数据的枪编号不一样，获取索引
                            if (!m_LstStorage[index][i + 1][0].Column.ToString().Contains(chargerID.ToString()))
                            {
                                rowIndex = i + 1;
                                break;
                            }
                        }
                    }
                }
                if (rowIndex == 0)
                {
                    rowIndex = m_LstStorage[index].Count;
                }
                ListNodes temp = new ListNodes();
                //流程：先取出要插入的索引之前的所有数据，复制到另一个临时对象中，再在临时对象中插入需要的数据，再把后面的数据复制到这个对象中
                //最后数据对象赋值给数据仓库
                int count;
                for (int i = 0; i < m_LstStorage.Count; i++)
                {
                    temp.Add(i);
                    if (i != index)
                    {
                        count = m_LstStorage[i].Count;
                    }
                    else
                    {
                        count = rowIndex;
                    }
                    //不是需要插入数据的检测项，就全部复制

                    for (int j = 0; j < count; j++)
                    {
                        //string str = m_LstStorage[i][j][0].Column.ToString();//行头文本  如：1号枪
                        //int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", ""));//行头字符串提取出枪号

                        temp[i].AddRow(m_LstStorage[i][j].ChargerID);
                        for (int k = 0; k < m_LstStorage[i][j].Count; k++)
                        {
                            temp[i][j][k].Column = m_LstStorage[i][j][k].Column;
                        }
                    }
                    //是当前在检，需要插入的项，则分三次复制
                    if (i == index)//只有这一个检测项需要插入一条数据
                    {
                        temp[i].AddRow(chargerID);//新插入的空数据

                        for (int j = rowIndex; j < m_LstStorage[i].Count; j++)
                        {
                            string str = m_LstStorage[i][j][0].Column.ToString();
                            int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", ""));
                            temp[i].AddRow(id);
                            for (int k = 0; k < m_LstStorage[i][j].Count; k++)
                            {
                                if (i == index)
                                {
                                    temp[i][j + 1][k].Column = m_LstStorage[i][j][k].Column;
                                }
                                else
                                {
                                    temp[i][j][k].Column = m_LstStorage[i][j][k].Column;
                                }
                            }
                        }
                    }
                }
                m_LstStorage = temp;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// 增加一整行数据
        /// </summary>
        /// <param name="nItemId">当前需要增加的检定项序号</param>
        /// <param name="nRowIndex">行号索引(从零开始的下标)</param>
        /// <param name="arrRowValues">行值集合，各检定项的值排列见配置文档或检测类说明</param>
        public void dgvDataView_AddRow_2(int nItemId, int nRowIndex, ArrayList arrRowValues)
        {
            for (int i = 0; i < arrRowValues.Count; i++)
            {
                if (nItemId == nSelectedIndex)
                {
                    dgvDataView[i + 3, nRowIndex].Value = arrRowValues[i].ToString();
                }
                Dictionary_Add(nItemId, i + 3, nRowIndex, arrRowValues[i], false);
            }
        }

        #endregion

        #region -------------- ExtendedDataGridView --------------



        /// <summary>
        /// 拖动滚动条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDataView_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)//垂直
            {
                dgvDataView.FirstDisplayedScrollingRowIndex = e.NewValue;
            }
        }

        /// <summary>
        /// 刷新检定数据
        /// </summary>
        /// <param name="rowIndex">检定方案行索引</param>
        /// <param name="funcNo">检定功能代码</param>
        public virtual void dgvDataView_RefreshData(int rowIndex, EmTrialType funcNo)
        {
            if (nSelectedIndex != rowIndex)
                nSelectedIndex = rowIndex;
            //刷新数据视图
            dgvDataView_RefreshCellValue(rowIndex);
        }

        /// <summary>
        /// 重新设置dataGridView列表头的显示列
        /// </summary>
        /// <param name="strHeaderTexts">表头文本</param>
        public void dgvDataView_ResetColumnHeader(params string[] strHeaderTexts)
        {
            for (int colIndex = strHeaderTexts.GetLowerBound(0); colIndex <= strHeaderTexts.GetUpperBound(0); colIndex++)
            {

                string HeaderText = Convert.ToString(strHeaderTexts.GetValue(colIndex));
                string str = "截图";
                string TranslateStr = LanguageManager.GetByKey(str);
                if (HeaderText.Contains(TranslateStr))
                {
                    dgvDataView.Columns[17].Visible = true;
                    dgvDataView.Columns[17].FillWeight = 10;
                    dgvDataView.Columns[17].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    str = HeaderText;
                    TranslateStr = LanguageManager.GetByKey(str);
                    dgvDataView.Columns[17].HeaderText = TranslateStr;
                    if (dgvDataView.Columns[17] is DataGridViewButtonColumn col)
                        col.Text = TranslateStr;
                }
                else
                {
                    dgvDataView.Columns[colIndex].Visible = true;
                    dgvDataView.Columns[colIndex].HeaderText = HeaderText;
                    dgvDataView.Columns[colIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

            }
        }

        /// <summary>
        /// 重新设置dataGridView列表头的SizeMode
        /// </summary>
        /// <param name="nFillWeights">宽度填充比例</param>
        public void dgvDataView_ResetColumnWidth(params int[] nFillWeights)
        {
            for (int ii = 0; ii < dgvDataView.ColumnCount; ii++)
            {
                dgvDataView.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvDataView.Columns[ii].HeaderText = String.Empty;
                dgvDataView.Columns[ii].Visible = false;
                dgvDataView.Columns[ii].Width = 5;
            }

            for (int jj = 0; jj < nFillWeights.Length; jj++)
            {

                dgvDataView.Columns[jj].Visible = true;
                dgvDataView.Columns[jj].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDataView.Columns[jj].FillWeight = nFillWeights[jj];
                dgvDataView.Columns[jj].MinimumWidth = 5;

            }
        }

        /// <summary>
        /// 加载待检充电枪
        /// </summary>
        /// <param name="lstCharger">充电枪位号集合</param>
        public void ChargerInfo_AutoLoad(List<ChargerInfoModel> lstCharger)
        {
            if (dgvDataView.InvokeRequired)
                dgvDataView.Invoke(new ChargerAutoEventHandler(ChargerInfoLoadInvoked), lstCharger);
            else
                ChargerInfoLoadInvoked(lstCharger);
        }


        /// <summary>
        /// 重新加载UI枪信息
        /// </summary>
        /// <param name="lstCharger">充电枪位号集合</param>
        public void RefreshChargerInfo(List<int> lstCharger, int itemID)
        {
            if (dgvDataView.InvokeRequired)
                dgvDataView.Invoke(new ChargerRefreshEventHandler(RefreshChargerInfoInvoked), lstCharger, itemID);
            else
                RefreshChargerInfoInvoked(lstCharger, itemID);
        }
        /// <summary>
        /// 插入充电枪信息委托
        /// </summary>
        /// <param name="lstChargerrId">充电枪位号集合</param>
        private delegate void ChargerAutoEventHandler(List<ChargerInfoModel> lstChargerrId);
        /// <summary>
        /// 插入充电枪信息回调方法
        /// </summary>
        /// <param name="lstCharger">充电枪位号集合</param>
        private void ChargerInfoLoadInvoked(List<ChargerInfoModel> lstCharger)
        {
            dgvDataView.Rows.Clear();
            m_ChargerId = 0;
            DataGridViewCellStyle dgvCellStyle = new DataGridViewCellStyle();
            dgvCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (lstCharger != null)
            {
                for (int i = 0; i < lstCharger.Count; i++)
                {
                    DataGridViewRow dgvRow = new DataGridViewRow();
                    m_ChargerId = lstCharger[i].ChargerId;
                    string str = "号枪";
                    string TranslateStr = LanguageManager.GetByKey(str);
                    dgvRow.HeaderCell.Value = string.Format("{0}"+ TranslateStr, m_ChargerId);
                    dgvDataView.Rows.Add(dgvRow);
                }
            }
        }
        /// <summary>
        /// 刷新充电枪信息委托
        /// </summary>
        /// <param name="lstChargerrId">充电枪位号集合</param>
        private delegate void ChargerRefreshEventHandler(List<int> lstChargerId, int itemID);
        private void RefreshChargerInfoInvoked(List<int> lstCharger, int itemID)
        {
            string str = "号枪";
            string TranslateStr = LanguageManager.GetByKey(str);
            for (int i = lstCharger.Count - 1; i >= 0; i--)
            {
                for (int j = dgvDataView.RowCount - 1; j >= 0; j--)
                {
                    if (dgvDataView.Rows[j].HeaderCell.Value.ToString() == lstCharger[i].ToString() + TranslateStr)
                    {
                        dgvDataView.Rows.RemoveAt(j);
                    }
                }
            }

            m_ChargerId = 0;
            DataGridViewCellStyle dgvCellStyle = new DataGridViewCellStyle();
            dgvCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (lstCharger != null)
            {
                for (int i = 0; i < lstCharger.Count; i++)
                {
                    DataGridViewRow dgvRow = new DataGridViewRow();
                    m_ChargerId = lstCharger[i];
                    dgvRow.HeaderCell.Value = string.Format("{0}"+ TranslateStr, m_ChargerId);
                    dgvDataView.Rows.Add(dgvRow);
                }
            }
        }

        /// <summary>
        /// 清空当前视图数据
        /// </summary>
        public void dgvDataView_Clear()
        {
            dgvDataView.Rows.Clear();
            for (int i = 0; i < dgvDataView.RowCount; i++)
            {
                dgvDataView.Rows[i].HeaderCell.Tag = null;
                for (int j = 0; j < dgvDataView.ColumnCount; j++)
                {
                    dgvDataView.Rows[i].Cells[j].Value = String.Empty;
                }
            }
        }


        /// <summary>
        /// 清空所有数据，待新数据插入
        /// </summary>
        /// <param name="itemId">检定项序号</param>
        /// <param name="lstChargerID">需要检测的枪编号集合</param>
        public void DataTable_Clear(int itemId, List<int> lstChargerID)
        {
            try
            {

                //不检测的枪数据保留，需要检测的枪数据清空
                BusinessManage BCM = BusinessManage.GetInstance();
                if (itemId < 0) return;

                if (m_LstStorage.ContainsId(itemId))
                {
                    for (int j = lstChargerID.Count - 1; j >= 0; j--)
                    {
                        for (int i = m_LstStorage[itemId].Count - 1; i >= 0; i--)
                        {

                            if (m_LstStorage[itemId][i].ChargerID == lstChargerID[j])
                            {
                                m_LstStorage[itemId].RemoveAt(i);
                            }
                        }
                    }
                    m_LstStorage[itemId].Init(lstChargerID, 18);
                }

                //dgvDataView_Clear();
                if (itemId == nSelectedIndex)
                {
                    ChargerInfo_AutoLoad(BCM.lstChargerInfo);

                    // RefreshChargerInfo(lstChargerID, itemId);
                }
            }
            catch (Exception e) { }
        }
        #endregion

        #region ----------------辅助方法-----------------
        /// <summary>
        /// 初始化所有检定结果
        /// </summary>
        /// <param name="itemCount">检定项数量</param>
        /// <param name="chargerCount">待检充电枪数量</param>
        public void DataCollection_Init(int itemCount, int chargerCount)
        {
            if (m_LstStorage == null)
                m_LstStorage = new ListNodes();
            else
                m_LstStorage.Clear();
            for (int i = 0; i < itemCount; i++)
            {
                m_LstStorage.Add(i);
                m_LstStorage[i].Init(chargerCount, 18);
            }
        }

        //私有: 返回可见的列
        private int GetVisibleColumn()
        {
            int nVisibleCount = 0;
            for (int i = 0; i < dgvDataView.ColumnCount; i++)
                if (dgvDataView.Columns[i].Visible)
                    nVisibleCount++;
            return nVisibleCount;
        }
        //私有: 增加检定数据 
        //   nItemId--检定项序号
        //   nColIndex--列节点序号
        //   nRowIndex--行节点序号
        //   objCellValue--单元格值
        private void Dictionary_Add(int nItemId, int nColIndex, int nRowIndex, object objCellValue, bool isAddNewRow)
        {
            if (m_LstStorage.ContainsId(nItemId))
            {
                //第一列存枪号信息，所有数据列索引向后增加一位
                m_LstStorage[nItemId][nRowIndex][nColIndex + 1].Column = objCellValue;
                m_LstStorage[nItemId][nRowIndex][nColIndex + 1].Visible = true;
            }
        }

        #endregion
        private void dgvDataView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string str = "报表";
            string TranslateStr = LanguageManager.GetByKey(str);
            if (e.RowIndex >= 0 && e.ColumnIndex == 17)
            {
                try
                {
                    string path = "";
                    for (int i = 0; i < m_LstStorage[nSelectedIndex][e.RowIndex].Count; i++)
                    {
                        if (m_LstStorage[nSelectedIndex][e.RowIndex][i].Column.ToString().Contains(TranslateStr))
                        {

                            path = System.Environment.CurrentDirectory + "\\" + m_LstStorage[nSelectedIndex][e.RowIndex][i].Column.ToString();
                            if (path.Contains(".jpg") || path.Contains(".bmp"))
                            {
                                FrmOscilloscopeImage frm = FrmOscilloscopeImage.GetInstance();

                                frm.StartPosition = FormStartPosition.CenterScreen;
                                frm.Text = m_LstStorage[nSelectedIndex][e.RowIndex].ChargerID.ToString() + LanguageManager.GetByKey("号枪位示波器截图");
                                frm.pictureBox1.Image = Image.FromFile(path);
                                frm.Show();
                            }
                        }
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
