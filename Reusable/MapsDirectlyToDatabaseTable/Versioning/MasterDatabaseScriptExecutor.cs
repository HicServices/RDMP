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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FAnsi;
using FAnsi.Discovery;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using TypeGuesser;
using Version = System.Version;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <summary>
    /// Creates new databases with a fixed (versioned) schema (determined by an <see cref="IPatcher"/>) into a database server (e.g. localhost\sqlexpress).
    /// </summary>
    public class MasterDatabaseScriptExecutor
    {
        public DiscoveredDatabase Database { get; }

        /// <summary>
        /// Returns the name of the schema we expect to create/store the Version / ScriptsRun tables in.  Returns null if
        /// <see cref="Database"/> is not a DBMS that supports schemas (e.g. MySql).
        /// </summary>
        public string RoundhouseSchemaName => GetRoundhouseSchemaName(Database);

        public static string GetRoundhouseSchemaName(DiscoveredDatabase database)
        {
            return database.Server.DatabaseType == DatabaseType.MicrosoftSQLServer ? "RoundhousE" : null;
        }

        public const string RoundhouseVersionTable = "Version";
        public const string RoundhouseScriptsRunTable = "ScriptsRun";

        private const string InitialDatabaseScriptName = @"Initial Database Setup";
        
        public MasterDatabaseScriptExecutor(DiscoveredDatabase database)
        {
            Database = database;
        }
        
        public bool BinaryCollation { get; set; }
        
        public bool CreateDatabase(Patch initialCreationPatch, ICheckNotifier notifier)
        {
            try
            {
                if (Database.Exists())//make sure database does not already exist
                {
                    bool createAnyway = notifier.OnCheckPerformed(new CheckEventArgs($"Database {Database.GetRuntimeName()} already exists", CheckResult.Warning, null,"Attempt to create database inside existing database (will cause problems if the database is not empty)?"));

                    if(!createAnyway)
                        throw new Exception("User chose not continue");
                }
                else
                {
                    if (Database.Server.DatabaseType == DatabaseType.MicrosoftSQLServer && BinaryCollation)
                    {
                        var master = Database.Server.ExpectDatabase("master");
                        using (var con = master.Server.GetConnection())
                        {
                            con.Open();
                            using(var cmd = Database.Server.GetCommand("CREATE DATABASE " + Database + " COLLATE Latin1_General_BIN2", con))
                                cmd.ExecuteNonQuery();
                        }
                    }    
                    else
                        Database.Create();

                    if (!Database.Exists())
                        throw new Exception(
                            "Create database failed without Exception! (It did not Exist after creation)");

                    notifier.OnCheckPerformed(new CheckEventArgs("Database " + Database + " created", CheckResult.Success, null));
                }

                if(Database.Server.DatabaseType == DatabaseType.MicrosoftSQLServer)
                    Database.CreateSchema(RoundhouseSchemaName);

                Database.CreateTable("ScriptsRun",new []
                    {
                        new DatabaseColumnRequest("id",new DatabaseTypeRequest(typeof(int))){IsAutoIncrement = true, IsPrimaryKey = true},
                        new DatabaseColumnRequest("version_id",new DatabaseTypeRequest(typeof(int))),
                        new DatabaseColumnRequest("script_name",new DatabaseTypeRequest(typeof(string),255)),
                        new DatabaseColumnRequest("text_of_script",new DatabaseTypeRequest(typeof(string),int.MaxValue)),
                        new DatabaseColumnRequest("text_hash",new DatabaseTypeRequest(typeof(string),512){Unicode = true}),
                        new DatabaseColumnRequest("one_time_script",new DatabaseTypeRequest(typeof(bool))),
                        new DatabaseColumnRequest("entry_date",new DatabaseTypeRequest(typeof(DateTime))),
                        new DatabaseColumnRequest("modified_date",new DatabaseTypeRequest(typeof(DateTime))),
                        new DatabaseColumnRequest("entered_by",new DatabaseTypeRequest(typeof(string),50))

                    }, RoundhouseSchemaName);

                
                Database.CreateTable("Version", new[]
                    {
                        new DatabaseColumnRequest("id",new DatabaseTypeRequest(typeof(int))){IsAutoIncrement = true, IsPrimaryKey = true},
                        new DatabaseColumnRequest("repository_path",new DatabaseTypeRequest(typeof(string),255){Unicode = true}),
                        new DatabaseColumnRequest("version",new DatabaseTypeRequest(typeof(string),50){Unicode = true}),
                        new DatabaseColumnRequest("entry_date",new DatabaseTypeRequest(typeof(DateTime))),
                        new DatabaseColumnRequest("modified_date",new DatabaseTypeRequest(typeof(DateTime))),
                        new DatabaseColumnRequest("entered_by",new DatabaseTypeRequest(typeof(string),50))

                    }, RoundhouseSchemaName);
                
                
                RunSQL(new KeyValuePair<string, Patch>(InitialDatabaseScriptName, initialCreationPatch));
                
                notifier.OnCheckPerformed(new CheckEventArgs("Tables created", CheckResult.Success, null));

                notifier.OnCheckPerformed(new CheckEventArgs("Setup Completed successfully", CheckResult.Success, null));

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Create failed", CheckResult.Fail, e));
                return false;
            }
        }

        private void RunSQL(KeyValuePair<string,Patch> kvp)
        {
            using (var con = Database.Server.GetConnection())
            {
                con.Open();
                UsefulStuff.ExecuteBatchNonQuery(kvp.Value.GetScriptBody(), con);  
            }
            
            var now = DateTime.Now;

            Database.ExpectTable(RoundhouseScriptsRunTable, RoundhouseSchemaName)
                    .Insert(new Dictionary<string, object>()
                    {
                        {"script_name", kvp.Key},
                        {"text_of_script", kvp.Value.EntireScript},
                        {"text_hash", CalculateHash(kvp.Value.EntireScript)},

                        {"entry_date", now},
                        {"modified_date", now},
                        {"entered_by", Environment.UserName},

                    });

            SetVersion(kvp.Key,kvp.Value.DatabaseVersionNumber.ToString());
            
        }
        
        public string CalculateHash(string input)
        {
            // step 1, calculate MD5 hash from input

            var hashProvider = SHA512.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = hashProvider.ComputeHash(inputBytes);


            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(i.ToString("X2"));

            return sb.ToString();

        }



        private void SetVersion(string name, string version)
        {
            var versionTable = Database.ExpectTable(RoundhouseVersionTable,RoundhouseSchemaName);
            versionTable.Truncate();

            //repository_path	version	entry_date	modified_date	entered_by
            //Patching	2.6.0.1	2018-02-05 08:26:54.000	2018-02-05 08:26:54.000	DUNDEE\TZNind

            var now = DateTime.Now;

            versionTable.Insert(new Dictionary<string, object>()
            {
                {"repository_path", name},
                {"version", version},
                
                {"entry_date", now},
                {"modified_date", now},
                {"entered_by", Environment.UserName}
            });
        }

        public bool PatchDatabase(SortedDictionary<string, Patch> patches, ICheckNotifier notifier, Func<Patch, bool> patchPreviewShouldIRunIt, bool backupDatabase = true)
        {
            if(!patches.Any())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("There are no patches to apply so skipping patching", CheckResult.Success,null));
                return true;
            }

            Version maxPatchVersion = patches.Values.Max(pat => pat.DatabaseVersionNumber);

            if (backupDatabase && Database.Server.DatabaseType == DatabaseType.MicrosoftSQLServer) //todo: Only ms has a backup implementation in FAnsi currently
            {
                try
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("About to backup database", CheckResult.Success, null));

                    Database.CreateBackup("Full backup of " + Database);
            
                    notifier.OnCheckPerformed(new CheckEventArgs("Database backed up", CheckResult.Success, null));
                
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Patching failed during setup and preparation (includes failures due to backup creation failures)",
                        CheckResult.Fail, e));
                    return false;
                }
            }
            

            try
            {
                int i = 0;
                foreach (KeyValuePair<string, Patch> patch in patches)
                {
                    i++;

                    bool shouldRun = patchPreviewShouldIRunIt(patch.Value);

                    if (shouldRun)
                    {

                        try
                        {
                            RunSQL(patch);
                        }
                        catch(Exception e)
                        {
                            throw new Exception($"Failed to apply patch '{ patch.Key }'",e);
                        }
                        

                        notifier.OnCheckPerformed(new CheckEventArgs("Executed patch " + patch.Value, CheckResult.Success, null));
                    }
                    else
                        throw new Exception("User decided not to execute patch " + patch.Key + " - aborting ");
                }
                
                SetVersion("Patching",maxPatchVersion.ToString());
                notifier.OnCheckPerformed(new CheckEventArgs("Updated database version to " + maxPatchVersion.ToString(), CheckResult.Success, null));

                //all went fine
                notifier.OnCheckPerformed(new CheckEventArgs("All patches applied, transaction committed", CheckResult.Success, null));
                
                return true;

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Error occurred during patching", CheckResult.Fail, e));
                return false;
            }
        }

        /// <summary>
        /// Patches the <see cref="Database"/> with ONLY the patches that are outstanding from <paramref name="patcher"/>
        /// </summary>
        /// <param name="patcher"></param>
        /// <param name="notifier"></param>
        /// <param name="patchPreviewShouldIRunIt"></param>
        /// <param name="backupDatabaseFunc"></param>
        /// <returns></returns>
        public bool PatchDatabase(IPatcher patcher, ICheckNotifier notifier, Func<Patch, bool> patchPreviewShouldIRunIt, Func<bool> backupDatabaseFunc)
        {
            var status = Patch.IsPatchingRequired(Database,patcher, out var databaseVersion, out var patchesInDatabase, out var allPatchesInAssembly);

            if(status != Patch.PatchingState.Required)
                return false;

            bool stop = false;
            var hostAssemblyVersion = patcher.GetDbAssembly().GetName().Version;

            //start with the assumption that we will apply all patches
            SortedDictionary<string, Patch> toApply = new SortedDictionary<string, Patch>();

            foreach (Patch potentialInstallable in allPatchesInAssembly.Values.Except(patchesInDatabase))
                toApply.Add(potentialInstallable.locationInAssembly, potentialInstallable);

            try
            {
                //make sure the existing patches in the live database are not freaky phantom patches
                foreach (Patch patch in patchesInDatabase)
                    //if patch is not in database assembly
                    if (!allPatchesInAssembly.Any(a => a.Value.Equals(patch)))
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "The database contains an unexplained patch called " + patch.locationInAssembly +
                            " (it is not in " + patcher.GetDbAssembly().FullName + " ) so how did it get there?", CheckResult.Warning,
                            null));
                    }
                    else if (!allPatchesInAssembly[patch.locationInAssembly].GetScriptBody().Equals(patch.GetScriptBody()))
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "The contents of patch " + patch.locationInAssembly +
                            " are different between live database and the database patching assembly", CheckResult.Warning,
                            null));

                        //do not apply this patch
                        toApply.Remove(patch.locationInAssembly);
                    }
                    else
                    {
                        //we found it and it was intact
                        notifier.OnCheckPerformed(new CheckEventArgs("Patch " + patch.locationInAssembly + " was previously installed successfully so no need to touch it",CheckResult.Success, null));
                    
                        //do not apply this patch
                        toApply.Remove(patch.locationInAssembly);

                    }
            }
            catch (Exception exception)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Patch evaluation failed", CheckResult.Fail, exception));
                stop = true;
            }            

            //if any of the patches we are trying to apply are earlier than the latest in the database
            IEnumerable<Patch> missedOpportunities = toApply.Values.Where(p => p.DatabaseVersionNumber < patchesInDatabase.Max(p2 => p2.DatabaseVersionNumber));
            foreach (Patch missedOpportunity in missedOpportunities)
            {
                stop = true;
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Patch " + missedOpportunity.locationInAssembly +
                    " cannot be applied because its version number is " + missedOpportunity.DatabaseVersionNumber +
                    " but the current database is at version " + databaseVersion
                    + Environment.NewLine
                    + " Contents of patch was:" + Environment.NewLine +missedOpportunity.EntireScript
                    , CheckResult.Fail, null));
            }

            //if the patches to be applied would bring the version number above that of the host Library
            foreach (Patch futurePatch in toApply.Values.Where(patch => patch.DatabaseVersionNumber > hostAssemblyVersion))
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Cannot apply patch "+futurePatch.locationInAssembly+" because its database version number is "+futurePatch.DatabaseVersionNumber+" which is higher than the currently loaded host assembly (" +patcher.GetDbAssembly().FullName+ "). ", CheckResult.Fail, null));
                stop = true;
                
            }

            if (stop)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Abandoning patching process (no patches have been applied) because of one or more previous errors",CheckResult.Fail, null));
                return false;
            }

            //todo: Only MS SQL has a backup implementation in FAnsi currently
            bool backupDatabase = Database.Server.DatabaseType == DatabaseType.MicrosoftSQLServer &&
                            backupDatabaseFunc();

            return PatchDatabase(toApply, notifier, patchPreviewShouldIRunIt, backupDatabase);
        }


        public Patch[] GetPatchesRun()
        { 
            List<Patch> toReturn = new List<Patch>();

            var scriptsRun = Database.ExpectTable(RoundhouseScriptsRunTable, RoundhouseSchemaName);

            var dt = scriptsRun.GetDataTable();

            foreach(DataRow r in dt.Rows)
            {
                string text_of_script = r["text_of_script"] as string;
                string script_name = r["script_name"] as string;

                if (string.IsNullOrWhiteSpace(script_name) ||
                    string.IsNullOrWhiteSpace(text_of_script) ||
                    script_name.Equals(InitialDatabaseScriptName))
                    continue;

                Patch p = new Patch(script_name, text_of_script);
                toReturn.Add(p);
            }

            return toReturn.ToArray();
        }

        /// <summary>
        /// Creates a new platform database and patches it
        /// </summary>
        /// <param name="patcher">Determines the SQL schema created</param>
        /// <param name="notifier">audit object, can be a new ThrowImmediatelyCheckNotifier if you aren't in a position to pass one</param>
        public void CreateAndPatchDatabase(IPatcher patcher, ICheckNotifier notifier)
        {
            var initialPatch = patcher.GetInitialCreateScriptContents(Database);
            CreateDatabase(initialPatch, notifier);

            //get everything in the /up/ folder that is .sql
            var patches = patcher.GetAllPatchesInAssembly(Database);
            PatchDatabase(patches,notifier,(p)=>true);//apply all patches without question
        }
    }
}
