using BrightIdeasSoftware;

namespace Rdmp.UI.ExtractionUIs
{
    partial class ViewExtractionSqlUI
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
            this.scMainLeftRightSplit = new System.Windows.Forms.SplitContainer();
            this.scFiltersAndColumns = new System.Windows.Forms.SplitContainer();
            this.olvExtractionInformations = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvFilters = new BrightIdeasSoftware.ObjectListView();
            this.olvFilterName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.scMainLeftRightSplit)).BeginInit();
            this.scMainLeftRightSplit.Panel1.SuspendLayout();
            this.scMainLeftRightSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scFiltersAndColumns)).BeginInit();
            this.scFiltersAndColumns.Panel1.SuspendLayout();
            this.scFiltersAndColumns.Panel2.SuspendLayout();
            this.scFiltersAndColumns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvFilters)).BeginInit();
            this.SuspendLayout();
            // 
            // scMainLeftRightSplit
            // 
            this.scMainLeftRightSplit.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scMainLeftRightSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainLeftRightSplit.Location = new System.Drawing.Point(0, 0);
            this.scMainLeftRightSplit.Name = "scMainLeftRightSplit";
            // 
            // scMainLeftRightSplit.Panel1
            // 
            this.scMainLeftRightSplit.Panel1.Controls.Add(this.scFiltersAndColumns);
            this.scMainLeftRightSplit.Size = new System.Drawing.Size(1150, 828);
            this.scMainLeftRightSplit.SplitterDistance = 211;
            this.scMainLeftRightSplit.TabIndex = 0;
            // 
            // scFiltersAndColumns
            // 
            this.scFiltersAndColumns.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scFiltersAndColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scFiltersAndColumns.Location = new System.Drawing.Point(0, 0);
            this.scFiltersAndColumns.Name = "scFiltersAndColumns";
            this.scFiltersAndColumns.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scFiltersAndColumns.Panel1
            // 
            this.scFiltersAndColumns.Panel1.Controls.Add(this.olvExtractionInformations);
            // 
            // scFiltersAndColumns.Panel2
            // 
            this.scFiltersAndColumns.Panel2.Controls.Add(this.olvFilters);
            this.scFiltersAndColumns.Size = new System.Drawing.Size(211, 828);
            this.scFiltersAndColumns.SplitterDistance = 462;
            this.scFiltersAndColumns.TabIndex = 1;
            // 
            // olvExtractionInformations
            // 
            this.olvExtractionInformations.AllColumns.Add(this.olvColumn1);
            this.olvExtractionInformations.CellEditUseWholeCell = false;
            this.olvExtractionInformations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.olvExtractionInformations.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvExtractionInformations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvExtractionInformations.FullRowSelect = true;
            this.olvExtractionInformations.Location = new System.Drawing.Point(0, 0);
            this.olvExtractionInformations.Name = "olvExtractionInformations";
            this.olvExtractionInformations.RowHeight = 19;
            this.olvExtractionInformations.Size = new System.Drawing.Size(207, 458);
            this.olvExtractionInformations.TabIndex = 3;
            this.olvExtractionInformations.UseCompatibleStateImageBehavior = false;
            this.olvExtractionInformations.View = System.Windows.Forms.View.Details;
            this.olvExtractionInformations.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "ExtractionInformation(s)";
            // 
            // olvFilters
            // 
            this.olvFilters.AllColumns.Add(this.olvFilterName);
            this.olvFilters.CellEditUseWholeCell = false;
            this.olvFilters.CheckBoxes = true;
            this.olvFilters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvFilterName});
            this.olvFilters.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvFilters.FullRowSelect = true;
            this.olvFilters.Location = new System.Drawing.Point(0, 0);
            this.olvFilters.Name = "olvFilters";
            this.olvFilters.RowHeight = 19;
            this.olvFilters.Size = new System.Drawing.Size(207, 358);
            this.olvFilters.TabIndex = 4;
            this.olvFilters.UseCompatibleStateImageBehavior = false;
            this.olvFilters.View = System.Windows.Forms.View.Details;
            this.olvFilters.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.olvFilters_ItemChecked);
            this.olvFilters.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            // 
            // olvFilterName
            // 
            this.olvFilterName.AspectName = "ToString";
            this.olvFilterName.FillsFreeSpace = true;
            this.olvFilterName.Groupable = false;
            this.olvFilterName.Text = "Filters";
            // 
            // ViewExtractionSql
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scMainLeftRightSplit);
            this.Name = "ViewExtractionSql";
            this.Size = new System.Drawing.Size(1150, 828);
            this.scMainLeftRightSplit.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainLeftRightSplit)).EndInit();
            this.scMainLeftRightSplit.ResumeLayout(false);
            this.scFiltersAndColumns.Panel1.ResumeLayout(false);
            this.scFiltersAndColumns.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scFiltersAndColumns)).EndInit();
            this.scFiltersAndColumns.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvFilters)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scMainLeftRightSplit;
        private System.Windows.Forms.SplitContainer scFiltersAndColumns;
        private ObjectListView olvExtractionInformations;
        private OLVColumn olvColumn1;
        private ObjectListView olvFilters;
        private OLVColumn olvFilterName;
    }
}