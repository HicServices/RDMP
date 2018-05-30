using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using HIC.Logging;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public class ExtractionRunner : IRunner
    {
        private ExtractionOptions _options;
        ExtractionConfiguration _configuration;
        IProject _project;

        public Dictionary<ISelectedDataSets, ToMemoryCheckNotifier> ChecksDictionary { get; private set; }

        ExtractGlobalsCommand _globalsCommand;
        public Dictionary<ISelectedDataSets, ExtractDatasetCommand> ExtractCommands { get; private set; }

        public ExtractionRunner(ExtractionOptions extractionOpts)
        {
            _options = extractionOpts;
            ChecksDictionary = new Dictionary<ISelectedDataSets, ToMemoryCheckNotifier>();
            ExtractCommands = new Dictionary<ISelectedDataSets, ExtractDatasetCommand>();
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            _configuration = repositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);
            _project = _configuration.Project;
            var pipeline = repositoryLocator.CatalogueRepository.GetObjectByID<Pipeline>(_options.Pipeline);

            switch (_options.Command)
            {
                case CommandLineActivity.run:

                    var dli = StartAudit();

                    if(_options.ExtractGlobals)
                    {
                        var g = _configuration.GetGlobals();
                        var globals = new GlobalsBundle(g.OfType<SupportingDocument>().ToArray(),g.OfType<SupportingSQLTable>().ToArray());
                        _globalsCommand = new ExtractGlobalsCommand(repositoryLocator, _project, _configuration, globals);
                        var useCase = new ExtractionPipelineUseCase(_project, _globalsCommand, pipeline, dli);
                        useCase.Execute(new OverrideSenderIDataLoadEventListener("Globals",listener));
                    }

                    ExtractCommands.Clear();

                    var factory = new ExtractCommandCollectionFactory();

                    Semaphore semaphore = null;
                    if (_options.MaxConcurrentExtractions != null)
                        semaphore = new Semaphore(0, _options.MaxConcurrentExtractions.Value);

                    foreach (ISelectedDataSets sds in GetSelectedDataSets())
                    {
                        var extractDatasetCommand = factory.Create(repositoryLocator, sds);
                        ExtractCommands.Add(sds, extractDatasetCommand);
                    }

                    foreach (var kvp in ExtractCommands)
                    {
                        var executeUseCase = new ExtractionPipelineUseCase(_project, kvp.Value, pipeline, dli);
                        var name = kvp.Key.ToString();

                        if (semaphore != null)
                            semaphore.WaitOne();

                        Task.Run(() =>
                        {
                            try
                            {
                                executeUseCase.Execute(new OverrideSenderIDataLoadEventListener(name, listener));
                            }
                            finally
                            {
                                if (semaphore != null)
                                    semaphore.Release();
                            }
                        }
                        );
                    }
                    
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

            if(_options.Command == CommandLineActivity.run)
            {
                var sds = ExtractCommands.Keys.FirstOrDefault(k => k.ExtractableDataSet_ID == rowObject.ID);

                if (sds == null)
                    return null;

                return ExtractCommands[sds].State;
            }

            return null;
        }

        private DataLoadInfo StartAudit()
        {
            DataLoadInfo dataLoadInfo;

            var logManager = _configuration.GetExplicitLoggingDatabaseServerOrDefault();

            try
            {
                //populate DataLoadInfo object (Audit)
                dataLoadInfo = new DataLoadInfo(ExecuteDatasetExtractionSource.AuditTaskName,
                                                     Process.GetCurrentProcess().ProcessName,
                                                     _project.Name + "(ExtractionConfiguration ID=" +
                                                     _configuration.ID + ")",
                                                     "", false, logManager.Server);
            }
            catch (Exception e)
            {
                throw new Exception("Problem occurred trying to create Logging Component:" + e.Message + " (check user has access to " + logManager.Server + " and that the DataLoadTask '" + ExecuteDatasetExtractionSource.AuditTaskName + "' exists)", e);
            }

            return dataLoadInfo;
        }
    }
}