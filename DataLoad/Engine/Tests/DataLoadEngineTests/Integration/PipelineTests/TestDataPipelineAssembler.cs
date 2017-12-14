using System;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using DataLoadEngineTests.Integration.Cache;

namespace DataLoadEngineTests.Integration.PipelineTests
{
    public class TestDataPipelineAssembler
    {
        public PipelineComponent Destination { get; set; }
        public PipelineComponent Source { get; set; }
        public Pipeline Pipeline { get; set; }

        public TestDataPipelineAssembler(string pipeName, CatalogueRepository catalogueRepository)
        {
            Pipeline = new Pipeline(catalogueRepository, pipeName);
            Source = new PipelineComponent(catalogueRepository, Pipeline, typeof(TestDataInventor), 1, "DataInventorSource");
            Destination = new PipelineComponent(catalogueRepository, Pipeline, typeof(TestDataWriter), 2, "DataInventorDestination");

            Destination.CreateArgumentsForClassIfNotExists<TestDataWriter>();
            
            Pipeline.SourcePipelineComponent_ID = Source.ID;
            Pipeline.DestinationPipelineComponent_ID = Destination.ID;
            Pipeline.SaveToDatabase();
        }

        public void ConfigureCacheProgressToUseThePipeline(CacheProgress cp)
        {
            cp.Pipeline_ID = Pipeline.ID;
            cp.ChunkPeriod = new TimeSpan(12, 0, 0);
            cp.SaveToDatabase();
        }


        public void Destroy()
        {
            Pipeline.DeleteInDatabase();
        }
    }
}
