using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using HIC.Logging.Listeners;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public class ReleaseRunner:ManyRunner
    {
        private readonly ReleaseOptions _options;
        private Pipeline _pipeline;
        private IProject _project;
        private IExtractionConfiguration[] _configurations;
        private ISelectedDataSets[] _selectedDatasets;

        public ReleaseRunner(ReleaseOptions options):base(options)
        {
            _options = options;
        }

        protected override void Initialize()
        {
            _pipeline = RepositoryLocator.CatalogueRepository.GetObjectByID<Pipeline>(_options.Pipeline);
            
            //some datasets only
            _selectedDatasets = RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<SelectedDataSets>(_options.SelectedDataSets).ToArray();

            //get all configurations user has picked or are refernced by _selectedDatasets
            HashSet<int> configurations = new HashSet<int>(_options.Configurations);
            foreach (ISelectedDataSets selectedDataSets in _selectedDatasets)
                configurations.Add(selectedDataSets.ExtractionConfiguration_ID);

            //fetch them all by ID
            _configurations = RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<ExtractionConfiguration>(configurations).ToArray();

            _project = _configurations.Select(c => c.Project).Distinct().Single();

            if (!_configurations.Any())
                throw new Exception("No Configurations have been selected for release");
        }

        protected override ICheckable[] GetCheckables()
        {
            List<ICheckable> toReturn = new List<ICheckable>();

            List<ReleasePotential> ReleasePotentials = new List<ReleasePotential>();

            toReturn.Add(new GlobalsReleaseChecker(_configurations));

            foreach (IExtractionConfiguration configuration in _configurations)
            {
                //create new ReleaseAssesments
                foreach (ISelectedDataSets selectedDataSet in GetSelectedDataSets(configuration))//todo only the ones user ticked
                {
                    var extractionResults = configuration.CumulativeExtractionResults.FirstOrDefault(r => r.IsFor(selectedDataSet));
                    
                    //if it has never been extracted
                    if (extractionResults == null || extractionResults.DestinationDescription == null)
                        ReleasePotentials.Add(new NoReleasePotential(RepositoryLocator, selectedDataSet)); //the potential is ZERO to release this dataset
                    else
                    {
                        //it's been extracted!, who extracted it?
                        var destinationThatExtractedIt = (IExecuteDatasetExtractionDestination)new ObjectConstructor().Construct(extractionResults.GetDestinationType());
                        
                        //destination tell us how releasable it is
                        var releasePotential = destinationThatExtractedIt.GetReleasePotential(RepositoryLocator, selectedDataSet);

                        //it is THIS much releasability!
                        ReleasePotentials.Add(releasePotential);
                    }
                }
            }

            toReturn.AddRange(ReleasePotentials);

            return toReturn.ToArray();
        }

        protected override object[] GetRunnables()
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ISelectedDataSets> GetSelectedDataSets(IExtractionConfiguration configuration)
        {
            //are we only releasing some of the datasets?
            var onlySomeDatasets = _selectedDatasets.Where(sds => sds.ExtractionConfiguration_ID == configuration.ID).ToArray();

            if (onlySomeDatasets.Any())
                return onlySomeDatasets;

            //no, we are releasing all of them
            return configuration.SelectedDataSets;
        }
    }

    public class GlobalsReleaseChecker:ICheckable
    {
        private readonly IExtractionConfiguration[] _configurations;

        public GlobalsReleaseChecker(IExtractionConfiguration[] configurations)
        {
            _configurations = configurations;
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Globals might not exist yet, not sure", CheckResult.Fail,new NotImplementedException()));
        }
    }
}
