// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Tests.CommandExecution;

public class AxisAndPivotCLITests : CommandCliTests
{
    [Test]
    public void SetPivot_DimensionNonExistant()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        var cmd = new ExecuteCommandSetPivot(GetMockActivator().Object, ac, "fff");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.AreEqual(
            "Could not find AggregateDimension fff in Aggregate My graph so could not set it as a pivot dimension.  Try adding the column to the aggregate first",
            ex.Message);
    }

    [Test]
    public void SetPivot_Exists()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";

        var cmd = new ExecuteCommandSetPivot(GetMockActivator().Object, ac, "frogmarch");
        cmd.Execute();

        Assert.AreEqual(dim.ID, ac.PivotOnDimensionID);

        cmd = new ExecuteCommandSetPivot(GetMockActivator().Object, ac, null);
        cmd.Execute();

        Assert.IsNull(ac.PivotOnDimensionID);
    }

    [Test]
    public void SetPivot_ExistsButIsADate()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";
        dim.ColumnInfo.Data_type = "datetime";

        var cmd = new ExecuteCommandSetPivot(GetMockActivator().Object, ac, "frogmarch");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.AreEqual("AggregateDimension frogmarch is a Date so cannot set it as a Pivot for Aggregate My graph",
            ex.Message);
    }

    [Test]
    public void SetAxis_DimensionNonExistant()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        var cmd = new ExecuteCommandSetAxis(GetMockActivator().Object, ac, "fff");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.AreEqual(
            "Could not find AggregateDimension fff in Aggregate My graph so could not set it as an axis dimension.  Try adding the column to the aggregate first",
            ex.Message);
    }

    [Test]
    public void SetAxis_Exists()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";
        dim.ColumnInfo.Data_type = "datetime";

        Assert.IsNull(ac.GetAxisIfAny());

        var cmd = new ExecuteCommandSetAxis(GetMockActivator().Object, ac, "frogmarch");
        cmd.Execute();

        Assert.IsNotNull(ac.GetAxisIfAny());

        cmd = new ExecuteCommandSetAxis(GetMockActivator().Object, ac, null);
        cmd.Execute();

        Assert.IsNull(ac.GetAxisIfAny());
    }

    [Test]
    public void SetAxis_ExistsButIsNotADate()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var dim = WhenIHaveA<AggregateDimension>();


        dim.AggregateConfiguration_ID = ac.ID;
        dim.Alias = "frogmarch";

        var cmd = new ExecuteCommandSetAxis(GetMockActivator().Object, ac, "frogmarch");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.AreEqual("AggregateDimension frogmarch is not a Date so cannot set it as an axis for Aggregate My graph",
            ex.Message);
    }
}