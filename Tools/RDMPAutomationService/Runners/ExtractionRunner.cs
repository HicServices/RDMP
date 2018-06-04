using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
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
    public class ExtractionRunner : ManyRunner
    {
        private ExtractionOptions _options;
        ExtractionConfiguration _configuration;
        IProject _project;

        ExtractGlobalsCommand _globalsCommand;
        private Pipeline _pipeline;
        private DataLoadInfo _dataLoadInfo;

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

        protected override object[] GetRunnables()
        {
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
                ExtractCommands.Add(sds,extractDatasetCommand);
            }

            return commands.ToArray();
        }

        protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
        {
            var globalCommand = runnable as ExtractGlobalsCommand;
            var datasetCommand = runnable as ExtractDatasetCommand;

            if(globalCommand != null)
            {
                var useCase = new ExtractionPipelineUseCase(_project, _globalsCommand, _pipeline, _dataLoadInfo) { Token = Token };
                useCase.Execute(new OverrideSenderIDataLoadEventListener(ExtractionDirectory.GLOBALS_DATA_NAME, listener));
            }

            if (datasetCommand != null)
            {
                var executeUseCase = new ExtractionPipelineUseCase(_project,datasetCommand, _pipeline, _dataLoadInfo) { Token = Token };
                executeUseCase.Execute(listener);
            }
        }

        protected override ICheckable[] GetCheckables()
        {
            ChecksDictionary.Clear();
            var checkables = new List<ICheckable>();

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
            
            checkables.Add(new GlobalExtractionChecker(_configuration));

            foreach (var runnable in GetRunnables())
            {
                var command = runnable as IExtractCommand;
                var datasetCommand = runnable as ExtractDatasetCommand;
                
                if (datasetCommand != null)
                    checkables.Add(new SelectedDatasetsChecker(datasetCommand.SelectedDataSets, RepositoryLocator));

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

        public object GetState(ExtractableDataSet rowObject)
        {
            if(_options.Command == CommandLineActivity.check)
            {
                var sds = ChecksDictionary.Keys.OfType<SelectedDatasetsChecker>().SingleOrDefault(k => k.SelectedDataSet.ExtractableDataSet_ID == rowObject.ID);

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

        public object GetGlobalsState()
        {
            if (_options.Command == CommandLineActivity.check)
            {
                var sds = ChecksDictionary.Keys.OfType<GlobalExtractionChecker>().SingleOrDefault();

                if (sds == null)
                    return null;

                return ChecksDictionary[sds].GetWorst();
            }

            if (_options.Command == CommandLineActivity.run && _globalsCommand != null)
                return _globalsCommand.State;

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

        private bool HasConfigurationPreviouslyBeenReleased()
        {
            var previouslyReleasedStuff = _configuration.ReleaseLogEntries;

            if (previouslyReleasedStuff.Any())
                return true;

            return false;
        }
    }
}