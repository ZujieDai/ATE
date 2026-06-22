namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmBMSVersion
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
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.lblSoftware = new Sunny.UI.UILabel();
            this.lblFlow = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnLoad = new Sunny.UI.UIButton();
            this.SuspendLayout();
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.IsScaled = false;
            this.uiLabel3.Location = new System.Drawing.Point(65, 88);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(446, 24);
            this.uiLabel3.TabIndex = 31;
            this.uiLabel3.Text = "当前下位机的版本为：";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSoftware
            // 
            this.lblSoftware.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSoftware.IsScaled = false;
            this.lblSoftware.Location = new System.Drawing.Point(65, 129);
            this.lblSoftware.Name = "lblSoftware";
            this.lblSoftware.Size = new System.Drawing.Size(446, 24);
            this.lblSoftware.TabIndex = 32;
            this.lblSoftware.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFlow
            // 
            this.lblFlow.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFlow.IsScaled = false;
            this.lblFlow.Location = new System.Drawing.Point(65, 254);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Size = new System.Drawing.Size(446, 24);
            this.lblFlow.TabIndex = 34;
            this.lblFlow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.IsScaled = false;
            this.uiLabel2.Location = new System.Drawing.Point(65, 213);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(446, 24);
            this.uiLabel2.TabIndex = 33;
            this.uiLabel2.Text = "版本流水号为：";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoad
            // 
            this.btnLoad.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoad.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLoad.IsScaled = false;
            this.btnLoad.Location = new System.Drawing.Point(234, 347);
            this.btnLoad.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(104, 42);
            this.btnLoad.TabIndex = 79;
            this.btnLoad.Text = "读取版本号";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // FrmBMSVersion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lblFlow);
            this.Controls.Add(this.uiLabel2);
            this.Controls.Add(this.lblSoftware);
            this.Controls.Add(this.uiLabel3);
            this.Name = "FrmBMSVersion";
            this.Text = "读取下位机软件版本";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmBMSVersion_FormClosing);
            this.Load += new System.EventHandler(this.FrmBMSVersion_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UILabel lblSoftware;
        private Sunny.UI.UILabel lblFlow;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnLoad;
    }
}