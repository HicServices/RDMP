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
            this.tlvTableInfoMigrations = new BrightIdeasSoftware.TreeListView();
            this.olvTableInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDestinationExtractionCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvMigrationPlan = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPickedANOTable = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDilution = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDestinationType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbStartDate = new System.Windows.Forms.TextBox();
            this.lblMandatory = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ddDateColumn = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbDateBasedLoad = new System.Windows.Forms.CheckBox();
            this.lblPlanIsSuggestion = new System.Windows.Forms.Label();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLoadPlan = new System.Windows.Forms.Button();
            this.btnSavePlan = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnRefreshChecks = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.gbANOTransforms = new System.Windows.Forms.GroupBox();
            this.tlvANOTables = new BrightIdeasSoftware.TreeListView();
            this.olvANOTablesName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSuffix = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfDigits = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfCharacters = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.gbTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfoMigrations)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbANOTransforms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvANOTables)).BeginInit();
            this.SuspendLayout();
            // 
            // gbTables
            // 
            this.gbTables.Controls.Add(this.tlvTableInfoMigrations);
            this.gbTables.Controls.Add(this.panel2);
            this.gbTables.Controls.Add(this.panel1);
            this.gbTables.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbTables.Location = new System.Drawing.Point(0, 388);
            this.gbTables.Name = "gbTables";
            this.gbTables.Size = new System.Drawing.Size(909, 364);
            this.gbTables.TabIndex = 6;
            this.gbTables.TabStop = false;
            this.gbTables.Text = "Table Migration";
            // 
            // tlvTableInfoMigrations
            // 
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvTableInfoName);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvDestinationExtractionCategory);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvMigrationPlan);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvPickedANOTable);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvDilution);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvDestinationType);
            this.tlvTableInfoMigrations.CellEditUseWholeCell = false;
            this.tlvTableInfoMigrations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvTableInfoName,
            this.olvDestinationExtractionCategory,
            this.olvMigrationPlan,
            this.olvPickedANOTable,
            this.olvDilution,
            this.olvDestinationType});
            this.tlvTableInfoMigrations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvTableInfoMigrations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvTableInfoMigrations.Location = new System.Drawing.Point(3, 16);
            this.tlvTableInfoMigrations.Name = "tlvTableInfoMigrations";
            this.tlvTableInfoMigrations.ShowGroups = false;
            this.tlvTableInfoMigrations.Size = new System.Drawing.Size(903, 286);
            this.tlvTableInfoMigrations.SmallImageList = this.imageList1;
            this.tlvTableInfoMigrations.TabIndex = 0;
            this.tlvTableInfoMigrations.UseCellFormatEvents = true;
            this.tlvTableInfoMigrations.UseCompatibleStateImageBehavior = false;
            this.tlvTableInfoMigrations.View = System.Windows.Forms.View.Details;
            this.tlvTableInfoMigrations.VirtualMode = true;
            this.tlvTableInfoMigrations.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.tlvTableInfoMigrations_FormatCell);
            // 
            // olvTableInfoName
            // 
            this.olvTableInfoName.AspectName = "ToString";
            this.olvTableInfoName.Text = "Name";
            this.olvTableInfoName.Width = 327;
            // 
            // olvDestinationExtractionCategory
            // 
            this.olvDestinationExtractionCategory.Text = "Category";
            this.olvDestinationExtractionCategory.Width = 102;
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
            // panel2
            // 
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.tbFilter);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 302);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(903, 27);
            this.panel2.TabIndex = 2;
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
            this.tbFilter.Location = new System.Drawing.Point(43, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(351, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbStartDate);
            this.panel1.Controls.Add(this.lblMandatory);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ddDateColumn);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.cbDateBasedLoad);
            this.panel1.Controls.Add(this.lblPlanIsSuggestion);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 329);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(903, 32);
            this.panel1.TabIndex = 1;
            // 
            // tbStartDate
            // 
            this.tbStartDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStartDate.Enabled = false;
            this.tbStartDate.Location = new System.Drawing.Point(770, 7);
            this.tbStartDate.Name = "tbStartDate";
            this.tbStartDate.Size = new System.Drawing.Size(130, 20);
            this.tbStartDate.TabIndex = 6;
            this.tbStartDate.Text = "2001-01-01";
            this.tbStartDate.TextChanged += new System.EventHandler(this.tbStartDate_TextChanged);
            // 
            // lblMandatory
            // 
            this.lblMandatory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMandatory.BackColor = System.Drawing.Color.Turquoise;
            this.lblMandatory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMandatory.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMandatory.Location = new System.Drawing.Point(3, 6);
            this.lblMandatory.Name = "lblMandatory";
            this.lblMandatory.Size = new System.Drawing.Size(20, 20);
            this.lblMandatory.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(708, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Starting At";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mandatory";
            // 
            // ddDateColumn
            // 
            this.ddDateColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDateColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDateColumn.Enabled = false;
            this.ddDateColumn.FormattingEnabled = true;
            this.ddDateColumn.Location = new System.Drawing.Point(355, 6);
            this.ddDateColumn.Name = "ddDateColumn";
            this.ddDateColumn.Size = new System.Drawing.Size(342, 21);
            this.ddDateColumn.TabIndex = 4;
            this.ddDateColumn.SelectedIndexChanged += new System.EventHandler(this.ddDateColumn_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Plan Is Suggestion";
            // 
            // cbDateBasedLoad
            // 
            this.cbDateBasedLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDateBasedLoad.AutoSize = true;
            this.cbDateBasedLoad.Location = new System.Drawing.Point(201, 9);
            this.cbDateBasedLoad.Name = "cbDateBasedLoad";
            this.cbDateBasedLoad.Size = new System.Drawing.Size(159, 17);
            this.cbDateBasedLoad.TabIndex = 3;
            this.cbDateBasedLoad.Text = "Load Data In Date Batches:";
            this.cbDateBasedLoad.UseVisualStyleBackColor = true;
            this.cbDateBasedLoad.CheckedChanged += new System.EventHandler(this.cbDateBasedLoad_CheckedChanged);
            // 
            // lblPlanIsSuggestion
            // 
            this.lblPlanIsSuggestion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPlanIsSuggestion.BackColor = System.Drawing.Color.LightCyan;
            this.lblPlanIsSuggestion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPlanIsSuggestion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblPlanIsSuggestion.Location = new System.Drawing.Point(83, 6);
            this.lblPlanIsSuggestion.Name = "lblPlanIsSuggestion";
            this.lblPlanIsSuggestion.Size = new System.Drawing.Size(20, 20);
            this.lblPlanIsSuggestion.TabIndex = 1;
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(6, 19);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(623, 146);
            this.serverDatabaseTableSelector1.TabIndex = 0;
            this.serverDatabaseTableSelector1.Username = "";
            this.serverDatabaseTableSelector1.SelectionChanged += new System.Action(this.serverDatabaseTableSelector1_SelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnLoadPlan);
            this.groupBox1.Controls.Add(this.btnSavePlan);
            this.groupBox1.Controls.Add(this.serverDatabaseTableSelector1);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 222);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(909, 166);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Destination Server";
            // 
            // btnLoadPlan
            // 
            this.btnLoadPlan.Location = new System.Drawing.Point(623, 132);
            this.btnLoadPlan.Name = "btnLoadPlan";
            this.btnLoadPlan.Size = new System.Drawing.Size(27, 27);
            this.btnLoadPlan.TabIndex = 71;
            this.btnLoadPlan.UseVisualStyleBackColor = true;
            this.btnLoadPlan.Click += new System.EventHandler(this.btnLoadPlan_Click);
            // 
            // btnSavePlan
            // 
            this.btnSavePlan.Location = new System.Drawing.Point(593, 132);
            this.btnSavePlan.Name = "btnSavePlan";
            this.btnSavePlan.Size = new System.Drawing.Size(27, 27);
            this.btnSavePlan.TabIndex = 71;
            this.btnSavePlan.UseVisualStyleBackColor = true;
            this.btnSavePlan.Click += new System.EventHandler(this.btnSavePlan_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnExecute);
            this.groupBox2.Controls.Add(this.ragSmiley1);
            this.groupBox2.Controls.Add(this.btnRefreshChecks);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(623, 61);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(280, 65);
            this.groupBox2.TabIndex = 70;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Execute ANO Configuration";
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(169, 19);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(86, 23);
            this.btnExecute.TabIndex = 5;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(94, 17);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 3;
            // 
            // btnRefreshChecks
            // 
            this.btnRefreshChecks.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshChecks.Image")));
            this.btnRefreshChecks.Location = new System.Drawing.Point(125, 18);
            this.btnRefreshChecks.Name = "btnRefreshChecks";
            this.btnRefreshChecks.Size = new System.Drawing.Size(22, 24);
            this.btnRefreshChecks.TabIndex = 4;
            this.btnRefreshChecks.UseVisualStyleBackColor = true;
            this.btnRefreshChecks.Click += new System.EventHandler(this.btnRefreshChecks_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(188, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "Execute";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 69;
            this.label3.Text = "Check";
            // 
            // gbANOTransforms
            // 
            this.gbANOTransforms.Controls.Add(this.tlvANOTables);
            this.gbANOTransforms.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbANOTransforms.Location = new System.Drawing.Point(0, 0);
            this.gbANOTransforms.Name = "gbANOTransforms";
            this.gbANOTransforms.Size = new System.Drawing.Size(909, 222);
            this.gbANOTransforms.TabIndex = 1;
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
            this.tlvANOTables.Size = new System.Drawing.Size(903, 203);
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
            this.olvSuffix.AspectName = "";
            this.olvSuffix.Text = "Suffix";
            // 
            // olvNumberOfDigits
            // 
            this.olvNumberOfDigits.AspectName = "";
            this.olvNumberOfDigits.Text = "Number of digits";
            this.olvNumberOfDigits.Width = 100;
            // 
            // olvNumberOfCharacters
            // 
            this.olvNumberOfCharacters.AspectName = "";
            this.olvNumberOfCharacters.Text = "Number of characters";
            this.olvNumberOfCharacters.Width = 120;
            // 
            // ForwardEngineerANOCatalogueUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.gbTables);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbANOTransforms);
            this.Name = "ForwardEngineerANOCatalogueUI";
            this.Size = new System.Drawing.Size(909, 758);
            this.gbTables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfoMigrations)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbANOTransforms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvANOTables)).EndInit();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblMandatory;
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.Label lblPlanIsSuggestion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilter;
        private OLVColumn olvDestinationExtractionCategory;
        private System.Windows.Forms.Button btnLoadPlan;
        private System.Windows.Forms.Button btnSavePlan;
    }
}
