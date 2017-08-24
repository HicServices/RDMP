using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;

namespace DataLoadEngine.LoadExecution.Components
{
    public interface ILoadComponent<T>
    {
        bool SkipComponent { get; }
        string Description { get;  }
        ExitCodeType Run(T job, GracefulCancellationToken cancellationToken);
    }
}