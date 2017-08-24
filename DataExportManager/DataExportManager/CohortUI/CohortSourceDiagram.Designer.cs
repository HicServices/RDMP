namespace DataExportManager.CohortUI
{
    partial class CohortSourceDiagram
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortSourceDiagram));
            this.lblReleaseId = new System.Windows.Forms.Label();
            this.lblPrivateId = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblReleaseId
            // 
            this.lblReleaseId.AutoSize = true;
            this.lblReleaseId.BackColor = System.Drawing.Color.White;
            this.lblReleaseId.Location = new System.Drawing.Point(4, 129);
            this.lblReleaseId.Name = "lblReleaseId";
            this.lblReleaseId.Size = new System.Drawing.Size(35, 13);
            this.lblReleaseId.TabIndex = 9;
            this.lblReleaseId.Text = "label1";
            // 
            // lblPrivateId
            // 
            this.lblPrivateId.AutoSize = true;
            this.lblPrivateId.BackColor = System.Drawing.Color.White;
            this.lblPrivateId.Location = new System.Drawing.Point(6, 69);
            this.lblPrivateId.Name = "lblPrivateId";
            this.lblPrivateId.Size = new System.Drawing.Size(35, 13);
            this.lblPrivateId.TabIndex = 8;
            this.lblPrivateId.Text = "label1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(281, 156);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // CohortSourceDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblReleaseId);
            this.Controls.Add(this.lblPrivateId);
            this.Controls.Add(this.pictureBox1);
            this.Name = "CohortSourceDiagram";
            this.Size = new System.Drawing.Size(290, 166);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblReleaseId;
        private System.Windows.Forms.Label lblPrivateId;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
