using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.Manage;
using SaiTer.ATE.UI.Assitand.PrjUI;
using SaiTer.ATE.UI.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using SaiTer.ATE.InterFace;
using System.IO;
using SaiTer.ATE.DataModel.EnumModel;
using System.Xml;
using System.Diagnostics;
using System.Drawing;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL;
using System.Windows.Documents;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using Sunny.UI;
using SaiTer.ATE.UI.Assitand;
using System.Configuration;
using SaiTer.ATE.UI;
using System.Globalization;
using SaiTer.ATE.MES;
using Saiter;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using NPOI.POIFS.Crypt.Dsig;
using System.Linq;

namespace SaiTer.ATE.UI
{
    public partial class FrmMain : FormBase
    {

        #region --------------变量定义--------------
        /// <summary>
        /// 是否加载检测合格率控件
        /// </summary>

        private bool isTestQuality = false;
        /// <summary>
        /// 是否加载MES设置
        /// </summary>

        private bool isCANSet = true;
        private bool isMESSet = false;
        private bool isAutoTest = false;
        private bool isGroupCharger = false;
        private string _UserType = "赛特新能";
        public string UserType
        {
            get { return _UserType; }
            set
            {
                _UserType = value;
                if (testItemForm != null)
                {
                    ucTestItems.UserType = _UserType;
                }
            }
        }

        CancellationTokenSource ctsStartTest = null;    //用于终止检测流程

        //用户控件

        private Panel _overlayPanel;
        public ucCheckData checkDataForm;//测试数据
        public ucTestItems testItemForm;//检测项
        public ucConnectState connectStateForm = ucConnectState.GetInstance();//设备连接状态
        public FrmInfo infoForm; //操作倒计时提示信息
        FrmMesage frmMesage;
        private ucWaitTestChagers _ucChargerInfo;//控件-选择需要检测的充电桩
        public ucEquipMonitorData _ucMonitor = new ucEquipMonitorData(new object());
        public static string SchemeName = "";//当前枪所使用的测试方案的名称

        //待检充电枪信息集合
        private List<TrialDataModel> m_LstChargers = null;
        //检定业务ID
        private int nCheckItemNo = -1;   //当前正在检定项序号(非鼠标或键盘选中检定项序号)
        /// <summary>
        /// 试验信息集合
        /// </summary>
        public List<StTrialItem> lstTestItems = new List<StTrialItem>();
        /// <summary>
        /// 检定数据集合
        /// </summary>
        private ArrayList arrTrialData = new ArrayList();

        #endregion

        #region 配置属性
        string SafeTestMode = ConfigurationManager.AppSettings["SafeTestMode"];     //安全测试模式（不强制停止检测）
        #endregion

        /// <summary>
        /// 检定系统
        /// </summary>
        private BusinessManage BCM;

        private static FrmMain Instance = null;
        public static FrmMain GetInstance(string schemeName)
        {
            SchemeName = schemeName;
            if (Instance == null)
            {
                Instance = new FrmMain();
            }
            Instance.Activate();
            return Instance;
        }
        private FrmMain()
        {
            InitializeComponent();
            try
            {
                this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

                SystemEvent.SendCountDownTimerEvent += SystemEvent_SendCountDownTimerEvent;
                SystemEvent.SendWaitSwipingCardEvent += SystemEvent_SendWaitSwipingCardEvent;
                SystemEvent.SetUIButtonEvent += SetUIButtonEnable;
                SystemEvent.EventSendTrialDataToUI += SystemEvent_EventSendTrialDataToUI;
                SystemEvent.SaveTrialDataEvent += SystemEvent_SaveTrialDataEvent;
                SystemEvent.MessageInfoEvent+= SystemEvent_MessageInfoEvent;
                SystemEvent.GetMessageInfoEvent += SystemEvent_GetMessageInfoEvent;
                SystemEvent.SetBMSEvent += SystemEvent_SetBMSEvent;
                SystemEvent.SetDtEvent += SystemEvent_SetDtEvent;
                //检定系统BCM初始化的时候会发送设备的监视数据上来，在此之前注册事件来加载主窗体上的设备实时状态数据控件
                SystemEvent.SendMonitorMessageEvent += SystemEvent_SendMonitorMessageEvent;
                #region 自动化产线使用
                SystemEvent.SetSchemeEvent += SystemEvent_SetSchemeEvent;
                SystemEvent.StartTestEvent += SystemEvent_StartTestEvent;
                SystemEvent.StopTestEvent += SystemEvent_StopTestEvent;
                SystemEvent.SetChargerInfoEvent += SystemEvent_ChargerInfoEvent;
                #endregion

                BCM = BusinessManage.GetInstance();
                BCM.GetTrialScheme(SchemeName);

                //检定系统初始化结束后，退订此事件。防止实时数据不断触发此事件
                SystemEvent.SendMonitorMessageEvent -= SystemEvent_SendMonitorMessageEvent;


                #region ---------------用户控件---------------
                lab_Left3Caption.Text = LanguageManager.GetByKey("检测方案") + ": " + SchemeName;
      
                lstTestItems = BCM.lstTrialItemsInfo;
                int MaxChargerCount = BCM._xmlInfoAssembly._systemXmlInfo.MaxChargerCount;
                testItemForm = ucTestItems.GetInstance(MaxChargerCount, _UserType);
                //整个方案测试不勾选
                //if (isAutoTest)
                //{
                //    testItemForm.dgvTestItem.Columns["clmCheck"].Visible = false;
                //}
                //pnl_Left.Width += BCM.lstChargerInfo.Count > 2 ? 38 + 40 : (BCM.lstChargerInfo.Count - 1) * 38 + 45;
                pnl_Left.Width += MaxChargerCount > 2 ? 38 + 40 : (MaxChargerCount - 1) * 38 + 45;

                //注册刷新检定数据窗体表头
                testItemForm.DataViewRefreshed += TestItemForm_DataViewRefreshed;
                //注册刷新检定数据窗体特殊检定点初始值
                testItemForm.RunRefreshed += TestItemForm_RunRefreshed;

                this.pnlCheckItems.Controls.Add(testItemForm);


                testItemForm.Dock = DockStyle.Fill;
                this.panel42.Controls.Add(connectStateForm);
                connectStateForm.Dock = DockStyle.Fill;


                ucLog _ucLog = new ucLog();
                this.pnlLog.Controls.Add(_ucLog);
                _ucLog.Dock = DockStyle.Fill;

                _ucChargerInfo = ucWaitTestChagers.GetInstance();
                this.panel3.Controls.Add(_ucChargerInfo);
                _ucChargerInfo.Dock = DockStyle.Fill;
                //this.panel3.Parent.Height = BCM.lstChargerInfo.Count > 0 ? 140 * (int)Math.Ceiling((double)(BCM.lstChargerInfo.Count / 4.0)) + 40 : 180;
                this.panel3.Parent.Height = 140 * (int)Math.Ceiling((double)(MaxChargerCount / 4.0)) + 40;

                checkDataForm = ucCheckData.GetInstance();
                pnlMain.Controls.Add(checkDataForm);
                checkDataForm.Dock = DockStyle.Fill;
                checkDataForm.dgvDataView_RefreshData(0, BCM.lstTrialItemsInfo[0].TrialType);

                //遮罩层初始化
                InitializeOverlay();
                #endregion
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        #region 遮罩层
        // 初始化遮罩层
        private void InitializeOverlay()
        {
            _overlayPanel = new Panel
            {
                BackColor = Color.FromArgb(128, 64, 64, 64), // 半透明白色改为灰色
                Dock = DockStyle.Fill,
                Visible = false
            };
            this.Controls.Add(_overlayPanel);
            _overlayPanel.BringToFront();
        }

        // 显示遮罩层（带渐变动画）
        private void ShowOverlay()
        {
            _overlayPanel.Visible = true;
            AnimateOverlay(0, 128); // 透明度从0到128
        }

        // 隐藏遮罩层
        private void HideOverlay()
        {
            AnimateOverlay(128, 0, () => _overlayPanel.Visible = false);
        }

        // 渐变动画效果
        private async void AnimateOverlay(int startAlpha, int endAlpha, Action onComplete = null)
        {
            int steps = 10;
            int delay = 20;

            //for (int i = 0; i <= steps; i++)
            //{
            //    int alpha = startAlpha + (endAlpha - startAlpha) * i / steps;
            //    _overlayPanel.BackColor = Color.FromArgb(alpha, 64, 64, 64);
            //    await Task.Delay(delay);
            //}
            onComplete?.Invoke();
        }
        #endregion

        private bool SystemEvent_GetMessageInfoEvent()
        {
            bool result = false;
            // 获取窗体实例并在主线程上执行显示操作
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    FrmMesage frmMesage = FrmMesage.GetInstance();

                   result= frmMesage.GetVisvible();
 
                }));
            }
            else
            {
                FrmMesage frmMesage = FrmMesage.GetInstance();

                result = frmMesage.GetVisvible();
            }

            return result;
        }

        private void SystemEvent_SetDtEvent(int TestID)
        {
            // 获取窗体实例并在主线程上执行显示操作
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    FrmConsistBMS frmConsistBMS = FrmConsistBMS.GetInstance(_ucChargerInfo.m_LstChargerInfos);
                    frmConsistBMS.SetDt(TestID);
                    // 确保窗体可被拖动，同时允许调整大小
                    frmConsistBMS.FormBorderStyle = FormBorderStyle.Sizable;
                    frmConsistBMS.ControlBox = false;
                    // 如果需要的话，可以设置窗体初始位置
                    frmConsistBMS.StartPosition = FormStartPosition.CenterScreen;
                }));
            }
            else
            {
                FrmConsistBMS frmConsistBMS = FrmConsistBMS.GetInstance(_ucChargerInfo.m_LstChargerInfos);
                frmConsistBMS.SetDt(TestID);
                // 确保窗体可被拖动，同时允许调整大小
                frmConsistBMS.FormBorderStyle = FormBorderStyle.Sizable;
                frmConsistBMS.ControlBox = false;
                // 如果需要的话，可以设置窗体初始位置
                frmConsistBMS.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void SystemEvent_SetBMSEvent(bool isShow = true)
        {
            // 获取窗体实例并在主线程上执行显示操作
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    FrmConsistBMS frmConsistBMS = FrmConsistBMS.GetInstance(_ucChargerInfo.m_LstChargerInfos);
                    // 确保窗体可被拖动，同时允许调整大小
                    frmConsistBMS.FormBorderStyle = FormBorderStyle.Sizable;
                    frmConsistBMS.ControlBox = false;
                    // 如果需要的话，可以设置窗体初始位置
                    frmConsistBMS.StartPosition = FormStartPosition.CenterScreen;
                    frmConsistBMS.SetBMS(isShow);

                }));
            }
            else
            {
                FrmConsistBMS frmConsistBMS = FrmConsistBMS.GetInstance(_ucChargerInfo.m_LstChargerInfos);
                // 确保窗体可被拖动，同时允许调整大小
                frmConsistBMS.FormBorderStyle = FormBorderStyle.Sizable;
                frmConsistBMS.ControlBox = false;
                // 如果需要的话，可以设置窗体初始位置
                frmConsistBMS.StartPosition = FormStartPosition.CenterScreen;
                frmConsistBMS.SetBMS(isShow);

            }
        }

        private void SystemEvent_MessageInfoEvent(bool IsShow, string Info,bool Confrim=false)
        {

            // 获取窗体实例并在主线程上执行显示操作
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    FrmMesage frmMesage = FrmMesage.GetInstance();
                    frmMesage.SetMessageInfo(IsShow, Info, Confrim);
                    // 确保窗体可被拖动，同时允许调整大小
                    frmMesage.FormBorderStyle = FormBorderStyle.Sizable;
                    frmMesage.ControlBox = false;
                    // 如果需要的话，可以设置窗体初始位置
                    frmMesage.StartPosition = FormStartPosition.CenterScreen;
                }));
            }
            else
            {
                FrmMesage frmMesage = FrmMesage.GetInstance();
                frmMesage.SetMessageInfo(IsShow, Info);

                // 同样确保窗体属性正确设置
                frmMesage.FormBorderStyle = FormBorderStyle.Sizable;
                frmMesage.ControlBox = false;
                frmMesage.StartPosition = FormStartPosition.CenterScreen;
            }
     
 

        }

        private void TestItemForm_RunRefreshed(string Column0, string Column1)
        {
            checkDataForm.dgvDataView_Refresh_Run(Column0, Column1);
        }
        private delegate void refWorkAreaCaption(string str);
        private void TestItemForm_DataViewRefreshed(int rowIndex, EmTrialType funcNo, string itemDetail)
        {
            try
            {
                if (rowIndex < 0) return;

                if (!string.IsNullOrEmpty(itemDetail))
                {
                    if (this.InvokeRequired)
                    {
                        this.ts_Tester.Invoke(new refWorkAreaCaption(RefreshWorkAreaCaption), itemDetail);
                    }
                    else
                    { RefreshWorkAreaCaption(itemDetail); }
                }

                //刷新检定数据
                checkDataForm.dgvDataView_RefreshData(rowIndex, funcNo);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        private void RefreshWorkAreaCaption(string str)
        {
            WorkAreaCaption = LanguageManager.GetByKey("检定项") + ">>" + str;
        }
        private delegate void DeleSetButton(bool Enable, string btnStopEnalbe = null);
        /// <summary>
        /// 设置主窗体菜单栏按钮可用状态
        /// </summary>
        /// <param name="Enable">是否可用</param>
        /// <param name="btnStopEnalbe">停止按钮是否可用</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SetUIButtonEnable(bool Enable, string btnStopEnalbe = null)
        {
            if (this.InvokeRequired)
            {
                this.ts_Tester.Invoke(new DeleSetButton(ShowButton), Enable, btnStopEnalbe);
            }
            else
            {
                ShowButton(Enable, btnStopEnalbe);
            }
        }

        private void ShowButton(bool Enable, string btnStopEnalbe = null)
        {
            if (btnStopEnalbe == null)
            {
                tsBtnTrialStart.Enabled = Enable;
                tsBtnAddChargeInfo.Enabled = Enable;
                tsBtnPortSet.Enabled = Enable;
                tsBtnSysParam.Enabled = Enable;
                tsBtnSchemeSelect.Enabled = Enable;
                tsBtnTrialData.Enabled = Enable;
                tsBtnSaveData.Enabled = Enable;
                tsBtnUser.Enabled = Enable;
                tsBtnEditScheme.Enabled = Enable;
                tsBtnMESSet.Enabled = Enable;
            }
            else
            {
                tsBtnTrialStop.Enabled = bool.Parse(btnStopEnalbe);
            }
            if (Enable)
                tsBtnTrialStop.Enabled = true;
        }
        /// <summary>
        /// 弹窗等待刷卡
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="tBMSDemandVoltage"></param>
        private void SystemEvent_SendWaitSwipingCardEvent(List<int> lstIDs, double tBMSDemandVoltage, EmChargerType BMSType, int type)
        {
            FrmWaitSwipingCard waitSwipingCardForm = new FrmWaitSwipingCard(BMSType);
            waitSwipingCardForm.WaitSwipingCard(lstIDs, tBMSDemandVoltage, type);
        }

        /// <summary>
        /// 弹窗提示信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="time"></param>
        /// <param name="type"></param>
        private void SystemEvent_SendCountDownTimerEvent(string info, int time, int type, string tag)
        {
            // infoForm = FrmInfo.GetInstance();
            infoForm = new FrmInfo();
            infoForm.CountDown(info, time, type, tag);
        }
        /// <summary>
        /// 保存数据到正式表
        /// </summary>
        private void SystemEvent_SaveTrialDataEvent()
        {
            SavaTrialDataToFormalTable();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                string strTestQuality = ConfigurationManager.AppSettings["isTestQuality"];
                if (strTestQuality != null)
                {
                    isTestQuality = bool.Parse(strTestQuality);
                }
                //是否加载统计测试合格率窗口
                if (isTestQuality)
                {
                    int totalCharger = GetTestChargerCount(out int PassCount);
                    this.SubExpRight1Visible = true;
                    this.SubExpRight1Height = 280;
                    lab_Right1Caption.Text = "检测信息统计";
                    ucSystemInfo uc = ucSystemInfo.GetInstance();
                    uc.TotalCharger = totalCharger;
                    uc.PassCharger = PassCount;
                    panel40.Controls.Add(uc);
                    uc.Dock = DockStyle.Fill;

                }
                //是否加载MES设置窗口
                string strMESSet = ConfigurationManager.AppSettings["isMESSet"];
                if (strMESSet != null)
                {
                    isMESSet = bool.Parse(strMESSet);
                    toolStripLabel13.Visible = bool.Parse(strMESSet);
                }
                toolStripLabel13.Visible = isMESSet;
                tsBtnMESSet.Visible = isMESSet;

                //是否加载CAN设置窗口
                string strCANSet = ConfigurationManager.AppSettings["isCANSet"];
                if (strCANSet != null)
                {
                    isCANSet = bool.Parse(strCANSet);
                    toolStripLabel4.Visible = bool.Parse(strCANSet);
                }
                tsBtnCAN.Visible = isCANSet;

                //是否加载自动检测
                string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"]; 
                if (strAutoTest != null)
                {
                    isAutoTest = bool.Parse(strAutoTest);
                }
                toolStripLabel15.Visible = isAutoTest;
                tsBtnAuto.Visible = isAutoTest;

                //是否为群充桩
                string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
                if (strIsGroupC != null)
                {
                    isGroupCharger = Convert.ToBoolean(strIsGroupC);
                }

                //客户定制中性测试系统带LOGO，需要隐藏SaiTer字样
                SetCustomerLogo();

                SetButtonVisible();
                lblUserName.Text = _UserType;
                testItemForm.LoadCheckItem(lstTestItems);
                SystemEvent.SendCheckItemsEvent += SystemEvent_SendCheckItems;
                if (BCM.lstChargerInfo != null && BCM.lstChargerInfo.Count > 0)
                {
                    string strAddTrialData = ConfigurationManager.AppSettings["isAddTrialData"];
                    if (strAddTrialData != null)
                    {
                        bool isAutoAddData = bool.Parse(strAddTrialData);
                        if (isAutoAddData)
                        {
                            BCM.AddTrialData();
                        }
                    }
                    else
                    {
                        BCM.AddTrialData();
                    }
                }

                DeleLogFile();

                SetLanguage();
                this.comboBox1.DataSource = LanguageManager.Nativevalue;
                this.comboBox1.SelectedItem = new CultureInfo(LanguageManager.strDefaultLanguage).NativeName;
                //spcRunState.SplitterDistance = this.Height - 300;
                //spcMonitor.SplitterDistance = spcMonitor.Height - 420;


            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void SetCustomerLogo()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.ToString().Trim().Equals("TPK"))
            {
                pictureBox17.Image = Properties.Resources.TPK_Logo;
                pictureBox17.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox17.Size = new Size(400, 60);
                pictureBox17.Dock = DockStyle.Fill;
                this.Icon = Properties.Resources.TPK_Icon;
            }
        }

        private void SetButtonVisible()
        {
            if (_UserType != LanguageManager.GetByKey("管理员") && _UserType != LanguageManager.GetByKey("超级管理员"))
            {
                tsBtnEquipOperate.Visible = false;
                lblOprate.Visible = false;
                tsBtnPortSet.Visible = false;
                lblComm.Visible = false;
                tsBtnSysParam.Visible = false;
                lplSysParam.Visible = false;
                tsBtnSchemeSelect.Visible = false;
                lblScheme.Visible = false;
                tsBtnUser.Visible = false;
                tsBtnEditScheme.Visible = false;
                tsBtnRegist.Visible = false;
            }
            if (_UserType == LanguageManager.GetByKey("超级管理员"))
            {
                tsBtnRegist.Visible = true;
            }
        }
        /// <summary>
        ///  获取当日测试枪总数量
        /// </summary>
        /// <param name="PassCount">合格桩数量</param>
        /// <returns>当日已测桩总数量</returns>
        private int GetTestChargerCount(out int PassCount)
        {
            int totalCharger = ChargerInfoManage.GetTestChargerCount(out PassCount);
            return totalCharger;
        }

        /// <summary>
        /// 程序启动时读取的历史数据解析加载到UI
        /// </summary>
        /// <param name="LstTrialData"></param>
        private void SystemEvent_EventSendTrialDataToUI(List<TrialDataModel> LstTrialData)
        {
            try
            {
                for (int i = 0; i < LstTrialData.Count; i++)
                {
                    arrTrialData.Clear();
                    int Rows = 0;
                    for (int j = 0; j < BCM.lstChargerInfo.Count; j++)
                    {
                        if (BCM.lstChargerInfo[j].ChargerId == LstTrialData[i].ChargerId)
                        {
                            Rows = j;
                            break;
                        }
                    }
                    //方案ID
                    int SchemesNum = lstTestItems.FindIndex(s => s.ItemName == LstTrialData[i].TrialName && s.TrialType == LstTrialData[i].TrialType);
                    nCheckItemNo = SchemesNum;
                    if (nCheckItemNo < 0)
                    {
                        continue;
                    }

                    string[] _ExtentData = LstTrialData[i].ExtentData.Split('|');
                    string imagePath = null;

                    for (int n = 0; n < _ExtentData.Length; n++)
                    {
                        if (_ExtentData[n] != "*")
                        {
                            if (_ExtentData[n].Contains(LanguageManager.GetByKey("报表(勿删)")))
                            {
                                imagePath = _ExtentData[n];
                                continue;
                            }
                            arrTrialData.Add(_ExtentData[n]);
                        }
                    }
                    Color color = new Color();
                    if (LstTrialData[i].TrialResult == EmTrialResult.Pass)
                    {

                        arrTrialData.Add("PASS");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }
                        color = Color.Black;
                    }
                    else if (LstTrialData[i].TrialResult == EmTrialResult.NA)
                    {

                        arrTrialData.Add("NA");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }
                        color = Color.Black;
                    }
                    else
                    {

                        arrTrialData.Add("FAIL");
                        if (imagePath != null)
                        {
                            arrTrialData.Add(imagePath);
                        }
                        color = Color.Red;
                    }

                    checkDataForm.dgvDataView_AddRow(nCheckItemNo, Rows, arrTrialData, Color.Black, true, LstTrialData[i].ChargerId);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 删除上一月日志文件
        /// </summary>
        private void DeleLogFile()
        {
            try
            {
                DirectoryInfo m = new DirectoryInfo(Environment.CurrentDirectory + "\\Log");
                DirectoryInfo[] f = m.GetDirectories();
                for (int i = 0; i < f.Length; i++)
                {
                    if (f[i].CreationTime.Month == DateTime.Now.AddMonths(-1).Month)
                    {
                        f[i].Delete(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        //设备实时数据监控信息上发
        private void SystemEvent_SendMonitorMessageEvent(object monitorDada)
        {
            StateDataBase db = (StateDataBase)monitorDada;
            if (db.ChargerID != 0)
            {
                foreach (Control c in pnlMonitor.Controls)
                {
                    ucEquipMonitorData uc = c as ucEquipMonitorData;
                    if (uc.ChargerID == db.ChargerID && uc.EquipName == db.EquipName)
                    {
                        return;//已经添加过这个设备控件
                    }
                }
                _ucMonitor = new ucEquipMonitorData(monitorDada);
                _ucMonitor.Margin = new Padding(10, 5, 10, 5);
                pnlMonitor.Controls.Add(_ucMonitor);
            }

        }

        private void SystemEvent_SendCheckItems(List<StTrialItem> lstTrialItems, bool isTestItem, string strSchemeName = null)
        {

            if (isTestItem)
            {
                lstTestItems = lstTrialItems;
            }
            else
            {
                testItemForm.LoadCheckItem(lstTrialItems);
                if (strSchemeName != null)
                {
                    SchemeName = strSchemeName;
                    lab_Left3Caption.Text = LanguageManager.GetByKey("检测方案") + ": " + strSchemeName;
                    lstTestItems = lstTrialItems;
                }
                else
                {
                    lab_Left3Caption.Text = LanguageManager.GetByKey("检测方案") + ": " + SchemeName;
                }
            }
        }


        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (BCM != null)
            {
                BCM.OFFAllEquip();
                BCM._xmlInfoAssembly._EquipMentControl.ControlBoard?.SetLightColor(EmLightColor.Close);
            }
            this.Dispose();
            System.Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }


        private delegate void refRunForm(int i);
        /// <summary>
        /// 锁
        /// </summary>
        private static object lockrefForm = new object();
        /// <summary>
        /// 刷新运行中的界面
        /// </summary>
        /// <param name="trialtype"></param>
        public void Refrunform(int trialtype)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new refRunForm(RefFrom), trialtype);
            }
            else
            {
                RefFrom(trialtype);
            }
        }

        /// <summary>
        /// 刷新检定界面
        /// </summary>
        /// <param name="trialtype">实验类型</param>
        public void RefFrom(int trialtype)
        {
            lock (lockrefForm)
            {
                try
                {

                }

                catch (Exception ex)
                {
                    MessageBox.Show("刷新界面异常");
                    Log.Log.LogException(ex);

                }
            }
        }
        Thread th;
        private void TsBtnTrialStart_Click(object sender, EventArgs e)
        {
            StartTest();
        }

        private void StartTest()
        {
            try
            { 
                UtilLicense m_objLicense = new UtilLicense(DeviceType.ST_ATEFrame);
                bool bRet = m_objLicense.CheckLicense(true, false, false); // 默认中文，默认启动校验
                if (!bRet)
                {
                    MessageBox.Show("注册已过期！");
                    Application.Exit();
                }
                if (lstTestItems.Count > 0)
                {
                    SetUIButtonEnable(false);

                    if (isTestQuality)
                    {
                        SystemEvent.SendTrialState(EmTrialState.Start);
                    }

                    for (int i = 0; i < lstTestItems.Count; i++)
                    {
                        //先找到待检的试验项在表格中的顺序索引， 再按照索引清空该项目的界面数据
                        int index = BCM.lstTrialItemsInfo.FindIndex(s => s.TrialType == lstTestItems[i].TrialType && s.ItemName == lstTestItems[i].ItemName);
                        if (index >= 0)
                        {
                            checkDataForm.DataTable_Clear(index, _ucChargerInfo.m_LstChargerInfos);
                        }
                    }
                    BCM.ResetChargerInfo(_ucChargerInfo.m_LstChargerInfos);
                    if (SafeTestMode != null && SafeTestMode.ToString() == "1")
                    {
                        ctsStartTest = new CancellationTokenSource();
                        Task.Run(() =>
                        {
                            BCM.StartTrialTest(lstTestItems, ctsStartTest);
                        }, ctsStartTest.Token);
                    }
                    else
                    {
                        ThreadStart ts = delegate { BCM.StartTrialTest(lstTestItems); };
                        th = new Thread(ts);
                        th.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);

                if (isTestQuality)
                {
                    SystemEvent.SendTrialState(EmTrialState.End);
                }

            }
        }

        private void TsBtnTrialStop_Click(object sender, EventArgs e)
        {
            //if (tsBtnTrialStart.Enabled)
            //{
            //    return;
            //}

            StopTest();
        }

        public void StopTest()
        {
            try
            {
                tsBtnTrialStop.Enabled = false;
                if (BCM.NowTrialScheme == null)
                {
                    tsBtnTrialStop.Enabled = true;
                    return;
                }

                if (BCM.NowTrialScheme.TrialType != EmTrialType.Null)
                {
                    int trialType = (int)BCM.NowTrialScheme.TrialType;
                    BCM._xmlInfoAssembly._businessAssmeblyManager.Sessions[trialType].StopEvent();
                }
                if (ctsStartTest != null)
                    ctsStartTest.Cancel();
                if (th != null && th.IsAlive)
                {
                    SystemEvent.SendTrialResult(EmTrialResult.Wait);
                    th.Abort();
                    Thread.Sleep(1000);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            finally
            {
                SetUIButtonEnable(true);
                tsBtnTrialStop.Enabled = true;

                //是否加载统计测试合格率窗口
                if (isTestQuality)
                {
                    SystemEvent.SendTrialState(EmTrialState.End);
                    for (int i = 0; i < BCM.lstChargerInfo.Count; i++)
                    {
                        ChargerInfoManage.UpdateChargerTrialResult(BCM.lstChargerInfo[i].PKID.ToString());
                    }

                }

            }
        }

        private void TsBtnAddChargeInfo_Click(object sender, EventArgs e)
        {
            if (isGroupCharger)
            {
                FrmGroupChargerInfo frmAddCharger = FrmGroupChargerInfo.GetInstance();
                frmAddCharger.Show();
            }
            else
            {
                FrmChargerInfo frmAddCharger = FrmChargerInfo.GetInstance();
                frmAddCharger.Show();
            }
        }

        private void TsBtnSysParam_Click(object sender, EventArgs e)
        {
            FrmXMLParams frmXml = FrmXMLParams.GetInstance();
            frmXml.Show();
        }

        private void TsBtnPortSet_Click(object sender, EventArgs e)
        {
            FrmCommunication frmCom = FrmCommunication.GetInstance();
            frmCom.Show();
        }

        private void TsBtnEquipOperate_Click(object sender, EventArgs e)
        {
            FrmEquipOperate frmOperate = FrmEquipOperate.GetInstance();
            frmOperate.Show();
        }

        private void TsBtnSchemeSelect_Click(object sender, EventArgs e)
        {
            FrmSchemeSelect frm = FrmSchemeSelect.GetInstance();
            frm.Show();
        }

        private void TsBtnTrialData_Click(object sender, EventArgs e)
        {
            FrmTrialData frm = FrmTrialData.GetInstance();
            frm.Show();
        }
        //临时库数据存入正式库
        private void TsBtnSaveData_Click(object sender, EventArgs e)
        {
            /*1.查询临时表枪信息，插入正式表PKID设置为当前时间 （年月日时分秒毫秒）
             * 
             * 
             */

            SavaTrialDataToFormalTable();
        }
        /// <summary>
        /// 保存试验数据到正式表
        /// </summary>
        private void SavaTrialDataToFormalTable()
        {
            try
            {
                //暂时设置为不保存
                //if(isAutoTest)
                //{
                //    return;
                //}
                string Customer = ConfigurationManager.AppSettings["Customer"];
                if ((Customer == null || !Customer.Contains("KLQ")) && !isAutoTest)
                {
                    string str = "保存数据会封存当前枪信息的所有检测数据";
                    string info = LanguageManager.GetByKey(str);
                    ////新乡KLQ要求不弹窗 ，直接保存
                    if (MessageBox.Show(info, LanguageManager.GetByKey("提示"), MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                int totalCount = 0;//桩总数
                int passCount = 0;//合格桩数量
                for (int i = 0; i < BCM.lstChargerInfo.Count; i++)
                {
                    List<TrialDataModel> lstData = TrialItemResultTmpManage.GetTrialResultFromPKID(BCM.lstChargerInfo[i].PKID.ToString(), "1", BCM.lstChargerInfo[i].SchemeName);
                    if (lstData.Count == 0)
                    {
                        //string info = "有充电桩未做任何测试.无法保存";
                        //MessageBox.Show(LanguageManager.GetByKey(info));
                        //return;
                    }
                    else
                    {
                        totalCount++;
                    }
                    if (BCM.lstChargerInfo[i].CheckResult == EmTrialResult.Pass)
                    {
                        passCount++;
                    }
                }

                if(totalCount == 0)
                {
                    string info = "所有充电桩未做任何测试无法保存";
                    MessageBox.Show(LanguageManager.GetByKey(info));
                    return;
                }

                //上传MES测量值
                MESUtils.PostTestValue(BCM.lstChargerInfo);
                //上传MES测试结果
                MESUtils.PostTestResult(BCM.lstChargerInfo[0].BarCode, BCM.lstChargerInfo[0].ChargerId, BCM.lstChargerInfo[0].CheckResult);

                TrialItemResultTmpManage.SaveTrialData();
                List<ChargerInfoModel> lstChargerInfo = new List<ChargerInfoModel>();
                bool isOK = ChargerInfoManage.InsertChargerInfo(lstChargerInfo);
                if (isOK)
                {
                    BCM.LoadChargerInfo();
                    BCM._xmlInfoAssembly._ControlsManage.ControlAssmeblyManager.UpdateChargerInfo(lstChargerInfo);
                    //ucChargerInfo chargerInfo = ucChargerInfo.GetInstance();
                    //chargerInfo.SetChargerInfo();
                    //chargerInfo.GetChargerInfos();
                    ucWaitTestChagers ucWaitTestChagers = ucWaitTestChagers.GetInstance();
                    ucWaitTestChagers.SetChargerInfo_Invoke();
                    ucWaitTestChagers.GetChargerInfos();
                    ucCheckData checkData = ucCheckData.GetInstance();
                    checkData.AutoLoadCharger();
                    ucSystemInfo uc = ucSystemInfo.GetInstance();
                    uc.TotalCharger += totalCount;
                    uc.PassCharger += passCount;
                }
                if ((Customer == null || !Customer.Contains("KLQ")) && !isAutoTest)
                {
                    string str = "数据保存成功当前枪信息已删除请重新录入桩信息";
                    string info = LanguageManager.GetByKey(str);
                    ////新乡KLQ要求不弹窗 ，直接保存
                    MessageBox.Show(info);
                }

                this.Invoke(new Action(() =>
                {
                    if (isGroupCharger)
                    {
                        FrmGroupChargerInfo frmAddCharger = FrmGroupChargerInfo.GetInstance();
                        frmAddCharger.SchemeName = SchemeName;
                    }
                    else
                    {
                        FrmChargerInfo frmAddCharger = FrmChargerInfo.GetInstance();
                        frmAddCharger.SchemeName = SchemeName;
                    }
                }));
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                MessageBox.Show(LanguageManager.GetByKey("数据保存失败"));
            }

        }

        private double DataParse(string value)
        {
            UInt32 x = Convert.ToUInt32(value, 16);
            double fy = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
            ////浮点数转16进制
            //byte[] bytes = BitConverter.GetBytes(fy);
            fy = Math.Round(fy, 3);
            return fy;
        }

        private void TsBtnUser_Click(object sender, EventArgs e)
        {
            FrmUserManage frm = FrmUserManage.GetInstance();
            frm.Show();
        }

        private void tsBtnEditScheme_Click(object sender, EventArgs e)
        {
            FrmSchemeEdit frm = FrmSchemeEdit.GetInstance();
            frm.Show();
        }

        private void tsBtnRegist_Click(object sender, EventArgs e)
        {
            //FrmRegister frm = new FrmRegister(LanguageManager.GetByKey("重新注册"));
            //frm.Show();
            UtilLicense m_objLicense = new UtilLicense(DeviceType.ST_ATEFrame);
            bool bRet = m_objLicense.CheckLicense(true, true, true, true); // 默认中文，默认启动校验
            Log.Log.LogMessage($"注册结果返回为{bRet}");
        }
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}

        private void tsBtnCAN_Click(object sender, EventArgs e)
        {
            FrmCAN frm = FrmCAN.GetInstance(_ucChargerInfo.m_LstChargerInfos);
            frm.Show();
        }

        private void tsBtnMESSet_Click(object sender, EventArgs e)
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.Equals("TB"))
            {
                MES.HuiZhou_TB.FrmSetMESPara_TB frm = MES.HuiZhou_TB.FrmSetMESPara_TB.GetInstance();
                frm.Show();
            }
        }

        private void tsBtnAuto_Click(object sender, EventArgs e)
        {
            string info = "开始自动检测，请确实设置参数已保存！";
            if (MessageBox.Show(info, LanguageManager.GetByKey("提示"), MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                return;
            }

            //ShowOverlay();
            FrmAutoTest frmAutoTest = new FrmAutoTest();
            //frmAutoTest.FormClosed += (s, args) => HideOverlay();
            frmAutoTest.Show(this);
        }

        private void SystemEvent_SetSchemeEvent(int schemeIndex)
        {
            FrmSchemeSelect frm = FrmSchemeSelect.GetInstance();
            frm.LoadParams();
            frm.SchemeIndex = schemeIndex;
            frm.ConfirmScheme();
        }

        private void SystemEvent_StartTestEvent()
        {
            StartTest();
        }

        private void SystemEvent_StopTestEvent()
        {
            StopTest();
        }

        private void SystemEvent_ChargerInfoEvent(string barcode, int SchemeIndex)
        {
            FrmChargerInfo frm = FrmChargerInfo.GetInstance();
            frm.SetChargerInfo(barcode, SchemeIndex);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            //string value = comboBox1.Text;

            //LanguageManager.ChangeLanguage(value);
            //SystemEvent.SendChangeLanguage();

            //for (int i = 0; i < ts_Tester.Items.Count; i++)
            //{
            //    if (ts_Tester.Items[i].GetType() == typeof(ToolStripButton))
            //    {
            //        ts_Tester.Items[i].Text = LanguageManager.GetByKey(ts_Tester.Items[i].Name);
            //    }
            //}

            //this.lab_Left2Caption.Text = LanguageManager.GetByKey("lab_Left2Caption");
            //this.lab_Left3Caption.Text = LanguageManager.GetByKey("lab_Left3Caption");
            //this.lblLog.Text = LanguageManager.GetByKey("lblLog");
            //this.label2.Text = LanguageManager.GetByKey("label2");
            //this.lab_MainCaption.Text = LanguageManager.GetByKey("lab_MainCaption");
            //this.lab_Right3Caption.Text = LanguageManager.GetByKey("lab_Right3Caption");

        }
        private void SetLanguage()
        {
            SystemEvent.SendChangeLanguage();

            for (int i = 0; i < ts_Tester.Items.Count; i++)
            {
                if (ts_Tester.Items[i].GetType() == typeof(ToolStripButton))
                {
                    ts_Tester.Items[i].Text = LanguageManager.GetByKey(ts_Tester.Items[i].Name);
                }
            }

            this.lab_Left2Caption.Text = LanguageManager.GetByKey("lab_Left2Caption");
            this.lab_Left3Caption.Text = LanguageManager.GetByKey("lab_Left3Caption");
            this.lblLog.Text = LanguageManager.GetByKey("lblLog");
            this.label2.Text = LanguageManager.GetByKey("label2");
            this.lab_MainCaption.Text = LanguageManager.GetByKey("lab_MainCaption");
            this.lab_Right3Caption.Text = LanguageManager.GetByKey("lab_Right3Caption");
        }
    }
}
