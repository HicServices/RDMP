using System;
using Common.Logging;

namespace ReusableLibraryCode.Progress
{
    public class FromILogToDataLoadEventListener:IDataLoadEventListener
    {
        private readonly ILog _logger;

        public FromILogToDataLoadEventListener(ILog logger)
        {
            _logger = logger;
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
            _logger.Trace(e.TaskDescription + ":" + e.Progress.Value + " " + e.Progress.UnitOfMeasurement);
        }
    }
}