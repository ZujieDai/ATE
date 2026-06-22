namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcBMS_JP_DC
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
            this.btnStart = new Sunny.UI.UIButton();
            this.btnRead = new Sunny.UI.UIButton();
            this.btnStop = new Sunny.UI.UIButton();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.txtChargingRateConst = new Sunny.UI.UITextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.txtMaxBatteryVolt = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.txtMinBatteryVolt = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiGroupBox2 = new Sunny.UI.UIGroupBox();
            this.txtChargingET = new Sunny.UI.UITextBox();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.txtMaxChargingTime_M = new Sunny.UI.UITextBox();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.txtMaxChargingTime_S = new Sunny.UI.UITextBox();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.uiGroupBox3 = new Sunny.UI.UIGroupBox();
            this.uiLabel20 = new Sunny.UI.UILabel();
            this.cmbVehicleStatus = new Sunny.UI.UIComboBox();
            this.uiLabel19 = new Sunny.UI.UILabel();
            this.cmbNormalStop = new Sunny.UI.UIComboBox();
            this.uiLabel18 = new Sunny.UI.UILabel();
            this.cmbSystemFault = new Sunny.UI.UIComboBox();
            this.uiLabel17 = new Sunny.UI.UILabel();
            this.cmbShiftPosition = new Sunny.UI.UIComboBox();
            this.uiLabel16 = new Sunny.UI.UILabel();
            this.cmbChargingEnabled = new Sunny.UI.UIComboBox();
            this.uiLabel15 = new Sunny.UI.UILabel();
            this.cmbBatteryVoltError = new Sunny.UI.UIComboBox();
            this.uiLabel14 = new Sunny.UI.UILabel();
            this.cmbBatteryCurrentError = new Sunny.UI.UIComboBox();
            this.uiLabel13 = new Sunny.UI.UILabel();
            this.cmbBatteryTempHight = new Sunny.UI.UIComboBox();
            this.uiLabel12 = new Sunny.UI.UILabel();
            this.cmbBatteryUnderVolt = new Sunny.UI.UIComboBox();
            this.uiLabel11 = new Sunny.UI.UILabel();
            this.cmbBatteryOverVolt = new Sunny.UI.UIComboBox();
            this.txtChargingCurrent = new Sunny.UI.UITextBox();
            this.uiLabel10 = new Sunny.UI.UILabel();
            this.txtChargingRate = new Sunny.UI.UITextBox();
            this.uiLabel7 = new Sunny.UI.UILabel();
            this.txtTargetBatteryVolt = new Sunny.UI.UITextBox();
            this.uiLabel8 = new Sunny.UI.UILabel();
            this.txtCHAdeMONumber = new Sunny.UI.UITextBox();
            this.uiLabel9 = new Sunny.UI.UILabel();
            this.btnSet = new Sunny.UI.UIButton();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uiGroupBox1.SuspendLayout();
            this.uiGroupBox2.SuspendLayout();
            this.uiGroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.IsScaled = false;
            this.btnStart.Location = new System.Drawing.Point(467, 494);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStart.Size = new System.Drawing.Size(110, 37);
            this.btnStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnStart.TabIndex = 39;
            this.btnStart.Text = "启动充电";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnRead
            // 
            this.btnRead.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRead.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRead.IsScaled = false;
            this.btnRead.Location = new System.Drawing.Point(143, 494);
            this.btnRead.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(116, 37);
            this.btnRead.Style = Sunny.UI.UIStyle.Custom;
            this.btnRead.TabIndex = 41;
            this.btnRead.Text = "读取充电参数";
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.IsScaled = false;
            this.btnStop.Location = new System.Drawing.Point(587, 494);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStop.Name = "btnStop";
            this.btnStop.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.btnStop.Size = new System.Drawing.Size(110, 37);
            this.btnStop.Style = Sunny.UI.UIStyle.Custom;
            this.btnStop.TabIndex = 40;
            this.btnStop.Text = "停止充电";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.txtChargingRateConst);
            this.uiGroupBox1.Controls.Add(this.uiLabel3);
            this.uiGroupBox1.Controls.Add(this.txtMaxBatteryVolt);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Controls.Add(this.txtMinBatteryVolt);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.IsScaled = false;
            this.uiGroupBox1.Location = new System.Drawing.Point(7, 5);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox1.Size = new System.Drawing.Size(705, 81);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 44;
            this.uiGroupBox1.Text = "BMS充电报文数据(H100)";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtChargingRateConst
            // 
            this.txtChargingRateConst.ButtonSymbol = 61761;
            this.txtChargingRateConst.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtChargingRateConst.DoubleValue = 100D;
            this.txtChargingRateConst.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtChargingRateConst.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtChargingRateConst.IntValue = 100;
            this.txtChargingRateConst.IsScaled = false;
            this.txtChargingRateConst.Location = new System.Drawing.Point(604, 37);
            this.txtChargingRateConst.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChargingRateConst.Maximum = 2147483647D;
            this.txtChargingRateConst.Minimum = -2147483648D;
            this.txtChargingRateConst.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtChargingRateConst.Name = "txtChargingRateConst";
            this.txtChargingRateConst.Size = new System.Drawing.Size(90, 25);
            this.txtChargingRateConst.Style = Sunny.UI.UIStyle.Custom;
            this.txtChargingRateConst.TabIndex = 5;
            this.txtChargingRateConst.Text = "100";
            this.txtChargingRateConst.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtChargingRateConst.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel3
            // 
            this.uiLabel3.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(464, 37);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(133, 25);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 4;
            this.uiLabel3.Text = "充电率参考常数(%):";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMaxBatteryVolt
            // 
            this.txtMaxBatteryVolt.ButtonSymbol = 61761;
            this.txtMaxBatteryVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxBatteryVolt.DoubleValue = 600D;
            this.txtMaxBatteryVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxBatteryVolt.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMaxBatteryVolt.IntValue = 600;
            this.txtMaxBatteryVolt.IsScaled = false;
            this.txtMaxBatteryVolt.Location = new System.Drawing.Point(359, 37);
            this.txtMaxBatteryVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxBatteryVolt.Maximum = 2147483647D;
            this.txtMaxBatteryVolt.Minimum = -2147483648D;
            this.txtMaxBatteryVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxBatteryVolt.Name = "txtMaxBatteryVolt";
            this.txtMaxBatteryVolt.Size = new System.Drawing.Size(90, 25);
            this.txtMaxBatteryVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxBatteryVolt.TabIndex = 5;
            this.txtMaxBatteryVolt.Text = "600";
            this.txtMaxBatteryVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxBatteryVolt.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel1
            // 
            this.uiLabel1.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(233, 37);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(119, 25);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 4;
            this.uiLabel1.Text = "最大电池电压(V):";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMinBatteryVolt
            // 
            this.txtMinBatteryVolt.ButtonSymbol = 61761;
            this.txtMinBatteryVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMinBatteryVolt.DoubleValue = 200D;
            this.txtMinBatteryVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMinBatteryVolt.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMinBatteryVolt.IntValue = 200;
            this.txtMinBatteryVolt.IsScaled = false;
            this.txtMinBatteryVolt.Location = new System.Drawing.Point(136, 37);
            this.txtMinBatteryVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinBatteryVolt.Maximum = 2147483647D;
            this.txtMinBatteryVolt.Minimum = -2147483648D;
            this.txtMinBatteryVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMinBatteryVolt.Name = "txtMinBatteryVolt";
            this.txtMinBatteryVolt.Size = new System.Drawing.Size(90, 25);
            this.txtMinBatteryVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtMinBatteryVolt.TabIndex = 3;
            this.txtMinBatteryVolt.Text = "200";
            this.txtMinBatteryVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMinBatteryVolt.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel2
            // 
            this.uiLabel2.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(3, 37);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(126, 25);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 2;
            this.uiLabel2.Text = "最小电池电压(V):";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.txtChargingET);
            this.uiGroupBox2.Controls.Add(this.uiLabel4);
            this.uiGroupBox2.Controls.Add(this.txtMaxChargingTime_M);
            this.uiGroupBox2.Controls.Add(this.uiLabel5);
            this.uiGroupBox2.Controls.Add(this.txtMaxChargingTime_S);
            this.uiGroupBox2.Controls.Add(this.uiLabel6);
            this.uiGroupBox2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox2.IsScaled = false;
            this.uiGroupBox2.Location = new System.Drawing.Point(7, 96);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.uiGroupBox2.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox2.Size = new System.Drawing.Size(705, 110);
            this.uiGroupBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox2.TabIndex = 45;
            this.uiGroupBox2.Text = "BMS充电报文数据(H101)";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtChargingET
            // 
            this.txtChargingET.ButtonSymbol = 61761;
            this.txtChargingET.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtChargingET.DoubleValue = 90D;
            this.txtChargingET.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtChargingET.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtChargingET.IntValue = 90;
            this.txtChargingET.IsScaled = false;
            this.txtChargingET.Location = new System.Drawing.Point(189, 74);
            this.txtChargingET.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChargingET.Maximum = 2147483647D;
            this.txtChargingET.Minimum = -2147483648D;
            this.txtChargingET.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtChargingET.Name = "txtChargingET";
            this.txtChargingET.Size = new System.Drawing.Size(90, 25);
            this.txtChargingET.Style = Sunny.UI.UIStyle.Custom;
            this.txtChargingET.TabIndex = 5;
            this.txtChargingET.Text = "90";
            this.txtChargingET.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtChargingET.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel4
            // 
            this.uiLabel4.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel4.IsScaled = false;
            this.uiLabel4.Location = new System.Drawing.Point(46, 74);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(136, 25);
            this.uiLabel4.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel4.TabIndex = 4;
            this.uiLabel4.Text = "预计充电时间(分钟):";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMaxChargingTime_M
            // 
            this.txtMaxChargingTime_M.ButtonSymbol = 61761;
            this.txtMaxChargingTime_M.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxChargingTime_M.DoubleValue = 90D;
            this.txtMaxChargingTime_M.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxChargingTime_M.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMaxChargingTime_M.IntValue = 90;
            this.txtMaxChargingTime_M.IsScaled = false;
            this.txtMaxChargingTime_M.Location = new System.Drawing.Point(471, 39);
            this.txtMaxChargingTime_M.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxChargingTime_M.Maximum = 2147483647D;
            this.txtMaxChargingTime_M.Minimum = -2147483648D;
            this.txtMaxChargingTime_M.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxChargingTime_M.Name = "txtMaxChargingTime_M";
            this.txtMaxChargingTime_M.Size = new System.Drawing.Size(90, 25);
            this.txtMaxChargingTime_M.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxChargingTime_M.TabIndex = 5;
            this.txtMaxChargingTime_M.Text = "90";
            this.txtMaxChargingTime_M.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxChargingTime_M.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel5
            // 
            this.uiLabel5.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel5.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel5.IsScaled = false;
            this.uiLabel5.Location = new System.Drawing.Point(285, 39);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(179, 25);
            this.uiLabel5.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel5.TabIndex = 4;
            this.uiLabel5.Text = "最大充电时间(0~255分钟):";
            this.uiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMaxChargingTime_S
            // 
            this.txtMaxChargingTime_S.ButtonSymbol = 61761;
            this.txtMaxChargingTime_S.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxChargingTime_S.DoubleValue = 90D;
            this.txtMaxChargingTime_S.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxChargingTime_S.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMaxChargingTime_S.IntValue = 90;
            this.txtMaxChargingTime_S.IsScaled = false;
            this.txtMaxChargingTime_S.Location = new System.Drawing.Point(189, 39);
            this.txtMaxChargingTime_S.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxChargingTime_S.Maximum = 2147483647D;
            this.txtMaxChargingTime_S.Minimum = -2147483648D;
            this.txtMaxChargingTime_S.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxChargingTime_S.Name = "txtMaxChargingTime_S";
            this.txtMaxChargingTime_S.Size = new System.Drawing.Size(90, 25);
            this.txtMaxChargingTime_S.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxChargingTime_S.TabIndex = 3;
            this.txtMaxChargingTime_S.Text = "90";
            this.txtMaxChargingTime_S.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxChargingTime_S.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel6
            // 
            this.uiLabel6.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel6.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel6.IsScaled = false;
            this.uiLabel6.Location = new System.Drawing.Point(9, 39);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(173, 25);
            this.uiLabel6.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel6.TabIndex = 2;
            this.uiLabel6.Text = "最大充电时间(0~2540秒):";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.Controls.Add(this.uiLabel20);
            this.uiGroupBox3.Controls.Add(this.cmbVehicleStatus);
            this.uiGroupBox3.Controls.Add(this.uiLabel19);
            this.uiGroupBox3.Controls.Add(this.cmbNormalStop);
            this.uiGroupBox3.Controls.Add(this.uiLabel18);
            this.uiGroupBox3.Controls.Add(this.cmbSystemFault);
            this.uiGroupBox3.Controls.Add(this.uiLabel17);
            this.uiGroupBox3.Controls.Add(this.cmbShiftPosition);
            this.uiGroupBox3.Controls.Add(this.uiLabel16);
            this.uiGroupBox3.Controls.Add(this.cmbChargingEnabled);
            this.uiGroupBox3.Controls.Add(this.uiLabel15);
            this.uiGroupBox3.Controls.Add(this.cmbBatteryVoltError);
            this.uiGroupBox3.Controls.Add(this.uiLabel14);
            this.uiGroupBox3.Controls.Add(this.cmbBatteryCurrentError);
            this.uiGroupBox3.Controls.Add(this.uiLabel13);
            this.uiGroupBox3.Controls.Add(this.cmbBatteryTempHight);
            this.uiGroupBox3.Controls.Add(this.uiLabel12);
            this.uiGroupBox3.Controls.Add(this.cmbBatteryUnderVolt);
            this.uiGroupBox3.Controls.Add(this.uiLabel11);
            this.uiGroupBox3.Controls.Add(this.cmbBatteryOverVolt);
            this.uiGroupBox3.Controls.Add(this.txtChargingCurrent);
            this.uiGroupBox3.Controls.Add(this.uiLabel10);
            this.uiGroupBox3.Controls.Add(this.txtChargingRate);
            this.uiGroupBox3.Controls.Add(this.uiLabel7);
            this.uiGroupBox3.Controls.Add(this.txtTargetBatteryVolt);
            this.uiGroupBox3.Controls.Add(this.uiLabel8);
            this.uiGroupBox3.Controls.Add(this.txtCHAdeMONumber);
            this.uiGroupBox3.Controls.Add(this.uiLabel9);
            this.uiGroupBox3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox3.IsScaled = false;
            this.uiGroupBox3.Location = new System.Drawing.Point(7, 216);
            this.uiGroupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.uiGroupBox3.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.uiGroupBox3.Size = new System.Drawing.Size(705, 270);
            this.uiGroupBox3.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox3.TabIndex = 46;
            this.uiGroupBox3.Text = "BMS充电报文数据(H102)";
            this.uiGroupBox3.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel20
            // 
            this.uiLabel20.AccessibleRole = System.Windows.Forms.AccessibleRole.OutlineButton;
            this.uiLabel20.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel20.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel20.IsScaled = false;
            this.uiLabel20.Location = new System.Drawing.Point(27, 216);
            this.uiLabel20.Name = "uiLabel20";
            this.uiLabel20.Size = new System.Drawing.Size(69, 25);
            this.uiLabel20.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel20.TabIndex = 27;
            this.uiLabel20.Text = "车辆状态:";
            this.uiLabel20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbVehicleStatus
            // 
            this.cmbVehicleStatus.DataSource = null;
            this.cmbVehicleStatus.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbVehicleStatus.FillColor = System.Drawing.Color.White;
            this.cmbVehicleStatus.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbVehicleStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbVehicleStatus.IsScaled = false;
            this.cmbVehicleStatus.Items.AddRange(new object[] {
            "EV接触器闭合或在焊接检测中",
            "EV接触器开路或焊接终止检测"});
            this.cmbVehicleStatus.Location = new System.Drawing.Point(103, 216);
            this.cmbVehicleStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbVehicleStatus.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbVehicleStatus.Name = "cmbVehicleStatus";
            this.cmbVehicleStatus.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbVehicleStatus.Size = new System.Drawing.Size(249, 25);
            this.cmbVehicleStatus.Style = Sunny.UI.UIStyle.Custom;
            this.cmbVehicleStatus.TabIndex = 26;
            this.cmbVehicleStatus.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel19
            // 
            this.uiLabel19.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel19.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel19.IsScaled = false;
            this.uiLabel19.Location = new System.Drawing.Point(218, 181);
            this.uiLabel19.Name = "uiLabel19";
            this.uiLabel19.Size = new System.Drawing.Size(139, 25);
            this.uiLabel19.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel19.TabIndex = 25;
            this.uiLabel19.Text = "充电前正常停止请求:";
            this.uiLabel19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbNormalStop
            // 
            this.cmbNormalStop.DataSource = null;
            this.cmbNormalStop.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbNormalStop.FillColor = System.Drawing.Color.White;
            this.cmbNormalStop.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbNormalStop.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbNormalStop.IsScaled = false;
            this.cmbNormalStop.Items.AddRange(new object[] {
            "无请求",
            "停止请求"});
            this.cmbNormalStop.Location = new System.Drawing.Point(364, 181);
            this.cmbNormalStop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbNormalStop.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbNormalStop.Name = "cmbNormalStop";
            this.cmbNormalStop.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbNormalStop.Size = new System.Drawing.Size(117, 25);
            this.cmbNormalStop.Style = Sunny.UI.UIStyle.Custom;
            this.cmbNormalStop.TabIndex = 24;
            this.cmbNormalStop.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel18
            // 
            this.uiLabel18.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel18.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel18.IsScaled = false;
            this.uiLabel18.Location = new System.Drawing.Point(466, 144);
            this.uiLabel18.Name = "uiLabel18";
            this.uiLabel18.Size = new System.Drawing.Size(131, 25);
            this.uiLabel18.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel18.TabIndex = 23;
            this.uiLabel18.Text = "充电系统故障:";
            this.uiLabel18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbSystemFault
            // 
            this.cmbSystemFault.DataSource = null;
            this.cmbSystemFault.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbSystemFault.FillColor = System.Drawing.Color.White;
            this.cmbSystemFault.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbSystemFault.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbSystemFault.IsScaled = false;
            this.cmbSystemFault.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbSystemFault.Location = new System.Drawing.Point(604, 144);
            this.cmbSystemFault.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbSystemFault.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbSystemFault.Name = "cmbSystemFault";
            this.cmbSystemFault.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbSystemFault.Size = new System.Drawing.Size(90, 25);
            this.cmbSystemFault.Style = Sunny.UI.UIStyle.Custom;
            this.cmbSystemFault.TabIndex = 22;
            this.cmbSystemFault.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel17
            // 
            this.uiLabel17.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel17.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel17.IsScaled = false;
            this.uiLabel17.Location = new System.Drawing.Point(369, 216);
            this.uiLabel17.Name = "uiLabel17";
            this.uiLabel17.Size = new System.Drawing.Size(131, 25);
            this.uiLabel17.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel17.TabIndex = 21;
            this.uiLabel17.Text = "车辆换挡位置:";
            this.uiLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbShiftPosition
            // 
            this.cmbShiftPosition.DataSource = null;
            this.cmbShiftPosition.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbShiftPosition.FillColor = System.Drawing.Color.White;
            this.cmbShiftPosition.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbShiftPosition.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbShiftPosition.IsScaled = false;
            this.cmbShiftPosition.Items.AddRange(new object[] {
            "“停车”位置",
            "其他位置"});
            this.cmbShiftPosition.Location = new System.Drawing.Point(507, 216);
            this.cmbShiftPosition.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbShiftPosition.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbShiftPosition.Name = "cmbShiftPosition";
            this.cmbShiftPosition.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbShiftPosition.Size = new System.Drawing.Size(124, 25);
            this.cmbShiftPosition.Style = Sunny.UI.UIStyle.Custom;
            this.cmbShiftPosition.TabIndex = 20;
            this.cmbShiftPosition.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel16
            // 
            this.uiLabel16.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel16.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel16.IsScaled = false;
            this.uiLabel16.Location = new System.Drawing.Point(466, 107);
            this.uiLabel16.Name = "uiLabel16";
            this.uiLabel16.Size = new System.Drawing.Size(131, 25);
            this.uiLabel16.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel16.TabIndex = 19;
            this.uiLabel16.Text = "开启车辆充电:";
            this.uiLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbChargingEnabled
            // 
            this.cmbChargingEnabled.DataSource = null;
            this.cmbChargingEnabled.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbChargingEnabled.FillColor = System.Drawing.Color.White;
            this.cmbChargingEnabled.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbChargingEnabled.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbChargingEnabled.IsScaled = false;
            this.cmbChargingEnabled.Items.AddRange(new object[] {
            "关闭",
            "开启"});
            this.cmbChargingEnabled.Location = new System.Drawing.Point(604, 107);
            this.cmbChargingEnabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbChargingEnabled.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbChargingEnabled.Name = "cmbChargingEnabled";
            this.cmbChargingEnabled.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbChargingEnabled.Size = new System.Drawing.Size(90, 25);
            this.cmbChargingEnabled.Style = Sunny.UI.UIStyle.Custom;
            this.cmbChargingEnabled.TabIndex = 18;
            this.cmbChargingEnabled.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel15
            // 
            this.uiLabel15.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel15.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel15.IsScaled = false;
            this.uiLabel15.Location = new System.Drawing.Point(226, 144);
            this.uiLabel15.Name = "uiLabel15";
            this.uiLabel15.Size = new System.Drawing.Size(131, 25);
            this.uiLabel15.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel15.TabIndex = 17;
            this.uiLabel15.Text = "电池电压偏差误差:";
            this.uiLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBatteryVoltError
            // 
            this.cmbBatteryVoltError.DataSource = null;
            this.cmbBatteryVoltError.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBatteryVoltError.FillColor = System.Drawing.Color.White;
            this.cmbBatteryVoltError.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBatteryVoltError.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbBatteryVoltError.IsScaled = false;
            this.cmbBatteryVoltError.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbBatteryVoltError.Location = new System.Drawing.Point(364, 144);
            this.cmbBatteryVoltError.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBatteryVoltError.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBatteryVoltError.Name = "cmbBatteryVoltError";
            this.cmbBatteryVoltError.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbBatteryVoltError.Size = new System.Drawing.Size(90, 25);
            this.cmbBatteryVoltError.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBatteryVoltError.TabIndex = 16;
            this.cmbBatteryVoltError.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel14
            // 
            this.uiLabel14.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel14.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel14.IsScaled = false;
            this.uiLabel14.Location = new System.Drawing.Point(230, 107);
            this.uiLabel14.Name = "uiLabel14";
            this.uiLabel14.Size = new System.Drawing.Size(127, 25);
            this.uiLabel14.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel14.TabIndex = 15;
            this.uiLabel14.Text = "电池电流偏差误差:";
            this.uiLabel14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBatteryCurrentError
            // 
            this.cmbBatteryCurrentError.DataSource = null;
            this.cmbBatteryCurrentError.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBatteryCurrentError.FillColor = System.Drawing.Color.White;
            this.cmbBatteryCurrentError.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBatteryCurrentError.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbBatteryCurrentError.IsScaled = false;
            this.cmbBatteryCurrentError.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbBatteryCurrentError.Location = new System.Drawing.Point(364, 107);
            this.cmbBatteryCurrentError.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBatteryCurrentError.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBatteryCurrentError.Name = "cmbBatteryCurrentError";
            this.cmbBatteryCurrentError.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbBatteryCurrentError.Size = new System.Drawing.Size(90, 25);
            this.cmbBatteryCurrentError.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBatteryCurrentError.TabIndex = 14;
            this.cmbBatteryCurrentError.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel13
            // 
            this.uiLabel13.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel13.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel13.IsScaled = false;
            this.uiLabel13.Location = new System.Drawing.Point(7, 181);
            this.uiLabel13.Name = "uiLabel13";
            this.uiLabel13.Size = new System.Drawing.Size(89, 25);
            this.uiLabel13.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel13.TabIndex = 13;
            this.uiLabel13.Text = "电池温度高:";
            this.uiLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBatteryTempHight
            // 
            this.cmbBatteryTempHight.DataSource = null;
            this.cmbBatteryTempHight.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBatteryTempHight.FillColor = System.Drawing.Color.White;
            this.cmbBatteryTempHight.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBatteryTempHight.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbBatteryTempHight.IsScaled = false;
            this.cmbBatteryTempHight.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbBatteryTempHight.Location = new System.Drawing.Point(103, 181);
            this.cmbBatteryTempHight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBatteryTempHight.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBatteryTempHight.Name = "cmbBatteryTempHight";
            this.cmbBatteryTempHight.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbBatteryTempHight.Size = new System.Drawing.Size(90, 25);
            this.cmbBatteryTempHight.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBatteryTempHight.TabIndex = 12;
            this.cmbBatteryTempHight.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel12
            // 
            this.uiLabel12.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel12.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel12.IsScaled = false;
            this.uiLabel12.Location = new System.Drawing.Point(23, 144);
            this.uiLabel12.Name = "uiLabel12";
            this.uiLabel12.Size = new System.Drawing.Size(73, 25);
            this.uiLabel12.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel12.TabIndex = 11;
            this.uiLabel12.Text = "电池欠压:";
            this.uiLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBatteryUnderVolt
            // 
            this.cmbBatteryUnderVolt.DataSource = null;
            this.cmbBatteryUnderVolt.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBatteryUnderVolt.FillColor = System.Drawing.Color.White;
            this.cmbBatteryUnderVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBatteryUnderVolt.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbBatteryUnderVolt.IsScaled = false;
            this.cmbBatteryUnderVolt.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbBatteryUnderVolt.Location = new System.Drawing.Point(103, 144);
            this.cmbBatteryUnderVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBatteryUnderVolt.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBatteryUnderVolt.Name = "cmbBatteryUnderVolt";
            this.cmbBatteryUnderVolt.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbBatteryUnderVolt.Size = new System.Drawing.Size(90, 25);
            this.cmbBatteryUnderVolt.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBatteryUnderVolt.TabIndex = 10;
            this.cmbBatteryUnderVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel11
            // 
            this.uiLabel11.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel11.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel11.IsScaled = false;
            this.uiLabel11.Location = new System.Drawing.Point(19, 107);
            this.uiLabel11.Name = "uiLabel11";
            this.uiLabel11.Size = new System.Drawing.Size(77, 25);
            this.uiLabel11.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel11.TabIndex = 9;
            this.uiLabel11.Text = "电池过压:";
            this.uiLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBatteryOverVolt
            // 
            this.cmbBatteryOverVolt.DataSource = null;
            this.cmbBatteryOverVolt.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBatteryOverVolt.FillColor = System.Drawing.Color.White;
            this.cmbBatteryOverVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBatteryOverVolt.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbBatteryOverVolt.IsScaled = false;
            this.cmbBatteryOverVolt.Items.AddRange(new object[] {
            "正常",
            "故障"});
            this.cmbBatteryOverVolt.Location = new System.Drawing.Point(103, 107);
            this.cmbBatteryOverVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBatteryOverVolt.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBatteryOverVolt.Name = "cmbBatteryOverVolt";
            this.cmbBatteryOverVolt.Padding = new System.Windows.Forms.Padding(10, 0, 30, 2);
            this.cmbBatteryOverVolt.Size = new System.Drawing.Size(90, 25);
            this.cmbBatteryOverVolt.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBatteryOverVolt.TabIndex = 8;
            this.cmbBatteryOverVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtChargingCurrent
            // 
            this.txtChargingCurrent.ButtonSymbol = 61761;
            this.txtChargingCurrent.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtChargingCurrent.DoubleValue = 20D;
            this.txtChargingCurrent.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtChargingCurrent.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtChargingCurrent.IntValue = 20;
            this.txtChargingCurrent.IsScaled = false;
            this.txtChargingCurrent.Location = new System.Drawing.Point(471, 72);
            this.txtChargingCurrent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChargingCurrent.Maximum = 2147483647D;
            this.txtChargingCurrent.Minimum = -2147483648D;
            this.txtChargingCurrent.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtChargingCurrent.Name = "txtChargingCurrent";
            this.txtChargingCurrent.Size = new System.Drawing.Size(90, 25);
            this.txtChargingCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.txtChargingCurrent.TabIndex = 7;
            this.txtChargingCurrent.Text = "20";
            this.txtChargingCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtChargingCurrent.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel10
            // 
            this.uiLabel10.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel10.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel10.IsScaled = false;
            this.uiLabel10.Location = new System.Drawing.Point(327, 72);
            this.uiLabel10.Name = "uiLabel10";
            this.uiLabel10.Size = new System.Drawing.Size(137, 25);
            this.uiLabel10.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel10.TabIndex = 6;
            this.uiLabel10.Text = "充电电流请求(A):";
            this.uiLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtChargingRate
            // 
            this.txtChargingRate.ButtonSymbol = 61761;
            this.txtChargingRate.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtChargingRate.DoubleValue = 69D;
            this.txtChargingRate.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtChargingRate.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtChargingRate.IntValue = 69;
            this.txtChargingRate.IsScaled = false;
            this.txtChargingRate.Location = new System.Drawing.Point(189, 72);
            this.txtChargingRate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChargingRate.Maximum = 2147483647D;
            this.txtChargingRate.Minimum = -2147483648D;
            this.txtChargingRate.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtChargingRate.Name = "txtChargingRate";
            this.txtChargingRate.Size = new System.Drawing.Size(90, 25);
            this.txtChargingRate.Style = Sunny.UI.UIStyle.Custom;
            this.txtChargingRate.TabIndex = 5;
            this.txtChargingRate.Text = "69";
            this.txtChargingRate.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtChargingRate.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel7
            // 
            this.uiLabel7.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel7.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel7.IsScaled = false;
            this.uiLabel7.Location = new System.Drawing.Point(30, 72);
            this.uiLabel7.Name = "uiLabel7";
            this.uiLabel7.Size = new System.Drawing.Size(152, 25);
            this.uiLabel7.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel7.TabIndex = 4;
            this.uiLabel7.Text = "充电率(%):";
            this.uiLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTargetBatteryVolt
            // 
            this.txtTargetBatteryVolt.ButtonSymbol = 61761;
            this.txtTargetBatteryVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTargetBatteryVolt.DoubleValue = 500D;
            this.txtTargetBatteryVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtTargetBatteryVolt.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtTargetBatteryVolt.IntValue = 500;
            this.txtTargetBatteryVolt.IsScaled = false;
            this.txtTargetBatteryVolt.Location = new System.Drawing.Point(471, 37);
            this.txtTargetBatteryVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTargetBatteryVolt.Maximum = 2147483647D;
            this.txtTargetBatteryVolt.Minimum = -2147483648D;
            this.txtTargetBatteryVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtTargetBatteryVolt.Name = "txtTargetBatteryVolt";
            this.txtTargetBatteryVolt.Size = new System.Drawing.Size(90, 25);
            this.txtTargetBatteryVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtTargetBatteryVolt.TabIndex = 5;
            this.txtTargetBatteryVolt.Text = "500";
            this.txtTargetBatteryVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtTargetBatteryVolt.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel8
            // 
            this.uiLabel8.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel8.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel8.IsScaled = false;
            this.uiLabel8.Location = new System.Drawing.Point(327, 37);
            this.uiLabel8.Name = "uiLabel8";
            this.uiLabel8.Size = new System.Drawing.Size(137, 25);
            this.uiLabel8.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel8.TabIndex = 4;
            this.uiLabel8.Text = "目标电池电压(V):";
            this.uiLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCHAdeMONumber
            // 
            this.txtCHAdeMONumber.ButtonSymbol = 61761;
            this.txtCHAdeMONumber.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCHAdeMONumber.DoubleValue = 2D;
            this.txtCHAdeMONumber.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtCHAdeMONumber.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtCHAdeMONumber.IntValue = 2;
            this.txtCHAdeMONumber.IsScaled = false;
            this.txtCHAdeMONumber.Location = new System.Drawing.Point(189, 37);
            this.txtCHAdeMONumber.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCHAdeMONumber.Maximum = 2147483647D;
            this.txtCHAdeMONumber.Minimum = -2147483648D;
            this.txtCHAdeMONumber.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtCHAdeMONumber.Name = "txtCHAdeMONumber";
            this.txtCHAdeMONumber.Size = new System.Drawing.Size(90, 25);
            this.txtCHAdeMONumber.Style = Sunny.UI.UIStyle.Custom;
            this.txtCHAdeMONumber.TabIndex = 3;
            this.txtCHAdeMONumber.Text = "2";
            this.txtCHAdeMONumber.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtCHAdeMONumber.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel9
            // 
            this.uiLabel9.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel9.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.uiLabel9.IsScaled = false;
            this.uiLabel9.Location = new System.Drawing.Point(20, 37);
            this.uiLabel9.Name = "uiLabel9";
            this.uiLabel9.Size = new System.Drawing.Size(162, 25);
            this.uiLabel9.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel9.TabIndex = 2;
            this.uiLabel9.Text = "CHAdeMO控制协议号:";
            this.uiLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSet
            // 
            this.btnSet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSet.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSet.IsScaled = false;
            this.btnSet.Location = new System.Drawing.Point(7, 494);
            this.btnSet.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(116, 37);
            this.btnSet.Style = Sunny.UI.UIStyle.Custom;
            this.btnSet.TabIndex = 47;
            this.btnSet.Text = "设置充电参数";
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn12.FillWeight = 1F;
            this.dataGridViewTextBoxColumn12.HeaderText = "";
            this.dataGridViewTextBoxColumn12.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn13.FillWeight = 4F;
            this.dataGridViewTextBoxColumn13.HeaderText = "";
            this.dataGridViewTextBoxColumn13.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.HeaderText = "";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn15.FillWeight = 4F;
            this.dataGridViewTextBoxColumn15.HeaderText = "";
            this.dataGridViewTextBoxColumn15.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn16.FillWeight = 1F;
            this.dataGridViewTextBoxColumn16.HeaderText = "";
            this.dataGridViewTextBoxColumn16.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn11.FillWeight = 4F;
            this.dataGridViewTextBoxColumn11.HeaderText = "";
            this.dataGridViewTextBoxColumn11.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // UcBMS_JP_DC
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.uiGroupBox3);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnStop);
            this.Name = "UcBMS_JP_DC";
            this.Load += new System.EventHandler(this.UcBMS_JP_DC_Load);
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private Sunny.UI.UIButton btnStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private Sunny.UI.UIButton btnRead;
        private Sunny.UI.UIButton btnStop;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UITextBox txtMinBatteryVolt;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UITextBox txtChargingRateConst;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UITextBox txtMaxBatteryVolt;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UITextBox txtChargingET;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UITextBox txtMaxChargingTime_M;
        private Sunny.UI.UILabel uiLabel5;
        private Sunny.UI.UITextBox txtMaxChargingTime_S;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UIGroupBox uiGroupBox3;
        private Sunny.UI.UITextBox txtChargingRate;
        private Sunny.UI.UILabel uiLabel7;
        private Sunny.UI.UITextBox txtTargetBatteryVolt;
        private Sunny.UI.UILabel uiLabel8;
        private Sunny.UI.UITextBox txtCHAdeMONumber;
        private Sunny.UI.UILabel uiLabel9;
        private Sunny.UI.UITextBox txtChargingCurrent;
        private Sunny.UI.UILabel uiLabel10;
        private Sunny.UI.UIComboBox cmbBatteryOverVolt;
        private Sunny.UI.UILabel uiLabel13;
        private Sunny.UI.UIComboBox cmbBatteryTempHight;
        private Sunny.UI.UILabel uiLabel12;
        private Sunny.UI.UIComboBox cmbBatteryUnderVolt;
        private Sunny.UI.UILabel uiLabel11;
        private Sunny.UI.UILabel uiLabel14;
        private Sunny.UI.UIComboBox cmbBatteryCurrentError;
        private Sunny.UI.UILabel uiLabel15;
        private Sunny.UI.UIComboBox cmbBatteryVoltError;
        private Sunny.UI.UILabel uiLabel19;
        private Sunny.UI.UIComboBox cmbNormalStop;
        private Sunny.UI.UILabel uiLabel18;
        private Sunny.UI.UIComboBox cmbSystemFault;
        private Sunny.UI.UILabel uiLabel17;
        private Sunny.UI.UIComboBox cmbShiftPosition;
        private Sunny.UI.UILabel uiLabel16;
        private Sunny.UI.UIComboBox cmbChargingEnabled;
        private Sunny.UI.UILabel uiLabel20;
        private Sunny.UI.UIComboBox cmbVehicleStatus;
        private Sunny.UI.UIButton btnSet;
    }
}
