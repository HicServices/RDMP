// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;
using static BrightIdeasSoftware.ObjectListView;
namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Allows you to change settings in the application that are optional e.g. whether to load the Home screen on startup or to load the state of the application when you last closed it.
/// 
/// <para>Settings are stored in AppData in a folder called RDMP in a file called UserSettings.txt</para>
/// </summary>
public partial class UserSettingsFileUI : Form
{
    private bool _bLoaded;
    private IActivateItems _activator;

    private const string WarnOnTimeoutOnExtractionChecks = "Extraction checks timeout";

    /// <summary>
    /// The maximum number of characters to allow per line in a tooltip before
    /// wrapping to next line
    /// </summary>
    private const int MaxTooltipWidth = 100;

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

        olvTreatment.Text = "Treatment";
        olvTreatment.AspectGetter += Treatment_Getter;
        olvTreatment.AspectPutter += Treatment_Putter;
        olvTreatment.CellEditUseWholeCell = true;
        olvTreatment.IsEditable = true;

        olvMessage.AspectName = nameof(ErrorCode.Message);
        olvMessage.Text = "Error Message";
        olvMessage.IsEditable = false;

        olvErrorCodes.RebuildColumns();

        //Resize known columns
        olvCode.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        olvCode.MaximumWidth = olvCode.Width;
        olvCode.MinimumWidth = olvCode.Width;
        olvTreatment.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        olvTreatment.MaximumWidth = olvTreatment.Width;
        olvTreatment.MinimumWidth = olvTreatment.Width;

        tbCreateDatabaseTimeout.Text = UserSettings.CreateDatabaseTimeout.ToString();
        tbArchiveTriggerTimeout.Text = UserSettings.ArchiveTriggerTimeout.ToString();
        tbTooltipAppearDelay.Text = UserSettings.TooltipAppearDelay.ToString();
        tbLocalFileSystemLocation.Text = UserSettings.LocalFileSystemLocation?.ToString();

        RegisterCheckbox(cbShowHomeOnStartup, nameof(UserSettings.ShowHomeOnStartup));
        RegisterCheckbox(cbEmphasiseOnTabChanged, nameof(UserSettings.EmphasiseOnTabChanged));
        RegisterCheckbox(cbConfirmExit, nameof(UserSettings.ConfirmApplicationExiting));
        RegisterCheckbox(cbFindShouldPin, nameof(UserSettings.FindShouldPin));
        RegisterCheckbox(cbThemeMenus, nameof(UserSettings.ApplyThemeToMenus));
        RegisterCheckbox(cbWait5Seconds, nameof(UserSettings.Wait5SecondsAfterStartupUI));
        RegisterCheckbox(cbShowCohortWizard, nameof(UserSettings.ShowCohortWizard));
        RegisterCheckbox(cbStrictValidationForCohortBuilderContainers,
            nameof(UserSettings.StrictValidationForCohortBuilderContainers));
        RegisterCheckbox(cbDoubleClickToExpand, nameof(UserSettings.DoubleClickToExpand));
        RegisterCheckbox(cbDebugPerformance, nameof(UserSettings.DebugPerformance));
        RegisterCheckbox(cbAutoResizeColumns, nameof(UserSettings.AutoResizeColumns));
        RegisterCheckbox(cbShowPipelineCompletedPopup, nameof(UserSettings.ShowPipelineCompletedPopup));
        RegisterCheckbox(cbSkipCohortBuilderValidationOnCommit,
            nameof(UserSettings.SkipCohortBuilderValidationOnCommit));
        RegisterCheckbox(cbHideEmptyTableLoadRunAudits, nameof(UserSettings.HideEmptyTableLoadRunAudits));
        RegisterCheckbox(cbScoreZeroForCohortAggregateContainers,
            nameof(UserSettings.ScoreZeroForCohortAggregateContainers));
        RegisterCheckbox(cbNewFind, nameof(UserSettings.NewFindAndReplace));
        RegisterCheckbox(cbAdvancedFindFilters, nameof(UserSettings.AdvancedFindFilters));
        RegisterCheckbox(cbIncludeZeroSeriesInGraphs, nameof(UserSettings.IncludeZeroSeriesInGraphs));
        RegisterCheckbox(cbSelectiveRefresh, nameof(UserSettings.SelectiveRefresh));
        RegisterCheckbox(cbAlwaysJoinEverything, nameof(UserSettings.AlwaysJoinEverything));
        RegisterCheckbox(cbAutoRunSqlQueries, nameof(UserSettings.AutoRunSqlQueries));
        RegisterCheckbox(cbExpandAllInCohortBuilder, nameof(UserSettings.ExpandAllInCohortBuilder));
        RegisterCheckbox(cbPromptFilterRename, nameof(UserSettings.PromptRenameOnCohortFilterChange));
        RegisterCheckbox(cbUseAliasInsteadOfTransformInGroupByAggregateGraphs,
            nameof(UserSettings.UseAliasInsteadOfTransformInGroupByAggregateGraphs));
        RegisterCheckbox(cbUseLocalFileSystem, nameof(UserSettings.UseLocalFileSystem));
        RegisterCheckbox(cbFlatLogs, nameof(UserSettings.DefaultLogViewFlat));
        AddTooltip(label7, nameof(UserSettings.CreateDatabaseTimeout));
        AddTooltip(tbCreateDatabaseTimeout, nameof(UserSettings.CreateDatabaseTimeout));
        AddTooltip(label13, nameof(UserSettings.ArchiveTriggerTimeout));
        AddTooltip(tbArchiveTriggerTimeout, nameof(UserSettings.ArchiveTriggerTimeout));
        AddTooltip(tbTooltipAppearDelay, nameof(UserSettings.TooltipAppearDelay));
        AddTooltip(label4, nameof(UserSettings.WrapMode));
        AddTooltip(ddWordWrap, nameof(UserSettings.WrapMode));
        AddTooltip(ddTheme, nameof(UserSettings.Theme));
        AddTooltip(label2, nameof(UserSettings.Theme));
        AddTooltip(label5, nameof(UserSettings.HeatMapColours));
        AddTooltip(tbHeatmapColours, nameof(UserSettings.HeatMapColours));

        //Add error codes
        olvErrorCodes.AddObjects(ErrorCodes.KnownCodes);

        //Once added we know what width we'd like to make the columns
        olvMessage.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        olvMessage.MaximumWidth = olvMessage.Width;
        olvMessage.MinimumWidth = olvMessage.Width;

        ddTheme.DataSource = new[]
        {
            "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme",
            "ResearchDataManagementPlatform.Theme.MyVS2015DarkTheme",
            "ResearchDataManagementPlatform.Theme.MyVS2015LightTheme"
        };

        ddTheme.SelectedItem = UserSettings.Theme;

        ddWordWrap.DataSource = Enum.GetValues(typeof(WrapMode));
        ddWordWrap.SelectedItem = (WrapMode)UserSettings.WrapMode;

        tbHeatmapColours.Text = UserSettings.HeatMapColours;
        tbWebhookUrl.Text = UserSettings.ExtractionWebhookUrl;
        tbWebhookUsername.Text = UserSettings.ExtractionWebhookUsername;

        _bLoaded = true;

        var cmd = new ExecuteCommandClearFavourites(activator);
        btnClearFavourites.Enabled = !cmd.IsImpossible;

        btnClearFavourites.Click += (s, e) =>
        {
            cmd.Execute();
            btnClearFavourites.Enabled = !cmd.IsImpossible;
        };

        var clearUserSettingsCmd = new ExecuteCommandClearUserSettings(activator);
        btnClearUserSettings.Enabled = true;

        btnClearUserSettings.Click += (s, e) =>
        {
            if (activator.YesNo(
              Core.GlobalStrings.ConfirmClearUserSettings,
                               Core.GlobalStrings.ClearUserSettings
            ))
            {
                clearUserSettingsCmd.Execute();
                Dispose(true);
                var settings = new UserSettingsFileUI(activator);
                settings.Show();
            }
        };
    }

    private Dictionary<CheckBox, PropertyInfo> checkboxDictionary = new();

    private void RegisterCheckbox(CheckBox cb, string propertyName)
    {
        // remember about this checkbox for later
        var prop = typeof(UserSettings).GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
        checkboxDictionary.Add(cb, prop);

        // set initial value from UserSettings
        cb.Checked = (bool)prop.GetValue(null);

        // register callback
        cb.CheckedChanged += CheckboxCheckedChanged;

        // add help
        AddTooltip(cb, propertyName);
    }

    private void AddTooltip(Control c, string propertyName)
    {
        var helpText =
            _activator.CommentStore.GetDocumentationIfExists($"{nameof(UserSettings)}.{propertyName}", false);
        if (string.IsNullOrWhiteSpace(helpText)) return;

        userSettingsToolTips.SetToolTip(c, UsefulStuff.SplitByLength(helpText, MaxTooltipWidth));
    }

    private void Treatment_Putter(object rowObject, object newValue)
    {
        UserSettings.SetErrorReportingLevelFor((ErrorCode)rowObject, (CheckResult)newValue);
    }

    private object Treatment_Getter(object rowObject) => UserSettings.GetErrorReportingLevelFor((ErrorCode)rowObject);

    private void CheckboxCheckedChanged(object sender, EventArgs e)
    {
        if (!_bLoaded)
            return;

        var cb = (CheckBox)sender;
        checkboxDictionary[cb].SetValue(null, cb.Checked);
    }

    private void ddTheme_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_bLoaded)
            return;

        if (ddTheme.SelectedItem is string t)
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
        if (int.TryParse(tbCreateDatabaseTimeout.Text, out var result)) UserSettings.CreateDatabaseTimeout = result;
    }

    private void tbArchiveTriggerTimeout_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(tbArchiveTriggerTimeout.Text, out var result)) UserSettings.ArchiveTriggerTimeout = result;
    }

    private void tbLocalFileSystemLocation_TextChanged(object sender, EventArgs e)
    {
        UserSettings.LocalFileSystemLocation = tbLocalFileSystemLocation.Text;
    }

    private void tbTooltipAppearDelay_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(tbTooltipAppearDelay.Text, out var result)) UserSettings.TooltipAppearDelay = result;
    }

    private void tbFind_TextChanged(object sender, EventArgs e)
    {
        Find(tbFind.Text);
    }

    private void Find(string text)
    {
        foreach (var cb in checkboxDictionary)
            cb.Key.Visible = string.IsNullOrWhiteSpace(text) ||
                             cb.Key.Text.Contains(text, StringComparison.CurrentCultureIgnoreCase) ||
                             cb.Value.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase);
    }

    private void label17_Click(object sender, EventArgs e)
    {

    }

    private void tbWebhookUrl_TextChanged(object sender, EventArgs e)
    {
        UserSettings.ExtractionWebhookUrl = tbWebhookUrl.Text;
    }

    private void tbWebhookUsername_TextChanged(object sender, EventArgs e)
    {
        UserSettings.ExtractionWebhookUsername = tbWebhookUsername.Text;

    }
}