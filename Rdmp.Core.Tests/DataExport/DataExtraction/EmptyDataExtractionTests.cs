// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class EmptyDataExtractionTests:TestsRequiringAnExtractionConfiguration
{

    private void TruncateDataTable()
    {
        var server = Database.Server;
        using (var con = server.GetConnection())
        {
            con.Open();

            var cmdTruncate = server.GetCommand("TRUNCATE TABLE TestTable",con);
            cmdTruncate.ExecuteNonQuery();

            con.Close();
        }

    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void TestAllowingEmptyDatasets(bool allowEmptyDatasetExtractions)
    {
        var p = SetupPipeline();
            
        TruncateDataTable();

        var host = new ExtractionPipelineUseCase(new ThrowImmediatelyActivator(RepositoryLocator),_request.Configuration.Project, _request, p, DataLoadInfo.Empty);

        var engine = host.GetEngine(p, new ThrowImmediatelyDataLoadEventListener());
        host.Source.AllowEmptyExtractions = allowEmptyDatasetExtractions;

        var token = new GracefulCancellationToken();
            
        if(allowEmptyDatasetExtractions)
        {

            var dt = host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token);
            Assert.IsNull(host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token));

            Assert.AreEqual(0,dt.Rows.Count);
            Assert.AreEqual(3, dt.Columns.Count);
        }
        else
        {
            var exception = Assert.Throws<Exception>(() => host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token));

            Assert.IsTrue(exception.Message.StartsWith("There is no data to load, query returned no rows, query was"));
        }

        p.DeleteInDatabase();
    }

    [Test]
    public void ProducesEmptyCSV()
    {
        TruncateDataTable();
        AllowEmptyExtractions = true;

        ExtractionPipelineUseCase execute;
        IExecuteDatasetExtractionDestination result;

        Assert.AreEqual(1, _request.ColumnsToExtract.Count(c => c.IsExtractionIdentifier));

        base.Execute(out execute, out result);

        var r = (ExecuteDatasetExtractionFlatFileDestination)result;

        //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
        Assert.AreEqual(@"ReleaseID,Name,DateOfBirth", File.ReadAllText(r.OutputFile).Trim());

        Assert.AreEqual(1, _request.QueryBuilder.SelectColumns.Count(c => c.IColumn is ReleaseIdentifierSubstitution));
        File.Delete(r.OutputFile);
    }
}