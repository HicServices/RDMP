namespace CohortManager.Wizard
{
    partial class CreateNewCohortIdentificationConfigurationUI
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
            this.pbBigImageTopLeft = new System.Windows.Forms.PictureBox();
            this.lblExclusionCriteria = new System.Windows.Forms.Label();
            this.lblInclusionCriteria = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbInclusion2 = new System.Windows.Forms.CheckBox();
            this.cbExclusion1 = new System.Windows.Forms.CheckBox();
            this.cbExclusion2 = new System.Windows.Forms.CheckBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.inclusionCriteria1 = new CohortManager.Wizard.SimpleCohortSetUI();
            this.setOperationInclude = new CohortManager.Wizard.SimpleSetOperation();
            this.inclusionCriteria2 = new CohortManager.Wizard.SimpleCohortSetUI();
            this.setOperationExclude = new CohortManager.Wizard.SimpleSetOperation();
            this.exclusionCriteria1 = new CohortManager.Wizard.SimpleCohortSetUI();
            this.exclusionCriteria2 = new CohortManager.Wizard.SimpleCohortSetUI();
            ((System.ComponentModel.ISupportInitialize)(this.pbBigImageTopLeft)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbBigImageTopLeft
            // 
            this.pbBigImageTopLeft.Location = new System.Drawing.Point(14, 12);
            this.pbBigImageTopLeft.Name = "pbBigImageTopLeft";
            this.pbBigImageTopLeft.Size = new System.Drawing.Size(75, 75);
            this.pbBigImageTopLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbBigImageTopLeft.TabIndex = 12;
            this.pbBigImageTopLeft.TabStop = false;
            // 
            // lblExclusionCriteria
            // 
            this.lblExclusionCriteria.AutoSize = true;
            this.lblExclusionCriteria.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExclusionCriteria.ForeColor = System.Drawing.SystemColors.Desktop;
            this.lblExclusionCriteria.Location = new System.Drawing.Point(725, 51);
            this.lblExclusionCriteria.Name = "lblExclusionCriteria";
            this.lblExclusionCriteria.Size = new System.Drawing.Size(244, 33);
            this.lblExclusionCriteria.TabIndex = 9;
            this.lblExclusionCriteria.Text = "Exclusion Criteria";
            // 
            // lblInclusionCriteria
            // 
            this.lblInclusionCriteria.AutoSize = true;
            this.lblInclusionCriteria.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInclusionCriteria.ForeColor = System.Drawing.SystemColors.Desktop;
            this.lblInclusionCriteria.Location = new System.Drawing.Point(118, 51);
            this.lblInclusionCriteria.Name = "lblInclusionCriteria";
            this.lblInclusionCriteria.Size = new System.Drawing.Size(234, 33);
            this.lblInclusionCriteria.TabIndex = 10;
            this.lblInclusionCriteria.Text = "Inclusion Criteria";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label2.Location = new System.Drawing.Point(95, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(239, 39);
            this.label2.TabIndex = 11;
            this.label2.Text = "Cohort Wizard";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbInclusion2
            // 
            this.cbInclusion2.AutoSize = true;
            this.cbInclusion2.Location = new System.Drawing.Point(14, 333);
            this.cbInclusion2.Name = "cbInclusion2";
            this.cbInclusion2.Size = new System.Drawing.Size(15, 14);
            this.cbInclusion2.TabIndex = 18;
            this.cbInclusion2.UseVisualStyleBackColor = true;
            this.cbInclusion2.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // cbExclusion1
            // 
            this.cbExclusion1.AutoSize = true;
            this.cbExclusion1.Location = new System.Drawing.Point(603, 17);
            this.cbExclusion1.Name = "cbExclusion1";
            this.cbExclusion1.Size = new System.Drawing.Size(15, 14);
            this.cbExclusion1.TabIndex = 18;
            this.cbExclusion1.UseVisualStyleBackColor = true;
            this.cbExclusion1.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // cbExclusion2
            // 
            this.cbExclusion2.AutoSize = true;
            this.cbExclusion2.Location = new System.Drawing.Point(596, 333);
            this.cbExclusion2.Name = "cbExclusion2";
            this.cbExclusion2.Size = new System.Drawing.Size(15, 14);
            this.cbExclusion2.TabIndex = 18;
            this.cbExclusion2.UseVisualStyleBackColor = true;
            this.cbExclusion2.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // btnGo
            // 
            this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo.Location = new System.Drawing.Point(792, 702);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(124, 23);
            this.btnGo.TabIndex = 19;
            this.btnGo.Text = "Create Configuration ";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(533, 705);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(253, 20);
            this.tbName.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(489, 708);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Name:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.cbInclusion2);
            this.panel1.Controls.Add(this.cbExclusion1);
            this.panel1.Controls.Add(this.inclusionCriteria1);
            this.panel1.Controls.Add(this.setOperationInclude);
            this.panel1.Controls.Add(this.cbExclusion2);
            this.panel1.Controls.Add(this.inclusionCriteria2);
            this.panel1.Controls.Add(this.setOperationExclude);
            this.panel1.Controls.Add(this.exclusionCriteria1);
            this.panel1.Controls.Add(this.exclusionCriteria2);
            this.panel1.Location = new System.Drawing.Point(68, 93);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1207, 603);
            this.panel1.TabIndex = 22;
            // 
            // inclusionCriteria1
            // 
            this.inclusionCriteria1.Location = new System.Drawing.Point(3, 3);
            this.inclusionCriteria1.Name = "inclusionCriteria1";
            this.inclusionCriteria1.Size = new System.Drawing.Size(576, 288);
            this.inclusionCriteria1.TabIndex = 13;
            // 
            // setOperationInclude
            // 
            this.setOperationInclude.Enabled = false;
            this.setOperationInclude.Location = new System.Drawing.Point(40, 297);
            this.setOperationInclude.Name = "setOperationInclude";
            this.setOperationInclude.Size = new System.Drawing.Size(539, 30);
            this.setOperationInclude.TabIndex = 14;
            // 
            // inclusionCriteria2
            // 
            this.inclusionCriteria2.Enabled = false;
            this.inclusionCriteria2.Location = new System.Drawing.Point(14, 322);
            this.inclusionCriteria2.Name = "inclusionCriteria2";
            this.inclusionCriteria2.Size = new System.Drawing.Size(576, 268);
            this.inclusionCriteria2.TabIndex = 13;
            // 
            // setOperationExclude
            // 
            this.setOperationExclude.Enabled = false;
            this.setOperationExclude.Location = new System.Drawing.Point(603, 297);
            this.setOperationExclude.Name = "setOperationExclude";
            this.setOperationExclude.Size = new System.Drawing.Size(542, 30);
            this.setOperationExclude.TabIndex = 17;
            // 
            // exclusionCriteria1
            // 
            this.exclusionCriteria1.Enabled = false;
            this.exclusionCriteria1.Location = new System.Drawing.Point(596, 3);
            this.exclusionCriteria1.Name = "exclusionCriteria1";
            this.exclusionCriteria1.Size = new System.Drawing.Size(576, 288);
            this.exclusionCriteria1.TabIndex = 15;
            // 
            // exclusionCriteria2
            // 
            this.exclusionCriteria2.Enabled = false;
            this.exclusionCriteria2.Location = new System.Drawing.Point(596, 322);
            this.exclusionCriteria2.Name = "exclusionCriteria2";
            this.exclusionCriteria2.Size = new System.Drawing.Size(576, 268);
            this.exclusionCriteria2.TabIndex = 16;
            // 
            // CreateNewCohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1328, 737);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.pbBigImageTopLeft);
            this.Controls.Add(this.lblExclusionCriteria);
            this.Controls.Add(this.lblInclusionCriteria);
            this.Controls.Add(this.label2);
            this.Name = "CreateNewCohortIdentificationConfigurationUI";
            this.Text = "CohortCreationWizard";
            ((System.ComponentModel.ISupportInitialize)(this.pbBigImageTopLeft)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbBigImageTopLeft;
        private System.Windows.Forms.Label lblExclusionCriteria;
        private System.Windows.Forms.Label lblInclusionCriteria;
        private System.Windows.Forms.Label label2;
        private SimpleCohortSetUI inclusionCriteria2;
        private SimpleCohortSetUI inclusionCriteria1;
        private SimpleSetOperation setOperationInclude;
        private SimpleSetOperation setOperationExclude;
        private SimpleCohortSetUI exclusionCriteria1;
        private SimpleCohortSetUI exclusionCriteria2;
        private System.Windows.Forms.CheckBox cbInclusion2;
        private System.Windows.Forms.CheckBox cbExclusion1;
        private System.Windows.Forms.CheckBox cbExclusion2;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
    }
}