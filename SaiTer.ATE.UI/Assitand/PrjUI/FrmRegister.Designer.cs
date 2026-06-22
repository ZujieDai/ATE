namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmRegister
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRegister));
            this.lblLoginTitle = new System.Windows.Forms.Label();
            this.lblRegState = new System.Windows.Forms.Label();
            this.pcbLogo = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRegCode = new System.Windows.Forms.TextBox();
            this.tbMachineCode = new System.Windows.Forms.TextBox();
            this.btnResgister = new Sunny.UI.UIButton();
            ((System.ComponentModel.ISupportInitialize)(this.pcbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // lblLoginTitle
            // 
            this.lblLoginTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblLoginTitle.Font = new System.Drawing.Font("黑体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLoginTitle.ForeColor = System.Drawing.Color.Transparent;
            this.lblLoginTitle.Location = new System.Drawing.Point(234, 97);
            this.lblLoginTitle.Name = "lblLoginTitle";
            this.lblLoginTitle.Size = new System.Drawing.Size(395, 85);
            this.lblLoginTitle.TabIndex = 19;
            this.lblLoginTitle.Text = "充电桩测试系统";
            this.lblLoginTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRegState
            // 
            this.lblRegState.AutoSize = true;
            this.lblRegState.BackColor = System.Drawing.Color.Transparent;
            this.lblRegState.ForeColor = System.Drawing.Color.White;
            this.lblRegState.Location = new System.Drawing.Point(143, 316);
            this.lblRegState.Name = "lblRegState";
            this.lblRegState.Size = new System.Drawing.Size(74, 21);
            this.lblRegState.TabIndex = 18;
            this.lblRegState.Text = "注册状态";
            // 
            // pcbLogo
            // 
            this.pcbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pcbLogo.Image = global::SaiTer.ATE.UI.Properties.Resources.白风车;
            this.pcbLogo.Location = new System.Drawing.Point(67, 82);
            this.pcbLogo.Name = "pcbLogo";
            this.pcbLogo.Size = new System.Drawing.Size(124, 108);
            this.pcbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pcbLogo.TabIndex = 17;
            this.pcbLogo.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(64, 280);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "注册码";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(45, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 12);
            this.label1.TabIndex = 14;
            this.label1.Text = "电脑特征";
            // 
            // txtRegCode
            // 
            this.txtRegCode.Location = new System.Drawing.Point(145, 267);
            this.txtRegCode.Name = "txtRegCode";
            this.txtRegCode.Size = new System.Drawing.Size(539, 29);
            this.txtRegCode.TabIndex = 12;
            // 
            // tbMachineCode
            // 
            this.tbMachineCode.Location = new System.Drawing.Point(145, 223);
            this.tbMachineCode.Name = "tbMachineCode";
            this.tbMachineCode.Size = new System.Drawing.Size(539, 29);
            this.tbMachineCode.TabIndex = 13;
            // 
            // btnResgister
            // 
            this.btnResgister.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResgister.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnResgister.IsScaled = false;
            this.btnResgister.Location = new System.Drawing.Point(542, 309);
            this.btnResgister.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnResgister.Name = "btnResgister";
            this.btnResgister.Size = new System.Drawing.Size(141, 49);
            this.btnResgister.Style = Sunny.UI.UIStyle.Custom;
            this.btnResgister.TabIndex = 20;
            this.btnResgister.Text = "注册";
            this.btnResgister.Click += new System.EventHandler(this.BtnResgister_Click);
            // 
            // FrmRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.frmloginBackGround;
            this.ClientSize = new System.Drawing.Size(775, 471);
            this.Controls.Add(this.btnResgister);
            this.Controls.Add(this.lblLoginTitle);
            this.Controls.Add(this.lblRegState);
            this.Controls.Add(this.pcbLogo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtRegCode);
            this.Controls.Add(this.tbMachineCode);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(775, 471);
            this.MinimumSize = new System.Drawing.Size(775, 471);
            this.Name = "FrmRegister";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "注册";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pcbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLoginTitle;
        private System.Windows.Forms.Label lblRegState;
        private System.Windows.Forms.PictureBox pcbLogo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRegCode;
        private System.Windows.Forms.TextBox tbMachineCode;
        private Sunny.UI.UIButton btnResgister;
    }
}