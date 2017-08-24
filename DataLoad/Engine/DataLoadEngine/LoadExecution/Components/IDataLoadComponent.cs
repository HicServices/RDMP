using DataLoadEngine.Job;

namespace DataLoadEngine.LoadExecution.Components
{
    public interface IDataLoadComponent : ILoadComponent<IDataLoadJob>, IDisposeAfterDataLoad
    {
        
    }
}