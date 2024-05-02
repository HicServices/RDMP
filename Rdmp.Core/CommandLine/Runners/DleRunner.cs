// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
///     <see cref="IRunner" /> for the Data Load Engine.  Supports both check and execute commands.
/// </summary>
public class DleRunner : Runner
{
    private readonly DleOptions _options;

    public DleRunner(DleOptions options)
    {
        _options = options;
    }

    public override int Run(IRDMPPlatformRepositoryServiceLocator locator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token)
    {
        ILoadProgress loadProgress = GetObjectFromCommandLineString<LoadProgress>(locator, _options.LoadProgress);
        ILoadMetadata loadMetadata = GetObjectFromCommandLineString<LoadMetadata>(locator, _options.LoadMetadata);

        if (loadMetadata == null && loadProgress != null)
            loadMetadata = loadProgress.LoadMetadata;

        if (loadMetadata == null)
            throw new ArgumentException("No Load Metadata specified");

        if (loadProgress != null && loadProgress.LoadMetadata_ID != loadMetadata.ID)
            throw new ArgumentException("The supplied LoadProgress does not belong to the supplied LoadMetadata load");

        var databaseConfiguration = new HICDatabaseConfiguration(loadMetadata);
        var flags = new HICLoadConfigurationFlags
        {
            ArchiveData = !_options.DoNotArchiveData,
            DoLoadToStaging = !_options.StopAfterRAW,
            DoMigrateFromStagingToLive = !_options.StopAfterSTAGING
        };

        var checkable = new CheckEntireDataLoadProcess(loadMetadata, databaseConfiguration, flags);

        switch (_options.Command)
        {
            case CommandLineActivity.run:

                var loggingServer = loadMetadata.GetDistinctLoggingDatabase();
                var logManager = new LogManager(loggingServer);

                // Create the pipeline to pass into the DataLoadProcess object
                var dataLoadFactory = new HICDataLoadFactory(loadMetadata, databaseConfiguration, flags,
                    locator.CatalogueRepository, logManager);

                var execution = dataLoadFactory.Create(listener);
                IDataLoadProcess dataLoadProcess;

                if (loadMetadata.LoadProgresses.Any())
                {
                    //Then the load is designed to run X days of source data at a time
                    //Load Progress
                    var whichLoadProgress = loadProgress != null
                        ? (ILoadProgressSelectionStrategy)new SingleLoadProgressSelectionStrategy(loadProgress)
                        : new AnyAvailableLoadProgressSelectionStrategy(loadMetadata);

                    var jobDateFactory = new JobDateGenerationStrategyFactory(whichLoadProgress);

                    dataLoadProcess = _options.Iterative
                        ? new IterativeScheduledDataLoadProcess(locator, loadMetadata, checkable,
                            execution, jobDateFactory, whichLoadProgress, _options.DaysToLoad, logManager, listener,
                            databaseConfiguration)
                        : new SingleJobScheduledDataLoadProcess(locator, loadMetadata, checkable, execution,
                            jobDateFactory, whichLoadProgress, _options.DaysToLoad, logManager, listener,
                            databaseConfiguration);
                }
                else
                    //OnDemand
                {
                    dataLoadProcess = new DataLoadProcess(locator, loadMetadata, checkable, logManager, listener,
                        execution, databaseConfiguration);
                }


                var exitCode = dataLoadProcess.Run(token);

                if (exitCode is ExitCodeType.Success)
                {
                    //Store the date of the last successful load
                    loadMetadata.LastLoadTime = DateTime.Now;
                    loadMetadata.SaveToDatabase();
                    var processTasks = loadMetadata.ProcessTasks.Where(ipt =>
                        ipt.Path == typeof(RemoteDatabaseAttacher).FullName ||
                        ipt.Path == typeof(RemoteTableAttacher).FullName).ToList();
                    if (processTasks.Count() > 0) //if using a remote attacher, there may be some additional work to do
                        foreach (IEnumerable<Argument> arguments in processTasks.Select(task => task.GetAllArguments()))
                        foreach (var argument in arguments.Where(arg =>
                                     arg.Name == RemoteAttacherPropertiesValidator("DeltaReadingStartDate") &&
                                     arg.Value is not null))
                        {
                            var scanForwardDate = arguments.Where(a =>
                                a.Name == RemoteAttacherPropertiesValidator("DeltaReadingLookForwardDays")).First();
                            var arg = (ProcessTaskArgument)argument;
                            arg.Value = DateTime.Parse(argument.Value).AddDays(int.Parse(scanForwardDate.Value))
                                .ToString();
                            if (arguments.Where(a =>
                                    a.Name == RemoteAttacherPropertiesValidator(
                                        "SetDeltaReadingToLatestSeenDatePostLoad")).First().Value == "True")
                            {
                                var mostRecentValue = arguments.Single(a =>
                                    a.Name == RemoteAttacherPropertiesValidator("MostRecentlySeenDate")).Value;
                                if (mostRecentValue is not null) arg.Value = DateTime.Parse(mostRecentValue).ToString();
                            }

                            arg.SaveToDatabase();
                        }
                }

                //return 0 for success or load not required otherwise return the exit code (which will be non zero so error)
                return exitCode is ExitCodeType.Success or ExitCodeType.OperationNotRequired ? 0 : (int)exitCode;
            case CommandLineActivity.check:

                checkable.Check(checkNotifier);

                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string RemoteAttacherPropertiesValidator(string propertyName)
    {
        var properties = typeof(RemoteAttacher).GetProperties();
        var foundProperties = properties.Where(p => p.Name == propertyName);
        if (foundProperties.Any()) return foundProperties.First().Name;
        throw new Exception(
            $"Attempting to access the property {propertyName} of the RemoteAttacher class. This property does not exist.");
    }
}