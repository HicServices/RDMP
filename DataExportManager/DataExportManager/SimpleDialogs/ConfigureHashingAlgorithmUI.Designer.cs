namespace DataExportManager.SimpleDialogs
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
            this.btnReferenceColumn = new System.Windows.Forms.Button();
            this.btnReferenceSalt = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbHashingAlgorithm = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnReferenceColumn
            // 
            this.btnReferenceColumn.Location = new System.Drawing.Point(351, 55);
            this.btnReferenceColumn.Name = "btnReferenceColumn";
            this.btnReferenceColumn.Size = new System.Drawing.Size(158, 23);
            this.btnReferenceColumn.TabIndex = 1;
            this.btnReferenceColumn.Text = "Reference Column {0}";
            this.btnReferenceColumn.UseVisualStyleBackColor = true;
            this.btnReferenceColumn.Click += new System.EventHandler(this.btnReferenceColumn_Click);
            // 
            // btnReferenceSalt
            // 
            this.btnReferenceSalt.Location = new System.Drawing.Point(515, 55);
            this.btnReferenceSalt.Name = "btnReferenceSalt";
            this.btnReferenceSalt.Size = new System.Drawing.Size(153, 23);
            this.btnReferenceSalt.TabIndex = 1;
            this.btnReferenceSalt.Text = "Reference Salt {1}";
            this.btnReferenceSalt.UseVisualStyleBackColor = true;
            this.btnReferenceSalt.Click += new System.EventHandler(this.btnReferenceSalt_Click);
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(13, 180);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(655, 73);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Preview:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hashing:";
            // 
            // tbHashingAlgorithm
            // 
            this.tbHashingAlgorithm.Location = new System.Drawing.Point(16, 85);
            this.tbHashingAlgorithm.Name = "tbHashingAlgorithm";
            this.tbHashingAlgorithm.Size = new System.Drawing.Size(652, 20);
            this.tbHashingAlgorithm.TabIndex = 3;
            this.tbHashingAlgorithm.TextChanged += new System.EventHandler(this.tbHashingAlgorithm_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 256);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(527, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Example data, referencing a column called [TestColumn] in database [TEST]  from a" +
    " project with a number 123";
            // 
            // ConfigureHashingAlgorithm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 274);
            this.Controls.Add(this.tbHashingAlgorithm);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReferenceSalt);
            this.Controls.Add(this.btnReferenceColumn);
            this.Controls.Add(this.panel2);
            this.Name = "ConfigureHashingAlgorithm";
            this.Text = "ConfigureHashingAlgorithm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReferenceColumn;
        private System.Windows.Forms.Button btnReferenceSalt;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbHashingAlgorithm;
        private System.Windows.Forms.Label label3;
    }
}