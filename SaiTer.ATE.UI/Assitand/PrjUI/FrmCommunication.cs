using SaiTer.ATE.UI.UserControls;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmCommunication : UIForm
    {
        private ucConnectState _ucConnectState = ucConnectState.GetInstance();

        private FrmCommunication()
        {
            InitializeComponent();
        }
        private static FrmCommunication Instance = null;
        /// <summary>
        /// 通讯设置窗体单例
        /// </summary>
        /// <returns></returns>
        public static FrmCommunication GetInstance()
        {
            if (Instance == null)
                Instance = new FrmCommunication();
            Instance.Activate();
            return Instance;
        }
        #region ---------------------------窗体事件---------------------------
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
                Point mousePos = Control.MousePosition;
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

        private void FrmCommunication_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            this.Dispose();
        }
        private void FrmCommunication_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();
        }

        #endregion

        private void FrmCommunication_Load(object sender, EventArgs e)
        {
            dgvEquip.Rows.Clear();
            
            foreach (var item in _ucConnectState.lstEquipment)
            {
                dgvEquip.Rows.Add();               
                dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[0].Value = item.EquipMentName + item.ChargerID;
                dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[1].Value = item.EquipMentPort.PortName;
                dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[2].Value = item.EquipMentPort.PortParams;
                if (item.EquipMentPort.GetType() == typeof(SaiTer.ATE.PortManage.PortType.SerialPort))
                {
                    SaiTer.ATE.PortManage.PortType.SerialPort sp = item.EquipMentPort as SaiTer.ATE.PortManage.PortType.SerialPort;
                    if (sp._SerialPort.IsOpen)
                    {
                        dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[3].Value = "断开";
                    }
                    else
                    {
                        dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[3].Value = "连接";
                    }
                }
                else if (item.EquipMentPort.GetType() == typeof(SaiTer.ATE.PortManage.PortType.TCPClient))
                {
                    SaiTer.ATE.PortManage.PortType.TCPClient tcp = item.EquipMentPort as SaiTer.ATE.PortManage.PortType.TCPClient;
                    if (tcp.blConntOk)
                    {
                        dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[3].Value = "断开";
                    }
                    else
                    {
                        dgvEquip.Rows[dgvEquip.Rows.Count - 1].Cells[3].Value = "连接";
                    }
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


        private void dgvEquip_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                string str = dgvEquip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (str == "断开")
                {
                    //_ucConnectState.lstEquipment[e.RowIndex].AutoReadData = false;
                    // Thread.Sleep(100);
                    if (_ucConnectState.lstEquipment[e.RowIndex].EquipMentPort is SaiTer.ATE.PortManage.PortType.TCPClient tcpClient)
                    {
                        tcpClient.isCanConnect = false;
                        tcpClient.Close();
                        dgvEquip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "连接";
                    }
                    else if (_ucConnectState.lstEquipment[e.RowIndex].EquipMentPort.Close())
                    {
                        dgvEquip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "连接";
                    }
                }
                else if (str == "连接")
                {
                    // _ucConnectState.lstEquipment[e.RowIndex].AutoReadData = true;
                    //Thread.Sleep(100);
                    if (_ucConnectState.lstEquipment[e.RowIndex].EquipMentPort is SaiTer.ATE.PortManage.PortType.TCPClient tcpClient)
                    {
                        tcpClient.isCanConnect = true;
                        //tcpClient.Open();
                        dgvEquip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "断开";
                    }
                    else if (_ucConnectState.lstEquipment[e.RowIndex].EquipMentPort.Open())
                    {
                        dgvEquip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "断开";
                    }
                }
            }
        }
    }
}
