using CatalogueLibrary.Data.Pipelines;

namespace CachingEngine.Locking
{
    /// <summary>
    /// Determines whether a given IDataFlowPipelineEngine can be executed or whether one of it's subcomponents is locked (e.g. because it is already running).
    /// </summary>
    public interface IEngineLockProvider
    {
        bool IsLocked(IDataFlowPipelineEngine engine);
        string Details(IDataFlowPipelineEngine engine);
    }
}