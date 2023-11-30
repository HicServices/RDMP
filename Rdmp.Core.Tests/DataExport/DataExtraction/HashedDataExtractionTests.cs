// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class HashedDataExtractionTests : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void ExtractNormally()
    {
        AdjustPipelineComponentDelegate = p =>
        {
            if (p.Class.Contains("ExecuteDatasetExtractionSource"))
            {
                var hashJoinsArg = p.PipelineComponentArguments.Single(a => a.Name.Equals("UseHashJoins"));
                hashJoinsArg.SetValue(true);
                hashJoinsArg.SaveToDatabase();
            }
        };

        _catalogue.Name = "TestTable";
        _catalogue.SaveToDatabase();
        _request.DatasetBundle.DataSet.RevertToDatabaseState();

        Assert.That(_request.ColumnsToExtract.Count(c => c.IsExtractionIdentifier), Is.EqualTo(1));
        var listener = new ToMemoryDataLoadEventListener(true);

        Execute(out _, out var result, listener);

        var messages =
            listener.EventsReceivedBySender.SelectMany(m => m.Value)
                .Where(m => m.ProgressEventType == ProgressEventType.Information &&
                            m.Message.Contains("/*Decided on extraction SQL:*/"))
                .ToArray();

        Assert.That(messages, Has.Length.EqualTo(1), "Expected a message about what the final extraction SQL was");
        Assert.That(messages[0].Message, Does.Contain(" HASH JOIN "),
            "expected use of hash matching was not reported by ExecuteDatasetExtractionSource in the SQL actually executed");

        var r = (ExecuteDatasetExtractionFlatFileDestination)result;

        Assert.Multiple(() =>
        {
            //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
            Assert.That(File.ReadAllText(r.OutputFile).Trim(), Is.EqualTo($@"ReleaseID,Name,DateOfBirth
{_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()]},Dave,2001-01-01"));

            Assert.That(_request.QueryBuilder.SelectColumns.Count(c => c.IColumn is ReleaseIdentifierSubstitution), Is.EqualTo(1));
        });
        File.Delete(r.OutputFile);
    }
}