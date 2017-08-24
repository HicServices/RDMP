using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public interface IRuntimeTask : IProcessTask, IDataLoadComponent
    {
        bool Exists();
        void Abort(IDataLoadEventListener postLoadEventListener);

        RuntimeArgumentCollection RuntimeArguments { get; }
    }
}