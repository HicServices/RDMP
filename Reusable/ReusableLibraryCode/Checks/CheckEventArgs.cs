using System;
using ReusableLibraryCode.Progress;

namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// Created when an ICheckable performs a check to indicate whether the check passed or not and whether there is an Exception or ProposedFix.  ProposedFix
    /// is a string that suggests how a problem can be resolved but where the resolution might be undesireable under certain circumstances (hence the choice).
    /// 
    /// The workflow is:
    /// 1. ICheckable has it's Check method called with an ICheckNotifier
    /// 2. Check logic performed
    /// 3. CheckEventArgs created and ICheckNotifier.OnCheckPerformed called
    /// 4. ICheckNotifier decides how to respond to the message (which can include throwing an Exception - which you should not catch/suppress).
    /// 5. If OnCheckPerformed compeltes without Exception evaluate the bool return if there was a ProposedFix and apply the fix if it is true
    /// 
    /// </summary>
    public class CheckEventArgs
    {
        public string Message { get; set; }
        public CheckResult Result { get; set; }
        public Exception Ex { get; set; }
        public string ProposedFix { get; set; }
        public string StackTrace { get; set; }

        public DateTime EventDate { get; private set; }

        public CheckEventArgs(string message, CheckResult result, Exception ex = null, string proposedFix = null)
        {
            Message = message;
            Result = result;
            Ex = ex;
            ProposedFix = proposedFix;

            EventDate = DateTime.Now;

            try
            {
                StackTrace = Environment.StackTrace;
            }
            catch (Exception)
            {
                //Stack trace not available ah well
            }
        }

        public override string ToString()
        {
            return Message;
        }

        public NotifyEventArgs ToNotifyEventArgs()
        {
            ProgressEventType status;
            
            switch (Result)
            {
                case CheckResult.Success:
                    status = ProgressEventType.Information;
                    break;
                case CheckResult.Warning:
                    status = ProgressEventType.Warning;
                    break;
                case CheckResult.Fail:
                    status = ProgressEventType.Error;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new NotifyEventArgs(status, Message,Ex);
        }
    }
}