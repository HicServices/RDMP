using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataQualityEngine.Reports;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    internal class DqeRunner:IRunner
    {
        private readonly DqeOptions _options;

        public DqeRunner(DqeOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier,GracefulCancellationToken token)
        {
            var catalogue = repositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_options.Catalogue);
            var report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID);
            
            switch (_options.Command)
            {
                case CommandLineActivity.run:
                    report.GenerateReport(catalogue, listener, token.AbortToken);
                    return 0;
                
                case CommandLineActivity.check:
                    report.Check(checkNotifier);
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}