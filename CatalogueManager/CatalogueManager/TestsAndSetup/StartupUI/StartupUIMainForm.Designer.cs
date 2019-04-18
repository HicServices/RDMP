using ReusableLibraryCode.Checks;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    partial class StartupUIMainForm : ICheckNotifier
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupUIMainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpUserFriendly = new System.Windows.Forms.TabPage();
            this.pbWhereIsDatabase = new System.Windows.Forms.PictureBox();
            this.lblStartupComplete1 = new System.Windows.Forms.Label();
            this.llException = new System.Windows.Forms.LinkLabel();
            this.lblProgress = new System.Windows.Forms.Label();
            this.pbLoadProgress = new System.Windows.Forms.ProgressBar();
            this.pbGreen = new System.Windows.Forms.PictureBox();
            this.pbRedDead = new System.Windows.Forms.PictureBox();
            this.pbRed = new System.Windows.Forms.PictureBox();
            this.pbYellow = new System.Windows.Forms.PictureBox();
            this.tpTechnical = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTier1 = new System.Windows.Forms.Label();
            this.flpTier1Databases = new System.Windows.Forms.FlowLayoutPanel();
            this.pbPluginPatchersArrow = new System.Windows.Forms.PictureBox();
            this.lblStartupComplete2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.flpTier3Databases = new System.Windows.Forms.FlowLayoutPanel();
            this.flpTier2Databases = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSetupPlatformDatabases = new System.Windows.Forms.Button();
            this.repositoryFinderUI1 = new CatalogueManager.TestsAndSetup.StartupUI.RepositoryFinderUI();
            this.Catalogue = new CatalogueManager.TestsAndSetup.StartupUI.ManagedDatabaseUI();
            this.mefStartupUI1 = new CatalogueManager.TestsAndSetup.StartupUI.MEFStartupUI();
            this.DataExport = new CatalogueManager.TestsAndSetup.StartupUI.ManagedDatabaseUI();
            this.tabControl1.SuspendLayout();
            this.tpUserFriendly.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhereIsDatabase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRedDead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbYellow)).BeginInit();
            this.tpTechnical.SuspendLayout();
            this.flpTier1Databases.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginPatchersArrow)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpUserFriendly);
            this.tabControl1.Controls.Add(this.tpTechnical);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1008, 729);
            this.tabControl1.TabIndex = 4;
            // 
            // tpUserFriendly
            // 
            this.tpUserFriendly.Controls.Add(this.btnSetupPlatformDatabases);
            this.tpUserFriendly.Controls.Add(this.pbWhereIsDatabase);
            this.tpUserFriendly.Controls.Add(this.lblStartupComplete1);
            this.tpUserFriendly.Controls.Add(this.llException);
            this.tpUserFriendly.Controls.Add(this.lblProgress);
            this.tpUserFriendly.Controls.Add(this.pbLoadProgress);
            this.tpUserFriendly.Controls.Add(this.pbGreen);
            this.tpUserFriendly.Controls.Add(this.pbRedDead);
            this.tpUserFriendly.Controls.Add(this.pbRed);
            this.tpUserFriendly.Controls.Add(this.pbYellow);
            this.tpUserFriendly.Location = new System.Drawing.Point(4, 22);
            this.tpUserFriendly.Name = "tpUserFriendly";
            this.tpUserFriendly.Padding = new System.Windows.Forms.Padding(3);
            this.tpUserFriendly.Size = new System.Drawing.Size(1000, 703);
            this.tpUserFriendly.TabIndex = 1;
            this.tpUserFriendly.Text = "User Friendly";
            this.tpUserFriendly.UseVisualStyleBackColor = true;
            // 
            // pbWhereIsDatabase
            // 
            this.pbWhereIsDatabase.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbWhereIsDatabase.Image = ((System.Drawing.Image)(resources.GetObject("pbWhereIsDatabase.Image")));
            this.pbWhereIsDatabase.Location = new System.Drawing.Point(322, 142);
            this.pbWhereIsDatabase.Name = "pbWhereIsDatabase";
            this.pbWhereIsDatabase.Size = new System.Drawing.Size(293, 281);
            this.pbWhereIsDatabase.TabIndex = 5;
            this.pbWhereIsDatabase.TabStop = false;
            this.pbWhereIsDatabase.Visible = false;
            // 
            // lblStartupComplete1
            // 
            this.lblStartupComplete1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblStartupComplete1.AutoSize = true;
            this.lblStartupComplete1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartupComplete1.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.lblStartupComplete1.Location = new System.Drawing.Point(217, 568);
            this.lblStartupComplete1.Name = "lblStartupComplete1";
            this.lblStartupComplete1.Size = new System.Drawing.Size(524, 25);
            this.lblStartupComplete1.TabIndex = 4;
            this.lblStartupComplete1.Text = "Startup Complete... Closing in 5s (Esc to cancel)";
            // 
            // llException
            // 
            this.llException.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.llException.AutoSize = true;
            this.llException.LinkColor = System.Drawing.Color.Red;
            this.llException.Location = new System.Drawing.Point(41, 488);
            this.llException.Name = "llException";
            this.llException.Size = new System.Drawing.Size(54, 13);
            this.llException.TabIndex = 3;
            this.llException.TabStop = true;
            this.llException.Text = "Exception";
            this.llException.Visible = false;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(38, 465);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(58, 13);
            this.lblProgress.TabIndex = 2;
            this.lblProgress.Text = "lblProgress";
            // 
            // pbLoadProgress
            // 
            this.pbLoadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLoadProgress.Location = new System.Drawing.Point(38, 435);
            this.pbLoadProgress.Name = "pbLoadProgress";
            this.pbLoadProgress.Size = new System.Drawing.Size(936, 23);
            this.pbLoadProgress.TabIndex = 1;
            // 
            // pbGreen
            // 
            this.pbGreen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbGreen.Image = ((System.Drawing.Image)(resources.GetObject("pbGreen.Image")));
            this.pbGreen.Location = new System.Drawing.Point(322, 142);
            this.pbGreen.Name = "pbGreen";
            this.pbGreen.Size = new System.Drawing.Size(293, 281);
            this.pbGreen.TabIndex = 0;
            this.pbGreen.TabStop = false;
            // 
            // pbRedDead
            // 
            this.pbRedDead.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbRedDead.Image = ((System.Drawing.Image)(resources.GetObject("pbRedDead.Image")));
            this.pbRedDead.Location = new System.Drawing.Point(322, 142);
            this.pbRedDead.Name = "pbRedDead";
            this.pbRedDead.Size = new System.Drawing.Size(293, 281);
            this.pbRedDead.TabIndex = 0;
            this.pbRedDead.TabStop = false;
            this.pbRedDead.Visible = false;
            // 
            // pbRed
            // 
            this.pbRed.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbRed.Image = ((System.Drawing.Image)(resources.GetObject("pbRed.Image")));
            this.pbRed.Location = new System.Drawing.Point(322, 142);
            this.pbRed.Name = "pbRed";
            this.pbRed.Size = new System.Drawing.Size(293, 281);
            this.pbRed.TabIndex = 0;
            this.pbRed.TabStop = false;
            this.pbRed.Visible = false;
            // 
            // pbYellow
            // 
            this.pbYellow.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbYellow.Image = ((System.Drawing.Image)(resources.GetObject("pbYellow.Image")));
            this.pbYellow.Location = new System.Drawing.Point(322, 142);
            this.pbYellow.Name = "pbYellow";
            this.pbYellow.Size = new System.Drawing.Size(293, 281);
            this.pbYellow.TabIndex = 0;
            this.pbYellow.TabStop = false;
            this.pbYellow.Visible = false;
            // 
            // tpTechnical
            // 
            this.tpTechnical.Controls.Add(this.label3);
            this.tpTechnical.Controls.Add(this.label2);
            this.tpTechnical.Controls.Add(this.lblTier1);
            this.tpTechnical.Controls.Add(this.flpTier1Databases);
            this.tpTechnical.Controls.Add(this.pbPluginPatchersArrow);
            this.tpTechnical.Controls.Add(this.lblStartupComplete2);
            this.tpTechnical.Controls.Add(this.flowLayoutPanel1);
            this.tpTechnical.Controls.Add(this.flpTier3Databases);
            this.tpTechnical.Controls.Add(this.flpTier2Databases);
            this.tpTechnical.Controls.Add(this.repositoryFinderUI1);
            this.tpTechnical.Location = new System.Drawing.Point(4, 22);
            this.tpTechnical.Name = "tpTechnical";
            this.tpTechnical.Padding = new System.Windows.Forms.Padding(3);
            this.tpTechnical.Size = new System.Drawing.Size(1000, 703);
            this.tpTechnical.TabIndex = 0;
            this.tpTechnical.Text = "Technical";
            this.tpTechnical.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Tier 3";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Azure;
            this.label2.Location = new System.Drawing.Point(3, 277);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Tier 2";
            // 
            // lblTier1
            // 
            this.lblTier1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTier1.BackColor = System.Drawing.Color.LightCyan;
            this.lblTier1.Location = new System.Drawing.Point(1, 456);
            this.lblTier1.Name = "lblTier1";
            this.lblTier1.Size = new System.Drawing.Size(36, 16);
            this.lblTier1.TabIndex = 1;
            this.lblTier1.Text = "Tier 1";
            // 
            // flpTier1Databases
            // 
            this.flpTier1Databases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTier1Databases.BackColor = System.Drawing.Color.LightCyan;
            this.flpTier1Databases.Controls.Add(this.Catalogue);
            this.flpTier1Databases.Controls.Add(this.mefStartupUI1);
            this.flpTier1Databases.Controls.Add(this.DataExport);
            this.flpTier1Databases.Location = new System.Drawing.Point(2, 455);
            this.flpTier1Databases.Margin = new System.Windows.Forms.Padding(0);
            this.flpTier1Databases.Name = "flpTier1Databases";
            this.flpTier1Databases.Size = new System.Drawing.Size(997, 170);
            this.flpTier1Databases.TabIndex = 8;
            // 
            // pbPluginPatchersArrow
            // 
            this.pbPluginPatchersArrow.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.pbPluginPatchersArrow.Image = ((System.Drawing.Image)(resources.GetObject("pbPluginPatchersArrow.Image")));
            this.pbPluginPatchersArrow.Location = new System.Drawing.Point(592, 251);
            this.pbPluginPatchersArrow.Name = "pbPluginPatchersArrow";
            this.pbPluginPatchersArrow.Size = new System.Drawing.Size(33, 202);
            this.pbPluginPatchersArrow.TabIndex = 1;
            this.pbPluginPatchersArrow.TabStop = false;
            this.pbPluginPatchersArrow.Visible = false;
            // 
            // lblStartupComplete2
            // 
            this.lblStartupComplete2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStartupComplete2.AutoSize = true;
            this.lblStartupComplete2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartupComplete2.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.lblStartupComplete2.Location = new System.Drawing.Point(352, 0);
            this.lblStartupComplete2.Name = "lblStartupComplete2";
            this.lblStartupComplete2.Size = new System.Drawing.Size(524, 25);
            this.lblStartupComplete2.TabIndex = 7;
            this.lblStartupComplete2.Text = "Startup Complete... Closing in 5s (Esc to cancel)";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.MistyRose;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 228);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(992, 46);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Plugin Patchers";
            // 
            // flpTier3Databases
            // 
            this.flpTier3Databases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTier3Databases.AutoScroll = true;
            this.flpTier3Databases.Location = new System.Drawing.Point(4, 45);
            this.flpTier3Databases.Name = "flpTier3Databases";
            this.flpTier3Databases.Size = new System.Drawing.Size(992, 180);
            this.flpTier3Databases.TabIndex = 5;
            // 
            // flpTier2Databases
            // 
            this.flpTier2Databases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTier2Databases.AutoScroll = true;
            this.flpTier2Databases.BackColor = System.Drawing.Color.Azure;
            this.flpTier2Databases.Location = new System.Drawing.Point(1, 273);
            this.flpTier2Databases.Name = "flpTier2Databases";
            this.flpTier2Databases.Size = new System.Drawing.Size(994, 180);
            this.flpTier2Databases.TabIndex = 4;
            // 
            // btnSetupPlatformDatabases
            // 
            this.btnSetupPlatformDatabases.Location = new System.Drawing.Point(370, 81);
            this.btnSetupPlatformDatabases.Name = "btnSetupPlatformDatabases";
            this.btnSetupPlatformDatabases.Size = new System.Drawing.Size(208, 23);
            this.btnSetupPlatformDatabases.TabIndex = 6;
            this.btnSetupPlatformDatabases.Text = "Setup Platform Databases";
            this.btnSetupPlatformDatabases.UseVisualStyleBackColor = true;
            this.btnSetupPlatformDatabases.Click += new System.EventHandler(this.btnSetupPlatformDatabases_Click);
            // 
            // repositoryFinderUI1
            // 
            this.repositoryFinderUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.repositoryFinderUI1.BackColor = System.Drawing.Color.OldLace;
            this.repositoryFinderUI1.Location = new System.Drawing.Point(0, 628);
            this.repositoryFinderUI1.Name = "repositoryFinderUI1";
            this.repositoryFinderUI1.Size = new System.Drawing.Size(997, 75);
            this.repositoryFinderUI1.TabIndex = 0;
            // 
            // Catalogue
            // 
            this.Catalogue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Catalogue.Location = new System.Drawing.Point(3, 3);
            this.Catalogue.Name = "Catalogue";
            this.Catalogue.Size = new System.Drawing.Size(300, 166);
            this.Catalogue.TabIndex = 2;
            this.Catalogue.Visible = false;
            // 
            // mefStartupUI1
            // 
            this.mefStartupUI1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.mefStartupUI1.Location = new System.Drawing.Point(309, 3);
            this.mefStartupUI1.Name = "mefStartupUI1";
            this.mefStartupUI1.Size = new System.Drawing.Size(364, 166);
            this.mefStartupUI1.TabIndex = 3;
            this.mefStartupUI1.Visible = false;
            // 
            // DataExport
            // 
            this.DataExport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DataExport.Location = new System.Drawing.Point(679, 3);
            this.DataExport.Name = "DataExport";
            this.DataExport.Size = new System.Drawing.Size(300, 166);
            this.DataExport.TabIndex = 2;
            this.DataExport.Visible = false;
            // 
            // StartupUIMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.tabControl1);
            this.KeyPreview = true;
            this.Name = "StartupUIMainForm";
            this.Text = "Startup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartupUIMainForm_FormClosing);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.StartupUIMainForm_KeyUp);
            this.tabControl1.ResumeLayout(false);
            this.tpUserFriendly.ResumeLayout(false);
            this.tpUserFriendly.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhereIsDatabase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRedDead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbYellow)).EndInit();
            this.tpTechnical.ResumeLayout(false);
            this.tpTechnical.PerformLayout();
            this.flpTier1Databases.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginPatchersArrow)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpTechnical;
        private ManagedDatabaseUI Catalogue;
        private System.Windows.Forms.Label lblTier1;
        private MEFStartupUI mefStartupUI1;
        private ManagedDatabaseUI DataExport;
        private System.Windows.Forms.TabPage tpUserFriendly;
        private System.Windows.Forms.PictureBox pbYellow;
        private System.Windows.Forms.PictureBox pbGreen;
        private System.Windows.Forms.PictureBox pbRedDead;
        private System.Windows.Forms.PictureBox pbRed;
        private System.Windows.Forms.ProgressBar pbLoadProgress;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.LinkLabel llException;
        private System.Windows.Forms.FlowLayoutPanel flpTier2Databases;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pbPluginPatchersArrow;
        private System.Windows.Forms.FlowLayoutPanel flpTier3Databases;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStartupComplete1;
        private System.Windows.Forms.Label lblStartupComplete2;
        private System.Windows.Forms.PictureBox pbWhereIsDatabase;
        private System.Windows.Forms.FlowLayoutPanel flpTier1Databases;
        private RepositoryFinderUI repositoryFinderUI1;
        private System.Windows.Forms.Button btnSetupPlatformDatabases;

    }
}