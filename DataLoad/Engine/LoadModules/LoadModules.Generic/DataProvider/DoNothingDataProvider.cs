using System;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider
{
    public class DoNothingDataProvider : IDataProvider
    {
        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,"DoNothingDataProvider did nothing!"));
            return ExitCodeType.Success;
            
        }
        
        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }

        
        public void Check(ICheckNotifier notifier)
        {
            
        }
    }
}
