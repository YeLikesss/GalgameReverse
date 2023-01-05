
namespace CxHashDecoder
{
    partial class HashDecoderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashDecoderForm));
            this.mTextViewer = new CxHashDecoder.TextViewer();
            this.mTextCreator = new CxHashDecoder.TextCreator();
            this.btnPathEnumerator = new System.Windows.Forms.Button();
            this.btnUseHashDump = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.btnBackupDecodedName = new System.Windows.Forms.Button();
            this.btnHashModeSwitch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mTextViewer
            // 
            this.mTextViewer.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.mTextViewer.Location = new System.Drawing.Point(13, 12);
            this.mTextViewer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mTextViewer.Name = "mTextViewer";
            this.mTextViewer.Size = new System.Drawing.Size(521, 665);
            this.mTextViewer.TabIndex = 0;
            this.mTextViewer.TextProcessCallBack = null;
            // 
            // mTextCreator
            // 
            this.mTextCreator.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.mTextCreator.Location = new System.Drawing.Point(542, 12);
            this.mTextCreator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mTextCreator.Name = "mTextCreator";
            this.mTextCreator.Size = new System.Drawing.Size(701, 457);
            this.mTextCreator.TabIndex = 1;
            this.mTextCreator.TextProcessCallBack = null;
            // 
            // btnPathEnumerator
            // 
            this.btnPathEnumerator.Location = new System.Drawing.Point(542, 475);
            this.btnPathEnumerator.Name = "btnPathEnumerator";
            this.btnPathEnumerator.Size = new System.Drawing.Size(182, 57);
            this.btnPathEnumerator.TabIndex = 2;
            this.btnPathEnumerator.Text = "遍历本地文件夹解码Hash";
            this.btnPathEnumerator.UseVisualStyleBackColor = true;
            this.btnPathEnumerator.Click += new System.EventHandler(this.btnPathEnumerator_Click);
            // 
            // btnUseHashDump
            // 
            this.btnUseHashDump.Location = new System.Drawing.Point(730, 475);
            this.btnUseHashDump.Name = "btnUseHashDump";
            this.btnUseHashDump.Size = new System.Drawing.Size(182, 57);
            this.btnUseHashDump.TabIndex = 3;
            this.btnUseHashDump.Text = "使用HashDump文件解码Hash";
            this.btnUseHashDump.UseVisualStyleBackColor = true;
            this.btnUseHashDump.Click += new System.EventHandler(this.btnUseHashDump_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 643);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "运行状态 :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStatus
            // 
            this.lbStatus.Location = new System.Drawing.Point(102, 643);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(419, 25);
            this.lbStatus.TabIndex = 5;
            this.lbStatus.Text = "lbStatus";
            // 
            // btnBackupDecodedName
            // 
            this.btnBackupDecodedName.Location = new System.Drawing.Point(918, 475);
            this.btnBackupDecodedName.Name = "btnBackupDecodedName";
            this.btnBackupDecodedName.Size = new System.Drawing.Size(182, 57);
            this.btnBackupDecodedName.TabIndex = 6;
            this.btnBackupDecodedName.Text = "备份已解码的路径/文件名";
            this.btnBackupDecodedName.UseVisualStyleBackColor = true;
            this.btnBackupDecodedName.Click += new System.EventHandler(this.btnBackupDecodedName_Click);
            // 
            // btnHashModeSwitch
            // 
            this.btnHashModeSwitch.Location = new System.Drawing.Point(1123, 475);
            this.btnHashModeSwitch.Name = "btnHashModeSwitch";
            this.btnHashModeSwitch.Size = new System.Drawing.Size(120, 57);
            this.btnHashModeSwitch.TabIndex = 7;
            this.btnHashModeSwitch.Text = "文件名模式";
            this.btnHashModeSwitch.UseVisualStyleBackColor = true;
            this.btnHashModeSwitch.Click += new System.EventHandler(this.btnHashModeSwitch_Click);
            // 
            // HashDecoderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.btnHashModeSwitch);
            this.Controls.Add(this.btnBackupDecodedName);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUseHashDump);
            this.Controls.Add(this.btnPathEnumerator);
            this.Controls.Add(this.mTextCreator);
            this.Controls.Add(this.mTextViewer);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "HashDecoderForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HashDecoderForm_FormClosed);
            this.Load += new System.EventHandler(this.HashDecoderForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextViewer mTextViewer;
        private TextCreator mTextCreator;
        private System.Windows.Forms.Button btnPathEnumerator;
        private System.Windows.Forms.Button btnUseHashDump;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Button btnBackupDecodedName;
        private System.Windows.Forms.Button btnHashModeSwitch;
    }
}