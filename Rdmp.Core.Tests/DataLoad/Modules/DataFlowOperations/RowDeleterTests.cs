// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Modules.DataFlowOperations;

internal class RowDeleterTests
{
    [Test]
    public void TestRowDeleter_OneCell()
    {
        var operation = new RowDeleter
        {
            ColumnNameToFind = "b",
            DeleteRowsWhereValuesMatch = new Regex("^cat$")
        };

        using var dt = new DataTable();
        dt.Columns.Add("a");
        dt.Columns.Add("b");

        dt.Rows.Add("cat", "cat");
        dt.Rows.Add("dog", "dog");
        dt.Rows.Add("cat", "dog");

        var listener = new ToMemoryDataLoadEventListener(true);

        var result = operation.ProcessPipelineData(dt, listener, new GracefulCancellationToken());

        Assert.That(result.Rows, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(result.Rows[0]["a"], Is.EqualTo("dog"));
            Assert.That(result.Rows[0]["b"], Is.EqualTo("dog"));

            Assert.That(result.Rows[1]["a"], Is.EqualTo("cat"));
            Assert.That(result.Rows[1]["b"], Is.EqualTo("dog"));
        });

        operation.Dispose(listener, null);

        var msg = listener.EventsReceivedBySender[operation].Single();

        Assert.Multiple(() =>
        {
            Assert.That(msg.ProgressEventType, Is.EqualTo(ProgressEventType.Warning));
            Assert.That(msg.Message, Is.EqualTo("Total RowDeleted operations for ColumnNameToFind 'b' was 1"));
        });
    }
}