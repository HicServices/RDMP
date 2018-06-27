using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.CohortCreationPipeline.Destinations;
using DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation;
using DataExportLibrary.CohortDatabaseWizard;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataExportLibrary.Tests.Cohort
{
    public class CreateNewCohortDatabaseWizardTests:DatabaseTests
    {
        private Catalogue _cata1;
        private Catalogue _cata2;
        private TableInfo _t1;
        private TableInfo _t2;
        private ColumnInfo _c1;
        private ColumnInfo _c2;
        private CatalogueItem _ci1;
        private CatalogueItem _ci2;

        private ExtractionInformation _extractionInfo1;
        private ExtractionInformation _extractionInfo2;
        
        [SetUp]
        public void SetupCatalogues()
        {
            _cata1 = new Catalogue(CatalogueRepository, "Dataset1");
            _cata2 = new Catalogue(CatalogueRepository, "Dataset2");
            
            _t1 = new TableInfo(CatalogueRepository, "T1");
            _t2 = new TableInfo(CatalogueRepository, "T2");
            
            _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c2 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierB", "int", _t2);
            
            _ci1 = new CatalogueItem(CatalogueRepository, _cata1, "PrivateIdentifierA");
            _ci2 = new CatalogueItem(CatalogueRepository, _cata2, "PrivateIdentifierB");
            
            _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString());
            _extractionInfo2 = new ExtractionInformation(CatalogueRepository, _ci2, _c2, _c2.ToString());

            cohortDatabaseName = TestDatabaseNames.GetConsistentName("Tests_CreateCohortDatabaseWizard");
        }

        private string cohortDatabaseName;

        [TearDown]
        public void TearDownCatalogues()
        {
            _cata1.DeleteInDatabase();
            _cata2.DeleteInDatabase();

            _t1.DeleteInDatabase();
            _t2.DeleteInDatabase();

            foreach (
                ExternalCohortTable source in
                    DataExportRepository.GetAllObjects<ExternalCohortTable>()
                        .Where(s => s.Name.Equals(cohortDatabaseName)))
                source.DeleteInDatabase();
        }

        [Test]
        public void TestMissingColumnInfos()
        {
            _extractionInfo1.IsExtractionIdentifier = true;
            _extractionInfo1.SaveToDatabase();
            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(null,CatalogueRepository, DataExportRepository);

            //it finds it!
            Assert.IsTrue(wizard.GetPrivateIdentifierCandidates().Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")));

            //delete the column info to make it a missing reference
            _c1.DeleteInDatabase();

            //now it should gracefully skip over it
            Assert.IsFalse(wizard.GetPrivateIdentifierCandidates().Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")));
            
        }

        [Test]
        public void ProposePrivateIdentifierDatatypes()
        {
            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(null,CatalogueRepository, DataExportRepository);

            var candidates = wizard.GetPrivateIdentifierCandidates();

            Assert.IsFalse(candidates.Any(c => c.RuntimeName.Equals("PrivateIdentifierA") || c.RuntimeName.Equals("PrivateIdentifierB")));

            _extractionInfo1.IsExtractionIdentifier = true;
            _extractionInfo1.SaveToDatabase();
            candidates = wizard.GetPrivateIdentifierCandidates();

            var candidate = candidates.Single(c => c.RuntimeName.Equals("PrivateIdentifierA"));
            Assert.AreEqual("varchar(10)", candidate.DataType);
            Assert.IsTrue(candidate.MatchingExtractionInformations.Single().ID== _extractionInfo1.ID);
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestActuallyCreatingIt(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            //drop it
            db.ForceDrop();

            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(db,CatalogueRepository, DataExportRepository);

            _extractionInfo2.IsExtractionIdentifier = true;
            _extractionInfo2.SaveToDatabase();

            var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
            var ect = wizard.CreateDatabase(
                candidate,
                new ThrowImmediatelyCheckNotifier());

            //database should exist
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(cohortDatabaseName);
            Assert.IsTrue(db.Exists());
            
            //did it create the correct type?
            Assert.AreEqual(type,ect.DatabaseType);

            //the ExternalCohortTable should pass tests
            ect.Check(new ThrowImmediatelyCheckNotifier());
            
            //now try putting someone in it
            //the project it will go under
            var project = new Project(DataExportRepository, "MyProject");
            project.ProjectNumber = 10;
            project.SaveToDatabase();

            //the request to put it under there
            var request = new CohortCreationRequest(project, new CohortDefinition(null, "My cohort", 1, 10, ect), DataExportRepository,"Blah");
            
            //the actual cohort data
            DataTable dt = new DataTable();
            dt.Columns.Add(_extractionInfo2.GetRuntimeName());
            dt.Rows.Add(101243); //_extractionInfo2 is of type int

            //the destination component that will put it there
            var dest = new BasicCohortDestination();

            dest.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            
            //tell it to use the guid allocator
            dest.ReleaseIdentifierAllocator = typeof (GuidReleaseIdentifierAllocator);
            
            dest.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

            var cohort = request.CohortCreatedIfAny;
            Assert.IsNotNull(cohort);
        }
    }
}
