namespace DataExportManager.ProjectUI
{
    partial class ExecuteDatasetExtractionHostUI
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
            this.lblDataset = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.btnWhereIsPipe = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblDataset
            // 
            this.lblDataset.AutoSize = true;
            this.lblDataset.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDataset.Location = new System.Drawing.Point(3, 0);
            this.lblDataset.Name = "lblDataset";
            this.lblDataset.Size = new System.Drawing.Size(100, 29);
            this.lblDataset.TabIndex = 19;
            this.lblDataset.Text = "Dataset:";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(803, 459);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressUI1
            // 
            this.progressUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI1.Location = new System.Drawing.Point(8, 32);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(895, 421);
            this.progressUI1.TabIndex = 24;
            // 
            // btnWhereIsPipe
            // 
            this.btnWhereIsPipe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWhereIsPipe.Location = new System.Drawing.Point(697, 459);
            this.btnWhereIsPipe.Name = "btnWhereIsPipe";
            this.btnWhereIsPipe.Size = new System.Drawing.Size(100, 23);
            this.btnWhereIsPipe.TabIndex = 23;
            this.btnWhereIsPipe.Text = "Where Is Pipe?";
            this.btnWhereIsPipe.UseVisualStyleBackColor = true;
            this.btnWhereIsPipe.Click += new System.EventHandler(this.btnWhereIsPipe_Click);
            // 
            // ExecuteDatasetExtractionHostUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnWhereIsPipe);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.progressUI1);
            this.Controls.Add(this.lblDataset);
            this.Name = "ExecuteDatasetExtractionHostUI";
            this.Size = new System.Drawing.Size(906, 485);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDataset;
        private System.Windows.Forms.Button btnCancel;
        private ReusableUIComponents.Progress.ProgressUI progressUI1;
        private System.Windows.Forms.Button btnWhereIsPipe;
    }
}
