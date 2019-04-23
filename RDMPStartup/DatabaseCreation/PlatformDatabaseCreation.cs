using System;
using System.Data.SqlClient;
using System.Reflection;
using CatalogueLibrary.Repositories;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using ReusableLibraryCode.Checks;

namespace RDMPStartup.DatabaseCreation
{
    public class PlatformDatabaseCreation
    {
        public const string DefaultCatalogueDatabaseName = "Catalogue";
        public const string DefaultDataExportDatabaseName = "DataExport";
        public const string DefaultDQEDatabaseName = "DQE";
        public const string DefaultLoggingDatabaseName = "Logging";

        public void CreatePlatformDatabases(PlatformDatabaseCreationOptions options)
        {
            Create(DefaultCatalogueDatabaseName, typeof(CatalogueLibrary.Database.Class1).Assembly, options);
            Create(DefaultDataExportDatabaseName, typeof(DataExportLibrary.Database.Class1).Assembly, options);

            var dqe = Create(DefaultDQEDatabaseName, typeof(DataQualityEngine.Database.Class1).Assembly, options);
            var logging = Create(DefaultLoggingDatabaseName, typeof(HIC.Logging.Database.Class1).Assembly, options);

            CatalogueRepository.SuppressHelpLoading = true;

            if (!options.SkipPipelines)
            {
                var creator = new CataloguePipelinesAndReferencesCreation(new PlatformDatabaseCreationRepositoryFinder(options), logging, dqe);
                creator.Create();
            }
        }

        private SqlConnectionStringBuilder Create(string databaseName, Assembly dotDatabaseAssembly, PlatformDatabaseCreationOptions options)
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
            executor.CreateAndPatchDatabaseWithDotDatabaseAssembly(dotDatabaseAssembly,new AcceptAllCheckNotifier());
            Console.WriteLine("Created " + builder.InitialCatalog + " on server " + builder.DataSource);
            
            return builder;
        }
    }
}