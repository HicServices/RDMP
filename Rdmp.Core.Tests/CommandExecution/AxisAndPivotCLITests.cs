// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.RemoteBackup;
using System;

namespace Rdmp.Core.Tests.CommandExecution;

public class AxisAndPivotCLITests : CommandCliTests
{
    [Test]
    public void SetPivot_DimensionNonExistant()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        var cmd = new ExecuteCommandSetPivot(GetMockActivator(), ac, "fff");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.That(
                    ex.Message, Is.EqualTo("Could not find AggregateDimension fff in Aggregate My graph so could not set it as a pivot dimension.  Try adding the column to the aggregate first"));
    }

    [Test]
    public void GitTest()
    {
        var lmd = new LoadMetadata();
        lmd.LocationOfCacheDirectory = "C:\\temp";
        var x = new GitRemoteBackupHelper(GetMockActivator(), lmd);
        x.PullChanges();
    }

    [Test]
    public void SetPivot_Exists()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";

        var cmd = new ExecuteCommandSetPivot(GetMockActivator(), ac, "frogmarch");
        cmd.Execute();

        Assert.That(ac.PivotOnDimensionID, Is.EqualTo(dim.ID));

        cmd = new ExecuteCommandSetPivot(GetMockActivator(), ac, null);
        cmd.Execute();

        Assert.That(ac.PivotOnDimensionID, Is.Null);
    }

    [Test]
    public void SetPivot_ExistsButIsADate()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";
        dim.ColumnInfo.Data_type = "datetime";

        var cmd = new ExecuteCommandSetPivot(GetMockActivator(), ac, "frogmarch");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.That(ex.Message, Is.EqualTo("AggregateDimension frogmarch is a Date so cannot set it as a Pivot for Aggregate My graph"));
    }

    [Test]
    public void SetAxis_DimensionNonExistant()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        var cmd = new ExecuteCommandSetAxis(GetMockActivator(), ac, "fff");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.That(
                    ex.Message, Is.EqualTo("Could not find AggregateDimension fff in Aggregate My graph so could not set it as an axis dimension.  Try adding the column to the aggregate first"));
    }

    [Test]
    public void SetAxis_Exists()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";
        dim.ColumnInfo.Data_type = "datetime";

        Assert.That(ac.GetAxisIfAny(), Is.Null);

        var cmd = new ExecuteCommandSetAxis(GetMockActivator(), ac, "frogmarch");
        cmd.Execute();

        Assert.That(ac.GetAxisIfAny(), Is.Not.Null);

        cmd = new ExecuteCommandSetAxis(GetMockActivator(), ac, null);
        cmd.Execute();

        Assert.That(ac.GetAxisIfAny(), Is.Null);
    }

    [Test]
    public void SetAxis_ExistsButIsNotADate()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";

        var cmd = new ExecuteCommandSetAxis(GetMockActivator(), ac, "frogmarch");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.That(ex.Message, Is.EqualTo("AggregateDimension frogmarch is not a Date so cannot set it as an axis for Aggregate My graph"));
    }
}