using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// Runner for Cohort Creation Tasks.
    /// </summary>
    public class CohortCreationRunner : IRunner
    {
        private readonly CohortCreationOptions _options;
        private ExtractionConfiguration _configuration;

        public CohortCreationRunner(CohortCreationOptions options)
        {
            _options = options;
            _configuration = _options.GetRepositoryLocator().DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            if (HasConfigurationPreviouslyBeenReleased())
                throw new Exception("Extraction Configuration has already been released");

            if (_options.Command == CommandLineActivity.run)
            {
                var engine = new CohortRefreshEngine(new ThrowImmediatelyDataLoadEventListener(), _configuration);
                engine.Execute();
            }

            return 0;
        }

        private bool HasConfigurationPreviouslyBeenReleased()
        {
            var previouslyReleasedStuff = _configuration.ReleaseLogEntries;

            if (previouslyReleasedStuff.Any())
                return true;

            return false;
        }
    }
}
