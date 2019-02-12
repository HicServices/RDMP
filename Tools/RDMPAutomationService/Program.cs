// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.DataFlowPipeline;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using HIC.Logging.Listeners.NLogListeners;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using RDMPAutomationService.Runners;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using LogLevel = NLog.LogLevel;

namespace RDMPAutomationService
{
    public class Program
    {
        public static int Main(string[] args)
        {
           try
           {
               var returnCode =
                   UsefulStuff.GetParser()
                       .ParseArguments<DleOptions, DqeOptions, CacheOptions, ListOptions, ExtractionOptions, ReleaseOptions, CohortCreationOptions>(args)
                       .MapResult(
                           //Add new verbs as options here and invoke relevant runner
                           (DleOptions opts) => Run(opts),
                           (DqeOptions opts) => Run(opts),
                           (CacheOptions opts) => Run(opts),
                           (ListOptions opts) => Run(opts),
                           (ExtractionOptions opts) => Run(opts),
                           (ReleaseOptions opts) => Run(opts),
                           (CohortCreationOptions opts) => Run(opts),
                           errs => 1);

               NLog.LogManager.GetCurrentClassLogger().Info("Exiting with code " + returnCode);
                return returnCode;
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(e,"Fatal error occurred so returning -1");
                return -1;
            }
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
