// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Settings;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// Allows you to change settings in the application that are optional e.g. whether to load the Home screen on startup or to load the state of the application when you last closed it.
    /// 
    /// <para>Settings are stored in AppData in a folder called RDMP in a file called UserSettings.txt</para>
    /// </summary>
    public partial class UserSettingsFileUI : Form
    {
        private bool _bLoaded;

        const string WarnOnTimeoutOnExtractionChecks = "Extraction checks timeout";

        public UserSettingsFileUI(IActivateItems activator)
        {
            InitializeComponent();

            hlpDebugPerformance.SetHelpText("Debug Performance","When enabled RDMP will record certain performance related metrics (how long refresh takes etc).  These figures are completely internal to the application and are not transmitted anywhere.  You can view the results in the toolbar.");
            hlpIdentifiableExtractions.SetHelpText("Allow Identifiable Extractions","Controls whether RDMP permits cohorts to be created where the release ID and private ID are the same (i.e. the linkage ids are not anonymised).  Changing this setting will not affect how cohorts are currently configured or extracted.  It only supresses a specific error message that is generated when a cohort source is configured where the private and release identifiers reference the same column ");

            cbShowHomeOnStartup.Checked = UserSettings.ShowHomeOnStartup;
            cbEmphasiseOnTabChanged.Checked = UserSettings.EmphasiseOnTabChanged;
            cbConfirmExit.Checked = UserSettings.ConfirmApplicationExiting;
            cbFindShouldPin.Checked = UserSettings.FindShouldPin;
            cbThemeMenus.Checked = UserSettings.ApplyThemeToMenus;
            cbWait5Seconds.Checked = UserSettings.Wait5SecondsAfterStartupUI;
            cbShowCohortWizard.Checked = UserSettings.ShowCohortWizard;
            cbDoubleClickToExpand.Checked = UserSettings.DoubleClickToExpand;
            cbDebugPerformance.Checked = UserSettings.DebugPerformance;
            cbAllowIdentifiableExtractions.Checked = UserSettings.AllowIdentifiableExtractions;
            cbShowPipelineCompletedPopup.Checked = UserSettings.ShowPipelineCompletedPopup;
            cbHideEmptyTableLoadRunAudits.Checked = UserSettings.HideEmptyTableLoadRunAudits;

            clbWarnings.Items.Add(WarnOnTimeoutOnExtractionChecks, UserSettings.WarnOnTimeoutOnExtractionChecks);

            clbWarnings.ItemCheck += ClbWarnings_ItemCheck;

            ddTheme.DataSource = new []
            {
                "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015DarkTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015LightTheme"
            };

            ddTheme.SelectedItem = UserSettings.Theme;

            ddWordWrap.DataSource = Enum.GetValues(typeof(WrapMode));
            ddWordWrap.SelectedItem = (WrapMode)UserSettings.WrapMode;

            tbHeatmapColours.Text = UserSettings.HeatMapColours;

            _bLoaded = true;

            var cmd = new ExecuteCommandClearFavourites(activator);
            btnClearFavourites.Enabled = !cmd.IsImpossible;

            btnClearFavourites.Click += (s, e) =>
            {
                cmd.Execute();
                btnClearFavourites.Enabled = !cmd.IsImpossible;
            };
        }

        private void ClbWarnings_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var item = clbWarnings.Items[e.Index];

            if(Equals(item , WarnOnTimeoutOnExtractionChecks))
            {
                UserSettings.WarnOnTimeoutOnExtractionChecks = e.NewValue == CheckState.Checked;
            }
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (!_bLoaded)
                return;
            
            var cb = (CheckBox)sender;

            if (cb == cbShowHomeOnStartup)
                UserSettings.ShowHomeOnStartup = cb.Checked;

            if (cb == cbEmphasiseOnTabChanged)
                UserSettings.EmphasiseOnTabChanged = cb.Checked;

            if(cb == cbConfirmExit)
                UserSettings.ConfirmApplicationExiting = cb.Checked;
            
            if (cb == cbThemeMenus)
                UserSettings.ApplyThemeToMenus = cb.Checked;

            if(cb == cbFindShouldPin)
                UserSettings.FindShouldPin = cb.Checked;

            if(cb == cbWait5Seconds)
                UserSettings.Wait5SecondsAfterStartupUI = cb.Checked;

            if(cb == cbShowCohortWizard)
                UserSettings.ShowCohortWizard = cb.Checked;

            if (cb == cbDoubleClickToExpand)
                UserSettings.DoubleClickToExpand = cb.Checked;

            if(cb == cbDebugPerformance)
                UserSettings.DebugPerformance = cb.Checked;

            if(cb == cbAllowIdentifiableExtractions)
                UserSettings.AllowIdentifiableExtractions = cb.Checked;
            
            if(cb == cbShowPipelineCompletedPopup)
                UserSettings.ShowPipelineCompletedPopup = cb.Checked;

            if (cb == cbHideEmptyTableLoadRunAudits)
                UserSettings.HideEmptyTableLoadRunAudits = cb.Checked;
        }

        private void ddTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!_bLoaded)
                return;
            
            var t = ddTheme.SelectedItem as string;
            
            if(t != null)
                UserSettings.Theme = t;
        }

        private void ddWordWrap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_bLoaded)
                return;

            var wrap = (WrapMode)ddWordWrap.SelectedItem;
            UserSettings.WrapMode = (int)wrap;
        }

        private void TbHeatmapColours_TextChanged(object sender, EventArgs e)
        {
            UserSettings.HeatMapColours = tbHeatmapColours.Text;
        }
    }
}
