// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.ReusableLibraryCode.Settings;

/// <summary>
/// This is the Settings static class that can be used in your Core solution or in any
/// of your client applications. All settings are laid out the same exact way with getters
/// and setters.
/// </summary>
public static class UserSettings
{
    private static readonly Lazy<RDMPApplicationSettings> Implementation =
        new(static () => new RDMPApplicationSettings(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

    private static RDMPApplicationSettings AppSettings => Implementation.Value ??
                                                          throw new NotImplementedException(
                                                              "Isolated Storage does not work in this environment...");


    public static bool UseLocalFileSystem
    {
        get => AppSettings.GetValueOrDefault("UseLocalFileSystem", false);
        set => AppSettings.AddOrUpdateValue("UseLocalFileSystem", value);
    }

    public static string LocalFileSystemLocation 
    {
        get => AppSettings.GetValueOrDefault("LocalFileSystemLocation", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rdmp")); 
        set => AppSettings.AddOrUpdateValue("LocalFileSystemLocation", value);
    }

    /// <summary>
    /// Show a Yes/No confirmation dialog box when closing RDMP
    /// </summary>
    public static bool ConfirmApplicationExiting
    {
        get => AppSettings.GetValueOrDefault("ConfirmExit", true);
        set => AppSettings.AddOrUpdateValue("ConfirmExit", value);
    }

    /// <summary>
    /// True if the user has accepted the open source license agreements for RDMP and
    /// dependencies used in the software (i.e. MIT and GNU licenses).
    /// </summary>
    public static string LicenseAccepted
    {
        get => AppSettings.GetValueOrDefault("LicenseAccepted", null);
        set => AppSettings.AddOrUpdateValue("LicenseAccepted", value);
    }

    /// <summary>
    /// Automatically launch the RDMP Home Screen on launch of the RDMP application, regardless of the last window you viewed.
    /// </summary>
    public static bool ShowHomeOnStartup
    {
        get => AppSettings.GetValueOrDefault("ShowHomeOnStartup", false);
        set => AppSettings.AddOrUpdateValue("ShowHomeOnStartup", value);
    }

    /// <summary>
    /// If checked series included in graphs that have no values will not be displayed
    /// </summary>
    public static bool IncludeZeroSeriesInGraphs
    {
        get => AppSettings.GetValueOrDefault("IncludeZeroSeriesInGraphs", true);
        set => AppSettings.AddOrUpdateValue("IncludeZeroSeriesInGraphs", value);
    }

    /// <summary>
    /// Adds an additional behaviour when changing tabs that highlights the tree view
    /// collection that has that object visible in it.
    /// </summary>
    public static bool EmphasiseOnTabChanged
    {
        get => AppSettings.GetValueOrDefault("EmphasiseOnTabChanged", false);
        set => AppSettings.AddOrUpdateValue("EmphasiseOnTabChanged", value);
    }

    /// <summary>
    /// True to disable any auto starting tutorials
    /// </summary>
    public static bool DisableTutorials
    {
        get => AppSettings.GetValueOrDefault("DisableTutorials", false);
        set => AppSettings.AddOrUpdateValue("DisableTutorials", value);
    }

    /// <summary>
    /// The connection string to the main RDMP platform database
    /// </summary>
    public static string CatalogueConnectionString
    {
        get => AppSettings.GetValueOrDefault("CatalogueConnectionString", "");
        set => AppSettings.AddOrUpdateValue("CatalogueConnectionString", value);
    }

    /// <summary>
    /// The connection string to the data export RDMP platform database.  This database will contain
    /// references to objects in the <see cref="CatalogueConnectionString"/> database
    /// </summary>
    public static string DataExportConnectionString
    {
        get => AppSettings.GetValueOrDefault("DataExportConnectionString", "");
        set => AppSettings.AddOrUpdateValue("DataExportConnectionString", value);
    }

    /// <summary>
    /// The colour scheme and format for the RDMP gui client application
    /// </summary>
    public static string Theme
    {
        get => AppSettings.GetValueOrDefault("Theme", "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme");
        set => AppSettings.AddOrUpdateValue("Theme", value);
    }

    /// <summary>
    /// When selecting a result from the Find dialog the selected item is pinned in the corresponding view.
    /// </summary>
    public static bool FindShouldPin
    {
        get => AppSettings.GetValueOrDefault("FindShouldPin", true);
        set => AppSettings.AddOrUpdateValue("FindShouldPin", value);
    }

    /// <summary>
    /// Set the amount of time (in seconds) that the Create Database processes should wait before timing out.
    /// </summary>
    public static int CreateDatabaseTimeout
    {
        get => AppSettings.GetValueOrDefault("CreateDatabaseTimeout", 30);
        set => AppSettings.AddOrUpdateValue("CreateDatabaseTimeout",
            DiscoveredServerHelper.CreateDatabaseTimeoutInSeconds = Math.Max(value, 30));
    }

    /// <summary>
    /// Set the amount of time (in milliseconds) that tooltips should take to appear in the tree collection views (e.g. list of Catalogues etc)
    /// </summary>
    public static int TooltipAppearDelay
    {
        get => AppSettings.GetValueOrDefault("TooltipAppearDelay", 750);
        set => AppSettings.AddOrUpdateValue("TooltipAppearDelay", Math.Max(10, value));
    }

    /// <summary>
    /// When using the Find feature this option will automatically filter out any cohort set containers (i.e. UNION / INTERSECT / EXCEPT containers)
    /// </summary>
    public static bool ScoreZeroForCohortAggregateContainers
    {
        get => AppSettings.GetValueOrDefault("ScoreZeroForCohortAggregateContainers", false);
        set => AppSettings.AddOrUpdateValue("ScoreZeroForCohortAggregateContainers", value);
    }

    /// <summary>
    /// Create audit objects for specific objects/changes (e.g. changes to Catalogue Deprecated status).
    /// </summary>
    public static bool EnableCommits
    {
        get => AppSettings.GetValueOrDefault("EnableCommits", true);
        set => AppSettings.AddOrUpdateValue("EnableCommits", value);
    }


    public static bool PromptRenameOnCohortFilterChange
    {
        get => AppSettings.GetValueOrDefault("PromptRenameOnCohortFilterChange", true);
        set => AppSettings.AddOrUpdateValue("PromptRenameOnCohortFilterChange", value);
    }

    public static bool NewFindAndReplace
    {
        get => AppSettings.GetValueOrDefault("NewFindAndReplace", false);
        set => AppSettings.AddOrUpdateValue("NewFindAndReplace", value);
    }


    public static string ExtractionWebhookUsername
    {
        get => AppSettings.GetValueOrDefault("ExtractionWebhookUsername", null);
        set => AppSettings.AddOrUpdateValue("ExtractionWebhookUsername", value);
    }
    public static string ExtractionWebhookUrl
    {
        get => AppSettings.GetValueOrDefault("ExtractionWebhookUrl", null);
        set => AppSettings.AddOrUpdateValue("ExtractionWebhookUrl", value);
    }

    public static bool DefaultLogViewFlat
    {
        get => AppSettings.GetValueOrDefault("DefaultLogViewFlat", false);
        set => AppSettings.AddOrUpdateValue("DefaultLogViewFlat", value);
    }

    #region Catalogue flag visibility settings

    public static bool ShowInternalCatalogues
    {
        get => AppSettings.GetValueOrDefault("ShowInternalCatalogues", true);
        set => AppSettings.AddOrUpdateValue("ShowInternalCatalogues", value);
    }

    public static bool ShowDeprecatedCatalogues
    {
        get => AppSettings.GetValueOrDefault("ShowDeprecatedCatalogues", true);
        set => AppSettings.AddOrUpdateValue("ShowDeprecatedCatalogues", value);
    }

    public static bool ShowColdStorageCatalogues
    {
        get => AppSettings.GetValueOrDefault("ShowColdStorageCatalogues", true);
        set => AppSettings.AddOrUpdateValue("ShowColdStorageCatalogues", value);
    }

    public static bool ShowProjectSpecificCatalogues
    {
        get => AppSettings.GetValueOrDefault("ShowProjectSpecificCatalogues", true);
        set => AppSettings.AddOrUpdateValue("ShowProjectSpecificCatalogues", value);
    }

    public static bool ShowNonExtractableCatalogues
    {
        get => AppSettings.GetValueOrDefault("ShowNonExtractableCatalogues", true);
        set => AppSettings.AddOrUpdateValue("ShowNonExtractableCatalogues", value);
    }

    /// <summary>
    /// True to apply theme changes to context menus and tool strips.
    /// </summary>
    public static bool ApplyThemeToMenus
    {
        get => AppSettings.GetValueOrDefault("ApplyThemeToMenus", true);
        set => AppSettings.AddOrUpdateValue("ApplyThemeToMenus", value);
    }

    /// <summary>
    /// Determines line wrapping in multi line editor controls when lines stretch off the control client area
    /// </summary>
    public static int WrapMode
    {
        get => AppSettings.GetValueOrDefault("WrapMode", 0);
        set => AppSettings.AddOrUpdateValue("WrapMode", value);
    }

    /// <summary>
    /// <para>Base colours used for generating heatmaps in HEX format.  Colour intensity will vary
    /// from the first color to the second.</para>
    /// 
    /// <para>The first colour represents the lowest values and should
    /// typically be darker than the second which represents high values.</para>
    /// </summary>
    public static string HeatMapColours
    {
        get => AppSettings.GetValueOrDefault("HeatMapColours", null);
        set => AppSettings.AddOrUpdateValue("HeatMapColours", value);
    }

    /// <summary>
    /// <para>Adds a 5 second delay after startup</para>
    /// <para>Use this option to add a delay that can be helpful for troubleshooting issues on RDMP startup. </para>
    /// </summary>
    public static bool Wait5SecondsAfterStartupUI
    {
        get => AppSettings.GetValueOrDefault("Wait5SecondsAfterStartupUI", true);
        set => AppSettings.AddOrUpdateValue("Wait5SecondsAfterStartupUI", value);
    }

    /// <summary>
    /// True to show the cohort creation wizard when creating new cohorts.  False to create
    /// a default empty configuration.
    /// </summary>
    public static bool ShowCohortWizard
    {
        get => AppSettings.GetValueOrDefault("ShowCohortWizard", false);
        set => AppSettings.AddOrUpdateValue("ShowCohortWizard", value);
    }

    /// <summary>
    /// <para>True to enable "stirct validation" for containers in Cohort Builder Queries.</para>
    ///
    /// <para>Will not allow empty sets, or sets that only have one item.</para>
    /// </summary>
    public static bool StrictValidationForCohortBuilderContainers
    {
        get => AppSettings.GetValueOrDefault("StrictValidationForCohortBuilderContainers", true);
        set => AppSettings.AddOrUpdateValue("StrictValidationForCohortBuilderContainers", value);
    }

    /// <summary>
    /// Changes the behaviour of mouse double clicks in tree views.  When enabled double
    /// click expands nodes instead of opening the double clicked object (the default behaviour).
    /// </summary>
    public static bool DoubleClickToExpand
    {
        get => AppSettings.GetValueOrDefault("DoubleClickToExpand", false);
        set => AppSettings.AddOrUpdateValue("DoubleClickToExpand", value);
    }

    public static string RecentHistory
    {
        get => AppSettings.GetValueOrDefault("RecentHistory", "");
        set => AppSettings.AddOrUpdateValue("RecentHistory", value);
    }

    /// <summary>
    /// <para>When enabled RDMP will record certain performance related metrics (how long refresh takes etc).</para>
    /// <para>These figures are completely internal to the application and are not transmitted anywhere.You can view the results in the toolbar.</para>
    /// </summary>
    public static bool DebugPerformance
    {
        get => AppSettings.GetValueOrDefault("DebugPerformance", false);
        set => AppSettings.AddOrUpdateValue("DebugPerformance", value);
    }

    /// <summary>
    /// <para>Automatically resize columns in the RDMP user interface with fit contents.</para>
    /// <para>Can be disabled if problems arise with column content or header visibility</para>
    /// </summary>
    public static bool AutoResizeColumns
    {
        get => AppSettings.GetValueOrDefault("AutoResizeColumns", true);
        set => AppSettings.AddOrUpdateValue("AutoResizeColumns", value);
    }


    /// <summary>
    /// Show a popup confirmation dialog at the end of a pipeline completing execution
    /// </summary>
    public static bool ShowPipelineCompletedPopup
    {
        get => AppSettings.GetValueOrDefault("ShowPipelineCompletedPopup", true);
        set => AppSettings.AddOrUpdateValue("ShowPipelineCompletedPopup", value);
    }

    /// <summary>
    /// <para>Enable to skip the checking stage of pipeline source component CohortIdentificationConfigurationSource.</para>
    /// <para>In slow computer, or contesest databases this can take a while to compile. This option lets you disable it.</para>
    /// </summary>
    public static bool SkipCohortBuilderValidationOnCommit
    {
        get => AppSettings.GetValueOrDefault("SkipCohortBuilderValidationOnCommit", false);
        set => AppSettings.AddOrUpdateValue("SkipCohortBuilderValidationOnCommit", value);
    }

    public static string ConsoleColorScheme
    {
        get => AppSettings.GetValueOrDefault("ConsoleColorScheme", "default");
        set => AppSettings.AddOrUpdateValue("ConsoleColorScheme", value);
    }

    /// <summary>
    /// <para>When true RDMP log viewer will hide table load audits where no inserts/updates/deletes were applied.</para>
    /// <para>This is helpful if a load targets many tables not all of which will be updated in a given run</para>
    /// </summary>
    public static bool HideEmptyTableLoadRunAudits
    {
        get => AppSettings.GetValueOrDefault("HideEmptyTableLoadRunAudits", false);
        set => AppSettings.AddOrUpdateValue("HideEmptyTableLoadRunAudits", value);
    }

    /// <summary>
    /// <para>Enables additional Find filters for objects that are in:</para>
    /// <para>Cold Storage, Internal, Deprecated, Project Specific and Non Extractable</para>
    /// </summary>
    public static bool AdvancedFindFilters
    {
        get => AppSettings.GetValueOrDefault("AdvancedFindFilters", false);
        set => AppSettings.AddOrUpdateValue("AdvancedFindFilters", value);
    }

    /// <summary>
    /// Timeout in seconds to allow for creating archive trigger, index etc
    /// </summary>
    public static int ArchiveTriggerTimeout
    {
        get => AppSettings.GetValueOrDefault("ArchiveTriggerTimeout", 30);
        set => AppSettings.AddOrUpdateValue("ArchiveTriggerTimeout", value);
    }

    public static int FindWindowWidth
    {
        get => AppSettings.GetValueOrDefault("FindWindowWidth", 730);
        set => AppSettings.AddOrUpdateValue("FindWindowWidth", value);
    }

    public static int FindWindowHeight
    {
        get => AppSettings.GetValueOrDefault("FindWindowHeight", 400);
        set => AppSettings.AddOrUpdateValue("FindWindowHeight", value);
    }

    /// <summary>
    /// Enable to refresh only objects which you make changes to instead of
    /// fetching all database changes since your last edit.  This improves
    /// performance in large RDMP deployments with thousands of Projects configured.
    /// </summary>
    public static bool SelectiveRefresh
    {
        get => AppSettings.GetValueOrDefault("SelectiveRefresh", false);
        set => AppSettings.AddOrUpdateValue("SelectiveRefresh", value);
    }

    /// <summary>
    /// Set to true to always attempt to force joins on all tables under a Catalogue
    /// when building queries (in Cohort Builder).  This makes it impossible to untick
    /// force joins.
    /// </summary>
    public static bool AlwaysJoinEverything
    {
        get => AppSettings.GetValueOrDefault("AlwaysJoinEverything", false);
        set => AppSettings.AddOrUpdateValue("AlwaysJoinEverything", value);
    }

    /// <summary>
    /// <para>
    /// Determines whether queries are automatically sent and results displayed in
    /// data tabs in RDMP (e.g. View top 100 etc).  Enable to automatically send the
    /// queries.  Disable to show the SQL but require the user to press F5 or click Run
    /// to execute.
    /// </para>
    /// </summary>
    public static bool AutoRunSqlQueries
    {
        get => AppSettings.GetValueOrDefault("AutoRunSqlQueries", false);
        set => AppSettings.AddOrUpdateValue("AutoRunSqlQueries", value);
    }

    /// <summary>
    /// Enable to automatically expand the tree when opening or creating cohorts in
    /// Cohort Builder
    /// </summary>
    public static bool ExpandAllInCohortBuilder
    {
        get => AppSettings.GetValueOrDefault("ExpandAllInCohortBuilder", true);
        set => AppSettings.AddOrUpdateValue("ExpandAllInCohortBuilder", value);
    }

    /// <summary>
    /// True to show ProjectSpecific Catalogues' columns in extraction configuration user interface
    /// </summary>
    public static bool ShowProjectSpecificColumns
    {
        get => AppSettings.GetValueOrDefault("ShowProjectSpecificColumns", true);
        set => AppSettings.AddOrUpdateValue("ShowProjectSpecificColumns", value);
    }

    /// <summary>
    /// <para>When generating an aggregate graph, use the column alias instead of the select sql.  For example
    /// when you have the select column 'SELECT YEAR(dt) as myYear' then the GROUP BY will default to
    /// 'GROUP BY YEAR(dt)'.  Setting this property to true will instead use 'GROUP BY myYear'.  Typically
    /// this only works in MySql but it is not universally supported by all MySql versions and server settings
    /// </para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public static bool UseAliasInsteadOfTransformInGroupByAggregateGraphs
    {
        get => AppSettings.GetValueOrDefault("ShowProjectSpecificColumns", false);
        set => AppSettings.AddOrUpdateValue("ShowProjectSpecificColumns", value);
    }

    #endregion

    /// <summary>
    /// Returns the error level the user wants for <paramref name="errorCode"/> or <see cref="ErrorCode.DefaultTreatment"/> (if no custom
    /// reporting level has been set up for this error code).
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public static CheckResult GetErrorReportingLevelFor(ErrorCode errorCode)
    {
        var result = AppSettings.GetValueOrDefault($"EC_{errorCode.Code}", errorCode.DefaultTreatment.ToString());

        return Enum.Parse<CheckResult>(result);
    }

    /// <summary>
    /// Changes the reporting level of the given error to <paramref name="value"/> instead of its <see cref="ErrorCode.DefaultTreatment"/>
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="value"></param>
    public static void SetErrorReportingLevelFor(ErrorCode errorCode, CheckResult value)
    {
        AppSettings.AddOrUpdateValue($"EC_{errorCode.Code}", value.ToString());
    }

    public static bool GetTutorialDone(Guid tutorialGuid) =>
        tutorialGuid != Guid.Empty && AppSettings.GetValueOrDefault($"T_{tutorialGuid:N}", false);

    public static void SetTutorialDone(Guid tutorialGuid, bool value)
    {
        if (tutorialGuid == Guid.Empty)
            return;

        AppSettings.AddOrUpdateValue($"T_{tutorialGuid:N}", value);
    }

    public static void SetColumnWidth(Guid columnGuid, int width)
    {
        if (columnGuid == Guid.Empty)
            return;
        SetColumnWidth(columnGuid.ToString("N"), width);
    }

    public static void SetColumnWidth(string colIdentifier, int width)
    {
        AppSettings.AddOrUpdateValue($"ColW_{colIdentifier}", width);
    }

    public static void SetColumnVisible(Guid columnGuid, bool visible)
    {
        if (columnGuid == Guid.Empty)
            return;

        SetColumnVisible(columnGuid.ToString("N"), visible);
    }

    public static void SetColumnVisible(string colIdentifier, bool visible)
    {
        AppSettings.AddOrUpdateValue($"ColV_{colIdentifier}", visible);
    }

    public static int GetColumnWidth(Guid columnGuid) =>
        columnGuid == Guid.Empty ? 100 : GetColumnWidth(columnGuid.ToString("N"));

    public static int GetColumnWidth(string colIdentifier) =>
        AppSettings.GetValueOrDefault($"ColW_{colIdentifier}", 100);

    public static bool GetColumnVisible(Guid columnGuid) =>
        columnGuid == Guid.Empty || GetColumnVisible(columnGuid.ToString("N"));

    public static bool GetColumnVisible(string colIdentifier) =>
        AppSettings.GetValueOrDefault($"ColV_{colIdentifier}", true);

    public static string[] GetHistoryForControl(Guid controlGuid)
    {
        return AppSettings.GetValueOrDefault($"A_{controlGuid:N}", "").Split(new[] { "#!#" }, StringSplitOptions.None);
    }

    public static void SetHistoryForControl(Guid controlGuid, IEnumerable<string> history)
    {
        AppSettings.AddOrUpdateValue($"A_{controlGuid:N}", string.Join("#!#", history));
    }

    public static void AddHistoryForControl(Guid guid, string v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return;

        var l = GetHistoryForControl(guid).ToList();

        if (l.Contains(v))
            return;

        l.Add(v);

        SetHistoryForControl(guid, l.Distinct().ToList());
    }

    public static Tuple<string, bool> GetLastColumnSortForCollection(Guid controlGuid)
    {
        lock (_oLockUserSettings)
        {
            var value = AppSettings.GetValueOrDefault($"LastColumnSort_{controlGuid:N}", null);

            //if we don't have a value
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var args = value.Split(new[] { "#!#" }, StringSplitOptions.RemoveEmptyEntries);

            //or it doesn't split properly
            if (args.Length != 2)
                return null;

            //or either element is null
            if (string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
                return null;

            if (bool.TryParse(args[1], out var ascending))
                return Tuple.Create(args[0], ascending);
        }

        return null;
    }

    private static object _oLockUserSettings = new();

    public static void SetLastColumnSortForCollection(Guid controlGuid, string columnName, bool ascending)
    {
        lock (_oLockUserSettings)
        {
            AppSettings.AddOrUpdateValue($"LastColumnSort_{controlGuid:N}", $"{columnName}#!#{ascending}");
        }
    }


    /// <summary>
    /// Returns the last known manually set splitter distance for the Control who is
    /// identified by <paramref name="controlGuid"/> or -1 if none set yet
    /// </summary>
    /// <param name="controlGuid"></param>
    /// <returns></returns>
    public static int GetSplitterDistance(Guid controlGuid) =>
        AppSettings.GetValueOrDefault($"SplitterDistance_{controlGuid:N}", -1);

    /// <summary>
    /// Records that the user has manually changed the splitter distance of the Control
    /// who is identified by <paramref name="controlGuid"/>
    /// </summary>
    /// <param name="controlGuid"></param>
    /// <param name="splitterDistance"></param>
    public static void SetSplitterDistance(Guid controlGuid, int splitterDistance)
    {
        lock (_oLockUserSettings)
        {
            AppSettings.AddOrUpdateValue($"SplitterDistance_{controlGuid:N}", splitterDistance);
        }
    }

    public static void ClearUserSettings()
    {
        AppSettings.Clear();
    }
}