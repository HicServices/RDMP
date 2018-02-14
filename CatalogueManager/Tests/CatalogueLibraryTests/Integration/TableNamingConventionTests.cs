using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    class TableNamingConventionTests : DatabaseTests
    {
        [Test]
        public void GetAllTableInfos_moreThan1_pass()
        {
            var ti = new TableInfo(CatalogueRepository, "AMAGAD!!!");
            Assert.IsTrue(CatalogueRepository.GetAllObjects<TableInfo>().Any());
            ti.DeleteInDatabase();
        }


        [Test]
        public void update_changeAllProperties_pass()
        {
            var tableInfo = new TableInfo(CatalogueRepository, "CHI_AMALG..SearchStuff")
            {
                Database = "CHI_AMALG",
                Server = "Highly restricted",
                Name = "Fishmongery!",
                State = "Totally unstable",
                DatabaseType = DatabaseType.Oracle
            };

            tableInfo.SaveToDatabase();

            var tableInfoAfter = CatalogueRepository.GetObjectByID<TableInfo>(tableInfo.ID);

            Assert.IsTrue(tableInfoAfter.Database == "CHI_AMALG");
            Assert.IsTrue(tableInfoAfter.Server == "Highly restricted");
            Assert.IsTrue(tableInfoAfter.Name == "Fishmongery!");
            Assert.IsTrue(tableInfoAfter.State == "Totally unstable");
            Assert.IsTrue(tableInfoAfter.DatabaseType == DatabaseType.Oracle);

            tableInfoAfter.DeleteInDatabase();
            
        }

        [Test]
        public void SuffixBasedTableNamingConventionHelper()
        {
            const string baseTableName = "MyTable";
            var namingScheme = new SuffixBasedNamer();

            var stagingTable = namingScheme.GetName(baseTableName, LoadBubble.Staging);
            Assert.AreEqual("MyTable_STAGING", stagingTable);

            var newLookupTable = namingScheme.GetName(baseTableName, LoadBubble.Live);
            Assert.AreEqual("MyTable", newLookupTable);
        }

    }
}
