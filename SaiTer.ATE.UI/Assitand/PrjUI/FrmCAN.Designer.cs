namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmCAN
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCAN));
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.switch_CAN = new Sunny.UI.UISwitch();
            this.dgvPacket = new Sunny.UI.UIDataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChargeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RecvTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TimeIncrement = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FrameID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DLC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Explain = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnPause = new Sunny.UI.UIButton();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClear = new Sunny.UI.UIButton();
            this.btnSave = new Sunny.UI.UIButton();
            this.cmbBMS = new Sunny.UI.UIComboBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.txtLogEU = new Sunny.UI.UIRichTextBox();
            this.btnDbglevel3 = new Sunny.UI.UIButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPacket)).BeginInit();
            this.SuspendLayout();
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(48, 76);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(101, 30);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 30;
            this.uiLabel3.Text = "CAN报文：";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // switch_CAN
            // 
            this.switch_CAN.ActiveText = "启动";
            this.switch_CAN.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.switch_CAN.InActiveText = "关闭";
            this.switch_CAN.IsScaled = false;
            this.switch_CAN.Location = new System.Drawing.Point(155, 78);
            this.switch_CAN.MinimumSize = new System.Drawing.Size(1, 1);
            this.switch_CAN.Name = "switch_CAN";
            this.switch_CAN.Size = new System.Drawing.Size(87, 28);
            this.switch_CAN.Style = Sunny.UI.UIStyle.Custom;
            this.switch_CAN.TabIndex = 29;
            this.switch_CAN.Text = "关闭";
            this.switch_CAN.ValueChanged += new Sunny.UI.UISwitch.OnValueChanged(this.switch_CAN_ValueChanged);
            // 
            // dgvPacket
            // 
            this.dgvPacket.AllowUserToAddRows = false;
            this.dgvPacket.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.dgvPacket.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPacket.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPacket.BackgroundColor = System.Drawing.Color.White;
            this.dgvPacket.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPacket.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPacket.ColumnHeadersHeight = 40;
            this.dgvPacket.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvPacket.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.ChargeID,
            this.RecvTime,
            this.TimeIncrement,
            this.FrameID,
            this.DLC,
            this.Data,
            this.Explain});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPacket.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvPacket.EnableHeadersVisualStyles = false;
            this.dgvPacket.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgvPacket.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.dgvPacket.Location = new System.Drawing.Point(14, 130);
            this.dgvPacket.Name = "dgvPacket";
            this.dgvPacket.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPacket.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvPacket.RowHeadersVisible = false;
            this.dgvPacket.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            this.dgvPacket.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvPacket.RowTemplate.Height = 23;
            this.dgvPacket.SelectedIndex = -1;
            this.dgvPacket.ShowGridLine = true;
            this.dgvPacket.Size = new System.Drawing.Size(1568, 827);
            this.dgvPacket.Style = Sunny.UI.UIStyle.Custom;
            this.dgvPacket.TabIndex = 31;
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "Order";
            this.Column1.HeaderText = "帧序号";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 125;
            // 
            // ChargeID
            // 
            this.ChargeID.DataPropertyName = "ChargeID";
            this.ChargeID.HeaderText = "枪号";
            this.ChargeID.MinimumWidth = 6;
            this.ChargeID.Name = "ChargeID";
            this.ChargeID.ReadOnly = true;
            this.ChargeID.Width = 50;
            // 
            // RecvTime
            // 
            this.RecvTime.DataPropertyName = "RecvTime";
            this.RecvTime.HeaderText = "接收时间";
            this.RecvTime.MinimumWidth = 6;
            this.RecvTime.Name = "RecvTime";
            this.RecvTime.ReadOnly = true;
            this.RecvTime.Width = 125;
            // 
            // TimeIncrement
            // 
            this.TimeIncrement.DataPropertyName = "TimeIncrement";
            this.TimeIncrement.HeaderText = "时间增量";
            this.TimeIncrement.MinimumWidth = 6;
            this.TimeIncrement.Name = "TimeIncrement";
            this.TimeIncrement.ReadOnly = true;
            this.TimeIncrement.Width = 125;
            // 
            // FrameID
            // 
            this.FrameID.DataPropertyName = "FrameID";
            this.FrameID.HeaderText = "帧ID";
            this.FrameID.MinimumWidth = 6;
            this.FrameID.Name = "FrameID";
            this.FrameID.ReadOnly = true;
            this.FrameID.Width = 125;
            // 
            // DLC
            // 
            this.DLC.DataPropertyName = "DLC";
            this.DLC.HeaderText = "DLC";
            this.DLC.MinimumWidth = 6;
            this.DLC.Name = "DLC";
            this.DLC.ReadOnly = true;
            this.DLC.Width = 40;
            // 
            // Data
            // 
            this.Data.DataPropertyName = "Data";
            this.Data.HeaderText = "数据";
            this.Data.MinimumWidth = 6;
            this.Data.Name = "Data";
            this.Data.ReadOnly = true;
            this.Data.Width = 300;
            // 
            // Explain
            // 
            this.Explain.DataPropertyName = "Explain";
            this.Explain.HeaderText = "BMS报文翻译";
            this.Explain.MinimumWidth = 6;
            this.Explain.Name = "Explain";
            this.Explain.ReadOnly = true;
            this.Explain.Width = 770;
            // 
            // btnPause
            // 
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnPause.IsScaled = false;
            this.btnPause.Location = new System.Drawing.Point(303, 67);
            this.btnPause.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(104, 42);
            this.btnPause.Style = Sunny.UI.UIStyle.Custom;
            this.btnPause.TabIndex = 78;
            this.btnPause.Text = "暂停显示";
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Mark";
            this.dataGridViewTextBoxColumn1.HeaderText = "收发标志";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "ChargeID";
            this.dataGridViewTextBoxColumn2.HeaderText = "枪号";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "RecvTime";
            this.dataGridViewTextBoxColumn3.HeaderText = "接收时间";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "TimeIncrement";
            this.dataGridViewTextBoxColumn4.HeaderText = "时间增量";
            this.dataGridViewTextBoxColumn4.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 125;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "FrameID";
            this.dataGridViewTextBoxColumn5.HeaderText = "帧ID";
            this.dataGridViewTextBoxColumn5.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 125;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "DLC";
            this.dataGridViewTextBoxColumn6.HeaderText = "DLC";
            this.dataGridViewTextBoxColumn6.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 125;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Data";
            this.dataGridViewTextBoxColumn7.HeaderText = "数据";
            this.dataGridViewTextBoxColumn7.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.ReadOnly = true;
            this.dataGridViewTextBoxColumn7.Width = 300;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "Explain";
            this.dataGridViewTextBoxColumn8.HeaderText = "BMS报文翻译";
            this.dataGridViewTextBoxColumn8.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.ReadOnly = true;
            this.dataGridViewTextBoxColumn8.Width = 150;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "Explain";
            this.dataGridViewTextBoxColumn9.HeaderText = "BMS报文翻译";
            this.dataGridViewTextBoxColumn9.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ReadOnly = true;
            this.dataGridViewTextBoxColumn9.Width = 150;
            // 
            // btnClear
            // 
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClear.IsScaled = false;
            this.btnClear.Location = new System.Drawing.Point(498, 67);
            this.btnClear.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(104, 42);
            this.btnClear.Style = Sunny.UI.UIStyle.Custom;
            this.btnClear.TabIndex = 79;
            this.btnClear.Text = "清空报文";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnSave
            // 
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSave.IsScaled = false;
            this.btnSave.Location = new System.Drawing.Point(688, 67);
            this.btnSave.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(104, 42);
            this.btnSave.Style = Sunny.UI.UIStyle.Custom;
            this.btnSave.TabIndex = 80;
            this.btnSave.Text = "保存报文";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbBMS
            // 
            this.cmbBMS.DataSource = null;
            this.cmbBMS.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbBMS.FillColor = System.Drawing.Color.White;
            this.cmbBMS.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbBMS.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbBMS.IsScaled = false;
            this.cmbBMS.Location = new System.Drawing.Point(1041, 73);
            this.cmbBMS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBMS.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbBMS.Name = "cmbBMS";
            this.cmbBMS.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbBMS.Size = new System.Drawing.Size(348, 33);
            this.cmbBMS.Style = Sunny.UI.UIStyle.Custom;
            this.cmbBMS.TabIndex = 81;
            this.cmbBMS.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbBMS.SelectedIndexChanged += new System.EventHandler(this.cmbBMS_SelectedIndexChanged);
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(933, 76);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(101, 30);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 82;
            this.uiLabel1.Text = "选择设备：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLogEU
            // 
            this.txtLogEU.AutoWordSelection = true;
            this.txtLogEU.FillColor = System.Drawing.Color.White;
            this.txtLogEU.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtLogEU.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLogEU.IsScaled = false;
            this.txtLogEU.Location = new System.Drawing.Point(14, 130);
            this.txtLogEU.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtLogEU.MinimumSize = new System.Drawing.Size(1, 1);
            this.txtLogEU.Name = "txtLogEU";
            this.txtLogEU.Padding = new System.Windows.Forms.Padding(2);
            this.txtLogEU.Size = new System.Drawing.Size(1568, 827);
            this.txtLogEU.Style = Sunny.UI.UIStyle.Custom;
            this.txtLogEU.TabIndex = 83;
            this.txtLogEU.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtLogEU.Visible = false;
            this.txtLogEU.WordWrap = true;
            // 
            // btnDbglevel3
            // 
            this.btnDbglevel3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDbglevel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDbglevel3.IsScaled = false;
            this.btnDbglevel3.Location = new System.Drawing.Point(53, 67);
            this.btnDbglevel3.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnDbglevel3.Name = "btnDbglevel3";
            this.btnDbglevel3.Size = new System.Drawing.Size(168, 42);
            this.btnDbglevel3.Style = Sunny.UI.UIStyle.Custom;
            this.btnDbglevel3.TabIndex = 84;
            this.btnDbglevel3.Text = "发送 dbglevel 3";
            this.btnDbglevel3.Visible = false;
            this.btnDbglevel3.Click += new System.EventHandler(this.btnDbglevel3_Click);
            // 
            // FrmCAN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1595, 969);
            this.Controls.Add(this.switch_CAN);
            this.Controls.Add(this.btnDbglevel3);
            this.Controls.Add(this.dgvPacket);
            this.Controls.Add(this.txtLogEU);
            this.Controls.Add(this.uiLabel1);
            this.Controls.Add(this.cmbBMS);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.uiLabel3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2222, 2222);
            this.Name = "FrmCAN";
            this.Padding = new System.Windows.Forms.Padding(0, 40, 0, 0);
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "实时报文";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.TitleHeight = 40;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmCAN_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmCAN_FormClosed);
            this.Load += new System.EventHandler(this.FrmCAN_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPacket)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UISwitch switch_CAN;
        private Sunny.UI.UIDataGridView dgvPacket;
        private Sunny.UI.UIButton btnPause;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChargeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn RecvTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeIncrement;
        private System.Windows.Forms.DataGridViewTextBoxColumn FrameID;
        private System.Windows.Forms.DataGridViewTextBoxColumn DLC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
        private System.Windows.Forms.DataGridViewTextBoxColumn Explain;
        private Sunny.UI.UIButton btnClear;
        private Sunny.UI.UIButton btnSave;
        private Sunny.UI.UIComboBox cmbBMS;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIRichTextBox txtLogEU;
        private Sunny.UI.UIButton btnDbglevel3;
    }
}