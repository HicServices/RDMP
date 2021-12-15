namespace Rdmp.UI.SimpleDialogs
{
    partial class UserSettingsFileUI
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
            this.components = new System.ComponentModel.Container();
            this.cbShowHomeOnStartup = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbEmphasiseOnTabChanged = new System.Windows.Forms.CheckBox();
            this.cbConfirmExit = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbThemeMenus = new System.Windows.Forms.CheckBox();
            this.ddTheme = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ddWordWrap = new System.Windows.Forms.ComboBox();
            this.cbFindShouldPin = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbHeatmapColours = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbWait5Seconds = new System.Windows.Forms.CheckBox();
            this.cbShowCohortWizard = new System.Windows.Forms.CheckBox();
            this.btnClearFavourites = new System.Windows.Forms.Button();
            this.cbDoubleClickToExpand = new System.Windows.Forms.CheckBox();
            this.cbDebugPerformance = new System.Windows.Forms.CheckBox();
            this.cbAllowIdentifiableExtractions = new System.Windows.Forms.CheckBox();
            this.cbShowPipelineCompletedPopup = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.olvErrorCodes = new BrightIdeasSoftware.ObjectListView();
            this.olvCode = new BrightIdeasSoftware.OLVColumn();
            this.olvTreatment = new BrightIdeasSoftware.OLVColumn();
            this.olvMessage = new BrightIdeasSoftware.OLVColumn();
            this.cbHideEmptyTableLoadRunAudits = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbCreateDatabaseTimeout = new System.Windows.Forms.TextBox();
            this.cbScoreZeroForCohortAggregateContainers = new System.Windows.Forms.CheckBox();
            this.cbAdvancedFindFilters = new System.Windows.Forms.CheckBox();
            this.cbIncludeZeroSeriesInGraphs = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbTooltipAppearDelay = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.userSettingsToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvErrorCodes)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbShowHomeOnStartup
            // 
            this.cbShowHomeOnStartup.AutoSize = true;
            this.cbShowHomeOnStartup.Location = new System.Drawing.Point(8, 22);
            this.cbShowHomeOnStartup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbShowHomeOnStartup.Name = "cbShowHomeOnStartup";
            this.cbShowHomeOnStartup.Size = new System.Drawing.Size(149, 19);
            this.cbShowHomeOnStartup.TabIndex = 0;
            this.cbShowHomeOnStartup.Text = "Show Home on Startup";
            this.cbShowHomeOnStartup.UseVisualStyleBackColor = true;
            this.cbShowHomeOnStartup.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Settings will automatically be Saved as you change them ";
            // 
            // cbEmphasiseOnTabChanged
            // 
            this.cbEmphasiseOnTabChanged.AutoSize = true;
            this.cbEmphasiseOnTabChanged.Location = new System.Drawing.Point(7, 47);
            this.cbEmphasiseOnTabChanged.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbEmphasiseOnTabChanged.Name = "cbEmphasiseOnTabChanged";
            this.cbEmphasiseOnTabChanged.Size = new System.Drawing.Size(232, 19);
            this.cbEmphasiseOnTabChanged.TabIndex = 2;
            this.cbEmphasiseOnTabChanged.Text = "Show Object Collection on Tab Change";
            this.cbEmphasiseOnTabChanged.UseVisualStyleBackColor = true;
            this.cbEmphasiseOnTabChanged.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbConfirmExit
            // 
            this.cbConfirmExit.AutoSize = true;
            this.cbConfirmExit.Location = new System.Drawing.Point(7, 72);
            this.cbConfirmExit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbConfirmExit.Name = "cbConfirmExit";
            this.cbConfirmExit.Size = new System.Drawing.Size(156, 19);
            this.cbConfirmExit.TabIndex = 2;
            this.cbConfirmExit.Text = "Confirm Application Exit";
            this.cbConfirmExit.UseVisualStyleBackColor = true;
            this.cbConfirmExit.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Theme*:";
            // 
            // cbThemeMenus
            // 
            this.cbThemeMenus.AutoSize = true;
            this.cbThemeMenus.Location = new System.Drawing.Point(514, 38);
            this.cbThemeMenus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbThemeMenus.Name = "cbThemeMenus";
            this.cbThemeMenus.Size = new System.Drawing.Size(150, 19);
            this.cbThemeMenus.TabIndex = 4;
            this.cbThemeMenus.Text = "Apply Theme To Menus";
            this.cbThemeMenus.UseVisualStyleBackColor = true;
            this.cbThemeMenus.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // ddTheme
            // 
            this.ddTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddTheme.FormattingEnabled = true;
            this.ddTheme.Location = new System.Drawing.Point(74, 36);
            this.ddTheme.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddTheme.Name = "ddTheme";
            this.ddTheme.Size = new System.Drawing.Size(432, 23);
            this.ddTheme.TabIndex = 5;
            this.ddTheme.SelectedIndexChanged += new System.EventHandler(this.ddTheme_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 750);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "*Requires restart";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 97);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "SQL Word Wrap:";
            // 
            // ddWordWrap
            // 
            this.ddWordWrap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddWordWrap.FormattingEnabled = true;
            this.ddWordWrap.Location = new System.Drawing.Point(153, 94);
            this.ddWordWrap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddWordWrap.Name = "ddWordWrap";
            this.ddWordWrap.Size = new System.Drawing.Size(100, 23);
            this.ddWordWrap.TabIndex = 7;
            this.ddWordWrap.SelectedIndexChanged += new System.EventHandler(this.ddWordWrap_SelectedIndexChanged);
            // 
            // cbFindShouldPin
            // 
            this.cbFindShouldPin.AutoSize = true;
            this.cbFindShouldPin.Location = new System.Drawing.Point(7, 22);
            this.cbFindShouldPin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbFindShouldPin.Name = "cbFindShouldPin";
            this.cbFindShouldPin.Size = new System.Drawing.Size(108, 19);
            this.cbFindShouldPin.TabIndex = 2;
            this.cbFindShouldPin.Text = "Find should Pin";
            this.cbFindShouldPin.UseVisualStyleBackColor = true;
            this.cbFindShouldPin.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 47);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Heatmap Colours:";
            // 
            // tbHeatmapColours
            // 
            this.tbHeatmapColours.Location = new System.Drawing.Point(107, 43);
            this.tbHeatmapColours.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbHeatmapColours.Name = "tbHeatmapColours";
            this.tbHeatmapColours.Size = new System.Drawing.Size(100, 23);
            this.tbHeatmapColours.TabIndex = 9;
            this.tbHeatmapColours.TextChanged += new System.EventHandler(this.TbHeatmapColours_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(53, 70);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(158, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "(Format: #000000->#FFFFFF)";
            // 
            // cbWait5Seconds
            // 
            this.cbWait5Seconds.AutoSize = true;
            this.cbWait5Seconds.Location = new System.Drawing.Point(7, 47);
            this.cbWait5Seconds.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbWait5Seconds.Name = "cbWait5Seconds";
            this.cbWait5Seconds.Size = new System.Drawing.Size(173, 19);
            this.cbWait5Seconds.TabIndex = 2;
            this.cbWait5Seconds.Text = "Wait 5 seconds after Startup";
            this.cbWait5Seconds.UseVisualStyleBackColor = true;
            this.cbWait5Seconds.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbShowCohortWizard
            // 
            this.cbShowCohortWizard.AutoSize = true;
            this.cbShowCohortWizard.Location = new System.Drawing.Point(7, 45);
            this.cbShowCohortWizard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbShowCohortWizard.Name = "cbShowCohortWizard";
            this.cbShowCohortWizard.Size = new System.Drawing.Size(134, 19);
            this.cbShowCohortWizard.TabIndex = 2;
            this.cbShowCohortWizard.Text = "Show Cohort Wizard";
            this.cbShowCohortWizard.UseVisualStyleBackColor = true;
            this.cbShowCohortWizard.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // btnClearFavourites
            // 
            this.btnClearFavourites.Location = new System.Drawing.Point(71, 73);
            this.btnClearFavourites.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearFavourites.Name = "btnClearFavourites";
            this.btnClearFavourites.Size = new System.Drawing.Size(100, 25);
            this.btnClearFavourites.TabIndex = 10;
            this.btnClearFavourites.Text = "Clear Favourites";
            this.userSettingsToolTips.SetToolTip(this.btnClearFavourites, "Clear all the Favourites (items that have been \'started\') from your collection vi" +
        "ews.");
            this.btnClearFavourites.UseVisualStyleBackColor = true;
            // 
            // cbDoubleClickToExpand
            // 
            this.cbDoubleClickToExpand.AutoSize = true;
            this.cbDoubleClickToExpand.Location = new System.Drawing.Point(7, 22);
            this.cbDoubleClickToExpand.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbDoubleClickToExpand.Name = "cbDoubleClickToExpand";
            this.cbDoubleClickToExpand.Size = new System.Drawing.Size(149, 19);
            this.cbDoubleClickToExpand.TabIndex = 0;
            this.cbDoubleClickToExpand.Text = "Double Click to Expand";
            this.cbDoubleClickToExpand.UseVisualStyleBackColor = true;
            this.cbDoubleClickToExpand.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbDebugPerformance
            // 
            this.cbDebugPerformance.AutoSize = true;
            this.cbDebugPerformance.Location = new System.Drawing.Point(7, 23);
            this.cbDebugPerformance.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbDebugPerformance.Name = "cbDebugPerformance";
            this.cbDebugPerformance.Size = new System.Drawing.Size(319, 19);
            this.cbDebugPerformance.TabIndex = 11;
            this.cbDebugPerformance.Text = "Record Performance Metrics (local data collection only)";
            this.cbDebugPerformance.UseVisualStyleBackColor = true;
            this.cbDebugPerformance.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbAllowIdentifiableExtractions
            // 
            this.cbAllowIdentifiableExtractions.AutoSize = true;
            this.cbAllowIdentifiableExtractions.Location = new System.Drawing.Point(7, 20);
            this.cbAllowIdentifiableExtractions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbAllowIdentifiableExtractions.Name = "cbAllowIdentifiableExtractions";
            this.cbAllowIdentifiableExtractions.Size = new System.Drawing.Size(179, 19);
            this.cbAllowIdentifiableExtractions.TabIndex = 13;
            this.cbAllowIdentifiableExtractions.Text = "Allow Identifiable Extractions";
            this.cbAllowIdentifiableExtractions.UseVisualStyleBackColor = true;
            this.cbAllowIdentifiableExtractions.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbShowPipelineCompletedPopup
            // 
            this.cbShowPipelineCompletedPopup.AutoSize = true;
            this.cbShowPipelineCompletedPopup.Location = new System.Drawing.Point(7, 72);
            this.cbShowPipelineCompletedPopup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbShowPipelineCompletedPopup.Name = "cbShowPipelineCompletedPopup";
            this.cbShowPipelineCompletedPopup.Size = new System.Drawing.Size(200, 19);
            this.cbShowPipelineCompletedPopup.TabIndex = 15;
            this.cbShowPipelineCompletedPopup.Text = "Show Pipeline Completed Popup";
            this.cbShowPipelineCompletedPopup.UseVisualStyleBackColor = true;
            this.cbShowPipelineCompletedPopup.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.Controls.Add(this.olvErrorCodes);
            this.groupBox1.Location = new System.Drawing.Point(359, 345);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(376, 320);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Warnings";
            this.userSettingsToolTips.SetToolTip(this.groupBox1, "Change the treatment of warnings in RDMP that result from Checks.\r\n\r\nSuccess and " +
        "Warning will not block.\r\nFail will block the process from proceeding.");
            // 
            // olvErrorCodes
            // 
            this.olvErrorCodes.AllColumns.Add(this.olvCode);
            this.olvErrorCodes.AllColumns.Add(this.olvTreatment);
            this.olvErrorCodes.AllColumns.Add(this.olvMessage);
            this.olvErrorCodes.CellEditUseWholeCell = false;
            this.olvErrorCodes.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvErrorCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvErrorCodes.HideSelection = false;
            this.olvErrorCodes.Location = new System.Drawing.Point(3, 19);
            this.olvErrorCodes.Name = "olvErrorCodes";
            this.olvErrorCodes.Size = new System.Drawing.Size(370, 298);
            this.olvErrorCodes.TabIndex = 0;
            this.olvErrorCodes.View = System.Windows.Forms.View.Details;
            // 
            // cbHideEmptyTableLoadRunAudits
            // 
            this.cbHideEmptyTableLoadRunAudits.AutoSize = true;
            this.cbHideEmptyTableLoadRunAudits.Location = new System.Drawing.Point(7, 47);
            this.cbHideEmptyTableLoadRunAudits.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbHideEmptyTableLoadRunAudits.Name = "cbHideEmptyTableLoadRunAudits";
            this.cbHideEmptyTableLoadRunAudits.Size = new System.Drawing.Size(208, 19);
            this.cbHideEmptyTableLoadRunAudits.TabIndex = 15;
            this.cbHideEmptyTableLoadRunAudits.Text = "Hide Empty Table Load Run Audits";
            this.cbHideEmptyTableLoadRunAudits.UseVisualStyleBackColor = true;
            this.cbHideEmptyTableLoadRunAudits.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 72);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(142, 15);
            this.label7.TabIndex = 17;
            this.label7.Text = "Create Database Timeout:";
            // 
            // tbCreateDatabaseTimeout
            // 
            this.tbCreateDatabaseTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCreateDatabaseTimeout.Location = new System.Drawing.Point(153, 68);
            this.tbCreateDatabaseTimeout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbCreateDatabaseTimeout.Name = "tbCreateDatabaseTimeout";
            this.tbCreateDatabaseTimeout.Size = new System.Drawing.Size(100, 23);
            this.tbCreateDatabaseTimeout.TabIndex = 18;
            this.tbCreateDatabaseTimeout.TextChanged += new System.EventHandler(this.tbCreateDatabaseTimeout_TextChanged);
            // 
            // cbScoreZeroForCohortAggregateContainers
            // 
            this.cbScoreZeroForCohortAggregateContainers.AutoSize = true;
            this.cbScoreZeroForCohortAggregateContainers.Location = new System.Drawing.Point(7, 72);
            this.cbScoreZeroForCohortAggregateContainers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbScoreZeroForCohortAggregateContainers.Name = "cbScoreZeroForCohortAggregateContainers";
            this.cbScoreZeroForCohortAggregateContainers.Size = new System.Drawing.Size(230, 19);
            this.cbScoreZeroForCohortAggregateContainers.TabIndex = 15;
            this.cbScoreZeroForCohortAggregateContainers.Text = "Hide Cohort Builder Containers in Find";
            this.cbScoreZeroForCohortAggregateContainers.UseVisualStyleBackColor = true;
            this.cbScoreZeroForCohortAggregateContainers.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbAdvancedFindFilters
            // 
            this.cbAdvancedFindFilters.AutoSize = true;
            this.cbAdvancedFindFilters.Location = new System.Drawing.Point(7, 47);
            this.cbAdvancedFindFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbAdvancedFindFilters.Name = "cbAdvancedFindFilters";
            this.cbAdvancedFindFilters.Size = new System.Drawing.Size(171, 19);
            this.cbAdvancedFindFilters.TabIndex = 19;
            this.cbAdvancedFindFilters.Text = "Show Advanced Find Filters";
            this.cbAdvancedFindFilters.UseVisualStyleBackColor = true;
            this.cbAdvancedFindFilters.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbIncludeZeroSeriesInGraphs
            // 
            this.cbIncludeZeroSeriesInGraphs.AutoSize = true;
            this.cbIncludeZeroSeriesInGraphs.Location = new System.Drawing.Point(7, 22);
            this.cbIncludeZeroSeriesInGraphs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbIncludeZeroSeriesInGraphs.Name = "cbIncludeZeroSeriesInGraphs";
            this.cbIncludeZeroSeriesInGraphs.Size = new System.Drawing.Size(178, 19);
            this.cbIncludeZeroSeriesInGraphs.TabIndex = 19;
            this.cbIncludeZeroSeriesInGraphs.Text = "Include Zero Series in Graphs";
            this.cbIncludeZeroSeriesInGraphs.UseVisualStyleBackColor = true;
            this.cbIncludeZeroSeriesInGraphs.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbShowHomeOnStartup);
            this.groupBox2.Controls.Add(this.cbWait5Seconds);
            this.groupBox2.Controls.Add(this.cbConfirmExit);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(240, 165);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Application Startup / Shutdown";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.cbDoubleClickToExpand);
            this.groupBox3.Controls.Add(this.cbEmphasiseOnTabChanged);
            this.groupBox3.Controls.Add(this.btnClearFavourites);
            this.groupBox3.Location = new System.Drawing.Point(249, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(240, 165);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tabbed UI Options";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 77);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 15);
            this.label8.TabIndex = 19;
            this.label8.Text = "Favourites:";
            this.userSettingsToolTips.SetToolTip(this.label8, "Clear all the Favourites (items that have been \'started\') from your collection vi" +
        "ews.");
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbScoreZeroForCohortAggregateContainers);
            this.groupBox4.Controls.Add(this.cbAdvancedFindFilters);
            this.groupBox4.Controls.Add(this.cbFindShouldPin);
            this.groupBox4.Location = new System.Drawing.Point(495, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(240, 165);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Find (CTRL + F) Options";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cbIncludeZeroSeriesInGraphs);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.tbHeatmapColours);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Location = new System.Drawing.Point(3, 174);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(240, 165);
            this.groupBox5.TabIndex = 23;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Graphing";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Controls.Add(this.groupBox3);
            this.flowLayoutPanel1.Controls.Add(this.groupBox4);
            this.flowLayoutPanel1.Controls.Add(this.groupBox5);
            this.flowLayoutPanel1.Controls.Add(this.groupBox6);
            this.flowLayoutPanel1.Controls.Add(this.groupBox7);
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 69);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(756, 681);
            this.flowLayoutPanel1.TabIndex = 24;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.cbAllowIdentifiableExtractions);
            this.groupBox6.Controls.Add(this.cbShowPipelineCompletedPopup);
            this.groupBox6.Controls.Add(this.cbHideEmptyTableLoadRunAudits);
            this.groupBox6.Location = new System.Drawing.Point(249, 174);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(240, 165);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Pipeline Components";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label11);
            this.groupBox7.Controls.Add(this.tbTooltipAppearDelay);
            this.groupBox7.Controls.Add(this.label10);
            this.groupBox7.Controls.Add(this.label9);
            this.groupBox7.Controls.Add(this.cbDebugPerformance);
            this.groupBox7.Controls.Add(this.tbCreateDatabaseTimeout);
            this.groupBox7.Controls.Add(this.ddWordWrap);
            this.groupBox7.Controls.Add(this.label4);
            this.groupBox7.Controls.Add(this.label7);
            this.groupBox7.Controls.Add(this.cbShowCohortWizard);
            this.groupBox7.Location = new System.Drawing.Point(3, 345);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(350, 165);
            this.groupBox7.TabIndex = 25;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Miscellaneous";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(260, 124);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(73, 15);
            this.label11.TabIndex = 22;
            this.label11.Text = "milliseconds";
            // 
            // tbTooltipAppearDelay
            // 
            this.tbTooltipAppearDelay.Location = new System.Drawing.Point(153, 121);
            this.tbTooltipAppearDelay.Name = "tbTooltipAppearDelay";
            this.tbTooltipAppearDelay.Size = new System.Drawing.Size(100, 23);
            this.tbTooltipAppearDelay.TabIndex = 21;
            this.tbTooltipAppearDelay.TextChanged += new System.EventHandler(this.tbTooltipAppearDelay_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(21, 124);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(124, 15);
            this.label10.TabIndex = 20;
            this.label10.Text = "Tooltip Appear Delay*:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(260, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 15);
            this.label9.TabIndex = 19;
            this.label9.Text = "seconds";
            // 
            // userSettingsToolTips
            // 
            this.userSettingsToolTips.AutomaticDelay = 250;
            this.userSettingsToolTips.AutoPopDelay = 30000;
            this.userSettingsToolTips.InitialDelay = 250;
            this.userSettingsToolTips.IsBalloon = true;
            this.userSettingsToolTips.ReshowDelay = 50;
            // 
            // UserSettingsFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 768);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.ddTheme);
            this.Controls.Add(this.cbThemeMenus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "UserSettingsFileUI";
            this.Text = "User Settings";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvErrorCodes)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbShowHomeOnStartup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbEmphasiseOnTabChanged;
        private System.Windows.Forms.CheckBox cbConfirmExit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbThemeMenus;
        private System.Windows.Forms.ComboBox ddTheme;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddWordWrap;
        private System.Windows.Forms.CheckBox cbFindShouldPin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbHeatmapColours;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbWait5Seconds;
        private System.Windows.Forms.CheckBox cbShowCohortWizard;
        private System.Windows.Forms.Button btnClearFavourites;
        private System.Windows.Forms.CheckBox cbDoubleClickToExpand;
        private System.Windows.Forms.CheckBox cbDebugPerformance;
        private System.Windows.Forms.CheckBox cbAllowIdentifiableExtractions;
        private System.Windows.Forms.CheckBox cbShowPipelineCompletedPopup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbHideEmptyTableLoadRunAudits;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbCreateDatabaseTimeout;
        private System.Windows.Forms.CheckBox cbScoreZeroForCohortAggregateContainers;
        private System.Windows.Forms.CheckBox cbAdvancedFindFilters;
        private BrightIdeasSoftware.ObjectListView olvErrorCodes;
        private BrightIdeasSoftware.OLVColumn olvCode;
        private BrightIdeasSoftware.OLVColumn olvTreatment;
        private BrightIdeasSoftware.OLVColumn olvMessage;
        private System.Windows.Forms.CheckBox cbIncludeZeroSeriesInGraphs;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolTip userSettingsToolTips;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbTooltipAppearDelay;
    }
}