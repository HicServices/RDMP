// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Diagnostics.TestData;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests
{
    public class PipelineReadPerformanceTest:DatabaseTests
    {
        private BulkTestsData _bulkTestData;
        
        [OneTimeSetUp]
        public void SetupBulkTestData()
        {
            _bulkTestData = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            _bulkTestData.SetupTestData();

        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            _bulkTestData.Destroy();
        }

        [Test]
        public void BulkTestDataContainsExpectedNumberOfRows()
        {
            var server = _bulkTestData.BulkDataDatabase.Server;

            using (DbConnection con = server.GetConnection())
            {
                con.Open();
                DbCommand cmd = server.GetCommand("Select count(*) from " + BulkTestsData.BulkDataTable, con);
                int manualCount = Convert.ToInt32(cmd.ExecuteScalar());

                //manual count matches expected
                Assert.AreEqual(_bulkTestData.ExpectedNumberOfRowsInTestData,manualCount);

                //now get the fast approximate rowcount
                int fastRowcount = _bulkTestData.BulkDataDatabase
                    .ExpectTable(BulkTestsData.BulkDataTable)
                    .GetRowCount();

                //it should also match
                Assert.AreEqual(_bulkTestData.ExpectedNumberOfRowsInTestData,fastRowcount);
            }
        }

        [Test]
        [TestCase(BulkTestsData.SlowView)]
        [TestCase(BulkTestsData.BulkDataTable)]
        public void SimpleDataAdapterFill(string tableName)
        {
            GC.Collect();

            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("Memory allocated at start:"+ Process.GetCurrentProcess().PagedMemorySize64 /(1024*1024) + " MB");

            DataTable dt = null;
            var server = _bulkTestData.BulkDataDatabase.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                
                dt = new DataTable();
                var da = server.GetDataAdapter("select * from " + tableName, con);
                da.Fill(dt);

                con.Close();
            }
            
            sw.Stop();
            Console.WriteLine("Time taken:"+sw.Elapsed);
            Console.WriteLine("Memory allocated at end:" + Process.GetCurrentProcess().PagedMemorySize64 / (1024 * 1024) + " MB");

            if (tableName == BulkTestsData.SlowView)
                Assert.Greater(dt.Rows.Count,0);
            else
                Assert.AreEqual(_bulkTestData.ExpectedNumberOfRowsInTestData, dt.Rows.Count);
        }

        [Test]
        [TestCase(BulkTestsData.SlowView)]
        [TestCase(BulkTestsData.BulkDataTable)]
        public void SqlDataReader(string tableName)
        {
            GC.Collect();

            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("Memory allocated at start:" + Process.GetCurrentProcess().PagedMemorySize64 / (1024 * 1024) + " MB");

            int linesRead = 0;

            DbDataCommandDataFlowSource source = new DbDataCommandDataFlowSource("select * from " + tableName, "ignoreme", _bulkTestData.BulkDataDatabase.Server.Builder, 30);
                
            DataTable chunk;

            //read all chunks
            while ((chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken())) != null)
            {
                linesRead += chunk.Rows.Count;
                chunk.Dispose();
            }
                
            sw.Stop();
            Console.WriteLine("Time taken:" + sw.Elapsed);
            Console.WriteLine("Memory allocated at end:" + Process.GetCurrentProcess().PagedMemorySize64 / (1024 * 1024) + " MB");

            if (tableName == BulkTestsData.SlowView)
                Assert.Greater(linesRead, 0);
            else
                Assert.AreEqual(_bulkTestData.ExpectedNumberOfRowsInTestData, linesRead);
        }
    }
}
