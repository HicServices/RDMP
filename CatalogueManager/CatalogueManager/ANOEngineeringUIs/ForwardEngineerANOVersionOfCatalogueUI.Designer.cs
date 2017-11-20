using BrightIdeasSoftware;

namespace CatalogueManager.ANOEngineeringUIs
{
    partial class ForwardEngineerANOVersionOfCatalogueUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForwardEngineerANOVersionOfCatalogueUI));
            this.gbTables = new System.Windows.Forms.GroupBox();
            this.tbMandatory = new System.Windows.Forms.Label();
            this.tlvTableInfoMigrations = new BrightIdeasSoftware.TreeListView();
            this.olvTableInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvMigrationPlan = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPickedANOTable = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gbANOTransforms = new System.Windows.Forms.GroupBox();
            this.tlvANOTables = new BrightIdeasSoftware.TreeListView();
            this.olvANOTablesName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
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
            this.gbTables.Controls.Add(this.tbMandatory);
            this.gbTables.Controls.Add(this.tlvTableInfoMigrations);
            this.gbTables.Controls.Add(this.label1);
            this.gbTables.Location = new System.Drawing.Point(3, 446);
            this.gbTables.Name = "gbTables";
            this.gbTables.Size = new System.Drawing.Size(942, 182);
            this.gbTables.TabIndex = 0;
            this.gbTables.TabStop = false;
            this.gbTables.Text = "Table Migration";
            // 
            // tbMandatory
            // 
            this.tbMandatory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbMandatory.BackColor = System.Drawing.Color.Azure;
            this.tbMandatory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbMandatory.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbMandatory.Location = new System.Drawing.Point(7, 156);
            this.tbMandatory.Name = "tbMandatory";
            this.tbMandatory.Size = new System.Drawing.Size(20, 20);
            this.tbMandatory.TabIndex = 4;
            // 
            // tlvTableInfoMigrations
            // 
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvTableInfoName);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvMigrationPlan);
            this.tlvTableInfoMigrations.AllColumns.Add(this.olvPickedANOTable);
            this.tlvTableInfoMigrations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvTableInfoMigrations.CellEditUseWholeCell = false;
            this.tlvTableInfoMigrations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvTableInfoName,
            this.olvMigrationPlan,
            this.olvPickedANOTable});
            this.tlvTableInfoMigrations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvTableInfoMigrations.Location = new System.Drawing.Point(3, 19);
            this.tlvTableInfoMigrations.Name = "tlvTableInfoMigrations";
            this.tlvTableInfoMigrations.ShowGroups = false;
            this.tlvTableInfoMigrations.Size = new System.Drawing.Size(936, 134);
            this.tlvTableInfoMigrations.SmallImageList = this.imageList1;
            this.tlvTableInfoMigrations.TabIndex = 0;
            this.tlvTableInfoMigrations.UseCompatibleStateImageBehavior = false;
            this.tlvTableInfoMigrations.View = System.Windows.Forms.View.Details;
            this.tlvTableInfoMigrations.VirtualMode = true;
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
            this.olvPickedANOTable.Width = 90;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ANOTable");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 160);
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
            this.tlvANOTables.CellEditUseWholeCell = false;
            this.tlvANOTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvANOTablesName});
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
            this.olvANOTablesName.FillsFreeSpace = true;
            this.olvANOTablesName.Text = "Name";
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
            // ForwardEngineerANOVersionOfCatalogueUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbTables);
            this.Controls.Add(this.gbANOTransforms);
            this.Name = "ForwardEngineerANOVersionOfCatalogueUI";
            this.Size = new System.Drawing.Size(948, 631);
            this.gbTables.ResumeLayout(false);
            this.gbTables.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfoMigrations)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label tbMandatory;
        private OLVColumn olvMigrationPlan;
        private OLVColumn olvPickedANOTable;
        private System.Windows.Forms.ImageList imageList1;
    }
}
