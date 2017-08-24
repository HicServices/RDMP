using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace AnonymisationTests
{
    /// <summary>
    /// For any test that requires both the ANOStore and the IdentifierDump and anything else we come up with in terms of anonymisation
    /// </summary>
    public class TestsRequiringFullAnonymisationSuite : TestsRequiringANOStore
    {
        protected SqlConnectionStringBuilder IdentifierDump_ConnectionStringBuilder { get; set; }
        protected string IdentifierDump_DatabaseName = TestDatabaseNames.GetConsistentName("IdentifierDump");
        protected ExternalDatabaseServer IdentifierDump_ExternalDatabaseServer { get; set; }

        [TestFixtureSetUp]
        public void Setup_IdentifierDump()
        {
            IdentifierDump_ConnectionStringBuilder = new SqlConnectionStringBuilder(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString);
            IdentifierDump_ConnectionStringBuilder.InitialCatalog = "";

            CreateIdentifierDumpDatabase();

            CreateReferenceInCatalogueToIdentifierDump();
        }

        [TestFixtureTearDown]
        public override void FixtureTearDown()
        {
            RemovePreExistingReferenceToIdentifierDump();

            var database = new DiscoveredServer(IdentifierDump_ConnectionStringBuilder).ExpectDatabase(IdentifierDump_DatabaseName);
            if (database != null && database.Exists())
                database.ForceDrop();

            base.FixtureTearDown();
        }

        private void CreateReferenceInCatalogueToIdentifierDump()
        {
            RemovePreExistingReferenceToIdentifierDump();

            //now create a new reference!
            IdentifierDump_ExternalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, IdentifierDump_DatabaseName)
            {
                Database = IdentifierDump_ConnectionStringBuilder.InitialCatalog,
                Server = IdentifierDump_ConnectionStringBuilder.DataSource,
                Username = IdentifierDump_ConnectionStringBuilder.UserID,
                Password = IdentifierDump_ConnectionStringBuilder.Password
            };

            IdentifierDump_ExternalDatabaseServer.SaveToDatabase();
        }

        private void RemovePreExistingReferenceToIdentifierDump()
        {
            //There will likely be an old reference to the external database server
            var preExisting = CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().SingleOrDefault(e => e.Name.Equals(IdentifierDump_DatabaseName));

            if (preExisting == null) return;

            //Some child tests will likely create ANOTables that reference this server so we need to cleanup those for them so that we can cleanup the old server reference too
            foreach (var lingeringTableInfo in CatalogueRepository.GetAllObjects<TableInfo>().Where(t => t.IdentifierDumpServer_ID == preExisting.ID))
            {
                foreach (PreLoadDiscardedColumn lingeringPreLoadDiscardedColumn in lingeringTableInfo.PreLoadDiscardedColumns)
                    lingeringPreLoadDiscardedColumn.DeleteInDatabase();

                lingeringTableInfo.DeleteInDatabase();
            }

            //now delete the old server reference
            preExisting.DeleteInDatabase();
        }


        private void CreateIdentifierDumpDatabase()
        {
            var database = new DiscoveredServer(IdentifierDump_ConnectionStringBuilder).ExpectDatabase(IdentifierDump_DatabaseName);
            if (database != null && database.Exists())
                database.ForceDrop();

            var scriptCreate = new MasterDatabaseScriptExecutor(IdentifierDump_ConnectionStringBuilder.DataSource, IdentifierDump_DatabaseName, IdentifierDump_ConnectionStringBuilder.UserID, IdentifierDump_ConnectionStringBuilder.Password);
            scriptCreate.CreateAndPatchDatabase(typeof(IdentifierDump.Class1).Assembly, new ThrowImmediatelyCheckNotifier());
            IdentifierDump_ConnectionStringBuilder.InitialCatalog = IdentifierDump_DatabaseName;
        }
    }
}