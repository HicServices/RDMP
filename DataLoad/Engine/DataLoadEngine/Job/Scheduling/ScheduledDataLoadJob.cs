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
    /// <summary>
    /// DataLoadJob that is part of an ongoing data load where only specific dates are loaded.  Typically this involves advancing the head of a LoadProgress
    /// (e.g. 'Load the next 5 days of LoadProgress - Tayside Biochemistry Load').
    /// </summary>
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