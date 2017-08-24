using System;

namespace ReusableLibraryCode.Checks
{
    public class CheckEventArgs
    {
        public string Message { get; set; }
        public CheckResult Result { get; set; }
        public Exception Ex { get; set; }
        public string ProposedFix { get; set; }
        public string StackTrace { get; set; }

        public CheckEventArgs(string message, CheckResult result, Exception ex = null, string proposedFix = null)
        {
            Message = message;
            Result = result;
            Ex = ex;
            ProposedFix = proposedFix;

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
    }
}