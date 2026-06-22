namespace SaiTer.ATE.UI.UserControls
{
    partial class ucWaitTestChagers
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
            this.flpChargers = new System.Windows.Forms.FlowLayoutPanel();
            this.chbAllTestItems = new MaterialSkin.Controls.MaterialCheckbox();
            this.SuspendLayout();
            // 
            // flpChargers
            // 
            this.flpChargers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpChargers.Location = new System.Drawing.Point(0, 0);
            this.flpChargers.Name = "flpChargers";
            this.flpChargers.Size = new System.Drawing.Size(350, 150);
            this.flpChargers.TabIndex = 0;
            // 
            // chbAllTestItems
            // 
            this.chbAllTestItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chbAllTestItems.AutoSize = true;
            this.chbAllTestItems.Checked = true;
            this.chbAllTestItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbAllTestItems.Depth = 0;
            this.chbAllTestItems.Location = new System.Drawing.Point(23, 151);
            this.chbAllTestItems.Margin = new System.Windows.Forms.Padding(0);
            this.chbAllTestItems.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chbAllTestItems.MouseState = MaterialSkin.MouseState.HOVER;
            this.chbAllTestItems.Name = "chbAllTestItems";
            this.chbAllTestItems.Ripple = true;
            this.chbAllTestItems.Size = new System.Drawing.Size(163, 37);
            this.chbAllTestItems.TabIndex = 4;
            this.chbAllTestItems.Text = "选中所有检测项目";
            this.chbAllTestItems.UseVisualStyleBackColor = true;
            this.chbAllTestItems.CheckedChanged += new System.EventHandler(this.chbAllTestItems_CheckedChanged);
            // 
            // ucWaitTestChagers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.flpChargers);
            this.Controls.Add(this.chbAllTestItems);
            this.Name = "ucWaitTestChagers";
            this.Size = new System.Drawing.Size(350, 186);
            this.Load += new System.EventHandler(this.ucChargerInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpChargers;
        public MaterialSkin.Controls.MaterialCheckbox chbAllTestItems;
    }
}
