// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using NLog;
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
using YamlDotNet.RepresentationModel;

namespace rdmp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {    
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var nlog = Path.Combine(assemblyFolder ,"NLog.config");
                    
                if(File.Exists(nlog))
                    NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlog, true);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not load NLog.config");
            }

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

            PopulateConnectionStringsFromYamlIfMissing(opts);

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

        private static void PopulateConnectionStringsFromYamlIfMissing(RDMPCommandLineOptions opts)
        {
            var logger = LogManager.GetCurrentClassLogger();

            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var yaml = Path.Combine(assemblyFolder,"Databases.yaml");
            
            if(File.Exists(yaml))
            {
                try
                {
                        // Setup the input
			        using(var input = new StreamReader(yaml))
                    {
                        // Load the stream
			            var yamlStream = new YamlStream();
			            yamlStream.Load(input);
                    
                        // Examine the stream
                        var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode;

                    
                        foreach (var entry in mapping.Children)
                        {
                            string key = ((YamlScalarNode)entry.Key).Value;
                            string value = ((YamlScalarNode)entry.Value).Value;


                            try
                            {
                                var prop = typeof(RDMPCommandLineOptions).GetProperty(key);
                                prop.SetValue(opts,value);
                                logger.Info("Setting yaml config value for " + key);
                            }
                            catch (Exception)
                            {
                                logger.Error("Could not set property called " + key);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to read yaml file '" + yaml +"'");
                }
                
            }
        }
    }
}
