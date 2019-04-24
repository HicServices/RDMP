using BrightIdeasSoftware;

namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    partial class LookupConfigurationUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.olvExtractionInformations = new BrightIdeasSoftware.ObjectListView();
            this.olvExtractionInformationsNameColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tbCollation = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.olvSelectedDescriptionColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvDescriptionsColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLookupColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvLookupNameColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnImportNewTableInfo = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbxLookup = new MapsDirectlyToDatabaseTableUI.SelectIMapsDirectlyToDatabaseTableComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tbCatalogue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnPrimaryKeyCompositeHelp = new System.Windows.Forms.Button();
            this.btnCreateLookup = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.fk3 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.fk2 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.fk1 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.pk2 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.pk3 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.pk1 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectedDescriptionColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvLookupColumns)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 354);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Extractable Dataset Columns";
            // 
            // olvExtractionInformations
            // 
            this.olvExtractionInformations.AllColumns.Add(this.olvExtractionInformationsNameColumn);
            this.olvExtractionInformations.AllowDrop = true;
            this.olvExtractionInformations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.olvExtractionInformations.CellEditUseWholeCell = false;
            this.olvExtractionInformations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvExtractionInformationsNameColumn});
            this.olvExtractionInformations.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvExtractionInformations.Location = new System.Drawing.Point(9, 370);
            this.olvExtractionInformations.Name = "olvExtractionInformations";
            this.olvExtractionInformations.Size = new System.Drawing.Size(476, 369);
            this.olvExtractionInformations.TabIndex = 1;
            this.olvExtractionInformations.UseCompatibleStateImageBehavior = false;
            this.olvExtractionInformations.View = System.Windows.Forms.View.Details;
            this.olvExtractionInformations.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            // 
            // olvExtractionInformationsNameColumn
            // 
            this.olvExtractionInformationsNameColumn.AspectName = "ToString";
            this.olvExtractionInformationsNameColumn.FillsFreeSpace = true;
            this.olvExtractionInformationsNameColumn.Groupable = false;
            this.olvExtractionInformationsNameColumn.Text = "ExtractionInformations";
            // 
            // tbCollation
            // 
            this.tbCollation.Location = new System.Drawing.Point(661, 595);
            this.tbCollation.Name = "tbCollation";
            this.tbCollation.Size = new System.Drawing.Size(229, 20);
            this.tbCollation.TabIndex = 18;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(605, 599);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 13);
            this.label13.TabIndex = 17;
            this.label13.Text = "Collation:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(605, 437);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(146, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Drag in Description Column(s)";
            // 
            // olvSelectedDescriptionColumns
            // 
            this.olvSelectedDescriptionColumns.AllColumns.Add(this.olvDescriptionsColumn);
            this.olvSelectedDescriptionColumns.CellEditUseWholeCell = false;
            this.olvSelectedDescriptionColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvDescriptionsColumn});
            this.olvSelectedDescriptionColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvSelectedDescriptionColumns.IsSimpleDropSink = true;
            this.olvSelectedDescriptionColumns.Location = new System.Drawing.Point(603, 456);
            this.olvSelectedDescriptionColumns.Name = "olvSelectedDescriptionColumns";
            this.olvSelectedDescriptionColumns.Size = new System.Drawing.Size(406, 134);
            this.olvSelectedDescriptionColumns.TabIndex = 14;
            this.olvSelectedDescriptionColumns.UseCompatibleStateImageBehavior = false;
            this.olvSelectedDescriptionColumns.View = System.Windows.Forms.View.Details;
            this.olvSelectedDescriptionColumns.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvSelectedDescriptionColumns_ModelCanDrop);
            this.olvSelectedDescriptionColumns.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvSelectedDescriptionColumns_ModelDropped);
            this.olvSelectedDescriptionColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.olvSelectedDescriptionColumns_KeyUp);
            // 
            // olvDescriptionsColumn
            // 
            this.olvDescriptionsColumn.AspectName = "ToString";
            this.olvDescriptionsColumn.FillsFreeSpace = true;
            this.olvDescriptionsColumn.Groupable = false;
            this.olvDescriptionsColumn.Text = "Description Fields";
            // 
            // olvLookupColumns
            // 
            this.olvLookupColumns.AllColumns.Add(this.olvLookupNameColumn);
            this.olvLookupColumns.AllowDrop = true;
            this.olvLookupColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvLookupColumns.CellEditUseWholeCell = false;
            this.olvLookupColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvLookupNameColumn});
            this.olvLookupColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvLookupColumns.Location = new System.Drawing.Point(6, 67);
            this.olvLookupColumns.Name = "olvLookupColumns";
            this.olvLookupColumns.Size = new System.Drawing.Size(461, 148);
            this.olvLookupColumns.TabIndex = 12;
            this.olvLookupColumns.UseCompatibleStateImageBehavior = false;
            this.olvLookupColumns.View = System.Windows.Forms.View.Details;
            this.olvLookupColumns.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.olvLookupColumns_CellRightClick);
            this.olvLookupColumns.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            // 
            // olvLookupNameColumn
            // 
            this.olvLookupNameColumn.AspectName = "ToString";
            this.olvLookupNameColumn.FillsFreeSpace = true;
            this.olvLookupNameColumn.Groupable = false;
            this.olvLookupNameColumn.Text = "ColumnInfos";
            // 
            // btnImportNewTableInfo
            // 
            this.btnImportNewTableInfo.Location = new System.Drawing.Point(441, 24);
            this.btnImportNewTableInfo.Name = "btnImportNewTableInfo";
            this.btnImportNewTableInfo.Size = new System.Drawing.Size(26, 26);
            this.btnImportNewTableInfo.TabIndex = 148;
            this.btnImportNewTableInfo.UseVisualStyleBackColor = true;
            this.btnImportNewTableInfo.Click += new System.EventHandler(this.btnImportNewTableInfo_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxLookup);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.olvLookupColumns);
            this.groupBox1.Controls.Add(this.btnImportNewTableInfo);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 234);
            this.groupBox1.TabIndex = 149;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "1.Choose Lookup Table (contains the codes and descriptions e.g. T = Tayside, F = " +
    "Fife";
            // 
            // cbxLookup
            // 
            this.cbxLookup.Location = new System.Drawing.Point(9, 26);
            this.cbxLookup.Name = "cbxLookup";
            this.cbxLookup.SelectedItem = null;
            this.cbxLookup.Size = new System.Drawing.Size(426, 24);
            this.cbxLookup.TabIndex = 152;
            this.cbxLookup.SelectedItemChanged += new System.EventHandler<System.EventArgs>(this.cbxLookup_SelectedItemChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(224, 218);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 151;
            this.label3.Text = "(Columns)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(435, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 151;
            this.label6.Text = "Import";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(121, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 151;
            this.label2.Text = "(Lookup Table)";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 325);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(25, 25);
            this.pictureBox1.TabIndex = 150;
            this.pictureBox1.TabStop = false;
            // 
            // tbCatalogue
            // 
            this.tbCatalogue.Location = new System.Drawing.Point(43, 331);
            this.tbCatalogue.Name = "tbCatalogue";
            this.tbCatalogue.ReadOnly = true;
            this.tbCatalogue.Size = new System.Drawing.Size(442, 20);
            this.tbCatalogue.TabIndex = 151;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(807, 295);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 152;
            this.label4.Text = "=";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(807, 334);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 152;
            this.label7.Text = "=";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(807, 373);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(13, 13);
            this.label10.TabIndex = 152;
            this.label10.Text = "=";
            // 
            // btnPrimaryKeyCompositeHelp
            // 
            this.btnPrimaryKeyCompositeHelp.Location = new System.Drawing.Point(1058, 328);
            this.btnPrimaryKeyCompositeHelp.Name = "btnPrimaryKeyCompositeHelp";
            this.btnPrimaryKeyCompositeHelp.Size = new System.Drawing.Size(26, 26);
            this.btnPrimaryKeyCompositeHelp.TabIndex = 153;
            this.btnPrimaryKeyCompositeHelp.UseVisualStyleBackColor = true;
            this.btnPrimaryKeyCompositeHelp.Click += new System.EventHandler(this.btnPrimaryKeyCompositeHelp_Click);
            // 
            // btnCreateLookup
            // 
            this.btnCreateLookup.Enabled = false;
            this.btnCreateLookup.Location = new System.Drawing.Point(603, 621);
            this.btnCreateLookup.Name = "btnCreateLookup";
            this.btnCreateLookup.Size = new System.Drawing.Size(109, 23);
            this.btnCreateLookup.TabIndex = 162;
            this.btnCreateLookup.Text = "Create Lookup";
            this.btnCreateLookup.UseVisualStyleBackColor = true;
            this.btnCreateLookup.Click += new System.EventHandler(this.btnCreateLookup_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(572, 619);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 163;
            // 
            // fk3
            // 
            this.fk3.IsValidGetter = null;
            this.fk3.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk3.Location = new System.Drawing.Point(826, 369);
            this.fk3.Name = "fk3";
            this.fk3.Size = new System.Drawing.Size(226, 35);
            this.fk3.TabIndex = 161;
            // 
            // fk2
            // 
            this.fk2.IsValidGetter = null;
            this.fk2.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk2.Location = new System.Drawing.Point(826, 328);
            this.fk2.Name = "fk2";
            this.fk2.Size = new System.Drawing.Size(226, 35);
            this.fk2.TabIndex = 160;
            // 
            // fk1
            // 
            this.fk1.IsValidGetter = null;
            this.fk1.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk1.Location = new System.Drawing.Point(826, 285);
            this.fk1.Name = "fk1";
            this.fk1.Size = new System.Drawing.Size(226, 35);
            this.fk1.TabIndex = 159;
            // 
            // pk2
            // 
            this.pk2.IsValidGetter = null;
            this.pk2.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk2.Location = new System.Drawing.Point(575, 328);
            this.pk2.Name = "pk2";
            this.pk2.Size = new System.Drawing.Size(226, 35);
            this.pk2.TabIndex = 158;
            // 
            // pk3
            // 
            this.pk3.IsValidGetter = null;
            this.pk3.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk3.Location = new System.Drawing.Point(575, 369);
            this.pk3.Name = "pk3";
            this.pk3.Size = new System.Drawing.Size(226, 35);
            this.pk3.TabIndex = 157;
            // 
            // pk1
            // 
            this.pk1.IsValidGetter = null;
            this.pk1.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk1.Location = new System.Drawing.Point(575, 285);
            this.pk1.Name = "pk1";
            this.pk1.Size = new System.Drawing.Size(226, 35);
            this.pk1.TabIndex = 156;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilter.Location = new System.Drawing.Point(43, 745);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(442, 20);
            this.tbFilter.TabIndex = 164;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 748);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 165;
            this.label5.Text = "Filter:";
            // 
            // LookupConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnCreateLookup);
            this.Controls.Add(this.fk3);
            this.Controls.Add(this.fk2);
            this.Controls.Add(this.fk1);
            this.Controls.Add(this.pk2);
            this.Controls.Add(this.pk3);
            this.Controls.Add(this.pk1);
            this.Controls.Add(this.btnPrimaryKeyCompositeHelp);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbCatalogue);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tbCollation);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.olvSelectedDescriptionColumns);
            this.Controls.Add(this.olvExtractionInformations);
            this.Controls.Add(this.label1);
            this.Name = "LookupConfiguration";
            this.Size = new System.Drawing.Size(1103, 772);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LookupConfiguration_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectedDescriptionColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvLookupColumns)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ObjectListView olvExtractionInformations;
        private ObjectListView olvLookupColumns;
        private ObjectListView olvSelectedDescriptionColumns;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbCollation;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnImportNewTableInfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private OLVColumn olvLookupNameColumn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox tbCatalogue;
        private OLVColumn olvExtractionInformationsNameColumn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnPrimaryKeyCompositeHelp;
        private KeyDropLocationUI pk1;
        private KeyDropLocationUI pk3;
        private KeyDropLocationUI pk2;
        private KeyDropLocationUI fk1;
        private KeyDropLocationUI fk2;
        private KeyDropLocationUI fk3;
        private OLVColumn olvDescriptionsColumn;
        private System.Windows.Forms.Button btnCreateLookup;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private MapsDirectlyToDatabaseTableUI.SelectIMapsDirectlyToDatabaseTableComboBox cbxLookup;
    }
}