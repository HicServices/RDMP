using System;
using System.Data.SqlClient;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.Core.Databases;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.DatabaseCreation
{
    public class PlatformDatabaseCreation
    {
        public const string DefaultCatalogueDatabaseName = "Catalogue";
        public const string DefaultDataExportDatabaseName = "DataExport";
        public const string DefaultDQEDatabaseName = "DQE";
        public const string DefaultLoggingDatabaseName = "Logging";

        public void CreatePlatformDatabases(PlatformDatabaseCreationOptions options)
        {
            
            Create(DefaultCatalogueDatabaseName, new CataloguePatcher(), options);
            Create(DefaultDataExportDatabaseName, new DataExportPatcher(), options);

            var dqe = Create(DefaultDQEDatabaseName, new DataQualityEnginePatcher(), options);
            var logging = Create(DefaultLoggingDatabaseName, new LoggingDatabasePatcher(), options);

            CatalogueRepository.SuppressHelpLoading = true;

            if (!options.SkipPipelines)
            {
                var creator = new CataloguePipelinesAndReferencesCreation(new PlatformDatabaseCreationRepositoryFinder(options), logging, dqe);
                creator.Create();
            }
        }

        private SqlConnectionStringBuilder Create(string databaseName, IPatcher patcher, PlatformDatabaseCreationOptions options)
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
            executor.CreateAndPatchDatabase(patcher,new AcceptAllCheckNotifier());
            Console.WriteLine("Created " + builder.InitialCatalog + " on server " + builder.DataSource);
            
            return builder;
        }
    }
}