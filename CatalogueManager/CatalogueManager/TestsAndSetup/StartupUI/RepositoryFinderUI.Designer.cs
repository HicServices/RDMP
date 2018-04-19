using ReusableLibraryCode.Checks;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    partial class RepositoryFinderUI 
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblExport = new System.Windows.Forms.Label();
            this.lblCatalogue = new System.Windows.Forms.Label();
            this.btnChange = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Location = new System.Drawing.Point(217, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection Strings";
            // 
            // lblExport
            // 
            this.lblExport.AutoSize = true;
            this.lblExport.Location = new System.Drawing.Point(28, 37);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(40, 13);
            this.lblExport.TabIndex = 1;
            this.lblExport.Text = "Export:";
            // 
            // lblCatalogue
            // 
            this.lblCatalogue.AutoSize = true;
            this.lblCatalogue.Location = new System.Drawing.Point(10, 20);
            this.lblCatalogue.Name = "lblCatalogue";
            this.lblCatalogue.Size = new System.Drawing.Size(58, 13);
            this.lblCatalogue.TabIndex = 1;
            this.lblCatalogue.Text = "Catalogue:";
            // 
            // btnChange
            // 
            this.btnChange.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnChange.Location = new System.Drawing.Point(205, 50);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(123, 23);
            this.btnChange.TabIndex = 3;
            this.btnChange.Text = "Change...";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // RepositoryFinderUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnChange);
            this.Controls.Add(this.lblCatalogue);
            this.Controls.Add(this.lblExport);
            this.Controls.Add(this.label1);
            this.Name = "RepositoryFinderUI";
            this.Size = new System.Drawing.Size(523, 75);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblExport;
        private System.Windows.Forms.Label lblCatalogue;
        private System.Windows.Forms.Button btnChange;
    }
}
