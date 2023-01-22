namespace PbdGalleryMerger
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.cbGameTitle = new System.Windows.Forms.ComboBox();
            this.btnSelectPbdFile = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.lbImageInformation = new System.Windows.Forms.ListBox();
            this.picPanel = new System.Windows.Forms.Panel();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.lbPictureRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnSetEmote = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSetBackground = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSetNone = new System.Windows.Forms.ToolStripMenuItem();
            this.picPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.lbPictureRightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbGameTitle
            // 
            this.cbGameTitle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGameTitle.FormattingEnabled = true;
            this.cbGameTitle.Location = new System.Drawing.Point(12, 12);
            this.cbGameTitle.Name = "cbGameTitle";
            this.cbGameTitle.Size = new System.Drawing.Size(491, 27);
            this.cbGameTitle.TabIndex = 0;
            // 
            // btnSelectPbdFile
            // 
            this.btnSelectPbdFile.Location = new System.Drawing.Point(12, 45);
            this.btnSelectPbdFile.Name = "btnSelectPbdFile";
            this.btnSelectPbdFile.Size = new System.Drawing.Size(157, 40);
            this.btnSelectPbdFile.TabIndex = 1;
            this.btnSelectPbdFile.Text = "选择pbd文件";
            this.btnSelectPbdFile.UseVisualStyleBackColor = true;
            this.btnSelectPbdFile.Click += new System.EventHandler(this.btnSelectPbdFile_Click);
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(355, 45);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(148, 40);
            this.btnMerge.TabIndex = 2;
            this.btnMerge.Text = "合并";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // lbImageInformation
            // 
            this.lbImageInformation.FormattingEnabled = true;
            this.lbImageInformation.ItemHeight = 19;
            this.lbImageInformation.Location = new System.Drawing.Point(12, 91);
            this.lbImageInformation.Name = "lbImageInformation";
            this.lbImageInformation.Size = new System.Drawing.Size(491, 536);
            this.lbImageInformation.TabIndex = 3;
            this.lbImageInformation.SelectedIndexChanged += new System.EventHandler(this.lbImageInformation_SelectedIndexChanged);
            this.lbImageInformation.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbImageInformation_MouseUp);
            // 
            // picPanel
            // 
            this.picPanel.AutoScroll = true;
            this.picPanel.Controls.Add(this.pictureBoxPreview);
            this.picPanel.Location = new System.Drawing.Point(509, 12);
            this.picPanel.Name = "picPanel";
            this.picPanel.Size = new System.Drawing.Size(743, 653);
            this.picPanel.TabIndex = 4;
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(712, 586);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(12, 642);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(491, 23);
            this.labelStatus.TabIndex = 5;
            // 
            // lbPictureRightClickMenu
            // 
            this.lbPictureRightClickMenu.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbPictureRightClickMenu.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.lbPictureRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSetEmote,
            this.btnSetBackground,
            this.btnSetNone});
            this.lbPictureRightClickMenu.Name = "lbPictureRightClickMenu";
            this.lbPictureRightClickMenu.Size = new System.Drawing.Size(144, 76);
            // 
            // btnSetEmote
            // 
            this.btnSetEmote.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSetEmote.Name = "btnSetEmote";
            this.btnSetEmote.Size = new System.Drawing.Size(143, 24);
            this.btnSetEmote.Text = "设置为表情";
            this.btnSetEmote.Click += new System.EventHandler(this.btnRightClickMenu_Click);
            // 
            // btnSetBackground
            // 
            this.btnSetBackground.Name = "btnSetBackground";
            this.btnSetBackground.Size = new System.Drawing.Size(143, 24);
            this.btnSetBackground.Text = "设置为背景";
            this.btnSetBackground.Click += new System.EventHandler(this.btnRightClickMenu_Click);
            // 
            // btnSetNone
            // 
            this.btnSetNone.Name = "btnSetNone";
            this.btnSetNone.Size = new System.Drawing.Size(143, 24);
            this.btnSetNone.Text = "清空设置";
            this.btnSetNone.Click += new System.EventHandler(this.btnRightClickMenu_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.picPanel);
            this.Controls.Add(this.lbImageInformation);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnSelectPbdFile);
            this.Controls.Add(this.cbGameTitle);
            this.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "PbdGalleryMerger - Static Mode ";
            this.picPanel.ResumeLayout(false);
            this.picPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.lbPictureRightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbGameTitle;
        private System.Windows.Forms.Button btnSelectPbdFile;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.ListBox lbImageInformation;
        private System.Windows.Forms.Panel picPanel;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ContextMenuStrip lbPictureRightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem btnSetEmote;
        private System.Windows.Forms.ToolStripMenuItem btnSetBackground;
        private System.Windows.Forms.ToolStripMenuItem btnSetNone;
    }
}