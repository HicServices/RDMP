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
            this.gbMarkAllExtractable = new System.Windows.Forms.GroupBox();
            this.olvColumnExtractability = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractionCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractable = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIsExtractionIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddToExisting = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ddCategoriseMany = new System.Windows.Forms.ComboBox();
            this.btnClearFilterMany = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbPastedColumns = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.gbMarkAllExtractable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMarkAllExtractable
            // 
            this.gbMarkAllExtractable.Controls.Add(this.olvColumnExtractability);
            this.gbMarkAllExtractable.Controls.Add(this.panel2);
            this.gbMarkAllExtractable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMarkAllExtractable.Location = new System.Drawing.Point(0, 0);
            this.gbMarkAllExtractable.Name = "gbMarkAllExtractable";
            this.gbMarkAllExtractable.Size = new System.Drawing.Size(722, 717);
            this.gbMarkAllExtractable.TabIndex = 7;
            this.gbMarkAllExtractable.TabStop = false;
            this.gbMarkAllExtractable.Text = "Extractability";
            // 
            // olvColumnExtractability
            // 
            this.olvColumnExtractability.AllColumns.Add(this.olvColumnInfoName);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractionCategory);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractable);
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
            this.olvColumnExtractability.Location = new System.Drawing.Point(3, 19);
            this.olvColumnExtractability.Name = "olvColumnExtractability";
            this.olvColumnExtractability.RowHeight = 19;
            this.olvColumnExtractability.ShowGroups = false;
            this.olvColumnExtractability.Size = new System.Drawing.Size(716, 485);
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
            this.olvColumnInfoName.Text = "Column Name";
            // 
            // olvExtractionCategory
            // 
            this.olvExtractionCategory.Groupable = false;
            this.olvExtractionCategory.Text = "Catgeory";
            // 
            // olvExtractable
            // 
            this.olvExtractable.DisplayIndex = 2;
            this.olvExtractable.IsVisible = false;
            // 
            // olvIsExtractionIdentifier
            // 
            this.olvIsExtractionIdentifier.Text = "Is Patient Id";
            this.olvIsExtractionIdentifier.Width = 106;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnAddToExisting);
            this.panel2.Controls.Add(this.btnOk);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.ddCategoriseMany);
            this.panel2.Controls.Add(this.btnClearFilterMany);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lbPastedColumns);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.tbFilter);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 507);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(716, 207);
            this.panel2.TabIndex = 5;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(214, 175);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(207, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel (Do not create a Catalogue)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddToExisting
            // 
            this.btnAddToExisting.Location = new System.Drawing.Point(427, 175);
            this.btnAddToExisting.Name = "btnAddToExisting";
            this.btnAddToExisting.Size = new System.Drawing.Size(161, 23);
            this.btnAddToExisting.TabIndex = 7;
            this.btnAddToExisting.Text = "Add to existing Catalogue";
            this.btnAddToExisting.UseVisualStyleBackColor = true;
            this.btnAddToExisting.Click += new System.EventHandler(this.btnAddToExisting_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(89, 175);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(119, 23);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(472, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Set Category for all:";
            // 
            // ddCategoriseMany
            // 
            this.ddCategoriseMany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCategoriseMany.FormattingEnabled = true;
            this.ddCategoriseMany.Location = new System.Drawing.Point(578, 6);
            this.ddCategoriseMany.Name = "ddCategoriseMany";
            this.ddCategoriseMany.Size = new System.Drawing.Size(121, 21);
            this.ddCategoriseMany.TabIndex = 5;
            this.ddCategoriseMany.SelectedIndexChanged += new System.EventHandler(this.ddCategoriseMany_SelectedIndexChanged);
            // 
            // btnClearFilterMany
            // 
            this.btnClearFilterMany.Enabled = false;
            this.btnClearFilterMany.Location = new System.Drawing.Point(37, 57);
            this.btnClearFilterMany.Name = "btnClearFilterMany";
            this.btnClearFilterMany.Size = new System.Drawing.Size(75, 23);
            this.btnClearFilterMany.TabIndex = 4;
            this.btnClearFilterMany.Text = "Clear";
            this.btnClearFilterMany.UseVisualStyleBackColor = true;
            this.btnClearFilterMany.Click += new System.EventHandler(this.btnClearFilterMany_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter many (paste in):";
            // 
            // lbPastedColumns
            // 
            this.lbPastedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPastedColumns.FormattingEnabled = true;
            this.lbPastedColumns.Location = new System.Drawing.Point(118, 29);
            this.lbPastedColumns.Name = "lbPastedColumns";
            this.lbPastedColumns.Size = new System.Drawing.Size(257, 134);
            this.lbPastedColumns.TabIndex = 2;
            this.lbPastedColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbPastedColumns_KeyUp);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Location = new System.Drawing.Point(118, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(351, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // ConfigureCatalogueExtractabilityUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 717);
            this.Controls.Add(this.gbMarkAllExtractable);
            this.Name = "ConfigureCatalogueExtractabilityUI";
            this.Text = "ConfigureCatalogueExtractabilityUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigureCatalogueExtractabilityUI_FormClosing);
            this.gbMarkAllExtractable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbMarkAllExtractable;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilter;
        private BrightIdeasSoftware.ObjectListView olvColumnExtractability;
        private BrightIdeasSoftware.OLVColumn olvColumnInfoName;
        private BrightIdeasSoftware.OLVColumn olvExtractionCategory;
        private BrightIdeasSoftware.OLVColumn olvExtractable;
        private System.Windows.Forms.ListBox lbPastedColumns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearFilterMany;
        private System.Windows.Forms.ComboBox ddCategoriseMany;
        private System.Windows.Forms.Label label3;
        private BrightIdeasSoftware.OLVColumn olvIsExtractionIdentifier;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnAddToExisting;
    }
}
