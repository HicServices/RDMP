// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using NLog;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Gui;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners.NLogListeners;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using YamlDotNet.Serialization;

namespace Rdmp.Core
{
    class Program
    {
        const int REPO_ERROR = 7;

        private static EnvironmentInfo GetEnvironmentInfo()
        {
            return new EnvironmentInfo(PluginFolders.Main);
        }
        
        static int Main(string[] args)
        {
            try
            {    
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var nlog = Path.Combine(assemblyFolder ,"NLog.config");

                if (File.Exists(nlog))
                {
                    LogManager.ThrowConfigExceptions = false;
                    NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlog);
                }
                    
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not load NLog.config:" + ex.Message);
            }
            
            if(args.Any(a=>a.Equals("-q")) || args.Any(a=>a.Equals("--quiet",StringComparison.CurrentCultureIgnoreCase)))
            {
                foreach(var t in LogManager.Configuration.AllTargets.ToArray())
                {
                    if(t.GetType().Name.Contains("Console",StringComparison.CurrentCultureIgnoreCase))
                        LogManager.Configuration.RemoveTarget(t.Name);
                }
            }

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("Dotnet Version:" + Environment.Version);
            logger.Info("RDMP Version:" + typeof(Catalogue).Assembly.GetName().Version);

            Startup.Startup.PreStartup();

            return HandleArguments(args,logger);
        }

        private static int HandleArguments(string[] args, Logger logger)
        {
            try
            {
                var returnCode =
                    UsefulStuff.GetParser()
                        .ParseArguments<
                            DleOptions,
                            DqeOptions,
                            CacheOptions,
                            ExtractionOptions,
                            ReleaseOptions,
                            CohortCreationOptions,
                            PackOptions,
                            ExecuteCommandOptions,
                            ConsoleGuiOptions,
                            PlatformDatabaseCreationOptions,
                            PatchDatabaseOptions>(args)
                        .MapResult(
                            //Add new verbs as options here and invoke relevant runner
                            (DleOptions opts) => Run(opts),
                            (DqeOptions opts) => Run(opts),
                            (CacheOptions opts) => Run(opts),
                            (ExtractionOptions opts) => Run(opts),
                            (ReleaseOptions opts) => Run(opts),
                            (CohortCreationOptions opts) => Run(opts),
                            (PackOptions opts) => Run(opts),
                            (PlatformDatabaseCreationOptions opts) => Run(opts),
                            (ExecuteCommandOptions opts) => RunCmd(opts),
                            (ConsoleGuiOptions opts) => Run(opts),
                            (PatchDatabaseOptions opts) => Run(opts),
                            errs => 1);

                logger.Info("Exiting with code " + returnCode);
                return returnCode;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Info(e, "Fatal error occurred so returning -1");
                return -1;
            }
        }

        private static int Run(PlatformDatabaseCreationOptions opts)
        {
            var serverName = opts.ServerName;
            var prefix = opts.Prefix;

            Console.WriteLine("About to create on server '" + serverName + "' databases with prefix '" + prefix + "'");
            
            try
            {
                var creator = new PlatformDatabaseCreation();
                creator.CreatePlatformDatabases(opts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            return 0;
        }

        private static int RunCmd(ExecuteCommandOptions opts)
        {
            if(!string.IsNullOrWhiteSpace(opts.File))
            {
                if(!File.Exists(opts.File))
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
                catch(Exception ex)
                {
                    Console.WriteLine($"Error deserializing '{opts.File}': {ex.Message}");
                    return -57;                    
                }
            }

            return Run((RDMPCommandLineOptions) opts);
        }

        private static int Run(RDMPCommandLineOptions opts)
        {
            opts.PopulateConnectionStringsFromYamlIfMissing(new ThrowImmediatelyCheckNotifier());

            // where RDMP objects are stored
            var repositoryLocator = opts.GetRepositoryLocator();

            if (!CheckRepo(repositoryLocator))
            {
                return REPO_ERROR;
            }

            var listener = new NLogIDataLoadEventListener(false);
            var checker = new NLogICheckNotifier(true, false);

            CatalogueRepository.SuppressHelpLoading = false;

            var factory = new RunnerFactory();
            opts.DoStartup(GetEnvironmentInfo(),opts.LogStartup ? (ICheckNotifier)checker: new IgnoreAllErrorsCheckNotifier());

            //if user wants to run checking chances are they don't want checks to fail becasue of errors logged during startup (MEF shows lots of errors!)
            if(opts.LogStartup && opts.Command == CommandLineActivity.check)
                checker.Worst = LogLevel.Info;
           
            var runner = opts is ConsoleGuiOptions g ? 
                        new ConsoleGuiRunner(g):
                         factory.CreateRunner(new ThrowImmediatelyActivator(repositoryLocator,checker),opts);

            // Let's not worry about global errors during the CreateRunner process
            // These are mainly UI/GUI and unrelated to the actual process to run
            if (checker.Worst > LogLevel.Warn)
                checker.Worst = LogLevel.Warn;

            int runExitCode = runner.Run(repositoryLocator, listener, checker, new GracefulCancellationToken());

            if (opts.Command == CommandLineActivity.check)
                checker.OnCheckPerformed(checker.Worst <= LogLevel.Warn
                    ? new CheckEventArgs("Checks Passed", CheckResult.Success)
                    : new CheckEventArgs("Checks Failed", CheckResult.Fail));

            if (runExitCode != 0)
                return runExitCode;

            //or if either listener reports error
            if (listener.Worst >= LogLevel.Error || checker.Worst >= LogLevel.Error)
                return -1;

            if (opts.FailOnWarnings && (listener.Worst >= LogLevel.Warn || checker.Worst >= LogLevel.Warn))
                return 1;

            return 0;
        }
        
        private static int Run(PatchDatabaseOptions opts)
        {
            opts.PopulateConnectionStringsFromYamlIfMissing(new ThrowImmediatelyCheckNotifier());

            var repo = opts.GetRepositoryLocator();

            if(!CheckRepo(repo))
            {
                return REPO_ERROR;
            }

            var checker = new NLogICheckNotifier(true, false);

            var start = new Startup.Startup(GetEnvironmentInfo(),repo);
            bool badTimes = false;

            start.DatabaseFound += (s,e)=>{
                
                var db = e.Repository.DiscoveredServer.GetCurrentDatabase();
                     
                if(e.Status == Startup.Events.RDMPPlatformDatabaseStatus.RequiresPatching)
                {
                    var mds = new MasterDatabaseScriptExecutor(db);
                    mds.PatchDatabase(e.Patcher, checker, (p) => true, () => opts.BackupDatabase);
                }

                if(e.Status <= Startup.Events.RDMPPlatformDatabaseStatus.Broken)
                {
                    checker.OnCheckPerformed(new CheckEventArgs($"Database {db} had status {e.Status}",CheckResult.Fail));
                    badTimes = true;
                }                    
            };

            start.DoStartup(new IgnoreAllErrorsCheckNotifier());

            return badTimes ? -1 :0;
        }

        private static bool CheckRepo(Repositories.IRDMPPlatformRepositoryServiceLocator repo)
        {
            var logger = LogManager.GetCurrentClassLogger();
            if(repo is LinkedRepositoryProvider l)
            {
                if(l.CatalogueRepository is TableRepository c)
                {
                    try
                    {
                        c.DiscoveredServer.TestConnection(15_000);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex,$"Could not reach {c.DiscoveredServer} (Database:{c.DiscoveredServer.GetCurrentDatabase()}).  Ensure that you have configured RDMP database connections in Databases.yaml correctly and/or that you have run install to setup platform databases");
                        return false;
                    }
                }

                if (l.DataExportRepository is TableRepository d)
                {
                    try
                    {
                        d.DiscoveredServer.TestConnection();
                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex,$"Could not reach {d.DiscoveredServer} (Database:{d.DiscoveredServer.GetCurrentDatabase()}).  Ensure that you have configured RDMP database connections in Databases.yaml correctly and/or that you have run install to setup platform databases");
                        return false;

                    }
                }
            }

            return true;
        }
    }
}
