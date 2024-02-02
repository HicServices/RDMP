// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components;

[TestFixture]
[Category("Unit")]
public class TransposerTests
{
    private DataTable dt = new();


    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        dt.Columns.Add("recipe");
        dt.Columns.Add("Fishcakes");
        dt.Columns.Add("Chips");
        dt.Columns.Add("Gateau");
        dt.Rows.Add("protein", "20", "30", "40");
        dt.Rows.Add("fat", "11", "2", "33");
        dt.Rows.Add("carbohydrate", "55", "0", "5");
    }

    [Test]
    public void TransposerTest_ThrowOnDualBatches()
    {
        var transposer = new Transposer();
        transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
        var ex = Assert.Throws<NotSupportedException>(() =>
            transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        Assert.That(
            ex.Message, Is.EqualTo("Error, we received multiple batches, Transposer only works when all the data arrives in a single DataTable"));
    }

    [Test]
    public void TransposerTest_ThrowOnEmptyDataTable()
    {
        var transposer = new Transposer();
        var ex = Assert.Throws<NotSupportedException>(() =>
            transposer.ProcessPipelineData(new DataTable(), new ThrowImmediatelyDataLoadJob(),
                new GracefulCancellationToken()));
        Assert.That(ex.Message, Is.EqualTo("DataTable toProcess had 0 rows and 0 columns, thus it cannot be transposed"));
    }


    [Test]
    public void TransposerTest_TableTransposed()
    {
        var transposer = new Transposer();
        var actual =
            transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        var expectedResult = new DataTable();

        expectedResult.Columns.Add("recipe");
        expectedResult.Columns.Add("protein");
        expectedResult.Columns.Add("fat");
        expectedResult.Columns.Add("carbohydrate");

        expectedResult.Rows.Add("Fishcakes", "20", "11", "55");
        expectedResult.Rows.Add("Chips", "30", "2", "0");
        expectedResult.Rows.Add("Gateau", "40", "33", "5");

        for (var i = 0; i < actual.Columns.Count; i++)
            Assert.That(actual.Columns[i].ColumnName, Is.EqualTo(expectedResult.Columns[i].ColumnName));

        for (var i = 0; i < expectedResult.Rows.Count; i++)
        for (var j = 0; j < actual.Columns.Count; j++)
                Assert.That(actual.Rows[i][j], Is.EqualTo(expectedResult.Rows[i][j]));
    }

    [Test]
    public void TestTransposerDodgyHeaders()
    {
        var dr = dt.Rows.Add("32 GramMax", "55", "0", "5");

        var transposer = new Transposer
        {
            MakeHeaderNamesSane = true
        };
        var actual =
            transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        Assert.That(actual.Columns.Contains("_32GramMax"));

        dt.Rows.Remove(dr);
    }
}