// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using ScintillaNET;
using static BrightIdeasSoftware.ObjectListView;
using static ReusableLibraryCode.Checks.CheckEventArgs;

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
        private IActivateItems _activator;

        const string WarnOnTimeoutOnExtractionChecks = "Extraction checks timeout";

        /// <summary>
        /// The maximum number of characters to allow per line in a tooltip before
        /// wrapping to next line
        /// </summary>
        const int MaxTooltipWidth = 100;

        public UserSettingsFileUI(IActivateItems activator)
        {
            _activator = activator;

            InitializeComponent();
            //Stop mouse wheel scroll from scrolling the combobox when it's closed to avoid the value being changed without user noticing.
            RDMPControlCommonFunctionality.DisableMouseWheel(ddWordWrap);
            RDMPControlCommonFunctionality.DisableMouseWheel(ddTheme);


            olvErrorCodes.CellEditActivation = CellEditActivateMode.SingleClick;
            olvErrorCodes.ShowGroups = false;

            olvCode.AspectName = nameof(ErrorCode.Code);
            olvCode.Text = "Code";
            olvCode.IsEditable = false;
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvErrorCodes, olvCode, new Guid("bba20a20-ffa2-4db6-b4fe-a5dcc5a03727"));

            olvMessage.AspectName = nameof(ErrorCode.Message);
            olvMessage.Text = "Error Message";
            olvMessage.IsEditable = false;
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvErrorCodes, olvMessage, new Guid("21a785e9-52f4-494b-89d0-6ccc68689ce9"));

            olvTreatment.Text = "Treatment";
            olvTreatment.Width = 20;
            olvTreatment.AspectGetter += Treatment_Getter;
            olvTreatment.AspectPutter += Treatment_Putter;
            olvTreatment.CellEditUseWholeCell = true;
            olvTreatment.IsEditable = true;
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvErrorCodes, olvTreatment, new Guid("75d54469-f870-4870-86cf-2dd782a27f57"));

            olvErrorCodes.RebuildColumns();

            cbShowHomeOnStartup.Checked = UserSettings.ShowHomeOnStartup;
            cbEmphasiseOnTabChanged.Checked = UserSettings.EmphasiseOnTabChanged;
            cbConfirmExit.Checked = UserSettings.ConfirmApplicationExiting;
            cbFindShouldPin.Checked = UserSettings.FindShouldPin;
            cbThemeMenus.Checked = UserSettings.ApplyThemeToMenus;
            cbWait5Seconds.Checked = UserSettings.Wait5SecondsAfterStartupUI;
            cbShowCohortWizard.Checked = UserSettings.ShowCohortWizard;
            cbDoubleClickToExpand.Checked = UserSettings.DoubleClickToExpand;
            cbDebugPerformance.Checked = UserSettings.DebugPerformance;
            cbShowPipelineCompletedPopup.Checked = UserSettings.ShowPipelineCompletedPopup;
            cbHideEmptyTableLoadRunAudits.Checked = UserSettings.HideEmptyTableLoadRunAudits;
            cbScoreZeroForCohortAggregateContainers.Checked = UserSettings.ScoreZeroForCohortAggregateContainers;
            cbAdvancedFindFilters.Checked = UserSettings.AdvancedFindFilters;
            cbIncludeZeroSeriesInGraphs.Checked = UserSettings.IncludeZeroSeriesInGraphs;
            cbAlwaysJoinEverything.Checked = UserSettings.AlwaysJoinEverything;
            tbCreateDatabaseTimeout.Text = UserSettings.CreateDatabaseTimeout.ToString();
            tbArchiveTriggerTimeout.Text = UserSettings.ArchiveTriggerTimeout.ToString();
            tbTooltipAppearDelay.Text = UserSettings.TooltipAppearDelay.ToString();

            AddTooltip(cbShowHomeOnStartup,nameof(UserSettings.ShowHomeOnStartup));
            AddTooltip(cbEmphasiseOnTabChanged,nameof(UserSettings.EmphasiseOnTabChanged));
            AddTooltip(cbConfirmExit,nameof(UserSettings.ConfirmApplicationExiting));
            AddTooltip(cbFindShouldPin,nameof(UserSettings.FindShouldPin));
            AddTooltip(cbThemeMenus,nameof(UserSettings.ApplyThemeToMenus));
            AddTooltip(cbWait5Seconds,nameof(UserSettings.Wait5SecondsAfterStartupUI));
            AddTooltip(cbShowCohortWizard,nameof(UserSettings.ShowCohortWizard));
            AddTooltip(cbDoubleClickToExpand,nameof(UserSettings.DoubleClickToExpand));
            AddTooltip(cbDebugPerformance,nameof(UserSettings.DebugPerformance));
            AddTooltip(cbShowPipelineCompletedPopup,nameof(UserSettings.ShowPipelineCompletedPopup));
            AddTooltip(cbHideEmptyTableLoadRunAudits,nameof(UserSettings.HideEmptyTableLoadRunAudits));
            AddTooltip(cbScoreZeroForCohortAggregateContainers,nameof(UserSettings.ScoreZeroForCohortAggregateContainers));
            AddTooltip(cbAdvancedFindFilters,nameof(UserSettings.AdvancedFindFilters));
            AddTooltip(cbIncludeZeroSeriesInGraphs,nameof(UserSettings.IncludeZeroSeriesInGraphs));
            AddTooltip(cbAlwaysJoinEverything,nameof(UserSettings.AlwaysJoinEverything));
            AddTooltip(label7, nameof(UserSettings.CreateDatabaseTimeout));
            AddTooltip(tbCreateDatabaseTimeout, nameof(UserSettings.CreateDatabaseTimeout));
            AddTooltip(label13, nameof(UserSettings.ArchiveTriggerTimeout));
            AddTooltip(tbArchiveTriggerTimeout, nameof(UserSettings.ArchiveTriggerTimeout));
            AddTooltip(tbTooltipAppearDelay, nameof(UserSettings.TooltipAppearDelay));
            AddTooltip(label4, nameof(UserSettings.WrapMode));
            AddTooltip(ddWordWrap,nameof(UserSettings.WrapMode));
            AddTooltip(ddTheme, nameof(UserSettings.Theme));
            AddTooltip(label2, nameof(UserSettings.Theme));
            AddTooltip(label5, nameof(UserSettings.HeatMapColours));
            AddTooltip(tbHeatmapColours, nameof(UserSettings.HeatMapColours));


            olvErrorCodes.AddObjects(ErrorCodes.KnownCodes);

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

        private void AddTooltip(Control c, string propertyName)
        {
            string helpText = _activator.CommentStore.GetDocumentationIfExists($"{ nameof(UserSettings)}.{propertyName}", false);
            if(string.IsNullOrWhiteSpace(helpText))
            {
                return;
            }

            userSettingsToolTips.SetToolTip(c, UsefulStuff.SplitByLength(helpText, MaxTooltipWidth));
        }

        private void Treatment_Putter(object rowObject, object newValue)
        {
            UserSettings.SetErrorReportingLevelFor((ErrorCode)rowObject, (CheckResult)newValue);
        }

        private object Treatment_Getter(object rowObject)
        {
            return UserSettings.GetErrorReportingLevelFor((ErrorCode)rowObject);
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
                        
            if(cb == cbShowPipelineCompletedPopup)
                UserSettings.ShowPipelineCompletedPopup = cb.Checked;

            if (cb == cbHideEmptyTableLoadRunAudits)
                UserSettings.HideEmptyTableLoadRunAudits = cb.Checked;

            if (cb == cbScoreZeroForCohortAggregateContainers)
                UserSettings.ScoreZeroForCohortAggregateContainers = cb.Checked;

            if (cb == cbAdvancedFindFilters)
                UserSettings.AdvancedFindFilters = cb.Checked;

            if (cb == cbIncludeZeroSeriesInGraphs)
                UserSettings.IncludeZeroSeriesInGraphs = cb.Checked;

            if (cb == cbAlwaysJoinEverything)
                UserSettings.AlwaysJoinEverything = cb.Checked;
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

        private void tbCreateDatabaseTimeout_TextChanged(object sender, EventArgs e)
        {
            if(int.TryParse(tbCreateDatabaseTimeout.Text,out int result))
            {
                UserSettings.CreateDatabaseTimeout = result;
            }
        }
        private void tbArchiveTriggerTimeout_TextChanged(object sender, EventArgs e)
        {

            if (int.TryParse(tbArchiveTriggerTimeout.Text, out int result))
            {
                UserSettings.ArchiveTriggerTimeout = result;
            }
        }
        private void tbTooltipAppearDelay_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(tbTooltipAppearDelay.Text, out int result))
            {
                UserSettings.TooltipAppearDelay = result;
            }
        }

    }
}
