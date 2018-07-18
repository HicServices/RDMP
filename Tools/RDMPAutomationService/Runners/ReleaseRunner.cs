using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// Runs the release process for one or more <see cref="ExtractionConfiguration"/> in the same <see cref="Project"/>.  This is the proces by which we gather all the artifacts
    /// produced by the Extraction Engine (anonymised project extracts, bundled lookups and documents etc) and transmit them somewhere as a final released package.
    /// </summary>
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

            if (!_configurations.Any())
                throw new Exception("No Configurations have been selected for release");

            _project = _configurations.Select(c => c.Project).Distinct().Single();
        }

        protected override void AfterRun()
        {
            
        }

        protected override ICheckable[] GetCheckables(ICheckNotifier checkNotifier)
        {
            List<ICheckable> toReturn = new List<ICheckable>();

            if (_options.ReleaseGlobals)
            {
                toReturn.AddRange(_configurations.First()
                                                 .GetGlobals()
                                                 .Select(availableGlobal => 
                                                     new GlobalsReleaseChecker(RepositoryLocator, _configurations, availableGlobal).GetEvaluator()));
            }

            foreach (IExtractionConfiguration configuration in _configurations)
            {
                toReturn.AddRange(GetReleasePotentials(configuration));
                toReturn.Add(new ReleaseEnvironmentPotential(configuration));
            }

            if(_pipeline == null)
            {
                checkNotifier.OnCheckPerformed(new CheckEventArgs("No Pipeline has been picked", CheckResult.Fail));
                return new ICheckable[0];
            }

            var useCase = GetReleaseUseCase();
            var engine = useCase.GetEngine(_pipeline, new ThrowImmediatelyDataLoadEventListener());
            toReturn.Add(engine);

            return toReturn.ToArray();
        }

        private List<ReleasePotential> GetReleasePotentials(IExtractionConfiguration configuration)
        {
            var toReturn = new List<ReleasePotential>();

            //create new ReleaseAssesments
            foreach (ISelectedDataSets selectedDataSet in GetSelectedDataSets(configuration))//todo only the ones user ticked
            {
                var extractionResults = configuration.CumulativeExtractionResults.FirstOrDefault(r => r.IsFor(selectedDataSet));

                //if it has never been extracted
                if (extractionResults == null || extractionResults.DestinationDescription == null)
                    toReturn.Add(new NoReleasePotential(RepositoryLocator, selectedDataSet)); //the potential is ZERO to release this dataset
                else
                {
                    //it's been extracted!, who extracted it?
                    var destinationThatExtractedIt = (IExecuteDatasetExtractionDestination)new ObjectConstructor().Construct(extractionResults.GetDestinationType());

                    //destination tell us how releasable it is
                    var releasePotential = destinationThatExtractedIt.GetReleasePotential(RepositoryLocator, selectedDataSet);

                    //it is THIS much releasability!
                    toReturn.Add(releasePotential);
                }
            }
            
            return toReturn;
        }

        protected override object[] GetRunnables()
        {
            return new[] { GetReleaseUseCase() };
        }

        private ReleaseUseCase GetReleaseUseCase()
        {
            var data = new ReleaseData(RepositoryLocator);

            foreach (IExtractionConfiguration configuration in _configurations)
            {
                data.ConfigurationsForRelease.Add(configuration, GetReleasePotentials(configuration));
                data.EnvironmentPotentials.Add(configuration, new ReleaseEnvironmentPotential(configuration));
                data.SelectedDatasets.Add(configuration, GetSelectedDataSets(configuration));
            }

            data.ReleaseGlobals = _options.ReleaseGlobals;

            var allDdatasets = _configurations.SelectMany(ec => ec.GetAllExtractableDataSets()).ToList();
            var selectedDatasets = data.SelectedDatasets.Values.SelectMany(sd => sd.ToList()).ToList();

            data.ReleaseState = allDdatasets.Count != selectedDatasets.Count 
                                    ? ReleaseState.DoingPatch 
                                    : ReleaseState.DoingProperRelease;

            return new ReleaseUseCase(_project, data);
        }

        protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
        {
            var useCase = (ReleaseUseCase) runnable;
            var engine = useCase.GetEngine(_pipeline, listener);
            engine.ExecutePipeline(Token);
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
}
