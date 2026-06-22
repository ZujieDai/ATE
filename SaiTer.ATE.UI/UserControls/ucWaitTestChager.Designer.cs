namespace SaiTer.ATE.UI.UserControls
{
    partial class ucWaitTestChager
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
            this.chbCharger = new MaterialSkin.Controls.MaterialCheckbox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // chbCharger
            // 
            this.chbCharger.AutoSize = true;
            this.chbCharger.Checked = true;
            this.chbCharger.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbCharger.Depth = 0;
            this.chbCharger.Location = new System.Drawing.Point(4, 89);
            this.chbCharger.Margin = new System.Windows.Forms.Padding(0);
            this.chbCharger.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chbCharger.MouseState = MaterialSkin.MouseState.HOVER;
            this.chbCharger.Name = "chbCharger";
            this.chbCharger.Ripple = true;
            this.chbCharger.Size = new System.Drawing.Size(76, 37);
            this.chbCharger.TabIndex = 0;
            this.chbCharger.Text = "枪号1";
            this.chbCharger.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.chbCharger);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(91, 135);
            this.panel1.TabIndex = 6;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SaiTer.ATE.UI.Properties.Resources.充电桩_Small;
            this.pictureBox1.Location = new System.Drawing.Point(15, 2);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(65, 84);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // ucWaitTestChager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "ucWaitTestChager";
            this.Size = new System.Drawing.Size(91, 135);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public MaterialSkin.Controls.MaterialCheckbox chbCharger;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.PictureBox pictureBox1;
    }
}
