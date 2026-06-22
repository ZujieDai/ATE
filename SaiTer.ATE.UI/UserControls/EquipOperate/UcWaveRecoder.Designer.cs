namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    partial class UcWaveRecoder
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
            this.components = new System.ComponentModel.Container();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.rtb_DCs = new System.Windows.Forms.RichTextBox();
            this.chk_AllChnel = new System.Windows.Forms.CheckedListBox();
            this.btn_ShowData = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.zgc_bx = new ZedGraph.ZedGraphControl();
            this.btn_Start = new System.Windows.Forms.Button();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.groupBox6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox6.Controls.Add(this.btn_Stop);
            this.groupBox6.Controls.Add(this.btn_Start);
            this.groupBox6.Controls.Add(this.rtb_DCs);
            this.groupBox6.Controls.Add(this.chk_AllChnel);
            this.groupBox6.Controls.Add(this.btn_ShowData);
            this.groupBox6.Font = new System.Drawing.Font("Times New Roman", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(5, 6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(129, 294);
            this.groupBox6.TabIndex = 48;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "通道选择";
            // 
            // rtb_DCs
            // 
            this.rtb_DCs.ImeMode = System.Windows.Forms.ImeMode.Close;
            this.rtb_DCs.Location = new System.Drawing.Point(3, 141);
            this.rtb_DCs.Name = "rtb_DCs";
            this.rtb_DCs.Size = new System.Drawing.Size(123, 58);
            this.rtb_DCs.TabIndex = 44;
            this.rtb_DCs.Text = "";
            // 
            // chk_AllChnel
            // 
            this.chk_AllChnel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.chk_AllChnel.CheckOnClick = true;
            this.chk_AllChnel.FormattingEnabled = true;
            this.chk_AllChnel.Items.AddRange(new object[] {
            "通道1",
            "通道2",
            "通道3",
            "通道4",
            "通道5",
            "通道6",
            "通道7",
            "通道8",
            "通道9",
            "通道10",
            "通道11",
            "通道12",
            "通道13",
            "通道14",
            "通道15",
            "通道16"});
            this.chk_AllChnel.Location = new System.Drawing.Point(3, 17);
            this.chk_AllChnel.Name = "chk_AllChnel";
            this.chk_AllChnel.Size = new System.Drawing.Size(123, 118);
            this.chk_AllChnel.TabIndex = 0;
            this.chk_AllChnel.ThreeDCheckBoxes = true;
            // 
            // btn_ShowData
            // 
            this.btn_ShowData.Location = new System.Drawing.Point(19, 266);
            this.btn_ShowData.Name = "btn_ShowData";
            this.btn_ShowData.Size = new System.Drawing.Size(80, 25);
            this.btn_ShowData.TabIndex = 43;
            this.btn_ShowData.Text = "展示数据";
            this.btn_ShowData.UseVisualStyleBackColor = true;
            this.btn_ShowData.Click += new System.EventHandler(this.btn_ShowData_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.zgc_bx);
            this.groupBox3.Font = new System.Drawing.Font("Times New Roman", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(140, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(532, 294);
            this.groupBox3.TabIndex = 47;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "波形图";
            // 
            // zgc_bx
            // 
            this.zgc_bx.BackColor = System.Drawing.Color.DimGray;
            this.zgc_bx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgc_bx.Location = new System.Drawing.Point(3, 20);
            this.zgc_bx.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.zgc_bx.Name = "zgc_bx";
            this.zgc_bx.ScrollGrace = 0D;
            this.zgc_bx.ScrollMaxX = 0D;
            this.zgc_bx.ScrollMaxY = 0D;
            this.zgc_bx.ScrollMaxY2 = 0D;
            this.zgc_bx.ScrollMinX = 0D;
            this.zgc_bx.ScrollMinY = 0D;
            this.zgc_bx.ScrollMinY2 = 0D;
            this.zgc_bx.Size = new System.Drawing.Size(526, 271);
            this.zgc_bx.TabIndex = 1;
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(19, 208);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(80, 25);
            this.btn_Start.TabIndex = 45;
            this.btn_Start.Text = "开始录波";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // btn_Stop
            // 
            this.btn_Stop.Location = new System.Drawing.Point(19, 235);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(80, 25);
            this.btn_Stop.TabIndex = 46;
            this.btn_Stop.Text = "停止录波";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // UcWaveRecoder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox3);
            this.Name = "UcWaveRecoder";
            this.Size = new System.Drawing.Size(672, 300);
            this.Load += new System.EventHandler(this.UcWaveRecoder_Load);
            this.groupBox6.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RichTextBox rtb_DCs;
        private System.Windows.Forms.CheckedListBox chk_AllChnel;
        private System.Windows.Forms.Button btn_ShowData;
        private System.Windows.Forms.GroupBox groupBox3;
        private ZedGraph.ZedGraphControl zgc_bx;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.Button btn_Start;
    }
}
