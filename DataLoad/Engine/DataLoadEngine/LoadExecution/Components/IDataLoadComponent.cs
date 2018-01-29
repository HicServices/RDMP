using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;

namespace DataLoadEngine.LoadExecution.Components
{
    /// <summary>
    /// A discrete step in DLE execution either configured by the user (ProcessTask=>RuntimeTask) or a fixed step e.g. MigrateRAWTableToStaging.  See
    /// DataLoadEngine.cd for how all the various components interact in the larger scheme of the DLE IDataLoadExecution.
    /// </summary>
    public interface IDataLoadComponent : IDisposeAfterDataLoad
    {
        bool SkipComponent { get; }
        string Description { get; }
        ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken);
    }
}