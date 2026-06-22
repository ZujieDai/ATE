namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmTrialParams
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTrialParams));
            this.pnl_Top = new System.Windows.Forms.Panel();
            this.pbx_Top_Right = new System.Windows.Forms.PictureBox();
            this.pbox_Top_Mid = new System.Windows.Forms.PictureBox();
            this.lbl_Title = new System.Windows.Forms.Label();
            this.pbox_Top_Left = new System.Windows.Forms.PictureBox();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.rtxtTrialMethod = new System.Windows.Forms.RichTextBox();
            this.uiGroupBox2 = new Sunny.UI.UIGroupBox();
            this.rtxtDecideStandard = new System.Windows.Forms.RichTextBox();
            this.uiGroupBox3 = new Sunny.UI.UIGroupBox();
            this.fPnlParams = new System.Windows.Forms.TableLayoutPanel();
            this.btnOK = new Sunny.UI.UIButton();
            this.btnCancel = new Sunny.UI.UIButton();
            this.uiPanel1 = new Sunny.UI.UIPanel();
            this.pnl_Top.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbx_Top_Right)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Top_Mid)).BeginInit();
            this.pbox_Top_Mid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Top_Left)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.uiGroupBox2.SuspendLayout();
            this.uiGroupBox3.SuspendLayout();
            this.uiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl_Top
            // 
            this.pnl_Top.Controls.Add(this.pbx_Top_Right);
            this.pnl_Top.Controls.Add(this.pbox_Top_Mid);
            this.pnl_Top.Controls.Add(this.pbox_Top_Left);
            this.pnl_Top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_Top.Location = new System.Drawing.Point(0, 0);
            this.pnl_Top.Margin = new System.Windows.Forms.Padding(2);
            this.pnl_Top.Name = "pnl_Top";
            this.pnl_Top.Size = new System.Drawing.Size(1013, 48);
            this.pnl_Top.TabIndex = 1;
            // 
            // pbx_Top_Right
            // 
            this.pbx_Top_Right.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.pictureBox3_BackgroundImage;
            this.pbx_Top_Right.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbx_Top_Right.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbx_Top_Right.Location = new System.Drawing.Point(949, 0);
            this.pbx_Top_Right.Margin = new System.Windows.Forms.Padding(2);
            this.pbx_Top_Right.Name = "pbx_Top_Right";
            this.pbx_Top_Right.Size = new System.Drawing.Size(64, 48);
            this.pbx_Top_Right.TabIndex = 2;
            this.pbx_Top_Right.TabStop = false;
            // 
            // pbox_Top_Mid
            // 
            this.pbox_Top_Mid.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.pictureBox27_BackgroundImage;
            this.pbox_Top_Mid.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbox_Top_Mid.Controls.Add(this.lbl_Title);
            this.pbox_Top_Mid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbox_Top_Mid.Location = new System.Drawing.Point(6, 0);
            this.pbox_Top_Mid.Margin = new System.Windows.Forms.Padding(2);
            this.pbox_Top_Mid.Name = "pbox_Top_Mid";
            this.pbox_Top_Mid.Size = new System.Drawing.Size(1007, 48);
            this.pbox_Top_Mid.TabIndex = 1;
            this.pbox_Top_Mid.TabStop = false;
            // 
            // lbl_Title
            // 
            this.lbl_Title.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_Title.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Title.ForeColor = System.Drawing.Color.White;
            this.lbl_Title.Location = new System.Drawing.Point(0, 0);
            this.lbl_Title.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Title.Name = "lbl_Title";
            this.lbl_Title.Size = new System.Drawing.Size(1007, 48);
            this.lbl_Title.TabIndex = 3;
            this.lbl_Title.Text = "试验项参数";
            this.lbl_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblTitle_MouseDown);
            this.lbl_Title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblTitle_MouseMove);
            this.lbl_Title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblTitle_MouseUp);
            // 
            // pbox_Top_Left
            // 
            this.pbox_Top_Left.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.pictureBox29_BackgroundImage;
            this.pbox_Top_Left.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbox_Top_Left.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbox_Top_Left.Location = new System.Drawing.Point(0, 0);
            this.pbox_Top_Left.Margin = new System.Windows.Forms.Padding(2);
            this.pbox_Top_Left.Name = "pbox_Top_Left";
            this.pbox_Top_Left.Size = new System.Drawing.Size(6, 48);
            this.pbox_Top_Left.TabIndex = 0;
            this.pbox_Top_Left.TabStop = false;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.rtxtTrialMethod);
            this.uiGroupBox1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(17, 19);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(10, 30, 10, 10);
            this.uiGroupBox1.Size = new System.Drawing.Size(963, 177);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 2;
            this.uiGroupBox1.Text = "测试方法和要求";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rtxtTrialMethod
            // 
            this.rtxtTrialMethod.BackColor = System.Drawing.Color.White;
            this.rtxtTrialMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtTrialMethod.Location = new System.Drawing.Point(10, 30);
            this.rtxtTrialMethod.Name = "rtxtTrialMethod";
            this.rtxtTrialMethod.ReadOnly = true;
            this.rtxtTrialMethod.Size = new System.Drawing.Size(943, 137);
            this.rtxtTrialMethod.TabIndex = 0;
            this.rtxtTrialMethod.Text = "";
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.rtxtDecideStandard);
            this.uiGroupBox2.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox2.IsScaled = false;
            this.uiGroupBox2.Location = new System.Drawing.Point(17, 206);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(10, 30, 10, 10);
            this.uiGroupBox2.Size = new System.Drawing.Size(963, 137);
            this.uiGroupBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox2.TabIndex = 3;
            this.uiGroupBox2.Text = "判定方法";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rtxtDecideStandard
            // 
            this.rtxtDecideStandard.BackColor = System.Drawing.Color.White;
            this.rtxtDecideStandard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtDecideStandard.Location = new System.Drawing.Point(10, 30);
            this.rtxtDecideStandard.Name = "rtxtDecideStandard";
            this.rtxtDecideStandard.ReadOnly = true;
            this.rtxtDecideStandard.Size = new System.Drawing.Size(943, 97);
            this.rtxtDecideStandard.TabIndex = 1;
            this.rtxtDecideStandard.Text = "";
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.Controls.Add(this.fPnlParams);
            this.uiGroupBox3.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox3.IsScaled = false;
            this.uiGroupBox3.Location = new System.Drawing.Point(17, 353);
            this.uiGroupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Padding = new System.Windows.Forms.Padding(10, 30, 10, 10);
            this.uiGroupBox3.Size = new System.Drawing.Size(963, 342);
            this.uiGroupBox3.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox3.TabIndex = 4;
            this.uiGroupBox3.Text = "参数设置";
            this.uiGroupBox3.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // fPnlParams
            // 
            this.fPnlParams.ColumnCount = 4;
            this.fPnlParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.fPnlParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.fPnlParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.fPnlParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.fPnlParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fPnlParams.Location = new System.Drawing.Point(10, 30);
            this.fPnlParams.Name = "fPnlParams";
            this.fPnlParams.RowCount = 8;
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.fPnlParams.Size = new System.Drawing.Size(943, 302);
            this.fPnlParams.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOK.IsScaled = false;
            this.btnOK.Location = new System.Drawing.Point(280, 703);
            this.btnOK.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(105, 44);
            this.btnOK.Style = Sunny.UI.UIStyle.Custom;
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "修改参数";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCancel.IsScaled = false;
            this.btnCancel.Location = new System.Drawing.Point(564, 703);
            this.btnCancel.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 44);
            this.btnCancel.Style = Sunny.UI.UIStyle.Custom;
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "关闭";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // uiPanel1
            // 
            this.uiPanel1.Controls.Add(this.uiGroupBox3);
            this.uiPanel1.Controls.Add(this.btnCancel);
            this.uiPanel1.Controls.Add(this.uiGroupBox2);
            this.uiPanel1.Controls.Add(this.btnOK);
            this.uiPanel1.Controls.Add(this.uiGroupBox1);
            this.uiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel1.IsScaled = false;
            this.uiPanel1.Location = new System.Drawing.Point(0, 48);
            this.uiPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel1.Name = "uiPanel1";
            this.uiPanel1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiPanel1.Size = new System.Drawing.Size(1013, 780);
            this.uiPanel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiPanel1.TabIndex = 7;
            this.uiPanel1.Text = null;
            this.uiPanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmTrialParams
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1013, 828);
            this.Controls.Add(this.uiPanel1);
            this.Controls.Add(this.pnl_Top);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1001, 815);
            this.Name = "FrmTrialParams";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.pnl_Top.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbx_Top_Right)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Top_Mid)).EndInit();
            this.pbox_Top_Mid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Top_Left)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox3.ResumeLayout(false);
            this.uiPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_Top;
        private System.Windows.Forms.PictureBox pbx_Top_Right;
        private System.Windows.Forms.PictureBox pbox_Top_Mid;
        private System.Windows.Forms.PictureBox pbox_Top_Left;
        public System.Windows.Forms.Label lbl_Title;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UIGroupBox uiGroupBox3;
        private System.Windows.Forms.RichTextBox rtxtTrialMethod;
        private System.Windows.Forms.RichTextBox rtxtDecideStandard;
        private Sunny.UI.UIButton btnOK;
        private Sunny.UI.UIButton btnCancel;
        private System.Windows.Forms.TableLayoutPanel fPnlParams;
        private Sunny.UI.UIPanel uiPanel1;
    }
}