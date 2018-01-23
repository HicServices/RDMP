using System;

namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// ICheckNotifier which converts failed CheckEventArgs into Exceptions.  Can optionally also throw on Warning messages.  By default all messages are written
    /// to the Console.  The use case for this is any time you want to run Checks programatically (i.e. without user intervention via a UI component) before running
    /// and you don't expect any Checks to fail but want to make sure.  Or when you are in a Test and you want to make sure that a specific configuration bombs
    /// when Checked with an appropriate failure message.
    /// </summary>
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

        /// <summary>
        /// By default this class will also log to StdOut. Set to false to skip this (in case of non-interactive environments)
        /// </summary>
        public bool WriteToConsole { get; set; }
    }
}