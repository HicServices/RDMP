using System;
using System.ServiceProcess;
using CommandLine;
using RDMPAutomationService.OptionRunners;
using RDMPAutomationService.Options;
using ReusableLibraryCode;

namespace RDMPAutomationService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                try
                {
                    //people who will handle the various Verbs
                    var service = new ServiceOptionsRunner();
                    var dle = new DleOptionsRunner();

                    return
                        Parser.Default.ParseArguments<ServiceOptions, DleOptions>(args)
                            .MapResult(

                                //Add new verbs as options here and invoke relevant runner
                                (ServiceOptions opts) => service.Run(opts),
                                (DleOptions opts) => dle.Run(opts),
                                errs => 1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(e));
                    return -1;
                }
            }
            
            var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
            ServiceBase.Run(servicesToRun);
            return 0;
        }
    }
}
