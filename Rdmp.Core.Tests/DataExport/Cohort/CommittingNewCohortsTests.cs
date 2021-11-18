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
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Cohort
{
    public class CommittingNewCohortsTests : TestsRequiringACohort
    {
        private string filename;
        private string projName = "MyProj";

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            EmptyCohortTables();

            filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "CommittingNewCohorts.csv");

            StreamWriter sw = new StreamWriter(filename);    
            sw.WriteLine("PrivateID,ReleaseID,SomeHeader");
            sw.WriteLine("Priv_1111,Pub_1111,Smile buddy");
            sw.WriteLine("Priv_2222,Pub_2222,Your on tv");
            sw.WriteLine("Priv_3333,Pub_3333,Smile buddy");
            sw.Close();
        }


        [Test]
        public void CommittingNewCohortFile_IDPopulated_Throws()
        {
            var proj = new Project(DataExportRepository, projName);

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(511, "CommittingNewCohorts",1,999,_externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            var ex = Assert.Throws<Exception>(()=>request.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Expected the cohort definition CommittingNewCohorts(Version 1, ID=511) to have a null ID - we are trying to create this, why would it already exist?",ex.Message);
        }

        [Test]
        public void CommittingNewCohortFile_ProjectNumberNumberMissing()
        {
            var proj = new Project(DataExportRepository, projName);

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            var ex = Assert.Throws<Exception>(()=>request.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Project MyProj does not have a ProjectNumber specified, it should have the same number as the CohortCreationRequest (999)",ex.Message);
        }

        [Test]
        public void CommittingNewCohortFile_ProjectNumberMismatch()
        {
            var proj = new Project(DataExportRepository, projName) {ProjectNumber = 321};
            proj.SaveToDatabase();

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            var ex = Assert.Throws<Exception>(()=>request.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Project MyProj has ProjectNumber=321 but the CohortCreationRequest.ProjectNumber is 999",ex.Message);
        }

        [Test]
        public void CommittingNewCohortFile_CallPipeline()
        {
            var listener = new ThrowImmediatelyDataLoadEventListener();

            var proj = new Project(DataExportRepository, projName);
            proj.ProjectNumber = 999;
            proj.SaveToDatabase();

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            request.Check(new ThrowImmediatelyCheckNotifier());


            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            BasicCohortDestination destination = new BasicCohortDestination();
            
            source.Separator = ",";
            source.StronglyTypeInput = true;
            
            DataFlowPipelineEngine<DataTable> pipeline = new DataFlowPipelineEngine<DataTable>((DataFlowPipelineContext<DataTable>) request.GetContext(),source,destination,listener);
            pipeline.Initialize(new FlatFileToLoad(new FileInfo(filename)),request);
            pipeline.ExecutePipeline(new GracefulCancellationToken());

            //there should be a new ExtractableCohort now
            Assert.NotNull(request.NewCohortDefinition.ID);

            var ec = DataExportRepository.GetAllObjects<ExtractableCohort>().Single(c => c.OriginID == request.NewCohortDefinition.ID);

            //with the data in it from the test file
            Assert.AreEqual(ec.Count,3);
        }
    }
}
