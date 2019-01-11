using System;
using DataLoadEngine.Job;
using DataLoadEngine.Migration;
using DataLoadEngine.Migration.QueryBuilding;
using FAnsi.Connections;
using FAnsi.Discovery;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class MigrationStrategyTests : DatabaseTests
    {
        [Test]
        public void OverwriteMigrationStrategy_NoPrimaryKey()
        {
            var from = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("Bob",new[] {new DatabaseColumnRequest("Field", "int")});
            var to = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("Frank", new[] { new DatabaseColumnRequest("Field", "int") });

            var connection = MockRepository.GenerateStub<IManagedConnection>();
            var job = MockRepository.GenerateStub<IDataLoadJob>();
            var strategy = new OverwriteMigrationStrategy(connection);

            var migrationFieldProcessor = MockRepository.GenerateStub<IMigrationFieldProcessor>();

            var ex = Assert.Throws<Exception>(() => new MigrationColumnSet(from, to, migrationFieldProcessor));
            Assert.AreEqual("There are no primary keys declared in table Bob", ex.Message);
        }
    }

}
