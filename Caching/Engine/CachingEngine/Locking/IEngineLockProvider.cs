using CatalogueLibrary.Data.Pipelines;

namespace CachingEngine.Locking
{
    public interface IEngineLockProvider
    {
        bool IsLocked(IDataFlowPipelineEngine engine);
        void Lock(IDataFlowPipelineEngine engine);
        void Unlock(IDataFlowPipelineEngine engine);
        string Details(IDataFlowPipelineEngine engine);
    }
}