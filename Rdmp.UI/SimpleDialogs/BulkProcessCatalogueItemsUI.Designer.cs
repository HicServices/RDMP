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
            this.cbExactMatch = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.lbPastedColumns = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.olvCatalogueItems = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbRecategorise = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ddExtractionCategory = new System.Windows.Forms.ComboBox();
            this.btnApplyTransform = new System.Windows.Forms.Button();
            this.rbMarkExtractable = new System.Windows.Forms.RadioButton();
            this.rbGuessNewAssociatedColumns = new System.Windows.Forms.RadioButton();
            this.rbDeleteExtrctionInformation = new System.Windows.Forms.RadioButton();
            this.rbDeleteAssociatedColumnInfos = new System.Windows.Forms.RadioButton();
            this.rbDelete = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.cbTableInfos = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogueItems)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Paste Columns Here:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "CatalogueItemsAffected";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cbExactMatch);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.btnClear);
            this.splitContainer1.Panel1.Controls.Add(this.lbPastedColumns);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(940, 601);
            this.splitContainer1.SplitterDistance = 252;
            this.splitContainer1.TabIndex = 2;
            // 
            // cbExactMatch
            // 
            this.cbExactMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbExactMatch.AutoSize = true;
            this.cbExactMatch.Checked = true;
            this.cbExactMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbExactMatch.Location = new System.Drawing.Point(128, 580);
            this.cbExactMatch.Name = "cbExactMatch";
            this.cbExactMatch.Size = new System.Drawing.Size(121, 17);
            this.cbExactMatch.TabIndex = 4;
            this.cbExactMatch.Text = "Exact Matches Only";
            this.cbExactMatch.UseVisualStyleBackColor = true;
            this.cbExactMatch.CheckedChanged += new System.EventHandler(this.cbExactMatch_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(-15, -15);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(6, 575);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lbPastedColumns
            // 
            this.lbPastedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPastedColumns.FormattingEnabled = true;
            this.lbPastedColumns.Location = new System.Drawing.Point(6, 25);
            this.lbPastedColumns.Name = "lbPastedColumns";
            this.lbPastedColumns.Size = new System.Drawing.Size(243, 550);
            this.lbPastedColumns.TabIndex = 1;
            this.lbPastedColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbPastedColumns_KeyUp);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.olvCatalogueItems);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(684, 601);
            this.splitContainer2.SplitterDistance = 279;
            this.splitContainer2.TabIndex = 3;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(43, 578);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(233, 20);
            this.tbFilter.TabIndex = 3;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 581);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Filter:";
            // 
            // olvCatalogueItems
            // 
            this.olvCatalogueItems.AllColumns.Add(this.olvName);
            this.olvCatalogueItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCatalogueItems.CellEditUseWholeCell = false;
            this.olvCatalogueItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvCatalogueItems.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvCatalogueItems.HideSelection = false;
            this.olvCatalogueItems.Location = new System.Drawing.Point(6, 25);
            this.olvCatalogueItems.Name = "olvCatalogueItems";
            this.olvCatalogueItems.RowHeight = 19;
            this.olvCatalogueItems.ShowGroups = false;
            this.olvCatalogueItems.Size = new System.Drawing.Size(270, 550);
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
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.cbRecategorise);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.ddExtractionCategory);
            this.groupBox2.Controls.Add(this.btnApplyTransform);
            this.groupBox2.Controls.Add(this.rbMarkExtractable);
            this.groupBox2.Controls.Add(this.rbGuessNewAssociatedColumns);
            this.groupBox2.Controls.Add(this.rbDeleteExtrctionInformation);
            this.groupBox2.Controls.Add(this.rbDeleteAssociatedColumnInfos);
            this.groupBox2.Controls.Add(this.rbDelete);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbTableInfos);
            this.groupBox2.Location = new System.Drawing.Point(17, 9);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(372, 579);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Transform:";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // cbRecategorise
            // 
            this.cbRecategorise.AutoSize = true;
            this.cbRecategorise.Location = new System.Drawing.Point(17, 70);
            this.cbRecategorise.Name = "cbRecategorise";
            this.cbRecategorise.Size = new System.Drawing.Size(349, 17);
            this.cbRecategorise.TabIndex = 7;
            this.cbRecategorise.Text = "Recategorise Matching CatalogueItems (that are already extractable)";
            this.cbRecategorise.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Category:";
            // 
            // ddExtractionCategory
            // 
            this.ddExtractionCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionCategory.FormattingEnabled = true;
            this.ddExtractionCategory.Location = new System.Drawing.Point(66, 43);
            this.ddExtractionCategory.Name = "ddExtractionCategory";
            this.ddExtractionCategory.Size = new System.Drawing.Size(281, 21);
            this.ddExtractionCategory.TabIndex = 5;
            this.ddExtractionCategory.SelectedIndexChanged += new System.EventHandler(this.ddExtractionCategory_SelectedIndexChanged);
            // 
            // btnApplyTransform
            // 
            this.btnApplyTransform.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplyTransform.Location = new System.Drawing.Point(139, 251);
            this.btnApplyTransform.Name = "btnApplyTransform";
            this.btnApplyTransform.Size = new System.Drawing.Size(97, 23);
            this.btnApplyTransform.TabIndex = 4;
            this.btnApplyTransform.Text = "Apply Transform";
            this.btnApplyTransform.UseVisualStyleBackColor = true;
            this.btnApplyTransform.Click += new System.EventHandler(this.btnApplyTransform_Click);
            // 
            // rbMarkExtractable
            // 
            this.rbMarkExtractable.AutoSize = true;
            this.rbMarkExtractable.Location = new System.Drawing.Point(6, 19);
            this.rbMarkExtractable.Name = "rbMarkExtractable";
            this.rbMarkExtractable.Size = new System.Drawing.Size(203, 17);
            this.rbMarkExtractable.TabIndex = 0;
            this.rbMarkExtractable.TabStop = true;
            this.rbMarkExtractable.Text = "Mark Associated Columns Extractable";
            this.rbMarkExtractable.UseVisualStyleBackColor = true;
            this.rbMarkExtractable.CheckedChanged += new System.EventHandler(this.rbMarkExtractable_CheckedChanged);
            // 
            // rbGuessNewAssociatedColumns
            // 
            this.rbGuessNewAssociatedColumns.AutoSize = true;
            this.rbGuessNewAssociatedColumns.Location = new System.Drawing.Point(6, 93);
            this.rbGuessNewAssociatedColumns.Name = "rbGuessNewAssociatedColumns";
            this.rbGuessNewAssociatedColumns.Size = new System.Drawing.Size(178, 17);
            this.rbGuessNewAssociatedColumns.TabIndex = 0;
            this.rbGuessNewAssociatedColumns.TabStop = true;
            this.rbGuessNewAssociatedColumns.Text = "Guess New Associated Columns";
            this.rbGuessNewAssociatedColumns.UseVisualStyleBackColor = true;
            // 
            // rbDeleteExtrctionInformation
            // 
            this.rbDeleteExtrctionInformation.AutoSize = true;
            this.rbDeleteExtrctionInformation.Location = new System.Drawing.Point(6, 205);
            this.rbDeleteExtrctionInformation.Name = "rbDeleteExtrctionInformation";
            this.rbDeleteExtrctionInformation.Size = new System.Drawing.Size(161, 17);
            this.rbDeleteExtrctionInformation.TabIndex = 3;
            this.rbDeleteExtrctionInformation.TabStop = true;
            this.rbDeleteExtrctionInformation.Text = "Delete Extraction Information";
            this.rbDeleteExtrctionInformation.UseVisualStyleBackColor = true;
            // 
            // rbDeleteAssociatedColumnInfos
            // 
            this.rbDeleteAssociatedColumnInfos.AutoSize = true;
            this.rbDeleteAssociatedColumnInfos.Location = new System.Drawing.Point(6, 228);
            this.rbDeleteAssociatedColumnInfos.Name = "rbDeleteAssociatedColumnInfos";
            this.rbDeleteAssociatedColumnInfos.Size = new System.Drawing.Size(352, 17);
            this.rbDeleteAssociatedColumnInfos.TabIndex = 3;
            this.rbDeleteAssociatedColumnInfos.TabStop = true;
            this.rbDeleteAssociatedColumnInfos.Text = "Delete Associated ColumnInfos (will also delete extraction information)";
            this.rbDeleteAssociatedColumnInfos.UseVisualStyleBackColor = true;
            // 
            // rbDelete
            // 
            this.rbDelete.AutoSize = true;
            this.rbDelete.Location = new System.Drawing.Point(6, 182);
            this.rbDelete.Name = "rbDelete";
            this.rbDelete.Size = new System.Drawing.Size(135, 17);
            this.rbDelete.TabIndex = 3;
            this.rbDelete.TabStop = true;
            this.rbDelete.Text = "Delete Catalogue Items";
            this.rbDelete.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(28, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(337, 34);
            this.label4.TabIndex = 1;
            this.label4.Text = "(every matching CatalogueItem will have an associated ColumnInfo guessed from the" +
    " TableInfo you select in the box above)";
            // 
            // cbTableInfos
            // 
            this.cbTableInfos.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbTableInfos.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbTableInfos.FormattingEnabled = true;
            this.cbTableInfos.Location = new System.Drawing.Point(31, 117);
            this.cbTableInfos.Name = "cbTableInfos";
            this.cbTableInfos.Size = new System.Drawing.Size(315, 21);
            this.cbTableInfos.TabIndex = 2;
            this.cbTableInfos.SelectedIndexChanged += new System.EventHandler(this.cbTableInfos_SelectedIndexChanged);
            // 
            // BulkProcessCatalogueItemsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "BulkProcessCatalogueItemsUI";
            this.Size = new System.Drawing.Size(940, 601);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogueItems)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
    }
}