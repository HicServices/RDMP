using System;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Migration;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class MigrationStrategyTests : DatabaseTests
    {
        [Test]
        public void OverwriteMigrationStrategy_NoPrimaryKey()
        {
            var databaseName = DiscoveredDatabaseICanCreateRandomTablesIn.GetRuntimeName();
            var connection = MockRepository.GenerateStub<IManagedConnection>();
            var logManager = MockRepository.GenerateStub<ILogManager>();
            var strategy = new OverwriteMigrationStrategy(databaseName, connection);

            var sourceFields = new[] {"Field"};
            var destinationFields = new[] { "Field" };
            var column = MockRepository.GenerateStub<IColumnMetadata>();
            var migrationFieldProcessor = MockRepository.GenerateStub<IMigrationFieldProcessor>();
            
            var columnsToMigrate = new MigrationColumnSet(databaseName, "SourceTable", "DestinationTable", sourceFields, destinationFields, new [] {column}, migrationFieldProcessor);
            var cts = new GracefulCancellationTokenSource();
            var inserts = 0;
            var updates = 0;

            var ex = Assert.Throws<InvalidOperationException>(() => strategy.MigrateTable(columnsToMigrate, logManager, 1, cts.Token, ref inserts, ref updates));
            Assert.IsTrue(ex.InnerException.Message.Contains("None of the columns to be migrated are configured as a Primary Key"));
        }
    }

}
