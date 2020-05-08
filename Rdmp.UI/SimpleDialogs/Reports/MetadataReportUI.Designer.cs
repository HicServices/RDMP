using Rdmp.UI.AggregationUIs;
using Rdmp.UI.Progress;

namespace Rdmp.UI.SimpleDialogs.Reports
{
    partial class MetadataReportUI
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
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.cbxCatalogues = new System.Windows.Forms.ComboBox();
            this.gbInclude = new System.Windows.Forms.GroupBox();
            this.cbIncludeDeprecatedCatalogueItems = new System.Windows.Forms.CheckBox();
            this.cbIncludeGraphs = new System.Windows.Forms.CheckBox();
            this.cbIncludeNonExtractable = new System.Windows.Forms.CheckBox();
            this.cbIncludeInternalCatalogueItems = new System.Windows.Forms.CheckBox();
            this.cbIncludeDistinctIdentifiers = new System.Windows.Forms.CheckBox();
            this.cbIncludeRowCounts = new System.Windows.Forms.CheckBox();
            this.btnPick = new System.Windows.Forms.Button();
            this.rbSpecificCatalogue = new System.Windows.Forms.RadioButton();
            this.rbAllCatalogues = new System.Windows.Forms.RadioButton();
            this.gbCatalogue = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.aggregateGraph1 = new Rdmp.UI.AggregationUIs.AggregateGraphUI();
            this.nMaxLookupRows = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBarsUI1 = new Rdmp.UI.Progress.ProgressBarsUI();
            this.gbReportOptions = new System.Windows.Forms.GroupBox();
            this.btnFolder = new System.Windows.Forms.Button();
            this.gbInclude.SuspendLayout();
            this.gbCatalogue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMaxLookupRows)).BeginInit();
            this.gbReportOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnGenerateReport.Location = new System.Drawing.Point(374, 139);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(112, 23);
            this.btnGenerateReport.TabIndex = 1;
            this.btnGenerateReport.Text = "Generate Report";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnStop.Location = new System.Drawing.Point(492, 139);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(112, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // cbxCatalogues
            // 
            this.cbxCatalogues.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbxCatalogues.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxCatalogues.Enabled = false;
            this.cbxCatalogues.FormattingEnabled = true;
            this.cbxCatalogues.Location = new System.Drawing.Point(124, 41);
            this.cbxCatalogues.Name = "cbxCatalogues";
            this.cbxCatalogues.Size = new System.Drawing.Size(258, 21);
            this.cbxCatalogues.TabIndex = 4;
            this.cbxCatalogues.SelectedIndexChanged += new System.EventHandler(this.cbxCatalogues_SelectedIndexChanged);
            // 
            // gbInclude
            // 
            this.gbInclude.Controls.Add(this.cbIncludeDeprecatedCatalogueItems);
            this.gbInclude.Controls.Add(this.cbIncludeGraphs);
            this.gbInclude.Controls.Add(this.cbIncludeNonExtractable);
            this.gbInclude.Controls.Add(this.cbIncludeInternalCatalogueItems);
            this.gbInclude.Controls.Add(this.cbIncludeDistinctIdentifiers);
            this.gbInclude.Controls.Add(this.cbIncludeRowCounts);
            this.gbInclude.Location = new System.Drawing.Point(470, 4);
            this.gbInclude.Name = "gbInclude";
            this.gbInclude.Size = new System.Drawing.Size(325, 117);
            this.gbInclude.TabIndex = 0;
            this.gbInclude.TabStop = false;
            this.gbInclude.Text = "Include";
            // 
            // cbIncludeDeprecatedCatalogueItems
            // 
            this.cbIncludeDeprecatedCatalogueItems.AutoSize = true;
            this.cbIncludeDeprecatedCatalogueItems.Checked = true;
            this.cbIncludeDeprecatedCatalogueItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeDeprecatedCatalogueItems.Location = new System.Drawing.Point(145, 65);
            this.cbIncludeDeprecatedCatalogueItems.Name = "cbIncludeDeprecatedCatalogueItems";
            this.cbIncludeDeprecatedCatalogueItems.Size = new System.Drawing.Size(158, 17);
            this.cbIncludeDeprecatedCatalogueItems.TabIndex = 5;
            this.cbIncludeDeprecatedCatalogueItems.Text = "Deprecated CatalogueItems";
            this.cbIncludeDeprecatedCatalogueItems.UseVisualStyleBackColor = true;
            this.cbIncludeDeprecatedCatalogueItems.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // cbIncludeGraphs
            // 
            this.cbIncludeGraphs.AutoSize = true;
            this.cbIncludeGraphs.Checked = true;
            this.cbIncludeGraphs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeGraphs.Location = new System.Drawing.Point(6, 19);
            this.cbIncludeGraphs.Name = "cbIncludeGraphs";
            this.cbIncludeGraphs.Size = new System.Drawing.Size(60, 17);
            this.cbIncludeGraphs.TabIndex = 5;
            this.cbIncludeGraphs.Text = "Graphs";
            this.cbIncludeGraphs.UseVisualStyleBackColor = true;
            this.cbIncludeGraphs.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // cbIncludeNonExtractable
            // 
            this.cbIncludeNonExtractable.AutoSize = true;
            this.cbIncludeNonExtractable.Location = new System.Drawing.Point(145, 42);
            this.cbIncludeNonExtractable.Name = "cbIncludeNonExtractable";
            this.cbIncludeNonExtractable.Size = new System.Drawing.Size(181, 17);
            this.cbIncludeNonExtractable.TabIndex = 5;
            this.cbIncludeNonExtractable.Tag = "";
            this.cbIncludeNonExtractable.Text = "Non Extractable Catalogue Items";
            this.cbIncludeNonExtractable.UseVisualStyleBackColor = true;
            this.cbIncludeNonExtractable.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // cbIncludeInternalCatalogueItems
            // 
            this.cbIncludeInternalCatalogueItems.AutoSize = true;
            this.cbIncludeInternalCatalogueItems.Location = new System.Drawing.Point(145, 19);
            this.cbIncludeInternalCatalogueItems.Name = "cbIncludeInternalCatalogueItems";
            this.cbIncludeInternalCatalogueItems.Size = new System.Drawing.Size(140, 17);
            this.cbIncludeInternalCatalogueItems.TabIndex = 5;
            this.cbIncludeInternalCatalogueItems.Text = "Internal Catalogue Items";
            this.cbIncludeInternalCatalogueItems.UseVisualStyleBackColor = true;
            this.cbIncludeInternalCatalogueItems.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // cbIncludeDistinctIdentifiers
            // 
            this.cbIncludeDistinctIdentifiers.AutoSize = true;
            this.cbIncludeDistinctIdentifiers.Checked = true;
            this.cbIncludeDistinctIdentifiers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeDistinctIdentifiers.Location = new System.Drawing.Point(6, 43);
            this.cbIncludeDistinctIdentifiers.Name = "cbIncludeDistinctIdentifiers";
            this.cbIncludeDistinctIdentifiers.Size = new System.Drawing.Size(135, 17);
            this.cbIncludeDistinctIdentifiers.TabIndex = 5;
            this.cbIncludeDistinctIdentifiers.Text = "Distinct Identifier Count";
            this.cbIncludeDistinctIdentifiers.UseVisualStyleBackColor = true;
            this.cbIncludeDistinctIdentifiers.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // cbIncludeRowCounts
            // 
            this.cbIncludeRowCounts.AutoSize = true;
            this.cbIncludeRowCounts.Checked = true;
            this.cbIncludeRowCounts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeRowCounts.Location = new System.Drawing.Point(6, 66);
            this.cbIncludeRowCounts.Name = "cbIncludeRowCounts";
            this.cbIncludeRowCounts.Size = new System.Drawing.Size(84, 17);
            this.cbIncludeRowCounts.TabIndex = 5;
            this.cbIncludeRowCounts.Text = "Row Counts";
            this.cbIncludeRowCounts.UseVisualStyleBackColor = true;
            // 
            // btnPick
            // 
            this.btnPick.Enabled = false;
            this.btnPick.Location = new System.Drawing.Point(389, 41);
            this.btnPick.Name = "btnPick";
            this.btnPick.Size = new System.Drawing.Size(32, 21);
            this.btnPick.TabIndex = 5;
            this.btnPick.Text = "...";
            this.btnPick.UseVisualStyleBackColor = true;
            this.btnPick.Click += new System.EventHandler(this.btnPick_Click);
            // 
            // rbSpecificCatalogue
            // 
            this.rbSpecificCatalogue.AutoSize = true;
            this.rbSpecificCatalogue.Location = new System.Drawing.Point(7, 42);
            this.rbSpecificCatalogue.Name = "rbSpecificCatalogue";
            this.rbSpecificCatalogue.Size = new System.Drawing.Size(111, 17);
            this.rbSpecificCatalogue.TabIndex = 3;
            this.rbSpecificCatalogue.Text = "Only Catalogue(s):";
            this.rbSpecificCatalogue.UseVisualStyleBackColor = true;
            this.rbSpecificCatalogue.CheckedChanged += new System.EventHandler(this.rbSpecificCatalogue_CheckedChanged);
            // 
            // rbAllCatalogues
            // 
            this.rbAllCatalogues.AutoSize = true;
            this.rbAllCatalogues.Checked = true;
            this.rbAllCatalogues.Location = new System.Drawing.Point(7, 19);
            this.rbAllCatalogues.Name = "rbAllCatalogues";
            this.rbAllCatalogues.Size = new System.Drawing.Size(92, 17);
            this.rbAllCatalogues.TabIndex = 0;
            this.rbAllCatalogues.TabStop = true;
            this.rbAllCatalogues.Text = "All Catalogues";
            this.rbAllCatalogues.UseVisualStyleBackColor = true;
            this.rbAllCatalogues.CheckedChanged += new System.EventHandler(this.rbAllCatalogues_CheckedChanged);
            // 
            // gbCatalogue
            // 
            this.gbCatalogue.Controls.Add(this.btnFolder);
            this.gbCatalogue.Controls.Add(this.btnPick);
            this.gbCatalogue.Controls.Add(this.rbSpecificCatalogue);
            this.gbCatalogue.Controls.Add(this.rbAllCatalogues);
            this.gbCatalogue.Controls.Add(this.cbxCatalogues);
            this.gbCatalogue.Location = new System.Drawing.Point(5, 4);
            this.gbCatalogue.Name = "gbCatalogue";
            this.gbCatalogue.Size = new System.Drawing.Size(459, 117);
            this.gbCatalogue.TabIndex = 0;
            this.gbCatalogue.TabStop = false;
            this.gbCatalogue.Text = "Generate For Catalogue";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Query Timeout:";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(101, 18);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 20);
            this.tbTimeout.TabIndex = 9;
            this.tbTimeout.Text = "30";
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // aggregateGraph1
            // 
            this.aggregateGraph1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.aggregateGraph1.BackColor = System.Drawing.Color.White;
            this.aggregateGraph1.Location = new System.Drawing.Point(5, 351);
            this.aggregateGraph1.Name = "aggregateGraph1";
            this.aggregateGraph1.Silent = false;
            this.aggregateGraph1.Size = new System.Drawing.Size(781, 501);
            this.aggregateGraph1.TabIndex = 10;
            this.aggregateGraph1.Timeout = 0;
            this.aggregateGraph1.Visible = false;
            // 
            // nMaxLookupRows
            // 
            this.nMaxLookupRows.Location = new System.Drawing.Point(101, 41);
            this.nMaxLookupRows.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nMaxLookupRows.Name = "nMaxLookupRows";
            this.nMaxLookupRows.Size = new System.Drawing.Size(120, 20);
            this.nMaxLookupRows.TabIndex = 11;
            this.nMaxLookupRows.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Max lookup rows:";
            // 
            // progressBarsUI1
            // 
            this.progressBarsUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarsUI1.Location = new System.Drawing.Point(12, 168);
            this.progressBarsUI1.Name = "progressBarsUI1";
            this.progressBarsUI1.Size = new System.Drawing.Size(1016, 181);
            this.progressBarsUI1.TabIndex = 13;
            // 
            // gbReportOptions
            // 
            this.gbReportOptions.Controls.Add(this.label3);
            this.gbReportOptions.Controls.Add(this.tbTimeout);
            this.gbReportOptions.Controls.Add(this.label2);
            this.gbReportOptions.Controls.Add(this.nMaxLookupRows);
            this.gbReportOptions.Location = new System.Drawing.Point(799, 4);
            this.gbReportOptions.Name = "gbReportOptions";
            this.gbReportOptions.Size = new System.Drawing.Size(229, 117);
            this.gbReportOptions.TabIndex = 14;
            this.gbReportOptions.TabStop = false;
            this.gbReportOptions.Text = "Options";
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(421, 41);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(32, 21);
            this.btnFolder.TabIndex = 6;
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // MetadataReportUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 864);
            this.Controls.Add(this.gbInclude);
            this.Controls.Add(this.gbCatalogue);
            this.Controls.Add(this.gbReportOptions);
            this.Controls.Add(this.progressBarsUI1);
            this.Controls.Add(this.aggregateGraph1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnGenerateReport);
            this.Name = "MetadataReportUI";
            this.Text = "Metadata Report";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigureMetadataReport_FormClosing);
            this.gbInclude.ResumeLayout(false);
            this.gbInclude.PerformLayout();
            this.gbCatalogue.ResumeLayout(false);
            this.gbCatalogue.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMaxLookupRows)).EndInit();
            this.gbReportOptions.ResumeLayout(false);
            this.gbReportOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ComboBox cbxCatalogues;
        private System.Windows.Forms.GroupBox gbInclude;
        private System.Windows.Forms.GroupBox gbCatalogue;
        private System.Windows.Forms.RadioButton rbSpecificCatalogue;
        private System.Windows.Forms.RadioButton rbAllCatalogues;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbTimeout;
        private AggregateGraphUI aggregateGraph1;
        private System.Windows.Forms.CheckBox cbIncludeRowCounts;
        private System.Windows.Forms.CheckBox cbIncludeDistinctIdentifiers;
        private System.Windows.Forms.CheckBox cbIncludeGraphs;
        private System.Windows.Forms.CheckBox cbIncludeNonExtractable;
        private System.Windows.Forms.NumericUpDown nMaxLookupRows;
        private System.Windows.Forms.Label label3;
        private ProgressBarsUI progressBarsUI1;
        private System.Windows.Forms.Button btnPick;
        private System.Windows.Forms.GroupBox gbReportOptions;
        private System.Windows.Forms.CheckBox cbIncludeDeprecatedCatalogueItems;
        private System.Windows.Forms.CheckBox cbIncludeInternalCatalogueItems;
        private System.Windows.Forms.Button btnFolder;
    }
}