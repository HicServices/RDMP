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
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCommitting;

public class CreateNewCohortDatabaseWizardTests : DatabaseTests
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
        var wizard = new CreateNewCohortDatabaseWizard(null, CatalogueRepository, DataExportRepository, false);

        //it finds it!
        Assert.That(wizard.GetPrivateIdentifierCandidates()
            .Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")));

        //delete the column info to make it a missing reference
        _c1.DeleteInDatabase();

        //now it should gracefully skip over it
        Assert.That(wizard.GetPrivateIdentifierCandidates()
            .Any(prototype => prototype.RuntimeName.Equals("PrivateIdentifierA")), Is.False);
    }

    [Test]
    public void ProposePrivateIdentifierDatatypes()
    {
        var wizard = new CreateNewCohortDatabaseWizard(null, CatalogueRepository, DataExportRepository, false);

        var candidates = wizard.GetPrivateIdentifierCandidates();

        Assert.That(candidates.Any(c =>
            c.RuntimeName.Equals("PrivateIdentifierA") || c.RuntimeName.Equals("PrivateIdentifierB")), Is.False);

        _extractionInfo1.IsExtractionIdentifier = true;
        _extractionInfo1.SaveToDatabase();
        candidates = wizard.GetPrivateIdentifierCandidates();

        var candidate = candidates.Single(c => c.RuntimeName.Equals("PrivateIdentifierA"));
        Assert.Multiple(() =>
        {
            Assert.That(candidate.DataType, Is.EqualTo("varchar(10)"));
            Assert.That(candidate.MatchingExtractionInformations.Single().ID, Is.EqualTo(_extractionInfo1.ID));
        });
    }

    [TestCase("text")]
    //[TestCase("ntext")] // TODO: FAnsiSql doesn't know that this is max width
    [TestCase("varchar(max)")]
    [TestCase("nvarchar(max)")]
    public void TestVarcharMaxNotAllowed(string term)
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var wizard = new CreateNewCohortDatabaseWizard(db, CatalogueRepository, DataExportRepository, false);

        _extractionInfo2.IsExtractionIdentifier = true;
        _c2.Data_type = term;
        _c2.SaveToDatabase();

        _extractionInfo2.SaveToDatabase();

        var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
        var ex = Assert.Throws<Exception>(() => wizard.CreateDatabase(
            candidate,
            ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(
            ex.Message, Is.EqualTo("Private identifier datatype cannot be varchar(max) style as this prevents Primary Key creation on the table"));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestActuallyCreatingIt(DatabaseType type)
    {
        var db = GetCleanedServer(type);

        var wizard = new CreateNewCohortDatabaseWizard(db, CatalogueRepository, DataExportRepository, false);

        _extractionInfo2.IsExtractionIdentifier = true;
        _extractionInfo2.SaveToDatabase();

        var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
        var ect = wizard.CreateDatabase(
            candidate,
            ThrowImmediatelyCheckNotifier.Quiet);

        Assert.That(ect.DatabaseType, Is.EqualTo(type));

        //database should exist
        DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(cohortDatabaseName);
        Assert.Multiple(() =>
        {
            Assert.That(db.Exists());

            //did it create the correct type?
            Assert.That(ect.DatabaseType, Is.EqualTo(type));
        });

        //the ExternalCohortTable should pass tests
        ect.Check(ThrowImmediatelyCheckNotifier.Quiet);

        //now try putting someone in it
        //the project it will go under
        var project = new Project(DataExportRepository, "MyProject")
        {
            ProjectNumber = 10
        };
        project.SaveToDatabase();

        //the request to put it under there
        var request = new CohortCreationRequest(project, new CohortDefinition(null, "My cohort", 1, 10, ect),
            DataExportRepository, "Blah");

        //the actual cohort data
        var dt = new DataTable();
        dt.Columns.Add(_extractionInfo2.GetRuntimeName());
        dt.Rows.Add(101243); //_extractionInfo2 is of type int

        //the destination component that will put it there
        var dest = new BasicCohortDestination();

        dest.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);

        //tell it to use the guid allocator
        dest.ReleaseIdentifierAllocator = typeof(GuidReleaseIdentifierAllocator);

        dest.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        dest.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

        var cohort = request.CohortCreatedIfAny;
        Assert.That(cohort, Is.Not.Null);

        var externalData = cohort.GetExternalData();
        Assert.Multiple(() =>
        {
            Assert.That(externalData.ExternalProjectNumber, Is.EqualTo(10));
            Assert.That(string.IsNullOrEmpty(externalData.ExternalDescription), Is.False);


            Assert.That(externalData.ExternalCohortCreationDate.Value.Year, Is.EqualTo(DateTime.Now.Year));
            Assert.That(externalData.ExternalCohortCreationDate.Value.Month, Is.EqualTo(DateTime.Now.Month));
            Assert.That(externalData.ExternalCohortCreationDate.Value.Day, Is.EqualTo(DateTime.Now.Day));
            Assert.That(externalData.ExternalCohortCreationDate.Value.Hour, Is.EqualTo(DateTime.Now.Hour));
        });

        cohort.AppendToAuditLog("Test");

        Assert.Multiple(() =>
        {
            Assert.That(cohort.AuditLog, Does.Contain("Test"));

            Assert.That(cohort.Count, Is.EqualTo(1));
        });
        Assert.That(cohort.CountDistinct, Is.EqualTo(1));

        var cohortTable = cohort.FetchEntireCohort();

        Assert.That(cohortTable.Rows, Has.Count.EqualTo(1));

        var helper = ect.GetQuerySyntaxHelper();

        Assert.That(cohortTable.Rows[0][helper.GetRuntimeName(ect.PrivateIdentifierField)], Is.EqualTo(101243));
        var aguid = cohortTable.Rows[0][helper.GetRuntimeName(ect.ReleaseIdentifierField)].ToString();
        Assert.That(string.IsNullOrWhiteSpace(aguid), Is.False); //should be a guid

        //test reversing the anonymisation of something
        var dtAno = new DataTable();
        dtAno.Columns.Add(cohort.GetReleaseIdentifier(true));
        dtAno.Columns.Add("Age");
        dtAno.Rows.Add(aguid, 23);
        dtAno.Rows.Add(aguid, 99);

        cohort.ReverseAnonymiseDataTable(dtAno, ThrowImmediatelyDataLoadEventListener.Quiet, true);

        Assert.That(dtAno.Columns, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(dtAno.Columns.Contains(cohort.GetPrivateIdentifier(true)));

            Assert.That(dtAno.Rows[0][cohort.GetPrivateIdentifier(true)], Is.EqualTo("101243"));
            Assert.That(dtAno.Rows[1][cohort.GetPrivateIdentifier(true)], Is.EqualTo("101243"));
        });

        //make sure that it shows up in the child provider (provides fast object access in CLI and builds tree model for UI)
        var repo = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);
        var descendancy = repo.GetDescendancyListIfAnyFor(cohort);
        Assert.That(descendancy, Is.Not.Null);
    }

    [Test]
    public void Test_IdentifiableExtractions()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var wizard = new CreateNewCohortDatabaseWizard(db, CatalogueRepository, DataExportRepository, false);

        _extractionInfo2.IsExtractionIdentifier = true;
        _extractionInfo2.SaveToDatabase();

        var candidate = wizard.GetPrivateIdentifierCandidates().Single(c => c.RuntimeName.Equals("PrivateIdentifierB"));
        var ect = wizard.CreateDatabase(
            candidate,
            ThrowImmediatelyCheckNotifier.Quiet);

        ect.Check(ThrowImmediatelyCheckNotifier.Quiet);

        ect.ReleaseIdentifierField = ect.PrivateIdentifierField;
        ect.SaveToDatabase();

        UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Fail);

        var ex = Assert.Throws<Exception>(() => ect.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(
            ex.Message, Is.EqualTo("R004 PrivateIdentifierField and ReleaseIdentifierField are the same, this means your cohort will extract identifiable data (no cohort identifier substitution takes place)"));

        UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Warning);

        ect.Check(ThrowImmediatelyCheckNotifier.Quiet);

        UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Fail);
    }
}