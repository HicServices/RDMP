// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;
using Rdmp.Core.Logging;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DataLoadProgressUpdateInfoTests : DatabaseTests
{
    private ScheduledDataLoadJob _job;

    #region Setup Methods

    public DataLoadProgressUpdateInfoTests()
    {
        var cata = Substitute.For<ICatalogue>();
        cata.LoggingDataTask.Returns("NothingTask");
        cata.GetTableInfoList(false).Returns(Array.Empty<TableInfo>());
        cata.GetLookupTableInfoList().Returns(Array.Empty<TableInfo>());

        var lmd = Substitute.For<ILoadMetadata>();
        lmd.GetAllCatalogues().Returns(new[] { cata });

        _job = new ScheduledDataLoadJob(null, "fish", Substitute.For<ILogManager>(), lmd, null,
            new ThrowImmediatelyDataLoadJob(), null);
    }

    #endregion

    [Test]
    public void AddBasicNormalStrategy_NoDates()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, null));
        Assert.That(ex.Message, Does.StartWith("Job does not have any DatesToRetrieve"));
    }

    [Test]
    public void AddBasicNormalStrategy_MaxDate()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        Assert.That(updateInfo.Strategy, Is.EqualTo(DataLoadProgressUpdateStrategy.UseMaxRequestedDay));

        _job.DatesToRetrieve = new List<DateTime>
        {
            new(2001, 1, 1),
            new(2001, 1, 2),
            new(2001, 1, 3)
        };
        try
        {
            var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job, null);


            Assert.That(added.DateToSetProgressTo, Is.EqualTo(new DateTime(2001, 1, 3)));
        }
        finally
        {
            _job.DatesToRetrieve.Clear();
        }
    }

    [Test]
    public void AddRAWSQLStrategy_NoSQL()
    {
        var updateInfo = new DataLoadProgressUpdateInfo
        {
            Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW
        };

        var ex = Assert.Throws<Exception>(() =>
            updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.That(ex.Message, Does.StartWith("Strategy is ExecuteScalarSQLInRAW but there is no ExecuteScalarSQL"));
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlCrashes()
    {
        var updateInfo = new DataLoadProgressUpdateInfo
        {
            Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW,
            ExecuteScalarSQL = "SELECT Top 1 BarrelORum from CaptainMorgansSpicedRumBarrel"
        };

        var ex = Assert.Throws<DataLoadProgressUpdateException>(() =>
            updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.StartWith("Failed to execute the following SQL in the RAW database"));
            Assert.That(ex.InnerException, Is.InstanceOf<SqlException>());
        });
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNull()
    {
        var updateInfo = new DataLoadProgressUpdateInfo
        {
            Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW,
            ExecuteScalarSQL = "SELECT null"
        };

        var ex = Assert.Throws<DataLoadProgressUpdateException>(() =>
            updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.That(ex.Message, Does.Contain("ExecuteScalarSQL"));
        Assert.That(ex.Message, Does.Contain("returned null"));
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNonDate()
    {
        var updateInfo = new DataLoadProgressUpdateInfo
        {
            Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW,
            ExecuteScalarSQL = "SELECT 'fishfish'"
        };

        var ex = Assert.Throws<DataLoadProgressUpdateException>(() =>
            updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.Multiple(() =>
        {
            Assert.That(
                    ex.Message, Is.EqualTo("ExecuteScalarSQL specified for determining the maximum date of data loaded returned a value that was not a Date:fishfish"));
            Assert.That(ex.InnerException, Is.InstanceOf<FormatException>());
        });
    }

    [Test]
    public void AddRAWSQLStrategy_SQLCorrect()
    {
        _job.DatesToRetrieve = new List<DateTime>
        {
            new(2001, 1, 6),
            new(2001, 1, 7),
            new(2001, 1, 8)
        };

        var updateInfo = new DataLoadProgressUpdateInfo
        {
            Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW,
            ExecuteScalarSQL = "SELECT '2001-01-07'"
        };

        var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job,
            GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));

        Assert.That(added.DateToSetProgressTo, Is.EqualTo(new DateTime(2001, 1, 7)));

        _job.DatesToRetrieve.Clear();
    }
}