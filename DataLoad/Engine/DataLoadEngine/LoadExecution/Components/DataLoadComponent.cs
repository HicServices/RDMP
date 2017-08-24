using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    public abstract class DataLoadComponent : IDataLoadComponent
    {
        public bool SkipComponent { get; set; }
        public string Description { get; set; }
        
        public abstract ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken);

        protected DataLoadComponent()
        {
            SkipComponent = false;
        }

        protected bool Skip(IDataLoadJob job)
        {
            if (!SkipComponent) return false;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Skipped load component: " + Description));
            return true;
        }


        public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
        }
    }
}