using System;
using CatalogueLibrary;
using DataLoadEngine;
using DataLoadEngine.Job.Scheduling;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.LoadProgressUpdating
{
    public class UpdateProgressToSpecificValueIfLoadsuccessful : UpdateProgressIfLoadsuccessful
    {
        public UpdateProgressToSpecificValueIfLoadsuccessful(ScheduledDataLoadJob job, DateTime specificValue) : base(job)
        {
            DateToSetProgressTo = specificValue;
        }
    }
}