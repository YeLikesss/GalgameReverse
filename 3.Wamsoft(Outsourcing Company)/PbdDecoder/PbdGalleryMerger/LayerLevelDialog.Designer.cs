namespace PbdGalleryMerger
{
    partial class LayerLevelDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.numUDLayerLevel = new System.Windows.Forms.NumericUpDown();
            this.btnSetLayerLevel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numUDLayerLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "图层等级";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numUDLayerLevel
            // 
            this.numUDLayerLevel.Location = new System.Drawing.Point(102, 36);
            this.numUDLayerLevel.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numUDLayerLevel.Name = "numUDLayerLevel";
            this.numUDLayerLevel.Size = new System.Drawing.Size(86, 25);
            this.numUDLayerLevel.TabIndex = 1;
            this.numUDLayerLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSetLayerLevel
            // 
            this.btnSetLayerLevel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSetLayerLevel.Location = new System.Drawing.Point(79, 86);
            this.btnSetLayerLevel.Name = "btnSetLayerLevel";
            this.btnSetLayerLevel.Size = new System.Drawing.Size(85, 33);
            this.btnSetLayerLevel.TabIndex = 2;
            this.btnSetLayerLevel.Text = "确定";
            this.btnSetLayerLevel.UseVisualStyleBackColor = true;
            // 
            // LayerLevelDialog
            // 
            this.AcceptButton = this.btnSetLayerLevel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 141);
            this.Controls.Add(this.btnSetLayerLevel);
            this.Controls.Add(this.numUDLayerLevel);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayerLevelDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置图层等级";
            ((System.ComponentModel.ISupportInitialize)(this.numUDLayerLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numUDLayerLevel;
        private System.Windows.Forms.Button btnSetLayerLevel;
    }
}