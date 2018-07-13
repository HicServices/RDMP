using System;
using NLog;

namespace HIC.Logging.Listeners.NLogListeners
{
    /// <summary>
    /// Base class for all RDMP Listeners (e.g. <see cref="ICheckNotifier"/>) which handle events by writing to an NLog.LogManager
    /// </summary>
    public abstract class NLogListener
    {
        public LogLevel Worst { get; private set; }
        public bool ThrowOnError { get; set; }

        public NLogListener(bool throwOnError)
        {
            ThrowOnError = throwOnError;
            Worst = LogLevel.Info;
        }
        
        protected void Log(object sender, LogLevel level, Exception exception, string message)
        {
            if (level > Worst)
                Worst = level;

            NLog.LogManager.GetLogger((sender ?? "Null").ToString()).Log(level, exception, message);
            
            if (ThrowOnError && level >= LogLevel.Error)
                throw exception??new Exception(message);
        }
    }
}