using System;

namespace ReusableLibraryCode.Checks
{
    public class AcceptAllCheckNotifier : ICheckNotifier
    {
        public virtual bool OnCheckPerformed(CheckEventArgs args)
        {

            //if there is a proposed fix then accept it regardless of whether it was a Fail.
            if (!string.IsNullOrWhiteSpace(args.ProposedFix))
                return true;

            if (args.Result == CheckResult.Fail)
                throw new Exception("Failed check with message: " + args.Message, args.Ex);

            return true;
        }
    }
}