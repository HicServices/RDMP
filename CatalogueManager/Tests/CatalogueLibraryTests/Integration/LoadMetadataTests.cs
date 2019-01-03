using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibraryTests.Mocks;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class LoadMetadataTests : DatabaseTests
    {
        [Test]
        public void CreateNewAndGetBackFromDatabase()
        {
            var loadMetadata = new LoadMetadata(CatalogueRepository);

            try
            {
                loadMetadata.LocationOfFlatFiles = "C:\\temp";
                loadMetadata.SaveToDatabase();
                
                var loadMetadataWithIdAfterwards = CatalogueRepository.GetObjectByID<LoadMetadata>(loadMetadata.ID);
                Assert.AreEqual(loadMetadataWithIdAfterwards.LocationOfFlatFiles, "C:\\temp");
            }
            finally
            {
                loadMetadata.DeleteInDatabase();
            }
        }

        [Test]
        public void TestPreExecutionChecker_TablesDontExist()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("Imaginary");

            Assert.IsFalse(tbl.Exists());

            var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl);
            var checker = new PreExecutionChecker(lmd, new HICDatabaseConfiguration(DiscoveredDatabaseICanCreateRandomTablesIn.Server));
            var ex = Assert.Throws<Exception>(()=>checker.Check(new ThrowImmediatelyCheckNotifier()));

            StringAssert.IsMatch("Table '.*Imaginary.*' does not exist", ex.Message);
        }
     }
}
