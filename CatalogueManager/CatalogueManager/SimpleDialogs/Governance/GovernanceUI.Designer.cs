namespace CatalogueManager.SimpleDialogs.Governance
{
    partial class GovernanceUI
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
            this.ddGovernance = new System.Windows.Forms.ComboBox();
            this.btnCreateNewGovernancePeriod = new System.Windows.Forms.Button();
            this.gbGovernancePeriod = new System.Windows.Forms.GroupBox();
            this.governancePeriodUI1 = new CatalogueManager.SimpleDialogs.Governance.GovernancePeriodUI();
            this.btnDelete = new System.Windows.Forms.Button();
            this.gbGovernancePeriod.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddGovernance
            // 
            this.ddGovernance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddGovernance.FormattingEnabled = true;
            this.ddGovernance.Location = new System.Drawing.Point(122, 13);
            this.ddGovernance.Name = "ddGovernance";
            this.ddGovernance.Size = new System.Drawing.Size(475, 21);
            this.ddGovernance.Sorted = true;
            this.ddGovernance.TabIndex = 0;
            this.ddGovernance.SelectedIndexChanged += new System.EventHandler(this.ddGovernance_SelectedIndexChanged);
            // 
            // btnCreateNewGovernancePeriod
            // 
            this.btnCreateNewGovernancePeriod.Location = new System.Drawing.Point(603, 11);
            this.btnCreateNewGovernancePeriod.Name = "btnCreateNewGovernancePeriod";
            this.btnCreateNewGovernancePeriod.Size = new System.Drawing.Size(62, 23);
            this.btnCreateNewGovernancePeriod.TabIndex = 2;
            this.btnCreateNewGovernancePeriod.Text = "Add New";
            this.btnCreateNewGovernancePeriod.UseVisualStyleBackColor = true;
            this.btnCreateNewGovernancePeriod.Click += new System.EventHandler(this.btnCreateNewGovernancePeriod_Click);
            // 
            // gbGovernancePeriod
            // 
            this.gbGovernancePeriod.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbGovernancePeriod.Controls.Add(this.governancePeriodUI1);
            this.gbGovernancePeriod.Location = new System.Drawing.Point(17, 40);
            this.gbGovernancePeriod.Name = "gbGovernancePeriod";
            this.gbGovernancePeriod.Size = new System.Drawing.Size(1145, 924);
            this.gbGovernancePeriod.TabIndex = 3;
            this.gbGovernancePeriod.TabStop = false;
            this.gbGovernancePeriod.Text = "Governance Period";
            // 
            // governancePeriodUI1
            // 
            this.governancePeriodUI1.AutoSize = true;
            this.governancePeriodUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.governancePeriodUI1.GovernancePeriod = null;
            this.governancePeriodUI1.Location = new System.Drawing.Point(3, 16);
            this.governancePeriodUI1.Name = "governancePeriodUI1";
            this.governancePeriodUI1.Size = new System.Drawing.Size(1139, 905);
            this.governancePeriodUI1.TabIndex = 1;
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(671, 11);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // GovernanceUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1174, 976);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.gbGovernancePeriod);
            this.Controls.Add(this.btnCreateNewGovernancePeriod);
            this.Controls.Add(this.ddGovernance);
            this.Name = "GovernanceUI";
            this.Text = "Governance";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GovernanceUI_FormClosing);
            this.gbGovernancePeriod.ResumeLayout(false);
            this.gbGovernancePeriod.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox ddGovernance;
        private GovernancePeriodUI governancePeriodUI1;
        private System.Windows.Forms.Button btnCreateNewGovernancePeriod;
        private System.Windows.Forms.GroupBox gbGovernancePeriod;
        private System.Windows.Forms.Button btnDelete;
    }
}