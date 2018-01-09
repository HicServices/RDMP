using System;
using CatalogueLibrary;
using DataLoadEngine.Job.Scheduling;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.LoadProgressUpdating
{
    /// <summary>
    /// UpdateProgressIfLoadsuccessful which executes an arbitrary Func in order to determine what date to update the LoadProgress.DataLoadProgress to 
    /// (See UpdateProgressIfLoadsuccessful).
    /// </summary>
    public class UpdateProgressToResultOfDelegate : UpdateProgressIfLoadsuccessful
    {
        private readonly Func<DateTime> _delegateToRun;
        
        public UpdateProgressToResultOfDelegate(ScheduledDataLoadJob job, Func<DateTime> delegateToRun) : base(job)
        {
            _delegateToRun = delegateToRun;
            DateToSetProgressTo = DateTime.MinValue;
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            if (exitCode == ExitCodeType.Success)
                DateToSetProgressTo = _delegateToRun();

            base.LoadCompletedSoDispose(exitCode, postLoadEventListener);

        }
    }
}