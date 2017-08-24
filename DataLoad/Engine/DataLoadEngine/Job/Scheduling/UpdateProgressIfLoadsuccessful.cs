using System;
using System.Linq;
using CatalogueLibrary;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    public class UpdateProgressIfLoadsuccessful : IUpdateLoadProgress
    { 
        protected readonly ScheduledDataLoadJob Job;
        

        public DateTime DateToSetProgressTo;

        public UpdateProgressIfLoadsuccessful(ScheduledDataLoadJob job)
        {
            Job = job;

            if (Job.DatesToRetrieve == null || !Job.DatesToRetrieve.Any())
                throw new DataLoadProgressUpdateException("Job does not have any DatesToRetrieve! collection was null or empty");

            DateToSetProgressTo = Job.DatesToRetrieve.Max();
        }

        virtual public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            if (exitCode != ExitCodeType.Success)
                return;

            var progress = Job.LoadProgress;

            if (progress.DataLoadProgress > DateToSetProgressTo)
                throw new DataLoadProgressUpdateException("Cannot set DataLoadProgress to " + DateToSetProgressTo + " because it is less than the currently recorded progress:"+progress.DataLoadProgress);

            postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Updating DataLoadProgress of '" + progress + "' to " + DateToSetProgressTo));
            progress.DataLoadProgress = DateToSetProgressTo;
            
            progress.SaveToDatabase();
        }
    }
}