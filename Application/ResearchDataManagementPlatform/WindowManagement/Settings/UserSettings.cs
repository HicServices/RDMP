using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ResearchDataManagementPlatform.WindowManagement.Settings
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class UserSettings
    {
        private static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
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
    }
}
