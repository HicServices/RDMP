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
            this.label1 = new System.Windows.Forms.Label();
            this.ddExtractionCategory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDefaultOrder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbAlias = new System.Windows.Forms.TextBox();
            this.cbHashOnDataRelease = new System.Windows.Forms.CheckBox();
            this.cbIsExtractionIdentifier = new System.Windows.Forms.CheckBox();
            this.cbIsPrimaryKey = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pSql = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.lblFromTable = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbId = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Category:";
            // 
            // ddExtractionCategory
            // 
            this.ddExtractionCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionCategory.FormattingEnabled = true;
            this.ddExtractionCategory.Location = new System.Drawing.Point(60, 28);
            this.ddExtractionCategory.Name = "ddExtractionCategory";
            this.ddExtractionCategory.Size = new System.Drawing.Size(152, 21);
            this.ddExtractionCategory.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Default Order:";
            // 
            // tbDefaultOrder
            // 
            this.tbDefaultOrder.Location = new System.Drawing.Point(226, 3);
            this.tbDefaultOrder.Name = "tbDefaultOrder";
            this.tbDefaultOrder.ReadOnly = true;
            this.tbDefaultOrder.Size = new System.Drawing.Size(106, 20);
            this.tbDefaultOrder.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(338, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Alias:";
            // 
            // tbAlias
            // 
            this.tbAlias.Location = new System.Drawing.Point(376, 3);
            this.tbAlias.Name = "tbAlias";
            this.tbAlias.ReadOnly = true;
            this.tbAlias.Size = new System.Drawing.Size(147, 20);
            this.tbAlias.TabIndex = 4;
            // 
            // cbHashOnDataRelease
            // 
            this.cbHashOnDataRelease.AutoSize = true;
            this.cbHashOnDataRelease.Location = new System.Drawing.Point(218, 32);
            this.cbHashOnDataRelease.Name = "cbHashOnDataRelease";
            this.cbHashOnDataRelease.Size = new System.Drawing.Size(134, 17);
            this.cbHashOnDataRelease.TabIndex = 5;
            this.cbHashOnDataRelease.Text = "Hash on Data Release";
            this.cbHashOnDataRelease.UseVisualStyleBackColor = true;
            this.cbHashOnDataRelease.CheckedChanged += new System.EventHandler(this.cbHashOnDataRelease_CheckedChanged);
            // 
            // cbIsExtractionIdentifier
            // 
            this.cbIsExtractionIdentifier.AutoSize = true;
            this.cbIsExtractionIdentifier.Location = new System.Drawing.Point(358, 32);
            this.cbIsExtractionIdentifier.Name = "cbIsExtractionIdentifier";
            this.cbIsExtractionIdentifier.Size = new System.Drawing.Size(127, 17);
            this.cbIsExtractionIdentifier.TabIndex = 6;
            this.cbIsExtractionIdentifier.Text = "Is Extraction Identifier";
            this.cbIsExtractionIdentifier.UseVisualStyleBackColor = true;
            // 
            // cbIsPrimaryKey
            // 
            this.cbIsPrimaryKey.AutoSize = true;
            this.cbIsPrimaryKey.Location = new System.Drawing.Point(491, 32);
            this.cbIsPrimaryKey.Name = "cbIsPrimaryKey";
            this.cbIsPrimaryKey.Size = new System.Drawing.Size(92, 17);
            this.cbIsPrimaryKey.TabIndex = 7;
            this.cbIsPrimaryKey.Text = "Is Primary Key";
            this.cbIsPrimaryKey.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Blue;
            this.label4.Location = new System.Drawing.Point(3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "SELECT";
            // 
            // pSql
            // 
            this.pSql.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSql.Location = new System.Drawing.Point(51, 22);
            this.pSql.Name = "pSql";
            this.pSql.Size = new System.Drawing.Size(1073, 491);
            this.pSql.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(4, 516);
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
            this.lblFromTable.Location = new System.Drawing.Point(52, 516);
            this.lblFromTable.Name = "lblFromTable";
            this.lblFromTable.Size = new System.Drawing.Size(1072, 18);
            this.lblFromTable.TabIndex = 3;
            this.lblFromTable.Text = "Your Header Goes Here";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "ID:";
            // 
            // tbId
            // 
            this.tbId.Location = new System.Drawing.Point(60, 3);
            this.tbId.Name = "tbId";
            this.tbId.ReadOnly = true;
            this.tbId.Size = new System.Drawing.Size(81, 20);
            this.tbId.TabIndex = 13;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tbId);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.tbDefaultOrder);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.cbIsPrimaryKey);
            this.panel2.Controls.Add(this.ddExtractionCategory);
            this.panel2.Controls.Add(this.cbIsExtractionIdentifier);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.cbHashOnDataRelease);
            this.panel2.Controls.Add(this.tbAlias);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1127, 62);
            this.panel2.TabIndex = 14;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.lblFromTable);
            this.panel3.Controls.Add(this.pSql);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 62);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1127, 536);
            this.panel3.TabIndex = 15;
            // 
            // ExtractionInformationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Name = "ExtractionInformationUI";
            this.Size = new System.Drawing.Size(1127, 598);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddExtractionCategory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDefaultOrder;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbHashOnDataRelease;
        private System.Windows.Forms.CheckBox cbIsExtractionIdentifier;
        private System.Windows.Forms.CheckBox cbIsPrimaryKey;
        
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pSql;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblFromTable;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbId;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}