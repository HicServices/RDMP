namespace CachingUI
{
    partial class CachingEngineUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CachingEngineUI));
            this.label1 = new System.Windows.Forms.Label();
            this.ddLoadProgress = new System.Windows.Forms.ComboBox();
            this.btnStartCaching = new System.Windows.Forms.Button();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnStopCaching = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnStartCacheDaemon = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpStartDateToRetrieve = new System.Windows.Forms.DateTimePicker();
            this.btnStartSingleDateRetrieve = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabSingleJob = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.tabDaemon = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.tabCustomDate = new System.Windows.Forms.TabPage();
            this.cbIgnorePermissionWindow = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dtpEndDateToRetrieve = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.tabFailures = new System.Windows.Forms.TabPage();
            this.btnRetryFailures = new System.Windows.Forms.Button();
            this.btnViewFailures = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnShowConfiguration = new System.Windows.Forms.Button();
            this.btnShowCachingPipeline = new System.Windows.Forms.Button();
            this.checksUI = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.progressUI = new ReusableUIComponents.Progress.ProgressUI();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeCatalogueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchDiagnosticsScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPerformanceCounterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabSingleJob.SuspendLayout();
            this.tabDaemon.SuspendLayout();
            this.tabCustomDate.SuspendLayout();
            this.tabFailures.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Load Schedule:";
            // 
            // ddLoadProgress
            // 
            this.ddLoadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddLoadProgress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLoadProgress.FormattingEnabled = true;
            this.ddLoadProgress.Location = new System.Drawing.Point(96, 5);
            this.ddLoadProgress.Name = "ddLoadProgress";
            this.ddLoadProgress.Size = new System.Drawing.Size(611, 21);
            this.ddLoadProgress.Sorted = true;
            this.ddLoadProgress.TabIndex = 1;
            this.ddLoadProgress.SelectedIndexChanged += new System.EventHandler(this.ddLoadProgress_SelectedIndexChanged);
            // 
            // btnStartCaching
            // 
            this.btnStartCaching.Location = new System.Drawing.Point(182, 66);
            this.btnStartCaching.Name = "btnStartCaching";
            this.btnStartCaching.Size = new System.Drawing.Size(153, 23);
            this.btnStartCaching.TabIndex = 2;
            this.btnStartCaching.Text = "Start Single Cache Job";
            this.btnStartCaching.UseVisualStyleBackColor = true;
            this.btnStartCaching.Click += new System.EventHandler(this.btnStartCaching_Click);
            // 
            // btnRunChecks
            // 
            this.btnRunChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunChecks.Location = new System.Drawing.Point(703, 438);
            this.btnRunChecks.Name = "btnRunChecks";
            this.btnRunChecks.Size = new System.Drawing.Size(129, 23);
            this.btnRunChecks.TabIndex = 2;
            this.btnRunChecks.Text = "Re-run Checks";
            this.btnRunChecks.UseVisualStyleBackColor = true;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(431, 201);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(129, 23);
            this.btnAbort.TabIndex = 2;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnStopCaching
            // 
            this.btnStopCaching.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnStopCaching.Enabled = false;
            this.btnStopCaching.Location = new System.Drawing.Point(265, 201);
            this.btnStopCaching.Name = "btnStopCaching";
            this.btnStopCaching.Size = new System.Drawing.Size(129, 23);
            this.btnStopCaching.TabIndex = 2;
            this.btnStopCaching.Text = "Stop";
            this.btnStopCaching.UseVisualStyleBackColor = true;
            this.btnStopCaching.Click += new System.EventHandler(this.btnStopCaching_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.Location = new System.Drawing.Point(713, 3);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(113, 23);
            this.btnOpenFolder.TabIndex = 5;
            this.btnOpenFolder.Text = "Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnStartCacheDaemon
            // 
            this.btnStartCacheDaemon.Location = new System.Drawing.Point(191, 68);
            this.btnStartCacheDaemon.Name = "btnStartCacheDaemon";
            this.btnStartCacheDaemon.Size = new System.Drawing.Size(140, 23);
            this.btnStartCacheDaemon.TabIndex = 6;
            this.btnStartCacheDaemon.Text = "Start Cache Daemon";
            this.btnStartCacheDaemon.UseVisualStyleBackColor = true;
            this.btnStartCacheDaemon.Click += new System.EventHandler(this.btnStartCacheDaemon_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(116, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Start date:";
            // 
            // dtpStartDateToRetrieve
            // 
            this.dtpStartDateToRetrieve.Location = new System.Drawing.Point(178, 37);
            this.dtpStartDateToRetrieve.Name = "dtpStartDateToRetrieve";
            this.dtpStartDateToRetrieve.Size = new System.Drawing.Size(152, 20);
            this.dtpStartDateToRetrieve.TabIndex = 8;
            this.dtpStartDateToRetrieve.ValueChanged += new System.EventHandler(this.dtpStartDateToRetrieve_ValueChanged);
            // 
            // btnStartSingleDateRetrieve
            // 
            this.btnStartSingleDateRetrieve.Location = new System.Drawing.Point(178, 120);
            this.btnStartSingleDateRetrieve.Name = "btnStartSingleDateRetrieve";
            this.btnStartSingleDateRetrieve.Size = new System.Drawing.Size(103, 23);
            this.btnStartSingleDateRetrieve.TabIndex = 9;
            this.btnStartSingleDateRetrieve.Text = "Start";
            this.btnStartSingleDateRetrieve.UseVisualStyleBackColor = true;
            this.btnStartSingleDateRetrieve.Click += new System.EventHandler(this.btnStartSingleDateRetrieve_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabSingleJob);
            this.tabControl1.Controls.Add(this.tabDaemon);
            this.tabControl1.Controls.Add(this.tabCustomDate);
            this.tabControl1.Controls.Add(this.tabFailures);
            this.tabControl1.Location = new System.Drawing.Point(6, 19);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.ShowToolTips = true;
            this.tabControl1.Size = new System.Drawing.Size(808, 176);
            this.tabControl1.TabIndex = 11;
            // 
            // tabSingleJob
            // 
            this.tabSingleJob.BackColor = System.Drawing.SystemColors.Control;
            this.tabSingleJob.Controls.Add(this.label3);
            this.tabSingleJob.Controls.Add(this.btnStartCaching);
            this.tabSingleJob.Location = new System.Drawing.Point(4, 22);
            this.tabSingleJob.Name = "tabSingleJob";
            this.tabSingleJob.Padding = new System.Windows.Forms.Padding(3);
            this.tabSingleJob.Size = new System.Drawing.Size(800, 150);
            this.tabSingleJob.TabIndex = 0;
            this.tabSingleJob.Text = "Single Job";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(363, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Starts a single cache fetch, determined by the chunk size in CacheProgress";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabDaemon
            // 
            this.tabDaemon.BackColor = System.Drawing.SystemColors.Control;
            this.tabDaemon.Controls.Add(this.label4);
            this.tabDaemon.Controls.Add(this.btnStartCacheDaemon);
            this.tabDaemon.Location = new System.Drawing.Point(4, 22);
            this.tabDaemon.Name = "tabDaemon";
            this.tabDaemon.Padding = new System.Windows.Forms.Padding(3);
            this.tabDaemon.Size = new System.Drawing.Size(800, 150);
            this.tabDaemon.TabIndex = 1;
            this.tabDaemon.Text = "Daemon";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(83, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(365, 54);
            this.label4.TabIndex = 7;
            this.label4.Text = "Starts the Cache Daemon, which will consecutively fetch cache chunks until the be" +
    "ginning of the lag period is reached (i.e. caching is finished)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabCustomDate
            // 
            this.tabCustomDate.BackColor = System.Drawing.SystemColors.Control;
            this.tabCustomDate.Controls.Add(this.cbIgnorePermissionWindow);
            this.tabCustomDate.Controls.Add(this.label6);
            this.tabCustomDate.Controls.Add(this.dtpEndDateToRetrieve);
            this.tabCustomDate.Controls.Add(this.label5);
            this.tabCustomDate.Controls.Add(this.btnStartSingleDateRetrieve);
            this.tabCustomDate.Controls.Add(this.label2);
            this.tabCustomDate.Controls.Add(this.dtpStartDateToRetrieve);
            this.tabCustomDate.Location = new System.Drawing.Point(4, 22);
            this.tabCustomDate.Name = "tabCustomDate";
            this.tabCustomDate.Size = new System.Drawing.Size(800, 150);
            this.tabCustomDate.TabIndex = 2;
            this.tabCustomDate.Text = "Custom Date";
            // 
            // cbIgnorePermissionWindow
            // 
            this.cbIgnorePermissionWindow.AutoSize = true;
            this.cbIgnorePermissionWindow.Location = new System.Drawing.Point(178, 89);
            this.cbIgnorePermissionWindow.Name = "cbIgnorePermissionWindow";
            this.cbIgnorePermissionWindow.Size = new System.Drawing.Size(303, 17);
            this.cbIgnorePermissionWindow.TabIndex = 13;
            this.cbIgnorePermissionWindow.Text = "Ignore Permission Window (only use for testing/debugging)";
            this.cbIgnorePermissionWindow.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(119, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "End date:";
            // 
            // dtpEndDateToRetrieve
            // 
            this.dtpEndDateToRetrieve.Location = new System.Drawing.Point(178, 63);
            this.dtpEndDateToRetrieve.Name = "dtpEndDateToRetrieve";
            this.dtpEndDateToRetrieve.Size = new System.Drawing.Size(152, 20);
            this.dtpEndDateToRetrieve.TabIndex = 12;
            this.dtpEndDateToRetrieve.ValueChanged += new System.EventHandler(this.dtpEndDateToRetrieve_ValueChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(-4, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(506, 23);
            this.label5.TabIndex = 10;
            this.label5.Text = "Overrides the CacheFillProgress date and fetches chunks for the provided range of" +
    " dates.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabFailures
            // 
            this.tabFailures.BackColor = System.Drawing.SystemColors.Control;
            this.tabFailures.Controls.Add(this.btnRetryFailures);
            this.tabFailures.Controls.Add(this.btnViewFailures);
            this.tabFailures.Location = new System.Drawing.Point(4, 22);
            this.tabFailures.Name = "tabFailures";
            this.tabFailures.Padding = new System.Windows.Forms.Padding(3);
            this.tabFailures.Size = new System.Drawing.Size(800, 150);
            this.tabFailures.TabIndex = 3;
            this.tabFailures.Text = "Failures";
            // 
            // btnRetryFailures
            // 
            this.btnRetryFailures.Location = new System.Drawing.Point(168, 81);
            this.btnRetryFailures.Name = "btnRetryFailures";
            this.btnRetryFailures.Size = new System.Drawing.Size(139, 23);
            this.btnRetryFailures.TabIndex = 1;
            this.btnRetryFailures.Text = "Retry Failures";
            this.btnRetryFailures.UseVisualStyleBackColor = true;
            this.btnRetryFailures.Click += new System.EventHandler(this.btnRetryFailures_Click);
            // 
            // btnViewFailures
            // 
            this.btnViewFailures.Location = new System.Drawing.Point(168, 30);
            this.btnViewFailures.Name = "btnViewFailures";
            this.btnViewFailures.Size = new System.Drawing.Size(139, 23);
            this.btnViewFailures.TabIndex = 0;
            this.btnViewFailures.Text = "View Failures";
            this.btnViewFailures.UseVisualStyleBackColor = true;
            this.btnViewFailures.Click += new System.EventHandler(this.btnViewFailures_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.Controls.Add(this.btnAbort);
            this.groupBox1.Controls.Add(this.btnStopCaching);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(12, 496);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(820, 232);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Job Control";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnShowConfiguration);
            this.splitContainer1.Panel1.Controls.Add(this.btnShowCachingPipeline);
            this.splitContainer1.Panel1.Controls.Add(this.btnRunChecks);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenFolder);
            this.splitContainer1.Panel1.Controls.Add(this.ddLoadProgress);
            this.splitContainer1.Panel1.Controls.Add(this.checksUI);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.progressUI);
            this.splitContainer1.Size = new System.Drawing.Size(1484, 718);
            this.splitContainer1.SplitterDistance = 839;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 13;
            // 
            // btnShowConfiguration
            // 
            this.btnShowConfiguration.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnShowConfiguration.Location = new System.Drawing.Point(306, 467);
            this.btnShowConfiguration.Name = "btnShowConfiguration";
            this.btnShowConfiguration.Size = new System.Drawing.Size(143, 23);
            this.btnShowConfiguration.TabIndex = 14;
            this.btnShowConfiguration.Text = "Show Configuration";
            this.btnShowConfiguration.UseVisualStyleBackColor = true;
            this.btnShowConfiguration.Click += new System.EventHandler(this.btnShowConfiguration_Click);
            // 
            // btnShowCachingPipeline
            // 
            this.btnShowCachingPipeline.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnShowCachingPipeline.Location = new System.Drawing.Point(157, 467);
            this.btnShowCachingPipeline.Name = "btnShowCachingPipeline";
            this.btnShowCachingPipeline.Size = new System.Drawing.Size(143, 23);
            this.btnShowCachingPipeline.TabIndex = 13;
            this.btnShowCachingPipeline.Text = "Show Caching Pipeline";
            this.btnShowCachingPipeline.UseVisualStyleBackColor = true;
            this.btnShowCachingPipeline.Click += new System.EventHandler(this.btnShowCachingPipeline_Click);
            // 
            // checksUI
            // 
            this.checksUI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI.Location = new System.Drawing.Point(12, 32);
            this.checksUI.Name = "checksUI";
            this.checksUI.Size = new System.Drawing.Size(820, 429);
            this.checksUI.TabIndex = 3;
            // 
            // progressUI
            // 
            this.progressUI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI.Location = new System.Drawing.Point(3, 0);
            this.progressUI.Name = "progressUI";
            this.progressUI.Size = new System.Drawing.Size(607, 728);
            this.progressUI.TabIndex = 4;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.databaseToolStripMenuItem,
            this.testsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1484, 24);
            this.menuStrip1.TabIndex = 57;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeCatalogueToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.databaseToolStripMenuItem.Text = "Locations";
            // 
            // changeCatalogueToolStripMenuItem
            // 
            this.changeCatalogueToolStripMenuItem.Name = "changeCatalogueToolStripMenuItem";
            this.changeCatalogueToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.changeCatalogueToolStripMenuItem.Text = "Change Platform Databases...";
            this.changeCatalogueToolStripMenuItem.Click += new System.EventHandler(this.changeCatalogueToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // testsToolStripMenuItem
            // 
            this.testsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchDiagnosticsScreenToolStripMenuItem,
            this.showPerformanceCounterToolStripMenuItem});
            this.testsToolStripMenuItem.Name = "testsToolStripMenuItem";
            this.testsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.testsToolStripMenuItem.Text = "Diagnostics";
            // 
            // launchDiagnosticsScreenToolStripMenuItem
            // 
            this.launchDiagnosticsScreenToolStripMenuItem.Name = "launchDiagnosticsScreenToolStripMenuItem";
            this.launchDiagnosticsScreenToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.launchDiagnosticsScreenToolStripMenuItem.Text = "Launch Diagnostics Screen...";
            this.launchDiagnosticsScreenToolStripMenuItem.Click += new System.EventHandler(this.launchDiagnosticsScreenToolStripMenuItem_Click);
            // 
            // showPerformanceCounterToolStripMenuItem
            // 
            this.showPerformanceCounterToolStripMenuItem.Name = "showPerformanceCounterToolStripMenuItem";
            this.showPerformanceCounterToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.showPerformanceCounterToolStripMenuItem.Text = "Show Performance Counter...";
            this.showPerformanceCounterToolStripMenuItem.Click += new System.EventHandler(this.showPerformanceCounterToolStripMenuItem_Click);
            // 
            // CachingEngineUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1484, 747);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CachingEngineUI";
            this.Text = "Caching Engine UI";
            this.Load += new System.EventHandler(this.CachingEngineUI_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabSingleJob.ResumeLayout(false);
            this.tabSingleJob.PerformLayout();
            this.tabDaemon.ResumeLayout(false);
            this.tabCustomDate.ResumeLayout(false);
            this.tabCustomDate.PerformLayout();
            this.tabFailures.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddLoadProgress;
        private System.Windows.Forms.Button btnStartCaching;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI;
        private ReusableUIComponents.Progress.ProgressUI progressUI;
        private System.Windows.Forms.Button btnRunChecks;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnStopCaching;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnStartCacheDaemon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpStartDateToRetrieve;
        private System.Windows.Forms.Button btnStartSingleDateRetrieve;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSingleJob;
        private System.Windows.Forms.TabPage tabDaemon;
        private System.Windows.Forms.TabPage tabCustomDate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnShowCachingPipeline;
        private System.Windows.Forms.Button btnShowConfiguration;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeCatalogueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchDiagnosticsScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPerformanceCounterToolStripMenuItem;
        private System.Windows.Forms.TabPage tabFailures;
        private System.Windows.Forms.Button btnRetryFailures;
        private System.Windows.Forms.Button btnViewFailures;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dtpEndDateToRetrieve;
        private System.Windows.Forms.CheckBox cbIgnorePermissionWindow;
    }
}

