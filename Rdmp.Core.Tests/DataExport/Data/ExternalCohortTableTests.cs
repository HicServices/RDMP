using System;
using System.Collections.Generic;
using System.Text;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.Data
{
    class ExternalCohortTableTests:UnitTests
    {
        /// <summary>
        /// Demonstrates the minimum properties required to create a <see cref="ExternalCohortTable"/>.  See <see cref="CreateNewCohortDatabaseWizard"/>
        /// for how to create one of these based on the datasets currently held in rdmp.
        /// </summary>
        [Test]
        public void Create_ExternalCohortTable_Manually()
        {
            MemoryDataExportRepository repository = new MemoryDataExportRepository();
            var table = new ExternalCohortTable(repository, "My Cohort Database", DatabaseType.MicrosoftSQLServer);
            table.Database = "mydb";
            table.PrivateIdentifierField = "chi";
            table.ReleaseIdentifierField = "release";
            table.DefinitionTableForeignKeyField = "c_id";
            table.TableName = "Cohorts";
            table.DefinitionTableName = "InventoryTable";
            table.Server = "superfastdatabaseserver\\sqlexpress";
            table.SaveToDatabase();

            var ex = Assert.Throws<Exception>(()=>table.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Could not connect to Cohort database called 'My Cohort Database'",ex.Message);
        }

        /// <summary>
        /// Demonstrates how to get a hydrated instance during unit tests.  This will not map to an actually existing database
        /// </summary>
        [Test]
        public void Create_ExternalCohortTable_InTests()
        {
            var tbl = WhenIHaveA<ExternalCohortTable>();
            
            Assert.IsNotNull(tbl);
            Assert.IsNotNull(tbl.PrivateIdentifierField);
            Assert.IsNotNull(tbl.ReleaseIdentifierField);
        }
    }
}
