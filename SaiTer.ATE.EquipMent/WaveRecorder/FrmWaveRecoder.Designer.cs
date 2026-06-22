namespace SaiTer.ATE.EquipMent.WaveRecorder
{
    partial class FrmWaveRecoder
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
            this.zgc_bx = new ZedGraph.ZedGraphControl();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // zgc_bx
            // 
            this.zgc_bx.BackColor = System.Drawing.Color.DimGray;
            this.zgc_bx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgc_bx.Location = new System.Drawing.Point(0, 35);
            this.zgc_bx.Margin = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.zgc_bx.Name = "zgc_bx";
            this.zgc_bx.ScrollGrace = 0D;
            this.zgc_bx.ScrollMaxX = 0D;
            this.zgc_bx.ScrollMaxY = 0D;
            this.zgc_bx.ScrollMaxY2 = 0D;
            this.zgc_bx.ScrollMinX = 0D;
            this.zgc_bx.ScrollMinY = 0D;
            this.zgc_bx.ScrollMinY2 = 0D;
            this.zgc_bx.Size = new System.Drawing.Size(1261, 639);
            this.zgc_bx.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1011, 54);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(161, 34);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmWaveRecoder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1261, 674);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.zgc_bx);
            this.Name = "FrmWaveRecoder";
            this.Text = "FrmWaveRecoder";
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl zgc_bx;
        private System.Windows.Forms.Button button1;
    }
}