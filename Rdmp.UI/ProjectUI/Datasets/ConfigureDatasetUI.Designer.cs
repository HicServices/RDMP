using BrightIdeasSoftware;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.ProjectUI.Datasets
{
    partial class ConfigureDatasetUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureDatasetUI));
            this.label1 = new System.Windows.Forms.Label();
            this.cbShowProjectSpecific = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExcludeAll = new System.Windows.Forms.Button();
            this.btnExclude = new System.Windows.Forms.Button();
            this.btnInclude = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbSearchAvailable = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.olvAvailable = new BrightIdeasSoftware.ObjectListView();
            this.olvAvailableColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvAvailableColumnCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvAvailableIsExtractionIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnSelectCore = new System.Windows.Forms.Button();
            this.flpCouldNotJoinTables = new System.Windows.Forms.FlowLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.tbSearchTables = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.olvJoin = new BrightIdeasSoftware.ObjectListView();
            this.olvJoinTableName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvJoinColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tbSearchSelected = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.olvSelected = new BrightIdeasSoftware.ObjectListView();
            this.olvSelectedColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIssues = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSelectedColumnOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSelectedCatalogue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSelectedCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.helpIconJoin = new Rdmp.UI.SimpleControls.HelpIcon();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvAvailable)).BeginInit();
            this.flpCouldNotJoinTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvSelected)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Available Columns:";
            // 
            // cbShowProjectSpecific
            // 
            this.cbShowProjectSpecific.Location = new System.Drawing.Point(337, 240);
            this.cbShowProjectSpecific.Name = "cbShowProjectSpecific";
            this.cbShowProjectSpecific.Size = new System.Drawing.Size(96, 30);
            this.cbShowProjectSpecific.TabIndex = 38;
            this.cbShowProjectSpecific.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.cbShowProjectSpecific.Text = "Show Project Specific";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Selected Columns:";
            // 
            // btnExcludeAll
            // 
            this.btnExcludeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcludeAll.Location = new System.Drawing.Point(337, 95);
            this.btnExcludeAll.Name = "btnExcludeAll";
            this.btnExcludeAll.Size = new System.Drawing.Size(75, 23);
            this.btnExcludeAll.TabIndex = 44;
            this.btnExcludeAll.Text = "<<";
            this.btnExcludeAll.UseVisualStyleBackColor = true;
            this.btnExcludeAll.Click += new System.EventHandler(this.btnExcludeAll_Click);
            // 
            // btnExclude
            // 
            this.btnExclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExclude.Location = new System.Drawing.Point(337, 66);
            this.btnExclude.Name = "btnExclude";
            this.btnExclude.Size = new System.Drawing.Size(75, 23);
            this.btnExclude.TabIndex = 42;
            this.btnExclude.Text = "<";
            this.btnExclude.UseVisualStyleBackColor = true;
            this.btnExclude.Click += new System.EventHandler(this.btnExclude_Click);
            // 
            // btnInclude
            // 
            this.btnInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInclude.Location = new System.Drawing.Point(337, 37);
            this.btnInclude.Name = "btnInclude";
            this.btnInclude.Size = new System.Drawing.Size(75, 23);
            this.btnInclude.TabIndex = 41;
            this.btnInclude.Text = ">";
            this.btnInclude.UseVisualStyleBackColor = true;
            this.btnInclude.Click += new System.EventHandler(this.btnInclude_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tbSearchAvailable);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.olvAvailable);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectCore);
            this.splitContainer1.Panel1.Controls.Add(this.cbShowProjectSpecific);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnInclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExcludeAll);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flpCouldNotJoinTables);
            this.splitContainer1.Panel2.Controls.Add(this.tbSearchTables);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.olvJoin);
            this.splitContainer1.Panel2.Controls.Add(this.tbSearchSelected);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.olvSelected);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(755, 738);
            this.splitContainer1.SplitterDistance = 421;
            this.splitContainer1.TabIndex = 52;
            // 
            // tbSearchAvailable
            // 
            this.tbSearchAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchAvailable.Location = new System.Drawing.Point(56, 711);
            this.tbSearchAvailable.Name = "tbSearchAvailable";
            this.tbSearchAvailable.Size = new System.Drawing.Size(275, 20);
            this.tbSearchAvailable.TabIndex = 156;
            this.tbSearchAvailable.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 714);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 155;
            this.label3.Text = "Search:";
            // 
            // olvAvailable
            // 
            this.olvAvailable.AllColumns.Add(this.olvAvailableColumnName);
            this.olvAvailable.AllColumns.Add(this.olvAvailableColumnCategory);
            this.olvAvailable.AllColumns.Add(this.olvAvailableIsExtractionIdentifier);
            this.olvAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvAvailable.CellEditUseWholeCell = false;
            this.olvAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvAvailableColumnName,
            this.olvAvailableIsExtractionIdentifier});
            this.olvAvailable.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvAvailable.FullRowSelect = true;
            this.olvAvailable.HideSelection = false;
            this.olvAvailable.IsSimpleDragSource = true;
            this.olvAvailable.Location = new System.Drawing.Point(6, 16);
            this.olvAvailable.Name = "olvAvailable";
            this.olvAvailable.RowHeight = 19;
            this.olvAvailable.Size = new System.Drawing.Size(325, 689);
            this.olvAvailable.TabIndex = 154;
            this.olvAvailable.Text = "label14";
            this.olvAvailable.UseCompatibleStateImageBehavior = false;
            this.olvAvailable.View = System.Windows.Forms.View.Details;
            this.olvAvailable.ItemActivate += new System.EventHandler(this.olvAvailable_ItemActivate);
            // 
            // olvAvailableColumnName
            // 
            this.olvAvailableColumnName.AspectName = "ToString";
            this.olvAvailableColumnName.MinimumWidth = 100;
            this.olvAvailableColumnName.Text = "Name";
            this.olvAvailableColumnName.Width = 200;
            // 
            // olvAvailableColumnCategory
            // 
            this.olvAvailableColumnCategory.DisplayIndex = 1;
            this.olvAvailableColumnCategory.IsVisible = false;
            this.olvAvailableColumnCategory.Text = "Category";
            // 
            // olvSelectedIsExtractionIdentifier
            // 
            this.olvAvailableIsExtractionIdentifier.AspectName = "IsExtractionIdentifier";
            this.olvAvailableIsExtractionIdentifier.Text = "IsExtractionIdentifier";
            this.olvAvailableIsExtractionIdentifier.Width = 120;
            // 
            // btnSelectCore
            // 
            this.btnSelectCore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCore.Location = new System.Drawing.Point(337, 210);
            this.btnSelectCore.Name = "btnSelectCore";
            this.btnSelectCore.Size = new System.Drawing.Size(75, 26);
            this.btnSelectCore.TabIndex = 46;
            this.btnSelectCore.Text = "Select Core";
            this.btnSelectCore.UseVisualStyleBackColor = true;
            this.btnSelectCore.Click += new System.EventHandler(this.btnSelectCore_Click);
            // 
            // flpCouldNotJoinTables
            // 
            this.flpCouldNotJoinTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpCouldNotJoinTables.Controls.Add(this.label8);
            this.flpCouldNotJoinTables.Controls.Add(this.helpIconJoin);
            this.flpCouldNotJoinTables.Location = new System.Drawing.Point(8, 442);
            this.flpCouldNotJoinTables.Name = "flpCouldNotJoinTables";
            this.flpCouldNotJoinTables.Size = new System.Drawing.Size(260, 24);
            this.flpCouldNotJoinTables.TabIndex = 168;
            this.flpCouldNotJoinTables.Visible = false;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(3, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(204, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Missing join information for included tables";
            // 
            // tbSearchTables
            // 
            this.tbSearchTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchTables.Location = new System.Drawing.Point(53, 713);
            this.tbSearchTables.Name = "tbSearchTables";
            this.tbSearchTables.Size = new System.Drawing.Size(270, 20);
            this.tbSearchTables.TabIndex = 164;
            this.tbSearchTables.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 717);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 163;
            this.label6.Text = "Search:";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(5, 469);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 159;
            this.label5.Text = "FROM";
            // 
            // olvJoin
            // 
            this.olvJoin.AllColumns.Add(this.olvJoinTableName);
            this.olvJoin.AllColumns.Add(this.olvJoinColumn);
            this.olvJoin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvJoin.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olvJoin.CellEditUseWholeCell = false;
            this.olvJoin.CheckBoxes = true;
            this.olvJoin.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvJoinTableName,
            this.olvJoinColumn});
            this.olvJoin.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvJoin.FullRowSelect = true;
            this.olvJoin.HideSelection = false;
            this.olvJoin.IsSimpleDropSink = true;
            this.olvJoin.Location = new System.Drawing.Point(6, 490);
            this.olvJoin.Name = "olvJoin";
            this.olvJoin.RowHeight = 19;
            this.olvJoin.Size = new System.Drawing.Size(317, 219);
            this.olvJoin.TabIndex = 161;
            this.olvJoin.UseCompatibleStateImageBehavior = false;
            this.olvJoin.View = System.Windows.Forms.View.Details;
            // 
            // olvJoinTableName
            // 
            this.olvJoinTableName.AspectName = "ToString";
            this.olvJoinTableName.Groupable = false;
            this.olvJoinTableName.IsEditable = false;
            this.olvJoinTableName.MinimumWidth = 100;
            this.olvJoinTableName.Text = "Table Name";
            this.olvJoinTableName.Width = 163;
            // 
            // olvJoinColumn
            // 
            this.olvJoinColumn.AspectName = "";
            this.olvJoinColumn.ButtonSizing = BrightIdeasSoftware.OLVColumn.ButtonSizingMode.CellBounds;
            this.olvJoinColumn.Groupable = false;
            this.olvJoinColumn.IsButton = true;
            this.olvJoinColumn.Text = "Join";
            this.olvJoinColumn.Width = 100;
            // 
            // tbSearchSelected
            // 
            this.tbSearchSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchSelected.Location = new System.Drawing.Point(53, 417);
            this.tbSearchSelected.Name = "tbSearchSelected";
            this.tbSearchSelected.Size = new System.Drawing.Size(270, 20);
            this.tbSearchSelected.TabIndex = 158;
            this.tbSearchSelected.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 420);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 157;
            this.label4.Text = "Search:";
            // 
            // olvSelected
            // 
            this.olvSelected.AllColumns.Add(this.olvSelectedColumnName);
            this.olvSelected.AllColumns.Add(this.olvSelectedCategory);
            this.olvSelected.AllColumns.Add(this.olvIssues);
            this.olvSelected.AllColumns.Add(this.olvSelectedColumnOrder);
            this.olvSelected.AllColumns.Add(this.olvSelectedCatalogue);
            this.olvSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvSelected.CellEditUseWholeCell = false;
            this.olvSelected.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvSelectedColumnName,
            this.olvSelectedCategory,
            this.olvIssues,
            this.olvSelectedCatalogue});
            this.olvSelected.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvSelected.FullRowSelect = true;
            this.olvSelected.HideSelection = false;
            this.olvSelected.IsSimpleDragSource = true;
            this.olvSelected.IsSimpleDropSink = true;
            this.olvSelected.Location = new System.Drawing.Point(6, 16);
            this.olvSelected.Name = "olvSelected";
            this.olvSelected.RowHeight = 19;
            this.olvSelected.ShowGroups = false;
            this.olvSelected.Size = new System.Drawing.Size(317, 395);
            this.olvSelected.TabIndex = 154;
            this.olvSelected.Text = "label14";
            this.olvSelected.UseCompatibleStateImageBehavior = false;
            this.olvSelected.View = System.Windows.Forms.View.Details;
            this.olvSelected.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvSelected_ModelCanDrop);
            this.olvSelected.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvSelected_ModelDropped);
            // 
            // olvSelectedColumnName
            // 
            this.olvSelectedColumnName.AspectName = "ToString";
            this.olvSelectedColumnName.MinimumWidth = 100;
            this.olvSelectedColumnName.Sortable = false;
            this.olvSelectedColumnName.Text = "Name";
            this.olvSelectedColumnName.Width = 100;
            // 
            // olvIssues
            // 
            this.olvIssues.Sortable = false;
            this.olvIssues.Text = "Issues";
            this.olvIssues.Width = 100;
            // 
            // olvSelectedColumnOrder
            // 
            this.olvSelectedColumnOrder.AspectName = "Order";
            this.olvSelectedColumnOrder.DisplayIndex = 3;
            this.olvSelectedColumnOrder.IsVisible = false;
            this.olvSelectedColumnOrder.Text = "Order";
            // 
            // olvSelectedCatalogue
            // 
            this.olvSelectedCatalogue.Sortable = false;
            this.olvSelectedCatalogue.Text = "Catalogue";
            this.olvSelectedCatalogue.Width = 100;
            // 
            // olvSelectedCategory
            // 
            this.olvSelectedCategory.Sortable = false;
            this.olvSelectedCategory.Text = "Category";
            // 
            // helpIconJoin
            // 
            this.helpIconJoin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.helpIconJoin.BackColor = System.Drawing.Color.Transparent;
            this.helpIconJoin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconJoin.BackgroundImage")));
            this.helpIconJoin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIconJoin.Location = new System.Drawing.Point(213, 3);
            this.helpIconJoin.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIconJoin.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIconJoin.Name = "helpIconJoin";
            this.helpIconJoin.Size = new System.Drawing.Size(19, 19);
            this.helpIconJoin.SuppressClick = false;
            this.helpIconJoin.TabIndex = 1;
            // 
            // ConfigureDatasetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ConfigureDatasetUI";
            this.Size = new System.Drawing.Size(755, 738);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvAvailable)).EndInit();
            this.flpCouldNotJoinTables.ResumeLayout(false);
            this.flpCouldNotJoinTables.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvSelected)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox cbShowProjectSpecific;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnExcludeAll;
        private System.Windows.Forms.Button btnExclude;
        private System.Windows.Forms.Button btnInclude;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnSelectCore;
        private ObjectListView olvAvailable;
        private ObjectListView olvSelected;
        private OLVColumn olvAvailableColumnName;
        private OLVColumn olvAvailableColumnCategory;
        private OLVColumn olvSelectedColumnName;
        private OLVColumn olvSelectedColumnOrder;
        private System.Windows.Forms.TextBox tbSearchAvailable;
        private System.Windows.Forms.Label label3;
        private OLVColumn olvAvailableIsExtractionIdentifier;
        private System.Windows.Forms.TextBox tbSearchSelected;
        private System.Windows.Forms.Label label4;
        private OLVColumn olvSelectedCatalogue;
        private System.Windows.Forms.Label label5;
        private ObjectListView olvJoin;
        private OLVColumn olvJoinTableName;
        private OLVColumn olvJoinColumn;
        private System.Windows.Forms.TextBox tbSearchTables;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.FlowLayoutPanel flpCouldNotJoinTables;
        private System.Windows.Forms.Label label8;
        private HelpIcon helpIconJoin;
        private OLVColumn olvIssues;
        private OLVColumn olvSelectedCategory;
    }
}
