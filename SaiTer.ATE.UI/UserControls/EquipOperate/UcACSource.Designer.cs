namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcACSource
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
            this.btnSetFreq = new Sunny.UI.UIButton();
            this.txtFreq = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVoltage = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bntClose = new Sunny.UI.UIButton();
            this.bntStart = new Sunny.UI.UIButton();
            this.btnSetVoltage = new Sunny.UI.UIButton();
            this.txtVoltage = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.btnSetFreq);
            this.uiGroupBox1.Controls.Add(this.txtFreq);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Controls.Add(this.label2);
            this.uiGroupBox1.Controls.Add(this.lblVoltage);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.bntClose);
            this.uiGroupBox1.Controls.Add(this.bntStart);
            this.uiGroupBox1.Controls.Add(this.btnSetVoltage);
            this.uiGroupBox1.Controls.Add(this.txtVoltage);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 21, 0, 0);
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox1.Size = new System.Drawing.Size(730, 530);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 1;
            this.uiGroupBox1.Text = "设置电压";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSetFreq
            // 
            this.btnSetFreq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnSetFreq.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetFreq.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSetFreq.IsScaled = false;
            this.btnSetFreq.Location = new System.Drawing.Point(353, 238);
            this.btnSetFreq.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSetFreq.Name = "btnSetFreq";
            this.btnSetFreq.Size = new System.Drawing.Size(74, 30);
            this.btnSetFreq.Style = Sunny.UI.UIStyle.Custom;
            this.btnSetFreq.TabIndex = 27;
            this.btnSetFreq.Text = "设置";
            this.btnSetFreq.Click += new System.EventHandler(this.btnSetFreq_Click);
            // 
            // txtFreq
            // 
            this.txtFreq.ButtonSymbol = 61761;
            this.txtFreq.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtFreq.DoubleValue = 50D;
            this.txtFreq.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtFreq.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtFreq.HasMaximum = true;
            this.txtFreq.HasMinimum = true;
            this.txtFreq.IntValue = 50;
            this.txtFreq.IsScaled = false;
            this.txtFreq.Location = new System.Drawing.Point(207, 238);
            this.txtFreq.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFreq.Maximum = 300D;
            this.txtFreq.MaximumEnabled = true;
            this.txtFreq.MaxLength = 3;
            this.txtFreq.Minimum = 0D;
            this.txtFreq.MinimumEnabled = true;
            this.txtFreq.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtFreq.Name = "txtFreq";
            this.txtFreq.Size = new System.Drawing.Size(100, 30);
            this.txtFreq.Style = Sunny.UI.UIStyle.Custom;
            this.txtFreq.TabIndex = 26;
            this.txtFreq.Text = "50";
            this.txtFreq.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtFreq.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel2
            // 
            this.uiLabel2.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(94, 240);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(106, 25);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 25;
            this.uiLabel2.Text = "频率(Hz):";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(273, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 31);
            this.label2.TabIndex = 24;
            this.label2.Text = "V";
            // 
            // lblVoltage
            // 
            this.lblVoltage.AutoSize = true;
            this.lblVoltage.ForeColor = System.Drawing.Color.Red;
            this.lblVoltage.Location = new System.Drawing.Point(212, 162);
            this.lblVoltage.Name = "lblVoltage";
            this.lblVoltage.Size = new System.Drawing.Size(56, 31);
            this.lblVoltage.TabIndex = 23;
            this.lblVoltage.Text = "380";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(66, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(518, 62);
            this.label1.TabIndex = 22;
            this.label1.Text = "注意：此处设置的是相电压，如果是两相火线，\r\n则线电压约为";
            // 
            // bntClose
            // 
            this.bntClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.bntClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bntClose.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.bntClose.IsScaled = false;
            this.bntClose.Location = new System.Drawing.Point(386, 370);
            this.bntClose.Margin = new System.Windows.Forms.Padding(2);
            this.bntClose.MinimumSize = new System.Drawing.Size(1, 1);
            this.bntClose.Name = "bntClose";
            this.bntClose.Size = new System.Drawing.Size(100, 30);
            this.bntClose.Style = Sunny.UI.UIStyle.Custom;
            this.bntClose.TabIndex = 21;
            this.bntClose.Text = "关闭输出";
            this.bntClose.Click += new System.EventHandler(this.bntClose_Click);
            // 
            // bntStart
            // 
            this.bntStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.bntStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bntStart.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.bntStart.IsScaled = false;
            this.bntStart.Location = new System.Drawing.Point(168, 370);
            this.bntStart.Margin = new System.Windows.Forms.Padding(2);
            this.bntStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.bntStart.Name = "bntStart";
            this.bntStart.Size = new System.Drawing.Size(100, 30);
            this.bntStart.Style = Sunny.UI.UIStyle.Custom;
            this.bntStart.TabIndex = 20;
            this.bntStart.Text = "启动输出";
            this.bntStart.Click += new System.EventHandler(this.bntStart_Click);
            // 
            // btnSetVoltage
            // 
            this.btnSetVoltage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnSetVoltage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetVoltage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSetVoltage.IsScaled = false;
            this.btnSetVoltage.Location = new System.Drawing.Point(353, 73);
            this.btnSetVoltage.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSetVoltage.Name = "btnSetVoltage";
            this.btnSetVoltage.Size = new System.Drawing.Size(74, 30);
            this.btnSetVoltage.Style = Sunny.UI.UIStyle.Custom;
            this.btnSetVoltage.TabIndex = 17;
            this.btnSetVoltage.Text = "设置";
            this.btnSetVoltage.Click += new System.EventHandler(this.btnSetVoltage_Click);
            // 
            // txtVoltage
            // 
            this.txtVoltage.ButtonSymbol = 61761;
            this.txtVoltage.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVoltage.DoubleValue = 220D;
            this.txtVoltage.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtVoltage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtVoltage.HasMaximum = true;
            this.txtVoltage.HasMinimum = true;
            this.txtVoltage.IntValue = 220;
            this.txtVoltage.IsScaled = false;
            this.txtVoltage.Location = new System.Drawing.Point(207, 73);
            this.txtVoltage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtVoltage.Maximum = 300D;
            this.txtVoltage.MaximumEnabled = true;
            this.txtVoltage.MaxLength = 3;
            this.txtVoltage.Minimum = 0D;
            this.txtVoltage.MinimumEnabled = true;
            this.txtVoltage.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVoltage.Name = "txtVoltage";
            this.txtVoltage.Size = new System.Drawing.Size(100, 30);
            this.txtVoltage.Style = Sunny.UI.UIStyle.Custom;
            this.txtVoltage.TabIndex = 1;
            this.txtVoltage.Text = "220";
            this.txtVoltage.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVoltage.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            this.txtVoltage.TextChanged += new System.EventHandler(this.txtVoltage_TextChanged);
            // 
            // uiLabel1
            // 
            this.uiLabel1.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(94, 75);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(106, 25);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "输出电压(V):";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UcACSource
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "UcACSource";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
            this.Load += new System.EventHandler(this.UcACSource_Load);
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UITextBox txtVoltage;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIButton btnSetVoltage;
        private Sunny.UI.UIButton bntClose;
        private Sunny.UI.UIButton bntStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblVoltage;
        private System.Windows.Forms.Label label1;
        private Sunny.UI.UIButton btnSetFreq;
        private Sunny.UI.UITextBox txtFreq;
        private Sunny.UI.UILabel uiLabel2;
    }
}
