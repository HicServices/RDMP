using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.DatabaseManagement
{
    class TableInfoCloneOperationTests : DatabaseTests
    {
        [Test]
        public void Test_CloneTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("FF");
            
            var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
            var tbl = db.CreateTable("MyTable",dt);

            Import(tbl,out var ti, out _);
            
            var config = new HICDatabaseConfiguration(tbl.Database.Server);

            //create a RAW table schema called TableName_Isolation
            var cloner = new TableInfoCloneOperation(config,(TableInfo)ti,LoadBubble.Live,new ThrowImmediatelyDataLoadEventListener());
            cloner.CloneTable(tbl.Database, tbl.Database,tbl, tbl.GetRuntimeName() + "_copy", true, true, true, ti.PreLoadDiscardedColumns);
             
            var tbl2 = tbl.Database.ExpectTable(tbl.GetRuntimeName() + "_copy");

            Assert.IsTrue(tbl2.Exists());
        }
    }
}
