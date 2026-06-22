using NPOI.POIFS.Crypt.Dsig;
using Org.BouncyCastle.Bcpg;
using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.InterFace;
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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static SaiTer.ATE.DataModel.Consist;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmCAN : UIForm
    {
        private int rowCount = 0;
        private int order = 1;
        private bool isPause = false;
        private bool bmsIsOn = false;
        public ControlsListManager EquipMentControl = XmlInfoAndAssembly.GetInstance()._EquipMentControl;
        public List<int> lstChargerID = new List<int>();
        /// <summary>
        /// 该设备对应的充电枪编号
        /// </summary>
        public int SelectedChargerID { get; set; }
        public string SelectedEquipMentName { get; set; }
        private List<CANPacket> packets = new List<CANPacket>();
        Thread threadLoadEvent = null;


        #region 双缓冲
        private void DoubleBuffer()
        {
            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            dgvPacket.DoubleBuffered(true);
        }
        #endregion 双缓冲

        private FrmCAN()
        {
            InitializeComponent();
            DoubleBuffer();
            //AppendData();
        }

        private void FrmCAN_Load(object sender, EventArgs e)
        {
            //EquipMentControl.BMS.BMS_DC_SetControl(new List<int> { SelectedChargerID }, 0x50, false);
            switch_CAN.Active = false;

            //加载下拉框
            if(EquipMentControl.BMS != null)
            {
                var dicBMS = EquipMentControl.BMS.DitEquipMentBase.Values.Where(b => b.EquipMentName.Contains("BMS"));
                foreach (var bms in dicBMS)
                {
                    if(bms is emtBMS_GB_DC)
                    {
                        cmbBMS.Items.Add($"{bms.EquipMentName}_{bms.ChargerID}");
                        if (lstChargerID == null || lstChargerID.Count == 0)
                        {
                            cmbBMS.SelectedIndex = 0;
                        }
                        else
                        {
                            if (bms.ChargerID == lstChargerID[0])
                            {
                                cmbBMS.SelectedIndex = cmbBMS.Items.Count - 1;
                            }
                        }
                    }
                    if (bms is emtBMS_EU_DC)
                    {
                        cmbBMS.Items.Add($"{bms.EquipMentName}_{bms.ChargerID}");
                    }
                    if (bms is emtBMS_USA_DC)
                    {
                        cmbBMS.Items.Add($"{bms.EquipMentName}_{bms.ChargerID}");
                    }
                }
            }
            SetCustomerLogo();

            //threadLoadEvent = new Thread(LoadEvent);
            //threadLoadEvent.Start();
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

        public void LoadEvent()
        {
            Thread.Sleep(1000);//延时等待窗体加载完成
            SystemEvent.RevCANPacketEvent += SystemEventRevCANPacket;
            SystemEvent.RevEUMsgEvent += SystemEventRevEUMsg;
        }

        private void SystemEventRevCANPacket(Dictionary<int, CanMsgRich> dicPacket)
        {
            try
            {
                //if (isPause)
                //{
                //    return;
                //}
                foreach (var DATA in dicPacket)
                {
                    var data = DATA.Value;
                    if (data != null)
                    {
                        CANPacket packet = new CANPacket()
                        {
                            Mark = "读取",
                            ChargeID = DATA.Key,
                            RecvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"),
                            TimeIncrement = data.TimeIncrement,
                            FrameID = data.Id,
                            DLC = data.Dlc.ToString(),
                            Data = data.MsgData,
                            Explain = data.MsgText,
                        };
                        if (packet != null && !string.IsNullOrEmpty(packet.Data))
                        {
                            packet.Order = order;

                            if (this.InvokeRequired)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    AddDataToUI(packet);
                                });
                            }
                            else
                            {
                                AddDataToUI(packet);
                            }
                                                   
                            order++;
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void SystemEventRevEUMsg(Dictionary<int, string> dicMsg)
        {
            foreach(int item in dicMsg.Keys)
            {
                if (string.IsNullOrEmpty(cmbBMS.SelectedText) || string.IsNullOrEmpty(dicMsg[item]) || item != SelectedChargerID)
                    continue;

                AddDataToUI(dicMsg[item]);
            }
        }

        private void AddDataToUI(CANPacket packet)
        {
            try
            {
                if (cmbBMS.SelectedIndex < 0 || packet.ChargeID != SelectedChargerID)
                    return;

                //数据量过大不清理会导致软件卡死
                if (rowCount > 20000)
                {
                    order = 1;
                    dgvPacket.Rows.Clear();
                    rowCount = 0;
                }
                rowCount++;

                packets.Add(packet);
                dgvPacket.Rows.Add(1);
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[0].Value = packet.Order;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[1].Value = packet.ChargeID;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[2].Value = packet.RecvTime;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[3].Value = packet.TimeIncrement;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[4].Value = packet.FrameID;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[5].Value = packet.DLC;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[6].Value = packet.Data;
                dgvPacket.Rows[dgvPacket.Rows.Count - 1].Cells[7].Value = packet.Explain;

                dgvPacket.FirstDisplayedScrollingRowIndex = dgvPacket.Rows.Count - 1;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void AddDataToUI(string EU_Msg)
        {
            if(EU_Msg.IndexOf("STATE_PEV_SLAC") > -1)
            {
                Log.Log.LogMessage($"检索到STATE_PEV_SLAC发送dbglevel 3，内容为：{EU_Msg}");
                EquipMentControl.BMS.BMSSendEUMsg(lstChargerID, "dbglevel 3", new string[] { "emtBMS_EU_DC_Msg" });
            }
            // 获取窗体实例并在主线程上执行显示操作
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    txtLogEU.Text += EU_Msg;
                    txtLogEU.SelectionStart = txtLogEU.Text.Length;
                    txtLogEU.SelectionLength = 0;
                }));
            }
        }


        private static FrmCAN Instance = null;
        /// <summary>
        /// 设备操作窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmCAN GetInstance(List<int> _lstChargerID)
        {
            if (Instance == null)
                Instance = new FrmCAN();
            Instance.lstChargerID = _lstChargerID;
            Instance.Activate();
            return Instance;
        }


        private void switch_CAN_ValueChanged(object sender, bool value)
        {
            bmsIsOn = value;
            if (bmsIsOn)
                EquipMentControl.BMS.BMSProtocolConsistency(new List<int> { SelectedChargerID }, 0, 0, 0, 0, 0, 0, 0, 0);
            EquipMentControl.BMS.BMS_DC_SetControl(new List<int> { SelectedChargerID }, 0x50, value);
            if (threadLoadEvent == null)
            {
                threadLoadEvent = new Thread(LoadEvent);
                threadLoadEvent.Start();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            isPause = !isPause;
            if (isPause)
            {
                btnPause.Text = "继续显示";
                SystemEvent.RevCANPacketEvent -= SystemEventRevCANPacket;
                SystemEvent.RevEUMsgEvent -= SystemEventRevEUMsg;
            }
            else
            {
                btnPause.Text = "暂停显示";
                SystemEvent.RevCANPacketEvent += SystemEventRevCANPacket;
                SystemEvent.RevEUMsgEvent += SystemEventRevEUMsg;
            }
        }

        private void FrmCAN_FormClosing(object sender, FormClosingEventArgs e)
        {
            isPause = true;
            btnPause.Text = "继续显示";
            SystemEvent.RevCANPacketEvent -= SystemEventRevCANPacket;
            SystemEvent.RevEUMsgEvent -= SystemEventRevEUMsg;
            EquipMentControl.BMS.BMS_DC_SetControl(new List<int> { SelectedChargerID }, 0x50, false);
            order = 1;
            dgvPacket.Rows.Clear();
        }

        private void FrmCAN_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance.Dispose();
            Instance = null;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (SelectedEquipMentName == "GB_BMS")
            {
                order = 1;
                dgvPacket.Rows.Clear();
                packets = new List<CANPacket>();
            }
            else if (SelectedEquipMentName == "EU_BMS")
            {
                txtLogEU.Text = "";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbBMS.SelectedText.Contains("国标"))
            {
                string fileName = $"GBMessage_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                try
                {
                    if (packets.Count > 0)
                    {
                        // 创建并配置保存文件对话框
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            // 设置默认文件名
                            saveFileDialog.FileName = $"{fileName}.xls";

                            // 设置默认文件过滤器
                            saveFileDialog.Filter = "Text files (*.xls)|*.xls|All files (*.*)|*.*";

                            // 设置默认文件类型
                            saveFileDialog.DefaultExt = "*.xls";

                            // 显示对话框
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                // 获取用户选择的完整文件路径
                                string filePath = saveFileDialog.FileName;

                                Queue<string> Q = new Queue<string>();
                                Q.Enqueue("帧序号");
                                Q.Enqueue("枪号");
                                Q.Enqueue("接收时间");
                                Q.Enqueue("时间增量");
                                Q.Enqueue("帧ID");
                                Q.Enqueue("帧长度DLC");
                                Q.Enqueue("有效数据");
                                Q.Enqueue("报文翻译");
                                int index = 0;
                                NPIOUtil.CreateExcelFile(filePath, SelectedChargerID, Q);
                                NpioOperation.ExcelWR.OpenExceWorkBook(NpioOperation.ExcelSaveFileName, NpioOperation.ExcelSaveSheetName);
                                for (int i = 0; i < packets.Count; i++)
                                {
                                    index++;
                                    var DATA = packets[i];
                                    NpioOperation.ExcelWR.WriteExceWorkBook(++NpioOperation.RowNum, index.ToString(),
                                        SelectedChargerID.ToString(), DATA.RecvTime,
                                        DATA.TimeIncrement, DATA.FrameID.ToString(), DATA.DLC.ToString(), DATA.Data, DATA.Explain);
                                }
                                NpioOperation.ExcelWR.CloseExceWorkBook(NpioOperation.ExcelSaveFileName);
                                UIMessageTip.ShowOk("保存成功！");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    UIMessageTip.ShowError("保存失败！");
                }
            }
            else if (cmbBMS.SelectedText.Contains("欧标"))
            {
                string fileName = $"EUMessage_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                if(FileHelper.SaveContent(fileName, txtLogEU.Text.TrimEnd()))
                {
                    UIMessageTip.ShowOk("保存成功！");
                }
                else
                {
                    UIMessageTip.ShowError("保存失败！");
                }
            }
            else if (cmbBMS.SelectedText.Contains("美标"))
            {
                string fileName = $"USAMessage_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                if (FileHelper.SaveContent(fileName, txtLogEU.Text.TrimEnd()))
                {
                    UIMessageTip.ShowOk("保存成功！");
                }
                else
                {
                    UIMessageTip.ShowError("保存失败！");
                }
            }
        }

        private void cmbBMS_SelectedIndexChanged(object sender, EventArgs e)
        {
            order = 1;
            dgvPacket.Rows.Clear();
            txtLogEU.Text = "";
            if (cmbBMS.SelectedText.Contains("国标"))
            {
                //if (SelectedChargerID > 0)
                //    EquipMentControl.BMS.BMS_DC_SetControl(new List<int> { SelectedChargerID }, 0x50, false);
                txtLogEU.Visible = false;
                dgvPacket.Visible = true;
                uiLabel3.Visible = true;
                switch_CAN.Visible = true;
                btnDbglevel3.Visible = false;
                SelectedChargerID = Convert.ToInt32(cmbBMS.SelectedText.Split('_')[1]);
                SelectedEquipMentName = "GB_BMS";
                //EquipMentControl.BMS.BMS_DC_SetControl(new List<int> { SelectedChargerID }, 0x50, true);
                //switch_CAN.Active = true;
            }
            else if (cmbBMS.SelectedText.Contains("欧标"))
            {
                txtLogEU.Visible = true;
                dgvPacket.Visible = false;
                uiLabel3.Visible = false;
                switch_CAN.Visible = false;
                btnDbglevel3.Visible = true;
                SelectedChargerID = Convert.ToInt32(cmbBMS.SelectedText.Split('_')[1]);
                SelectedEquipMentName = "EU_BMS";
            }
            else if (cmbBMS.SelectedText.Contains("美标"))
            {
                txtLogEU.Visible = true;
                dgvPacket.Visible = false;
                uiLabel3.Visible = false;
                switch_CAN.Visible = false;
                btnDbglevel3.Visible = true;
                SelectedChargerID = Convert.ToInt32(cmbBMS.SelectedText.Split('_')[1]);
                SelectedEquipMentName = "USA_BMS";
            }
        }

        private void btnDbglevel3_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMSSendEUMsg(lstChargerID, "dbglevel 3", new string[] { "emtBMS_EU_DC_Msg" });
        }
    }
}
