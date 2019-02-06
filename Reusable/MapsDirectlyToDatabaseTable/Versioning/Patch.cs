// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <summary>
    /// Represents a single file in the up directory of a .Database assembly (e.g. CatalogueLibrary.Database).  Used during patching to ensure that the live 
    /// database that is about to be patched is in the expected state and ready for new patches to be applied.
    /// </summary>
    public class Patch : IComparable
    {
        public const string InitialCreateScriptRegexPattern = @".*\.runAfterCreateDatabase\..*\.sql";
        public const string UpgradePatchesRegexPattern = @".*\.up\.(.*\.sql)";

        public const string VersionKey = "--Version:";
        public const string DescriptionKey = "--Description:";
        
        public string EntireScript { get; set; }
        public string locationInAssembly { get; private set; }

        public Version DatabaseVersionNumber { get; set; }
        public string Description { get; set; }
        

        public Patch(string scriptName, string entireScriptContents)
        {
            locationInAssembly = scriptName;
            EntireScript = entireScriptContents;

            ExtractDescriptionAndVersionFromScriptContents();
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Description))
                return "Patch " + DatabaseVersionNumber;

            if(Description.Length> 100)
                return "Patch " + DatabaseVersionNumber + "("+Description.Substring(0,100)+"...)";

            return "Patch " + DatabaseVersionNumber + "(" + Description+")";

        }

        private void ExtractDescriptionAndVersionFromScriptContents()
        {
            var lines = EntireScript.Split(new []{'\r', '\n'},StringSplitOptions.RemoveEmptyEntries);

            if(!lines[0].StartsWith(VersionKey))
                throw new InvalidPatchException(locationInAssembly,"Script does not start with " + VersionKey);

            string versionNumber = lines[0].Substring(VersionKey.Length);

            try
            {
                DatabaseVersionNumber = new Version(versionNumber);
            }
            catch (Exception e)
            {
                throw new InvalidPatchException(locationInAssembly,"Could not process the scripts --Version: entry ('"+versionNumber +"') into a valid C# Version object",e);

            }

            if(!lines[1].StartsWith(DescriptionKey))
                throw new InvalidPatchException(locationInAssembly,"Second line of patch scripts must start with " + DescriptionKey);

            string description = lines[1].Substring(DescriptionKey.Length);
            Description = description;
        }


        public static string GetInitialCreateScriptContents(Assembly assembly)
        {
            var candidates = assembly.GetManifestResourceNames().Where(r => Regex.IsMatch(r, InitialCreateScriptRegexPattern)).ToArray();

            if (candidates.Length == 1)
            {
                var sr = new StreamReader(assembly.GetManifestResourceStream(candidates[0]));
                return sr.ReadToEnd();
            }

            if(candidates.Length == 0)
                throw new FileNotFoundException("Could not find an initial create database script in dll "+assembly.FullName + ".  Make sure it is marked as an Embedded Resource and that it is in a folder called 'runAfterCreateDatabase' (and matches regex "+InitialCreateScriptRegexPattern +"). And make sure that it is marked as 'Embedded Resource' in the .csproj build action");

            throw new Exception("There are too many create scripts in the assembly " + assembly.FullName + " only 1 create database script is allowed, all other scripts must go into the up folder");

        }

        public static SortedDictionary<string, Patch> GetAllPatchesInAssembly(Assembly assembly)
        {
            
            var files = new SortedDictionary<string, Patch>();
            
            //get all resources out of 
            foreach (string manifestResourceName in assembly.GetManifestResourceNames())
            {
                var match = Regex.Match(manifestResourceName, UpgradePatchesRegexPattern);
                if (match.Success)
                {
                    string fileContents = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName)).ReadToEnd();

                    files.Add(match.Groups[1].Value, new Patch(match.Groups[1].Value, fileContents));
                }
            }

            return files;
        }

        public override int GetHashCode()
        {
            return locationInAssembly.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var y = obj as Patch;
            var x = this;

            if (y == null)
                return false;

            bool equal = x.locationInAssembly.Equals(y.locationInAssembly);


            if (!equal)
                return false;

            if (x.DatabaseVersionNumber.Equals(y.DatabaseVersionNumber))
                return true;
            else
                throw new InvalidPatchException(x.locationInAssembly,
                    "Patches x and y are being compared and they have the same location in assembly (" +
                    x.locationInAssembly + ")  but different Verison numbers", null);
        }
        public int CompareTo(object obj)
        {
            if (obj is Patch)
            {
                return -System.String.Compare(((Patch)obj).locationInAssembly, locationInAssembly, System.StringComparison.Ordinal); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }

        public static bool IsPatchingRequired(SqlConnectionStringBuilder builder, Assembly databaseAssembly, Assembly hostAssembly, out Version databaseVersion, out Patch[] patchesInDatabase, out SortedDictionary<string, Patch> allPatchesInAssembly)
        {
            databaseVersion = DatabaseVersionProvider.GetVersionFromDatabase(builder);

            MasterDatabaseScriptExecutor scriptExecutor = new MasterDatabaseScriptExecutor(builder.DataSource, builder.InitialCatalog, builder.UserID, builder.Password);
            patchesInDatabase = scriptExecutor.GetPatchesRun();

            allPatchesInAssembly = GetAllPatchesInAssembly(databaseAssembly);

            AssemblyName databaseAssemblyName = databaseAssembly.GetName();
            
            if (databaseAssemblyName.Version < databaseVersion)
                throw new VersionNotFoundException("The software library " + databaseAssemblyName.Name + " is version " + databaseAssemblyName.Version + " but the database is version " + databaseVersion + " which means your software is out of date and behind the current version of the database.  Most likely you have a new version of one software app in the suite e.g. CatalogueManager which has updated the database with a new patch and you are now trying to run an old version of DataExportManager or Dashboard or an older version of CatalogueManager or something? Your software is likely to be VERY unstable and it is strongly advised that you update the correct version of the software for interacting with this database");

            // a.Union(b).Except(a.Intersect(b));
            //if there is a patch mismatch
            return
                patchesInDatabase.Union(allPatchesInAssembly.Values)
                    .Except(patchesInDatabase.Intersect(allPatchesInAssembly.Values))
                    .Any();
        }
    }
}