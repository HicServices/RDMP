﻿namespace Rdmp.UI.SimpleDialogs
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
            this.SuspendLayout();
            // 
            // cbShowHomeOnStartup
            // 
            this.cbShowHomeOnStartup.AutoSize = true;
            this.cbShowHomeOnStartup.Location = new System.Drawing.Point(39, 51);
            this.cbShowHomeOnStartup.Name = "cbShowHomeOnStartup";
            this.cbShowHomeOnStartup.Size = new System.Drawing.Size(138, 17);
            this.cbShowHomeOnStartup.TabIndex = 0;
            this.cbShowHomeOnStartup.Text = "Show Home On Startup";
            this.cbShowHomeOnStartup.UseVisualStyleBackColor = true;
            this.cbShowHomeOnStartup.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(274, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Settings will automatically be Saved as you change them";
            // 
            // cbEmphasiseOnTabChanged
            // 
            this.cbEmphasiseOnTabChanged.AutoSize = true;
            this.cbEmphasiseOnTabChanged.Location = new System.Drawing.Point(39, 74);
            this.cbEmphasiseOnTabChanged.Name = "cbEmphasiseOnTabChanged";
            this.cbEmphasiseOnTabChanged.Size = new System.Drawing.Size(215, 17);
            this.cbEmphasiseOnTabChanged.TabIndex = 2;
            this.cbEmphasiseOnTabChanged.Text = "Show Object Collection On Tab Change";
            this.cbEmphasiseOnTabChanged.UseVisualStyleBackColor = true;
            this.cbEmphasiseOnTabChanged.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbConfirmExit
            // 
            this.cbConfirmExit.AutoSize = true;
            this.cbConfirmExit.Location = new System.Drawing.Point(39, 97);
            this.cbConfirmExit.Name = "cbConfirmExit";
            this.cbConfirmExit.Size = new System.Drawing.Size(136, 17);
            this.cbConfirmExit.TabIndex = 2;
            this.cbConfirmExit.Text = "Confirm Application Exit";
            this.cbConfirmExit.UseVisualStyleBackColor = true;
            this.cbConfirmExit.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 197);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Theme*:";
            // 
            // cbThemeMenus
            // 
            this.cbThemeMenus.AutoSize = true;
            this.cbThemeMenus.Location = new System.Drawing.Point(76, 221);
            this.cbThemeMenus.Name = "cbThemeMenus";
            this.cbThemeMenus.Size = new System.Drawing.Size(139, 17);
            this.cbThemeMenus.TabIndex = 4;
            this.cbThemeMenus.Text = "Apply Theme To Menus";
            this.cbThemeMenus.UseVisualStyleBackColor = true;
            this.cbThemeMenus.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // ddTheme
            // 
            this.ddTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddTheme.FormattingEnabled = true;
            this.ddTheme.Location = new System.Drawing.Point(76, 194);
            this.ddTheme.Name = "ddTheme";
            this.ddTheme.Size = new System.Drawing.Size(371, 21);
            this.ddTheme.TabIndex = 5;
            this.ddTheme.SelectedIndexChanged += new System.EventHandler(this.ddTheme_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 543);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "*Requires restart";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 268);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Sql Word Wrap:";
            // 
            // ddWordWrap
            // 
            this.ddWordWrap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddWordWrap.FormattingEnabled = true;
            this.ddWordWrap.Location = new System.Drawing.Point(91, 265);
            this.ddWordWrap.Name = "ddWordWrap";
            this.ddWordWrap.Size = new System.Drawing.Size(124, 21);
            this.ddWordWrap.TabIndex = 7;
            this.ddWordWrap.SelectedIndexChanged += new System.EventHandler(this.ddWordWrap_SelectedIndexChanged);
            // 
            // cbFindShouldPin
            // 
            this.cbFindShouldPin.AutoSize = true;
            this.cbFindShouldPin.Location = new System.Drawing.Point(39, 118);
            this.cbFindShouldPin.Name = "cbFindShouldPin";
            this.cbFindShouldPin.Size = new System.Drawing.Size(134, 17);
            this.cbFindShouldPin.TabIndex = 2;
            this.cbFindShouldPin.Text = "Find (Ctrl+F) should Pin";
            this.cbFindShouldPin.UseVisualStyleBackColor = true;
            this.cbFindShouldPin.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 312);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "HeatmapColours:";
            // 
            // tbHeatmapColours
            // 
            this.tbHeatmapColours.Location = new System.Drawing.Point(104, 309);
            this.tbHeatmapColours.Name = "tbHeatmapColours";
            this.tbHeatmapColours.Size = new System.Drawing.Size(238, 20);
            this.tbHeatmapColours.TabIndex = 9;
            this.tbHeatmapColours.TextChanged += new System.EventHandler(this.TbHeatmapColours_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(153, 332);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "(Format: #000000->#FFFFFF)";
            // 
            // cbWait5Seconds
            // 
            this.cbWait5Seconds.AutoSize = true;
            this.cbWait5Seconds.Location = new System.Drawing.Point(39, 141);
            this.cbWait5Seconds.Name = "cbWait5Seconds";
            this.cbWait5Seconds.Size = new System.Drawing.Size(161, 17);
            this.cbWait5Seconds.TabIndex = 2;
            this.cbWait5Seconds.Text = "Wait 5 seconds after Startup";
            this.cbWait5Seconds.UseVisualStyleBackColor = true;
            this.cbWait5Seconds.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbShowCohortWizard
            // 
            this.cbShowCohortWizard.AutoSize = true;
            this.cbShowCohortWizard.Location = new System.Drawing.Point(39, 164);
            this.cbShowCohortWizard.Name = "cbShowCohortWizard";
            this.cbShowCohortWizard.Size = new System.Drawing.Size(123, 17);
            this.cbShowCohortWizard.TabIndex = 2;
            this.cbShowCohortWizard.Text = "Show Cohort Wizard";
            this.cbShowCohortWizard.UseVisualStyleBackColor = true;
            this.cbShowCohortWizard.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // btnClearFavourites
            // 
            this.btnClearFavourites.Location = new System.Drawing.Point(104, 362);
            this.btnClearFavourites.Name = "btnClearFavourites";
            this.btnClearFavourites.Size = new System.Drawing.Size(96, 23);
            this.btnClearFavourites.TabIndex = 10;
            this.btnClearFavourites.Text = "Clear Favourites";
            this.btnClearFavourites.UseVisualStyleBackColor = true;
            // 
            // cbDoubleClickToExpand
            // 
            this.cbDoubleClickToExpand.AutoSize = true;
            this.cbDoubleClickToExpand.Location = new System.Drawing.Point(298, 51);
            this.cbDoubleClickToExpand.Name = "cbDoubleClickToExpand";
            this.cbDoubleClickToExpand.Size = new System.Drawing.Size(141, 17);
            this.cbDoubleClickToExpand.TabIndex = 0;
            this.cbDoubleClickToExpand.Text = "Double Click To Expand";
            this.cbDoubleClickToExpand.UseVisualStyleBackColor = true;
            this.cbDoubleClickToExpand.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbDebugPerformance
            // 
            this.cbDebugPerformance.AutoSize = true;
            this.cbDebugPerformance.Location = new System.Drawing.Point(298, 74);
            this.cbDebugPerformance.Name = "cbDebugPerformance";
            this.cbDebugPerformance.Size = new System.Drawing.Size(286, 17);
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
            this.hlpDebugPerformance.Location = new System.Drawing.Point(584, 73);
            this.hlpDebugPerformance.MaximumSize = new System.Drawing.Size(19, 19);
            this.hlpDebugPerformance.MinimumSize = new System.Drawing.Size(19, 19);
            this.hlpDebugPerformance.Name = "hlpDebugPerformance";
            this.hlpDebugPerformance.Size = new System.Drawing.Size(19, 19);
            this.hlpDebugPerformance.SuppressClick = false;
            this.hlpDebugPerformance.TabIndex = 12;
            // 
            // cbAllowIdentifiableExtractions
            // 
            this.cbAllowIdentifiableExtractions.AutoSize = true;
            this.cbAllowIdentifiableExtractions.Location = new System.Drawing.Point(298, 97);
            this.cbAllowIdentifiableExtractions.Name = "cbAllowIdentifiableExtractions";
            this.cbAllowIdentifiableExtractions.Size = new System.Drawing.Size(160, 17);
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
            this.hlpIdentifiableExtractions.Location = new System.Drawing.Point(464, 97);
            this.hlpIdentifiableExtractions.MaximumSize = new System.Drawing.Size(19, 19);
            this.hlpIdentifiableExtractions.MinimumSize = new System.Drawing.Size(19, 19);
            this.hlpIdentifiableExtractions.Name = "hlpIdentifiableExtractions";
            this.hlpIdentifiableExtractions.Size = new System.Drawing.Size(19, 19);
            this.hlpIdentifiableExtractions.SuppressClick = false;
            this.hlpIdentifiableExtractions.TabIndex = 14;
            // 
            // cbShowPipelineCompletedPopup
            // 
            this.cbShowPipelineCompletedPopup.AutoSize = true;
            this.cbShowPipelineCompletedPopup.Location = new System.Drawing.Point(298, 120);
            this.cbShowPipelineCompletedPopup.Name = "cbShowPipelineCompletedPopup";
            this.cbShowPipelineCompletedPopup.Size = new System.Drawing.Size(180, 17);
            this.cbShowPipelineCompletedPopup.TabIndex = 15;
            this.cbShowPipelineCompletedPopup.Text = "Show Pipeline Completed Popup";
            this.cbShowPipelineCompletedPopup.UseVisualStyleBackColor = true;
            this.cbShowPipelineCompletedPopup.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // UserSettingsFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 565);
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
            this.Name = "UserSettingsFileUI";
            this.Text = "User Settings";
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
    }
}