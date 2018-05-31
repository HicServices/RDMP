using System;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public class ReleaseRunner:IRunner
    {
        private readonly ReleaseOptions _options;

        public ReleaseRunner(ReleaseOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            switch (_options.Command)
            {
                case CommandLineActivity.run:
                    break;
                case CommandLineActivity.check:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0;
        }
    }
}
