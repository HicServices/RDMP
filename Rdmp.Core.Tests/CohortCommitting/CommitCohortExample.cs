// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCommitting;

internal class CommitCohortExample : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer, "varchar(10)")]
    [TestCase(DatabaseType.MySql, "varchar(10)")]
    [TestCase(DatabaseType.Oracle, "varchar2(10)")]
    public void CommitCohortExampleTest(DatabaseType dbType, string privateDataType)
    {
        RunBlitzDatabases(RepositoryLocator);

        //find the test server (where we will create the store schema)
        var db = GetCleanedServer(dbType);

        //create the cohort store table
        var wizard = new CreateNewCohortDatabaseWizard(db, CatalogueRepository, DataExportRepository, false);
        var privateColumn = new PrivateIdentifierPrototype("chi", privateDataType);
        var externalCohortTable = wizard.CreateDatabase(privateColumn, ThrowImmediatelyCheckNotifier.Quiet);

        Assert.That(externalCohortTable.DatabaseType, Is.EqualTo(dbType));

        //create a project into which we want to import a cohort
        var project = new Project(DataExportRepository, "MyProject")
        {
            ProjectNumber = 500
        };
        project.SaveToDatabase();

        //create a description of the cohort we are importing
        var definition = new CohortDefinition(null, "MyCohort", 1, 500, externalCohortTable);

        //create our cohort (normally this would be read from a file or be the results of cohort identification query)
        var dt = new DataTable();
        dt.Columns.Add("chi");
        dt.Rows.Add("0101010101");
        dt.Rows.Add("0202020202");

        //Create a pipeline (we only need the destination)
        var pipelineDestination = new BasicCohortDestination
        {
            //choose how to allocate the anonymous release identifiers
            ReleaseIdentifierAllocator = typeof(ProjectConsistentGuidReleaseIdentifierAllocator)
        };

        //initialize the destination
        pipelineDestination.PreInitialize(
            new CohortCreationRequest(project, definition, DataExportRepository,
                "A cohort created in an example unit test"),
            ThrowImmediatelyDataLoadEventListener.Quiet);

        //process the cohort data table
        pipelineDestination.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        //there should be no cohorts yet
        Assert.That(DataExportRepository.GetAllObjects<ExtractableCohort>(), Is.Empty);

        //dispose of the pipeline
        pipelineDestination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

        //now there should be one
        var cohort = DataExportRepository.GetAllObjects<ExtractableCohort>().Single();

        Assert.Multiple(() =>
        {
            //make sure we are all on the same page about what the DBMS type is (nothing cached etc)
            Assert.That(cohort.ExternalCohortTable.DatabaseType, Is.EqualTo(dbType));
            Assert.That(cohort.GetQuerySyntaxHelper().DatabaseType, Is.EqualTo(dbType));

            Assert.That(cohort.ExternalProjectNumber, Is.EqualTo(500));
            Assert.That(cohort.CountDistinct, Is.EqualTo(2));
        });

        var tbl = externalCohortTable.DiscoverCohortTable();
        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));
        var dtInDatabase = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //guid will be something like "6fb23de5-e8eb-46eb-84b5-dd368da21073"
            Assert.That(dtInDatabase.Rows[0]["ReleaseId"].ToString(), Has.Length.EqualTo(36));
            Assert.That(dtInDatabase.Rows[0]["chi"], Is.EqualTo("0101010101"));
        });
    }
}