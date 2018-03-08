using System;
using Common.Logging;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// IDataLoadEventListener which routes messages to an Common.Logging.ILog.  Use this if you already have a logging channel set up (e.g. in log4net or NLog and want to
    /// route messages from RDMP to it).  By default OnProgress messages (which are incremental counts and may number in the thousands per task) are logged as Trace.  Set
    /// LogOnProgressAsTrace to false to suppress these messages.
    /// </summary>
    public class FromILogToDataLoadEventListener:IDataLoadEventListener
    {
        private readonly ILog _logger;
        public bool LogOnProgressAsTrace { get; set; }

        public FromILogToDataLoadEventListener(ILog logger)
        {
            _logger = logger;
            LogOnProgressAsTrace = true;//default
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            switch (e.ProgressEventType)
            {
                case ProgressEventType.Trace:
                    _logger.Trace(e.Message,e.Exception);
                    break;
                case ProgressEventType.Debug:
                    _logger.Debug(e.Message,e.Exception);
                    break;
                case ProgressEventType.Information:
                    _logger.Info(e.Message,e.Exception);
                    break;
                case ProgressEventType.Warning:
                    _logger.Warn(e.Message,e.Exception);
                    break;
                case ProgressEventType.Error:
                    _logger.Error(e.Message,e.Exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if (LogOnProgressAsTrace)
                _logger.Trace(e.TaskDescription + ":" + e.Progress.Value + " " + e.Progress.UnitOfMeasurement);
        }
    }
}