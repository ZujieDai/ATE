namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcElectronicLoad
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
            this.btnStop = new Sunny.UI.UIButton();
            this.btnStart = new Sunny.UI.UIButton();
            this.btnSet = new Sunny.UI.UIButton();
            this.txtValue = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.cmbFunction = new Sunny.UI.UIComboBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.btnStop);
            this.uiGroupBox2.Controls.Add(this.btnStart);
            this.uiGroupBox2.Controls.Add(this.btnSet);
            this.uiGroupBox2.Controls.Add(this.txtValue);
            this.uiGroupBox2.Controls.Add(this.uiLabel2);
            this.uiGroupBox2.Controls.Add(this.cmbFunction);
            this.uiGroupBox2.Controls.Add(this.uiLabel1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiGroupBox2.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiGroupBox2.IsScaled = false;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(20);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(20);
            this.uiGroupBox2.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox2.Size = new System.Drawing.Size(730, 530);
            this.uiGroupBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox2.TabIndex = 5;
            this.uiGroupBox2.Text = "电子负载操作";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnStop
            // 
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.IsScaled = false;
            this.btnStop.Location = new System.Drawing.Point(384, 350);
            this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(133, 48);
            this.btnStop.Style = Sunny.UI.UIStyle.Custom;
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "关闭输出";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.IsScaled = false;
            this.btnStart.Location = new System.Drawing.Point(110, 350);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(133, 48);
            this.btnStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "启动输出";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSet
            // 
            this.btnSet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSet.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSet.IsScaled = false;
            this.btnSet.Location = new System.Drawing.Point(513, 151);
            this.btnSet.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(105, 46);
            this.btnSet.Style = Sunny.UI.UIStyle.Custom;
            this.btnSet.TabIndex = 4;
            this.btnSet.Text = "设置";
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // txtValue
            // 
            this.txtValue.ButtonSymbol = 61761;
            this.txtValue.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtValue.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtValue.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtValue.IsScaled = false;
            this.txtValue.Location = new System.Drawing.Point(271, 197);
            this.txtValue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtValue.Maximum = 2147483647D;
            this.txtValue.Minimum = -2147483648D;
            this.txtValue.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(205, 45);
            this.txtValue.Style = Sunny.UI.UIStyle.Custom;
            this.txtValue.TabIndex = 3;
            this.txtValue.Text = "0";
            this.txtValue.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtValue.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(125, 197);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(118, 41);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 2;
            this.uiLabel2.Text = "设置值：";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbFunction
            // 
            this.cmbFunction.DataSource = null;
            this.cmbFunction.FillColor = System.Drawing.Color.White;
            this.cmbFunction.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbFunction.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbFunction.IsScaled = false;
            this.cmbFunction.Items.AddRange(new object[] {
            "定电流(mA)",
            "定电压(mV)",
            "定功率(mW)",
            "定电阻(mΩ)"});
            this.cmbFunction.Location = new System.Drawing.Point(271, 114);
            this.cmbFunction.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbFunction.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbFunction.Name = "cmbFunction";
            this.cmbFunction.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbFunction.Size = new System.Drawing.Size(205, 43);
            this.cmbFunction.Style = Sunny.UI.UIStyle.Custom;
            this.cmbFunction.TabIndex = 1;
            this.cmbFunction.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbFunction.SelectedIndexChanged += new System.EventHandler(this.cmbFunction_SelectedIndexChanged);
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(125, 117);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(118, 41);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "功能选择:";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UcElectronicLoad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiGroupBox2);
            this.Margin = new System.Windows.Forms.Padding(20);
            this.Name = "UcElectronicLoad";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
            this.Load += new System.EventHandler(this.UcElectronicLoad_Load);
            this.uiGroupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UIComboBox cmbFunction;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnStop;
        private Sunny.UI.UIButton btnStart;
        private Sunny.UI.UIButton btnSet;
        private Sunny.UI.UITextBox txtValue;
    }
}
