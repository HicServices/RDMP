using NLog;
using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners
{
    /// <summary>
    /// Wraps the ToLoggingDatabaseDataLoadEventListener in order to route any debug and trace message to file and leave
    /// all info/warning/errors to go into the database.
    /// </summary>
    public class ToFileAndDbDataLoadEventListener : ToLoggingDatabaseDataLoadEventListener
    {
        private Logger _logger;

        public ToFileAndDbDataLoadEventListener(object hostingApplication, LogManager logManager, string loggingTask, string runDescription) : base(hostingApplication, logManager, loggingTask, runDescription)
        {
            _logger = NLog.LogManager.GetLogger(loggingTask);
        }

        public ToFileAndDbDataLoadEventListener(LogManager logManager, IDataLoadInfo dataLoadInfo) : base(logManager, dataLoadInfo)
        {
        }
        
        public override void OnNotify(object sender, NotifyEventArgs e)
        {
            if (e.ProgressEventType == ProgressEventType.Error || e.ProgressEventType == ProgressEventType.Warning || e.ProgressEventType == ProgressEventType.Information)
            {
                base.OnNotify(sender, e);
            }

            _logger.Log(LogLevel.FromString(e.ProgressEventType.GetLogLevel()), e.Exception, e.Message);
        }
    }

    public static class NlogLoggerExtensions
    {
        public static string GetLogLevel(this ProgressEventType pet)
        {
            switch (pet)
            {
                case ProgressEventType.Trace:
                    return "Trace";
                case ProgressEventType.Debug:
                    return "Debug";
                case ProgressEventType.Information:
                    return "Info";
                case ProgressEventType.Warning:
                    return "Warn";
                case ProgressEventType.Error:
                    return "Error";
                default:
                    return "Off";
            }
        }
    }
}