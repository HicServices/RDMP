using System;
using System.Diagnostics;
using CatalogueLibrary.DataFlowPipeline;
using CommandLine;
using HIC.Logging.Listeners.NLogListeners;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
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
                    UsefulStuff.GetParser().ParseArguments<DleOptions, DqeOptions, CacheOptions,ListOptions,ExtractionOptions>(args)
                        .MapResult(
                            //Add new verbs as options here and invoke relevant runner
                            (DleOptions opts) => Run(opts),
                            (DqeOptions opts)=> Run(opts),
                            (CacheOptions opts)=>Run(opts),
                            (ListOptions opts)=>Run(opts),
                            (ExtractionOptions opts)=>Run(opts),
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

            return 0;
        }
    }
}
