// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Listeners;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// Runs the extraction process for an <see cref="ExtractionConfiguration"/> in which all the datasets are linked and extracted to appropriate destination
/// (e.g. CSV, remote database etc)
/// </summary>
public class ExtractionRunner : ManyRunner
{
    private ExtractionOptions _options;
    private IBasicActivateItems _activator;
    private ExtractionConfiguration _configuration;
    private IProject _project;

    private ExtractGlobalsCommand _globalsCommand;
    private Pipeline _pipeline;
    private LogManager _logManager;
    private object _oLock = new();
    public Dictionary<ISelectedDataSets, ExtractCommand> ExtractCommands { get; private set; }

    public ExtractionRunner(IBasicActivateItems activator, ExtractionOptions extractionOpts) : base(activator,extractionOpts)
    {
        _options = extractionOpts;
        _activator = activator;
        ExtractCommands = new Dictionary<ISelectedDataSets, ExtractCommand>();
    }

    protected override void Initialize()
    {
        _configuration =
            GetObjectFromCommandLineString<ExtractionConfiguration>(RepositoryLocator,
                _options.ExtractionConfiguration);
        _project = _configuration.Project;
        _pipeline = GetObjectFromCommandLineString<Pipeline>(RepositoryLocator, _options.Pipeline);

        if (HasConfigurationPreviouslyBeenReleased())
            throw new Exception("Extraction Configuration has already been released");
    }

    protected override void AfterRun()
    {
    }

    protected override object[] GetRunnables()
    {
        lock (_oLock)
        {
            ExtractCommands.Clear();
        }

        var commands = new List<IExtractCommand>();

        //if we are extracting globals
        if (_options.ExtractGlobals)
        {
            var g = _configuration.GetGlobals();
            var globals = new GlobalsBundle(g.OfType<SupportingDocument>().ToArray(),
                g.OfType<SupportingSQLTable>().ToArray());
            _globalsCommand = new ExtractGlobalsCommand(RepositoryLocator, _project, _configuration, globals);
            commands.Add(_globalsCommand);
        }

        foreach (var sds in GetSelectedDataSets())
        {
            var extractDatasetCommand = ExtractCommandCollectionFactory.Create(RepositoryLocator, sds);
            commands.Add(extractDatasetCommand);

            lock (_oLock)
            {
                ExtractCommands.Add(sds, extractDatasetCommand);
            }
        }

        return commands.ToArray();
    }

    protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
    {
        var dataLoadInfo = StartAudit();

        var datasetCommand = runnable as ExtractDatasetCommand;

        var logging = new ToLoggingDatabaseDataLoadEventListener(_logManager, dataLoadInfo);
        var fork =
            datasetCommand != null
                ? new ForkDataLoadEventListener(logging, listener, new ElevateStateListener(datasetCommand))
                : new ForkDataLoadEventListener(logging, listener);

        if (runnable is ExtractGlobalsCommand)
        {
            var useCase = new ExtractionPipelineUseCase(_activator, _project, _globalsCommand, _pipeline, dataLoadInfo)
            { Token = Token };
            useCase.Execute(fork);
        }

        if (datasetCommand != null)
        {
            var executeUseCase =
                new ExtractionPipelineUseCase(_activator, _project, datasetCommand, _pipeline, dataLoadInfo)
                { Token = Token };
            executeUseCase.Execute(fork);
        }

        logging.FinalizeTableLoadInfos();
        dataLoadInfo.CloseAndMarkComplete();
    }

    protected override ICheckable[] GetCheckables(ICheckNotifier checkNotifier)
    {
        var checkables = new List<ICheckable>();

        if (_pipeline == null)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs("No Pipeline has been picked", CheckResult.Fail));
            return Array.Empty<ICheckable>();
        }

        checkables.Add(new ProjectChecker(_activator, _configuration.Project)
        {
            CheckDatasets = false,
            CheckConfigurations = false
        });

        checkables.Add(new ExtractionConfigurationChecker(_activator, _configuration)
        {
            CheckDatasets = false,
            CheckGlobals = false
        });

        foreach (var runnable in GetRunnables())
        {
            if (runnable is ExtractGlobalsCommand globalsCommand)
                checkables.Add(new GlobalExtractionChecker(_activator, _configuration, globalsCommand, _pipeline));

            if (runnable is ExtractDatasetCommand datasetCommand)
                checkables.Add(new SelectedDataSetsChecker(_activator, datasetCommand.SelectedDataSets, false,
                    _pipeline));
        }

        return checkables.ToArray();
    }

    private ISelectedDataSets[] GetSelectedDataSets()
    {
        if (_options.Datasets == null || !_options.Datasets.Any())
            return _configuration.SelectedDataSets;

        var eds = GetObjectsFromCommandLineString<ExtractableDataSet>(RepositoryLocator, _options.Datasets);
        var datasetIds = eds.Select(e => e.ID).ToArray();

        return _configuration.SelectedDataSets.Where(ds => datasetIds.Contains(ds.ExtractableDataSet_ID)).ToArray();
    }

    public ToMemoryCheckNotifier GetGlobalCheckNotifier() => GetSingleCheckerResults<GlobalExtractionChecker>();

    public ToMemoryCheckNotifier GetCheckNotifier(IExtractableDataSet extractableData)
    {
        return GetSingleCheckerResults<SelectedDataSetsChecker>(sds =>
            sds.SelectedDataSet.ExtractableDataSet_ID == extractableData.ID);
    }

    public object GetState(IExtractableDataSet extractableData)
    {
        if (_options.Command == CommandLineActivity.check)
        {
            var sds = GetCheckNotifier(extractableData);

            return sds?.GetWorst();
        }

        if (_options.Command == CommandLineActivity.run)
            lock (_oLock)
            {
                var sds = ExtractCommands.Keys.FirstOrDefault(k => k.ExtractableDataSet_ID == extractableData.ID);

                return sds == null ? null : ExtractCommands[sds].State;
            }

        return null;
    }

    public object GetGlobalsState()
    {
        if (_options.Command == CommandLineActivity.check)
        {
            var g = GetSingleCheckerResults<GlobalExtractionChecker>();

            return g?.GetWorst();
        }

        return _options.Command == CommandLineActivity.run && _globalsCommand != null ? _globalsCommand.State : null;
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
                _configuration.GetLoggingRunName(),
                "", false, _logManager.Server);
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Problem occurred trying to create Logging Component:{e.Message} (check user has access to {_logManager.Server} and that the DataLoadTask '{ExecuteDatasetExtractionSource.AuditTaskName}' exists)",
                e);
        }

        return dataLoadInfo;
    }

    private bool HasConfigurationPreviouslyBeenReleased()
    {
        var previouslyReleasedStuff = _configuration.ReleaseLog;

        return previouslyReleasedStuff.Any();
    }
}