using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.DataFlowPipeline;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.DataExport.Data;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCommitting
{
    class CommitCohortExample: DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,"varchar(10)")]
        [TestCase(DatabaseType.MySql,"varchar(10)")]
        [TestCase(DatabaseType.Oracle,"varchar2(10)")]
        public void CommitCohortExampleTest(DatabaseType dbType,string privateDataType)
        {
            RunBlitzDatabases(RepositoryLocator);

            //find the test server (where we will create the store schema)
            var db = GetCleanedServer(dbType);
            
            //create the cohort store table
            var wizard = new CreateNewCohortDatabaseWizard(db,CatalogueRepository,DataExportRepository,false);
            var privateColumn = new PrivateIdentifierPrototype("chi", privateDataType);
            var externalCohortTable = wizard.CreateDatabase(privateColumn,new ThrowImmediatelyCheckNotifier());

            //create a project into which we want to import a cohort
            var project = new Project(DataExportRepository, "MyProject");
            project.ProjectNumber = 500;
            project.SaveToDatabase();

            //create a description of the cohort we are importing
            var definition = new CohortDefinition(null, "MyCohort", 1, 500, externalCohortTable);

            //create our cohort (normally this would be read from a file or be the results of cohort identification query)
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Rows.Add("0101010101");
            dt.Rows.Add("0202020202");

            //Create a pipeline (we only need the destination)
            var pipelineDestination = new BasicCohortDestination();

            //choose how to allocate the anonymous release identifiers
            pipelineDestination.ReleaseIdentifierAllocator = typeof(ProjectConsistentGuidReleaseIdentifierAllocator);

            //initialize the destination
            pipelineDestination.PreInitialize(
                new CohortCreationRequest(project, definition, DataExportRepository,"A cohort created in an example unit test"),
                new ThrowImmediatelyDataLoadEventListener());

            //process the cohort data table
            pipelineDestination.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            //there should be no cohorts yet
            Assert.IsEmpty(DataExportRepository.GetAllObjects<ExtractableCohort>());

            //dispose of the pipeline
            pipelineDestination.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

            //now there should be one
            ExtractableCohort cohort = DataExportRepository.GetAllObjects<ExtractableCohort>().Single();

            Assert.AreEqual(500,cohort.ExternalProjectNumber);
            Assert.AreEqual(2,cohort.CountDistinct);

            var tbl = externalCohortTable.Discover().ExpectTable(externalCohortTable.TableName);
            Assert.AreEqual(2,tbl.GetRowCount());
            var dtInDatabase = tbl.GetDataTable();
            
            //guid will be something like "6fb23de5-e8eb-46eb-84b5-dd368da21073"
            Assert.AreEqual(36,dtInDatabase.Rows[0]["ReleaseId"].ToString().Length);
            Assert.AreEqual("0101010101",dtInDatabase.Rows[0]["chi"]);
        }
    }
}
