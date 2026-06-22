namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcResisLoad
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
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.btnOFF = new Sunny.UI.UIButton();
            this.btnStart = new Sunny.UI.UIButton();
            this.btnFreq = new Sunny.UI.UIButton();
            this.txtPower = new Sunny.UI.UITextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.btnCurrent = new Sunny.UI.UIButton();
            this.txtCurrent = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnSetVoltCur = new Sunny.UI.UIButton();
            this.txtVolt = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.uiLabel4);
            this.uiGroupBox1.Controls.Add(this.btnOFF);
            this.uiGroupBox1.Controls.Add(this.btnStart);
            this.uiGroupBox1.Controls.Add(this.btnFreq);
            this.uiGroupBox1.Controls.Add(this.txtPower);
            this.uiGroupBox1.Controls.Add(this.uiLabel3);
            this.uiGroupBox1.Controls.Add(this.btnCurrent);
            this.uiGroupBox1.Controls.Add(this.txtCurrent);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Controls.Add(this.btnSetVoltCur);
            this.uiGroupBox1.Controls.Add(this.txtVolt);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox1.Size = new System.Drawing.Size(730, 530);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 3;
            this.uiGroupBox1.Text = "电阻负载操作";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel4
            // 
            this.uiLabel4.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.IsScaled = false;
            this.uiLabel4.Location = new System.Drawing.Point(134, 197);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(447, 89);
            this.uiLabel4.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel4.TabIndex = 25;
            this.uiLabel4.Text = "注：例如此处设置30A负载电流，如果是单相桩，则ABC各输出10A，总输出30A电流。如果是三相桩为ABC各输出30A，三相总输出90A";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOFF
            // 
            this.btnOFF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnOFF.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOFF.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnOFF.IsScaled = false;
            this.btnOFF.Location = new System.Drawing.Point(427, 396);
            this.btnOFF.Margin = new System.Windows.Forms.Padding(2);
            this.btnOFF.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOFF.Name = "btnOFF";
            this.btnOFF.Size = new System.Drawing.Size(106, 53);
            this.btnOFF.Style = Sunny.UI.UIStyle.Custom;
            this.btnOFF.TabIndex = 24;
            this.btnOFF.Text = "关闭";
            this.btnOFF.Click += new System.EventHandler(this.btnOFF_Click);
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnStart.IsScaled = false;
            this.btnStart.Location = new System.Drawing.Point(201, 396);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(110, 53);
            this.btnStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnStart.TabIndex = 23;
            this.btnStart.Text = "启动";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnFreq
            // 
            this.btnFreq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnFreq.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFreq.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnFreq.IsScaled = false;
            this.btnFreq.Location = new System.Drawing.Point(476, 315);
            this.btnFreq.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnFreq.Name = "btnFreq";
            this.btnFreq.Size = new System.Drawing.Size(74, 30);
            this.btnFreq.Style = Sunny.UI.UIStyle.Custom;
            this.btnFreq.TabIndex = 22;
            this.btnFreq.Text = "设置";
            this.btnFreq.Click += new System.EventHandler(this.btnFreq_Click);
            // 
            // txtPower
            // 
            this.txtPower.ButtonSymbol = 61761;
            this.txtPower.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPower.DoubleValue = 3D;
            this.txtPower.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtPower.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPower.IntValue = 3;
            this.txtPower.IsScaled = false;
            this.txtPower.Location = new System.Drawing.Point(345, 315);
            this.txtPower.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPower.Maximum = 2147483647D;
            this.txtPower.Minimum = -2147483648D;
            this.txtPower.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtPower.Name = "txtPower";
            this.txtPower.Size = new System.Drawing.Size(100, 30);
            this.txtPower.Style = Sunny.UI.UIStyle.Custom;
            this.txtPower.TabIndex = 21;
            this.txtPower.Text = "3";
            this.txtPower.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtPower.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // uiLabel3
            // 
            this.uiLabel3.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(180, 315);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(158, 25);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 20;
            this.uiLabel3.Text = "负载需求功率(KW):";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCurrent
            // 
            this.btnCurrent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnCurrent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCurrent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCurrent.IsScaled = false;
            this.btnCurrent.Location = new System.Drawing.Point(507, 34);
            this.btnCurrent.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCurrent.Name = "btnCurrent";
            this.btnCurrent.Size = new System.Drawing.Size(74, 30);
            this.btnCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.btnCurrent.TabIndex = 19;
            this.btnCurrent.Text = "设置";
            this.btnCurrent.Visible = false;
            this.btnCurrent.Click += new System.EventHandler(this.btnCurrent_Click);
            // 
            // txtCurrent
            // 
            this.txtCurrent.ButtonSymbol = 61761;
            this.txtCurrent.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCurrent.DoubleValue = 10D;
            this.txtCurrent.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtCurrent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCurrent.IsScaled = false;
            this.txtCurrent.Location = new System.Drawing.Point(345, 162);
            this.txtCurrent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCurrent.Maximum = 2147483647D;
            this.txtCurrent.Minimum = -2147483648D;
            this.txtCurrent.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtCurrent.Name = "txtCurrent";
            this.txtCurrent.Size = new System.Drawing.Size(100, 30);
            this.txtCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.txtCurrent.TabIndex = 18;
            this.txtCurrent.Text = "10.00";
            this.txtCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtCurrent.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // uiLabel2
            // 
            this.uiLabel2.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(180, 162);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(158, 25);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 17;
            this.uiLabel2.Text = "负载需求电流(A):";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSetVoltCur
            // 
            this.btnSetVoltCur.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnSetVoltCur.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetVoltCur.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSetVoltCur.IsScaled = false;
            this.btnSetVoltCur.Location = new System.Drawing.Point(476, 112);
            this.btnSetVoltCur.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSetVoltCur.Name = "btnSetVoltCur";
            this.btnSetVoltCur.Size = new System.Drawing.Size(74, 30);
            this.btnSetVoltCur.Style = Sunny.UI.UIStyle.Custom;
            this.btnSetVoltCur.TabIndex = 16;
            this.btnSetVoltCur.Text = "设置";
            this.btnSetVoltCur.Click += new System.EventHandler(this.btnSetVoltCur_Click);
            // 
            // txtVolt
            // 
            this.txtVolt.ButtonSymbol = 61761;
            this.txtVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVolt.DoubleValue = 220D;
            this.txtVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtVolt.IsScaled = false;
            this.txtVolt.Location = new System.Drawing.Point(345, 82);
            this.txtVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtVolt.Maximum = 2147483647D;
            this.txtVolt.Minimum = -2147483648D;
            this.txtVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVolt.Name = "txtVolt";
            this.txtVolt.Size = new System.Drawing.Size(100, 30);
            this.txtVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtVolt.TabIndex = 3;
            this.txtVolt.Text = "220.00";
            this.txtVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVolt.Type = Sunny.UI.UITextBox.UIEditType.Double;
            // 
            // uiLabel1
            // 
            this.uiLabel1.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(180, 82);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(158, 25);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 2;
            this.uiLabel1.Text = "负载需求电压(V):";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UcResisLoad
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "UcResisLoad";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
            this.Load += new System.EventHandler(this.UcResisLoad_Load);
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UIButton btnCurrent;
        private Sunny.UI.UITextBox txtCurrent;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnSetVoltCur;
        private Sunny.UI.UITextBox txtVolt;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIButton btnFreq;
        private Sunny.UI.UITextBox txtPower;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UIButton btnOFF;
        private Sunny.UI.UIButton btnStart;
        private Sunny.UI.UILabel uiLabel4;
    }
}
