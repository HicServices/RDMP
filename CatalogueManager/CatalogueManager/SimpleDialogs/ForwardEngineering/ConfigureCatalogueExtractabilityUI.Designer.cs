namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    partial class ConfigureCatalogueExtractabilityUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureCatalogueExtractabilityUI));
            this.olvColumnExtractability = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractionCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIsExtractionIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddToExisting = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ddCategoriseMany = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.pChangeAll = new System.Windows.Forms.Panel();
            this.pFilter = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).BeginInit();
            this.pChangeAll.SuspendLayout();
            this.pFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvColumnExtractability
            // 
            this.olvColumnExtractability.AllColumns.Add(this.olvColumnInfoName);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractionCategory);
            this.olvColumnExtractability.AllColumns.Add(this.olvIsExtractionIdentifier);
            this.olvColumnExtractability.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvColumnExtractability.CellEditUseWholeCell = false;
            this.olvColumnExtractability.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnInfoName,
            this.olvExtractionCategory,
            this.olvIsExtractionIdentifier});
            this.olvColumnExtractability.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvColumnExtractability.FullRowSelect = true;
            this.olvColumnExtractability.Location = new System.Drawing.Point(6, 28);
            this.olvColumnExtractability.Name = "olvColumnExtractability";
            this.olvColumnExtractability.RowHeight = 19;
            this.olvColumnExtractability.ShowGroups = false;
            this.olvColumnExtractability.Size = new System.Drawing.Size(751, 530);
            this.olvColumnExtractability.TabIndex = 7;
            this.olvColumnExtractability.UseCompatibleStateImageBehavior = false;
            this.olvColumnExtractability.UseFiltering = true;
            this.olvColumnExtractability.View = System.Windows.Forms.View.Details;
            // 
            // olvColumnInfoName
            // 
            this.olvColumnInfoName.AspectName = "ToString";
            this.olvColumnInfoName.FillsFreeSpace = true;
            this.olvColumnInfoName.Groupable = false;
            this.olvColumnInfoName.Text = "Name";
            // 
            // olvExtractionCategory
            // 
            this.olvExtractionCategory.Text = "Is Extractable";
            this.olvExtractionCategory.Width = 100;
            // 
            // olvIsExtractionIdentifier
            // 
            this.olvIsExtractionIdentifier.CheckBoxes = true;
            this.olvIsExtractionIdentifier.Text = "IsExtractionIdentifier";
            this.olvIsExtractionIdentifier.Width = 106;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(269, 597);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(207, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel (Do not create a Catalogue)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddToExisting
            // 
            this.btnAddToExisting.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAddToExisting.Location = new System.Drawing.Point(482, 597);
            this.btnAddToExisting.Name = "btnAddToExisting";
            this.btnAddToExisting.Size = new System.Drawing.Size(161, 23);
            this.btnAddToExisting.TabIndex = 7;
            this.btnAddToExisting.Text = "Add to existing Catalogue";
            this.btnAddToExisting.UseVisualStyleBackColor = true;
            this.btnAddToExisting.Click += new System.EventHandler(this.btnAddToExisting_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(144, 597);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(119, 23);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Set All:";
            // 
            // ddCategoriseMany
            // 
            this.ddCategoriseMany.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddCategoriseMany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCategoriseMany.FormattingEnabled = true;
            this.ddCategoriseMany.Location = new System.Drawing.Point(50, 3);
            this.ddCategoriseMany.Name = "ddCategoriseMany";
            this.ddCategoriseMany.Size = new System.Drawing.Size(207, 21);
            this.ddCategoriseMany.TabIndex = 5;
            this.ddCategoriseMany.SelectedIndexChanged += new System.EventHandler(this.ddCategoriseMany_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(441, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(201, 3);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 8;
            // 
            // pChangeAll
            // 
            this.pChangeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pChangeAll.Controls.Add(this.ddCategoriseMany);
            this.pChangeAll.Controls.Add(this.label3);
            this.pChangeAll.Location = new System.Drawing.Point(497, 564);
            this.pChangeAll.Name = "pChangeAll";
            this.pChangeAll.Size = new System.Drawing.Size(260, 26);
            this.pChangeAll.TabIndex = 8;
            // 
            // pFilter
            // 
            this.pFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pFilter.Controls.Add(this.label6);
            this.pFilter.Controls.Add(this.tbFilter);
            this.pFilter.Location = new System.Drawing.Point(6, 564);
            this.pFilter.Name = "pFilter";
            this.pFilter.Size = new System.Drawing.Size(485, 27);
            this.pFilter.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Choose which columns are extractable";
            // 
            // ConfigureCatalogueExtractabilityUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 622);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pChangeAll);
            this.Controls.Add(this.btnAddToExisting);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.pFilter);
            this.Controls.Add(this.olvColumnExtractability);
            this.Name = "ConfigureCatalogueExtractabilityUI";
            this.Text = "ConfigureCatalogueExtractabilityUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigureCatalogueExtractabilityUI_FormClosing);
            this.Load += new System.EventHandler(this.ConfigureCatalogueExtractabilityUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).EndInit();
            this.pChangeAll.ResumeLayout(false);
            this.pChangeAll.PerformLayout();
            this.pFilter.ResumeLayout(false);
            this.pFilter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilter;
        private BrightIdeasSoftware.ObjectListView olvColumnExtractability;
        private BrightIdeasSoftware.OLVColumn olvColumnInfoName;
        private BrightIdeasSoftware.OLVColumn olvExtractionCategory;
        private System.Windows.Forms.ComboBox ddCategoriseMany;
        private System.Windows.Forms.Label label3;
        private BrightIdeasSoftware.OLVColumn olvIsExtractionIdentifier;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnAddToExisting;
        private ReusableUIComponents.HelpIcon helpIcon1;
        private System.Windows.Forms.Panel pChangeAll;
        private System.Windows.Forms.Panel pFilter;
        private System.Windows.Forms.Label label1;
    }
}
