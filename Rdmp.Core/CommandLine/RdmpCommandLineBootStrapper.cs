// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using CommandLine;
using NLog;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners.NLogListeners;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Startup;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandLine;

/// <summary>
///     Parses strings into relevant <see cref="RDMPCommandLineOptions" /> subclasses
///     and runs appropriate <see cref="Runners.Runner" />
/// </summary>
public class RdmpCommandLineBootStrapper
{
    public static int HandleArgumentsWithStandardRunner(string[] args, Logger logger,
        IRDMPPlatformRepositoryServiceLocator existingLocator = null)
    {
        try
        {
            var returnCode = UsefulStuff.GetParser()
                .ParseArguments<
                    DleOptions,
                    DqeOptions,
                    CacheOptions,
                    ExtractionOptions,
                    ReleaseOptions,
                    CohortCreationOptions,
                    ExecuteCommandOptions>(args)
                .MapResult(
                    //Add new verbs as options here and invoke relevant runner
                    (DleOptions opts) => Run(opts, null, existingLocator),
                    (DqeOptions opts) => Run(opts, null, existingLocator),
                    (CacheOptions opts) => Run(opts, null, existingLocator),
                    (ExtractionOptions opts) => Run(opts, null, existingLocator),
                    (ReleaseOptions opts) => Run(opts, null, existingLocator),
                    (CohortCreationOptions opts) => Run(opts, null, existingLocator),
                    (ExecuteCommandOptions opts) => RunCmd(opts, existingLocator),
                    errs => 1);

            logger.Info($"Exiting with code {returnCode}");
            return returnCode;
        }
        catch (Exception e)
        {
            logger.Error(e.Message);
            logger.Info(e, "Fatal error occurred so returning -1");
            return -1;
        }
    }

    public static int RunCmd(ExecuteCommandOptions opts, IRDMPPlatformRepositoryServiceLocator existingLocator = null)
    {
        if (!string.IsNullOrWhiteSpace(opts.File))
        {
            if (!File.Exists(opts.File))
            {
                Console.WriteLine($"Could not find file '{opts.File}'");
                return -55;
            }

            var content = File.ReadAllText(opts.File);

            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"File is empty ('{opts.File}')");
                return -56;
            }

            try
            {
                var d = new Deserializer();
                opts.Script = d.Deserialize<RdmpScript>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing '{opts.File}': {ex.Message}");
                return -57;
            }
        }

        return Run(opts, null, existingLocator);
    }

    public static int Run(RDMPCommandLineOptions opts, IRunner explicitRunner = null,
        IRDMPPlatformRepositoryServiceLocator existingLocator = null)
    {
        // if we have already done startup great!
        var repositoryLocator = existingLocator;

        var listener = new NLogIDataLoadEventListener(false);
        var checker = new NLogICheckNotifier(true, false);

        // not done startup so check that we can reach stuff
        if (repositoryLocator == null)
        {
            opts.PopulateConnectionStringsFromYamlIfMissing(ThrowImmediatelyCheckNotifier.Quiet);

            // where RDMP objects are stored
            repositoryLocator = opts.GetRepositoryLocator();

            if (repositoryLocator?.CatalogueRepository == null)
            {
                listener.OnNotify(typeof(RdmpCommandLineBootStrapper),
                    new NotifyEventArgs(ProgressEventType.Error,
                        "No repository has been specified.  Either create a Databases.yaml file or provide repository connection strings/paths as command line arguments"));
                return REPO_ERROR;
            }


            if (!CheckRepo(repositoryLocator)) return REPO_ERROR;

            CatalogueRepository.SuppressHelpLoading = false;
            opts.DoStartup(opts.LogStartup ? checker : IgnoreAllErrorsCheckNotifier.Instance);
        }

        //if user wants to run checking chances are they don't want checks to fail because of errors logged during startup (MEF shows lots of errors!)
        if (opts.LogStartup && opts.Command == CommandLineActivity.check)
            checker.Worst = LogLevel.Info;

        var runner = explicitRunner ??
                     RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(repositoryLocator, checker), opts);

        // Let's not worry about global errors during the CreateRunner process
        // These are mainly UI/GUI and unrelated to the actual process to run
        if (checker.Worst > LogLevel.Warn)
            checker.Worst = LogLevel.Warn;

        var runExitCode = runner.Run(repositoryLocator, listener, checker, new GracefulCancellationToken());

        if (opts.Command == CommandLineActivity.check)
            checker.OnCheckPerformed(checker.Worst <= LogLevel.Warn
                ? new CheckEventArgs("Checks Passed", CheckResult.Success)
                : new CheckEventArgs("Checks Failed", CheckResult.Fail));

        if (runExitCode != 0)
            return runExitCode;

        //or if either listener reports error
        if (listener.Worst >= LogLevel.Error || checker.Worst >= LogLevel.Error)
            return -1;

        return opts.FailOnWarnings && (listener.Worst >= LogLevel.Warn || checker.Worst >= LogLevel.Warn) ? 1 : 0;
    }

    /// <summary>
    ///     The error to return when there is a problem contacting the repository databases
    /// </summary>
    public const int REPO_ERROR = 7;

    public static bool CheckRepo(IRDMPPlatformRepositoryServiceLocator repo)
    {
        var logger = LogManager.GetCurrentClassLogger();
        if (repo is not LinkedRepositoryProvider l) return true;
        if (l.CatalogueRepository is TableRepository c)
            try
            {
                c.DiscoveredServer.TestConnection(15_000);
            }
            catch (Exception ex)
            {
                logger.Error(ex,
                    $"Could not reach {c.DiscoveredServer} (Database:{c.DiscoveredServer.GetCurrentDatabase()}).  Ensure that you have configured RDMP database connections in Databases.yaml correctly and/or that you have run install to setup platform databases");
                return false;
            }

        if (l.DataExportRepository is TableRepository d)
            try
            {
                d.DiscoveredServer.TestConnection();
            }
            catch (Exception ex)
            {
                logger.Error(ex,
                    $"Could not reach {d.DiscoveredServer} (Database:{d.DiscoveredServer.GetCurrentDatabase()}).  Ensure that you have configured RDMP database connections in Databases.yaml correctly and/or that you have run install to setup platform databases");
                return false;
            }

        return true;
    }
}