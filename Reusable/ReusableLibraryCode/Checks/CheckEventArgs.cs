using System;
using ReusableLibraryCode.Progress;

namespace ReusableLibraryCode.Checks
{
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