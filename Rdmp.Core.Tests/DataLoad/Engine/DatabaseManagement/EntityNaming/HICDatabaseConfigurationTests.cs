// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.DatabaseManagement.EntityNaming;

internal class HICDatabaseConfigurationTests : UnitTests
{
    /// <summary>
    /// Tests the ability of <see cref="HICDatabaseConfiguration"/> to predict where tables will exist
    /// during a load at various stages (RAW, STAGING etc).  This is largely controlled by what tables the
    /// <see cref="IDataLoadJob"/> says it loads and what the names should be according to
    /// the <see cref="INameDatabasesAndTablesDuringLoads"/>
    /// </summary>
    /// <param name="testLookup"></param>
    [TestCase(true)]
    [TestCase(false)]
    public void TestHICDatabaseConfiguration_ExpectTables(bool testLookup)
    {
        var conf = new HICDatabaseConfiguration(new DiscoveredServer("localhost", "mydb",
            DatabaseType.MicrosoftSQLServer, null, null), new FixedStagingDatabaseNamer("mydb"));

        var ti = WhenIHaveA<TableInfo>();
        var lookup = WhenIHaveA<TableInfo>();
        lookup.Name = "MyHeartyLookup";
        lookup.Database = "LookupsDb";
        lookup.SaveToDatabase();

        var job = Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo>(new[] { ti }));
        job.LookupTablesToLoad.Returns(new List<ITableInfo>(new[] { lookup }));

        var result = conf.ExpectTables(job, LoadBubble.Raw, testLookup).ToArray();

        Assert.AreEqual(testLookup ? 2 : 1, result.Length);
        StringAssert.AreEqualIgnoringCase("mydb_RAW", result[0].Database.GetRuntimeName());
        StringAssert.AreEqualIgnoringCase("My_Table", result[0].GetRuntimeName());

        if (testLookup)
        {
            StringAssert.AreEqualIgnoringCase("mydb_RAW", result[1].Database.GetRuntimeName());
            StringAssert.AreEqualIgnoringCase("MyHeartyLookup", result[1].GetRuntimeName());
        }

        result = conf.ExpectTables(job, LoadBubble.Staging, testLookup).ToArray();
        Assert.AreEqual(testLookup ? 2 : 1, result.Length);
        StringAssert.AreEqualIgnoringCase("DLE_STAGING", result[0].Database.GetRuntimeName());
        StringAssert.AreEqualIgnoringCase("mydb_My_Table_STAGING", result[0].GetRuntimeName());

        if (testLookup)
        {
            StringAssert.AreEqualIgnoringCase("DLE_STAGING", result[1].Database.GetRuntimeName());
            StringAssert.AreEqualIgnoringCase("mydb_MyHeartyLookup_STAGING", result[1].GetRuntimeName());
        }
    }
}