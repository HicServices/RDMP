using System;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    public abstract class ScheduledJobFactory : IJobFactory
    {
        protected readonly int? OverrideNumberOfDaysToLoad;
        protected readonly string JobDescription;
        protected readonly ILoadMetadata LoadMetadata;
        protected readonly ILogManager LogManager;

        protected ScheduledJobFactory(int? overrideNumberOfDaysToLoad, ILoadMetadata loadMetadata, ILogManager logManager)
        {
            OverrideNumberOfDaysToLoad = overrideNumberOfDaysToLoad;
            JobDescription = loadMetadata.Name;
            LoadMetadata = loadMetadata;
            LogManager = logManager;
        }

        public abstract IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener);
        public abstract bool HasJobs();
    }
}