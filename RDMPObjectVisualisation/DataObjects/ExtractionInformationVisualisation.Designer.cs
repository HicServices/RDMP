namespace RDMPObjectVisualisation.DataObjects
{
    partial class ExtractionInformationVisualisation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractionInformationVisualisation));
            this.lblName = new System.Windows.Forms.Label();
            this.pbExtractionInformation = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionInformation)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 13);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "lblName";
            // 
            // pbExtractionInformation
            // 
            this.pbExtractionInformation.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pbExtractionInformation.Image = ((System.Drawing.Image)(resources.GetObject("pbExtractionInformation.Image")));
            this.pbExtractionInformation.Location = new System.Drawing.Point(128, 39);
            this.pbExtractionInformation.Name = "pbExtractionInformation";
            this.pbExtractionInformation.Size = new System.Drawing.Size(23, 23);
            this.pbExtractionInformation.TabIndex = 2;
            this.pbExtractionInformation.TabStop = false;
            // 
            // ExtractionInformationVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pbExtractionInformation);
            this.Controls.Add(this.lblName);
            this.Name = "ExtractionInformationVisualisation";
            this.Size = new System.Drawing.Size(284, 98);
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionInformation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.PictureBox pbExtractionInformation;
    }
}
