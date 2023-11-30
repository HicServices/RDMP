// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components;

[Category("Unit")]
public class RemoveDuplicatesTests
{
    [Test]
    public void TestRemovingDuplicatesFromDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2", typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);

        Assert.That(dt.Rows, Has.Count.EqualTo(3));


        Assert.That(dt.Rows[0]["Col2"], Is.EqualTo(123));

        var receiver = new ToMemoryDataLoadEventListener(true);

        var result = new RemoveDuplicates().ProcessPipelineData(dt, receiver, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            //should have told us that it processed 3 rows
            Assert.That(receiver.LastProgressRecieivedByTaskName["Evaluating For Duplicates"].Progress.Value, Is.EqualTo(3));

            //and discarded 2 of them as duplicates
            Assert.That(receiver.LastProgressRecieivedByTaskName["Discarding Duplicates"].Progress.Value, Is.EqualTo(2));

            Assert.That(result.Rows, Has.Count.EqualTo(1));
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Rows[0]["Col1"], Is.EqualTo("Fish"));
            Assert.That(result.Rows[0]["Col2"], Is.EqualTo(123));
        });
    }

    [Test]
    public void TestEmptyDataTable()
    {
        Assert.That(new RemoveDuplicates().ProcessPipelineData(new DataTable(), ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken()).Rows, Is.Empty);
    }

    [Test]
    public void TestMultipleBatches()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2", typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);


        var dt2 = new DataTable();
        dt2.Columns.Add("Col1");
        dt2.Columns.Add("Col2", typeof(int));

        dt2.Rows.Add("Fish", 123);
        dt2.Rows.Add("Haddock", 123);


        var remover = new RemoveDuplicates();

        Assert.Multiple(() =>
        {
            //send it the batch with the duplication it will return 1 row
            Assert.That(remover.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                    new GracefulCancellationToken()).Rows, Has.Count.EqualTo(1));

            //now send it the second batch which contains 2 records, one duplication against first batch and one new one, expect only 1 row to come back
            Assert.That(remover.ProcessPipelineData(dt2, ThrowImmediatelyDataLoadEventListener.Quiet,
                    new GracefulCancellationToken()).Rows, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TestNulls()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2", typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", null);
        dt.Rows.Add(null, 123);
        dt.Rows.Add("Pizza", null);
        dt.Rows.Add(null, null);
        dt.Rows.Add(null, null);

        var remover = new RemoveDuplicates();

        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(6));

            //send it the batch with the duplication it will return 5 rows (the only duplicate is the double null)
            Assert.That(remover.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                    new GracefulCancellationToken()).Rows, Has.Count.EqualTo(5));
        });
    }
}