using System;
using HIC.Logging.Listeners.Extensions;
using NLog;
using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners
{
    /// <summary>
    /// Writes log messages to file, mainly used by Automation Service to log progress to file.
    /// It will log "OnProgress" calls as "Trace", which may make the log file very large!
    /// </summary>
    public class ToFileDataLoadEventListener : IDataLoadEventListener
    {
        private Logger _logger;

        public ToFileDataLoadEventListener(string logger)
        {
            _logger = NLog.LogManager.GetLogger(logger);
        }

        public ToFileDataLoadEventListener(Type logger) 
            : this(logger.FullName)
        { }

        public ToFileDataLoadEventListener(object logger)
            : this(logger.GetType())
        { }
        
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            _logger = NLog.LogManager.GetLogger(sender.GetType().FullName);
            _logger.Log(e.ToLogLevel(), e.Exception, e.Message);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            _logger = NLog.LogManager.GetLogger(sender.GetType().FullName);
            
        }
    }
}