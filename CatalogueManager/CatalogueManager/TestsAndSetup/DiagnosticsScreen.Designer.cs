using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup
{
    partial class DiagnosticsScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticsScreen));
            this.gbChecks = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCatalogueCheck = new System.Windows.Forms.Button();
            this.btnCatalogueTableNames = new System.Windows.Forms.Button();
            this.btnListBadAssemblies = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCatalogueFields = new System.Windows.Forms.Button();
            this.btnCohortDatabase = new System.Windows.Forms.Button();
            this.btnDataExportManagerFields = new System.Windows.Forms.Button();
            this.btnCheckANOConfigurations = new System.Windows.Forms.Button();
            this.btnViewOriginalException = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.gbChecks.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbChecks
            // 
            this.gbChecks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbChecks.Controls.Add(this.groupBox4);
            this.gbChecks.Controls.Add(this.btnListBadAssemblies);
            this.gbChecks.Controls.Add(this.groupBox2);
            this.gbChecks.Controls.Add(this.btnCheckANOConfigurations);
            this.gbChecks.Location = new System.Drawing.Point(12, 12);
            this.gbChecks.Name = "gbChecks";
            this.gbChecks.Size = new System.Drawing.Size(1130, 104);
            this.gbChecks.TabIndex = 0;
            this.gbChecks.TabStop = false;
            this.gbChecks.Text = "Checks:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnCatalogueCheck);
            this.groupBox4.Controls.Add(this.btnCatalogueTableNames);
            this.groupBox4.Location = new System.Drawing.Point(578, 17);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(377, 77);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Entity Checks";
            // 
            // btnCatalogueCheck
            // 
            this.btnCatalogueCheck.Location = new System.Drawing.Point(6, 19);
            this.btnCatalogueCheck.Name = "btnCatalogueCheck";
            this.btnCatalogueCheck.Size = new System.Drawing.Size(114, 23);
            this.btnCatalogueCheck.TabIndex = 5;
            this.btnCatalogueCheck.Text = "Catalogue.Check()";
            this.btnCatalogueCheck.UseVisualStyleBackColor = true;
            this.btnCatalogueCheck.Click += new System.EventHandler(this.btnCatalogueCheck_Click);
            // 
            // btnCatalogueTableNames
            // 
            this.btnCatalogueTableNames.Location = new System.Drawing.Point(126, 19);
            this.btnCatalogueTableNames.Name = "btnCatalogueTableNames";
            this.btnCatalogueTableNames.Size = new System.Drawing.Size(212, 23);
            this.btnCatalogueTableNames.TabIndex = 4;
            this.btnCatalogueTableNames.Text = "DodgyNamedTableAndColumnsChecker";
            this.btnCatalogueTableNames.UseVisualStyleBackColor = true;
            this.btnCatalogueTableNames.Click += new System.EventHandler(this.btnCatalogueTableNames_Click);
            // 
            // btnListBadAssemblies
            // 
            this.btnListBadAssemblies.Location = new System.Drawing.Point(196, 71);
            this.btnListBadAssemblies.Name = "btnListBadAssemblies";
            this.btnListBadAssemblies.Size = new System.Drawing.Size(179, 23);
            this.btnListBadAssemblies.TabIndex = 3;
            this.btnListBadAssemblies.Text = "Evaluate MEF Exports (dlls)";
            this.btnListBadAssemblies.UseVisualStyleBackColor = true;
            this.btnListBadAssemblies.Click += new System.EventHandler(this.btnListBadAssemblies_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnCatalogueFields);
            this.groupBox2.Controls.Add(this.btnCohortDatabase);
            this.groupBox2.Controls.Add(this.btnDataExportManagerFields);
            this.groupBox2.Location = new System.Drawing.Point(6, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(566, 49);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Check For Missing Fields";
            // 
            // btnCatalogueFields
            // 
            this.btnCatalogueFields.Location = new System.Drawing.Point(5, 19);
            this.btnCatalogueFields.Name = "btnCatalogueFields";
            this.btnCatalogueFields.Size = new System.Drawing.Size(180, 23);
            this.btnCatalogueFields.TabIndex = 0;
            this.btnCatalogueFields.Text = "Catalogue Database";
            this.btnCatalogueFields.UseVisualStyleBackColor = true;
            this.btnCatalogueFields.Click += new System.EventHandler(this.btnCatalogueFields_Click);
            // 
            // btnCohortDatabase
            // 
            this.btnCohortDatabase.Location = new System.Drawing.Point(376, 19);
            this.btnCohortDatabase.Name = "btnCohortDatabase";
            this.btnCohortDatabase.Size = new System.Drawing.Size(180, 23);
            this.btnCohortDatabase.TabIndex = 2;
            this.btnCohortDatabase.Text = "Cohort Database";
            this.btnCohortDatabase.UseVisualStyleBackColor = true;
            this.btnCohortDatabase.Click += new System.EventHandler(this.btnCohortDatabase_Click);
            // 
            // btnDataExportManagerFields
            // 
            this.btnDataExportManagerFields.Location = new System.Drawing.Point(190, 19);
            this.btnDataExportManagerFields.Name = "btnDataExportManagerFields";
            this.btnDataExportManagerFields.Size = new System.Drawing.Size(180, 23);
            this.btnDataExportManagerFields.TabIndex = 1;
            this.btnDataExportManagerFields.Text = "Data Export Manager Database";
            this.btnDataExportManagerFields.UseVisualStyleBackColor = true;
            this.btnDataExportManagerFields.Click += new System.EventHandler(this.btnDataExportManagerFields_Click);
            // 
            // btnCheckANOConfigurations
            // 
            this.btnCheckANOConfigurations.Location = new System.Drawing.Point(11, 71);
            this.btnCheckANOConfigurations.Name = "btnCheckANOConfigurations";
            this.btnCheckANOConfigurations.Size = new System.Drawing.Size(180, 23);
            this.btnCheckANOConfigurations.TabIndex = 2;
            this.btnCheckANOConfigurations.Text = "Anonymisation Configurations";
            this.btnCheckANOConfigurations.UseVisualStyleBackColor = true;
            this.btnCheckANOConfigurations.Click += new System.EventHandler(this.btnCheckANOConfigurations_Click);
            // 
            // btnViewOriginalException
            // 
            this.btnViewOriginalException.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewOriginalException.Enabled = false;
            this.btnViewOriginalException.Location = new System.Drawing.Point(981, 647);
            this.btnViewOriginalException.Name = "btnViewOriginalException";
            this.btnViewOriginalException.Size = new System.Drawing.Size(144, 23);
            this.btnViewOriginalException.TabIndex = 2;
            this.btnViewOriginalException.Text = "View Original Exception";
            this.btnViewOriginalException.UseVisualStyleBackColor = true;
            this.btnViewOriginalException.Click += new System.EventHandler(this.btnViewOriginalException_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(15, 644);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(683, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "\"Please report any unauthorized database interactions to your direct superior. Re" +
    "member: A smooth operation is everyone\'s direct responsibility.\"";
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(12, 122);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1113, 519);
            this.checksUI1.TabIndex = 4;
            // 
            // DiagnosticsScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 675);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.gbChecks);
            this.Controls.Add(this.btnViewOriginalException);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DiagnosticsScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Diagnostics Screen";
            this.gbChecks.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbChecks;
        private System.Windows.Forms.Button btnCheckANOConfigurations;
        private System.Windows.Forms.Button btnDataExportManagerFields;
        private System.Windows.Forms.Button btnCatalogueFields;
        private System.Windows.Forms.Button btnViewOriginalException;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCohortDatabase;
        private System.Windows.Forms.Button btnListBadAssemblies;
        private System.Windows.Forms.Button btnCatalogueTableNames;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCatalogueCheck;
        private System.Windows.Forms.Label label8;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
    }
}