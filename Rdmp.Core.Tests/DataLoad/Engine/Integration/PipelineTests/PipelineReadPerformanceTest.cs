// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using NUnit.Framework;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests;

public class PipelineReadPerformanceTest:DatabaseTests
{
    private BulkTestsData _bulkTestData;
        
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        _bulkTestData = new BulkTestsData(CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));
        _bulkTestData.SetupTestData();

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
}