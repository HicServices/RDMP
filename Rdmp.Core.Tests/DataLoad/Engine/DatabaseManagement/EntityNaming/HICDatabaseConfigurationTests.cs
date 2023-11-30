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

        Assert.That(result, Has.Length.EqualTo(testLookup ? 2 : 1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Database.GetRuntimeName(), Is.EqualTo("mydb_RAW").IgnoreCase);
            Assert.That(result[0].GetRuntimeName(), Is.EqualTo("My_Table").IgnoreCase);
        });

        if (testLookup)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result[1].Database.GetRuntimeName(), Is.EqualTo("mydb_RAW").IgnoreCase);
                Assert.That(result[1].GetRuntimeName(), Is.EqualTo("MyHeartyLookup").IgnoreCase);
            });
        }

        result = conf.ExpectTables(job, LoadBubble.Staging, testLookup).ToArray();
        Assert.That(result, Has.Length.EqualTo(testLookup ? 2 : 1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Database.GetRuntimeName(), Is.EqualTo("DLE_STAGING").IgnoreCase);
            Assert.That(result[0].GetRuntimeName(), Is.EqualTo("mydb_My_Table_STAGING").IgnoreCase);
        });

        if (testLookup)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result[1].Database.GetRuntimeName(), Is.EqualTo("DLE_STAGING").IgnoreCase);
                Assert.That(result[1].GetRuntimeName(), Is.EqualTo("mydb_MyHeartyLookup_STAGING").IgnoreCase);
            });
        }
    }
}