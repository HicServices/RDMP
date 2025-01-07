namespace Rdmp.UI.SubComponents
{
    partial class HDRDatasetConfigurationUI
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
            columnButtonRenderer1 = new BrightIdeasSoftware.ColumnButtonRenderer();
            btnViewOnHDR = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            tbDescription = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            tbStartDate = new System.Windows.Forms.TextBox();
            btnSave = new System.Windows.Forms.Button();
            tbAbstract = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            tbKeywords = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            tbDOI = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            tbContactPoint = new System.Windows.Forms.TextBox();
            tbEndDate = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // columnButtonRenderer1
            // 
            columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
            // 
            // btnViewOnHDR
            // 
            btnViewOnHDR.Enabled = false;
            btnViewOnHDR.Location = new System.Drawing.Point(29, 26);
            btnViewOnHDR.Name = "btnViewOnHDR";
            btnViewOnHDR.Size = new System.Drawing.Size(121, 23);
            btnViewOnHDR.TabIndex = 1;
            btnViewOnHDR.Text = "View on HDR";
            btnViewOnHDR.UseVisualStyleBackColor = true;
            btnViewOnHDR.Click += btnViewOnPure_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(122, 72);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 15);
            label1.TabIndex = 2;
            label1.Text = "Name:";
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(170, 69);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(712, 23);
            tbName.TabIndex = 3;
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(170, 293);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(712, 92);
            tbDescription.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(94, 296);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 15);
            label2.TabIndex = 4;
            label2.Text = "Description:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(106, 395);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(61, 15);
            label3.TabIndex = 6;
            label3.Text = "Start Date:";
            // 
            // tbStartDate
            // 
            tbStartDate.Location = new System.Drawing.Point(170, 395);
            tbStartDate.Name = "tbStartDate";
            tbStartDate.Size = new System.Drawing.Size(181, 23);
            tbStartDate.TabIndex = 7;
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(807, 427);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 23);
            btnSave.TabIndex = 14;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // tbAbstract
            // 
            tbAbstract.Location = new System.Drawing.Point(170, 108);
            tbAbstract.Multiline = true;
            tbAbstract.Name = "tbAbstract";
            tbAbstract.Size = new System.Drawing.Size(712, 92);
            tbAbstract.TabIndex = 17;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(110, 108);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(54, 15);
            label5.TabIndex = 18;
            label5.Text = "Abstract:";
            // 
            // tbKeywords
            // 
            tbKeywords.Location = new System.Drawing.Point(170, 206);
            tbKeywords.Name = "tbKeywords";
            tbKeywords.Size = new System.Drawing.Size(712, 23);
            tbKeywords.TabIndex = 19;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(103, 209);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(61, 15);
            label6.TabIndex = 20;
            label6.Text = "Keywords:";
            // 
            // tbDOI
            // 
            tbDOI.Location = new System.Drawing.Point(170, 235);
            tbDOI.Name = "tbDOI";
            tbDOI.Size = new System.Drawing.Size(181, 23);
            tbDOI.TabIndex = 21;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(134, 238);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(30, 15);
            label7.TabIndex = 22;
            label7.Text = "DOI:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(81, 264);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(83, 15);
            label8.TabIndex = 24;
            label8.Text = "Contact Point:";
            // 
            // tbContactPoint
            // 
            tbContactPoint.Location = new System.Drawing.Point(170, 264);
            tbContactPoint.Name = "tbContactPoint";
            tbContactPoint.Size = new System.Drawing.Size(181, 23);
            tbContactPoint.TabIndex = 23;
            // 
            // tbEndDate
            // 
            tbEndDate.Location = new System.Drawing.Point(170, 424);
            tbEndDate.Name = "tbEndDate";
            tbEndDate.Size = new System.Drawing.Size(181, 23);
            tbEndDate.TabIndex = 28;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(106, 427);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(57, 15);
            label4.TabIndex = 27;
            label4.Text = "End Date:";
            // 
            // HDRDatasetConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbEndDate);
            Controls.Add(label4);
            Controls.Add(btnSave);
            Controls.Add(label8);
            Controls.Add(tbContactPoint);
            Controls.Add(label7);
            Controls.Add(tbDOI);
            Controls.Add(label6);
            Controls.Add(tbKeywords);
            Controls.Add(label5);
            Controls.Add(tbAbstract);
            Controls.Add(tbStartDate);
            Controls.Add(label3);
            Controls.Add(tbDescription);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(label1);
            Controls.Add(btnViewOnHDR);
            Name = "HDRDatasetConfigurationUI";
            Size = new System.Drawing.Size(955, 648);
            Load += PureDatasetConfigurationUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
        private System.Windows.Forms.Button btnViewOnHDR;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tbAbstract;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbKeywords;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbDOI;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbContactPoint;
        private System.Windows.Forms.TextBox tbEndDate;
        private System.Windows.Forms.Label label4;
    }
}
