using System;
using System.Linq;
using CatalogueLibrary;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Data Load Engine disposal step for scheduled data loads (See ScheduledDataLoadJob) in which the LoadProgress head pointer date is updated.  E.g. if the 
    /// job was to load 5 days then the LoadProgress.DataLoadProgress date would be updated to reflect the loaded date range.  This is non trivial because it might
    /// be that although the job was to load 100 days the source data read ended after 10 days so you might only want to update the DataLoadProgress date by 10
    /// days on teh assumption that more data will appear later to fill that gap.
    /// </summary>
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