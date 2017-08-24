using System;

namespace ReusableLibraryCode.Checks
{
    public class ThrowImmediatelyCheckNotifier : ICheckNotifier
    {
        public ThrowImmediatelyCheckNotifier()
        {
            WriteToConsole = true;
        }

        virtual public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (WriteToConsole)
                Console.WriteLine(args.Message);

            if (args.Result == CheckResult.Fail)
                throw new Exception(args.Message, args.Ex);

            if(args.Result == CheckResult.Warning && ThrowOnWarning)
                throw new Exception(args.Message, args.Ex);

            //do not apply fixes to warnings/success
            return false;
        }

        /// <summary>
        /// By default this class will only throw Fail results but if you set this flag then it will also throw warning messages
        /// </summary>
        public bool ThrowOnWarning { get; set; }
        
        public bool WriteToConsole { get; set; }
    }
}