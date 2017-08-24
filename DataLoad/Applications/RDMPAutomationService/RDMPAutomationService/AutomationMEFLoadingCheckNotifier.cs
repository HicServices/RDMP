using System;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService
{
    internal class AutomationMEFLoadingCheckNotifier : ICheckNotifier
    {
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if(args.Result == CheckResult.Fail)
            {
                Console.WriteLine("MEF Loading ERROR:___________");
                Console.WriteLine(args.Message);

                if(args.Ex != null)
                {
                    Console.WriteLine("EXCEPTION:___________");
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(args.Ex));
                }
            }

            //accept all suggestions
            return true;
        }
    }
}