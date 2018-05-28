using System;
using System.Diagnostics;
using CatalogueLibrary.DataFlowPipeline;
using CommandLine;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService
{
    public class Program
    {
        public static int Main(string[] args)
        {
           try
            {
               
               return
                    Parser.Default.ParseArguments<DleOptions, DqeOptions, CacheOptions>(args)
                        .MapResult(
                            //Add new verbs as options here and invoke relevant runner
                            (DleOptions opts) => Run(opts),
                            (DqeOptions opts)=> Run(opts),
                            (CacheOptions opts)=>Run(opts),
                            errs => 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(e,true));
                return -1;
            }
        }

        private static int Run(RDMPCommandLineOptions opts)
        {
            var factory = new RunnerFactory();

            opts.LoadFromAppConfig();
            opts.DoStartup();

            var runner = factory.CreateRunner(opts);
            return runner.Run(opts.GetRepositoryLocator(),new ThrowImmediatelyDataLoadEventListener(){WriteToConsole = false},new ThrowImmediatelyCheckNotifier(),new GracefulCancellationToken());
        }
    }
}
