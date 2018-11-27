using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
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
using MapsDirectlyToDatabaseTable;
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
            foreach (var configuration in _configurations)
                IdentifyAndRemoveOldExtractionResults(checkNotifier, configuration);

            List<ICheckable> toReturn = new List<ICheckable>();

            if (_options.ReleaseGlobals)
            {
                toReturn.Add(new GlobalsReleaseChecker(RepositoryLocator, _configurations));
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

            var useCase = GetReleaseUseCase(checkNotifier);
            if (useCase != null)
            {
                var engine = useCase.GetEngine(_pipeline, new ThrowImmediatelyDataLoadEventListener());
                toReturn.Add(engine);
            }

            return toReturn.ToArray();
        }

        private void IdentifyAndRemoveOldExtractionResults(ICheckNotifier checkNotifier, IExtractionConfiguration configuration)
        {
            var oldResults = configuration.CumulativeExtractionResults
                .Where(cer => !configuration.GetAllExtractableDataSets().Contains(cer.ExtractableDataSet))
                .ToArray();

            if (oldResults.Any())
            {
                string message = "In Configuration " + configuration + ":" + Environment.NewLine + Environment.NewLine +
                                 "The following CumulativeExtractionResults reflect datasets that were previously extracted under the existing Configuration but are no longer in the CURRENT configuration:";

                message = oldResults.Aggregate(message, (s, n) => s + Environment.NewLine + n);

                if (
                    checkNotifier.OnCheckPerformed(new CheckEventArgs(message, CheckResult.Fail, null,
                        "Delete expired CumulativeExtractionResults for configuration." + Environment.NewLine +
                        "Not doing so may result in failures at Release time.")))
                {
                    foreach (var result in oldResults)
                        result.DeleteInDatabase();
                }
            }

            var oldLostSupplemental = configuration.CumulativeExtractionResults
                .SelectMany(c => c.SupplementalExtractionResults)
                .Union(configuration.SupplementalExtractionResults)
                .Where(s => !RepositoryLocator.ArbitraryDatabaseObjectExists(s.ReferencedObjectRepositoryType, s.ReferencedObjectType, s.ReferencedObjectID))
                .ToArray();

            if (oldLostSupplemental.Any())
            {
                string message = "In Configuration " + configuration + ":" + Environment.NewLine + Environment.NewLine +
                                 "The following list reflect objects (supporting sql, lookups or documents) " +
                                 "that were previously extracted but have since been deleted:";

                message = oldLostSupplemental.Aggregate(message, (s, n) => s + Environment.NewLine + n.DestinationDescription);

                if (
                    checkNotifier.OnCheckPerformed(new CheckEventArgs(message, CheckResult.Fail, null,
                        "Delete expired Extraction Results for configuration." + Environment.NewLine +
                        "Not doing so may result in failures at Release time.")))
                {
                    foreach (var result in oldLostSupplemental)
                        result.DeleteInDatabase();
                }
            }
        }

        private List<ReleasePotential> GetReleasePotentials(IExtractionConfiguration configuration)
        {
            var toReturn = new List<ReleasePotential>();

            //create new ReleaseAssesments
            foreach (ISelectedDataSets selectedDataSet in GetSelectedDataSets(configuration))//todo only the ones user ticked
            {
                var extractionResults = selectedDataSet.GetCumulativeExtractionResultsIfAny();

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
            return new[] { GetReleaseUseCase(new ThrowImmediatelyCheckNotifier()) };
        }

        private ReleaseUseCase GetReleaseUseCase(ICheckNotifier checkNotifier)
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

            try
            {
                return new ReleaseUseCase(_project, data,RepositoryLocator.CatalogueRepository);
            }
            catch (Exception ex)
            {
                checkNotifier.OnCheckPerformed(new CheckEventArgs("FAIL: " + ex.Message, CheckResult.Fail, ex));
            }

            return null;
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

        public object GetState(IExtractionConfiguration configuration)
        {
            var matches = GetCheckerResults<ReleaseEnvironmentPotential>((rp) => rp.Configuration.Equals(configuration));

            if (matches.Length == 0)
                return null;

            return ((ReleaseEnvironmentPotential) matches.Single().Key).Assesment;
        }

        public object GetState(ISelectedDataSets selectedDataSets)
        {
            var matches = GetCheckerResults<ReleasePotential>(rp => rp.SelectedDataSet.ID == selectedDataSets.ID);
            
            if (matches.Length == 0)
                return null;

            var releasePotential = (ReleasePotential) matches.Single().Key;
            var results = matches.Single().Value;

            //not been released ever
            if (releasePotential is NoReleasePotential)
                return Releaseability.NeverBeenSuccessfullyExecuted;

            //do we know the release state of the assesments
            if (releasePotential.Assessments != null && releasePotential.Assessments.Any())
            {
                var releasability = releasePotential.Assessments.Values.Min();

                if (releasability != Releaseability.Undefined)
                    return releasability;
            }

            //otherwise use the checks of it
            return results.GetWorst();
        }

        public object GetState(SupportingSQLTable global)
        {
            return GetState((IMapsDirectlyToDatabaseTable)global);
        }

        public object GetState(SupportingDocument global)
        {
            return GetState((IMapsDirectlyToDatabaseTable)global);
        }

        private object GetState(IMapsDirectlyToDatabaseTable global)
        {
            var matches = GetCheckerResults<GlobalReleasePotential>(rp => rp.RelatedGlobal.Equals(global));

            if (matches.Length == 0)
                return null;

            return ((GlobalReleasePotential) matches.Single().Key).Releasability;
        }

        public CheckResult? GetGlobalReleaseState()
        {
            var result = GetSingleCheckerResults<GlobalsReleaseChecker>();

            if (result == null)
                return null;

            return result.GetWorst();
        }
    }
}
