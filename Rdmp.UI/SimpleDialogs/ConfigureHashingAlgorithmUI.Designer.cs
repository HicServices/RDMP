namespace Rdmp.UI.SimpleDialogs
{
    partial class ConfigureHashingAlgorithmUI
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
            btnReferenceColumn = new System.Windows.Forms.Button();
            btnReferenceSalt = new System.Windows.Forms.Button();
            panel2 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tbHashingAlgorithm = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbHashingType = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btnReferenceColumn
            // 
            btnReferenceColumn.Location = new System.Drawing.Point(410, 63);
            btnReferenceColumn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnReferenceColumn.Name = "btnReferenceColumn";
            btnReferenceColumn.Size = new System.Drawing.Size(184, 27);
            btnReferenceColumn.TabIndex = 1;
            btnReferenceColumn.Text = "Reference Column {0}";
            btnReferenceColumn.UseVisualStyleBackColor = true;
            btnReferenceColumn.Click += btnReferenceColumn_Click;
            // 
            // btnReferenceSalt
            // 
            btnReferenceSalt.Location = new System.Drawing.Point(601, 63);
            btnReferenceSalt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnReferenceSalt.Name = "btnReferenceSalt";
            btnReferenceSalt.Size = new System.Drawing.Size(178, 27);
            btnReferenceSalt.TabIndex = 1;
            btnReferenceSalt.Text = "Reference Salt {1}";
            btnReferenceSalt.UseVisualStyleBackColor = true;
            btnReferenceSalt.Click += btnReferenceSalt_Click;
            // 
            // panel2
            // 
            panel2.Location = new System.Drawing.Point(15, 208);
            panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(764, 84);
            panel2.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 189);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(51, 15);
            label1.TabIndex = 2;
            label1.Text = "Preview:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(15, 78);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 2;
            label2.Text = "Hashing:";
            // 
            // tbHashingAlgorithm
            // 
            tbHashingAlgorithm.Location = new System.Drawing.Point(19, 98);
            tbHashingAlgorithm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbHashingAlgorithm.Name = "tbHashingAlgorithm";
            tbHashingAlgorithm.Size = new System.Drawing.Size(760, 23);
            tbHashingAlgorithm.TabIndex = 3;
            tbHashingAlgorithm.TextChanged += tbHashingAlgorithm_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 295);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(585, 15);
            label3.TabIndex = 2;
            label3.Text = "Example data, referencing a column called [TestColumn] in database [TEST]  from a project with a number 123";
            // 
            // tbHashingType
            // 
            tbHashingType.Location = new System.Drawing.Point(16, 147);
            tbHashingType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbHashingType.Name = "tbHashingType";
            tbHashingType.Size = new System.Drawing.Size(760, 23);
            tbHashingType.TabIndex = 4;
            tbHashingType.TextChanged += tbHashingType_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(16, 129);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(146, 15);
            label4.TabIndex = 5;
            label4.Text = "Hashing SQL Output Type:";
            // 
            // ConfigureHashingAlgorithmUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(793, 316);
            Controls.Add(label4);
            Controls.Add(tbHashingType);
            Controls.Add(tbHashingAlgorithm);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(btnReferenceSalt);
            Controls.Add(btnReferenceColumn);
            Controls.Add(panel2);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ConfigureHashingAlgorithmUI";
            Text = "Configure Hashing Algorithm";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReferenceColumn;
        private System.Windows.Forms.Button btnReferenceSalt;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbHashingAlgorithm;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbHashingType;
        private System.Windows.Forms.Label label4;
    }
}