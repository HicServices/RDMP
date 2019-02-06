// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
    /// <summary>
    /// Determines whether a given Microsoft Office component (e.g. Microsoft Word) is installed on the current computer.
    /// </summary>
    public class OfficeVersionFinder
    {
        public enum OfficeComponent
        {
            Word,
            Excel,
            PowerPoint,
            Outlook
        }

        /// <summary>
        /// gets the component's path from the registry. if it can't find it - retuns 
        /// an empty string
        /// </summary>
        /// <param name="_component"></param>
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

        /// <summary>
        /// Gets the major version of the path. if file not found (or any other        
        /// exception occures - returns 0
        /// </summary>
        /// <param name="component"></param>
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
