// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Database;
using CatalogueLibrary.Repositories;
using CommandLine;
using FAnsi.Discovery;
using Fansi.Implementations.MicrosoftSQL;
using MapsDirectlyToDatabaseTable.Versioning;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace DatabaseCreation
{
    /// <summary>
    /// Creates the minimum set of databases required to get RDMP working (Catalogue and Data Export databases) with an optional prefix.  Also creates satellite
    /// Tier 2 databases (logging / dqe)
    /// </summary>
    public class DatabaseCreationProgram
    {
        public const string DefaultCatalogueDatabaseName = "Catalogue";
        public const string DefaultDataExportDatabaseName = "DataExport";
        public const string DefaultDQEDatabaseName = "DQE";
        public const string DefaultLoggingDatabaseName = "Logging";

        public static int Main(string[] args)
        {
            return UsefulStuff.GetParser().ParseArguments<DatabaseCreationProgramOptions>(args).MapResult(RunOptionsAndReturnExitCode, errs => 1);
        }

        public static int RunOptionsAndReturnExitCode(DatabaseCreationProgramOptions options)
        {
            
            var serverName = options.ServerName;
            var prefix = options.Prefix;

            Console.WriteLine("About to create on server '" + serverName + "' databases with prefix '" + prefix + "'");

            try
            {
                Create(DefaultCatalogueDatabaseName, typeof(Class1).Assembly, options);
                Create(DefaultDataExportDatabaseName, typeof(DataExportLibrary.Database.Class1).Assembly, options);

                var dqe = Create(DefaultDQEDatabaseName, typeof(DataQualityEngine.Database.Class1).Assembly, options);
                var logging = Create(DefaultLoggingDatabaseName, typeof(HIC.Logging.Database.Class1).Assembly, options);

                CatalogueRepository.SuppressHelpLoading = true;

                if (!options.SkipPipelines)
                {
                    var creator = new CataloguePipelinesAndReferencesCreation(new DatabaseCreationRepositoryFinder(options), logging, dqe);
                    creator.Create();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            return 0;
        }

        private static SqlConnectionStringBuilder Create(string databaseName, Assembly assembly, DatabaseCreationProgramOptions options)
        {
            SqlConnection.ClearAllPools();

            var builder = options.GetBuilder(databaseName);

            DiscoveredDatabase db = new DiscoveredServer(builder).ExpectDatabase(builder.InitialCatalog);

            if (options.DropDatabases && db.Exists())
            {
                Console.WriteLine("Dropping Database:" + builder.InitialCatalog);
                db.Drop();
            }
            
            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(builder.ConnectionString);
            executor.BinaryCollation = options.BinaryCollation;
            executor.CreateAndPatchDatabaseWithDotDatabaseAssembly(assembly,new AcceptAllCheckNotifier());
            Console.WriteLine("Created " + builder.InitialCatalog + " on server " + builder.DataSource);
            
            return builder;
        }

    }
}
