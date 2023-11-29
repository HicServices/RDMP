// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class ExecuteCrossServerDatasetExtractionSourceTest : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void CrossServerExtraction()
    {
        Execute(out _, out var result);

        var r = (ExecuteDatasetExtractionFlatFileDestination)result;

        //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
        Assert.That(File.ReadAllText(r.OutputFile).Trim(), Is.EqualTo($@"ReleaseID,Name,DateOfBirth
{_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()]},Dave,2001-01-01"));

        File.Delete(r.OutputFile);
    }

    protected override Pipeline SetupPipeline()
    {
        var pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
        var component = new PipelineComponent(CatalogueRepository, pipeline,
            typeof(ExecuteDatasetExtractionFlatFileDestination), 0, "Destination");
        var arguments = component.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionFlatFileDestination>()
            .ToArray();

        if (arguments.Length < 3)
            throw new Exception(
                "Expected only 2 arguments for type ExecuteDatasetExtractionFlatFileDestination, did somebody add another [DemandsInitialization]? if so handle it below");

        arguments.Single(a => a.Name.Equals("DateFormat")).SetValue("yyyy-MM-dd");
        arguments.Single(a => a.Name.Equals("DateFormat")).SaveToDatabase();

        arguments.Single(a => a.Name.Equals("FlatFileType")).SetValue(ExecuteExtractionToFlatFileType.CSV);
        arguments.Single(a => a.Name.Equals("FlatFileType")).SaveToDatabase();

        AdjustPipelineComponentDelegate?.Invoke(component);

        var component2 = new PipelineComponent(CatalogueRepository, pipeline,
            typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
        var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
            .ToArray();
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();
        arguments2.Single(a => a.Name.Equals(nameof(ExecuteCrossServerDatasetExtractionSource.TemporaryTableName)))
            .SetValue("");
        arguments2.Single(a => a.Name.Equals(nameof(ExecuteCrossServerDatasetExtractionSource.TemporaryTableName)))
            .SaveToDatabase();
        AdjustPipelineComponentDelegate?.Invoke(component2);


        //configure the component as the destination
        pipeline.DestinationPipelineComponent_ID = component.ID;
        pipeline.SourcePipelineComponent_ID = component2.ID;
        pipeline.SaveToDatabase();

        return pipeline;
    }

    [Test]
    public void HackSQLTest_Normal()
    {
        if (_request.QueryBuilder == null)
            _request.GenerateQueryBuilder();

        var expectedOutput =
            string.Format(@"/*The ID of the cohort in [tempdb]..[Cohort]*/
DECLARE @CohortDefinitionID AS int;
SET @CohortDefinitionID=-599;
/*The project number of project {0}ExtractionConfiguration*/
DECLARE @ProjectNumber AS int;
SET @ProjectNumber=1;

SELECT DISTINCT 

[tempdb]..[Cohort].[ReleaseID] AS ReleaseID,
[{0}ScratchArea].[dbo].[TestTable].[Name],
[{0}ScratchArea].[dbo].[TestTable].[DateOfBirth]
FROM 
[{0}ScratchArea].[dbo].[TestTable]
INNER JOIN [tempdb]..[Cohort] ON [{0}ScratchArea].[dbo].[TestTable].[PrivateID]=[tempdb]..[Cohort].[PrivateID]
WHERE
[tempdb]..[Cohort].[cohortDefinition_id]=-599
", TestDatabaseNames.Prefix);

        //cross server is only used if cohort and dataset are on different servers so pretend the cohort is on bob server
        var ect = (ExternalCohortTable)_request.ExtractableCohort.ExternalCohortTable;
        ect.Server = "bob";

        var e = DataExportRepository.GetObjectByID<ExternalCohortTable>(_request.ExtractableCohort
            .ExternalCohortTable_ID);
        var origValue = e.Database;

        e.Database = CohortDatabaseName;
        e.SaveToDatabase();
        try
        {
            var s = new ExecuteCrossServerDatasetExtractionSource
            {
                TemporaryDatabaseName = "tempdb"
            };
            s.PreInitialize(_request, ThrowImmediatelyDataLoadEventListener.Quiet);
            var hacked = s.HackExtractionSQL(_request.QueryBuilder.SQL,
                ThrowImmediatelyDataLoadEventListener.QuietPicky);

            Assert.That(hacked.Trim(), Is.EqualTo(expectedOutput.Trim()));
        }
        finally
        {
            e.Database = origValue;
            e.SaveToDatabase();
        }
    }
}