using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// Basic IJobFactory for creating an 'OnDemand', one off, self contained (not date based) IDataLoadJob.
    /// </summary>
    public class JobFactory : IJobFactory
    {
        private readonly ILoadMetadata _loadMetadata;
        private readonly ILogManager _logManager;

        public JobFactory(ILoadMetadata loadMetadata, ILogManager logManager)
        {
            _loadMetadata = loadMetadata;
            _logManager = logManager;
        }

        public IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener)
        {
            var description = _loadMetadata.Name;
            var hicProjectDirectory = new HICProjectDirectory(_loadMetadata.LocationOfFlatFiles, false);
            return new DataLoadJob(repositoryLocator,description, _logManager, _loadMetadata, hicProjectDirectory, listener);
        }
    }
}