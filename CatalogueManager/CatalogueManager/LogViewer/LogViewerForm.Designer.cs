using CatalogueManager.LogViewer.Tabs;

namespace CatalogueManager.LogViewer
{
    partial class LogViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogViewerForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tbCurrentFilter = new System.Windows.Forms.TextBox();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpTasks = new System.Windows.Forms.TabPage();
            this.tpRuns = new System.Windows.Forms.TabPage();
            this.tpTableLoads = new System.Windows.Forms.TabPage();
            this.tpDataSources = new System.Windows.Forms.TabPage();
            this.tpFatalErrors = new System.Windows.Forms.TabPage();
            this.tpProgressMessages = new System.Windows.Forms.TabPage();
            this.breadcrumbNavigation = new CatalogueManager.LogViewer.BreadcrumbNavigation();
            this.loggingTasks = new CatalogueManager.LogViewer.Tabs.LoggingTasksTab();
            this.loggingRunsTab1 = new CatalogueManager.LogViewer.Tabs.LoggingRunsTab();
            this.loggingTableLoadsTab1 = new CatalogueManager.LogViewer.Tabs.LoggingTableLoadsTab();
            this.loggingDataSources = new CatalogueManager.LogViewer.Tabs.LoggingDataSourcesTab();
            this.loggingFatalErrors = new CatalogueManager.LogViewer.Tabs.LoggingFatalErrorsTab();
            this.loggingProgressMessagesTab1 = new CatalogueManager.LogViewer.Tabs.LoggingProgressMessagesTab();
            this.logViewerNavigationPane = new CatalogueManager.LogViewer.LogViewerNavigationPane();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpTasks.SuspendLayout();
            this.tpRuns.SuspendLayout();
            this.tpTableLoads.SuspendLayout();
            this.tpDataSources.SuspendLayout();
            this.tpFatalErrors.SuspendLayout();
            this.tpProgressMessages.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 767);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(227, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Current Filter (double click rows to add to filter):";
            // 
            // tbCurrentFilter
            // 
            this.tbCurrentFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCurrentFilter.Location = new System.Drawing.Point(1, 783);
            this.tbCurrentFilter.Name = "tbCurrentFilter";
            this.tbCurrentFilter.ReadOnly = true;
            this.tbCurrentFilter.Size = new System.Drawing.Size(1333, 20);
            this.tbCurrentFilter.TabIndex = 3;
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFilter.Location = new System.Drawing.Point(1340, 783);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(59, 23);
            this.btnClearFilter.TabIndex = 4;
            this.btnClearFilter.Text = "Clear";
            this.btnClearFilter.UseVisualStyleBackColor = true;
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 1);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.breadcrumbNavigation);
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.logViewerNavigationPane);
            this.splitContainer1.Size = new System.Drawing.Size(1398, 763);
            this.splitContainer1.SplitterDistance = 1015;
            this.splitContainer1.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpTasks);
            this.tabControl1.Controls.Add(this.tpRuns);
            this.tabControl1.Controls.Add(this.tpTableLoads);
            this.tabControl1.Controls.Add(this.tpDataSources);
            this.tabControl1.Controls.Add(this.tpFatalErrors);
            this.tabControl1.Controls.Add(this.tpProgressMessages);
            this.tabControl1.Location = new System.Drawing.Point(0, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1015, 736);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpTasks
            // 
            this.tpTasks.Controls.Add(this.loggingTasks);
            this.tpTasks.Location = new System.Drawing.Point(4, 4);
            this.tpTasks.Name = "tpTasks";
            this.tpTasks.Padding = new System.Windows.Forms.Padding(3);
            this.tpTasks.Size = new System.Drawing.Size(1007, 710);
            this.tpTasks.TabIndex = 0;
            this.tpTasks.Text = "Tasks";
            this.tpTasks.UseVisualStyleBackColor = true;
            // 
            // tpRuns
            // 
            this.tpRuns.Controls.Add(this.loggingRunsTab1);
            this.tpRuns.Location = new System.Drawing.Point(4, 4);
            this.tpRuns.Name = "tpRuns";
            this.tpRuns.Padding = new System.Windows.Forms.Padding(3);
            this.tpRuns.Size = new System.Drawing.Size(1007, 710);
            this.tpRuns.TabIndex = 1;
            this.tpRuns.Text = "Runs";
            this.tpRuns.UseVisualStyleBackColor = true;
            // 
            // tpTableLoads
            // 
            this.tpTableLoads.Controls.Add(this.loggingTableLoadsTab1);
            this.tpTableLoads.Location = new System.Drawing.Point(4, 4);
            this.tpTableLoads.Name = "tpTableLoads";
            this.tpTableLoads.Padding = new System.Windows.Forms.Padding(3);
            this.tpTableLoads.Size = new System.Drawing.Size(1007, 710);
            this.tpTableLoads.TabIndex = 2;
            this.tpTableLoads.Text = "Table Loads";
            this.tpTableLoads.UseVisualStyleBackColor = true;
            // 
            // tpDataSources
            // 
            this.tpDataSources.Controls.Add(this.loggingDataSources);
            this.tpDataSources.Location = new System.Drawing.Point(4, 4);
            this.tpDataSources.Name = "tpDataSources";
            this.tpDataSources.Padding = new System.Windows.Forms.Padding(3);
            this.tpDataSources.Size = new System.Drawing.Size(1007, 710);
            this.tpDataSources.TabIndex = 3;
            this.tpDataSources.Text = "Data Sources";
            this.tpDataSources.UseVisualStyleBackColor = true;
            // 
            // tpFatalErrors
            // 
            this.tpFatalErrors.Controls.Add(this.loggingFatalErrors);
            this.tpFatalErrors.Location = new System.Drawing.Point(4, 4);
            this.tpFatalErrors.Name = "tpFatalErrors";
            this.tpFatalErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tpFatalErrors.Size = new System.Drawing.Size(1007, 710);
            this.tpFatalErrors.TabIndex = 4;
            this.tpFatalErrors.Text = "Fatal Errors";
            this.tpFatalErrors.UseVisualStyleBackColor = true;
            // 
            // tpProgressMessages
            // 
            this.tpProgressMessages.Controls.Add(this.loggingProgressMessagesTab1);
            this.tpProgressMessages.Location = new System.Drawing.Point(4, 4);
            this.tpProgressMessages.Name = "tpProgressMessages";
            this.tpProgressMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tpProgressMessages.Size = new System.Drawing.Size(1007, 710);
            this.tpProgressMessages.TabIndex = 5;
            this.tpProgressMessages.Text = "Progress Messages";
            this.tpProgressMessages.UseVisualStyleBackColor = true;
            // 
            // breadcrumbNavigation
            // 
            this.breadcrumbNavigation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.breadcrumbNavigation.AutoSize = true;
            this.breadcrumbNavigation.Location = new System.Drawing.Point(0, 0);
            this.breadcrumbNavigation.Name = "breadcrumbNavigation";
            this.breadcrumbNavigation.Size = new System.Drawing.Size(1015, 28);
            this.breadcrumbNavigation.TabIndex = 1;
            // 
            // loggingTasks
            // 
            this.loggingTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingTasks.Location = new System.Drawing.Point(3, 3);
            this.loggingTasks.Name = "loggingTasks";
            this.loggingTasks.Size = new System.Drawing.Size(1001, 704);
            this.loggingTasks.TabIndex = 0;
            // 
            // loggingRunsTab1
            // 
            this.loggingRunsTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingRunsTab1.Location = new System.Drawing.Point(3, 3);
            this.loggingRunsTab1.Name = "loggingRunsTab1";
            this.loggingRunsTab1.Size = new System.Drawing.Size(1001, 704);
            this.loggingRunsTab1.TabIndex = 0;
            // 
            // loggingTableLoadsTab1
            // 
            this.loggingTableLoadsTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingTableLoadsTab1.Location = new System.Drawing.Point(3, 3);
            this.loggingTableLoadsTab1.Name = "loggingTableLoadsTab1";
            this.loggingTableLoadsTab1.Size = new System.Drawing.Size(1001, 704);
            this.loggingTableLoadsTab1.TabIndex = 0;
            // 
            // loggingDataSources
            // 
            this.loggingDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingDataSources.Location = new System.Drawing.Point(3, 3);
            this.loggingDataSources.Name = "loggingDataSources";
            this.loggingDataSources.Size = new System.Drawing.Size(1001, 704);
            this.loggingDataSources.TabIndex = 0;
            // 
            // loggingFatalErrors
            // 
            this.loggingFatalErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingFatalErrors.Location = new System.Drawing.Point(3, 3);
            this.loggingFatalErrors.Name = "loggingFatalErrors";
            this.loggingFatalErrors.Size = new System.Drawing.Size(1001, 704);
            this.loggingFatalErrors.TabIndex = 1;
            // 
            // loggingProgressMessagesTab1
            // 
            this.loggingProgressMessagesTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggingProgressMessagesTab1.Location = new System.Drawing.Point(3, 3);
            this.loggingProgressMessagesTab1.Name = "loggingProgressMessagesTab1";
            this.loggingProgressMessagesTab1.Size = new System.Drawing.Size(1001, 704);
            this.loggingProgressMessagesTab1.TabIndex = 0;
            // 
            // logViewerNavigationPane
            // 
            this.logViewerNavigationPane.AutoSize = true;
            this.logViewerNavigationPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logViewerNavigationPane.Location = new System.Drawing.Point(0, 0);
            this.logViewerNavigationPane.Name = "logViewerNavigationPane";
            this.logViewerNavigationPane.Size = new System.Drawing.Size(379, 763);
            this.logViewerNavigationPane.TabIndex = 0;
            // 
            // LogViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1404, 815);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnClearFilter);
            this.Controls.Add(this.tbCurrentFilter);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogViewerForm";
            this.Text = "Log Viewer";
            this.Load += new System.EventHandler(this.LogViewerForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tpTasks.ResumeLayout(false);
            this.tpRuns.ResumeLayout(false);
            this.tpTableLoads.ResumeLayout(false);
            this.tpDataSources.ResumeLayout(false);
            this.tpFatalErrors.ResumeLayout(false);
            this.tpProgressMessages.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpTasks;
        private System.Windows.Forms.TabPage tpRuns;
        private System.Windows.Forms.TabPage tpTableLoads;
        private System.Windows.Forms.TabPage tpFatalErrors;
        private System.Windows.Forms.TabPage tpProgressMessages;
        private System.Windows.Forms.TabPage tpDataSources;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbCurrentFilter;
        private LoggingTasksTab loggingTasks;
        private LoggingRunsTab loggingRunsTab1;
        private System.Windows.Forms.Button btnClearFilter;
        private LoggingTableLoadsTab loggingTableLoadsTab1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private LogViewerNavigationPane logViewerNavigationPane;
        private LoggingDataSourcesTab loggingDataSources;
        private LoggingFatalErrorsTab loggingFatalErrors;
        private LoggingProgressMessagesTab loggingProgressMessagesTab1;
        private BreadcrumbNavigation breadcrumbNavigation;
    }
}