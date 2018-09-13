using System;
using System.Collections.Generic;
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

        public static bool LicenseAccepted
        {
            get { return AppSettings.GetValueOrDefault("LicenseAccepted", false); }
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
        
        public static bool GetTutorialDone(Guid tutorialGuid)
        {
            return AppSettings.GetValueOrDefault("T_" + tutorialGuid.ToString("N"), false); 
        }

        public static void SetTutorialDone(Guid tutorialGuid,bool value)
        {
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

        static ISettings CreateSettings()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
            return new RDMPApplicationSettings();
#endif
        }
    }
}
