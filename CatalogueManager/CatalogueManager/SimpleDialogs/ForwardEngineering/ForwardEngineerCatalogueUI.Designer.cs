namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    partial class ForwardEngineerCatalogueUI
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
            this.cbGenerateEntireCatalogue = new System.Windows.Forms.RadioButton();
            this.cbAddToExisting = new System.Windows.Forms.RadioButton();
            this.cbxExistingCatalogue = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxFolder = new System.Windows.Forms.ComboBox();
            this.gbCreateCatalogue = new System.Windows.Forms.GroupBox();
            this.configureCatalogueExtractabilityUI1 = new CatalogueManager.SimpleDialogs.ForwardEngineering.ConfigureCatalogueExtractabilityUI();
            this.rbYesToCatalogue = new System.Windows.Forms.RadioButton();
            this.rbNoToCatalogue = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbCreateCatalogue.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbGenerateEntireCatalogue
            // 
            this.cbGenerateEntireCatalogue.AutoSize = true;
            this.cbGenerateEntireCatalogue.Location = new System.Drawing.Point(88, 20);
            this.cbGenerateEntireCatalogue.Name = "cbGenerateEntireCatalogue";
            this.cbGenerateEntireCatalogue.Size = new System.Drawing.Size(387, 17);
            this.cbGenerateEntireCatalogue.TabIndex = 0;
            this.cbGenerateEntireCatalogue.TabStop = true;
            this.cbGenerateEntireCatalogue.Text = "Generate new Catalogue with 1 to 1 mapping CatalogueItems to ColumnInfos";
            this.cbGenerateEntireCatalogue.UseVisualStyleBackColor = true;
            this.cbGenerateEntireCatalogue.CheckedChanged += new System.EventHandler(this.cbGenerateEntireCatalogue_CheckedChanged);
            // 
            // cbAddToExisting
            // 
            this.cbAddToExisting.AutoSize = true;
            this.cbAddToExisting.Location = new System.Drawing.Point(88, 43);
            this.cbAddToExisting.Name = "cbAddToExisting";
            this.cbAddToExisting.Size = new System.Drawing.Size(432, 17);
            this.cbAddToExisting.TabIndex = 1;
            this.cbAddToExisting.TabStop = true;
            this.cbAddToExisting.Text = "Add to existing Catalogue, creating a 1 to 1 mapping of CatalogueItems to ColumnI" +
    "nfos";
            this.cbAddToExisting.UseVisualStyleBackColor = true;
            this.cbAddToExisting.CheckedChanged += new System.EventHandler(this.cbAddToExisting_CheckedChanged);
            // 
            // cbxExistingCatalogue
            // 
            this.cbxExistingCatalogue.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbxExistingCatalogue.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxExistingCatalogue.FormattingEnabled = true;
            this.cbxExistingCatalogue.Location = new System.Drawing.Point(159, 65);
            this.cbxExistingCatalogue.Name = "cbxExistingCatalogue";
            this.cbxExistingCatalogue.Size = new System.Drawing.Size(262, 21);
            this.cbxExistingCatalogue.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(222, 280);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(113, 25);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(353, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Make Table an extractable data set (Catalogue)?";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Catalogue Folder";
            // 
            // cbxFolder
            // 
            this.cbxFolder.FormattingEnabled = true;
            this.cbxFolder.Location = new System.Drawing.Point(98, 143);
            this.cbxFolder.Name = "cbxFolder";
            this.cbxFolder.Size = new System.Drawing.Size(483, 21);
            this.cbxFolder.TabIndex = 9;
            this.cbxFolder.SelectedIndexChanged += new System.EventHandler(this.cbxFolder_SelectedIndexChanged);
            this.cbxFolder.TextChanged += new System.EventHandler(this.cbxFolder_TextChanged);
            // 
            // gbCreateCatalogue
            // 
            this.gbCreateCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbCreateCatalogue.Controls.Add(this.configureCatalogueExtractabilityUI1);
            this.gbCreateCatalogue.Controls.Add(this.cbAddToExisting);
            this.gbCreateCatalogue.Controls.Add(this.cbxFolder);
            this.gbCreateCatalogue.Controls.Add(this.label3);
            this.gbCreateCatalogue.Controls.Add(this.cbGenerateEntireCatalogue);
            this.gbCreateCatalogue.Controls.Add(this.cbxExistingCatalogue);
            this.gbCreateCatalogue.Enabled = false;
            this.gbCreateCatalogue.Location = new System.Drawing.Point(60, 61);
            this.gbCreateCatalogue.Name = "gbCreateCatalogue";
            this.gbCreateCatalogue.Size = new System.Drawing.Size(632, 187);
            this.gbCreateCatalogue.TabIndex = 10;
            this.gbCreateCatalogue.TabStop = false;
            this.gbCreateCatalogue.Text = "Create Catalogue";
            // 
            // configureCatalogueExtractabilityUI1
            // 
            this.configureCatalogueExtractabilityUI1.AutoSize = true;
            this.configureCatalogueExtractabilityUI1.Location = new System.Drawing.Point(12, 92);
            this.configureCatalogueExtractabilityUI1.Name = "configureCatalogueExtractabilityUI1";
            this.configureCatalogueExtractabilityUI1.Size = new System.Drawing.Size(614, 45);
            this.configureCatalogueExtractabilityUI1.TabIndex = 10;
            // 
            // rbYesToCatalogue
            // 
            this.rbYesToCatalogue.AutoSize = true;
            this.rbYesToCatalogue.Location = new System.Drawing.Point(40, 33);
            this.rbYesToCatalogue.Name = "rbYesToCatalogue";
            this.rbYesToCatalogue.Size = new System.Drawing.Size(140, 17);
            this.rbYesToCatalogue.TabIndex = 11;
            this.rbYesToCatalogue.TabStop = true;
            this.rbYesToCatalogue.Text = "Yes, Create a Catalogue";
            this.rbYesToCatalogue.UseVisualStyleBackColor = true;
            this.rbYesToCatalogue.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbNoToCatalogue
            // 
            this.rbNoToCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbNoToCatalogue.AutoSize = true;
            this.rbNoToCatalogue.Location = new System.Drawing.Point(40, 254);
            this.rbNoToCatalogue.Name = "rbNoToCatalogue";
            this.rbNoToCatalogue.Size = new System.Drawing.Size(170, 17);
            this.rbNoToCatalogue.TabIndex = 11;
            this.rbNoToCatalogue.TabStop = true;
            this.rbNoToCatalogue.Text = "No, Do not create a Catalogue";
            this.rbNoToCatalogue.UseVisualStyleBackColor = true;
            this.rbNoToCatalogue.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(341, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(113, 25);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ForwardEngineerCatalogueUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 317);
            this.Controls.Add(this.rbNoToCatalogue);
            this.Controls.Add(this.rbYesToCatalogue);
            this.Controls.Add(this.gbCreateCatalogue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "ForwardEngineerCatalogueUI";
            this.Text = "Create Catalogue (ForwardEngineerCatalogueUI)";
            this.Load += new System.EventHandler(this.ForwardEngineerCatalogue_Load);
            this.gbCreateCatalogue.ResumeLayout(false);
            this.gbCreateCatalogue.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton cbGenerateEntireCatalogue;
        private System.Windows.Forms.RadioButton cbAddToExisting;
        private System.Windows.Forms.ComboBox cbxExistingCatalogue;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxFolder;
        private System.Windows.Forms.GroupBox gbCreateCatalogue;
        private System.Windows.Forms.RadioButton rbYesToCatalogue;
        private System.Windows.Forms.RadioButton rbNoToCatalogue;
        private System.Windows.Forms.Button btnCancel;
        private ConfigureCatalogueExtractabilityUI configureCatalogueExtractabilityUI1;
    }
}