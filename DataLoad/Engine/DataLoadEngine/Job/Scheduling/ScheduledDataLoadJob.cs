using System;
using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    public class ScheduledDataLoadJob : DataLoadJob
    {
        public ILoadProgress LoadProgress { get; set; }
        public List<DateTime> DatesToRetrieve { get; set; }
        
        public ScheduledDataLoadJob(string description, ILogManager logManager, ILoadMetadata loadMetadata, IHICProjectDirectory hicProjectDirectory, IDataLoadEventListener listener)
            : base(description, logManager, loadMetadata, hicProjectDirectory, listener)
        {
        }
    }
}