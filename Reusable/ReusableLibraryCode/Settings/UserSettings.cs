// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;
using Plugin.Settings.Abstractions;
using ReusableLibraryCode.Checks;
using static ReusableLibraryCode.Checks.CheckEventArgs;

namespace ReusableLibraryCode.Settings
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class UserSettings
    {
        static Lazy<ISettings> implementation = new Lazy<ISettings>(() => CreateSettings(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        private static ISettings AppSettings
        {
            get
            {
                ISettings ret = implementation.Value;
                if (ret == null)
                {
                    throw new NotImplementedException("Isolated Storage does not work in this environment...");
                }
                return ret;
            }
        }

        /// <summary>
        /// Show a Yes/No confirmation dialog box when closing RDMP
        /// </summary>
        public static bool ConfirmApplicationExiting
        {
            get { return AppSettings.GetValueOrDefault("ConfirmExit", true); }
            set { AppSettings.AddOrUpdateValue("ConfirmExit", value); }
        }

        /// <summary>
        /// True if the user has accepted the open source license agreements for RDMP and
        /// dependencies used in the software (i.e. MIT and GNU licenses).
        /// </summary>
        public static string LicenseAccepted
        {
            get { return AppSettings.GetValueOrDefault("LicenseAccepted",null); }
            set { AppSettings.AddOrUpdateValue("LicenseAccepted", value); }
        }

        /// <summary>
        /// Automatically launch the RDMP Home Screen on launch of the RDMP application, regardless of the last window you viewed.
        /// </summary>
        public static bool ShowHomeOnStartup
        {
            get { return AppSettings.GetValueOrDefault("ShowHomeOnStartup", false); }
            set { AppSettings.AddOrUpdateValue("ShowHomeOnStartup", value); }
        }

        /// <summary>
        /// If checked series included in graphs that have no values will not be displayed
        /// </summary>
        public static bool IncludeZeroSeriesInGraphs
        {
            get { return AppSettings.GetValueOrDefault("IncludeZeroSeriesInGraphs", true); }
            set { AppSettings.AddOrUpdateValue("IncludeZeroSeriesInGraphs", value); }
        }

        /// <summary>
        /// Adds an additional behaviour when changing tabs that highlights the tree view
        /// collection that has that object visible in it.
        /// </summary>
        public static bool EmphasiseOnTabChanged
        {
            get { return AppSettings.GetValueOrDefault("EmphasiseOnTabChanged", false); }
            set { AppSettings.AddOrUpdateValue("EmphasiseOnTabChanged", value); }
        }

        /// <summary>
        /// True to disable any auto starting tutorials
        /// </summary>
        public static bool DisableTutorials
        {
            get { return AppSettings.GetValueOrDefault("DisableTutorials", false); }
            set { AppSettings.AddOrUpdateValue("DisableTutorials", value); }
        }

        /// <summary>
        /// The connection string to the main RDMP platform database
        /// </summary>
        public static string CatalogueConnectionString
        {
            get { return AppSettings.GetValueOrDefault("CatalogueConnectionString", ""); }
            set { AppSettings.AddOrUpdateValue("CatalogueConnectionString", value); }
        }

        /// <summary>
        /// The connection string to the data export RDMP platform database.  This database will contain
        /// refrerences to objects in the <see cref="CatalogueConnectionString"/> database
        /// </summary>
        public static string DataExportConnectionString
        {
            get { return AppSettings.GetValueOrDefault("DataExportConnectionString", ""); }
            set { AppSettings.AddOrUpdateValue("DataExportConnectionString", value); }
        }

        /// <summary>
        /// <para>Controls whether RDMP permits cohorts to be created where the release ID and private ID are the same (i.e. the linkage ids are not anonymised).</para>
        /// 
        /// <para>Changing this setting will not affect how cohorts are currently configured or extracted, it only supresses a specific error message that is generated when a cohort source is configured where the private and release identifiers reference the same column</para>
        /// </summary>
        public static bool AllowIdentifiableExtractions
        {
            get { return AppSettings.GetValueOrDefault("AllowIdentifiableExtractions", false); }
            set { AppSettings.AddOrUpdateValue("AllowIdentifiableExtractions", value); }
        }

        /// <summary>
        /// The colour scheme and format for the RDMP gui client application
        /// </summary>
        public static string Theme
        {
            get { return AppSettings.GetValueOrDefault("Theme", "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme"); }
            set { AppSettings.AddOrUpdateValue("Theme", value); }
        }

        /// <summary>
        /// When selecting a result from the Find dialog the selected item is pinned in the corresponding view.
        /// </summary>
        public static bool FindShouldPin
        {
            get { return AppSettings.GetValueOrDefault("FindShouldPin", true); }
            set { AppSettings.AddOrUpdateValue("FindShouldPin", value); }
        }

        /// <summary>
        /// Set the amount of time (in seconds) that the Create Database processes should wait before timing out.
        /// </summary>
        public static int CreateDatabaseTimeout
        {
            get { return AppSettings.GetValueOrDefault("CreateDatabaseTimeout", 30); }
            set { AppSettings.AddOrUpdateValue("CreateDatabaseTimeout", DiscoveredServerHelper.CreateDatabaseTimeoutInSeconds = Math.Max(value,30)); }
        }

        /// <summary>
        /// Set the amount of time (in milliseconds) that tooltips should take to appear in the tree collection views (e.g. list of Cataologues etc)
        /// </summary>
        public static int TooltipAppearDelay
        {
            get { return AppSettings.GetValueOrDefault("TooltipAppearDelay", 750); }
            set { AppSettings.AddOrUpdateValue("TooltipAppearDelay", Math.Max(10,value)); }
        }

        /// <summary>
        /// When using the Find feature this option will automatically filter out any cohort set containers (i.e. UNION / INTERSECT / EXCEPT containers)
        /// </summary>
        public static bool ScoreZeroForCohortAggregateContainers
        {
            get { return AppSettings.GetValueOrDefault("ScoreZeroForCohortAggregateContainers", false); }
            set { AppSettings.AddOrUpdateValue("ScoreZeroForCohortAggregateContainers", value); }
        }

        

        #region Catalogue flag visibility settings
        public static bool ShowInternalCatalogues
        {
            get { return AppSettings.GetValueOrDefault("ShowInternalCatalogues", true); }
            set { AppSettings.AddOrUpdateValue("ShowInternalCatalogues", value); }
        }
        public static bool ShowDeprecatedCatalogues
        {
            get { return AppSettings.GetValueOrDefault("ShowDeprecatedCatalogues", true); }
            set { AppSettings.AddOrUpdateValue("ShowDeprecatedCatalogues", value); }
        }
        public static bool ShowColdStorageCatalogues
        {
            get { return AppSettings.GetValueOrDefault("ShowColdStorageCatalogues", true); }
            set { AppSettings.AddOrUpdateValue("ShowColdStorageCatalogues", value); }
        }
        public static bool ShowProjectSpecificCatalogues
        {
            get { return AppSettings.GetValueOrDefault("ShowProjectSpecificCatalogues", true); }
            set { AppSettings.AddOrUpdateValue("ShowProjectSpecificCatalogues", value); }
        }
        public static bool ShowNonExtractableCatalogues
        {
            get { return AppSettings.GetValueOrDefault("ShowNonExtractableCatalogues", true); }
            set { AppSettings.AddOrUpdateValue("ShowNonExtractableCatalogues", value); }
        }

        /// <summary>
        /// True to apply theme changes to context menus and tool strips.
        /// </summary>
        public static bool ApplyThemeToMenus
        {
            get { return AppSettings.GetValueOrDefault("ApplyThemeToMenus", true); }
            set { AppSettings.AddOrUpdateValue("ApplyThemeToMenus", value); }
        }

        /// <summary>
        /// Determines line wrapping in multi line editor controls when lines stretch off the control client area
        /// </summary>
        public static int WrapMode
        {
            get { return AppSettings.GetValueOrDefault("WrapMode", 0); }
            set { AppSettings.AddOrUpdateValue("WrapMode", value); }
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
            get { return AppSettings.GetValueOrDefault("HeatMapColours", null); }
            set { AppSettings.AddOrUpdateValue("HeatMapColours", value); }
        }

        /// <summary>
        /// <para>Adds a 5 second delay after startup</para>
        /// <para>Use this option to add a delay that can be helpful for troubleshooting issues on RDMP startup. </para>
        /// </summary>
        public static bool Wait5SecondsAfterStartupUI
        {
            get { return AppSettings.GetValueOrDefault("Wait5SecondsAfterStartupUI", true); }
            set { AppSettings.AddOrUpdateValue("Wait5SecondsAfterStartupUI", value); }
        }

        /// <summary>
        /// True to show the cohort creation wizard when creating new cohorts.  False to create
        /// a default empty configuration.
        /// </summary>
        public static bool ShowCohortWizard
        {
            get { return AppSettings.GetValueOrDefault("ShowCohortWizard", false); }
            set { AppSettings.AddOrUpdateValue("ShowCohortWizard", value); }
        }

        /// <summary>
        /// Changes the behaviour of mouse double clicks in tree views.  When enabled double
        /// click expands nodes instead of opening the double clicked object (the default behaviour).
        /// </summary>
        public static bool DoubleClickToExpand
        {
            get { return AppSettings.GetValueOrDefault("DoubleClickToExpand", false); }
            set { AppSettings.AddOrUpdateValue("DoubleClickToExpand", value); }
        }

        public static string RecentHistory
        {
            get { return AppSettings.GetValueOrDefault("RecentHistory", ""); }
            set { AppSettings.AddOrUpdateValue("RecentHistory", value); }
        }

        /// <summary>
        /// <para>When enabled RDMP will record certain performance related metrics (how long refresh takes etc).</para>
        /// <para>These figures are completely internal to the application and are not transmitted anywhere.You can view the results in the toolbar.</para>
        /// </summary>
        public static bool DebugPerformance { 
            get { return AppSettings.GetValueOrDefault("DebugPerformance", false); }
            set { AppSettings.AddOrUpdateValue("DebugPerformance", value); } }

        /// <summary>
        /// Show a popup confirmation dialog at the end of a pipeline completing execution
        /// </summary>
        public static bool ShowPipelineCompletedPopup { 
            get { return AppSettings.GetValueOrDefault("ShowPipelineCompletedPopup", false); }
            set { AppSettings.AddOrUpdateValue("ShowPipelineCompletedPopup", value); } }

        public static string ConsoleColorScheme
        {
            get { return AppSettings.GetValueOrDefault("ConsoleColorScheme", "default"); }
            set { AppSettings.AddOrUpdateValue("ConsoleColorScheme", value); }
        }

        /// <summary>
        /// <para>When true RDMP log viewer will hide table load audits where no inserts/updates/deletes were applied.</para>
        /// <para>This is helpful if a load targets many tables not all of which will be updated in a given run</para>
        /// </summary>
        public static bool HideEmptyTableLoadRunAudits
        {
            get { return AppSettings.GetValueOrDefault("HideEmptyTableLoadRunAudits", false); }
            set { AppSettings.AddOrUpdateValue("HideEmptyTableLoadRunAudits", value); }
        }

        /// <summary>
        /// <para>Enables additional Find filters for objects that are in:</para>
        /// <para>Cold Storage, Internal, Deprecated, Project Specific and Non Extractable</para>
        /// </summary>
        public static bool AdvancedFindFilters
        {
            get { return AppSettings.GetValueOrDefault("AdvancedFindFilters", false); }
            set { AppSettings.AddOrUpdateValue("AdvancedFindFilters", value); }
        }

        /// <summary>
        /// Timeout in seconds to allow for creating archive trigger, index etc
        /// </summary>
        public static int ArchiveTriggerTimeout
        {
            get { return AppSettings.GetValueOrDefault("ArchiveTriggerTimeout", 30); }
            set { AppSettings.AddOrUpdateValue("ArchiveTriggerTimeout", value); }
        }

        public static int FindWindowWidth
        {
            get { return AppSettings.GetValueOrDefault("FindWindowWidth", 300); }
            set { AppSettings.AddOrUpdateValue("FindWindowWidth", value); }
        }
        public static int FindWindowHeight
        {
            get { return AppSettings.GetValueOrDefault("FindWindowHeight", 500); }
            set { AppSettings.AddOrUpdateValue("FindWindowHeight", value); }
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
            var result = AppSettings.GetValueOrDefault("EC_" + errorCode.Code, errorCode.DefaultTreatment.ToString());

            return Enum.Parse<CheckResult>(result);
        }

        /// <summary>
        /// Changes the reporting level of the given error to <paramref name="value"/> instead of it's <see cref="ErrorCode.DefaultTreatment"/>
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="value"></param>
        public static void SetErrorReportingLevelFor(ErrorCode errorCode,CheckResult value)
        {
            AppSettings.AddOrUpdateValue("EC_" + errorCode.Code, value.ToString());
        }

        public static bool GetTutorialDone(Guid tutorialGuid)
        {
            if(tutorialGuid == Guid.Empty)
                return false;

            return AppSettings.GetValueOrDefault("T_" + tutorialGuid.ToString("N"), false); 
        }
        
        public static void SetTutorialDone(Guid tutorialGuid,bool value)
        {
            if(tutorialGuid == Guid.Empty)
                return;

            AppSettings.AddOrUpdateValue("T_" + tutorialGuid.ToString("N"), value);
        }

        public static void SetColumnWidth(Guid columnGuid, int width)
        {
            if (columnGuid == Guid.Empty)
                return;

            AppSettings.AddOrUpdateValue("ColW_" + columnGuid.ToString("N"), width);
        }

        public static void SetColumnVisible(Guid columnGuid, bool visible)
        {
            if (columnGuid == Guid.Empty)
                return;

            AppSettings.AddOrUpdateValue("ColV_" + columnGuid.ToString("N"), visible);
        }
        public static int GetColumnWidth(Guid columnGuid)
        {
            if (columnGuid == Guid.Empty)
                return 100;

            return AppSettings.GetValueOrDefault("ColW_" + columnGuid.ToString("N"), 100);
        }
        public static bool GetColumnVisible(Guid columnGuid)
        {
            if (columnGuid == Guid.Empty)
                return true;

            return AppSettings.GetValueOrDefault("ColV_" + columnGuid.ToString("N"), true);
        }
        public static string[] GetHistoryForControl(Guid controlGuid)
        {
            return AppSettings.GetValueOrDefault("A_" +controlGuid.ToString("N"), "").Split(new []{"#!#"},StringSplitOptions.RemoveEmptyEntries);
        }
        
        public static void SetHistoryForControl(Guid controlGuid,IEnumerable<string> history)
        {
            AppSettings.AddOrUpdateValue("A_" + controlGuid.ToString("N"), string.Join("#!#", history));
        }
        public static Tuple<string,bool> GetLastColumnSortForCollection(Guid controlGuid)
        {
            lock (_oLockUserSettings)
            {
                var value = AppSettings.GetValueOrDefault("LastColumnSort_" + controlGuid.ToString("N"), null);

                //if we dont have a value
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                string[] args = value.Split(new[] {"#!#"}, StringSplitOptions.RemoveEmptyEntries);

                //or it doesn't split properly 
                if (args.Length != 2)
                    return null;

                //or either element is null
                if (string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
                    return null;

                bool ascending;
                if (bool.TryParse(args[1], out ascending))
                    return Tuple.Create(args[0], ascending);
            }

            return null;
        }

        private static object _oLockUserSettings = new object();
        public static void SetLastColumnSortForCollection(Guid controlGuid, string columnName, bool ascending)
        {
            lock (_oLockUserSettings)
            {
                AppSettings.AddOrUpdateValue("LastColumnSort_" + controlGuid.ToString("N"), columnName +"#!#" + ascending);    
            }
        }
        

        static ISettings CreateSettings()
        {
            return new RDMPApplicationSettings();
        }

    }

}
