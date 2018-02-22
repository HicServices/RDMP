using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    class TableInfoTests : DatabaseTests
    {
        [Test]
        public void GetAllTableInfos_moreThan1_pass()
        {
            var tableInfo = new TableInfo(CatalogueRepository, "AMAGAD!!!");
            Assert.IsTrue(CatalogueRepository.GetAllObjects<TableInfo>().Any());
            tableInfo.DeleteInDatabase();
        }

        [Test]
        public void CreateNewTableInfoInDatabase_valid_pass()
        {
            TableInfo table = new TableInfo(CatalogueRepository, "TestDB..TestTableName");

            Assert.NotNull(table);

            table.DeleteInDatabase();

            var ex = Assert.Throws<KeyNotFoundException>(() => CatalogueRepository.GetObjectByID<TableInfo>(table.ID));
            Assert.AreEqual(ex.Message, "Could not find TableInfo with ID " + table.ID);
        }

        [Test]
        public void update_changeAllProperties_pass()
        {
            TableInfo table = new TableInfo(CatalogueRepository, "CHI_AMALG..SearchStuff")
            {
                Database = "CHI_AMALG",
                Server = "Highly restricted",
                Name = "Fishmongery!",
                State = "Totally unstable",
                DatabaseType = DatabaseType.Oracle
            };

            table.SaveToDatabase();

            TableInfo tableAfter = CatalogueRepository.GetObjectByID<TableInfo>(table.ID);

            Assert.IsTrue(tableAfter.Database == "CHI_AMALG");
            Assert.IsTrue(tableAfter.Server == "Highly restricted");
            Assert.IsTrue(tableAfter.Name == "Fishmongery!");
            Assert.IsTrue(tableAfter.State == "Totally unstable");
            Assert.IsTrue(tableAfter.DatabaseType == DatabaseType.Oracle);

            tableAfter.DeleteInDatabase();
        }



        [Test]
        [TestCase("[TestDB]..[TestTableName]", "[TestDB]..[TestTableName].[ANOMyCol]")]
        [TestCase("TestDB..TestTableName", "TestDB..TestTableName.ANOMyCol")]
        public void CreateNewTableInfoInDatabase_Naming(string tableName, string columnName)
        {
            TableInfo table = new TableInfo(CatalogueRepository, tableName);
            table.Database = "TestDB";
            table.SaveToDatabase();

            ColumnInfo c = new ColumnInfo(CatalogueRepository, columnName, "varchar(100)", table);
            c.ANOTable_ID = -100;
            
            try
            {
                Assert.AreEqual("ANOMyCol",c.GetRuntimeName());
                Assert.AreEqual("MyCol", c.GetRuntimeName(LoadStage.AdjustRaw));
                Assert.AreEqual("ANOMyCol", c.GetRuntimeName(LoadStage.PostLoad));

                Assert.AreEqual("TestTableName", table.GetRuntimeName());
                Assert.AreEqual("TestTableName", table.GetRuntimeName(LoadBubble.Raw));
                Assert.AreEqual("TestDB_TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging));

                Assert.AreEqual("TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging, new SuffixBasedNamer()));
                Assert.AreEqual("TestDB_TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging, new FixedStagingDatabaseNamer("TestDB")));

                Assert.AreEqual("TestTableName", table.GetRuntimeName(LoadBubble.Live));

            }
            finally 
            {
                c.DeleteInDatabase();
                table.DeleteInDatabase();
            }
        }
    }
}
