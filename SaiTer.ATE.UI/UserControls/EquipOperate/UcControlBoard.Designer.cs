namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcControlBoard
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.lblText = new Sunny.UI.UILabel();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.btnRead = new Sunny.UI.UIButton();
            this.pnlRelay = new Sunny.UI.UIPanel();
            this.chbS_1 = new Sunny.UI.UICheckBox();
            this.chbS_16 = new Sunny.UI.UICheckBox();
            this.chbS_2 = new Sunny.UI.UICheckBox();
            this.chbS_15 = new Sunny.UI.UICheckBox();
            this.chbS_3 = new Sunny.UI.UICheckBox();
            this.chbS_14 = new Sunny.UI.UICheckBox();
            this.chbS_4 = new Sunny.UI.UICheckBox();
            this.chbS_13 = new Sunny.UI.UICheckBox();
            this.chbS_5 = new Sunny.UI.UICheckBox();
            this.chbS_12 = new Sunny.UI.UICheckBox();
            this.chbS_6 = new Sunny.UI.UICheckBox();
            this.chbS_11 = new Sunny.UI.UICheckBox();
            this.chbS_7 = new Sunny.UI.UICheckBox();
            this.chbS_10 = new Sunny.UI.UICheckBox();
            this.chbS_8 = new Sunny.UI.UICheckBox();
            this.chbS_9 = new Sunny.UI.UICheckBox();
            this.lblInfo = new Sunny.UI.UILabel();
            this.uiGroupBox1.SuspendLayout();
            this.pnlRelay.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.lblText);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Controls.Add(this.btnRead);
            this.uiGroupBox1.Controls.Add(this.pnlRelay);
            this.uiGroupBox1.Controls.Add(this.lblInfo);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(10, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox1.Size = new System.Drawing.Size(720, 530);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "继电器开关控制";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblText
            // 
            this.lblText.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblText.IsScaled = false;
            this.lblText.Location = new System.Drawing.Point(19, 219);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(643, 239);
            this.lblText.Style = Sunny.UI.UIStyle.Custom;
            this.lblText.TabIndex = 23;
            this.lblText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(19, 469);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(429, 36);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 22;
            this.uiLabel1.Text = "勾选闭合，否则断开\r\n\r\n";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRead
            // 
            this.btnRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnRead.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRead.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRead.IsScaled = false;
            this.btnRead.Location = new System.Drawing.Point(479, 178);
            this.btnRead.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(149, 38);
            this.btnRead.Style = Sunny.UI.UIStyle.Custom;
            this.btnRead.TabIndex = 21;
            this.btnRead.Text = "读继电器状态";
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // pnlRelay
            // 
            this.pnlRelay.Controls.Add(this.chbS_1);
            this.pnlRelay.Controls.Add(this.chbS_16);
            this.pnlRelay.Controls.Add(this.chbS_2);
            this.pnlRelay.Controls.Add(this.chbS_15);
            this.pnlRelay.Controls.Add(this.chbS_3);
            this.pnlRelay.Controls.Add(this.chbS_14);
            this.pnlRelay.Controls.Add(this.chbS_4);
            this.pnlRelay.Controls.Add(this.chbS_13);
            this.pnlRelay.Controls.Add(this.chbS_5);
            this.pnlRelay.Controls.Add(this.chbS_12);
            this.pnlRelay.Controls.Add(this.chbS_6);
            this.pnlRelay.Controls.Add(this.chbS_11);
            this.pnlRelay.Controls.Add(this.chbS_7);
            this.pnlRelay.Controls.Add(this.chbS_10);
            this.pnlRelay.Controls.Add(this.chbS_8);
            this.pnlRelay.Controls.Add(this.chbS_9);
            this.pnlRelay.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.pnlRelay.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pnlRelay.IsScaled = false;
            this.pnlRelay.Location = new System.Drawing.Point(43, 37);
            this.pnlRelay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlRelay.MinimumSize = new System.Drawing.Size(1, 1);
            this.pnlRelay.Name = "pnlRelay";
            this.pnlRelay.Size = new System.Drawing.Size(570, 120);
            this.pnlRelay.Style = Sunny.UI.UIStyle.Custom;
            this.pnlRelay.TabIndex = 20;
            this.pnlRelay.Text = null;
            this.pnlRelay.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chbS_1
            // 
            this.chbS_1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_1.IsScaled = false;
            this.chbS_1.Location = new System.Drawing.Point(30, 11);
            this.chbS_1.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_1.Name = "chbS_1";
            this.chbS_1.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_1.Size = new System.Drawing.Size(70, 35);
            this.chbS_1.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_1.TabIndex = 4;
            this.chbS_1.Text = "S1";
            this.chbS_1.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_16
            // 
            this.chbS_16.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_16.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_16.IsScaled = false;
            this.chbS_16.Location = new System.Drawing.Point(497, 72);
            this.chbS_16.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_16.Name = "chbS_16";
            this.chbS_16.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_16.Size = new System.Drawing.Size(70, 35);
            this.chbS_16.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_16.TabIndex = 19;
            this.chbS_16.Text = "S16";
            this.chbS_16.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_2
            // 
            this.chbS_2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_2.IsScaled = false;
            this.chbS_2.Location = new System.Drawing.Point(96, 11);
            this.chbS_2.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_2.Name = "chbS_2";
            this.chbS_2.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_2.Size = new System.Drawing.Size(70, 35);
            this.chbS_2.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_2.TabIndex = 5;
            this.chbS_2.Text = "S2";
            this.chbS_2.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_15
            // 
            this.chbS_15.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_15.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_15.IsScaled = false;
            this.chbS_15.Location = new System.Drawing.Point(431, 72);
            this.chbS_15.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_15.Name = "chbS_15";
            this.chbS_15.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_15.Size = new System.Drawing.Size(70, 35);
            this.chbS_15.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_15.TabIndex = 18;
            this.chbS_15.Text = "S15";
            this.chbS_15.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_3
            // 
            this.chbS_3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_3.IsScaled = false;
            this.chbS_3.Location = new System.Drawing.Point(162, 11);
            this.chbS_3.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_3.Name = "chbS_3";
            this.chbS_3.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_3.Size = new System.Drawing.Size(70, 35);
            this.chbS_3.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_3.TabIndex = 6;
            this.chbS_3.Text = "S3";
            this.chbS_3.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_14
            // 
            this.chbS_14.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_14.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_14.IsScaled = false;
            this.chbS_14.Location = new System.Drawing.Point(365, 72);
            this.chbS_14.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_14.Name = "chbS_14";
            this.chbS_14.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_14.Size = new System.Drawing.Size(70, 35);
            this.chbS_14.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_14.TabIndex = 17;
            this.chbS_14.Text = "S14";
            this.chbS_14.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_4
            // 
            this.chbS_4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_4.IsScaled = false;
            this.chbS_4.Location = new System.Drawing.Point(228, 11);
            this.chbS_4.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_4.Name = "chbS_4";
            this.chbS_4.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_4.Size = new System.Drawing.Size(70, 35);
            this.chbS_4.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_4.TabIndex = 7;
            this.chbS_4.Text = "S4";
            this.chbS_4.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_13
            // 
            this.chbS_13.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_13.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_13.IsScaled = false;
            this.chbS_13.Location = new System.Drawing.Point(299, 72);
            this.chbS_13.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_13.Name = "chbS_13";
            this.chbS_13.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_13.Size = new System.Drawing.Size(70, 35);
            this.chbS_13.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_13.TabIndex = 16;
            this.chbS_13.Text = "S13";
            this.chbS_13.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_5
            // 
            this.chbS_5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_5.IsScaled = false;
            this.chbS_5.Location = new System.Drawing.Point(299, 11);
            this.chbS_5.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_5.Name = "chbS_5";
            this.chbS_5.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_5.Size = new System.Drawing.Size(70, 35);
            this.chbS_5.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_5.TabIndex = 8;
            this.chbS_5.Text = "S5";
            this.chbS_5.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_12
            // 
            this.chbS_12.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_12.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_12.IsScaled = false;
            this.chbS_12.Location = new System.Drawing.Point(228, 72);
            this.chbS_12.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_12.Name = "chbS_12";
            this.chbS_12.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_12.Size = new System.Drawing.Size(70, 35);
            this.chbS_12.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_12.TabIndex = 15;
            this.chbS_12.Text = "S12";
            this.chbS_12.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_6
            // 
            this.chbS_6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_6.IsScaled = false;
            this.chbS_6.Location = new System.Drawing.Point(365, 11);
            this.chbS_6.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_6.Name = "chbS_6";
            this.chbS_6.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_6.Size = new System.Drawing.Size(70, 35);
            this.chbS_6.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_6.TabIndex = 9;
            this.chbS_6.Text = "S6";
            this.chbS_6.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_11
            // 
            this.chbS_11.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_11.IsScaled = false;
            this.chbS_11.Location = new System.Drawing.Point(162, 72);
            this.chbS_11.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_11.Name = "chbS_11";
            this.chbS_11.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_11.Size = new System.Drawing.Size(70, 35);
            this.chbS_11.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_11.TabIndex = 14;
            this.chbS_11.Text = "S11";
            this.chbS_11.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_7
            // 
            this.chbS_7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_7.IsScaled = false;
            this.chbS_7.Location = new System.Drawing.Point(431, 11);
            this.chbS_7.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_7.Name = "chbS_7";
            this.chbS_7.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_7.Size = new System.Drawing.Size(70, 35);
            this.chbS_7.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_7.TabIndex = 10;
            this.chbS_7.Text = "S7";
            this.chbS_7.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_10
            // 
            this.chbS_10.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_10.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_10.IsScaled = false;
            this.chbS_10.Location = new System.Drawing.Point(96, 72);
            this.chbS_10.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_10.Name = "chbS_10";
            this.chbS_10.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_10.Size = new System.Drawing.Size(70, 35);
            this.chbS_10.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_10.TabIndex = 13;
            this.chbS_10.Text = "S10";
            this.chbS_10.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_8
            // 
            this.chbS_8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_8.IsScaled = false;
            this.chbS_8.Location = new System.Drawing.Point(497, 11);
            this.chbS_8.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_8.Name = "chbS_8";
            this.chbS_8.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_8.Size = new System.Drawing.Size(70, 35);
            this.chbS_8.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_8.TabIndex = 11;
            this.chbS_8.Text = "S8";
            this.chbS_8.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // chbS_9
            // 
            this.chbS_9.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbS_9.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbS_9.IsScaled = false;
            this.chbS_9.Location = new System.Drawing.Point(30, 72);
            this.chbS_9.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbS_9.Name = "chbS_9";
            this.chbS_9.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbS_9.Size = new System.Drawing.Size(70, 35);
            this.chbS_9.Style = Sunny.UI.UIStyle.Custom;
            this.chbS_9.TabIndex = 12;
            this.chbS_9.Text = "S9";
            this.chbS_9.CheckedChanged += new System.EventHandler(this.chbS_1_CheckedChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.IsScaled = false;
            this.lblInfo.Location = new System.Drawing.Point(19, 168);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(429, 41);
            this.lblInfo.Style = Sunny.UI.UIStyle.Custom;
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "S1:黄灯     S2:绿灯     S3：红灯     S4：蜂鸣器\r\n";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UcControlBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "UcControlBoard";
            this.Load += new System.EventHandler(this.UcControlBoard_Load);
            this.uiGroupBox1.ResumeLayout(false);
            this.pnlRelay.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UILabel lblInfo;
        private Sunny.UI.UIPanel pnlRelay;
        private Sunny.UI.UICheckBox chbS_1;
        private Sunny.UI.UICheckBox chbS_16;
        private Sunny.UI.UICheckBox chbS_2;
        private Sunny.UI.UICheckBox chbS_15;
        private Sunny.UI.UICheckBox chbS_3;
        private Sunny.UI.UICheckBox chbS_14;
        private Sunny.UI.UICheckBox chbS_4;
        private Sunny.UI.UICheckBox chbS_13;
        private Sunny.UI.UICheckBox chbS_5;
        private Sunny.UI.UICheckBox chbS_12;
        private Sunny.UI.UICheckBox chbS_6;
        private Sunny.UI.UICheckBox chbS_11;
        private Sunny.UI.UICheckBox chbS_7;
        private Sunny.UI.UICheckBox chbS_10;
        private Sunny.UI.UICheckBox chbS_8;
        private Sunny.UI.UICheckBox chbS_9;
        private Sunny.UI.UIButton btnRead;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel lblText;
    }
}
