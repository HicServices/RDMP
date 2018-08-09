using System.Data;
using System.IO;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.DataFlowPipeline.Destinations;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.PipelineUseCases
{
    /// <summary>
    /// Describes the use case of uploading a <see cref="FileInfo"/> to a target database server.  Compatible pipelines for achieving this must have a destination
    /// of (or inheriting from) <see cref="DataTableUploadDestination"/> and a source that implements IPipelineRequirement&lt;FlatFileToLoad&gt;.
    /// </summary>
    public class UploadFileUseCase:PipelineUseCase
    {
        public UploadFileUseCase(FileInfo file, DiscoveredDatabase targetDatabase)
        {
            AddInitializationObject(new FlatFileToLoad(file));
            AddInitializationObject(targetDatabase);;
        }

        protected override IDataFlowPipelineContext GenerateContext()
        {
            var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
            context.MustHaveDestination = typeof(DataTableUploadDestination);
            return context;
        }
        
        private UploadFileUseCase():base(new []
        {
            typeof (FlatFileToLoad),
                typeof (DiscoveredDatabase)
        })
        {
            
        }

        public static PipelineUseCase DesignTime()
        {
            return new UploadFileUseCase();
        }
    }
}