namespace Rdmp.UI.Wizard
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
            this.cbInclusion2 = new System.Windows.Forms.CheckBox();
            this.cbExclusion1 = new System.Windows.Forms.CheckBox();
            this.cbExclusion2 = new System.Windows.Forms.CheckBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlWizard = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.inclusionCriteria1 = new Rdmp.UI.Wizard.SimpleCohortSetUI();
            this.setOperationInclude = new Rdmp.UI.Wizard.SimpleSetOperation();
            this.inclusionCriteria2 = new Rdmp.UI.Wizard.SimpleCohortSetUI();
            this.setOperationExclude = new Rdmp.UI.Wizard.SimpleSetOperation();
            this.exclusionCriteria1 = new Rdmp.UI.Wizard.SimpleCohortSetUI();
            this.exclusionCriteria2 = new Rdmp.UI.Wizard.SimpleCohortSetUI();
            this.cbUseWizard = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlWizard.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbInclusion2
            // 
            this.cbInclusion2.AutoSize = true;
            this.cbInclusion2.Location = new System.Drawing.Point(7, 377);
            this.cbInclusion2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbInclusion2.Name = "cbInclusion2";
            this.cbInclusion2.Size = new System.Drawing.Size(15, 14);
            this.cbInclusion2.TabIndex = 18;
            this.cbInclusion2.UseVisualStyleBackColor = true;
            this.cbInclusion2.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // cbExclusion1
            // 
            this.cbExclusion1.AutoSize = true;
            this.cbExclusion1.Location = new System.Drawing.Point(676, 9);
            this.cbExclusion1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbExclusion1.Name = "cbExclusion1";
            this.cbExclusion1.Size = new System.Drawing.Size(15, 14);
            this.cbExclusion1.TabIndex = 18;
            this.cbExclusion1.UseVisualStyleBackColor = true;
            this.cbExclusion1.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // cbExclusion2
            // 
            this.cbExclusion2.AutoSize = true;
            this.cbExclusion2.Location = new System.Drawing.Point(676, 377);
            this.cbExclusion2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbExclusion2.Name = "cbExclusion2";
            this.cbExclusion2.Size = new System.Drawing.Size(15, 14);
            this.cbExclusion2.TabIndex = 18;
            this.cbExclusion2.UseVisualStyleBackColor = true;
            this.cbExclusion2.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // btnGo
            // 
            this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo.Location = new System.Drawing.Point(1194, 5);
            this.btnGo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(145, 27);
            this.btnGo.TabIndex = 19;
            this.btnGo.Text = "OK";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(54, 3);
            this.tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(500, 23);
            this.tbName.TabIndex = 0;
            this.tbName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbName_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 21;
            this.label1.Text = "Name:";
            // 
            // pnlWizard
            // 
            this.pnlWizard.AutoScroll = true;
            this.pnlWizard.Controls.Add(this.label2);
            this.pnlWizard.Controls.Add(this.cbInclusion2);
            this.pnlWizard.Controls.Add(this.label3);
            this.pnlWizard.Controls.Add(this.cbExclusion1);
            this.pnlWizard.Controls.Add(this.inclusionCriteria1);
            this.pnlWizard.Controls.Add(this.setOperationInclude);
            this.pnlWizard.Controls.Add(this.cbExclusion2);
            this.pnlWizard.Controls.Add(this.inclusionCriteria2);
            this.pnlWizard.Controls.Add(this.setOperationExclude);
            this.pnlWizard.Controls.Add(this.exclusionCriteria1);
            this.pnlWizard.Controls.Add(this.exclusionCriteria2);
            this.pnlWizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlWizard.Enabled = false;
            this.pnlWizard.Location = new System.Drawing.Point(0, 56);
            this.pnlWizard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlWizard.Name = "pnlWizard";
            this.pnlWizard.Size = new System.Drawing.Size(1352, 718);
            this.pnlWizard.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(695, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(183, 30);
            this.label2.TabIndex = 24;
            this.label2.Text = "Exclusion Criteria";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(1, -2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 30);
            this.label3.TabIndex = 23;
            this.label3.Text = "Inclusion Criteria";
            // 
            // inclusionCriteria1
            // 
            this.inclusionCriteria1.Location = new System.Drawing.Point(0, 31);
            this.inclusionCriteria1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.inclusionCriteria1.Name = "inclusionCriteria1";
            this.inclusionCriteria1.Size = new System.Drawing.Size(672, 332);
            this.inclusionCriteria1.TabIndex = 1;
            // 
            // setOperationInclude
            // 
            this.setOperationInclude.Enabled = false;
            this.setOperationInclude.Location = new System.Drawing.Point(22, 371);
            this.setOperationInclude.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.setOperationInclude.Name = "setOperationInclude";
            this.setOperationInclude.Size = new System.Drawing.Size(620, 30);
            this.setOperationInclude.TabIndex = 14;
            // 
            // inclusionCriteria2
            // 
            this.inclusionCriteria2.Enabled = false;
            this.inclusionCriteria2.Location = new System.Drawing.Point(0, 402);
            this.inclusionCriteria2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.inclusionCriteria2.Name = "inclusionCriteria2";
            this.inclusionCriteria2.Size = new System.Drawing.Size(672, 309);
            this.inclusionCriteria2.TabIndex = 13;
            // 
            // setOperationExclude
            // 
            this.setOperationExclude.Enabled = false;
            this.setOperationExclude.Location = new System.Drawing.Point(695, 371);
            this.setOperationExclude.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.setOperationExclude.Name = "setOperationExclude";
            this.setOperationExclude.Size = new System.Drawing.Size(620, 30);
            this.setOperationExclude.TabIndex = 17;
            // 
            // exclusionCriteria1
            // 
            this.exclusionCriteria1.Enabled = false;
            this.exclusionCriteria1.Location = new System.Drawing.Point(672, 31);
            this.exclusionCriteria1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.exclusionCriteria1.Name = "exclusionCriteria1";
            this.exclusionCriteria1.Size = new System.Drawing.Size(672, 332);
            this.exclusionCriteria1.TabIndex = 15;
            // 
            // exclusionCriteria2
            // 
            this.exclusionCriteria2.Enabled = false;
            this.exclusionCriteria2.Location = new System.Drawing.Point(675, 400);
            this.exclusionCriteria2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.exclusionCriteria2.Name = "exclusionCriteria2";
            this.exclusionCriteria2.Size = new System.Drawing.Size(672, 309);
            this.exclusionCriteria2.TabIndex = 16;
            // 
            // cbUseWizard
            // 
            this.cbUseWizard.AutoSize = true;
            this.cbUseWizard.Location = new System.Drawing.Point(561, 5);
            this.cbUseWizard.Name = "cbUseWizard";
            this.cbUseWizard.Size = new System.Drawing.Size(164, 19);
            this.cbUseWizard.TabIndex = 24;
            this.cbUseWizard.Text = "Use Cohort Builder Wizard";
            this.cbUseWizard.UseVisualStyleBackColor = true;
            this.cbUseWizard.CheckedChanged += new System.EventHandler(this.cbUseWizard_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.tbName);
            this.panel2.Controls.Add(this.cbUseWizard);
            this.panel2.Location = new System.Drawing.Point(7, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(730, 28);
            this.panel2.TabIndex = 26;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1352, 56);
            this.panel1.TabIndex = 28;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCancel);
            this.panel3.Controls.Add(this.btnGo);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 774);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1352, 44);
            this.panel3.TabIndex = 29;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1041, 5);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(145, 27);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CreateNewCohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1352, 818);
            this.Controls.Add(this.pnlWizard);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CreateNewCohortIdentificationConfigurationUI";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "New Cohort Builder Query";
            this.Load += new System.EventHandler(this.CreateNewCohortIdentificationConfigurationUI_Load);
            this.pnlWizard.ResumeLayout(false);
            this.pnlWizard.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

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
        private System.Windows.Forms.Panel pnlWizard;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbUseWizard;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnCancel;
    }
}