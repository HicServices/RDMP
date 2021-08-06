using BrightIdeasSoftware;

namespace Rdmp.UI.SimpleDialogs
{
    partial class BulkProcessCatalogueItemsUI
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
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbExactMatch = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.lbPastedColumns = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.olvCatalogueItems = new BrightIdeasSoftware.ObjectListView();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnApplyTransform = new System.Windows.Forms.Button();
            this.rbDeleteExtrctionInformation = new System.Windows.Forms.RadioButton();
            this.rbDeleteAssociatedColumnInfos = new System.Windows.Forms.RadioButton();
            this.rbDelete = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.cbTableInfos = new System.Windows.Forms.ComboBox();
            this.rbGuessNewAssociatedColumns = new System.Windows.Forms.RadioButton();
            this.cbRecategorise = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.ddExtractionCategory = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.rbMarkExtractable = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogueItems)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Paste Columns Here:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "CatalogueItemsAffected";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel2);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.lbPastedColumns);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1097, 693);
            this.splitContainer1.SplitterDistance = 294;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cbExactMatch);
            this.panel2.Controls.Add(this.btnClear);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 661);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(294, 32);
            this.panel2.TabIndex = 8;
            // 
            // cbExactMatch
            // 
            this.cbExactMatch.AutoSize = true;
            this.cbExactMatch.Checked = true;
            this.cbExactMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbExactMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbExactMatch.Location = new System.Drawing.Point(88, 0);
            this.cbExactMatch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbExactMatch.Name = "cbExactMatch";
            this.cbExactMatch.Size = new System.Drawing.Size(206, 32);
            this.cbExactMatch.TabIndex = 4;
            this.cbExactMatch.Text = "Exact Matches Only";
            this.cbExactMatch.UseVisualStyleBackColor = true;
            this.cbExactMatch.CheckedChanged += new System.EventHandler(this.cbExactMatch_CheckedChanged);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnClear.Location = new System.Drawing.Point(0, 0);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(88, 32);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(-18, -17);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 19);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // lbPastedColumns
            // 
            this.lbPastedColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPastedColumns.FormattingEnabled = true;
            this.lbPastedColumns.ItemHeight = 15;
            this.lbPastedColumns.Location = new System.Drawing.Point(0, 15);
            this.lbPastedColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lbPastedColumns.Name = "lbPastedColumns";
            this.lbPastedColumns.Size = new System.Drawing.Size(294, 678);
            this.lbPastedColumns.TabIndex = 1;
            this.lbPastedColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbPastedColumns_KeyUp);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panel1);
            this.splitContainer2.Panel1.Controls.Add(this.olvCatalogueItems);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(798, 693);
            this.splitContainer2.SplitterDistance = 325;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbFilter);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 661);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(325, 32);
            this.panel1.TabIndex = 8;
            // 
            // tbFilter
            // 
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFilter.Location = new System.Drawing.Point(36, 0);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(289, 23);
            this.tbFilter.TabIndex = 3;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Filter:";
            // 
            // olvCatalogueItems
            // 
            this.olvCatalogueItems.AllColumns.Add(this.olvName);
            this.olvCatalogueItems.CellEditUseWholeCell = false;
            this.olvCatalogueItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvCatalogueItems.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvCatalogueItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvCatalogueItems.HideSelection = false;
            this.olvCatalogueItems.Location = new System.Drawing.Point(0, 15);
            this.olvCatalogueItems.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvCatalogueItems.Name = "olvCatalogueItems";
            this.olvCatalogueItems.RowHeight = 19;
            this.olvCatalogueItems.ShowGroups = false;
            this.olvCatalogueItems.Size = new System.Drawing.Size(325, 678);
            this.olvCatalogueItems.TabIndex = 1;
            this.olvCatalogueItems.UseCompatibleStateImageBehavior = false;
            this.olvCatalogueItems.View = System.Windows.Forms.View.Details;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.MinimumWidth = 100;
            this.olvName.Width = 100;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnApplyTransform);
            this.groupBox2.Controls.Add(this.rbDeleteExtrctionInformation);
            this.groupBox2.Controls.Add(this.rbDeleteAssociatedColumnInfos);
            this.groupBox2.Controls.Add(this.rbDelete);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbTableInfos);
            this.groupBox2.Controls.Add(this.rbGuessNewAssociatedColumns);
            this.groupBox2.Controls.Add(this.cbRecategorise);
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Controls.Add(this.rbMarkExtractable);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(468, 693);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Transform:";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // btnApplyTransform
            // 
            this.btnApplyTransform.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnApplyTransform.Location = new System.Drawing.Point(4, 223);
            this.btnApplyTransform.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnApplyTransform.Name = "btnApplyTransform";
            this.btnApplyTransform.Size = new System.Drawing.Size(460, 27);
            this.btnApplyTransform.TabIndex = 4;
            this.btnApplyTransform.Text = "Apply Transform";
            this.btnApplyTransform.UseVisualStyleBackColor = true;
            this.btnApplyTransform.Click += new System.EventHandler(this.btnApplyTransform_Click);
            // 
            // rbDeleteExtrctionInformation
            // 
            this.rbDeleteExtrctionInformation.AutoSize = true;
            this.rbDeleteExtrctionInformation.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbDeleteExtrctionInformation.Location = new System.Drawing.Point(4, 204);
            this.rbDeleteExtrctionInformation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbDeleteExtrctionInformation.Name = "rbDeleteExtrctionInformation";
            this.rbDeleteExtrctionInformation.Size = new System.Drawing.Size(460, 19);
            this.rbDeleteExtrctionInformation.TabIndex = 3;
            this.rbDeleteExtrctionInformation.TabStop = true;
            this.rbDeleteExtrctionInformation.Text = "Delete Extraction Information";
            this.rbDeleteExtrctionInformation.UseVisualStyleBackColor = true;
            // 
            // rbDeleteAssociatedColumnInfos
            // 
            this.rbDeleteAssociatedColumnInfos.AutoSize = true;
            this.rbDeleteAssociatedColumnInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbDeleteAssociatedColumnInfos.Location = new System.Drawing.Point(4, 185);
            this.rbDeleteAssociatedColumnInfos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbDeleteAssociatedColumnInfos.Name = "rbDeleteAssociatedColumnInfos";
            this.rbDeleteAssociatedColumnInfos.Size = new System.Drawing.Size(460, 19);
            this.rbDeleteAssociatedColumnInfos.TabIndex = 3;
            this.rbDeleteAssociatedColumnInfos.TabStop = true;
            this.rbDeleteAssociatedColumnInfos.Text = "Delete Associated ColumnInfos (will also delete extraction information)";
            this.rbDeleteAssociatedColumnInfos.UseVisualStyleBackColor = true;
            // 
            // rbDelete
            // 
            this.rbDelete.AutoSize = true;
            this.rbDelete.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbDelete.Location = new System.Drawing.Point(4, 166);
            this.rbDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbDelete.Name = "rbDelete";
            this.rbDelete.Size = new System.Drawing.Size(460, 19);
            this.rbDelete.TabIndex = 3;
            this.rbDelete.TabStop = true;
            this.rbDelete.Text = "Delete Catalogue Items";
            this.rbDelete.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(4, 127);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(460, 39);
            this.label4.TabIndex = 1;
            this.label4.Text = "(every matching CatalogueItem will have an associated ColumnInfo guessed from the" +
    " TableInfo you select in the box above)";
            // 
            // cbTableInfos
            // 
            this.cbTableInfos.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbTableInfos.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbTableInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbTableInfos.FormattingEnabled = true;
            this.cbTableInfos.Location = new System.Drawing.Point(4, 104);
            this.cbTableInfos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbTableInfos.Name = "cbTableInfos";
            this.cbTableInfos.Size = new System.Drawing.Size(460, 23);
            this.cbTableInfos.TabIndex = 2;
            this.cbTableInfos.SelectedIndexChanged += new System.EventHandler(this.cbTableInfos_SelectedIndexChanged);
            // 
            // rbGuessNewAssociatedColumns
            // 
            this.rbGuessNewAssociatedColumns.AutoSize = true;
            this.rbGuessNewAssociatedColumns.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbGuessNewAssociatedColumns.Location = new System.Drawing.Point(4, 85);
            this.rbGuessNewAssociatedColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbGuessNewAssociatedColumns.Name = "rbGuessNewAssociatedColumns";
            this.rbGuessNewAssociatedColumns.Size = new System.Drawing.Size(460, 19);
            this.rbGuessNewAssociatedColumns.TabIndex = 0;
            this.rbGuessNewAssociatedColumns.TabStop = true;
            this.rbGuessNewAssociatedColumns.Text = "Guess New Associated Columns";
            this.rbGuessNewAssociatedColumns.UseVisualStyleBackColor = true;
            // 
            // cbRecategorise
            // 
            this.cbRecategorise.AutoSize = true;
            this.cbRecategorise.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbRecategorise.Location = new System.Drawing.Point(4, 66);
            this.cbRecategorise.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbRecategorise.Name = "cbRecategorise";
            this.cbRecategorise.Size = new System.Drawing.Size(460, 19);
            this.cbRecategorise.TabIndex = 7;
            this.cbRecategorise.Text = "Recategorise Matching CatalogueItems (that are already extractable)";
            this.cbRecategorise.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.ddExtractionCategory);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(4, 38);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(460, 28);
            this.panel3.TabIndex = 8;
            // 
            // ddExtractionCategory
            // 
            this.ddExtractionCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ddExtractionCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionCategory.FormattingEnabled = true;
            this.ddExtractionCategory.Location = new System.Drawing.Point(58, 0);
            this.ddExtractionCategory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddExtractionCategory.Name = "ddExtractionCategory";
            this.ddExtractionCategory.Size = new System.Drawing.Size(402, 23);
            this.ddExtractionCategory.TabIndex = 5;
            this.ddExtractionCategory.SelectedIndexChanged += new System.EventHandler(this.ddExtractionCategory_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Category:";
            // 
            // rbMarkExtractable
            // 
            this.rbMarkExtractable.AutoSize = true;
            this.rbMarkExtractable.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbMarkExtractable.Location = new System.Drawing.Point(4, 19);
            this.rbMarkExtractable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbMarkExtractable.Name = "rbMarkExtractable";
            this.rbMarkExtractable.Size = new System.Drawing.Size(460, 19);
            this.rbMarkExtractable.TabIndex = 0;
            this.rbMarkExtractable.TabStop = true;
            this.rbMarkExtractable.Text = "Mark Associated Columns Extractable";
            this.rbMarkExtractable.UseVisualStyleBackColor = true;
            this.rbMarkExtractable.CheckedChanged += new System.EventHandler(this.rbMarkExtractable_CheckedChanged);
            // 
            // BulkProcessCatalogueItemsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "BulkProcessCatalogueItemsUI";
            this.Size = new System.Drawing.Size(1097, 693);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogueItems)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ObjectListView olvCatalogueItems;
        private System.Windows.Forms.RadioButton rbDeleteAssociatedColumnInfos;
        private System.Windows.Forms.RadioButton rbDelete;
        private System.Windows.Forms.ComboBox cbTableInfos;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rbGuessNewAssociatedColumns;
        private System.Windows.Forms.RadioButton rbMarkExtractable;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnApplyTransform;
        private System.Windows.Forms.RadioButton rbDeleteExtrctionInformation;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ddExtractionCategory;
        private System.Windows.Forms.CheckBox cbRecategorise;
        private System.Windows.Forms.ListBox lbPastedColumns;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label3;
        private OLVColumn olvName;
        private System.Windows.Forms.CheckBox cbExactMatch;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}