using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace DataExportManager.ProjectUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExcludeAll = new System.Windows.Forms.Button();
            this.btnExclude = new System.Windows.Forms.Button();
            this.btnInclude = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.olvAvailable = new BrightIdeasSoftware.ObjectListView();
            this.olvAvailableColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvAvailableColumnCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnSelectCore = new System.Windows.Forms.Button();
            this.olvSelected = new BrightIdeasSoftware.ObjectListView();
            this.olvSelectedColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSelectedColumnOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvAvailable)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.olvAvailable);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectCore);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnInclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExcludeAll);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.olvSelected);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(755, 738);
            this.splitContainer1.SplitterDistance = 421;
            this.splitContainer1.TabIndex = 52;
            // 
            // olvAvailable
            // 
            this.olvAvailable.AllColumns.Add(this.olvAvailableColumnName);
            this.olvAvailable.AllColumns.Add(this.olvAvailableColumnCategory);
            this.olvAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvAvailable.CellEditUseWholeCell = false;
            this.olvAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvAvailableColumnName});
            this.olvAvailable.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvAvailable.FullRowSelect = true;
            this.olvAvailable.HideSelection = false;
            this.olvAvailable.IsSimpleDragSource = true;
            this.olvAvailable.Location = new System.Drawing.Point(6, 16);
            this.olvAvailable.Name = "olvAvailable";
            this.olvAvailable.RowHeight = 19;
            this.olvAvailable.Size = new System.Drawing.Size(325, 715);
            this.olvAvailable.TabIndex = 154;
            this.olvAvailable.Text = "label14";
            this.olvAvailable.UseCompatibleStateImageBehavior = false;
            this.olvAvailable.View = System.Windows.Forms.View.Details;
            this.olvAvailable.ItemActivate += new System.EventHandler(this.olvAvailable_ItemActivate);
            // 
            // olvAvailableColumnName
            // 
            this.olvAvailableColumnName.AspectName = "ToString";
            this.olvAvailableColumnName.FillsFreeSpace = true;
            this.olvAvailableColumnName.Text = "Name";
            this.olvAvailableColumnName.Width = 200;
            // 
            // olvAvailableColumnCategory
            // 
            this.olvAvailableColumnCategory.DisplayIndex = 1;
            this.olvAvailableColumnCategory.IsVisible = false;
            this.olvAvailableColumnCategory.Text = "Category";
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
            // olvSelected
            // 
            this.olvSelected.AllColumns.Add(this.olvSelectedColumnName);
            this.olvSelected.AllColumns.Add(this.olvSelectedColumnOrder);
            this.olvSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvSelected.CellEditUseWholeCell = false;
            this.olvSelected.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvSelectedColumnName,
            this.olvSelectedColumnOrder});
            this.olvSelected.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvSelected.FullRowSelect = true;
            this.olvSelected.HideSelection = false;
            this.olvSelected.IsSimpleDragSource = true;
            this.olvSelected.IsSimpleDropSink = true;
            this.olvSelected.Location = new System.Drawing.Point(6, 16);
            this.olvSelected.Name = "olvSelected";
            this.olvSelected.RowHeight = 19;
            this.olvSelected.ShowGroups = false;
            this.olvSelected.Size = new System.Drawing.Size(317, 715);
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
            this.olvSelectedColumnName.FillsFreeSpace = true;
            this.olvSelectedColumnName.Sortable = false;
            this.olvSelectedColumnName.Text = "Name";
            // 
            // olvSelectedColumnOrder
            // 
            this.olvSelectedColumnOrder.AspectName = "Order";
            this.olvSelectedColumnOrder.Text = "Order";
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
            ((System.ComponentModel.ISupportInitialize)(this.olvSelected)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

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
    }
}
