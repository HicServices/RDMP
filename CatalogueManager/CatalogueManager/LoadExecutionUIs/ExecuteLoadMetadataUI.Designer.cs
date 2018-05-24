using CatalogueManager.SimpleControls;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Progress;

namespace CatalogueManager.LoadExecutionUIs
{
    partial class ExecuteLoadMetadataUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecuteLoadMetadataUI));
            this.gbLoad = new System.Windows.Forms.GroupBox();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.loadProgressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.cbSkipArchiving = new System.Windows.Forms.CheckBox();
            this.gbDebugOptions = new System.Windows.Forms.GroupBox();
            this.cbMigrateStagingToLive = new System.Windows.Forms.CheckBox();
            this.cbMigrateRAWToStaging = new System.Windows.Forms.CheckBox();
            this.ddLoadProgress = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.udDaysPerJob = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.ragChecks = new ReusableUIComponents.RAGSmiley();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gbLoadProgresses = new System.Windows.Forms.GroupBox();
            this.btnUnlockAll = new System.Windows.Forms.Button();
            this.cbRunIteratively = new System.Windows.Forms.CheckBox();
            this.cbAbortShouldActuallyCancelInstead = new System.Windows.Forms.CheckBox();
            this.btnRefreshLoadProgresses = new System.Windows.Forms.Button();
            this.helpIconAbortShouldCancel = new ReusableUIComponents.HelpIcon();
            this.helpIconRunRepeatedly = new ReusableUIComponents.HelpIcon();
            this.btnAbortLoad = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.executeInAutomationServerUI1 = new ExecuteInAutomationServerUI();
            this.gbLoad.SuspendLayout();
            this.gbDebugOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udDaysPerJob)).BeginInit();
            this.gbControls.SuspendLayout();
            this.gbLoadProgresses.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLoad
            // 
            this.gbLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLoad.Controls.Add(this.checksUI1);
            this.gbLoad.Controls.Add(this.loadProgressUI1);
            this.gbLoad.Location = new System.Drawing.Point(3, 112);
            this.gbLoad.Name = "gbLoad";
            this.gbLoad.Size = new System.Drawing.Size(916, 618);
            this.gbLoad.TabIndex = 10;
            this.gbLoad.TabStop = false;
            this.gbLoad.Text = "Load Execution";
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(3, 16);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(910, 599);
            this.checksUI1.TabIndex = 0;
            // 
            // loadProgressUI1
            // 
            this.loadProgressUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadProgressUI1.Location = new System.Drawing.Point(3, 16);
            this.loadProgressUI1.Name = "loadProgressUI1";
            this.loadProgressUI1.Size = new System.Drawing.Size(910, 599);
            this.loadProgressUI1.TabIndex = 42;
            // 
            // cbSkipArchiving
            // 
            this.cbSkipArchiving.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSkipArchiving.AutoSize = true;
            this.cbSkipArchiving.Location = new System.Drawing.Point(20, 19);
            this.cbSkipArchiving.Name = "cbSkipArchiving";
            this.cbSkipArchiving.Size = new System.Drawing.Size(93, 17);
            this.cbSkipArchiving.TabIndex = 6;
            this.cbSkipArchiving.Text = "Skip archiving";
            this.cbSkipArchiving.UseVisualStyleBackColor = true;
            // 
            // gbDebugOptions
            // 
            this.gbDebugOptions.Controls.Add(this.cbMigrateStagingToLive);
            this.gbDebugOptions.Controls.Add(this.cbMigrateRAWToStaging);
            this.gbDebugOptions.Controls.Add(this.cbSkipArchiving);
            this.gbDebugOptions.Location = new System.Drawing.Point(659, 3);
            this.gbDebugOptions.Name = "gbDebugOptions";
            this.gbDebugOptions.Size = new System.Drawing.Size(166, 90);
            this.gbDebugOptions.TabIndex = 16;
            this.gbDebugOptions.TabStop = false;
            this.gbDebugOptions.Text = "Debug Options";
            // 
            // cbMigrateStagingToLive
            // 
            this.cbMigrateStagingToLive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbMigrateStagingToLive.AutoSize = true;
            this.cbMigrateStagingToLive.Checked = true;
            this.cbMigrateStagingToLive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMigrateStagingToLive.Location = new System.Drawing.Point(20, 65);
            this.cbMigrateStagingToLive.Name = "cbMigrateStagingToLive";
            this.cbMigrateStagingToLive.Size = new System.Drawing.Size(135, 17);
            this.cbMigrateStagingToLive.TabIndex = 10;
            this.cbMigrateStagingToLive.Text = "Migrate Staging to Live";
            this.cbMigrateStagingToLive.UseVisualStyleBackColor = true;
            // 
            // cbMigrateRAWToStaging
            // 
            this.cbMigrateRAWToStaging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbMigrateRAWToStaging.AutoSize = true;
            this.cbMigrateRAWToStaging.Checked = true;
            this.cbMigrateRAWToStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMigrateRAWToStaging.Location = new System.Drawing.Point(20, 42);
            this.cbMigrateRAWToStaging.Name = "cbMigrateRAWToStaging";
            this.cbMigrateRAWToStaging.Size = new System.Drawing.Size(141, 17);
            this.cbMigrateRAWToStaging.TabIndex = 9;
            this.cbMigrateRAWToStaging.Text = "Migrate RAW to Staging";
            this.cbMigrateRAWToStaging.UseVisualStyleBackColor = true;
            this.cbMigrateRAWToStaging.CheckedChanged += new System.EventHandler(this.cbMigrateRAWToStaging_CheckedChanged);
            // 
            // ddLoadProgress
            // 
            this.ddLoadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddLoadProgress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLoadProgress.FormattingEnabled = true;
            this.ddLoadProgress.Location = new System.Drawing.Point(88, 20);
            this.ddLoadProgress.Name = "ddLoadProgress";
            this.ddLoadProgress.Size = new System.Drawing.Size(291, 21);
            this.ddLoadProgress.TabIndex = 48;
            this.ddLoadProgress.SelectedIndexChanged += new System.EventHandler(this.ddLoadProgress_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 50;
            this.label4.Text = "Load Progress";
            // 
            // udDaysPerJob
            // 
            this.udDaysPerJob.Location = new System.Drawing.Point(88, 45);
            this.udDaysPerJob.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udDaysPerJob.Name = "udDaysPerJob";
            this.udDaysPerJob.Size = new System.Drawing.Size(54, 20);
            this.udDaysPerJob.TabIndex = 48;
            this.udDaysPerJob.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 46;
            this.label3.Text = "Days per job";
            // 
            // gbControls
            // 
            this.gbControls.Controls.Add(this.executeInAutomationServerUI1);
            this.gbControls.Controls.Add(this.btnAbortLoad);
            this.gbControls.Controls.Add(this.btnExecute);
            this.gbControls.Controls.Add(this.ragChecks);
            this.gbControls.Controls.Add(this.label5);
            this.gbControls.Controls.Add(this.label2);
            this.gbControls.Controls.Add(this.label1);
            this.gbControls.Controls.Add(this.btnRunChecks);
            this.gbControls.Location = new System.Drawing.Point(3, 3);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new System.Drawing.Size(187, 103);
            this.gbControls.TabIndex = 56;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "Controls";
            // 
            // ragChecks
            // 
            this.ragChecks.AlwaysShowHandCursor = false;
            this.ragChecks.BackColor = System.Drawing.Color.Transparent;
            this.ragChecks.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragChecks.Location = new System.Drawing.Point(7, 16);
            this.ragChecks.Name = "ragChecks";
            this.ragChecks.Size = new System.Drawing.Size(25, 25);
            this.ragChecks.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(151, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Abort";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(91, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Execute";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Checks";
            // 
            // gbLoadProgresses
            // 
            this.gbLoadProgresses.Controls.Add(this.btnUnlockAll);
            this.gbLoadProgresses.Controls.Add(this.btnRefreshLoadProgresses);
            this.gbLoadProgresses.Controls.Add(this.helpIconAbortShouldCancel);
            this.gbLoadProgresses.Controls.Add(this.helpIconRunRepeatedly);
            this.gbLoadProgresses.Controls.Add(this.cbRunIteratively);
            this.gbLoadProgresses.Controls.Add(this.cbAbortShouldActuallyCancelInstead);
            this.gbLoadProgresses.Controls.Add(this.ddLoadProgress);
            this.gbLoadProgresses.Controls.Add(this.udDaysPerJob);
            this.gbLoadProgresses.Controls.Add(this.label3);
            this.gbLoadProgresses.Controls.Add(this.label4);
            this.gbLoadProgresses.Location = new System.Drawing.Point(196, 3);
            this.gbLoadProgresses.Name = "gbLoadProgresses";
            this.gbLoadProgresses.Size = new System.Drawing.Size(457, 90);
            this.gbLoadProgresses.TabIndex = 57;
            this.gbLoadProgresses.TabStop = false;
            this.gbLoadProgresses.Text = "Progressable To Execute";
            // 
            // btnUnlockAll
            // 
            this.btnUnlockAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnlockAll.Location = new System.Drawing.Point(385, 18);
            this.btnUnlockAll.Name = "btnUnlockAll";
            this.btnUnlockAll.Size = new System.Drawing.Size(30, 23);
            this.btnUnlockAll.TabIndex = 59;
            this.btnUnlockAll.UseVisualStyleBackColor = true;
            this.btnUnlockAll.Click += new System.EventHandler(this.btnUnlockAll_Click);
            // 
            // cbRunIteratively
            // 
            this.cbRunIteratively.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRunIteratively.AutoSize = true;
            this.cbRunIteratively.Location = new System.Drawing.Point(6, 68);
            this.cbRunIteratively.Name = "cbRunIteratively";
            this.cbRunIteratively.Size = new System.Drawing.Size(103, 17);
            this.cbRunIteratively.TabIndex = 51;
            this.cbRunIteratively.Text = "Run Repeatedly";
            this.cbRunIteratively.UseVisualStyleBackColor = true;
            this.cbRunIteratively.CheckedChanged += new System.EventHandler(this.cbRunIteratively_CheckedChanged);
            // 
            // cbAbortShouldActuallyCancelInstead
            // 
            this.cbAbortShouldActuallyCancelInstead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAbortShouldActuallyCancelInstead.AutoSize = true;
            this.cbAbortShouldActuallyCancelInstead.Enabled = false;
            this.cbAbortShouldActuallyCancelInstead.Location = new System.Drawing.Point(129, 68);
            this.cbAbortShouldActuallyCancelInstead.Name = "cbAbortShouldActuallyCancelInstead";
            this.cbAbortShouldActuallyCancelInstead.Size = new System.Drawing.Size(152, 17);
            this.cbAbortShouldActuallyCancelInstead.TabIndex = 11;
            this.cbAbortShouldActuallyCancelInstead.Text = "Clicking Abort Should Wait";
            this.cbAbortShouldActuallyCancelInstead.UseVisualStyleBackColor = true;
            // 
            // btnRefreshLoadProgresses
            // 
            this.btnRefreshLoadProgresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshLoadProgresses.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshLoadProgresses.Image")));
            this.btnRefreshLoadProgresses.Location = new System.Drawing.Point(421, 18);
            this.btnRefreshLoadProgresses.Name = "btnRefreshLoadProgresses";
            this.btnRefreshLoadProgresses.Size = new System.Drawing.Size(30, 23);
            this.btnRefreshLoadProgresses.TabIndex = 59;
            this.btnRefreshLoadProgresses.UseVisualStyleBackColor = true;
            this.btnRefreshLoadProgresses.Click += new System.EventHandler(this.btnRefreshLoadProgresses_Click);
            // 
            // helpIconAbortShouldCancel
            // 
            this.helpIconAbortShouldCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconAbortShouldCancel.BackgroundImage")));
            this.helpIconAbortShouldCancel.Location = new System.Drawing.Point(281, 66);
            this.helpIconAbortShouldCancel.Name = "helpIconAbortShouldCancel";
            this.helpIconAbortShouldCancel.Size = new System.Drawing.Size(19, 19);
            this.helpIconAbortShouldCancel.TabIndex = 52;
            // 
            // helpIconRunRepeatedly
            // 
            this.helpIconRunRepeatedly.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconRunRepeatedly.BackgroundImage")));
            this.helpIconRunRepeatedly.Location = new System.Drawing.Point(104, 67);
            this.helpIconRunRepeatedly.Name = "helpIconRunRepeatedly";
            this.helpIconRunRepeatedly.Size = new System.Drawing.Size(19, 19);
            this.helpIconRunRepeatedly.TabIndex = 52;
            // 
            // btnAbortLoad
            // 
            this.btnAbortLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnAbortLoad.Image")));
            this.btnAbortLoad.Location = new System.Drawing.Point(152, 13);
            this.btnAbortLoad.Name = "btnAbortLoad";
            this.btnAbortLoad.Size = new System.Drawing.Size(29, 29);
            this.btnAbortLoad.TabIndex = 58;
            this.btnAbortLoad.UseVisualStyleBackColor = true;
            this.btnAbortLoad.Click += new System.EventHandler(this.btnAbortLoad_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(67, 13);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(86, 29);
            this.btnExecute.TabIndex = 58;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnRunChecks
            // 
            this.btnRunChecks.Image = ((System.Drawing.Image)(resources.GetObject("btnRunChecks.Image")));
            this.btnRunChecks.Location = new System.Drawing.Point(38, 13);
            this.btnRunChecks.Name = "btnRunChecks";
            this.btnRunChecks.Size = new System.Drawing.Size(30, 29);
            this.btnRunChecks.TabIndex = 1;
            this.btnRunChecks.UseVisualStyleBackColor = true;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);
            // 
            // executeInAutomationServerUI1
            // 
            this.executeInAutomationServerUI1.Location = new System.Drawing.Point(29, 57);
            this.executeInAutomationServerUI1.Name = "executeInAutomationServerUI1";
            this.executeInAutomationServerUI1.Size = new System.Drawing.Size(155, 46);
            this.executeInAutomationServerUI1.TabIndex = 59;
            // 
            // ExecuteLoadMetadataUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbLoadProgresses);
            this.Controls.Add(this.gbControls);
            this.Controls.Add(this.gbLoad);
            this.Controls.Add(this.gbDebugOptions);
            this.Name = "ExecuteLoadMetadataUI";
            this.Size = new System.Drawing.Size(922, 733);
            this.gbLoad.ResumeLayout(false);
            this.gbDebugOptions.ResumeLayout(false);
            this.gbDebugOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udDaysPerJob)).EndInit();
            this.gbControls.ResumeLayout(false);
            this.gbControls.PerformLayout();
            this.gbLoadProgresses.ResumeLayout(false);
            this.gbLoadProgresses.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLoad;
        private System.Windows.Forms.CheckBox cbSkipArchiving;
        private System.Windows.Forms.GroupBox gbDebugOptions;
        private ProgressUI loadProgressUI1;
        private System.Windows.Forms.CheckBox cbMigrateStagingToLive;
        private System.Windows.Forms.CheckBox cbMigrateRAWToStaging;
        private ChecksUI checksUI1;
        private System.Windows.Forms.Button btnRunChecks;
        private System.Windows.Forms.ComboBox ddLoadProgress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown udDaysPerJob;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.Label label1;
        private ReusableUIComponents.RAGSmiley ragChecks;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAbortLoad;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox gbLoadProgresses;
        private ReusableUIComponents.HelpIcon helpIconRunRepeatedly;
        private System.Windows.Forms.CheckBox cbRunIteratively;
        private System.Windows.Forms.CheckBox cbAbortShouldActuallyCancelInstead;
        private ReusableUIComponents.HelpIcon helpIconAbortShouldCancel;
        private System.Windows.Forms.Button btnUnlockAll;
        private System.Windows.Forms.Button btnRefreshLoadProgresses;
        private ExecuteInAutomationServerUI executeInAutomationServerUI1;
    }
}

