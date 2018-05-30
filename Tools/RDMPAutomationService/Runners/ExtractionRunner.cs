using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public class ExtractionRunner : IRunner
    {
        private ExtractionOptions _options;
        ExtractionConfiguration _configuration;

        public Dictionary<ISelectedDataSets, ToMemoryCheckNotifier> ChecksDictionary { get; private set; }

        public ExtractionRunner(ExtractionOptions extractionOpts)
        {
            _options = extractionOpts;
            ChecksDictionary = new Dictionary<ISelectedDataSets, ToMemoryCheckNotifier>();
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            _configuration = repositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);

            switch (_options.Command)
            {
                case CommandLineActivity.run:

                    break;
                case CommandLineActivity.check:

                    var memory = new ToMemoryCheckNotifier(checkNotifier);

                    var projectChecker = new ProjectChecker(repositoryLocator, _configuration.Project) {CheckDatasets = false,CheckConfigurations = false};
                    projectChecker.Check(memory);

                    var configurationChecker = new ExtractionConfigurationChecker(repositoryLocator, _configuration) {CheckDatasets = false};
                    configurationChecker.Check(memory);

                    //don't bother checking datasets if the Project / Configuration checks fail
                    if (memory.GetWorst() > CheckResult.Warning)
                        return 0;
                    
                    ChecksDictionary.Clear();

                    foreach(var sds in GetSelectedDataSets())
                        ChecksDictionary.Add(sds,new ToMemoryCheckNotifier(checkNotifier));

                    List<Task> tasks = new List<Task>();

                    foreach (var kvp in ChecksDictionary)
                    {
                        KeyValuePair<ISelectedDataSets, ToMemoryCheckNotifier> kvp1 = kvp;
                        var t = new Task(() => new SelectedDatasetsChecker(kvp1.Key, repositoryLocator).Check(kvp1.Value));
                        t.Start();
                        tasks.Add(t);
                    }

                    Task.WaitAll(tasks.ToArray(), token.StopToken);
                    
                    
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0;
        }

        private ISelectedDataSets[] GetSelectedDataSets()
        {
            if (_options.Datasets == null || !_options.Datasets.Any())
                return _configuration.SelectedDataSets;

            return _configuration.SelectedDataSets.Where(ds => _options.Datasets.Contains(ds.ExtractableDataSet_ID)).ToArray();
        }

        public object GetState(ExtractableDataSet rowObject)
        {
            if(_options.Command == CommandLineActivity.check)
            {

                var sds = ChecksDictionary.Keys.SingleOrDefault(k => k.ExtractableDataSet_ID == rowObject.ID);

                if (sds == null)
                    return null;

                return ChecksDictionary[sds].GetWorst();
            }

            //todo execution state
            return null;
        }
    }
}