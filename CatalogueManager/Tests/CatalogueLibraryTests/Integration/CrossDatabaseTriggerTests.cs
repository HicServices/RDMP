using CatalogueLibrary.Triggers;
using CatalogueLibrary.Triggers.Implementations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CrossDatabaseTriggerTests : DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void NoTriggerExists(DatabaseType type)
        {
            var db = GetCleanedServer(type, "CrossDatabaseTriggerTests");
            var tbl = db.CreateTable("MyTable", new[]
            {
                new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof (string), 30)),
                new DatabaseColumnRequest("bubbles", new DatabaseTypeRequest(typeof (int)))
            });

            var factory = new TriggerImplementerFactory(type);
            var implementer = factory.Create(tbl);
            Assert.AreEqual(TriggerStatus.Missing,implementer.GetTriggerStatus());
        }
    }
}