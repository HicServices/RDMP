using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ReusableLibraryCode
{
    public class OfficeVersionFinder
    {
        public enum OfficeComponent
        {
            Word,
            Excel,
            PowerPoint,
            Outlook
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// gets the component's path from the registry. if it can't find it - retuns 
        ///an empty string
        /// <span class="code-SummaryComment"></summary></span>
        public static string GetComponentPath(OfficeComponent _component)
        {
            const string RegKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
            string toReturn = string.Empty;
            string _key = string.Empty;

            switch (_component)
            {
                case OfficeComponent.Word:
                    _key = "winword.exe";
                    break;
                case OfficeComponent.Excel:
                    _key = "excel.exe";
                    break;
                case OfficeComponent.PowerPoint:
                    _key = "powerpnt.exe";
                    break;
                case OfficeComponent.Outlook:
                    _key = "outlook.exe";
                    break;
            }

            //looks inside CURRENT_USER:
            RegistryKey _mainKey = Registry.CurrentUser;
            try
            {
                _mainKey = _mainKey.OpenSubKey(RegKey + "\\" + _key, false);
                if (_mainKey != null)
                {
                    toReturn = _mainKey.GetValue(string.Empty).ToString();
                }
            }
            catch
            { }

            //if not found, looks inside LOCAL_MACHINE:
            _mainKey = Registry.LocalMachine;
            if (string.IsNullOrEmpty(toReturn))
            {
                try
                {
                    _mainKey = _mainKey.OpenSubKey(RegKey + "\\" + _key, false);
                    if (_mainKey != null)
                    {
                        toReturn = _mainKey.GetValue(string.Empty).ToString();
                    }
                }
                catch
                { }
            }

            //closing the handle:
            if (_mainKey != null)
                _mainKey.Close();

            return toReturn;
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Gets the major version of the path. if file not found (or any other        
        /// exception occures - returns 0
        /// <span class="code-SummaryComment"></summary></span>
        public static int GetMajorVersion(OfficeComponent component)
        {
            int toReturn = 0;
            string path = GetComponentPath(component);

            if (File.Exists(path))
            {
                try
                {
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(path);
                    toReturn = fileVersion.FileMajorPart;
                }
                catch
                { }
            }
            return toReturn;
        }

        /// <summary>
        /// Returns the full version of the file.  If not found null returns
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static FileVersionInfo GetVersion(OfficeComponent component)
        {
            string path = GetComponentPath(component);

            if (File.Exists(path))
            {
                try
                {
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(path);
                    return fileVersion;
                }
                catch
                { }
            }
            return null;
        }
    }
}
