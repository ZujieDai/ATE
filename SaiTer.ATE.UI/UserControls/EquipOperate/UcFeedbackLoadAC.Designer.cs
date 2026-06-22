namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcFeedbackLoadAC
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
            this.btnStop = new Sunny.UI.UIButton();
            this.btnStart = new Sunny.UI.UIButton();
            this.txtCurrent = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnSetVolt = new Sunny.UI.UIButton();
            this.txtVolt = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.btnStop);
            this.uiGroupBox1.Controls.Add(this.btnStart);
            this.uiGroupBox1.Controls.Add(this.txtCurrent);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Controls.Add(this.btnSetVolt);
            this.uiGroupBox1.Controls.Add(this.txtVolt);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(10, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox1.Size = new System.Drawing.Size(720, 530);
            this.uiGroupBox1.TabIndex = 1;
            this.uiGroupBox1.Text = "交流回馈负载操作";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnStop
            // 
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.IsScaled = false;
            this.btnStop.Location = new System.Drawing.Point(407, 369);
            this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(134, 68);
            this.btnStop.TabIndex = 7;
            this.btnStop.Text = "关闭输出";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.IsScaled = false;
            this.btnStart.Location = new System.Drawing.Point(143, 369);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(134, 68);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "启动输出";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
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
            this.txtCurrent.Location = new System.Drawing.Point(234, 200);
            this.txtCurrent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCurrent.Maximum = 150D;
            this.txtCurrent.MaximumEnabled = true;
            this.txtCurrent.Minimum = 0D;
            this.txtCurrent.MinimumEnabled = true;
            this.txtCurrent.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtCurrent.Name = "txtCurrent";
            this.txtCurrent.Size = new System.Drawing.Size(191, 35);
            this.txtCurrent.TabIndex = 4;
            this.txtCurrent.Text = "20.00";
            this.txtCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtCurrent.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(93, 200);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(134, 36);
            this.uiLabel2.TabIndex = 3;
            this.uiLabel2.Text = "电流设置(A)";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSetVolt
            // 
            this.btnSetVolt.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSetVolt.IsScaled = false;
            this.btnSetVolt.Location = new System.Drawing.Point(491, 194);
            this.btnSetVolt.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSetVolt.Name = "btnSetVolt";
            this.btnSetVolt.Size = new System.Drawing.Size(124, 45);
            this.btnSetVolt.TabIndex = 2;
            this.btnSetVolt.Text = "设置";
            this.btnSetVolt.Click += new System.EventHandler(this.btnSetVolt_Click);
            // 
            // txtVolt
            // 
            this.txtVolt.ButtonSymbol = 61761;
            this.txtVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVolt.DoubleValue = 220D;
            this.txtVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtVolt.HasMinimum = true;
            this.txtVolt.IsScaled = false;
            this.txtVolt.Location = new System.Drawing.Point(234, 106);
            this.txtVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtVolt.Maximum = 400D;
            this.txtVolt.MaxLength = 4;
            this.txtVolt.Minimum = 0D;
            this.txtVolt.MinimumEnabled = true;
            this.txtVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVolt.Multiline = true;
            this.txtVolt.Name = "txtVolt";
            this.txtVolt.Size = new System.Drawing.Size(191, 35);
            this.txtVolt.TabIndex = 1;
            this.txtVolt.Text = "220.00";
            this.txtVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVolt.Type = Sunny.UI.UITextBox.UIEditType.Double;
            this.txtVolt.Visible = false;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(93, 106);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(134, 36);
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "电压设置(V):";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.uiLabel1.Visible = false;
            // 
            // UcFeedbackLoadAC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "UcFeedbackLoadAC";
            this.Load += new System.EventHandler(this.UcFeedbackLoadAC_Load);
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UIButton btnStop;
        private Sunny.UI.UIButton btnStart;
        private Sunny.UI.UITextBox txtCurrent;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnSetVolt;
        private Sunny.UI.UITextBox txtVolt;
        private Sunny.UI.UILabel uiLabel1;
    }
}
