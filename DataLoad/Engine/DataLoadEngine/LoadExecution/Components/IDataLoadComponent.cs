using DataLoadEngine.Job;
using MapsDirectlyToDatabaseTable;

namespace DataLoadEngine.LoadExecution.Components
{
    public interface IDataLoadComponent : ILoadComponent<IDataLoadJob>, IDisposeAfterDataLoad,IMapsDirectlyToDatabaseTable
    {
        
    }
}