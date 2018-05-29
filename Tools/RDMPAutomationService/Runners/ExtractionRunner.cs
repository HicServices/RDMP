using System;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public class ExtractionRunner : IRunner
    {
        private ExtractionOptions _options;

        public ExtractionRunner(ExtractionOptions extractionOpts)
        {
            _options = extractionOpts;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            switch (_options.Command)
            {
                case CommandLineActivity.run:

                    break;
                case CommandLineActivity.check:
                    /*
                     * 
                     * 
                     * if (_extractionConfiguration.Cohort_ID == null)
                throw new Exception("There is no cohort associated with this extraction!");

            if (HasConfigurationPreviouslyBeenReleased(_extractionConfiguration))
            {
                lblAlreadyReleased.Visible = true;
                this.Enabled = false;//disable entire form
            }
            try
            {
                if (configuration.DefaultPipeline_ID != null)
                    _pipelineSelectionUI1.Pipeline = configuration.DefaultPipeline;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
            chooseExtractablesUI1.Setup(_extractionConfiguration);*/
                    
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0;
        }

        public object GetState(ExtractableDataSet rowObject)
        {
            return CheckResult.Success;
        }
    }
}