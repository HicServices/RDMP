using BrightIdeasSoftware;

namespace CatalogueManager.ANOEngineeringUIs
{
    partial class ForwardEngineerANOCatalogueUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForwardEngineerANOCatalogueUI));
            this.gbTables = new System.Windows.Forms.GroupBox();
            this.ddDateColumn = new System.Windows.Forms.ComboBox();
            this.cbDateBasedLoad = new System.Windows.Forms.CheckBox();
            this.tbMandatory = new System.Windows.Forms.Label();
            this.tlvTableInfoMigrations = new BrightIdeasSoftware.TreeListView();
            this.olvTableInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvMigrationPlan = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPickedANOTable = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDilution = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDestinationType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gbANOTransforms = new System.Windows.Forms.GroupBox();
            this.tlvANOTables = new BrightIdeasSoftware.TreeListView();
            this.olvANOTablesName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSuffix = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfDigits = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfCharacters = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnRefreshChecks = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.gbTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfoMigrations)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.gbANOTransforms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvANOTables)).BeginInit();
            this.SuspendLayout();
            // 
            // gbTables
            // 
            this.gbTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTables.Controls.Add(this.ddDateColumn);
            this.gbTables.Controls.Add(this.cbDateBasedLoad);
            this.gbTables.Controls.Add(this.tbMandatory);
            this.gbTables.Controls.Add(this.tlvTableInfoMigrations);
            this.gbTables.Controls.Add(this.label1);
            this.gbTables.Location = new System.Drawing.Point(3, 464);
            this.gbTables.Name = "gbTables";
            this.gbTables.Size = new System.Drawing.Size(942, 315);
            this.gbTables.TabIndex = 0;
            this.gbTables.TabStop = false;
            this.gbTables.Text = "Table Migration";
            // 
            // ddDateColumn
            // 
            this.ddDateColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddDateColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDateColumn.Enabled = false;
            this.ddDateColumn.FormattingEnabled = true;
            this.ddDateColumn.Location = new System.Drawing.Point(244, 289);
            this.ddDateColumn.Name = "ddDateColumn";
            this.ddDateColumn.Size = new System.Drawing.Size(494, 21);
            this.ddDateColumn.TabIndex = 6;
            this.ddDateColumn.SelectedIndexChanged += new System.EventHandler(this.ddDateColumn_SelectedIndexChanged);
            // 
            // cbDateBasedLoad
            // 
            this.cbDateBasedLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDateBasedLoad.AutoSize = true;
            this.cbDateBasedLoad.Location = new System.Drawing.Point(90, 292);
            this.cbDateBasedLoad.Name = "cbDateBasedLoad";
            this.cbDateBasedLoad.Size = new System.Drawing.Size(159, 17);
            this.cbDateBasedLoad.TabIndex = 5;
            this.cbDateBasedLoad.Text = "Load Data In Date Batches:";
            this.cbDateBasedLoad.UseVisualStyleBackColor = true;
            this.cbDateBasedLoad.CheckedChanged += new System.EventHandler(this.cbDateBasedLoad_CheckedChanged);
            // 
            // tbMandatory
            // 
            this.tbMandatory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbMandatory.BackColor = System.Drawing.Color.LightCyan;
            this.tbMandatory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbMandatory.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbMandatory.Location = new System.Drawing.Point(7, 289);
            this.tbMandatory.Name = "tbMandatory";
            this.tbMandatory.Size = new System.Drawing.Size(20, 20);
            this.tbMandatory.TabIndex = 4;
            // 
            // tlvTableInfoMigrations
            // 
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvTableInfoName);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvMigrationPlan);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvPickedANOTable);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvDilution);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvDestinationType);
            this.tlvTableInfoMigrations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvTableInfoMigrations.CellEditUseWholeCell = false;
            this.tlvTableInfoMigrations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvTableInfoName,
            this.olvMigrationPlan,
            this.olvPickedANOTable,
            this.olvDilution,
            this.olvDestinationType});
            this.tlvTableInfoMigrations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvTableInfoMigrations.Location = new System.Drawing.Point(3, 19);
            this.tlvTableInfoMigrations.Name = "tlvTableInfoMigrations";
            this.tlvTableInfoMigrations.ShowGroups = false;
            this.tlvTableInfoMigrations.Size = new System.Drawing.Size(936, 267);
            this.tlvTableInfoMigrations.SmallImageList = this.imageList1;
            this.tlvTableInfoMigrations.TabIndex = 0;
            this.tlvTableInfoMigrations.UseCellFormatEvents = true;
            this.tlvTableInfoMigrations.UseCompatibleStateImageBehavior = false;
            this.tlvTableInfoMigrations.View = System.Windows.Forms.View.Details;
            this.tlvTableInfoMigrations.VirtualMode = true;
            this.tlvTableInfoMigrations.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.tlvTableInfoMigrations_FormatCell);
            this.tlvTableInfoMigrations.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.tlvTableInfoMigrations_FormatRow);
            // 
            // olvTableInfoName
            // 
            this.olvTableInfoName.AspectName = "ToString";
            this.olvTableInfoName.Text = "Name";
            this.olvTableInfoName.Width = 400;
            // 
            // olvMigrationPlan
            // 
            this.olvMigrationPlan.CellEditUseWholeCell = true;
            this.olvMigrationPlan.Text = "Plan";
            this.olvMigrationPlan.Width = 140;
            // 
            // olvPickedANOTable
            // 
            this.olvPickedANOTable.Text = "ANOTable";
            this.olvPickedANOTable.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvPickedANOTable.Width = 120;
            // 
            // olvDilution
            // 
            this.olvDilution.Text = "Dilution";
            this.olvDilution.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvDilution.Width = 120;
            // 
            // olvDestinationType
            // 
            this.olvDestinationType.Text = "Destination Type";
            this.olvDestinationType.Width = 100;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ANOTable");
            this.imageList1.Images.SetKeyName(1, "PreLoadDiscardedColumn");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 293);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Mandatory";
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(6, 19);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(581, 146);
            this.serverDatabaseTableSelector1.TabIndex = 1;
            this.serverDatabaseTableSelector1.Table = "";
            this.serverDatabaseTableSelector1.Username = "";
            this.serverDatabaseTableSelector1.SelectionChanged += new System.Action(this.serverDatabaseTableSelector1_SelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.serverDatabaseTableSelector1);
            this.groupBox1.Location = new System.Drawing.Point(6, 253);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(592, 166);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Destination Server";
            // 
            // gbANOTransforms
            // 
            this.gbANOTransforms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbANOTransforms.Controls.Add(this.tlvANOTables);
            this.gbANOTransforms.Location = new System.Drawing.Point(3, 25);
            this.gbANOTransforms.Name = "gbANOTransforms";
            this.gbANOTransforms.Size = new System.Drawing.Size(942, 222);
            this.gbANOTransforms.TabIndex = 0;
            this.gbANOTransforms.TabStop = false;
            this.gbANOTransforms.Text = "ANO Concepts";
            // 
            // tlvANOTables
            // 
            this.tlvANOTables.AllColumns.Add(this.olvANOTablesName);
            this.tlvANOTables.AllColumns.Add(this.olvSuffix);
            this.tlvANOTables.AllColumns.Add(this.olvNumberOfDigits);
            this.tlvANOTables.AllColumns.Add(this.olvNumberOfCharacters);
            this.tlvANOTables.CellEditUseWholeCell = false;
            this.tlvANOTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvANOTablesName,
            this.olvSuffix,
            this.olvNumberOfDigits,
            this.olvNumberOfCharacters});
            this.tlvANOTables.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvANOTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvANOTables.Location = new System.Drawing.Point(3, 16);
            this.tlvANOTables.Name = "tlvANOTables";
            this.tlvANOTables.ShowGroups = false;
            this.tlvANOTables.Size = new System.Drawing.Size(936, 203);
            this.tlvANOTables.TabIndex = 0;
            this.tlvANOTables.Text = "label1";
            this.tlvANOTables.UseCompatibleStateImageBehavior = false;
            this.tlvANOTables.View = System.Windows.Forms.View.Details;
            this.tlvANOTables.VirtualMode = true;
            // 
            // olvANOTablesName
            // 
            this.olvANOTablesName.AspectName = "ToString";
            this.olvANOTablesName.Text = "Name";
            this.olvANOTablesName.Width = 230;
            // 
            // olvSuffix
            // 
            this.olvSuffix.AspectName = "Suffix";
            this.olvSuffix.Text = "Suffix";
            // 
            // olvNumberOfDigits
            // 
            this.olvNumberOfDigits.AspectName = "NumberOfIntegersToUseInAnonymousRepresentation";
            this.olvNumberOfDigits.Text = "Number of digits";
            this.olvNumberOfDigits.Width = 100;
            // 
            // olvNumberOfCharacters
            // 
            this.olvNumberOfCharacters.AspectName = "NumberOfCharactersToUseInAnonymousRepresentation";
            this.olvNumberOfCharacters.Text = "Number of characters";
            this.olvNumberOfCharacters.Width = 120;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(255, 423);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 4;
            // 
            // btnRefreshChecks
            // 
            this.btnRefreshChecks.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshChecks.Image")));
            this.btnRefreshChecks.Location = new System.Drawing.Point(286, 424);
            this.btnRefreshChecks.Name = "btnRefreshChecks";
            this.btnRefreshChecks.Size = new System.Drawing.Size(22, 24);
            this.btnRefreshChecks.TabIndex = 6;
            this.btnRefreshChecks.UseVisualStyleBackColor = true;
            this.btnRefreshChecks.Click += new System.EventHandler(this.btnRefreshChecks_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(330, 425);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(86, 23);
            this.btnExecute.TabIndex = 68;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(349, 452);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "Execute";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(264, 452);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 69;
            this.label3.Text = "Check";
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(0, 0);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(948, 22);
            this.rdmpObjectsRibbonUI1.TabIndex = 3;
            // 
            // ForwardEngineerANOCatalogueUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnRefreshChecks);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbTables);
            this.Controls.Add(this.gbANOTransforms);
            this.Name = "ForwardEngineerANOCatalogueUI";
            this.Size = new System.Drawing.Size(948, 782);
            this.gbTables.ResumeLayout(false);
            this.gbTables.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfoMigrations)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbANOTransforms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvANOTables)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTables;
        private TreeListView tlvTableInfoMigrations;
        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbANOTransforms;
        private TreeListView tlvANOTables;
        private OLVColumn olvTableInfoName;
        private OLVColumn olvANOTablesName;
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label tbMandatory;
        private OLVColumn olvMigrationPlan;
        private OLVColumn olvPickedANOTable;
        private System.Windows.Forms.ImageList imageList1;
        private OLVColumn olvDilution;
        private OLVColumn olvDestinationType;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnRefreshChecks;
        private OLVColumn olvNumberOfDigits;
        private OLVColumn olvNumberOfCharacters;
        private OLVColumn olvSuffix;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbDateBasedLoad;
        private System.Windows.Forms.ComboBox ddDateColumn;
    }
}
