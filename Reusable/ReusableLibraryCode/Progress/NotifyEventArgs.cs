using System;
using System.Diagnostics;

namespace ReusableLibraryCode.Progress
{
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
    }
}