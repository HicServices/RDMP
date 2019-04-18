// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Allows switching app.config file at runtime, either permanently or 
    /// temporarily with a 'using' statement
    /// </summary>
    public abstract class AppConfig : IDisposable
    {
        /// <summary>
        /// Changes the app.config with the one specified
        /// </summary>
        /// <param name="path">full path to the new config file</param>
        /// <returns></returns>
        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public abstract void Dispose();

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();

                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                var fieldInfo = typeof(ConfigurationManager)
                                    .GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
                if (fieldInfo != null)
                    fieldInfo.SetValue(null, 0);

                fieldInfo = typeof(ConfigurationManager)
                                .GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static);
                if (fieldInfo != null)
                    fieldInfo.SetValue(null, null);

                fieldInfo = typeof(ConfigurationManager)
                                .Assembly
                                .GetTypes().First(x => x.FullName == "System.Configuration.ClientConfigPaths")
                                .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static);
                if (fieldInfo != null)
                    fieldInfo.SetValue(null, null);
            }
        }
    }
}