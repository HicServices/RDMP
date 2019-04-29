using System;
using System.Data.SqlClient;
using System.IO;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using NLog;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Options.Abstracts;
using Rdmp.Core.CommandLine.Packing;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners.NLogListeners;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup.PluginManagement;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace rdmp
{
    class Program
    {
        static int Main(string[] args)
        {
            PreStartup();

            return HandleArguments(args);
        }
        
        private static void PreStartup()
        {
            AssemblyResolver.SetupAssemblyResolver();

            ImplementationManager.Load(
                typeof(MicrosoftSQLImplementation).Assembly,
                typeof(MySqlImplementation).Assembly,
                typeof(OracleImplementation).Assembly
            );
        }

        private static int HandleArguments(string[] args)
        {
            try
            {
                var returnCode =
                    UsefulStuff.GetParser()
                        .ParseArguments<
                            DleOptions,
                            DqeOptions,
                            CacheOptions,
                            ListOptions,
                            ExtractionOptions,
                            ReleaseOptions,
                            CohortCreationOptions,
                            PackOptions,
                            PlatformDatabaseCreationOptions>(args)
                        .MapResult(
                            //Add new verbs as options here and invoke relevant runner
                            (DleOptions opts) => Run(opts),
                            (DqeOptions opts) => Run(opts),
                            (CacheOptions opts) => Run(opts),
                            (ListOptions opts) => Run(opts),
                            (ExtractionOptions opts) => Run(opts),
                            (ReleaseOptions opts) => Run(opts),
                            (CohortCreationOptions opts) => Run(opts),
                            (PackOptions opts) => Run(opts),
                            (PlatformDatabaseCreationOptions opts) => Run(opts),
                            errs => 1);

                NLog.LogManager.GetCurrentClassLogger().Info("Exiting with code " + returnCode);
                return returnCode;
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(e, "Fatal error occurred so returning -1");
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

        private static int Run(PackOptions opts)
        {
            FileInfo f = new FileInfo(opts.SolutionFile);
            if (!f.Exists)
            {
                Console.WriteLine("Could not find Solution File");
                return -58;
            }

            if (f.Extension != ".sln")
            {
                Console.WriteLine("SolutionFile must be a .sln file");
                return -57;
            }

            Packager p = new Packager(f, opts.ZipFileName, opts.SkipSourceCodeCollection,opts.Release);
            p.PackageUpFile(new ThrowImmediatelyCheckNotifier());

            if (!string.IsNullOrWhiteSpace(opts.Server))
            {
                ImplementationManager.Load(typeof(MicrosoftSQLImplementation).Assembly);
                var builder = new SqlConnectionStringBuilder() { DataSource = opts.Server, InitialCatalog = opts.Database, IntegratedSecurity = true };

                CatalogueRepository.SuppressHelpLoading = true;
                var processor = new PluginProcessor(new ThrowImmediatelyCheckNotifier(), new CatalogueRepository(builder));
                processor.ProcessFileReturningTrueIfIsUpgrade(new FileInfo(opts.ZipFileName));
            }

            return 0;
        }

        private static int Run(RDMPCommandLineOptions opts)
        {
            ImplementationManager.Load(typeof(MicrosoftSQLImplementation).Assembly,
                                       typeof(OracleImplementation).Assembly,
                                       typeof(MySqlImplementation).Assembly);

            var factory = new RunnerFactory();

            opts.LoadFromAppConfig();
            opts.DoStartup();

            var runner = factory.CreateRunner(opts);

            var listener = new NLogIDataLoadEventListener(false);
            var checker = new NLogICheckNotifier(true, false);

            int runExitCode = runner.Run(opts.GetRepositoryLocator(), listener, checker, new GracefulCancellationToken());

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
    }
}
