// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Reflection;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using NLog;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Gui;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners.NLogListeners;
using Rdmp.Core.Startup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Rdmp.Core
{
    class Program
    {
        private static EnvironmentInfo GetEnvironmentInfo()
        {
            return new EnvironmentInfo("netcoreapp2.2");
        }
        
        static int Main(string[] args)
        {
            Console.WriteLine("Environment:"+ Environment.Version);

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

            PreStartup();

            return HandleArguments(args);
        }
        
        private static void PreStartup()
        {
            AssemblyResolver.SetupAssemblyResolver();

            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
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
                            ExtractionOptions,
                            ReleaseOptions,
                            CohortCreationOptions,
                            PackOptions,
                            ExecuteCommandOptions,
                            ConsoleGuiOptions,
                            PlatformDatabaseCreationOptions>(args)
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
                            errs => 1);

                NLog.LogManager.GetCurrentClassLogger().Info("Exiting with code " + returnCode);
                return returnCode;
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(e.Message);
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

        private static int RunCmd(ExecuteCommandOptions opts)
        {
            if(!string.IsNullOrWhiteSpace(opts.File))
            {
                if(!File.Exists(opts.File))
                {
                    Console.WriteLine($"Could not find file '{opts.File}'");
                    return -55;
                }

                try
                {
                    var d = new Deserializer();
                    opts.Script = d.Deserialize<RdmpScript>(File.ReadAllText(opts.File));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error deserializing '{opts.File}': {ex.Message}");
                    return -66;                    
                }
            }

            return Run((RDMPCommandLineOptions) opts);
        }

        private static int Run(RDMPCommandLineOptions opts)
        {
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();

            PopulateConnectionStringsFromYamlIfMissing(opts);
            
            var listener = new NLogIDataLoadEventListener(false);
            var checker = new NLogICheckNotifier(true, false);

            var factory = new RunnerFactory();
            opts.DoStartup(GetEnvironmentInfo(),opts.LogStartup ? (ICheckNotifier)checker: new IgnoreAllErrorsCheckNotifier());

            //if user wants to run checking chances are they don't want checks to fail becasue of errors logged during startup (MEF shows lots of errors!)
            if(opts.LogStartup && opts.Command == CommandLineActivity.check)
                checker.Worst = LogLevel.Info;

            var runner = opts is ConsoleGuiOptions g ? 
                        new ConsoleGuiRunner(g):
                         factory.CreateRunner(opts);

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

                        
            if(!opts.NoConnectionStringsSpecified())
            {
                logger.Info("Connection string options have been specified on command line, yaml config values will be ignored");
                return;
            }

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
