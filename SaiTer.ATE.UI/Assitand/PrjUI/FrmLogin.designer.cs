namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmLogin
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLogin));
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lklbl_wz = new Sunny.UI.UILinkLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblRegist = new Sunny.UI.UILabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new System.Drawing.Point(44, 15);
            this.lblTitle.Text = "";
            // 
            // uiPanel1
            // 
            this.uiPanel1.BackColor = System.Drawing.Color.Transparent;
            this.uiPanel1.Location = new System.Drawing.Point(515, 52);
            // 
            // lblSubText
            // 
            this.lblSubText.ForeColor = System.Drawing.Color.White;
            this.lblSubText.Location = new System.Drawing.Point(291, 315);
            this.lblSubText.Size = new System.Drawing.Size(235, 26);
            this.lblSubText.Style = Sunny.UI.UIStyle.Custom;
            this.lblSubText.Text = "深圳市赛特新能科技有限公司";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image = global::SaiTer.ATE.UI.Properties.Resources.口号_白色;
            this.pictureBox2.Location = new System.Drawing.Point(49, 200);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(442, 50);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 11;
            this.pictureBox2.TabStop = false;
            // 
            // lklbl_wz
            // 
            this.lklbl_wz.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(155)))), ((int)(((byte)(40)))));
            this.lklbl_wz.BackColor = System.Drawing.Color.Transparent;
            this.lklbl_wz.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lklbl_wz.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.lklbl_wz.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.lklbl_wz.Location = new System.Drawing.Point(532, 318);
            this.lklbl_wz.Name = "lklbl_wz";
            this.lklbl_wz.Size = new System.Drawing.Size(206, 23);
            this.lklbl_wz.Style = Sunny.UI.UIStyle.Custom;
            this.lklbl_wz.TabIndex = 12;
            this.lklbl_wz.TabStop = true;
            this.lklbl_wz.Text = "http://www.saiter.cn";
            this.lklbl_wz.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lklbl_wz.Click += new System.EventHandler(this.lklbl_wz_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(49, 52);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(442, 175);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // lblRegist
            // 
            this.lblRegist.BackColor = System.Drawing.Color.Transparent;
            this.lblRegist.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRegist.ForeColor = System.Drawing.Color.Yellow;
            this.lblRegist.IsScaled = false;
            this.lblRegist.Location = new System.Drawing.Point(182, 268);
            this.lblRegist.Name = "lblRegist";
            this.lblRegist.Size = new System.Drawing.Size(247, 47);
            this.lblRegist.Style = Sunny.UI.UIStyle.Custom;
            this.lblRegist.TabIndex = 13;
            this.lblRegist.Text = "软件有效期还剩 7 天";
            this.lblRegist.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRegist.Visible = false;
            // 
            // FrmLogin
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.BackgroundImage = global::SaiTer.ATE.UI.Properties.Resources.frmloginBackGround;
            this.ClientSize = new System.Drawing.Size(750, 350);
            this.Controls.Add(this.lblRegist);
            this.Controls.Add(this.lklbl_wz);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(750, 350);
            this.MinimumSize = new System.Drawing.Size(750, 350);
            this.Name = "FrmLogin";
            this.ShowInTaskbar = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.SubText = "深圳市赛特新能科技有限公司";
            this.Text = "Form1";
            this.Title = "";
            this.TopMost = true;
            this.ButtonLoginClick += new System.EventHandler(this.btn_Login_Click);
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.pictureBox2, 0);
            this.Controls.SetChildIndex(this.lklbl_wz, 0);
            this.Controls.SetChildIndex(this.lblTitle, 0);
            this.Controls.SetChildIndex(this.lblSubText, 0);
            this.Controls.SetChildIndex(this.uiPanel1, 0);
            this.Controls.SetChildIndex(this.lblRegist, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox2;
        private Sunny.UI.UILinkLabel lklbl_wz;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Sunny.UI.UILabel lblRegist;
    }
}

