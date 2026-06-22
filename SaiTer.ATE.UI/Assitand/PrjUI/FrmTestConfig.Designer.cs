namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmTestConfig
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
            this.txt_LoadWaitTime_s = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.btnCancel = new Sunny.UI.UIButton();
            this.btnCommit = new Sunny.UI.UIButton();
            this.SuspendLayout();
            // 
            // txt_LoadWaitTime_s
            // 
            this.txt_LoadWaitTime_s.ButtonSymbol = 61761;
            this.txt_LoadWaitTime_s.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txt_LoadWaitTime_s.DoubleValue = 1D;
            this.txt_LoadWaitTime_s.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.txt_LoadWaitTime_s.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txt_LoadWaitTime_s.HasMaximum = true;
            this.txt_LoadWaitTime_s.HasMinimum = true;
            this.txt_LoadWaitTime_s.IntValue = 1;
            this.txt_LoadWaitTime_s.IsScaled = false;
            this.txt_LoadWaitTime_s.Location = new System.Drawing.Point(330, 159);
            this.txt_LoadWaitTime_s.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_LoadWaitTime_s.Maximum = 1000D;
            this.txt_LoadWaitTime_s.MaximumEnabled = true;
            this.txt_LoadWaitTime_s.Minimum = 0D;
            this.txt_LoadWaitTime_s.MinimumEnabled = true;
            this.txt_LoadWaitTime_s.MinimumSize = new System.Drawing.Size(1, 16);
            this.txt_LoadWaitTime_s.Name = "txt_LoadWaitTime_s";
            this.txt_LoadWaitTime_s.Size = new System.Drawing.Size(217, 36);
            this.txt_LoadWaitTime_s.TabIndex = 68;
            this.txt_LoadWaitTime_s.Text = "1";
            this.txt_LoadWaitTime_s.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txt_LoadWaitTime_s.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.IsScaled = false;
            this.uiLabel1.Location = new System.Drawing.Point(47, 159);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(225, 36);
            this.uiLabel1.TabIndex = 67;
            this.uiLabel1.Text = "负载等待时间(s)：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCancel.IsScaled = false;
            this.btnCancel.Location = new System.Drawing.Point(490, 401);
            this.btnCancel.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 53);
            this.btnCancel.TabIndex = 80;
            this.btnCancel.Text = "关闭";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCommit
            // 
            this.btnCommit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCommit.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCommit.IsScaled = false;
            this.btnCommit.Location = new System.Drawing.Point(108, 401);
            this.btnCommit.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(110, 53);
            this.btnCommit.TabIndex = 79;
            this.btnCommit.Text = "提交";
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // FrmTestConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 546);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCommit);
            this.Controls.Add(this.txt_LoadWaitTime_s);
            this.Controls.Add(this.uiLabel1);
            this.Name = "FrmTestConfig";
            this.Text = "测试设置";
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITextBox txt_LoadWaitTime_s;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIButton btnCancel;
        private Sunny.UI.UIButton btnCommit;
    }
}