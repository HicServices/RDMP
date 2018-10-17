using System;
using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
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
        
        public ScheduledDataLoadJob(IRDMPPlatformRepositoryServiceLocator repositoryLocator, string description, ILogManager logManager, ILoadMetadata loadMetadata, IHICProjectDirectory hicProjectDirectory, IDataLoadEventListener listener,HICDatabaseConfiguration configuration)
            : base(repositoryLocator,description, logManager, loadMetadata, hicProjectDirectory, listener,configuration)
        {
        }
    }
}