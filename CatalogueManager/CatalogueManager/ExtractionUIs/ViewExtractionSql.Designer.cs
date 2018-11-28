using BrightIdeasSoftware;

namespace CatalogueManager.ExtractionUIs
{
    partial class ViewExtractionSql
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
            this.btnAdvancedReorder = new System.Windows.Forms.Button();
            this.clbFilters = new System.Windows.Forms.CheckedListBox();
            this.scRadioButtonsSqlSplit = new System.Windows.Forms.SplitContainer();
            this.rbCoreSupplementalAndSpecialApproval = new System.Windows.Forms.RadioButton();
            this.rbInternal = new System.Windows.Forms.RadioButton();
            this.rbSupplemental = new System.Windows.Forms.RadioButton();
            this.rbCoreOnly = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.scMainLeftRightSplit)).BeginInit();
            this.scMainLeftRightSplit.Panel1.SuspendLayout();
            this.scMainLeftRightSplit.Panel2.SuspendLayout();
            this.scMainLeftRightSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scFiltersAndColumns)).BeginInit();
            this.scFiltersAndColumns.Panel1.SuspendLayout();
            this.scFiltersAndColumns.Panel2.SuspendLayout();
            this.scFiltersAndColumns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scRadioButtonsSqlSplit)).BeginInit();
            this.scRadioButtonsSqlSplit.Panel1.SuspendLayout();
            this.scRadioButtonsSqlSplit.SuspendLayout();
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
            // 
            // scMainLeftRightSplit.Panel2
            // 
            this.scMainLeftRightSplit.Panel2.Controls.Add(this.scRadioButtonsSqlSplit);
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
            this.scFiltersAndColumns.Panel1.Controls.Add(this.btnAdvancedReorder);
            // 
            // scFiltersAndColumns.Panel2
            // 
            this.scFiltersAndColumns.Panel2.Controls.Add(this.clbFilters);
            this.scFiltersAndColumns.Size = new System.Drawing.Size(211, 828);
            this.scFiltersAndColumns.SplitterDistance = 464;
            this.scFiltersAndColumns.TabIndex = 1;
            // 
            // olvExtractionInformations
            // 
            this.olvExtractionInformations.AllColumns.Add(this.olvColumn1);
            this.olvExtractionInformations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvExtractionInformations.CellEditUseWholeCell = false;
            this.olvExtractionInformations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.olvExtractionInformations.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvExtractionInformations.FullRowSelect = true;
            this.olvExtractionInformations.Location = new System.Drawing.Point(3, 1);
            this.olvExtractionInformations.Name = "olvExtractionInformations";
            this.olvExtractionInformations.RowHeight = 19;
            this.olvExtractionInformations.Size = new System.Drawing.Size(204, 433);
            this.olvExtractionInformations.TabIndex = 3;
            this.olvExtractionInformations.UseCompatibleStateImageBehavior = false;
            this.olvExtractionInformations.View = System.Windows.Forms.View.Details;
            this.olvExtractionInformations.ItemActivate += new System.EventHandler(this.olvExtractionInformations_ItemActivate);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "ExtractionInformation(s)";
            // 
            // btnAdvancedReorder
            // 
            this.btnAdvancedReorder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvancedReorder.Location = new System.Drawing.Point(144, 434);
            this.btnAdvancedReorder.Name = "btnAdvancedReorder";
            this.btnAdvancedReorder.Size = new System.Drawing.Size(63, 23);
            this.btnAdvancedReorder.TabIndex = 2;
            this.btnAdvancedReorder.Text = "Re-Order";
            this.btnAdvancedReorder.UseVisualStyleBackColor = true;
            this.btnAdvancedReorder.Click += new System.EventHandler(this.btnAdvancedReorder_Click);
            // 
            // clbFilters
            // 
            this.clbFilters.CheckOnClick = true;
            this.clbFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clbFilters.FormattingEnabled = true;
            this.clbFilters.Location = new System.Drawing.Point(0, 0);
            this.clbFilters.Name = "clbFilters";
            this.clbFilters.Size = new System.Drawing.Size(207, 356);
            this.clbFilters.TabIndex = 0;
            this.clbFilters.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbFilters_ItemCheck);
            // 
            // scRadioButtonsSqlSplit
            // 
            this.scRadioButtonsSqlSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scRadioButtonsSqlSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scRadioButtonsSqlSplit.Location = new System.Drawing.Point(0, 0);
            this.scRadioButtonsSqlSplit.Name = "scRadioButtonsSqlSplit";
            this.scRadioButtonsSqlSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scRadioButtonsSqlSplit.Panel1
            // 
            this.scRadioButtonsSqlSplit.Panel1.Controls.Add(this.rbCoreSupplementalAndSpecialApproval);
            this.scRadioButtonsSqlSplit.Panel1.Controls.Add(this.rbInternal);
            this.scRadioButtonsSqlSplit.Panel1.Controls.Add(this.rbSupplemental);
            this.scRadioButtonsSqlSplit.Panel1.Controls.Add(this.rbCoreOnly);
            this.scRadioButtonsSqlSplit.Panel1MinSize = 36;
            // 
            // scRadioButtonsSqlSplit.Panel2
            // 
            this.scRadioButtonsSqlSplit.Panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scRadioButtonsSqlSplit.Size = new System.Drawing.Size(931, 824);
            this.scRadioButtonsSqlSplit.SplitterDistance = 36;
            this.scRadioButtonsSqlSplit.TabIndex = 0;
            // 
            // rbCoreSupplementalAndSpecialApproval
            // 
            this.rbCoreSupplementalAndSpecialApproval.AutoSize = true;
            this.rbCoreSupplementalAndSpecialApproval.Checked = true;
            this.rbCoreSupplementalAndSpecialApproval.Location = new System.Drawing.Point(209, 9);
            this.rbCoreSupplementalAndSpecialApproval.Name = "rbCoreSupplementalAndSpecialApproval";
            this.rbCoreSupplementalAndSpecialApproval.Size = new System.Drawing.Size(215, 17);
            this.rbCoreSupplementalAndSpecialApproval.TabIndex = 4;
            this.rbCoreSupplementalAndSpecialApproval.TabStop = true;
            this.rbCoreSupplementalAndSpecialApproval.Text = "Core + Supplemental + Special Approval";
            this.rbCoreSupplementalAndSpecialApproval.UseVisualStyleBackColor = true;
            this.rbCoreSupplementalAndSpecialApproval.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
            // 
            // rbInternal
            // 
            this.rbInternal.AutoSize = true;
            this.rbInternal.Location = new System.Drawing.Point(430, 9);
            this.rbInternal.Name = "rbInternal";
            this.rbInternal.Size = new System.Drawing.Size(60, 17);
            this.rbInternal.TabIndex = 2;
            this.rbInternal.Text = "Internal";
            this.rbInternal.UseVisualStyleBackColor = true;
            this.rbInternal.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
            // 
            // rbSupplemental
            // 
            this.rbSupplemental.AutoSize = true;
            this.rbSupplemental.Location = new System.Drawing.Point(80, 9);
            this.rbSupplemental.Name = "rbSupplemental";
            this.rbSupplemental.Size = new System.Drawing.Size(123, 17);
            this.rbSupplemental.TabIndex = 1;
            this.rbSupplemental.Text = "Core + Supplemental";
            this.rbSupplemental.UseVisualStyleBackColor = true;
            this.rbSupplemental.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
            // 
            // rbCoreOnly
            // 
            this.rbCoreOnly.AutoSize = true;
            this.rbCoreOnly.Location = new System.Drawing.Point(3, 9);
            this.rbCoreOnly.Name = "rbCoreOnly";
            this.rbCoreOnly.Size = new System.Drawing.Size(71, 17);
            this.rbCoreOnly.TabIndex = 0;
            this.rbCoreOnly.Text = "Core Only";
            this.rbCoreOnly.UseVisualStyleBackColor = true;
            this.rbCoreOnly.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
            // 
            // ViewExtractionSql
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scMainLeftRightSplit);
            this.Name = "ViewExtractionSql";
            this.Size = new System.Drawing.Size(1150, 828);
            this.scMainLeftRightSplit.Panel1.ResumeLayout(false);
            this.scMainLeftRightSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainLeftRightSplit)).EndInit();
            this.scMainLeftRightSplit.ResumeLayout(false);
            this.scFiltersAndColumns.Panel1.ResumeLayout(false);
            this.scFiltersAndColumns.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scFiltersAndColumns)).EndInit();
            this.scFiltersAndColumns.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).EndInit();
            this.scRadioButtonsSqlSplit.Panel1.ResumeLayout(false);
            this.scRadioButtonsSqlSplit.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scRadioButtonsSqlSplit)).EndInit();
            this.scRadioButtonsSqlSplit.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scMainLeftRightSplit;
        private System.Windows.Forms.SplitContainer scRadioButtonsSqlSplit;
        private System.Windows.Forms.RadioButton rbSupplemental;
        private System.Windows.Forms.RadioButton rbCoreOnly;
        private System.Windows.Forms.SplitContainer scFiltersAndColumns;
        private System.Windows.Forms.CheckedListBox clbFilters;
        private System.Windows.Forms.RadioButton rbInternal;
        private System.Windows.Forms.Button btnAdvancedReorder;
        private System.Windows.Forms.RadioButton rbCoreSupplementalAndSpecialApproval;
        private ObjectListView olvExtractionInformations;
        private OLVColumn olvColumn1;
    }
}