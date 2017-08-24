namespace CatalogueManager.SimpleDialogs
{
    partial class ImportCloneOfCatalogueItem
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
            this.lbl_CatalogueToImportInto = new System.Windows.Forms.Label();
            this.cbx_CatalogueToImportFrom = new System.Windows.Forms.ComboBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_CatalogueToImportInto
            // 
            this.lbl_CatalogueToImportInto.AutoSize = true;
            this.lbl_CatalogueToImportInto.Location = new System.Drawing.Point(26, 13);
            this.lbl_CatalogueToImportInto.Name = "lbl_CatalogueToImportInto";
            this.lbl_CatalogueToImportInto.Size = new System.Drawing.Size(70, 13);
            this.lbl_CatalogueToImportInto.TabIndex = 0;
            this.lbl_CatalogueToImportInto.Text = "Import is for...";
            // 
            // cbx_CatalogueToImportFrom
            // 
            this.cbx_CatalogueToImportFrom.FormattingEnabled = true;
            this.cbx_CatalogueToImportFrom.Location = new System.Drawing.Point(29, 29);
            this.cbx_CatalogueToImportFrom.Name = "cbx_CatalogueToImportFrom";
            this.cbx_CatalogueToImportFrom.Size = new System.Drawing.Size(587, 21);
            this.cbx_CatalogueToImportFrom.TabIndex = 1;
            this.cbx_CatalogueToImportFrom.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbx_CatalogueToImportFrom_KeyUp);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(121, 55);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(125, 39);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import ";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(368, 56);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(128, 39);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ImportCloneOfCatalogueItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 106);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.cbx_CatalogueToImportFrom);
            this.Controls.Add(this.lbl_CatalogueToImportInto);
            this.Name = "ImportCloneOfCatalogueItem";
            this.Text = "ImportCloneOfCatalogueItem";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ImportCloneOfCatalogueItem_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_CatalogueToImportInto;
        private System.Windows.Forms.ComboBox cbx_CatalogueToImportFrom;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnClose;
    }
}