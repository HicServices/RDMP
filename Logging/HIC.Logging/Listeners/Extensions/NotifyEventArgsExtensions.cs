using System;
using NLog;
using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners.Extensions
{
    public static class NotifyEventArgsExtensions
    {
        public static LogLevel ToLogLevel(this NotifyEventArgs args)
        {
            switch (args.ProgressEventType)
            {
                case ProgressEventType.Trace:
                    return LogLevel.Trace;
                case ProgressEventType.Debug:
                    return LogLevel.Debug;
                case ProgressEventType.Information:
                    return LogLevel.Info;
                case ProgressEventType.Warning:
                    return LogLevel.Warn;
                case ProgressEventType.Error:
                    return LogLevel.Error;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}