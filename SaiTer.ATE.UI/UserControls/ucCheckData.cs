using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.PrjUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ucCheckData : DataGridViewBase
    {
        #region --------------变量定义--------------
        //静态实例
        private static ucCheckData Instance = null;
        private static object _SynLock = new object();
        //检定数据集合
        private ArrayList arrTrialData = new ArrayList();

        private BusinessManage BCM = BusinessManage.GetInstance();

        private Dictionary<int, string[]> DicTitleNames = new Dictionary<int, string[]>();  //业务表头,int 为业务类型号，业务显示的列名
        private Dictionary<int, int[]> DicTitleNamesLength = new Dictionary<int, int[]>();  //业务表头长度


        /// <summary>
        /// 委托
        /// </summary>
        /// <param name="trlNo"></param>
        /// <param name="itemId"></param>
        private delegate void PaintColumnHeaderEventHandler(EmTrialType trlNo);
        #endregion



        #region --------------构造函数--------------
        /// <summary>
        /// 构造函数
        /// </summary>
        public ucCheckData()
        {
            InitializeComponent();
            try
            {
                ReadInterfaceConfig();
                AutoLoadCharger();
                //SystemEvent.SendTestResultToUIEvent += SystemEvent_SendTestResultToUIEvent;
                SystemEvent.SendDataMessageToUIEvent += SystemEvent_SendDataMessageToUIEvent;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void SystemEvent_SendDataMessageToUIEvent(TrialDataModel TrialData, bool isClear = false)
        {
            try
            {
                #region 一个检测项多条数据新代码

                arrTrialData.Clear();

                int Rows = 0;
                bool isAddNewDgvRow = false; //UI表格添加新行或者更新已有行的数据
                if (nSelectedIndex == BCM.TrialItemOrderID)//当前选中的检测项=正在测试的项，才刷新ui数据，否则只更新数据仓库
                {
                    for (int i = 0; i < dgvDataView.Rows.Count; i++)
                    {
                        string str = dgvDataView.Rows[i].HeaderCell.Value.ToString();//行头文本  如：1号枪
                        int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", ""));//行头字符串提取出枪号

                        if (id == TrialData.ChargerId)
                        {
                            if (dgvDataView.Rows[i].Cells[0].Value == null || dgvDataView.Rows[i].Cells[0].Value.ToString() == "")//这个值为空代表这一行还没有添加数据
                            {
                                Rows = i;
                                isAddNewDgvRow = false;
                                break;
                            }
                            else//这一行有数据了，插在下一行
                            {
                                Rows = i + 1;//需要插入的数据在最后一行枪号相同的数据下面
                                isAddNewDgvRow = true;
                            }
                        }
                    }
                }
                if (TrialData.TrialResult != EmTrialResult.Wait && TrialData.ExtentData != "")//如果是检测刚开始，这个值为空，不刷新界面
                {
                    string[] _ExtentData = TrialData.ExtentData.Split('|');
                    string imagePath = null;
                    for (int i = 0; i < _ExtentData.Length; i++)
                    {
                        if (_ExtentData[i] != "*")
                        {
                            if (_ExtentData[i].Contains(LanguageManager.GetByKey("报表(勿删)")))
                            {
                                imagePath = _ExtentData[i];
                                continue;
                            }
                            arrTrialData.Add(_ExtentData[i]);
                        }
                    }
                    if (TrialData.TrialResult == EmTrialResult.Pass)
                    {
                        arrTrialData.Add("PASS");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }

                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Black, isAddNewDgvRow, TrialData.ChargerId);
                    }
                    else if (TrialData.TrialResult == EmTrialResult.NA)
                    {
                        arrTrialData.Add("NA");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }

                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Black, isAddNewDgvRow, TrialData.ChargerId);
                    }
                    else
                    {
                        arrTrialData.Add("FAIL");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }
                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Red, isAddNewDgvRow, TrialData.ChargerId);
                    }

                }
                //dgvDataView_RefreshCellValue(nSelectedIndex);
                #endregion

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public void AutoLoadCharger()
        {
            DataCollection_Init(BCM.lstTrialItemsInfo.Count, BCM.lstChargerInfo.Count);

            ChargerInfo_AutoLoad(BCM.lstChargerInfo);
        }


        private void ReadInterfaceConfig()
        {
            try
            {
                //加载界面类名配置文件 
                XDocument _XDocInterface = XDocument.Load("xml\\InterfaceManage.xml");
                foreach (XElement item in _XDocInterface.Descendants("Interfaces").Elements("Interface"))
                {
                    if (!DicTitleNames.ContainsKey(Convert.ToInt32(item.Attribute("TrialTypeID").Value)))
                    {
                        string[] tempTitle = item.Attribute("TitleName").Value.ToString().Split('|');
                        string[] title = new string[tempTitle.Length];
                        for (int i = 0; i < title.Length; i++)
                        {
                            title[i] = LanguageManager.GetByKey(tempTitle[i]);
                        }

                        if (tempTitle.Length > 0)
                        {
                            DicTitleNames.Add(Convert.ToInt32(item.Attribute("TrialTypeID").Value), title);
                        }
                    }
                    if (!DicTitleNamesLength.ContainsKey(Convert.ToInt32(item.Attribute("TrialTypeID").Value)))
                    {
                        string[] tempTitleLength = item.Attribute("TitleLength").Value.ToString().Split('|');

                        if (tempTitleLength.Length > 0)
                        {
                            int[] nTitleLength = new int[tempTitleLength.Length];
                            for (int n = 0; n < tempTitleLength.Length; n++)
                            {
                                nTitleLength[n] = Convert.ToInt32(tempTitleLength[n]);
                            }
                            DicTitleNamesLength.Add(Convert.ToInt32(item.Attribute("TrialTypeID").Value), nTitleLength);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 静态实例初始化函数
        /// </summary>
        /// <returns>静态实例</returns>
        public static ucCheckData GetInstance()
        {
            if (Instance == null)
                Instance = new ucCheckData();
            return Instance;
        }
        #endregion



        #region --------------控件事件--------------

        /// <summary>
        /// 刷新检定数据
        /// </summary>
        /// <param name="rowIndex">检定方案行索引</param>
        /// <param name="trlNo">检定功能代码</param>
        /// <param name="itemId">检定功能中检定项编号</param>
        public override void dgvDataView_RefreshData(int rowIndex, EmTrialType trlNo)
        {
            if (SelectedItemIndex != rowIndex) //刷新列头            
                dgvDataView_PaintColumnHeader(trlNo);

            base.dgvDataView_RefreshData(rowIndex, trlNo);
        }

        /// <summary>
        /// 刷新特定数据
        /// </summary>
        /// <param name="Column0"></param>
        /// <param name="Column1"></param>
        public void dgvDataView_Refresh_Run(string Column0, string Column1)
        {
            for (int i = 0; i < dgvDataView.RowCount; i++)
            {
                dgvDataView.Rows[i].Cells[0].Value = Column0;
                dgvDataView.Rows[i].Cells[1].Value = Column1;
            }
        }

        /// <summary>
        /// 重画DataGridView列头
        /// </summary>
        /// <param name="trlNo">检定功能代码</param>
        /// <param name="itemId">检定功能中检定项编号</param>
        private void dgvDataView_PaintColumnHeader(EmTrialType trlNo)
        {
            if (dgvDataView.InvokeRequired)
                dgvDataView.Invoke(new PaintColumnHeaderEventHandler(PaintColumnHeaderInvoked), trlNo);
            else
                PaintColumnHeaderInvoked(trlNo);
        }
        /// <summary>
        /// 重绘表头  
        /// </summary>
        /// <param name="trlNo"></param>
        /// <param name="itemId"></param>
        private void PaintColumnHeaderInvoked(EmTrialType trlNo)
        {
            lock (_SynLock)
            {
                if (DicTitleNames.ContainsKey((int)trlNo) && DicTitleNamesLength.ContainsKey((int)trlNo))
                {
                    dgvDataView_ResetColumnWidth(DicTitleNamesLength[(int)trlNo]);
                    dgvDataView_ResetColumnHeader(DicTitleNames[(int)trlNo]);
                }
                else
                {
                    //如果刷新的业务在配置文件中不存在，则刷新一个只有合格列的默认界面
                    dgvDataView_ResetColumnWidth(100);
                    dgvDataView_ResetColumnHeader(LanguageManager.GetByKey("检测结果"));
                }
            }
        }

        private void SystemEvent_SendTestResultToUIEvent(TrialDataModel TrialData)
        {
            try
            {

                #region 一个检测项多条数据新代码

                arrTrialData.Clear();

                int Rows = 0;
                bool isAddNewDgvRow = false; //UI表格添加新行或者更新已有行的数据
                if (nSelectedIndex == BCM.TrialItemOrderID)//当前选中的检测项=正在测试的项，才刷新ui数据，否则只更新数据仓库
                {
                    for (int i = 0; i < dgvDataView.Rows.Count; i++)
                    {
                        string str = dgvDataView.Rows[i].HeaderCell.Value.ToString();//行头文本  如：1号枪
                        int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", ""));//行头字符串提取出枪号

                        if (id == TrialData.ChargerId)
                        {
                            if (dgvDataView.Rows[i].Cells[0].Value == null || dgvDataView.Rows[i].Cells[0].Value.ToString() == "")//这个值为空代表这一行还没有添加数据
                            {
                                Rows = i;
                                isAddNewDgvRow = false;
                                break;
                            }
                            else//这一行有数据了，插在下一行
                            {
                                Rows = i + 1;//需要插入的数据在最后一行枪号相同的数据下面
                                isAddNewDgvRow = true;
                            }
                        }
                    }
                }
                if (TrialData.TrialResult != EmTrialResult.Wait && TrialData.ExtentData != "")//如果是检测刚开始，这个值为空，不刷新界面
                {
                    string[] _ExtentData = TrialData.ExtentData.Split('|');
                    for (int i = 0; i < _ExtentData.Length; i++)
                    {
                        if (_ExtentData[i] != "*")
                        {
                            arrTrialData.Add(_ExtentData[i]);
                        }
                    }
                    if (TrialData.TrialResult == EmTrialResult.Pass)
                    {
                        arrTrialData.Add("PASS");
                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Black, isAddNewDgvRow, TrialData.ChargerId);
                    }
                    else if (TrialData.TrialResult == EmTrialResult.NA)
                    {
                        arrTrialData.Add("NA");
                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Black, isAddNewDgvRow, TrialData.ChargerId);
                    }
                    else
                    {
                        arrTrialData.Add("FAIL");
                        dgvDataView_AddRow(BCM.TrialItemOrderID, Rows, arrTrialData, Color.Red, isAddNewDgvRow, TrialData.ChargerId);
                    }
                }
                dgvDataView_RefreshCellValue(nSelectedIndex);
                #endregion

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        #endregion

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var parms = base.CreateParams;
        //        parms.Style &= ~0x02000000; // Turn off WS_CLIPCHILDREN
        //        return parms;
        //    }
        //}
    }
}
