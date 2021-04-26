using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandAlterTableMakeDistinctTests : DatabaseTests
    {

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.PostgreSql)]
        public void Test(DatabaseType dbType)
        {
            var db = GetCleanedServer(dbType);

            var dt = new DataTable();
            dt.Columns.Add("fff");
            dt.Rows.Add("1");
            dt.Rows.Add("1");
            dt.Rows.Add("2");
            dt.Rows.Add("2");
            dt.Rows.Add("2");

            var tbl = db.CreateTable("MyTable", dt);

            Import(tbl, out ITableInfo tblInfo,out _);

            Assert.AreEqual(5, tbl.GetRowCount());

            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            var cmd = new ExecuteCommandAlterTableMakeDistinct(activator, tblInfo, 700, true);

            Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);

            cmd.Execute();

            Assert.AreEqual(2, tbl.GetRowCount());
            
            tbl.CreatePrimaryKey(tbl.DiscoverColumn("fff"));

            cmd = new ExecuteCommandAlterTableMakeDistinct(activator, tblInfo, 700, true);

            var ex = Assert.Throws<Exception>(()=>cmd.Execute());

            Assert.AreEqual("Table 'MyTable' has primary key columns so cannot contain duplication", ex.Message);
        }
    }
}
