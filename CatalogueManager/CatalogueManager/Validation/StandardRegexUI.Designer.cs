namespace CatalogueManager.Validation
{
    partial class StandardRegexUI
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
            this.ddConcepts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.gbStandardRegex = new System.Windows.Forms.GroupBox();
            this.gbTesting = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lblResultOfTest = new System.Windows.Forms.Label();
            this.tbTesting = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbRegex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbConceptName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gbStandardRegex.SuspendLayout();
            this.gbTesting.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddConcepts
            // 
            this.ddConcepts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddConcepts.FormattingEnabled = true;
            this.ddConcepts.Location = new System.Drawing.Point(72, 12);
            this.ddConcepts.Name = "ddConcepts";
            this.ddConcepts.Size = new System.Drawing.Size(369, 21);
            this.ddConcepts.Sorted = true;
            this.ddConcepts.TabIndex = 0;
            this.ddConcepts.SelectedIndexChanged += new System.EventHandler(this.ddConcepts_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Concept:";
            // 
            // btnAddNew
            // 
            this.btnAddNew.Location = new System.Drawing.Point(447, 10);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(75, 23);
            this.btnAddNew.TabIndex = 1;
            this.btnAddNew.Text = "Add New";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btnAddNew_Click);
            // 
            // gbStandardRegex
            // 
            this.gbStandardRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbStandardRegex.Controls.Add(this.gbTesting);
            this.gbStandardRegex.Controls.Add(this.label6);
            this.gbStandardRegex.Controls.Add(this.btnDelete);
            this.gbStandardRegex.Controls.Add(this.btnSave);
            this.gbStandardRegex.Controls.Add(this.tbDescription);
            this.gbStandardRegex.Controls.Add(this.label5);
            this.gbStandardRegex.Controls.Add(this.tbRegex);
            this.gbStandardRegex.Controls.Add(this.label4);
            this.gbStandardRegex.Controls.Add(this.tbConceptName);
            this.gbStandardRegex.Controls.Add(this.label3);
            this.gbStandardRegex.Controls.Add(this.tbID);
            this.gbStandardRegex.Controls.Add(this.label2);
            this.gbStandardRegex.Enabled = false;
            this.gbStandardRegex.Location = new System.Drawing.Point(19, 39);
            this.gbStandardRegex.Name = "gbStandardRegex";
            this.gbStandardRegex.Size = new System.Drawing.Size(978, 570);
            this.gbStandardRegex.TabIndex = 3;
            this.gbStandardRegex.TabStop = false;
            this.gbStandardRegex.Text = "Edit";
            // 
            // gbTesting
            // 
            this.gbTesting.Controls.Add(this.label7);
            this.gbTesting.Controls.Add(this.lblResultOfTest);
            this.gbTesting.Controls.Add(this.tbTesting);
            this.gbTesting.Controls.Add(this.label8);
            this.gbTesting.Controls.Add(this.btnTest);
            this.gbTesting.Location = new System.Drawing.Point(6, 353);
            this.gbTesting.Name = "gbTesting";
            this.gbTesting.Size = new System.Drawing.Size(966, 211);
            this.gbTesting.TabIndex = 10;
            this.gbTesting.TabStop = false;
            this.gbTesting.Text = "Testing";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Testing Box:";
            // 
            // lblResultOfTest
            // 
            this.lblResultOfTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResultOfTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultOfTest.Location = new System.Drawing.Point(6, 64);
            this.lblResultOfTest.Name = "lblResultOfTest";
            this.lblResultOfTest.Size = new System.Drawing.Size(960, 144);
            this.lblResultOfTest.TabIndex = 9;
            // 
            // tbTesting
            // 
            this.tbTesting.Location = new System.Drawing.Point(92, 18);
            this.tbTesting.Name = "tbTesting";
            this.tbTesting.Size = new System.Drawing.Size(439, 20);
            this.tbTesting.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(89, 41);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(442, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "(Type a value in above and click to test whether it validates with your current R" +
    "egex pattern)";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(537, 16);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 8;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(110, 308);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(798, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "(In addition to explaining what the concept is (e.g. patient identifier for scott" +
    "ish patients), you should also explain the meaning of the regex (e.g. exactly 10" +
    " digits in a row))";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(194, 324);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(113, 324);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(113, 105);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(859, 200);
            this.tbDescription.TabIndex = 3;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(44, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Description:";
            // 
            // tbRegex
            // 
            this.tbRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRegex.Location = new System.Drawing.Point(113, 79);
            this.tbRegex.Name = "tbRegex";
            this.tbRegex.Size = new System.Drawing.Size(859, 20);
            this.tbRegex.TabIndex = 2;
            this.tbRegex.TextChanged += new System.EventHandler(this.tbRegex_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(66, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Regex:";
            // 
            // tbConceptName
            // 
            this.tbConceptName.Location = new System.Drawing.Point(113, 53);
            this.tbConceptName.Name = "tbConceptName";
            this.tbConceptName.Size = new System.Drawing.Size(309, 20);
            this.tbConceptName.TabIndex = 1;
            this.tbConceptName.TextChanged += new System.EventHandler(this.tbConceptName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Concept Name:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(113, 27);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(86, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "ID:";
            // 
            // StandardRegexUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1009, 620);
            this.Controls.Add(this.gbStandardRegex);
            this.Controls.Add(this.btnAddNew);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddConcepts);
            this.Name = "StandardRegexUI";
            this.Text = "StandardRegexUI";
            this.gbStandardRegex.ResumeLayout(false);
            this.gbStandardRegex.PerformLayout();
            this.gbTesting.ResumeLayout(false);
            this.gbTesting.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ddConcepts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.GroupBox gbStandardRegex;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbRegex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbConceptName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblResultOfTest;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbTesting;
        private System.Windows.Forms.GroupBox gbTesting;
    }
}