
namespace CxHashDecoder
{
    partial class TextViewer
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
            this.gbTextView = new System.Windows.Forms.GroupBox();
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnIntput = new System.Windows.Forms.Button();
            this.tbPreview = new System.Windows.Forms.TextBox();
            this.cbSelectTextFile = new System.Windows.Forms.ComboBox();
            this.gbEncoding = new System.Windows.Forms.GroupBox();
            this.rbtUTF16 = new System.Windows.Forms.RadioButton();
            this.rbtUTF8 = new System.Windows.Forms.RadioButton();
            this.rbtShiftJIS = new System.Windows.Forms.RadioButton();
            this.rbtGBK = new System.Windows.Forms.RadioButton();
            this.gbTextView.SuspendLayout();
            this.gbEncoding.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTextView
            // 
            this.gbTextView.Controls.Add(this.btnExtract);
            this.gbTextView.Controls.Add(this.btnIntput);
            this.gbTextView.Controls.Add(this.tbPreview);
            this.gbTextView.Controls.Add(this.cbSelectTextFile);
            this.gbTextView.Controls.Add(this.gbEncoding);
            this.gbTextView.Location = new System.Drawing.Point(4, 3);
            this.gbTextView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTextView.Name = "gbTextView";
            this.gbTextView.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTextView.Size = new System.Drawing.Size(509, 618);
            this.gbTextView.TabIndex = 0;
            this.gbTextView.TabStop = false;
            this.gbTextView.Text = "文本导入浏览";
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(155, 563);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(156, 46);
            this.btnExtract.TabIndex = 5;
            this.btnExtract.Text = "导出并解码Hash";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnIntput
            // 
            this.btnIntput.Location = new System.Drawing.Point(7, 563);
            this.btnIntput.Name = "btnIntput";
            this.btnIntput.Size = new System.Drawing.Size(142, 46);
            this.btnIntput.TabIndex = 4;
            this.btnIntput.Text = "导入现有文本";
            this.btnIntput.UseVisualStyleBackColor = true;
            this.btnIntput.Click += new System.EventHandler(this.btnIntput_Click);
            // 
            // tbPreview
            // 
            this.tbPreview.Location = new System.Drawing.Point(7, 66);
            this.tbPreview.MaxLength = 65536;
            this.tbPreview.Multiline = true;
            this.tbPreview.Name = "tbPreview";
            this.tbPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbPreview.Size = new System.Drawing.Size(496, 418);
            this.tbPreview.TabIndex = 3;
            this.tbPreview.TabStop = false;
            this.tbPreview.WordWrap = false;
            // 
            // cbSelectTextFile
            // 
            this.cbSelectTextFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectTextFile.FormattingEnabled = true;
            this.cbSelectTextFile.Location = new System.Drawing.Point(7, 28);
            this.cbSelectTextFile.Name = "cbSelectTextFile";
            this.cbSelectTextFile.Size = new System.Drawing.Size(496, 29);
            this.cbSelectTextFile.TabIndex = 2;
            this.cbSelectTextFile.SelectedIndexChanged += new System.EventHandler(this.cbSelectTextFile_SelectedIndexChanged);
            // 
            // gbEncoding
            // 
            this.gbEncoding.Controls.Add(this.rbtUTF16);
            this.gbEncoding.Controls.Add(this.rbtUTF8);
            this.gbEncoding.Controls.Add(this.rbtShiftJIS);
            this.gbEncoding.Controls.Add(this.rbtGBK);
            this.gbEncoding.Location = new System.Drawing.Point(7, 490);
            this.gbEncoding.Name = "gbEncoding";
            this.gbEncoding.Size = new System.Drawing.Size(495, 67);
            this.gbEncoding.TabIndex = 0;
            this.gbEncoding.TabStop = false;
            this.gbEncoding.Text = "编码";
            // 
            // rbtUTF16
            // 
            this.rbtUTF16.AutoSize = true;
            this.rbtUTF16.Location = new System.Drawing.Point(264, 29);
            this.rbtUTF16.Name = "rbtUTF16";
            this.rbtUTF16.Size = new System.Drawing.Size(100, 25);
            this.rbtUTF16.TabIndex = 4;
            this.rbtUTF16.Text = "UTF-16LE";
            this.rbtUTF16.UseVisualStyleBackColor = true;
            this.rbtUTF16.CheckedChanged += new System.EventHandler(this.EncodingBtn_CheckedChanged);
            // 
            // rbtUTF8
            // 
            this.rbtUTF8.AutoSize = true;
            this.rbtUTF8.Location = new System.Drawing.Point(181, 28);
            this.rbtUTF8.Name = "rbtUTF8";
            this.rbtUTF8.Size = new System.Drawing.Size(74, 25);
            this.rbtUTF8.TabIndex = 2;
            this.rbtUTF8.Text = "UTF-8";
            this.rbtUTF8.UseVisualStyleBackColor = true;
            this.rbtUTF8.CheckedChanged += new System.EventHandler(this.EncodingBtn_CheckedChanged);
            // 
            // rbtShiftJIS
            // 
            this.rbtShiftJIS.AutoSize = true;
            this.rbtShiftJIS.Location = new System.Drawing.Point(21, 29);
            this.rbtShiftJIS.Name = "rbtShiftJIS";
            this.rbtShiftJIS.Size = new System.Drawing.Size(88, 25);
            this.rbtShiftJIS.TabIndex = 1;
            this.rbtShiftJIS.Text = "Shift JIS";
            this.rbtShiftJIS.UseVisualStyleBackColor = true;
            this.rbtShiftJIS.CheckedChanged += new System.EventHandler(this.EncodingBtn_CheckedChanged);
            // 
            // rbtGBK
            // 
            this.rbtGBK.AutoSize = true;
            this.rbtGBK.Location = new System.Drawing.Point(115, 29);
            this.rbtGBK.Name = "rbtGBK";
            this.rbtGBK.Size = new System.Drawing.Size(60, 25);
            this.rbtGBK.TabIndex = 0;
            this.rbtGBK.Text = "GBK";
            this.rbtGBK.UseVisualStyleBackColor = true;
            this.rbtGBK.CheckedChanged += new System.EventHandler(this.EncodingBtn_CheckedChanged);
            // 
            // TextViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTextView);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TextViewer";
            this.Size = new System.Drawing.Size(520, 628);
            this.Load += new System.EventHandler(this.TextViewer_Load);
            this.gbTextView.ResumeLayout(false);
            this.gbTextView.PerformLayout();
            this.gbEncoding.ResumeLayout(false);
            this.gbEncoding.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTextView;
        private System.Windows.Forms.GroupBox gbEncoding;
        private System.Windows.Forms.RadioButton rbtUTF16;
        private System.Windows.Forms.RadioButton rbtUTF8;
        private System.Windows.Forms.RadioButton rbtShiftJIS;
        private System.Windows.Forms.RadioButton rbtGBK;
        private System.Windows.Forms.ComboBox cbSelectTextFile;
        private System.Windows.Forms.TextBox tbPreview;
        private System.Windows.Forms.Button btnIntput;
        private System.Windows.Forms.Button btnExtract;
    }
}
