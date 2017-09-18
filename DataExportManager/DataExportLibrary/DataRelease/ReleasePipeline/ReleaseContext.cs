using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public static class ReleaseContext
    {
        public static DataFlowPipelineContext<ReleaseData> Context { get; set; }

        static ReleaseContext()
        {
            var contextFactory = new DataFlowPipelineContextFactory<ReleaseData>();
            Context = contextFactory.Create(PipelineUsage.None);
            Context.CannotHave.Add(typeof(IDataFlowSource<ReleaseData>));

            Context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseData>);   
        }   
    }
}