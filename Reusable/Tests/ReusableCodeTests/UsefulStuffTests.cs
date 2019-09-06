// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using NUnit.Framework;
using Tests.Common;
using Tests.Common.Scenarios;

namespace ReusableCodeTests
{
    public class UsefulStuffTests:DatabaseTests
    {
        private BulkTestsData _bulkTests;
        private DiscoveredDatabase _db;


        [SetUp]
        public void BulkTestsSetUp()
        {
            _db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
            _bulkTests = new BulkTestsData(CatalogueRepository, _db);
            _bulkTests.SetupTestData();
        }

        [TearDown]
        public void BulkTestsTearDown()
        {
            _bulkTests.Destroy();
        }
        
        [Test]
        public void GetRowCountWhenNoIndexes()
        {
            var table = _db.ExpectTable("GetRowCountWhenNoIndexes");
            Assert.AreEqual("GetRowCountWhenNoIndexes",table.GetRuntimeName());
            var server = table.Database.Server;

            using (var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand("CREATE TABLE " + table .GetRuntimeName()+ " (age int, name varchar(5))", con);
                cmd.ExecuteNonQuery();

                var cmdInsert = server.GetCommand("INSERT INTO " + table.GetRuntimeName() + " VALUES (1,'Fish')", con);
                Assert.AreEqual(1,cmdInsert.ExecuteNonQuery());

                Assert.AreEqual(1,table.GetRowCount());
                
            }
        }

        [Test]
        public void GetRowCount_Views()
        {
            
            using (var con = _db.Server.GetConnection())
            {
                con.Open();

                var cmd = _db.Server.GetCommand("CREATE TABLE GetRowCount_Views (age int, name varchar(5))", con);
                cmd.ExecuteNonQuery();

                var cmdInsert = _db.Server.GetCommand("INSERT INTO GetRowCount_Views VALUES (1,'Fish')", con);
                Assert.AreEqual(1, cmdInsert.ExecuteNonQuery());


                var cmdView = _db.Server.GetCommand(
                    "CREATE VIEW v_GetRowCount_Views as select * from GetRowCount_Views", con);
                cmdView.ExecuteNonQuery();

                Assert.AreEqual(1, _db.ExpectTable("v_GetRowCount_Views").GetRowCount());
            }
        }
    }
}
