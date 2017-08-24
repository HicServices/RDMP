using CatalogueManager.Refreshing;

namespace DataExportManager.CohortUI
{
    partial class ExtractableCohortUI
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
            this.tbID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSaved = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbOverrideReleaseIdentifierSQL = new System.Windows.Forms.TextBox();
            this.pSqlPreview = new System.Windows.Forms.Panel();
            this.pDescription = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(161, 19);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "ID:";
            // 
            // lblSaved
            // 
            this.lblSaved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSaved.AutoSize = true;
            this.lblSaved.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSaved.Location = new System.Drawing.Point(998, 830);
            this.lblSaved.Name = "lblSaved";
            this.lblSaved.Size = new System.Drawing.Size(47, 13);
            this.lblSaved.TabIndex = 0;
            this.lblSaved.Text = "Saved...";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(5, 77);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 13;
            this.lblDescription.Text = "Description:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "OverrideReleaseIdentifierSQL:";
            // 
            // tbOverrideReleaseIdentifierSQL
            // 
            this.tbOverrideReleaseIdentifierSQL.Location = new System.Drawing.Point(161, 45);
            this.tbOverrideReleaseIdentifierSQL.Name = "tbOverrideReleaseIdentifierSQL";
            this.tbOverrideReleaseIdentifierSQL.Size = new System.Drawing.Size(468, 20);
            this.tbOverrideReleaseIdentifierSQL.TabIndex = 12;
            this.tbOverrideReleaseIdentifierSQL.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // pSqlPreview
            // 
            this.pSqlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSqlPreview.Location = new System.Drawing.Point(8, 233);
            this.pSqlPreview.Name = "pSqlPreview";
            this.pSqlPreview.Size = new System.Drawing.Size(1035, 565);
            this.pSqlPreview.TabIndex = 14;
            // 
            // pDescription
            // 
            this.pDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pDescription.Location = new System.Drawing.Point(74, 71);
            this.pDescription.Name = "pDescription";
            this.pDescription.Size = new System.Drawing.Size(969, 156);
            this.pDescription.TabIndex = 15;
            // 
            // ExtractableCohortUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pDescription);
            this.Controls.Add(this.pSqlPreview);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.tbOverrideReleaseIdentifierSQL);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblSaved);
            this.Name = "ExtractableCohortUI";
            this.Size = new System.Drawing.Size(1046, 843);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSaved;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbOverrideReleaseIdentifierSQL;
        private System.Windows.Forms.Panel pSqlPreview;
        private System.Windows.Forms.Panel pDescription;
    }
}
