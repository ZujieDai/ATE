namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmInfo
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmInfo));
            this.lblInfo = new Sunny.UI.UILabel();
            this.lbl = new Sunny.UI.UILabel();
            this.lblTime = new Sunny.UI.UILabel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new Sunny.UI.UIButton();
            this.btnTrue = new Sunny.UI.UIButton();
            this.btnFalse = new Sunny.UI.UIButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.pnlChargerID = new System.Windows.Forms.Panel();
            this.pnlInputInfo = new System.Windows.Forms.Panel();
            this.txtInput = new Sunny.UI.UITextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel_timeInput = new System.Windows.Forms.Panel();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.uiTextBox_inTime6 = new Sunny.UI.UITextBox();
            this.uiTextBox_inTime5 = new Sunny.UI.UITextBox();
            this.uiTextBox_inTime4 = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiTextBox_inTime3 = new Sunny.UI.UITextBox();
            this.uiTextBox_inTime2 = new Sunny.UI.UITextBox();
            this.uiTextBox_inTime1 = new Sunny.UI.UITextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.pnlChargerID.SuspendLayout();
            this.pnlInputInfo.SuspendLayout();
            this.panel_timeInput.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.IsScaled = false;
            this.lblInfo.Location = new System.Drawing.Point(34, 40);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(636, 118);
            this.lblInfo.Style = Sunny.UI.UIStyle.Custom;
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "提示信息";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl
            // 
            this.lbl.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl.IsScaled = false;
            this.lbl.Location = new System.Drawing.Point(26, 19);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(100, 46);
            this.lbl.Style = Sunny.UI.UIStyle.Custom;
            this.lbl.TabIndex = 1;
            this.lbl.Text = "倒计时:";
            this.lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTime
            // 
            this.lblTime.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTime.IsScaled = false;
            this.lblTime.Location = new System.Drawing.Point(145, 11);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(127, 63);
            this.lblTime.Style = Sunny.UI.UIStyle.Custom;
            this.lblTime.TabIndex = 2;
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(320, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "秒";
            // 
            // btnOK
            // 
            this.btnOK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnOK.IsScaled = false;
            this.btnOK.Location = new System.Drawing.Point(297, 415);
            this.btnOK.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(94, 31);
            this.btnOK.Style = Sunny.UI.UIStyle.Custom;
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确定";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnTrue
            // 
            this.btnTrue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrue.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnTrue.IsScaled = false;
            this.btnTrue.Location = new System.Drawing.Point(173, 415);
            this.btnTrue.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnTrue.Name = "btnTrue";
            this.btnTrue.Size = new System.Drawing.Size(94, 31);
            this.btnTrue.Style = Sunny.UI.UIStyle.Custom;
            this.btnTrue.TabIndex = 6;
            this.btnTrue.Text = "是";
            this.btnTrue.Click += new System.EventHandler(this.btnTrue_Click);
            // 
            // btnFalse
            // 
            this.btnFalse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFalse.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnFalse.IsScaled = false;
            this.btnFalse.Location = new System.Drawing.Point(429, 415);
            this.btnFalse.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnFalse.Name = "btnFalse";
            this.btnFalse.Size = new System.Drawing.Size(94, 31);
            this.btnFalse.Style = Sunny.UI.UIStyle.Custom;
            this.btnFalse.TabIndex = 7;
            this.btnFalse.Text = "否";
            this.btnFalse.Click += new System.EventHandler(this.btnFalse_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl);
            this.panel1.Controls.Add(this.lblTime);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(141, 304);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(382, 82);
            this.panel1.TabIndex = 8;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Checked = true;
            this.checkBox8.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox8.Location = new System.Drawing.Point(250, 90);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(70, 25);
            this.checkBox8.TabIndex = 109;
            this.checkBox8.Text = "8号桩";
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.Visible = false;
            this.checkBox8.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Checked = true;
            this.checkBox7.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox7.Location = new System.Drawing.Point(170, 90);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(70, 25);
            this.checkBox7.TabIndex = 109;
            this.checkBox7.Text = "7号桩";
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.Visible = false;
            this.checkBox7.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Checked = true;
            this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox6.Location = new System.Drawing.Point(90, 90);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(70, 25);
            this.checkBox6.TabIndex = 109;
            this.checkBox6.Text = "6号桩";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.Visible = false;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Checked = true;
            this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox5.Location = new System.Drawing.Point(10, 90);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(70, 25);
            this.checkBox5.TabIndex = 109;
            this.checkBox5.Text = "5号桩";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.Visible = false;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Location = new System.Drawing.Point(250, 10);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(70, 25);
            this.checkBox4.TabIndex = 12;
            this.checkBox4.Text = "4号桩";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.Visible = false;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(170, 10);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(70, 25);
            this.checkBox3.TabIndex = 11;
            this.checkBox3.Text = "3号桩";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Visible = false;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(90, 10);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(70, 25);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "2号桩";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Visible = false;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(10, 10);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(70, 25);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "1号桩";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // pnlChargerID
            // 
            this.pnlChargerID.Controls.Add(this.checkBox8);
            this.pnlChargerID.Controls.Add(this.checkBox7);
            this.pnlChargerID.Controls.Add(this.checkBox6);
            this.pnlChargerID.Controls.Add(this.checkBox5);
            this.pnlChargerID.Controls.Add(this.checkBox4);
            this.pnlChargerID.Controls.Add(this.checkBox3);
            this.pnlChargerID.Controls.Add(this.checkBox2);
            this.pnlChargerID.Controls.Add(this.checkBox1);
            this.pnlChargerID.Enabled = false;
            this.pnlChargerID.Location = new System.Drawing.Point(3, 297);
            this.pnlChargerID.Name = "pnlChargerID";
            this.pnlChargerID.Size = new System.Drawing.Size(330, 141);
            this.pnlChargerID.TabIndex = 13;
            // 
            // pnlInputInfo
            // 
            this.pnlInputInfo.Controls.Add(this.txtInput);
            this.pnlInputInfo.Location = new System.Drawing.Point(3, 150);
            this.pnlInputInfo.Name = "pnlInputInfo";
            this.pnlInputInfo.Size = new System.Drawing.Size(310, 141);
            this.pnlInputInfo.TabIndex = 14;
            // 
            // txtInput
            // 
            this.txtInput.ButtonSymbol = 61761;
            this.txtInput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtInput.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtInput.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInput.IsScaled = false;
            this.txtInput.Location = new System.Drawing.Point(62, 47);
            this.txtInput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtInput.Maximum = 2147483647D;
            this.txtInput.Minimum = -2147483648D;
            this.txtInput.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(173, 44);
            this.txtInput.Style = Sunny.UI.UIStyle.Custom;
            this.txtInput.TabIndex = 0;
            this.txtInput.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // panel_timeInput
            // 
            this.panel_timeInput.Controls.Add(this.uiLabel3);
            this.panel_timeInput.Controls.Add(this.uiLabel4);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime6);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime5);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime4);
            this.panel_timeInput.Controls.Add(this.uiLabel2);
            this.panel_timeInput.Controls.Add(this.uiLabel1);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime3);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime2);
            this.panel_timeInput.Controls.Add(this.uiTextBox_inTime1);
            this.panel_timeInput.Location = new System.Drawing.Point(3, 3);
            this.panel_timeInput.Name = "panel_timeInput";
            this.panel_timeInput.Size = new System.Drawing.Size(379, 141);
            this.panel_timeInput.TabIndex = 15;
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(246, 76);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(30, 46);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 9;
            this.uiLabel3.Text = ":";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel4
            // 
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.IsScaled = false;
            this.uiLabel4.Location = new System.Drawing.Point(117, 72);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(30, 46);
            this.uiLabel4.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel4.TabIndex = 8;
            this.uiLabel4.Text = ":";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime6
            // 
            this.uiTextBox_inTime6.ButtonSymbol = 61761;
            this.uiTextBox_inTime6.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime6.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime6.IsScaled = false;
            this.uiTextBox_inTime6.Location = new System.Drawing.Point(285, 76);
            this.uiTextBox_inTime6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime6.Maximum = 2147483647D;
            this.uiTextBox_inTime6.Minimum = -2147483648D;
            this.uiTextBox_inTime6.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime6.Name = "uiTextBox_inTime6";
            this.uiTextBox_inTime6.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime6.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime6.TabIndex = 7;
            this.uiTextBox_inTime6.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime5
            // 
            this.uiTextBox_inTime5.ButtonSymbol = 61761;
            this.uiTextBox_inTime5.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime5.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime5.IsScaled = false;
            this.uiTextBox_inTime5.Location = new System.Drawing.Point(156, 74);
            this.uiTextBox_inTime5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime5.Maximum = 2147483647D;
            this.uiTextBox_inTime5.Minimum = -2147483648D;
            this.uiTextBox_inTime5.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime5.Name = "uiTextBox_inTime5";
            this.uiTextBox_inTime5.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime5.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime5.TabIndex = 6;
            this.uiTextBox_inTime5.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime4
            // 
            this.uiTextBox_inTime4.ButtonSymbol = 61761;
            this.uiTextBox_inTime4.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime4.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime4.IsScaled = false;
            this.uiTextBox_inTime4.Location = new System.Drawing.Point(27, 72);
            this.uiTextBox_inTime4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime4.Maximum = 2147483647D;
            this.uiTextBox_inTime4.Minimum = -2147483648D;
            this.uiTextBox_inTime4.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime4.Name = "uiTextBox_inTime4";
            this.uiTextBox_inTime4.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime4.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime4.TabIndex = 5;
            this.uiTextBox_inTime4.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(246, 22);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(30, 46);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 4;
            this.uiLabel2.Text = "-";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(117, 18);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(30, 46);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 3;
            this.uiLabel1.Text = "-";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime3
            // 
            this.uiTextBox_inTime3.ButtonSymbol = 61761;
            this.uiTextBox_inTime3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime3.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime3.IsScaled = false;
            this.uiTextBox_inTime3.Location = new System.Drawing.Point(285, 22);
            this.uiTextBox_inTime3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime3.Maximum = 2147483647D;
            this.uiTextBox_inTime3.Minimum = -2147483648D;
            this.uiTextBox_inTime3.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime3.Name = "uiTextBox_inTime3";
            this.uiTextBox_inTime3.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime3.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime3.TabIndex = 2;
            this.uiTextBox_inTime3.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime2
            // 
            this.uiTextBox_inTime2.ButtonSymbol = 61761;
            this.uiTextBox_inTime2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime2.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime2.IsScaled = false;
            this.uiTextBox_inTime2.Location = new System.Drawing.Point(156, 20);
            this.uiTextBox_inTime2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime2.Maximum = 2147483647D;
            this.uiTextBox_inTime2.Minimum = -2147483648D;
            this.uiTextBox_inTime2.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime2.Name = "uiTextBox_inTime2";
            this.uiTextBox_inTime2.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime2.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime2.TabIndex = 1;
            this.uiTextBox_inTime2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTextBox_inTime1
            // 
            this.uiTextBox_inTime1.ButtonSymbol = 61761;
            this.uiTextBox_inTime1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox_inTime1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.uiTextBox_inTime1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox_inTime1.IsScaled = false;
            this.uiTextBox_inTime1.Location = new System.Drawing.Point(27, 18);
            this.uiTextBox_inTime1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox_inTime1.Maximum = 2147483647D;
            this.uiTextBox_inTime1.Minimum = -2147483648D;
            this.uiTextBox_inTime1.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox_inTime1.Name = "uiTextBox_inTime1";
            this.uiTextBox_inTime1.Size = new System.Drawing.Size(81, 44);
            this.uiTextBox_inTime1.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox_inTime1.TabIndex = 0;
            this.uiTextBox_inTime1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.panel_timeInput);
            this.flowLayoutPanel1.Controls.Add(this.pnlInputInfo);
            this.flowLayoutPanel1.Controls.Add(this.pnlChargerID);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(141, 159);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(382, 141);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // FrmInfo
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 533);
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnFalse);
            this.Controls.Add(this.btnTrue);
            this.Controls.Add(this.btnOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmInfo";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "操作提示";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmInfo_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlChargerID.ResumeLayout(false);
            this.pnlChargerID.PerformLayout();
            this.pnlInputInfo.ResumeLayout(false);
            this.panel_timeInput.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UILabel lblInfo;
        private Sunny.UI.UILabel lbl;
        private Sunny.UI.UILabel lblTime;
        private System.Windows.Forms.Label label1;
        private Sunny.UI.UIButton btnOK;
        private Sunny.UI.UIButton btnTrue;
        private Sunny.UI.UIButton btnFalse;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Panel pnlChargerID;
        private System.Windows.Forms.Panel pnlInputInfo;
        private Sunny.UI.UITextBox txtInput;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel panel_timeInput;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UITextBox uiTextBox_inTime6;
        private Sunny.UI.UITextBox uiTextBox_inTime5;
        private Sunny.UI.UITextBox uiTextBox_inTime4;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UITextBox uiTextBox_inTime3;
        private Sunny.UI.UITextBox uiTextBox_inTime2;
        private Sunny.UI.UITextBox uiTextBox_inTime1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}