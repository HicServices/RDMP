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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panel2 = new System.Windows.Forms.Panel();
            cbExactMatch = new System.Windows.Forms.CheckBox();
            btnClear = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            lbPastedColumns = new System.Windows.Forms.ListBox();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            panel1 = new System.Windows.Forms.Panel();
            tbFilter = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            olvCatalogueItems = new ObjectListView();
            olvName = new OLVColumn();
            groupBox2 = new System.Windows.Forms.GroupBox();
            btnApplyTransform = new System.Windows.Forms.Button();
            rbDeleteExtrctionInformation = new System.Windows.Forms.RadioButton();
            rbDeleteAssociatedColumnInfos = new System.Windows.Forms.RadioButton();
            rbDelete = new System.Windows.Forms.RadioButton();
            label4 = new System.Windows.Forms.Label();
            cbTableInfos = new System.Windows.Forms.ComboBox();
            rbGuessNewAssociatedColumns = new System.Windows.Forms.RadioButton();
            cbRecategorise = new System.Windows.Forms.CheckBox();
            panel3 = new System.Windows.Forms.Panel();
            ddExtractionCategory = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            rbMarkExtractable = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)olvCatalogueItems).BeginInit();
            groupBox2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Top;
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(117, 15);
            label1.TabIndex = 0;
            label1.Text = "Paste Columns Here:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = System.Windows.Forms.DockStyle.Top;
            label2.Location = new System.Drawing.Point(0, 0);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(135, 15);
            label2.TabIndex = 1;
            label2.Text = "CatalogueItemsAffected";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel2);
            splitContainer1.Panel1.Controls.Add(checkBox1);
            splitContainer1.Panel1.Controls.Add(lbPastedColumns);
            splitContainer1.Panel1.Controls.Add(label1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(1097, 693);
            splitContainer1.SplitterDistance = 294;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Controls.Add(cbExactMatch);
            panel2.Controls.Add(btnClear);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(0, 661);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(294, 32);
            panel2.TabIndex = 8;
            // 
            // cbExactMatch
            // 
            cbExactMatch.AutoSize = true;
            cbExactMatch.Checked = true;
            cbExactMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            cbExactMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            cbExactMatch.Location = new System.Drawing.Point(88, 0);
            cbExactMatch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbExactMatch.Name = "cbExactMatch";
            cbExactMatch.Size = new System.Drawing.Size(206, 32);
            cbExactMatch.TabIndex = 4;
            cbExactMatch.Text = "Exact Matches Only";
            cbExactMatch.UseVisualStyleBackColor = true;
            cbExactMatch.CheckedChanged += cbExactMatch_CheckedChanged;
            // 
            // btnClear
            // 
            btnClear.Dock = System.Windows.Forms.DockStyle.Left;
            btnClear.Location = new System.Drawing.Point(0, 0);
            btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(88, 32);
            btnClear.TabIndex = 2;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(-18, -17);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(83, 19);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // lbPastedColumns
            // 
            lbPastedColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            lbPastedColumns.FormattingEnabled = true;
            lbPastedColumns.ItemHeight = 15;
            lbPastedColumns.Location = new System.Drawing.Point(0, 15);
            lbPastedColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lbPastedColumns.Name = "lbPastedColumns";
            lbPastedColumns.Size = new System.Drawing.Size(294, 678);
            lbPastedColumns.TabIndex = 1;
            lbPastedColumns.KeyUp += lbPastedColumns_KeyUp;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(panel1);
            splitContainer2.Panel1.Controls.Add(olvCatalogueItems);
            splitContainer2.Panel1.Controls.Add(label2);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBox2);
            splitContainer2.Size = new System.Drawing.Size(798, 693);
            splitContainer2.SplitterDistance = 325;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbFilter);
            panel1.Controls.Add(label3);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 661);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(325, 32);
            panel1.TabIndex = 8;
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            tbFilter.Location = new System.Drawing.Point(36, 0);
            tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(289, 23);
            tbFilter.TabIndex = 3;
            tbFilter.TextChanged += tbFilter_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Left;
            label3.Location = new System.Drawing.Point(0, 0);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(36, 15);
            label3.TabIndex = 2;
            label3.Text = "Filter:";
            // 
            // olvCatalogueItems
            // 
            olvCatalogueItems.AllColumns.Add(olvName);
            olvCatalogueItems.CellEditUseWholeCell = false;
            olvCatalogueItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName });
            olvCatalogueItems.Dock = System.Windows.Forms.DockStyle.Fill;
            olvCatalogueItems.Location = new System.Drawing.Point(0, 15);
            olvCatalogueItems.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            olvCatalogueItems.Name = "olvCatalogueItems";
            olvCatalogueItems.RowHeight = 19;
            olvCatalogueItems.ShowGroups = false;
            olvCatalogueItems.Size = new System.Drawing.Size(325, 678);
            olvCatalogueItems.TabIndex = 1;
            olvCatalogueItems.UseCompatibleStateImageBehavior = false;
            olvCatalogueItems.View = System.Windows.Forms.View.Details;
            olvCatalogueItems.KeyUp += olvCatalogueItems_KeyUp;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.FillsFreeSpace = true;
            olvName.MinimumWidth = 100;
            olvName.Width = 100;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnApplyTransform);
            groupBox2.Controls.Add(rbDeleteExtrctionInformation);
            groupBox2.Controls.Add(rbDeleteAssociatedColumnInfos);
            groupBox2.Controls.Add(rbDelete);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(cbTableInfos);
            groupBox2.Controls.Add(rbGuessNewAssociatedColumns);
            groupBox2.Controls.Add(cbRecategorise);
            groupBox2.Controls.Add(panel3);
            groupBox2.Controls.Add(rbMarkExtractable);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox2.Location = new System.Drawing.Point(0, 0);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(468, 693);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Transform:";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // btnApplyTransform
            // 
            btnApplyTransform.Dock = System.Windows.Forms.DockStyle.Top;
            btnApplyTransform.Location = new System.Drawing.Point(4, 223);
            btnApplyTransform.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnApplyTransform.Name = "btnApplyTransform";
            btnApplyTransform.Size = new System.Drawing.Size(460, 27);
            btnApplyTransform.TabIndex = 4;
            btnApplyTransform.Text = "Apply Transform";
            btnApplyTransform.UseVisualStyleBackColor = true;
            btnApplyTransform.Click += btnApplyTransform_Click;
            // 
            // rbDeleteExtrctionInformation
            // 
            rbDeleteExtrctionInformation.AutoSize = true;
            rbDeleteExtrctionInformation.Dock = System.Windows.Forms.DockStyle.Top;
            rbDeleteExtrctionInformation.Location = new System.Drawing.Point(4, 204);
            rbDeleteExtrctionInformation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbDeleteExtrctionInformation.Name = "rbDeleteExtrctionInformation";
            rbDeleteExtrctionInformation.Size = new System.Drawing.Size(460, 19);
            rbDeleteExtrctionInformation.TabIndex = 3;
            rbDeleteExtrctionInformation.TabStop = true;
            rbDeleteExtrctionInformation.Text = "Delete Extraction Information";
            rbDeleteExtrctionInformation.UseVisualStyleBackColor = true;
            // 
            // rbDeleteAssociatedColumnInfos
            // 
            rbDeleteAssociatedColumnInfos.AutoSize = true;
            rbDeleteAssociatedColumnInfos.Dock = System.Windows.Forms.DockStyle.Top;
            rbDeleteAssociatedColumnInfos.Location = new System.Drawing.Point(4, 185);
            rbDeleteAssociatedColumnInfos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbDeleteAssociatedColumnInfos.Name = "rbDeleteAssociatedColumnInfos";
            rbDeleteAssociatedColumnInfos.Size = new System.Drawing.Size(460, 19);
            rbDeleteAssociatedColumnInfos.TabIndex = 3;
            rbDeleteAssociatedColumnInfos.TabStop = true;
            rbDeleteAssociatedColumnInfos.Text = "Delete Associated ColumnInfos (will also delete extraction information)";
            rbDeleteAssociatedColumnInfos.UseVisualStyleBackColor = true;
            // 
            // rbDelete
            // 
            rbDelete.AutoSize = true;
            rbDelete.Dock = System.Windows.Forms.DockStyle.Top;
            rbDelete.Location = new System.Drawing.Point(4, 166);
            rbDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbDelete.Name = "rbDelete";
            rbDelete.Size = new System.Drawing.Size(460, 19);
            rbDelete.TabIndex = 3;
            rbDelete.TabStop = true;
            rbDelete.Text = "Delete Catalogue Items";
            rbDelete.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Dock = System.Windows.Forms.DockStyle.Top;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
            label4.Location = new System.Drawing.Point(4, 127);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(460, 39);
            label4.TabIndex = 1;
            label4.Text = "(every matching CatalogueItem will have an associated ColumnInfo guessed from the TableInfo you select in the box above)";
            // 
            // cbTableInfos
            // 
            cbTableInfos.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            cbTableInfos.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cbTableInfos.Dock = System.Windows.Forms.DockStyle.Top;
            cbTableInfos.FormattingEnabled = true;
            cbTableInfos.Location = new System.Drawing.Point(4, 104);
            cbTableInfos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbTableInfos.Name = "cbTableInfos";
            cbTableInfos.Size = new System.Drawing.Size(460, 23);
            cbTableInfos.TabIndex = 2;
            cbTableInfos.SelectedIndexChanged += cbTableInfos_SelectedIndexChanged;
            // 
            // rbGuessNewAssociatedColumns
            // 
            rbGuessNewAssociatedColumns.AutoSize = true;
            rbGuessNewAssociatedColumns.Dock = System.Windows.Forms.DockStyle.Top;
            rbGuessNewAssociatedColumns.Location = new System.Drawing.Point(4, 85);
            rbGuessNewAssociatedColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbGuessNewAssociatedColumns.Name = "rbGuessNewAssociatedColumns";
            rbGuessNewAssociatedColumns.Size = new System.Drawing.Size(460, 19);
            rbGuessNewAssociatedColumns.TabIndex = 0;
            rbGuessNewAssociatedColumns.TabStop = true;
            rbGuessNewAssociatedColumns.Text = "Guess New Associated Columns";
            rbGuessNewAssociatedColumns.UseVisualStyleBackColor = true;
            // 
            // cbRecategorise
            // 
            cbRecategorise.AutoSize = true;
            cbRecategorise.Dock = System.Windows.Forms.DockStyle.Top;
            cbRecategorise.Location = new System.Drawing.Point(4, 66);
            cbRecategorise.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbRecategorise.Name = "cbRecategorise";
            cbRecategorise.Size = new System.Drawing.Size(460, 19);
            cbRecategorise.TabIndex = 7;
            cbRecategorise.Text = "Recategorise Matching CatalogueItems (that are already extractable)";
            cbRecategorise.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Controls.Add(ddExtractionCategory);
            panel3.Controls.Add(label5);
            panel3.Dock = System.Windows.Forms.DockStyle.Top;
            panel3.Location = new System.Drawing.Point(4, 38);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(460, 28);
            panel3.TabIndex = 8;
            // 
            // ddExtractionCategory
            // 
            ddExtractionCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            ddExtractionCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddExtractionCategory.FormattingEnabled = true;
            ddExtractionCategory.Location = new System.Drawing.Point(58, 0);
            ddExtractionCategory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ddExtractionCategory.Name = "ddExtractionCategory";
            ddExtractionCategory.Size = new System.Drawing.Size(402, 23);
            ddExtractionCategory.TabIndex = 5;
            ddExtractionCategory.SelectedIndexChanged += ddExtractionCategory_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = System.Windows.Forms.DockStyle.Left;
            label5.Location = new System.Drawing.Point(0, 0);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(58, 15);
            label5.TabIndex = 6;
            label5.Text = "Category:";
            // 
            // rbMarkExtractable
            // 
            rbMarkExtractable.AutoSize = true;
            rbMarkExtractable.Dock = System.Windows.Forms.DockStyle.Top;
            rbMarkExtractable.Location = new System.Drawing.Point(4, 19);
            rbMarkExtractable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbMarkExtractable.Name = "rbMarkExtractable";
            rbMarkExtractable.Size = new System.Drawing.Size(460, 19);
            rbMarkExtractable.TabIndex = 0;
            rbMarkExtractable.TabStop = true;
            rbMarkExtractable.Text = "Mark Associated Columns Extractable";
            rbMarkExtractable.UseVisualStyleBackColor = true;
            rbMarkExtractable.CheckedChanged += rbMarkExtractable_CheckedChanged;
            // 
            // BulkProcessCatalogueItemsUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "BulkProcessCatalogueItemsUI";
            Size = new System.Drawing.Size(1097, 693);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)olvCatalogueItems).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
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