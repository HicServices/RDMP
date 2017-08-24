using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DataTableExtension;
using Tests.Common;

namespace DataLoadEngineTests
{
    public class SimpleFileImportingTests:DatabaseTests
    {
        [Test]
        public void TestUploadingToTempDB()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof (int));
            dt.Columns.Add("Name", typeof(string));

            dt.Rows.Add(new object[] {1, "bob"});
            dt.Rows.Add(new object[] { 2, "fra" });
            dt.TableName = "MyExcitingTempTable";

            DataTableHelper  helper = new DataTableHelper(dt);

            var server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
            helper.CommitDataTableToTempDB(server, true);

            Assert.Contains("MyExcitingTempTable", server.ExpectDatabase("tempdb").DiscoverTables(false).Select(t=>t.GetRuntimeName()).ToArray());
        }
    }
}
