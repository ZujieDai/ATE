using SaiTer.ATE.DataModel;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    partial class FrmEquipOperate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEquipOperate));
            this.tbMenu = new Sunny.UI.UITabControlMenu();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.tbpSafety1 = new System.Windows.Forms.TabPage();
            this.SuspendLayout();
            // 
            // tbMenu
            // 
            this.tbMenu.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tbMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMenu.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tbMenu.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbMenu.ItemSize = new System.Drawing.Size(40, 200);
            this.tbMenu.Location = new System.Drawing.Point(0, 45);
            this.tbMenu.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.tbMenu.Multiline = true;
            this.tbMenu.Name = "tbMenu";
            this.tbMenu.SelectedIndex = 0;
            this.tbMenu.Size = new System.Drawing.Size(925, 575);
            this.tbMenu.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tbMenu.Style = Sunny.UI.UIStyle.Custom;
            this.tbMenu.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.tbMenu.TabIndex = 0;
            this.tbMenu.SelectedIndexChanged += new System.EventHandler(this.tbMenu_SelectedIndexChanged);
            // 
            // tabPage7
            // 
            this.tabPage7.Location = new System.Drawing.Point(4, 40);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(719, 499);
            this.tabPage7.TabIndex = 1;
            this.tabPage7.Text = "2号枪";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // tbpSafety1
            // 
            this.tbpSafety1.Location = new System.Drawing.Point(4, 40);
            this.tbpSafety1.Name = "tbpSafety1";
            this.tbpSafety1.Padding = new System.Windows.Forms.Padding(3);
            this.tbpSafety1.Size = new System.Drawing.Size(519, 499);
            this.tbpSafety1.TabIndex = 0;
            this.tbpSafety1.Text = "1号枪";
            this.tbpSafety1.UseVisualStyleBackColor = true;
            // 
            // FrmEquipOperate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(925, 620);
            this.Controls.Add(this.tbMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 450);
            this.Name = "FrmEquipOperate";
            this.Padding = new System.Windows.Forms.Padding(0, 45, 0, 0);
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = LanguageManager.GetByKey("FrmEquipOperate"); 
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(140)))));
            this.TitleHeight = 45;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmEquipOperate_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmEquipOperate_FormClosed);
            this.Load += new System.EventHandler(this.FrmEquipOperate_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITabControlMenu tbMenu;
        public System.Windows.Forms.TabPage tabPage7;
        public System.Windows.Forms.TabPage tbpSafety1;
    }
}