namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmSchemeEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSchemeEdit));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbAll = new Sunny.UI.UIListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbSelect = new Sunny.UI.UIListBox();
            this.btnAdd = new Sunny.UI.UIButton();
            this.btnDel = new Sunny.UI.UIButton();
            this.btnAll = new Sunny.UI.UIButton();
            this.btnClear = new Sunny.UI.UIButton();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new Sunny.UI.UITextBox();
            this.btnSave = new Sunny.UI.UIButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnUp = new Sunny.UI.UIButton();
            this.btnDown = new Sunny.UI.UIButton();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbScheme = new Sunny.UI.UIComboBox();
            this.btnDelScheme = new Sunny.UI.UIButton();
            this.cmbStandard = new Sunny.UI.UIComboBox();
            this.lblVolt = new Sunny.UI.UILabel();
            this.txtMaxVoltage = new Sunny.UI.UITextBox();
            this.lblCurrent = new Sunny.UI.UILabel();
            this.txtRateCurrent = new Sunny.UI.UITextBox();
            this.cmbType = new Sunny.UI.UIComboBox();
            this.lblType = new Sunny.UI.UILabel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbAll);
            this.panel1.Location = new System.Drawing.Point(15, 93);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(336, 589);
            this.panel1.TabIndex = 0;
            // 
            // lbAll
            // 
            this.lbAll.AutoScroll = true;
            this.lbAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbAll.FillColor = System.Drawing.Color.White;
            this.lbAll.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.lbAll.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbAll.FormatString = "";
            this.lbAll.IsScaled = false;
            this.lbAll.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.lbAll.Location = new System.Drawing.Point(0, 0);
            this.lbAll.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbAll.MinimumSize = new System.Drawing.Size(1, 1);
            this.lbAll.Name = "lbAll";
            this.lbAll.Padding = new System.Windows.Forms.Padding(2);
            this.lbAll.Size = new System.Drawing.Size(336, 589);
            this.lbAll.Style = Sunny.UI.UIStyle.Custom;
            this.lbAll.TabIndex = 0;
            this.lbAll.Text = null;
            this.lbAll.ItemDoubleClick += new System.EventHandler(this.lbAll_ItemDoubleClick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lbSelect);
            this.panel2.Location = new System.Drawing.Point(534, 93);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(336, 587);
            this.panel2.TabIndex = 1;
            // 
            // lbSelect
            // 
            this.lbSelect.AutoScroll = true;
            this.lbSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSelect.FillColor = System.Drawing.Color.White;
            this.lbSelect.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.lbSelect.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbSelect.FormatString = "";
            this.lbSelect.IsScaled = false;
            this.lbSelect.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.lbSelect.Location = new System.Drawing.Point(0, 0);
            this.lbSelect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbSelect.MinimumSize = new System.Drawing.Size(1, 1);
            this.lbSelect.Name = "lbSelect";
            this.lbSelect.Padding = new System.Windows.Forms.Padding(2);
            this.lbSelect.Size = new System.Drawing.Size(336, 587);
            this.lbSelect.Style = Sunny.UI.UIStyle.Custom;
            this.lbSelect.TabIndex = 1;
            this.lbSelect.Text = null;
            this.lbSelect.ItemDoubleClick += new System.EventHandler(this.lbSelect_ItemDoubleClick);
            // 
            // btnAdd
            // 
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAdd.IsScaled = false;
            this.btnAdd.Location = new System.Drawing.Point(379, 124);
            this.btnAdd.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(121, 48);
            this.btnAdd.Style = Sunny.UI.UIStyle.Custom;
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "增加-->";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDel
            // 
            this.btnDel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDel.IsScaled = false;
            this.btnDel.Location = new System.Drawing.Point(379, 239);
            this.btnDel.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(121, 48);
            this.btnDel.Style = Sunny.UI.UIStyle.Custom;
            this.btnDel.TabIndex = 3;
            this.btnDel.Text = "<--删除";
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // btnAll
            // 
            this.btnAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAll.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAll.IsScaled = false;
            this.btnAll.Location = new System.Drawing.Point(379, 348);
            this.btnAll.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(121, 48);
            this.btnAll.Style = Sunny.UI.UIStyle.Custom;
            this.btnAll.TabIndex = 4;
            this.btnAll.Text = "全选";
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnClear
            // 
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClear.IsScaled = false;
            this.btnClear.Location = new System.Drawing.Point(379, 455);
            this.btnClear.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(121, 48);
            this.btnClear.Style = Sunny.UI.UIStyle.Custom;
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "清空";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 700);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 27);
            this.label1.TabIndex = 6;
            this.label1.Text = "新建方案名称：";
            // 
            // txtName
            // 
            this.txtName.ButtonSymbol = 61761;
            this.txtName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtName.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtName.IsScaled = false;
            this.txtName.Location = new System.Drawing.Point(228, 690);
            this.txtName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtName.Maximum = 2147483647D;
            this.txtName.Minimum = -2147483648D;
            this.txtName.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(383, 43);
            this.txtName.Style = Sunny.UI.UIStyle.Custom;
            this.txtName.TabIndex = 7;
            this.txtName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSave
            // 
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSave.IsScaled = false;
            this.btnSave.Location = new System.Drawing.Point(685, 690);
            this.btnSave.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(121, 48);
            this.btnSave.Style = Sunny.UI.UIStyle.Custom;
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "保存方案";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 27);
            this.label2.TabIndex = 9;
            this.label2.Text = "可用测试项：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(541, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 27);
            this.label3.TabIndex = 10;
            this.label3.Text = "新方案使用测试项：";
            // 
            // btnUp
            // 
            this.btnUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUp.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnUp.IsScaled = false;
            this.btnUp.Location = new System.Drawing.Point(876, 209);
            this.btnUp.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(50, 78);
            this.btnUp.Style = Sunny.UI.UIStyle.Custom;
            this.btnUp.TabIndex = 11;
            this.btnUp.Text = "↑";
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDown.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDown.IsScaled = false;
            this.btnDown.Location = new System.Drawing.Point(877, 423);
            this.btnDown.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(49, 80);
            this.btnDown.Style = Sunny.UI.UIStyle.Custom;
            this.btnDown.TabIndex = 12;
            this.btnDown.Text = "↓";
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 791);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 27);
            this.label4.TabIndex = 13;
            this.label4.Text = "已有方案名称列表：";
            // 
            // cmbScheme
            // 
            this.cmbScheme.DataSource = null;
            this.cmbScheme.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbScheme.FillColor = System.Drawing.Color.White;
            this.cmbScheme.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbScheme.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbScheme.IsScaled = false;
            this.cmbScheme.Location = new System.Drawing.Point(224, 780);
            this.cmbScheme.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbScheme.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbScheme.Name = "cmbScheme";
            this.cmbScheme.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbScheme.Size = new System.Drawing.Size(386, 47);
            this.cmbScheme.Style = Sunny.UI.UIStyle.Custom;
            this.cmbScheme.TabIndex = 14;
            this.cmbScheme.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDelScheme
            // 
            this.btnDelScheme.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelScheme.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDelScheme.IsScaled = false;
            this.btnDelScheme.Location = new System.Drawing.Point(685, 779);
            this.btnDelScheme.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnDelScheme.Name = "btnDelScheme";
            this.btnDelScheme.Size = new System.Drawing.Size(121, 48);
            this.btnDelScheme.Style = Sunny.UI.UIStyle.Custom;
            this.btnDelScheme.TabIndex = 15;
            this.btnDelScheme.Text = "删除方案";
            this.btnDelScheme.Click += new System.EventHandler(this.btnDelScheme_Click);
            // 
            // cmbStandard
            // 
            this.cmbStandard.DataSource = null;
            this.cmbStandard.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbStandard.FillColor = System.Drawing.Color.White;
            this.cmbStandard.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbStandard.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbStandard.IsScaled = false;
            this.cmbStandard.Location = new System.Drawing.Point(160, 50);
            this.cmbStandard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbStandard.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbStandard.Name = "cmbStandard";
            this.cmbStandard.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbStandard.Size = new System.Drawing.Size(191, 38);
            this.cmbStandard.Style = Sunny.UI.UIStyle.Custom;
            this.cmbStandard.TabIndex = 15;
            this.cmbStandard.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbStandard.SelectedIndexChanged += new System.EventHandler(this.cmbStandard_SelectedIndexChanged);
            // 
            // lblVolt
            // 
            this.lblVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVolt.IsScaled = false;
            this.lblVolt.Location = new System.Drawing.Point(25, 846);
            this.lblVolt.Name = "lblVolt";
            this.lblVolt.Size = new System.Drawing.Size(144, 42);
            this.lblVolt.Style = Sunny.UI.UIStyle.Custom;
            this.lblVolt.TabIndex = 83;
            this.lblVolt.Text = "额定电压(V)：";
            this.lblVolt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblVolt.Visible = false;
            // 
            // txtMaxVoltage
            // 
            this.txtMaxVoltage.ButtonSymbol = 61761;
            this.txtMaxVoltage.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxVoltage.DoubleValue = 220D;
            this.txtMaxVoltage.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxVoltage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtMaxVoltage.HasMaximum = true;
            this.txtMaxVoltage.HasMinimum = true;
            this.txtMaxVoltage.IntValue = 220;
            this.txtMaxVoltage.IsScaled = false;
            this.txtMaxVoltage.Location = new System.Drawing.Point(176, 852);
            this.txtMaxVoltage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxVoltage.Maximum = 1500D;
            this.txtMaxVoltage.MaximumEnabled = true;
            this.txtMaxVoltage.MaxLength = 4;
            this.txtMaxVoltage.Minimum = 110D;
            this.txtMaxVoltage.MinimumEnabled = true;
            this.txtMaxVoltage.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxVoltage.Name = "txtMaxVoltage";
            this.txtMaxVoltage.Size = new System.Drawing.Size(101, 36);
            this.txtMaxVoltage.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxVoltage.TabIndex = 84;
            this.txtMaxVoltage.Text = "220";
            this.txtMaxVoltage.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxVoltage.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            this.txtMaxVoltage.Visible = false;
            // 
            // lblCurrent
            // 
            this.lblCurrent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrent.IsScaled = false;
            this.lblCurrent.Location = new System.Drawing.Point(295, 851);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(143, 39);
            this.lblCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.lblCurrent.TabIndex = 85;
            this.lblCurrent.Text = "额定电流(A)：";
            this.lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCurrent.Visible = false;
            // 
            // txtRateCurrent
            // 
            this.txtRateCurrent.ButtonSymbol = 61761;
            this.txtRateCurrent.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtRateCurrent.DoubleValue = 32D;
            this.txtRateCurrent.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtRateCurrent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRateCurrent.HasMaximum = true;
            this.txtRateCurrent.HasMinimum = true;
            this.txtRateCurrent.IntValue = 32;
            this.txtRateCurrent.IsScaled = false;
            this.txtRateCurrent.Location = new System.Drawing.Point(445, 854);
            this.txtRateCurrent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRateCurrent.Maximum = 500D;
            this.txtRateCurrent.MaximumEnabled = true;
            this.txtRateCurrent.MaxLength = 3;
            this.txtRateCurrent.Minimum = 8D;
            this.txtRateCurrent.MinimumEnabled = true;
            this.txtRateCurrent.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtRateCurrent.Name = "txtRateCurrent";
            this.txtRateCurrent.Size = new System.Drawing.Size(101, 36);
            this.txtRateCurrent.Style = Sunny.UI.UIStyle.Custom;
            this.txtRateCurrent.TabIndex = 86;
            this.txtRateCurrent.Text = "32";
            this.txtRateCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtRateCurrent.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            this.txtRateCurrent.Visible = false;
            // 
            // cmbType
            // 
            this.cmbType.DataSource = null;
            this.cmbType.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbType.FillColor = System.Drawing.Color.White;
            this.cmbType.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbType.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbType.IsScaled = false;
            this.cmbType.Location = new System.Drawing.Point(703, 852);
            this.cmbType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbType.MaxLength = 1;
            this.cmbType.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbType.Name = "cmbType";
            this.cmbType.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbType.Size = new System.Drawing.Size(217, 36);
            this.cmbType.Style = Sunny.UI.UIStyle.Custom;
            this.cmbType.TabIndex = 87;
            this.cmbType.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbType.Visible = false;
            // 
            // lblType
            // 
            this.lblType.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblType.IsScaled = false;
            this.lblType.Location = new System.Drawing.Point(573, 852);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(112, 36);
            this.lblType.Style = Sunny.UI.UIStyle.Custom;
            this.lblType.TabIndex = 88;
            this.lblType.Text = "产品类型：";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblType.Visible = false;
            // 
            // FrmSchemeEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 906);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblCurrent);
            this.Controls.Add(this.txtRateCurrent);
            this.Controls.Add(this.lblVolt);
            this.Controls.Add(this.txtMaxVoltage);
            this.Controls.Add(this.cmbStandard);
            this.Controls.Add(this.btnDelScheme);
            this.Controls.Add(this.cmbScheme);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2222, 2222);
            this.Name = "FrmSchemeEdit";
            this.Padding = new System.Windows.Forms.Padding(0, 40, 0, 0);
            this.RectColor = System.Drawing.Color.SteelBlue;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "方案编辑";
            this.TitleColor = System.Drawing.Color.SteelBlue;
            this.TitleHeight = 40;
            this.Load += new System.EventHandler(this.FrmSchemeEdit_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Sunny.UI.UIListBox lbAll;
        private System.Windows.Forms.Panel panel2;
        private Sunny.UI.UIListBox lbSelect;
        private Sunny.UI.UIButton btnAdd;
        private Sunny.UI.UIButton btnDel;
        private Sunny.UI.UIButton btnAll;
        private Sunny.UI.UIButton btnClear;
        private System.Windows.Forms.Label label1;
        private Sunny.UI.UITextBox txtName;
        private Sunny.UI.UIButton btnSave;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Sunny.UI.UIButton btnUp;
        private Sunny.UI.UIButton btnDown;
        private System.Windows.Forms.Label label4;
        private Sunny.UI.UIComboBox cmbScheme;
        private Sunny.UI.UIButton btnDelScheme;
        private Sunny.UI.UIComboBox cmbStandard;
        private Sunny.UI.UILabel lblVolt;
        private Sunny.UI.UITextBox txtMaxVoltage;
        private Sunny.UI.UILabel lblCurrent;
        private Sunny.UI.UITextBox txtRateCurrent;
        private Sunny.UI.UIComboBox cmbType;
        private Sunny.UI.UILabel lblType;
    }
}