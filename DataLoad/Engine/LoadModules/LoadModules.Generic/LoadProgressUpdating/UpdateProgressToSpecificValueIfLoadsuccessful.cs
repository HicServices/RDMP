using System;
using CatalogueLibrary;
using DataLoadEngine;
using DataLoadEngine.Job.Scheduling;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.LoadProgressUpdating
{
    /// <summary>
    /// UpdateProgressIfLoadsuccessful which uses a fixed 'specificValue' to update the LoadProgress.DataLoadProgress to 
    /// (See UpdateProgressIfLoadsuccessful).
    /// </summary>
    public class UpdateProgressToSpecificValueIfLoadsuccessful : UpdateProgressIfLoadsuccessful
    {
        public UpdateProgressToSpecificValueIfLoadsuccessful(ScheduledDataLoadJob job, DateTime specificValue) : base(job)
        {
            DateToSetProgressTo = specificValue;
        }
    }
}