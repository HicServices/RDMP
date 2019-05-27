// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.ObjectModel;

namespace Rdmp.Core.Startup
{
    public class EnvironmentInfo
    {
        /// <summary>
        /// List of RIDs that are allowed for <see cref="RuntimeIdentifier"/>
        /// </summary>
        public static ReadOnlyCollection<string> SupportedRIDs = new ReadOnlyCollection<string>(new string[]
            {
                "linux",
                "win",
                "osx"
            });

        /// <summary>
        /// The target framework of the running application e.g. "netcoreapp2.2", "net461".  This determines which
        /// plugins versions are loaded.  Leave blank to not load any plugins.
        /// </summary>
        public string TargetFramework;

        /// <summary>
        /// The RID of the currently executing environment e.g. "linux", "win".  Does not include 
        /// </summary>
        public string RuntimeIdentifier { get; set; }

        /// <summary>
        /// Creates a new instance specifying your applications build target.  <see cref="RuntimeIdentifier"/> will be guessed
        /// based on the System.Environment.OSVersion.Platform
        /// </summary>
        public EnvironmentInfo(string targetFramework):this()
        {
            TargetFramework = targetFramework;
        }

        /// <summary>
        /// Returns true if all the information is available to make plugin compatibility decisions.
        /// </summary>
        /// <returns></returns>
        public bool IsLegal()
        {
            if(string.IsNullOrWhiteSpace(TargetFramework) || string.IsNullOrWhiteSpace(RuntimeIdentifier))
                return false;

            return SupportedRIDs.Contains(RuntimeIdentifier);
        }

        /// <summary>
        /// Creates a new instance in which plugins are not loaded (no <see cref="TargetFramework"/> is known).
        /// </summary>
        public EnvironmentInfo()
        {
            RuntimeIdentifier = IsLinux ? "linux" : "win";
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }
    }
}