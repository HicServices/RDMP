using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Options.Abstracts;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// interface for all processes which satiate an <see cref="RDMPCommandLineOptions"/> derrived command by running a given engine on the supplied input objects
    /// </summary>
    public interface IRunner
    {
        int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token);
    }
}