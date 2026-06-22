namespace SaiTer.ATE.UI.UserControls
{
    partial class UcChagerInfo
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
            this.uiLabel10 = new Sunny.UI.UILabel();
            this.txtMaxVoltage = new Sunny.UI.UITextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiLabel11 = new Sunny.UI.UILabel();
            this.txtRateCurrent = new Sunny.UI.UITextBox();
            this.uiLabel7 = new Sunny.UI.UILabel();
            this.lblFrequency = new Sunny.UI.UILabel();
            this.txtFreq = new Sunny.UI.UITextBox();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.lblMaxPower = new Sunny.UI.UILabel();
            this.lblMaxPowerSig = new Sunny.UI.UILabel();
            this.txtMaxPower = new Sunny.UI.UITextBox();
            this.lblMaxCurrSig = new Sunny.UI.UILabel();
            this.txtMaxCurr = new Sunny.UI.UITextBox();
            this.lblMaxCurr = new Sunny.UI.UILabel();
            this.lblMinVolt = new Sunny.UI.UILabel();
            this.txtMinVolt = new Sunny.UI.UITextBox();
            this.lblMinVoltSig = new Sunny.UI.UILabel();
            this.lblVolt = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlDC = new System.Windows.Forms.Panel();
            this.lbl1 = new Sunny.UI.UILabel();
            this.txtBarCode = new Sunny.UI.UITextBox();
            this.chbCharger = new Sunny.UI.UICheckBox();
            this.pnlBarcode = new System.Windows.Forms.Panel();
            this.pnlDC.SuspendLayout();
            this.pnlBarcode.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiLabel10
            // 
            this.uiLabel10.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel10.IsScaled = false;
            this.uiLabel10.Location = new System.Drawing.Point(20, 63);
            this.uiLabel10.Name = "uiLabel10";
            this.uiLabel10.Size = new System.Drawing.Size(144, 42);
            this.uiLabel10.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel10.TabIndex = 97;
            this.uiLabel10.Text = "额定电压(V)：";
            this.uiLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.txtMaxVoltage.Location = new System.Drawing.Point(193, 70);
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
            this.txtMaxVoltage.TabIndex = 98;
            this.txtMaxVoltage.Text = "220";
            this.txtMaxVoltage.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxVoltage.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            this.txtMaxVoltage.TextChanged += new System.EventHandler(this.txtMaxVoltage_TextChanged);
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(301, 71);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(40, 36);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 99;
            this.uiLabel3.Text = "*";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel11
            // 
            this.uiLabel11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel11.IsScaled = false;
            this.uiLabel11.Location = new System.Drawing.Point(367, 68);
            this.uiLabel11.Name = "uiLabel11";
            this.uiLabel11.Size = new System.Drawing.Size(143, 39);
            this.uiLabel11.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel11.TabIndex = 100;
            this.uiLabel11.Text = "额定电流(A)：";
            this.uiLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.txtRateCurrent.Location = new System.Drawing.Point(540, 69);
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
            this.txtRateCurrent.TabIndex = 101;
            this.txtRateCurrent.Text = "32";
            this.txtRateCurrent.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtRateCurrent.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel7
            // 
            this.uiLabel7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel7.IsScaled = false;
            this.uiLabel7.Location = new System.Drawing.Point(648, 71);
            this.uiLabel7.Name = "uiLabel7";
            this.uiLabel7.Size = new System.Drawing.Size(40, 36);
            this.uiLabel7.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel7.TabIndex = 102;
            this.uiLabel7.Text = "*";
            this.uiLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFrequency
            // 
            this.lblFrequency.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFrequency.IsScaled = false;
            this.lblFrequency.Location = new System.Drawing.Point(20, 117);
            this.lblFrequency.Name = "lblFrequency";
            this.lblFrequency.Size = new System.Drawing.Size(136, 39);
            this.lblFrequency.Style = Sunny.UI.UIStyle.Custom;
            this.lblFrequency.TabIndex = 108;
            this.lblFrequency.Text = "频率(Hz)：";
            this.lblFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.txtFreq.Location = new System.Drawing.Point(193, 123);
            this.txtFreq.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFreq.Maximum = 64D;
            this.txtFreq.MaximumEnabled = true;
            this.txtFreq.MaxLength = 2;
            this.txtFreq.Minimum = 0D;
            this.txtFreq.MinimumEnabled = true;
            this.txtFreq.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtFreq.Name = "txtFreq";
            this.txtFreq.Size = new System.Drawing.Size(101, 36);
            this.txtFreq.Style = Sunny.UI.UIStyle.Custom;
            this.txtFreq.TabIndex = 106;
            this.txtFreq.Text = "50";
            this.txtFreq.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtFreq.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel6
            // 
            this.uiLabel6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel6.IsScaled = false;
            this.uiLabel6.Location = new System.Drawing.Point(301, 121);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(40, 36);
            this.uiLabel6.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel6.TabIndex = 107;
            this.uiLabel6.Text = "*";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMaxPower
            // 
            this.lblMaxPower.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMaxPower.IsScaled = false;
            this.lblMaxPower.Location = new System.Drawing.Point(342, 4);
            this.lblMaxPower.Name = "lblMaxPower";
            this.lblMaxPower.Size = new System.Drawing.Size(153, 39);
            this.lblMaxPower.Style = Sunny.UI.UIStyle.Custom;
            this.lblMaxPower.TabIndex = 109;
            this.lblMaxPower.Text = "最大输出功率(KW):";
            this.lblMaxPower.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMaxPowerSig
            // 
            this.lblMaxPowerSig.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMaxPowerSig.IsScaled = false;
            this.lblMaxPowerSig.Location = new System.Drawing.Point(623, 2);
            this.lblMaxPowerSig.Name = "lblMaxPowerSig";
            this.lblMaxPowerSig.Size = new System.Drawing.Size(40, 36);
            this.lblMaxPowerSig.Style = Sunny.UI.UIStyle.Custom;
            this.lblMaxPowerSig.TabIndex = 111;
            this.lblMaxPowerSig.Text = "*";
            this.lblMaxPowerSig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaxPower
            // 
            this.txtMaxPower.ButtonSymbol = 61761;
            this.txtMaxPower.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxPower.DoubleValue = 160D;
            this.txtMaxPower.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxPower.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtMaxPower.HasMaximum = true;
            this.txtMaxPower.HasMinimum = true;
            this.txtMaxPower.IntValue = 160;
            this.txtMaxPower.IsScaled = false;
            this.txtMaxPower.Location = new System.Drawing.Point(515, 4);
            this.txtMaxPower.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxPower.Maximum = 600D;
            this.txtMaxPower.MaximumEnabled = true;
            this.txtMaxPower.MaxLength = 4;
            this.txtMaxPower.Minimum = 0D;
            this.txtMaxPower.MinimumEnabled = true;
            this.txtMaxPower.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxPower.Name = "txtMaxPower";
            this.txtMaxPower.Size = new System.Drawing.Size(101, 36);
            this.txtMaxPower.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxPower.TabIndex = 110;
            this.txtMaxPower.Text = "160";
            this.txtMaxPower.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxPower.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // lblMaxCurrSig
            // 
            this.lblMaxCurrSig.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMaxCurrSig.IsScaled = false;
            this.lblMaxCurrSig.Location = new System.Drawing.Point(623, 58);
            this.lblMaxCurrSig.Name = "lblMaxCurrSig";
            this.lblMaxCurrSig.Size = new System.Drawing.Size(40, 36);
            this.lblMaxCurrSig.Style = Sunny.UI.UIStyle.Custom;
            this.lblMaxCurrSig.TabIndex = 114;
            this.lblMaxCurrSig.Text = "*";
            this.lblMaxCurrSig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaxCurr
            // 
            this.txtMaxCurr.ButtonSymbol = 61761;
            this.txtMaxCurr.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMaxCurr.DoubleValue = 250D;
            this.txtMaxCurr.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMaxCurr.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtMaxCurr.HasMaximum = true;
            this.txtMaxCurr.HasMinimum = true;
            this.txtMaxCurr.IntValue = 250;
            this.txtMaxCurr.IsScaled = false;
            this.txtMaxCurr.Location = new System.Drawing.Point(515, 60);
            this.txtMaxCurr.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxCurr.Maximum = 500D;
            this.txtMaxCurr.MaximumEnabled = true;
            this.txtMaxCurr.MaxLength = 3;
            this.txtMaxCurr.Minimum = 0D;
            this.txtMaxCurr.MinimumEnabled = true;
            this.txtMaxCurr.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMaxCurr.Name = "txtMaxCurr";
            this.txtMaxCurr.Size = new System.Drawing.Size(101, 36);
            this.txtMaxCurr.Style = Sunny.UI.UIStyle.Custom;
            this.txtMaxCurr.TabIndex = 113;
            this.txtMaxCurr.Text = "250";
            this.txtMaxCurr.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMaxCurr.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // lblMaxCurr
            // 
            this.lblMaxCurr.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMaxCurr.IsScaled = false;
            this.lblMaxCurr.Location = new System.Drawing.Point(342, 56);
            this.lblMaxCurr.Name = "lblMaxCurr";
            this.lblMaxCurr.Size = new System.Drawing.Size(164, 39);
            this.lblMaxCurr.Style = Sunny.UI.UIStyle.Custom;
            this.lblMaxCurr.TabIndex = 112;
            this.lblMaxCurr.Text = "最大允许充电电流(A):";
            this.lblMaxCurr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMinVolt
            // 
            this.lblMinVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMinVolt.IsScaled = false;
            this.lblMinVolt.Location = new System.Drawing.Point(-5, 55);
            this.lblMinVolt.Name = "lblMinVolt";
            this.lblMinVolt.Size = new System.Drawing.Size(164, 39);
            this.lblMinVolt.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinVolt.TabIndex = 115;
            this.lblMinVolt.Text = "最小允许充电电压(V):";
            this.lblMinVolt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMinVolt
            // 
            this.txtMinVolt.ButtonSymbol = 61761;
            this.txtMinVolt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMinVolt.DoubleValue = 200D;
            this.txtMinVolt.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtMinVolt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtMinVolt.HasMaximum = true;
            this.txtMinVolt.HasMinimum = true;
            this.txtMinVolt.IntValue = 200;
            this.txtMinVolt.IsScaled = false;
            this.txtMinVolt.Location = new System.Drawing.Point(168, 59);
            this.txtMinVolt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinVolt.Maximum = 1000D;
            this.txtMinVolt.MaximumEnabled = true;
            this.txtMinVolt.MaxLength = 3;
            this.txtMinVolt.Minimum = 0D;
            this.txtMinVolt.MinimumEnabled = true;
            this.txtMinVolt.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMinVolt.Name = "txtMinVolt";
            this.txtMinVolt.Size = new System.Drawing.Size(101, 36);
            this.txtMinVolt.Style = Sunny.UI.UIStyle.Custom;
            this.txtMinVolt.TabIndex = 116;
            this.txtMinVolt.Text = "200";
            this.txtMinVolt.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMinVolt.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // lblMinVoltSig
            // 
            this.lblMinVoltSig.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMinVoltSig.IsScaled = false;
            this.lblMinVoltSig.Location = new System.Drawing.Point(276, 57);
            this.lblMinVoltSig.Name = "lblMinVoltSig";
            this.lblMinVoltSig.Size = new System.Drawing.Size(40, 36);
            this.lblMinVoltSig.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinVoltSig.TabIndex = 117;
            this.lblMinVoltSig.Text = "*";
            this.lblMinVoltSig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVolt
            // 
            this.lblVolt.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVolt.ForeColor = System.Drawing.Color.Red;
            this.lblVolt.Location = new System.Drawing.Point(161, 209);
            this.lblVolt.Name = "lblVolt";
            this.lblVolt.Size = new System.Drawing.Size(155, 44);
            this.lblVolt.TabIndex = 119;
            this.lblVolt.Text = "380V";
            this.lblVolt.Visible = false;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(73, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(358, 71);
            this.label1.TabIndex = 118;
            this.label1.Text = "重要：此处设置的电压为交流源输出的【相电压】。如果是美标接双相火线，输出的【线电压】约为\r\n";
            this.label1.Visible = false;
            // 
            // pnlDC
            // 
            this.pnlDC.Controls.Add(this.lblMinVolt);
            this.pnlDC.Controls.Add(this.txtMinVolt);
            this.pnlDC.Controls.Add(this.lblMinVoltSig);
            this.pnlDC.Controls.Add(this.lblMaxCurrSig);
            this.pnlDC.Controls.Add(this.txtMaxCurr);
            this.pnlDC.Controls.Add(this.lblMaxCurr);
            this.pnlDC.Controls.Add(this.lblMaxPower);
            this.pnlDC.Controls.Add(this.lblMaxPowerSig);
            this.pnlDC.Controls.Add(this.txtMaxPower);
            this.pnlDC.Location = new System.Drawing.Point(25, 117);
            this.pnlDC.Name = "pnlDC";
            this.pnlDC.Size = new System.Drawing.Size(686, 196);
            this.pnlDC.TabIndex = 120;
            // 
            // lbl1
            // 
            this.lbl1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl1.IsScaled = false;
            this.lbl1.Location = new System.Drawing.Point(630, 0);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(26, 36);
            this.lbl1.Style = Sunny.UI.UIStyle.Custom;
            this.lbl1.TabIndex = 123;
            this.lbl1.Text = "*";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBarCode
            // 
            this.txtBarCode.ButtonSymbol = 61761;
            this.txtBarCode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBarCode.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txtBarCode.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBarCode.IsScaled = false;
            this.txtBarCode.Location = new System.Drawing.Point(168, 0);
            this.txtBarCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtBarCode.Maximum = 2147483647D;
            this.txtBarCode.Minimum = -2147483648D;
            this.txtBarCode.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtBarCode.Name = "txtBarCode";
            this.txtBarCode.Size = new System.Drawing.Size(455, 36);
            this.txtBarCode.Style = Sunny.UI.UIStyle.Custom;
            this.txtBarCode.TabIndex = 121;
            this.txtBarCode.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chbCharger
            // 
            this.chbCharger.Checked = true;
            this.chbCharger.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chbCharger.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chbCharger.IsScaled = false;
            this.chbCharger.Location = new System.Drawing.Point(0, 0);
            this.chbCharger.MinimumSize = new System.Drawing.Size(1, 1);
            this.chbCharger.Name = "chbCharger";
            this.chbCharger.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chbCharger.Size = new System.Drawing.Size(155, 38);
            this.chbCharger.Style = Sunny.UI.UIStyle.Custom;
            this.chbCharger.TabIndex = 122;
            this.chbCharger.Text = "条码/编号：";
            // 
            // pnlBarcode
            // 
            this.pnlBarcode.Controls.Add(this.lbl1);
            this.pnlBarcode.Controls.Add(this.txtBarCode);
            this.pnlBarcode.Controls.Add(this.chbCharger);
            this.pnlBarcode.Location = new System.Drawing.Point(18, 22);
            this.pnlBarcode.Name = "pnlBarcode";
            this.pnlBarcode.Size = new System.Drawing.Size(693, 43);
            this.pnlBarcode.TabIndex = 124;
            // 
            // UcChagerInfo
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.pnlBarcode);
            this.Controls.Add(this.lblFrequency);
            this.Controls.Add(this.txtFreq);
            this.Controls.Add(this.uiLabel6);
            this.Controls.Add(this.uiLabel11);
            this.Controls.Add(this.txtRateCurrent);
            this.Controls.Add(this.uiLabel7);
            this.Controls.Add(this.uiLabel10);
            this.Controls.Add(this.txtMaxVoltage);
            this.Controls.Add(this.uiLabel3);
            this.Controls.Add(this.pnlDC);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblVolt);
            this.Name = "UcChagerInfo";
            this.Size = new System.Drawing.Size(1210, 363);
            this.pnlDC.ResumeLayout(false);
            this.pnlBarcode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UILabel uiLabel10;
        private Sunny.UI.UITextBox txtMaxVoltage;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UILabel uiLabel11;
        private Sunny.UI.UITextBox txtRateCurrent;
        private Sunny.UI.UILabel uiLabel7;
        private Sunny.UI.UILabel lblFrequency;
        private Sunny.UI.UITextBox txtFreq;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UILabel lblMaxPower;
        private Sunny.UI.UILabel lblMaxPowerSig;
        private Sunny.UI.UITextBox txtMaxPower;
        private Sunny.UI.UILabel lblMaxCurrSig;
        private Sunny.UI.UITextBox txtMaxCurr;
        private Sunny.UI.UILabel lblMaxCurr;
        private Sunny.UI.UILabel lblMinVolt;
        private Sunny.UI.UITextBox txtMinVolt;
        private Sunny.UI.UILabel lblMinVoltSig;
        private System.Windows.Forms.Label lblVolt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlDC;
        private Sunny.UI.UILabel lbl1;
        private Sunny.UI.UITextBox txtBarCode;
        private Sunny.UI.UICheckBox chbCharger;
        private System.Windows.Forms.Panel pnlBarcode;
    }
}
