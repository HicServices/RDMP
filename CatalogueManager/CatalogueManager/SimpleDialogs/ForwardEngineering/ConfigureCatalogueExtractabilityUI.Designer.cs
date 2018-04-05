namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    partial class ConfigureCatalogueExtractabilityUI
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
            this.gbMarkAllExtractable = new System.Windows.Forms.GroupBox();
            this.olvColumnExtractability = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractionCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractable = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.ddExtractionIdentifier = new System.Windows.Forms.ComboBox();
            this.cbMakeAllColumnsExtractable = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.lbPastedColumns = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.btnClearFilterMany = new System.Windows.Forms.Button();
            this.ddCategoriseMany = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gbMarkAllExtractable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMarkAllExtractable
            // 
            this.gbMarkAllExtractable.Controls.Add(this.olvColumnExtractability);
            this.gbMarkAllExtractable.Controls.Add(this.panel1);
            this.gbMarkAllExtractable.Controls.Add(this.panel2);
            this.gbMarkAllExtractable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMarkAllExtractable.Location = new System.Drawing.Point(0, 0);
            this.gbMarkAllExtractable.Name = "gbMarkAllExtractable";
            this.gbMarkAllExtractable.Size = new System.Drawing.Size(708, 674);
            this.gbMarkAllExtractable.TabIndex = 7;
            this.gbMarkAllExtractable.TabStop = false;
            this.gbMarkAllExtractable.Text = "Extractability";
            // 
            // olvColumnExtractability
            // 
            this.olvColumnExtractability.AllColumns.Add(this.olvColumnInfoName);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractionCategory);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractable);
            this.olvColumnExtractability.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvColumnExtractability.CellEditUseWholeCell = false;
            this.olvColumnExtractability.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnInfoName,
            this.olvExtractionCategory});
            this.olvColumnExtractability.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvColumnExtractability.FullRowSelect = true;
            this.olvColumnExtractability.Location = new System.Drawing.Point(3, 46);
            this.olvColumnExtractability.Name = "olvColumnExtractability";
            this.olvColumnExtractability.ShowGroups = false;
            this.olvColumnExtractability.Size = new System.Drawing.Size(702, 415);
            this.olvColumnExtractability.TabIndex = 7;
            this.olvColumnExtractability.UseCompatibleStateImageBehavior = false;
            this.olvColumnExtractability.UseFiltering = true;
            this.olvColumnExtractability.View = System.Windows.Forms.View.Details;
            // 
            // olvColumnInfoName
            // 
            this.olvColumnInfoName.AspectName = "ToString";
            this.olvColumnInfoName.FillsFreeSpace = true;
            this.olvColumnInfoName.Groupable = false;
            this.olvColumnInfoName.Text = "Column Name";
            // 
            // olvExtractionCategory
            // 
            this.olvExtractionCategory.Groupable = false;
            this.olvExtractionCategory.Text = "Catgeory";
            // 
            // olvExtractable
            // 
            this.olvExtractable.DisplayIndex = 2;
            this.olvExtractable.IsVisible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ddExtractionIdentifier);
            this.panel1.Controls.Add(this.cbMakeAllColumnsExtractable);
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(702, 29);
            this.panel1.TabIndex = 6;
            // 
            // ddExtractionIdentifier
            // 
            this.ddExtractionIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExtractionIdentifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionIdentifier.Enabled = false;
            this.ddExtractionIdentifier.FormattingEnabled = true;
            this.ddExtractionIdentifier.Location = new System.Drawing.Point(274, 3);
            this.ddExtractionIdentifier.Name = "ddExtractionIdentifier";
            this.ddExtractionIdentifier.Size = new System.Drawing.Size(382, 21);
            this.ddExtractionIdentifier.TabIndex = 2;
            // 
            // cbMakeAllColumnsExtractable
            // 
            this.cbMakeAllColumnsExtractable.AutoSize = true;
            this.cbMakeAllColumnsExtractable.Location = new System.Drawing.Point(1, 6);
            this.cbMakeAllColumnsExtractable.Name = "cbMakeAllColumnsExtractable";
            this.cbMakeAllColumnsExtractable.Size = new System.Drawing.Size(163, 17);
            this.cbMakeAllColumnsExtractable.TabIndex = 0;
            this.cbMakeAllColumnsExtractable.Text = "Make all columns extractable";
            this.cbMakeAllColumnsExtractable.UseVisualStyleBackColor = true;
            this.cbMakeAllColumnsExtractable.CheckedChanged += new System.EventHandler(this.cbMakeAllColumnsExtractable_CheckedChanged);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(662, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(40, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Extraction Identifier:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.ddCategoriseMany);
            this.panel2.Controls.Add(this.btnClearFilterMany);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lbPastedColumns);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.tbFilter);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 464);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(702, 207);
            this.panel2.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter many (paste in):";
            // 
            // lbPastedColumns
            // 
            this.lbPastedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPastedColumns.FormattingEnabled = true;
            this.lbPastedColumns.Location = new System.Drawing.Point(118, 29);
            this.lbPastedColumns.Name = "lbPastedColumns";
            this.lbPastedColumns.Size = new System.Drawing.Size(243, 173);
            this.lbPastedColumns.TabIndex = 2;
            this.lbPastedColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbPastedColumns_KeyUp);
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
            this.tbFilter.Location = new System.Drawing.Point(118, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(351, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // btnClearFilterMany
            // 
            this.btnClearFilterMany.Enabled = false;
            this.btnClearFilterMany.Location = new System.Drawing.Point(37, 57);
            this.btnClearFilterMany.Name = "btnClearFilterMany";
            this.btnClearFilterMany.Size = new System.Drawing.Size(75, 23);
            this.btnClearFilterMany.TabIndex = 4;
            this.btnClearFilterMany.Text = "Clear";
            this.btnClearFilterMany.UseVisualStyleBackColor = true;
            this.btnClearFilterMany.Click += new System.EventHandler(this.btnClearFilterMany_Click);
            // 
            // ddCategoriseMany
            // 
            this.ddCategoriseMany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCategoriseMany.FormattingEnabled = true;
            this.ddCategoriseMany.Location = new System.Drawing.Point(517, 97);
            this.ddCategoriseMany.Name = "ddCategoriseMany";
            this.ddCategoriseMany.Size = new System.Drawing.Size(121, 21);
            this.ddCategoriseMany.TabIndex = 5;
            this.ddCategoriseMany.SelectedIndexChanged += new System.EventHandler(this.ddCategoriseMany_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(411, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Set Category for all:";
            // 
            // ConfigureCatalogueExtractabilityUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbMarkAllExtractable);
            this.Name = "ConfigureCatalogueExtractabilityUI";
            this.Size = new System.Drawing.Size(708, 674);
            this.gbMarkAllExtractable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbMarkAllExtractable;
        private System.Windows.Forms.ComboBox ddExtractionIdentifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbMakeAllColumnsExtractable;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Panel panel1;
        private BrightIdeasSoftware.ObjectListView olvColumnExtractability;
        private BrightIdeasSoftware.OLVColumn olvColumnInfoName;
        private BrightIdeasSoftware.OLVColumn olvExtractionCategory;
        private BrightIdeasSoftware.OLVColumn olvExtractable;
        private System.Windows.Forms.ListBox lbPastedColumns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearFilterMany;
        private System.Windows.Forms.ComboBox ddCategoriseMany;
        private System.Windows.Forms.Label label3;
    }
}
