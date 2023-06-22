// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Cohort;

public class CommittingNewCohortsTests : TestsRequiringACohort
{
    private string _filename;
    private const string ProjName = "MyProj";

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        using var con = _cohortDatabase.Server.GetConnection();
        con.Open();
        EmptyCohortTables(con);

        _filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "CommittingNewCohorts.csv");

        var sw = new StreamWriter(_filename);    
        sw.WriteLine("PrivateID,ReleaseID,SomeHeader");
        sw.WriteLine("Priv_1111,Pub_1111,Smile buddy");
        sw.WriteLine("Priv_2222,Pub_2222,Your on tv");
        sw.WriteLine("Priv_3333,Pub_3333,Smile buddy");
        sw.Close();
    }


    [Test]
    public void CommittingNewCohortFile_IDPopulated_Throws()
    {
        var proj = new Project(DataExportRepository, ProjName);

        var request = new CohortCreationRequest(proj, new CohortDefinition(511, "CommittingNewCohorts",1,999,_externalCohortTable), DataExportRepository, "fish");
        var ex = Assert.Throws<Exception>(()=>request.Check(ThrowImmediatelyCheckNotifier.Quiet()));
        Assert.AreEqual("Expected the cohort definition CommittingNewCohorts(Version 1, ID=511) to have a null ID - we are trying to create this, why would it already exist?",ex.Message);
    }

    [Test]
    public void CommittingNewCohortFile_ProjectNumberNumberMissing()
    {
        var proj = new Project(DataExportRepository, ProjName);

        var request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), DataExportRepository, "fish");
        var ex = Assert.Throws<Exception>(()=>request.Check(ThrowImmediatelyCheckNotifier.Quiet()));
        Assert.AreEqual("Project MyProj does not have a ProjectNumber specified, it should have the same number as the CohortCreationRequest (999)",ex.Message);
    }

    [Test]
    public void CommittingNewCohortFile_ProjectNumberMismatch()
    {
        var proj = new Project(DataExportRepository, ProjName) {ProjectNumber = 321};
        proj.SaveToDatabase();

        var request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), DataExportRepository, "fish");
        var ex = Assert.Throws<Exception>(()=>request.Check(ThrowImmediatelyCheckNotifier.Quiet()));
        Assert.AreEqual("Project MyProj has ProjectNumber=321 but the CohortCreationRequest.ProjectNumber is 999",ex.Message);
    }

    [Test]
    public void CommittingNewCohortFile_CallPipeline()
    {
        var listener = ThrowImmediatelyDataLoadEventListener.Quiet;

        var proj = new Project(DataExportRepository, ProjName)
        {
            ProjectNumber = 999
        };
        proj.SaveToDatabase();

        var request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), DataExportRepository, "fish");
        request.Check(ThrowImmediatelyCheckNotifier.Quiet());

        var source = new DelimitedFlatFileDataFlowSource();
        var destination = new BasicCohortDestination();
            
        source.Separator = ",";
        source.StronglyTypeInput = true;
            
        var pipeline = new DataFlowPipelineEngine<DataTable>((DataFlowPipelineContext<DataTable>) request.GetContext(),source,destination,listener);
        pipeline.Initialize(new FlatFileToLoad(new FileInfo(_filename)),request);
        pipeline.ExecutePipeline(new GracefulCancellationToken());

        //there should be a new ExtractableCohort now
        Assert.NotNull(request.NewCohortDefinition.ID);

        var ec = DataExportRepository.GetAllObjects<ExtractableCohort>().Single(c => c.OriginID == request.NewCohortDefinition.ID);

        //with the data in it from the test file
        Assert.AreEqual(ec.Count,3);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DeprecateOldCohort(bool deprecate)
    {
        var proj = new Project(DataExportRepository, ProjName)
        {
            ProjectNumber = 999
        };
        proj.SaveToDatabase();

        // we are replacing this imaginary cohort
        var definition998 = new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable);
        // with this one (v2)
        var definition999 = new CohortDefinition(null, "CommittingNewCohorts", 2, 999, _externalCohortTable);
            
        // Create a basic cohort first
        var request1 = new CohortCreationRequest(proj, definition998, DataExportRepository, "fish");
        request1.Check(ThrowImmediatelyCheckNotifier.Quiet());

        using var con = _cohortDatabase.Server.GetManagedConnection();
        request1.PushToServer(con);
        request1.ImportAsExtractableCohort(deprecate, false);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort998 = request1.CohortCreatedIfAny;
        Assert.IsNotNull(cohort998);
        Assert.IsFalse(cohort998.IsDeprecated);

        // define that the new definition attempts to replace the old one
        definition999.CohortReplacedIfAny = cohort998;

        var request2 = new CohortCreationRequest(proj, definition999, DataExportRepository, "fish");
        request2.Check(ThrowImmediatelyCheckNotifier.Quiet());
        request2.PushToServer(con);
        request2.ImportAsExtractableCohort(deprecate, false);

        // after committing the new cohort the old one should be deprecated?
        cohort998.RevertToDatabaseState();
        Assert.AreEqual(deprecate, cohort998.IsDeprecated);
    }


    [TestCase(true)]
    [TestCase(false)]
    public void MigrateUsages(bool migrate)
    {
        var proj = new Project(DataExportRepository, ProjName)
        {
            ProjectNumber = 999
        };
        proj.SaveToDatabase();

        // we are replacing this imaginary cohort
        var definition998 = new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable);
        // with this one (v2)
        var definition999 = new CohortDefinition(null, "CommittingNewCohorts", 2, 999, _externalCohortTable);

        // Create a basic cohort first
        var request1 = new CohortCreationRequest(proj, definition998, DataExportRepository, "fish");
        request1.Check(ThrowImmediatelyCheckNotifier.Quiet());

        using var con = _cohortDatabase.Server.GetManagedConnection();
        request1.PushToServer(con);
        request1.ImportAsExtractableCohort(true, migrate);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort998 = request1.CohortCreatedIfAny;
        Assert.IsNotNull(cohort998);
        Assert.IsFalse(cohort998.IsDeprecated);

        // legit user 1
        var ec1 = new ExtractionConfiguration(DataExportRepository, proj)
        {
            IsReleased = false,
            Cohort_ID = cohort998.ID
        };
        ec1.SaveToDatabase();

        // legit user 2
        var ec2 = new ExtractionConfiguration(DataExportRepository,proj)
        {
            IsReleased = false,
            Cohort_ID = cohort998.ID
        };
        ec2.SaveToDatabase();

        // has no cohort yet defined so should not be migrated
        var ec3 = new ExtractionConfiguration(DataExportRepository,proj);

        // is frozen so should not be migrated
        var ec4 = new ExtractionConfiguration(DataExportRepository,proj)
        {
            IsReleased = true,
            Cohort_ID = cohort998.ID
        };
        ec4.SaveToDatabase();

        // define that the new definition attempts to replace the old one
        definition999.CohortReplacedIfAny = cohort998;

        var request2 = new CohortCreationRequest(proj, definition999, DataExportRepository, "fish");
        request2.Check(ThrowImmediatelyCheckNotifier.Quiet());
        request2.PushToServer(con);
        request2.ImportAsExtractableCohort(true, migrate);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort999 = request2.CohortCreatedIfAny;
        Assert.IsNotNull(cohort999);

        // after committing the new cohort who should be migrated?
        ec1.RevertToDatabaseState();
        ec2.RevertToDatabaseState();
        ec3.RevertToDatabaseState();
        ec4.RevertToDatabaseState();

        // should have been updated to use the new cohort
        Assert.AreEqual(ec1.Cohort_ID, migrate ? cohort999.ID : cohort998.ID);
        Assert.AreEqual(ec2.Cohort_ID, migrate ? cohort999.ID: cohort998.ID);

        // should not have magically gotten a cohort
        Assert.IsNull(ec3.Cohort_ID);

        // is frozen so should not have been changed to the new cohort (and therefore still use cohort998)
        Assert.AreEqual(ec4.Cohort_ID, cohort998.ID);
    }
}