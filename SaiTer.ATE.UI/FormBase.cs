
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SaiTer.ATE.UI
{
    #region =========== 界面基类 ===========
    /// <summary>
    /// 工位界面基类
    /// </summary>
    public partial class FormBase : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FormBase()
        {
            InitializeComponent();
        }

        #region -----------自定义窗体属性-----------

        private Image formCaptionImage = null;
        /// <summary>
        /// 窗体标题属性
        /// </summary>
        [Browsable(true),
         Description("窗体标题图标")]
        public Image FormCaptionImage
        {
            get
            {
                return formCaptionImage;
            }
            set
            {
                this.pboxTitle.BackgroundImage = value;
                formCaptionImage = value;
            }
        }

        private string workAreaCaption = "工作区域";
        /// <summary>
        /// 中间工作区域标题属性
        /// </summary>
        [Browsable(true),
         Description("中间工作区信息标题属性")]
        public string WorkAreaCaption
        {
            get
            {
                return workAreaCaption;
            }
            set
            {
                this.lab_MainCaption.Text = value;
                workAreaCaption = value;
            }
        }
        /// <summary>
        /// 左侧信息栏宽度属性
        /// </summary>
        [Browsable(true),
         Description("左侧信息栏宽度属性")]
        public int SubExpLeftWidth
        {
            get
            {
                return this.pnl_Left.Width;
            }
            set
            {
                this.pnl_Left.Width = value;
            }
        }
        /// <summary>
        /// 左侧信息栏高度属性
        /// </summary>
        [Browsable(true),
         Description("左侧信息栏高度属性")]
        public int SubExpLeftHeight
        {
            get
            {
                return this.pnl_Left.Height-5;
            }
            set
            {
                this.pnl_Left.Height = value;
            }
        }

        private bool subExpLeft1Visible = true;
        /// <summary>
        /// 左侧第一个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("左边第一个信息栏显示/隐藏属性")]
        public bool SubExpLeft1Visible
        {
            get
            {
                return subExpLeft1Visible;
            }
            set
            {
                this.pnl_Left1.Visible = value;
                subExpLeft1Visible = value;
                this.splitter2.Visible = value;
            }
        }

        private string subExpLeft1Caption = "信息栏";
        /// <summary>
        /// 左侧第一个信息栏标题属性
        /// </summary>
        [Browsable(true),
         Description("左边第一个信息栏标题属性")]
        public string SubExpLeft1Caption
        {
            get
            {
                return subExpLeft1Caption;
            }
            set
            {
                this.lab_Left1Caption.Text = value;
                subExpLeft1Caption = value;
            }
        }

        private int subExpLeft1Height = 153;
        /// <summary>
        /// 左侧第一个信息栏高度属性
        /// </summary>
        [Browsable(true),
         Description("左边第一个信息栏高度属性")]
        public int SubExpLeft1Height
        {
            get
            {
                return subExpLeft1Height;
            }
            set
            {
                this.pnl_Left1.Height = value;
                subExpLeft1Height = value;
            }
        }

        private bool subExpLeft2Visible = true;
        /// <summary>
        /// 左侧第二个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("左边第二个信息栏显示/隐藏属性")]
        public bool SubExpLeft2Visible
        {
            get
            {
                return subExpLeft2Visible;
            }
            set
            {
                this.pnl_Left2.Visible = value;
                subExpLeft2Visible = value;
                this.splitter3.Visible = value;
            }
        }

        private string subExpLeft2Caption = "信息栏";
        /// <summary>
        /// 左侧第二个信息栏标题属性
        /// </summary>
        [Browsable(true),
        Description("左边第二个信息栏标题属性")]
        public string SubExpLeft2Caption
        {
            get
            {
                return subExpLeft2Caption;
            }
            set
            {
                this.lab_Left2Caption.Text = value;
                subExpLeft2Caption = value;
            }
        }

        private int subExpLeft2Height = 153;
        /// <summary>
        /// 左侧第二个信息栏高度属性
        /// </summary>
        [Browsable(true),
        Description("左边第二个信息栏高度属性")]
        public int SubExpLeft2Height
        {
            get
            {
                return subExpLeft2Height;
            }
            set
            {
                this.pnl_Left2.Height = value;
                subExpLeft2Height = value;
            }
        }

        private bool subExpLeft3Visible = true;
        /// <summary>
        /// 左侧第三个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("左边第三个信息栏显示/隐藏属性")]
        public bool SubExpLeft3Visible
        {
            get
            {
                return subExpLeft3Visible;
            }
            set
            {
                this.pnl_Left3.Visible = value;
                subExpLeft3Visible = value;
            }
        }

        private string subExpLeft3Caption = "信息栏";
        /// <summary>
        /// 左侧第三个信息栏标题属性
        /// </summary>
        [Browsable(true),
        Description("左边第三个信息栏标题属性")]
        public string SubExpLeft3Caption
        {
            get
            {
                return subExpLeft3Caption;
            }
            set
            {
                this.lab_Left3Caption.Text = value;
                subExpLeft3Caption = value;
            }
        }



        //-------------------------

        /// <summary>
        /// 右侧信息栏宽度属性
        /// </summary>
        [Browsable(true),
         Description("右侧信息栏宽度属性")]
        public int SubExpRightWidth
        {
            get
            {
                return this.pnl_Right.Width;
            }
            set
            {
                this.pnl_Right.Width = value;
            }
        }
        /// <summary>
        /// 右侧信息栏高度属性
        /// </summary>
        [Browsable(true),
         Description("右侧信息栏高度属性")]
        public int SubExpRightHeight
        {
            get
            {
                return this.pnl_Right.Height - 5;
            }
            set
            {
                this.pnl_Right.Height = value;
            }
        }

        private bool subExpRight1Visible = true;
        /// <summary>
        /// 右侧第一个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("右边第一个信息栏显示/隐藏属性")]
        public bool SubExpRight1Visible
        {
            get
            {
                return subExpRight1Visible;
            }
            set
            {
                this.pnl_Right1.Visible = value;
                subExpRight1Visible = value;
                this.splitter4.Visible = value;
            }
        }

        private string subExpRight1Caption = "信息栏";
        /// <summary>
        /// 右侧第一个信息栏标题属性
        /// </summary>
        [Browsable(true),
         Description("右边第一个信息栏标题属性")]
        public string SubExpRight1Caption
        {
            get
            {
                return subExpRight1Caption;
            }
            set
            {
                this.lab_Right1Caption.Text = value;
                subExpRight1Caption = value;
            }
        }

        private int subExpRight1Height = 153;
        /// <summary>
        /// 右侧第一个信息栏高度属性
        /// </summary>
        [Browsable(true),
         Description("右边第一个信息栏高度属性")]
        public int SubExpRight1Height
        {
            get
            {
                return subExpRight1Height;
            }
            set
            {
                this.pnl_Right1.Height = value;
                subExpRight1Height = value;
            }
        }

        private bool subExpRight2Visible = true;
        /// <summary>
        /// 右侧第二个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("右边第二个信息栏显示/隐藏属性")]
        public bool SubExpRight2Visible
        {
            get
            {
                return subExpRight2Visible;
            }
            set
            {
                this.pnl_Right2.Visible = value;
                subExpRight2Visible = value;
                this.splitter5.Visible = value;
            }
        }

        private string subExpRight2Caption = "信息栏";
        /// <summary>
        /// 右侧第二个信息栏标题属性
        /// </summary>
        [Browsable(true),
        Description("右边第二个信息栏标题属性")]
        public string SubExpRight2Caption
        {
            get
            {
                return subExpRight2Caption;
            }
            set
            {
                this.lab_Right2Caption.Text = value;
                subExpRight2Caption = value;
            }
        }

        private int subExpRight2Height = 153;
        /// <summary>
        /// 右侧第二个信息栏高度属性
        /// </summary>
        [Browsable(true),
        Description("右边第二个信息栏高度属性")]
        public int SubExpRight2Height
        {
            get
            {
                return subExpRight2Height;
            }
            set
            {
                this.pnl_Right2.Height = value;
                subExpRight2Height = value;
            }
        }

        private bool subExpRight3Visible = true;
        /// <summary>
        /// 右侧第三个信息栏显示/隐藏属性
        /// </summary>
        [Browsable(true),
         Description("右边第三个信息栏显示/隐藏属性")]
        public bool SubExpRight3Visible
        {
            get
            {
                return subExpRight3Visible;
            }
            set
            {
                this.pnl_Right3.Visible = value;
                subExpRight3Visible = value;
            }
        }

        private string subExpRight3Caption = "信息栏";
        /// <summary>
        /// 右侧第三个信息栏标题属性
        /// </summary>
        [Browsable(true),
        Description("右边第三个信息栏标题属性")]
        public string SubExpRight3Caption
        {
            get
            {
                return subExpRight3Caption;
            }
            set
            {
                this.lab_Right3Caption.Text = value;
                subExpRight3Caption = value;
            }
        }

        private bool toolsVisible = true;
        /// <summary>
        /// 获取或设置一个值，该值指示工具栏显示/隐藏
        /// </summary>
        [Browsable(true),
         Description("获取或设置一个值，该值指示工具栏显示/隐藏")]
        public bool ToolsVisible
        {
            get
            {
                return toolsVisible;
            }
            set
            {
                this.pnl_Tools.Visible = value;
                toolsVisible = value;
            }
        }


        #endregion

        #region -----------控件触发事件-------------
       


        private void tsbtn_Exit_Click(object sender, EventArgs e)
        {
            ExitClick();
        }
        /// <summary>
        /// 退出按钮事件
        /// </summary>
        protected virtual void ExitClick()
        {
            this.Close();
        }
        private void tsbtn_Exit_MouseEnter(object sender, EventArgs e)
        {
            tsbtn_Exit.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.active;
        }
        private void tsbtn_Exit_MouseLeave(object sender, EventArgs e)
        {
            tsbtn_Exit.BackgroundImage = null;
        }            

        
        private void tsbtn_Min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义   
                const int WS_SYSMENU = 0x00080000;
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX | WS_SYSMENU;   // 允许最小化操作   
                return cp;
            }
        }

        #endregion

        private void lab_MainCaption_Click(object sender, EventArgs e)
        {

        }
    } 
    #endregion
}