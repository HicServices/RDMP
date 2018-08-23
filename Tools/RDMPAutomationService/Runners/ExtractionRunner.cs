using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using HIC.Logging;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// Runs the extraction process for an <see cref="ExtractionConfiguration"/> in which all the datasets are linked and extracted to appropriate destination
    /// (e.g. CSV, remote database etc)
    /// </summary>
    public class ExtractionRunner : ManyRunner
    {
        private ExtractionOptions _options;
        ExtractionConfiguration _configuration;
        IProject _project;

        ExtractGlobalsCommand _globalsCommand;
        private Pipeline _pipeline;
        private DataLoadInfo _dataLoadInfo;
        private LogManager _logManager;

        object _oLock = new object();
        public Dictionary<ISelectedDataSets, ExtractCommand> ExtractCommands { get;private set; }

        public ExtractionRunner(ExtractionOptions extractionOpts):base(extractionOpts)
        {
            _options = extractionOpts;
            ExtractCommands = new Dictionary<ISelectedDataSets, ExtractCommand>();
        }

        protected override void Initialize()
        {
            _configuration = RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);
            _project = _configuration.Project;
            _pipeline = RepositoryLocator.CatalogueRepository.GetObjectByID<Pipeline>(_options.Pipeline);

            if (HasConfigurationPreviouslyBeenReleased())
                throw new Exception("Extraction Configuration has already been released");
        }

        protected override void AfterRun()
        {
            if(_dataLoadInfo != null && !_dataLoadInfo.IsClosed)
                _dataLoadInfo.CloseAndMarkComplete();
        }

        protected override object[] GetRunnables()
        {
            lock(_oLock)
                ExtractCommands.Clear();

            var commands = new List<IExtractCommand>();

            _dataLoadInfo = StartAudit();

            //if we are extracting globals
            if (_options.ExtractGlobals)
            {
                var g = _configuration.GetGlobals();
                var globals = new GlobalsBundle(g.OfType<SupportingDocument>().ToArray(), g.OfType<SupportingSQLTable>().ToArray());
                _globalsCommand = new ExtractGlobalsCommand(RepositoryLocator, _project, _configuration, globals);
                commands.Add(_globalsCommand);
            }
            
            var factory = new ExtractCommandCollectionFactory();

            foreach (ISelectedDataSets sds in GetSelectedDataSets())
            {
                var extractDatasetCommand = factory.Create(RepositoryLocator, sds);
                commands.Add(extractDatasetCommand);
                
                lock(_oLock)
                    ExtractCommands.Add(sds,extractDatasetCommand);
            }

            return commands.ToArray();
        }

        protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
        {
            var globalCommand = runnable as ExtractGlobalsCommand;
            var datasetCommand = runnable as ExtractDatasetCommand;

            var logging = new ToLoggingDatabaseDataLoadEventListener(_logManager, _dataLoadInfo);
            var fork = new ForkDataLoadEventListener(logging,listener);

            if(globalCommand != null)
            {
                var useCase = new ExtractionPipelineUseCase(_project, _globalsCommand, _pipeline, _dataLoadInfo) { Token = Token };
                useCase.Execute(fork);
            }

            if (datasetCommand != null)
            {
                var executeUseCase = new ExtractionPipelineUseCase(_project,datasetCommand, _pipeline, _dataLoadInfo) { Token = Token };
                executeUseCase.Execute(fork);
            }

            logging.FinalizeTableLoadInfos();
        }

        protected override ICheckable[] GetCheckables(ICheckNotifier checkNotifier)
        {
            var checkables = new List<ICheckable>();

            if (_pipeline == null)
            {
                checkNotifier.OnCheckPerformed(new CheckEventArgs("No Pipeline has been picked", CheckResult.Fail));
                return new ICheckable[0];
            }

            checkables.Add(new ProjectChecker(RepositoryLocator, _configuration.Project)
            {
                CheckDatasets = false,
                CheckConfigurations = false
            });

            checkables.Add(new ExtractionConfigurationChecker(RepositoryLocator, _configuration)
            {
                CheckDatasets = false,
                CheckGlobals = false
            });
            
            if(_options.ExtractGlobals)
                checkables.Add(new GlobalExtractionChecker(_configuration));

            foreach (var runnable in GetRunnables())
            {
                var command = runnable as IExtractCommand;
                var datasetCommand = runnable as ExtractDatasetCommand;
                
                if (datasetCommand != null)
                    checkables.Add(new SelectedDataSetsChecker(datasetCommand.SelectedDataSets, RepositoryLocator));

                checkables.Add(new ExtractionPipelineUseCase(_project, command, _pipeline, DataLoadInfo.Empty) { Token = Token }
                                       .GetEngine(_pipeline, new ToMemoryDataLoadEventListener(false)));
            }
            
            return checkables.ToArray();
        }
        
        private ISelectedDataSets[] GetSelectedDataSets()
        {
            if (_options.Datasets == null || !_options.Datasets.Any())
                return _configuration.SelectedDataSets;

            return _configuration.SelectedDataSets.Where(ds => _options.Datasets.Contains(ds.ExtractableDataSet_ID)).ToArray();
        }

        public ToMemoryCheckNotifier GetGlobalCheckNotifier()
        {
            return GetSingleCheckerResults<GlobalExtractionChecker>();
        }

        public ToMemoryCheckNotifier GetCheckNotifier(IExtractableDataSet extractableData)
        {
            return GetSingleCheckerResults<SelectedDataSetsChecker>((sds) => sds.SelectedDataSet.ExtractableDataSet_ID == extractableData.ID);
        }

        public object GetState(IExtractableDataSet extractableData)
        {
            if(_options.Command == CommandLineActivity.check)
            {
                var sds = GetCheckNotifier(extractableData);

                if (sds == null)
                    return null;

                return sds.GetWorst();
            }

            if(_options.Command == CommandLineActivity.run)
            {
                lock (_oLock)
                {
                    var sds = ExtractCommands.Keys.FirstOrDefault(k => k.ExtractableDataSet_ID == extractableData.ID);

                    if (sds == null)
                        return null;

                    return ExtractCommands[sds].State;
                }
            }
                
            
            return null;
        }

        public object GetGlobalsState()
        {
            if (_options.Command == CommandLineActivity.check)
            {
                var g = GetSingleCheckerResults<GlobalExtractionChecker>();

                if (g == null)
                    return null;

                return g.GetWorst();
            }

            if (_options.Command == CommandLineActivity.run && _globalsCommand != null)
                return _globalsCommand.State;

            return null;
            
        }

        private DataLoadInfo StartAudit()
        {
            DataLoadInfo dataLoadInfo;

            _logManager = _configuration.GetExplicitLoggingDatabaseServerOrDefault();

            try
            {
                //populate DataLoadInfo object (Audit)
                dataLoadInfo = new DataLoadInfo(ExecuteDatasetExtractionSource.AuditTaskName,
                                                     Process.GetCurrentProcess().ProcessName,
                                                     _project.Name + "(ExtractionConfiguration ID=" +
                                                     _configuration.ID + ")",
                                                     "", false, _logManager.Server);
            }
            catch (Exception e)
            {
                throw new Exception("Problem occurred trying to create Logging Component:" + e.Message + " (check user has access to " + _logManager.Server + " and that the DataLoadTask '" + ExecuteDatasetExtractionSource.AuditTaskName + "' exists)", e);
            }

            return dataLoadInfo;
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