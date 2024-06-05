// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using CommandLine;
using NLog;
using Rdmp.Core.CommandLine;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Gui;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Logging.Listeners.NLogListeners;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core;

internal class Program
{
    /// <summary>
    /// True if the user passed the -q switch at startup to suppress any helpful messages we might
    /// show (e.g. maybe they want to pipe the results somewhere)
    /// </summary>
    public static bool Quiet { get; private set; }

    private static int Main(string[] args)
    {
        try
        {
            var nlog = Path.Combine(AppContext.BaseDirectory, "NLog.config");

            if (File.Exists(nlog))
            {
                LogManager.ThrowConfigExceptions = false;
                LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlog);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load NLog.config:{ex.Message}");
        }

        if (args.Any(a => a.Equals("-q")) ||
            args.Any(a => a.Equals("--quiet", StringComparison.CurrentCultureIgnoreCase)))
        {
            Quiet = true;

            DisableConsoleLogging();
        }

        var logger = LogManager.GetCurrentClassLogger();

        logger.Info($"Dotnet Version:{Environment.Version}");
        logger.Info($"RDMP Version:{typeof(Catalogue).Assembly.GetName().Version}");

        Startup.Startup.PreStartup();

        return HandleArguments(args, logger);
    }

    /// <summary>
    /// Disables all log targets that contain the word 'Console'.  This prevents logging output
    /// corrupting the screen during TUI or cli use (e.g. with -q flag).
    /// </summary>
    public static void DisableConsoleLogging()
    {
        foreach (var t in LogManager.Configuration.AllTargets.ToArray())
            if (t.GetType().Name.Contains("Console", StringComparison.CurrentCultureIgnoreCase))
                LogManager.Configuration.RemoveTarget(t.Name);
    }

    private static int HandleArguments(string[] args, Logger logger)
    {
        int returnCode;
        try
        {
            returnCode =
                UsefulStuff.GetParser()
                    .ParseArguments<
                        PackOptions,
                        ConsoleGuiOptions,
                        PlatformDatabaseCreationOptions,
                        PatchDatabaseOptions,
                        DleOptions,
                        DqeOptions,
                        CacheOptions,
                        ExtractionOptions,
                        ReleaseOptions,
                        CohortCreationOptions,
                        ExecuteCommandOptions>(args)
                    .MapResult(static (PackOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (ConsoleGuiOptions opts) =>
                            RdmpCommandLineBootStrapper.Run(opts, new ConsoleGuiRunner(opts)),
                        static (PlatformDatabaseCreationOptions opts) => Run(opts),
                        static (PatchDatabaseOptions opts) => Run(opts),
                        static (DleOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (DqeOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (CacheOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (ExtractionOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (ReleaseOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (CohortCreationOptions opts) => RdmpCommandLineBootStrapper.Run(opts),
                        static (ExecuteCommandOptions opts) => RdmpCommandLineBootStrapper.RunCmd(opts),
                        errs => HasHelpArguments(args)
                            ? returnCode = 0
                            : returnCode =
                                RdmpCommandLineBootStrapper.HandleArgumentsWithStandardRunner(args, logger));

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

    private static bool HasHelpArguments(string[] args)
    {
        return
            args.Any(a =>
                a.Equals("--help", StringComparison.CurrentCultureIgnoreCase) ||
                a.Equals("--version", StringComparison.CurrentCultureIgnoreCase)
            );
    }

    private static int Run(PlatformDatabaseCreationOptions opts)
    {
        var serverName = opts.ServerName;
        var prefix = opts.Prefix;

        Console.WriteLine($"About to create on server '{serverName}' databases with prefix '{prefix}'");

        try
        {
            PlatformDatabaseCreation.CreatePlatformDatabases(opts);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }

        return 0;
    }


    private static int Run(PatchDatabaseOptions opts)
    {
        opts.PopulateConnectionStringsFromYamlIfMissing(ThrowImmediatelyCheckNotifier.Quiet);

        var repo = opts.GetRepositoryLocator();

        if (!RdmpCommandLineBootStrapper.CheckRepo(repo)) return RdmpCommandLineBootStrapper.REPO_ERROR;

        var checker = new NLogICheckNotifier(true, false);

        var start = new Startup.Startup(repo);
        var badTimes = false;

        start.DatabaseFound += (s, e) =>
        {
            var db = e.Repository.DiscoveredServer.GetCurrentDatabase();

            switch (e.Status)
            {
                case Startup.Events.RDMPPlatformDatabaseStatus.RequiresPatching:
                    {
                        var mds = new MasterDatabaseScriptExecutor(db);
                        mds.PatchDatabase(e.Patcher, checker, p => true, () => opts.BackupDatabase);
                        break;
                    }
                case <= Startup.Events.RDMPPlatformDatabaseStatus.Broken:
                    checker.OnCheckPerformed(new CheckEventArgs($"Database {db} had status {e.Status}",
                        CheckResult.Fail));
                    badTimes = true;
                    break;
            }
        };

        start.DoStartup(IgnoreAllErrorsCheckNotifier.Instance);

        return badTimes ? -1 : 0;
    }
}