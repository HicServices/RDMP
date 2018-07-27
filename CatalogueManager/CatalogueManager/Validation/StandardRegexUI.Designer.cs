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
            this.gbTesting = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lblResultOfTest = new System.Windows.Forms.Label();
            this.tbTesting = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbRegex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbConceptName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.gbTesting.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTesting
            // 
            this.gbTesting.Controls.Add(this.label7);
            this.gbTesting.Controls.Add(this.lblResultOfTest);
            this.gbTesting.Controls.Add(this.tbTesting);
            this.gbTesting.Controls.Add(this.label8);
            this.gbTesting.Controls.Add(this.btnTest);
            this.gbTesting.Location = new System.Drawing.Point(2, 335);
            this.gbTesting.Name = "gbTesting";
            this.gbTesting.Size = new System.Drawing.Size(966, 211);
            this.gbTesting.TabIndex = 10;
            this.gbTesting.TabStop = false;
            this.gbTesting.Text = "Testing";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(49, 10);
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
            this.lblResultOfTest.Location = new System.Drawing.Point(35, 53);
            this.lblResultOfTest.Name = "lblResultOfTest";
            this.lblResultOfTest.Size = new System.Drawing.Size(925, 144);
            this.lblResultOfTest.TabIndex = 9;
            // 
            // tbTesting
            // 
            this.tbTesting.Location = new System.Drawing.Point(121, 7);
            this.tbTesting.Name = "tbTesting";
            this.tbTesting.Size = new System.Drawing.Size(439, 20);
            this.tbTesting.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(118, 30);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(442, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "(Type a value in above and click to test whether it validates with your current R" +
    "egex pattern)";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(566, 5);
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
            this.label6.Location = new System.Drawing.Point(106, 290);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(798, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "(In addition to explaining what the concept is (e.g. patient identifier for scott" +
    "ish patients), you should also explain the meaning of the regex (e.g. exactly 10" +
    " digits in a row))";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(109, 87);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(859, 200);
            this.tbDescription.TabIndex = 3;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(40, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Description:";
            // 
            // tbRegex
            // 
            this.tbRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRegex.Location = new System.Drawing.Point(109, 61);
            this.tbRegex.Name = "tbRegex";
            this.tbRegex.Size = new System.Drawing.Size(859, 20);
            this.tbRegex.TabIndex = 2;
            this.tbRegex.TextChanged += new System.EventHandler(this.tbRegex_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(62, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Regex:";
            // 
            // tbConceptName
            // 
            this.tbConceptName.Location = new System.Drawing.Point(109, 35);
            this.tbConceptName.Name = "tbConceptName";
            this.tbConceptName.Size = new System.Drawing.Size(309, 20);
            this.tbConceptName.TabIndex = 1;
            this.tbConceptName.TextChanged += new System.EventHandler(this.tbConceptName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Concept Name:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(109, 9);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(82, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "ID:";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(109, 312);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(54, 27);
            this.objectSaverButton1.TabIndex = 11;
            // 
            // StandardRegexUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.gbTesting);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.tbConceptName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbRegex);
            this.Name = "StandardRegexUI";
            this.Size = new System.Drawing.Size(1009, 620);
            this.gbTesting.ResumeLayout(false);
            this.gbTesting.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbRegex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbConceptName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblResultOfTest;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbTesting;
        private System.Windows.Forms.GroupBox gbTesting;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}