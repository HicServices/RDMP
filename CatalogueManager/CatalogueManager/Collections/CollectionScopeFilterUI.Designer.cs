namespace CatalogueManager.Collections
{
    partial class CollectionScopeFilterUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollectionScopeFilterUI));
            this.lblFilter = new System.Windows.Forms.Label();
            this.pbRemoveFilter = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFilter
            // 
            this.lblFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFilter.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblFilter.Location = new System.Drawing.Point(2, 3);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(300, 16);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "Filtered Object";
            // 
            // pbRemoveFilter
            // 
            this.pbRemoveFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbRemoveFilter.Image = ((System.Drawing.Image)(resources.GetObject("pbRemoveFilter.Image")));
            this.pbRemoveFilter.Location = new System.Drawing.Point(300, 0);
            this.pbRemoveFilter.Name = "pbRemoveFilter";
            this.pbRemoveFilter.Size = new System.Drawing.Size(19, 19);
            this.pbRemoveFilter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbRemoveFilter.TabIndex = 1;
            this.pbRemoveFilter.TabStop = false;
            this.pbRemoveFilter.Click += new System.EventHandler(this.pbRemoveFilter_Click);
            // 
            // CollectionScopeFilterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Goldenrod;
            this.Controls.Add(this.pbRemoveFilter);
            this.Controls.Add(this.lblFilter);
            this.Name = "CollectionScopeFilterUI";
            this.Size = new System.Drawing.Size(319, 19);
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.PictureBox pbRemoveFilter;
    }
}
