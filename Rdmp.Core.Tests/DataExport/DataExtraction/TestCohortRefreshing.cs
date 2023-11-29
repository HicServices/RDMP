// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class TestCohortRefreshing : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void RefreshCohort()
    {
        var pipe = SetupPipeline();
        pipe.Name = "RefreshPipe";
        pipe.SaveToDatabase();

        Execute(out _, out _);

        var oldcohort = _configuration.Cohort;


        _configuration.CohortIdentificationConfiguration_ID =
            new CohortIdentificationConfiguration(RepositoryLocator.CatalogueRepository, "RefreshCohort.cs").ID;
        _configuration.CohortRefreshPipeline_ID = pipe.ID;
        _configuration.SaveToDatabase();

        var engine = new CohortRefreshEngine(ThrowImmediatelyDataLoadEventListener.Quiet, _configuration);

        Assert.That(engine.Request.NewCohortDefinition, Is.Not.Null);

        var oldData = oldcohort.GetExternalData();

        engine.Request.NewCohortDefinition.CohortReplacedIfAny = oldcohort;

        Assert.That(engine.Request.NewCohortDefinition.Description, Is.EqualTo(oldData.ExternalDescription));
        Assert.That(engine.Request.NewCohortDefinition.Version, Is.EqualTo(oldData.ExternalVersion + 1));
    }

    /// <summary>
    /// This is a giant scenario test in which we create a cohort of 5 people and a dataset with a single row with 1 person in it and a result field (the basic setup for
    /// TestsRequiringAnExtractionConfiguration).
    /// 
    /// <para>1.We run the extraction.
    /// 2.We create a cohort refresh query that pulls the 1 dude from the above single row table
    /// 3.We configure a query caching server which the cohort query is setup to use so that after executing the sql to identify the person it will cache the identifier list (of 1)
    /// 4.We then the ExtractionConfiguration that its refresh pipeline is a cohort query builder query and build a pipeline for executing the cic and using basic cohort destination
    /// 5.We then run the refresh pipeline which should execute the cic and cache the record and commit it as a new version of cohort for the ExtractionConfiguration
    /// 6.We then truncate the live table, this will result in the cic returning nobody
    /// 7.Without touching the cache we run the cohort refresh pipeline again</para>
    /// 
    /// <para>Thing being tested: After 7 we are confirming that the refresh failed because there was nobody identified by the query, furthermore we then test that the progress messages sent
    /// included an explicit message about clearing the cache</para>
    /// </summary>
    [Test]
    public void RefreshCohort_WithCaching()
    {
        var pipe = new Pipeline(CatalogueRepository, "RefreshPipeWithCaching");

        var source =
            new PipelineComponent(CatalogueRepository, pipe, typeof(CohortIdentificationConfigurationSource), 0);
        var args = source.CreateArgumentsForClassIfNotExists<CohortIdentificationConfigurationSource>();
        var freezeArg = args.Single(a => a.Name.Equals("FreezeAfterSuccessfulImport"));
        freezeArg.SetValue(false);
        freezeArg.SaveToDatabase();

        var dest = new PipelineComponent(CatalogueRepository, pipe, typeof(BasicCohortDestination), 0);
        var argsDest = dest.CreateArgumentsForClassIfNotExists<BasicCohortDestination>();
        var allocatorArg = argsDest.Single(a => a.Name.Equals("ReleaseIdentifierAllocator"));
        allocatorArg.SetValue(null);
        allocatorArg.SaveToDatabase();

        pipe.SourcePipelineComponent_ID = source.ID;
        pipe.DestinationPipelineComponent_ID = dest.ID;
        pipe.SaveToDatabase();

        Execute(out _, out _);

        var oldcohort = _configuration.Cohort;

        //Create a query cache
        var p = new QueryCachingPatcher();
        var queryCacheServer = new ExternalDatabaseServer(CatalogueRepository, "TestCohortRefreshing_CacheTest", p);

        var cachedb =
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("TestCohortRefreshing_CacheTest");
        if (cachedb.Exists())
            cachedb.Drop();

        new MasterDatabaseScriptExecutor(cachedb).CreateAndPatchDatabase(p, ThrowImmediatelyCheckNotifier.Quiet);
        queryCacheServer.SetProperties(cachedb);

        //Create a Cohort Identification configuration (query) that will identify the cohort
        var cic = new CohortIdentificationConfiguration(RepositoryLocator.CatalogueRepository, "RefreshCohort.cs");

        try
        {
            //make it use the cache
            cic.QueryCachingServer_ID = queryCacheServer.ID;
            cic.SaveToDatabase();

            //give it a single table query to fetch distinct chi from test data
            var agg = cic.CreateNewEmptyConfigurationForCatalogue(_catalogue, null);

            //add the sub query as the only entry in the cic (in the root container)
            cic.CreateRootContainerIfNotExists();
            cic.RootCohortAggregateContainer.AddChild(agg, 1);

            //make the ExtractionConfiguration refresh cohort query be the cic
            _configuration.CohortIdentificationConfiguration_ID = cic.ID;
            _configuration.CohortRefreshPipeline_ID = pipe.ID;
            _configuration.SaveToDatabase();

            //get a refreshing engine
            var engine = new CohortRefreshEngine(ThrowImmediatelyDataLoadEventListener.Quiet, _configuration);
            engine.Execute();

            Assert.That(engine.Request.NewCohortDefinition, Is.Not.Null);

            var oldData = oldcohort.GetExternalData();

            Assert.That(engine.Request.NewCohortDefinition.Description, Is.EqualTo(oldData.ExternalDescription));
            Assert.That(engine.Request.NewCohortDefinition.Version, Is.EqualTo(oldData.ExternalVersion + 1));

            Assert.That(engine.Request.CohortCreatedIfAny.CountDistinct, Is.Not.EqualTo(oldcohort.CountDistinct));

            //now nuke all data in the catalogue so the cic returns nobody (except that the identifiers are cached eh?)
            DataAccessPortal.ExpectDatabase(_tableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable(_tableInfo.GetRuntimeName()).Truncate();

            var toMem = new ToMemoryDataLoadEventListener(false);

            //get a new engine
            engine = new CohortRefreshEngine(toMem, _configuration);

            //execute it
            var ex = Assert.Throws<PipelineCrashedException>(() => engine.Execute());

            Assert.That(
                ex.InnerException.InnerException.Message.Contains(
                    "CohortIdentificationCriteria execution resulted in an empty dataset"));

            //expected this message to happen
            //that it did clear the cache
            Assert.That(toMem.EventsReceivedBySender.SelectMany(kvp => kvp.Value)
                    .Count(msg => msg.Message.Equals("Clearing Cohort Identifier Cache")), Is.EqualTo(1));
        }
        finally
        {
            //make the ExtractionConfiguration not use the cic query
            _configuration.CohortRefreshPipeline_ID = null;
            _configuration.CohortIdentificationConfiguration_ID = null;
            _configuration.SaveToDatabase();

            //delete the cic query
            cic.QueryCachingServer_ID = null;
            cic.SaveToDatabase();
            cic.DeleteInDatabase();

            //delete the caching database
            queryCacheServer.DeleteInDatabase();
            cachedb.Drop();
        }
    }
}