using System;
using System.Diagnostics;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Event args for IDataLoadEventListener.OnNotify events.  Includes the StackTrace the message was raised from, the ProgressEventType (Error, Warning etc) and
    /// Any Exception.
    /// </summary>
    public class NotifyEventArgs
    {
        public ProgressEventType ProgressEventType { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public bool Handled { get; set; }

        public string StackTrace { get; private set; }

        public NotifyEventArgs(ProgressEventType progressEventType, string message)
        {
            ProgressEventType = progressEventType;
            Message = message;
            Handled = false;
            try
            {
                StackTrace = Environment.StackTrace;
            }
            catch (Exception)
            {
                //Stack trace not available ah well
            }
        }
        public NotifyEventArgs(ProgressEventType progressEventType, string message,  Exception exception)
        {
            ProgressEventType = progressEventType;
            Message = message;
            Exception = exception;
            Handled = false;

            try{
                StackTrace = Environment.StackTrace;
            }
            catch (Exception)
            {
                //Stack trace not available ah well
            }
        }

        public CheckEventArgs ToCheckEventArgs()
        {
            CheckResult result;
            switch (ProgressEventType)
            {
                case ProgressEventType.Information:
                    result = CheckResult.Success;
                    break;
                case ProgressEventType.Warning:
                    result = CheckResult.Warning;
                    break;
                case ProgressEventType.Error:
                    result = CheckResult.Fail;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new CheckEventArgs(Message, result, Exception);
        }
    }
}