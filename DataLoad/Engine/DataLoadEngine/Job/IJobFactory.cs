using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// Creates the DataLoadJob which will run in a given DataLoadProcess (either one off load or an iterative load of a specific range of dates - See LoadProgress).
    /// </summary>
    public interface IJobFactory
    {
        IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator,IDataLoadEventListener listener);
    }
}