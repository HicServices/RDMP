// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FAnsi;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <inheritdoc/>
    public abstract class Patcher:IPatcher
    {
        /// <inheritdoc/>
        public virtual Assembly GetDbAssembly()
        {
            return GetType().Assembly;
        }

        /// <inheritdoc/>
        public string ResourceSubdirectory { get; private set; }

        /// <inheritdoc/>
        public int Tier { get; }

        public string Name => GetDbAssembly().GetName().Name + (string.IsNullOrEmpty(ResourceSubdirectory) ? "" : "/" + ResourceSubdirectory);
        public string LegacyName { get; protected set; }

        protected Patcher(int tier,string resourceSubdirectory)
        {
            Tier = tier;
            ResourceSubdirectory = resourceSubdirectory;
        }
        
        /// <inheritdoc/>
        public SortedDictionary<string, Patch> GetAllPatchesInAssembly(DatabaseType dbType)
        {
            var assembly = GetDbAssembly();
            var subdirectory = ResourceSubdirectory;
            Regex upgradePatchesRegexPattern;

            if (string.IsNullOrWhiteSpace(subdirectory))
                upgradePatchesRegexPattern = new Regex(@".*\.up\.(.*\.sql)");
            else
                upgradePatchesRegexPattern = new Regex(@".*\." + Regex.Escape(subdirectory) + @"\.up\.(.*\.sql)");

            var files = new SortedDictionary<string, Patch>();

            //get all resources out of 
            foreach (string manifestResourceName in assembly.GetManifestResourceNames())
            {
                var match = upgradePatchesRegexPattern.Match(manifestResourceName);
                if (match.Success)
                {
                    string fileContents = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName)).ReadToEnd();
                    files.Add(match.Groups[1].Value, new Patch(match.Groups[1].Value, fileContents));
                }
            }

            return files;
        }
    }

}