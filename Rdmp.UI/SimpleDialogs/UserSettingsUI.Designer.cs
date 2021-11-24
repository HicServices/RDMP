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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserSettingsFileUI));
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
            this.hlpDebugPerformance = new Rdmp.UI.SimpleControls.HelpIcon();
            this.cbAllowIdentifiableExtractions = new System.Windows.Forms.CheckBox();
            this.hlpIdentifiableExtractions = new Rdmp.UI.SimpleControls.HelpIcon();
            this.cbShowPipelineCompletedPopup = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbHideEmptyTableLoadRunAudits = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbCreateDatabaseTimeout = new System.Windows.Forms.TextBox();
            this.cbScoreZeroForCohortAggregateContainers = new System.Windows.Forms.CheckBox();
            this.cbAdvancedFindFilters = new System.Windows.Forms.CheckBox();
            this.olvErrorCodes = new BrightIdeasSoftware.ObjectListView();
            this.olvCode = new BrightIdeasSoftware.OLVColumn();
            this.olvTreatment = new BrightIdeasSoftware.OLVColumn();
            this.olvMessage = new BrightIdeasSoftware.OLVColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvErrorCodes)).BeginInit();
            this.SuspendLayout();
            // 
            // cbShowHomeOnStartup
            // 
            this.cbShowHomeOnStartup.AutoSize = true;
            this.cbShowHomeOnStartup.Location = new System.Drawing.Point(46, 59);
            this.cbShowHomeOnStartup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbShowHomeOnStartup.Name = "cbShowHomeOnStartup";
            this.cbShowHomeOnStartup.Size = new System.Drawing.Size(151, 19);
            this.cbShowHomeOnStartup.TabIndex = 0;
            this.cbShowHomeOnStartup.Text = "Show Home On Startup";
            this.cbShowHomeOnStartup.UseVisualStyleBackColor = true;
            this.cbShowHomeOnStartup.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(305, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Settings will automatically be Saved as you change them";
            // 
            // cbEmphasiseOnTabChanged
            // 
            this.cbEmphasiseOnTabChanged.AutoSize = true;
            this.cbEmphasiseOnTabChanged.Location = new System.Drawing.Point(46, 85);
            this.cbEmphasiseOnTabChanged.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbEmphasiseOnTabChanged.Name = "cbEmphasiseOnTabChanged";
            this.cbEmphasiseOnTabChanged.Size = new System.Drawing.Size(234, 19);
            this.cbEmphasiseOnTabChanged.TabIndex = 2;
            this.cbEmphasiseOnTabChanged.Text = "Show Object Collection On Tab Change";
            this.cbEmphasiseOnTabChanged.UseVisualStyleBackColor = true;
            this.cbEmphasiseOnTabChanged.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbConfirmExit
            // 
            this.cbConfirmExit.AutoSize = true;
            this.cbConfirmExit.Location = new System.Drawing.Point(46, 112);
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
            this.label2.Location = new System.Drawing.Point(28, 227);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Theme*:";
            // 
            // cbThemeMenus
            // 
            this.cbThemeMenus.AutoSize = true;
            this.cbThemeMenus.Location = new System.Drawing.Point(89, 255);
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
            this.ddTheme.Location = new System.Drawing.Point(89, 224);
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
            this.label3.Location = new System.Drawing.Point(15, 627);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "*Requires restart";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 309);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Sql Word Wrap:";
            // 
            // ddWordWrap
            // 
            this.ddWordWrap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddWordWrap.FormattingEnabled = true;
            this.ddWordWrap.Location = new System.Drawing.Point(106, 306);
            this.ddWordWrap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddWordWrap.Name = "ddWordWrap";
            this.ddWordWrap.Size = new System.Drawing.Size(144, 23);
            this.ddWordWrap.TabIndex = 7;
            this.ddWordWrap.SelectedIndexChanged += new System.EventHandler(this.ddWordWrap_SelectedIndexChanged);
            // 
            // cbFindShouldPin
            // 
            this.cbFindShouldPin.AutoSize = true;
            this.cbFindShouldPin.Location = new System.Drawing.Point(46, 136);
            this.cbFindShouldPin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbFindShouldPin.Name = "cbFindShouldPin";
            this.cbFindShouldPin.Size = new System.Drawing.Size(152, 19);
            this.cbFindShouldPin.TabIndex = 2;
            this.cbFindShouldPin.Text = "Find (Ctrl+F) should Pin";
            this.cbFindShouldPin.UseVisualStyleBackColor = true;
            this.cbFindShouldPin.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 360);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "HeatmapColours:";
            // 
            // tbHeatmapColours
            // 
            this.tbHeatmapColours.Location = new System.Drawing.Point(121, 357);
            this.tbHeatmapColours.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbHeatmapColours.Name = "tbHeatmapColours";
            this.tbHeatmapColours.Size = new System.Drawing.Size(277, 23);
            this.tbHeatmapColours.TabIndex = 9;
            this.tbHeatmapColours.TextChanged += new System.EventHandler(this.TbHeatmapColours_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(178, 383);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(158, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "(Format: #000000->#FFFFFF)";
            // 
            // cbWait5Seconds
            // 
            this.cbWait5Seconds.AutoSize = true;
            this.cbWait5Seconds.Location = new System.Drawing.Point(46, 163);
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
            this.cbShowCohortWizard.Location = new System.Drawing.Point(46, 189);
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
            this.btnClearFavourites.Location = new System.Drawing.Point(121, 456);
            this.btnClearFavourites.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearFavourites.Name = "btnClearFavourites";
            this.btnClearFavourites.Size = new System.Drawing.Size(112, 27);
            this.btnClearFavourites.TabIndex = 10;
            this.btnClearFavourites.Text = "Clear Favourites";
            this.btnClearFavourites.UseVisualStyleBackColor = true;
            // 
            // cbDoubleClickToExpand
            // 
            this.cbDoubleClickToExpand.AutoSize = true;
            this.cbDoubleClickToExpand.Location = new System.Drawing.Point(348, 59);
            this.cbDoubleClickToExpand.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbDoubleClickToExpand.Name = "cbDoubleClickToExpand";
            this.cbDoubleClickToExpand.Size = new System.Drawing.Size(150, 19);
            this.cbDoubleClickToExpand.TabIndex = 0;
            this.cbDoubleClickToExpand.Text = "Double Click To Expand";
            this.cbDoubleClickToExpand.UseVisualStyleBackColor = true;
            this.cbDoubleClickToExpand.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbDebugPerformance
            // 
            this.cbDebugPerformance.AutoSize = true;
            this.cbDebugPerformance.Location = new System.Drawing.Point(348, 85);
            this.cbDebugPerformance.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbDebugPerformance.Name = "cbDebugPerformance";
            this.cbDebugPerformance.Size = new System.Drawing.Size(319, 19);
            this.cbDebugPerformance.TabIndex = 11;
            this.cbDebugPerformance.Text = "Record Performance Metrics (local data collection only)";
            this.cbDebugPerformance.UseVisualStyleBackColor = true;
            this.cbDebugPerformance.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // hlpDebugPerformance
            // 
            this.hlpDebugPerformance.BackColor = System.Drawing.Color.Transparent;
            this.hlpDebugPerformance.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpDebugPerformance.BackgroundImage")));
            this.hlpDebugPerformance.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpDebugPerformance.Location = new System.Drawing.Point(681, 84);
            this.hlpDebugPerformance.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.hlpDebugPerformance.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpDebugPerformance.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpDebugPerformance.Name = "hlpDebugPerformance";
            this.hlpDebugPerformance.Size = new System.Drawing.Size(22, 22);
            this.hlpDebugPerformance.SuppressClick = false;
            this.hlpDebugPerformance.TabIndex = 12;
            // 
            // cbAllowIdentifiableExtractions
            // 
            this.cbAllowIdentifiableExtractions.AutoSize = true;
            this.cbAllowIdentifiableExtractions.Location = new System.Drawing.Point(348, 112);
            this.cbAllowIdentifiableExtractions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbAllowIdentifiableExtractions.Name = "cbAllowIdentifiableExtractions";
            this.cbAllowIdentifiableExtractions.Size = new System.Drawing.Size(179, 19);
            this.cbAllowIdentifiableExtractions.TabIndex = 13;
            this.cbAllowIdentifiableExtractions.Text = "Allow Identifiable Extractions";
            this.cbAllowIdentifiableExtractions.UseVisualStyleBackColor = true;
            this.cbAllowIdentifiableExtractions.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // hlpIdentifiableExtractions
            // 
            this.hlpIdentifiableExtractions.BackColor = System.Drawing.Color.Transparent;
            this.hlpIdentifiableExtractions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpIdentifiableExtractions.BackgroundImage")));
            this.hlpIdentifiableExtractions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpIdentifiableExtractions.Location = new System.Drawing.Point(541, 112);
            this.hlpIdentifiableExtractions.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.hlpIdentifiableExtractions.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpIdentifiableExtractions.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpIdentifiableExtractions.Name = "hlpIdentifiableExtractions";
            this.hlpIdentifiableExtractions.Size = new System.Drawing.Size(22, 22);
            this.hlpIdentifiableExtractions.SuppressClick = false;
            this.hlpIdentifiableExtractions.TabIndex = 14;
            // 
            // cbShowPipelineCompletedPopup
            // 
            this.cbShowPipelineCompletedPopup.AutoSize = true;
            this.cbShowPipelineCompletedPopup.Location = new System.Drawing.Point(348, 138);
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
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.olvErrorCodes);
            this.groupBox1.Location = new System.Drawing.Point(447, 255);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 385);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Warnings Settings";
            // 
            // cbHideEmptyTableLoadRunAudits
            // 
            this.cbHideEmptyTableLoadRunAudits.AutoSize = true;
            this.cbHideEmptyTableLoadRunAudits.Location = new System.Drawing.Point(348, 163);
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
            this.label7.Location = new System.Drawing.Point(12, 405);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(142, 15);
            this.label7.TabIndex = 17;
            this.label7.Text = "Create Database Timeout:";
            // 
            // tbCreateDatabaseTimeout
            // 
            this.tbCreateDatabaseTimeout.Location = new System.Drawing.Point(158, 402);
            this.tbCreateDatabaseTimeout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbCreateDatabaseTimeout.Name = "tbCreateDatabaseTimeout";
            this.tbCreateDatabaseTimeout.Size = new System.Drawing.Size(240, 23);
            this.tbCreateDatabaseTimeout.TabIndex = 18;
            this.tbCreateDatabaseTimeout.TextChanged += new System.EventHandler(this.tbCreateDatabaseTimeout_TextChanged);
            // 
            // cbScoreZeroForCohortAggregateContainers
            // 
            this.cbScoreZeroForCohortAggregateContainers.AutoSize = true;
            this.cbScoreZeroForCohortAggregateContainers.Location = new System.Drawing.Point(348, 188);
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
            this.cbAdvancedFindFilters.Location = new System.Drawing.Point(715, 59);
            this.cbAdvancedFindFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbAdvancedFindFilters.Name = "cbAdvancedFindFilters";
            this.cbAdvancedFindFilters.Size = new System.Drawing.Size(139, 19);
            this.cbAdvancedFindFilters.TabIndex = 19;
            this.cbAdvancedFindFilters.Text = "Advanced Find Filters";
            this.cbAdvancedFindFilters.UseVisualStyleBackColor = true;
            this.cbAdvancedFindFilters.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // olvErrorCodes
            // 
            this.olvErrorCodes.CellEditUseWholeCell = false;
            this.olvErrorCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvErrorCodes.HideSelection = false;
            this.olvErrorCodes.Location = new System.Drawing.Point(3, 19);
            this.olvErrorCodes.Name = "olvErrorCodes";
            this.olvErrorCodes.Size = new System.Drawing.Size(401, 363);
            this.olvErrorCodes.TabIndex = 0;
            this.olvErrorCodes.View = System.Windows.Forms.View.Details;
            this.olvErrorCodes.AllColumns.Add(olvCode);
            this.olvErrorCodes.AllColumns.Add(olvTreatment);
            this.olvErrorCodes.AllColumns.Add(olvMessage);
            // 
            // UserSettingsFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 652);
            this.Controls.Add(this.cbAdvancedFindFilters);
            this.Controls.Add(this.tbCreateDatabaseTimeout);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbScoreZeroForCohortAggregateContainers);
            this.Controls.Add(this.cbHideEmptyTableLoadRunAudits);
            this.Controls.Add(this.cbShowPipelineCompletedPopup);
            this.Controls.Add(this.hlpIdentifiableExtractions);
            this.Controls.Add(this.cbAllowIdentifiableExtractions);
            this.Controls.Add(this.hlpDebugPerformance);
            this.Controls.Add(this.cbDebugPerformance);
            this.Controls.Add(this.btnClearFavourites);
            this.Controls.Add(this.tbHeatmapColours);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ddWordWrap);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ddTheme);
            this.Controls.Add(this.cbThemeMenus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbWait5Seconds);
            this.Controls.Add(this.cbShowCohortWizard);
            this.Controls.Add(this.cbFindShouldPin);
            this.Controls.Add(this.cbConfirmExit);
            this.Controls.Add(this.cbEmphasiseOnTabChanged);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbDoubleClickToExpand);
            this.Controls.Add(this.cbShowHomeOnStartup);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "UserSettingsFileUI";
            this.Text = "User Settings";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvErrorCodes)).EndInit();
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
        private SimpleControls.HelpIcon hlpDebugPerformance;
        private System.Windows.Forms.CheckBox cbAllowIdentifiableExtractions;
        private SimpleControls.HelpIcon hlpIdentifiableExtractions;
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
    }
}