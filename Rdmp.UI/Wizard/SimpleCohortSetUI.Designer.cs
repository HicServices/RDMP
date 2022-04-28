using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.Wizard
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
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddFilter = new System.Windows.Forms.Button();
            this.ddAvailableFilters = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gbFilters = new System.Windows.Forms.GroupBox();
            this.ddAndOr = new System.Windows.Forms.ComboBox();
            this.pbFilters = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxColumns = new Rdmp.UI.SuggestComboBox();
            this.btnLockExtractionIdentifier = new System.Windows.Forms.Button();
            this.pbCatalogue = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbExtractionIdentifier = new System.Windows.Forms.PictureBox();
            this.cbxCatalogues = new Rdmp.UI.SimpleControls.SelectIMapsDirectlyToDatabaseTableComboBox();
            this.gbFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFilters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCatalogue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionIdentifier)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(611, 3);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(25, 25);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnAddFilter
            // 
            this.btnAddFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFilter.Image = ((System.Drawing.Image)(resources.GetObject("btnAddFilter.Image")));
            this.btnAddFilter.Location = new System.Drawing.Point(501, 21);
            this.btnAddFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
            this.ddAvailableFilters.Location = new System.Drawing.Point(68, 22);
            this.ddAvailableFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddAvailableFilters.Name = "ddAvailableFilters";
            this.ddAvailableFilters.Size = new System.Drawing.Size(430, 23);
            this.ddAvailableFilters.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 15);
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
            this.gbFilters.Location = new System.Drawing.Point(4, 65);
            this.gbFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbFilters.Name = "gbFilters";
            this.gbFilters.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbFilters.Size = new System.Drawing.Size(604, 208);
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
            this.ddAndOr.Location = new System.Drawing.Point(529, 22);
            this.ddAndOr.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddAndOr.Name = "ddAndOr";
            this.ddAndOr.Size = new System.Drawing.Size(67, 23);
            this.ddAndOr.TabIndex = 16;
            this.ddAndOr.SelectedIndexChanged += new System.EventHandler(this.ddAndOr_SelectedIndexChanged);
            // 
            // pbFilters
            // 
            this.pbFilters.Location = new System.Drawing.Point(10, 23);
            this.pbFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbFilters.Name = "pbFilters";
            this.pbFilters.Size = new System.Drawing.Size(23, 23);
            this.pbFilters.TabIndex = 15;
            this.pbFilters.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 53);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(587, 149);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Identifier:";
            // 
            // cbxColumns
            // 
            this.cbxColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxColumns.FilterRule = null;
            this.cbxColumns.FormattingEnabled = true;
            this.cbxColumns.Location = new System.Drawing.Point(117, 36);
            this.cbxColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbxColumns.Name = "cbxColumns";
            this.cbxColumns.PropertySelector = null;
            this.cbxColumns.Size = new System.Drawing.Size(465, 23);
            this.cbxColumns.SuggestBoxHeight = 126;
            this.cbxColumns.SuggestListOrderRule = null;
            this.cbxColumns.TabIndex = 1;
            // 
            // btnLockExtractionIdentifier
            // 
            this.btnLockExtractionIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLockExtractionIdentifier.Location = new System.Drawing.Point(584, 35);
            this.btnLockExtractionIdentifier.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnLockExtractionIdentifier.Name = "btnLockExtractionIdentifier";
            this.btnLockExtractionIdentifier.Size = new System.Drawing.Size(25, 25);
            this.btnLockExtractionIdentifier.TabIndex = 11;
            this.btnLockExtractionIdentifier.UseVisualStyleBackColor = true;
            this.btnLockExtractionIdentifier.Click += new System.EventHandler(this.btnLockExtractionIdentifier_Click);
            // 
            // pbCatalogue
            // 
            this.pbCatalogue.Location = new System.Drawing.Point(91, 5);
            this.pbCatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbCatalogue.Name = "pbCatalogue";
            this.pbCatalogue.Size = new System.Drawing.Size(23, 23);
            this.pbCatalogue.TabIndex = 14;
            this.pbCatalogue.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Catalogue:";
            // 
            // pbExtractionIdentifier
            // 
            this.pbExtractionIdentifier.Location = new System.Drawing.Point(91, 34);
            this.pbExtractionIdentifier.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbExtractionIdentifier.Name = "pbExtractionIdentifier";
            this.pbExtractionIdentifier.Size = new System.Drawing.Size(23, 23);
            this.pbExtractionIdentifier.TabIndex = 14;
            this.pbExtractionIdentifier.TabStop = false;
            // 
            // cbxCatalogues
            // 
            this.cbxCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxCatalogues.Location = new System.Drawing.Point(116, 3);
            this.cbxCatalogues.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.cbxCatalogues.Name = "cbxCatalogues";
            this.cbxCatalogues.SelectedItem = null;
            this.cbxCatalogues.Size = new System.Drawing.Size(493, 28);
            this.cbxCatalogues.TabIndex = 17;
            this.cbxCatalogues.SelectedItemChanged += new System.EventHandler<System.EventArgs>(this.cbxCatalogues_SelectedIndexChanged);
            // 
            // SimpleCohortSetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbxCatalogues);
            this.Controls.Add(this.pbExtractionIdentifier);
            this.Controls.Add(this.pbCatalogue);
            this.Controls.Add(this.btnLockExtractionIdentifier);
            this.Controls.Add(this.gbFilters);
            this.Controls.Add(this.cbxColumns);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SimpleCohortSetUI";
            this.Size = new System.Drawing.Size(642, 284);
            this.gbFilters.ResumeLayout(false);
            this.gbFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFilters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCatalogue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbExtractionIdentifier)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddFilter;
        private System.Windows.Forms.ComboBox ddAvailableFilters;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbFilters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private SuggestComboBox cbxColumns;
        private System.Windows.Forms.Button btnLockExtractionIdentifier;
        private System.Windows.Forms.PictureBox pbCatalogue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbExtractionIdentifier;
        private System.Windows.Forms.PictureBox pbFilters;
        private System.Windows.Forms.ComboBox ddAndOr;
        private SelectIMapsDirectlyToDatabaseTableComboBox cbxCatalogues;
    }
}
