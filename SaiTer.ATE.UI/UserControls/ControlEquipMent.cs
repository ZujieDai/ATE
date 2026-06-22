using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.UI.UserControls
{
    public partial class ControlEquipMent : UserControl
    {
        public ControlEquipMent()
        {
            InitializeComponent();

        }
        /// <summary>
        /// 总Id
        /// </summary>
        private int _Id;
        /// <summary>
        /// 总ID
        /// </summary>
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }
        /// <summary>
        /// 坐标
        /// </summary>
        private Dictionary<string, Rectangle> _DitMainLocation;
        /// <summary>
        /// 坐标
        /// </summary>
        public Dictionary<string, Rectangle> DitMainLocation
        {
            get { return _DitMainLocation; }
            set { _DitMainLocation = value; }
        }
        /// <summary>
        /// 标题
        /// </summary>
        private string _ThisText;
        /// <summary>
        /// 标题
        /// </summary>
        public string ThisText
        {
            get { return _ThisText; }
            set
            {
                _ThisText = value;
                this.button1.Text = value;
            }
        }
        /// <summary>
        /// 通讯地址
        /// </summary>
        private string _EquipMentId;
        /// <summary>
        /// 通讯地址
        /// </summary>
        public string EquipMentId
        {
            get { return _EquipMentId; }
            set { _EquipMentId = value; }
        }
        /// <summary>
        /// 设备管控枪位
        /// </summary>
        private List<int> _LstEquipChargerId;
        /// <summary>
        /// 设备管控枪位
        /// </summary>
        public List<int> LstEquipChargerId
        {
            get { return _LstEquipChargerId; }
            set { _LstEquipChargerId = value; }
        }
        /// <summary>
        /// 通讯参数
        /// </summary>
        private string _CommParams;
        /// <summary>
        /// 通讯参数
        /// </summary>
        public string CommParams
        {
            get { return _CommParams; }
            set { _CommParams = value; }
        }
        /// <summary>
        /// 通讯类型
        /// </summary>
        private string _CommType;
        /// <summary>
        /// 通讯类型
        /// </summary>
        public string CommType
        {
            get { return _CommType; }
            set { _CommType = value; }
        }
        /// <summary>
        /// 重复通讯次数
        /// </summary>
        private string _ReCommNum;
        /// <summary>
        /// 重复通讯次数
        /// </summary>
        public string ReCommNum
        {
            get { return _ReCommNum; }
            set { _ReCommNum = value; }
        }
        /// <summary>
        /// 超时时间
        /// </summary>
        private string _OutTime;
        /// <summary>
        /// 超时时间
        /// </summary>
        public string OutTime
        {
            get { return _OutTime; }
            set { _OutTime = value; }
        }
        /// <summary>
        /// 端口名称
        /// </summary>
        private string _ComName;
        /// <summary>
        /// 端口名称
        /// </summary>
        public string ComName
        {
            get { return _ComName; }
            set { _ComName = value; }
        }
        private string _PortNum;
        /// <summary>
        /// 端口号
        /// </summary>
        public string PortNum
        {
            get { return _PortNum; }
            set { _PortNum = value; }
        }
        /// <summary>
        /// 最大区域
        /// </summary>
        private Rectangle _MaxLocation;
        /// <summary>
        /// 最大区域
        /// </summary>
        public Rectangle MaxLocation
        {
            get { return _MaxLocation; }
            set { _MaxLocation = value; }
        }




        private void button1_Click(object sender, EventArgs e)
        {
            this.BringToFront();
        }


        private Point mouseOffset;
        private bool isMouseDown = false;
        private Rectangle NowRectagle;
        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;
            if (e.Button == MouseButtons.Left)
            {
                this.BringToFront();
                xOffset = this.Location.X - System.Windows.Forms.Cursor.Position.X;
                yOffset = this.Location.Y - System.Windows.Forms.Cursor.Position.Y;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
                this.button1.Cursor = Cursors.Hand;
            }
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
                NowRectagle = new Rectangle(this.Location, this.Size);
                foreach (KeyValuePair<string, Rectangle> item in _DitMainLocation)
                {
                    if (NowRectagle.Y >= item.Value.Y && NowRectagle.Y <= (item.Value.Y + item.Value.Height)
                        && NowRectagle.X >= item.Value.X && NowRectagle.X <= (item.Value.X + item.Value.Width))
                    {
                        this.Width = item.Value.Width;
                        this.Height = item.Value.Height;
                        this.Location = item.Value.Location;
                        this.button1.BackColor = Color.SpringGreen;
                        if (item.Key == "COMHS")
                        {
                            this.Dispose();
                            SystemEvent.SendComText(Id, _ComName);
                        }
                        else
                        {
                            _ComName = item.Key;
                            if (!ThisText.Contains("COM"))
                            {
                                if (!ThisText.Contains("TCP") && !ThisText.Contains("."))
                                {
                                    ThisText += "(" + _ComName + ")";
                                }
                            }
                            SystemEvent.SendLocationAndWH(item.Value.X + item.Value.Width, item.Value.Y + item.Value.Height, 0, 0, Id, _ComName);
                        }
                        return;
                    }
                    else
                    {
                        this.button1.BackColor = Color.White;
                        if (ThisText.Contains("(COM") || ThisText.Contains("(TCP") || ThisText.Contains("(."))
                        {
                            ThisText = ThisText.Remove(ThisText.IndexOf("("));

                        }
                    }
                }
                this.button1.Cursor = Cursors.Default;
            }
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);

                if (mousePos.X < _MaxLocation.X)
                {
                    mousePos.X = _MaxLocation.X + 1;
                }
                if (mousePos.Y < _MaxLocation.Y)
                {
                    mousePos.Y = _MaxLocation.Y + 1;
                }
                if (mousePos.X > _MaxLocation.Width - this.Width)
                {
                    mousePos.X = _MaxLocation.Width - this.Width;
                }
                if (mousePos.Y > _MaxLocation.Height - this.Height)
                {
                    mousePos.Y = _MaxLocation.Height - this.Height;
                }
                this.Location = mousePos;
            }
        }


        /// <summary>
        /// 加载控件
        /// </summary>
        public void LoadEquipMent(int index)
        {
            try
            {
                if (_DitMainLocation.ContainsKey(_ComName))
                {
                    this.Location = new Point(_DitMainLocation[_ComName].X + 1, _DitMainLocation[_ComName].Y + 1);
                }
                else if (_ComName.Contains("."))
                {
                    if (_DitMainLocation.ContainsKey("TCP" + index))
                    {
                        this.Location = new Point(_DitMainLocation["TCP" + index].X + 2, _DitMainLocation["TCP" + index].Y + 2);

                    }
                }
                else { return; }
                isMouseDown = false;
                NowRectagle = new Rectangle(this.Location, this.Size);
                foreach (KeyValuePair<string, Rectangle> item in _DitMainLocation)
                {
                    if (NowRectagle.Y >= item.Value.Y && NowRectagle.Y <= (item.Value.Y + item.Value.Height) && NowRectagle.X >= item.Value.X && NowRectagle.X <= (item.Value.X + item.Value.Width))
                    {
                        this.Width = item.Value.Width;
                        this.Height = item.Value.Height;
                        this.Location = item.Value.Location;
                        this.button1.BackColor = Color.SpringGreen;
                        if (item.Key == "COMHS")
                        {
                            this.Dispose();
                            //SystemEvent.SendComText(Id, _ComName, this._ParentControlName);
                        }
                        else
                        {
                            _ComName = item.Key;
                            if (!ThisText.Contains("COM"))
                            {
                                if (!ThisText.Contains("TCP") && !ThisText.Contains("."))
                                {
                                    ThisText += "(" + _ComName + ")";
                                }
                            }

                            //SystemEvent.SendLocationAndWH(item.Value.X + item.Value.Width, item.Value.Y + item.Value.Height, 0, 0, this._ParentControlName, Id, _ComName);
                        }
                        return;
                    }
                    else
                    {
                        this.button1.BackColor = Color.White;
                        if (ThisText != null&&(ThisText.Contains("(COM") || ThisText.Contains("(TCP") || ThisText.Contains("(.")))
                        {
                            ThisText = ThisText.Remove(ThisText.IndexOf("("));

                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
    }
}
