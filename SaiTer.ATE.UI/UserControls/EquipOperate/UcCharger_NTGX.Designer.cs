namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcCharger_NTGX
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
            this.uiGroupBox2 = new Sunny.UI.UIGroupBox();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.btnLoadStop = new Sunny.UI.UIButton();
            this.txtVolt = new Sunny.UI.UITextBox();
            this.btnLoadStart = new Sunny.UI.UIButton();
            this.btnSetLoadParam = new Sunny.UI.UIButton();
            this.txtCurrent = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnStart = new Sunny.UI.UIButton();
            this.btnOFF = new Sunny.UI.UIButton();
            this.uiGroupBox2.SuspendLayout();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.uiGroupBox1);
            this.uiGroupBox2.Controls.Add(this.btnStart);
            this.uiGroupBox2.Controls.Add(this.btnOFF);
            this.uiGroupBox2.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox2.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiGroupBox2.IsScaled = false;
            this.uiGroupBox2.Location = new System.Drawing.Point(14, 8);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(5, 19, 5, 5);
            this.uiGroupBox2.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox2.Size = new System.Drawing.Size(686, 530);
            this.uiGroupBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox2.TabIndex = 4;
            this.uiGroupBox2.Text = "充电桩模拟器操作";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Controls.Add(this.btnLoadStop);
            this.uiGroupBox1.Controls.Add(this.txtVolt);
            this.uiGroupBox1.Controls.Add(this.btnLoadStart);
            this.uiGroupBox1.Controls.Add(this.btnSetLoadParam);
            this.uiGroupBox1.Controls.Add(this.txtCurrent);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(9, 224);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 21, 0, 0);
            this.uiGroupBox1.Size = new System.Drawing.Size(656, 279);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 29;
            this.uiGroupBox1.Text = "负载功能";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(111, 47);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(134, 36);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 21;
            this.uiLabel1.Text = "电压设置(V):";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnLoadStop
            // 
            this.btnLoadStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLoadStop.IsScaled = false;
            this.btnLoadStop.Location = new System.Drawing.Point(407, 172);
            this.btnLoadStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnLoadStop.Name = "btnLoadStop";
            this.btnLoadStop.Size = new System.Drawing.Size(134, 68);
            this.btnLoadStop.Style = Sunny.UI.UIStyle.Custom;
            this.btnLoadStop.TabIndex = 28;
            this.btnLoadStop.Text = "关闭输出";
            this.btnLoadStop.Click += new System.EventHandler(this.btnLoadStop_Click);
            // 
            // txtVolt
            // 
            this.txtVolt.ButtonSymbol = 61761;
            this.txtVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVolt.DoubleValue = 740D;
            this.txtVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtVolt.HasMinimum = true;
            this.txtVolt.IsScaled = false;
            this.txtVolt.Location = new System.Drawing.Point(252, 47);
            this.txtVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtVolt.Maximum = 1500D;
            this.txtVolt.MaxLength = 4;
            this.txtVolt.Minimum = 0D;
            this.txtVolt.MinimumEnabled = true;
            this.txtVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVolt.Multiline = true;
            this.txtVolt.Name = "txtVolt";
            this.txtVolt.Size = new System.Drawing.Size(191, 35);
            this.txtVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtVolt.TabIndex = 22;
            this.txtVolt.Text = "740.00";
            this.txtVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVolt.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // btnLoadStart
            // 
            this.btnLoadStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLoadStart.IsScaled = false;
            this.btnLoadStart.Location = new System.Drawing.Point(91, 172);
            this.btnLoadStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnLoadStart.Name = "btnLoadStart";
            this.btnLoadStart.Size = new System.Drawing.Size(134, 68);
            this.btnLoadStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnLoadStart.TabIndex = 27;
            this.btnLoadStart.Text = "启动输出";
            this.btnLoadStart.Click += new System.EventHandler(this.btnLoadStart_Click);
            // 
            // btnSetLoadParam
            // 
            this.btnSetLoadParam.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetLoadParam.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSetLoadParam.IsScaled = false;
            this.btnSetLoadParam.Location = new System.Drawing.Point(492, 76);
            this.btnSetLoadParam.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSetLoadParam.Name = "btnSetLoadParam";
            this.btnSetLoadParam.Size = new System.Drawing.Size(97, 33);
            this.btnSetLoadParam.Style = Sunny.UI.UIStyle.Custom;
            this.btnSetLoadParam.TabIndex = 23;
            this.btnSetLoadParam.Text = "设置";
            this.btnSetLoadParam.Click += new System.EventHandler(this.btnSetLoadParam_Click);
            // 
            // txtCurrent
            // 
            this.txtCurrent.ButtonSymbol = 61761;
            this.txtCurrent.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCurrent.DoubleValue = 20D;
            this.txtCurrent.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtCurrent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCurrent.HasMaximum = true;
            this.txtCurrent.HasMinimum = true;
            this.txtCurrent.IsScaled = false;
            this.txtCurrent.Location = new System.Drawing.Point(252, 101);
            this.txtCurrent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCurrent.Maximum = 300D;
            this.txtCurrent.MaximumEnabled = true;
            this.txtCurrent.Minimum = 0D;
            this.txtCurrent.MinimumEnabled = true;
            this.txtCurrent.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtCurrent.Name = "txtCurrent";
            this.txtCurrent.Size = new System.Drawing.Size(191, 35);
            this.txtCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.txtCurrent.TabIndex = 25;
            this.txtCurrent.Text = "20.00";
            this.txtCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtCurrent.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(111, 101);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(134, 36);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 24;
            this.uiLabel2.Text = "电流设置(A)";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.IsScaled = false;
            this.btnStart.Location = new System.Drawing.Point(100, 79);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(152, 63);
            this.btnStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnStart.TabIndex = 19;
            this.btnStart.Text = "启动";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnOFF
            // 
            this.btnOFF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnOFF.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOFF.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOFF.IsScaled = false;
            this.btnOFF.Location = new System.Drawing.Point(416, 79);
            this.btnOFF.Margin = new System.Windows.Forms.Padding(2);
            this.btnOFF.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOFF.Name = "btnOFF";
            this.btnOFF.Size = new System.Drawing.Size(152, 63);
            this.btnOFF.Style = Sunny.UI.UIStyle.Custom;
            this.btnOFF.TabIndex = 20;
            this.btnOFF.Text = "关闭";
            this.btnOFF.Click += new System.EventHandler(this.btnOFF_Click);
            // 
            // UcCharger_NTGX
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.uiGroupBox2);
            this.Name = "UcCharger_NTGX";
            this.Load += new System.EventHandler(this.UcCharger_NTGX_Load);
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UIButton btnStart;
        private Sunny.UI.UIButton btnOFF;
        private Sunny.UI.UIButton btnLoadStop;
        private Sunny.UI.UIButton btnLoadStart;
        private Sunny.UI.UITextBox txtCurrent;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnSetLoadParam;
        private Sunny.UI.UITextBox txtVolt;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIGroupBox uiGroupBox1;
    }
}
