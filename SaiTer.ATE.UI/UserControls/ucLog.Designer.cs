namespace SaiTer.ATE.UI.UserControls
{
    partial class ucLog
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
            this.txtLog = new Sunny.UI.UITextBox();
            this.spc2 = new System.Windows.Forms.SplitContainer();
            this.uiPanel1 = new Sunny.UI.UIPanel();
            this.spc1 = new System.Windows.Forms.SplitContainer();
            this.uiPanel2 = new Sunny.UI.UIPanel();
            this.lblResult = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.uiPanel3 = new Sunny.UI.UIPanel();
            this.picResult = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.spc2)).BeginInit();
            this.spc2.Panel1.SuspendLayout();
            this.spc2.Panel2.SuspendLayout();
            this.spc2.SuspendLayout();
            this.uiPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spc1)).BeginInit();
            this.spc1.Panel1.SuspendLayout();
            this.spc1.Panel2.SuspendLayout();
            this.spc1.SuspendLayout();
            this.uiPanel2.SuspendLayout();
            this.uiPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.AutoScroll = true;
            this.txtLog.ButtonSymbol = 61761;
            this.txtLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLog.IsScaled = false;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.txtLog.Maximum = 2147483647D;
            this.txtLog.Minimum = -2147483648D;
            this.txtLog.MinimumSize = new System.Drawing.Size(2, 13);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ShowScrollBar = true;
            this.txtLog.Size = new System.Drawing.Size(618, 75);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "空闲";
            this.txtLog.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spc2
            // 
            this.spc2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spc2.Location = new System.Drawing.Point(0, 0);
            this.spc2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spc2.Name = "spc2";
            // 
            // spc2.Panel1
            // 
            this.spc2.Panel1.Controls.Add(this.uiPanel1);
            // 
            // spc2.Panel2
            // 
            this.spc2.Panel2.Controls.Add(this.txtLog);
            this.spc2.Size = new System.Drawing.Size(814, 75);
            this.spc2.SplitterDistance = 193;
            this.spc2.SplitterWidth = 3;
            this.spc2.TabIndex = 1;
            // 
            // uiPanel1
            // 
            this.uiPanel1.Controls.Add(this.spc1);
            this.uiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel1.FillColor = System.Drawing.Color.White;
            this.uiPanel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel1.IsScaled = false;
            this.uiPanel1.Location = new System.Drawing.Point(0, 0);
            this.uiPanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel1.Name = "uiPanel1";
            this.uiPanel1.Size = new System.Drawing.Size(193, 75);
            this.uiPanel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiPanel1.TabIndex = 0;
            this.uiPanel1.Text = null;
            this.uiPanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // spc1
            // 
            this.spc1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spc1.Location = new System.Drawing.Point(0, 0);
            this.spc1.Name = "spc1";
            // 
            // spc1.Panel1
            // 
            this.spc1.Panel1.Controls.Add(this.uiPanel2);
            // 
            // spc1.Panel2
            // 
            this.spc1.Panel2.Controls.Add(this.uiPanel3);
            this.spc1.Size = new System.Drawing.Size(193, 75);
            this.spc1.SplitterDistance = 130;
            this.spc1.TabIndex = 0;
            // 
            // uiPanel2
            // 
            this.uiPanel2.Controls.Add(this.lblResult);
            this.uiPanel2.Controls.Add(this.label2);
            this.uiPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel2.FillColor = System.Drawing.Color.White;
            this.uiPanel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel2.IsScaled = false;
            this.uiPanel2.Location = new System.Drawing.Point(0, 0);
            this.uiPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel2.Name = "uiPanel2";
            this.uiPanel2.Size = new System.Drawing.Size(130, 75);
            this.uiPanel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiPanel2.TabIndex = 0;
            this.uiPanel2.Text = null;
            this.uiPanel2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("宋体", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblResult.Location = new System.Drawing.Point(3, 21);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(235, 60);
            this.lblResult.TabIndex = 3;
            this.lblResult.Text = "待测...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 27);
            this.label2.TabIndex = 3;
            this.label2.Visible = false;
            // 
            // uiPanel3
            // 
            this.uiPanel3.BackColor = System.Drawing.Color.Transparent;
            this.uiPanel3.Controls.Add(this.picResult);
            this.uiPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel3.FillColor = System.Drawing.Color.White;
            this.uiPanel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel3.IsScaled = false;
            this.uiPanel3.Location = new System.Drawing.Point(0, 0);
            this.uiPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel3.Name = "uiPanel3";
            this.uiPanel3.Size = new System.Drawing.Size(59, 75);
            this.uiPanel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiPanel3.TabIndex = 1;
            this.uiPanel3.Text = null;
            this.uiPanel3.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picResult
            // 
            this.picResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picResult.Image = global::SaiTer.ATE.UI.Properties.Resources.未开始;
            this.picResult.Location = new System.Drawing.Point(0, 0);
            this.picResult.Name = "picResult";
            this.picResult.Size = new System.Drawing.Size(59, 75);
            this.picResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picResult.TabIndex = 4;
            this.picResult.TabStop = false;
            // 
            // ucLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.spc2);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ucLog";
            this.Size = new System.Drawing.Size(814, 75);
            this.spc2.Panel1.ResumeLayout(false);
            this.spc2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spc2)).EndInit();
            this.spc2.ResumeLayout(false);
            this.uiPanel1.ResumeLayout(false);
            this.spc1.Panel1.ResumeLayout(false);
            this.spc1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spc1)).EndInit();
            this.spc1.ResumeLayout(false);
            this.uiPanel2.ResumeLayout(false);
            this.uiPanel2.PerformLayout();
            this.uiPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITextBox txtLog;
        private System.Windows.Forms.SplitContainer spc2;
        private System.Windows.Forms.PictureBox picResult;
        private System.Windows.Forms.Label lblResult;
        private Sunny.UI.UIPanel uiPanel1;
        private System.Windows.Forms.SplitContainer spc1;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel3;
        private System.Windows.Forms.Label label2;
    }
}
