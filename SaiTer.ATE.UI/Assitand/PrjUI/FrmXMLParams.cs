

using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
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
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmXMLParams : Form
    {
        private EquipDescripe equipDescripe = EquipDescripe.GetInstance();

        /// <summary>
        /// 所有格子的区域集合
        /// </summary>
        private Dictionary<string, Rectangle> DitControlLocation;
        /// <summary>
        /// 所有设备集合
        /// </summary>
        public Dictionary<int, ControlEquipMent> DitControlEquipMent;

        /// <summary>
        /// 名字
        /// </summary>
        private string _TextNum;
        /// <summary>
        /// 名字
        /// </summary>
        public string TextNum
        {
            get { return _TextNum; }
            set { _TextNum = value; }
        }

        /// <summary>
        /// 远端端口
        /// </summary>
        private string _RemotePort;
        /// <summary>
        /// 远端端口
        /// </summary>
        public string RemotePort
        {
            get { return _RemotePort; }
            set { _RemotePort = value; }
        }

        private static FrmXMLParams p = null;
        /// <summary>
        /// 配置文件管理单例
        /// </summary>
        /// <returns></returns>
        public static FrmXMLParams GetInstance()
        {
            if (p == null || p.IsDisposed)
            {
                p = new FrmXMLParams();
            }
            p.Activate();
            return p;
        }
        public FrmXMLParams()
        {
            InitializeComponent();
            DitControlEquipMent = new Dictionary<int, ControlEquipMent>();
            DitControlLocation = new Dictionary<string, Rectangle>();
        }
        private void FrmXMLParams_Load(object sender, EventArgs e)
        {
            try
            {
                InitForm();
                LoadParams();
                LoadSystemConfig();
                int index = 1;
                foreach (KeyValuePair<int, ControlEquipMent> item in DitControlEquipMent)
                {
                    item.Value.DitMainLocation = DitControlLocation;
                    item.Value.LoadEquipMent(index);

                    if (item.Value.CommParams.Contains(".") && index < 8)
                    {
                        index++;
                    }
                }
                SetCustomerLogo();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "UI操作异常日志");
            }
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

        /// <summary>
        /// 加载系统配置
        /// </summary>
        private void LoadSystemConfig()
        {
            try
            {
                SysParam sys = SysParam.GetInstance();
                string path = ConfigurationManager.AppSettings["System"].ToString();
                sys.ReadXml(path);
                this.pgSysParams.SelectedObject = sys;
                this.pgSysParams.CollapseAllGridItems();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 保存系统参数配置
        /// </summary>
        private void SaveSystemConfig()
        {
            try
            {
                if (this.pgSysParams.SelectedObject is SysParam)
                {
                    SysParam sys = (SysParam)this.pgSysParams.SelectedObject;
                    sys.SaveXml(ConfigurationManager.AppSettings["System"].ToString(), sys);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 初始化窗体
        /// </summary>
        private void InitForm()
        {
            string[] ArryPort = SerialPort.GetPortNames();
            // Array.Sort(ArryPort);
            StringNumberComparer comparer = new StringNumberComparer();

            ArryPort = ArryPort.OrderBy(s => s, new StringNumberComparer()).ToArray();


            for (int i = 0; i < ArryPort.Length; i++)
            {
                ControlCOMPanle ccp = new ControlCOMPanle();
                ccp.Name = ArryPort[i];
                ccp.Dock = DockStyle.Fill;
                tlpCOM.Controls.Add(ccp);
            }


            COMhs.BackName = "回收站";
            GbxInfo.Visible = false;
            this.CmbComm.SelectedIndex = 0;
            CbxEquipName.SelectedIndex = 0;
            SystemEvent.EventLocationEvent += SystemEvent_EventLocation;
            SystemEvent.ComNameEvent += SystemEvent_ComNameEvent;
            DitControlLocation = new Dictionary<string, Rectangle>();
            foreach (Control ctrl in this.tlpCOM.Controls)
            {
                if (ctrl is ControlCOMPanle)
                {

                    DitControlLocation.Add(ctrl.Name, new Rectangle(new Point(ctrl.Location.X, ctrl.Location.Y + 38 + 40), ctrl.Size));
                }
            }
            foreach (Control ctrl in this.tableLayoutPanel1.Controls)
            {
                if (ctrl is ControlCOMPanle)
                {

                    DitControlLocation.Add(ctrl.Name, new Rectangle(new Point(ctrl.Location.X + 2, ctrl.Location.Y + 38 + 290 + 40), ctrl.Size));
                }
            }
            Point location = new Point();
            location.X = COMhs.Location.X;
            location.Y = COMhs.Location.Y + 43;
            DitControlLocation.Add("COMHS", new Rectangle(location, COMhs.Size));
           
            int index = 1;
            foreach (KeyValuePair<int, ControlEquipMent> item in DitControlEquipMent)
            {
                item.Value.DitMainLocation = DitControlLocation;
                item.Value.LoadEquipMent(index);
                if (item.Value.CommParams.Contains(".") && index < 8)
                {
                    index++;
                }
            }
            GbxInfo.Visible = false;
            this.textBox1.Text = "温馨提示:\r\n请拖动设备到对应的COM口,并设置好对应的详细参数后保存数据！不要使用的设备请拖动至回收站回收处理。\r\n\r\n";
        }

        private void SystemEvent_ComNameEvent(int id, string ComName)
        {
            if (DitControlEquipMent.ContainsKey(id))
            {
                DitControlEquipMent.Remove(id);
            }
        }

        private void SystemEvent_EventLocation(int x, int y, int h, int w, int id, string ComName)
        {
            try
            {

                if (DitControlEquipMent.ContainsKey(id))
                {
                    int indexnum = DitControlEquipMent.Where(s => s.Value.ComName == ComName).Count();
                    if (indexnum > 1)
                    {
                        int TempNum = 0;
                        int TempW = DitControlEquipMent[id].Width;
                        int TempH = DitControlEquipMent[id].Height;
                        int SetW = TempW / indexnum;
                        int TempX = DitControlEquipMent[id].Location.X;
                        int TempY = DitControlEquipMent[id].Location.Y;
                        foreach (KeyValuePair<int, ControlEquipMent> item in DitControlEquipMent)
                        {
                            if (item.Value.ComName == ComName)
                            {
                                item.Value.Width = SetW;
                                item.Value.Location = new Point(TempX + TempNum * SetW, TempY);
                                TempNum++;
                            }
                        }
                    }
                }
                GbxInfo.Visible = true;
                //if (Math.Abs(y - this.tableLayoutPanel1.Height) < GbxInfo.Height && Math.Abs(x - this.Width) < GbxInfo.Width)
                //{
                //    GbxInfo.Location = new Point(x - GbxInfo.Width, y - GbxInfo.Height - COM1.Height);
                //}
                //else if (Math.Abs(y - this.tableLayoutPanel1.Height) < GbxInfo.Height)
                //{
                //    GbxInfo.Location = new Point(x, y - GbxInfo.Height);
                //}
                //else if (Math.Abs(x - this.Width) < GbxInfo.Width)
                //{
                //    GbxInfo.Location = new Point(x - GbxInfo.Width, y);
                //}
                //else
                //{
                //    GbxInfo.Location = new Point(x, y);
                //}
                if (DitControlEquipMent.ContainsKey(id))
                {
                    //波特率
                    if (DitControlEquipMent[id].CommParams != null && DitControlEquipMent[id].CommParams != "")
                    {
                        txtBaudRate.Text = DitControlEquipMent[id].CommParams;
                    }
                    //通讯类型
                    if (DitControlEquipMent[id].CommType != null && DitControlEquipMent[id].CommType != "")
                    {
                        CmbComm.Text = DitControlEquipMent[id].CommType;
                        if (DitControlEquipMent[id].CommType.Contains("SerialPort"))
                        {
                            CmbComm.Text = "COM";
                        }
                    }
                    //设备通信地址
                    if (DitControlEquipMent[id].EquipMentId != null && DitControlEquipMent[id].EquipMentId != "")
                    {
                        txtPortNum.Text = DitControlEquipMent[id].EquipMentId;
                        txtPortNum.Text = DitControlEquipMent[id].PortNum;
                    }
                    //管控枪位号
                    if (DitControlEquipMent[id].LstEquipChargerId != null && DitControlEquipMent[id].LstEquipChargerId.Count > 0)
                    {
                        string Temp = "";
                        for (int i = 0; i < DitControlEquipMent[id].LstEquipChargerId.Count; i++)
                        {
                            if (i == DitControlEquipMent[id].LstEquipChargerId.Count - 1)
                            {
                                Temp += DitControlEquipMent[id].LstEquipChargerId[i].ToString();
                            }
                            else
                            {
                                Temp += DitControlEquipMent[id].LstEquipChargerId[i].ToString() + "|";
                            }
                        }
                        TxtMeterNo.Text = Temp;
                    }
                }
                TempId = id;
                GbxInfo.BringToFront();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "UI操作异常日志");
            }
        }

        /// <summary>
        /// 从配置文件加载控件
        /// </summary>
        private void LoadParams()
        {
            try
            {
                XDocument XEquipDoc = XDocument.Load(ConfigurationManager.AppSettings["EquipMents"].ToString());//设备

                List<int> lstEquipNum = new List<int>();
                List<int> lstItem = new List<int>();
                foreach (XElement EquipItem in XEquipDoc.Descendants("EquipMents").Elements("Equip"))
                {
                    int idTemp = int.Parse(EquipItem.Attribute("ID").Value);
                    if (!lstItem.Contains(idTemp))
                    {
                        lstItem.Add(idTemp);
                    }
                    else
                    {
                        continue;
                    }

                    string EquipMentName = "";
                    equipDescripe.DicEquipDescripe.TryGetValue(EquipItem.Attribute("EquipMentName").Value.ToString(), out EquipMentName); //.GetValueOrDefault(EquipItem.Attribute("EquipMentName").Value, "");//设备名字  
                    string CommParams = EquipItem.Attribute("PortParams").Value;//通讯参数
                    string ReCommNum = EquipItem.Attribute("ReConnNum").Value;//重复通讯次数
                    string OutTime = EquipItem.Attribute("OutTime").Value;//超时时间
                    string ComName = EquipItem.Attribute("PortNum").Value;//端口名
                    string CommType = EquipItem.Attribute("ComType").Value;//端口类型

                    List<int> LstEquipChargerId = new List<int>();
                    string[] strTemp = EquipItem.Attribute("ChargerId").Value.Split('|');
                    for (int i = 0; i < strTemp.Length; i++)
                    {
                        LstEquipChargerId.Add(int.Parse(strTemp[i]));
                    }
                    LoadNewEquipMent(EquipMentName, "1", LstEquipChargerId, CommParams, CommType, ReCommNum, OutTime, ComName);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("配置文件加载出错,请检查配置文件！");
                Log.Log.LogException(ex, "配置文件异常");
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("保存成功后需要重启软件，确定要保存吗？", "操作提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {

                    SaveSystemConfig();

                    XDocument XEquipDoc = new XDocument();
                    XElement XEquipElt = new XElement("EquipMentConfig", null);
                    XElement TempEquipMents = new XElement("EquipMents", null);
                    XElement TempEquip;

                    XElement TempControls = new XElement("Controls", null);

                    int tempindex = 0;

                    List<PortInfo> lstTempPortInfo = new List<PortInfo>();
                    string controlClass = "";

                    #region---保存端口和设备数据配置文件---
                    foreach (KeyValuePair<int, ControlEquipMent> PortItem in DitControlEquipMent)
                    {

                        tempindex++;
                        string TempChargerid = "";
                        for (int i = 0; i < PortItem.Value.LstEquipChargerId.Count; i++)
                        {
                            if (i == PortItem.Value.LstEquipChargerId.Count - 1)
                            {
                                TempChargerid += PortItem.Value.LstEquipChargerId[i].ToString();
                            }
                            else
                            {
                                TempChargerid += PortItem.Value.LstEquipChargerId[i].ToString() + "|";
                            }
                        }



                        string strTempEquipMentName = "";

                        string[] ArrTempName = PortItem.Value.ThisText.Split('(');
                        //string ComType = "";
                        string PortNum = ArrTempName[1].TrimEnd(')');
                        string CommParams = PortItem.Value.CommParams;
                        if (PortNum.Contains("TCP"))
                        {
                            PortNum = PortItem.Value.CommParams;//这里应该是IP地址
                            CommParams = PortItem.Value.PortNum;//这里是端口号                                                   
                        }
                       
                        string TempEquipMentName = "";
                        if (ArrTempName.Length > 0)
                        {
                            strTempEquipMentName = ArrTempName[0];
                            TempEquipMentName = equipDescripe.DicEquipDescripe.FirstOrDefault(s => s.Value == strTempEquipMentName).Key;
                            //某种设备下面有多种类型的情况，  如交流BMS ，直流BMS，控制类统一为BMS
                            //单一类别的设备不需要添加以下判断
                            if (strTempEquipMentName.Contains("BMS"))
                            {
                                strTempEquipMentName = "BMS";
                            }
                            else if (strTempEquipMentName.Contains("交流源"))
                            {
                                strTempEquipMentName = "交流源";
                            }
                            else if (strTempEquipMentName.Contains("示波器"))
                            {
                                strTempEquipMentName = "示波器";
                            }
                            else if (strTempEquipMentName.Contains("剩余电流保护测试仪"))
                            {
                                strTempEquipMentName = "剩余电流保护测试仪";
                            }
                            else if (strTempEquipMentName.Contains("电阻负载"))
                            {
                                strTempEquipMentName = "电阻负载";
                            }
                         
                            if (!controlClass.Contains(strTempEquipMentName))
                            {
                                controlClass += strTempEquipMentName + "|";
                            }

                        }


                        TempEquip = new XElement("Equip",
                           new XAttribute("ID", tempindex),
                           new XAttribute("ChargerId", TempChargerid),
                           new XAttribute("EquipMentName", TempEquipMentName),
                           new XAttribute("ComType", PortItem.Value.CommType),
                           new XAttribute("PortNum", PortNum),
                           new XAttribute("PortParams", CommParams),
                           new XAttribute("OutTime", PortItem.Value.OutTime),
                           new XAttribute("ReConnNum", PortItem.Value.ReCommNum));

                        TempEquipMents.Add(TempEquip);
                    }

                    #endregion
                    XEquipElt.Add(TempEquipMents);
                    XElement tempControl = new XElement("Control", new XAttribute("ControlClass", controlClass.TrimEnd('|')));
                    TempControls.Add(tempControl);
                    XEquipElt.Add(TempControls);

                    XEquipDoc.Add(XEquipElt);



                    //保存配置文件

                    XEquipDoc.Save("XML\\EquipMentManage.xml");
                    XmlInfoAndAssembly xmlInfoAndAssembly = XmlInfoAndAssembly.GetInstance();
                    //this.Close();
                    //// 重启程序，目前有点问题
                    System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();

                    for (int i = 0; i < processes.Length; i++)
                    {
                        if (String.Compare(processes[i].ProcessName, "SaiTer充电桩机测试系统", true) == 0)
                        {
                            System.Diagnostics.ProcessStartInfo p = new ProcessStartInfo();
                            p.FileName = "SaiTer充电桩机测试系统.exe";
                            p.WorkingDirectory = System.Windows.Forms.Application.StartupPath;//设置此外部程序所在windows目录、
                            processes[i].CloseMainWindow();

                            Application.Restart();
                            // System.Diagnostics.Process Proc = System.Diagnostics.Process.Start(p);//调用外部程序
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("配置文件保存出错");
                Log.Log.LogException(ex, "配置文件保存出错");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
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
        private void lblTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }
        private void lblTitle_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }
        int Index = 0;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            ControlEquipMent c = new ControlEquipMent();
            c.ThisText = CbxEquipName.Text;
            c.DitMainLocation = DitControlLocation;
            c.Parent = this;
            c.MaxLocation = new Rectangle(new Point(0, 38 + 40), this.Size);
            c.Show();
            this.Controls.Add(c);
            c.Location = new Point(this.CbxEquipName.Location.X, this.CbxEquipName.Location.Y + 70 + 40);
            c.BringToFront();
            Index++;
            c.Id = Index;
            if (!DitControlEquipMent.ContainsKey(Index))
            {
                DitControlEquipMent.Add(Index, c);
            }
        }
        int TempId = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            GbxInfo.Visible = false;
            if (DitControlEquipMent.ContainsKey(TempId))
            {
                DitControlEquipMent[TempId].ReCommNum = "1";
                DitControlEquipMent[TempId].OutTime = "300";
                DitControlEquipMent[TempId].EquipMentId = txtPortNum.Text;
                if (CmbComm.Text == "Com")
                {
                    DitControlEquipMent[TempId].CommType = "SerialPort";
                    DitControlEquipMent[TempId].CommParams = txtBaudRate.Text;
                }
                else if (CmbComm.Text == "TcpClient")
                {
                    DitControlEquipMent[TempId].CommType = CmbComm.Text;
                    DitControlEquipMent[TempId].CommParams = txtBaudRate.Text;
                    DitControlEquipMent[TempId].ComName = txtPortNum.Text.Trim();
                }
                else
                {
                    DitControlEquipMent[TempId].CommType = CmbComm.Text;
                }

                if (TxtMeterNo.Text != "")
                {
                    DitControlEquipMent[TempId].LstEquipChargerId = new List<int>();
                    string[] strTemp = TxtMeterNo.Text.Split('|');
                    for (int i = 0; i < strTemp.Length; i++)
                    {
                        DitControlEquipMent[TempId].LstEquipChargerId.Add(int.Parse(strTemp[i]));
                    }
                }
            }
        }
        /// <summary>
        /// 加载设备
        /// </summary>
        /// <param name="EquipMentName">设备名</param>
        /// <param name="Adrress">通讯地址</param>
        /// <param name="LstEquipChargerId">设备管理枪位号</param>
        /// <param name="CommParams">通讯参数</param>
        /// <param name="CommType">通讯类型</param>
        /// <param name="ReCommNum">重复通讯次数</param>
        /// <param name="OutTime">超时时间</param>
        /// <param name="ComName">端口名</param>
        public void LoadNewEquipMent(string EquipMentName, string Adrress, List<int> LstEquipChargerId, string CommParams, string CommType, string ReCommNum, string OutTime, string ComName)
        {
            ControlEquipMent c = new ControlEquipMent();
            c.ThisText = EquipMentName;
            c.DitMainLocation = DitControlLocation;
            c.Parent = this;
            c.MaxLocation = new Rectangle(new Point(0, 38 + 40), this.Size);
            c.EquipMentId = Adrress;
            c.LstEquipChargerId = LstEquipChargerId;
            c.CommType = CommType;
            c.CommParams = CommParams;
            if (CommType == "TcpClient")
            {
                c.CommParams = ComName;
                c.PortNum = CommParams;
            }

            c.ReCommNum = ReCommNum;
            c.OutTime = OutTime;
            c.ComName = ComName;

            c.Show();
            this.Controls.Add(c);
            c.Location = new Point(this.CbxEquipName.Location.X, this.CbxEquipName.Location.Y + 70 + 40);
            c.BringToFront();
            Index++;
            c.Id = Index;
            if (!DitControlEquipMent.ContainsKey(Index))
            {
                DitControlEquipMent.Add(Index, c);
            }
            //c.LoadEquipMent();
        }

        private void CmbComm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbComm.Text == "TcpClient")
            {
                txtPortNum.Visible = true;
                label1.Visible = true;
            }
            else
            {
                txtPortNum.Visible = false;
                label1.Visible = false;
            }
        }

        private void uiTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            foreach (var item in this.Controls)
            {
                if (item.GetType() == typeof(ControlEquipMent))
                {
                    ControlEquipMent c = item as ControlEquipMent;
                    if (uiTabControl1.SelectedIndex == 0)
                    {
                        c.Visible = true;
                    }
                    else
                    {
                        c.Visible = false;
                    }
                }
            }

        }
    }
    public struct PortInfo
    {
        public string PortName;
        public string PortType;
    }
    public class StringNumberComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            x = x.RemoveLeft(3);
            y = y.RemoveLeft(3);
            bool xIsNum = int.TryParse(x, out int xNum);
            bool yIsNum = int.TryParse(y, out int yNum);
            if (xIsNum && yIsNum)
            {
                return xNum > yNum ? 1 : -1;
            }
            else
            {
                return string.CompareOrdinal(x, y);
            }

        }
    }

}