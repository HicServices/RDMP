using CatalogueManager.AggregationUIs;

namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class ConfigureMetadataReport
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
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.cbIncludeDeprecated = new System.Windows.Forms.CheckBox();
            this.cbIncludeInternal = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.cbxCatalogues = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnPick = new System.Windows.Forms.Button();
            this.rbSpecificCatalogue = new System.Windows.Forms.RadioButton();
            this.rbAllCatalogues = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.cbIncludeRowCounts = new System.Windows.Forms.CheckBox();
            this.cbIncludeDistinctIdentifiers = new System.Windows.Forms.CheckBox();
            this.aggregateGraph1 = new CatalogueManager.AggregationUIs.AggregateGraph();
            this.cbIncludeGraphs = new System.Windows.Forms.CheckBox();
            this.nMaxLookupRows = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBarsUI1 = new ReusableUIComponents.Progress.ProgressBarsUI();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMaxLookupRows)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "To Generate For:";
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
            // cbIncludeDeprecated
            // 
            this.cbIncludeDeprecated.AutoSize = true;
            this.cbIncludeDeprecated.Location = new System.Drawing.Point(124, 34);
            this.cbIncludeDeprecated.Name = "cbIncludeDeprecated";
            this.cbIncludeDeprecated.Size = new System.Drawing.Size(120, 17);
            this.cbIncludeDeprecated.TabIndex = 1;
            this.cbIncludeDeprecated.Text = "Include Deprecated";
            this.cbIncludeDeprecated.UseVisualStyleBackColor = true;
            // 
            // cbIncludeInternal
            // 
            this.cbIncludeInternal.AutoSize = true;
            this.cbIncludeInternal.Location = new System.Drawing.Point(246, 34);
            this.cbIncludeInternal.Name = "cbIncludeInternal";
            this.cbIncludeInternal.Size = new System.Drawing.Size(99, 17);
            this.cbIncludeInternal.TabIndex = 2;
            this.cbIncludeInternal.Text = "Include Internal";
            this.cbIncludeInternal.UseVisualStyleBackColor = true;
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
            this.cbxCatalogues.Location = new System.Drawing.Point(48, 79);
            this.cbxCatalogues.Name = "cbxCatalogues";
            this.cbxCatalogues.Size = new System.Drawing.Size(530, 21);
            this.cbxCatalogues.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnPick);
            this.groupBox1.Controls.Add(this.rbSpecificCatalogue);
            this.groupBox1.Controls.Add(this.rbAllCatalogues);
            this.groupBox1.Controls.Add(this.cbxCatalogues);
            this.groupBox1.Controls.Add(this.cbIncludeInternal);
            this.groupBox1.Controls.Add(this.cbIncludeDeprecated);
            this.groupBox1.Location = new System.Drawing.Point(17, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(622, 111);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Catalogue";
            // 
            // btnPick
            // 
            this.btnPick.Enabled = false;
            this.btnPick.Location = new System.Drawing.Point(584, 79);
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
            this.rbSpecificCatalogue.Location = new System.Drawing.Point(26, 56);
            this.rbSpecificCatalogue.Name = "rbSpecificCatalogue";
            this.rbSpecificCatalogue.Size = new System.Drawing.Size(100, 17);
            this.rbSpecificCatalogue.TabIndex = 3;
            this.rbSpecificCatalogue.Text = "Only Catalogue:";
            this.rbSpecificCatalogue.UseVisualStyleBackColor = true;
            this.rbSpecificCatalogue.CheckedChanged += new System.EventHandler(this.rbSpecificCatalogue_CheckedChanged);
            // 
            // rbAllCatalogues
            // 
            this.rbAllCatalogues.AutoSize = true;
            this.rbAllCatalogues.Checked = true;
            this.rbAllCatalogues.Location = new System.Drawing.Point(26, 33);
            this.rbAllCatalogues.Name = "rbAllCatalogues";
            this.rbAllCatalogues.Size = new System.Drawing.Size(92, 17);
            this.rbAllCatalogues.TabIndex = 0;
            this.rbAllCatalogues.TabStop = true;
            this.rbAllCatalogues.Text = "All Catalogues";
            this.rbAllCatalogues.UseVisualStyleBackColor = true;
            this.rbAllCatalogues.CheckedChanged += new System.EventHandler(this.rbAllCatalogues_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(797, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Query Timeout:";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(882, 136);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 20);
            this.tbTimeout.TabIndex = 9;
            this.tbTimeout.Text = "30";
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // cbIncludeRowCounts
            // 
            this.cbIncludeRowCounts.AutoSize = true;
            this.cbIncludeRowCounts.Checked = true;
            this.cbIncludeRowCounts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeRowCounts.Location = new System.Drawing.Point(664, 75);
            this.cbIncludeRowCounts.Name = "cbIncludeRowCounts";
            this.cbIncludeRowCounts.Size = new System.Drawing.Size(122, 17);
            this.cbIncludeRowCounts.TabIndex = 5;
            this.cbIncludeRowCounts.Text = "Include Row Counts";
            this.cbIncludeRowCounts.UseVisualStyleBackColor = true;
            // 
            // cbIncludeDistinctIdentifiers
            // 
            this.cbIncludeDistinctIdentifiers.AutoSize = true;
            this.cbIncludeDistinctIdentifiers.Checked = true;
            this.cbIncludeDistinctIdentifiers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeDistinctIdentifiers.Location = new System.Drawing.Point(664, 52);
            this.cbIncludeDistinctIdentifiers.Name = "cbIncludeDistinctIdentifiers";
            this.cbIncludeDistinctIdentifiers.Size = new System.Drawing.Size(173, 17);
            this.cbIncludeDistinctIdentifiers.TabIndex = 5;
            this.cbIncludeDistinctIdentifiers.Text = "Include Distinct Identifier Count";
            this.cbIncludeDistinctIdentifiers.UseVisualStyleBackColor = true;
            this.cbIncludeDistinctIdentifiers.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // aggregateGraph1
            // 
            this.aggregateGraph1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.aggregateGraph1.BackColor = System.Drawing.Color.White;
            this.aggregateGraph1.Location = new System.Drawing.Point(5, 351);
            this.aggregateGraph1.Name = "aggregateGraph1";
            this.aggregateGraph1.Size = new System.Drawing.Size(781, 501);
            this.aggregateGraph1.TabIndex = 10;
            this.aggregateGraph1.Timeout = 0;
            this.aggregateGraph1.Visible = false;
            // 
            // cbIncludeGraphs
            // 
            this.cbIncludeGraphs.AutoSize = true;
            this.cbIncludeGraphs.Checked = true;
            this.cbIncludeGraphs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeGraphs.Location = new System.Drawing.Point(664, 29);
            this.cbIncludeGraphs.Name = "cbIncludeGraphs";
            this.cbIncludeGraphs.Size = new System.Drawing.Size(98, 17);
            this.cbIncludeGraphs.TabIndex = 5;
            this.cbIncludeGraphs.Text = "Include Graphs";
            this.cbIncludeGraphs.UseVisualStyleBackColor = true;
            this.cbIncludeGraphs.CheckedChanged += new System.EventHandler(this.cbIncludeDistinctIdentifiers_CheckedChanged);
            // 
            // nMaxLookupRows
            // 
            this.nMaxLookupRows.Location = new System.Drawing.Point(756, 97);
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
            this.label3.Location = new System.Drawing.Point(664, 99);
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
            // ConfigureMetadataReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 864);
            this.Controls.Add(this.progressBarsUI1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nMaxLookupRows);
            this.Controls.Add(this.cbIncludeGraphs);
            this.Controls.Add(this.cbIncludeDistinctIdentifiers);
            this.Controls.Add(this.cbIncludeRowCounts);
            this.Controls.Add(this.aggregateGraph1);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.label1);
            this.Name = "ConfigureMetadataReport";
            this.Text = "ConfigureMetadataReport";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigureMetadataReport_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMaxLookupRows)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.CheckBox cbIncludeDeprecated;
        private System.Windows.Forms.CheckBox cbIncludeInternal;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ComboBox cbxCatalogues;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbSpecificCatalogue;
        private System.Windows.Forms.RadioButton rbAllCatalogues;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbTimeout;
        private AggregateGraph aggregateGraph1;
        private System.Windows.Forms.CheckBox cbIncludeRowCounts;
        private System.Windows.Forms.CheckBox cbIncludeDistinctIdentifiers;
        private System.Windows.Forms.CheckBox cbIncludeGraphs;
        private System.Windows.Forms.NumericUpDown nMaxLookupRows;
        private System.Windows.Forms.Label label3;
        private ReusableUIComponents.Progress.ProgressBarsUI progressBarsUI1;
        private System.Windows.Forms.Button btnPick;
    }
}