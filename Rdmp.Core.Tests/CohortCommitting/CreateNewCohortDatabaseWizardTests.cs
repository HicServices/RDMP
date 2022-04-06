// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Settings;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCommitting
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
        protected override void SetUp()
        {
            base.SetUp();

            RunBlitzDatabases(RepositoryLocator);

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

        
        [Test]
        public void TestMissingColumnInfos()
        {
            _extractionInfo1.IsExtractionIdentifier = true;
            _extractionInfo1.SaveToDatabase();
            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(null,CatalogueRepository, DataExportRepository,false);

            //it finds it!
            Assert.IsTrue(wizard.GetPrivateIdentifierCandidates().Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")));

            //delete the column info to make it a missing reference
            _c1.DeleteInDatabase();
            
            //now it should gracefully skip over it
            Assert.IsFalse(wizard.GetPrivateIdentifierCandidates()
                .Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")));
            
        }

        [Test]
        public void ProposePrivateIdentifierDatatypes()
        {
            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(null,CatalogueRepository, DataExportRepository,false);

            var candidates = wizard.GetPrivateIdentifierCandidates();

            Assert.IsFalse(candidates.Any(c => c.RuntimeName.Equals("PrivateIdentifierA") || c.RuntimeName.Equals("PrivateIdentifierB")));

            _extractionInfo1.IsExtractionIdentifier = true;
            _extractionInfo1.SaveToDatabase();
            candidates = wizard.GetPrivateIdentifierCandidates();

            var candidate = candidates.Single(c => c.RuntimeName.Equals("PrivateIdentifierA"));
            Assert.AreEqual("varchar(10)", candidate.DataType);
            Assert.IsTrue(candidate.MatchingExtractionInformations.Single().ID== _extractionInfo1.ID);
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void TestActuallyCreatingIt(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(db,CatalogueRepository, DataExportRepository,false);

            _extractionInfo2.IsExtractionIdentifier = true;
            _extractionInfo2.SaveToDatabase();

            var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
            var ect = wizard.CreateDatabase(
                candidate,
                new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(type,ect.DatabaseType);

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

            var externalData = cohort.GetExternalData();
            Assert.AreEqual(10,externalData.ExternalProjectNumber);
            Assert.IsFalse(string.IsNullOrEmpty(externalData.ExternalDescription));


            Assert.AreEqual(DateTime.Now.Year, externalData.ExternalCohortCreationDate.Value.Year);
            Assert.AreEqual(DateTime.Now.Month, externalData.ExternalCohortCreationDate.Value.Month);
            Assert.AreEqual(DateTime.Now.Day,  externalData.ExternalCohortCreationDate.Value.Day);
            Assert.AreEqual(DateTime.Now.Hour, externalData.ExternalCohortCreationDate.Value.Hour);

            cohort.AppendToAuditLog("Test");
            
            Assert.IsTrue(cohort.AuditLog.Contains("Test"));

            Assert.AreEqual(1,cohort.Count); 
            Assert.AreEqual(1,cohort.CountDistinct);

            var cohortTable = cohort.FetchEntireCohort();

            Assert.AreEqual(1,cohortTable.Rows.Count);

            var helper = ect.GetQuerySyntaxHelper();

            Assert.AreEqual(101243, cohortTable.Rows[0][helper.GetRuntimeName(ect.PrivateIdentifierField)]);
            var aguid = cohortTable.Rows[0][helper.GetRuntimeName(ect.ReleaseIdentifierField)].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(aguid)); //should be a guid

            //test reversing the anonymisation of something
            var dtAno = new DataTable();
            dtAno.Columns.Add(cohort.GetReleaseIdentifier(true));
            dtAno.Columns.Add("Age");
            dtAno.Rows.Add(aguid, 23);
            dtAno.Rows.Add(aguid, 99);

            cohort.ReverseAnonymiseDataTable(dtAno, new ThrowImmediatelyDataLoadEventListener(), true);

            Assert.AreEqual(2, dtAno.Columns.Count);
            Assert.IsTrue(dtAno.Columns.Contains(cohort.GetPrivateIdentifier(true)));

            Assert.AreEqual("101243", dtAno.Rows[0][cohort.GetPrivateIdentifier(true)]);
            Assert.AreEqual("101243", dtAno.Rows[1][cohort.GetPrivateIdentifier(true)]);

            //make sure that it shows up in the child provider (provides fast object access in CLI and builds tree model for UI)
            var repo = new DataExportChildProvider(RepositoryLocator, null,new ThrowImmediatelyCheckNotifier(),null);
            var descendancy = repo.GetDescendancyListIfAnyFor(cohort);
            Assert.IsNotNull(descendancy);
        }

        [Test]
        public void Test_IdentifiableExtractions()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            CreateNewCohortDatabaseWizard wizard = new CreateNewCohortDatabaseWizard(db,CatalogueRepository, DataExportRepository,false);

            _extractionInfo2.IsExtractionIdentifier = true;
            _extractionInfo2.SaveToDatabase();

            var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
            var ect = wizard.CreateDatabase(
                candidate,
                new ThrowImmediatelyCheckNotifier());

            ect.Check(new ThrowImmediatelyCheckNotifier());

            ect.ReleaseIdentifierField = ect.PrivateIdentifierField;
            ect.SaveToDatabase();
            
            UserSettings.AllowIdentifiableExtractions = false;

            var ex = Assert.Throws<Exception>(()=>ect.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("cohort will extract IDENTIFIABLE data",ex.Message);

            UserSettings.AllowIdentifiableExtractions = true;

            ect.Check(new ThrowImmediatelyCheckNotifier());

            UserSettings.AllowIdentifiableExtractions = false;

        }
    }
}
