// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;
using Plugin.Settings.Abstractions;

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

        public static bool ConfirmApplicationExiting
        {
            get { return AppSettings.GetValueOrDefault("ConfirmExit", true); }
            set { AppSettings.AddOrUpdateValue("ConfirmExit", value); }
        }

        public static string LicenseAccepted
        {
            get { return AppSettings.GetValueOrDefault("LicenseAccepted",null); }
            set { AppSettings.AddOrUpdateValue("LicenseAccepted", value); }
        }

        public static bool ShowHomeOnStartup
        {
            get { return AppSettings.GetValueOrDefault("ShowHomeOnStartup", false); }
            set { AppSettings.AddOrUpdateValue("ShowHomeOnStartup", value); }
        }

        public static bool EmphasiseOnTabChanged
        {
            get { return AppSettings.GetValueOrDefault("EmphasiseOnTabChanged", false); }
            set { AppSettings.AddOrUpdateValue("EmphasiseOnTabChanged", value); }
        }

        public static bool DisableTutorials
        {
            get { return AppSettings.GetValueOrDefault("DisableTutorials", false); }
            set { AppSettings.AddOrUpdateValue("DisableTutorials", value); }
        }

        public static string CatalogueConnectionString
        {
            get { return AppSettings.GetValueOrDefault("CatalogueConnectionString", ""); }
            set { AppSettings.AddOrUpdateValue("CatalogueConnectionString", value); }
        }

        public static string DataExportConnectionString
        {
            get { return AppSettings.GetValueOrDefault("DataExportConnectionString", ""); }
            set { AppSettings.AddOrUpdateValue("DataExportConnectionString", value); }
        }

        /// <summary>
        /// Controls whether RDMP permits cohorts to be created where the release ID and private ID are the same (i.e. the linkage ids are not anonymised).
        /// </summary>
        public static bool AllowIdentifiableExtractions
        {
            get { return AppSettings.GetValueOrDefault("AllowIdentifiableExtractions", false); }
            set { AppSettings.AddOrUpdateValue("AllowIdentifiableExtractions", value); }
        }

        public static string Theme
        {
            get { return AppSettings.GetValueOrDefault("Theme", "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme"); }
            set { AppSettings.AddOrUpdateValue("Theme", value); }
        }

        public static bool FindShouldPin
        {
            get { return AppSettings.GetValueOrDefault("FindShouldPin", true); }
            set { AppSettings.AddOrUpdateValue("FindShouldPin", value); }
        }

        public static int CreateDatabaseTimeout
        {
            get { return AppSettings.GetValueOrDefault("CreateDatabaseTimeout", 30); }
            set { AppSettings.AddOrUpdateValue("CreateDatabaseTimeout", DiscoveredServerHelper.CreateDatabaseTimeoutInSeconds = Math.Max(value,30)); }
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

        public static bool ShowColumnProjectNumber
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnProjectNumber", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnProjectNumber", value); }
        }

        public static bool ShowColumnCohortSource
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnCohortSource", false); }
            set { AppSettings.AddOrUpdateValue("ShowColumnCohortSource", value); }
        }

        public static bool ShowColumnCohortVersion
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnCohortVersion", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnCohortVersion", value); }
        }

        public static bool ShowColumnFavourite
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnFavourite", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnFavourite", value); }
        }

        public static bool ShowColumnCheck
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnCheck", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnCheck", value); }
        }

        public static bool ShowColumnFilters
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnFilters", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnFilters", value); }
        }
        public static bool ShowColumnValue
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnValue", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnValue", value); }
        }
        public static bool ShowOrderColumn
        {
            get { return AppSettings.GetValueOrDefault("ShowOrderColumn", true); }
            set { AppSettings.AddOrUpdateValue("ShowOrderColumn", value); }
        }

        public static bool ShowColumnDataType
        {
            get { return AppSettings.GetValueOrDefault("ShowColumnDataType", true); }
            set { AppSettings.AddOrUpdateValue("ShowColumnDataType", value); }
        }

        public static bool ApplyThemeToMenus
        {
            get { return AppSettings.GetValueOrDefault("ApplyThemeToMenus", true); }
            set { AppSettings.AddOrUpdateValue("ApplyThemeToMenus", value); }
        }

        public static int WrapMode
        {
            get { return AppSettings.GetValueOrDefault("WrapMode", 0); }
            set { AppSettings.AddOrUpdateValue("WrapMode", value); }
        }

        public static string HeatMapColours 
        {
            get { return AppSettings.GetValueOrDefault("HeatMapColours", null); }
            set { AppSettings.AddOrUpdateValue("HeatMapColours", value); }
        }
        
        public static bool Wait5SecondsAfterStartupUI
        {
            get { return AppSettings.GetValueOrDefault("Wait5SecondsAfterStartupUI", true); }
            set { AppSettings.AddOrUpdateValue("Wait5SecondsAfterStartupUI", value); }
        }

        public static bool ShowCohortWizard
        {
            get { return AppSettings.GetValueOrDefault("ShowCohortWizard", false); }
            set { AppSettings.AddOrUpdateValue("ShowCohortWizard", value); }
        }

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

        public static bool DebugPerformance { 
            get { return AppSettings.GetValueOrDefault("DebugPerformance", false); }
            set { AppSettings.AddOrUpdateValue("DebugPerformance", value); } }

        public static bool ShowPipelineCompletedPopup { 
            get { return AppSettings.GetValueOrDefault("ShowPipelineCompletedPopup", false); }
            set { AppSettings.AddOrUpdateValue("ShowPipelineCompletedPopup", value); } }

        public static bool WarnOnTimeoutOnExtractionChecks {
            get { return AppSettings.GetValueOrDefault("WarnOnTimeoutOnExtractionChecks", true); }
            set { AppSettings.AddOrUpdateValue("WarnOnTimeoutOnExtractionChecks", value); }
        }

        public static string ConsoleColorScheme
        {
            get { return AppSettings.GetValueOrDefault("ConsoleColorScheme", "default"); }
            set { AppSettings.AddOrUpdateValue("ConsoleColorScheme", value); }
        }

        public static bool HideEmptyTableLoadRunAudits
        {
            get { return AppSettings.GetValueOrDefault("HideEmptyTableLoadRunAudits", false); }
            set { AppSettings.AddOrUpdateValue("HideEmptyTableLoadRunAudits", value); }
        }


        #endregion

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
