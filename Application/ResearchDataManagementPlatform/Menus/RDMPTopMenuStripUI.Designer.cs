using System;

namespace ResearchDataManagementPlatform.Menus
{
    partial class RDMPTopMenuStripUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDMPTopMenuStripUI));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LocationsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.configureExternalServersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setTicketingSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigateBackwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigateForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.issuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.metadataReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.governanceReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dITAExtractionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateTestDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.codeGenerationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listAllTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryCatalogue = new System.Windows.Forms.ToolStripMenuItem();
            this.queryDataExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showPerformanceCounterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openExeDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateClassTableSummaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.licenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rdmpTaskBar1 = new ResearchDataManagementPlatform.WindowManagement.TopBar.RDMPTaskBarUI();
            this.newSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.LocationsMenu,
            this.viewToolStripMenuItem,
            this.issuesToolStripMenuItem,
            this.testsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1160, 24);
            this.menuStrip1.TabIndex = 56;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.newSessionToolStripMenuItem,
            this.runToolStripMenuItem,
            this.openToolStripMenuItem,
            this.findToolStripMenuItem,
            this.findAndReplaceToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.newToolStripMenuItem.Text = "New...";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.runToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.runToolStripMenuItem.Text = "Run...";
            this.runToolStripMenuItem.Click += new System.EventHandler(this.runToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // findAndReplaceToolStripMenuItem
            // 
            this.findAndReplaceToolStripMenuItem.Name = "findAndReplaceToolStripMenuItem";
            this.findAndReplaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.findAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.findAndReplaceToolStripMenuItem.Text = "Find and Replace";
            this.findAndReplaceToolStripMenuItem.Click += new System.EventHandler(this.findAndReplaceToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // LocationsMenu
            // 
            this.LocationsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureExternalServersToolStripMenuItem,
            this.setTicketingSystemToolStripMenuItem});
            this.LocationsMenu.Name = "LocationsMenu";
            this.LocationsMenu.Size = new System.Drawing.Size(70, 20);
            this.LocationsMenu.Text = "Locations";
            // 
            // configureExternalServersToolStripMenuItem
            // 
            this.configureExternalServersToolStripMenuItem.Name = "configureExternalServersToolStripMenuItem";
            this.configureExternalServersToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.configureExternalServersToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.configureExternalServersToolStripMenuItem.Text = "Default Servers...";
            this.configureExternalServersToolStripMenuItem.Click += new System.EventHandler(this.configureExternalServersToolStripMenuItem_Click);
            // 
            // setTicketingSystemToolStripMenuItem
            // 
            this.setTicketingSystemToolStripMenuItem.Name = "setTicketingSystemToolStripMenuItem";
            this.setTicketingSystemToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.setTicketingSystemToolStripMenuItem.Text = "Set Ticketing System...";
            this.setTicketingSystemToolStripMenuItem.Click += new System.EventHandler(this.setTicketingSystemToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logViewerToolStripMenuItem,
            this.userSettingsToolStripMenuItem,
            this.navigateBackwardToolStripMenuItem,
            this.navigateForwardToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // logViewerToolStripMenuItem
            // 
            this.logViewerToolStripMenuItem.Name = "logViewerToolStripMenuItem";
            this.logViewerToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.logViewerToolStripMenuItem.Text = "Log Viewer...";
            this.logViewerToolStripMenuItem.Click += new System.EventHandler(this.logViewerToolStripMenuItem_Click);
            // 
            // userSettingsToolStripMenuItem
            // 
            this.userSettingsToolStripMenuItem.Name = "userSettingsToolStripMenuItem";
            this.userSettingsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.userSettingsToolStripMenuItem.Text = "User Settings...";
            this.userSettingsToolStripMenuItem.Click += new System.EventHandler(this.userSettingsToolStripMenuItem_Click);
            // 
            // navigateBackwardToolStripMenuItem
            // 
            this.navigateBackwardToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("navigateBackwardToolStripMenuItem.Image")));
            this.navigateBackwardToolStripMenuItem.Name = "navigateBackwardToolStripMenuItem";
            this.navigateBackwardToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+-";
            this.navigateBackwardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.navigateBackwardToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.navigateBackwardToolStripMenuItem.Text = "Navigate Backward";
            this.navigateBackwardToolStripMenuItem.Click += new System.EventHandler(this.navigateBackwardToolStripMenuItem_Click);
            // 
            // navigateForwardToolStripMenuItem
            // 
            this.navigateForwardToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("navigateForwardToolStripMenuItem.Image")));
            this.navigateForwardToolStripMenuItem.Name = "navigateForwardToolStripMenuItem";
            this.navigateForwardToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+-";
            this.navigateForwardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.OemMinus)));
            this.navigateForwardToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.navigateForwardToolStripMenuItem.Text = "Navigate Forward";
            this.navigateForwardToolStripMenuItem.Click += new System.EventHandler(this.navigateForwardToolStripMenuItem_Click);
            // 
            // issuesToolStripMenuItem
            // 
            this.issuesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateReportToolStripMenuItem});
            this.issuesToolStripMenuItem.Name = "issuesToolStripMenuItem";
            this.issuesToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.issuesToolStripMenuItem.Text = "Reports";
            // 
            // generateReportToolStripMenuItem
            // 
            this.generateReportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.metadataReportToolStripMenuItem,
            this.governanceReportToolStripMenuItem,
            this.dITAExtractionToolStripMenuItem});
            this.generateReportToolStripMenuItem.Name = "generateReportToolStripMenuItem";
            this.generateReportToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.generateReportToolStripMenuItem.Text = "Generate...";
            // 
            // metadataReportToolStripMenuItem
            // 
            this.metadataReportToolStripMenuItem.Name = "metadataReportToolStripMenuItem";
            this.metadataReportToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.metadataReportToolStripMenuItem.Text = "Metadata Report...";
            this.metadataReportToolStripMenuItem.Click += new System.EventHandler(this.metadataReportToolStripMenuItem_Click);
            // 
            // governanceReportToolStripMenuItem
            // 
            this.governanceReportToolStripMenuItem.Name = "governanceReportToolStripMenuItem";
            this.governanceReportToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.governanceReportToolStripMenuItem.Text = "Governance Report";
            this.governanceReportToolStripMenuItem.Click += new System.EventHandler(this.governanceReportToolStripMenuItem_Click);
            // 
            // dITAExtractionToolStripMenuItem
            // 
            this.dITAExtractionToolStripMenuItem.Name = "dITAExtractionToolStripMenuItem";
            this.dITAExtractionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.dITAExtractionToolStripMenuItem.Text = "DITA Extraction...";
            this.dITAExtractionToolStripMenuItem.Click += new System.EventHandler(this.dITAExtractionToolStripMenuItem_Click);
            // 
            // testsToolStripMenuItem
            // 
            this.testsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateTestDataToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.toolStripSeparator1,
            this.showPerformanceCounterToolStripMenuItem,
            this.openExeDirectoryToolStripMenuItem,
            this.queryToolStripMenuItem});
            this.testsToolStripMenuItem.Name = "testsToolStripMenuItem";
            this.testsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.testsToolStripMenuItem.Text = "Diagnostics";
            // 
            // generateTestDataToolStripMenuItem
            // 
            this.generateTestDataToolStripMenuItem.Name = "generateTestDataToolStripMenuItem";
            this.generateTestDataToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.generateTestDataToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.generateTestDataToolStripMenuItem.Text = "Generate Test Data...";
            this.generateTestDataToolStripMenuItem.Click += new System.EventHandler(this.generateTestDataToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.codeGenerationToolStripMenuItem,
            this.listAllTypesToolStripMenuItem});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            // 
            // queryToolStripMenuItem
            // 
            this.queryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
               this.queryCatalogue,
               this.queryDataExport
            });
            this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
            this.queryToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.queryToolStripMenuItem.Text = "Query";
            // 
            // codeGenerationToolStripMenuItem
            // 
            this.codeGenerationToolStripMenuItem.Name = "codeGenerationToolStripMenuItem";
            this.codeGenerationToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.codeGenerationToolStripMenuItem.Text = "Code Generation...";
            this.codeGenerationToolStripMenuItem.Click += new System.EventHandler(this.codeGenerationToolStripMenuItem_Click);


            // 
            // queryCatalogue
            // 
            this.queryCatalogue.Name = "queryCatalogue";
            this.queryCatalogue.Size = new System.Drawing.Size(172, 22);
            this.queryCatalogue.Text = "Catalogue...";
            this.queryCatalogue.Click += new System.EventHandler(this.queryCatalogue_Click);
            // 
            // queryDataExport
            // 
            this.queryDataExport.Name = "queryDataExport";
            this.queryDataExport.Size = new System.Drawing.Size(172, 22);
            this.queryDataExport.Text = "Data Export...";
            this.queryDataExport.Click += new System.EventHandler(this.queryDataExport_Click);
            // 
            // listAllTypesToolStripMenuItem
            // 
            this.listAllTypesToolStripMenuItem.Name = "listAllTypesToolStripMenuItem";
            this.listAllTypesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.listAllTypesToolStripMenuItem.Text = "List All Types";
            this.listAllTypesToolStripMenuItem.Click += new System.EventHandler(this.ListAllTypesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // showPerformanceCounterToolStripMenuItem
            // 
            this.showPerformanceCounterToolStripMenuItem.Name = "showPerformanceCounterToolStripMenuItem";
            this.showPerformanceCounterToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.showPerformanceCounterToolStripMenuItem.Text = "Show Performance Counter...";
            this.showPerformanceCounterToolStripMenuItem.Click += new System.EventHandler(this.showPerformanceCounterToolStripMenuItem_Click);
            // 
            // openExeDirectoryToolStripMenuItem
            // 
            this.openExeDirectoryToolStripMenuItem.Name = "openExeDirectoryToolStripMenuItem";
            this.openExeDirectoryToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.openExeDirectoryToolStripMenuItem.Text = "Open exe Directory";
            this.openExeDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openExeDirectoryToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userManualToolStripMenuItem,
            this.generateClassTableSummaryToolStripMenuItem,
            this.showHelpToolStripMenuItem,
            this.tutorialsToolStripMenuItem,
            this.licenseToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.userManualToolStripMenuItem.Text = "Show User Manual";
            this.userManualToolStripMenuItem.Click += new System.EventHandler(this.userManualToolStripMenuItem_Click);
            // 
            // generateClassTableSummaryToolStripMenuItem
            // 
            this.generateClassTableSummaryToolStripMenuItem.Name = "generateClassTableSummaryToolStripMenuItem";
            this.generateClassTableSummaryToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.generateClassTableSummaryToolStripMenuItem.Text = "Show Objects Help";
            this.generateClassTableSummaryToolStripMenuItem.ToolTipText = "Lists all RDMP objects (e.g. Catalogue) and what they model within the system.";
            this.generateClassTableSummaryToolStripMenuItem.Click += new System.EventHandler(this.generateClassTableSummaryToolStripMenuItem_Click);
            // 
            // showHelpToolStripMenuItem
            // 
            this.showHelpToolStripMenuItem.Name = "showHelpToolStripMenuItem";
            this.showHelpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.showHelpToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.showHelpToolStripMenuItem.Text = "Show Help";
            this.showHelpToolStripMenuItem.Click += new System.EventHandler(this.showHelpToolStripMenuItem_Click);
            // 
            // tutorialsToolStripMenuItem
            // 
            this.tutorialsToolStripMenuItem.Name = "tutorialsToolStripMenuItem";
            this.tutorialsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.tutorialsToolStripMenuItem.Text = "Tutorials";
            // 
            // licenseToolStripMenuItem
            // 
            this.licenseToolStripMenuItem.Name = "licenseToolStripMenuItem";
            this.licenseToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.licenseToolStripMenuItem.Text = "License";
            this.licenseToolStripMenuItem.Click += new System.EventHandler(this.licenseToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // rdmpTaskBar1
            // 
            this.rdmpTaskBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpTaskBar1.Location = new System.Drawing.Point(0, 24);
            this.rdmpTaskBar1.Name = "rdmpTaskBar1";
            this.rdmpTaskBar1.Size = new System.Drawing.Size(1160, 25);
            this.rdmpTaskBar1.TabIndex = 57;
            // 
            // newSessionToolStripMenuItem
            // 
            this.newSessionToolStripMenuItem.Name = "newSessionToolStripMenuItem";
            this.newSessionToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.newSessionToolStripMenuItem.Text = "New Session";
            this.newSessionToolStripMenuItem.Click += new System.EventHandler(this.newSessionToolStripMenuItem_Click);
            // 
            // RDMPTopMenuStripUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rdmpTaskBar1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "RDMPTopMenuStripUI";
            this.Size = new System.Drawing.Size(1160, 48);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ToolStripMenuItem LocationsMenu;
        private System.Windows.Forms.ToolStripMenuItem configureExternalServersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTicketingSystemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem issuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem governanceReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dITAExtractionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateTestDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showPerformanceCounterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openExeDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateClassTableSummaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queryCatalogue;
        private System.Windows.Forms.ToolStripMenuItem queryDataExport;
        private System.Windows.Forms.ToolStripMenuItem codeGenerationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tutorialsToolStripMenuItem;
        private WindowManagement.TopBar.RDMPTaskBarUI rdmpTaskBar1;
        private System.Windows.Forms.ToolStripMenuItem userSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem licenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findAndReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem navigateBackwardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem navigateForwardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listAllTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newSessionToolStripMenuItem;
    }
}
