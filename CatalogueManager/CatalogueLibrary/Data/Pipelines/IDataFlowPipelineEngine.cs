using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IDataFlowPipelineEngine : ICheckable
    {
        void ExecutePipeline(GracefulCancellationToken cancellationToken);
        bool ExecuteSinglePass(GracefulCancellationToken cancellationToken);

        void Initialize(params object[] initializationObjects);

        List<object> ComponentObjects { get; }
        object DestinationObject { get; }
        object SourceObject { get; }
        object ContextObject { get; }
        
    }
}