using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public interface IRuntimeTask : IDataLoadComponent
    {
        bool Exists();
        void Abort(IDataLoadEventListener postLoadEventListener);

        IProcessTask ProcessTask { get; }

        RuntimeArgumentCollection RuntimeArguments { get; }
    }
}