// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components;

[Category("Unit")]
public class RemoveDuplicatesTests
{
    [Test]
    public void TestRemovingDuplicatesFromDataTable()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2",typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);

        Assert.AreEqual(3,dt.Rows.Count);
            
            
        Assert.AreEqual(123, dt.Rows[0]["Col2"]);

        var receiver = new ToMemoryDataLoadEventListener(true);

        var result = new RemoveDuplicates().ProcessPipelineData(dt, receiver, new GracefulCancellationToken());

        //should have told us that it processed 3 rows
        Assert.AreEqual(3,receiver.LastProgressRecieivedByTaskName["Evaluating For Duplicates"].Progress.Value);

        //and discarded 2 of them as duplicates
        Assert.AreEqual(2, receiver.LastProgressRecieivedByTaskName["Discarding Duplicates"].Progress.Value);

        Assert.AreEqual(1, result.Rows.Count);
        Assert.AreEqual("Fish", result.Rows[0]["Col1"]);
        Assert.AreEqual(123, result.Rows[0]["Col2"]);
    }

    [Test]
    public void TestEmptyDataTable()
    {
        Assert.AreEqual(0,new RemoveDuplicates().ProcessPipelineData(new DataTable(),new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken()).Rows.Count);
    }

    [Test]
    public void TestMultipleBatches()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2", typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", 123);


        DataTable dt2 = new DataTable();
        dt2.Columns.Add("Col1");
        dt2.Columns.Add("Col2", typeof(int));

        dt2.Rows.Add("Fish", 123);
        dt2.Rows.Add("Haddock", 123);


        var remover = new RemoveDuplicates(); 

        //send it the batch with the duplication it will return 1 row
        Assert.AreEqual(1,remover.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()).Rows.Count);

        //now send it the second batch which contains 2 records, one duplication against first batch and one new one, expect only 1 row to come back
        Assert.AreEqual(1, remover.ProcessPipelineData(dt2, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()).Rows.Count);
    }

    [Test]
    public void TestNulls()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2", typeof(int));

        dt.Rows.Add("Fish", 123);
        dt.Rows.Add("Fish", null);
        dt.Rows.Add(null, 123);
        dt.Rows.Add("Pizza", null);
        dt.Rows.Add(null, null);
        dt.Rows.Add(null, null);

        var remover = new RemoveDuplicates();

        Assert.AreEqual(6,dt.Rows.Count);

        //send it the batch with the duplication it will return 5 rows (the only duplicate is the double null)
        Assert.AreEqual(5, remover.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()).Rows.Count);


    }
}