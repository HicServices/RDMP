namespace CohortManager.Wizard
{
    partial class SimpleCohortSetUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleCohortSetUI));
            this.cbxCatalogues = new ReusableUIComponents.SuggestComboBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddFilter = new System.Windows.Forms.Button();
            this.ddAvailableFilters = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gbFilters = new System.Windows.Forms.GroupBox();
            this.ddAndOr = new System.Windows.Forms.ComboBox();
            this.pbFilters = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxColumns = new ReusableUIComponents.SuggestComboBox();
            this.btnLockExtractionIdentifier = new System.Windows.Forms.Button();
            this.pbCatalogue = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbExtractionIdentifier = new System.Windows.Forms.PictureBox();
            this.gbFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFilters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCatalogue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionIdentifier)).BeginInit();
            this.SuspendLayout();
            // 
            // cbxCatalogues
            // 
            this.cbxCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxCatalogues.FilterRule = null;
            this.cbxCatalogues.FormattingEnabled = true;
            this.cbxCatalogues.Location = new System.Drawing.Point(116, 9);
            this.cbxCatalogues.Name = "cbxCatalogues";
            this.cbxCatalogues.PropertySelector = null;
            this.cbxCatalogues.Size = new System.Drawing.Size(396, 21);
            this.cbxCatalogues.SuggestBoxHeight = 96;
            this.cbxCatalogues.SuggestListOrderRule = null;
            this.cbxCatalogues.TabIndex = 1;
            this.cbxCatalogues.SelectedIndexChanged += new System.EventHandler(this.cbxCatalogues_SelectedIndexChanged);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(518, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(25, 25);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnAddFilter
            // 
            this.btnAddFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFilter.Image = ((System.Drawing.Image)(resources.GetObject("btnAddFilter.Image")));
            this.btnAddFilter.Location = new System.Drawing.Point(423, 16);
            this.btnAddFilter.Name = "btnAddFilter";
            this.btnAddFilter.Size = new System.Drawing.Size(25, 25);
            this.btnAddFilter.TabIndex = 6;
            this.btnAddFilter.UseVisualStyleBackColor = true;
            this.btnAddFilter.Click += new System.EventHandler(this.btnAddFilter_Click);
            // 
            // ddAvailableFilters
            // 
            this.ddAvailableFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddAvailableFilters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAvailableFilters.FormattingEnabled = true;
            this.ddAvailableFilters.Location = new System.Drawing.Point(69, 19);
            this.ddAvailableFilters.Name = "ddAvailableFilters";
            this.ddAvailableFilters.Size = new System.Drawing.Size(348, 21);
            this.ddAvailableFilters.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Add:";
            // 
            // gbFilters
            // 
            this.gbFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFilters.Controls.Add(this.ddAndOr);
            this.gbFilters.Controls.Add(this.pbFilters);
            this.gbFilters.Controls.Add(this.tableLayoutPanel1);
            this.gbFilters.Controls.Add(this.ddAvailableFilters);
            this.gbFilters.Controls.Add(this.btnAddFilter);
            this.gbFilters.Controls.Add(this.label3);
            this.gbFilters.Location = new System.Drawing.Point(21, 75);
            this.gbFilters.Name = "gbFilters";
            this.gbFilters.Size = new System.Drawing.Size(511, 267);
            this.gbFilters.TabIndex = 8;
            this.gbFilters.TabStop = false;
            this.gbFilters.Text = "Filters";
            // 
            // ddAndOr
            // 
            this.ddAndOr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddAndOr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAndOr.Enabled = false;
            this.ddAndOr.FormattingEnabled = true;
            this.ddAndOr.Location = new System.Drawing.Point(453, 19);
            this.ddAndOr.Name = "ddAndOr";
            this.ddAndOr.Size = new System.Drawing.Size(52, 21);
            this.ddAndOr.TabIndex = 16;
            this.ddAndOr.SelectedIndexChanged += new System.EventHandler(this.ddAndOr_SelectedIndexChanged);
            // 
            // pbFilters
            // 
            this.pbFilters.Location = new System.Drawing.Point(9, 20);
            this.pbFilters.Name = "pbFilters";
            this.pbFilters.Size = new System.Drawing.Size(20, 20);
            this.pbFilters.TabIndex = 15;
            this.pbFilters.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 46);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(496, 215);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Identifier Field:";
            // 
            // cbxColumns
            // 
            this.cbxColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxColumns.FilterRule = null;
            this.cbxColumns.FormattingEnabled = true;
            this.cbxColumns.Location = new System.Drawing.Point(116, 36);
            this.cbxColumns.Name = "cbxColumns";
            this.cbxColumns.PropertySelector = null;
            this.cbxColumns.Size = new System.Drawing.Size(367, 21);
            this.cbxColumns.SuggestBoxHeight = 96;
            this.cbxColumns.SuggestListOrderRule = null;
            this.cbxColumns.TabIndex = 1;
            // 
            // btnLockExtractionIdentifier
            // 
            this.btnLockExtractionIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLockExtractionIdentifier.Location = new System.Drawing.Point(489, 33);
            this.btnLockExtractionIdentifier.Name = "btnLockExtractionIdentifier";
            this.btnLockExtractionIdentifier.Size = new System.Drawing.Size(25, 25);
            this.btnLockExtractionIdentifier.TabIndex = 11;
            this.btnLockExtractionIdentifier.UseVisualStyleBackColor = true;
            this.btnLockExtractionIdentifier.Click += new System.EventHandler(this.btnLockExtractionIdentifier_Click);
            // 
            // pbCatalogue
            // 
            this.pbCatalogue.Location = new System.Drawing.Point(90, 9);
            this.pbCatalogue.Name = "pbCatalogue";
            this.pbCatalogue.Size = new System.Drawing.Size(20, 20);
            this.pbCatalogue.TabIndex = 14;
            this.pbCatalogue.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Data Set";
            // 
            // pbExtractionIdentifier
            // 
            this.pbExtractionIdentifier.Location = new System.Drawing.Point(90, 35);
            this.pbExtractionIdentifier.Name = "pbExtractionIdentifier";
            this.pbExtractionIdentifier.Size = new System.Drawing.Size(20, 20);
            this.pbExtractionIdentifier.TabIndex = 14;
            this.pbExtractionIdentifier.TabStop = false;
            // 
            // SimpleCohortSetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbExtractionIdentifier);
            this.Controls.Add(this.pbCatalogue);
            this.Controls.Add(this.btnLockExtractionIdentifier);
            this.Controls.Add(this.gbFilters);
            this.Controls.Add(this.cbxColumns);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.cbxCatalogues);
            this.Controls.Add(this.label1);
            this.Name = "SimpleCohortSetUI";
            this.Size = new System.Drawing.Size(546, 345);
            this.gbFilters.ResumeLayout(false);
            this.gbFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFilters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCatalogue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionIdentifier)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ReusableUIComponents.SuggestComboBox cbxCatalogues;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddFilter;
        private System.Windows.Forms.ComboBox ddAvailableFilters;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbFilters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private ReusableUIComponents.SuggestComboBox cbxColumns;
        private System.Windows.Forms.Button btnLockExtractionIdentifier;
        private System.Windows.Forms.PictureBox pbCatalogue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbExtractionIdentifier;
        private System.Windows.Forms.PictureBox pbFilters;
        private System.Windows.Forms.ComboBox ddAndOr;
    }
}
