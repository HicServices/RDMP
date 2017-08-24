using BrightIdeasSoftware;
using CatalogueManager.ExtractionUIs.FilterUIs;

namespace CatalogueManager.ExtractionUIs
{
    partial class ExtractionInformationUI
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
            this.gbIsDirectlyExtractable = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.ddExtractionCategory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDefaultOrder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbAlias = new System.Windows.Forms.TextBox();
            this.cbHashOnDataRelease = new System.Windows.Forms.CheckBox();
            this.cbIsExtractionIdentifier = new System.Windows.Forms.CheckBox();
            this.cbIsPrimaryKey = new System.Windows.Forms.CheckBox();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.label4 = new System.Windows.Forms.Label();
            this.pSql = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.lblFromTable = new System.Windows.Forms.Label();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.gbIsDirectlyExtractable.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbIsDirectlyExtractable
            // 
            this.gbIsDirectlyExtractable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbIsDirectlyExtractable.Controls.Add(this.flowLayoutPanel1);
            this.gbIsDirectlyExtractable.Location = new System.Drawing.Point(14, 3);
            this.gbIsDirectlyExtractable.Name = "gbIsDirectlyExtractable";
            this.gbIsDirectlyExtractable.Size = new System.Drawing.Size(1100, 80);
            this.gbIsDirectlyExtractable.TabIndex = 2;
            this.gbIsDirectlyExtractable.TabStop = false;
            this.gbIsDirectlyExtractable.Text = "Extraction Flags";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.ddExtractionCategory);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.tbDefaultOrder);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.tbAlias);
            this.flowLayoutPanel1.Controls.Add(this.cbHashOnDataRelease);
            this.flowLayoutPanel1.Controls.Add(this.cbIsExtractionIdentifier);
            this.flowLayoutPanel1.Controls.Add(this.cbIsPrimaryKey);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1094, 61);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Extraction Category:";
            // 
            // ddExtractionCategory
            // 
            this.ddExtractionCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionCategory.FormattingEnabled = true;
            this.ddExtractionCategory.Location = new System.Drawing.Point(111, 3);
            this.ddExtractionCategory.Name = "ddExtractionCategory";
            this.ddExtractionCategory.Size = new System.Drawing.Size(152, 21);
            this.ddExtractionCategory.TabIndex = 2;
            this.ddExtractionCategory.SelectedIndexChanged += new System.EventHandler(this.ddExtractionCategory_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(269, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Default Order:";
            // 
            // tbDefaultOrder
            // 
            this.tbDefaultOrder.Location = new System.Drawing.Point(348, 3);
            this.tbDefaultOrder.Name = "tbDefaultOrder";
            this.tbDefaultOrder.ReadOnly = true;
            this.tbDefaultOrder.Size = new System.Drawing.Size(106, 20);
            this.tbDefaultOrder.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(460, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Alias:";
            // 
            // tbAlias
            // 
            this.tbAlias.Location = new System.Drawing.Point(498, 3);
            this.tbAlias.Name = "tbAlias";
            this.tbAlias.ReadOnly = true;
            this.tbAlias.Size = new System.Drawing.Size(147, 20);
            this.tbAlias.TabIndex = 10;
            // 
            // cbHashOnDataRelease
            // 
            this.cbHashOnDataRelease.AutoSize = true;
            this.cbHashOnDataRelease.Location = new System.Drawing.Point(651, 3);
            this.cbHashOnDataRelease.Name = "cbHashOnDataRelease";
            this.cbHashOnDataRelease.Size = new System.Drawing.Size(134, 17);
            this.cbHashOnDataRelease.TabIndex = 11;
            this.cbHashOnDataRelease.Text = "Hash on Data Release";
            this.cbHashOnDataRelease.UseVisualStyleBackColor = true;
            this.cbHashOnDataRelease.CheckedChanged += new System.EventHandler(this.cbHashOnDataRelease_CheckedChanged);
            // 
            // cbIsExtractionIdentifier
            // 
            this.cbIsExtractionIdentifier.AutoSize = true;
            this.cbIsExtractionIdentifier.Location = new System.Drawing.Point(791, 3);
            this.cbIsExtractionIdentifier.Name = "cbIsExtractionIdentifier";
            this.cbIsExtractionIdentifier.Size = new System.Drawing.Size(127, 17);
            this.cbIsExtractionIdentifier.TabIndex = 12;
            this.cbIsExtractionIdentifier.Text = "Is Extraction Identifier";
            this.cbIsExtractionIdentifier.UseVisualStyleBackColor = true;
            this.cbIsExtractionIdentifier.CheckedChanged += new System.EventHandler(this.cbIsExtractionIdentifier_CheckedChanged);
            // 
            // cbIsPrimaryKey
            // 
            this.cbIsPrimaryKey.AutoSize = true;
            this.cbIsPrimaryKey.Location = new System.Drawing.Point(924, 3);
            this.cbIsPrimaryKey.Name = "cbIsPrimaryKey";
            this.cbIsPrimaryKey.Size = new System.Drawing.Size(92, 17);
            this.cbIsPrimaryKey.TabIndex = 13;
            this.cbIsPrimaryKey.Text = "Is Primary Key";
            this.cbIsPrimaryKey.UseVisualStyleBackColor = true;
            this.cbIsPrimaryKey.CheckedChanged += new System.EventHandler(this.cbIsPrimaryKey_CheckedChanged);
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(17, 86);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 4;
            this.objectSaverButton1.Text = "objectSaverButton1";
            this.objectSaverButton1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Blue;
            this.label4.Location = new System.Drawing.Point(20, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "SELECT";
            // 
            // pSql
            // 
            this.pSql.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSql.Location = new System.Drawing.Point(43, 149);
            this.pSql.Name = "pSql";
            this.pSql.Size = new System.Drawing.Size(1068, 326);
            this.pSql.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(20, 487);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "FROM";
            // 
            // lblFromTable
            // 
            this.lblFromTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFromTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFromTable.Location = new System.Drawing.Point(68, 487);
            this.lblFromTable.Name = "lblFromTable";
            this.lblFromTable.Size = new System.Drawing.Size(1043, 18);
            this.lblFromTable.TabIndex = 10;
            this.lblFromTable.Text = "Your Header Goes Here";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(3, 149);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(37, 38);
            this.ragSmiley1.TabIndex = 11;
            // 
            // ExtractionInformationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.lblFromTable);
            this.Controls.Add(this.pSql);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.gbIsDirectlyExtractable);
            this.Name = "ExtractionInformationUI";
            this.Size = new System.Drawing.Size(1127, 546);
            this.gbIsDirectlyExtractable.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbIsDirectlyExtractable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddExtractionCategory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDefaultOrder;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbHashOnDataRelease;
        private System.Windows.Forms.CheckBox cbIsExtractionIdentifier;
        private System.Windows.Forms.CheckBox cbIsPrimaryKey;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pSql;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblFromTable;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
    }
}