
namespace CxHashDecoder
{
    partial class LoaderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoaderForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbArchivePath = new System.Windows.Forms.TextBox();
            this.tbGamePath = new System.Windows.Forms.TextBox();
            this.btnViewArchivePath = new System.Windows.Forms.Button();
            this.btnViewGamePath = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnSetStaticMode = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "资源目录";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 21);
            this.label2.TabIndex = 1;
            this.label2.Text = "游戏目录";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbArchivePath
            // 
            this.tbArchivePath.Location = new System.Drawing.Point(108, 20);
            this.tbArchivePath.MaxLength = 1024;
            this.tbArchivePath.Name = "tbArchivePath";
            this.tbArchivePath.ReadOnly = true;
            this.tbArchivePath.Size = new System.Drawing.Size(940, 29);
            this.tbArchivePath.TabIndex = 2;
            // 
            // tbGamePath
            // 
            this.tbGamePath.Location = new System.Drawing.Point(108, 82);
            this.tbGamePath.MaxLength = 1024;
            this.tbGamePath.Name = "tbGamePath";
            this.tbGamePath.ReadOnly = true;
            this.tbGamePath.Size = new System.Drawing.Size(940, 29);
            this.tbGamePath.TabIndex = 3;
            // 
            // btnViewArchivePath
            // 
            this.btnViewArchivePath.Location = new System.Drawing.Point(1071, 12);
            this.btnViewArchivePath.Name = "btnViewArchivePath";
            this.btnViewArchivePath.Size = new System.Drawing.Size(139, 43);
            this.btnViewArchivePath.TabIndex = 4;
            this.btnViewArchivePath.Text = "浏览";
            this.btnViewArchivePath.UseVisualStyleBackColor = true;
            this.btnViewArchivePath.Click += new System.EventHandler(this.ViewPathButton_Click);
            // 
            // btnViewGamePath
            // 
            this.btnViewGamePath.Location = new System.Drawing.Point(1071, 74);
            this.btnViewGamePath.Name = "btnViewGamePath";
            this.btnViewGamePath.Size = new System.Drawing.Size(139, 43);
            this.btnViewGamePath.TabIndex = 5;
            this.btnViewGamePath.Text = "浏览";
            this.btnViewGamePath.UseVisualStyleBackColor = true;
            this.btnViewGamePath.Click += new System.EventHandler(this.ViewPathButton_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(480, 154);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(228, 69);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSetStaticMode
            // 
            this.btnSetStaticMode.Location = new System.Drawing.Point(1029, 154);
            this.btnSetStaticMode.Name = "btnSetStaticMode";
            this.btnSetStaticMode.Size = new System.Drawing.Size(181, 69);
            this.btnSetStaticMode.TabIndex = 9;
            this.btnSetStaticMode.Text = "设置静态模式";
            this.btnSetStaticMode.UseVisualStyleBackColor = true;
            this.btnSetStaticMode.Click += new System.EventHandler(this.btnSetStaticMode_Click);
            // 
            // LoaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1222, 277);
            this.Controls.Add(this.btnSetStaticMode);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnViewGamePath);
            this.Controls.Add(this.btnViewArchivePath);
            this.Controls.Add(this.tbGamePath);
            this.Controls.Add(this.tbArchivePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "LoaderForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "CxHashDecoder Loader";
            this.Load += new System.EventHandler(this.LoaderForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbArchivePath;
        private System.Windows.Forms.TextBox tbGamePath;
        private System.Windows.Forms.Button btnViewArchivePath;
        private System.Windows.Forms.Button btnViewGamePath;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnSetStaticMode;
    }
}

