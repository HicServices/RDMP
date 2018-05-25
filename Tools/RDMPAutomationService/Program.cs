using System;
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
           try
            {
                //people who will handle the various Verbs
                var dle = new DleOptionsRunner();

                return
                    Parser.Default.ParseArguments<DleOptions>(args)
                        .MapResult(

                            //Add new verbs as options here and invoke relevant runner
                            (DleOptions opts) => dle.Run(opts),
                            errs => 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(e));
                return -1;
            }
        }
    }
}
