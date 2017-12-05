using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Dashboarding;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform.WindowManagement.UserSettings
{
    /// <summary>
    /// Class for maintaining the users settings file which records user configurable variables e.g. whether to show HomeUI on startup each time
    /// </summary>
    public class UserSettingsFile
    {
        private readonly Dictionary<string, string> _dictionary;
        private PersistStringHelper _helper;
        private FileInfo _settingsFile;

        private static UserSettingsFile _instance;
        private static object lockInstance = new object();

        public static UserSettingsFile GetInstance()
        {
            lock (lockInstance)
            {
                return _instance ?? (_instance = new UserSettingsFile());
            }
        }

        private UserSettingsFile()
        {
            var rdmpDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP");

            //create the directory %appdata%\RDMP\ if it doesn't exist
            if (!Directory.Exists(rdmpDirectory))
                Directory.CreateDirectory(rdmpDirectory);

            _settingsFile = new FileInfo(Path.Combine(rdmpDirectory, "UserSettings.txt"));
            _helper = new PersistStringHelper();

            if (!_settingsFile.Exists)
            {
                using(var fs =  _settingsFile.Create())
                {
                    fs.Flush();
                    fs.Dispose();
                    _dictionary = new Dictionary<string, string>();
                }
            }
            else
                try
                {
                    _dictionary = _helper.LoadDictionaryFromString(File.ReadAllText(_settingsFile.FullName));
                }
                catch (Exception)
                {
                    _dictionary = new Dictionary<string, string>();
                }
        }

        public bool ShowHomeScreenInsteadOfPersistence
        {
            get
            {
                return GetBoolean("ShowHomeScreenInsteadOfPersistence",true);
            }
            set
            {
                SetBoolean("ShowHomeScreenInsteadOfPersistence",value);
            }
        }

        public bool EmphasiseOnTabChanged
        {
            get
            {
                return GetBoolean("EmphasiseOnTabChanged", false);
            }
            set
            {
                SetBoolean("EmphasiseOnTabChanged", value);
            }
        }

        public bool LicenseAccepted
        {
            get
            {
                return GetBoolean("LicenseAccepted", false);
            }
            set
            {
                SetBoolean("LicenseAccepted", value);
            }
        }

        private void SetBoolean(string key, bool value)
        {
            if (!_dictionary.ContainsKey(key))
                _dictionary.Add(key, value.ToString());
            else
                _dictionary[key] = value.ToString();

            SaveAll();
        }

        private void SaveAll()
        {
            lock (lockInstance)
            {
                File.WriteAllText(_settingsFile.FullName,_helper.SaveDictionaryToString(_dictionary));
            }
        }

        private bool GetBoolean(string key, bool defaultIfMissing)
        {
            if (!_dictionary.ContainsKey(key))
                return defaultIfMissing;//default if not value specified
            try
            {
                return Convert.ToBoolean(_dictionary[key]);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Corrupt user setting '" + key + "' (value was '" + _dictionary[key] +"').  User setting will be reset to '" + defaultIfMissing + "'",e);

                SetBoolean(key, defaultIfMissing);

                return defaultIfMissing;
            }
        }
    }
}
