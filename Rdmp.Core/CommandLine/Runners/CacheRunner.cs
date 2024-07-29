// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// Automation task that runs a single CacheProgress until it is up-to-date (all available data read) or crashes.
/// </summary>
public class CacheRunner : Runner
{
    private readonly CacheOptions _options;

    public CacheRunner(CacheOptions options)
    {
        _options = options;
    }

    public override int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        var cp = GetObjectFromCommandLineString<CacheProgress>(repositoryLocator, _options.CacheProgress);
        var dataLoadTask = cp.GetDistinctLoggingTask();

        var defaults = repositoryLocator.CatalogueRepository;
        var loggingServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID) ??
                            throw new NotSupportedException(
                                "No default logging server specified, you must specify one in ");
        var logManager = new LogManager(loggingServer);

        logManager.CreateNewLoggingTaskIfNotExists(dataLoadTask);

        switch (_options.Command)
        {
            case CommandLineActivity.run:

                //Setup dual listeners for the Cache process, one ticks the lifeline one very message and one logs to the logging db
                var toLog = new ToLoggingDatabaseDataLoadEventListener(this, logManager, dataLoadTask,
                    cp.GetLoggingRunName());
                var forkListener = new ForkDataLoadEventListener(toLog, listener);
                try
                {
                    var cachingHost = new CachingHost(repositoryLocator.CatalogueRepository)
                    {
                        RetryMode = _options.RetryMode,
                        CacheProgress = cp, //run the cp
                        //By default caching host will block
                        TerminateIfOutsidePermissionWindow = true
                    };

                    cachingHost.Start(forkListener, token);
                }
                finally
                {
                    //finish everything
                    toLog.FinalizeTableLoadInfos();
                }

                break;
            case CommandLineActivity.check:
                var checkable = new CachingPreExecutionChecker(cp);
                checkable.Check(checkNotifier);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return 0;
    }
}